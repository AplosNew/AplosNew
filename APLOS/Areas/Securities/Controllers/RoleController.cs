using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Securites;
using Library.Service.Securites;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Securities.Controllers
{
    #region Previous
    //public class RoleController : BaseController
    //{
    //    private readonly IRoleService _roleService;

    //    public RoleController(IRoleService roleService)
    //    {
    //        _roleService = roleService;
    //    }

    //    [HttpGet, Authorize]
    //    public JsonResult GetRoleByCompanyGroup(string companyGroupId)
    //    {
    //        return Json(new SelectList(_roleService.GetRoleByCompanyGroup(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpGet]
    //    public ActionResult Aplos()
    //    {
    //        return View();
    //    }

    //    [HttpGet, Authorize]
    //    public ActionResult GetList(GridParameter parameters)
    //    {
    //        return Json(_roleService.Query(parameters), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpGet, Authorize]
    //    public ActionResult RoleDetailsData(GridParameter parameters, string modules, string menuFrame)
    //    {
    //        return Json(_roleService.RoleDataSearch(parameters, modules, menuFrame), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpPost]
    //    public JsonResult Create(Role role)
    //    {
    //        _roleService.Insert(role);
    //        return Json(new { Role = role, Message = AplosMessage.Insert });
    //    }

    //    [HttpPost]
    //    public JsonResult Edit(Role role)
    //    {
    //        _roleService.Update(role);
    //        return Json(new { Message = AplosMessage.Updated });
    //    }

    //    [HttpPost]
    //    public ActionResult Delete(string id)
    //    {
    //        _roleService.Archive(id);
    //        return Json(new { Message = AplosMessage.Deleted });
    //    }
    //} 
    #endregion

    #region New

    public class RoleController : BaseController
    {
        private readonly Library.Security.Core.RoleService _roleService = new Library.Security.Core.RoleService();


        CustomIdentity identity = null;

        public RoleController()
        {
            //_roleService = roleService;
            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }

        [HttpGet, Authorize]
        public JsonResult GetRoleByCompanyGroup(string companyGroupId)
        {
            try
            {
                return Json(_roleService.GetRoleByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        [HttpGet, Authorize]
        public JsonResult GetRoleByCompanyGroupUpdated(string companyGroupId)
        {
            try
            {
                return Json(_roleService.GetRoleByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_roleService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult RoleDetailsData(GridParameter parameters, string modules, string menuFrame)
        {
            return Json(_roleService.RoleDataSearch(parameters, modules, menuFrame), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Role role)
        {
            role.AddedBy = identity.UserId;
            role.AddedFromIP = identity.IPAddress;
            role.AddedDate = DateTime.Now;
            role.UpdatedBy = identity.UserId;
            role.UpdatedFromIP = identity.IPAddress;
            role.UpdatedDate = DateTime.Now;
            role.CompanyGroupId = identity.CompanyGroupId;

            _roleService.Insert(role, identity.CompanyGroupId);
            return Json(new { Role = role, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Role role)
        {
            role.UpdatedBy = identity.UserId;
            role.UpdatedFromIP = identity.IPAddress;
            role.UpdatedDate = DateTime.Now;

            _roleService.Insert(role, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        //[HttpPost]
        //public ActionResult Delete(string id)
        //{
        //    _roleService.Archive(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}
    }
    #endregion
}