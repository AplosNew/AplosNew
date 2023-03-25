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
using Aplos.MaterialManagement.MaterialQuery;
using Library.Data.Sql;

#endregion using

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class OSissueRegisterController : BaseController
    {
        #region -- Constructor
        private readonly IInventoryIssueService _inventoryIssueService;
        private readonly ISqlRepository _sqlRepository;

        public OSissueRegisterController(
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


        //      [Authorize, HttpPost]
        //public JsonResult GetMaterialLedger(string fromDate,string toDate)
        //      {
        //          var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //          return Json(_inventoryReceiveService.GetMaterialLedger(fromDate,toDate), JsonRequestBehavior.AllowGet);
        //      }
        //[Authorize, HttpPost]
        //public JsonResult GetPurchaseRegister(string fromDate, string toDate, string Type) 
        //{
        //	if(fromDate==null || fromDate == "")
        //	{
        //		throw new CustomException("Select From Date");
        //	}
        //	else if (toDate == null || toDate == "")
        //	{
        //		throw new CustomException("Select To Date");
        //	}
        //	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //	return Json(_inventoryReceiveService.GetPurchaseRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
        //}
        // [Authorize, HttpGet]
        //public JsonResult GetOperationPositionMPBudget(string id)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_OperationPositionMPBudgetService.GetOperationPositionMPBudgetService(id), JsonRequestBehavior.AllowGet);
        //}


        [HttpPost]
        public JsonResult GetOSIssueRegister(string fromDate, string toDate, string Type)
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetOSIssueRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
        }

		[HttpPost]
		public JsonResult GetOSIssueRegisterBYGRN(string fromDate, string toDate, string Type) 
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetOSIssueRegisterBYGRN(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
		}

		#endregion Pages





		[Authorize, HttpGet]
		public JsonResult GetIssueRegisterDetail(string Id)
        {
          
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetIssueRegisterDetail(Id), JsonRequestBehavior.AllowGet);
        }


        #region Issue Register Report

        [HttpGet, Authorize]
        public ActionResult Report(Library.Model.Enums.ReportFormat reportFormat, string plantId, string fromDate, string toDate,string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "OutSource Issue Register" + fromDate + "To" + fromDate + "";
            var workbook = _inventoryIssueService.CreateOSIssueRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);
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
        public ActionResult CreateOSIssueRegisterGRNIssueReport(Library.Model.Enums.ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "GRN Wise Out Source Issue Register" + fromDate + "To" + fromDate + "";
            var workbook = _inventoryIssueService.CreateOSIssueRegisterGRNIssueReport(identity.CompanyId, plantId, fromDate, toDate, Type);
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


        [HttpPost]
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetIssueReturnRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
        }

        #endregion

    }


}