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
    public class SOPDocumentCategoryController : BaseController
    {
        #region Constructor
        /// <summary>   The SOPDocumentCategoryService service. </summary>
        private readonly ISOPDocumentCategoryService _SOPDocumentCategoryService;
        private readonly ICompanyGroupSOPDocumentCategoryService _companyGroupSOPDocumentCategoryService;

        public SOPDocumentCategoryController(
              ISOPDocumentCategoryService SOPDocumentCategoryService
            , ICompanyGroupSOPDocumentCategoryService companyGroupSOPDocumentCategoryService
            )
        {
            _SOPDocumentCategoryService = SOPDocumentCategoryService;
            _companyGroupSOPDocumentCategoryService = companyGroupSOPDocumentCategoryService;
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
            return Json(new SelectList(_companyGroupSOPDocumentCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupSOPDocumentCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SOPDocumentCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SOPDocumentCategory SOPDocumentCategory)
        {
            _SOPDocumentCategoryService.Insert(SOPDocumentCategory);
            return Json(new { SOPDocumentCategory = SOPDocumentCategory, Sequence = _SOPDocumentCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(SOPDocumentCategory SOPDocumentCategory)
        {
            _SOPDocumentCategoryService.Update(SOPDocumentCategory);
            return Json(new { Sequence = _SOPDocumentCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _SOPDocumentCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _SOPDocumentCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}