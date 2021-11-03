#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class JobDescriptionSubCategoryController : BaseController
    {
        #region Constructor
        private readonly IJobDescriptionSubCategoryService _jobDescriptionSubCategoryService;
        public JobDescriptionSubCategoryController(
              IJobDescriptionSubCategoryService jobDescriptionSubCategoryService
            )
        {
            _jobDescriptionSubCategoryService = jobDescriptionSubCategoryService;
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
            return Json(new SelectList(_jobDescriptionSubCategoryService.GetCbo(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionSubCategoryService.Query(parameters,identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionSubCategoryService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(JobDescriptionSubCategory model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _jobDescriptionSubCategoryService.Insert(model);
            return Json(new { JobDescriptionSubCategory= model, Sequence=_jobDescriptionSubCategoryService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(JobDescriptionSubCategory model)
        {
            _jobDescriptionSubCategoryService.Update(model);
            return Json(new { Sequence = _jobDescriptionSubCategoryService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _jobDescriptionSubCategoryService.Delete(id);
            return Json(new { Sequence = _jobDescriptionSubCategoryService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}