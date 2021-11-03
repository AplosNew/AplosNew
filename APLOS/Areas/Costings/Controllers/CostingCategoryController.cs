#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Model.Costings;
using Library.Service.Costings;

#endregion

namespace Aplos.Areas.Costings.Controllers
{
    public class CostingCategoryController : BaseController
    {
        #region Constructor
        private readonly ICostingCategoryService _CostingCategoryService;

        public CostingCategoryController(ICostingCategoryService CostingCategoryService)
        {
            _CostingCategoryService = CostingCategoryService;
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_CostingCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_CostingCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_CostingCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CostingCategory entity)
        {
            _CostingCategoryService.Insert(entity);
            return Json(new { CostingCategory = entity, Sequence = _CostingCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(CostingCategory entity)
        {
            _CostingCategoryService.Update(entity);
            return Json(new { Sequence = _CostingCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var entity = _CostingCategoryService.Find(id);
            _CostingCategoryService.Delete(entity);
            return Json(new { Sequence = _CostingCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion
    }
}