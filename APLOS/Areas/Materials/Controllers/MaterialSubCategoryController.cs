using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialSubCategoryController : BaseController
    {
        #region -- Constructor
        private readonly IMaterialSubCategoryService _materialSubCategoryService;
        public MaterialSubCategoryController(IMaterialSubCategoryService materialSubCategoryService)
        {
            _materialSubCategoryService = materialSubCategoryService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialSubCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboByMaterialMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_materialSubCategoryService.GetCboByMaterialMaster(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialSubCategoryService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(MaterialSubCategory materialSubCategory)
        {
            _materialSubCategoryService.Insert(materialSubCategory);
            return Json(new { MaterialSubCategory = materialSubCategory, Sequence = _materialSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult Edit(MaterialSubCategory materialSubCategory)
        {
            _materialSubCategoryService.Update(materialSubCategory);
            return Json(new { Sequence = _materialSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult Delete(string id)
        {
            _materialSubCategoryService.Delete(id);
            return Json(new { Sequence = _materialSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}