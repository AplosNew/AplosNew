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
    public class AttendanceEntryController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;


        public AttendanceEntryController(
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
        public ActionResult GetEmpInfo(string SearchValue)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Today = DateTime.Now;
            string FirstDayOfTheMonth = "01-" + Convert.ToDateTime(Today).ToString("MMM") + "-" + Convert.ToDateTime(Today).ToString("yyyy");
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

            string sql = @" SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
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
                              Where EMP.PlantId='" + identity.PlantId + @"' and emp.EmployeeCode='" + SearchValue + @"'                             
                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Save(AttendanceEntry AttendanceEntry)
        {
            try
            {
                // previous 2 days
                if (AttendanceEntry.PType == "IN")
                {
                    string otInTime = Convert.ToDateTime(AttendanceEntry.PDate).ToString("dd-MMM-yyyy");
                    string otTimeInTime = Convert.ToDateTime(AttendanceEntry.InTime).ToString("dd-MMM-yyyy");
                    DateTime NewPreviousDateInTime = Convert.ToDateTime(otInTime).AddDays(-2);

                    if (Convert.ToDateTime(NewPreviousDateInTime) >= Convert.ToDateTime(otTimeInTime))
                    {
                        throw new Exception("Out time Can't be less then in time...");
                    }
                }
                else
                {
                    string ot = Convert.ToDateTime(AttendanceEntry.PDate).ToString("dd-MMM-yyyy");
                    string otTime = Convert.ToDateTime(AttendanceEntry.OutTime).ToString("dd-MMM-yyyy");
                    DateTime NewPreviousDate = Convert.ToDateTime(ot).AddDays(-2);

                    if (Convert.ToDateTime(NewPreviousDate) >= Convert.ToDateTime(otTime))
                    {
                        throw new Exception("Out time Can't be less then in time...");
                    }
                }

                //future date
                if (AttendanceEntry.PType == "IN")
                {
                    DateTime NewFutureDateInTime;
                    string otFutureDateInTime = Convert.ToDateTime(AttendanceEntry.PDate).ToString("dd-MMM-yyyy");
                    string otFutureTimeInTime = Convert.ToDateTime(AttendanceEntry.InTime).ToString("dd-MMM-yyyy");
                    NewFutureDateInTime = Convert.ToDateTime(otFutureDateInTime).AddDays(1);

                    if (Convert.ToDateTime(NewFutureDateInTime) < Convert.ToDateTime(otFutureTimeInTime))
                    {
                        throw new Exception("Working Hour Max Limit is 24..");
                    }
                }
                else
                {
                    DateTime NewFutureDate;
                    string otFutureDate = Convert.ToDateTime(AttendanceEntry.PDate).ToString("dd-MMM-yyyy");
                    string otFutureTime = Convert.ToDateTime(AttendanceEntry.OutTime).ToString("dd-MMM-yyyy");
                    NewFutureDate = Convert.ToDateTime(otFutureDate).AddDays(1);

                    if (Convert.ToDateTime(NewFutureDate) < Convert.ToDateTime(otFutureTime))
                    {
                        throw new Exception("Working Hour Max Limit is 24..");
                    }
                }

                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //DataSet dsHourMinutes;
                //DataTable dtHourMinutes;
                //ConnectionManager.DAL.ConManager Con;
                //Con = new ConnectionManager.DAL.ConManager("1");
                //string sql4 = @"
                //            select C.UserName,isnull(C.GMTHour,0)as GMTHour ,isnull(C.GMTMinute,0)as GMTMinute  From ORG.Plant AS P
                //            LEFT JOIN [MST].[AddressMaster] as AM ON AM.Id=P.AddressMasterId
                //            left join [SCS].[Country] C ON C.Id=AM.CountryId
                //            WHERE P.Id='" + identity.PlantId+@"'";
                //Con = new ConnectionManager.DAL.ConManager("1");
                //Con.OpenDataSetThroughAdapter(sql4, out dsHourMinutes, false, "1");
                //dtHourMinutes = dsHourMinutes.Tables[0];
                //int Hours = Convert.ToInt32(dtHourMinutes.Rows[0]["GMTHour"].ToString());
                //int Minutes = Convert.ToInt32(dtHourMinutes.Rows[0]["GMTMinute"].ToString());
                ////moment future
                //if (AttendanceEntry.PType == "IN")
                //{
                //    DateTime dateInTime = DateTime.UtcNow.AddHours(Hours).AddMinutes(Minutes);
                //    TimeSpan timeInTime = new TimeSpan(0, 0, 01, 0);
                //    DateTime combinedInTime = dateInTime.Add(timeInTime);
                //    if (Convert.ToDateTime(combinedInTime) < Convert.ToDateTime(AttendanceEntry.InTime))
                //    {
                //        throw new Exception("Future Time is not allowed..");
                //    }

                //}
                //else
                //{
                //    DateTime date = DateTime.UtcNow.AddHours(Hours).AddMinutes(Minutes);
                //    TimeSpan time = new TimeSpan(0, 0, 01, 0);
                //    DateTime combined = date.Add(time);
                //    if (Convert.ToDateTime(combined) < Convert.ToDateTime(AttendanceEntry.OutTime))
                //    {
                //        throw new Exception("Future Time is not allowed..");
                //    }

                //}

                if (AttendanceEntry.Id == null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    if (AttendanceEntry.PType == "IN")
                    {
                        DataSet dsvalidation;
                        string sql3 = "SELECT EmployeeId, PDate,OutTime,InTime FROM [dbo].[AttdnRawDataFromApp] WHERE EmployeeId = '" + AttendanceEntry.EmployeeId + @"' and PDate = '" + AttendanceEntry.PDate + @"' and InTime is not null ";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql3, out dsvalidation, false, "1");
                        if (dsvalidation.Tables[0].Rows.Count > 0)
                        {
                            Exception ex = new Exception("Already Have In Time In This Work Date");
                            throw (ex);
                        }
                    }
                    else
                    {
                        DataSet dsMaster;
                        string sql2 = "SELECT EmployeeId, PDate,OutTime,InTime FROM [dbo].[AttdnRawDataFromApp] WHERE EmployeeId = '" + AttendanceEntry.EmployeeId + @"' and PDate = '" + AttendanceEntry.PDate + @"' and OutTime is not null ";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql2, out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            Exception ex = new Exception("Already Have Out Time In This Work Date");
                            throw (ex);
                        }
                    }
                }
              
              
                SaveAttendanceEntry(AttendanceEntry);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public void SaveAttendanceEntry(AttendanceEntry AttendanceEntry)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [dbo].[AttdnRawDataFromApp] WHERE EmployeeId='"+ AttendanceEntry.EmployeeId+@"' AND PDate='"+AttendanceEntry.PDate+"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[AttdnRawDataFromApp]", out sID);
                    dr["Id"] = "MA" + sID;
                    dr["PlantId"] = identity.PlantId;
                    dr["EmployeeId"] = AttendanceEntry.EmployeeId;
                    dr["PDate"] = AttendanceEntry.PDate;

                    if (AttendanceEntry.PType == "IN")
                    {
                        if (AttendanceEntry.InTime != null)
                        {
                            dr["InTime"] = AttendanceEntry.InTime;
                        }
                    }
                    if (AttendanceEntry.PType == "OUT")
                    {
                        if (AttendanceEntry.OutTime != null)
                        {
                            dr["OutTime"] = AttendanceEntry.OutTime;
                        }
                    }

                    dr["Latitude"] = AttendanceEntry.Latitude;
                    dr["Longitude"] = AttendanceEntry.Longitude;
                    dr["Remarks"] = AttendanceEntry.Remarks;
                    dr["IsProcessed"] = AttendanceEntry.IsProcessed;
                    dr["IsLocked"] = AttendanceEntry.IsLocked;
                    dr["SourceFlag"] = AttendanceEntry.SourceFlag;
                    dr["INLocationDesc"] = AttendanceEntry.INLocationDesc;
                    dr["OutLocationDesc"] = AttendanceEntry.OutLocationDesc;
                    dr["isApprovedIN"] = AttendanceEntry.isApprovedIN;
                    dr["ApprovedByIN"] = AttendanceEntry.ApprovedByIN;
                    if (AttendanceEntry.ApprovalDateIN !=null)
                    {
                    dr["ApprovalDateIN"] = AttendanceEntry.ApprovalDateIN;
                    }
                    dr["isApprovedOUT"] = AttendanceEntry.isApprovedOUT;
                    dr["ApprovedByOUT"] = AttendanceEntry.ApprovedByOUT;
                    if (AttendanceEntry.ApprovalDateOUT !=null)
                    {
                    dr["ApprovalDateOUT"] = AttendanceEntry.ApprovalDateOUT;
                    }
                    dr["LatitudeOUT"] = AttendanceEntry.LatitudeOUT;
                    dr["LongitudeOUT"] = AttendanceEntry.LongitudeOUT;
                    dr["RemarksOUT"] = AttendanceEntry.RemarksOUT;
                    dr["LocationDesc"] = AttendanceEntry.LocationDesc;
                    dr["SourceFlag"] = "AttendanceEntry";
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {

                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["PlantId"] = identity.PlantId;
                    dr["EmployeeId"] = AttendanceEntry.EmployeeId;
                    dr["PDate"] = AttendanceEntry.PDate;
                    if (AttendanceEntry.PType == "IN")
                    {
                        if (AttendanceEntry.InTime != null)
                        {
                            dr["InTime"] = AttendanceEntry.InTime;
                        }
                    }
                    if (AttendanceEntry.PType == "OUT")
                    {
                        if (AttendanceEntry.OutTime != null)
                        {
                            dr["OutTime"] = AttendanceEntry.OutTime;
                        }
                    }
                   
                    dr["Latitude"] = AttendanceEntry.Latitude;
                    dr["Longitude"] = AttendanceEntry.Longitude;
                    dr["Remarks"] = AttendanceEntry.Remarks;
                    dr["IsProcessed"] = AttendanceEntry.IsProcessed;
                    dr["IsLocked"] = AttendanceEntry.IsLocked;
                    dr["SourceFlag"] = AttendanceEntry.SourceFlag;
                    dr["INLocationDesc"] = AttendanceEntry.INLocationDesc;
                    dr["OutLocationDesc"] = AttendanceEntry.OutLocationDesc;
                    dr["isApprovedIN"] = AttendanceEntry.isApprovedIN;
                    dr["ApprovedByIN"] = AttendanceEntry.ApprovedByIN;
                    if (AttendanceEntry.ApprovalDateIN != null)
                    {
                        dr["ApprovalDateIN"] = AttendanceEntry.ApprovalDateIN;
                    }
                    dr["isApprovedOUT"] = AttendanceEntry.isApprovedOUT;
                    dr["ApprovedByOUT"] = AttendanceEntry.ApprovedByOUT;
                    if (AttendanceEntry.ApprovalDateOUT != null)
                    {
                        dr["ApprovalDateOUT"] = AttendanceEntry.ApprovalDateOUT;
                    }
                    dr["LatitudeOUT"] = AttendanceEntry.LatitudeOUT;
                    dr["LongitudeOUT"] = AttendanceEntry.LongitudeOUT;
                    dr["RemarksOUT"] = AttendanceEntry.RemarksOUT;
                    dr["LocationDesc"] = AttendanceEntry.LocationDesc;
                    dr["SourceFlag"] = "AttendanceEntry";
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

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

        [HttpGet]
        [Authorize]
        public ActionResult GetOffDuty(string empId, string FromDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	select  Id,EmployeeId
                             ,Format(InTime,'dd-MMM-yyyy hh:mm tt')InTime
                             ,Format(OutTime,'dd-MMM-yyyy hh:mm tt')OutTime
                             ,Format(PDate,'dd-MMM-yyyy')PDate
                             ,PDate as PDates
 ,Latitude,Longitude,Remarks,IsProcessed,IsLocked,SourceFlag,INLocationDesc,OutLocationDesc,isApprovedIN,ApprovedByIN,ApprovalDateIN
 ,isApprovedOUT,ApprovedByOUT,ApprovalDateOUT,LatitudeOUT,LongitudeOUT,RemarksOUT,LocationDesc,PType='IN'
                             from [dbo].[AttdnRawDataFromApp] where EmployeeId='" + empId + @"'  and PDate='" + FromDate + @"' ORDER BY  PDates DESC ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetAttendanceEntry(string empId, string FromDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	 select  Id,EmployeeId
                             ,Format(InTime,'dd-MMM-yyyy hh:mm tt')InTime
                             ,Format(OutTime,'dd-MMM-yyyy hh:mm tt')OutTime
                             ,Format(PDate,'dd-MMM-yyyy')PDate
                             ,PDate as PDates
 ,Latitude,Longitude,Remarks,IsProcessed,IsLocked,SourceFlag,INLocationDesc,OutLocationDesc,isApprovedIN,ApprovedByIN,ApprovalDateIN
 ,isApprovedOUT,ApprovedByOUT,ApprovalDateOUT,LatitudeOUT,LongitudeOUT,RemarksOUT,LocationDesc,PType='OUT'
                             from [dbo].[AttdnRawDataFromApp] where EmployeeId='" + empId + @"'  and PDate='" + FromDate + @"' ORDER BY  PDates DESC";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            DataSet dsCheck;
            DataSet dsUpdate;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql1 = @"select * from [dbo].[AttdnRawDataFromApp] where id='" + Id + @"' and OutTime is not null";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsCheck, false, "1");

                if (dsCheck.Tables[0].Rows.Count > 0)
                {
                    string sql2 = @"Update [dbo].[AttdnRawDataFromApp] set InTime=null where id='"+Id+@"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql2, out dsUpdate, false, "1");
                }
                else
                {
                    string sql = @"Delete FROM [dbo].[AttdnRawDataFromApp] WHERE Id='" + Id + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult DeleteOut(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            DataSet dsCheck;
            DataSet dsUpdate;

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql1 = @"select * from [dbo].[AttdnRawDataFromApp] where id='" + Id + @"' and InTime is not null";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsCheck, false, "1");

                if (dsCheck.Tables[0].Rows.Count > 0)
                {
                    string sql2 = @"Update [dbo].[AttdnRawDataFromApp] set OutTime=null where id='" + Id + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql2, out dsUpdate, false, "1");
                }
                else
                {
                    string sql = @"Delete FROM [dbo].[AttdnRawDataFromApp] WHERE Id='" + Id + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations  
        public class AttendanceEntry : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string PlantId { get; set; }
            public string EmployeeId { get; set; }
            public DateTime? PDate { get; set; }
            public DateTime? InTime { get; set; }
            public DateTime? OutTime { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string Remarks { get; set; }
            public bool IsProcessed { get; set; }
            public bool IsLocked { get; set; }
            public string SourceFlag { get; set; }
            public string INLocationDesc { get; set; }
            public string OutLocationDesc { get; set; }
            public bool isApprovedIN { get; set; }
            public string ApprovedByIN { get; set; }
            public DateTime? ApprovalDateIN { get; set; }
            public bool isApprovedOUT { get; set; }
            public string ApprovedByOUT { get; set; }
            public DateTime? ApprovalDateOUT { get; set; }
            public string LatitudeOUT { get; set; }
            public string LongitudeOUT { get; set; }
            public string RemarksOUT { get; set; }
            public string LocationDesc { get; set; }
            public string PType { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }

            #endregion Audit Properties
        }

    }

}