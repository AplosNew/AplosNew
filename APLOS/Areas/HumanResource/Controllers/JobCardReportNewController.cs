using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Syncfusion.DocIO.DLS;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using Library.HumanResource.NewAttendanceProcess;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class JobCardReportNewController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly ISqlRepository _sqlRepository;
        public JobCardReportNewController(
              IAttendanceManagementService AttendanceManagementService, ISqlRepository R
            )
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

        #region Get Compliance Job Card Report  -----real job card
        [HttpGet]
        public ActionResult GetComplianceJobCardReport(ReportFormat reportFormat, string[] employeeId, string fromDate, string toDate, bool chkAdditionInfo)
        {
            try
            {
                NewJobCardReportService app = new NewJobCardReportService();
                string EmpIdLoop = "";
                foreach (string item in employeeId)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "" + item + ""; ;
                    }
                    
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = app.GetComplianceJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EmpIdLoop, fromDate, toDate, chkAdditionInfo);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:                      
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }                      
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;

                    case ReportFormat.PdfView:
                        PdfDocument document1 = new PdfDocument();
                        ExcelToPdfConverterSettings settings1 = new ExcelToPdfConverterSettings();
                        settings1.TemplateDocument = document1;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document1 = converter1.Convert(settings1);
                        }
                        document1.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                        //return RenderReportAsPdf(document1, reportFileName);
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
                //throw new Exception(ex.Message);
            }
        }

        [HttpGet,Authorize]
        public ActionResult GetComplianceJobCardReportView(string EmpIdLoop, string fromDate, string toDate, bool chkAdditionInfo)
        {
            try
            {
                NewJobCardReportService app = new NewJobCardReportService();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = app.GetComplianceJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EmpIdLoop, fromDate, toDate, chkAdditionInfo);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);
                
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
               
            }
        }
        #endregion
       
      
        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string fromDate, string toDate, string criteria)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            JsonResult json = Json(_AttendanceManagementService.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, fromDate, toDate, criteria), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

            //return Json(_AttendanceManagementService.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, fromDate, toDate, criteria), JsonRequestBehavior.AllowGet);
        }

    }
}