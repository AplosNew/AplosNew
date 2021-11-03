#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using Library.Model.Biometrics;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.Attendances;
using Library.Model.Attendances;


#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class AccessControllerListController : BaseController
    {

        private readonly IAccessControllerListService _accessControllerListService;

        public AccessControllerListController(IAccessControllerListService accessControllerListService)
        {
            _accessControllerListService = accessControllerListService;
        }


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accessControllerListService.Query(parameters, identity.CompanyGroupId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AccessControllerList model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _accessControllerListService.Insert(model, identity.CompanyGroupId);
            return Json(new { AccessControllerList = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(AccessControllerList model)
        {
            _accessControllerListService.Update(model);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _accessControllerListService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accessControllerListService.GetCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
    }
}