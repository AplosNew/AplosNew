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
using System.Linq;

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

        #region Operation
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
        public JsonResult GetSkuDetails(string OtherSku,string SOId, string Sequence)
        {
            return Json(cp.GetOtherSkuDetailList(OtherSku, SOId, Sequence), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetSeceondIterationData(string MasterId)
        {
            var _sql = "";
            var _sql1 = "";
            var _sql2 = "";
            _sql = @"SELECT distinct cv.UserName,cv.Sequence,cpf.MarkerRatio
                            FROM CutPlanFormation AS cpf
                            JOIN CutPlanChild AS cpc ON cpc.Id = cpf.CutPlanChildId
                            JOIN CutPlanMarkerDetails AS cpmd ON cpmd.Id = cpc.CutPlanMarkerDetailsId
                            JOIN hkp.CharacteristicsValue AS cv ON cv.Id = cpf.MarkerCharacteristicsValueId
                            WHERE cpmd.CutPlanMasterId='" + MasterId + @"' 
                            ORDER BY cv.Sequence";

            _sql1 = @"SELECT cpf.QtyForCalculation,cpf.CalculatedQty,(cpf.QtyForCalculation-cpf.CalculatedQty) CurrentQty,cpc.CharacteristicsValueId
                            FROM CutPlanFormation AS cpf
                            JOIN CutPlanChild AS cpc ON cpc.Id = cpf.CutPlanChildId
                            JOIN CutPlanMarkerDetails AS cpmd ON cpmd.Id = cpc.CutPlanMarkerDetailsId
                            JOIN hkp.CharacteristicsValue AS cv ON cv.Id = cpf.MarkerCharacteristicsValueId
                            JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id = cpf.MarkerCharacteristicsValueId
                            WHERE cpmd.CutPlanMasterId='" + MasterId + @"' 
                            ORDER BY CV1.Sequence,cv.Sequence";

            _sql2 = @"SELECT DISTINCT cv.UserName,cpf.QtyForCalculation,cv.Sequence,cpc.CharacteristicsValueId, null as Details
                            FROM CutPlanFormation AS cpf
                            JOIN CutPlanChild AS cpc ON cpc.Id = cpf.CutPlanChildId
                            JOIN CutPlanMarkerDetails AS cpmd ON cpmd.Id = cpc.CutPlanMarkerDetailsId
                            JOIN hkp.CharacteristicsValue AS cv ON cv.Id = cpc.CharacteristicsValueId
                            WHERE cpmd.CutPlanMasterId='" + MasterId + @"' 
                            ORDER BY cv.Sequence";

            var ColorData = _sqlRepository.GetDataCollection(_sql2);
            var SizeData = _sqlRepository.GetDataCollection(_sql1);
            for (int i = 0; i < ColorData.Count; i++)
            {
                var TempData = SizeData.Where(x=>x["CharacteristicsValueId"].ToString() == ColorData[i]["CharacteristicsValueId"].ToString() && x["QtyForCalculation"].ToString() == ColorData[i]["QtyForCalculation"].ToString()).ToList();
                ColorData[i]["Details"] = TempData;
            }

            var jsondata = Json(new { HeaderData = _sqlRepository.GetDataCollection(_sql), MaintData = ColorData }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public JsonResult Create( List<Dictionary<string, object>> CalculatedValueList, List<Dictionary<string, object>> FGCharacteristicsValueList, CutPlanMaster MasterData, CutPlanMarkerDetails CPMarkerDetails,List<Dictionary<string,object>> SkuValueList)
        {
            try
            {
                cp.Save(CalculatedValueList, FGCharacteristicsValueList,MasterData, CPMarkerDetails, SkuValueList);
                return Json(new { Error = false, data = MasterData, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion
    }
}
