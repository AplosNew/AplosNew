using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Accounts;
using Library.Model.Enums;
using Library.Service.Invoices;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.SalesManagements.Controllers
{
    public class SalesInvoiceController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly AccountsInvoiceService _accountsInvoiceService;

        public SalesInvoiceController(
              IInvoiceService invoiceService, AccountsInvoiceService accountsInvoiceService)
        {
            _invoiceService = invoiceService;
            _accountsInvoiceService = accountsInvoiceService;
        }

        [Authorize, HttpGet]
        public ActionResult SalesInvoice()
        {
            return View("~/Areas/SalesManagements/Views/SalesInvoice.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerInvoiceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.InvoiceQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerInvoice), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertCustomerInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList, OtherInvoice otherInvoiceVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (!voucherVM.IsExcludingTax && voucherVM.Amount != voucherDetailVMList.Sum(r => r.TotalAmount))
                throw new CustomException("Total Amount and Invoice Amount not match!");
            else if (voucherVM.IsExcludingTax && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Total Amount and Invoice Amount not match!");
            voucherVM.SourceType = SourceType.CustomerInvoice.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceService.InsertCustomerInvoice(voucherVM, voucherDetailVMList, taxDetailVMList, otherInvoiceVM)) });
        }

        [HttpPost]
        public ActionResult UpdateCustomerInvoice()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostCustomerInvoice(string invoiceId)
        {
            _invoiceService.Post(invoiceId);
            return Json(new { Message = AplosMessage.Posted });
        }
    }
}