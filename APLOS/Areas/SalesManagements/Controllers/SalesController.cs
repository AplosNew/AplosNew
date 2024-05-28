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
using Library.Service.Helpers;
using Library.Model.Accounts;
//using OTSBD;

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
        public ActionResult InputCredit()
        {
            return View("~/Areas/SalesManagements/Views/InputCredit.cshtml");
        }
        public ActionResult InputCreditCheck()
        {
            return View("~/Areas/SalesManagements/Views/InputCreditCheck.cshtml");
        }
        public ActionResult InputCreditApprove()
        {
            return View("~/Areas/SalesManagements/Views/InputCreditApprove.cshtml");
        }
        public ActionResult SalesProcess()
        {
            return View("~/Areas/SalesManagements/Views/SalesProcess.cshtml");
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
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.Sales where DocRefNo='" + voucherVM.DocRefNo + "' AND  Id<>'" + voucherVM.Id + "'", out DataSet dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Doc Ref already exists!!!");

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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult UpdateSales(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.Sales where DocRefNo='" + voucherVM.DocRefNo + "' AND  Id<>'" + voucherVM.Id + "'", out DataSet dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Doc Ref already exists!!!");

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
            catch (Exception ex)
            {
                throw ex;
            }
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
        public JsonResult CancelSalesMaterial(string Id, string remark)
        {
            if (remark == null)
                throw new CustomException("Please Input Remark !");
            _salesService.CancelSalesMaterial(Id, remark);

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
        public ActionResult GetLotWiseTaxInvoice(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.GetLotWiseTaxInvoiceService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);

            return View();
        }

        [HttpPost, Authorize]
        public JsonResult SendMailInvoiceReport(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.GetLotWiseTaxInvoiceServiceReporttoMail(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);
            return Json(new { Message = "Mail send successfully." });
        }

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
            //_salesReportService.CommercialInvoicePackingListService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult LRDraft(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.LRDraftService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult BillofExchange(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.BillofExchange(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult CertificateofOrigin(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.CertificateofOrigin(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult InsuranceCoverLetter(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.InsuranceCoverLetter(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult ANNEXUREReport(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.ANNEXUREReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult BeneficiaryCertificate(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.BeneficiaryCertificate(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult BankLatter(string salesId, string BankName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.BankLatter(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId, BankName);
            return View();
        }
        [Authorize, HttpGet]
        public ActionResult CommercialInvoicePackingList(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.CommercialInvoicePackingListService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, identity.Name, salesId);

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
        public ActionResult GetPackingSalesDetailDataBySales(string salesId, string packingId, string smIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetPackingSalesMaterialData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId, packingId, smIds), JsonRequestBehavior.AllowGet);
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
            , List<Dictionary<string, object>> taxList, List<Dictionary<string, object>> itemScanCildList, Dictionary<string, object> ItemScandata, List<Dictionary<string, object>> itemScanCildNewList)
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
                string _id = InsertSalesReturn(data, detaildataList, taxList, itemScanCildList, ItemScandata, itemScanCildNewList);
                return Json(new { Id = _id, Message = string.Format(AplosMessage.Success + " Sales Return No <b>" + _id + "</b>") });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string InsertSalesReturn(Dictionary<string, object> data, List<Dictionary<string, object>> detaildataList, List<Dictionary<string, object>> taxList
            , List<Dictionary<string, object>> itemScanCildList, Dictionary<string, object> ItemScandata, List<Dictionary<string, object>> itemScanCildNewList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsDetail;
            DataSet dstax;
            DataSet dsitemscanChild;
            DataSet dsitemscanChildNew;
            DataSet dsitemscanNew;
            string TableName = "dbo.ItemScanChild";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                string sqlmaster = "SELECT * FROM [TRN].[SalesReturn] WHERE Id='" + data["SalesId"].ToString() + "'";
                string sqlDetail = "SELECT * FROM [TRN].[SalesReturnDetail] WHERE SalesId='" + data["SalesId"].ToString() + "'";
                string taxsql = "SELECT * FROM [TRN].[SalesReturnTax] WHERE SalesId='" + data["SalesId"].ToString() + "'";
                string itemScanChildsql = "SELECT * FROM dbo.ItemScanChild WHERE SalesId='" + data["SalesId"].ToString() + "'";
                string itemScansql = "SELECT * FROM dbo.ItemScan WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlmaster, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(sqlDetail, out dsDetail, false, "1");
                objCon.OpenDataSetThroughAdapter(taxsql, out dstax, false, "1");
                objCon.OpenDataSetThroughAdapter(itemScanChildsql, out dsitemscanChild, false, "1");
                objCon.OpenDataSetThroughAdapter(itemScansql, out dsitemscanNew, false, "1");
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
                                if (dsitemscanNew.Tables[0].Rows.Count == 0)
                                {
                                    DataRow dris = dsitemscanNew.Tables[0].NewRow();
                                    bplib.clsGenID id = new bplib.clsGenID();
                                    id.GenIDYearly(DateTime.Now.ToShortDateString(), "Item Scan", out string NewId);

                                    dris["Id"] = NewId;
                                    dris["WorkDate"] = ItemScandata["WorkDate"].ToString();
                                    dris["Time"] = Convert.ToDateTime(ItemScandata["WorkDate"].ToString() + " " + DateTime.Now.ToString("HH:mm:ss"));
                                    dris["ShiftId"] = ItemScandata["ShiftId"].ToString();
                                    dris["LocMasterId"] = ItemScandata["LocMasterId"].ToString();
                                    dris["PurposeId"] = ItemScandata["PurposeId"].ToString();
                                    dris["Grade"] = ItemScandata["Grade"].ToString();
                                    dris["AddedBy"] = identity.Name;
                                    dris["AddedDate"] = DateTime.Now;
                                    dsitemscanNew.Tables[0].Rows.Add(dris);
                                }

                                string _ItemScanId = dsitemscanNew.Tables[0].Rows[0]["Id"].ToString();


                                foreach (var scitemNew in itemScanCildNewList.Where(r => r["SalesId"].ToString() == item["SalesId"].ToString()
                                    && r["ActualPackingId"].ToString() == item["PackingId"].ToString()
                                    && r["SalesOrderId"].ToString() == item["SalesOrderId"].ToString()))
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
                                        scitemNew["MasterId"] = _ItemScanId;
                                        scitemNew["IsReturn"] = true;
                                        scitemNew["SalesMaterialId"] = DBNull.Value;
                                        scitemNew["LocMasterId"] = data["LocMasterId"].ToString();
                                        scitemNew["NetWeight"] = scitemNew["ReturnQty"];
                                        scitemNew["PackingId"] = DBNull.Value;
                                        AddNewRowD(dsitemscanChildNew.Tables[0], scitemNew);
                                    }
                                }
                            }

                        }

                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail, dstax, dsitemscanChild, dsitemscanNew, dsitemscanChildNew);
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


        [HttpGet, Authorize]
        public JsonResult GetSalesReturnLocationCbo()
        {
            return Json(GetSalesReturnLocationData(), JsonRequestBehavior.AllowGet);
        }
        public List<Dictionary<string, object>> GetSalesReturnLocationData()
        {
            string sql = "";
            sql = @"SELECT MMM.ToLocation [TEXT],MMM.Id [Value],MMM.PurposeId from MST.MaterialMovementMaster MMM 
                        LEFT JOIN HKP.MaterialMovementPurpose MMP ON MMP.Id=MMM.PurposeId
                        WHERE MMP.UserName='Sales Return'";
            return _sqlRepository.GetDataCollection(sql);

        }

        #endregion

        #region Sales Return Post
        public ActionResult SalesReturnPost()
        {
            return View("~/Areas/SalesManagements/Views/SalesReturnPost.cshtml");
        }

        [Authorize, HttpPost]
        public JsonResult GetSalesReturnPostedList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetSalesReturnPostedData(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
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

        [Authorize, HttpPost]
        public JsonResult GetCreditNoteAdditionalTaxDetail(string additionalTaxId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetCreditNoteAdditionalTaxDetail(additionalTaxId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetSalesReturnTaxDetail(string salesId)
        {
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetMaterialSalesTaxDetail(salesId), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpGet]
        public JsonResult GetSalesReturnJournal(string salesReturnId, string customerId, string taxApplicable)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetSalesReturnJournalData(identity.CompanyId, identity.PlantId, salesReturnId, customerId, taxApplicable), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesReturnDetailGLUpdateData(string salesReturnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsSalesService accountsSalesService = new AccountsSalesService(_sqlRepository);
            return Json(accountsSalesService.GetSalesReturnDetailGLUpdateData(identity.CompanyId, identity.PlantId, salesReturnId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertSalesReturnCreditNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, List<Dictionary<string, object>> salesReturnDetailList, IEnumerable<InvoiceTaxViewModel> tdsTaxList)
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
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, PostSalesReturn(voucherVM, voucherDetailVMList, salesReturnDetailList, tdsTaxList)) });
        }
        public string PostSalesReturn(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, List<Dictionary<string, object>> salesReturnDetailList, IEnumerable<InvoiceTaxViewModel> additionalTaxList)
        {
            try
            {
                var flag = false;
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                AccountCommonExtensionService _accountsCommonExtensionService = new AccountCommonExtensionService();

                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                DataSet _ajNDetailData = null;
                DataSet _invTaxDetailData = null;
                DataSet _invTaxDetailCrData = null; DataSet _adTaxDetailCrData = null;
                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                DataSet _salesReturnData = null;
                DataSet _iTaxDrdataset = null;
                DataSet _iTaxCrdataset = null; DataSet _aTaxCrdataset = null;
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
                    SettlementType = voucherVM.SettlementType,
                    SalesReturnId = voucherVM.SalesReturnId
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
                                AType = "Dr",
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
                            TrnNature = TransactionNature.Sales.ToString(),
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
                            AdjustmentNoteId = adjustmentNote.Id,
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
                            AType = "Cr",
                            AddedBy = invoiceTax.AddedBy,
                            AddedDate = invoiceTax.AddedDate,
                            AddedFromIP = invoiceTax.AddedFromIP
                        };
                        _accountsCommonService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, ref _invTaxDetailCrData);
                    }
                }
                if (null != additionalTaxList && additionalTaxList.Count() > 0)
                {
                    var tdsTax = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = additionalTaxList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = additionalTaxList.Sum(r => r.TaxAutoAmount),
                        AdjustmentNoteId = adjustmentNote.Id,
                        PartyId = voucherVM.PlantId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        //InvoiceId = invoice.Id,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _accountsCommonService.InsertAddtionalTax(adjustmentNote, tdsTax, ref _aTaxCrdataset);

                    int addtionalTaxDetailId = 0;
                    foreach (var tdsTaxVM in additionalTaxList)
                    {

                        if (null == tdsTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _accountsCommonExtensionService.GetTaxCodeGL(tdsTaxVM.TaxCodeId);


                        addtionalTaxDetailId++;
                        var tdsTaxDetail = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                            BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                            ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                            Amount = tdsTaxVM.TaxAmount,
                            AdditionalTaxId = tdsTax.Id,
                            TaxCodeId = tdsTaxVM.TaxCodeId,
                            TaxCategoryId = tdsTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id = _accountsCommonService.MakePK(tdsTax.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _accountsCommonService.InsertAddtionalTaxDetail(tdsTax, tdsTaxDetail, ref _adTaxDetailCrData);

                    }
                }




                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _ANdataset, _ajNDetailData, _crvDetailData, _crvDetailCurrencyData, _iTaxDrdataset, _invTaxDetailData, _drvDetailData, _drvDetailCurrencyData, _iTaxCrdataset, _invTaxDetailCrData, _salesReturnData, dsitemscanChild, _aTaxCrdataset, _adTaxDetailCrData);
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
        public JsonResult GetPostedMasterOrderSalesList(string column, string value, string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(clsSales.GetMasterOrderSalesPostedList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, FromDate, ToDate), JsonRequestBehavior.AllowGet);
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
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalesAdditionalInfo Where SalesId='" + salesId + "'", out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        if (Convert.ToBoolean(item["Flag"]) == true)
                        {
                            DataView dv = new DataView(dsMaster.Tables[0]);
                            dv.RowFilter = "AdditionalInfoId='" + item["AdditionalInfoId"] + "' AND SalesId='" + item["SalesId"] + "' ";


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


        [HttpPost, Authorize]
        public ActionResult GetInvoiceReport(ReportFormat reportFormat, string Ids)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var reportFileName = "Invoice Report";
                var fileName = GetInvoiceDataReport(reportFileName, Ids);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GetInvoiceDataReport(string SheetName, string Ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder, dtParameter;
                clsSales.GetMasterData(Ids, out dtOrder);
                Dictionary<string, InvoiceParameter> shtListNew = null;
                Dictionary<string, List<DataRow>> dicParameter = clsSales.GetParameterData(Ids, out dtParameter);
                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                int ROW = 6; int COL = 1;

                int endGenericColumn = 0;

                #region ColumnsHeader

                sheet[ROW, COL].Text = "InvoiceNo"; sheet[ROW, COL].ColumnWidth = 16; int colInvoiceNo = COL; COL++;
                sheet[ROW, COL].Text = "VoucherNo"; sheet[ROW, COL].ColumnWidth = 16; int colVoucherNo = COL; COL++;
                sheet[ROW, COL].Text = "PartyCode"; sheet[ROW, COL].ColumnWidth = 16; int colPartyCode = COL; COL++;
                sheet[ROW, COL].Text = "Party"; sheet[ROW, COL].ColumnWidth = 16; int colParty = COL; COL++;
                sheet[ROW, COL].Text = "Party Type"; sheet[ROW, COL].ColumnWidth = 35; int colPT = COL; COL++;
                sheet[ROW, COL].Text = "BillTo"; sheet[ROW, COL].ColumnWidth = 25; int colBillTo = COL; COL++;
                sheet[ROW, COL].Text = "DocRefNo"; sheet[ROW, COL].ColumnWidth = 12; int colDocRefNo = COL; COL++;
                sheet[ROW, COL].Text = "Currency"; sheet[ROW, COL].ColumnWidth = 12; int colCurrency = COL; COL++;
                sheet[ROW, COL].Text = "Amount"; sheet[ROW, COL].ColumnWidth = 8; int colAmount = COL;
                endGenericColumn = COL;

                CreateDynamicSHead(dtParameter, ref sheet, ref ROW, ref COL, ref colAmount, out shtListNew);

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                #endregion columns

                ROW++;
                int startRow = ROW;

                #region DataPlot
                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    sheet[ROW, colInvoiceNo].Text = dtOrder.Rows[i]["InvoiceNo"].ToString();
                    sheet[ROW, colVoucherNo].Text = dtOrder.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colPartyCode].Text = dtOrder.Rows[i]["PartyCode"].ToString();
                    sheet[ROW, colParty].Text = dtOrder.Rows[i]["PartyName"].ToString();
                    sheet[ROW, colPT].Text = dtOrder.Rows[i]["PartyAccountGroup"].ToString();
                    sheet[ROW, colBillTo].Text = dtOrder.Rows[i]["BillTo"].ToString();
                    sheet[ROW, colDocRefNo].Text = dtOrder.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, colCurrency].Text = dtOrder.Rows[i]["CurrencyCode"].ToString();
                    sheet[ROW, colAmount].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["Amount"].ToString());

                    if (dicParameter.ContainsKey(dtOrder.Rows[i]["InvoiceNo"].ToString()))
                    {
                        List<DataRow> drSalaryHeadCollection = dicParameter[dtOrder.Rows[i]["InvoiceNo"].ToString()];
                        for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                        {
                            try
                            {
                                InvoiceParameter xx = shtListNew[drSalaryHeadCollection[CI]["AdditionalInfoId"].ToString()];
                                if (xx != null)
                                {
                                    if (xx.CharecterType == "Decimal")
                                    {
                                        sheet.Range[ROW, xx.XLColIndex].Number = Library.Security.Core.clsStaticInfo.dbl(drSalaryHeadCollection[CI]["Value"].ToString());
                                        sheet.Range[ROW, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet.Range[ROW, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                    else
                                    {
                                        sheet.Range[ROW, xx.XLColIndex].Text = drSalaryHeadCollection[CI]["Value"].ToString();
                                        sheet.Range[ROW, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }

                        }
                    }

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                #endregion

                #region ReportHeader
                IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Production Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;
                #endregion

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
                //return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateDynamicSHead(DataTable dtSalaryHead, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out Dictionary<string, InvoiceParameter> list)
        {
            try
            {
                list = new Dictionary<string, InvoiceParameter>();
                int countGrossPostion = 0;


                xlsCol += 0;
                countGrossPostion++;

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    xlsCol++;
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["UserName"].ToString().Trim().Length > 0)
                    {

                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["UserName"].ToString();
                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;

                        InvoiceParameter HeadSequence = new InvoiceParameter();

                        HeadSequence.AdditionalInfoId = dtSalaryHead.Rows[ci]["AdditionalInfoId"].ToString();
                        HeadSequence.UserName = dtSalaryHead.Rows[ci]["UserName"].ToString();
                        HeadSequence.CharecterType = dtSalaryHead.Rows[ci]["CharecterType"].ToString();

                        HeadSequence.XLColIndex = ColGrs + countCTCPosition;

                        list.Add(dtSalaryHead.Rows[ci]["AdditionalInfoId"].ToString(), HeadSequence);
                        countCTCPosition++;



                    }//Parameter 
                    #endregion

                }//for

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region InputCredit

        [HttpPost, Authorize]
        public ActionResult GetInputCreditList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT IC.*,EI.EmployeeName ResponsiblePerson FROM HKP.InputCredit IC
			LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IC.ResponsiblePersonId) AS TEMP WHERE " + strkey + " order by AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetInputCreditCheckList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT TOP 100 * FROM (SELECT IC.*,EI.EmployeeName ResponsiblePerson FROM HKP.InputCredit IC
			LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IC.ResponsiblePersonId
			Where CheckById='" + identity.EmployeeId + "' AND	IC.CheckByStatus='To Be Checked') AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetInputCreditApproveList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT TOP 100 * FROM (SELECT IC.*,EI.EmployeeName ResponsiblePerson FROM HKP.InputCredit IC
			LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IC.ResponsiblePersonId
			Where ApproveById='" + identity.EmployeeId + "' AND IC.CheckByStatus='Checked' AND IC.ApprovedStatus='To Be Approve') AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateInputCredit(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from HKP.InputCredit where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from HKP.InputCredit where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from HKP.InputCredit where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("InputCredit", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }


        public ActionResult DeleteInputCredit(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from HKP.InputCredit where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.InputCredit");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [Authorize, HttpGet]
        public JsonResult GetCheckByCbo()
        {
            var sql = @"select E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text from dbo.AuthorizationConfig  A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where E.EmployeeStatus='Active' AND A.ActionStatus='InputCreditCheckedBy'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetApprovedByCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text from dbo.AuthorizationConfig  A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where E.EmployeeStatus='Active' AND A.ActionStatus='InputCreditApproveBy'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesMaterialDataList(string fromDate, string toDate, string inputCreditId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsSales.GetSalesMaterialDataList(identity.PlantId, fromDate, toDate, inputCreditId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaggedSalesMaterialDataList(string inputCreditId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsSales.GetTaggedSalesMaterialDataList(inputCreditId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveTagWithInputCredit(List<Dictionary<string, object>> data, string inputCreditId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsChild, dsInvMat;
            string id = string.Empty;
            string inid = string.Empty;
            try
            {
                #region Sales 
                objCon = new ConnectionManager.DAL.ConManager("1");
                foreach (var item in data.Where(r => r["SourceType"].ToString() == "SalesMaterial"))
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }
                foreach (var item in data.Where(r => r["SourceType"].ToString() == "InventorySales"))
                {
                    if (inid == "")
                        inid = "'" + item["Id"] + "'";
                    else
                        inid = inid + ",'" + item["Id"] + "'";
                }
                string mosql = "SELECT * FROM TRN.SalesMaterial WHERE Id IN (" + id + ")";
                string invsql = "SELECT * FROM TRN.InventorySalesDetail WHERE Id IN (" + inid + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter(invsql, out dsInvMat, false, "1");
                foreach (var item in data.Where(r => r["SourceType"].ToString() == "SalesMaterial"))
                {
                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["InputCreditId"] = inputCreditId;
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }

                foreach (var item in data.Where(r => r["SourceType"].ToString() == "InventorySales"))
                {
                    DataView dv = new DataView(dsInvMat.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["InputCreditId"] = inputCreditId;
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsChild, dsInvMat);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult CreateCheckBy(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from HKP.InputCredit where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    data["CheckByStatus"] = "Checked";
                    data["ApprovedStatus"] = "To Be Approve";
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateApproveBy(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from HKP.InputCredit where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    data["ApprovedStatus"] = "Approved";
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        #endregion

        #region SalesProcess

        [HttpPost, Authorize]
        public ActionResult GetSalesProcessList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT IC.* FROM HKP.SalesProcessMaster IC) AS TEMP WHERE " + strkey + " order by AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalesProcessTransactionList()
        {
            return Json(clsSales.GetSalesProcessTransactionList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBankMaster()
        {
            return Json(clsSales.GetBankMaster(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetDepartment()
        {
            return Json(clsSales.GetDepartment(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSalesProcessAutoSequence()
        {
            return Json(GetSalesProcessSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateSalesProcess(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
              

                con.OpenDataSetThroughAdapter("select * from HKP.SalesProcessMaster where SalesProcess='" + data["SalesProcess"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Sales Process already exists!!!");


                con.OpenDataSetThroughAdapter("select * from HKP.SalesProcessMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SalesProcessMaster", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Sequence = GetSalesProcessSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }


        public ActionResult DeleteSalesProcess(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from HKP.SalesProcessMaster where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSalesProcessSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        private double GetSalesProcessSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.SalesProcessMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

      

        #endregion
    }

    public class InvoiceParameter
    {
        public string Invoiced { get; set; }
        public string AdditionalInfoId { get; set; }
        public string CharecterType { get; set; }
        public string UserName { get; set; }
        public string Value { get; set; }
        public int XLColIndex { get; set; }
    }
}