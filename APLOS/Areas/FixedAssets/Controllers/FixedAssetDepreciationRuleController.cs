using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetDepreciationRuleController : BaseController
    {
        private readonly IFixedAssetDepreciationRuleService _fixedAssetDepreciationRuleService;
        private readonly ICompanyFixedAssetDepreciationRuleService _companyFixedAssetDepreciationRuleService;

        public FixedAssetDepreciationRuleController(
            IFixedAssetDepreciationRuleService fixedAssetDepreciationRuleService
            , ICompanyFixedAssetDepreciationRuleService companyFixedAssetDepreciationRuleService)
        {
            _fixedAssetDepreciationRuleService = fixedAssetDepreciationRuleService;
            _companyFixedAssetDepreciationRuleService = companyFixedAssetDepreciationRuleService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_fixedAssetDepreciationRuleService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_fixedAssetDepreciationRuleService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetDepreciationRule(GridParameter parameters)
        {
            return Json(_fixedAssetDepreciationRuleService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDepreciationRuleById(string id)
        {
            return Json(_fixedAssetDepreciationRuleService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FixedAssetDepreciationRule DepreciationRule)
        {
            _fixedAssetDepreciationRuleService.Insert(DepreciationRule);
            return Json(new { DepreciationRule, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FixedAssetDepreciationRule DepreciationRule)
        {
            _fixedAssetDepreciationRuleService.Update(DepreciationRule);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _fixedAssetDepreciationRuleService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}