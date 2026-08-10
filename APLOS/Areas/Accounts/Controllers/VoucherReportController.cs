using Aplos.Controllers;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Advances;
using Library.Service.Banks;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Finances;
using Library.Service.FixedAssets;
using Library.Service.Invoices;
using Library.Service.OpeningBalances;
using Library.Service.Organizations;
using Library.MaterialManagement.Reports;
using Library.Service.SalesManagements;
using Library.Service.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Accounting.FixedAssets;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Library.Data;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherReportController : BaseController
    {

        private readonly ISqlRepository _sqlRepository;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        private readonly IInvoiceReportService _invoiceReportService;
        private readonly IEmployeeReportService _employeeReportService;
        private readonly ISalesReportService _salesReportService;
        private readonly ISalesService _salesService;

        private readonly ICashReportService _cashReportService;
        private readonly IBankReportService _bankReportService;
        private readonly IInventoryReceiveReportService _inventoryReportService;
        //private readonly ISqlRepository _sqlRepository;
        private readonly CompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;
        private readonly IVoucherReportService _voucharReportService;
        private readonly IAdjustmentNoteReportService _adjustmentNoteReportService;
        private readonly IAdvanceReportService _advanceReportService;
        private readonly IFixedAssetRegisterService _fixedAssetRegisterService;
        private readonly ILoanReportService _loanReportService;
        private readonly IInvestmentReportService _investmentReportService;
        private readonly IOpeningBalanceService _openingBalanceService;
        private string parallelCurrency;

        public VoucherReportController(ISalesService salesService,
              ISqlRepository sqlRepository, IInventoryReceiveReportService inventoryReportService, ICashReportService cashReportService, IBankReportService bankReportService
             , AccountVoucherReportService accountVoucherReportService, IInvoiceReportService invoiceReportService, IEmployeeReportService employeeReportService, ISalesReportService salesReportService

            , CompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService
            , IVoucherReportService voucharReportService
             , IAdjustmentNoteReportService adjustmentNoteReportService
             , IAdvanceReportService advanceReportService

             , IFixedAssetRegisterService fixedAssetRegisterService
            , ILoanReportService loanReportService
            , IInvestmentReportService investmentReportService
            , IOpeningBalanceService openingBalanceService
            )
        {
            _sqlRepository = sqlRepository;
            _accountVoucherReportService = accountVoucherReportService;
            _invoiceReportService = invoiceReportService;
            _employeeReportService = employeeReportService;
            _cashReportService = cashReportService;
            _bankReportService = bankReportService;
            _inventoryReportService = inventoryReportService;
            _salesReportService = salesReportService;
            _salesService = salesService;

            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
            _voucharReportService = voucharReportService;
            _adjustmentNoteReportService = adjustmentNoteReportService;
            _advanceReportService = advanceReportService;
            _fixedAssetRegisterService = fixedAssetRegisterService;
            _loanReportService = loanReportService;
            _investmentReportService = investmentReportService;
            _openingBalanceService = openingBalanceService;
        }

        public ActionResult DayBookReport()
        {
            return View("~/Areas/Accounts/Views/DayBookReport.cshtml");
        }
        public ActionResult ParkedReport()
        {
            return View("~/Areas/Accounts/Views/ParkedReport.cshtml");
        }

        public ActionResult ExpenseRegisterReport()
        {
            return View("~/Areas/Accounts/Views/ExpenseRegisterReport.cshtml");
        }
        public ActionResult EDReport()
        {
            return View("~/Areas/Accounts/Views/EDReport.cshtml");
        }
        [Authorize]
        public ActionResult GetDayBookReport(ReportFormat reportFormat, DateTime fromdate, DateTime todate, string entityId, string dateType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetDayBooksReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromdate, todate, entityId, dateType);
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
        [Authorize, HttpGet]
        public ActionResult GetParkedReport(ReportFormat reportFormat, DateTime fromdate, DateTime todate,string reportType)
        {
            string reportFileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Syncfusion.XlsIO.IWorkbook workbook = null;
            if(reportType== "Voucher")
            {
                workbook = _accountVoucherReportService.GetVoucherParkedReport(out  reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromdate, todate);
            }
            else if (reportType == "GRN")
            {
                workbook = _accountVoucherReportService.GetGRNParkedReport(out reportFileName, identity.PlantId);
            }
            else if (reportType == "Issue")
            {
                workbook = _accountVoucherReportService.GetIssueParkedReport(out reportFileName, identity.PlantId);
            }
            else if (reportType == "Service")
            {
                workbook = _accountVoucherReportService.GetServiceParkedReport(out reportFileName, identity.PlantId);
            }
            else
            {
                workbook = _accountVoucherReportService.GetServiceTDSReport(out reportFileName, identity.PlantId);
            }

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

        [Authorize]
        public ActionResult GetExpenseRegisterReport(ReportFormat reportFormat, string fromdate, string todate, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetExpenseRegisterReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromdate, todate, entityId);
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
        public ActionResult OpeningBalanceReport(string parallelCurrency)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Opening Balance Report " + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _openingBalanceService.GetOpeningBalanceReport(identity.CompanyId, identity.PlantName, new JavaScriptSerializer().Deserialize<string[]>(parallelCurrency));
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }


        [Authorize, HttpGet]
        public ActionResult GetCommonVoucherReport(ReportFormat reportFormat, string compnayGroupId, string companyId, string plantId, string sourceType, string voucherId, string inventoryIssueId, string inventoryReceiveId, string salesSourceType, string invoiceWriteOffGroupNo, string openingBalanceId,string otherVendorId)
        {
            string reportFileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string plantName = _sqlRepository.GetDataTable(@"SELECT * FROM ORG.Plant WHERE Id = '" + plantId + @"'").Rows[0]["UserName"].ToString();

            Syncfusion.XlsIO.IWorkbook workbook = null; // _accountVoucherReportService.GetExpenseRegisterReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromdate, todate, entityId);
            /*1*/
            if (sourceType.ToUpper() == SourceType.VendorPayment.ToString().ToUpper())
                workbook = _invoiceReportService.GetVendorPaymentReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId);
            /*2*/
            if (sourceType.ToUpper() == SourceType.EmployeePayment.ToString().ToUpper())
                workbook = _employeeReportService.GetEmployeePayment(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId);
            /*3*/
            if (sourceType.ToUpper() == SourceType.CashJournal.ToString().ToUpper())
                workbook = _cashReportService.GetCashJVReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId, SourceType.CashJournal);
            /*4*/
            if (sourceType.ToUpper() == SourceType.BankJournal.ToString().ToUpper())
                workbook = _bankReportService.GetPaymentByBankReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId, SourceType.BankJournal);
            /*5*/
            //if (sourceType.ToUpper() == SourceType.BankJournal.ToString().ToUpper())
            // workbook = _bankReportService.GetBankJournalReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId, SourceType.BankJournal);
            /*6*/
            if (sourceType.ToUpper() == SourceType.VendorInvoice.ToString().ToUpper())
                workbook = _invoiceReportService.GetVendorInvoiceReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId, sourceType);
            /*7*/
            if (sourceType.ToUpper() == SourceType.CustomerInvoice.ToString().ToUpper())
                workbook = _invoiceReportService.GetCustomerInvoiceReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId);

            /*8*/
            if (sourceType.ToUpper() == SourceType.CustomerInvoice.ToString().ToUpper())
                workbook = _invoiceReportService.GetCustomerInvoiceReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId);
            /*9*/
            if (sourceType.ToUpper() == SourceType.CustomerReceipt.ToString().ToUpper())
                workbook = _invoiceReportService.GetCustomerInvoiceReceiptReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId, SourceType.CustomerReceipt.ToString());
            /*10 */
            if (sourceType.ToUpper() == SourceType.CustomerReceipt.ToString().ToUpper())
                workbook = _invoiceReportService.GetCustomerInvoiceReceiptReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId, SourceType.CustomerReceipt.ToString());
            /*11*/
            if (sourceType.ToUpper() == SourceType.EmployeePayable.ToString().ToUpper())
                workbook = _employeeReportService.GetEmployeePayableExpenseBookingReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId);
            /*12*/
            //if (sourceType.ToUpper() == SourceType.SalesInvoice.ToString().ToUpper())
            //{
            //    AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService, _plantService);
            //    workbook = _accountsSalesReportService.GetSalesInvoiceReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId);
            //}
            /*13*/
            if (sourceType.ToUpper() == SourceType.SalaryJournal.ToString().ToUpper())
                workbook = _accountVoucherReportService.GetSalaryJournalVoucherReport(out reportFileName, compnayGroupId, companyId, plantId, plantName, voucherId);
            /*14*/
            if (sourceType.ToUpper() == SourceType.IssueJournal.ToString().ToUpper())
            {
                AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);

                reportFileName = "Inventory Issue Journal";
                workbook = accountsInventoryPayableReportService.IssueJournal(reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, inventoryIssueId);
            }

            /*15*/
            if (sourceType.ToUpper() == SourceType.JournalVoucher.ToString().ToUpper())
            {
                // reportFileName = "Inventory Issue Journal";
                // workbook = _inventoryReportService.IssueJournal(reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, inventoryIssueId);
                workbook = _voucharReportService.GetGeneralVoucher(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);

            }
            /*16*/
            if (sourceType.ToUpper() == SourceType.DebitNote.ToString().ToUpper())
            {
                // reportFileName = "Inventory Issue Journal";
                // workbook = _inventoryReportService.IssueJournal(reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, inventoryIssueId);
                // workbook = _voucharReportService.GetGeneralVoucher(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
                workbook = _adjustmentNoteReportService.GetDebitNoteReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.DebitNote);
            }
            /*17*/
            if (sourceType.ToUpper() == SourceType.CreditNote.ToString().ToUpper())
            {
                // reportFileName = "Inventory Issue Journal";

                workbook = _adjustmentNoteReportService.GetCreditNoteReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CreditNote);
            }
            /*18*/
            if (sourceType.ToUpper() == SourceType.DebitNoteSetOff.ToString().ToUpper())
            {
                // reportFileName = "Inventory Issue Journal";
                // workbook = _adjustmentNoteReportService.GetCreditNoteReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CreditNote);
                workbook = _adjustmentNoteReportService.DebitNoteSetOffReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.DebitNoteSetOff);
            }
            /*19*/
            if (sourceType.ToUpper() == SourceType.CreditNoteSetOff.ToString().ToUpper())
            {
                // reportFileName = "Inventory Issue Journal";
                // workbook = _adjustmentNoteReportService.GetCreditNoteReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CreditNote);
                workbook = _adjustmentNoteReportService.CreditNoteSetOffReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CreditNoteSetOff);
            }
            /*20*/
            if (sourceType.ToUpper() == SourceType.SalaryJournal.ToString().ToUpper())
            {
                workbook = _accountVoucherReportService.GetSalaryJournalVoucherReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            }
            /*21*/
            //if (sourceType.ToUpper() == SourceType.SalesInvoice.ToString().ToUpper())
            //{
            //    AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService, _plantService);
            //    workbook = _accountsSalesReportService.GetMasterOrderSalesPostReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            //}

            if (sourceType.ToUpper() == SourceType.SalesInvoice.ToString().ToUpper())
            {
                AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService, _plantService);
                if (salesSourceType != null)
                {

                    if (salesSourceType.ToUpper() == "MasterOrderSales".ToUpper())
                    {
                        //AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService, _plantService);
                        workbook = _accountsSalesReportService.GetMasterOrderSalesPostReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);

                    }
                    else if (salesSourceType.ToUpper() == "Sales".ToUpper())
                    {
                        workbook = _accountsSalesReportService.GetMasterOrderSalesPostReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);

                    }
                    //else if (salesSourceType.ToUpper() == "Sales".ToUpper())
                    //{
                    //   workbook = _openingBalanceService.GetOpeningBalanceReport(identity.CompanyId, identity.PlantName, new JavaScriptSerializer().Deserialize<string[]>(parallelCurrency));
                    //}
                }

                else
                {
                    workbook = _accountsSalesReportService.GetSalesPostingReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
                }

            }

            /*22*/
            if (sourceType.ToUpper() == SourceType.CustomerAdvance.ToString().ToUpper())
            {
                workbook = _advanceReportService.GetAdvanceReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CustomerAdvance);
            }
            /*23*/
            if (sourceType.ToUpper() == SourceType.CustomerAdvanceWriteOff.ToString().ToUpper())
            {
                workbook = _advanceReportService.GetCustomerAdvanceWriteOffReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            }
            /*24*/
            if (sourceType.ToUpper() == SourceType.InventoryPayable.ToString().ToUpper())
            {
                AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
                reportFileName = "GRN";
                workbook = accountsInventoryPayableReportService.PabyableJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, null, false, false, reportFileName, otherVendorId);

            }
            /*25*/
            if (sourceType.ToUpper() == SourceType.ServicePayable.ToString().ToUpper())
            {
                AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
                workbook = accountsInventoryPayableReportService.GetServicePayableReportSheet(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            }
            /*26*/
            if (sourceType.ToUpper() == SourceType.PurchaseLCOpeningCharges.ToString().ToUpper())
            {
                workbook = _invoiceReportService.GetPurchaseLCChargesReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            }
            /*27*/
            if (sourceType.ToUpper() == SourceType.PurchaseDocAcceptance.ToString().ToUpper())
            {
                workbook = _invoiceReportService.DocumentAcceptanceVoucher(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            }
            /*28*/
            if (sourceType.ToUpper() == SourceType.FixedAssetCapitalizeJournal.ToString().ToUpper())
            {
                workbook = _fixedAssetRegisterService.GetFixedAssetCapitalizeJournalReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, salesSourceType);
            }
            /*28*/
            if (sourceType.ToUpper() == SourceType.VendorAdvance.ToString().ToUpper())
            {
                workbook = _advanceReportService.GetAdvanceReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.VendorAdvance);
            }
            /*29*/
            if (sourceType.ToUpper() == SourceType.VendorAdvanceWriteOff.ToString().ToUpper())
            {
                workbook = _advanceReportService.GetVendorAdvanceWriteOffReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            }
            /*29*/
            if (sourceType.ToUpper() == SourceType.VendorInvoiceCharge.ToString().ToUpper())
            {
                AccountsInvoiceReportService _accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
                workbook = _accountsInvoiceReportService.GetVendorInvoiceChargeReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            }
            /*29*/
            if (sourceType.ToUpper() == SourceType.EmployeeAdvance.ToString().ToUpper())
            {
                workbook = _advanceReportService.GetEmployeeAdvanceReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.EmployeeAdvance);
            }
            /*30*/
            if (sourceType.ToUpper() == SourceType.EmployeeAdvanceWriteOff.ToString().ToUpper())
            {
                workbook = _advanceReportService.GetEmployeeAdvanceWriteOffReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            }
            /*31*/
            if (sourceType.ToUpper() == SourceType.Loan.ToString().ToUpper())
            {
                workbook = _loanReportService.GetLoanReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantName, identity.PlantId, voucherId, SourceType.Loan.ToString());
            }
            /*32*/
            if (sourceType.ToUpper() == "OtherExpensesPayable".ToString().ToUpper())
            {
                workbook = _loanReportService.GetLoanInterestPayableReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, sourceType);
            }
            /*33*/
            if (sourceType.ToUpper() == "LoanInterestPayableReverse".ToString().ToUpper())
            {
                workbook = _loanReportService.GetLoanInterestPayableReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, sourceType);
            }
            /*34*/
            if (sourceType.ToUpper() == SourceType.LoanPayment.ToString().ToUpper())
            {
                workbook = _loanReportService.GetLoanWriteOffReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.LoanPayment.ToString());
            }
            /*35*/
            if (sourceType.ToUpper() == SourceType.Investment.ToString().ToUpper())
            {
               
                workbook = _investmentReportService.GetInvestmentReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantName, identity.PlantId, voucherId, SourceType.Investment.ToString());
            }
            /*36*/
            if (sourceType.ToUpper() == SourceType.CustomerBanksReceipt.ToString().ToUpper())
            {
                
                AccountsInvoiceReportService _accInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
                reportFormat = ReportFormat.Excel;
                workbook = _accInvoiceReportService.GetCustomerInvoiceReceiptBanksReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, invoiceWriteOffGroupNo,SourceType.CustomerBanksReceipt.ToString());
            }
            /*37*/
            if (sourceType.ToUpper() == SourceType.OpeningBalance.ToString().ToUpper())
            {
                reportFormat = ReportFormat.Excel;
                //workbook = _openingBalanceService.GetOpeningBalanceReport(identity.CompanyId, identity.PlantName, new JavaScriptSerializer().Deserialize<string[]>(parallelCurrency));
                workbook = _voucharReportService.GetOBAdvanceJournalVoucher(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, openingBalanceId);

            }




            return formatReportDownload(workbook, reportFormat, reportFileName);
        }




        private ActionResult formatReportDownload(Syncfusion.XlsIO.IWorkbook workbook, ReportFormat reportFormat, string reportFileName)
        {
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


        public ActionResult AssetWIPStatusReport()
        {
            return View("~/Areas/Accounts/Views/AssetWIPStatusReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetAssetWIPData()
        {
            AssetWIPQueryService assetWIPQueryService = new AssetWIPQueryService();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = assetWIPQueryService.GetFixedAssetWIPstatusSQL(), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult AssetWIPstatusReportExcel(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
        {
            AssetWIPQueryService assetWIPQueryService = new AssetWIPQueryService();

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                //IWorkbook workbook = assetWIPQueryService.AssetWIPstatusList( materialMasterId, materialMasterArticleId, voucherId, grnNo, glId, activityId);

                //string strFileName = "Fixed Assets Register Report.xlsx";
                //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //workbook.Close();

                string fileName = "";
          
                fileName = assetWIPQueryService.AssetWIPstatusList(materialMasterId, materialMasterArticleId, voucherId, grnNo, glId, activityId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

        }

        //[HttpGet, Authorize]
        //public ActionResult AssetWIPstatusReportPdf(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
        //{
        //    AssetWIPQueryService assetWIPQueryService = new AssetWIPQueryService(_sqlRepository);
        //    //string PartyType, string PartyId, string MaterialMasterId, string FixedAssetsId, string FromDate, string ToDate
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    try
        //    {
        //        // if (string.IsNullOrEmpty(MasterLCList))
        //        //   throw new Exception("Please select at least one master Order");

        //        ExcelEngine excelEngine = new ExcelEngine();

        //        IWorkbook workbook = assetWIPQueryService.AssetWIPstatusList(materialMasterId, materialMasterArticleId, voucherId, grnNo, glId, activityId);
        //        // string strFileName = "Fixed Assets Register Report.pdf";
        //        string strFileName = "Fixed Assets Register Report.xlsx";
        //        ExcelToPdfConverter convert = new ExcelToPdfConverter(workbook);
        //        PdfDocument pdfDoc = convert.Convert();
        //        workbook.Close();
        //        pdfDoc.Save(strFileName, System.Web.HttpContext.Current.Response, HttpReadType.Save);
        //        //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);

        //    }
        //    return null;
        //}

         [HttpPost, Authorize]
        public ActionResult GetIssueQtyList(string inventoryReceiveDetailId)
        {
            try
            {
                AssetWIPQueryService assetWIPQueryService = new AssetWIPQueryService();
                var data = assetWIPQueryService.GetIssueQtyList(inventoryReceiveDetailId);
                return Json(new { Data = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public JsonResult GetExpenseDistribution(string fromDate, string toDate)
        {
            try
            {
                if (fromDate == null || fromDate == "")
                {
                    throw new CustomException("Select From Date");
                }
                else if (toDate == null || toDate == "")
                {
                    throw new CustomException("Select To Date");
                }
                AccountsInvoiceReportService _accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository); 
                var jsondata = Json(_accountsInvoiceReportService.GetExpenseDistributionSql(fromDate, toDate), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult WeeklyReceiptAndPaymentStatement()
        {
            return View("~/Areas/Accounts/Views/WeeklyReceiptAndPaymentStatement.cshtml");
        }

        [HttpGet]
        public ActionResult GetWeeklyReceiptAndPaymentStatement(ReportFormat reportFormat, DateTime fromdate, DateTime todate, string cashMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInvoiceReportService _accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var workbook = _accountsInvoiceReportService.GetWeeklyReceiptAndPaymnetWorkBook(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromdate, todate, cashMasterId,null);
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