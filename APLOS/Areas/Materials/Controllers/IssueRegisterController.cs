#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Enums;
using Library.MaterialManagement.Inventory;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Aplos.MaterialManagement.MaterialQuery;
using Library.Data.Sql;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class IssueRegisterController : BaseController
    {
        #region -- Constructor
        //private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IInventoryIssueService _inventoryIssueService;
        private readonly ISqlRepository _sqlRepository;
        public IssueRegisterController(
             IInventoryIssueService inventoryIssueService
            , ISqlRepository sqlRepository  
            )
        {
            _inventoryIssueService = inventoryIssueService;
            _sqlRepository = sqlRepository;


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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(inventoryIssueQueryService.GetIssueRegister(fromDate, toDate, Type));
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(_inventoryIssueService.GetIssueRegisterBYGRN(fromDate, toDate, Type));
            var jsondata = Json(new { NewData, Message = AplosMessage.Success });
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

		#endregion Pages





		[Authorize, HttpGet]
		public JsonResult GetIssueRegisterDetail(string Id)
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetIssueRegisterDetail(Id), JsonRequestBehavior.AllowGet);
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetIssueReturnRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
        }

        #endregion

    }


}