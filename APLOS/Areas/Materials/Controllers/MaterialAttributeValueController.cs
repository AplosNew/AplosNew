#region using

using Library.Core;
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Data;
using Library.Service.Materials;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialAttributeValueController : BaseController
    {
        #region -- Constructor

        private readonly IMaterialAttributeValueService _materialValueService;

        public MaterialAttributeValueController(IMaterialAttributeValueService materialValueService)
        {
            this._materialValueService = materialValueService;
        }

        #endregion -- Constructor

        #region --pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

		#endregion --pages

		#region -- Operations

		[Authorize, HttpGet]
		public JsonResult GetCbo(string attributeId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialValueService.GetCbo(identity.CompanyGroupId, attributeId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string materialAttributeId)
        {
            return Json(_materialValueService.Query(parameters, materialAttributeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAttributeValueList(GridParameter parameters, string assignment, string materialMasterId, string attributeId)
        {
            return Json(_materialValueService.GetAttributeValueList(parameters, assignment, materialMasterId, attributeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string materialAttributeId, string materialId)
        {
            return Json(_materialValueService.GetAutoSequence(materialAttributeId, materialId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MaterialAttributeValue materialValue)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            materialValue.CompanyGroupId = identity.CompanyGroupId;
            _materialValueService.Insert(materialValue);
            return Json(new { MaterialAttributeValue = materialValue, Sequence = _materialValueService.GetAutoSequence(materialValue.MaterialAttributeId, materialValue.MaterialMasterId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MaterialAttributeValue materialValue)
        {
            _materialValueService.Update(materialValue);
            return Json(new { Sequence = _materialValueService.GetAutoSequence(materialValue.MaterialAttributeId, materialValue.MaterialMasterId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var data = _materialValueService.Find(id);
                _materialValueService.Delete(data);
                return Json(new { Sequence = _materialValueService.GetAutoSequence(data.MaterialAttributeId, data.MaterialMasterId), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}