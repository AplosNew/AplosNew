#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using Library.OrderManagement.Production;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.Data;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Aplos.Areas.Commercial.Controllers;
using System.Drawing;
using Library.OrderManagement.Sales;
using Library.ViewModel.Vouchers;
using Library.ViewModel.SalesManagements;
using Library.Model.SalesManagements;
using Library.Service.SalesManagements;
using Library.Model.Inventory;
using Library.Model.Enums;
using Syncfusion.ExcelToPdfConverter;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class PackingInvoiceController : BaseController
    {
        private readonly ISalesService _salesService;
        PackingData det = new PackingData();
        clsSales clsSales = new clsSales();
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PackingInvoiceController(ISalesService salesService, ISqlRepository R)
        {
            _salesService = salesService;
            _sqlRepository = R;
            det = new PackingData();
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsSales.GetPackingSalesList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesMaterialData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsSales.GetPackingSalesMaterialData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPackingSOData(string PackingId)
        {
            return Json(clsSales.GetPackingSOData(PackingId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackingData()
        {
            return Json(det.GetPackingData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSalesPackingData(string salesId)
        {
            return Json(clsSales.GetSalesPackingData(salesId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedPackingList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            DataSet dsDetail;
            DataSet dsHistory, dsItemScanData;
            if (salesMaterialVMList != null)
            {
                foreach (var item in salesMaterialVMList)
                {
                    if (item.MaterialMasterId == null)
                        throw new CustomException("Please Select Material !");
                    if (item.TransactionAmount == 0)
                        throw new CustomException("Please Input Amount !");
                    if (item.TransactionQty == 0)
                        throw new CustomException("Please Input Quantity !");
                }
            }
            if (salesServiceVMList != null)
            {
                foreach (var item in salesServiceVMList)
                {
                    if (item.ServiceMasterId == null)
                        throw new CustomException("Please Select Service !");
                    if (item.Amount == 0)
                        throw new CustomException("Please Input Service Amount !");
                }
            }
            string PackingId = "";
            if (selectedPackingList != null)
            {
                foreach (var item in selectedPackingList)
                {
                    var data = clsSales.GetQtyAmountByPackingId(item.PackingId);
                    item.Qty = Convert.ToDecimal(data["Qty"].ToString());
                    item.Amount = Convert.ToDecimal(data["Amount"].ToString());
                    item.ProductLibraryId = data["ProductLibraryId"].ToString();

                    if (PackingId == "")
                    {
                        PackingId = "'" + item.PackingId + "'";
                    }
                    else
                    {
                        PackingId += ",'" + item.PackingId + "'";
                    }
                }
            }
            GetIssueDetail(PackingId, out dsDetail);
            GetIssueHistory(PackingId, out dsHistory);
            GetItemScanChildData(PackingId, out dsItemScanData);


            _salesService.PackingInvoiceInsert(voucherVM, salesMaterialVMList, selectedPackingList, salesServiceVMList, dsDetail, dsHistory, dsItemScanData);



            return Json(new { Data = voucherVM, Message = AplosMessage.Insert + "Invoice No: " + voucherVM.Id + "" });
        }

        public void GetIssueHistory(string packingid, out DataSet dsRef)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                string sql = @"select RD.Id InventoryReceiveDetailId,RD.TransactionQty Qty,RD.MaterialTranRate,RD.TotalMaterialTranAmount TotalAmount,RD.BooksCurrencyBaseRate,RD.TotalMaterialBooksCurrencyAmount
								,PLI.PackingId,RD.MaterialTranRate
								from TRN.InventoryReceiveDetail RD
								left join(Select distinct InventoryReceiveDetailId,PackingId from dbo.ItemScanChild) ISC ON ISC.InventoryReceiveDetailId=RD.Id
								 JOIN TRN.POLotReference POR ON ISC.PackingId=POR.Id
								 JOIN TRN.PackingLineItem PLI ON POR.PackingLineItemId=PLI.PackingLineItemId
								Where PLI.PackingId IN(" + packingid + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetItemScanChildData(string packingid, out DataSet dsRef)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                string sql = @"Select * from trn.PackingLineItem  PLI
LEFT JOIN 
(							
Select SC.Id,SC.MasterId,ISNULL(sc.NetWeight,0) Qty,PackingLineItemId from trn.POLotReference po
left join dbo.ItemScanChild sc on sc.PackingId = po.Id AND sc.Booked = 1 
Where SC.Id<>''
)POLR ON POLR.PackingLineItemId=PLI.PackingLineItemId
								Where PLI.PackingId IN(" + packingid + ") AND Id<>''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetIssueDetail(string packingid, out DataSet dsRef)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                string sql = @"select RD.InventoryMaterialId,SUM(RD.TransactionQty)TransactionQty,PolicyRate=SUM(RD.TotalMaterialTranAmount)/SUM(RD.TransactionQty),PolicyAmount=SUM(RD.TotalMaterialTranAmount)
                                    ,PLI.PackingId,RD.TransactionUoMId,RD.BaseUOMId
                                    from TRN.InventoryReceiveDetail RD
                                    left join(Select distinct InventoryReceiveDetailId,PackingId from dbo.ItemScanChild) ISC ON ISC.InventoryReceiveDetailId=RD.Id
                                    LEFT JOIN TRN.POLotReference POR ON ISC.PackingId=POR.Id
                                    LEFT JOIN TRN.PackingLineItem PLI ON POR.PackingLineItemId=PLI.PackingLineItemId
								Where PLI.PackingId IN(" + packingid + ") GROUP BY RD.InventoryMaterialId,PLI.PackingId,RD.TransactionUoMId,RD.BaseUOMId";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Edit(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedPackingList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            string PackingId = "";
            DataSet dsItemScanData;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (salesMaterialVMList != null)
            {
                foreach (var item in salesMaterialVMList)
                {
                    if (item.MaterialMasterId == null)
                        throw new CustomException("Please Select Material !");
                    if (item.TransactionAmount == 0)
                        throw new CustomException("Please Input Amount !");
                    if (item.TransactionQty == 0)
                        throw new CustomException("Please Input Quantity !");
                }
            }
            if (salesServiceVMList != null)
            {
                foreach (var item in salesServiceVMList)
                {
                    if (item.ServiceMasterId == null)
                        throw new CustomException("Please Select Service !");
                    if (item.Amount == 0)
                        throw new CustomException("Please Input  Service Amount !");
                }
            }

            if (selectedPackingList != null)
            {
                foreach (var item in selectedPackingList)
                {
                    var data = clsSales.GetQtyAmountByPackingId(item.PackingId);
                    item.Qty = Convert.ToDecimal(data["Qty"].ToString());
                    item.Amount = Convert.ToDecimal(data["Amount"].ToString());
                    item.ProductLibraryId = data["ProductLibraryId"].ToString();

                    if (PackingId == "")
                    {
                        PackingId = "'" + item.PackingId + "'";
                    }
                    else
                    {
                        PackingId += ",'" + item.PackingId + "'";
                    }
                }
            }
            GetItemScanChildData(PackingId, out dsItemScanData);
            _salesService.PackingInvoiceUpdate(voucherVM, salesMaterialVMList, selectedPackingList, salesServiceVMList, dsItemScanData);
            return Json(new { Data = voucherVM, Message = AplosMessage.Updated + "Invoice No: " + voucherVM.Id + "" });
        }

        [HttpPost]
        public JsonResult Delete(string Id)
        {
            DeleteData(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string id)
        {
            string strSQL, strPSQL, strBSQL, strOSQL, strSSQL, strASQL, strPISQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strOSQL = "DELETE FROM TRN.SalesTax WHERE SalesId='" + id + "'";
                strASQL = "DELETE FROM TRN.SalesAdditionalTax WHERE SalesId='" + id + "'";
                strSSQL = "DELETE FROM TRN.SalesService WHERE SalesId='" + id + "'";
                strPSQL = "DELETE FROM dbo.SalesPacking WHERE SalesId='" + id + "'";
                strBSQL = "DELETE FROM TRN.SalesMaterial WHERE SalesId='" + id + "'";
                strPISQL = "DELETE FROM [dbo].[PostSalesInvoice] WHERE SalesId='" + id + "'";
                strSQL = "DELETE FROM TRN.Sales WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strASQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPISQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost]
        public JsonResult DeleteTaxRow(string Id)
        {
            _salesService.DeleteTaxRow(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public JsonResult DeleteServiceTaxRow(string Id)
        {
            _salesService.DeleteServiceTaxRow(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteSalesMaterial(string Id)
        {
            _salesService.DeleteSalesMaterial(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteSalesService(string Id)
        {
            _salesService.DeleteSalesService(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

        private string GetAdditionalInfoPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ContractTermsAndConditions", out sID);
            return sID;
        }

        [HttpGet, Authorize]
        public ActionResult GetAdditionalInfoList(string salesId)
        {
            string sql = @"SELECT CT.*,TC.Sequence,TC.Code,TC.ShortName,TC.StandardName,TC.UserName,TC.Description  FROM [dbo].[CommercialInvoiceAdditionalInfo] CT
                            LEFT JOIN dbo.CommercialAdditionalInfo TC ON TC.Id=CT.AdditionalInfoId
                            WHERE CT.SalesId='" + salesId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateAdditionalInfo(List<Dictionary<string, object>> data, string salesId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.CommercialInvoiceAdditionalInfo where  SalesId='" + salesId + "'", out dsChild, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetAdditionalInfoPK();

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        [Authorize, HttpPost]
        public ActionResult DeleteCommercialInvoiceAdditionalInfo(string id)
        {
            DeleteCommercialInvoiceAdditionalInfoData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteCommercialInvoiceAdditionalInfoData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[CommercialInvoiceAdditionalInfo] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost, Authorize]
        public JsonResult GetProductionOrderSOList(string productionOrderId)
        {
            return Json(clsSales.GetProductionOrderSOList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

    }
}