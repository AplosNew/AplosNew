using Library.Core;
using Library.Model.Accounts;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.ViewModel.Inventory;
using Library.ViewModel.Invoices;
using Library.ViewModel.Materials;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;

namespace Library.Service.Invoices
{
    public interface IInventoryPayableService
    {
        string InsertInventoryPayable(string receiveId,string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<InvoiceTaxViewModel> additionalTaxList, IEnumerable<VoucherDetailViewModel> otherVendorChargesList);
        string InsertEmployeePayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList);
        string InsertAdditionalTaxPayable(VoucherViewModel voucherVM, string additionalTaxId);
        void InsertInventoryShortagePayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);
        void InsertInventoryRejectPayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);
        void InsertIssueJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InventoryMaterialViewModel> invIssueDetailList, IEnumerable<InventoryMaterialViewModel> invIssueDetailGLList);
        void InsertIssueReturnJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InventoryMaterialViewModel> invIssueDetailList, IEnumerable<InventoryMaterialViewModel> invIssueDetailGLList);
        void InsertGRNFixedAssetCapitalizeJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        void InsertIssueFixedAssetCapitalizeJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InventoryMaterialViewModel> invIssueDetailList);
        void InsertIssueInventoryCapitalizeJournal(string issueId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<InventoryMaterialViewModel> invIssueDetailList);
        void PostDocumentAcceptance(VoucherViewModel voucherVM, IEnumerable<PurchaseDocAcceptanceDetailViewModel> docAcceptanceDetails, IEnumerable<PurchaseDocAcceptanceDetailViewModel> rowDetails, bool IsNonCreditable);
        void PostDocumentAcceptanceService(VoucherViewModel voucherVM, IEnumerable<PurchaseDocAcceptanceViewModel> voucherRows
            , IEnumerable<PurchaseDocAcceptanceChargesViewModel> purchaseDocAcceptanceServiceList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);
        void InsertExpensesCapitalizeJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
       
        void InsertServicePayable(string serviceAcknowledgementMasterId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
            , IEnumerable<ServiceAcknowledgementDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList);
        void PostSingleJournalSales(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, OtherInvoice otherInvoiceVM);
        void PostMultipleJournalSales(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList, OtherInvoice otherInvoiceVM);
        void PostMultipleJournalSalesReturn(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
         , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
          , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList, OtherInvoice otherInvoiceVM);
        string InsertPurchaseReturnPayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList, bool isDebitNote);
        string InsertInventoryTransferPayable(string receiveId, VoucherViewModel voucherVM
           , IEnumerable<VoucherDetailViewModel> fromPlantInventoryTransferJV
           , IEnumerable<VoucherDetailViewModel> toPlantInventoryTransferJV
           , IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
           );
        string InventoryOSReceivedPost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> inventoryJobWorkWIPList
            , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
        , IEnumerable<VoucherDetailViewModel> voucherDetailVMList
        , IEnumerable<VoucherDetailViewModel> inventoryJobWorkGIRIList, VoucherViewModel ServiceVM);
        void DeleteTDSPostServicePayable(string invoiceWriteOffId, string voucherId, string serviceAckId);
        void DeleteTDSServicePayable(string additionalTaxId,string voucherId);
        void DeleteIssueJournal(string issueId, string voucherId);
        void DeleteIssueReturnJournal(string issueId, string voucherId);
        string InsertCreditNoteAdditionalTaxPost(VoucherViewModel voucherVM, string additionalTaxId);
        string InsertShortageDebitNote(VoucherViewModel voucherVM, string grnId, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
    }
}