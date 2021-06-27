using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialStorageController : BaseController
    {
        #region -- Constructor

        private readonly IMaterialStorageService _storageService;

        public MaterialStorageController(IMaterialStorageService storageService)
        {
            _storageService = storageService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string companyId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_storageService.Query(parameters, identity.CompanyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId)) companyId = identity.CompanyId;
            if (string.IsNullOrEmpty(plantId)) plantId = identity.PlantId;
            return Json(_storageService.GetCbo(identity.CompanyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboForOnlyMaterialTransfer(string companyId, string plantId) 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId)) companyId = identity.CompanyId;
            if (string.IsNullOrEmpty(plantId)) plantId = identity.PlantId;
            return Json(_storageService.GetCboForOnlyMaterialTransfer(identity.CompanyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet); 
        }
        

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string companyId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_storageService.GetAutoSequence(identity.CompanyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MaterialStorage entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _storageService.Insert(entity);
            return Json(new { MaterialStorage = entity, Sequence = _storageService.GetAutoSequence(entity.CompanyGroupId, entity.CompanyId, entity.PlantId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MaterialStorage entity)
        {
            _storageService.Update(entity);
            return Json(new { Sequence = _storageService.GetAutoSequence(entity.CompanyGroupId, entity.CompanyId, entity.PlantId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            var entity = _storageService.Find(id);
            _storageService.Delete(id);
            return Json(new { Sequence = _storageService.GetAutoSequence(entity.CompanyGroupId, entity.CompanyId, entity.PlantId), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}