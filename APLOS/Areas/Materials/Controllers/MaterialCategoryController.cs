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
    public class MaterialCategoryController : BaseController
    {
        #region -- Constructor
        private readonly IMaterialCategoryService _materialCategoryService;
        public MaterialCategoryController(IMaterialCategoryService materialCategoryService)
        {
            _materialCategoryService = materialCategoryService;
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
            return Json(_materialCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboByMaterialMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_materialCategoryService.GetCboByMaterialMaster(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialCategoryService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MaterialCategory materialCategory)
        {
            _materialCategoryService.Insert(materialCategory);
            return Json(new { MaterialCategory = materialCategory, Sequence = _materialCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MaterialCategory materialCategory)
        {
            _materialCategoryService.Update(materialCategory);
            return Json(new { Sequence = _materialCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult Delete(string id)
        {
            _materialCategoryService.Delete(id);
            return Json(new { Sequence = _materialCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}