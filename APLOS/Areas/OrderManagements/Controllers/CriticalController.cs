#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class CriticalController : BaseController
    {
        #region Constructor
        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly ICriticalService _buyerDepartmentService;

        public CriticalController(ICriticalService buyerDepartmentService
            )
        {
            this._buyerDepartmentService = buyerDepartmentService;
        }
        #endregion

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
        #endregion

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
        public JsonResult GetCriticalById(string id)
        {
            return Json(_buyerDepartmentService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Critical buyerDepartment)
        {
            _buyerDepartmentService.Insert(buyerDepartment);
            return Json(new { Critical = buyerDepartment, Sequence = _buyerDepartmentService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Critical buyerDepartment)
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
        #endregion
    }
}