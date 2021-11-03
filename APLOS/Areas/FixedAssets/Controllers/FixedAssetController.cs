#region Using
using Aplos.Controllers;
using Aplos.Model.FixedAssets;
using Aplos.Properties;
using Aplos.Service.FixedAssets;
using Library.Core;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetController : BaseController
    {
        #region -- Constractor
        private readonly IFixedAssetService _fixedAssetService;
        public FixedAssetController(IFixedAssetService fixedAssetService)
        {
            _fixedAssetService = fixedAssetService;
        }
        #endregion

        #region dll
        [Authorize]
        public JsonResult GetFixedAssetList()
        {
            return Json(new SelectList(_fixedAssetService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_fixedAssetService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFixedAsset(GridParameter parameters)
        {
            return Json(_fixedAssetService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFixedAssetById(string id)
        {
            return Json(_fixedAssetService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FixedAsset FixedAsset)
        {
            _fixedAssetService.Insert(FixedAsset);
            return Json(new { FixedAsset = FixedAsset, Sequence = _fixedAssetService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FixedAsset FixedAsset)
        {
            _fixedAssetService.Update(FixedAsset);
            return Json(new { Sequence = _fixedAssetService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _fixedAssetService.Archive(id);
            return Json(new { Sequence = _fixedAssetService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}