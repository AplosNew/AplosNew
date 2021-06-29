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
            return Json(new { Data= voucherVM, Message = AplosMessage.Insert + "Invoice No: " + voucherVM.Id + "" });
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

            return Json(new { Data = voucherVM, Message = AplosMessage.Updated });
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

        [HttpPost]
        public JsonResult SalesInvoicePost(VoucherViewModel sales,IEnumerable<VoucherDetailViewModel> salesJVDetail
            , IEnumerable<SalesMaterialViewModel> salesDetailList, IEnumerable<SalesServiceViewModel> salesServiceDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sales.CompanyGroupId = identity.CompanyGroupId;
            sales.CompanyId = identity.CompanyId;
            sales.PlantId = identity.PlantId;
            sales.PostingDate = sales.PostingDate;
            if (salesJVDetail != null)
            {
                if (salesJVDetail.Where(r => r.TrnType == "Dr").Sum(r => r.Amount) != salesJVDetail.Where(r=>r.TrnType=="Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount is not match!");
                foreach (var item in salesJVDetail)
                {
                    if (item.GLGeneralInfoId ==null)
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

            _salesReportService.GetSalesWordReportService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

            return View();
        }


        #endregion


        [Authorize, HttpGet]
        public ActionResult LocalTaxInvoice(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.LocalTaxInvoiceService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, salesId);

            return View();
        }

        [Authorize, HttpGet]
        public ActionResult CommercialInvoice(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salesReportService.CommercialInvoiceService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, salesId);

            return View();
        }


        #endregion


        [HttpGet, Authorize]
        public ActionResult SalesReceivableReport(ReportFormat reportFormat, string voucherId)
        {
            AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService,_plantService);
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
            return Json(new { Data = voucherVM, Message = AplosMessage.Updated });
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
            return Json(_accountsSalesService.GetMasterOrderSalesPostedList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value), JsonRequestBehavior.AllowGet);
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
    }
}