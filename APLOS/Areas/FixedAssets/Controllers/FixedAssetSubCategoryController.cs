using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetSubCategoryController : BaseController
    {
        private readonly IFixedAssetSubCategoryService _fixedAssetSubCategoryService;

        public FixedAssetSubCategoryController(IFixedAssetSubCategoryService fixedAssetSubCategoryService)
        {
            _fixedAssetSubCategoryService = fixedAssetSubCategoryService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetSubCategory.cshtml");
        }

        [Authorize]
        public JsonResult GetFixedAssetSubCategoryList()
        {
            return Json(new SelectList(_fixedAssetSubCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_fixedAssetSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFixedAssetSubCategory(GridParameter parameters)
        {
            return Json(_fixedAssetSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFixedAssetSubCategoryById(string id)
        {
            return Json(_fixedAssetSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FixedAssetSubCategory FixedAssetSubCategory)
        {
            _fixedAssetSubCategoryService.Insert(FixedAssetSubCategory);
            return Json(new { FixedAssetSubCategory, Sequence = _fixedAssetSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FixedAssetSubCategory FixedAssetSubCategory)
        {
            _fixedAssetSubCategoryService.Update(FixedAssetSubCategory);
            return Json(new { Sequence = _fixedAssetSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _fixedAssetSubCategoryService.Delete(id);
            return Json(new { Sequence = _fixedAssetSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}