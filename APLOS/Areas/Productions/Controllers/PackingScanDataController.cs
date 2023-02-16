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
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Accounts;
using Library.ViewModel.Productions;
using Library.Model.Productions;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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

                List<PackingScanUploadedData> data = new List<PackingScanUploadedData>();

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
                                string NetWeight = "0.0";
                                string GWeight = "0.0";
                                NetWeight = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                GWeight = dsExcel.Tables[0].Rows[i][6].ToString().Trim();
                                PackingScanUploadedData vm = new PackingScanUploadedData();

                                //vm.MasterId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.ProductCode = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.POId = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.LotNo = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.RefNo = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                vm.Cones = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                vm.NetWeight = Convert.ToDecimal(string.IsNullOrEmpty(NetWeight) ? "0" : NetWeight);
                                vm.GWeight = Convert.ToDecimal(string.IsNullOrEmpty(GWeight) ? "0" : GWeight);
                                vm.PackedBy = dsExcel.Tables[0].Rows[i][7].ToString().Trim();
                                vm.Shade = dsExcel.Tables[0].Rows[i][8].ToString().Trim();
                                vm.Booked = dsExcel.Tables[0].Rows[i][9].ToString().Trim();
                                vm.PackingId = dsExcel.Tables[0].Rows[i][10].ToString().Trim();
                                vm.AddedBy = dsExcel.Tables[0].Rows[i][11].ToString().Trim();
                                vm.AddedDate = dsExcel.Tables[0].Rows[i][12].ToString().Trim();
                                vm.UpdatedBy = dsExcel.Tables[0].Rows[i][13].ToString().Trim();
                                vm.UpdatedDate = dsExcel.Tables[0].Rows[i][14].ToString().Trim();
                                vm.LocMasterId = dsExcel.Tables[0].Rows[i][15].ToString().Trim();
                                vm.IsDespatch = dsExcel.Tables[0].Rows[i][16].ToString().Trim();
                                vm.BookedDate = dsExcel.Tables[0].Rows[i][17].ToString().Trim();
                                vm.InventoryReceiveDetailId = dsExcel.Tables[0].Rows[i][18].ToString().Trim();
                                vm.SalesId = dsExcel.Tables[0].Rows[i][19].ToString().Trim();
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

        #region Packing Scan Data Upload
        [HttpPost]
        public ActionResult SavePackingScanUploadData(PackingScanUpload packingScanUploadvm, IEnumerable<PackingScanUploadedData> packingScanUploadedDataList)
        {
            SavePackingScanUpload(packingScanUploadvm, packingScanUploadedDataList);

            return Json(new { Message = AplosMessage.Insert });
        }


        public void SavePackingScanUpload(PackingScanUpload packingScanUploadvm, IEnumerable<PackingScanUploadedData> packingScanUploadedDataList)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet _BankReconciliationUploadedData = null;
                DataSet _BankReconciliationUpload = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var packingScanUpload = new PackingScanUpload
                {
                    WorkDate = packingScanUploadvm.WorkDate,
                    Time = packingScanUploadvm.Time,
                    ShiftId = packingScanUploadvm.ShiftId,
                    Grade = packingScanUploadvm.Grade,
                    LocMasterId = packingScanUploadvm.LocMasterId,
                    PurposeId = packingScanUploadvm.PurposeId,
                    Remarks = packingScanUploadvm.Remarks,
                };

                InsertPackingScanUpload(packingScanUpload, ref _BankReconciliationUpload);

                foreach (var item in packingScanUploadedDataList)
                {
                    var packingScanUploadedData = new PackingScanUploadedData
                    {
                        MasterId = packingScanUpload.Id,
                        ProductCode = item.ProductCode,
                        POId = item.POId,
                        LotNo = item.LotNo,
                        RefNo = item.RefNo,
                        Cones = item.Cones,
                        NetWeight = item.NetWeight,
                        GWeight = item.GWeight,
                        PackedBy = item.PackedBy,
                        Shade = item.Shade,
                        Booked = item.Booked,
                        PackingId = item.PackingId,
                        AddedBy = item.AddedBy,
                        AddedDate = item.AddedDate,
                        UpdatedBy = item.UpdatedBy,
                        UpdatedDate = item.UpdatedDate,
                        LocMasterId = item.LocMasterId,
                        IsDespatch = item.IsDespatch,
                        BookedDate = item.BookedDate,
                        InventoryReceiveDetailId = null,
                        SalesId = null
                    };

                    InsertBankReconciliationUploadedData(packingScanUploadedData, ref _BankReconciliationUploadedData);
                }

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_BankReconciliationUpload, _BankReconciliationUploadedData);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void InsertPackingScanUpload(PackingScanUpload packingScanUpload, ref DataSet dsData)
        {
            packingScanUpload.Id = GetAutoNumber(nameof(PackingScanUpload), PKGeneratorEnum.Yearly, null, DateTime.Now);

            if (string.IsNullOrEmpty(packingScanUpload.AddedBy))
                AuditService.AddedLog(packingScanUpload);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [dbo].[ItemScan] where 1=2", out dsData);
            }
            AddNewRow<PackingScanUpload>(dsData.Tables[0], packingScanUpload);

        }
        public void InsertBankReconciliationUploadedData(PackingScanUploadedData packingScanUploadedData, ref DataSet dsData)
        {
            packingScanUploadedData.Id = GetAutoNumber(nameof(PackingScanUploadedData), PKGeneratorEnum.Yearly, null, DateTime.Now);

            if (string.IsNullOrEmpty(packingScanUploadedData.AddedBy))
                AuditService.AddedLog(packingScanUploadedData);
            if (dsData == null || dsData.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [dbo].[ItemScanChild] where 1=2", out dsData);
            }
            AddNewRow<PackingScanUploadedData>(dsData.Tables[0], packingScanUploadedData);

        }
        public string GetAutoNumber(string fieldName, PKGeneratorEnum period, string companyGroupId, DateTime date)
        {
            string prefix = null; var condition = "";
            switch (period.ToString())
            {
                case "Auto":
                    prefix = MakePeriodAuto();
                    condition = $"WHERE FieldName='{fieldName}' AND [Period]='{prefix}' AND (CompanyGroupId IS NULL OR CompanyGroupId='{companyGroupId}') ";
                    break;

                case "Yearly":
                    prefix = MakePeriodYearly(date);
                    condition = $"WHERE FieldName='{fieldName}' AND [Period]='{prefix}' AND (CompanyGroupId IS NULL OR CompanyGroupId='{companyGroupId}') ";
                    break;

                case "Monthly":
                    prefix = MakePeriodMonthly(date);
                    condition = $"WHERE FieldName='{fieldName}' AND [Period]='{prefix}' AND (CompanyGroupId IS NULL OR CompanyGroupId='{companyGroupId}') ";
                    break;

                case "Daily":
                    prefix = MakePeriodDaily(date);
                    condition += $"WHERE FieldName='{fieldName}' AND [Period]='{prefix}' AND (CompanyGroupId IS NULL OR CompanyGroupId='{companyGroupId}') ";
                    break;

                default:
                    break;
            }
            var cId = companyGroupId == null ? "null" : (object)$"'{companyGroupId}'";
            var sql = "DECLARE @lastNumber AS BIGINT=0; " +
                   $"SELECT @lastNumber=MaxNumber FROM [ACS].[PKGenerator] {condition} " +
                   "IF @lastNumber > 0  " +
                   "BEGIN  " +
                       $"UPDATE [ACS].[PKGenerator] SET UpdatedDate=GETDATE(), MaxNumber=@lastNumber + 1 {condition} " +
                   "END " +
                   "ELSE    " +
                       $"INSERT INTO [ACS].[PKGenerator](FieldName, [Period], CompanyGroupId, MaxNumber, UpdatedDate) VALUES('{fieldName}', '{prefix}', {cId}, 1, GETDATE()); " +
                   "SELECT @lastNumber + 1 AS MaxNumber";


            var number = _sqlRepository.GetDataCollection(sql)[0]["MaxNumber"].ToString();
            return period == PKGeneratorEnum.Auto ? number : prefix + number;
        }

        private void AddNewRow<T>(DataTable dt, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));
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

            dt.Rows.Add(dr);
        }
        private void EditRow<T>(DataRow dr, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (item.ToUpper() == "ID")
                        continue;

                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr.EndEdit();
        }

        private static string MakePeriodDaily(DateTime date)
        {
            return date.Year + date.Month.ToString().PadLeft(2, '0') + date.Day.ToString().PadLeft(2, '0');
        }
        private static string MakePeriodMonthly(DateTime date)
        {
            return date.Year + date.Month.ToString().PadLeft(2, '0');
        }

        private static string MakePeriodYearly(DateTime date)
        {
            return date.Year.ToString();
        }
        private static string MakePeriodAuto()
        {
            return PKGeneratorEnum.Auto.ToString();
        }

        #endregion

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
        //[HttpPost, Authorize]
        //public ActionResult SaveAdjustmentJournalBankReconciliationMap(PackingScanUploadedData bankReconciliation, IEnumerable<PackingScanUploadedData> bankReconciliationList)
        //{
        //    AccountsPostInvoiceService accountsPostInvoiceService = new AccountsPostInvoiceService(_sqlRepository);
        //    accountsPostInvoiceService.SaveAdjustmentJournalBankReconciliationMap(bankReconciliation, bankReconciliationList);

        //    return Json(new { Message = AplosMessage.Insert });
        //}
        //[HttpPost]
        //public ActionResult SaveBankReconciliationMap(BankReconciliation bankReconciliation, IEnumerable<PackingScanUploadedData> bankReconciliationList)
        //{
        //    AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
        //    accountsCommonService.SaveBankReconciliationMap(bankReconciliation, bankReconciliationList);

        //    return Json(new { Message = AplosMessage.Insert });
        //}
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

        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            IWorkbook workbook = accountsInventoryPayableReportService.GetPackingScanSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Packing Scan Data upload Sample File";
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

        //public class PackingScanUpload : BaseModel
        //{
        //    //public PackingScanUpload();

        //    public string UpdatedFromIP { get; set; }
        //    [NeverUpdate]
        //    public string AddedBy { get; set; }
        //    [NeverUpdate]
        //    public DateTime AddedDate { get; set; }
        //    [NeverUpdate]
        //    public string AddedFromIP { get; set; }
        //    public string Remarks { get; set; }
        //    //public virtual EmployeeInformation EmployeeInformation { get; set; }
        //    //public string EmployeeId { get; set; }
        //    public string BankStatementNo { get; set; }
        //    public DateTime ToDate { get; set; }
        //    public DateTime FromDate { get; set; }
        //    public decimal ClosingBalance { get; set; }
        //    public decimal OpeningBlance { get; set; }
        //    public virtual BankMaster BankMaster { get; set; }
        //    public string BankMasterId { get; set; }
        //    [NeverUpdate]
        //    public string PlantId { get; set; }
        //    //public virtual Company Company { get; set; }
        //    [NeverUpdate]
        //    public string CompanyId { get; set; }
        //    [NeverUpdate]
        //    public string CompanyGroupId { get; set; }
        //    public string Id { get; set; }
        //    public DateTime? UpdatedDate { get; set; }
        //    public string UpdatedBy { get; set; }
        //}

        //public class PackingScanUploadedData : BaseModel
        //{
        //    //public PackingScanUploadedData();

        //    public string UpdatedFromIP { get; set; }
        //    [NeverUpdate]
        //    public string AddedBy { get; set; }
        //    [NeverUpdate]
        //    public DateTime AddedDate { get; set; }
        //    [NeverUpdate]
        //    public string AddedFromIP { get; set; }
        //    public string BankParticulars { get; set; }
        //    public string Remarks { get; set; }
        //    public string OwnRefNo { get; set; }
        //    public decimal CrAmount { get; set; }
        //    public DateTime? UpdatedDate { get; set; }
        //    public decimal DrAmount { get; set; }
        //    public DateTime BankStatementDate { get; set; }
        //    public virtual PackingScanUpload PackingScanUpload { get; set; }
        //    public string BankReconciliationUploadId { get; set; }
        //    [NeverUpdate]
        //    public string PlantId { get; set; }
        //    //public virtual Company Company { get; set; }
        //    [NeverUpdate]
        //    public string CompanyId { get; set; }
        //    [NeverUpdate]
        //    public string CompanyGroupId { get; set; }
        //    public string Id { get; set; }
        //    public string BankRefNo { get; set; }
        //    public string UpdatedBy { get; set; }
        //}

        //New End
        #endregion Operation
    }
}