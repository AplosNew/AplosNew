using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.Machines.Controllers
{
    public class SpecialIssueControlRegisterController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        ResudeceStatusReportService rsr = new ResudeceStatusReportService();
        private readonly ISqlRepository _sqlRepository;
        public SpecialIssueControlRegisterController(IAttendanceManagementService AttendanceManagementService, ISqlRepository R)
        {
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region Special Issue Control Register    

        [Authorize, HttpGet]
        public JsonResult GetShiftList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select distinct SD.SystemID as Value,SD.UserName as Text from [MST].[SpecialIssueDefinePeriod] SIDP
 left join ShiftDefination SD ON SD.SystemID=SIDP.Shift";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSpecialIssueMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT *,format(SIC.TargetDate,'dd-MMM-yyyy') as TDate,MonitoringPeriod as MonitoringPeriods,
(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=SIC.ResponsiblePersonId) as ResponsiblePerson
 FROM TRN.SpecialIssueControl SIC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetIssueControlJobCardReportView(string Shift, string IssueId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetIssueControlJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, Shift, IssueId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        public ActionResult RenderReportAsPdf(IWorkbook workbook, string fileName, bool isOpen = true)
        {
            try
            {
                using (var converter = new ExcelToPdfConverter(workbook))
                {
                    var pdfDocument = new PdfDocument();
                    ExcelToPdfConverterSettings _settings = new ExcelToPdfConverterSettings();
                    _settings.AutoDetectComplexScript = true;
                    _settings.EmbedFonts = true;
                    _settings.LayoutOptions = LayoutOptions.FitAllColumnsOnOnePage;

                    pdfDocument = converter.Convert(_settings);

                    if (isOpen == true)
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                    else
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
