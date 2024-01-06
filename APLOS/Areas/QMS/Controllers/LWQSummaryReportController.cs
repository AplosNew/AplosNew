using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.OrderManagement.Production;

namespace Aplos.Areas.QMS.Controllers
{
    public class LWQSummaryReportController : Controller
    {
        #region Constructor


        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;

        public LWQSummaryReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        

        [Authorize, HttpGet]
        public ActionResult GetCustomerList()
        {
            return Json(_productionSummaryData.GetSummaryCustomerList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetInvoiceList(string PartyId)
        {
            return Json(_productionSummaryData.GetSummaryInvoiceList(PartyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetPOList(string InvoiceId)
        {
            return Json(_productionSummaryData.GetSummaryInvoicePOList(InvoiceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetLotNumberLists(string POId)
        {
            return Json(_productionSummaryData.GetSummaryLotNumberLists(POId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCustomerLWQSummaryJobCardReport(string CustomerId, string InvoiceId, string ProductionOrderId, string LotNumber)
        {
            try
            {
                NewJobCardReportService app = new NewJobCardReportService();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = app.GetCustomerLWQSummaryJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, CustomerId, InvoiceId, ProductionOrderId, LotNumber);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Quality Test Report";
                return RenderReportAsExcel(workbook, reportFileName);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        private ActionResult RenderReportAsExcel(IWorkbook workbook, string fileName)
        {
            workbook.SaveAs(fileName + ".xls", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        #endregion -- Operations
    }
}