using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetSubClassController : BaseController
    {
        private readonly IFixedAssetSubClassService _fixedAssetSubClassService;
        private readonly ICompanyGroupFixedAssetSubClassService _companyGroupFixedAssetSubClassService;

        public FixedAssetSubClassController(
              IFixedAssetSubClassService fixedAssetSubClassService
            , ICompanyGroupFixedAssetSubClassService companyGroupFixedAssetSubClassService
            )
        {
            _fixedAssetSubClassService = fixedAssetSubClassService;
            _companyGroupFixedAssetSubClassService = companyGroupFixedAssetSubClassService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetSubClass.cshtml");
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupFixedAssetSubClassService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupFixedAssetSubClassService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_fixedAssetSubClassService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FixedAssetSubClass fixedAssetSubClass)
        {
            _fixedAssetSubClassService.Insert(fixedAssetSubClass);
            return Json(new { FixedAssetSubClass = fixedAssetSubClass, Sequence = _fixedAssetSubClassService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(FixedAssetSubClass fixedAssetSubClass)
        {
            _fixedAssetSubClassService.Update(fixedAssetSubClass);
            return Json(new { Sequence = _fixedAssetSubClassService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _fixedAssetSubClassService.DeleteGraph(id);
            return Json(new { Sequence = _fixedAssetSubClassService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}