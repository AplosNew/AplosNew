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
using Library.ViewModel.Invoices;
using Library.Service.Extension.Accounts;
using Library.Model.Parties;
using Library.Model.Invoices;
using Library.Model.Vouchers;

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

        [Authorize, HttpGet]
        public JsonResult GetSalesReturnList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetSalesReturnData(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetPackingSalesListForReturn(string column, string value)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsSalesService.GetPackingSalesListForReturn(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetSalesListForReturn(string column, string value)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsSalesService.GetSalesListForReturn(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
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
        public ActionResult GetPackingSalesDetailDataBySales(string salesId, string packingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetPackingSalesMaterialData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId, packingId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetItemScanChildData(string salesId, string packingId, string soId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetItemScanChildData(salesId, packingId, soId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetItemScanChildDataByPackingId(string salesId, string packingId, string soId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetItemScanChildDataByPackingId(salesId, packingId, soId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveSalesReturn(Dictionary<string, object> data, List<Dictionary<string, object>> detaildataList
            , List<Dictionary<string, object>> taxList, List<Dictionary<string, object>> itemScanCildList, List<Dictionary<string, object>> itemScanCildNewList)
        {
            try
            {
                if (itemScanCildNewList != null)
                {
                    foreach (var item in itemScanCildNewList)
                    {
                        item["Id"] = null;
                        item["Booked"] = 0;
                        item["IsDespatch"] = 0;
                        item["ReturnNetWeight"] = 0;
                    }
                }
                string _id = InsertSalesReturn(data, detaildataList, taxList, itemScanCildList, itemScanCildNewList);
                return Json(new { Id = _id, Message = string.Format(AplosMessage.Success + " Sales Return No <b>" + _id + "</b>") });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string InsertSalesReturn(Dictionary<string, object> data, List<Dictionary<string, object>> detaildataList, List<Dictionary<string, object>> taxList
            , List<Dictionary<string, object>> itemScanCildList, List<Dictionary<string, object>> itemScanCildNewList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsDetail;
            DataSet dstax;
            DataSet dsitemscanChild;
            DataSet dsitemscanChildNew;
            string TableName = "dbo.ItemScanChild";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                string sqlmaster = "SELECT * FROM [TRN].[SalesReturn] WHERE Id='" + data["SalesId"].ToString() + "'";
                string sqlDetail = "SELECT * FROM [TRN].[SalesReturnDetail] WHERE SalesId='" + data["SalesId"].ToString() + "'";
                string taxsql = "SELECT * FROM [TRN].[SalesReturnTax] WHERE SalesId='" + data["SalesId"].ToString() + "'";
                string itemScanChildsql = "SELECT * FROM dbo.ItemScanChild WHERE SalesId='" + data["SalesId"].ToString() + "'";
                //string itemScanChildNewsql = "SELECT * FROM dbo.ItemScanChild WHERE SalesId='" + data["SalesId"].ToString() + "'";
                //string poUpdateLogsql = "SELECT Top(1) * FROM [TRN].[PurchaseOrderUpdateLog] WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlmaster, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(sqlDetail, out dsDetail, false, "1");
                objCon.OpenDataSetThroughAdapter(taxsql, out dstax, false, "1");
                objCon.OpenDataSetThroughAdapter(itemScanChildsql, out dsitemscanChild, false, "1");
                //objCon.OpenDataSetThroughAdapter(itemScanChildNewsql, out dsitemscanChildNew, false, "1");
                objCon.getDataSet("Select * from dbo.ItemScanChild where 1=2", out dsitemscanChildNew);
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
                        int Index = 0; string _itemNewId = "";
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_Id, ccount, 2);
                            item["Id"] = detailid;
                            item["SalesReturnId"] = _Id;
                            item["SalesMaterialId"] = item["SalesMaterialId"];
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
                                foreach (var tx in taxList.Where(r => r["SalesMaterialId"].ToString() == item["SalesMaterialId"].ToString()))
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
                                foreach (var scitem in itemScanCildList.Where(r => r["SalesId"].ToString() == item["SalesId"].ToString()
                                    && r["ActualPackingId"].ToString() == item["PackingId"].ToString()
                                    && r["SalesOrderId"].ToString() == item["SalesOrderId"].ToString()))
                                {
                                    DataView dvsc = new DataView(dsitemscanChild.Tables[0]);
                                    dvsc.RowFilter = "Id='" + scitem["Id"] + "'";

                                    if (dvsc.Count > 0)
                                    {
                                        DataRow drmo = dvsc[0].Row;
                                        drmo.BeginEdit();
                                        drmo["SalesReturnId"] = _Id;
                                        //drmo["ReturnNetWeight"] = scitem["ReturnNetWeight"];
                                        //drmo["Booked"] = false;
                                        drmo["UpdatedBy"] = identity.Name;
                                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                                        drmo.EndEdit();

                                    }

                                }
                            }
                            if (itemScanCildNewList != null)
                            {
                                foreach (var scitemNew in itemScanCildNewList)
                                {
                                    Index++;
                                    DataView dvnewitem = new DataView(dsitemscanChildNew.Tables[0]);
                                    dvnewitem.RowFilter = "Id='" + scitemNew["Id"] + "' ";
                                    if (dv.Count == 0)
                                    {
                                        if (_itemNewId == "")
                                        {
                                            clsGenID genid = new clsGenID();
                                            genid.GenID(TableName, out _itemNewId);
                                        }
                                        scitemNew["Id"] = "SC" + _itemNewId + "-" + Index;
                                        scitemNew["SalesId"] = DBNull.Value;
                                        AddNewRowD(dsitemscanChildNew.Tables[0], scitemNew);
                                    }
                                }
                            }

                        }

                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail, dstax, dsitemscanChild, dsitemscanChildNew);
                return _Id;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void AddNewRowD(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow(); foreach (var item in sourceData.Keys)
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
            dr["UpdatedBy"] = identity.Name;
            dt.Rows.Add(dr);
        }
        
        private string GetSalesReturnPK()
        {
            string sID = string.Empty;
            MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
            materialCommonService.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalesReturn", out sID);
            return sID;
        }



        #endregion

        #region Sales Return Post
        public ActionResult SalesReturnPost()
        {
            return View("~/Areas/SalesManagements/Views/SalesReturnPost.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesReturnPostedList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetSalesReturnPostedData(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetSalesReturnPopUpData(string column, string value)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsSalesService.GetSalesReturnPopUpData(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesReturnDetailDataBySalesReturn(string salesReturnId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetSalesReturnDetailBySalesReturn(salesReturnId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetSalesReturnTaxDetail(string salesId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetMaterialSalesTaxDetail(salesId), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpGet]
        public JsonResult GetSalesReturnJournal(string salesReturnId, string customerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetSalesReturnJournalData(identity.CompanyId, identity.PlantId, salesReturnId, customerId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesReturnDetailGLUpdateData(string salesReturnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetSalesReturnDetailGLUpdateData(identity.CompanyId, identity.PlantId, salesReturnId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSalesReturnCreditNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, List<Dictionary<string, object>> salesReturnDetailList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = false;
            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {
                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is Not Mapped!");
                }

                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");
            voucherVM.SourceType = SourceType.CreditNote.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, PostSalesReturn(voucherVM, voucherDetailVMList, salesReturnDetailList, invoiceTaxVMList)) });
        }
        public string PostSalesReturn(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, List<Dictionary<string, object>> salesReturnDetailList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList)
        {
            var flag = false;
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                DataSet _ajNDetailData = null;
                DataSet _invTaxDetailData = null;
                DataSet _invTaxDetailCrData = null;
                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                DataSet _salesReturnData = null;
                DataSet _iTaxDrdataset = null;
                DataSet _iTaxCrdataset = null;
                DataSet dsitemscanChild;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //_unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.PartyType = PartyType.Vendor.ToString();
                voucherVM.NoteType = NoteType.CustomerCreditNote.ToString();
                voucherVM.Amount = voucherDetailVMList.Where(r => r.OtherName == "Return").Sum(r => r.Amount);
                voucherVM.DocRefNo = "PR" + voucherVM.DocRefNo;
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.CreditNote.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                var adjustmentNote = new AdjustmentNote
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    CurrencyId = voucherVM.CurrencyId,
                    Amount = voucherVM.Amount,
                    VoucherDate = voucher.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PartyType = voucherVM.PartyType,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    SourceType = voucherVM.SourceType,
                    IsPark = voucherVM.IsPark,
                    NoteType = voucherVM.NoteType,
                    InvoiceId = voucherVM.InvoiceId,
                    Archive = false,
                    SettlementType = voucherVM.SettlementType
                };
                if (adjustmentNote.SourceType == SourceType.CreditNote.ToString())
                {
                    if (adjustmentNote.NoteType == NoteType.CustomerCreditNote.ToString())
                        adjustmentNote.PartyType = PartyType.Customer.ToString();
                    else if (adjustmentNote.NoteType == NoteType.VendorCreditNote.ToString())
                        adjustmentNote.PartyType = PartyType.Vendor.ToString();
                    else throw new CustomException("Party type is null.");
                }
                else if (adjustmentNote.SourceType == SourceType.DebitNote.ToString())
                {
                    if (adjustmentNote.NoteType == NoteType.CustomerDebitNote.ToString())
                        adjustmentNote.PartyType = PartyType.Customer.ToString();
                    else if (adjustmentNote.NoteType == NoteType.VendorDebitNote.ToString())
                        adjustmentNote.PartyType = PartyType.Vendor.ToString();
                    else throw new CustomException("Party type is null.");
                }

                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);
                adjustmentNote.VoucherId = voucher.Id;
                _accountsCommonService.InsertAdjustmentNote(adjustmentNote, out DataSet _ANdataset);
                ConnectionManager.DAL.ConManager objCon;
                string salesReturn = "SELECT * FROM TRN.SalesReturn WHERE Id='" + voucherVM.SalesReturnId + "'";
                string itemScanChildsql = "SELECT * FROM TRN.SalesReturnDetail WHERE SalesReturnId='" + voucherVM.SalesReturnId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(salesReturn, out _salesReturnData, false, "1");
                objCon.OpenDataSetThroughAdapter(itemScanChildsql, out dsitemscanChild, false, "1");

                DataView dvsc = new DataView(_salesReturnData.Tables[0]);
                dvsc.RowFilter = "Id='" + voucherVM.SalesReturnId + "'";


                if (dvsc.Count > 0)
                {
                    DataRow drmo = dvsc[0].Row;
                    if (!string.IsNullOrEmpty(drmo["VoucherId"].ToString()))
                    {
                        throw new CustomException("Already have posted!!");
                    }
                    drmo.BeginEdit();
                    drmo["VoucherId"] = voucher.Id;
                    drmo["UpdatedBy"] = voucher.AddedBy;
                    drmo["UpdatedFromIP"] = voucher.AddedFromIP;
                    drmo["UpdatedDate"] = DateTime.Now.ToString();
                    drmo.EndEdit();

                }

                var currentVoucherDetailId = 0;
                decimal totalAmountDr = 0;
                decimal totalAmountCr = 0;

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.OtherName == "Return")
                    {
                        var adjustmentNoteDetail = new AdjustmentNoteDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = adjustmentNote.Amount,
                            WrittenOffAmount = 0,
                            IsWrittenOff = false
                        };
                        _accountsCommonService.InsertAdjustmentNoteDetail(adjustmentNote, adjustmentNoteDetail, 1, ref _ajNDetailData);
                        var voucherDetail = new VoucherDetail
                        {
                            GLGeneralInfoId = adjustmentNoteDetail.GLGeneralInfoId,
                            BudgetMasterId = adjustmentNoteDetail.BudgetMasterId,
                            ActivityId = adjustmentNoteDetail.ActivityId,
                            EntityId = voucher.EntityId,
                            PartyType = adjustmentNote.PartyType,
                            PartyId = adjustmentNote.PartyId,
                            PartyPlantId = adjustmentNote.PartyPlantId,
                            TrnNature = TransactionNature.CreditNote.ToString(),
                            AdjustmentNoteDetailId = adjustmentNoteDetail.Id,
                            CrAmount = voucherVM.Amount
                        };
                        totalAmountCr += voucherDetail.CrAmount;
                        currentVoucherDetailId++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId, ref _crvDetailData);

                        // INSERT INTO VoucherDetailCurrency
                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount
                        }, ref _crvDetailCurrencyData);

                        if (salesReturnDetailList != null)
                        {
                            foreach (var scitem in salesReturnDetailList.Where(r => r["GLGeneralInfoId"].ToString() == voucherDetailVM.GLGeneralInfoId
                                && r["BudgetMasterId"].ToString() == voucherDetailVM.BudgetMasterId
                                && r["ActivityId"].ToString() == voucherDetailVM.ActivityId && r["OtherName"].ToString() == "Return" && r["TrnType"].ToString() == "Cr"))
                            {
                                DataView srd = new DataView(dsitemscanChild.Tables[0]);
                                srd.RowFilter = "Id='" + scitem["SalesReturnDetailId"] + "'";
                                if (srd.Count > 0)
                                {
                                    DataRow drmo = srd[0].Row;
                                    drmo.BeginEdit();
                                    drmo["PostCrGLGeneralInfoId"] = scitem["GLGeneralInfoId"];
                                    drmo["PostCrBudgetMasterId"] = scitem["BudgetMasterId"];
                                    drmo["PostCrActivityId"] = scitem["ActivityId"];
                                    drmo["VoucherDetailId"] = voucherDetail.Id;
                                    drmo["UpdatedBy"] = identity.Name;
                                    drmo["UpdatedDate"] = DateTime.Now.ToString();
                                    drmo.EndEdit();
                                }
                            }
                        }
                    }


                    if (voucherDetailVM.OtherName == "Tax" && voucherDetailVM.TrnType == "Dr" || voucherDetailVM.OtherName == "TCS" && voucherDetailVM.TrnType == "Dr" || voucherDetailVM.OtherName == "Material" && voucherDetailVM.TrnType == "Dr")
                    {
                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            TrnNature = TransactionNature.Sales.ToString(),
                            DrAmount = voucherDetailVM.Amount
                        };
                        totalAmountDr += voucherDetailDr.DrAmount;
                        currentVoucherDetailId++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId, ref _crvDetailData);

                        // INSERT INTO VoucherDetailCurrency
                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                        }, ref _crvDetailCurrencyData);
                        if (salesReturnDetailList != null)
                        {
                            foreach (var scitem in salesReturnDetailList.Where(r => r["GLGeneralInfoId"].ToString() == voucherDetailVM.GLGeneralInfoId
                                && r["BudgetMasterId"].ToString() == voucherDetailVM.BudgetMasterId
                                && r["ActivityId"].ToString() == voucherDetailVM.ActivityId && r["OtherName"].ToString() == "Material" && r["TrnType"].ToString() == "Dr"))
                            {
                                DataView srd = new DataView(dsitemscanChild.Tables[0]);
                                srd.RowFilter = "Id='" + scitem["SalesReturnDetailId"] + "'";
                                if (srd.Count > 0)
                                {
                                    DataRow drmo = srd[0].Row;
                                    drmo.BeginEdit();
                                    drmo["PostDrGLGeneralInfoId"] = scitem["GLGeneralInfoId"];
                                    drmo["PostDrBudgetMasterId"] = scitem["BudgetMasterId"];
                                    drmo["PostDrActivityId"] = scitem["ActivityId"];
                                    drmo["UpdatedBy"] = identity.Name;
                                    drmo["UpdatedDate"] = DateTime.Now.ToString();
                                    drmo.EndEdit();
                                }
                            }
                        }

                        if (voucherDetailVM.OtherName == "Tax" && voucherDetailVM.TrnType == "Dr" || voucherDetailVM.OtherName == "TCS" && voucherDetailVM.TrnType == "Dr")
                        {
                            var invoiceTax = new InvoiceTax
                            {
                                Archive = false,
                                VoucherDetailId = voucherDetailDr.Id,//voucherDetailDrId,
                                VoucherId = voucher.Id,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                AdjustmentNoteId = adjustmentNote.Id,
                                TaxAmount = voucherDetailVM.Amount,
                                TaxAutoAmount = 0,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.CreditNote.ToString(),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _accountsCommonService.InsertInvoiceTax(adjustmentNote, invoiceTax, ref _iTaxDrdataset);
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                Id = invoiceTax.Id + 1,
                                InvoiceTaxId = invoiceTax.Id,
                                Amount = invoiceTax.TaxAmount,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AType = "Cr",
                                AddedBy = invoiceTax.AddedBy,
                                AddedDate = invoiceTax.AddedDate,
                                AddedFromIP = invoiceTax.AddedFromIP
                            };
                            _accountsCommonService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, ref _invTaxDetailData);
                        }

                    }

                    if (voucherDetailVM.OtherName == "Tax" && voucherDetailVM.TrnType == "Cr")
                    {
                        var voucherDetailDrTax = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            TrnNature = TransactionNature.Purchases.ToString(),
                            CrAmount = voucherDetailVM.Amount
                        };
                        totalAmountCr += voucherDetailDrTax.CrAmount;
                        currentVoucherDetailId++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDetailDrTax, currentVoucherDetailId, ref _crvDetailData);

                        // INSERT INTO VoucherDetailCurrency
                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDetailDrTax, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDrTax.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucherDetailDrTax.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDrTax.CrAmount
                        }, ref _crvDetailCurrencyData);


                        var invoiceTax = new InvoiceTax
                        {
                            Archive = false,
                            VoucherDetailId = voucherDetailDrTax.Id,//voucherDetailDrId,
                            VoucherId = voucher.Id,
                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            TaxCategoryId = voucherDetailVM.TaxCategoryId,
                            TaxAmount = voucherDetailVM.Amount,
                            TaxAutoAmount = 0,
                            PartyId = voucherVM.PartyId,
                            SourceType = SourceType.CreditNote.ToString(),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _accountsCommonService.InsertInvoiceTax(adjustmentNote, invoiceTax, ref _iTaxCrdataset);
                        var invoiceTaxDetail = new InvoiceTaxDetail
                        {
                            Id = invoiceTax.Id + 1,
                            InvoiceTaxId = invoiceTax.Id,
                            Amount = invoiceTax.TaxAmount,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            AType = "Dr",
                            AddedBy = invoiceTax.AddedBy,
                            AddedDate = invoiceTax.AddedDate,
                            AddedFromIP = invoiceTax.AddedFromIP
                        };
                        _accountsCommonService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, ref _invTaxDetailCrData);
                    }
                }


                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _ANdataset, _ajNDetailData, _crvDetailData, _crvDetailCurrencyData, _iTaxDrdataset, _invTaxDetailData, _drvDetailData, _drvDetailCurrencyData, _iTaxCrdataset, _invTaxDetailCrData, _salesReturnData, dsitemscanChild);
                return voucher.VoucherNo;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesReturnCreditNoteReport(ReportFormat reportFormat, string voucherId, SourceType sourceType)
        {
            AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService, _plantService);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountsSalesReportService.GetSalesReturnCreditNoteReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, sourceType);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
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
        public ActionResult DeleteMasterOrderSalePost(string salesId, string voucherId, string deletedRemarks)
        {
            if (deletedRemarks == null || deletedRemarks == "")
                throw new CustomException("Deleted Remarks is required!");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _salesService.DeleteMasterOrderSalePost(identity.CompanyId, identity.PlantId, salesId, voucherId, deletedRemarks);

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

        #region Sales Incentive

        [Authorize, HttpGet]
        public ActionResult SalesIncentive()
        {
            return View("~/Areas/SalesManagements/Views/SalesIncentive.cshtml");
        }
        //public ActionResult MasterOrderSalesPost()
        //{
        //    return View("~/Areas/SalesManagements/Views/MasterOrderSalesPost.cshtml");
        //}

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesIncentiveList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService _accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(_accountsSalesService.GetMasterOrderSalesIncentiveList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

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
        //    JsonResult json = Json(_accountsSalesService.GetMasterOrderSalesPostedList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value), JsonRequestBehavior.AllowGet);
        //    json.MaxJsonLength = int.MaxValue;
        //    return json;
        //}


        //[HttpPost, Authorize]
        //public JsonResult GetPostedMasterOrderSalesList(string column, string value)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    JsonResult json = Json(clsSales.GetMasterOrderSalesPostedList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value), JsonRequestBehavior.AllowGet);
        //    json.MaxJsonLength = int.MaxValue;
        //    return json;
        //}

        //[HttpPost]
        //public ActionResult DeleteMasterOrderSalePost(string salesId, string voucherId, string deletedRemarks)
        //{
        //    if (deletedRemarks == null || deletedRemarks == "")
        //        throw new CustomException("Deleted Remarks is required!");
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    _salesService.DeleteMasterOrderSalePost(identity.CompanyId, identity.PlantId, salesId, voucherId, deletedRemarks);

        //    return Json(new { Message = AplosMessage.Deleted });
        //}




        //[HttpPost]
        //public JsonResult PostMasterOrderSales(VoucherViewModel sales, IEnumerable<SalesMaterialViewModel> salesDetailVMList
        //    , IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList, IEnumerable<SalesServiceViewModel> salesServiceDetailGLList)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    sales.CompanyGroupId = identity.CompanyGroupId;
        //    sales.CompanyId = identity.CompanyId;
        //    sales.PlantId = identity.PlantId;
        //    if (salesDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != salesDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
        //        throw new CustomException("Dr Cr Amount not equal");
        //    foreach (var item in salesDetailVMList)
        //    {
        //        if (item.GLGeneralInfoId == null)
        //            throw new CustomException("GL is not found");
        //        if (item.BudgetMasterId == null)
        //            throw new CustomException("Budget is not found");
        //        if (item.ActivityId == null)
        //            throw new CustomException("Activity is not found");
        //    }
        //    _salesService.MasterOrderSalesPost(sales, salesDetailVMList, salesMaterialDetailGLList, salesServiceDetailGLList);

        //    return Json(new { Message = AplosMessage.Posted });
        //}
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
        public ActionResult CreateSalesAdditionalInfo(List<Dictionary<string, object>> data, string salesId)
        {
            try
            {
                SaveSalesAdditionalInfodata(data, salesId);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        private void SaveSalesAdditionalInfodata(List<Dictionary<string, object>> data, string salesId)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalesAdditionalInfo Where SalesId='"+ salesId + "'", out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        if (Convert.ToBoolean(item["Flag"])==true)
                        {
                            DataView dv = new DataView(dsMaster.Tables[0]);
                            dv.RowFilter = "AdditionalInfoId='" + item["AdditionalInfoId"] + "' AND SalesId='"+ item["SalesId"] + "' ";


                            if (dv.Count == 0)
                            {
                                item["Id"] = GetPK();
                                item["SalesId"] = salesId;

                                AddNewRow(dsMaster.Tables[0], item);
                            }
                            else
                            {
                                DataRow drmo = dv[0].Row;
                                EditRow(drmo, item);
                            } 
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

        [Authorize, HttpGet]
        public ActionResult SalesReturnReport(string salesReturnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.SalesReturnService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesReturnId);

            return View();
        }
    }
}