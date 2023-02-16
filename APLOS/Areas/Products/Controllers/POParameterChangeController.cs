using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
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
            _inventoryDetailService = inventoryDetailService;
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
            DataSet dsMaster, dsInvMaster;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                
                con.OpenDataSetThroughAdapter(@"SELECT * FROM [TRN].[PurchaseOrder] WHERE Id='" + data.Id + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.InventoryReceive WHERE Id IN(SELECT InventoryReceiveId FROM TRN.InventoryReceiveDetail WHERE POId='" + data.Id + "') AND ISNULL(VoucherId,'')='' AND ISNULL(Status,'')=''", out dsInvMaster, false, "1");

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
                    if (!string.IsNullOrEmpty(data.BaseOnDueDate.ToString()))
                    {
                        dr["BaseOnDueDate"] = data.BaseOnDueDate; 
                    }
                    if (!string.IsNullOrEmpty(data.BaseNoOfDays.ToString()))
                    {
                        dr["BaseNoOfDays"] = data.BaseNoOfDays; 
                    }
                    if (!string.IsNullOrEmpty(data.MatureDate.ToString()))
                    {
                        dr["MatureDate"] = data.MatureDate; 
                    }
                    dr["DocRefNo"] = data.DocRefNo;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                if (dsInvMaster.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsInvMaster.Tables[0].Rows.Count; i++)
                    {
                        dsInvMaster.Tables[0].DefaultView.RowFilter = "Id='" + dsInvMaster.Tables[0].Rows[i]["Id"].ToString() + "'";

                        if (dsInvMaster.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = dsInvMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr["PaymentTermId"] = data.PaymentTermId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr.EndEdit(); 
                        }
                    } 
                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsInvMaster);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult UpdateDetail(PurchaseOrderDetail entity, List<Dictionary<string, object>> poTaxList)
        {
            try
            {
                UpdateDetailData(entity, poTaxList);
                return Json(new { entity, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateDetailData(PurchaseOrderDetail data, List<Dictionary<string, object>> poTaxList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsPOTax;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                string sql = "SELECT * FROM [TRN].[PurchaseOrderDetail] WHERE Id='" + data.Id + "'";
                string poTaxsql = "SELECT * FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(poTaxsql, out dsPOTax, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    if (Convert.ToDecimal(dr["GRNRcvQty"].ToString()) > 0)
                    {
                        throw new CustomException(@"GRN have been created against this PO.  PO Update is not allow !!!");
                    }
                    dr.BeginEdit();
                    
                    dr["Tolerance"] = data.Tolerance;
                    dr["TransactionRate"] = data.TransactionRate;
                    dr["TransactionAmount"] = data.TransactionAmount;
                    dr["TransactionQty"] = data.TransactionQty;
                    dr["TotalTaxAmount"] = data.TotalTaxAmount;
                    dr["BaseQty"] = data.BaseQty;
                    dr["BaseAmount"] = data.BaseAmount;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                if (poTaxList != null)
                {
                    foreach (var item in poTaxList)
                    {
                        DataView dv = new DataView(dsPOTax.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count >0)
                        {
                            DataRow dr = dv[0].Row;
                            materialCommonService.EditRowD(dr, item);
                        }
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsPOTax);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult POUpdate(PurchaseOrder data, List<Dictionary<string, object>> detaildataList, List<Dictionary<string, object>> poTaxList, bool isToleranceUpdate)
        {
            try
            {
                POParameterChangeUPdate(data, detaildataList, poTaxList, isToleranceUpdate);
                return Json(new { data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void POParameterChangeUPdate(PurchaseOrder data,List<Dictionary<string, object>> detaildataList, List<Dictionary<string, object>> poTaxList,bool isToleranceUpdate)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsDetail;
            DataSet dsPOTax;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                string sqlmaster = "SELECT * FROM [TRN].[PurchaseOrder] WHERE Id='" + data.Id + "'";
                string sqlDetail = "SELECT * FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId='" + data.Id + "'";
                string poTaxsql = "SELECT * FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlmaster, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(sqlDetail, out dsDetail, false, "1");
                objCon.OpenDataSetThroughAdapter(poTaxsql, out dsPOTax, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    
                    dr.BeginEdit();

                    dr["Tolerance"] = data.Tolerance;
                    dr["PaymentTermId"] = data.PaymentTermId;
                    dr["DocRefNo"] = data.DocRefNo;
                    dr["DocDate"] = data.DocDate;
                    dr["Tolerance"] = data.Tolerance;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                if (detaildataList != null)
                {
                    foreach (var item in detaildataList)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count > 0)
                        {
                            DataRow dr = dv[0].Row;
                            if (isToleranceUpdate)
                            {
                                dr["Tolerance"] = Convert.ToDecimal(item["Tolerance"].ToString());
                            }
                            else
                            {
                                dr["BaseQty"] = Convert.ToDecimal(item["TransactionQty"].ToString());
                                dr["WithInvoiceRate"] = Convert.ToDecimal(item["TransactionRate"].ToString());
                                dr["AfterInvoiceRate"] = Convert.ToDecimal(item["TransactionRate"].ToString());
                                if (dr["TransactionUoMId"].ToString() == dr["BaseUOMId"].ToString())
                                {
                                    dr["BaseUoMFactor"] = 1;
                                }
                                if (Convert.ToDecimal(dr["GRNRcvQty"].ToString()) > 0 && isToleranceUpdate == false)
                                {
                                    throw new CustomException(@"GRN have been created against this PO.  PO Update is not allow !!!");
                                }
                            }
                           
                            materialCommonService.EditRowD(dr, item);
                        }
                    }
                }
              

                if (poTaxList != null)
                {
                    foreach (var item in poTaxList)
                    {
                        DataView dv = new DataView(dsPOTax.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count > 0)
                        {
                            DataRow dr = dv[0].Row;
                            materialCommonService.EditRowD(dr, item);
                        }
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail, dsPOTax);
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