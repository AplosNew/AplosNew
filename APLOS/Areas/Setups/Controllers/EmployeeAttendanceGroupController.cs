#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion using

namespace Aplos.Areas.Setups.Controllers
{
    public class EmployeeAttendanceGroupController : BaseController
    {
        #region -- Constructor

        private readonly IEmployeeAttendanceGroupService _employeeAttendanceGroupService;

        public EmployeeAttendanceGroupController(IEmployeeAttendanceGroupService employeeAttendanceGroupService)
        {
            _employeeAttendanceGroupService = employeeAttendanceGroupService;
        }

        #endregion -- Constructor

        #region Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string attendanceGroupId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeAttendanceGroupService.Query(parameters, identity.CompanyGroupId, attendanceGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetListWithEmployee(GridParameter parameters, string employeeId, string attendanceGroupIds)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeAttendanceGroupService.QueryWithEmployee(parameters, identity.CompanyGroupId, employeeId, new JavaScriptSerializer().Deserialize<string[]>(attendanceGroupIds)), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetListWithUser(GridParameter parameters, string userId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeAttendanceGroupService.QueryWithUser(parameters, identity.CompanyGroupId, userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult AttendanceGroupQuery(string attendanceGroupId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeAttendanceGroupService.AttendanceGroupQuery(identity.CompanyGroupId, attendanceGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<EmployeeAttendanceGroup> entities)
        {
            _employeeAttendanceGroupService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _employeeAttendanceGroupService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}