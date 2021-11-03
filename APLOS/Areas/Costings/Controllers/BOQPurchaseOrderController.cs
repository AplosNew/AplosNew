#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class BOQPurchaseOrderController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public BOQPurchaseOrderController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetBOMList(string column, string value, Dictionary<string, DateTime> Date)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQPurchaseOrder().GetBOMList(column, value, Date);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetPartyInformationById(string VendorId)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQPurchaseOrder().GetPartyInformationById(VendorId);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetBOQItems(string CostingItemIds,string CostingBOQMasterIds, string ContractId, string VendorId, string IsOwnVendor, string inveReveiveMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.PurchaseOrderService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderService();
                return Json(obj.GetCostingBOQItems(CostingItemIds, CostingBOQMasterIds, ContractId, VendorId, IsOwnVendor, inveReveiveMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [Authorize, HttpGet]
        public JsonResult GetCostingBOQItemsListForUpdate(string VendorId, string inveReveiveId, string inveReveiveMasterId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.PurchaseOrderService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderService();
                return Json(obj.GetCostingBOQItemsListForUpdate(VendorId, inveReveiveId, inveReveiveMasterId, MaterialMasterId, ArticleId, FirstCharacteristicsValueId, SecondCharacteristicsValueId, ThirdCharacteristicsValueId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}