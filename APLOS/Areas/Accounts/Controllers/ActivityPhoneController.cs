#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.ManagementChartOfAccounts;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Accounts.Controllers
{
    public class ActivityPhoneController : BaseController
    {
        private readonly IActivityPhoneService _activityPhoneService;

        public ActivityPhoneController(IActivityPhoneService ActivityPhoneService)
        {
            _activityPhoneService = ActivityPhoneService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/ActivityPhone.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetActivityPhoneByEmployeeActivity(string employeeId, string activityId)
        {
            if (!string.IsNullOrEmpty(employeeId))
                return Json(_activityPhoneService.GetActivityPhoneByEmployeeActivity(employeeId, activityId), JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(identity.EmployeeId))
                employeeId = identity.EmployeeId;
            return Json(_activityPhoneService.GetActivityPhoneByEmployeeActivity(employeeId, activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_activityPhoneService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActivityPhoneById(string id)
        {
            return Json(_activityPhoneService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ActivityPhone activityPhone)
        {
            _activityPhoneService.Insert(activityPhone);
            return Json(new { ActivityPhone = activityPhone, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ActivityPhone activityPhone)
        {
            _activityPhoneService.Update(activityPhone);
            return Json(new { ActivityPhone = activityPhone, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _activityPhoneService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}