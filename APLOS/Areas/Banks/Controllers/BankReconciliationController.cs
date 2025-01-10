#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Service.Banks;
using Library.Service.Helpers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Banks.Controllers
{
    public class BankReconciliationController : BaseController
    {
        #region Constructor

        private readonly IBankReconciliationService _bankReconciliationService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IBankReportService _bankReportService;

        public BankReconciliationController(IBankReconciliationService bankReconciliationService, ISqlRepository sqlRepository, IBankReportService bankReportService)
        {
            _bankReconciliationService = bankReconciliationService;
            _bankReportService = bankReportService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Aplos


        public ActionResult BankReconciliation()
        {
            return View("~/Areas/Banks/Views/BankReconciliation.cshtml");
        }
        public ActionResult BankReconciliationDataUpload()
        {
            return View("~/Areas/Banks/Views/BankReconciliationDataUpload.cshtml");
        }
        public ActionResult BankReconciliationDataUploadReconciled()
        {
            return View("~/Areas/Banks/Views/BankReconciliationDataUploadReconciled.cshtml");
        }
        public ActionResult BankReconciliationClosing()
        {
            return View("~/Areas/Banks/Views/BankReconciliationClosing.cshtml");
        }

        #endregion Aplos

        #region Operation
        [Authorize, HttpGet]
        public JsonResult GetBankreconciliationList(GridParameter parameters)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankReconciledList(DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankReconciledList(identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssuedNotPresentList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetIssuedNotPresentList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetReceivedNotPresentList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetReceivedNotPresentList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCrReconList(GridParameter parameters, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankCrReconList(parameters, identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetBankCrReconListSyncfusion(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetBankCrReconListSyncfusion(identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetBankDrReconListSyncfusion(DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetBankDrReconListSyncfusion(identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [HttpPost, Authorize]
        public ActionResult GetBankDrReconListUploadedData(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetBankDrReconListUploadedData(identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpGet, Authorize]
        public JsonResult GetBankDrReconList(GridParameter parameters, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankDrReconList(parameters, identity.CompanyGroupId, identity.CompanyId, cutOffDate, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankReconLastDate(string bankMasterId)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankReconLastDate(identity.CompanyGroupId, identity.CompanyId, bankMasterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBankReconUploadLastDate(string bankMasterId)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankReconUploadLastDate(identity.CompanyGroupId, identity.CompanyId, bankMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankReconDrCrTotalAmount(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankReconDrCrTotalAmount(identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(BankReconciliation bankReconciliation, List<GLTransactionDetail> tempList)
        {
            _bankReconciliationService.InsertBankReconciliation(bankReconciliation, tempList);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpGet, Authorize]
        public ActionResult CRReconcileReport(string BankMasterID, string fromDate, string toDate)
        {
            try
            {
                _bankReportService.CRReconcileReport(BankMasterID, fromDate, toDate);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult DRReconcileReport(string BankMasterID, string fromDate, string toDate, string cutOffDate)
        {
            try
            {
                _bankReportService.DRReconcileReport(BankMasterID, fromDate, toDate, cutOffDate);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpGet, Authorize]
        public ActionResult DRReconcilePendingReport(string bankMasterId, string fromDate, string toDate)
        {
            try
            {
                _bankReportService.DRReconcilePendingReport(bankMasterId, fromDate, toDate);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpGet, Authorize]
        public ActionResult CRReconcilePendingReport(string bankMasterId, string fromDate, string toDate)
        {
            try
            {
                _bankReportService.CRReconcilePendingReport(bankMasterId, fromDate, toDate);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost]
        public JsonResult DeleteBankreconciliation(string bankReconciliationId)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);

            accountsBankReconcilliationService.DeleteBankreconciliation(bankReconciliationId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult ImportData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                List<BankReconciliationUploadedDataViewModel> data = new List<BankReconciliationUploadedDataViewModel>();

                var pre = form["modelNew"];
                var file = Request.Files["file"];
                var _objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(pre);
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {

                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                string drAmount = "0.0";
                                string crAmount = "0.0";
                                drAmount = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                crAmount = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                BankReconciliationUploadedDataViewModel vm = new BankReconciliationUploadedDataViewModel();

                                vm.DrAmount = Convert.ToDecimal(string.IsNullOrEmpty(drAmount) ? "0" : drAmount);
                                vm.CrAmount = Convert.ToDecimal(string.IsNullOrEmpty(crAmount) ? "0" : crAmount);
                                vm.BankStatementDate = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.BankRefNo = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.BankParticulars = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.Remarks = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                vm.OwnRefNo = dsExcel.Tables[0].Rows[i][6].ToString().Trim();
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
                        }
                    }
                    catch (Exception ex)
                    {

                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            docFile.Delete();
                        }
                        throw (ex);
                    }

                }
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult SaveBankReconciliationUploadData(BankReconciliationUpload bankReconciliationUploadvm, IEnumerable<BankReconciliationUploadedData> bankReconciliationUploadedDataList)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.SaveBankReconciliationUploadData(bankReconciliationUploadvm, bankReconciliationUploadedDataList);

            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpGet, Authorize]
        public JsonResult LoadBankReconciliationUploadedData(string bankMasterId)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetBankReconciliationUploadedData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankMasterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBankReconciliationUploadedDataReport(ReportFormat reportFormat, string bankReconciliationUploadId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "UploadedData";
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var workbook = accountsInventoryPayableReportService.GetBankReconciliationUploadedDataReport(identity.CompanyId, identity.PlantId, bankReconciliationUploadId, reportFileName);
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
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            IWorkbook workbook = accountsInventoryPayableReportService.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "BankReconciliation Data upload Sample File";
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
        [HttpPost]
        public ActionResult DeleteBankReconciliationUploadedData(string bankReconciliationUploadId)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);

            accountsBankReconcilliationService.DeleteBankReconciliationUploadedData(bankReconciliationUploadId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public ActionResult DeleteBankReconciliationMapData(string voucherDetailId)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);

            accountsBankReconcilliationService.DeleteBankReconciliationMapData(voucherDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [Authorize, HttpGet]
        public JsonResult GetAvailableBankReconciliationUploadedDataList(GridParameter parameters, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsBankReconcilliationService.GetAvailableBankReconciliationUploadedDataList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankMasterId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetAvailableBankReconciliationUploadedDrDataList(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetAvailableBankReconciliationUploadedDrDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [HttpPost, Authorize]
        public ActionResult SaveAdjustmentJournalBankReconciliationMap(BankReconciliationUploadedDataViewModel bankReconciliation, IEnumerable<BankReconciliationUploadedDataViewModel> bankReconciliationList)
        {
            AccountsPostInvoiceService accountsPostInvoiceService = new AccountsPostInvoiceService(_sqlRepository);
            accountsPostInvoiceService.SaveAdjustmentJournalBankReconciliationMap(bankReconciliation, bankReconciliationList);

            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        public ActionResult SaveBankReconciliationMap(BankReconciliation bankReconciliation, IEnumerable<BankReconciliationUploadedDataViewModel> bankReconciliationList)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.SaveBankReconciliationMap(bankReconciliation, bankReconciliationList);

            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost, Authorize]
        public ActionResult GetBankDrReconciledList(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetBankDrReconciledList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [HttpPost, Authorize]
        public ActionResult GetBankCrReconListUploadedData(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetBankCrReconListUploadedData(identity.CompanyGroupId, identity.CompanyId, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [HttpPost, Authorize]
        public ActionResult GetAvailableBankReconciliationUploadedCrDataList(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetAvailableBankReconciliationUploadedCrDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [HttpPost, Authorize]
        public ActionResult GetBankCrReconciledList(string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(new { DATA = accountsBankReconcilliationService.GetBankCrReconciledList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankMasterId, fromDate, toDate), Error = false }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpGet, Authorize]
        public ActionResult GetBankReconciliationUploadedDataForMailReport(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var reportFileName = "BankReconciliationUploadData";
                AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
                IWorkbook workbook = accountsInventoryPayableReportService.GetBankReconciliationUploadedDataForMailReport(identity.CompanyId, identity.PlantId, reportFileName);

                string strFileName = "BankReconciliationUploadData.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        #endregion Operation

        #region Bank Reconciliation Closing
        [Authorize, HttpPost]
        public ActionResult GetBankReconciliationClosingList(string column, string value)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(accountsBankReconcilliationService.GetBankReconciliationClosingList(column, value, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveBankReconciliationClosing(Dictionary<string, object> data)
        {
            try
            {
                AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
                accountsBankReconcilliationService.SaveBankReconciliationClosingData(data, out string masterId);
                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }
        #endregion

        #region Bank Settlement
        public ActionResult BankSettlementReconciliation()
        {
            return View("~/Areas/Banks/Views/BankSettlementReco/BankSettlementReconciliation.cshtml");
        }
        [Authorize]
        public ActionResult BankSettlementCustomerAdvance()
        {
            return View("~/Areas/Banks/Views/BankSettlementReco/BankSettlementCustomerAdvance.cshtml");
        }
        [Authorize]
        public ActionResult BankSettlementCustomerReceipt()
        {
            return View("~/Areas/Banks/Views/BankSettlementReco/BankSettlementCustomerReceipt.cshtml");
        }
        [Authorize]
        public ActionResult BankSettlementJournal()
        {
            return View("~/Areas/Banks/Views/BankSettlementReco/BankSettlementJournal.cshtml");
        }
        [Authorize, HttpPost]
        public ActionResult GetBankUploadInfoById(string id)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            return Json(accountsBankReconcilliationService.GetBankUploadInfoById(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetBankDrUploadInfoById(string id)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            return Json(accountsBankReconcilliationService.GetBankDrUploadInfoById(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult InsertExpenseToBankReconcil(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.BankJournal.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");

            if (voucherVM.BankJournalType == BankJournalType.ProfitEarn.ToString() && voucherVM.FinancingTypeId == null)
                throw new CustomException("Please select Investment Type!");

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, accountsBankReconcilliationService.InsertExpenseToBankReconcil(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult InsertCustomerInvoiceReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.CustomerReceipt.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                if (voucherVM.CurrencyId != advanceDetailVM.CurrencyId)
                    throw new CustomException("Transaction currency and Payable currency should be same.!!!");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, accountsBankReconcilliationService.InsertCustomerInvoiceReceipt(voucherVM, voucherDetailVMList, bankChargeDetailVMList, taxDetailVMList)) });
        }

        [HttpPost]
        public JsonResult ParkCustomerAdvance(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList
           )
        {
            AccountsBankReconcilliationService accountsBankReconcilliationService = new AccountsBankReconcilliationService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            if (advanceVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if ((advanceVM.Amount < 0 || advanceVM.Amount == 0))
                throw new CustomException("Please Input Amount !");
            advanceVM.SourceType = SourceType.CustomerAdvance.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, accountsBankReconcilliationService.InsertCustomerAdvance(advanceVM, advanceDetailVMList)) });
        }
        #endregion
    }
}