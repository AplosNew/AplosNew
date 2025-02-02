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
    public class BOQGenerationController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public BOQGenerationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        


        [HttpPost, Authorize]
        public ActionResult GetCustomerList(string column, string value)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQ().GetCustomerList(column, value);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetEditList(string column, string value)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQ().GetEditList(column, value);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult SalesOrderListForExistingProcess(string SalesOrderIds, string OrderProcurementCostingDirectMaterialId)
        {
            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(new Library.OrderManagement.Production.ProductionOrder().SalesOrderListForExistingProcess(SalesOrderIds, OrderProcurementCostingDirectMaterialId));
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetExistingSalesOrderList(string BOMMasterId)
        {
            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(new Library.OrderManagement.Production.ProductionOrder().GetExistingSalesOrderList(BOMMasterId));
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, Authorize]
        public ActionResult GetSalesOrderList(string column, string value, string PartyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                                   " + new Library.OrderManagement.Production.ProductionOrder().SalesOrderListForCostingBOQ(PartyId) + @"
                            ) AS TEMP WHERE " + strkey + @" ORDER BY OrderCostingMasterTemplateId DESC";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);


            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetItemList(List<Dictionary<string, object>> SelectedSalesOrders, string SalesOrderId,string CostingBOQMasterId)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQ().GetAllCostingDirectMaterial(SelectedSalesOrders, SalesOrderId, CostingBOQMasterId);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ReportXls(string CostingBOQMasterId)
        {
            try
            {
                new Library.OrderManagement.Costing.CostingBOQ().ReportXls(CostingBOQMasterId);
                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetNonProcessReportXls(string CostingBOQMasterId)
        {
            try
            {
                new Library.OrderManagement.Costing.CostingBOQ().GetNonProcessReportXls(CostingBOQMasterId);
                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpPost]
        public ActionResult Save(Dictionary<string, object> MasterData, List<Dictionary<string, object>> SalesOrderData, List<Dictionary<string, object>> ItemData)
        {
            try
            {
                Dictionary<string, object> data = new Library.OrderManagement.Costing.CostingBOQ().Save(MasterData, SalesOrderData, ItemData);
                return Json(new { DATA = data, Error = false, Message = "BOM Generated Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost]
        public ActionResult Delete(string Id)
        {
            try
            {
                new Library.OrderManagement.Costing.CostingBOQ().Delete(Id);
                return Json(new { Error = false, Message = "Data deleted successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult UnProcessedRowItem(string costingBOQMasterId,string costingItemId)
        {
            try
            {
                new Library.OrderManagement.Costing.CostingBOQ().DeleteProcessedRowItem(costingBOQMasterId, costingItemId);
                return Json(new { Error = false, Message = "Data Unporcessed successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }


    }
}