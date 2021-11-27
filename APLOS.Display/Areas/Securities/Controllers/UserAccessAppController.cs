#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data;
using Library.Model.Securites;
using Library.Service.Securites;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    public class UserAccessAppController : BaseController
    {
        #region Constructor

        private readonly IUserAccessAppService _userAccessAppService;

        public UserAccessAppController(IUserAccessAppService userAccessAppService)
        {
            _userAccessAppService = userAccessAppService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetList(string companyId, string userId)
        {
            return Json(_userAccessAppService.GetUserAccessAppList(companyId, userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetListWithCompany(string companyId, string userId)
        {
            return Json(_userAccessAppService.GetUserAppAccessListWithCompany(companyId, userId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<UserAccessApp> userAccessApp)
        {
            _userAccessAppService.Insert(userAccessApp);
            return Json(new { UserAccessApp = userAccessApp, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(UserAccessApp userAccessApp)
        {
            if (!ModelState.IsValid) throw new CustomException(Resources.RequiredFieldMessage);
            _userAccessAppService.Update(userAccessApp);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _userAccessAppService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}