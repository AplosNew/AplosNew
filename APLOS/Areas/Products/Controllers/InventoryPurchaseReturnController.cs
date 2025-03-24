using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.MaterialManagement.Inventory;
using Library.Service.Invoices;
using Library.ViewModel.Invoices;
using Library.ViewModel.Materials;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Products.Controllers
{
    public class InventoryPurchaseReturnController : BaseController
    {
        #region Constructor


        
        private readonly IInventoryReceiveService _inventoryReveiveService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<InventoryReceiveDetail> _receiveDetailRepository;  
        private readonly IRepositoryAsync<PurchaseReturnDetail> _PurchaseReturnDetailRepository;
        private readonly IInventoryPayableService _inventoryPayableService;

        public InventoryPurchaseReturnController(IInventoryReceiveService inventoryReveiveService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseReturnDetail> PurchaseReturnDetailRepository
            , IInventoryPayableService inventoryPayableService
            )

        {
            _inventoryReveiveService = inventoryReveiveService;
            _sqlRepository = sqlRepository;
            _PurchaseReturnDetailRepository =PurchaseReturnDetailRepository;
            _inventoryPayableService = inventoryPayableService;

        }

        #endregion Constructor

        #region Aplos
      
       
        public ActionResult InventoryPurchaseReturnPost()
        {
            return View("~/Areas/Products/Views/InventoryPurchaseReturn/InventoryPurchaseReturnPost.cshtml");

        }

        [Authorize, HttpGet]
        public JsonResult GetPurchaseReturnPostedData(GridParameter parameters)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetPurchaseReturnPostedData(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.InventoryReturnPayable), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPurchaseReturnPostableData()
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetPurchaseReturnPostableData(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetGetPurchaseReturnMaterialList(GridParameter parameters, string purchaseReturnId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetPurchaseReturnMaterial(parameters,identity.CompanyId,identity.PlantId, purchaseReturnId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGetPurchaseReturnServiceList(GridParameter parameters, string purchaseReturnId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetPurchaseReturnService(parameters, identity.CompanyId, identity.PlantId, purchaseReturnId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPurchaseReturnMaterialPayable(string purchaseReturnId,bool isTaxApplicable, bool isDebitNote)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            if(!isTaxApplicable)
            return Json(_accountsInventoryPayableService.GetPurchaseReturnMaterialPayable(identity.CompanyId, identity.PlantId, purchaseReturnId, isDebitNote), JsonRequestBehavior.AllowGet);
            else
            return Json(_accountsInventoryPayableService.GetPurchaseReturnMaterialRCMPayable(identity.CompanyId, identity.PlantId, purchaseReturnId, isDebitNote), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult InsertPurchaseReturnPayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList, bool isDebitNote)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = false;
            voucherVM.SourceType = SourceType.InventoryReturnPayable.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _inventoryPayableService.InsertPurchaseReturnPayable(voucherVM, voucherDetailVMList, invoiceTaxVMList, isDebitNote)) });
        }
        [HttpGet, Authorize]
        public ActionResult GetPurchaseReturnReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var workbook = accountsInvoiceReportService.GetInventoryReturnPayableReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.InventoryReturnPayable);
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
        #endregion Aplos

        #region purchase-return

        #endregion purchase-return



    }
}