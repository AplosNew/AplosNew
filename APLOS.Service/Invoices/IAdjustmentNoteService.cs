using Library.Core;
using Library.Data.Repositories;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Invoices;
using Library.Service.Core;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.Invoices
{
    public interface IAdjustmentNoteService : IService<AdjustmentNote>
    {
        GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);

        FinancingTypeGL GetCreditNoteGL(string companyId, string financingTypeId);

        FinancingTypeGL GetDebitNoteGL(string companyId, string financingTypeId);

        void Post(string adjustmentNoteId, string entityId, string voucherId);

        string InsertCreditNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList);

        string InsertDebitNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList);
        string InsertDebitNote_InvoiceSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList);
        GridModel GetDebitNoteList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string partyId, string partyType);
        GridModel GetCreditNoteList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string partyId, string partyType);
        void DeleteAdjustmentNote(string adjustmentNoteId, string voucherId);
        IEnumerable<AdjustmentNoteDetail> QueryInvoiceDetailEnumerable(IEnumerable<string> query);
        void UpdateAdjustmentNoteDetail(AdjustmentNoteDetail adjustmentNoteDetail);
        IQueryFluent<AdjustmentNoteDetail> QueryAdjustmentNoteDetail(string adjustmentNoteId);

         AdjustmentNote InsertAdjustmentNote(VoucherViewModel voucherVM);
        AdjustmentNoteDetail InsertAdjustmentNoteDetail(AdjustmentNote adjustmentNote, AdjustmentNoteDetail adjustmentNoteDetail, int currentId);
    }
}