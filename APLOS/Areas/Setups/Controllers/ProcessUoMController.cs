#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Processes;
using Library.Service.Processes;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class ProcessUoMController : BaseController
    {
        #region Constructor

        private readonly IProcessUoMService _processUoMService;
        private readonly IProcessAlternativeUoMService _processAlternativeUoMService;

        public ProcessUoMController(
            IProcessUoMService processUoMService
            , IProcessAlternativeUoMService processAlternativeUoMService
            )
        {
            _processUoMService = processUoMService;
            _processAlternativeUoMService = processAlternativeUoMService;
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

        [HttpGet, Authorize]
        public ActionResult GetUoMCboByProcess(string processId)
        {
            return Json(_processUoMService.GetUoMCboByProcess(processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCapacityUoMCboByProcess(string processId)
        {
            return Json(new SelectList(_processUoMService.GetCapacityUoMCboByProcess(processId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_processUoMService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAltUomList(string masterId)
        {
            return Json(_processAlternativeUoMService.GetAltUomList(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProcessUoM entity, IEnumerable<ProcessAlternativeUoM> alternativeUoMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _processUoMService.Insert(entity, alternativeUoMList);
            return Json(new { ProcessUoM = entity, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ProcessUoM entity, IEnumerable<ProcessAlternativeUoM> alternativeUoMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _processUoMService.Update(entity, alternativeUoMList);
            return Json(new { ProcessUoM = entity, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _processUoMService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}