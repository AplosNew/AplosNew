#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class AccessControllerEmployeeTagController : BaseController
    {
        #region Constructor
        private readonly IAccessControllerEmployeeTagService _accessControllerEmployeeTagService;
        public AccessControllerEmployeeTagController(
              IAccessControllerEmployeeTagService accessControllerEmployeeTagService
            )
        {
            _accessControllerEmployeeTagService = accessControllerEmployeeTagService;
        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult EmployeeDevice()
        {
            return View();
        }
         
        [HttpGet, Authorize]
        public ActionResult GetAllEmployee(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(_accessControllerEmployeeTagService.GetAllEmployee(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeRelatedDevices(string systemId)
        {
            return Json(_accessControllerEmployeeTagService.GetEmployeeRelatedDevices(systemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeDevicesList(string deviceId)
        {
            return Json(_accessControllerEmployeeTagService.GetEmployeeDevicesList(deviceId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<AccessControllerEmployeeTag> AccessControllerEmployeeTags, string empId, bool registerProximate, bool registerFP)
        {
            _accessControllerEmployeeTagService.InsertOrUpdateGraph(AccessControllerEmployeeTags, empId, registerProximate, registerFP);
            return Json(new { AccessControllerEmployeeTag = AccessControllerEmployeeTags, Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult CreateEmloyeeDevice(IEnumerable<AccessControllerEmployeeTag> AccessControllerEmployeeTags, bool registerProximate, bool registerFP,string deviceId)
        {
            _accessControllerEmployeeTagService.InsertOrUpdateEmployeeDevice(AccessControllerEmployeeTags, registerProximate, registerFP, deviceId);
            return Json(new { AccessControllerEmployeeTag = AccessControllerEmployeeTags, Message = AplosMessage.Insert });
        }

        public ActionResult Delete(string id)
        {
            _accessControllerEmployeeTagService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}