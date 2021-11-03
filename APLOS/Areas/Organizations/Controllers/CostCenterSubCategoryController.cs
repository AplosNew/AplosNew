using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class CostCenterSubCategoryController : BaseController
    {
        #region -- Constructor

        private readonly ICostCenterSubCategoryService _costCenterSubCategoryService;

        public CostCenterSubCategoryController(ICostCenterSubCategoryService costCenterSubCategoryService)
        {
            _costCenterSubCategoryService = costCenterSubCategoryService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_costCenterSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_costCenterSubCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_costCenterSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCostCenterSubCategory()
        {
            return Json(_costCenterSubCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCostCenterSubCategoryById(string id)
        {
            return Json(_costCenterSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CostCenterSubCategory costCenterSubCategory)
        {
            _costCenterSubCategoryService.Insert(costCenterSubCategory);
            return Json(new { CostCenterSubCategory = costCenterSubCategory, Sequence = _costCenterSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(CostCenterSubCategory costCenterSubCategory)
        {
            _costCenterSubCategoryService.Update(costCenterSubCategory);
            return Json(new { Sequence = _costCenterSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _costCenterSubCategoryService.Delete(id);
            return Json(new { Sequence = _costCenterSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}