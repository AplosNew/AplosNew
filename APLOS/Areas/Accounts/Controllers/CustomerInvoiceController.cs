using Aplos.Areas.Commercial.Controllers;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Accounts;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Payments;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Invoices;
using Library.Service.Organizations;
using Library.ViewModel.Accounts;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CustomerInvoiceController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;

        public CustomerInvoiceController(
            ISqlRepository sqlRepository
              )
        {
            _sqlRepository = sqlRepository;
        }


        //public ActionResult CustomerInvoice()
        //{
        //    return View("~/Areas/Accounts/Views/CustomerInvoice.cshtml");
        //}


       


        [HttpGet, Authorize]
        public JsonResult GetCustomerAvailableInvoiceList(GridParameter parameters, string partyId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetCustomerAvailableInvoiceList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCustomerAvailableReceivableData()
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetCustomerAvailableInvoiceList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetCustomerAllReceivableData(string column, string value)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetCustomerAllInvoiceList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetCustomerAvailableInvoiceNewList(GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetCustomerAvailableInvoiceList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderPopUp()
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_accountsInvoiceService.GetMasterOrderList(identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet, Authorize]
        public ActionResult GetMasterOrderListByPartyId(string partyId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_accountsInvoiceService.GetMasterOrderListByPartyId(identity.CompanyId, identity.PlantId, partyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet, Authorize]
        public JsonResult GetInvoiceSalesAvailable(string voucherId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoiceSalesAvailable(voucherId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetInvoicePurchasesAvailable(string voucherId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoicePurchasesAvailable(voucherId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetInvoiceTaxAvailable(string invoiceId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoiceTaxAvailable(invoiceId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSaleTypeGLBudget(string saleTypeId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetSaleTypeGLBudget(saleTypeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetVoucherGLBudget(string voucherId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetVoucherGLBudget(voucherId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCustomerInvoiceList(GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.InvoiceQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerInvoice), JsonRequestBehavior.AllowGet);
        }

      

        //[HttpGet, Authorize]
        //public ActionResult CustomerReceipt()
        //{
        //    return View("~/Areas/Accounts/Views/CustomerReceipt.cshtml");
        //}

        //[HttpGet, Authorize]
        //public JsonResult GetCustomerReceiptList(GridParameter parameters)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_invoiceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerReceipt), JsonRequestBehavior.AllowGet);
        //}

      


        //[HttpGet, Authorize]
        //public ActionResult CustomerInvoiceReceiveReport(string voucherId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var workbook = _invoiceReportService.GetCustomerInvoiceReceive(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
        //    workbook.SaveAs(DateTime.Now.ToString("yyMMdd") + " Payment Receipt Voucher.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
        //    return null;
        //}

        [HttpGet, Authorize]
        public ActionResult CustomerInterPlantCompanyReceipt()
        {
            return View();
        }




        #region Customer Invoice Receipt


        //public ActionResult CustomerInvoiceReceipt()
        //{
        //    return View("~/Areas/Accounts/Views/CustomerInvoiceReceipt.cshtml");
        //}

        [Authorize, HttpGet]
        public JsonResult GetCustomerInvoiceReceiptList(GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.CustomerInvoiceReceipt(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerReceipt), JsonRequestBehavior.AllowGet);
        }



        #endregion
        #region Customer Invoice Banks Receipt

        //public ActionResult CustomerInvoiceBanksReceipt()
        //{
        //    return View("~/Areas/Accounts/Views/CustomerInvoiceBanksReceipt.cshtml");
        //}

        //[Authorize, HttpGet]
        //public JsonResult GetCustomerInvoiceBanksQueryList(GridParameter parameters)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_invoiceWriteOffService.CustomerInvoiceBanksQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerBanksReceipt), JsonRequestBehavior.AllowGet);
        //}


        #endregion


    }
}