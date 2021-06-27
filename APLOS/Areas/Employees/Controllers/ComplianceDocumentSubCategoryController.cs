#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Documents;
using Library.Service.Employees;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class ComplianceDocumentSubCategoryController : BaseController
    {
        #region Constructor

        private readonly IComplianceDocumentSubCategoryService _SalutaionService;

        public ComplianceDocumentSubCategoryController(
              IComplianceDocumentSubCategoryService SalutaionService
            )
        {
            _SalutaionService = SalutaionService;
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
            return Json(new SelectList(_SalutaionService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_SalutaionService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SalutaionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ComplianceDocumentSubCategory complianceDocumentSubCategory)
        {
            _SalutaionService.Insert(complianceDocumentSubCategory);
            return Json(new { ComplianceDocumentSubCategory = complianceDocumentSubCategory, Sequence = _SalutaionService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ComplianceDocumentSubCategory complianceDocumentSubCategory)
        {
            _SalutaionService.Update(complianceDocumentSubCategory);
            return Json(new { Sequence = _SalutaionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _SalutaionService.Delete(id);
            return Json(new { Sequence = _SalutaionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}