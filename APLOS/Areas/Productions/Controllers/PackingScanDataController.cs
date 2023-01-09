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
using Library.Model.Vouchers;
using Library.Security.Core;
using Library.Service.Banks;
using Library.Service.Helpers;
using Library.ViewModel.Accounts;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class PackingScanDataController : BaseController
    {
        #region Constructor

        private readonly IBankReconciliationService _bankReconciliationService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IBankReportService _bankReportService;

        public PackingScanDataController(IBankReconciliationService bankReconciliationService, ISqlRepository sqlRepository, IBankReportService bankReportService)
        {
            _bankReconciliationService = bankReconciliationService;
            _bankReportService = bankReportService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Aplos

        public ActionResult Aplos()
        {
            return View();
        }
        //public ActionResult BankReconciliation()
        //{
        //    return View("~/Areas/Banks/Views/BankReconciliation.cshtml");
        //}
        //public ActionResult BankReconciliationDataUpload()
        //{
        //    return View("~/Areas/Banks/Views/BankReconciliationDataUpload.cshtml");
        //}
        //public ActionResult BankReconciliationDataUploadReconciled()
        //{
        //    return View("~/Areas/Banks/Views/BankReconciliationDataUploadReconciled.cshtml");
        //}

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

        //New 

        [HttpPost, Authorize]
        public JsonResult GetPurpose()
        {
            try
            {
                string sql = "";
                    sql = @"select Id as PurposeId, UserName as Text from [HKP].[MaterialMovementPurpose]";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult GetMaterialMovementList(string purposeId)
        {
            try
            {
                string sql = "";
                sql = @"select Id LocMasterId,FromLocation,ToLocation  from [MST].[MaterialMovementMaster]
                        where PurposeId='" + purposeId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpGet, Authorize]
        public JsonResult GetShiftList()
        {
            string sql = @"SELECT distinct sd.SystemID [Value],sd.UserName [Text] FROM [dbo].[WorkCenterWiseShift] WCS
                                        LEFT JOIN dbo.ShiftDefination AS sd ON sd.SystemID = WCS.ShiftDefinationID
                                        WHERE WorkCenterMasterId IN(SELECT Id FROM SCS.WorkCenterMaster AS wcm)";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public Dictionary<string, object> Save(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "dbo.ItemScan";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                bplib.clsGenID genid = new bplib.clsGenID();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data Master update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        //public JsonResult Save(Dictionary<string, object> data)
        //{
        //    try
        //    {
        //        DataSet dsMaster;
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
        //        con.OpenDataSetThroughAdapter("select * from dbo.ItemScan where Id='" + data["Id"] + "'", out dsMaster, false, "1");
        //        string _Id = "";

        //        #region data update
        //        if (dsMaster.Tables[0].Rows.Count == 0)
        //        {
        //            bplib.clsGenID genid = new bplib.clsGenID();
        //            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "dbo.ItemScan", out _Id);

        //            data["Id"] = _Id;
        //            AddNewRow(dsMaster.Tables[0], data);
        //        }
        //        else
        //        {
        //            _Id = data["Id"].ToString();
        //            EditRow(dsMaster.Tables[0].Rows[0], data);
        //        }
        //        #endregion data update
        //        clsStaticInfo _info = new clsStaticInfo();
        //        _info.SaveDataSets(dsMaster);

        //        return Json(new { Error = false, Message = AplosMessage.Insert });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, Message = ex.Message });
        //    }
        //}

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();

            dr.EndEdit();
        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }




            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();

            dt.Rows.Add(dr);
        }

        //New End
        #endregion Operation
    }
}