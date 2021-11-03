#region Using
using Aplos.Properties;
using Library.Core;
using Library.Model.Employees;
using Library.Service.Skills;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Skills.Controllers
{
    public class SkillCategoryController : Controller
    {
        #region Constructor
        private readonly ISkillCategoryService _skillCategoryService;
        private readonly ICompanyGroupSkillCategoryService _companyGroupSkillCategoryService;
        public SkillCategoryController(
              ISkillCategoryService skillCategoryService
            , ICompanyGroupSkillCategoryService companyGroupSkillCategoryService)
        {
            _skillCategoryService = skillCategoryService;
            _companyGroupSkillCategoryService = companyGroupSkillCategoryService;
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
            return Json(new SelectList(_companyGroupSkillCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupSkillCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_skillCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SkillCategory skillCategory)
        {
            _skillCategoryService.Insert(skillCategory);
            return Json(new { SkillCategory = skillCategory, Sequence = _skillCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(SkillCategory skillCategory)
        {
            _skillCategoryService.Update(skillCategory);
            return Json(new { Sequence = _skillCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _skillCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _skillCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}