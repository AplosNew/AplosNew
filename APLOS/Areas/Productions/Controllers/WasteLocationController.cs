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
    public class WasteLocationController : BaseController
    {
        #region -- Constructor

        private readonly IMaterialStorageService _storageService;

        public WasteLocationController(IMaterialStorageService storageService)
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
        #endregion -- Operations
    }
}