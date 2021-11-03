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
    public class JobDescriptionCategoryController : BaseController
    {
        #region Constructor
        private readonly IJobDescriptionCategoryService _jobDescriptionCategoryService;
        public JobDescriptionCategoryController(
              IJobDescriptionCategoryService jobDescriptionCategoryService
            )
        {
            _jobDescriptionCategoryService = jobDescriptionCategoryService;
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
            return Json(new SelectList(_jobDescriptionCategoryService.GetCbo(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionCategoryService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(JobDescriptionCategory model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _jobDescriptionCategoryService.Insert(model);
            return Json(new { JobDescriptionCategory = model, Sequence = _jobDescriptionCategoryService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(JobDescriptionCategory model)
        {
            _jobDescriptionCategoryService.Update(model);
            return Json(new { Sequence = _jobDescriptionCategoryService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _jobDescriptionCategoryService.Delete(id);
            return Json(new { Sequence = _jobDescriptionCategoryService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}