using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class WorkGroupController : BaseController
    {
        #region Constructor

        private readonly IWorkGroupService _workGroupService;

        public WorkGroupController(
              IWorkGroupService workGroupService
            )
        {
            _workGroupService = workGroupService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [AllowAnonymous]
        public JsonResult GetCbo(string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(new SelectList(_workGroupService.GetCbo(plantId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string plantId)
        {
            return Json(_workGroupService.Query(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_workGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(WorkGroup workGroup)
        {
            _workGroupService.Insert(workGroup);
            return Json(new { WorkGroup = workGroup, Sequence = _workGroupService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(WorkGroup workGroup)
        {
            _workGroupService.Update(workGroup);
            return Json(new { Sequence = _workGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _workGroupService.Delete(id);
            return Json(new { Sequence = _workGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}