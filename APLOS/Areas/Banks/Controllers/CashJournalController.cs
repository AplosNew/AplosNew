using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Model.Expenses;
using Library.Service.Banks;
using Library.Service.Expenses;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class CashJournalController : BaseController
    {
        private readonly ICashJournalService _cashJournalService;
        private readonly IBankJournalService _bankJournalService;
        private readonly IVoucherReportService _voucherReportService;
        private readonly IExpenseBookingService _expenseBookingService;

        public CashJournalController(
            ICashJournalService cashJournalService
            , IVoucherReportService voucherReportService
            , IExpenseBookingService expenseBookingService,
            IBankJournalService bankJournalService)
        {
            _cashJournalService = cashJournalService;
            _voucherReportService = voucherReportService;
            _expenseBookingService = expenseBookingService;
            _bankJournalService = bankJournalService;
        }

        [HttpGet]
        public ActionResult CashJournal()
        {
            return View("~/Areas/Banks/Views/CashJournal.cshtml");
        }

        [HttpGet]
        public ActionResult EntityExpenseBooking()
        {
            return View("~/Areas/Banks/Views/EntityExpenseBooking.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult EntityExpenseBookingApproval()
        {
            return View("~/Areas/Banks/Views/EntityExpenseBookingApproval.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCashJournalList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_cashJournalService.GetCashJournalList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CashJournal), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCashJournalDetailList(GridParameter parameters, string voucherId, string voucherDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_cashJournalService.GetCashJournalDetail(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherId, voucherDetailId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertCashJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.CashJournal.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.ApprovedById != null)
            {
                voucherVM.ApprovedByStatus = "ToBeApproved";
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _cashJournalService.InsertCashJournal(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateCashJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            if (voucherVM.ApprovedById != null && voucherVM.ApprovedByStatus == "Approved")
                throw new CustomException("Updated is not allowed after approved !!");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.CashJournal.ToString();
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _cashJournalService.UpdateCashJournal(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult DeleteCashJournalDetail(string Id, string voucherId,string cashJournalDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _cashJournalService.DeleteCashJournalDetail(Id, voucherId, cashJournalDetailId, identity.PlantId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult DeleteCashJournal(string cashJournalId, string voucherId)
        {
            _cashJournalService.DeleteCashJournal(cashJournalId, voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult PostCashJournal(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException(Resources.IdNotFound);
            _cashJournalService.PostCashJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public JsonResult GetBankJournalDetailList(string id)
        {
            return Json(_cashJournalService.GetCashJournalDetailList(id), JsonRequestBehavior.AllowGet);
        }

        #region Entity Expenses Booking

        [HttpGet, Authorize]
        public JsonResult GetExpenseBookingList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_cashJournalService.GetCashJournalList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CashExpenses), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExpensesBookingById(GridParameter parameters, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_expenseBookingService.GetExpenseBookingApprovedData(parameters, identity.CompanyId, identity.PlantId, Id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExpensesBookingDetail(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseBookingService.GetEntityExpensesBookingDetail(identity.CompanyGroupId,identity.CompanyId,identity.PlantId, voucherId, SourceType.CashExpenses), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExpenseBookingPendingList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseBookingService.GetEntityExpenseBookingPendingList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityExpenseBookingSubmittedList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseBookingService.GetEntityExpenseBookingSubmittedList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityExpenseBookingSubmittedData(GridParameter parameters, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_expenseBookingService.GetEntityExpenseBookingSubmittedData(parameters, identity.CompanyId, identity.PlantId, Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertEntityExpenseBooking(VoucherViewModel expenseBooking, IEnumerable<VoucherDetailViewModel> expenseBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
           
            if (expenseBookingDetailList == null)
                throw new CustomException("Please Add GL.");
            expenseBooking.Amount = expenseBookingDetailList.Sum(r => r.Amount);
            foreach (var advanceDetailVM in expenseBookingDetailList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0.");
            }
            _expenseBookingService.InsertEntityExpenses(expenseBooking, expenseBookingDetailList);
            return Json(new { BudgetTransactionMaster = expenseBooking, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EntityExpensesBookingEdit(VoucherViewModel expenseBooking, IEnumerable<VoucherDetailViewModel> expenseBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            foreach (var advanceDetailVM in expenseBookingDetailList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
            }
            _expenseBookingService.UpdateCashJournal(expenseBooking, expenseBookingDetailList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult EntityExpensesBookingSubmit(ExpenseBooking expenseBooking, IEnumerable<ExpenseBookingDetail> expenseBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            expenseBooking.CompanyGroupId = identity.CompanyGroupId;
            expenseBooking.CompanyId = identity.CompanyId;
            expenseBooking.PlantId = identity.PlantId;
            if (expenseBooking.ApprovalStatus == "Submitted")
                throw new CustomException("Update is not allowed after Submitted.");
            expenseBooking.AppliedBy = AppliedByBooking.Entity.ToString();
            if (expenseBooking.EmployeeId == null)
                expenseBooking.EmployeeId = identity.EmployeeId;
            foreach (var advanceDetailVM in expenseBookingDetailList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
            }
            _expenseBookingService.EntityExpenseBookingSubmit(expenseBooking, expenseBookingDetailList);
            expenseBooking.EmployeeId = identity.EmployeeId;
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostEntityExpenseBooking(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.CompanyCurrencyRate = 1;
            if (voucherVM.IsPosted)
                throw new CustomException("Post is not allowed after Posted.");
            voucherVM.BankJournalType = BankJournalType.CashExpense.ToString();
            voucherVM.SourceType = SourceType.CashJournal.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _cashJournalService.PostEntityExpensesBooking(voucherVM, voucherDetailList)) });
        }

        #endregion Entity Expenses Booking

        #region Telly

        #region PaymentByCash

        [HttpGet]
        public ActionResult PaymentByCash()
        {
            return View("~/Areas/Banks/Views/PaymentByCash.cshtml");
        }

      
        [HttpGet, Authorize]
        public JsonResult GetPaymentByCashList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByCash), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCashPaymentDetailList(GridParameter parameters, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentDetailList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByCash, bankJournalId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertPaymentByCash(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.PaymentByBank.ToString();
            if (voucherVM.BankJournalType == null)
                throw new CustomException("Please Select To !");
            if (voucherVM.BankJournalType == BankJournalType.CashToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToVendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToVendor.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please Select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _cashJournalService.InsertCashPayment(voucherVM, voucherDetailVMList)) });
        }


        [HttpPost]
        public JsonResult UpdatePaymentByCash(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.PaymentByCash.ToString();
            if (voucherVM.BankJournalType == BankJournalType.CashToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To  Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To  Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToVendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToVendor.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _cashJournalService.UpdateCashPayment(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostPaymentByCash(string id)
        {
            _cashJournalService.PostCashJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        #endregion PaymentByCash

        #region ReceiptByCash

        [HttpGet]
        public ActionResult ReceiptByCash()
        {
            return View("~/Areas/Banks/Views/ReceiptByCash.cshtml");
        }


        [HttpGet, Authorize]
        public JsonResult GetReceiptByCashList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByCash), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCashReceiptDetailList(GridParameter parameters, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentDetailList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByCash, bankJournalId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertReceiptByCash(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.ReceiptByCash.ToString();
            if (voucherVM.BankJournalType == BankJournalType.CashToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToCustomer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToCustomer.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _cashJournalService.InsertCashReceipt(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateReceiptByCash(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.ReceiptByCash.ToString();
            if (voucherVM.BankJournalType == BankJournalType.CashToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToCustomer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToCustomer.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.CashToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _cashJournalService.UpdateCashReceipt(voucherVM, voucherDetailVMList)) });
        }


        [HttpPost]
        public JsonResult PostReceiptByCash(string id)
        {
            _cashJournalService.PostCashJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        #endregion ReceiptByCash

        #endregion Telly
    }
}