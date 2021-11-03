#region Using

using Library.Model.Machines;
using Aplos.Properties;
using Library.Service.Machines;
using Library.Core;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Machines.Controllers
{
    public class OperationActivityController : Controller
    {
        #region Constructor

        private readonly IOperationActivityService _OperationActivityService;
        private readonly ICompanyGroupOperationActivityService _companyGroupOperationActionService;

        public OperationActivityController(IOperationActivityService OperationActivityService, ICompanyGroupOperationActivityService companyGroupOperationActionService)
        {
            _OperationActivityService = OperationActivityService;
            _companyGroupOperationActionService = companyGroupOperationActionService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupOperationActionService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupOperationActionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_OperationActivityService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OperationActivity operationAction)
        {
            _OperationActivityService.Insert(operationAction);
            return Json(new { OperationAction = operationAction, Sequence = _OperationActivityService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(OperationActivity operationAction)
        {
            _OperationActivityService.Update(operationAction);
            return Json(new { Sequence = _OperationActivityService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _OperationActivityService.DeleteGraph(id);
            return Json(new { Sequence = _OperationActivityService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}