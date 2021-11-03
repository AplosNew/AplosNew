using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetAttributeController : BaseController
    {
        private readonly IFixedAssetAttributeService _fixedAssetAttributeService;
        private readonly ICompanyGroupFixedAssetAttributeService _groupFxedAssetAttributeService;

        public FixedAssetAttributeController(
            IFixedAssetAttributeService fixedAssetAttributeService
            , ICompanyGroupFixedAssetAttributeService groupFxedAssetAttributeService)
        {
            _fixedAssetAttributeService = fixedAssetAttributeService;
            _groupFxedAssetAttributeService = groupFxedAssetAttributeService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_fixedAssetAttributeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_groupFxedAssetAttributeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_groupFxedAssetAttributeService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FixedAssetAttribute fixedAssetAttribute)
        {
            _fixedAssetAttributeService.Insert(fixedAssetAttribute);
            return Json(new { FixedAssetAttribute = fixedAssetAttribute, Sequence = _fixedAssetAttributeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FixedAssetAttribute FixedAssetAttribute)
        {
            _fixedAssetAttributeService.Update(FixedAssetAttribute);
            return Json(new { Sequence = _fixedAssetAttributeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _fixedAssetAttributeService.DeleteGraph(id);
            return Json(new { Sequence = _fixedAssetAttributeService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}