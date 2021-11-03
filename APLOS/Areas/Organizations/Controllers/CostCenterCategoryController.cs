using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class CostCenterCategoryController : BaseController
    {
        #region -- Constructor

        private readonly ICostCenterCategoryService _costCenterCategoryService;

        public CostCenterCategoryController(ICostCenterCategoryService costCenterCategoryService)
        {
            _costCenterCategoryService = costCenterCategoryService;
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

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_costCenterCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_costCenterCategoryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_costCenterCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCostCenterCategory()
        {
            return Json(_costCenterCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCostCenterCategoryById(string id)
        {
            return Json(_costCenterCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CostCenterCategory costCenterCategory)
        {
            _costCenterCategoryService.Insert(costCenterCategory);
            return Json(new { CostCenterCategory = costCenterCategory, Sequence = _costCenterCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(CostCenterCategory costCenterCategory)
        {
            _costCenterCategoryService.Update(costCenterCategory);
            return Json(new { Sequence = _costCenterCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _costCenterCategoryService.Delete(id);
                return Json(new { Sequence = _costCenterCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}