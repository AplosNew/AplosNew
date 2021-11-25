using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Securites;
using Library.Service.Securites;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Securities.Controllers
{
    public class UserEntityController : BaseController
    {
        private readonly IUserEntityService _userEntityService;

        public UserEntityController(IUserEntityService userEntityService)
        {
            _userEntityService = userEntityService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetPlantCboByUser(string userId)
        {
            return Json(_userEntityService.GetPlantCboByUser(userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(string userId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_userEntityService.Query(identity.CompanyGroupId,  userId, plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlantList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var flag = (identity.IsControlAdmin || identity.IsSysAdmin) ? true : false;

            return Json(_userEntityService.GetEntityList(identity.CompanyGroupId, identity.CompanyId, identity.UserId, identity.PlantId, flag), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<UserEntity> entities)
        {
            _userEntityService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}