#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Payrolls;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class DailyAllowanceTransactionController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public DailyAllowanceTransactionController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations


        [HttpGet]
        public ActionResult GetEmployeeBySectionAndWorkDate(string SectionId, DateTime workDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Active=CASE WHEN POT.EmpSystemID IS NOT NULL AND POT.WorkDate='" + workDate + @"' THEN 1 ELSE 0 END
                                        ,Emp.SystemID EmpSystemID,EMP.EmployeeName,CONVERT (int, EMP.EmployeeCode) EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line, ATN.DayStatus, POT.PreallocatedOTHr, DT.Category,PR.SectionId,EMP.DepartmentId
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
										LEFT JOIN dbo.EmployeeOTEntitle OT on OT.EmpSystemID=EMP.SystemId
										LEFT OUTER JOIN
										    (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId,dm.plantid
                                        ,dg.UserName GivenDesignationGroup,DM.IsOTEntitled
                                        from ( SELECT DC.SalaryRuleMasterId,dc.plantid,dm.*,DC.IsOTEntitled FROM MST.DesignationMaster DM
				                        		 LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                   ON DM.Id=DC.DesignationMasterId
                                                   )  dm
                                        LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                        ) egdsggso on egdsggso.DesignationId=EMP.GivenDesignationId and egdsggso.PlantId=e.PlantId

				                        LEFT JOIN (SELECT* FROM AttdnProcessData Where WorkDate='" + workDate + @"')ATN ON ATN.EmpSystemID=EMP.SystemId
                                        LEFT JOIN DayType DT ON DT.DayType=ATN.DayStatus 
                                        LEFT JOIN [dbo].[PreallocatedOT] POT ON POT.EmpSystemID=EMP.SystemId AND POT.WorkDate='" + workDate + @"'
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and (EMP.EmployeeStatus='Active' OR DOS>'" + workDate + @"') and PR.SectionId='" + SectionId + @"' 
				                        AND (egdsggso.IsOTEntitled=1 or OT.IsOTEntitle=1) ORDER BY CONVERT (int, EMP.EmployeeCode)";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(IEnumerable<DailyAllowanceTransaction> entities)
        {

            try
            {
                if (entities==null)
                {
                    throw new Exception("Select Employees.");
                }
                SaveData(entities);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private DataSet PlantWiseLock(string plantId, DateTime workDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT FORMAT(LockedDate,'dd-MMM-yyyy') LockedDate FROM PlantWiseAttendanceLock where PlantId='" + plantId + "' And LockedDate='" + workDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "PreallocatedOT", out idFromDB);
            systemID = "POT" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private void SaveData(IEnumerable<DailyAllowanceTransaction> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {

                foreach (var item in data)
                {
                    DeleteData(item.WorkDate, item.SectionId, item.DepartmentId);
                }


                foreach (var item in data)
                {
                    var plantLock = PlantWiseLock(item.PlantID, item.WorkDate);
                    if (plantLock.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Attendance is locked on " + plantLock.Tables[0].Rows[0]["LockedDate"] + "");
                    }
                    else
                    {

                        string sql = "SELECT * FROM [dbo].[PreallocatedOT] WHERE EmpSystemID='" + item.EmpSystemID + "' AND WorkDate='" + item.WorkDate + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();


                            dr["EmpSystemID"] = item.EmpSystemID;
                            dr["WorkDate"] = item.WorkDate;
                            dr["PreallocatedOTHr"] = item.PreallocatedOTHr;
                            dr["GroupID"] = identity.CompanyGroupId;
                            dr["PlantID"] = identity.PlantId;
                            dr["GroupID"] = identity.CompanyGroupId;
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;


                            dsMaster.Tables[0].Rows.Add(dr);
                        }

                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }

            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public void DeleteData(DateTime workDate, string sectionId, string departmentId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM  [dbo].[PreallocatedOT] WHERE WorkDate='" + workDate + "' AND EmpSystemID IN (SELECT SystemID FROM EmployeeInformation WHERE SectionId='" + sectionId + "' AND DepartmentId='" + departmentId + "')";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        #endregion
    }

    public class DailyAllowanceTransaction : BaseModel
    {
        public string EmpSystemID { get; set; }
        public DateTime WorkDate { get; set; }
        public double PreallocatedOTHr { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }

        public string SectionId { get; set; }
        public string DepartmentId { get; set; }

        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }

    }
}