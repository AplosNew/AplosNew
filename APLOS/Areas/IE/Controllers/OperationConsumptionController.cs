#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.IE;
using Library.Model.Setups;
using Library.Service.IE;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IE.Controllers
{
    public class OperationConsumptionController : BaseController
    {
        #region Constructor

        private readonly IOperationConsumptionService _operationConsumptionService;

        public OperationConsumptionController(
              IOperationConsumptionService operationConsumptionService
            )
        {
            _operationConsumptionService = operationConsumptionService;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_operationConsumptionService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_operationConsumptionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_operationConsumptionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OperationConsumption model)
        {
            _operationConsumptionService.Insert(model);
            return Json(new { SizeGroup = model, Sequence = _operationConsumptionService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(OperationConsumption model)
        {
            _operationConsumptionService.Update(model);
            return Json(new { Sequence = _operationConsumptionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _operationConsumptionService.Delete(id);
            return Json(new { Sequence = _operationConsumptionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}