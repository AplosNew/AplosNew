using Aplos.Controllers;
using Aplos.Properties;
using ExcelDataReader;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.OrderManagements;
using Library.Service.Helpers;
using Library.Service.OrderManagements;
using Library.Service.Properties;
using Library.ViewModel.OrderManagements;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class LineEmployeeAssignController : BaseController
    {
        #region Constructor
        private readonly ILineEmployeeAssignService _lineEmployeeAssignService;

        public LineEmployeeAssignController(ILineEmployeeAssignService lineEmployeeAssignService)
        {
            _lineEmployeeAssignService = lineEmployeeAssignService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult LineEmployeeEdit()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(string date, string salesOrderName, string line,string shift)
        {
            return Json(_lineEmployeeAssignService.QueryGraph(date, salesOrderName, line,shift), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetForLineEmpAssignEdit(string date, string salesOrderName, string line, string shift)
        {
            return Json(_lineEmployeeAssignService.GetForEditPrdBooking(date, salesOrderName, line, shift), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeDetailList(string lineOperationBookingId)
        {
            return Json(_lineEmployeeAssignService.GetLineEmployeeDetail(lineOperationBookingId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(string date, string line, IEnumerable<LineEmployeeAssign> lineEmployeeAssign, IEnumerable<LineEmployeeAssign> tempLineEmployeeAssign)
        {
            _lineEmployeeAssignService.InsertOrUpdateGraph(date,line,lineEmployeeAssign,tempLineEmployeeAssign);
            return Json(new { LineEmployeeAssign = lineEmployeeAssign, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(IEnumerable<LineProductionOperationBookingViewModel> entities)
        {
            _lineEmployeeAssignService.UpdateGraph(entities);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _lineEmployeeAssignService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpGet, Authorize]
        public ActionResult GetLineCbo(string date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_lineEmployeeAssignService.GetLineCbo(date, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOperationCbo(string date, string linetext)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_lineEmployeeAssignService.GetOperationCbo(date, linetext, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalesOrderCbo(string date, string linetext,string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_lineEmployeeAssignService.GetSalesOrderCbo(date, linetext, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetShiftCbo(string date, string linetext, string salesorder, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_lineEmployeeAssignService.GetShiftCbo(date, linetext,salesorder,identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalesOrder(string date, string linename, string operationName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_lineEmployeeAssignService.GetSalesOrder(date, linename, operationName, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProduction(string date, string linename, string salesOrderName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_lineEmployeeAssignService.GetProduction(date, linename, salesOrderName, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region report
        [HttpGet, Authorize]
        public ActionResult ReportLineEmployeeAssign(string date,string line)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Employee Assign Report " + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _lineEmployeeAssignService.GetEmployeeAssignReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantName, "Employee Assign",date,line);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        [HttpGet, Authorize]
        public ActionResult ReportEmployee(string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Employee Report " + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _lineEmployeeAssignService.GetEmployeeReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantName, "Employee Date Report", fromdate, todate);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        [Authorize]
        public ActionResult LineEmployeeDateReport()
        {
            return View();
        }
        #endregion

    }
}