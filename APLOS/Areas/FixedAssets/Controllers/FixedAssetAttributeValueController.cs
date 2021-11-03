using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Web.Mvc;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetAttributeValueController : BaseController
    {
        private readonly IFixedAssetAttributeValueService _attributeValueService;

        public FixedAssetAttributeValueController(IFixedAssetAttributeValueService attributeValueService)
        {
            _attributeValueService = attributeValueService;
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string fixedAssetAttributeId)
        {
            return Json(_attributeValueService.Query(parameters, fixedAssetAttributeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_attributeValueService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FixedAssetAttributeValue entity)
        {
            _attributeValueService.Insert(entity);
            return Json(new { FixedAssetAttributeValue = entity, Sequence = _attributeValueService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FixedAssetAttributeValue entity)
        {
            _attributeValueService.Update(entity);
            return Json(new { Sequence = _attributeValueService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _attributeValueService.DeleteGraph(id);
                return Json(new { Sequence = _attributeValueService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}