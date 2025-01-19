#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Linq;
using clsAttendance;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ManualAttendanceConfirmationController : BaseController
    {


        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public ManualAttendanceConfirmationController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor
        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages



        [HttpPost, Authorize]
        public ActionResult getAttendanceData(string pdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceData(pdate);


            var jsondata = Json(new { data = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public ActionResult getAttendanceDataPending(string pdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceDataPending();


            var jsondata = Json(new { data = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }



        [HttpPost]
        public ActionResult SaveSingleEmployee(string employeeid, string workdate, string inOrOut)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {


                DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE pwal.LockedDate='" + workdate + "' AND pwal.PlantId='" + identity.PlantId + "' and IsActive=1 ");
                if (dtLock.Rows.Count > 0)
                    throw new Exception("Day locked");


                DataSet dsManualAttendance, dsAppAttendance;


                con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.getDataSet(@"SELECT * FROM AttdnManualData AS SA WHERE SA.EmpSystemID = '" + employeeid + "' AND sa.WorkDate = '" + workdate + "'", out dsManualAttendance);
                con.CommitTransaction();


                con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.getDataSet(@"SELECT * FROM AttdnRawDataFromApp AS SA WHERE SA.EmployeeId = '" + employeeid + "' AND sa.Pdate = '" + workdate + "'", out dsAppAttendance);
                con.CommitTransaction();

                if (dsAppAttendance.Tables[0].Rows.Count == 0)
                    throw new Exception("No data found");

                dsAppAttendance.Tables[0].Rows[0].BeginEdit();
                if (dsManualAttendance.Tables[0].Rows.Count > 0)
                {

                    DataRow dr = dsManualAttendance.Tables[0].Rows[0];

                    dr.BeginEdit();


                    if (dsAppAttendance.Tables[0].Rows[0]["InTime"].ToString() != "")
                    {
                        if (inOrOut.ToUpper() == "IN")
                        {
                            dr["InTime"] = dsAppAttendance.Tables[0].Rows[0]["InTime"].ToString();

                            dsAppAttendance.Tables[0].Rows[0]["isApprovedIN"] = true;
                            dsAppAttendance.Tables[0].Rows[0]["ApprovedByIN"] = identity.Name;
                            dsAppAttendance.Tables[0].Rows[0]["ApprovalDateIN"] = DateTime.Now.ToString();
                        }

                    }

                    if (dsAppAttendance.Tables[0].Rows[0]["OutTime"].ToString() != "")
                    {
                        if (inOrOut.ToUpper() == "OUT")
                        {
                            dr["OutTime"] = dsAppAttendance.Tables[0].Rows[0]["OutTime"].ToString();

                            dsAppAttendance.Tables[0].Rows[0]["isApprovedOUT"] = true;
                            dsAppAttendance.Tables[0].Rows[0]["ApprovedByOUT"] = identity.Name;
                            dsAppAttendance.Tables[0].Rows[0]["ApprovalDateOUT"] = DateTime.Now.ToString();
                        }
                    }



                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now;


                    dr.EndEdit();
                }
                else
                {

                    DataRow dr = dsManualAttendance.Tables[0].NewRow();

                    dr["EmpSystemID"] = employeeid;
                    dr["WorkDate"] = workdate;
                    dr["GroupID"] = identity.CompanyGroupId;
                    //dr["PlantID"] = identity.PlantId;


                    if (dsAppAttendance.Tables[0].Rows[0]["InTime"].ToString() != "")
                    {
                        if (inOrOut.ToUpper() == "IN")
                        {
                            dr["InTime"] = dsAppAttendance.Tables[0].Rows[0]["InTime"].ToString();

                            dsAppAttendance.Tables[0].Rows[0]["isApprovedIN"] = true;
                            dsAppAttendance.Tables[0].Rows[0]["ApprovedByIN"] = identity.Name;
                            dsAppAttendance.Tables[0].Rows[0]["ApprovalDateIN"] = DateTime.Now.ToString();
                        }

                    }

                    if (dsAppAttendance.Tables[0].Rows[0]["OutTime"].ToString() != "")
                    {
                        if (inOrOut.ToUpper() == "OUT")
                        {
                            dr["OutTime"] = dsAppAttendance.Tables[0].Rows[0]["OutTime"].ToString();
                            dsAppAttendance.Tables[0].Rows[0]["isApprovedOUT"] = true;
                            dsAppAttendance.Tables[0].Rows[0]["ApprovedByOUT"] = identity.Name;
                            dsAppAttendance.Tables[0].Rows[0]["ApprovalDateOUT"] = DateTime.Now.ToString();
                        }
                    }



                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now;
                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = System.DateTime.Now;

                    dsManualAttendance.Tables[0].Rows.Add(dr);



                }

                dsAppAttendance.Tables[0].Rows[0].EndEdit();

                clsStaticInfo objStatic = new clsStaticInfo();
                objStatic.SaveDataSets(dsManualAttendance, dsAppAttendance);


                try
                {
                    clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    ReturnType r = obj.SaveTotal(identity.PlantId, workdate, "'" + employeeid + "'", false);//laila


                }
                catch (Exception ex)
                {

                    throw new Exception("Error occured while processing attendance " + ex.Message);

                }


                return Json(new { Error = false, Message = "Attendance Approved" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }


        private string stringAttendanceData(string pdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @" SELECT 
                            emp.SystemId AS Id,emp.EmployeeCode,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,
                            isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(app.PDate,'dd-MMM-yyyy') AS PDate,
                            isnull(app.INLocationDesc,'')AS INLocationDesc,isnull(APP.OutLocationDesc,'') AS OutLocationDesc,
                            isnull(format(app.InTime,'hh:mm:ss tt'),'') InTime, ISNULL(app.Remarks,'') Remarks,ISNULL(app.RemarksOUT,'') AS RemarksOUT,
                            isnull(format(app.OutTime,'hh:mm:ss tt'),'') OutTime, format(apd.InTime,'dd-MMM-yyyy hh:mm:ss tt') ProcessedInTime, format(apd.OutTime,'dd-MMM-yyyy hh:mm:ss tt') ProcessedOutTime, 
                            format(apd.PunchInTime,'dd-MMM-yyyy hh:mm:ss tt') PunchInTime, format(apd.PunchOutTime,'dd-MMM-yyyy hh:mm:ss tt') PunchOutTime, 
                            ISNULL(apd.DayStatus,'') AS DayStatus,
                            app.Remarks,isnull(APP.isApprovedIN,0) AS isApprovedIN,isnull(APP.isApprovedOUT,0) isApprovedOUT,APP.ApprovedByIN,
                            APP.ApprovedByOUT
                            

                             FROM  
                             AttdnRawDataFromApp APP 
                            LEFT JOIN EmployeeInformation EMP ON emp.SystemId=app.EmployeeId
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id	
                            LEFT JOIN AttdnProcessData AS apd ON apd.EmpSystemID=app.EmployeeId AND apd.WorkDate='" + pdate + @"'

                        WHERE app.PDate='" + pdate + "' AND emp.PlantId='" + identity.PlantId + @"'		
                        ORDER BY emp.EmployeeCode";



        }
        private string stringAttendanceDataPending()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @" SELECT 
                            emp.SystemId AS Id,emp.EmployeeCode,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,
                            isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(app.PDate,'dd-MMM-yyyy') AS PDate,
                            isnull(app.INLocationDesc,'')AS INLocationDesc,isnull(APP.OutLocationDesc,'') AS OutLocationDesc,
                            isnull(format(app.InTime,'hh:mm:ss tt'),'') InTime, ISNULL(app.Remarks,'') Remarks,ISNULL(app.RemarksOUT,'') AS RemarksOUT,
                            isnull(format(app.OutTime,'hh:mm:ss tt'),'') OutTime, format(apd.InTime,'dd-MMM-yyyy hh:mm:ss tt') ProcessedInTime, format(apd.OutTime,'dd-MMM-yyyy hh:mm:ss tt') ProcessedOutTime, 
                            format(apd.PunchInTime,'dd-MMM-yyyy hh:mm:ss tt') PunchInTime, format(apd.PunchOutTime,'dd-MMM-yyyy hh:mm:ss tt') PunchOutTime, 
                            ISNULL(apd.DayStatus,'') AS DayStatus,
                            app.Remarks,isnull(APP.isApprovedIN,0) AS isApprovedIN,isnull(APP.isApprovedOUT,0) isApprovedOUT,APP.ApprovedByIN,
                            APP.ApprovedByOUT
                            

                             FROM  
                             AttdnRawDataFromApp APP 
                            LEFT JOIN EmployeeInformation EMP ON emp.SystemId=app.EmployeeId
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id	
                            LEFT JOIN AttdnProcessData AS apd ON apd.EmpSystemID=app.EmployeeId AND apd.WorkDate=APP.Pdate

                          WHERE (
                        	
                                       ( isnull(app.isApprovedIN,0)=0 AND isnull(app.InTime,'')<>'')
                        
                                        OR 
                        
                                       ( isnull(app.isApprovedOUT,0)=0 AND isnull(app.OutTime,'')<>'')
                        
                        )  AND emp.PlantId='" + identity.PlantId + @"'		
                        ORDER BY app.pdate ASC, emp.EmployeeCode";



        }
    }

}