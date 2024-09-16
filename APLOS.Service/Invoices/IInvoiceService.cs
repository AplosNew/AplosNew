using Library.Core;
using Library.Data.Repositories;
using Library.Model.Accounts;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Systems;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.ViewModel.Accounts;
using Library.ViewModel.Currencies;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Library.Service.Invoices
{
    public interface IInvoiceService : IService<Invoice>
    {
        Invoice InsertInvoice(Invoice invoice);

        Invoice InsertInvoice(Invoice invoice, long currentId);

        Invoice InsertInvoice(VoucherViewModel voucherVM);

        void InsertInvoiceDetail(Invoice invoice, InvoiceDetail invoiceDetail, int currentId);

        void UpdateInvoiceDetail(InvoiceDetail invoiceDetail);

        PKGenerator GetAdditionalTaxMaxNumber();


        string InsertCustomerInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, OtherInvoice otherInvoiceVM);

        void InsertSales(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<ExchangeRateViewModel> exchangeRateVMList);

        string InsertVendorInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList, IEnumerable<VoucherViewModel> existingLoanList);

        string InsertVendorInvoiceBeneficiaryEmployee(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList);

        string UpdateVendorInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string InsertIncentiveReceivableInvoice(VoucherViewModel voucherVM, IEnumerable<IncentiveReceivableMap> incentiveReceivableMapList);

        void InsertMultipleVendorAvailableApproved(IEnumerable<MultipleVendorIdViewModel> partyIdList, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);

        IQueryFluent<InvoiceDetail> GetInvoiceDetailList(Expression<Func<InvoiceDetail, bool>> query);

        void Post(string invoiceId);
        void PostVoucher(Voucher voucher, string invoiceId, string type, IEnumerable<VoucherDetailViewModel> voucherDetailList);

        InvoiceDetail FindInvoiceDetail(string invoiceDetailId);
        void DeleteInvoice(string invoiceId, string voucherId, string deletedRemarks);
        void DeleteJV(string voucherId, string deletedRemarks);
        void DeleteIncentiveReceivableInvoice(string invoiceId, string voucherId);
        Invoice FindInvoice(string Id);
        void DeleteInvoice(string id);
        IQueryFluent<InvoiceDetail> QueryInvoiceDetail(string invoiceId);
        IEnumerable<InvoiceDetail> QueryInvoiceDetailEnumerable(IEnumerable<string> query);
        void DeleteInvoiceDetail(string id);

        string InsertInvoiceOverhead(VoucherViewModel voucherVM, IEnumerable<ServiceChargesViewModel> voucherDetailVMList, IEnumerable<ServiceChargesTaxViewModel> taxDetailVMList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList);
        string InsertInvoiceOverheadPost(VoucherViewModel voucherVM, IEnumerable<ServiceChargesViewModel> voucherDetailVMList);
        void DeleteInvoiceOverhead(string invoiceId, string voucherId);

        string InsertMultiplePaymnet(MultiplePayment entity, IEnumerable<MultiplePaymentDetail> multiplePaymentDetailList);
        void DeleteInventoryPayable(string grnId, string invoiceId,string otherVendorId, string voucherId, string deletedRemarks);
        void DeleteServicePayable(string serviceAckId, string invoiceId, string voucherId);
        void DeleteInventorySales(string salesId, string voucherId, string InventoryVoucherId);
    }
}