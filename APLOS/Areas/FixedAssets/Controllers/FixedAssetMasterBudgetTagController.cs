using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetMasterBudgetTagController : BaseController
    {
        private readonly IFixedAssetMasterBudgetTagService _FixedAssetMasterBudgetTagService;

        public FixedAssetMasterBudgetTagController(IFixedAssetMasterBudgetTagService FixedAssetMasterBudgetTagService)
        {
            _FixedAssetMasterBudgetTagService = FixedAssetMasterBudgetTagService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return RedirectToAction("FixedAssetMasterBudgetTag", "FixedAssetMaster");
        }

        [Authorize, HttpGet]
        public JsonResult GetFixedAssetMasterBudgetTagList(GridParameter parameters, string coaId)
        {
            return Json(_FixedAssetMasterBudgetTagService.Query(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFixedAssetMasterBudgetTagById(string id)
        {
            return Json(_FixedAssetMasterBudgetTagService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<FixedAssetMasterBudgetTag> fixedAssetMasterBudgetTag)
        {
            _FixedAssetMasterBudgetTagService.InsertOrUpdateGraph(fixedAssetMasterBudgetTag);
            return Json(new { fixedAssetMasterBudgetTag, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _FixedAssetMasterBudgetTagService.Delete(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}