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
    public class PackingFormController : BaseController
    {
        #region -- Constructor
        private readonly IPackingFormService _packingFormService;

        public PackingFormController(IPackingFormService packingFormService)
        {
            _packingFormService = packingFormService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_packingFormService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(_packingFormService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_packingFormService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PackingForm packingForm)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            packingForm.CompanyGroupId = identity.CompanyGroupId;
            _packingFormService.Insert(packingForm);
            return Json(new { PackingForm = packingForm, Sequence = _packingFormService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(PackingForm packingForm)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            packingForm.CompanyGroupId = identity.CompanyGroupId;
            _packingFormService.Update(packingForm);
            return Json(new { Sequence = _packingFormService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _packingFormService.Delete(id);
            return Json(new { Sequence = _packingFormService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}