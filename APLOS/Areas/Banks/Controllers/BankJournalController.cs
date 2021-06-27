using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Service.Banks;
using Library.ViewModel.Banks;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankJournalController : BaseController
    {
        private readonly IBankJournalService _bankJournalService;
        private readonly IBankReportService _bankReportService;

        public BankJournalController(
            IBankJournalService bankJournalService
            , IBankReportService bankReportService
            )
        {
            _bankJournalService = bankJournalService;
            _bankReportService = bankReportService;
        }

        [HttpGet, Authorize]
        public ActionResult BankJournal()
        {
            return View("~/Areas/Banks/Views/BankJournal.cshtml");
        }

        [HttpGet]
        public JsonResult GetBankJournalList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankJournalList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.BankJournal), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertBankJournal(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.BankJournal.ToString();
            voucherVM.IsPark = true;
             if (voucherVM.BankJournalType == BankJournalType.CashExpense.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            if (voucherVM.BankJournalType == BankJournalType.BankCharge.ToString() && bankChargeDetailVMList == null)
                throw new CustomException("Please select GL!");
            if (voucherVM.BankJournalType == BankJournalType.ProfitEarn.ToString() && voucherVM.FinancingTypeId == null)
                throw new CustomException("Please select Investment Type!");

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _bankJournalService.InsertBankJournal(voucherVM, voucherDetailVMList, bankChargeDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateBankJournal(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.BankJournal.ToString();
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankJournal(voucherVM, voucherDetailVMList, bankChargeDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostBankJournal(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteBankJournal(string bankJournalId,string voucherId)
        {
            _bankJournalService.DeleteBankJournal(bankJournalId, voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public JsonResult GetBankJournal(string id)
        {
            return Json(_bankJournalService.GetBankJournal(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankJournalDetailList(string id)
        {
            return Json(_bankJournalService.GetBankChargeList(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAdvanceBankChargeList(string bankChargeId)
        {
            return Json(_bankJournalService.GetAdvanceBankChargeList(bankChargeId), JsonRequestBehavior.AllowGet);
        }

        #region Telly

       
        public ActionResult PaymentByBank()
        {
            return View("~/Areas/Banks/Views/PaymentByBank.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetPaymentByBankList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCashPaymentDetailList(GridParameter parameters, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentDetailList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByBank, bankJournalId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertPaymentByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.PaymentByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To  Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To  Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            var no = _bankJournalService.InsertBankPayment(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, no) });
        }

        [HttpPost]
        public JsonResult UpdatePaymentByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.PaymentByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To  Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To  Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankPayment(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostPaymentByBank(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet]
        public ActionResult ReceiptByBank()
        {
            return View("~/Areas/Banks/Views/ReceiptByBank.cshtml");
        }

      
        [HttpGet, Authorize]
        public JsonResult GetReceiptByBankList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCashReceiptDetailList(GridParameter parameters, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentDetailList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByBank, bankJournalId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertReceiptByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.ReceiptByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            var no = _bankJournalService.InsertBankReceipt(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, no) });

        }

        [HttpPost]
        public JsonResult UpdateReceiptByBank(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.ReceiptByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankReceipt(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostReceiptByBank(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        #endregion Telly

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerPaymentList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetAvilabeCustomerPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
    }
}