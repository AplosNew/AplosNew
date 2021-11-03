using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Projects;
using Library.Service.Projects;
using System.Web.Mvc;
using Library.Core;

namespace Aplos.Areas.Projects.Controllers
{
    public class ProjectPlanningSubCategoryController : BaseController
    {
        #region -- Constructor
        private readonly IProjectPlanningSubCategoryService _productSubCategoryService;
        public ProjectPlanningSubCategoryController(IProjectPlanningSubCategoryService productSubCategoryService)
        {
            this._productSubCategoryService = productSubCategoryService;
        }
        #endregion
        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region -- Operations
        [Authorize]
        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_productSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_productSubCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_productSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningSubCategory()
        {
            return Json(_productSubCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningSubCategoryById(string id)
        {
            return Json(_productSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Authorize]
        public JsonResult Create(ProjectPlanningSubCategory productSubCategory)
        {
            _productSubCategoryService.Insert(productSubCategory);
            return Json(new { ProjectPlanningSubCategory = productSubCategory, Sequence = _productSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }
        [HttpPost]
        [Authorize]
        public JsonResult Edit(ProjectPlanningSubCategory productSubCategory)
        {
            _productSubCategoryService.Update(productSubCategory);
            return Json(new { Sequence = _productSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }
        [HttpPost]
        [Authorize]
        public JsonResult Delete(string id)
        {
            _productSubCategoryService.Delete(id);
            return Json(new { Sequence = _productSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}