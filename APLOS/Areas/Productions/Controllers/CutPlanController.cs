#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;
using Library.MaterialManagement.Material;
using System.Web;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Library.Core;
using Library.MaterialManagement.CutPlan;
using Library.Service.OrderManagements;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class CutPlanController : BaseController
    {
        #region Constructor
        private readonly IProductionOrderService _productionOrderService;
        clsCutPlan cp = new clsCutPlan();
		private readonly ISqlRepository _sqlRepository;
        public CutPlanController(ISqlRepository R, IProductionOrderService productionOrderService)
        {
            _productionOrderService = productionOrderService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Get 
        [HttpGet, Authorize]
        public JsonResult GetProductionOrderDataList(string entityId)
        {
            return Json(cp.GetProductionOrderData(entityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetLineItemData(string entityId, string processId, string productionOrderId, string masterId)
        {
            return Json(cp.GetLineItemData(entityId, processId, productionOrderId, masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionRecipeMaterialList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionRecipeMaterialList(productionOrderId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMarker(string MaterialId)
        {
            return Json(cp.getMarkerList(MaterialId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMarkerDetails(string MarkerId)
        {
            return Json(cp.GetMarkerDetailList(MarkerId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSkuDetails(string OtherSku,string MaterialMasterId)
        {
            return Json(cp.GetOtherSkuDetailList(OtherSku, MaterialMasterId), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
