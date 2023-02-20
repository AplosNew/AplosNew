#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Materials;
using Library.ViewModel.Materials;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Aplos.MaterialManagement;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;




#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialLedgerController : BaseController
    {
        #region -- Constructor
        //private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IInventoryReceiveService _inventoryReceiveService;
        private readonly ISqlRepository _sqlRepository;

        private readonly IMaterialMasterAlternativeUOMService _materialMasterAlternativeUOMService;
        private readonly IMaterialMasterProcessRoutingService _materialMasterProcessRoutingService;
        private readonly IMaterialMasterUsageService _materialMasterUsageService;
        private readonly IMaterialMasterAttributeValueService _materialMasterAttributeValueService;
        private readonly IMaterialAttributeValueService _materialValueService;
        private readonly IMaterialMasterCharacteristicsValueService _materialMasterCharacteristicsValueService;
        private readonly IMaterialMasterProcessSetService _materialMasterProcessService;
        private readonly IMaterialMasterMachineProcessService _assetItemProcessService;
        //private readonly IInventoryReceiveService _inventoryReceiveService;

        public MaterialLedgerController(
              ISqlRepository sqlRepository,
              IInventoryReceiveService inventoryReceiveService
             , IMaterialMasterService materialMasterService
            , IMaterialMasterAlternativeUOMService materialMasterAlternativeUOMService
            , IMaterialMasterProcessRoutingService materialMasterProcessRoutingService
            , IMaterialMasterUsageService materialMasterUsageService
            , IMaterialMasterAttributeValueService materialMasterAttributeValueService
            , IMaterialMasterCharacteristicsValueService materialMasterCharacteristicsValueService
            , IMaterialMasterProcessSetService materialMasterProcessService
            , IMaterialMasterMachineProcessService assetItemProcessService
            , IMaterialAttributeValueService materialValueService
        
            )
        {

            _sqlRepository = sqlRepository;
             _inventoryReceiveService = inventoryReceiveService;
            _materialMasterService = materialMasterService;
            _materialMasterAlternativeUOMService = materialMasterAlternativeUOMService;
            _materialMasterProcessRoutingService = materialMasterProcessRoutingService;
            _materialMasterUsageService = materialMasterUsageService;
            _materialMasterAttributeValueService = materialMasterAttributeValueService;
            _materialMasterCharacteristicsValueService = materialMasterCharacteristicsValueService;
            _materialMasterProcessService = materialMasterProcessService;
            _assetItemProcessService = assetItemProcessService;
            _materialValueService = materialValueService;
    

        }

        #endregion -- Constructor

        #region Pages
        public ActionResult Aplos()
        {
            return View();
        }
		public ActionResult MaterialStockBalance()
		{
			return View();
		}
    
        public ActionResult MaterialStoreLedger()
        {
            return View();
        }
      
        public ActionResult MaterialConsumption()
        {
            return View();
        }
        public ActionResult MaterialReceiptsReport()
        {
            return View();
        }
    
        public ActionResult MaterialIssueReport()
        {
            return View();
        }
    
		public ActionResult PurchaseRegister() 
		{
			return View();
		}

        [Authorize]
        public ActionResult PurchaseOrderRegister()
        {
            return View();
        }


        public ActionResult ServiceAcktRegister()
        {
            return View();
        }


        public ActionResult MaterialMasterStock() 
        {
            return View();
        }

        public ActionResult Materialstationeryrequest()
        {
            return View();
        }
       
        public ActionResult PhysicalInventory()
        {
            return View();
        }
      
        public ActionResult PurchaseReturnRegister()
        {
            return View();
        }
        public ActionResult ServicePORegister()
        {
            return View();
        }

        public ActionResult InWardMaterial()
        {
            return View();
        }

        [Authorize, HttpPost]
		public JsonResult GetMaterialLedger(string fromDate,string toDate)
        {
			DateTime fDate = DateTime.Parse(fromDate);
			DateTime tDate = DateTime.Parse(toDate);
			if (fromDate == null || fromDate == "")
			{
				throw new CustomException("Select From Date");
			}
			else if (toDate == null || toDate == "")
			{
				throw new CustomException("Select To Date");
			}
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata= Json(_inventoryReceiveService.GetMaterialLedger(fromDate,toDate), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
		}
        


   

        #endregion Pages
        #region material-ledger Reports

        [Authorize, HttpGet]
        public ActionResult Report(ReportFormat reportFormat,  string plantId, string fromDate, string toDate,string Qty ,string Amount, string Unit)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "MaterialLedger " + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.CreateMaterialLedgerReportSheet(identity.CompanyId, plantId, fromDate, toDate,Qty,Amount, Unit);
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
        #endregion

        #region material-stock balance


        [Authorize, HttpGet]
		public ActionResult MaterialStockBalanceReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue,string Asset,string Inventory, string Country,string materialStorage) 
	            {
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			plantId = identity.PlantId;
			var reportFileName = "Material Stock Balance" + fromDate + "To" + toDate + "";
          
            Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                var workbook = obj.CreateMaterialStockBalanceSheet(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory, Country, materialStorage);

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
        #endregion

        #region Material-Store-Ledger

        [Authorize, HttpGet]
        public ActionResult MaterialStoreLedgerReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId,string Sku1,string Sku2,string Sku3)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            if (string.IsNullOrEmpty(Sku1) || Sku1 == "undefined")
                Sku1 = null;
            if (string.IsNullOrEmpty(Sku2) || Sku2 == "undefined")
                Sku2 = null;
            if (string.IsNullOrEmpty(Sku3) || Sku3 == "undefined" )
                Sku3 = null;
            var reportFileName = "Material Store Ledger" + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.CreateMaterialStoreLedger(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue,  MaterialId,  ArticleId,Sku1,Sku2,Sku3);
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
        #endregion

        
        #region material-consumption-report
        [Authorize, HttpPost]
        public JsonResult GetMaterialConsumptionGL(string fromDate, string toDate, string Type)
        {
            if (fromDate == null || fromDate == "")
            {
                throw new CustomException("Select From Date");
            }
            else if (toDate == null || toDate == "")
            {
                throw new CustomException("Select To Date");
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.GetMaterialConsumption(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetMaterialConsumptionCostCenter(string fromDate, string toDate, string Type)
        {
            if (fromDate == null || fromDate == "")
            {
                throw new CustomException("Select From Date");
            }
            else if (toDate == null || toDate == "")
            {
                throw new CustomException("Select To Date");
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.GetMaterialConsumptionCostCenter(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
        }

        [Authorize,  HttpGet]
        public ActionResult MaterialConsumptionReports(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue)
        {
            var reportFileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            if (RcptIssue == "null")
            {
                 reportFileName = "GLWiseMaterialConsumptionReport" + fromDate + "To" + toDate + "";
            }
            else
            {
                 reportFileName = "CostCenterWiseMaterialConsumptionReport" + fromDate + "To" + toDate + "";
            }
            
            var workbook = _materialMasterService.CreateMaterialConsumptionReport(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue);
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
        #endregion

        #region PurchaseRegister

        [HttpPost, Authorize]
        public ActionResult PurchaseRegisterGRNWiseData(string PlantId, string ToDate, string FromDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(obj.GetPurchaseRegisterGRNWiseData(identity.CompanyId, identity.PlantId, FromDate, ToDate, null,false));
                var jsondata = Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult PurchaseRegisterGRNWiseReport(string PlantId, string ToDate, string FromDate, string GRNNo, string SheetName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);

                string fileName = "";
                fileName = obj.CreatePurchaseRegisterGRNWiseReportSheet(identity.CompanyId, identity.PlantId, FromDate, ToDate,GRNNo, "Purchase Register Report GRN Wise " + FromDate + " To " + ToDate + "");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public JsonResult GetPurchaseRegisterPartyWiseData(string fromDate, string toDate, string Type)
        {

            DateTime fDate = DateTime.Parse(fromDate);
            DateTime tDate = DateTime.Parse(toDate);
            if (fromDate == null || fromDate == "")
            {
                throw new CustomException("Select From Date");
            }
            else if (toDate == null || toDate == "")
            {
                throw new CustomException("Select To Date");
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);
            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(obj.GetPurchaseRegisterPartyWiseData(identity.CompanyId, identity.PlantId, fromDate, toDate,null,false));
            return Json(new { NewData, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public ActionResult PurchaseRegisterPartyWiseReport(string PlantId, string ToDate, string FromDate,string PartyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);

                string fileName = "";
                fileName = obj.CreatePurchaseRegisterPartyWiseReportSheet(identity.CompanyId, identity.PlantId, FromDate, ToDate, PartyId, "Purchase Report Register Party Wise" + FromDate + " To " + ToDate + "");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [Authorize, HttpPost]
        public JsonResult PurchaseRegisterItemWiseData(string fromDate, string toDate, string Type)
        {
            DateTime fDate = DateTime.Parse(fromDate);
            DateTime tDate = DateTime.Parse(toDate);
            if (fromDate == null || fromDate == "")
            {
                throw new CustomException("Select From Date");
            }
            else if (toDate == null || toDate == "")
            {
                throw new CustomException("Select To Date");
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);
            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(obj.GetPurchaseRegisterItemData(identity.CompanyId, identity.PlantId, fromDate, toDate,null, false));
            var jsondata = Json(new { NewData, Message = AplosMessage.Success });
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpPost]
        public ActionResult PurchaseRegisterItemWiseReport(string plantId, string fromDate, string toDate,string SLNo,string SheetName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);

            string fileName = "";
            fileName = obj.CreatePurchaseRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, SLNo, "Purchase Register Item Wise " + fromDate + " To " + toDate + "");
            return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


        }


        [Authorize, HttpGet]
        public ActionResult GetStatusAllGRNPendingList(string CompanyId, string GRNPendingStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.GetGRNPendingList(identity.CompanyGroupId, GRNPendingStatus), JsonRequestBehavior.AllowGet);
        }

        #endregion
        private class TempGRNDelay
        {
            public string Description { get; set; } = "";
            public int From { get; set; } = 0;
            public int To { get; set; } = 0;
            public int Count { get; set; } = 0;
        }
        [Authorize, HttpGet]
        public ActionResult GetPendingListGRN()
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string CompanyId = identity.CompanyId;
                List<TempGRNDelay> Slot = new List<TempGRNDelay>();
                Slot.Add(new TempGRNDelay { Description = "1-3", From = 1, To = 3, Count = 0 });
                Slot.Add(new TempGRNDelay { Description = "4-10", From = 4, To = 10, Count = 0 });
                Slot.Add(new TempGRNDelay { Description = "11-20", From = 11, To = 20, Count = 0 });
                Slot.Add(new TempGRNDelay { Description = "21-30", From = 21, To = 30, Count = 0 });
                Slot.Add(new TempGRNDelay { Description = "More Than 30 Days", From = 30, To = 9000000, Count = 0 });

                string sql = @"SELECT G.Id
                          ,ISNULL(PWG.UserName,'') GateName
                          ,Replace(CONVERT(VARCHAR(11),G.EntryDate, 106), ' ', '-') EntryDate ,DATEDIFF(day,G.EntryDate,getdate()) AS DaysCount
                          ,P.Code PartyCode 
						  ,isnull(p.UserName,'') PartyName
						  ,CG.UserName CompanyGrpName
						  ,C.UserName CompanyName
						  ,Pl.UserName PlantName
	                      --,isnull(P.UserName,'') As PartyName
                          ,isnull(G.Description,'') Description
                          ,G.PackageQty
                          ,G.ModeofTransport
                          ,G.Bill
                          ,G.PersonName
                          ,G.MobileNo
                          ,Isnull(G.Remarks,'') Remarks
                          ,G.AddedBy
                          ,G.AddedDate
                          ,G.AddedFromIP
                          ,G.UpdatedBy
                          ,G.UpdatedDate
                          ,G.UpdatedFromIp
                          ,EI.FirstName As MaterialReceivedBy,G.GateEntryTime
                          ,IR.Id GRNId
						  ,Isnull(EI1.SystemId +'-'+ EI1.FirstName,'') AS EmployeeName
                      FROM TRN.[GateEntry] G
                      LEFT Join hkp.Party p ON P.Id= G.PartyId
					  LEFT Join ORG.CompanyGroup CG ON CG .Id= G.CompanyGroupId
					  LEFT Join ORG.Company C ON C.Id= G.CompanyId
					  LEFT Join ORG.Plant Pl ON Pl.Id= G.PlantId
                      Left join trn.InventoryReceive IR ON IR.GateEntryNo=G.Id
                      LEFT JOin dbo.EmployeeInformation EI ON  EI.SystemId=G.EmployeeId
                      LEFT JOin dbo.EmployeeInformation EI1 ON  EI1.SystemId=G.EmployeeIdForGateEntry
                      LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=G.PlantWiseGateId
                      Left JOIN SEC.UserPlantGate UPG ON UPG.PlantGateId=PWG.Id
                      Where G.FlagStatus!='Cancel' 
                        AND G.PlantId='" + identity.PlantId + @"'
                     AND G.Id not in (select GateEntryNo from trn.InventoryReceive where GateEntryNo is not null)
					AND  CONVERT(DATE, G.EntryDate)<Convert(date,GETDATE())
                    Order By G.Id Desc";
                DataTable dtDays = _sqlRepository.GetDataTable(sql);

                foreach (TempGRNDelay item in Slot)
                {
                    item.Count = (int)clsStaticInfo.dbl(dtDays.Compute("COUNT(DaysCount)", "DaysCount >= " + item.From.ToString() + " AND " + "DaysCount <= " + item.To.ToString()).ToString());

                }
                return Json(Slot, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        #region material-Receipts- Reports

        [Authorize, HttpGet]
        public ActionResult MaterialReceiptsReports(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue,string Asset,string Inventory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Material Receipts Reports" + fromDate + "To" + toDate + "";
           var workbook = _materialMasterService.CreateMaterialReceiptsReports(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory);
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
        #endregion

        #region material-issue- Reports
        [Authorize, HttpGet]
        public ActionResult MaterialIssueReports(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Material Issue Reports" + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.CreateMaterialIssueReports(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue);
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
        #endregion

        [Authorize, HttpGet]
        public ActionResult MaterialMasterStatus(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Material Master Stock " + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.MaterialMasterStatus(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory);
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

        #region material-stationery-request
        [Authorize, HttpGet]
        public ActionResult MaterialStationeryRequestReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Material Stationery Request" + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.CreateMaterialStationeryRequestReport(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory);
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
        #endregion

        #region Physical Inventory Report
        [Authorize, HttpGet]
        public ActionResult PhysicalInventoryReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Physical Inventory Report" + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.CreatePhysicalInventoryReport(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory);
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
        #endregion

        #region purchase return register

        [Authorize, HttpPost]
        public JsonResult GetPurchaseReturnRegister(string fromDate, string toDate, string Type)
        {

            DateTime fDate = DateTime.Parse(fromDate);
            DateTime tDate = DateTime.Parse(toDate);
            if (fromDate == null || fromDate == "")
            {
                throw new CustomException("Select From Date");
            }
            else if (toDate == null || toDate == "")
            {
                throw new CustomException("Select To Date");
            }

            //else if (tDate  < fDate)
            //{
            //	throw new CustomException("To Date can not less than From date");
            //}
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_inventoryReceiveService.GetPurchaseReturnRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpGet]
        public ActionResult PurchaseRegisterReturnReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Material Return Register" + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.CreatePurchaseRegisterReturnReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);
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

        #endregion

        #region Purchase Order Register
        public JsonResult GetPurchaseOrderRegister(string fromDate, string toDate, string Type)
        {
            try
            {
                DateTime fDate = DateTime.Parse(fromDate);
                DateTime tDate = DateTime.Parse(toDate);
                if (fromDate == null || fromDate == "")
                {
                    throw new CustomException("Select From Date");
                }
                else if (toDate == null || toDate == "")
                {
                    throw new CustomException("Select To Date");
                }


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
                var jsondata = Json(obj.PurchaseOrderRegisterData(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult PurchaseOrderRegisterReport(ReportFormat reportFormat, string fromDate, string toDate, string Type,string POId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string plantId = identity.PlantId;
            var reportFileName = "Purchase Report Register" + fromDate + "To" + toDate + "";
            Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
            // return Json(obj.CreatePurchaseRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
            var workbook = obj.CreatePurchaseOrderRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type, POId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);
                default:
                    return View();
            }
        }

        #endregion Purchase Order Register

        #region Service PO Register
        [Authorize, HttpPost]
        public JsonResult GetServicePurchaseOrderRegister(string fromDate, string toDate, string Type)
        {
            try
            {
                DateTime fDate = DateTime.Parse(fromDate);
                DateTime tDate = DateTime.Parse(toDate);
                if (fromDate == null || fromDate == "")
                {
                    throw new CustomException("Select From Date");
                }
                else if (toDate == null || toDate == "")
                {
                    throw new CustomException("Select To Date");
                }


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
                var jsondata = Json(obj.ServicePurchaseOrderRegisterData(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public ActionResult ServicePORegisterReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Service PO Register Report" + fromDate + "To" + toDate + "";
            Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
            var workbook = obj.CreateServicePurchaseOrderRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);
                default:
                    return View();
            }
        }

        #endregion

        #region service-acknowledgement-register
        public JsonResult GetServiceAcknowledgementRegister(string fromDate, string toDate, string Type)
        {
            try
            {
                DateTime fDate = DateTime.Parse(fromDate);
                DateTime tDate = DateTime.Parse(toDate);
                if (fromDate == null || fromDate == "")
                {
                    throw new CustomException("Select From Date");
                }
                else if (toDate == null || toDate == "")
                {
                    throw new CustomException("Select To Date");
                }


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
                return Json(obj.ServiceAcknowledgementRegisterGridData(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult ServiceAcknowledgementRegisterReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Service Acknowledgement Register" + fromDate + "To" + toDate + "";
            Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
            // return Json(obj.CreatePurchaseRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
            var workbook = obj.CreateServiceAcknowledgementRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);
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

        #endregion Purchase Order Register

        #region Material-Store-Ledger ALL

        [Authorize, HttpGet]
        public ActionResult MaterialStoreLedgerReportAll(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId, string Asset, string Inventory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "";
            plantId = identity.PlantId;
            if (Asset == "" || Asset == "undefined" || Asset == null || Asset == "false")
                Asset = null;
            if (Inventory == "" || Inventory == "undefined" || Inventory == null || Inventory == "false")
                Inventory = null;
            if(Asset==null && Inventory !=null)
			{
                reportFileName = "Material Store Ledger Of Inventory";// + fromDate + "To" + toDate + "";
            }
            if (Asset != null && Inventory == null)
            {
                reportFileName = "Material Store Ledger Of Asset";// + fromDate + "To" + toDate + "";
            }
            if (Asset != null && Inventory != null)
            {
                reportFileName = "Material Store Ledger Of Inventory And Asset";// + fromDate + "To" + toDate + "";
            }
           
            Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
            var workbook = obj.CreateMaterialStoreLedgerAll(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, MaterialId, ArticleId,  Asset,  Inventory);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);

                default:
                    return View();
            }
        }
        #endregion

        #region In Ward Material Report

        public JsonResult GetInWardMaterialData(string fromDate, string toDate)
        {
            try
            {
                DateTime fDate = DateTime.Parse(fromDate);
                DateTime tDate = DateTime.Parse(toDate);
                if (fromDate == null || fromDate == "")
                {
                    throw new CustomException("Select From Date");
                }
                else if (toDate == null || toDate == "")
                {
                    throw new CustomException("Select To Date");
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
                var jsondata = Json(obj.InWardMaterialSql(fromDate, toDate), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult InWardMaterialReport(string fromDate, string toDate,List<Dictionary<string, string>> IRDId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = _materialMasterService.CreateInWardMaterialReport( identity.CompanyId, identity.PlantId, fromDate, toDate, IRDId);
                string strFileName = "In Ward Material.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        #endregion

    }


}