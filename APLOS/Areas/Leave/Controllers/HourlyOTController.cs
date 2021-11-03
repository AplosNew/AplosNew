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
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class HourlyOTController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;


        public HourlyOTController(
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
        public ActionResult HourlyOtReport()
        {
            return View();
        }
        public ActionResult HourlyOtReportMonth()
        {
            return View();
        }
        public ActionResult IndividualDailyOt()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public ActionResult Save(HourlyOt HourlyOt)
        {
            try
            {
                DateTime NewWorkDate;
                string ot = Convert.ToDateTime(HourlyOt.FromDate).ToString("dd-MMM-yyyy");
                NewWorkDate = Convert.ToDateTime(ot).AddDays(-1);

                if (Convert.ToDateTime(NewWorkDate) > Convert.ToDateTime(HourlyOt.WorkDate))
                {
                    throw new Exception("Only Previous Day Allow From From Date");
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsHourlyOt obj = new clsHourlyOt();
                HourlyOt.AddedBy = identity.Name;
                HourlyOt.AddedDate = DateTime.Now;
                HourlyOt.PlantId = identity.PlantId;
                HourlyOt.UpdatedDate = DateTime.Now;
                HourlyOt.UpdatedBy = identity.Name;
                HourlyOt.AddedFromIP = identity.IPAddress;
                HourlyOt.UpdatedFromIP = identity.IPAddress;

                obj.SaveDutyHour(HourlyOt);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public ActionResult GetOffDuty(string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	 select  Id,EmpSystemId,
                Format(FromDate,'dd-MMM-yyyy hh:mm tt')FromDate,Format(ToDate,'dd-MMM-yyyy hh:mm tt')ToDate,Duration,Format(WorkDate,'dd-MMM-yyyy')WorkDate
                              from HourlyOT where EmpSystemId='" + empId + "' and OTType='DiscreteOT' ORDER BY  FromDate DESC ";
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
                string sql = @"Delete FROM HourlyOT WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
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

        #region Hourly ot Report

        [HttpGet]
        public ActionResult GetHourlyOT(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetHourlyOT(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, FromDate, ToDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Hourly Ot";
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
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Hourly ot Report Monthly 

        [HttpGet]
        public ActionResult GetHourlyOTMonthly(ReportFormat reportFormat, string YearNo, string MonthNo, bool isActive, bool isSeperated)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetHourlyOTMonthly(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, YearNo, MonthNo, isActive, isSeperated);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Hourly Ot Monthly";
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
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }
        #endregion

        #region Individual OT Report

        [HttpGet, Authorize]
        public ActionResult GetIndividualDailyOT(ReportFormat reportFormat, string FromDate, string ToDate, string OTDuration, string OTfinal, bool CheckBox)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetIndividualDailyOT(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, FromDate, ToDate, OTDuration, CheckBox, OTfinal, "");
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Hourly Ot Monthly";
                //switch (reportFormat)
                //{
                //    case ReportFormat.Pdf:
                //        return RenderReportAsPdf(workbook, reportFileName);

                //    case ReportFormat.Excel:
                //        return RenderReportAsExcel(workbook, reportFileName);

                //    default:
                //        return RenderReportAsExcel(workbook, reportFileName);
                //}
                workbook.SaveAs(reportFileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion  Individual OT

        #endregion -- Operations  
    }
}