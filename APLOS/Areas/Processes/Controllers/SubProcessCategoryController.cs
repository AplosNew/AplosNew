#region Using
using Aplos.Controllers;
using Library.Core;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;

using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class SubProcessCategoryController : BaseController
    {
        #region Constructor
        private readonly ISubProcessCategoryService _subProcessCategoryService;
        private readonly ICompanyGroupSubProcessCategoryService _companyGroupSubProcessCategoryService;
        public SubProcessCategoryController(
            ISubProcessCategoryService subProcessCategoryService
            , ICompanyGroupSubProcessCategoryService companyGroupSubProcessCategoryService)
        {
            _subProcessCategoryService = subProcessCategoryService;
            _companyGroupSubProcessCategoryService = companyGroupSubProcessCategoryService;
        }
        #endregion

        #region Aplos
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region GetSearchGridData
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupSubProcessCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupSubProcessCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -- Operations

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_subProcessCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SubProcessCategory subProcessCategory)
        {
            _subProcessCategoryService.Insert(subProcessCategory);
            return Json(new { SubProcessCategory = subProcessCategory, Sequence = _subProcessCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SubProcessCategory subProcessCategory)
        {
            _subProcessCategoryService.Update(subProcessCategory);
            return Json(new { Sequence = _subProcessCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _subProcessCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _subProcessCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}