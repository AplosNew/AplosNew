#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Service.Employees;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class JobDescriptionItemController : BaseController
    {
        #region Constructor
        private readonly IJobDescriptionItemService _jobDescriptionItemService;
        public JobDescriptionItemController(
              IJobDescriptionItemService jobDescriptionItemService
            )
        {
            _jobDescriptionItemService = jobDescriptionItemService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_jobDescriptionItemService.GetCbo(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionItemService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionItemService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(JobDescriptionItem model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _jobDescriptionItemService.Insert(model);
            return Json(new { JobDescriptionItem = model, Sequence = _jobDescriptionItemService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(JobDescriptionItem model)
        {
            _jobDescriptionItemService.Update(model);
            return Json(new { Sequence = _jobDescriptionItemService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _jobDescriptionItemService.Delete(id);
            return Json(new { Sequence = _jobDescriptionItemService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}