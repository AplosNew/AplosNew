#region Using
using Aplos.Controllers;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessCategoryController : BaseController
    {
        #region Constructor
        /// <summary>   The ProcessCategoryService service. </summary>
        private readonly IProcessCategoryService _processCategoryService;
        private readonly ICompanyGroupProcessCategoryService _companyGroupProcessCategoryService;

        public ProcessCategoryController(
              IProcessCategoryService processCategoryService
            , ICompanyGroupProcessCategoryService companyGroupProcessCategoryService
            )
        {
            _processCategoryService = processCategoryService;
            _companyGroupProcessCategoryService = companyGroupProcessCategoryService;
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
            return Json(new SelectList(_companyGroupProcessCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupProcessCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_processCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProcessCategory processCategory)
        {
            _processCategoryService.Insert(processCategory);
            return Json(new { ProcessCategory = processCategory, Sequence = _processCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ProcessCategory processCategory)
        {
            _processCategoryService.Update(processCategory);
            return Json(new { Sequence = _processCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _processCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _processCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}