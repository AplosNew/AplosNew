using Aplos.Controllers;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using Library.Service.Currencies;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Aplos.Areas.Accounts.Controllers
{
    public class AccountStatusDashboardController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;


        public AccountStatusDashboardController(ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService)
        {
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
        }

        public ActionResult PartyPaymentStatus()
        {
            return View("~/Areas/Accounts/Views/FinancialStatusDashboard/PartyPaymentStatus.cshtml");
        }


        public ActionResult CustomerReceivableInvoiceDetail()
        {
            return View("~/Areas/Accounts/Views/FinancialStatusDashboard/CustomerReceivableInvoiceDetail.cshtml");
        }
        //MasterGride for party Payment Status

        [HttpPost, Authorize]
        public ActionResult GetPartyPaymentStatusInvoiceList()
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        }

        //summary Report Downloard
        [HttpGet, Authorize]
        public ActionResult PartyPaymentStatusReport(string[] MasterLCList)
        {

            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //    throw new Exception("Please select at least one Invoice");

                string masterLCList = "";

                foreach (var item in MasterLCList)
                {
                    if (string.IsNullOrEmpty(masterLCList))
                    {
                        masterLCList += "''," + item;
                    }
                    else
                    {
                        masterLCList += "," + item;
                    }

                }

                //if (string.IsNullOrEmpty(masterLCList))
                //   throw new Exception("Please select at least one Invoice");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsStatusDashboardService.GetPartyPaymentStatusReport(excelEngine, masterLCList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "PayableSummary.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        //PartyPaymentStatusAgingReport
        [Authorize]
        public ActionResult PartyPaymentStatusAgingReport(string MasterLCList)
        {

            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //throw new Exception("Please select at least one Invoice");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsStatusDashboardService.GetPartyPaymentStatusAgingReport(excelEngine, MasterLCList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name);
                // return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
                string strFileName = "PayableAging.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        //Pia Chart and donught for aging 

        //GetPartyPaymentStatusAgingList
        [HttpPost, Authorize]
        public ActionResult GetPartyPaymentStatusAgingList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyPaymentStatusAgingPiaChartData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #region donught PopUp  get data
        [HttpGet, Authorize]
        public ActionResult GetPartyAgingDueList(string overDueDetailAmount)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyAgingDueData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, overDueDetailAmount), JsonRequestBehavior.AllowGet);
        }
        //Voucher Print for over due more due 30 PopUp
        [HttpGet, Authorize]
        public ActionResult GetPartyAgingDueVoucherPrintList(string partyId, string setOffDetailAgingType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyAgingDueVoucherPrintList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, setOffDetailAgingType), JsonRequestBehavior.AllowGet);
        }

        //SetOff Detail donught get data 
        //[HttpGet, Authorize]
        //public ActionResult GetpartyPaymentSetOffDetailList(string invoiceId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository);

        //    return Json(accountsStatusDashboardService.GetpartyPaymentSetOffDetailList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, invoiceId), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost, Authorize]
        public ActionResult GetpartyPaymentSetOffDetailList(string setOffPaymentDetailAgingType, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetpartyPaymentSetOffDetailList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, setOffPaymentDetailAgingType,partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyVendorPayableNoOfInvoiceDetailList(string partyId, string vendorPayableInvoiceDetailAgingType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyVendorPayableNoOfInvoiceDetailList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, vendorPayableInvoiceDetailAgingType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAgingNoOfInvoiceSetOffDetilList(string partyId, string invoiceId, string setOffDetailAgingType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetAgingNoOfInvoiceSetOffDetilList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, invoiceId, setOffDetailAgingType), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetInvoiceDetailSetOffPaymentDetailPopUp(string setOffPaymentDetailAgingType, string partyId, string invoiceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetInvoiceDetailSetOffPaymentDetailPopUp(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, setOffPaymentDetailAgingType, partyId, invoiceId), JsonRequestBehavior.AllowGet);
        }
        #endregion 
        //Payable Tab Master Gride Data

        [HttpPost, Authorize]
        public ActionResult getDateRangeWisePayableData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(new { DATA = accountsStatusDashboardService.getDateRangeWisePayableData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);

        }

        //Payable report date range wise
        [HttpGet, Authorize]
        public ActionResult GetDateRangeWiseReport(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //IWorkbook workbook = IssueReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkbox);
                // IWorkbook workbook = OperationReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                IWorkbook workbook = accountsStatusDashboardService.GetDateRangeWiseReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);

                string strFileName = "DateRangePayableList.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        //Payment Tab for Master Gride Data 
        [HttpPost, Authorize]
        public ActionResult getDateRangeWisePaymentData(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            // AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(new { DATA = accountsStatusDashboardService.getDateRangeWisePaymentData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);

        }
        //Payment report master gride date range wise

        [HttpGet, Authorize]
        public ActionResult GetDateRangeWisePaymentReport(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsStatusDashboardService.GetDateRangeWisePaymentReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);
                string strFileName = "DateRangePaymentList.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }
        //Payment PopUp Data

        [HttpPost, Authorize]
        public ActionResult getDateRangeWisePaymentPopUpData(string id, string type, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);


            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(new { DATA = accountsStatusDashboardService.GetPartyPaymentDetailPopUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, id, type, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);

        }
        //Payment detail PopUp  Report 
        //GetDateRangeWiseDetailPaymentPoPUpReport
        [HttpGet, Authorize]
        public ActionResult GetDateRangeWiseDetailPaymentPoPUpReport(string fromDate, string toDate, string id, string type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsStatusDashboardService.GetDateRangeWiseDetailPaymentPoPUpReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, id, type);
                string strFileName = "DateRangePaymentDetailList.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        //Payment Steake bar Chart and Grape sheet
        [HttpPost, Authorize]
        public ActionResult getDateRangeWisePaymentDataBarChart(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.getDateRangeWisePaymentDataBarChart(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate), JsonRequestBehavior.AllowGet);

        }

        #region Fixed assets
       
        [HttpPost, Authorize]
        public ActionResult GetFixedAssetsList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(new { DATA = accountsStatusDashboardService.GetFixedAssetsListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetFixedArticalList(string materialMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(new { DATA = accountsStatusDashboardService.GetFixedArticalListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult MaterialMasterReport2(/*string MaterialTypeId, bool Article*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.MaterialMasterReport2(/*MaterialTypeId, Article*/);


                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        [HttpGet, Authorize]
        public ActionResult MaterialMasterArticalReport(/*string MaterialTypeId, bool Article*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.MaterialMasterArticalReport(/*MaterialTypeId, Article*/);


                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }


        [HttpPost, Authorize]
        public ActionResult GetFixedAssetsRegisterPopUpList(string materialMasterId, string materialMasterArticleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetFixedAssetsRegisterPopUpList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId, materialMasterArticleId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult getFixedAssetRegisterReport(/*string MaterialTypeId, bool Article*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.getFixedAssetRegisterReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        #endregion

        #region Cash Tab
        //getMasterCashListData
        [HttpPost, Authorize]
        public ActionResult getMasterCashListData(/*string fromDate,*/ string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json( accountsStatusDashboardService.getMasterCashListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,/* fromDate,*/ toDate), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Bank Tab
        //getMasterCashListData
        [HttpPost, Authorize]
        public ActionResult getBankMasterListData(/*string fromDate,*/ string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.getBankMasterListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,/* fromDate,*/ toDate), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Loan Taken

        [HttpPost, Authorize]
        public ActionResult getLoanListData(string transactionType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json( accountsStatusDashboardService.getLoanListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,transactionType), JsonRequestBehavior.AllowGet);

        }

    
        [HttpGet, Authorize]
        public ActionResult getLoanTakenSetOffPopUpData( string financingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json( accountsStatusDashboardService.GetLoanTakenSetOffPopUpData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, financingId), JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult getLoanTakenInterestPopUpData(string financingId )
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetLoanTakenInterestPopUpData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, financingId), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult getLoanTakenChargesPayablePopUpData(string financingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetLoanTakenChargesPayablePopUpData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, financingId), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult getLoanTakenAdditionalLoanPayablePopUpData(string financingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetLoanTakenAdditionalLoanPayablePopUpData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, financingId), JsonRequestBehavior.AllowGet);

        }
        //Loan Register Report

        [HttpGet, Authorize]
        public ActionResult GetLoanRegisterLedgerReport(ReportFormat reportFormat, TransactionType transactionType, string voucherId, string financingId)
        {
            if (financingId == null)
                throw new CustomException("Please Select Interest !");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            var workbook = accountsStatusDashboardService.GetLoanRegisterLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, transactionType, voucherId, financingId);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Loan Register";
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

        #endregion Loan taken

        #region Customer Tab

        [HttpPost, Authorize]
        public ActionResult GetFinancialDashboardCustomerReceiptMasterList()
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetFinancialDashboardCustomerReceiptMasterListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult FinancialDashboardCustomerSummaryReport(string[] masterCustomerSummaryList)
        {

            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //    throw new Exception("Please select at least one Invoice");

                string masterCustomerReceiptList = "";

                foreach (var item in masterCustomerSummaryList)
                {
                    if (string.IsNullOrEmpty(masterCustomerReceiptList))
                    {
                        masterCustomerReceiptList += "''," + item;
                    }
                    else
                    {
                        masterCustomerReceiptList += "," + item;
                    }

                }

                //if (string.IsNullOrEmpty(masterLCList))
                //   throw new Exception("Please select at least one Invoice");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsStatusDashboardService.GetFinancialDashboardCustomerReceiptSummaryReport(excelEngine, masterCustomerReceiptList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "CustomerReceivableSummary.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        [Authorize]
        public ActionResult FinancialDashboardCustomerReceiptAgingReport(string masterCustomerReceiptAgingList)
        {

            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //throw new Exception("Please select at least one Invoice");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsStatusDashboardService.GetFinancialDashboardCustomerReceiptAgingReport(excelEngine, masterCustomerReceiptAgingList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name);
                // return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
                string strFileName = "CustomerReceivableAging.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        //get customer receivable Pia chart List
        [HttpPost, Authorize]
        public ActionResult GetFinancialDashboardCustomerReceivablePaiChartList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetFinancialDashboardCustomerReceivablePaiChartListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult getCustomerReceivableInvoiceDetailData(string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"SELECT IV.PartyId, IV.PartyPlantId,p.Code PartyCode, P.UserName PartyName, PP.UserName AS PartyPlantName
               ,V.Id VoucherId ,V.VoucherNo,V.DocRefNo InvoiceNo,V.SourceType
		        , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate 
				, REPLACE(CONVERT(VARCHAR(11),iv.DocDate, 106), ' ', '-') AS DocDate
				, REPLACE(CONVERT(VARCHAR(11),iv.ActualDueDate , 106), ' ', '-') AS ActualDueDate 
				 ,C.Code TrnCurrency
                , ISNULL(IVD.Amount,0) AS Gross
				,0 CreditNoteAmount
				,isnull( DIWD.DiscountAmount,0)as TranDiscountAmount
				, isnull( IWD.TaxAmount,0)TaxAmount 
                , SetOff=ISNULL(IVD.WrittenOffAmount, 0) -ISNULL(IWD.TaxAmount,0)-isnull( DIWD.DiscountAmount,0)
				, ISNULL(IVD.Amount-IVD.WrittenOffAmount,0) AS Balance

						 , ISNULL(IVD.Amount*IV.CompanyCurrencyRate,0) AS BooksGross
						 ,0 BooksCreditNoteAmount
						 ,isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0)as BooksDiscountAmount
				    ,ISNULL(IWD.TaxAmount*IV.CompanyCurrencyRate,0) BooksTaxAmount
				 ,ISNULL(IVD.WrittenOffAmount*IV.CompanyCurrencyRate,0)-ISNULL(IWD.TaxAmount*IV.CompanyCurrencyRate,0)-isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
           , ISNULL((IVD.Amount*IV.CompanyCurrencyRate)-(IVD.WrittenOffAmount*IV.CompanyCurrencyRate),0) AS BooksBalance

                        ,NULL InventorySalesId  
						
				--, ISNULL(IVD.Amount,0) AS GrossTranAmount
				--,0 DebitNoteTranAmount
				--,isnull( IWD.Amount,0)as TranAmount


                FROM [TRN].[InvoiceDetail] AS IVD
                LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id

			LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) DiscountAmount  FROM TRN.InvoiceWriteOffDetail wd 
					    LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Discount'
								group by wd.InvoiceDetailId
								) DIWD ON DIWD.InvoiceDetailId=IVD.Id


                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='"+identity.CompanyId+@"'
                ) AS CC ON CC.VoucherDetailId=VD.Id

                WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice')
                 AND IV.CompanyGroupId = '" + identity.CompanyGroupId + "' AND IV.CompanyId = '" + identity.CompanyId + "' AND IV.PlantId = '" + identity.PlantId + @"'
                --GROUP BY IV.PartyId, IV.PartyPlantId, PP.UserName,P.UserName
                 AND IV.PartyId in('" + partyId+@"')

                UNION ALL
                SELECT IV.PartyId, IV.PartyPlantId,p.Code PartyCode, P.UserName PartyName, PP.UserName AS PartyPlantName
                ,V.Id VoucherId ,V.VoucherNo,V.DocRefNo InvoiceNo,V.SourceType
	        	, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
				, REPLACE(CONVERT(VARCHAR(11),iv.DocDate, 106), ' ', '-') AS DocDate
				, REPLACE(CONVERT(VARCHAR(11),iv.ActualDueDate , 106), ' ', '-') AS ActualDueDate ,C.Code TrnCurrency

                , ISNULL(IVD.Amount,0) AS Gross
				,0 CreditNoteAmount
				,isnull( DIWD.DiscountAmount,0)as TranDiscountAmount
				, IWD.TaxAmount TaxAmount
                , SetOff=ISNULL(IVD.WrittenOffAmount, 0) -ISNULL(IWD.TaxAmount,0)-isnull( DIWD.DiscountAmount,0)
				, ISNULL(IVD.Amount - IVD.WrittenOffAmount, 0) AS Balance
				 , ISNULL(IVD.Amount*IV.CompanyCurrencyRate,0) AS BooksGross
				  ,0 BooksCreditNoteAmount
				 ,isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0)as BooksDiscountAmount
				 ,ISNULL(IWD.TaxAmount*IV.CompanyCurrencyRate,0) BooksTaxAmount
				 ,ISNULL(IVD.WrittenOffAmount*IV.CompanyCurrencyRate,0)-ISNULL(IWD.TaxAmount*IV.CompanyCurrencyRate,0)-isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
                , ISNULL((IVD.Amount*IV.CompanyCurrencyRate)-(IVD.WrittenOffAmount*IV.CompanyCurrencyRate),0) AS BooksBalance
                ,ISNULL( IVS.Id ,'')InventorySalesId

                FROM[TRN].[InvoiceDetail] AS IVD
             --   LEFT JOIN[TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id
				  LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN[HKP].[Party] AS P ON P.Id = IV.PartyId
                LEFT JOIN[HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
                LEFT JOIN[TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
                LEFT JOIN[TRN].[Voucher] AS V ON V.Id = VD.VoucherId
                LEFT JOIN[SCS].[Currency] AS C ON C.Id = IV.CurrencyId
                LEFT JOIN[ORG].[Entity] AS EN ON EN.Id = IV.EntityId
                LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id

			LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) DiscountAmount  FROM TRN.InvoiceWriteOffDetail wd 
					    LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Discount'
								group by wd.InvoiceDetailId
								) DIWD ON DIWD.InvoiceDetailId=IVD.Id

                 LEFT JOIN TRN.InventorySales IVS ON IVS.Id=IV.InventorySalesId

                LEFT JOIN(
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId= VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType= 'CompanyCurrency' AND CPC.CompanyId= '"+identity.CompanyId+@"'
                ) AS CC ON CC.VoucherDetailId = VD.Id

                WHERE IV.Archive = 0 AND IV.IsWrittenOff = 0 AND IVD.IsWrittenOff = 0 AND V.IsPark = 0 AND IVD.IsBlock = 0 AND IV.SourceType in ('InventorySales')
                 AND IV.CompanyGroupId = '" + identity.CompanyGroupId + "' AND IV.CompanyId = '" + identity.CompanyId + "' AND IV.PlantId = '" + identity.PlantId + @"'
               -- AND IR.PurchaseDocumentAcceptanceId IS NULL
                AND IV.PartyId in('" + partyId+ @"')
                UNION ALL
                SELECT IV.PartyId, IV.PartyPlantId,p.Code PartyCode, P.UserName PartyName, PP.UserName AS PartyPlantName
                ,V.Id VoucherId ,V.VoucherNo,V.DocRefNo InvoiceNo,V.SourceType
	        	, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
				, REPLACE(CONVERT(VARCHAR(11),iv.DocDate, 106), ' ', '-') AS DocDate
				, REPLACE(CONVERT(VARCHAR(11),iv.PostingDate , 106), ' ', '-') AS ActualDueDate ,C.Code TrnCurrency

                , ISNULL(IVD.Amount,0) AS Gross
				,0 CreditNoteAmount
				,isnull( DIWD.DiscountAmount,0)as TranDiscountAmount
				,ISNULL( IWD.TaxAmount ,0)TaxAmount
                , SetOff=ISNULL(IVD.WrittenOffAmount, 0) -ISNULL(IWD.TaxAmount,0)-isnull( DIWD.DiscountAmount,0)
				, ISNULL(IVD.Amount - IVD.WrittenOffAmount, 0) AS Balance
				 , ISNULL(IVD.Amount,0) AS BooksGross
				  ,0 BooksCreditNoteAmount
				 ,isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0)as BooksDiscountAmount
				  ,ISNULL(IWD.TaxAmount,0) BooksTaxAmount
				 ,ISNULL(IVD.WrittenOffAmount,0)-ISNULL(IWD.TaxAmount,0)-isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
               , ISNULL((IVD.Amount)-(IVD.WrittenOffAmount),0) AS BooksBalance
                ,ISNULL( IVS.Id,'') InventorySalesId

                --FROM [TRN].[AdjustmentNoteDetail] AS IVD
                --LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId = IV.Id

				   FROM[TRN].[InvoiceDetail] AS IVD
             --   LEFT JOIN[TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id
				  LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id = IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
              --  LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId = IVD.Id
			    LEFT JOIN[TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id = VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id = IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id = IV.EntityId
                LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id

			LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) DiscountAmount  FROM TRN.InvoiceWriteOffDetail wd 
					    LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Discount'
								group by wd.InvoiceDetailId
								) DIWD ON DIWD.InvoiceDetailId=IVD.Id

                 LEFT JOIN TRN.InventorySales IVS ON IVS.Id=IV.InventorySalesId
                LEFT JOIN(
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId= VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType= 'CompanyCurrency' AND CPC.CompanyId= '" + identity.CompanyId+@"'
                ) AS CC ON CC.VoucherDetailId = VD.Id

                WHERE IV.Archive = 0 AND IV.IsWrittenOff = 0 AND IVD.IsWrittenOff = 0 AND V.IsPark = 0  AND IV.SourceType in ('CreditNote','CustomerReceipt')
                AND IV.CompanyGroupId = '"+identity.CompanyGroupId+"' AND IV.CompanyId = '"+identity.CompanyId+"' AND IV.PlantId = '"+identity.PlantId+@"'
                AND IV.PartyId in('"+partyId+@"')
                order by P.UserName";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //get data for Aging Table popUp
        [HttpGet, Authorize]
        public ActionResult GetCustomerReceivableAgingDueList(string overDueDaysSlot)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetCustomerReceivableAgingDueList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, overDueDaysSlot), JsonRequestBehavior.AllowGet);
        }

        //CR Set Off detail popUp get data
        [HttpGet, Authorize]
        public ActionResult GetCustomerReceivableSetOffDetailList(string partyId, string crDueDaysSetOffDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetCustomerReceivableSetOffDetailList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, crDueDaysSetOffDetail), JsonRequestBehavior.AllowGet);
        }
        //CR Payment detail popup and get data
        [HttpPost, Authorize]
        public ActionResult GetCRPaymentDetailPopUpList(string crPaymentDetailDueDays, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetCRPaymentDetailPopUpList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, crPaymentDetailDueDays, partyId), JsonRequestBehavior.AllowGet);
        }

        //CR Aging Invoice Detail 
        [HttpGet, Authorize]
        public ActionResult GetCustomerReceivableAgingInvoiceDetailList(string partyId, string crAgingInvoiceDetailDueDaya)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetCustomerReceivableAgingInvoiceDetailList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, crAgingInvoiceDetailDueDaya), JsonRequestBehavior.AllowGet);
        }

        //CR aging Invoice SetOff Detail popUp data
        [HttpGet, Authorize]
        public ActionResult GetCustomerReceivableInvoiceSetOffDetailList(string partyId, string crDueDaysInvoiceSetOffDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetCustomerReceivableInvoiceSetOffDetailList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, crDueDaysInvoiceSetOffDetail), JsonRequestBehavior.AllowGet);
        }

        //get CR Invoice Payment detail PopUp data
        [HttpPost, Authorize]
        public ActionResult GetCRInvoicePaymentDetailPopUp(string crPaymentDetailDueDays, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetCRInvoicePaymentDetailPopUp(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, crPaymentDetailDueDays, partyId), JsonRequestBehavior.AllowGet);
        }
        #endregion Customer Tab

        #region Trian Balance

        [HttpPost, Authorize]
        //public ActionResult TrialBalanceReport(ReportFormat reportFormat, string date, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        public ActionResult getTrialBalanceData(string toDate, bool isBudgetLevel, bool isActivityLevel, bool IsDetailLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
           return Json(accountsStatusDashboardService.getTrialBalanceData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate, isBudgetLevel, isActivityLevel, IsDetailLevel), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult getLedgerActivityPoPUpListData(string gLInfoId, string budgetMasterId,  string activityId, string partyId, string partyPlantId, string bankMasterId, string cashMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.getLedgerActivityPoPUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, gLInfoId, budgetMasterId, activityId, partyId, partyPlantId, bankMasterId,cashMasterId,toDate), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetBankMasterLedgerHeading(string gLInfoId, string budgetMasterId, string activityId, string partyId, string partyPlantId, string bankMasterId, string cashMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetBankMasterLedgerHeading(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, gLInfoId, budgetMasterId, activityId, partyId, partyPlantId, bankMasterId, cashMasterId, toDate), JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult GetCashMasterLedgerHeading(string gLInfoId, string budgetMasterId, string activityId, string partyId, string partyPlantId, string bankMasterId, string cashMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetCashMasterLedgerHeading(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, gLInfoId, budgetMasterId, activityId, partyId, partyPlantId, bankMasterId, cashMasterId, toDate), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetPartyLedgerHeading(string gLInfoId, string budgetMasterId, string activityId, string partyId, string partyPlantId, string bankMasterId, string cashMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyLedgerHeading(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, gLInfoId, budgetMasterId, activityId, partyId, partyPlantId, bankMasterId, cashMasterId, toDate), JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult GetGeneralLedgerHeading(string gLInfoId, string budgetMasterId, string activityId, string partyId, string partyPlantId, string bankMasterId, string cashMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetGeneralLedgerHeading(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, gLInfoId, budgetMasterId, activityId, partyId, partyPlantId, bankMasterId, cashMasterId, toDate), JsonRequestBehavior.AllowGet);

        }


        //public Dictionary<string, object> GetBankMasterLedgerHeading(string bankMasterId)
        //{
        //    var sql = @"SELECT BM.Id, BM.AccountTitle, BM.AccountNumber, BM.CurrencyId, C.Code AS CurrencyCode, B.UserName AS BankName, BB.UserName AS BankBranchName, GLGI.AccountCode AS GLGeneralInfoCode
        //        , GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName
        //        FROM [MST].[BankMaster] AS BM
        //        LEFT JOIN [SCS].[Currency] AS C ON C.Id=BM.CurrencyId
        //        LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
        //        LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
        //        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
        //        LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=BM.BudgetMasterId
        //        LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
        //        LEFT JOIN [HKP].[Activity] AS A ON A.Id=BM.ActivityId
        //        WHERE BM.Id='" + bankMasterId + "'";
        //        return _sqlRepository.GetData(sql);
        //}

        #endregion Trial Balance

        #region Cash In Flow
        [HttpPost, Authorize]
        public ActionResult GetCashInFlowReceivableMasterList()
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetCashInFlowReceivableMasterList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        }
        #endregion Cash In Flow


        #region Cash Out Flow

        [HttpPost, Authorize]
        public ActionResult GetCashOutFlowMasterList()
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetCashOutFlowMasterList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        }
        #endregion Cash Out Flow

        #region Material Management
        public ActionResult GetGRNPostingList(string grnAndAccpType, string dateType, string fromDate, string toDate,bool isOrderSpecific,bool isNonOrderSpecific)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetGRNPostingList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,  grnAndAccpType, dateType, fromDate,  toDate,  isOrderSpecific,  isNonOrderSpecific), Error = false }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        public ActionResult GetAcceptancePostingList(string grnAndAccpType, string dateType, string fromDate, string toDate, bool isOrderSpecific, bool isNonOrderSpecific)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetAcceptancePostingList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, grnAndAccpType, dateType, fromDate, toDate, isOrderSpecific, isNonOrderSpecific), Error = false }, JsonRequestBehavior.AllowGet);
        }

        //GRN Posted Report
        [HttpGet, Authorize]
        public ActionResult GRNPostedReport( string grnAndAccpType, string dateType, string fromDate, string toDate, bool isOrderSpecific, bool isNonOrderSpecific)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.GRNPostedReport(identity.CompanyGroupId,  identity.CompanyId,  identity.PlantId,  grnAndAccpType,  dateType,  fromDate,  toDate,  isOrderSpecific,  isNonOrderSpecific);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
        //AcceptancePostedReport
        [HttpGet, Authorize]
        public ActionResult AcceptancePostedReport(string grnAndAccpType, string dateType, string fromDate, string toDate, bool isOrderSpecific, bool isNonOrderSpecific)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.AcceptancePostedReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, grnAndAccpType, dateType, fromDate, toDate, isOrderSpecific, isNonOrderSpecific);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
        #endregion Material Management

        #region Acceptance Liability Maturity

        [HttpPost, Authorize]
        public ActionResult GetAcceptanceLiabilityMaturityList( string toDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetAcceptanceLiabilityMaturityList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate), Error = false }, JsonRequestBehavior.AllowGet);
        }


        //[Authorize, HttpGet]
        //public JsonResult GetAutoLoanAvailableList(bool dateRange, string fromDate, string toDate)
        //{
        //    AccountsAutoLoanService accountsAutoLoanService = new AccountsAutoLoanService(_sqlRepository);
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var jsondata = Json(accountsAutoLoanService.GetAutoLoanAvailableList(identity.PlantId, dateRange, fromDate, toDate), JsonRequestBehavior.AllowGet);
        //    jsondata.MaxJsonLength = int.MaxValue;
        //    return jsondata;

        //}


        [HttpGet, Authorize]
        public ActionResult getAcceptanceLiabilityMaturityReport(string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.getAcceptanceLiabilityMaturityReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,toDate);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        #endregion Acceptance Liability Maturity

        #region Acceptance Liability 

        [HttpPost, Authorize]
        public ActionResult GetAcceptanceLiabilityList(string toDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetAcceptanceLiabilityList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getAcceptanceLiabilityReport(string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.getAcceptanceLiabilityReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        #endregion Acceptance Liability 

        #region Others liability
        [HttpPost, Authorize]
        public ActionResult GetOthersLiabilityDataList(string ToDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetOthersLiabilityDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,ToDate), Error = false }, JsonRequestBehavior.AllowGet);
        }

        //Others Liability Summary Report
        [HttpGet, Authorize]
        public ActionResult OthersLiabilitySummaryReport(string toDate, bool isWithAdvance)
        {

            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //    throw new Exception("Please select at least one Invoice");

                //string othersLiabList = "";

                //foreach (var item in othersLiabilityList)
                //{
                //    if (string.IsNullOrEmpty(othersLiabList))
                //    {
                //        othersLiabList += "''," + item;
                //    }
                //    else
                //    {
                //        othersLiabList += "," + item;
                //    }

                //}

                //if (string.IsNullOrEmpty(masterLCList))
                //   throw new Exception("Please select at least one Invoice");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                ExcelEngine excelEngine = new ExcelEngine();
         
                if (isWithAdvance)
                {
                    //var workbook = "";
                    IWorkbook workbook = accountsStatusDashboardService.GetOthersLiabilityWithAdvanceSummaryReport(excelEngine, toDate, isWithAdvance, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                    string strFileName = "OthersLiabilityWithAdvanceSummary.xlsx";
                    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                    workbook.Close();
                }
                else
                {
                    IWorkbook workbok = accountsStatusDashboardService.GetOthersLiabilitySummaryReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                    string strFileName = "OthersLiabilitySummary.xlsx";
                    workbok.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                    workbok.Close();
                }
       
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        [Authorize]
        public ActionResult OthersLiabilityAgingDetailReport(string toDate, bool isWithAdvance)
        {

            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //throw new Exception("Please select at least one Invoice");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

                ExcelEngine excelEngine = new ExcelEngine();
                if (isWithAdvance)
                {
                    IWorkbook workbook = accountsStatusDashboardService.GetOthersLiabilityAgingDetailWithAdvanceReport(excelEngine, toDate, isWithAdvance, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name);
                    // return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
                    string strFileName = "OthersLiabilityWithAdvanceAgingDetail.xlsx";
                    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                    workbook.Close();
                }
                else
                {
                    IWorkbook workbook = accountsStatusDashboardService.GetOthersLiabilityAgingDetailReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name);
                    // return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
                    string strFileName = "OthersLiabilityAgingDetail.xlsx";
                    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                    workbook.Close();


                }
     
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        #endregion Others Liability
    }
}