#region Using
using Aplos.Controllers;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Service.Machines;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class MachineVariantController : BaseController
    {
        #region Constructor
        private readonly IMachineVariantService _machineVariantService;

        public MachineVariantController(IMachineVariantService machineVariantService)
        {
            _machineVariantService = machineVariantService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_machineVariantService.GetCbo(idntity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineVariantService.Query(parameters, idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_machineVariantService.GetAutoSequence(idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MachineVariant entity)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = idntity.CompanyGroupId;
            _machineVariantService.Insert(entity);
            return Json(new { MachineVariant = entity, Sequence = _machineVariantService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(MachineVariant entity)
        {
            _machineVariantService.Update(entity);
            return Json(new { Sequence = _machineVariantService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var entity = _machineVariantService.Find(id);
            _machineVariantService.Delete(entity);
            return Json(new { Sequence = _machineVariantService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Deleted });
        }
    
        #endregion
    }
}