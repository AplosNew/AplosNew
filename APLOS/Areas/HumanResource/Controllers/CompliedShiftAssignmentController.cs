using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
namespace Aplos.Areas.HumanResource.Controllers
{
    public class CompliedShiftAssignmentController : BaseController
    {
        #region Constructor

        private readonly ICompliedShiftAssignmentService _compliedShiftAssignmentService;
        private readonly ICompliedEmployeeRosterService _compliedEmployeeRosterService;

        public CompliedShiftAssignmentController(
              ICompliedShiftAssignmentService compliedShiftAssignmentService
            , ICompliedEmployeeRosterService compliedEmployeeRosterService
            )
        {
            _compliedShiftAssignmentService = compliedShiftAssignmentService;
            _compliedEmployeeRosterService = compliedEmployeeRosterService;

        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }


        public ActionResult Daily()
        {
            return View();
        }

        public ActionResult ShiftRotation()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetSectionCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.GetSectionCbo(identity.CompanyGroupId, identity.IsSysAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCompliedShiftCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.GetCompliedShiftCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetActualShiftCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.GetActualShiftCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCompliedShiftGroupingCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.GetCompliedShiftGroupingCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeList(GridParameter parameters, string sectionId, string workDate, string compliedShiftGruopId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.GetAllEmployee(parameters, identity.PlantId, sectionId, workDate, compliedShiftGruopId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet,Authorize]
        public ActionResult GetList(GridParameter parameters, string workDate, string compliedShiftId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.Query(parameters, workDate, compliedShiftId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUnAssignEmployee(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.GetUnAssignEmployee(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompliedShiftAssignment> entities)
        {

            _compliedShiftAssignmentService.InsertOrUpdateGraph(entities);
            return Json(new { entities, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult SaveSingle(CompliedShiftAssignment entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.PlantId = identity.PlantId;
            _compliedShiftAssignmentService.Insert(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult SaveRosterShift(CompliedEmployeeRoster entity)
        {
            _compliedEmployeeRosterService.InsertOrUpdateRoster(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        public ActionResult Delete(string id)
        {
            _compliedShiftAssignmentService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string empId)
        {
            return Json(_compliedShiftAssignmentService.GetCbo(empId).Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCompliedRosterCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.GetCompliedRosterCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeFixedShift(string empId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_compliedShiftAssignmentService.GetEmployeeFixedShift(empId, identity.PlantId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations

        #region Report

        [HttpGet, Authorize]
        public ActionResult GetDailyComplianceReport(string workDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = DateTime.Now.ToString("yyMMdd") + " Daily Attendance.xlsx";
                var workbook = _compliedShiftAssignmentService.GetDailyComplianceReport(identity.PlantId, identity.PlantName, identity.CompanyId, workDate);
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetMonthlyShiftReport(string yearId, string monthId, string complianceShiftList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = DateTime.Now.ToString("yyMMdd") + " Monthly Attendance.xlsx";
                var workbook = _compliedShiftAssignmentService.GetMonthlyDailyShiftReport(identity.PlantId, identity.PlantName, identity.CompanyId, identity.Name, yearId, monthId, complianceShiftList);
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeJobCardReport(string fromDate, string toDate, string emp)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = DateTime.Now.ToString("yyMMdd") + " Employee Job Card.xlsx";
                var workbook = _compliedShiftAssignmentService.GetEmployeeJobCardReport(identity.PlantId, identity.PlantName, identity.CompanyId, identity.Name, fromDate, toDate, emp);
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDailyAttdnReportMonthy(string yearId, string monthId, string dayStatusReportType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = "AttendanceReport.xls";
                var workbook = _compliedShiftAssignmentService.GetMonthlyDailyAttendanceReport(identity.PlantId, identity.PlantName, identity.CompanyId, identity.Name, yearId, monthId, dayStatusReportType);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region TaskSchedularService
        [HttpPost, Authorize]
        public ActionResult CompliedshiftChange(string addedBy, string ip, string appVersion, string rotationDate, string request)
        {
            _compliedShiftAssignmentService.CompliedshiftChange(addedBy, ip, appVersion, Convert.ToDateTime(rotationDate), request);
            return Json(new { Message = AplosMessage.Success });
        }
        #endregion
    }
}