using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class CompanyFixedAssetDepreciationRuleController : BaseController
    {
        private readonly ICompanyFixedAssetDepreciationRuleService _companyFixedAssetDepreciationRuleService;

        public CompanyFixedAssetDepreciationRuleController(ICompanyFixedAssetDepreciationRuleService companyFixedAssetDepreciationRuleService)
        {
            _companyFixedAssetDepreciationRuleService = companyFixedAssetDepreciationRuleService;
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string companyId)
        {
            return Json(_companyFixedAssetDepreciationRuleService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetCompanyDepreciationRuleById(string id)
        {
            return Json(_companyFixedAssetDepreciationRuleService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombine(GridParameter parameters, string companyId)
        {
            return Json(_companyFixedAssetDepreciationRuleService.GetSearchWithCombine(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAll(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            return Json(_companyFixedAssetDepreciationRuleService.GetSearchWithCombineAll(parameters, companyId, FixedAssetCategoryIds, FixedAssetSubCategoryIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineAssing(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            return Json(_companyFixedAssetDepreciationRuleService.GetSearchWithCombineWithAssing(parameters, companyId, FixedAssetCategoryIds, FixedAssetSubCategoryIds), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineNotAssing(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            return Json(_companyFixedAssetDepreciationRuleService.GetSearchWithCombineWithNotAssing(parameters, companyId, FixedAssetCategoryIds, FixedAssetSubCategoryIds), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyFixedAssetDepreciationRule> CompanyDepreciationRule)
        {
            _companyFixedAssetDepreciationRuleService.InsertUpdateCDepreciation(CompanyDepreciationRule);
            return Json(new { CompanyDepreciationRule, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(CompanyFixedAssetDepreciationRule CompanyDepreciationRule)
        {
            _companyFixedAssetDepreciationRuleService.Update(CompanyDepreciationRule);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _companyFixedAssetDepreciationRuleService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}