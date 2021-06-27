using Aplos.Controllers;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Banks;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankReportController : BaseController
    {
        private readonly IBankReportService _bankReportService;
        private readonly ISqlRepository _sqlRepository;
        public BankReportController(
            IBankReportService bankReportService, ISqlRepository sqlRepository)
        {
            _bankReportService = bankReportService;
            _sqlRepository = sqlRepository;
        } 



       
        public ActionResult BankOpeningBalanceLedger()
        {
            return View("~/Areas/Banks/Views/BankOpeningBalanceLedger.cshtml");
        }




        [HttpGet, Authorize]
        public ActionResult GetBankJournalReport (ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook =  _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.BankJournal);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPaymentByBankReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.PaymentByBank);
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



        [HttpGet, Authorize]
        public ActionResult GetReceiptByBankReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.ReceiptByBank);
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

        [HttpGet, Authorize]
        public ActionResult GetBankOpeningBalanceLedgerReport(string fiscalYearId, bool isCompanyCurrency)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetBankOpeningBalanceLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearId, isCompanyCurrency);
            workbook.SaveAs(DateTime.Now.ToString("yy") + " Bank Opening Balance Ledger.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }

        
        public ActionResult BankLedgerReport()
        {
            return View("~/Areas/Banks/Views/BankLedgerReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetBankLedgerReport(ReportFormat reportFormat, string bankMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetBankLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, bankMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Bank Ledger";
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

        
        public ActionResult BankReconcileReport()
        {
            return View("~/Areas/Banks/Views/BankReconcileReport.cshtml");
        }
        [HttpGet, Authorize]
        public ActionResult GetBankReconcileReport(ReportFormat reportFormat, string bankMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetBankReconcileReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, bankMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Bank Ledger";
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

       
        public ActionResult BankBookReport()
        {
            return View("~/Areas/Banks/Views/BankBookReport.cshtml");
        }

        public ActionResult BankSheetGeneration()
        {
            return View("~/Areas/Banks/Views/BankSheetGeneration.cshtml");
        }

        #region Bank Sheet Generation Report
        [HttpGet, Authorize]
        public ActionResult GetBankSheetGenerationReport( string fromDate, string toDate, string bankMasterId)
        {
             var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsBankService accountsBankService = new AccountsBankService(_sqlRepository);
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = accountsBankService.GetBankSheetGenerationReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, bankMasterId);
                string strFileName = "BankSheetGeneration.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        #endregion Bank Sheet Generation Report



        [HttpGet, Authorize]
        public ActionResult GetBankBookReport(ReportFormat reportFormat, string bankMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetBankBookReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, bankMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Bank Book";
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

       
    }
}