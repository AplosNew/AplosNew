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
        

        #endregion Aplos

        #region Operation
      
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
                        Booked = "0",
                        PackingId = item.PackingId,
                        AddedBy = item.AddedBy,
                        AddedDate = item.AddedDate,
                        UpdatedBy = item.UpdatedBy,
                        UpdatedDate = item.UpdatedDate,
                        LocMasterId = item.LocMasterId,
                        IsDespatch = "0",
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