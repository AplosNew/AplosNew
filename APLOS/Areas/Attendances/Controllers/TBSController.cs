using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class TBSController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private DataSet dsRef;

        public TBSController(
              IMaternityLeavePolicyService LeavePolicyService,
               IAttendanceManagementService AttendanceManagementService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public ActionResult Save(TBS TBS)
        {
            try
            {
                DateTime dateInTime = DateTime.Now;
                string ToDayDate = Convert.ToDateTime(dateInTime).ToString("dd-MMM-yyyy");
                if (Convert.ToDateTime(ToDayDate) < Convert.ToDateTime(TBS.EntryDate))
                {
                    throw new Exception("Future Date is not allowed..");
                }

                ConnectionManager.DAL.ConManager objCon;
                if (TBS.Id == null)
                {
                    DataSet dsvalidation;
                    string sql3 = "select EmpSystemId From [HKP].[EmployeeDisciplinaryAction] where EmpSystemId='" + TBS.EmpSystemId + "' and EntryDate='" + TBS.EntryDate + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql3, out dsvalidation, false, "1");
                    if (dsvalidation.Tables[0].Rows.Count > 0)
                    {
                        Exception ex = new Exception("This Effective Date Is Already Assigned ");
                        throw (ex);
                    }
                }

                string sql = @"update EmployeeInformation SET EmployeeCurrentStatus = 'TBS', EmployeeCurrentStatusEffectiveDate='"+ TBS.EntryDate+@"' where SystemId='"+TBS.EmpSystemId+@"'";
                ExecuteRawSQL(sql);

                SaveTBS(TBS);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExecuteRawSQL(string sql1)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(sql1, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End Function


        public void SaveTBS(TBS TBS)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [HKP].[EmployeeDisciplinaryAction] WHERE Id='" + TBS.Id + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[HKP].[EmployeeDisciplinaryAction]", out sID);
                    dr["Id"] = "TBS" + sID;
                    dr["EmpSystemId"] = TBS.EmpSystemId;
                    dr["DisciplinaryActionCategoryId"] = TBS.DisciplinaryActionCategoryId;
                    dr["Description"] = TBS.Description;                    
                    dr["EntryDate"] = TBS.EntryDate;
                    dr["ActionType"] = TBS.ActionType;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {

                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["EmpSystemId"] = TBS.EmpSystemId;
                    dr["DisciplinaryActionCategoryId"] = TBS.DisciplinaryActionCategoryId;
                    dr["Description"] = TBS.Description;
                    dr["EntryDate"] = TBS.EntryDate;
                    dr["ActionType"] = TBS.ActionType;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetTBS(string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT ed.Id,ed.EmpSystemId,ed.DisciplinaryActionCategoryId,ed.[Description]
                            ,Format(ed.EntryDate,'dd-MMM-yyyy') as EntryDate, ed.EntryDate as EntryDates
                            ,da.UserName as ActionCategory,ActionType
                             FROM [HKP].[EmployeeDisciplinaryAction] as ed
                             left join [HKP].[DisciplinaryActionCategory] da on da.id=ed.DisciplinaryActionCategoryId
                             where ed.EmpSystemId='" + empId+ @"' and ActionType='TBS'
                             order by EntryDates DESC";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getTBSMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,eda.EntryDate,format(eda.EntryDate,'dd-MMM-yyyy')EntryDates
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
										inner join [HKP].[EmployeeDisciplinaryAction]   eda on eda.EmpSystemId=emp.SystemId
                              Where EMP.PlantId='" + identity.PlantId+ @"' 
                                AND EMP.EmployeeStatus='Active'  
                       AND    eda.ActionType='TBS'
                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric,EntryDate";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,UserName From [HKP].[DisciplinaryActionCategory]";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult Delete(string Id,string EmpSystemId,string Date)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                DataSet dsvalidation;
string sql3 = " select empsystemid,max(EntryDate) as EntryDate  From [HKP].[EmployeeDisciplinaryAction]   where EmpSystemId = '"+EmpSystemId+@"' and ActionType = 'TBS' group by empsystemid having max(EntryDate) > '"+Date+@"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql3, out dsvalidation, false, "1");
                if (dsvalidation.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("This Effective Date Is Not Last Date");
                    throw (ex);
                }

                string sql1 = @"update EmployeeInformation SET EmployeeCurrentStatus = null, EmployeeCurrentStatusEffectiveDate= null where SystemId='"+EmpSystemId+@"'";
                ExecuteRawSQL(sql1);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM [HKP].[EmployeeDisciplinaryAction] WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        public class TBS : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string EmpSystemId { get; set; }
            public string DisciplinaryActionCategoryId { get; set; }
            public string Description { get; set; }
            public DateTime? EntryDate { get; set; }
            public string ActionType { get; set; }
           
            #endregion Scalar Properties

            #region Audit Properties

            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string AddedFromIP { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string  UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        #endregion -- Operations  
    }
}