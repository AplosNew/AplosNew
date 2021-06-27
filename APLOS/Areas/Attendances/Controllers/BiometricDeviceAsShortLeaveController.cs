#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class BiometricDeviceAsShortLeaveController : BaseController
    {
        private readonly IBiometricDeviceAsShortLeaveService _biometricDeviceAsShortLeaveService;
        public BiometricDeviceAsShortLeaveController(IBiometricDeviceAsShortLeaveService biometricDeviceAsShortLeaveService)
        {
            _biometricDeviceAsShortLeaveService = biometricDeviceAsShortLeaveService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_biometricDeviceAsShortLeaveService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BiometricDeviceAsShortLeave model)
        {
            _biometricDeviceAsShortLeaveService.Insert(model);
            return Json(new { AccessControllerList = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(BiometricDeviceAsShortLeave model)
        {
            _biometricDeviceAsShortLeaveService.Update(model);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _biometricDeviceAsShortLeaveService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}