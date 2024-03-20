using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Model.Vouchers;
using Library.Service.Vouchers;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherTypeMatrixController : BaseController
    {
        private readonly IVoucherTypeMatrixService _voucherTypeMatrixService;

        public VoucherTypeMatrixController(IVoucherTypeMatrixService voucherTypeMatrixService)
        {
            _voucherTypeMatrixService = voucherTypeMatrixService;
        }

        [HttpPost]
        public JsonResult CreateVoucherTypeMatrix(VoucherTypeMatrix voucherTypeMatrix)
        {
            if (voucherTypeMatrix.CompanyGroupId == null)
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                voucherTypeMatrix.CompanyGroupId = identity.CompanyGroupId;
            }
            _voucherTypeMatrixService.Insert(voucherTypeMatrix);
            return Json(new { VoucherTypeMatrix = voucherTypeMatrix, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult EditVoucherTypeMatrix(VoucherTypeMatrix voucherTypeMatrix)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherTypeMatrix.CompanyGroupId = identity.CompanyGroupId;
            _voucherTypeMatrixService.Update(voucherTypeMatrix);
            return Json(new { VoucherTypeMatrix = voucherTypeMatrix, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult DeleteVoucherTypeMatrix(string id)
        {
            _voucherTypeMatrixService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetVoucherTypeMatrixList(GridParameter parameters, string companyGroupId)
        {
            if (companyGroupId == null || companyGroupId == "")
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyGroupId = identity.CompanyGroupId;
            }
            return Json(_voucherTypeMatrixService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeOpeningBalanceList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.OpeningBalance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeJournalVoucherList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.AdvanceJournalVoucher), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypePFESICDisbursementVoucherList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PFESICDisbursement), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeEmployeePayableList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeePayable), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeSalaryPayableList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SalaryPayable), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeSalaryDisbursementList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SalaryDisbursement), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeFinalSettlementDisbursementList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.FinalSettlementJournal), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeBonusDisbursementList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.BonusDisbursement), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeAccountReceivableList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerInvoice), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeReceiptList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerReceipt), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeBanksReceiptList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerBanksReceipt), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeSuspensePayableList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SuspensePayable), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeAccountPayableList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorInvoice), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeFGInventoryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.FGInventory), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypePostInvoiceList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PostInvoice), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeReceivableFromOthersList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceivableFromOthers), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeOutSourceBillingList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.OutSourceBilling), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypePackingJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PackingJournal), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeInvoiceOverheadList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorInvoice), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeIssueJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.IssueJournal), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeIssueReturnJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.IssueReturnJournal), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeFixedAssetCapitalizeJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.FixedAssetCapitalizeJournal), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeFiscalYearCloseJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.YearCloseJournal), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeFixedAssetDisposeJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.FixedAssetDisposeJournal), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeFixedAssetDepreciationJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.DepreciationJournal), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypePaymentList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorPayment), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeTaxPaymentList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.TaxPayment), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeEmployeePaymentList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeePayment), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeAdvanceTakenList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeCustomerSuspense()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerSuspense), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeAdvanceTakenWriteOffList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvanceWriteOff), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeAdvanceGivenList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeAdvanceGivenWriteOffList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorAdvanceWriteOff), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeEmployeeAdvanceList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvance), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeEmployeeAdvanceWriteOffList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.EmployeeAdvanceWriteOff), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeSecurityTakenList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SecurityDeposit), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeSecurityTakenWriteOffList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SecurityDeposit), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeSecurityGivenList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SecurityDeposit), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeSecurityGivenWriteOffList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SecurityDeposit), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeLoanList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.Loan), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeLoanPaymentList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.LoanPayment), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeInvestmentSetOffList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.InvestmentSetOff), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeAutoLoanList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.AutoLoan), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeLoanInterestPayableList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.LoanInterestPayable), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeLoanGivenWriteOffList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.Loan), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeCreditNoteList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CreditNote), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypePartyReconcilliationList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PartyReconcilliation), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeDebitNoteList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.DebitNote), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeInventoryReturnPayableList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.InventoryReturnPayable), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeSalesReturnList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SalesReturn), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboVoucherTypeInvestmentList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.Investment), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypeBankJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.BankJournal), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypePaymentByBankList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByBank), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypePaymentByCashList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByCash), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypeReceiptByBankList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByBank), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypeReceiptByCashList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByCash), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypeCashJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CashJournal), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypeCashExpensesList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CashExpenses), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypeInterTransactionList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.InterTransaction), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypePuechaseDocumentAcceptanceList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PurchaseDocAcceptance), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboVoucherTypePuechaseLCOpeningChargesList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucherTypeMatrixService.GetCboVoucherTypeList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PurchaseLCOpeningCharges), JsonRequestBehavior.AllowGet);
        }
    }
}