using Library.Core;
using Library.Data.Repositories;
using Library.Model.Accounts;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Extension;
using Library.ViewModel.Accounts;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.Invoices
{
    public interface IInvoiceWriteOffService : IService<InvoiceWriteOff>
    {
        InvoiceWriteOff FindInvoiceWriteOff(string Id);
        IQueryFluent<InvoiceWriteOff> QueryInvoiceWriteOff(string voucherId);
        void DeleteInvoiceWriteOff(string id);
        IQueryFluent<InvoiceWriteOffDetail> QueryInvoiceWriteOffDetail(string invoiceWriteOffId);
        void DeleteInvoiceWriteOffDetail(string id);
        InvoiceWriteOff InsertInvoiceWriteOff(InvoiceWriteOff invoiceWriteOff);

        InvoiceWriteOff InsertInvoiceWriteOff(VoucherViewModel voucherVM);
        InvoiceWriteOff InsertInvoiceWriteOffDifferentCurrency(InvoiceWriteOff invoiceWriteOffVM);
        InvoiceWriteOff InsertCustomerInvoiceSetOff(VoucherViewModel voucherVM);

        void InsertInvoiceWriteOffDetail(InvoiceWriteOff invoiceWriteOff, InvoiceWriteOffDetail invoiceWriteOffDetail, int currentId);

        string InsertVendorPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<PurchaseLCChargesViewModel> purchaseLCChargesVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<VoucherDetailViewModel> glVMList, IEnumerable<VoucherViewModel> existingLoanList);
        string InsertInvoiceToAcceptancePost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<VoucherDetailViewModel> glVMList);

        GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);
        GridModel GetMultiplePaymentVoucherList(GridParameter parameters, string plantId, SourceType sourceType);
        GridModel GetVendorPaymentParkedNonPostedList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);
        string InsertReceived(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);

        void Post(string invoiceWriteOffId);
        void ApproveVendorPayment(InvoiceWriteOff invoiceWriteOff, OTSBD.IdentityParameter para);
        void PostInvoiceToAcceptance(string invoiceWriteOffId);
        string InsertCustomerInvoiceReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
               , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);
        string InsertInvoiceRoundOffJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string InsertCustomerInvoiceBanksReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
              , IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);
        GridModel CustomerInvoiceBanksQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);
        void InsertCustomerInvoiceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        string InsertOtherInvicePost(VoucherViewModel voucherVM, string otherInvoiceId, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        List<Dictionary<string, object>> GetVoucherWriteOffList(string companyGroupId, string companyId, string plantId, string voucherWriteOffId);
        List<Dictionary<string, object>> GetVoucherWriteOffDetailList(string companyGroupId, string companyId, string plantId, string voucherWriteOffId);
        string InsertPartyReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string UpdatePartyReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string InsertDebitNoteSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);
        string InsertDebitNoteInvoiceSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList);
        string InsertCreditNoteSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);
        string InsertCreditNoteSetOffDifferentCurrency(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);
        string InsertCreditNoteInvoiceSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList);
        string InsertVendorCreditNoteSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList);
        void DeleteWriteOff(string invoiceWriteOffId, string voucherId, string deletedRemarks);
        void DeleteInvoiceToAcceptance(string invoiceWriteOffId, string voucherId);
        void DeleteCustomerBanksReceipt(string invoiceWriteOffGroupNo, SourceType sourceType);
        GridModel GetNoteSetOff(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);
        void PurchaseLCChargesPost(VoucherViewModel voucherVM, IEnumerable<PurchaseLCCharges> voucherRows
            , IEnumerable<PurchaseLCChargesViewModel> purchaseLCChargesList);
        void CustomerBanksPost(string invoiceWriteOffNo);

        string InsertPurchaseRealizationService(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList,
        IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList);
        void SuspensePayablePost(string invoiceGroupNo);
        string InsertAdditionalTaxPayable(VoucherViewModel voucherVM, string additionalTaxId);
        string PostMultipleVendorPayment(VoucherViewModel voucherVM, IEnumerable<MultiplePaymentViewModel> mpSummarylist, IEnumerable<MultiplePaymentDetailViewModel> multiplePaymentDetailList
                , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);
        void DeleteAdjustmentNoteWriteOff(string invoiceWriteOffId, string voucherId);
        string DeleteMultipleVendorRow(IEnumerable<MultiplePayment> multiplePaymentlist, IEnumerable<MultiplePaymentDetail> multiplePaymentDetailList);
        IWorkbook PaymentAdviceReportxlx(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string adviceNo);
    }
}