using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.WorkCenters;
using Library.Service.WorkCenters;
using System.Web.Mvc;

namespace Aplos.Areas.WorkCenters.Controllers
{
    public class WorkCenterSubCategoryController : BaseController
    {
        #region Constructor

        private readonly IWorkCenterSubCategoryService _workCenterSubCategoryService;

        public WorkCenterSubCategoryController(IWorkCenterSubCategoryService workCenterSubCategoryService)
        {
            _workCenterSubCategoryService = workCenterSubCategoryService;
        }

        #endregion Constructor

        public JsonResult GetWorkCenterSubCategoryList()
        {
            return Json(new SelectList(_workCenterSubCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_workCenterSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_workCenterSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetworkCenterCategoryById(string id)
        {
            return Json(_workCenterSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(WorkCenterSubCategory workCenterSubCategory)
        {
            _workCenterSubCategoryService.Insert(workCenterSubCategory);
            return Json(new { WorkCenterSubCategory = workCenterSubCategory, Sequence = _workCenterSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(WorkCenterSubCategory workCenterSubCategory)
        {
            _workCenterSubCategoryService.Update(workCenterSubCategory);
            return Json(new { Sequence = _workCenterSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _workCenterSubCategoryService.Archive(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}