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
using Library.Data.Sql;
using System.Web.Script.Serialization;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class FabricRollMasterController : BaseController
    {
        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        private SqlRepository _sqlRepository = new SqlRepository();
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




        [HttpPost, Authorize]
        public ActionResult GetGRNList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT IR.Id GRNNo,IR.Id,IR.CompanyGroupId,IR.CompanyId,IR.PlantId,P.UserName PartyName,SUM(IRD.TransactionQty) TotalDetailQty,SUM(IRD.MaterialTranAmount) TotalDetailAmount,C.Code Currency, REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-')  GRNDate,po.Id AS POID,po.PODate,C.Code FROM TRN.InventoryReceive IR
										LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
										LEFT JOIN TRN.InventoryReceiveDetail IRD ON IR.Id=IRD.InventoryReceiveId
                                        LEFT JOIN TRN.PurchaseOrder po on po.id=IRD.POId
										LEFT JOIN SCS.Currency C ON IR.CurrencyId=C.Id
										LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
                                       -- WHERE BP.BusinessProcessName='FabricRollManagement'
										GROUP BY IR.Id,IR.CompanyGroupId,IR.CompanyId,IR.PlantId,P.UserName,IR.GRNDate,C.Code,po.Id,po.PODate) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

    }
}