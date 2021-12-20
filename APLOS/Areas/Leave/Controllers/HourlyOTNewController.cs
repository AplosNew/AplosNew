using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewOTProcess;
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
    public class HourlyOTNewController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;


        public HourlyOTNewController(
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

       
        public ActionResult HourlyOtReport()
        {
            return View();
        }
        public ActionResult HourlyOtReportMonth()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        #region Hourly OT Report

        [HttpGet]
        public ActionResult GetHourlyOT(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                HourlyOTReportService ot = new HourlyOTReportService();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = ot.GetHourlyOT(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, FromDate, ToDate);
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
                HourlyOTReportService ot = new HourlyOTReportService();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = ot.GetHourlyOTMonthly(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, YearNo, MonthNo, isActive, isSeperated);
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