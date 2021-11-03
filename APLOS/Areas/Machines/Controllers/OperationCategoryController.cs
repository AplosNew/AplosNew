#region Using
using Library.Model.Machines;
using Aplos.Properties;
using Library.Service.Machines;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class OperationCategoryController : Controller
    {
        #region Constructor
        private readonly IOperationCategoryService _operationCategoryService;
        private readonly ICompanyGroupOperationCategoryService _companyGroupOperationCategoryService;
        public OperationCategoryController(
            IOperationCategoryService operationCategoryService
            , ICompanyGroupOperationCategoryService companyGroupOperationCategoryService
            )
        {
            _operationCategoryService = operationCategoryService;
            _companyGroupOperationCategoryService = companyGroupOperationCategoryService;
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
            return Json(new SelectList(_companyGroupOperationCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupOperationCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_operationCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OperationCategory entity)
        {
            _operationCategoryService.Insert(entity);
            return Json(new { OperationCategory = entity, Sequence = _operationCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(OperationCategory entity)
        {
            _operationCategoryService.Update(entity);
            return Json(new { Sequence = _operationCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _operationCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _operationCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}