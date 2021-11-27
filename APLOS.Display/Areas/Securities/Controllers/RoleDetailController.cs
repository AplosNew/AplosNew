using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Securites;
using Library.Service.Securites;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Securities.Controllers
{
    #region Previous
    //public class RoleDetailController : BaseController
    //{
    //    private readonly IRoleDetailService _roleDetailService;

    //    public RoleDetailController(IRoleDetailService roleDetailService)
    //    {
    //        _roleDetailService = roleDetailService;
    //    }

    //    [HttpGet]
    //    public ActionResult Aplos()
    //    {
    //        return View();
    //    }

    //    [HttpGet]
    //    public ActionResult RoleDetailAction()
    //    {
    //        return View();
    //    }

    //    #region -- Operations

    //    [HttpGet]
    //    public ActionResult GetRoleDetailList(string roleId, string moduleId, string menuFrameId)
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(_roleDetailService.GetMenuAndActionList(roleId, moduleId, menuFrameId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpGet]
    //    public ActionResult GetMenuFrameListByRole(string roleId)
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(_roleDetailService.GetMenuFrameListByRole(roleId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpGet]
    //    public ActionResult GetRoleDetailListForAditionalRole(string userId, string companyId, string moduleId, string menuFrameId)
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(_roleDetailService.GetMenuAndActionList(userId, companyId, moduleId, menuFrameId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpGet, Authorize]
    //    public ActionResult GetMenuAndActionList(string roleId, string userAccessId)
    //    {
    //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //        return Json(_roleDetailService.GetMenuAndActionList(roleId, userAccessId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpGet]
    //    public ActionResult GetRoleDetail(string id)
    //    {
    //        return Json(_roleDetailService.Find(id), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpPost]
    //    public JsonResult Create(IEnumerable<RoleDetail> roleDetails)
    //    {
    //        _roleDetailService.Save(roleDetails);
    //        return Json(new { Message = AplosMessage.Success });
    //    }

    //    [HttpPost]
    //    public JsonResult Edit(RoleDetail roleDetail)
    //    {
    //        _roleDetailService.Update(roleDetail);
    //        return Json(new { Message = AplosMessage.Updated });
    //    }

    //    #endregion -- Operations
    //} 
    #endregion

    #region New

    public class RoleDetailController : BaseController
    {
        private readonly SqlRepository _sqlRepository;
        Library.Security.Core.RoleDetailService _roleDetailService;
        public RoleDetailController()
        {

            _sqlRepository = new SqlRepository();
            _roleDetailService = new Library.Security.Core.RoleDetailService();
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult RoleDetailAction()
        {
            return View();
        }

        #region -- Operations

        [HttpGet]
        public ActionResult GetRoleDetailList(string roleId, string moduleId, string menuFrameId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_roleDetailService.GetMenuAndActionList(roleId, moduleId, menuFrameId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMenuFrameListByRole(string roleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSql = @"SELECT DISTINCT RD.RoleId, MM.ModuleId, M.UserName AS ModuleName, MM.MenuFrameId, MF.UserName AS MenuFrameName
                              FROM MST.MenuMaster AS MM
                              INNER JOIN MMS.Module AS M ON MM.ModuleId = M.Id
                              INNER JOIN MMS.MenuFrame AS MF ON MM.MenuFrameId = MF.Id
                              INNER JOIN SEC.RoleDetail AS RD ON MM.Id = RD.MenuMasterId INNER JOIN MST.CompanyGroupMenuMaster AS CGMM ON MM.Id = CGMM.MenuMasterId
                              WHERE  CGMM.CompanyGroupId = '" + identity.CompanyGroupId + "' AND RD.RoleId = '" + roleId + @"' ";

            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetRoleDetailListForAditionalRole(string userId, string companyId, string moduleId, string menuFrameId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_roleDetailService.GetMenuAndActionList(userId, companyId, moduleId, menuFrameId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMenuAndActionList(string roleId, string userAccessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_roleDetailService.GetMenuAndActionList(roleId, userAccessId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetRoleDetail(string id)
        {
            return Json(_sqlRepository.GetDataCollection("select * from sec.RoleDetail where Id = '" + id + @"'"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<RoleDetail> roleDetails)
        {
            _roleDetailService.Save(roleDetails);   
            return Json(new { Message = AplosMessage.Success });
        }



        #endregion -- Operations
    }
    #endregion
}