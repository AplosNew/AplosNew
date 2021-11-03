using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Tax;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.Extension.Payroll.Tax;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class OffDutyHoursController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private DataSet dsRef;

        public OffDutyHoursController(
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
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult OffDutyHoursReport()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public ActionResult Save(OffDutyHourMaster DutyHour)
        {
            try
            {
                //PT();
                SaveC(DutyHour); //first get yearly slab for monthly deduction (based on structure for forwarding month but earned amount for the previous month)
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        void PT()
        {
            try
            {
                ProfessionalTax pt = new ProfessionalTax();
                pt.ProcessPT("'207506','2010114'", "202017", 9, 2020);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SaveC(OffDutyHourMaster DutyHour)
        {
            try
            {
                DateTime NewWorkDatePre;
                //string ppDate = DateTime.Now.ToString("dd-MMM-yyyy");
                string FDpre = Convert.ToDateTime(DutyHour.FromDate).ToString("dd-MMM-yyyy");
                NewWorkDatePre = Convert.ToDateTime(FDpre).AddDays(-1);

                if (Convert.ToDateTime(NewWorkDatePre) > Convert.ToDateTime(DutyHour.WorkDate))
                {
                    throw new Exception("Only Previous Day Allow From From Date");
                }

                var code = CheckDayStatus(DutyHour.EmpSystemId, DutyHour.WorkDate.ToString());
                if (code.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Can Avail,Only if Present..");
                }
                //DateTime NewWorkDateNex;
                ////string ppDate = DateTime.Now.ToString("dd-MMM-yyyy");
                //string FDnex = Convert.ToDateTime(DutyHour.FromDate).ToString("dd-MMM-yyyy");
                //NewWorkDateNex = Convert.ToDateTime(FDnex).AddDays(1);

                //if (Convert.ToDateTime(NewWorkDateNex) < Convert.ToDateTime(DutyHour.WorkDate))
                //{
                //    throw new Exception("Only Next Day Allow From From Date");
                //}

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsOffDDutyHours obj = new clsOffDDutyHours();
                DutyHour.AddedBy = identity.Name;
                DutyHour.AddedDate = DateTime.Now;
                DutyHour.PlantId = identity.PlantId;
                DutyHour.UpdatedDate = DateTime.Now;
                DutyHour.UpdatedBy = identity.Name;
                DutyHour.AddedFromIP = identity.IPAddress;
                DutyHour.UpdatedFromIP = identity.IPAddress;
                obj.SaveDutyHour(DutyHour);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataSet CheckDayStatus(string EmpSystemId, string WorkDate)
        {
            string wd = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"   select EmpSystemID,WorkDate,DayStatus
                                 from AttdnProcessData 
                                 where DayStatus in(select DayType from DayType WHERE Category NOT IN ('Present','Late','Half Day'))
                                 AND EmpSystemID='"+ EmpSystemId + "' and WorkDate='"+ wd + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
            return dsRef;
        }//End Function
        
        [HttpGet]
        public ActionResult GetOffDuty(string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	 select  Id,Format(FromDate,'dd-MMM-yyyy hh:mm tt')FromDate,EmpSystemId
                                    ,Format(ToDate,'dd-MMM-yyyy hh:mm tt')ToDate,DurationInMin,IsApprove,HourlyLeaveReasonId
                                ,Format(WorkDate,'dd-MMM-yyyy')WorkDate
                              from HourlyOffDuty where EmpSystemId='" + empId + "' ORDER BY  FromDate DESC ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetShiftInfo(string EmpSystemID, string WorkDate)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsOffDDutyHours ob = new clsOffDDutyHours(_sqlRepository);
                var data = ob.GetShiftInfo(EmpSystemID, WorkDate);

                return Json(new { ShiftInfo = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT Id,UserName FROM HKP.HourlyLeaveReason";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM HourlyOffDuty WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        #region Hourly Leave

        [HttpGet,Authorize]
        public ActionResult GetHourlyLeave(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetHourlyLeave(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, FromDate, ToDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Off Duty Hours";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            

        }
        #endregion Hourly Leave


        #endregion -- Operations  
    }
}