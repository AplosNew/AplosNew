#region Using
using Aplos.Controllers;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessGroupController : BaseController
    {
        #region Constructor
        private readonly IProcessGroupService _processGroupService;

        public ProcessGroupController(IProcessGroupService processGroupService)
        {
            _processGroupService = processGroupService;
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
            return Json(new SelectList(_processGroupService.GetCbo(idntity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_processGroupService.Query(parameters, idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_processGroupService.GetAutoSequence(idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProcessGroup entity)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = idntity.CompanyGroupId;
            _processGroupService.Insert(entity);
            return Json(new { ProcessGroup = entity, Sequence = _processGroupService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ProcessGroup entity)
        {
            _processGroupService.Update(entity);
            return Json(new { Sequence = _processGroupService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var entity = _processGroupService.Find(id);
            _processGroupService.Delete(entity);
            return Json(new { Sequence = _processGroupService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Deleted });
        }
    
        #endregion
    }
}