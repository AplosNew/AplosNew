#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.SalesManagements;
using Library.Model.Taxations;
using Library.OrderManagement.Packing;
using Library.OrderManagement.Sales;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.Service.SalesManagements;
using Library.ViewModel.Invoices;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.SalesManagements;
using Library.ViewModel.Vouchers;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class PIInvoiceController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly ISalesService _salesService;
        clsPIInvoice pi = new clsPIInvoice();
        clsSales clsSales = new clsSales();
        public PIInvoiceController(ISqlRepository R, ISalesService salesService)
        {
            _sqlRepository = R;
            _salesService = salesService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetPackingData()
        {
            return Json(pi.GetPackingData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedPackingList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            DataSet dsDetail;
            DataSet dsHistory;
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
            //List<Dictionary<string,object>> InventoryIssueDetail = new List<Dictionary<string, object>>();
            //InventoryIssueDetail = dsDetail.Tables[0].ToList<Dictionary<string, object>>();
            //List<Dictionary<string, object>> InventoryHistoryList = dsHistory.Tables[0].ToList<Dictionary<string, object>>();


            _salesService.PackingInvoiceInsert(voucherVM, salesMaterialVMList, selectedPackingList, salesServiceVMList, dsDetail, dsHistory);
            return Json(new { Data = voucherVM, Message = AplosMessage.Insert + "Invoice No: " + voucherVM.Id + "" });
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

        [HttpGet, Authorize]
        public JsonResult GetPackingSOData(string PackingId)
        {
            return Json(pi.GetPackingSOData(PackingId), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }


}