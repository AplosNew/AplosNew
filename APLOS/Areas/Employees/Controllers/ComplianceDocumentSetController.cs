#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Documents;
using Library.Service.Employees;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class ComplianceDocumentSetController : BaseController
    {
        #region Constructor

        private readonly IComplianceDocumentSetService _complianceDocumentSetService;
        private readonly IComplianceDocumentSetDetailService _complianceDocumentSetDetailService;
        private readonly IComplianceDocumentSetProofTypeAssignService _complianceDocumentSetProofTypeAssignService;

        public ComplianceDocumentSetController(
              IComplianceDocumentSetService complianceDocumentSetService,
              IComplianceDocumentSetDetailService complianceDocumentSetDetailService,
              IComplianceDocumentSetProofTypeAssignService complianceDocumentSetProofTypeAssignService
            )
        {
            _complianceDocumentSetService = complianceDocumentSetService;
            _complianceDocumentSetDetailService = complianceDocumentSetDetailService;
            _complianceDocumentSetProofTypeAssignService = complianceDocumentSetProofTypeAssignService;
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
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_complianceDocumentSetService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_complianceDocumentSetService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_complianceDocumentSetService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetComplianceDocumentList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_complianceDocumentSetService.GetComplianceDocumentList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDocumentSetDetailList(string complianceDocumentSetId)
        {
            return Json(_complianceDocumentSetDetailService.Query(complianceDocumentSetId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ComplianceDocumentSet complianceDocumentSet, IEnumerable<ComplianceDocumentSetDetail> complianceDocumentSetDetail, IEnumerable<ComplianceDocumentSetProofTypeAssign> complianceDocumentSetProofTypeAssign)
        {
            _complianceDocumentSetService.InsertGraph(complianceDocumentSet, complianceDocumentSetDetail, complianceDocumentSetProofTypeAssign);
            return Json(new { ComplianceDocumentSet = complianceDocumentSet, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ComplianceDocumentSet complianceDocumentSet, IEnumerable<ComplianceDocumentSetDetail> complianceDocumentSetDetail, IEnumerable<ComplianceDocumentSetProofTypeAssign> complianceDocumentSetProofTypeAssign)
        {
            _complianceDocumentSetService.UpdateGraph(complianceDocumentSet, complianceDocumentSetDetail, complianceDocumentSetProofTypeAssign);
            return Json(new { Sequence = _complianceDocumentSetService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult GetComplianceDocumentProofTypeAssignList(GridParameter parameters, string complianceDocumentSetId)
        {
            return Json(_complianceDocumentSetProofTypeAssignService.QueryGraph(parameters, complianceDocumentSetId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {
            _complianceDocumentSetService.DeleteGraph(id);
            return Json(new { Sequence = _complianceDocumentSetService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        public ActionResult DeleteDocumentSetDetail(string id)
        {
            _complianceDocumentSetDetailService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}