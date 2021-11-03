#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Threading;
using System.Web.Mvc;
using Library.Core;
using Library.Model.Setups;
using Library.Service.OrderManagements;
using Library.Model.OrderManagements;
using System.Collections.Generic;

#endregion Using

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class PreCostingController : BaseController
    {
        #region Constructor

        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly IPreCostingService _preCostingService;

        public PreCostingController(IPreCostingService preCostingService
            )
        {
            this._preCostingService = preCostingService;
        }

        #endregion Constructor

        #region Aplos

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Aplos

        #region -- Operations

        [HttpGet]
        [Authorize]
        public ActionResult getList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preCostingService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetPreCostingDetailList(string preCostingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preCostingService.GetPreCostingDetailList(preCostingId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetFinishGoodsWithCompanyGroup(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preCostingService.GetFinishGoodsWithCompanyGroup(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public JsonResult GetPreCostingById(string id)
        {
            return Json(_preCostingService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public JsonResult GetMaterialGroupAltUOMList()
        {
            return Json(_preCostingService.getUomList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public JsonResult Create(PreCosting preCosting)
        {
            _preCostingService.InsertAndUpdate(preCosting);
            return Json(new { PreCosting = preCosting, Message = AplosMessage.Insert });
        }

        [HttpPost]
        [Authorize]
        public JsonResult PreCostingDetailCreate(IEnumerable<PreCostingDetail> preCostingDetail)
        {
            _preCostingService.PreCostingDetailInsertAndUpdate(preCostingDetail);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        [Authorize]
        public JsonResult Edit(PreCosting preCosting)
        {
            _preCostingService.Update(preCosting);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        [Authorize]
        public ActionResult Delete(string id)
        {
            _preCostingService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeletePreCostingDetail(string id)
        {
            _preCostingService.DeletePreCostingDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetPreCostingCalculation(string plantId, string fgId)
        {
            return Json(_preCostingService.GetPreCostingCalculation(plantId, fgId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetPreCostingCalculationWithEntity(string plantId, string fgId)
        {
            return Json(_preCostingService.GetPreCostingCalculationWithEntity(plantId, fgId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetPlantWithWorkCenter()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preCostingService.GetPlantWithWorkCenter(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetFGNoOfWorkStation(string finishGoodId)
        {
            return Json(_preCostingService.GetFGNoOfWorkStation(finishGoodId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetMaterialGroupArticlePrdProcessGroupList(string materialGroupArticleId)
        {
            return Json(_preCostingService.GetMaterialGroupArticlePrdProcessGroupList(materialGroupArticleId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetMaterialGroupProcessCritia(string productionProcessGroupId)
        {
            return Json(_preCostingService.GetMaterialGroupProcessCritia(productionProcessGroupId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}