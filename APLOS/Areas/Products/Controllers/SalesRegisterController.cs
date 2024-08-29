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
using Aplos.MaterialManagement.MaterialQuery;




#endregion using

namespace Aplos.Areas.Products.Controllers
{
    public class SalesRegisterController : BaseController
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

        public SalesRegisterController(
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

        public ActionResult SalesRegister()
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

        [Authorize, HttpPost]
        public JsonResult GetMaterialLedger(string fromDate, string toDate)
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
            var jsondata = Json(_inventoryReceiveService.GetMaterialLedger(fromDate, toDate), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [Authorize, HttpPost]
        public ActionResult GetSalesRegister(string FromDate, string ToDate, string Type)
        {

            DateTime fDate = DateTime.Parse(FromDate);
            DateTime tDate = DateTime.Parse(ToDate);
            if (FromDate == null || FromDate == "")
            {
                throw new CustomException("Select From Date");
            }
            else if (ToDate == null || ToDate == "")
            {
                throw new CustomException("Select To Date");
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            SalesQueryService obj = new SalesQueryService(_sqlRepository);
            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson
                (obj.GetSalesRegisterSql(FromDate, ToDate, Type));
            var jsondata = Json(new { NewData, Message = AplosMessage.Success });
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        #endregion Pages
        #region material-ledger Reports

        [Authorize, HttpGet]
        public ActionResult Report(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string Unit)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "MaterialLedger " + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.CreateMaterialLedgerReportSheet(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, Unit);
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
        public ActionResult MaterialStockBalanceReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Country, string materialStorage,bool bale,bool brand)

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Material Stock Balance" + fromDate + "To" + toDate + "";

            Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
            var workbook = obj.CreateMaterialStockBalanceSheet(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory, Country, materialStorage,bale,brand);

            //var workbook = _materialMasterService.CreateMaterialStockBalanceSheet(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory,Country);
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
        public ActionResult MaterialStoreLedgerReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId, string Sku1, string Sku2, string Sku3)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            if (string.IsNullOrEmpty(Sku1) || Sku1 == "undefined")
                Sku1 = null;
            if (string.IsNullOrEmpty(Sku2) || Sku2 == "undefined")
                Sku2 = null;
            if (string.IsNullOrEmpty(Sku3) || Sku3 == "undefined")
                Sku3 = null;
            var reportFileName = "Material Store Ledger" + fromDate + "To" + toDate + "";
            var workbook = _materialMasterService.CreateMaterialStoreLedger(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, MaterialId, ArticleId, Sku1, Sku2, Sku3);
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

        [Authorize, HttpGet]
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

        #region SalesRegister



        [HttpPost, Authorize]
        public ActionResult SalesRegisterCustomerWiseData(string PlantId, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalesQueryService obj = new SalesQueryService(_sqlRepository);
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(obj.getSalesOrderCustomerWiseReportSql(identity.CompanyId, identity.PlantId, FromDate, ToDate,null,false));
                var jsondata= Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        [HttpPost, Authorize]
        public ActionResult GetSalesRegisterItemWiseData(string PlantId, string ToDate, string FromDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SalesQueryService obj = new SalesQueryService(_sqlRepository);
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(obj.GetSalesRegisterItemWiseData(identity.CompanyId, identity.PlantId, FromDate, ToDate));
                var jsondata= Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
       
    }


}