using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Documents;
using Library.Service.Employees;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class ComplianceDocumentCategoryController : BaseController
    {
        private readonly IComplianceDocumentCategoryService _complianceDocumentCategoryService;

        public ComplianceDocumentCategoryController(IComplianceDocumentCategoryService complianceDocumentCategoryService)
        {
            _complianceDocumentCategoryService = complianceDocumentCategoryService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_complianceDocumentCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_complianceDocumentCategoryService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_complianceDocumentCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ComplianceDocumentCategory complianceDocumentCategory)
        {
            _complianceDocumentCategoryService.Insert(complianceDocumentCategory);
            return Json(new { ComplianceDocumentCategory = complianceDocumentCategory, Sequence = _complianceDocumentCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ComplianceDocumentCategory complianceDocumentCategory)
        {
            _complianceDocumentCategoryService.Update(complianceDocumentCategory);
            return Json(new { Sequence = _complianceDocumentCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _complianceDocumentCategoryService.Delete(id);
            return Json(new { Sequence = _complianceDocumentCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}