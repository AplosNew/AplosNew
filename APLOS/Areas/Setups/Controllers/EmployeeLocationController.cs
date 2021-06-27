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

namespace Aplos.Areas.Setups.Controllers
{
    public class EmployeeLocationController : BaseController
    {
        #region Constructor

        private readonly IEmployeeLocationService _employeeLocationService;

        public EmployeeLocationController(
              IEmployeeLocationService employeeLocationService
            )
        {
            _employeeLocationService = employeeLocationService;
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
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_employeeLocationService.GetCbo(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeLocationService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeLocationService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmployeeLocation model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _employeeLocationService.Insert(model);
            return Json(new { EmployeeLocation = model, Sequence = _employeeLocationService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(EmployeeLocation model)
        {
            _employeeLocationService.Update(model);
            return Json(new { Sequence = _employeeLocationService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeLocationService.Delete(id);
            return Json(new { Sequence = _employeeLocationService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}