using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.MaterialManagement.Inventory;
using Library.Model.Inventory;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Products.Controllers
{
    public class POParameterChangeController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IPurchaseOrderDetailService _inventoryDetailService;
        public POParameterChangeController(IPurchaseOrderService inventoryReveiveService, ISqlRepository R, IPurchaseOrderDetailService inventoryDetailService)
        {
            _inventoryReveiveService = inventoryReveiveService;
            _sqlRepository = R;
            _inventoryReveiveService = inventoryReveiveService;
        }

        public ActionResult Aplos()
        {
            return View();
        }


        [Authorize, HttpPost]
        public JsonResult GetAllPOList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetAllPOList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetPOUsedData(string masterId)
        {
            var LC = _inventoryReveiveService.GetLCList(masterId);
            var GRN = _inventoryReveiveService.GetGRNList(masterId);
            var Acpt = _inventoryReveiveService.GetAcceptanceList(masterId);
            return Json(new { LC, GRN, Acpt }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(PurchaseOrder entity)
        {
            try
            {
                SaveData(entity);
                return Json(new { entity, Message = AplosMessage.Updated + " PO No <b>" + entity.Id + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SaveData(PurchaseOrder data)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string sql = "SELECT * FROM [TRN].[PurchaseOrder] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["PartyId"] = data.PartyId;
                    dr["DeliveryByAddress"] = data.DeliveryByAddress;
                    dr["InvoicingByAddress"] = data.InvoicingByAddress;
                    dr["DeliveryPartyPlantId"] = data.DeliveryPartyPlantId;
                    dr["InvoicingPartyPlantId"] = data.InvoicingPartyPlantId;
                    dr["PaymentTermId"] = data.PaymentTermId;
                    dr["BaseOnDueDate"] = data.BaseOnDueDate;
                    dr["BaseNoOfDays"] = data.BaseNoOfDays;
                    dr["MatureDate"] = data.MatureDate;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult DetailDelete(string receiveDetailId, string OrderSpecific)
        {
            _inventoryDetailService.DeletePOMaterial(receiveDetailId, OrderSpecific);
            return Json(new { Message = AplosMessage.Deleted });
        }

    }
}