using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.WorkCenters;
using Library.Service.WorkCenters;
using System.Web.Mvc;

namespace Aplos.Areas.WorkCenters.Controllers
{
    public class WorkCenterCategoryController : BaseController
    {
        #region Constructor

        private readonly IWorkCenterCategoryService _workCenterCategoryService;

        public WorkCenterCategoryController(IWorkCenterCategoryService workCenterCategoryService)
        {
            _workCenterCategoryService = workCenterCategoryService;
        }

        #endregion Constructor

        public JsonResult GetWorkCenterCategoryList()
        {
            return Json(new SelectList(_workCenterCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_workCenterCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_workCenterCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetworkCenterCategoryById(string id)
        {
            return Json(_workCenterCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(WorkCenterCategory workCenterCategory)
        {
            _workCenterCategoryService.Insert(workCenterCategory);
            return Json(new { WorkCenterCategory = workCenterCategory, Sequence = _workCenterCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(WorkCenterCategory workCenterCategory)
        {
            _workCenterCategoryService.Update(workCenterCategory);
            return Json(new { Sequence = _workCenterCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _workCenterCategoryService.Archive(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}