#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class FabricRollMasterController : BaseController
    {
        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;

        public FabricRollMasterController(IFabricRollMasterService fabricRollMasterService)
        {
            _fabricRollMasterService = fabricRollMasterService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string paidHours)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fabricRollMasterService.Query(parameters, identity.CompanyGroupId, paidHours,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFabricIncrementValue()
        {
            return Json(_fabricRollMasterService.InsertOrUpdateGraphIncrement(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(IEnumerable<FabricRollMaster> entities)
        {
            _fabricRollMasterService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _fabricRollMasterService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpGet, Authorize]
        public JsonResult GetGRNList(GridParameter parameters)
        {
            return Json(_fabricRollMasterService.GetGRNList(parameters, BusinessProcessEnum.FabricRollManagement.ToString()), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetGRNDetailList(GridParameter parameters,string inventoryReceiveId)
        {
            return Json(_fabricRollMasterService.GetGRNDetailList(parameters, inventoryReceiveId, BusinessProcessEnum.FabricRollManagement.ToString()), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFABRollList(GridParameter parameters, string inventoryReceiveDetailId)
        {
            return Json(_fabricRollMasterService.GetFABRollList(parameters, inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBarCideList(string inventoryReceiveDetailId)
        {
            return Json(_fabricRollMasterService.GetBarCideList(inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations
    }
}