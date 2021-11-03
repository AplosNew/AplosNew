#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class SOPDocumentSubCategoryController : BaseController
    {
        #region Constructor
        /// <summary>   The SOPDocumentSubCategoryService service. </summary>
        private readonly ISOPDocumentSubCategoryService _SOPDocumentSubCategoryService;
        private readonly ICompanyGroupSOPDocumentSubCategoryService _companyGroupSOPDocumentSubCategoryService;

        public SOPDocumentSubCategoryController(
              ISOPDocumentSubCategoryService SOPDocumentSubCategoryService
            , ICompanyGroupSOPDocumentSubCategoryService companyGroupSOPDocumentSubCategoryService
            )
        {
            _SOPDocumentSubCategoryService = SOPDocumentSubCategoryService;
            _companyGroupSOPDocumentSubCategoryService = companyGroupSOPDocumentSubCategoryService;
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
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupSOPDocumentSubCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupSOPDocumentSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SOPDocumentSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SOPDocumentSubCategory SOPDocumentSubCategory)
        {
            _SOPDocumentSubCategoryService.Insert(SOPDocumentSubCategory);
            return Json(new { SOPDocumentSubCategory, Sequence = _SOPDocumentSubCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(SOPDocumentSubCategory SOPDocumentSubCategory)
        {
            _SOPDocumentSubCategoryService.Update(SOPDocumentSubCategory);
            return Json(new { Sequence = _SOPDocumentSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _SOPDocumentSubCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _SOPDocumentSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}