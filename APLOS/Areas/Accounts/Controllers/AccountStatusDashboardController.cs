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
    }
}