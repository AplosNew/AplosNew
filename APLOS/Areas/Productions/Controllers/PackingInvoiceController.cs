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
        public PackingInvoiceController(ISalesService salesService,ISqlRepository R)
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
            if (selectedPackingList != null)
            {
                foreach (var item in selectedPackingList)
                {
                    var data = clsSales.GetQtyAmountByPackingId(item.PackingId);
                    item.Qty = Convert.ToDecimal(data["Qty"].ToString());
                    item.Amount = Convert.ToDecimal(data["Amount"].ToString());
                    item.ProductLibraryId = data["ProductLibraryId"].ToString();
                }
            }


            _salesService.PackingInvoiceInsert(voucherVM, salesMaterialVMList, selectedPackingList, salesServiceVMList);
            return Json(new { Data = voucherVM, Message = AplosMessage.Insert + "Invoice No: " + voucherVM.Id + "" });
        }

        [HttpPost]
        public JsonResult Edit(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedPackingList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
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
                }
            }

            _salesService.PackingInvoiceUpdate(voucherVM, salesMaterialVMList, selectedPackingList, salesServiceVMList);
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
            string strSQL, strPSQL, strBSQL, strOSQL, strSSQL, strASQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //if (CheckUsing(id))
                //    throw new CustomException("First delete Operation!");

                strOSQL = "DELETE FROM TRN.SalesTax WHERE SalesId='" + id + "'";
                strASQL = "DELETE FROM TRN.SalesAdditionalTax WHERE SalesId='" + id + "'";
                strSSQL = "DELETE FROM TRN.SalesService WHERE SalesId='" + id + "'";
                strPSQL = "DELETE FROM dbo.SalesPacking WHERE SalesId='" + id + "'";
                strBSQL = "DELETE FROM TRN.SalesMaterial WHERE SalesId='" + id + "'";
                strSQL = "DELETE FROM TRN.Sales WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strASQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
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
                    throw ex;
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

    }
}