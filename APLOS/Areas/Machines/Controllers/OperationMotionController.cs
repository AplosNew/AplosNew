#region using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.Machines;
using Library.Model.Machines;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class OperationMotionController : BaseController
    {
        #region Constructor
        private readonly IOperationMotionService _OperationMotionService;

        public OperationMotionController(IOperationMotionService operationMotionService)
        {
            _OperationMotionService = operationMotionService;
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
            return Json(new SelectList(_OperationMotionService.GetCbo(idntity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationMotionService.Query(parameters, idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_OperationMotionService.GetAutoSequence(idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OperationMotion entity)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = idntity.CompanyGroupId;
            if (string.IsNullOrEmpty(entity.PlantId))
                entity.PlantId = idntity.PlantId;
            _OperationMotionService.Insert(entity);
            return Json(new { entity, Sequence = _OperationMotionService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(OperationMotion entity)
        {
            _OperationMotionService.Update(entity);
            return Json(new { Sequence = _OperationMotionService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var entity = _OperationMotionService.Find(id);
            _OperationMotionService.Delete(entity);
            return Json(new { Sequence = _OperationMotionService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        #endregion
    }
}