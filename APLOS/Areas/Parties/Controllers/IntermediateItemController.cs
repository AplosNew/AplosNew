#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Parties.Controllers
{
    public class IntermediateItemController : BaseController
    {
        #region Constructor

        private readonly IIntermediateItemService _buyerDepartmentService;

        public IntermediateItemController(IIntermediateItemService buyerDepartmentService
            )
        {
            this._buyerDepartmentService = buyerDepartmentService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerDepartmentService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        #region Aplos

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Aplos

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_buyerDepartmentService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerDepartmentService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetIntermediateItemById(string id)
        {
            return Json(_buyerDepartmentService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IntermediateItem buyerDepartment)
        {
            _buyerDepartmentService.Insert(buyerDepartment);
            return Json(new { IntermediateItem = buyerDepartment, Sequence = _buyerDepartmentService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(IntermediateItem buyerDepartment)
        {
            _buyerDepartmentService.Update(buyerDepartment);
            return Json(new { Sequence = _buyerDepartmentService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _buyerDepartmentService.DeleteGraph(id);
            return Json(new { Sequence = _buyerDepartmentService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}