#region Using
using Aplos.Controllers;
using Library.Core;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Service.Machines;

using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class OperationTypeController : BaseController
    {
        #region -- Constrator
        private readonly IOperationTypeService _operationTypeService;
        public OperationTypeController(IOperationTypeService operationTypeService)
        {
            this._operationTypeService = operationTypeService;
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- OperationTypes
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_operationTypeService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_operationTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetOperationType(string id)
        {
            return Json(_operationTypeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_operationTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OperationType operationType)
        {
            
                _operationTypeService.Insert(operationType);
                return Json(new { OperationType = operationType, Sequence = _operationTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
           
        }

        [HttpPost]
        public JsonResult Edit(OperationType operationType)
        {
                _operationTypeService.Update(operationType);
                return Json(new { Sequence = _operationTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
                _operationTypeService.Delete(id);
                return Json(new { Sequence = _operationTypeService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}