#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Parties.Controllers
{
    public class PartnerDeterminationProcedureController : BaseController
    {
        #region Constructor

        private readonly IPartnerDeterminationProcedureService _partnerDeterminationProcedureService;

        public PartnerDeterminationProcedureController(IPartnerDeterminationProcedureService partnerDeterminationProcedureService)
        {
            _partnerDeterminationProcedureService = partnerDeterminationProcedureService;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_partnerDeterminationProcedureService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_partnerDeterminationProcedureService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partnerDeterminationProcedureService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartnerDeterminationProcedure partnerDeterminationProcedure)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partnerDeterminationProcedure.CompanyGroupId = identity.CompanyGroupId;
            _partnerDeterminationProcedureService.Insert(partnerDeterminationProcedure);
            return Json(new { PartnerDeterminationProcedure = partnerDeterminationProcedure, Sequence = _partnerDeterminationProcedureService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartnerDeterminationProcedure partnerDeterminationProcedure)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partnerDeterminationProcedure.CompanyGroupId = identity.CompanyGroupId;
            _partnerDeterminationProcedureService.Update(partnerDeterminationProcedure);
            return Json(new { Sequence = _partnerDeterminationProcedureService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partnerDeterminationProcedureService.Archive(id);
            return Json(new { Sequence = _partnerDeterminationProcedureService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}