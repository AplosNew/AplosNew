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
    public class WeeklyAbsentismAssignmentController : BaseController
    {
        #region Constructor

        private readonly IWeeklyAbsentismAssignmentService _weeklyAbsentismAssignmentService;

        public WeeklyAbsentismAssignmentController(
            IWeeklyAbsentismAssignmentService weeklyAbsentismAssignmentService
            )
        {
            _weeklyAbsentismAssignmentService = weeklyAbsentismAssignmentService;
        }

        #endregion Constructor
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        
        [HttpGet, Authorize]
        public ActionResult GetAssignedList(GridParameter parameters, string month, string yearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_weeklyAbsentismAssignmentService.GetAssignedList(parameters, identity.PlantId, month, yearId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_weeklyAbsentismAssignmentService.Query(parameters, identity.PlantId, fromDate,toDate), JsonRequestBehavior.AllowGet);
        }

        
        [HttpGet, Authorize]
        public JsonResult GetEmployeeList(string yearId, string month, string day)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_weeklyAbsentismAssignmentService.GetEmployeeData(yearId, month, identity.PlantId, day), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetOffDayData(string yearId, string month)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_weeklyAbsentismAssignmentService.GetOffDayData(yearId, month, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEmployeesDetailsData(string workDate, string employeeCode)
        {
            return Json(_weeklyAbsentismAssignmentService.GetEmployeesDetailsData(workDate, employeeCode), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAssignedEmployeeList(string month, string yearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_weeklyAbsentismAssignmentService.GetAssignedEmployeeList(identity.PlantId, month, yearId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(IEnumerable<WeeklyAbsentismAssignment> weeklyAbsentismAssignments)
        {
            _weeklyAbsentismAssignmentService.InsertUpdate(weeklyAbsentismAssignments);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _weeklyAbsentismAssignmentService.DeleteMaster(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}