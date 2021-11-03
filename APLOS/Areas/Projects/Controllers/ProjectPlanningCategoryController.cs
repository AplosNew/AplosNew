using Library.Core;
using Library.Data;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Service.Projects;
using Library.Model.Projects;

namespace Aplos.Areas.Projects.Controllers
{
    public class ProjectPlanningCategoryController : BaseController
    {
        #region -- Constructor
        private readonly IProjectPlanningCategoryService _projectPlanningCategoryService;

        public ProjectPlanningCategoryController(IProjectPlanningCategoryService projectPlanningCategoryService)
        {
            this._projectPlanningCategoryService = projectPlanningCategoryService;
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
            return Json(_projectPlanningCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_projectPlanningCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_projectPlanningCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningCategory()
        {
            return Json(_projectPlanningCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningCategoryById(string id)
        {
            return Json(_projectPlanningCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpPost]
        public JsonResult Create(ProjectPlanningCategory projectPlanningCategory)
        {
            _projectPlanningCategoryService.Insert(projectPlanningCategory);
            return Json(new { ProjectPlanningCategory = projectPlanningCategory, Sequence = _projectPlanningCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }


        [Authorize]
        [HttpPost]
        public JsonResult Edit(ProjectPlanningCategory projectPlanningCategory)
        {
            _projectPlanningCategoryService.Update(projectPlanningCategory);
            return Json(new { Sequence = _projectPlanningCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [Authorize]
        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningCategoryService.Delete(id);
                return Json(new { Sequence = _projectPlanningCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}