#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Securites;
using Library.Service.Securites;
using System;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    public class SystemAdminController : BaseController
    {
        #region Constructor

        private readonly IUserService _userService;

        public SystemAdminController(IUserService userService)
        {
            _userService = userService;
        }

        #endregion Constructor

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult ShowAllUser()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AuthTokenChange()
        {
            return View();
        }

        public ActionResult Reset()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetAllCompanyGroupWise(string comnanyGroupId)
        {
            return Json(_userService.GetAllSysAdmin(comnanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetUserList(GridParameter parameters)
        {
            return Json(_userService.GetAllSystemAdmin(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult CreateAuth()
        {
            return Json(Guid.NewGuid(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult CreatePin()
        {
            var r = new Random();
            var randomPinNo = r.Next(100000, 999999);
            return Json(randomPinNo, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(User user)
        {
            _userService.InsertSysAdmin(user);
            return Json(new { User = user, AuthToken = Guid.NewGuid(), Pin = CreatePin(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(User user)
        {
            _userService.UpdateSysAdmin(user);
            return Json(new { AuthToken = Guid.NewGuid(), Pin = CreatePin(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _userService.Archive(id);
            return Json(new { AuthToken = Guid.NewGuid(), Pin = CreatePin(), Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult AuthTokenChange(User user)
        {
            _userService.UpdateAuthToken(user);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult Get(string id)
        {
            return Json(_userService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Reset(User user)
        {
            _userService.PasswordChange(user);
            return Json(new { Message = AplosMessage.Success });
        }

        public ActionResult SyAdminAuthTokenLockDate(GridParameter parameters)
        {
            return Json(_userService.AuthTokenLockDateSyAdmin(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SysAdminAuthTokenUnLock()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SysAdminAuthTokenUnLock(string id)
        {
            _userService.AuthTokenLockUpdate(id);
            return Json(new { AuthTokenLocked = false, Message = AplosMessage.Updated });
        }

        public ActionResult SyAdminLockDate(GridParameter parameters)
        {
            return Json(_userService.UserLockDateSyAdmin(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SysUnLock()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SysUnLock(string id)
        {
            _userService.UpdateUserLockUnLock(id);
            return Json(new { UserLocked = false, Message = AplosMessage.Updated });
        }

        [HttpGet,Authorize]
        public ActionResult GetAllUserByCompanyGroupList(GridParameter parameters,string companyGroupId)
        {
            return Json(_userService.GetAllUserByCompanyGroupList(parameters,companyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}