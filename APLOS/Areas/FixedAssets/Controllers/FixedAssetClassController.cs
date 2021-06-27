using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetClassController : BaseController
    {
        private readonly IFixedAssetClassService _fixedAssetClassService;
        private readonly ICompanyGroupFixedAssetClassService _companyGroupFixedAssetClassService;

        public FixedAssetClassController(
              IFixedAssetClassService fixedAssetClassService
            , ICompanyGroupFixedAssetClassService companyGroupFixedAssetClassService
            )
        {
            _fixedAssetClassService = fixedAssetClassService;
            _companyGroupFixedAssetClassService = companyGroupFixedAssetClassService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetClass.cshtml");
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupFixedAssetClassService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupFixedAssetClassService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_fixedAssetClassService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FixedAssetClass fixedAssetClass)
        {
            _fixedAssetClassService.Insert(fixedAssetClass);
            return Json(new { FixedAssetClass = fixedAssetClass, Sequence = _fixedAssetClassService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(FixedAssetClass fixedAssetClass)
        {
            _fixedAssetClassService.Update(fixedAssetClass);
            return Json(new { Sequence = _fixedAssetClassService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _fixedAssetClassService.DeleteGraph(id);
            return Json(new { Sequence = _fixedAssetClassService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}