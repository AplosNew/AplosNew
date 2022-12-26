#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Materials;
using Library.ViewModel.Materials;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class IssueRegisterController : BaseController
    {
        #region -- Constructor
        //private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IInventoryReceiveService _inventoryReceiveService;
        private readonly IInventoryIssueService _inventoryIssueService;

        private readonly IMaterialMasterAlternativeUOMService _materialMasterAlternativeUOMService;
        private readonly IMaterialMasterProcessRoutingService _materialMasterProcessRoutingService;
        private readonly IMaterialMasterUsageService _materialMasterUsageService;
        private readonly IMaterialMasterAttributeValueService _materialMasterAttributeValueService;
        private readonly IMaterialAttributeValueService _materialValueService;
        private readonly IMaterialMasterCharacteristicsValueService _materialMasterCharacteristicsValueService;
        private readonly IMaterialMasterProcessSetService _materialMasterProcessService;
        private readonly IMaterialMasterMachineProcessService _assetItemProcessService;
        //private readonly IInventoryReceiveService _inventoryReceiveService;

        public IssueRegisterController(
              IInventoryReceiveService inventoryReceiveService
            , IInventoryIssueService inventoryIssueService
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
            _inventoryIssueService = inventoryIssueService;


        }

        #endregion -- Constructor

        #region Pages

 
        public ActionResult Aplos()
        {
            return View();
        }
  
        public ActionResult IssueReturnRegister()
        {
            return View();
        }


        [Authorize,HttpPost]
        public JsonResult GetIssueRegister(string fromDate, string toDate, string Type)
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
            //return Json(_inventoryIssueService.GetIssueRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);

            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(_inventoryIssueService.GetIssueRegister(fromDate, toDate, Type));
            var jsondata = Json(new { NewData, Message = AplosMessage.Success });
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

		[Authorize,HttpPost]
		public JsonResult GetIssueRegisterBYGRN(string fromDate, string toDate, string Type) 
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
			//return Json(_inventoryIssueService.GetIssueRegisterBYGRN(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(_inventoryIssueService.GetIssueRegisterBYGRN(fromDate, toDate, Type));
            var jsondata = Json(new { NewData, Message = AplosMessage.Success });
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

		#endregion Pages





		[Authorize, HttpGet]
		public JsonResult GetIssueRegisterDetail(string Id)
        {
          
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryIssueService.GetIssueRegisterDetail(Id), JsonRequestBehavior.AllowGet);
        }


        #region Issue Register Report

        [HttpGet, Authorize]
        public ActionResult Report(Library.Model.Enums.ReportFormat reportFormat, string plantId, string fromDate, string toDate,string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Stores Issue Register" + fromDate + "To" + fromDate + "";
            var workbook = _inventoryIssueService.CreateIssueRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);
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


        #region GRN Issue Register Report

        [HttpGet, Authorize]
        public ActionResult CreateIssueRegisterGRNIssueReport(Library.Model.Enums.ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "GRN Wise Stores Issue Register" + fromDate + "To" + fromDate + "";
            var workbook = _inventoryIssueService.CreateIssueRegisterGRNIssueReport(identity.CompanyId, plantId, fromDate, toDate, Type);
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


        #region Issue Return Register Report
        
        [HttpGet, Authorize]
        public ActionResult IssueReturnRegisterReport(Library.Model.Enums.ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Stores Issue Return Register" + fromDate + "To" + fromDate + "";
            var workbook = _inventoryIssueService.CreateIssueReturnRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);
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


        [Authorize,HttpPost]
        public JsonResult GetIssueReturnRegister(string fromDate, string toDate, string Type)
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
            return Json(_inventoryIssueService.GetIssueReturnRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
        }

        #endregion

    }


}