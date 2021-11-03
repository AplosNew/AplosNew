using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetCategoryController : BaseController
    {
        private readonly IFixedAssetCategoryService _fixedAssetCategoryService;

        public FixedAssetCategoryController(IFixedAssetCategoryService fixedAssetCategoryService)
        {
            _fixedAssetCategoryService = fixedAssetCategoryService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetCategory.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetFixedAssetCategoryList()
        {
            return Json(new SelectList(_fixedAssetCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_fixedAssetCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFixedAssetCategory(GridParameter parameters)
        {
            return Json(_fixedAssetCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFixedAssetCategoryById(string id)
        {
            return Json(_fixedAssetCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public JsonResult Create(FixedAssetCategory FixedAssetCategory)
        {
            _fixedAssetCategoryService.Insert(FixedAssetCategory);
            return Json(new { FixedAssetCategory, Sequence = _fixedAssetCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        [Authorize]
        public JsonResult Edit(FixedAssetCategory FixedAssetCategory)
        {
            _fixedAssetCategoryService.Update(FixedAssetCategory);
            return Json(new { Sequence = _fixedAssetCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        [Authorize]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _fixedAssetCategoryService.Delete(id);
                return Json(new { Sequence = _fixedAssetCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}