#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Securites;
using Library.Service.Securites;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    #region Previous
    //public class UserRoleDetailController : BaseController
    //{
    //    #region Constructor

    //    private readonly IUserAccessDetailService _userAccessDetailService;

    //    public UserRoleDetailController(IUserAccessDetailService userAccessDetailService)
    //    {
    //        _userAccessDetailService = userAccessDetailService;
    //    }

    //    #endregion Constructor

    //    [Authorize]
    //    public ActionResult Aplos()
    //    {
    //        return View();
    //    }

    //    [Authorize]
    //    public ActionResult AdditionalRole()
    //    {
    //        return View();
    //    }

    //    [Authorize]
    //    public ActionResult AdditionalRoleAction()
    //    {
    //        return View();
    //    }

    //    // For role override.
    //    [HttpPost]
    //    public JsonResult Create(IEnumerable<UserAccessDetail> userRoleDetail, string roleId)
    //    {
    //        _userAccessDetailService.Insert(userRoleDetail, roleId);
    //        return Json(new { Message = AplosMessage.Insert });
    //    }

    //    #region Additional role

    //    [HttpGet]
    //    public JsonResult GetMenuFrameList(string userId, string companyId)
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(_userAccessDetailService.GetMenuFrameList(userId, companyId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpPost]
    //    public JsonResult Save(IEnumerable<UserAccessDetail> userRoleDetail)
    //    {
    //        _userAccessDetailService.Save(userRoleDetail);
    //        return Json(new { Message = AplosMessage.Insert });
    //    }

    //    [HttpPost]
    //    public JsonResult AdditionalRoleUpdate(UserAccessDetail userRoleDetail, string companyId)
    //    {
    //        _userAccessDetailService.Update(userRoleDetail, companyId);
    //        return Json(new { Message = AplosMessage.Updated });
    //    }

    //    [HttpPost]
    //    public JsonResult AdditionalRoleDelete(string moduleId, string menuFrameId, string userId, string companyId)
    //    {
    //        _userAccessDetailService.Delete(moduleId, menuFrameId, userId, companyId);
    //        return Json(new { Message = AplosMessage.Deleted });
    //    }

    //    #endregion Additional role
    //} 
    #endregion


    #region New 
    public class UserRoleDetailController : BaseController
    {
        #region Constructor

        private readonly Library.Security.Core.UserAccessDetailService _userAccessDetailService = new Library.Security.Core.UserAccessDetailService();

        CustomIdentity identity = null;

        public UserRoleDetailController(IUserAccessDetailService userAccessDetailService)
        {
            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult AdditionalRole()
        {
            return View();
        }

        [Authorize]
        public ActionResult AdditionalRoleAction()
        {
            return View();
        }

        // For role override.
        [HttpPost]
        public JsonResult Create(IEnumerable<UserAccessDetail> userRoleDetail, string roleId)
        {
            _userAccessDetailService.Insert(userRoleDetail, roleId);
            return Json(new { Message = AplosMessage.Insert });
        }

        #region Additional role

        [HttpGet]
        public JsonResult GetMenuFrameList(string userId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_userAccessDetailService.GetMenuFrameList(userId, companyId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(IEnumerable<UserAccessDetail> userRoleDetail)
        {
            _userAccessDetailService.Save(userRoleDetail);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult AdditionalRoleUpdate(UserAccessDetail userRoleDetail, string companyId)
        {
            _userAccessDetailService.Update(userRoleDetail, companyId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult AdditionalRoleDelete(string moduleId, string menuFrameId, string userId, string companyId)
        {
            _userAccessDetailService.Delete(moduleId, menuFrameId, userId, companyId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Additional role
    }
    #endregion
}