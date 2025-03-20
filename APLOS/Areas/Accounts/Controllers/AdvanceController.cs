using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Service.Advances;
using Library.Service.Banks;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class AdvanceController : BaseController
    {
        

        #region Constructor

        private readonly IAdvanceService _advanceService;
        private readonly IAdvanceWriteOffService _advanceWriteOffService;
        private readonly IAdvanceReportService _advanceReportService;
        private readonly IBankChargeService _bankChargeService;

        private readonly ISqlRepository _sqlRepository;

        private readonly IEmployeeReportService _employeeReportService;
        private readonly AccountVoucherReportService _accountVoucherReportService;

        public AdvanceController(IAdvanceService advanceService
            ,IAdvanceWriteOffService advanceWriteOffService
            , IAdvanceReportService advanceReportService
            , IBankChargeService bankChargeService

            , ISqlRepository sqlRepository
            , IEmployeeReportService employeeReportService
            , AccountVoucherReportService accountVoucherReportService)
        {
            _advanceService = advanceService;
            _advanceWriteOffService = advanceWriteOffService;
            _advanceReportService = advanceReportService;
            _bankChargeService = bankChargeService;

            _sqlRepository = sqlRepository;
            _employeeReportService = employeeReportService;
            _accountVoucherReportService = accountVoucherReportService;
        }

        #endregion Constructor

        [Authorize, HttpGet]
        public JsonResult GetAdvance(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetById(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAdvanceDetail(string advanceId)
        {
            return Json(_advanceService.GetDetail(advanceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerTotalAdvanceAmount(string partyId, string partyPlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetCustomerTotalAdvanceAmount(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, partyPlantId), JsonRequestBehavior.AllowGet);
        }

        #region CustomerAdvance

        
        public ActionResult CustomerAdvance()
        {
            return View("~/Areas/Accounts/Views/CustomerAdvance.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerAdvanceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvance), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkCustomerAdvance(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList
            , IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            if (advanceVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (advanceVM.PaymentSource != "MultiBank" && (advanceVM.Amount < 0 || advanceVM.Amount == 0))
                throw new CustomException("Please Input Amount !");
            advanceVM.SourceType = SourceType.CustomerAdvance.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            if (banksDetailVMList == null)
            {
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.InsertCustomerAdvance(advanceVM, advanceDetailVMList)) });
            }
            else
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.InsertMultiBankCustomerAdvance(advanceVM, advanceDetailVMList, banksDetailVMList, bankChargeDetailVMList)) });
        }

        [HttpPost,Authorize]
        public JsonResult ParkCustomerAdvanceBankReconcile(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            if (advanceVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (advanceVM.Amount < 0 || advanceVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            advanceVM.SourceType = SourceType.CustomerAdvance.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.InsertCustomerAdvance(advanceVM, advanceDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateCustomerAdvance(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> currencyList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.CustomerAdvance.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.UpdateCustomerAdvance(advanceVM, advanceDetailVMList, null,null)) });
        }

        [HttpPost]
        public JsonResult PostCustomerAdvance(string advanceId,string advanceGroupNo, VoucherViewModel voucherVM)
        {
            if(!string.IsNullOrEmpty(advanceId))
            _advanceService.Post(advanceId, voucherVM);
            else
                _advanceService.PostCustomerAdvanceGroupWise(advanceGroupNo);

            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult UnPostCustomerAdvance(string advanceId)
        {
            _advanceService.UnPost(advanceId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult ReportCustomerAdvance(ReportFormat reportFormat, string voucherId,string advanceGroupNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(advanceGroupNo))
            {
                var workbook = _advanceReportService.GetAdvanceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CustomerAdvance);
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
            else
            {
                AccountsInvoiceReportService _accInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
                var workbook = _accInvoiceReportService.GetCustomerAdvanceGroupReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, advanceGroupNo, SourceType.CustomerAdvance.ToString());
                switch (reportFormat)
                {
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);
                    case ReportFormat.Pdf:
                        return RenderReportAsExcel(workbook, reportFileName);
                    default:
                        return View();
                }
            }

        }

        [Authorize, HttpGet]
        public JsonResult GetAvailableJournalCustomerAdvance(GridParameter parameters, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetAvailableJournal(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, SourceType.CustomerAdvance), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult CustomerAdvanceWriteOff()
        {
            return View("~/Areas/Accounts/Views/CustomerAdvanceWriteOff.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerAdvanceWriteOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvanceWriteOff), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkCustomerAdvanceWriteOff(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> currencyList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.CustomerAdvanceWriteOff.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            if(advanceVM.PaymentSource==PaymentSource.Bank.ToString() && advanceVM.CurrencyId!=advanceVM.BankCurrencyId && advanceVM.BankAmount==0)
                throw new CustomException("Please Input BankAmount !");
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertCustomerAdvanceWriteOff(advanceVM, advanceDetailVMList, currencyList)) });
        }

        [HttpPost]
        public JsonResult PostCustomerAdvanceWriteOff(string advanceWriteOffId)
        {
            _advanceWriteOffService.Post(advanceWriteOffId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult DeleteCustomerAdvanceWriteOff(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _advanceWriteOffService.DeleteCustomerAdvanceWriteOff(voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult ParkMultiCustomerAdvanceWriteOff(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList
            , IEnumerable<VoucherDetailViewModel> voucherDetailListNew, IEnumerable<VoucherDetailCurrencyViewModel> currencyList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.CustomerAdvanceWriteOff.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            if (advanceVM.PaymentSource == PaymentSource.Bank.ToString() && advanceVM.CurrencyId != advanceVM.BankCurrencyId && advanceVM.BankAmount == 0)
                throw new CustomException("Please Input BankAmount !");
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertMultiCustomerAdvanceWriteOff(advanceVM, advanceDetailVMList, voucherDetailListNew, currencyList)) });
        }

        [HttpPost]
        public JsonResult UpdateCustomerAdvanceWriteOff()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult ReportCustomerAdvanceWriteOff(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetCustomerAdvanceWriteOffReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerAdvanceList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetAvilabeCustomerAdvanceList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerAdvanceByCustomerList(GridParameter parameters,string CustomerId)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetAvilabeCustomerAdvanceByCustomerList(parameters, CustomerId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerAdvance(string partyId, string advanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.Query(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, advanceId, SourceType.CustomerAdvance), JsonRequestBehavior.AllowGet);
        }

        #endregion CustomerAdvance

        #region CustomerSuspense

       
        public ActionResult CustomerSuspense()
        {
            return View("~/Areas/Accounts/Views/CustomerSuspense.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerSuspenseList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerSuspense), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkCustomerSuspense(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> currencyList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.CustomerSuspense.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.InsertCrAdvance(advanceVM, advanceDetailVMList, currencyList)) });
        }

        [HttpPost]
        public JsonResult UpdateCustomerSuspense(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> currencyList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.CustomerSuspense.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.UpdateCrAdvance(advanceVM, advanceDetailVMList, currencyList)) });
        }

        [HttpPost]
        public JsonResult PostCustomerSuspense(string advanceId, VoucherViewModel voucherVM)
        {
            _advanceService.Post(advanceId, voucherVM);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult UnPostCustomerSuspense(string advanceId)
        {
            _advanceService.UnPost(advanceId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult ReportCustomerSuspense(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetAdvanceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CustomerSuspense);
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

        [Authorize, HttpGet]
        public JsonResult GetAvailableJournalCustomerSuspense(GridParameter parameters, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetAvailableJournal(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, SourceType.CustomerSuspense), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult CustomerSuspenseWriteOff()
        {
            return View("~/Areas/Accounts/Views/CustomerSuspenseWriteOff.cshtml");
        }

        [HttpGet, ChaildAction(ParentActionName = nameof(GetCustomerAdvanceWriteOffList))]
        public JsonResult GetCustomerSuspenseWriteOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerSuspenseWriteOff), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerSuspenseList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetAdvance(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerSuspense), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ReportCustomerSuspenseWriteOff(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetAdvanceSetOffReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, "Customer Suspense WriteOff", SourceType.CustomerSuspenseWriteOff);
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

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerSuspense(string partyId, string advanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.Query(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, advanceId, SourceType.CustomerSuspense), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerInterTransactionSuspenseList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetAvilabeCustomerInterTransactionList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerSuspense, PartyType.Company), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkCustomerSuspenseWriteOff(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> currencyList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.CustomerSuspenseWriteOff.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            advanceVM.SettlementType = SettlementType.SetOff.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertCustomerAdvanceWriteOff(advanceVM, advanceDetailVMList, currencyList)) });
        }

        [HttpPost]
        public JsonResult UpdateCustomerSuspenseWriteOff()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostCustomerSuspenseWriteOff(string advanceWriteOffId)
        {
            _advanceWriteOffService.Post(advanceWriteOffId);
            return Json(new { Message = AplosMessage.Success });
        }

        #endregion CustomerSuspense

        #region VendorAdvance

       
        public ActionResult VendorAdvance()
        {
            return View("~/Areas/Accounts/Views/VendorAdvance.cshtml");
        }

        [HttpPost]
        public JsonResult ParkVendorAdvance(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList
                    , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.VendorAdvance.ToString();
            advanceVM.PartyType = PartyType.Vendor.ToString();
            if ((advanceVM.PaymentSource == "Bank") && (advanceVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((advanceVM.PaymentSource == "Cash") && (advanceVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.InsertDrAdvance(advanceVM, advanceDetailVMList, bankChargeDetailVMList, taxDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateVendorAdvance(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            advanceVM.IsPark = true;
            if ((advanceVM.PaymentSource == "Bank") && (advanceVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((advanceVM.PaymentSource == "Cash") && (advanceVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _advanceService.UpdateDrAdvance(advanceVM, advanceDetailVMList, bankChargeDetailVMList, taxDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostVendorAdvance(string advanceId, VoucherViewModel voucherVM)
        {
            _advanceService.Post(advanceId,voucherVM);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult DeleteVendorAdvance(string advanceId,string voucherId,string advanceGroupNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(advanceGroupNo))
            {
            _advanceService.DeleteVendorAdvance(identity.CompanyId, identity.PlantId, voucherId);
            }
            else
            {
                _advanceService.DeleteMultiVendorAdvance(identity.CompanyId, identity.PlantId, voucherId, advanceGroupNo);

            }
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UnPostVendorAdvance(string advanceId)
        {
            _advanceService.UnPost(advanceId);
            return Json(new { Message = AplosMessage.Success });
        }

        //Vendor Advance Report
        [HttpGet, Authorize]
        public ActionResult ReportVendorAdvance(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetAdvanceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.VendorAdvance);
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


        //[HttpGet, Authorize]
        //public ActionResult GetBankJournalReport(ReportFormat reportFormat, string voucherId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var workbook = _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.BankJournal);
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcel(workbook, reportFileName);

        //        default:
        //            return View();
        //    }
        //}



        [HttpGet, Authorize]
        public JsonResult GetVendorAdvanceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorAdvance), JsonRequestBehavior.AllowGet);
        }

       
        public ActionResult VendorAdvanceWriteOff()
        {
            return View("~/Areas/Accounts/Views/VendorAdvanceWriteOff.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetVendorAdvanceWriteOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorAdvanceWriteOff), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateVendorAdvanceWriteOff()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult ReportVendorAdvanceWriteOff(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetVendorAdvanceWriteOffReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        [Authorize, HttpGet]
        public JsonResult GetAvilabeVendorAdvance(string partyId, string advanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.Query(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, advanceId, SourceType.VendorAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetVendorAvilabeAdvanceList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetAdvance(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetAvailableAdvanceByVendor(string vendorId, string partyType)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if(partyType== "VendorDebitNote")
            return Json(_accountsAdvanceService.GetAvailableAdvanceByVendor(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorAdvance, vendorId), JsonRequestBehavior.AllowGet);
            else
                return Json(_accountsAdvanceService.GetAvailableAdvanceByVendor(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvance, vendorId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetPartyWiseOutstandingAdvance(string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetPartyWiseOutstandingAdvance(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, SourceType.VendorAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerWiseOutstandingAdvance(string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetPartyWiseOutstandingAdvance(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, SourceType.CustomerAdvance), JsonRequestBehavior.AllowGet);
        }



        [Authorize, HttpGet]
        public JsonResult GetPartyWiseOutstandingDebitNote(string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetPartyWiseOutstandingDebitNote(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, SourceType.VendorAdvance), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertVendorAdvanceWriteOff(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            var CurrencyId = "";
            if (advanceDetailVMList!=null)
            {
                CurrencyId = advanceDetailVMList.FirstOrDefault().CurrencyId;
                foreach (var advanceDetailVM in advanceDetailVMList)
                {
                    if (advanceDetailVM.DrAmount == 0 || advanceDetailVM.DrAmount.ToString() == null)
                        throw new CustomException(" Amount should more than 0");
                }
            }
           
            advanceVM.SourceType = SourceType.VendorAdvanceWriteOff.ToString();
            advanceVM.PartyType = PartyType.Vendor.ToString();
            if(advanceVM.CurrencyId != CurrencyId && CurrencyId!="")
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertVendorAdvanceWriteOffDifferentCurrency(advanceVM, advanceDetailVMList)) });
            }
            else
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertVendorAdvanceWriteOff(advanceVM, advanceDetailVMList)) });
            }
            
        }

        [HttpPost]
        public JsonResult PostVendorAdvanceWriteOff(string advanceId)
        {
            _advanceWriteOffService.Post(advanceId);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion VendorAdvance

        #region EmployeeAdvance

       
        public ActionResult EmployeeAdvance()
        {
            return View("~/Areas/Accounts/Views/EmployeeAdvance.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeAdvanceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeAvilabeAdvanceList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.EmployeeAdvanceQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetEmployeeAvilabeAllAdvanceList(string column, string value)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.EmployeeAvilabeAllAdvanceQuery(column, value, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetEmployeeAvilabeAdvanceSalaryList(string column, string value)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.EmployeeAdvanceSalaryQuery(column, value, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeAvilabeTotalAdvanceList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.EmployeeTotalAdvanceQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeAvilabeAdvanceByIdList(GridParameter parameters,string employeeId)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetEmployeeAvilabeAdvanceByIdList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, employeeId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeTotalAdvanceAmount(GridParameter parameters, string employeeId)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetEmployeeWiseOutstandingAdvance(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, employeeId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeTotalAdvanceAmountByEmployeeId(GridParameter parameters, string employeeId)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.EmployeeTotalAdvanceByEmployeeIdQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvance, employeeId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAdvanceReqSchedule(string Id)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetData(Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkEmployeeAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.IsPosted = false;
            voucherVM.SourceType = SourceType.EmployeeAdvance.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            if ((voucherVM.Amount == 0) || (voucherVM.Amount <= 0))
                throw new CustomException(" Amount should more than 0");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            if (voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException(" Please Select Transaction Type");
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                advanceDetailVM.Amount = voucherVM.Amount;
                advanceDetailVM.Narration = voucherVM.Narration;
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.InsertEmployeeAdvance(voucherVM, voucherDetailVMList, advanceSalarySchedulelist, bankChargeDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateEmployeeAdvance(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<AdvanceReqSchedule> DetailsList)
        {
            try
            {
                DataSet dsMaster;
                //ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceDeduction] where AdvanceId='" + advanceVM.Id + "' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("This Advanced already used in Salary Payable");

                //advanceVM.IsPark = true;
                if ((advanceVM.Amount == 0) || (advanceVM.Amount <= 0))
                    throw new CustomException(" Amount should more than 0");
                if ((advanceVM.PaymentSource == "Bank") && (advanceVM.BankMasterId == null))
                    throw new CustomException(Resources.SelectBank);
                if ((advanceVM.PaymentSource == "Cash") && (advanceVM.CashMasterId == null))
                    throw new CustomException(Resources.SelectCash);
                if ((advanceVM.Amount == 0) || (advanceVM.Amount <= 0))
                {
                    throw new CustomException(" Amount should more than 0");
                }
                foreach (var advanceDetailVM in advanceDetailVMList)
                {
                    advanceDetailVM.Amount = advanceVM.Amount;
                    advanceDetailVM.Narration = advanceVM.Narration;
                }
                advanceVM.PartyType = PartyType.Employee.ToString();
                return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _advanceService.UpdateEmployeeAdvance(advanceVM, advanceDetailVMList, bankChargeDetailVMList, DetailsList)) });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
            
        }

        [HttpPost]
        public JsonResult PostEmployeeAdvance(string advanceId, VoucherViewModel voucherVM)
        {
            _advanceService.Post(advanceId, voucherVM);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult PostEmployeeAdvanceHR(string voucherId, string requisitionId)
        {
            _advanceService.PostEmployeeAdvanceHR(voucherId, requisitionId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteEmployeeAdvance(string advanceId, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _advanceService.DeleteEmployeeAdvance(identity.CompanyId, identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult DeleteEmployeeAdvanceHR(string employeeAdvanceId, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _advanceService.DeleteEmployeeAdvanceHR(employeeAdvanceId, voucherId);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult UnPostEmployeeAdvance(string advanceId)
        {
            _advanceService.UnPost(advanceId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetEmployeeAdvanceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.EmployeeAdvance);
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

        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceHRReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetEmployeeAdvanceHRReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.EmployeeAdvance);
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

        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceReportPortal(ReportFormat reportFormat, string employeeAdvanceRequisitionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "EmployeeAdvance" + employeeAdvanceRequisitionId + "";
            var workbook = _advanceReportService.GetEmployeeAdvanceReportPortal(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, employeeAdvanceRequisitionId);
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

        [Authorize, HttpGet]
        public JsonResult GetEmployeeAvilabePayableList(GridParameter parameters, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetEmployeeAvilabePayableList(parameters, identity.CompanyGroupId, identity.CompanyId, employeeId), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult EmployeeAdvanceWriteOff()
        {
            return View("~/Areas/Accounts/Views/EmployeeAdvanceWriteOff.cshtml");
        }
        public ActionResult EmployeeTotalAdvanceWriteOff()
        {
            return View("~/Areas/Accounts/Views/EmployeeTotalAdvanceWriteOff.cshtml");
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeAdvanceWriteOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceWriteOffService.QueryEmployee(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvanceWriteOff), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeAdvanceWriteOffDetailList(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceWriteOffService.GetEmployeeAdvanceDetail(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerInterTransactionAdvanceList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetAvilabeCustomerInterTransactionList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvance, PartyType.Company), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertEmployeeAdvanceWriteOff(VoucherViewModel voucherVM, VoucherDetailViewModel VoucherDetailVM, IEnumerable<VoucherDetailViewModel> voucherDetailList,IEnumerable<VoucherDetailViewModel> voucherDetailGLList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.EmployeeAdvanceWriteOff.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            if (voucherVM.SettlementType == SettlementType.Return.ToString())
            {
                if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.BankMasterId == null)
                    throw new CustomException(Resources.SelectBank);
                else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.CashMasterId == null)
                    throw new CustomException(Resources.SelectCash);
                if ((voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.Amount == 0 || voucherVM.Amount.ToString() == null)
                    || voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.Amount == 0 || voucherVM.Amount.ToString() == null)
                    throw new CustomException("Please Input Amount.");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertEmployeeAdvanceWriteOff(voucherVM, VoucherDetailVM, voucherDetailList, voucherDetailGLList)) });
        }
        [HttpPost]
        public JsonResult InsertEmployeeTotalAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.EmployeeAdvanceWriteOff.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            if (voucherVM.SettlementType == SettlementType.Return.ToString())
            {
                if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.BankMasterId == null)
                    throw new CustomException(Resources.SelectBank);
                else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.CashMasterId == null)
                    throw new CustomException(Resources.SelectCash);
                if ((voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.Amount == 0 || voucherVM.Amount.ToString() == null)
                    || voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.Amount == 0 || voucherVM.Amount.ToString() == null)
                    throw new CustomException("Please Input Amount.");
            }
            if (voucherVM.EmployeeTransactionTypeId == "2")
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertEmployeeAdvanceWriteOff(voucherVM, null, voucherDetailList,null)) });
            }
            else
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertEmployeeTotalAdvanceWriteOff(voucherVM, voucherDetailList)) });
            }
           
        }

        [HttpPost]
        public JsonResult UpdateEmployeeAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.EmployeeAdvanceWriteOff.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            if (voucherVM.SettlementType == SettlementType.Return.ToString())
            {
                if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.BankMasterId == null)
                    throw new CustomException(Resources.SelectBank);
                else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.CashMasterId == null)
                    throw new CustomException(Resources.SelectCash);
                if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() || voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.Amount == 0 || voucherVM.Amount.ToString() == null)
                    throw new CustomException("Please Input Amount.");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _advanceWriteOffService.UpdateEmployeeAdvanceWriteOff(voucherVM, voucherDetailList)) });
        }

        [HttpPost]
        public JsonResult PostEmployeeAdvanceWriteOff(string advanceWriteOffId)
        {
            _advanceWriteOffService.Post(advanceWriteOffId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult DeleteEmployeeAdvanceWriteOff(string advanceWriteOffId,string voucherId)
        {
            _advanceService.DeleteEmployeeAdvanceWriteOff(advanceWriteOffId, voucherId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult DeleteEmployeeTotalAdvanceWriteOff(string advanceWriteOffId, string voucherId)
        {
            _advanceService.DeleteEmployeeTotalAdvanceWriteOff(advanceWriteOffId, voucherId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceWriteOffReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetEmployeeAdvanceWriteOffReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        #endregion EmployeeAdvance

        #region EmployeeAdvanceRequisition

        [HttpGet, Authorize]
        public ActionResult EmployeeAdvanceRequisition()
        {
            return View("~/Areas/Accounts/Views/EmployeeAdvanceRequisition.cshtml");
        }
        [HttpGet]
        public ActionResult HREmployeeAdvanceRequisition()
        {
            return View("~/Areas/Accounts/Views/EmployeeAdvanceRequisitionHR.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult EmployeeAdvanceRequisitionEdit()
        {
            return View("~/Areas/Accounts/Views/EmployeeAdvanceRequisitionEdit.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult EmployeeAdvanceRequisitionApprove()
        {
            return View("~/Areas/Accounts/Views/EmployeeAdvanceRequisitionApprove.cshtml");
        }
        [HttpPost, Authorize]
        public ActionResult EmployeeAdvanceRequisitionSave(Dictionary<string, object> EmpAdvanceReqList, string plants)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceRequisition] where SystemId='" + EmpAdvanceReqList["SystemId"] + "'", out DataSet dsEmpAdvanceReq, false, "1");
             

                DataRow dr;
                string _EmpAdvanceReqId = "";

                #region task master
                if (dsEmpAdvanceReq.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Employee Advance Requisition Creation", out _EmpAdvanceReqId);
                    _EmpAdvanceReqId = _EmpAdvanceReqId.Replace("-", "").Substring(2);


                    EmpAdvanceReqList["SystemId"] = _EmpAdvanceReqId;
                    EmpAdvanceReqList["EmpSystemId"] = identity.EmployeeId;
                    if(EmpAdvanceReqList["EmpSystemId"].ToString() == EmpAdvanceReqList["CheckedBy"].ToString())
                        throw new CustomException("Checked By can not same person!");

                    AddNewRow(dsEmpAdvanceReq.Tables[0], EmpAdvanceReqList);
                }
                else
                {
                    _EmpAdvanceReqId = EmpAdvanceReqList["SystemId"].ToString();
                    EditRow(dsEmpAdvanceReq.Tables[0].Rows[0], EmpAdvanceReqList);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpAdvanceReq);





                return Json(new { Error = false, Id = _EmpAdvanceReqId , Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult HREmployeeAdvanceRequisitionSave(Dictionary<string, object> EmpAdvanceReqList, IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist)
        {
            try
            {
                if (EmpAdvanceReqList["AdvanceType"].ToString() == "Salary" && advanceSalarySchedulelist == null)
                    throw new CustomException("Please input Advance Schedule!");

                AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet _dsAdvanceReqScheduleData = null;
                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceRequisition] where SystemId='" + EmpAdvanceReqList["SystemId"] + "'", out DataSet dsEmpAdvanceReq, false, "1");

                string _EmpAdvanceReqId = "";

                #region task master
                if (dsEmpAdvanceReq.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Employee Advance Requisition Creation", out _EmpAdvanceReqId);
                    _EmpAdvanceReqId = _EmpAdvanceReqId.Replace("-", "").Substring(2);


                    EmpAdvanceReqList["SystemId"] = _EmpAdvanceReqId;
                    if (EmpAdvanceReqList["EmpSystemId"].ToString() == EmpAdvanceReqList["CheckedBy"].ToString())
                        throw new CustomException("Checked By can not same person!");

                    AddNewRow(dsEmpAdvanceReq.Tables[0], EmpAdvanceReqList);
                }
                else
                {
                    _EmpAdvanceReqId = EmpAdvanceReqList["SystemId"].ToString();
                    EditRow(dsEmpAdvanceReq.Tables[0].Rows[0], EmpAdvanceReqList);
                }
                if (advanceSalarySchedulelist != null)
                {
                    foreach (var item in advanceSalarySchedulelist)
                    {
                        var advanceReqSchedule = new AdvanceReqSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance,
                            YearNo = item.InstallmentDate.Year,
                            MonthNo = item.InstallmentDate.Month,
                            RequisitionId = _EmpAdvanceReqId
                        };
                        accountsCommonService.InsertAdvanceReqSchedule(advanceReqSchedule, EmpAdvanceReqList["SystemId"].ToString(), ref _dsAdvanceReqScheduleData);
                    }
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpAdvanceReq, _dsAdvanceReqScheduleData);
                return Json(new { Error = false, Id = _EmpAdvanceReqId, Message = "Data saved successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
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

            dr["ApprovalStatus"] = ApprovalStatus.ToBeChecked.ToString();

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (sourceData["ApprovalStatus"].ToString()!= ApprovalStatus.ToBeChecked.ToString())
            {
                throw new CustomException("Unable to update after checked.");
            }

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
            dr["ApprovalStatus"] = ApprovalStatus.ToBeChecked.ToString();
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        
        [HttpPost, Authorize]
        public ActionResult EmployeeAdvanceRequisitionCheck(Dictionary<string, object> EmpAdvanceReqList, string plants)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceRequisition] where SystemId='" + EmpAdvanceReqList["SystemId"] + "'", out DataSet dsEmpAdvanceReq, false, "1");


                DataRow dr;
                string _EmpAdvanceReqId = "";

                #region task master
                if (dsEmpAdvanceReq.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Employee Advance Requisition Creation", out _EmpAdvanceReqId);
                    _EmpAdvanceReqId = _EmpAdvanceReqId.Replace("-", "").Substring(2);


                    EmpAdvanceReqList["SystemId"] = _EmpAdvanceReqId;
                    EmpAdvanceReqList["EmpSystemId"] = identity.EmployeeId;
                    AddNewRow(dsEmpAdvanceReq.Tables[0], EmpAdvanceReqList);
                }
                else
                {
                    if (EmpAdvanceReqList["ApprovedBy"]==null)
                        throw new CustomException("Please select Approved By!");
                    _EmpAdvanceReqId = EmpAdvanceReqList["SystemId"].ToString();
                    EditCheckRow(dsEmpAdvanceReq.Tables[0].Rows[0], EmpAdvanceReqList);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpAdvanceReq);





                return Json(new { Error = false, Id = _EmpAdvanceReqId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        private void EditCheckRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr["ApprovalStatus"] = ApprovalStatus.ToBeApproved.ToString();
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }


        [HttpPost, Authorize]
        public ActionResult EmployeeAdvanceRequisitionCheckHold(Dictionary<string, object> EmpAdvanceReqList, string plants)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceRequisition] where SystemId='" + EmpAdvanceReqList["SystemId"] + "'", out DataSet dsEmpAdvanceReq, false, "1");


                DataRow dr;
                string _EmpAdvanceReqId = "";

                #region task master
                if (dsEmpAdvanceReq.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Employee Advance Requisition Creation", out _EmpAdvanceReqId);
                    _EmpAdvanceReqId = _EmpAdvanceReqId.Replace("-", "").Substring(2);


                    EmpAdvanceReqList["SystemId"] = _EmpAdvanceReqId;
                    EmpAdvanceReqList["EmpSystemId"] = identity.EmployeeId;
                    AddNewRow(dsEmpAdvanceReq.Tables[0], EmpAdvanceReqList);
                }
                else
                {
                    _EmpAdvanceReqId = EmpAdvanceReqList["SystemId"].ToString();
                    EditCheckHoldRow(dsEmpAdvanceReq.Tables[0].Rows[0], EmpAdvanceReqList);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpAdvanceReq);





                return Json(new { Error = false, Id = _EmpAdvanceReqId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        private void EditCheckHoldRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr["ApprovalStatus"] = ApprovalStatus.CheckedHolded;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }


        [HttpPost, Authorize]
        public ActionResult EmployeeAdvanceRequisitionCheckedRejected(Dictionary<string, object> EmpAdvanceReqList, string plants)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceRequisition] where SystemId='" + EmpAdvanceReqList["SystemId"] + "'", out DataSet dsEmpAdvanceReq, false, "1");


                DataRow dr;
                string _EmpAdvanceReqId = "";

                #region task master
                if (dsEmpAdvanceReq.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Employee Advance Requisition Creation", out _EmpAdvanceReqId);
                    _EmpAdvanceReqId = _EmpAdvanceReqId.Replace("-", "").Substring(2);


                    EmpAdvanceReqList["SystemId"] = _EmpAdvanceReqId;
                    EmpAdvanceReqList["EmpSystemId"] = identity.EmployeeId;
                    AddNewRow(dsEmpAdvanceReq.Tables[0], EmpAdvanceReqList);
                }
                else
                {
                    _EmpAdvanceReqId = EmpAdvanceReqList["SystemId"].ToString();
                    EditCheckedRejectedRow(dsEmpAdvanceReq.Tables[0].Rows[0], EmpAdvanceReqList);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpAdvanceReq);





                return Json(new { Error = false, Id = _EmpAdvanceReqId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        private void EditCheckedRejectedRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr["ApprovalStatus"] = ApprovalStatus.CheckedRejected;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        [HttpPost, Authorize]
        public ActionResult EmployeeAdvanceRequisitionApprove(Dictionary<string, object> EmpAdvanceReqList, string plants)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceRequisition] where SystemId='" + EmpAdvanceReqList["SystemId"] + "'", out DataSet dsEmpAdvanceReq, false, "1");


                DataRow dr;
                string _EmpAdvanceReqId = "";

                #region task master
                if (dsEmpAdvanceReq.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Employee Advance Requisition Creation", out _EmpAdvanceReqId);
                    _EmpAdvanceReqId = _EmpAdvanceReqId.Replace("-", "").Substring(2);


                    EmpAdvanceReqList["SystemId"] = _EmpAdvanceReqId;
                    EmpAdvanceReqList["EmpSystemId"] = identity.EmployeeId;
                    AddNewRow(dsEmpAdvanceReq.Tables[0], EmpAdvanceReqList);
                }
                else
                {
                    _EmpAdvanceReqId = EmpAdvanceReqList["SystemId"].ToString();
                    EditApprovedRow(dsEmpAdvanceReq.Tables[0].Rows[0], EmpAdvanceReqList);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpAdvanceReq);





                return Json(new { Error = false, Id = _EmpAdvanceReqId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        private void EditApprovedRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr["ApprovalStatus"] = ApprovalStatus.Approved.ToString();
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }


        [HttpPost, Authorize]
        public ActionResult EmployeeAdvanceRequisitionApprovedRejected(Dictionary<string, object> EmpAdvanceReqList, string plants)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceRequisition] where SystemId='" + EmpAdvanceReqList["SystemId"] + "'", out DataSet dsEmpAdvanceReq, false, "1");


                DataRow dr;
                string _EmpAdvanceReqId = "";

                #region task master
                if (dsEmpAdvanceReq.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Employee Advance Requisition Creation", out _EmpAdvanceReqId);
                    _EmpAdvanceReqId = _EmpAdvanceReqId.Replace("-", "").Substring(2);


                    EmpAdvanceReqList["SystemId"] = _EmpAdvanceReqId;
                    EmpAdvanceReqList["EmpSystemId"] = identity.EmployeeId;
                    AddNewRow(dsEmpAdvanceReq.Tables[0], EmpAdvanceReqList);
                }
                else
                {
                    _EmpAdvanceReqId = EmpAdvanceReqList["SystemId"].ToString();
                    EditApprovedRejectedRow(dsEmpAdvanceReq.Tables[0].Rows[0], EmpAdvanceReqList);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpAdvanceReq);





                return Json(new { Error = false, Id = _EmpAdvanceReqId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        private void EditApprovedRejectedRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr["ApprovalStatus"] = ApprovalStatus.ApprovedRejected;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        [HttpPost, Authorize]
        public ActionResult EmployeeAdvanceRequisitionApprovedHold(Dictionary<string, object> EmpAdvanceReqList, string plants)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceRequisition] where SystemId='" + EmpAdvanceReqList["SystemId"] + "'", out DataSet dsEmpAdvanceReq, false, "1");


                DataRow dr;
                string _EmpAdvanceReqId = "";

                #region task master
                if (dsEmpAdvanceReq.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Employee Advance Requisition Creation", out _EmpAdvanceReqId);
                    _EmpAdvanceReqId = _EmpAdvanceReqId.Replace("-", "").Substring(2);


                    EmpAdvanceReqList["SystemId"] = _EmpAdvanceReqId;
                    EmpAdvanceReqList["EmpSystemId"] = identity.EmployeeId;
                    AddNewRow(dsEmpAdvanceReq.Tables[0], EmpAdvanceReqList);
                }
                else
                {
                    _EmpAdvanceReqId = EmpAdvanceReqList["SystemId"].ToString();
                    EditApprovedHoldRow(dsEmpAdvanceReq.Tables[0].Rows[0], EmpAdvanceReqList);
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpAdvanceReq);





                return Json(new { Error = false, Id = _EmpAdvanceReqId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        private void EditApprovedHoldRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr["ApprovalStatus"] = ApprovalStatus.ApprovedHolded;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }


        [HttpGet, Authorize]
        public ActionResult EmployeeAdvanceRequisitionGetList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT  CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate,EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy  
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.EmpSystemId = '" + identity.EmployeeId+ "' AND EAR.ApprovalStatus='" + ApprovalStatus.ToBeChecked + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeCheckedDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT  CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy    FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.EmpSystemId = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.ToBeApproved + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeCheckedHoldDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT  CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy    FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.EmpSystemId = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.CheckedHolded + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeCheckedRejectDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT  CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy    FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.EmpSystemId = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.CheckedRejected + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeeApprovedDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT  CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy    FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.EmpSystemId = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.Approved + "' AND EAR.IsPost=0) AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeePostedDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT  CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy
                            , 'Posted' ApprovalStatus, EEA. EmployeeName ApprovedBy,V.VoucherNo    FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            LEFT JOIN TRN.Advance A ON A.RequisitionId=EAR.SystemId
							LEFT JOIN TRN.Voucher V ON V.Id=A.VoucherId
                            WHERE EAR.EmpSystemId = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.Approved + "' AND EAR.IsPost=1) AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeApprovedHoldDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT  CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy    FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.EmpSystemId = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.ApprovedHolded + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeApprovedRejectDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy ,EAR.AddedDate    
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.EmpSystemId = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.ApprovedRejected + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult HREmployeeAdvanceRequisitionGetList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT  CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate,EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy ,EAR.AddedDate  
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovalStatus='" + ApprovalStatus.ToBeChecked + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHREmployeeCheckedDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy ,EAR.AddedDate    
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovalStatus='" + ApprovalStatus.ToBeApproved + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHREmployeeCheckedHoldDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy ,EAR.AddedDate    
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovalStatus='" + ApprovalStatus.CheckedHolded + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHREmployeeCheckedRejectDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy ,EAR.AddedDate    
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovalStatus='" + ApprovalStatus.CheckedRejected + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetHREmployeeApprovedDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy ,EAR.AddedDate    
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovalStatus='" + ApprovalStatus.Approved + "' AND EAR.IsPost=0) AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetHREmployeePostedDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy
                            , 'Posted' ApprovalStatus, EEA. EmployeeName ApprovedBy,V.VoucherNo ,EAR.AddedDate    
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            LEFT JOIN TRN.Advance A ON A.RequisitionId=EAR.SystemId
							LEFT JOIN TRN.Voucher V ON V.Id=A.VoucherId
                            WHERE EAR.ApprovalStatus='" + ApprovalStatus.Approved + "' AND EAR.IsPost=1) AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHREmployeeApprovedHoldDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy ,EAR.AddedDate    
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovalStatus='" + ApprovalStatus.ApprovedHolded + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHREmployeeApprovedRejectDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy ,EAR.AddedDate    
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovalStatus='" + ApprovalStatus.ApprovedRejected + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceRequisitionForCheckList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy, EEA. EmployeeName ApprovedBy,EAR.ApprovalStatus ,EAR.AddedDate   
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.CheckedBy = '" + identity.EmployeeId + "'  AND EAR.ApprovalStatus='" + ApprovalStatus.ToBeChecked + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCheckedByEmployeeCheckedDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy  FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.CheckedBy = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.ToBeApproved + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCheckedByEmployeeCheckedHoldDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy   FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.CheckedBy = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.CheckedHolded + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCheckedByEmployeeCheckedRejectDataList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy  FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.CheckedBy = '" + identity.EmployeeId + "' AND EAR.ApprovalStatus='" + ApprovalStatus.CheckedRejected + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceRequisitionForArroveList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy, EEA. EmployeeName ApprovedBy,EAR.ApprovalStatus   FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovedBy = '" + identity.EmployeeId + "'  AND EAR.ApprovalStatus='" + ApprovalStatus.ToBeApproved + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceRequisitionArrovedList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy, EEA. EmployeeName ApprovedBy,EAR.ApprovalStatus   FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovedBy = '" + identity.EmployeeId + "'  AND EAR.ApprovalStatus='" + ApprovalStatus.Approved + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetApprovedByEmployeeAdvanceRequisitionHoldList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy, EEA. EmployeeName ApprovedBy,EAR.ApprovalStatus   FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovedBy = '" + identity.EmployeeId + "'  AND EAR.ApprovalStatus='" + ApprovalStatus.ApprovedHolded + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetApprovedByEmployeeAdvanceRequisitionRejectList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT* FROM(SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy, EEA. EmployeeName ApprovedBy,EAR.ApprovalStatus   FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.ApprovedBy = '" + identity.EmployeeId + "'  AND EAR.ApprovalStatus='" + ApprovalStatus.ApprovedRejected + "') AS TEMP WHERE " + strkey + " ORDER BY RequisitionAddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection(@"select EAR.*,EI.EmployeeName,DP.UserName Department from [TRN].[EmployeeAdvanceRequisition] EAR 
                                        Join dbo.EmployeeInformation EI ON EI.SystemId = EAR.EmpSystemId
                                        Join ORG.Department DP ON DP.Id = EI.DepartmentId
                                        where EAR.SystemId = '" + Id + "'");
                               
                return Json(new { master = _master}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet, Authorize]
        public ActionResult EmployeeAdvanceRequisitionDelete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from  [TRN].[EmployeeAdvanceRequisition] where SystemId='" + id + "'");
                
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceRequisitionApprovedList()
        {
           
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate, EAR.AdvanceType, EAR.EmpSystemId, EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy  
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.IsPost=0 AND EAR.ApprovalStatus='" + ApprovalStatus.Approved + "' and ISNULL(EAR.SystemId,'') not in (select ISNULL(RequisitionId,'') from [TRN].[EmployeeAdvance])";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetAdvanceReqScheduleListByRequisitionId(string requisitionId)
        {
            string sql = @"SELECT a.Id,FORMAT(a.InstallmentDate, 'dd-MMM-yyyy') InstallmentDate
                            	,a.InstallmentNo,a.InstallmentAmount,a.ProfitAmount
                            	,a.PrincipalAmount,a.Balance,a.EmployeeSalaryAdvanceId
                            	,a.YearNo,a.MonthNo,a.ScheduleNo,a.Arrear,a.RequisitionId,a.EmployeeAdvanceDetailId
                            FROM AdvanceReqSchedule a
                            LEFT JOIN [TRN].[EmployeeAdvanceRequisition] e ON e.SystemId = a.RequisitionId
                            WHERE A.RequisitionId = '" + requisitionId + "' ORDER BY a.InstallmentNo";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
         }
        #endregion

        #region EmployeeAdvanceRequisitionPost


        public ActionResult EmployeeAdvanceRequisitionPost()
        {
            return View("~/Areas/Accounts/Views/EmployeeAdvanceRequisitionPost.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeAdvanceRequisitionPostList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsSalaryPayableService = new AccountsAdvanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsSalaryPayableService.GetEmployeeAdvanceHRList(parameters, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeAvilabeAdvanceRequisitionPostList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.EmployeeAdvanceQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeTotalAdvanceRequisitionPostAmount(GridParameter parameters, string employeeId)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetEmployeeWiseOutstandingAdvance(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, employeeId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkEmployeeAdvanceRequisitionPost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.IsPosted = false;
            voucherVM.SourceType = SourceType.EmployeeAdvance.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            AccountsCommonService materialCommonService = new AccountsCommonService(_sqlRepository);
            if ((voucherVM.Amount == 0) || (voucherVM.Amount <= 0))
                throw new CustomException(" Amount should more than 0");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            if (voucherVM.EmployeeTransactionTypeId == null && voucherVM.JournalType!= AdvanceType.Salary.ToString())
                throw new CustomException(" Please Select Transaction Type");
            if (voucherDetailVMList == null && voucherVM.JournalType == AdvanceType.Salary.ToString())
                throw new CustomException(" Please Select GL");
            if (advanceSalarySchedulelist != null && voucherVM.JournalType == AdvanceType.Salary.ToString())
            {
                if (voucherVM.Amount != advanceSalarySchedulelist.Sum(x => x.InstallmentAmount))
                {
                    throw new CustomException("Advance Amount and Advance Schedule Amount Should be same!");
                }
            }
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                advanceDetailVM.Amount = voucherVM.Amount;
                advanceDetailVM.Narration = voucherVM.Narration;
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.InsertEmployeeAdvanceRequisition(voucherVM, voucherDetailVMList, advanceSalarySchedulelist, bankChargeDetailVMList)) });
        }

        [HttpPost]
        public JsonResult ParkEmployeeAdvanceRequisition(VoucherViewModel voucherVM, Dictionary<string, object> data, List<Dictionary<string, object>> advanceDetail, IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.IsPosted = false;
            voucherVM.SourceType = SourceType.EmployeeAdvance.ToString();
            voucherVM.PartyType = PartyType.Employee.ToString();
            if ((voucherVM.Amount == 0) || (voucherVM.Amount <= 0))
                throw new CustomException(" Amount should more than 0");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            if (voucherVM.EmployeeTransactionTypeId == null && voucherVM.JournalType != AdvanceType.Salary.ToString())
                throw new CustomException(" Please Select Transaction Type");
            if (advanceSalarySchedulelist != null && voucherVM.JournalType == AdvanceType.Salary.ToString())
            {
                if(voucherVM.Amount!= advanceSalarySchedulelist.Sum(x=> x.InstallmentAmount))
                {
                    throw new CustomException("Advance Amount and Advance Schedule Amount Should be same!");
                }
            }
                

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.CreateEmployeeAdvanceHRPark(voucherVM, data, advanceDetail, advanceSalarySchedulelist)) });
        }


        [HttpPost]
        public JsonResult UpdateEmployeeAdvanceRequisitionPost(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            advanceVM.IsPark = true;
            if ((advanceVM.Amount == 0) || (advanceVM.Amount <= 0))
                throw new CustomException(" Amount should more than 0");
            if ((advanceVM.PaymentSource == "Bank") && (advanceVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((advanceVM.PaymentSource == "Cash") && (advanceVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            if ((advanceVM.Amount == 0) || (advanceVM.Amount <= 0))
            {
                throw new CustomException(" Amount should more than 0");
            }
            foreach (var advanceDetailVM in advanceDetailVMList)
            {
                advanceDetailVM.Amount = advanceVM.Amount;
                advanceDetailVM.Narration = advanceVM.Narration;
            }
            advanceVM.PartyType = PartyType.Employee.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _advanceService.UpdateEmployeeAdvance(advanceVM, advanceDetailVMList, bankChargeDetailVMList,null)) });
        }

        [HttpPost]
        public JsonResult PostEmployeeAdvanceRequisitionPost(string advanceId,string voucherId)
        {
            _advanceService.PostEmployeeAdvanceRequisition(advanceId, voucherId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UnPostEmployeeAdvanceRequisitionPost(string advanceId)
        {
            _advanceService.UnPost(advanceId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeAdvanceRequisitionPostReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetEmployeeAdvanceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.EmployeeAdvance);
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
        #region WriteOff

        [Authorize, HttpGet]
        public JsonResult GetAdvanceForWriteOff(string advanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetAdvance(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, advanceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAdvanceForJournal(string advanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetAvailableJournal(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, advanceId), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult CustomerInterTransactionPending()
        {
            return View();
        }

        #endregion WriteOff

        #region InterTransaction

       
        public ActionResult InterTransaction()
        {
            return View("~/Areas/Accounts/Views/InterTransaction.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetInterTransactionList(GridParameter parameters)
        {
            AccountsAdvanceService _accountsAdvanceService = new AccountsAdvanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsAdvanceService.GetInterTransactionList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkInterTransaction(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList
            , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> NoteSetOffList
            , IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.InterPlantId = advanceVM.PlantId;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            advanceVM.IsInterTransaction = true;
            if ((advanceVM.SettlementType == SettlementType.AdvanceToEmployee.ToString()) && (advanceVM.EmployeeTransactionTypeId == null))
                throw new CustomException("Please select Employee Transaction Type!");
            if ((advanceVM.SettlementType == SettlementType.Others.ToString()) && (advanceDetailVMList == null))
                throw new CustomException("Please select GL!");
            advanceVM.SourceType = SourceType.InterTransaction.ToString();
            _advanceService.InsertInterTransaction(advanceVM, advanceDetailVMList, bankChargeDetailVMList, NoteSetOffList, voucherDetailVMList, taxDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult UpdateInterTransaction(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> currencyList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.InterTransaction.ToString();
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostInterTransaction(string advanceId, VoucherViewModel voucherVM)
        {
            _advanceService.Post(advanceId, voucherVM);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult DeleteInterTransaction(string advanceId, string voucherId)
        {
            _advanceService.DeleteInterTransaction(advanceId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult ReportInterTransaction(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetInterTransactionVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);

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

        #endregion InterTransaction

        #region PaymentReceipt

        
        public ActionResult CustomerPayment()
        {
            return View("~/Areas/Accounts/Views/CustomerPayment.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerPaymentList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceService.GetCustomerPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerReceipt), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankChargeListByAdvance(string advanceId)
        {
            return Json(_bankChargeService.GetBankChargeListByAdvance(advanceId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveCustomerPayment(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.CustomerReceipt.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceService.InsertCustomerPayment(advanceVM, advanceDetailVMList, bankChargeDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateCustomerPayment(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPosted = false;
            advanceVM.IsPark = true;
            advanceVM.SourceType = SourceType.CustomerReceipt.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _advanceService.UpdateCustomerPayment(advanceVM, advanceDetailVMList, bankChargeDetailVMList)) });
        }

        [HttpGet, Authorize]
        public ActionResult GetCustomerPaymentReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetAdvanceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CustomerReceipt);
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

       
        public ActionResult CustomerInvoiceWriteOff()
        {
            return View("~/Areas/Accounts/Views/CustomerInvoiceWriteOff.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCustomerInvoiceWriteOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvanceWriteOff), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkCustomerInvoiceWriteOff(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            if ((advanceVM.SettlementType == SettlementType.Transfer.ToString() || advanceVM.SettlementType == SettlementType.Return.ToString() || advanceVM.SettlementType == SettlementType.Charge.ToString()) && (advanceVM.Amount == 0 || advanceVM.Amount.ToString() == null))
                throw new CustomException("Amount should more than 0.");
            if (advanceVM.SettlementType == SettlementType.Return.ToString())
            {
                if (advanceVM.PaymentSource == PaymentSource.Bank.ToString() && advanceVM.BankMasterId == null)
                    throw new CustomException(Resources.SelectBank);
                if (advanceVM.PaymentSource == PaymentSource.Cash.ToString() && advanceVM.CashMasterId == null)
                    throw new CustomException(Resources.SelectCash);
            }
            if (advanceVM.SettlementType == SettlementType.InterTransaction.ToString() && advanceVM.FinancingTypeId == null)
                throw new CustomException("Please select Transaction Type.");
            if ((advanceVM.SettlementType == SettlementType.Charge.ToString()) && (advanceVM.FinancingTypeId == null))
                throw new CustomException("Please select Charges.");
            advanceVM.SourceType = SourceType.CustomerAdvanceWriteOff.ToString();
            advanceVM.PartyType = PartyType.Customer.ToString();

            if (advanceVM.SettlementType == SettlementType.SetOff.ToString())
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertCustomerInvoiceWriteOff(advanceVM, advanceDetailVMList)) });
            }
            else
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertCustomerPaymentWriteOff(advanceVM, advanceDetailVMList)) });
            }
        }

        [HttpPost]
        public JsonResult PostCustomerInvoiceWriteOff(string advanceWriteOffId)
        {
            _advanceWriteOffService.Post(advanceWriteOffId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult UpdateCustomerInvoiceWriteOff()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult ReportCustomerInvoiceWriteOff(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetAdvanceSetOffReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, "Customer Invoioce Set Off", SourceType.CustomerAdvanceWriteOff);
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

        #endregion PaymentReceipt

        
        public ActionResult InvoiceChargeWriteOff()
        {
            return View("~/Areas/Accounts/Views/InvoiceChargeWriteOff.cshtml");
        }

        [HttpGet]
        public JsonResult GetInvoiceChargeWriteOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceWriteOffService.GetInvoiceCharge(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.InvoiceCharge), JsonRequestBehavior.AllowGet);
        }


      

        [HttpPost]
        public JsonResult ParkInvoiceChargeWriteOff(VoucherViewModel advanceVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            advanceVM.SettlementType = SettlementType.Charge.ToString();
            if ((advanceVM.SettlementType == SettlementType.Charge.ToString()) && (advanceVM.Amount == 0 || advanceVM.Amount.ToString() == null))
                throw new CustomException("Amount should more than 0.");
            if ((advanceVM.SettlementType == SettlementType.Charge.ToString()) && (advanceVM.FinancingTypeId == null))
                throw new CustomException("Please select Charges.");
            advanceVM.SourceType = SourceType.InvoiceCharge.ToString();//Source Type will change.
            advanceVM.PartyType = PartyType.Customer.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertInvoiceChargeWriteOff(advanceVM)) });
        }


      

        [HttpPost]
        public JsonResult PostInvoiceChargeWriteOff(string advanceWriteOffId)
        {
            _advanceWriteOffService.Post(advanceWriteOffId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult UpdateInvoiceChargeWriteOff()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult GetInvoiceChargeWriteOffReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _advanceReportService.GetInvoiceChargeReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, "Invoice Charge Report", SourceType.InvoiceCharge);
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

      
        #region VendorChargeWriteOff
        
        public ActionResult VendorChargeWriteOff()
        {
            return View("~/Areas/Accounts/Views/VendorChargeWriteOff.cshtml");
        }
        [HttpGet, Authorize]
        public JsonResult GetVendorInvoiceChargeWriteOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_advanceWriteOffService.GetInvoiceCharge(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorInvoiceCharge), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ParkVendorChargeWriteOff(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            advanceVM.CompanyGroupId = identity.CompanyGroupId;
            advanceVM.CompanyId = identity.CompanyId;
            advanceVM.PlantId = identity.PlantId;
            advanceVM.IsPark = true;
            advanceVM.SettlementType = SettlementType.Charge.ToString();
            if (voucherDetailVMList==null )
                throw new CustomException("Please add GL");
            else
            {
                foreach (var item in voucherDetailVMList)
                {
                    if(item.Amount==0)
                        throw new CustomException("Amount should greater then 0");
                }
            }
            
            //if ((advanceVM.SettlementType == SettlementType.Charge.ToString()) && (advanceVM.FinancingTypeId == null))
            //    throw new CustomException("Please select Charges.");
            advanceVM.SourceType = SourceType.VendorInvoiceCharge.ToString();//Source Type will change.
            advanceVM.PartyType = PartyType.Vendor.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertVendorChargeWriteOff(advanceVM, voucherDetailVMList)) });
        }
        [HttpPost]
        public JsonResult PostVenodrInvoiceCharge(string invoiceWriteOffId)
        {
            _advanceWriteOffService.PostVendorInvoiceCharge(invoiceWriteOffId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult DeleteVenodrInvoiceCharge(string invoiceWriteOffId, string voucherId)
        {
            _advanceWriteOffService.DeleteVendorInvoiceCharge(invoiceWriteOffId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });

        }

        //Vendor invoice charge writeoff
        [HttpGet, Authorize]
        public ActionResult GetVendorInvoiceChargeWriteOffReport(ReportFormat reportFormat, string voucherId)
        {
            AccountsInvoiceReportService _accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountsInvoiceReportService.GetVendorInvoiceChargeReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        #endregion VendorChargeWriteOff

        #region Employee Salary Advance Ledger
        [HttpGet]
        public ActionResult EmployeeSalaryAdvanceLedger()
        {
            return View("~/Areas/Accounts/Views/EmployeeSalaryAdvanceLedger.cshtml");
        }
        
        [HttpGet, Authorize]
        public ActionResult EmployeeSalaryAdvanceLedgerReport(ReportFormat reportFormat, string employeeId, string fromDate, string toDate)//GetEmployeeLedgerReport
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.EmployeeSalaryAdvanceLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, employeeId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Employee Salary Advance Ledger";
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

        [HttpGet, Authorize]
        public ActionResult EmployeeAdvanceDueList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = EmployeeAdvanceDueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "Employee Advance.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        private IWorkbook EmployeeAdvanceDueReport(string companyGroupId, string companyId, string plantId)
        {

            //Start EmployeeAdvanceDueList


            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtEmployeeAdvanceDueList = _sqlRepository.GetDataTable(@"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.AdvanceNo, AM.VoucherId,en.UserName as Entity
								, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, AM.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received, 0 DrAmount, 0 CrAmount
                                , AD.Amount-AD.WrittenOffAmount AS Balance
							    FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AM.EmployeeId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                                ) AS CC ON CC.VoucherDetailId=VD.Id
							    
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType in ('EmployeeAdvance','InterTransaction')
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.EmployeeId<>'' ");

            if (dtEmployeeAdvanceDueList.Rows.Count == 0)
                throw new Exception("No data found");




            worksheet.Name = "EmployeeAdvanceDueListReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            // worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //  ROW++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNO = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Employee";
            int colEmployee = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "DocDate";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Ref No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Entity";
            int colEntity = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Advanced";
            int colAdvanced = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Receivable"].ToString());
            // worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            // worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;

            COL++;

            worksheet[ROW, COL].Text = "Write-Off";
            int colWriteOff = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Received"].ToString());
            // worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            COL++;

            worksheet[ROW, COL].Text = "Balance";
            int colBalance = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Balance"].ToString());
            //worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            // COL++;

            // int ROW = 6; int COL = 1;

            //int EmployeeAdvanceDueListStartRow  = ROW;
            //worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //ROW++;
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ROW++;

            for (int i = 0; i < dtEmployeeAdvanceDueList.Rows.Count; i++)
            {
                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                worksheet[ROW, colVoucherNO].Text = dtEmployeeAdvanceDueList.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colEmployee].Text = dtEmployeeAdvanceDueList.Rows[i]["EmployeeName"].ToString();
                worksheet[ROW, colDocDate].Text = dtEmployeeAdvanceDueList.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtEmployeeAdvanceDueList.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colEntity].Text = dtEmployeeAdvanceDueList.Rows[i]["Entity"].ToString();
                worksheet[ROW, colCurrency].Text = dtEmployeeAdvanceDueList.Rows[i]["CurrencyCode"].ToString();
                worksheet[ROW, colAdvanced].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[i]["Receivable"].ToString());
                worksheet[ROW, colAdvanced].NumberFormat = clsStaticInfo.NumberFormat();
                // worksheet[ROW, colAdvanced].Text = dtEmployeeAdvanceDueList.Rows[i]["Receivable"].ToString();

                //worksheet[ROW, colWriteOff].Text = dtEmployeeAdvanceDueList.Rows[i]["Received"].ToString();
                worksheet[ROW, colWriteOff].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[i]["Received"].ToString());
                worksheet[ROW, colWriteOff].NumberFormat = clsStaticInfo.NumberFormat();

                //worksheet[ROW, colBalance].Text = dtEmployeeAdvanceDueList.Rows[i]["Balance"].ToString();
                worksheet[ROW, colBalance].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[i]["Balance"].ToString());
                worksheet[ROW, colBalance].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet[ROW, colPurchaseLCCurrencyId].Text = dsData.Tables[0].Rows[i]["PurchasePLCurrency"].ToString();




                // worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                //worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeaderWithOutLogo(ref worksheet, endCol, "Employee Advance", identity.PlantId);

            //reportUtility.PlantHeader(ref worksheet, endCol, "Employee Advance" , identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            return workbook;
        }

        [HttpGet, Authorize]
        public ActionResult EmployeeSalaryAdvanceDueList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = EmployeeSalaryAdvanceDueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "Employee Salary Advance.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        private IWorkbook EmployeeSalaryAdvanceDueReport(string companyGroupId, string companyId, string plantId)
        {

            //Start EmployeeAdvanceDueList


            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtEmployeeAdvanceDueList = _sqlRepository.GetDataTable(@"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.AdvanceNo, AM.VoucherId,en.UserName as Entity
								, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, AM.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received, 0 DrAmount, 0 CrAmount
                                , AD.Amount-AD.WrittenOffAmount AS Balance
							    FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AM.EmployeeId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
                                LEFT JOIN [HKP].[EmployeeTransactionType] AS ETT ON ETT.Id=AM.EmployeeTransactionTypeId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                                ) AS CC ON CC.VoucherDetailId=VD.Id
							    
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType in ('EmployeeAdvance','InterTransaction') and ETT.UserName='Employee Salary'
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.EmployeeId<>'' ");

            if (dtEmployeeAdvanceDueList.Rows.Count == 0)
                throw new Exception("No data found");




            worksheet.Name = "EmployeeSalaryAdvanceDueListReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            // worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //  ROW++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNO = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Employee";
            int colEmployee = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "DocDate";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Ref No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Entity";
            int colEntity = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Advanced";
            int colAdvanced = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Receivable"].ToString());
            // worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            // worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;

            COL++;

            worksheet[ROW, COL].Text = "Write-Off";
            int colWriteOff = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Received"].ToString());
            // worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            COL++;

            worksheet[ROW, COL].Text = "Balance";
            int colBalance = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //worksheet[ROW, COL].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[0]["Balance"].ToString());
            //worksheet[ROW, COL].NumberFormat = clsStaticInfo.NumberFormat();
            // COL++;

            // int ROW = 6; int COL = 1;

            //int EmployeeAdvanceDueListStartRow  = ROW;
            //worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //ROW++;
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ROW++;

            for (int i = 0; i < dtEmployeeAdvanceDueList.Rows.Count; i++)
            {
                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                worksheet[ROW, colVoucherNO].Text = dtEmployeeAdvanceDueList.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colEmployee].Text = dtEmployeeAdvanceDueList.Rows[i]["EmployeeName"].ToString();
                worksheet[ROW, colDocDate].Text = dtEmployeeAdvanceDueList.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtEmployeeAdvanceDueList.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colEntity].Text = dtEmployeeAdvanceDueList.Rows[i]["Entity"].ToString();
                worksheet[ROW, colCurrency].Text = dtEmployeeAdvanceDueList.Rows[i]["CurrencyCode"].ToString();
                worksheet[ROW, colAdvanced].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[i]["Receivable"].ToString());
                worksheet[ROW, colAdvanced].NumberFormat = clsStaticInfo.NumberFormat();
                // worksheet[ROW, colAdvanced].Text = dtEmployeeAdvanceDueList.Rows[i]["Receivable"].ToString();

                //worksheet[ROW, colWriteOff].Text = dtEmployeeAdvanceDueList.Rows[i]["Received"].ToString();
                worksheet[ROW, colWriteOff].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[i]["Received"].ToString());
                worksheet[ROW, colWriteOff].NumberFormat = clsStaticInfo.NumberFormat();

                //worksheet[ROW, colBalance].Text = dtEmployeeAdvanceDueList.Rows[i]["Balance"].ToString();
                worksheet[ROW, colBalance].Number = clsStaticInfo.dbl(dtEmployeeAdvanceDueList.Rows[i]["Balance"].ToString());
                worksheet[ROW, colBalance].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet[ROW, colPurchaseLCCurrencyId].Text = dsData.Tables[0].Rows[i]["PurchasePLCurrency"].ToString();




                // worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                //worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeaderWithOutLogo(ref worksheet, endCol, "Employee Salary Advance", identity.PlantId);

            //reportUtility.PlantHeader(ref worksheet, endCol, "Employee Advance" , identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            return workbook;
        }


        [HttpPost, Authorize]
        public ActionResult EmployeeAdvanceTotalListReportXls(List<Dictionary<string, object>> data, string reportFileName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string fileName = "";

                fileName = EmployeeAdvanceTotalReport(data, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }           
        }

        private string EmployeeAdvanceTotalReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {

            //Start EmployeeAdvanceDueList

            var filePath = "";

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];


            if (data.Count == 0)
                throw new Exception("No data found");            

            worksheet.Name = "EmployeeAdvanceDueListReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "Employee Code";
            int colEmployeeCode = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Employee";
            int colEmployee = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Advanced";
            int colAdvanced = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Write-Off";
            int colWriteOff = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Balance";
            int colBalance = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
        
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ROW++;

            for (int i = 0; i < data.Count; i++)
            {
                worksheet[ROW, colEmployeeCode].Text = data[i]["EmployeeCode"].ToString();
                worksheet[ROW, colEmployee].Text = data[i]["EmployeeName"].ToString();
                worksheet[ROW, colCurrency].Text = data[i]["CurrencyCode"].ToString();
                
                worksheet[ROW, colAdvanced].Number = clsStaticInfo.dbl(data[i]["Receivable"].ToString());
                worksheet[ROW, colAdvanced].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colWriteOff].Number = clsStaticInfo.dbl(data[i]["Received"].ToString());
                worksheet[ROW, colWriteOff].NumberFormat = clsStaticInfo.NumberFormat();

                worksheet[ROW, colBalance].Number = clsStaticInfo.dbl(data[i]["Balance"].ToString());
                worksheet[ROW, colBalance].NumberFormat = clsStaticInfo.NumberFormat();

                worksheet.Range[ROW , 1, ROW , endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW , 1, ROW , endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;
            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeaderWithOutLogo(ref worksheet, endCol, "Employee Advance" , identity.PlantId);
           
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }

        [HttpPost]
        public JsonResult InsertPartyLiabilityReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.PartyReconcilliation.ToString();
            
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertPartyLiabilityReconciliation(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult InsertPartyAssetReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.PartyReconcilliation.ToString();

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertPartyAssetReconciliation(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult InsertPartyAdvanceLiabilityReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.PartyReconcilliation.ToString();

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertPartyLiabilityAdvanceReconciliation(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult InsertPartyAdvanceAssetReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.PartyReconcilliation.ToString();

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertPartyAssetAdvanceReconciliation(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostPartyReconciliation(string voucherId)
        {
            _advanceWriteOffService.PostPartyReconciliation(voucherId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public ActionResult GetVendorAdvanceReport(string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AccountsBankService accountsBankService = new AccountsBankService(_sqlRepository);
                string fileName = "";
                fileName = _advanceReportService.VendorAdvanceReport(plantId,identity.CompanyGroupId, identity.CompanyId, SourceType.VendorAdvance, "Vendor Advance Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}