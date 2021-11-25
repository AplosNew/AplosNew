#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Securites;
using Library.Service.Securites;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    /// <summary>
    /// If an menu exist in multiple role,but those role contain different
    /// access then 1st priority which role contain allow access.
    /// </summary>
    #region Previous One
    //public class UserRoleController : BaseController
    //{
    //    #region Constructor

    //    private readonly IUserAccessService _userAccessService;
    //    private readonly IUserAccessDetailService _userAccessDetailService;

    //    public UserRoleController(
    //        IUserAccessService userAccessService,
    //        IUserAccessDetailService userAccessDetailService)
    //    {
    //        _userAccessService = userAccessService;
    //        _userAccessDetailService = userAccessDetailService;
    //    }

    //    #endregion Constructor

    //    [Authorize]
    //    public JsonResult GetModules(string panel)
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(identity.IsSysAdmin ? _userAccessService.GetSysAdminModuleList(identity.CompanyGroupId, panel) : _userAccessService.GetMenuFrameList(identity.CompanyGroupId, identity.CompanyId, identity.UserId, identity.EmployeeId, panel), JsonRequestBehavior.AllowGet);
    //    }

    //    [Authorize]
    //    public JsonResult GetMenus(string panel)
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(identity.IsSysAdmin ? _userAccessService.GetMenuFrameList(identity.CompanyGroupId, panel) : _userAccessService.GetMenuFrameList(identity.CompanyGroupId, identity.CompanyId, identity.UserId, identity.EmployeeId, panel), JsonRequestBehavior.AllowGet);

    //    }

    //    [Authorize]
    //    public ActionResult Aplos()
    //    {
    //        return View();
    //    }

    //    public ActionResult UserRoleDataSearch(GridParameter parameters, string roleId)
    //    {
    //        return Json(_userAccessService.UserRoleDataSearch(parameters, roleId), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpGet]
    //    public JsonResult GetList(GridParameter parameters)
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(_userAccessService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
    //    }

    //    [Authorize, HttpGet]
    //    public JsonResult GetCompanyList()
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(_userAccessService.GetCompanyList(identity.CompanyGroupId, identity.UserId), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpPost]
    //    public JsonResult Create(UserAccess userRole)
    //    {
    //        _userAccessService.Insert(userRole);
    //        return Json(new { UserRole = userRole, Message = AplosMessage.Insert });
    //    }

    //    [HttpPost]
    //    public JsonResult Edit(UserAccess userRole)
    //    {
    //        if (!ModelState.IsValid) throw new CustomException(Resources.RequiredFieldMessage);
    //        _userAccessService.Update(userRole);
    //        return Json(new { Message = AplosMessage.Updated });
    //    }

    //    [HttpPost]
    //    public JsonResult Update(UserAccess userRole, string companyId)
    //    {
    //        _userAccessDetailService.UserRoleUpdate(userRole, companyId);
    //        return Json(new { Message = AplosMessage.Updated });
    //    }

    //    [HttpPost]
    //    public JsonResult Delete(UserAccess userRole)
    //    {
    //        _userAccessDetailService.UserRoleDelete(userRole);
    //        return Json(new { Message = AplosMessage.Deleted });
    //    }
    //} 
    #endregion
    #region New 
    public class UserRoleController : BaseController
    {
        #region Constructor

        //private readonly IUserAccessService _userAccessService;
        //private readonly IUserAccessDetailService _userAccessDetailService;
        private readonly Library.Security.Core.UserAccessService _userAccessService = new Library.Security.Core.UserAccessService();
        private readonly Library.Security.Core.UserAccessDetailService _userAccessDetailService = new Library.Security.Core.UserAccessDetailService();
        CustomIdentity identity = null;

        public UserRoleController()
        {
            //identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }

        #endregion Constructor

        [Authorize]
        public JsonResult GetModules(string panel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(identity.IsSysAdmin ? _userAccessService.GetSysAdminModuleList(identity.CompanyGroupId, panel) : _userAccessService.GetMenuFrameList(identity.CompanyGroupId, identity.CompanyId, identity.UserId, identity.EmployeeId, panel), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetMenus(string panel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(identity.IsSysAdmin ? _userAccessService.GetMenuFrameList(identity.CompanyGroupId, panel) : _userAccessService.GetMenuFrameList(identity.CompanyGroupId, identity.CompanyId, identity.UserId, identity.EmployeeId, panel), JsonRequestBehavior.AllowGet);

        }
   
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult UserRoleDataSearch(GridParameter parameters, string roleId)
        {
            return Json(_userAccessService.UserRoleDataSearch(parameters, roleId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_userAccessService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCompanyList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_userAccessService.GetCompanyList(identity.CompanyGroupId, identity.UserId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(UserAccess userRole)
        {
            userRole.AddedDate = DateTime.Now;
            userRole.AddedFromIP = identity.IPAddress;
            userRole.AddedBy = identity.UserId;

            _userAccessService.InsertUserRole(userRole, identity.CompanyGroupId);
            return Json(new { UserRole = userRole, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(UserAccess userRole)
        {
            userRole.UpdatedDate = DateTime.Now;
            userRole.UpdatedFromIP = identity.IPAddress;
            userRole.UpdatedBy = identity.UserId;
            if (!ModelState.IsValid) throw new CustomException(Resources.RequiredFieldMessage);
            _userAccessService.InsertUserRole(userRole, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Update(UserAccess userRole, string companyId)
        {
            _userAccessDetailService.UserRoleUpdate(userRole, companyId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(UserAccess userRole)
        {
            _userAccessDetailService.UserRoleDelete(userRole);
            return Json(new { Message = AplosMessage.Deleted });
        }
    } 
    #endregion
}