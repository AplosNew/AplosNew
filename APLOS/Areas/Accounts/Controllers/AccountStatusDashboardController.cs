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
using System.Data;
using System.Web.Script.Serialization;
using Aplos.Helpers;
using System.Collections.Specialized;
using System.IO;
using System.Web.Hosting;
using Library.Service.Helpers;
using Library.Security.Core;
using Library.Accounting.FixedAssets;
using Library.Core;

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
        public ActionResult GetPartyPaymentStatusInvoiceList(string fromDate, string toDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
        }

        //summary Report Downloard
        [HttpPost, Authorize]
        public ActionResult PartyPaymentStatusReport(List<Dictionary<string, object>> MasterLCList, string fromDate, string toDate)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                ExcelEngine excelEngine = new ExcelEngine();
                var workbook = accountsStatusDashboardService.GetPartyPaymentStatusReport(/*excelEngine,*/ MasterLCList,  fromDate,  toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "PayableSummary.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


        }


        //PartyPaymentStatusAgingReport
        [HttpPost, Authorize]
        public ActionResult PartyPaymentStatusAgingReport(Dictionary<string, string> parameters, string fromDate, string toDate)
        {

            try
            {
               
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                string fileName = "";
                fileName = accountsStatusDashboardService.GetPartyPaymentStatusAgingReport(parameters, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "PayableAging");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


        }
       

    //GetPartyPaymentStatusAgingList
    [HttpPost, Authorize]
        public ActionResult GetPartyPaymentStatusAgingList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyPaymentStatusAgingPiaChartData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetPartyPaymentStatusPendingAdjustmentData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyPaymentStatusPendingAdjustmentData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetPartyReceiveStatusPendingAdjustmentData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyReceiveStatusPendingAdjustmentData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
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

       
        //Payable report date range wise
    

        //Payment Tab for Master Gride Data 
        [HttpPost, Authorize]
        public ActionResult GetDateRangeWisePaymentData(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            // AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(new { DATA = accountsStatusDashboardService.GetDateRangeWisePaymentData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);

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
        public ActionResult GetDateRangeWisePaymentPopUpData(string id, string type, string fromDate, string toDate)
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
        public ActionResult GetDateRangeWisePaymentDataBarChart(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetDateRangeWisePaymentDataBarChart(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate), JsonRequestBehavior.AllowGet);

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

        [HttpPost, Authorize]
        public ActionResult MaterialMasterReport2(string materialMasterId, string materialTypeId, string assetMasterId, string materialGroup1Id, string baseUOMId, string isAsset, string isMachine, string process, string skillId, string faCount)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            try
            {
                string fileName = "";
                fileName = accountsStatusDashboardService.MaterialMasterReport2(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId, materialTypeId, assetMasterId, materialGroup1Id, baseUOMId, isAsset, isMachine, process, skillId, faCount);
                //return null;
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        //[HttpGet, Authorize]
        //public ActionResult MaterialMasterReport2(/*string MaterialTypeId, string materialMasterId, string materialGroupMasterId, string materialCategoryId, string materialSubCategoryId, string materialGroup1Id*/)
        //{


        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

        //    try
        //    {
        //        accountsStatusDashboardService.MaterialMasterReport2( /*MaterialTypeId,  materialMasterId,  materialGroupMasterId,  materialCategoryId,  materialSubCategoryId,  materialGroup1Id*/);


        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;

        //    }
        //}

        [HttpGet, Authorize]
        public ActionResult MaterialMasterFilteringReport(string materialMasterId, string materialTypeId, string assetMasterId, string materialGroup1Id, string baseUOMId, string isAsset, string machine, string process, string skillId, string fACount)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.MaterialMasterFilteringReport(identity.CompanyGroupId,identity.CompanyId,identity.PlantId, materialMasterId, materialTypeId, assetMasterId, materialGroup1Id, baseUOMId, isAsset, machine, process, skillId, fACount);


                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
        

        [HttpGet, Authorize]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }

        //[HttpGet, Authorize]
        //public ActionResult MaterialMasterArticalReport(/*string MaterialTypeId, bool Article*/)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

        //    try
        //    {
        //        accountsStatusDashboardService.MaterialMasterArticalReport(/*MaterialTypeId, Article*/);


        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;

        //    }
        //}

        [HttpPost, Authorize]
        public ActionResult MaterialMasterArticalReport(string materialMasterId, string materialTypeId, string assetMasterId, string materialGroup1Id, string baseUOMId, string isAsset, string isMachine, string process, string skillId, string fACount)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                //accountsStatusDashboardService.MaterialMasterArticalReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId, materialTypeId, assetMasterId, materialGroup1Id, baseUOMId, isAsset, isMachine, process, skillId, fACount);
                //return null;

                string fileName = "";
                fileName = accountsStatusDashboardService.MaterialMasterArticalReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId, materialTypeId, assetMasterId, materialGroup1Id, baseUOMId, isAsset, isMachine, process, skillId, fACount);
                //return null;
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

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


        [HttpPost, Authorize]
        public ActionResult GetFixedAssetRegisterReport(string materialMasterId, string materialTypeId, string assetMasterId, string materialGroup1Id, string baseUOMId, string isAsset, string isMachine, string process, string skillId, string fACount)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            try
            {
                //accountsStatusDashboardService.GetFixedAssetRegisterReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                //return null;

                string fileName = "";
                fileName = accountsStatusDashboardService.GetFixedAssetRegisterReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId, materialTypeId, assetMasterId, materialGroup1Id, baseUOMId, isAsset, isMachine, process, skillId, fACount);
                //return null;
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetMasterCashListData(/*string fromDate,*/ string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json( accountsStatusDashboardService.GetMasterCashListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,/* fromDate,*/ toDate), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Bank Tab
        //getMasterCashListData
        [HttpPost, Authorize]
        public ActionResult GetBankMasterListData(/*string fromDate,*/ string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetBankMasterListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,/* fromDate,*/ toDate), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Loan Taken

        [HttpPost, Authorize]
        public ActionResult GetLoanListData(string transactionType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json( accountsStatusDashboardService.GetLoanListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,transactionType), JsonRequestBehavior.AllowGet);

        }

    
        [HttpGet, Authorize]
        public ActionResult GetLoanTakenSetOffPopUpData( string financingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json( accountsStatusDashboardService.GetLoanTakenSetOffPopUpData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, financingId), JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult GetLoanTakenInterestPopUpData(string financingId )
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetLoanTakenInterestPopUpData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, financingId), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetLoanTakenChargesPayablePopUpData(string financingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetLoanTakenChargesPayablePopUpData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, financingId), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetLoanTakenAdditionalLoanPayablePopUpData(string financingId)
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
        public ActionResult GetFinancialDashboardCustomerReceiptMasterList(string fromDate, string toDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetFinancialDashboardCustomerReceiptMasterListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCustomerListForConfirmation(GridParameter parameters, string FromDate, string ToDate, string PaymentStatus)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetCustomerListForConfirmation(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, FromDate, ToDate, PaymentStatus), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult FinancialDashboardCustomerSummaryReport(List<Dictionary<string, object>> masterCustomerSummaryList, string fromDate, string toDate)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                var workbook = accountsStatusDashboardService.GetFinancialDashboardCustomerReceiptSummaryReport(masterCustomerSummaryList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);
                string strFileName = "CustomerReceivableSummary.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        [Authorize]
        public ActionResult FinancialDashboardCustomerReceiptAgingReport(string masterCustomerReceiptAgingList, string fromDate, string toDate)
        {

            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //throw new Exception("Please select at least one Invoice");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsStatusDashboardService.GetFinancialDashboardCustomerReceiptAgingReport(excelEngine, masterCustomerReceiptAgingList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name, fromDate, toDate);
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
        public ActionResult GetCustomerReceivableInvoiceDetailData(string partyId)
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
        public ActionResult GetTrialBalanceData(string toDate, bool isBudgetLevel, bool isActivityLevel, bool IsDetailLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
           return Json(accountsStatusDashboardService.GetTrialBalanceData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate, isBudgetLevel, isActivityLevel, IsDetailLevel), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public JsonResult ExcelExportJson(object obj, string ReportHeader = "")
        {
            //Json
            try
            {
                DataTable dt = new DataTable("APIDATA");
                var json = new JavaScriptSerializer().Serialize(obj);

                if (json != "[]")
                {
                    json = json.Replace("\\", "");

                    dt = CustomJsonResult.ToDataTable(json);
                }

                StringCollection strCol = new StringCollection();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Columns[i].ColumnName.ToUpper().Contains("ID") || dt.Columns[i].ColumnName.ToUpper().Contains("PK") || dt.Columns[i].ColumnName.ToUpper().Contains("EJVALUE"))
                    {
                        strCol.Add(dt.Columns[i].ColumnName);
                    }
                }
                foreach (string item in strCol)
                {
                    dt.Columns.Remove(item);
                }

                string filename = GridToExcelReport(dt, ReportHeader);


                return Json(new { FileName = filename, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }
        private string GridToExcelReport(DataTable data, string ReportHeader)
        {
            string fileName = "GRID" + System.DateTime.Now.Ticks.ToString() + ".xlsx";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~/") + fileName);

                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2013;
                    IWorkbook workbook = application.Workbooks.Create(1);
                    IWorksheet sheet = workbook.Worksheets[0];

                    int ROW = 1;
                    ROW++;
                    ROW++;
                    ROW++;
                    sheet[ROW, 1].Text = ReportHeader;
                    sheet[ROW, 1].CellStyle.Font.Bold = true;

                    ROW++;



                    //sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    //sheet.UsedRange.CellStyle.Font.Size = 8f;
                    ReportUtility reportUtility = new ReportUtility();
                    //reportUtility.PlantHeader(ref worksheet, endCol, " Last 10 Days Payment List Created", PlantId);
                    reportUtility.PlantHeader(ref sheet, 1, /*"From " + fromDate + " To " + toDate + */ "", identity.PlantId);
                    //oRU.SetText(ref sheet, 5, 2, "From Date " + fromDate + " To Date " + toDate + "", ExcelHAlign.HAlignCenter);

                    //reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                    //sheet[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                   // sheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    //sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    //sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    //sheet.IsGridLinesVisible = false;


                    sheet.ImportDataTable(data, true, ROW, 1);
                    sheet[ROW, 1, ROW, data.Columns.Count].BorderAround(ExcelLineStyle.Hair);
                    sheet[ROW, 1, ROW, data.Columns.Count].BorderInside(ExcelLineStyle.Hair);
                    sheet[ROW, 1, ROW, data.Columns.Count].CellStyle.ColorIndex = ExcelKnownColors.Gold;
                    sheet[ROW, 1, ROW, data.Columns.Count].CellStyle.Font.Bold = true;

                    //ROW++;
                    //sheet[ROW, 1].Text = "Total";
                    //sheet[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    //sheet[ROW, 1].Formula = "SUM(" + clsStaticInfo.GetxlsCol(1) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(1) + (ROW - 1).ToString() + ")";
                    sheet[ROW, 1].NumberFormat = "#,##0.00;(#,##0.00)";
                    //sheet.Range[ROW, 1, ROW, 1].CellStyle.Font.Bold = true;
                    //sheet[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    workbook.SaveAs(fullPath);

                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

            }
            return fileName;
        }

        [HttpGet, Authorize]
        public ActionResult Download(string FileName)
        {
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();
                string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(fullPath);
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(FileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }

        

        [HttpGet, Authorize]
        public ActionResult getLedgerAllLevelDRPoPUpListData(string particulars, string gLInfoId, string budgetMasterId,  string activityId,  string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.getLedgerAllLevelDRPoPUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, particulars, gLInfoId, budgetMasterId, activityId,toDate), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetAllLevelBankMasterLedgerHeading(string particulars,string gLInfoId, string budgetMasterId, string activityId, string bankMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetBankMasterLedgerHeading(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, particulars,gLInfoId, budgetMasterId, activityId, bankMasterId, toDate), JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult GetCashMasterLedgerHeading(string particulars,string gLInfoId, string budgetMasterId, string activityId, string cashMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetCashMasterLedgerHeading(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, particulars,gLInfoId, budgetMasterId, activityId, cashMasterId, toDate), JsonRequestBehavior.AllowGet);

        }



        [HttpGet, Authorize]
        public ActionResult GetPartyLedgerHeading( string particulars,string gLInfoId, string budgetMasterId, string activityId, string partyId, string partyPlantId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetPartyLedgerHeading(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, particulars, gLInfoId, budgetMasterId, activityId, partyId, partyPlantId, toDate), JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult GetGeneralLedgerAllLevelDRHeading(string particulars, string gLInfoId, string budgetMasterId, string activityId,  string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            return Json(accountsStatusDashboardService.GetGeneralLedgerAllLevelDRHeading(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, particulars,gLInfoId, budgetMasterId, activityId,  toDate), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetPartyLedgerAllLevelPoPUpListData(string particulars, string gLInfoId, string budgetMasterId, string activityId,string partyId, string partyPlantId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetPartyLedgerAllLevelPoPUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, particulars, gLInfoId, budgetMasterId, activityId,partyId,partyPlantId, toDate), JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult GetCashLedgerAllLevelPoPUpListData(string particulars, string gLInfoId, string budgetMasterId, string activityId, string cashMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetCashLedgerAllLevelPoPUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, particulars, gLInfoId, budgetMasterId, activityId, cashMasterId, toDate), JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult GetBankLedgerAllLevelPoPUpListData(string particulars, string gLInfoId, string budgetMasterId, string activityId, string bankMasterId, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetBankLedgerAllLevelPoPUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, particulars, gLInfoId, budgetMasterId, activityId, bankMasterId, toDate), JsonRequestBehavior.AllowGet);

        }


        [HttpGet, Authorize]
        public ActionResult GetAccountGroupPoPUpListData(string accountGroupId, string accountGroupName, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(accountsStatusDashboardService.GetAccountGroupPoPUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, accountGroupId, accountGroupName,toDate), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetTrialBLAccountGroupReport(string toDate /*, bool isWithAdvance*/)
        {

            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                ExcelEngine excelEngine = new ExcelEngine();

                //if (isWithAdvance)
                //{
                //    //var workbook = "";
                //    IWorkbook workbook = accountsStatusDashboardService.GetAcceptanceLiabilityWithAdvanceSummaryReport(excelEngine, toDate, /*isWithAdvance,*/ identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                //    string strFileName = "OthersLiabilityWithAdvanceSummary.xlsx";
                //    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //    workbook.Close();
                //}
                //else
                //{
                //    IWorkbook workbok = accountsStatusDashboardService.getAcceptanceLiabilitySummaryReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                //    string strFileName = "OthersLiabilitySummary.xlsx";
                //    workbok.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //    workbok.Close();
                //}

                IWorkbook workbok = accountsStatusDashboardService.GetTrialBLAccountGroupReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                string strFileName = "TrialBLAccountGroupReport.xlsx";
                workbok.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbok.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }



        #region ---Assets and Liability Report---
        [HttpPost, Authorize]
        public JsonResult AccountGroupWiseReport(string allAccountGroupList,string toDate, string reportName,bool isDetailLevel, bool isActivityLevel,bool isBudgetLevel)
        {
            try
            {
                string LineId = string.Empty;

                //var settings = new JsonSerializerSettings
                //{
                //    NullValueHandling = NullValueHandling.Ignore,
                //    MissingMemberHandling = MissingMemberHandling.Ignore
                //};
                //List<FixedAssetRegister> accountGroupList = JsonConvert.DeserializeObject<List<FixedAssetRegister>>(allAccountGroupList, settings);

                //string accountGroupList = "";
                //foreach (var item in allAccountGroupList)
                //{
                //    if (string.IsNullOrEmpty(accountGroupList))
                //    {
                //        accountGroupList += "''," + item;
                //    }
                //    else
                //    {
                //        accountGroupList += "," + item;
                //    }

                //}

                //Dstatus = Dstatus.Replace('*', '"');
                //string ShiftId = "'" + shift.Replace(",", "','") + "'";//replaced with ""

                if (isDetailLevel)
                {
                    //var workbook = "";
                    //IWorkbook workbok = accountsStatusDashboardService.GetTrialBLAccountGroupReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                    //string strFileName = "TrialBLAccountGroupReport.xlsx";
                    //workbok.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                    //workbok.Close();


                    string fileName = "";
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                    // ExcelEngine excelEngine = new ExcelEngine();
                    //DailyAttendanceReport ep = new DailyAttendanceReport(mailReceiverDetailRepository);
                    fileName = accountsStatusDashboardService.GetTrialBLAccountGroupWiseDetailReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, allAccountGroupList, toDate, "", reportName);
                    return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
                }
                else if (isActivityLevel)
                {

                    string fileName = "";

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                    // ExcelEngine excelEngine = new ExcelEngine();
                    //DailyAttendanceReport ep = new DailyAttendanceReport(mailReceiverDetailRepository);
                    fileName = accountsStatusDashboardService.GetTrialBLAccountGroupWiseActivityReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, allAccountGroupList, toDate, "", reportName);
                    return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
                }
                else if (isBudgetLevel)
                {

                    string fileName = "";

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                    // ExcelEngine excelEngine = new ExcelEngine();
                    //DailyAttendanceReport ep = new DailyAttendanceReport(mailReceiverDetailRepository);
                    fileName = accountsStatusDashboardService.GetTrialBLAccountGroupWiseBudgetReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, allAccountGroupList, toDate, "", reportName);
                    return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    string fileName = "";

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                    // ExcelEngine excelEngine = new ExcelEngine();
                    //DailyAttendanceReport ep = new DailyAttendanceReport(mailReceiverDetailRepository);
                    fileName = accountsStatusDashboardService.GetTrialBLAccountGroupWiseGLReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, allAccountGroupList, toDate, "", reportName);
                    return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
                }

                //IWorkbook workbok = accountsStatusDashboardService.GetTrialBLAccountGroupReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                //string strFileName = "TrialBLAccountGroupReport.xlsx";
                //workbok.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //workbok.Close();


               // string fileName = "";

               // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
               // AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
               //// ExcelEngine excelEngine = new ExcelEngine();
               // //DailyAttendanceReport ep = new DailyAttendanceReport(mailReceiverDetailRepository);
               // fileName = accountsStatusDashboardService.GetTrialBLAccountGroupWiseReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, allAccountGroupList, toDate,"",reportName);
               // return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion



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
        public ActionResult GetAcceptanceLiabilityMaturityReport(string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.GetAcceptanceLiabilityMaturityReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,toDate);

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
        public ActionResult GetAcceptanceLiabilitySummaryReport(string toDate /*, bool isWithAdvance*/)
        {

            try
            {
               
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                ExcelEngine excelEngine = new ExcelEngine();

                //if (isWithAdvance)
                //{
                //    //var workbook = "";
                //    IWorkbook workbook = accountsStatusDashboardService.GetAcceptanceLiabilityWithAdvanceSummaryReport(excelEngine, toDate, /*isWithAdvance,*/ identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                //    string strFileName = "OthersLiabilityWithAdvanceSummary.xlsx";
                //    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //    workbook.Close();
                //}
                //else
                //{
                //    IWorkbook workbok = accountsStatusDashboardService.getAcceptanceLiabilitySummaryReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                //    string strFileName = "OthersLiabilitySummary.xlsx";
                //    workbok.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //    workbok.Close();
                //}

                IWorkbook workbok = accountsStatusDashboardService.GetAcceptanceLiabilitySummaryReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                string strFileName = "AcceptanceLiabilitySummaryReport.xlsx";
                workbok.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbok.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetAcceptanceLiabilityReport(string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            try
            {
                accountsStatusDashboardService.GetAcceptanceLiabilityReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate);

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
        public ActionResult OthersLiabilityAgingDetailReport(string toDate)
        {

            try
            {
                //if (string.IsNullOrEmpty(MasterLCList))
                //throw new Exception("Please select at least one Invoice");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

                ExcelEngine excelEngine = new ExcelEngine();
                //if (isWithAdvance)
                //{
                //    IWorkbook workbook = accountsStatusDashboardService.GetOthersLiabilityAgingDetailWithAdvanceReport(excelEngine, toDate, isWithAdvance, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name);
                //    // return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
                //    string strFileName = "OthersLiabilityWithAdvanceAgingDetail.xlsx";
                //    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //    workbook.Close();
                //}
                //else
                //{
                //    IWorkbook workbook = accountsStatusDashboardService.GetOthersLiabilityAgingDetailReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name);
                //    string strFileName = "OthersLiabilityAgingDetail.xlsx";
                //    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //    workbook.Close();


                //}

                IWorkbook workbook = accountsStatusDashboardService.GetOthersLiabilityAgingDetailReport(excelEngine, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name);
                string strFileName = "OthersLiabilityDetail.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        #endregion Others Liability

        #region GRN With Out INvoice
        [HttpPost, Authorize]
        public ActionResult GetGRNWithOutInvoiceDataList(string toDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(new { DATA = accountsStatusDashboardService.GetGRNWithOutInvoiceDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            var jsondata = Json(new { DATA = accountsStatusDashboardService.GetGRNWithOutInvoiceDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        #endregion Grn with out invoice

        #region GRN With Out INvoice
        [HttpPost, Authorize]
        public ActionResult GetInvoiceWithOutGRNDataList(string toDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsStatusDashboardService.GetInvoiceWithOutGRNDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion Grn with out invoice

        #region Party Status
        [HttpPost, Authorize]
        public ActionResult GetPartyStatusDataList(string toDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsStatusDashboardService.GetPartyStatusDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion
        #region Receipt from Customer
        [HttpPost, Authorize]
        public ActionResult GetReceiptFromCustomerList(string fromDate,string toDate)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsStatusDashboardService.GetReceiptFromCustomerList(fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetReceiptFromCustomerReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                string fileName = "";
                AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
                fileName = accountsStatusDashboardService.ReceiptFromCustomerxlsx(data, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Receipt from Customer

        [HttpGet, Authorize]
        public ActionResult GetNonRegisterAssetData()
        {
            AssetWIPQueryService assetWIPQueryService = new AssetWIPQueryService();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = assetWIPQueryService.GetNonRegisterAssetSQL(), Error = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NonRegisterAssetReportExcel(string materialMasterId, string materialMasterArticleId, string voucherId, string grnNo, string glId, string activityId)
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

                fileName = assetWIPQueryService.NonRegisterAssetList(materialMasterId, materialMasterArticleId, voucherId, grnNo, glId, activityId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult GetIssueQtyList2(string inventoryReceiveDetailId)
        {
            try
            {
                AssetWIPQueryService assetWIPQueryService = new AssetWIPQueryService();
                var data = assetWIPQueryService.GetIssueQtyList2(inventoryReceiveDetailId);
                return Json(new { Data = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }
        private string GetDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            try
            {
                return Convert.ToDateTime(s).ToString("dd-MMM-yyyy");
            }
            catch (Exception)
            {
                return "";
            }
        }

        [HttpGet,Authorize]
        public ActionResult GRNWithoutInvoiceReportExcelFormat(ReportFormat reportFormat,string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "GRN Without Invoice";
            var workbook = GetGRNWithoutInvoiceReportWorkSheet(identity.CompanyGroupId,identity.PlantId,identity.CompanyId,ToDate);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);
                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook,reportFileName);
                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        private IWorkbook GetGRNWithoutInvoiceReportWorkSheet(string companyGroupId,string plantId, string companyId, string toDate)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "GRNWithoutInvoice";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;



            DataTable data = GRNWithoutInvoiceList(companyGroupId, plantId, companyId, toDate);


            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "GRN No.", 10, ExcelHAlign.HAlignLeft);
            int ColGRNNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GRN Date", 10, ExcelHAlign.HAlignLeft);
            int ColGRNDate = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Doc RefNo", 15, ExcelHAlign.HAlignLeft);
            int ColDocRefNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Doc Date", 12, ExcelHAlign.HAlignLeft);
            int ColDocDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vendor", 18, ExcelHAlign.HAlignLeft);
            int ColVendor = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Voucher No.", 12, ExcelHAlign.HAlignLeft);
            int ColVoucherNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Qty", 12, ExcelHAlign.HAlignRight);
            int ColTransactionQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 8, ExcelHAlign.HAlignLeft);
            int ColCurrency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount", 15, ExcelHAlign.HAlignRight);
            int ColTotalMaterialTranAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Invoice Amount", 15, ExcelHAlign.HAlignRight);
            int ColInvoiceAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance", 15, ExcelHAlign.HAlignRight);
            int ColBalance = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Exchange Rate", 15, ExcelHAlign.HAlignRight);
            int ColToCurrencyRate = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Books val.", 15, ExcelHAlign.HAlignRight);
            int ColTotalMaterialBooksCurrencyAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO NO.", 10, ExcelHAlign.HAlignLeft);
            int ColPONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO DocRefNo.", 10, ExcelHAlign.HAlignLeft);
            int ColPODocRefNo = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "PI No.", 10, ExcelHAlign.HAlignLeft);
            int ColPINo = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "LC No.", 10, ExcelHAlign.HAlignLeft);
            int ColPLCRef = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "LC Opening Date", 10, ExcelHAlign.HAlignLeft);
            int ColLCOpeningDate = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Expiry Date", 10, ExcelHAlign.HAlignLeft);
            int ColExpiryDate = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "PLC Amount", 15, ExcelHAlign.HAlignRight);
            int ColPLCAmount = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Contract No.", 10, ExcelHAlign.HAlignLeft);
            int ColContractNo = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Customer", 15, ExcelHAlign.HAlignLeft);
            int ColCustomer = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "MLC Ref", 10, ExcelHAlign.HAlignLeft);
            int ColMasterLCRef = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "UD No", 10, ExcelHAlign.HAlignLeft);
            int ColUDNo = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "GRN Type", 10, ExcelHAlign.HAlignLeft);
            int ColGRNType = COL;

            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColGRNNo].Text = data.Rows[i]["GRNNo"].ToString();
                sheet[ROW, ColGRNDate].Text = GetDate(data.Rows[i]["GRNDate"].ToString());
                sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                sheet[ROW, ColDocDate].Text = data.Rows[i]["DocDate"].ToString(); 
                sheet[ROW, ColVendor].Text = data.Rows[i]["Vendor"].ToString();
                sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();

                sheet[ROW, ColTransactionQty].Number = clsStaticInfo.dbl(data.Rows[i]["TransactionQty"].ToString());
                sheet[ROW, ColTransactionQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColTransactionQty].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColTransactionQty].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();

                sheet[ROW, ColTotalMaterialTranAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalMaterialTranAmount"].ToString());
                sheet[ROW, ColTotalMaterialTranAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColTotalMaterialTranAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColTotalMaterialTranAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColInvoiceAmount].Number = clsStaticInfo.dbl(data.Rows[i]["InvoiceAmount"].ToString());
                sheet[ROW, ColInvoiceAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColInvoiceAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColInvoiceAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
                sheet[ROW, ColBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColToCurrencyRate].Number = clsStaticInfo.dbl(data.Rows[i]["ToCurrencyRate"].ToString());
                sheet[ROW, ColToCurrencyRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColToCurrencyRate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColToCurrencyRate].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColTotalMaterialBooksCurrencyAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalMaterialBooksCurrencyAmount"].ToString());
                sheet[ROW, ColTotalMaterialBooksCurrencyAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColTotalMaterialBooksCurrencyAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColTotalMaterialBooksCurrencyAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColPONo].Text = data.Rows[i]["POId"].ToString();
                sheet[ROW, ColPODocRefNo].Text = data.Rows[i]["PODocRefNo"].ToString();
                sheet[ROW, ColPINo].Text = data.Rows[i]["PINo"].ToString();
                sheet[ROW, ColPLCRef].Text = data.Rows[i]["PLCRef"].ToString();
                sheet[ROW, ColLCOpeningDate].Text = data.Rows[i]["LCOpeningDate"].ToString();
                sheet[ROW, ColExpiryDate].Text = data.Rows[i]["ExpiryDate"].ToString();

                sheet[ROW, ColPLCAmount].Number = clsStaticInfo.dbl(data.Rows[i]["PLCAmount"].ToString());
                sheet[ROW, ColPLCAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColPLCAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColPLCAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColContractNo].Text = data.Rows[i]["ContractNo"].ToString();
                sheet[ROW, ColCustomer].Text = data.Rows[i]["Customer"].ToString();
                sheet[ROW, ColMasterLCRef].Text = data.Rows[i]["MasterLCRef"].ToString();
                sheet[ROW, ColUDNo].Text = data.Rows[i]["UDNo"].ToString();
                sheet[ROW, ColGRNType].Text = data.Rows[i]["GRNType"].ToString();
                

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }
            report.SetHeaderText(ref sheet, ROW, 1, "Total", 10, ExcelHAlign.HAlignLeft);
            sheet.Range[ROW, ColGRNDate, ROW , ColCurrency].Merge();

            //sheet[ROW, ColTotalMaterialTranAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColTotalMaterialTranAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColTotalMaterialTranAmount)  + (ROW-1).ToString()+")";
            //sheet[ROW, ColTotalMaterialTranAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            //sheet[ROW, ColTotalMaterialTranAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet[ROW, ColTotalMaterialTranAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, ColTotalMaterialTranAmount].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, ColToCurrencyRate, ROW, ColToCurrencyRate].Merge();

            //sheet[ROW, ColInvoiceAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColInvoiceAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColInvoiceAmount) + (ROW - 1).ToString() + ")";
            //sheet[ROW, ColInvoiceAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            //sheet[ROW, ColInvoiceAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet[ROW, ColInvoiceAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, ColInvoiceAmount].CellStyle.Font.Bold = true;

            //sheet[ROW, ColBalance].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBalance) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBalance) + (ROW - 1).ToString() + ")";
            //sheet[ROW, ColBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            //sheet[ROW, ColBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet[ROW, ColBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, ColBalance].CellStyle.Font.Bold = true;

            sheet[ROW, ColTotalMaterialBooksCurrencyAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColTotalMaterialBooksCurrencyAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColTotalMaterialBooksCurrencyAmount) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColTotalMaterialBooksCurrencyAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColTotalMaterialBooksCurrencyAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColTotalMaterialBooksCurrencyAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColTotalMaterialBooksCurrencyAmount].CellStyle.Font.Bold = true;
            sheet.Range[ROW, ColPONo, ROW, ColExpiryDate].Merge();

            //sheet[ROW, ColPLCAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColPLCAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColPLCAmount) + (ROW - 1).ToString() + ")";
            //sheet[ROW, ColPLCAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            //sheet[ROW, ColPLCAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet[ROW, ColPLCAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, ColPLCAmount].CellStyle.Font.Bold = true;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8f;
            report.CompanyHeader(ref sheet, endCol, "GRN Without Invoice", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        public DataTable GRNWithoutInvoiceList(string companyGroupId, string plantId, string companyId, string toDate)
        {
            try
            {

                string strSQL = string.Empty;
                strSQL = @"select IR.Id GRNNo ,V.VoucherNo,IR.PartyId,p.UserName Vendor,IR.PartyType, IR.DocRefNo,isnull( format( IR.DocDate, 'dd-MMM-yyyy'),'')DocDate
                    ,IR.GateEntryNo,IR.EntryDate,IR.IsApproved ,IR.IsInvoice,IR.GRNType
					,isnull(format( IR.GRNDate,'dd-MMM-yyyy'),'')GRNDate ,ISNULL( IR.EmployeeId,'')EmployeeId
                    ,ISNULL( IRD.TransactionQty ,0)TransactionQty
					,IRD.MaterialTranAmount
                    ,IRD.TotalTaxAmount
					,IRD.ChargesTranAmount
					,IRD.ChargesTaxTranAmount
					,IRD.TotalMaterialTranAmount
					,ISNULL(ISNULL(IV.InvoiceAmount,ISNULL(PGIV.TransactionAmount,0)),0)InvoiceAmount
					,(IRD.MaterialTranAmount-ISNULL(IV.InvoiceAmount,0)-ISNULL(PGIV.TransactionAmount,0)) Balance
					,cc.Code ComCurrency
					,C.Code Currency
				    ,IR.ToCurrencyRate
                    ,IRD.TotalMaterialBooksCurrencyAmount
                    ,IRD.GRNQty
					, IRD.GRNTotalAmount
                    ,IRD.GrossAmount
					,IRD.DiscountAmount
				, isnull( plc.Amount,0) PLCAmount

				    --,con.Id ContractId 	,con.ContractNo, con.UDNo, cus.UserName Customer
					--,ML.Id MasterLCId, ML.LCRef MLCRef
					,isnull( ml.Amount ,0)MLCAmount

					  ,POId= STUFF((select distinct ','+PG.POId
			                            FROM TRN.POGGRNMap PG 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

					 ,PODocRefNo= STUFF((select distinct ','+PO.DocRefNo
			                            from TRN.POGGRNMap PG 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                            where PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                  
				    ,PLCRef= STUFF((select distinct ','+PLC.LCRef
			                            FROM PurchaseLC PLC 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.PurchaseLCId=PLC.Id	
										left join TRN.POGGRNMap pg on pg.PoId= po.Id
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


				    ,PINo= STUFF((select distinct ','+PLC.PINo
			                            FROM PurchaseLC PLC 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.PurchaseLCId=PLC.Id	
										left join TRN.POGGRNMap pg on pg.PoId= po.Id
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
               ,LCOpeningDate= STUFF((select distinct ','+FORMAT(PLC.LCDate,'dd-MMM-yyyy')
			                            FROM PurchaseLC PLC 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.PurchaseLCId=PLC.Id	
										left join TRN.POGGRNMap pg on pg.PoId= po.Id
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

			    ,ExpiryDate= STUFF((select distinct ','+format( PLC.ExpiryDate,'dd-MMM-yyyy')
			                            FROM PurchaseLC PLC 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.PurchaseLCId=PLC.Id	
										left join TRN.POGGRNMap pg on pg.PoId= po.Id
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


				 --,PLCAmount= STUFF((select distinct ','+PLC.Amount
			  --                          FROM PurchaseLC PLC 
     --                                   LEFT JOIN TRN.PurchaseOrder PO ON PO.PurchaseLCId=PLC.Id	
					--					left join TRN.POGGRNMap pg on pg.PoId= po.Id
			  --                          WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

			
				,ContractNo= STUFF((select distinct ','+Con.ContractNo
			                            FROM Contract Con 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.ContractId=Con.Id	
										left join TRN.POGGRNMap pg on pg.PoId= po.Id
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


			,Customer= STUFF((select distinct ','+cus.UserName
			                            FROM Contract Con 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.ContractId=Con.Id	
                                        LEFT JOIN hkp.Party cus ON cus.Id= con.CustomerId	
										left join TRN.POGGRNMap pg on pg.PoId= po.Id
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


			,UDNo= STUFF((select distinct ','+Con.UDNo
			                            FROM Contract Con 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.ContractId=Con.Id	
                                       -- LEFT JOIN hkp.Party cus ON cus.Id= con.CustomerId	
										left join TRN.POGGRNMap pg on pg.PoId= po.Id
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													
				,MasterLCRef= STUFF((select distinct ','+mlc.LCRef
			                            FROM Contract Con 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.ContractId=Con.Id	
                                        LEFT JOIN MasterLC mlc ON mlc.Id=Con.MasterLCId	
										left join TRN.POGGRNMap pg on pg.PoId= po.Id
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								
			
				   from trn.InventoryReceive IR  
                    --left join trn.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId = IR.Id
					LEFT JOIN (SELECT InventoryReceiveId,POId,SUM(MaterialTranAmount) MaterialTranAmount
											,SUM(ISNULL( TransactionQty ,0))TransactionQty
											,SUM(TotalMaterialTranAmount) TotalMaterialTranAmount,SUM(ChargesTranAmount) ChargesTranAmount
											,SUM(ChargesTaxTranAmount) ChargesTaxTranAmount ,SUM(TotalTaxAmount)TotalTaxAmount
											,SUM(TotalMaterialBooksCurrencyAmount)TotalMaterialBooksCurrencyAmount
											,SUM(GRNQty)GRNQty, SUM(GRNTotalAmount)GRNTotalAmount
											,SUM(GrossAmount)GrossAmount,SUM(DiscountAmount)DiscountAmount
											FROM TRN.InventoryReceiveDetail GROUP BY InventoryReceiveId,POId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN SCS.Currency C ON C.Id = IR.CurrencyId
                    left join trn.PurchaseOrder po on po.Id =IRD.POId
                    left join PurchaseLC plc on plc.Id = po.PurchaseLCId
                    --LEFT JOIN TRN.GRNAcceptanceMap GAM ON GAM.GRNId =IR.Id
					left join HKP.party p on p.Id = ir.PartyId
					left join org.Company Com on Com.Id = IR.CompanyId
                    LEFT JOIN SCS.Currency CC ON CC.Id = Com.BaseCurrencyId
					left join [Contract] as Con on plc.ContractId= Con.Id
					left join MasterLC ML on ML.Id=con.MasterLCId
                    LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IRD.InventoryReceiveId
					LEFT JOIN TRN.Voucher V ON V.Id=CASE WHEN IR.EmployeeId<>'' THEN EP.VoucherId ELSE IR.VoucherId  END
					LEFT JOIN (SELECT InventoryReceiveId,SUM(Amount) InvoiceAmount
											FROM TRN.Invoice GROUP BY InventoryReceiveId) AS IV ON IV.InventoryReceiveId=IR.Id
					LEFT JOIN (SELECT PGD.InventoryReceiveId,SUM(ISNULL(PGD.TransactionAmount,0)) TransactionAmount
											FROM TRN.Invoice I
											INNER JOIN dbo.PostGRNInvoiceDetail PGD ON PGD.PostGRNInvoiceId=I.PostGRNInvoiceId
											GROUP BY PGD.InventoryReceiveId) AS PGIV ON PGIV.InventoryReceiveId=IR.Id
					
                    where IR.CompanyGroupId = '" + companyGroupId + "' AND IR.CompanyId ='" + companyId + "' AND IR.PlantId='" + plantId + @"'
                   AND  IR.IsInvoice=0 
					--and isnull(GAM.PurchaseDocumentAcceptanceId,'') is not null
	                --AND IR.Id not in (select InventoryReceiveId from trn.Invoice where InventoryReceiveId<>'')
					AND (IRD.MaterialTranAmount-ISNULL(IV.InvoiceAmount,0)-ISNULL(PGIV.TransactionAmount,0))>0
					AND IR.Id not in (select InventoryReceiveId from trn.EmployeePayable where InventoryReceiveId<>'')
					and ir.VoucherId<>''
                          and IR.GRNDate <='" + toDate + @"'
			         --and plc.IsAccepptanceFirst=0
					--AND IR.Id='2021638'
					--group by 
					--IR.Id  ,V.VoucherNo,cc.Code   ,C.Code   ,IR.PartyId	,IR.PartyType , IR.DocRefNo	, IR.DocDate
                    --  ,IR.GateEntryNo	,IR.EntryDate    ,IR.IsApproved ,IR.IsInvoice,IR.GRNType	,IR.GRNDate   ,IR.EmployeeId
					--,IRD.IsAsset,p.UserName,IR.ToCurrencyRate";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        public ActionResult InvoiceWithoutGRNReportExcelFormat(ReportFormat reportFormat, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Invoice Without GRN";
            var workbook = GetInvoiceWithoutGRNReportWorkSheet(identity.CompanyGroupId, identity.PlantId, identity.CompanyId, ToDate);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);
                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);
                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        private IWorkbook GetInvoiceWithoutGRNReportWorkSheet(string companyGroupId, string plantId, string companyId, string toDate)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "InvoiceWithoutGRN";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;



            DataTable data = InvoiceWithoutGRNList(companyGroupId, plantId, companyId, toDate);


            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Invoice No.", 20, ExcelHAlign.HAlignLeft);
            int ColInvoiceNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Doc Ref No.", 15, ExcelHAlign.HAlignLeft);
            int ColDocRefNo = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "LC No.", 15, ExcelHAlign.HAlignLeft);
            int ColPurchaseLCNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO No.", 12, ExcelHAlign.HAlignLeft);
            int ColPONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Party Code", 10, ExcelHAlign.HAlignLeft);
            int ColPartyCode = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Party", 30, ExcelHAlign.HAlignLeft);
            int ColPartyName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Party Plant", 30, ExcelHAlign.HAlignLeft);
            int ColPartyPlantName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Voucher No.", 15, ExcelHAlign.HAlignLeft);
            int ColVoucherNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Doc Date", 10, ExcelHAlign.HAlignLeft);
            int ColDocDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Posting Date", 10, ExcelHAlign.HAlignLeft);
            int ColPostingDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 8, ExcelHAlign.HAlignLeft);
            int ColCurrencyCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Gross", 15, ExcelHAlign.HAlignRight);
            int ColGross = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Debit Note Amount", 15, ExcelHAlign.HAlignRight);
            int ColDebitNoteAmount = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Tax Amount", 15, ExcelHAlign.HAlignRight);
            int ColTaxAmount = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "GRN Amount", 15, ExcelHAlign.HAlignRight);
            int ColGRNAmount = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Balance", 15, ExcelHAlign.HAlignRight);
            int ColBalance = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Exchange Rate", 15, ExcelHAlign.HAlignRight);
            int ColCompanyCurrencyRate = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Books Gross", 15, ExcelHAlign.HAlignRight);
            int ColBooksGross = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Books DebitNote Amount", 15, ExcelHAlign.HAlignRight);
            int ColDebitNoteBooksAmount = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Books Tax Amount", 15, ExcelHAlign.HAlignRight);
            int ColBooksTaxAmount = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Books GRN Amount", 15, ExcelHAlign.HAlignRight);
            int ColBooksSetOff = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Books Balance", 15, ExcelHAlign.HAlignRight);
            int ColBooksBalance = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Books SetOff Amount", 15, ExcelHAlign.HAlignRight);
            int ColBooksWriteOffAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Books Balance", 15, ExcelHAlign.HAlignRight);
            int ColBooksInvoiceBalance = COL;

            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColInvoiceNo].Text = data.Rows[i]["InvoiceNo"].ToString();
                sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                sheet[ROW, ColPurchaseLCNo].Text = data.Rows[i]["PurchaseLCNo"].ToString();
                sheet[ROW, ColPONo].Text = data.Rows[i]["PONo"].ToString();
                sheet[ROW, ColPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
                sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();

                sheet[ROW, ColPartyPlantName].Text = data.Rows[i]["PartyPlantName"].ToString();
                sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
                sheet[ROW, ColDocDate].Text = GetDate(data.Rows[i]["DocDate"].ToString());
                sheet[ROW, ColPostingDate].Text = GetDate(data.Rows[i]["PostingDate"].ToString());
                sheet[ROW, ColCurrencyCode].Text = data.Rows[i]["CurrencyCode"].ToString();

                sheet[ROW, ColGross].Number = clsStaticInfo.dbl(data.Rows[i]["Gross"].ToString());
                sheet[ROW, ColGross].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColGross].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColDebitNoteAmount].Number = clsStaticInfo.dbl(data.Rows[i]["DebitNoteAmount"].ToString());
                sheet[ROW, ColDebitNoteAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColDebitNoteAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColDebitNoteAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TaxAmount"].ToString());
                sheet[ROW, ColTaxAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColTaxAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColTaxAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColGRNAmount].Number = clsStaticInfo.dbl(data.Rows[i]["SetOff"].ToString());
                sheet[ROW, ColGRNAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColGRNAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColGRNAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
                sheet[ROW, ColBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColCompanyCurrencyRate].Number = clsStaticInfo.dbl(data.Rows[i]["CompanyCurrencyRate"].ToString());
                sheet[ROW, ColCompanyCurrencyRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColCompanyCurrencyRate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColCompanyCurrencyRate].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBooksGross].Number = clsStaticInfo.dbl(data.Rows[i]["BooksGross"].ToString());
                sheet[ROW, ColBooksGross].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBooksGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBooksGross].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColDebitNoteBooksAmount].Number = clsStaticInfo.dbl(data.Rows[i]["DebitNoteBooksAmount"].ToString());
                sheet[ROW, ColDebitNoteBooksAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColDebitNoteBooksAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColDebitNoteBooksAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBooksTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["BooksTaxAmount"].ToString());
                sheet[ROW, ColBooksTaxAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBooksTaxAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBooksTaxAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBooksSetOff].Number = clsStaticInfo.dbl(data.Rows[i]["BooksSetOff"].ToString());
                sheet[ROW, ColBooksSetOff].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBooksSetOff].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBooksSetOff].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBooksBalance].Number = clsStaticInfo.dbl(data.Rows[i]["BooksBalance"].ToString());
                sheet[ROW, ColBooksBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBooksBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBooksBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBooksWriteOffAmount].Number = clsStaticInfo.dbl(data.Rows[i]["BooksWriteOffAmount"].ToString());
                sheet[ROW, ColBooksWriteOffAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBooksWriteOffAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBooksWriteOffAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBooksInvoiceBalance].Number = clsStaticInfo.dbl(data.Rows[i]["BooksInvoiceBalance"].ToString());
                sheet[ROW, ColBooksInvoiceBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBooksInvoiceBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBooksInvoiceBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

               
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }
            report.SetHeaderText(ref sheet, ROW, 1, "Total", 10, ExcelHAlign.HAlignLeft);
            sheet.Range[ROW, ColDocRefNo, ROW, ColCurrencyCode].Merge();

            sheet[ROW, ColGross].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColGross) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColGross) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColGross].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColGross].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColGross].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, ColToCurrencyRate, ROW, ColToCurrencyRate].Merge();

            sheet[ROW, ColDebitNoteAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColDebitNoteAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColDebitNoteAmount) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColDebitNoteAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColDebitNoteAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColDebitNoteAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColDebitNoteAmount].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, ColPONo, ROW, ColExpiryDate].Merge();

            sheet[ROW, ColTaxAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColTaxAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColTaxAmount) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColTaxAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColTaxAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColTaxAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColTaxAmount].CellStyle.Font.Bold = true;

            sheet[ROW, ColGRNAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColGRNAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColGRNAmount) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColGRNAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColGRNAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColGRNAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColGRNAmount].CellStyle.Font.Bold = true;

            sheet[ROW, ColBalance].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBalance) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBalance) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBalance].CellStyle.Font.Bold = true;

            sheet[ROW, ColBooksGross].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksGross) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksGross) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBooksGross].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBooksGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBooksGross].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBooksGross].CellStyle.Font.Bold = true;

            sheet[ROW, ColDebitNoteBooksAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColDebitNoteBooksAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColDebitNoteBooksAmount) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColDebitNoteBooksAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColDebitNoteBooksAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColDebitNoteBooksAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColDebitNoteBooksAmount].CellStyle.Font.Bold = true;

            sheet[ROW, ColBooksTaxAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksTaxAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksTaxAmount) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBooksTaxAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBooksTaxAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBooksTaxAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBooksTaxAmount].CellStyle.Font.Bold = true;

            sheet[ROW, ColBooksSetOff].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksSetOff) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksSetOff) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBooksSetOff].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBooksSetOff].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBooksSetOff].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBooksSetOff].CellStyle.Font.Bold = true;

            sheet[ROW, ColBooksBalance].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksBalance) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksBalance) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBooksBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBooksBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBooksBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBooksBalance].CellStyle.Font.Bold = true;

            sheet[ROW, ColBooksWriteOffAmount].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksWriteOffAmount) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksWriteOffAmount) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBooksWriteOffAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBooksWriteOffAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBooksWriteOffAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBooksWriteOffAmount].CellStyle.Font.Bold = true;

            sheet[ROW, ColBooksInvoiceBalance].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksInvoiceBalance) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksInvoiceBalance) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBooksInvoiceBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBooksInvoiceBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBooksInvoiceBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBooksInvoiceBalance].CellStyle.Font.Bold = true;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8f;
            report.CompanyHeader(ref sheet, endCol, "GRN Without Invoice", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        public DataTable InvoiceWithoutGRNList(string companyGroupId, string plantId, string companyId, string toDate)
        {
            try
            {

                string strSQL = string.Empty;
                strSQL = @"select x.* from (

                        SELECT   IV.PartyType,IV.PartyId, IV.PartyPlantId,p.code PartyCode, P.UserName PartyName, PP.UserName AS PartyPlantName
										,isnull( V.VoucherNo,'')VoucherNo,IVD.InvoiceId,VD.VoucherId,isnull( PDA.InvoiceNo,'')InvoiceNo
										 ,PurchaseLCNo= STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,PONo= isnull( STUFF((select distinct ','+xpomap.POId from
											dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
											LEFT JOIN trn.PurchaseDocAcceptancePOMap xpomap on xpomap.PurchaseDocAcceptanceId=xp.Id
										where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
                                        , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate,V.DocRefNo
										, replace (convert(varchar(11),iv.DocDate, 106),'', '-')as DocDate,iv.DocDate  SortDocDate
										, C.Code CurrencyCode
										,IV.BaseNoOfDays, REPLACE(CONVERT(VARCHAR(11), IV.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(VARCHAR(11), IV.ActualDueDate, 106), ' ', '-') AS ActualDueDate
										
										,Days=DATEDIFF(DAY, GETDATE(),IV.BaseOnDueDate)
										,AgingInvoice= case 
													--	when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<0 OR IV.ActualDueDate IS NULL then 'Overdue'

														when DATEDIFF(DAY, GETDATE(),Iv.ActualDueDate)<-30  OR IV.ActualDueDate IS NULL then 'OverDueMoreThan30'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<-15 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-30  OR IV.ActualDueDate IS NULL then 'OverDueMoreThan15'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-15  OR IV.ActualDueDate IS NULL then 'OverDueLessThan15'


															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then 'Today'
															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=7 then '1-7'
															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>7 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=30 then '8-30'
															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>30 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=60 then '31-60'
															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>60 then '60 Onword'
															end
										,AgingSorting= case 
														--when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<0 OR IV.ActualDueDate IS NULL then '1.Overdue'

														when DATEDIFF(DAY, GETDATE(),Iv.ActualDueDate)<-30  OR IV.ActualDueDate IS NULL then '1.OverDueMoreThan30'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<-15 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-30  OR IV.ActualDueDate IS NULL then '2.OverDueMoreThan15'
														when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>=-15  OR IV.ActualDueDate IS NULL then '3.OverDueLessThan15'


															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then '4.Today'
															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>0 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=7 then '5.1-7'
															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>7 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=30 then '6.8-30'
															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>30 and DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<=60 then '7.31-60'
															when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>60 then '8.60 Onward'
															end

										, ISNULL(IVD.NetAmount,0) AS Gross
										,0 DebitNoteAmount
										,isnull( IWD.TaxAmount ,0)TaxAmount,
                                         SetOff=ISNULL(IRD.TotalMaterialTranAmount, 0) 
										 , ISNULL(IVD.NetAmount-ISNULL(IRD.TotalMaterialTranAmount, 0),0) AS Balance

										,CC.CompanyCurrencyRate
										,ISNULL(IVD.NetAmount*CC.CompanyCurrencyRate,0) AS BooksGross
								    	,0 DebitNoteBooksAmount
										,isnull(IWD.TaxAmount* CC.CompanyCurrencyRate,0) BooksTaxAmount
										,ISNULL (ISNULL(IRD.TotalMaterialTranAmount, 0)*CC.CompanyCurrencyRate,0)   AS BooksSetOff
										,ISNULL((IVD.NetAmount*CC.CompanyCurrencyRate)-(ISNULL(IRD.TotalMaterialTranAmount, 0)*CC.CompanyCurrencyRate),0) AS BooksBalance
										,ISNULL (ISNULL(IDND.WriteOffAmount, 0)*CC.CompanyCurrencyRate,0)   AS BooksWriteOffAmount
										,ISNULL((IVD.NetAmount*CC.CompanyCurrencyRate)-(ISNULL(IDND.WriteOffAmount, 0)*CC.CompanyCurrencyRate),0) AS BooksInvoiceBalance
                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
										LEFT JOIN TRN.PurchasedocAcceptance AS PDA ON IV.PurchaseDocAcceptanceId=PDA.Id
									    LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
									    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
										LEFT JOIN (SELECT PurchaseDocumentAcceptanceId,SUM(MaterialTranAmount) MaterialTranAmount
											,SUM(TotalMaterialTranAmount) TotalMaterialTranAmount,SUM(ChargesTranAmount) ChargesTranAmount
											,SUM(ChargesTaxTranAmount) ChargesTaxTranAmount
											FROM TRN.InventoryReceiveDetail GROUP BY PurchaseDocumentAcceptanceId) AS IRD ON IRD.PurchaseDocumentAcceptanceId=PDA.Id
                                        LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								        LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								            where w.PaymentSource='Tax'
								            group by wd.InvoiceDetailId
								                ) IWD ON IWD.InvoiceDetailId=IVD.Id

                                         LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) WriteOffAmount  FROM TRN.InvoiceWriteOffDetail WD 
								                LEFT JOIN  TRN.InvoiceWriteOff DNW on wd.InvoiceWriteOffId =DNW.id
								                where WD.InvoiceDetailId<>'' and DNW.PaymentSource<>'Tax'
								                group by wd.InvoiceDetailId
								                ) IDND ON IDND.InvoiceDetailId=IVD.Id
										LEFT JOIN MST.PaymentTerm PT ON PT.Id=IV.PaymentTermId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									
                                        WHERE IV.Archive=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('PurchaseDocAcceptance')
                                        AND IV.CompanyGroupId='" + companyGroupId + "' AND IV.CompanyId='" + companyId + @"' AND IV.PlantId='" + plantId + @"'
                                       
                                        and IV.PostingDate <= '" + toDate + @"'
										--GROUP BY IV.PartyId, IV.PartyPlantId, PP.UserName,P.UserName
										) x
                                        where x.Balance>0
										order by x.SortDocDate asc";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        public ActionResult BankReportExcelFormat(ReportFormat reportFormat, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Bank Report";
            var workbook = GetBankReportWorkSheet(identity.CompanyGroupId, identity.PlantId, identity.CompanyId, toDate);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);
                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);
                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        private IWorkbook GetBankReportWorkSheet(string companyGroupId, string plantId, string companyId, string toDate)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "BankReport";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;



            DataTable data = BankReportList(companyGroupId, plantId, companyId, toDate);


            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Bank Master", 25, ExcelHAlign.HAlignLeft);
            int ColBankMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bank Name", 20, ExcelHAlign.HAlignLeft);
            int ColBankName = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Branch", 15, ExcelHAlign.HAlignLeft);
            int ColBranch = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Account Number", 20, ExcelHAlign.HAlignLeft);
            int ColAccountNumber = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bank Currency", 8, ExcelHAlign.HAlignLeft);
            int ColBankCurrency = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Bank Amount", 18, ExcelHAlign.HAlignRight);
            int ColBankAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Books Bank Balance", 18, ExcelHAlign.HAlignRight);
            int ColBooksBankBalance = COL;
         

            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColBankMaster].Text = data.Rows[i]["BankAccountDetails"].ToString();
                sheet[ROW, ColBankName].Text = data.Rows[i]["Bank"].ToString();
                sheet[ROW, ColBranch].Text = data.Rows[i]["Branch"].ToString();
                sheet[ROW, ColAccountNumber].Text = data.Rows[i]["AccountNumber"].ToString();
                sheet[ROW, ColBankCurrency].Text = data.Rows[i]["BankCurrency"].ToString();
                
                sheet[ROW, ColBankAmount].Number = clsStaticInfo.dbl(data.Rows[i]["BankAmount"].ToString());
                sheet[ROW, ColBankAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBankAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBankAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBooksBankBalance].Number = clsStaticInfo.dbl(data.Rows[i]["BooksBankBalance"].ToString());
                sheet[ROW, ColBooksBankBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBooksBankBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBooksBankBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

               


                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }
            report.SetHeaderText(ref sheet, ROW, 1, "Total", 25, ExcelHAlign.HAlignLeft);
            sheet.Range[ROW, ColBankName, ROW, ColBankAmount].Merge();

            sheet[ROW, ColBooksBankBalance].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksBankBalance) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksBankBalance) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBooksBankBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBooksBankBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBooksBankBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBooksBankBalance].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, ColToCurrencyRate, ROW, ColToCurrencyRate].Merge();

        

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8f;
            report.CompanyHeader(ref sheet, endCol, "Bank Report", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        public DataTable BankReportList(string companyGroupId, string plantId, string companyId, string toDate)
        {
            try
            {

                string strSQL = string.Empty;
                strSQL = @"DECLARE @companyGroupId VARCHAR(10)='CG20171'
                        DECLARE @companyId VARCHAR(10)='C20171';
                        DECLARE @plantId VARCHAR(10)='20171';
                        --DECLARE @cashMasterId VARCHAR(10)='1';
                        SELECT B.UserName Bank,BB.UserName Branch,BM.AccountNumber,[BankAccountDetails]= BM.AccountTitle  ,BM.Id,C.Code BankCurrency
                          ,SUM(ISNULL(GLTD.DrAmount,0)) DrAmount 
                        , SUM(ISNULL(GLTD.CrAmount,0)) CrAmount 
						 , SUM(ISNULL(GLTD.DrAmount,0))  -  SUM(ISNULL(GLTD.CrAmount,0)) BankAmount 
                        , SUM(ISNULL(CC.CompanyCurrencyDrAmount,0)) CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) CompanyCurrencyCrAmount
			
						,SUM(ISNULL(CC.CompanyCurrencyDrAmount,0))-SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) BooksBankBalance
                        FROM  trn.GLTransactionDetail GLTD
						JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLTD.VoucherDetailId
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId 
						LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
						left join hkp.BankBranch BB ON BB.Id=BM.BankBranchId
                      --  LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId and vd.CashMasterId<>''
						LEFT JOIN SCS.Currency C ON C.Id=BM.CurrencyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId --AND VD.CashMasterId=@cashMasterId 
						AND V.SourceType!='OpeningBalance'
						 AND V.PostingDate <= '19-Aug-2021' and vd.BankMasterId<>''
                         and BM.AccountType='HouseBank'
						 GROUP BY BM.AccountTitle ,BM.Id,c.Code,B.UserName,BB.UserName,BM.AccountNumber
                        UNION ALL
                        SELECT B.UserName Bank,BB.UserName Branch,BM.AccountNumber,[BankAccountDetails]= BM.AccountTitle,BM.Id,C.Code CashCurrency,
                          SUM(ISNULL(GLTD.DrAmount,0)) DrAmount ,
                         SUM(ISNULL(GLTD.CrAmount,0)) CrAmount 
						 , SUM(ISNULL(GLTD.DrAmount,0)) - SUM(ISNULL(GLTD.CrAmount,0)) BankAmount 
                        , SUM(ISNULL(CC.CompanyCurrencyDrAmount,0)) CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) CompanyCurrencyCrAmount
						--, ISNULL ((CC.CompanyCurrencyDrAmount,0)-(CC.CompanyCurrencyCrAmount),0) as CashBalance
						,SUM(ISNULL(CC.CompanyCurrencyDrAmount,0))-SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) BooksBankBalance
                        FROM  trn.GLTransactionDetail GLTD
						JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLTD.VoucherDetailId
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId 
						LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
						left join hkp.BankBranch BB ON BB.Id=BM.BankBranchId
                       -- LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId 
						LEFT JOIN SCS.Currency C ON C.Id=BM.CurrencyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId --AND VD.CashMasterId=@cashMasterId 
						AND V.SourceType='OpeningBalance'
						 AND V.PostingDate > '" + toDate + @"' and vd.BankMasterId<>''
						 and BM.AccountType='HouseBank'
						 GROUP BY BM.AccountTitle ,BM.Id,c.Code,B.UserName,BB.UserName,BM.AccountNumber
                       -- ORDER BY V.PostingDate ASC";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public ActionResult CashReportExcelFormat(ReportFormat reportFormat,string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Cash Report";
            var workbook = GetCashReportWorkSheet(identity.CompanyGroupId, identity.PlantId, identity.CompanyId, toDate);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);
                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);
                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        private IWorkbook GetCashReportWorkSheet(string companyGroupId, string plantId, string companyId, string toDate)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "CashReport";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;



            DataTable data = CashReportList(companyGroupId, plantId, companyId, toDate);


            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Cash", 25, ExcelHAlign.HAlignLeft);
            int ColCash = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 20, ExcelHAlign.HAlignLeft);
            int ColCurrency = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Cash Amount", 15, ExcelHAlign.HAlignRight);
            int ColCashAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Books Cash Balance", 20, ExcelHAlign.HAlignRight);
            int ColBooksCashBalance = COL;
           


            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColCash].Text = data.Rows[i]["Cash"].ToString();
                sheet[ROW, ColCurrency].Text = data.Rows[i]["CashCurrency"].ToString();
              
                sheet[ROW, ColCashAmount].Number = clsStaticInfo.dbl(data.Rows[i]["CashAmount"].ToString());
                sheet[ROW, ColCashAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColCashAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColCashAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBooksCashBalance].Number = clsStaticInfo.dbl(data.Rows[i]["BooksCashBalance"].ToString());
                sheet[ROW, ColBooksCashBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColBooksCashBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColBooksCashBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }
            report.SetHeaderText(ref sheet, ROW, 1, "Total", 25, ExcelHAlign.HAlignLeft);
            sheet.Range[ROW, ColCurrency, ROW, ColCashAmount].Merge();

            sheet[ROW, ColBooksCashBalance].Formula = "SUM(" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksCashBalance) + startRow.ToString() + ":" + OTSBD.clsStaticInfo.GetxlsCol(ColBooksCashBalance) + (ROW - 1).ToString() + ")";
            sheet[ROW, ColBooksCashBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
            sheet[ROW, ColBooksCashBalance].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColBooksCashBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, ColBooksCashBalance].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, ColToCurrencyRate, ROW, ColToCurrencyRate].Merge();



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8f;
            report.CompanyHeader(ref sheet, endCol, "Cash Report", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        public DataTable CashReportList(string companyGroupId, string plantId, string companyId, string toDate)
        {
            try
            {

                string strSQL = string.Empty;
                strSQL = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"'
                        DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        --DECLARE @cashMasterId VARCHAR(10)='1';
                        SELECT CM.UserName Cash,CM.Id,C.Code CashCurrency
                          ,SUM(ISNULL(GLTD.DrAmount,0)) DrAmount 
                        , SUM(ISNULL(GLTD.CrAmount,0)) CrAmount 
						 , SUM(ISNULL(GLTD.DrAmount,0))  -  SUM(ISNULL(GLTD.CrAmount,0)) CashAmount 
                        , SUM(ISNULL(CC.CompanyCurrencyDrAmount,0)) CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) CompanyCurrencyCrAmount
						--, ISNULL ((CC.CompanyCurrencyDrAmount,0)-(CC.CompanyCurrencyCrAmount),0) as CashBalance
			
						,SUM(ISNULL(CC.CompanyCurrencyDrAmount,0))-SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) BooksCashBalance
                        FROM  trn.GLTransactionDetail GLTD
						JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLTD.VoucherDetailId
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                       -- LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId and vd.CashMasterId<>''
						LEFT JOIN SCS.Currency C ON C.Id=CM.CurrencyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId --AND VD.CashMasterId=@cashMasterId 
						AND V.SourceType!='OpeningBalance'
						 AND V.PostingDate <= '" + toDate + @"' and vd.CashMasterId<>''
						 GROUP BY CM.UserName ,CM.Id,c.Code
                        UNION ALL
                        SELECT CM.UserName Cash,CM.Id,C.Code CashCurrency,
                          SUM(ISNULL(GLTD.DrAmount,0)) DrAmount ,
                         SUM(ISNULL(GLTD.CrAmount,0)) CrAmount 
						 , SUM(ISNULL(GLTD.DrAmount,0))  -     SUM(ISNULL(GLTD.CrAmount,0)) CashAmount 
                        , SUM(ISNULL(CC.CompanyCurrencyDrAmount,0)) CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) CompanyCurrencyCrAmount
						--, ISNULL ((CC.CompanyCurrencyDrAmount,0)-(CC.CompanyCurrencyCrAmount),0) as CashBalance
						,SUM(ISNULL(CC.CompanyCurrencyDrAmount,0))-SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) BooksCashBalance
                        FROM  trn.GLTransactionDetail GLTD
						JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLTD.VoucherDetailId
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        --LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId 
						LEFT JOIN SCS.Currency C ON C.Id=CM.CurrencyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId --AND VD.CashMasterId=@cashMasterId 
						AND V.SourceType='OpeningBalance'
						 AND V.PostingDate > '" + toDate + @"' and vd.CashMasterId<>''
						 GROUP BY CM.UserName ,CM.Id,c.Code
                       -- ORDER BY V.PostingDate ASC";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetReceiptPaymentStatusDataList()
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsStatusDashboardService.GetReceiptPaymentStatusDataList(), Error = false }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult ReceiptPaymentStatusSummaryReport(Dictionary<string, string> data)
        {
            AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                string fileName = "";

                fileName = accountsStatusDashboardService.CreateReceiptPaymentStatusSummaryReportSheet(data);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

        }

    }
}