#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Documents;
using Library.Service.Employees;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class ComplianceDocumentProofTypeController : BaseController
    {
        #region Constructor

        private readonly IComplianceDocumentProofTypeService _ComplianceDocumentProofTypeService;
        private readonly ICompanyGroupEmployeeBudgetCategoryService _companyGroupEmployeeBudgetCategoryService;

        public ComplianceDocumentProofTypeController(
              IComplianceDocumentProofTypeService ComplianceDocumentProofTypeService,
              ICompanyGroupEmployeeBudgetCategoryService companyGroupEmployeeBudgetCategoryService
            )
        {
            _ComplianceDocumentProofTypeService = ComplianceDocumentProofTypeService;
            _companyGroupEmployeeBudgetCategoryService = companyGroupEmployeeBudgetCategoryService; ;
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

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_ComplianceDocumentProofTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ComplianceDocumentProofType complianceDocumentProofType)
        {
            _ComplianceDocumentProofTypeService.Insert(complianceDocumentProofType);
            return Json(new { ComplianceDocumentProofType = complianceDocumentProofType, Sequence = _ComplianceDocumentProofTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ComplianceDocumentProofType complianceDocumentProofType)
        {
            _ComplianceDocumentProofTypeService.Update(complianceDocumentProofType);
            return Json(new { ComplianceDocumentProofType = complianceDocumentProofType, Sequence = _ComplianceDocumentProofTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _ComplianceDocumentProofTypeService.Delete(id);
            return Json(new { Sequence = _ComplianceDocumentProofTypeService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_ComplianceDocumentProofTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}