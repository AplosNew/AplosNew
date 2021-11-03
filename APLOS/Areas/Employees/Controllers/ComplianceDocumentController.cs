#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Documents;
using Library.Service.Employees;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class ComplianceDocumentController : BaseController
    {
        #region Constructor

        private readonly IComplianceDocumentService _complianceDocumentService;
        private readonly IComplianceDocumentPositonCodeService _complianceDocumentPositonCodeService;
        private readonly IComplianceDocumentPostRecruitmentService _complianceDocumentPostRecruitmentService;
        private readonly IComplianceDocumentProofTypeAssignService _complianceDocumentProofTypeAssignService;

        public ComplianceDocumentController(
              IComplianceDocumentService complianceDocumentService,
              IComplianceDocumentPositonCodeService complianceDocumentPositonCodeService,
              IComplianceDocumentPostRecruitmentService complianceDocumentPostRecruitmentService,
              IComplianceDocumentProofTypeAssignService complianceDocumentProofTypeAssignService
            )
        {
            _complianceDocumentService = complianceDocumentService;
            _complianceDocumentPositonCodeService = complianceDocumentPositonCodeService;
            _complianceDocumentPostRecruitmentService = complianceDocumentPostRecruitmentService;
            _complianceDocumentProofTypeAssignService = complianceDocumentProofTypeAssignService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult ComplianceDocumentReportPage()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_complianceDocumentService.Query(parameters, identity.CompanyGroupId, type), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetComplianceDocumentProofTypeAssignList(GridParameter parameters, string complianceDocumentId)
        {
            return Json(_complianceDocumentProofTypeAssignService.QueryGraph(parameters, complianceDocumentId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDocumentPositionList(string complianceDocumentId)
        {
            return Json(_complianceDocumentPositonCodeService.Query(complianceDocumentId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDocumentPostRecruitmentCboList(string complianceDocumentId)
        {
            return Json(_complianceDocumentPostRecruitmentService.Query(complianceDocumentId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ComplianceDocument complianceDocument, IEnumerable<ComplianceDocumentPositonCode> complianceDocumentPositon, IEnumerable<ComplianceDocumentPostRecruitment> complianceDocumentPostRecruitment, IEnumerable<ComplianceDocumentProofTypeAssign> complianceDocumentProofTypeAssign)
        {
            _complianceDocumentService.InsertGraph(complianceDocument, complianceDocumentPositon, complianceDocumentPostRecruitment, complianceDocumentProofTypeAssign);
            return Json(new { ComplianceDocument = complianceDocument, Sequence = _complianceDocumentService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ComplianceDocument complianceDocument, IEnumerable<ComplianceDocumentPositonCode> complianceDocumentPositon, IEnumerable<ComplianceDocumentPostRecruitment> complianceDocumentPostRecruitment, IEnumerable<ComplianceDocumentProofTypeAssign> complianceDocumentProofTypeAssign)
        {
            _complianceDocumentService.UpdateGraph(complianceDocument, complianceDocumentPositon, complianceDocumentPostRecruitment, complianceDocumentProofTypeAssign);
            return Json(new { Sequence = _complianceDocumentService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _complianceDocumentService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public ActionResult DeleteDocumentPosition(string id)
        {
            _complianceDocumentPositonCodeService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        #region Report

        public ActionResult ComplianceDocumentReport(string documentLevel, string plantId)
        {
            string fileName;
            fileName = documentLevel == "Document"
                ? "Compliance Document Report " + DateTime.Now.ToString("ddMMMyyyy") + ""
                : "Compliance Document Set Report " + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _complianceDocumentService.GetComplianceDocumentReport(documentLevel, plantId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_complianceDocumentService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        #endregion Report
    }
}