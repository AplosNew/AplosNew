using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Securites;
using Library.Service.Securites;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Securities.Controllers
{
    #region Previous
    //public class RoleMappingController : BaseController
    //{
    //    private readonly IRoleMappingService _roleMappingService;

    //    public RoleMappingController(IRoleMappingService roleMappingService)
    //    {
    //        _roleMappingService = roleMappingService;
    //    }

    //    [Authorize, HttpGet]
    //    public JsonResult GetListByPosition(string positionStructureId)
    //    {
    //        return Json(_roleMappingService.GetListByPosition(positionStructureId), JsonRequestBehavior.AllowGet);
    //    }

    //    [Authorize, HttpGet]
    //    public JsonResult GetRoleListByPosition(GridParameter parameters, string roleId)
    //    {
    //        return Json(_roleMappingService.GetRoleListByPosition(parameters, new JavaScriptSerializer().Deserialize<string[]>(roleId)), JsonRequestBehavior.AllowGet);
    //    }

    //    public ActionResult RoleMappingPosition()
    //    {
    //        return View();
    //    }

    //    [HttpPost]
    //    public JsonResult CreatePositionStructure(IEnumerable<RoleMapping> roleMappingPositionStructure)
    //    {
    //        _roleMappingService.InsertRoleMappingPositionStructure(roleMappingPositionStructure);
    //        return Json(new { Message = AplosMessage.Insert });
    //    }

    //    public ActionResult RoleMappingManPowerBudget()
    //    {
    //        return View();
    //    }

    //    [Authorize, HttpGet]
    //    public JsonResult GetListByManPowerBudget(string manPowerBudgetId)
    //    {
    //        return Json(_roleMappingService.GetListByManPowerBudget(manPowerBudgetId), JsonRequestBehavior.AllowGet);
    //    }

    //    [Authorize, HttpGet]
    //    public JsonResult GetRoleListByManPowerBudget(GridParameter parameters, string roleId)
    //    {
    //        return Json(_roleMappingService.GetRoleListByManPowerBudget(parameters, new JavaScriptSerializer().Deserialize<string[]>(roleId)), JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpPost]
    //    public JsonResult CreateManPowerBudget(IEnumerable<RoleMapping> roleMappingPositionStructure)
    //    {
    //        _roleMappingService.InsertRoleMappingManPowerBudget(roleMappingPositionStructure);
    //        return Json(new { Message = AplosMessage.Insert });
    //    }
    //} 
    #endregion

    #region New
    public class RoleMappingController : BaseController
    {
        private Library.Security.Core.RoleMappingService _roleMappingService = new Library.Security.Core.RoleMappingService();
        CustomIdentity identity = null;
        public RoleMappingController()
        {
            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }

        [Authorize, HttpGet]
        public JsonResult GetListByPosition(string positionStructureId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_roleMappingService.GetListByPosition(positionStructureId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRoleListByPosition(GridParameter parameters, string roleId)
        {
            return Json(_roleMappingService.GetRoleListByPosition(parameters, new JavaScriptSerializer().Deserialize<string[]>(roleId), identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RoleMappingPosition()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreatePositionStructure(IEnumerable<RoleMapping> roleMappingPositionStructure)
        {
            foreach (var item in roleMappingPositionStructure)
            {
                item.AddedBy = identity.UserId;
                item.UpdatedBy = identity.UserId;
                item.AddedFromIP = identity.IPAddress;

            }

            _roleMappingService.InsertRoleMappingPositionStructure(roleMappingPositionStructure, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Insert });
        }

        public ActionResult RoleMappingManPowerBudget()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetListByManPowerBudget(string manPowerBudgetId)
        {
            return Json(_roleMappingService.GetListByManPowerBudget(manPowerBudgetId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetRoleListByManPowerBudget(GridParameter parameters, string roleId)
        {
            return Json(_roleMappingService.GetRoleListByManPowerBudget(parameters, new JavaScriptSerializer().Deserialize<string[]>(roleId), identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateManPowerBudget(IEnumerable<RoleMapping> roleMappingPositionStructure)
        {
            foreach (var item in roleMappingPositionStructure)
            {
                item.AddedBy = identity.UserId;
                item.UpdatedBy = identity.UserId;
                item.AddedFromIP = identity.IPAddress;
            }
            _roleMappingService.InsertRoleMappingManPowerBudget(roleMappingPositionStructure, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Insert });
        }
    }
    #endregion
}