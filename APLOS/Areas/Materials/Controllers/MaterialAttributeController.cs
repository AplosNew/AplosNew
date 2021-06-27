#region using
using Library.Core;
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialAttributeController : BaseController
    {
        #region -- Constructor
        private readonly IMaterialAttributeService _materialAttributeService;

        public MaterialAttributeController(IMaterialAttributeService materialAttributeService)
        {
            this._materialAttributeService = materialAttributeService;
        }
        #endregion

        #region --pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_materialAttributeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_materialAttributeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialAttributeData(GridParameter parameters)
        {
            return Json(_materialAttributeService.GetMaterialAttributeData(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo(string valueAssignment)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialAttributeService.GetMaterialAttributeCbo(identity.CompanyGroupId, valueAssignment), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialAttribute()
        {
            return Json(_materialAttributeService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialAttributeId(string id)
        {
            return Json(_materialAttributeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MaterialAttribute materialAttribute)
        {
            _materialAttributeService.Insert(materialAttribute);
            return Json(new { MaterialAttribute = materialAttribute, Sequence = _materialAttributeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult Edit(MaterialAttribute materialAttribute)
        {
            _materialAttributeService.Update(materialAttribute);
            return Json(new { Sequence = _materialAttributeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult Delete(string id)
        {
            _materialAttributeService.Archive(id);
            return Json(new { Sequence = _materialAttributeService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}