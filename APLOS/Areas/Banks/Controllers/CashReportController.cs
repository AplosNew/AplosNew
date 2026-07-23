
using Aplos.Controllers;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Banks;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class CashReportController : BaseController
    {
        private readonly ICashReportService _cashReportService;
        private readonly ISqlRepository _sqlRepository;
        public CashReportController(ICashReportService cashReportService, ISqlRepository sqlRepository)
        {
            _cashReportService = cashReportService;
            _sqlRepository = sqlRepository;
        }

      
        public ActionResult CashOpeningBalanceLedger()
        {
            return View("~/Areas/Banks/Views/CashOpeningBalanceLedger.cshtml");
        }


        //cash journal report
        [HttpGet, Authorize]
        public ActionResult GetCashJournalReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetCashJVReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CashJournal);
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
        public ActionResult GetCashPaymentReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetCashJVReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CashJournal);
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
        public ActionResult GetCashReceiptReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetCashJVReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.ReceiptByCash);
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
        public ActionResult GetCashOpeningBalanceLedgerReport(string fiscalYearId, bool isCompanyCurrency)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetCashOpeningBalanceLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearId, isCompanyCurrency);
            workbook.SaveAs(DateTime.Now.ToString("yy") + " Cash Opening Balance Ledger.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

       
        public ActionResult CashLedgerReport()
        {
            return View("~/Areas/Banks/Views/CashLedgerReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetCashLedgerReport(ReportFormat reportFormat, string cashMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetCashBookReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, cashMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Cash Ledger";
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
        public ActionResult GetCashLedgerReportCompanyLevel(ReportFormat reportFormat, string cashMasterId, string fromDate, string toDate)
        {
            AccountsCashReportService accountsCashReportService = new AccountsCashReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsCashReportService.GetCashBookReportCompanyLevel(identity.CompanyGroupId, identity.CompanyId, cashMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Cash Ledger";
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

        #region Cash Receipt Payment Report

        [Authorize, HttpGet]
        public ActionResult CashReceiptPaymentReport()
        {
            return View("~/Areas/Banks/Views/CashReceiptPaymentReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetCashReceiptPayment(ReportFormat reportFormat, string cashMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetCashReceiptPaymentReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, cashMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Cash Receipt & Payment";
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


        #endregion Cash Receipt Payment Report

       
        public ActionResult CashBookReport()
        {
            return View("~/Areas/Banks/Views/CashBookReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetCashBookReport(ReportFormat reportFormat, string cashMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetCashLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, cashMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Cash Ledger";
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
        public ActionResult GetAdvanceCashBookReport(ReportFormat reportFormat, string cashMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetAdvanceCashBookReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, cashMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Cash Book";
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

        public ActionResult MonthlyExpenseAndAssetStatement()
        {
            return View("~/Areas/Banks/Views/MonthlyExpenseAndAssetStatement.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetMontlyExpensesAndAssetStatement(ReportFormat reportFormat, DateTime fromdate, DateTime todate, string entityId, string dateType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _cashReportService.GetMontlyExpensesAndAssetWorkBook(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromdate, todate);
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
    }
}