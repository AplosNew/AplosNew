using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.SalesManagements;
using Library.OrderManagement.Sales;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Extension;
using Library.Service.Materials;
using Library.Service.OrderManagements;
using Library.Service.Organizations;
using Library.MaterialManagement.Reports;
using Library.Service.SalesManagements;
using Library.ViewModel.SalesManagements;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using System.IO;
using System.Data;
using Aplos.MaterialManagement.MaterialQuery;

namespace Aplos.Areas.SalesManagements.Controllers
{
    public class SalesController : BaseController
    {
        private readonly ISalesService _salesService;
        private readonly ISalesReportService _salesReportService;
        private readonly IMasterOrderService _masterOrderService;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly ISqlRepository _sqlRepository;
        private readonly CompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        clsSales clsSales = new clsSales();
        //This is Shakawat
        public SalesController(ISalesService salesService, ISalesReportService salesReportService
            , IMasterOrderService masterOrderService
            , IMaterialMasterService materialMasterService
            , ISqlRepository sqlRepository
            , CompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService

            )
        {
            _salesService = salesService;
            _salesReportService = salesReportService;
            _masterOrderService = masterOrderService;
            _materialMasterService = materialMasterService;
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }

        #region Sales

        public ActionResult Sales()
        {
            return View("~/Areas/SalesManagements/Views/Sales.cshtml");
        }
        public ActionResult EInvoice()
        {
            return View("~/Areas/SalesManagements/Views/EInvoice.cshtml");
        }
        public ActionResult AdditionalInfo()
        {
            return View("~/Areas/SalesManagements/Views/AdditionalInfo.cshtml");
        }
        [HttpGet, Authorize]
        public ActionResult GetMaterialSalesList(GridParameter parameters)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetMaterialSalesList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesDataList(GridParameter parameters)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetMasterOrderSalesList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesMaterialData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsSales.GetSalesMaterialData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesMaterialData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsSales.GetMasterOrderSalesMaterialData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalesServiceData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetSalesServiceData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalesTaxData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetSalesTaxData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesServiceTaxData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetSalesServiceTaxData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSales(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
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
                        throw new CustomException("Please Input   Amount !");
                    if (item.TransactionQty == 0)
                        throw new CustomException("Please Input   Quantity !");
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
            _salesService.Insert(voucherVM, salesMaterialVMList, salesServiceVMList);
            return Json(new { Data = voucherVM, Message = AplosMessage.Insert + "Invoice No: " + voucherVM.Id + "" });
        }

        [HttpPost]
        public JsonResult UpdateSales(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
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
                        throw new CustomException("Please Input   Amount !");
                    if (item.TransactionQty == 0)
                        throw new CustomException("Please Input   Quantity !");
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

            _salesService.Update(voucherVM, salesMaterialVMList, salesServiceVMList);

            return Json(new { Data = voucherVM, Message = AplosMessage.Updated + "Invoice No: " + voucherVM.Id + "" });
        }

        [HttpPost]
        public JsonResult Delete(string Id)
        {
            _salesService.Delete(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

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

        [HttpPost, Authorize]
        public JsonResult SalesInvoicePost(VoucherViewModel sales, IEnumerable<VoucherDetailViewModel> salesJVDetail
            , IEnumerable<SalesMaterialViewModel> salesDetailList, IEnumerable<SalesServiceViewModel> salesServiceDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sales.CompanyGroupId = identity.CompanyGroupId;
            sales.CompanyId = identity.CompanyId;
            sales.PlantId = identity.PlantId;
            sales.PostingDate = sales.PostingDate;
            if (salesJVDetail != null)
            {
                if (salesJVDetail.Where(r => r.TrnType == "Dr").Sum(r => r.Amount) != salesJVDetail.Where(r => r.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount is not match!");
                foreach (var item in salesJVDetail)
                {
                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is not found");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is not found");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is not found");
                }
            }
            _salesService.SalesInvoicePost(sales, salesJVDetail, salesDetailList, salesServiceDetailList);

            return Json(new { Message = AplosMessage.Posted });
        }

        [Authorize, HttpGet]
        public ActionResult SalesInvoicePending()
        {
            return View("~/Areas/SalesManagements/Views/SalesInvoicePending.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesPendingList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetSalesPendingList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult SalesReport(ReportFormat reportFormat, string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Inventory Receive " + salesId + "";
            var workbook = _salesReportService.GetSalesReport(out reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, salesId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        #region SalesWordReport


        [Authorize, HttpGet]
        public ActionResult SalesReportService(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.GetSalesWordReportService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, grnId);

            return View();
        }


        #endregion


        [Authorize, HttpGet]
        public ActionResult LocalTaxInvoice(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.LocalTaxInvoiceService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);

            return View();
        }
        [Authorize, HttpGet]
        public ActionResult LocalTaxInvoiceWithProductDetailService(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.LocalTaxInvoiceWithProductDetailService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);

            return View();
        }

        [Authorize, HttpGet]
        public ActionResult LocalTaxInvoiceWithoutSKU(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.LocalTaxInvoiceWithoutSKUService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);

            return View();
        }

        [Authorize, HttpGet]
        public ActionResult CommercialInvoice(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.CommercialInvoiceService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);

            return View();
        }
        [Authorize, HttpGet]
        public ActionResult SalesInvoice(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.SalesInvoiceService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);

            return View();
        }

