using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.ManagementChartOfAccounts;
using Library.Service.ManagementChartOfAccounts;
using Library.ViewModel.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ActivityController : BaseController
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/Activity.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_activityService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboActivityPhone()
        {
            return Json(_activityService.GetCboActivityPhoneType(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_activityService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_activityService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActivityById(string id)
        {
            return Json(_activityService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Activity activity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _activityService.Insert(activity, identity.CompanyGroupId);
            return Json(new { Activity = activity, Sequence = _activityService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Activity activity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _activityService.Update(activity, identity.CompanyGroupId);
            return Json(new { Activity = activity, Sequence = _activityService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UpdateSpecial(ActivityViewModel activityVM, string budgetMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _activityService.UpdateSpecial(activityVM, budgetMasterId, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _activityService.Delete(id);
            return Json(new { Sequence = _activityService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
} 