#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Addresses;
using Library.Model.Banks;
using Library.Service.Productions;
using Library.Service.Reports;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using System.Web.Mvc;

#endregion


namespace Aplos.Areas.Productions.Controllers
{
    public class InventoryReportController : BaseController
    {
        private readonly IInventoryReportService _inventoryReportService;

        public InventoryReportController(
            InventoryReportService inventoryReportService)
        {
            _inventoryReportService = inventoryReportService;
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetInventoryReport(string materialId, string articleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Inventory Report " + DateTime.Now.ToString("ddMMMyyyy") + ".xlsx";
            var workbook = _inventoryReportService.GetInventoryReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,materialId,articleId);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

      
    }
}