        [HttpGet, Authorize]
        public ActionResult SalesReceivableReport(ReportFormat reportFormat, string voucherId)
        {
            AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService, _plantService);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountsSalesReportService.GetMasterOrderSalesPostReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        #endregion

        #region  Sales Return

        public ActionResult SalesReturn()
        {
            return View("~/Areas/SalesManagements/Views/SalesReturn.cshtml");
        }

        [Authorize, HttpPost]
        public JsonResult GetMaterialSalesListForReturn(string column, string value)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsSalesService.GetMaterialSalesListForReturn(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesDetailDataBySales(string salesId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetMaterialSalesDetailBySales(salesId), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpGet]
        public JsonResult GetMaterialSalesTaxDetail(string salesId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetMaterialSalesTaxDetail(salesId), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetPackingSalesDetailDataBySales(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetPackingSalesMaterialData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetItemScanChildData(string salesId,string packingId,string soId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetItemScanChildData(salesId, packingId, soId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveSalesReturn(Dictionary<string, object> data, List<Dictionary<string, object>> detaildataList
            , List<Dictionary<string, object>> taxList, List<Dictionary<string, object>> itemScanCildList)
        {
            try
            {
                InsertSalesReturn(data, detaildataList, taxList, itemScanCildList);
                return Json(new { data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void InsertSalesReturn(Dictionary<string, object> data, List<Dictionary<string, object>> detaildataList, List<Dictionary<string, object>> taxList,  List<Dictionary<string, object>> itemScanCildList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsDetail;
            DataSet dstax;
            DataSet dsitemscanChild;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                string sqlmaster = "SELECT * FROM [TRN].[SalesReturn] WHERE Id='" + data["SalesId"].ToString() + "'";
                string sqlDetail = "SELECT * FROM [TRN].[SalesReturnDetail] WHERE SalesId='" + data["SalesId"].ToString() + "'";
                string taxsql = "SELECT * FROM [TRN].[SalesReturnTax] WHERE SalesId='" + data["SalesId"].ToString() + "'";
                string itemScanChildsql = "SELECT * FROM dbo.ItemScanChild WHERE SalesId='" + data["SalesId"].ToString() + "'";
                //string poUpdateLogsql = "SELECT Top(1) * FROM [TRN].[PurchaseOrderUpdateLog] WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlmaster, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(sqlDetail, out dsDetail, false, "1");
                objCon.OpenDataSetThroughAdapter(taxsql, out dstax, false, "1");
                objCon.OpenDataSetThroughAdapter(itemScanChildsql, out dsitemscanChild, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetSalesReturnPK();
                    dr["SalesId"] = data["SalesId"].ToString();
                    dr["DocRefNo"] = data["DocRefNo"].ToString();
                    dr["SalesReturnDate"] = data["SalesReturnDate"].ToString();
                    dr["Narration"] = data["Narration"].ToString();
                    dr["EntryDate"] = DateTime.Now;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                string _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                int ccount = 0;
                int taxcount = 0;
                if (detaildataList != null)
                {
                    foreach (var item in detaildataList)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'"; 
                        if (dv.Count == 0)
                        {
                            ccount++; 
                            string detailid = materialCommonService.MakePK(_Id, ccount, 2);
                            item["Id"] = detailid;
                            item["SalesReturnId"] = _Id;
                            item["TransactionQty"] = item["ReturnQty"];
                            item["BaseQty"] = item["ReturnQty"];
                            item["BaseAmount"] = item["Amount"];
                            item["TransactionAmount"] = item["Amount"];
                            item["BooksCurrencyTransactionAmount"] = item["Amount"];
                            item["BooksCurrencyTaxAmount"] = item["TaxAmount"];
                            item["BooksCurrencyBaseRate"] = item["BaseRate"];
                            item["AddedBy"] = identity.Name;
                            item["AddedDate"] = DateTime.Now;
                            item["AddedFromIP"] = identity.IPAddress;
                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);

                            if (taxList != null)
                            {
                                foreach (var tx in taxList.Where(r=>r["SalesMaterialId"].ToString()== item["SalesMaterialId"].ToString()))
                                {
                                    DataView dvtx = new DataView(dstax.Tables[0]);
                                    dvtx.RowFilter = "Id='" + tx["Id"] + "'";

                                    if (dvtx.Count == 0)
                                    {
                                        taxcount++;
                                        string taxid = materialCommonService.MakePK(detailid, taxcount, 2);
                                        tx["Id"] = taxid;
                                        tx["SalesReturnId"] = _Id;
                                        tx["SalesReturnDetailId"] = detailid;
                                        tx["AddedBy"] = identity.Name;
                                        tx["AddedDate"] = DateTime.Now;
                                        tx["AddedFromIP"] = identity.IPAddress;
                                        materialCommonService.AddNewRowD(dstax.Tables[0], tx);
                                    }

                                }
                            }
                            if (itemScanCildList != null)
                            {
                                foreach (var isc in itemScanCildList.Where(r => r["SalesId"].ToString() == item["SalesId"].ToString() 
                                    && r["ActualPackingId"].ToString() == item["PackingId"].ToString() 
                                    && r["SalesOrderId"].ToString() == item["SalesOrderId"].ToString()))
                                {
                                    DataView dvisc = new DataView(dsitemscanChild.Tables[0]);
                                    dvisc.RowFilter = "Id='" + isc["Id"] + "'";
                                    if (dvisc.Count > 0)
                                    {
                                        DataRow drisc = dvisc[0].Row;
                                        drisc["IsDespatch"] = false;
                                        drisc["Booked"] = false;
                                        drisc["UpdatedBy"] = identity.UserId;
                                        drisc["UpdatedDate"] = DateTime.Now;
                                        EditItemScanChildRowD(drisc, isc);
                                    }

                                }
                            }
                        }
                        
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail, dstax, dsitemscanChild);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void EditItemScanChildRowD(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit(); foreach (var item in sourceData.Keys)
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
            dr.EndEdit();
        }
        private string GetSalesReturnPK()
        {
            string sID = string.Empty;
            MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
            materialCommonService.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalesReturn", out sID);
            return sID;
        }
        #endregion




        #region Master Order Sales

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId, string PODate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, PODate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult MasterOrderSales()
        {
            return View("~/Areas/SalesManagements/Views/MasterOrderSales.cshtml");
        }


        [HttpGet, Authorize]
        public ActionResult GetMasterOrderPopUp()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //return Json(_masterOrderService.GetMasterOrderList(identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(_masterOrderService.GetMasterOrderList(identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderIdBySalesId(string salesId)
        {
            return Json(_salesService.GetMasterOrderIdBySalesId(salesId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetMasterOrderDataByMasterOrderId(string masterOrderId, string masterOrderItemId, string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetMasterOrderDataByMasterOrderId(identity.CompanyId, masterOrderId, masterOrderItemId, salesId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetItemSOSKUList(string masterOrderId)
        {
            return Json(clsSales.GetItemSOSKUList(masterOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult CheckItemArticleSKUList()
        {
            return Json(_materialMasterService.CheckItemArticleSKUList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MasterOrderSalesInsert(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesOrderItem> selectedMasterOrderList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
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
            _salesService.MasterOrderSalesInsert(voucherVM, salesMaterialVMList, selectedMasterOrderList, salesServiceVMList);
            return Json(new { Data = voucherVM, Message = AplosMessage.Insert + "Invoice No: " + voucherVM.Id + "" });
        }

        [HttpPost]
        public JsonResult MasterOrderSalesUpdate(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesOrderItem> selectedMasterOrderList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
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
            _salesService.MasterOrderSalesUpdate(voucherVM, salesMaterialVMList, selectedMasterOrderList, salesServiceVMList);
            return Json(new { Data = voucherVM, Message = AplosMessage.Updated + "Invoice No: " + voucherVM.Id + "" });
        }


        public ActionResult MasterOrderSalesPost()
        {
            return View("~/Areas/SalesManagements/Views/MasterOrderSalesPost.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(_accountsSalesService.GetMasterOrderSalesList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesDetailList(string salesId, string partyAccountGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(_accountsSalesService.GetMasterOrderSalesDetailList(identity.CompanyGroupId, identity.CompanyId, salesId, partyAccountGroup), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesServiceDetailList(string salesId, string partyAccountGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(_accountsSalesService.GetMasterOrderSalesServiceDetailList(identity.CompanyGroupId, identity.CompanyId, salesId, partyAccountGroup), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesReceivableList(string salesId, string taxApplicable, string partyAccountGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(_accountsSalesService.GetMasterOrderSalesReceivable(identity.CompanyId, identity.PlantId, salesId, taxApplicable, partyAccountGroup), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetMasterOrderSalesPostedList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            JsonResult json = Json(_accountsSalesService.GetMasterOrderSalesPostedList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpPost, Authorize]
        public JsonResult GetPostedMasterOrderSalesList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(clsSales.GetMasterOrderSalesPostedList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost]
        public ActionResult DeleteMasterOrderSalePost(string salesId, string voucherId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _salesService.DeleteMasterOrderSalePost(identity.CompanyId, identity.PlantId, salesId, voucherId);

            return Json(new { Message = AplosMessage.Deleted });
        }




        [HttpPost]
        public JsonResult PostMasterOrderSales(VoucherViewModel sales, IEnumerable<SalesMaterialViewModel> salesDetailVMList
            , IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList, IEnumerable<SalesServiceViewModel> salesServiceDetailGLList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sales.CompanyGroupId = identity.CompanyGroupId;
            sales.CompanyId = identity.CompanyId;
            sales.PlantId = identity.PlantId;
            if (salesDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != salesDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            foreach (var item in salesDetailVMList)
            {
                if (item.GLGeneralInfoId == null)
                    throw new CustomException("GL is not found");
                if (item.BudgetMasterId == null)
                    throw new CustomException("Budget is not found");
                if (item.ActivityId == null)
                    throw new CustomException("Activity is not found");
            }
            _salesService.MasterOrderSalesPost(sales, salesDetailVMList, salesMaterialDetailGLList, salesServiceDetailGLList);

            return Json(new { Message = AplosMessage.Posted });
        }
        #endregion

        #region Additional Tax
        [Authorize, HttpPost]//
        public ActionResult SaveAdditinalTax(string salesId, decimal BooksCurrencyBaseRate, List<Dictionary<string, object>> UserSendData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                OTSBD.IdentityParameter para = new OTSBD.IdentityParameter
                {
                    CompanyGroupId = identity.CompanyGroupId,
                    CompanyId = identity.CompanyId,
                    PlantId = identity.PlantId,
                    AddedBy = identity.Name,
                    AddedDate = DateTime.Now,
                    AddedFromIP = identity.IPAddress,
                    UpdatedBy = identity.Name,
                    UpdatedDate = DateTime.Now,
                    UpdatedFromIP = identity.IPAddress
                };

                clsSales.SaveAdditinalTax(salesId, BooksCurrencyBaseRate, para, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        [HttpGet, Authorize]
        public JsonResult GetAdvanceTaxInfo(string SalesId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetAdvanceTaxInfo(SalesId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult AdditionalTaxDelete(string Id)
        {

            try
            {
                clsSales.AdditionalTaxDelete(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        #endregion

        #region SalesPackingPost
        public ActionResult SalesPackingPost()
        {
            return View("~/Areas/SalesManagements/Views/SalesPackingPost.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesPackingList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(_accountsSalesService.GetSalesPackingList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetPackingJournal(string salesId)
        {
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsSalesService.GetPackingJournal(identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetPackingDetail(string salesId)
        {
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsSalesService.GetPackingDetail(identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);

        }

        //[HttpGet, Authorize]
        //public ActionResult GetMasterOrderSalesList()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
        //    return Json(_accountsSalesService.GetMasterOrderSalesList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public ActionResult GetMasterOrderSalesDetailList(string salesId, string partyAccountGroup)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
        //    return Json(_accountsSalesService.GetMasterOrderSalesDetailList(identity.CompanyGroupId, identity.CompanyId, salesId, partyAccountGroup), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public ActionResult GetMasterOrderSalesServiceDetailList(string salesId, string partyAccountGroup)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
        //    return Json(_accountsSalesService.GetMasterOrderSalesServiceDetailList(identity.CompanyGroupId, identity.CompanyId, salesId, partyAccountGroup), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public ActionResult GetMasterOrderSalesReceivableList(string salesId, string taxApplicable, string partyAccountGroup)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
        //    return Json(_accountsSalesService.GetMasterOrderSalesReceivable(identity.CompanyId, identity.PlantId, salesId, taxApplicable, partyAccountGroup), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost, Authorize]
        //public JsonResult GetMasterOrderSalesPostedList(string column, string value)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
        //    return Json(_accountsSalesService.GetMasterOrderSalesPostedList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost, Authorize]
        public JsonResult PostSalesPacking(VoucherViewModel sales, IEnumerable<SalesMaterialViewModel> salesDetailVMList
            , IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList, IEnumerable<SalesServiceViewModel> salesServiceDetailGLList
            , SalesPacking packing, IEnumerable<SalesMaterialViewModel> PackingDetailVMList, string packingVoucherTypeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sales.CompanyGroupId = identity.CompanyGroupId;
            sales.CompanyId = identity.CompanyId;
            sales.PlantId = identity.PlantId;
            //if (packing.PackingId == null)
            //    throw new CustomException("Packing List are not yet tag in Sales!!.");
            if (PackingDetailVMList == null)
                throw new CustomException("Packing JV is missing!!.");
            if (salesDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != salesDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            if (PackingDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != PackingDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Packing Dr Cr Amount not equal");
            //if (PackingDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) == 0)
            //    throw new CustomException("Packing Dr  Amount can not 0 !");
            //if (PackingDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount) == 0)
            //    throw new CustomException("Packing Cr  Amount can not 0 !");

            foreach (var item in salesDetailVMList)
            {
                if (item.GLGeneralInfoId == null)
                    throw new CustomException("GL is not found");
                if (item.BudgetMasterId == null)
                    throw new CustomException("Budget is not found");
                if (item.ActivityId == null)
                    throw new CustomException("Activity is not found");
            }
            _salesService.PackingSalesPost(sales, salesDetailVMList, salesMaterialDetailGLList, salesServiceDetailGLList, packing, PackingDetailVMList, packingVoucherTypeId);

            return Json(new { Message = AplosMessage.Posted });
        }
        #endregion




        [HttpPost]
        public ActionResult DeleteSales(string invoiceId, string voucherId)
        {
            _salesService.DeleteSale(invoiceId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetParkedSalesList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(clsSales.GetParkedSalesList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }

        [HttpGet, Authorize]
        public JsonResult GetSalesMaterialList(string Ids)
        {
            JsonResult json = Json(clsSales.GetSalesMaterialList(Ids), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult GetEInvoiceSaveReports(ReportFormat reportFormat, string issueIds, List<Dictionary<string, object>> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveEInvoicedata(data);

                IWorkbook workbook = clsSales.GetEInvoiceReports(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, identity.Name, issueIds);

                workbook.Version = ExcelVersion.Excel2013;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "E-Invoice Reports.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        private void SaveEInvoicedata(List<Dictionary<string, object>> data)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.EInvoice", out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "SalesId='" + item["SalesId"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = item["SalesId"];
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }


                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }



            }
            catch (Exception ex)
            {
                throw (ex);
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

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalesAdditionalInfo", out sID);
            return sID;
        }

        [HttpPost]
        public ActionResult CreateSalesAdditionalInfo(Dictionary<string, object> data)
        {
            try
            {
                SaveSalesAdditionalInfodata(data);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        private void SaveSalesAdditionalInfodata(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalesAdditionalInfo", out dsMaster, false, "1");


                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        data["Id"] = GetPK();
                        data["SalesId"] = data["SalesId"];
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, data);
                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesAdditionalInfoData(string salesId)
        {
            return Json(clsSales.GetSalesAdditionalInfoData(salesId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteItem(string id)
        {
            DeleteItemData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteItemData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strSQL = "DELETE FROM [dbo].[SalesAdditionalInfo] WHERE Id = '" + id + "'";

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
    }
}