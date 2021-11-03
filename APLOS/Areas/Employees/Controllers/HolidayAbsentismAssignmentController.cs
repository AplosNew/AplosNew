using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class HolidayAbsentismAssignmentController : BaseController
    {
        #region Constructor

        private readonly IHolidayAbsentismAssignmentService _holidayAbsentismAssignmentService;

        public HolidayAbsentismAssignmentController(
            IHolidayAbsentismAssignmentService holidayAbsentismAssignmentService
            )
        {
            _holidayAbsentismAssignmentService = holidayAbsentismAssignmentService;
        }

        #endregion Constructor
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        
        [HttpGet, Authorize]
        public ActionResult GetAssignedList(GridParameter parameters, string workDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_holidayAbsentismAssignmentService.GetAssignedList(parameters, identity.PlantId, workDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_holidayAbsentismAssignmentService.Query(parameters, identity.PlantId, fromDate,toDate), JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet, Authorize]
        public JsonResult GetEmployeeList(string workDate,string day)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_holidayAbsentismAssignmentService.GetEmployeeData(workDate,day,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHolidayCbo(string yearId, string month)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_holidayAbsentismAssignmentService.GetHolidayCbo(yearId,month,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHolidayData(string yearId, string month)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_holidayAbsentismAssignmentService.GetHolidayData(yearId, month, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeesDetailsData(string workDate, string employeeCode)
        {
            return Json(_holidayAbsentismAssignmentService.GetEmployeesDetailsData(workDate, employeeCode), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAssignedEmployeeList(string workDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_holidayAbsentismAssignmentService.GetAssignedEmployeeList(identity.PlantId, workDate), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<HolidayAbsentismAssignment> HolidayAbsentismAssignments)
        {
            _holidayAbsentismAssignmentService.InsertUpdate(HolidayAbsentismAssignments);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _holidayAbsentismAssignmentService.DeleteMaster(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}