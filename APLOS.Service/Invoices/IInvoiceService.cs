using Library.Core;
using Library.Data.Repositories;
using Library.Model.Accounts;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Invoices;
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

      


        string InsertCustomerInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, OtherInvoice otherInvoiceVM);

        void InsertSales(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<ExchangeRateViewModel> exchangeRateVMList);

        string InsertVendorInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList);

        string InsertVendorInvoiceBeneficiaryEmployee(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList);

        string UpdateVendorInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
      

        void InsertMultipleVendorAvailableApproved(IEnumerable<MultipleVendorIdViewModel> partyIdList, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);

        IQueryFluent<InvoiceDetail> GetInvoiceDetailList(Expression<Func<InvoiceDetail, bool>> query);

        void Post(string invoiceId);

        InvoiceDetail FindInvoiceDetail(string invoiceDetailId);
        void DeleteInvoice(string invoiceId, string voucherId);
        Invoice FindInvoice(string Id);
        void DeleteInvoice(string id);
        IQueryFluent<InvoiceDetail> QueryInvoiceDetail(string invoiceId);
        void DeleteInvoiceDetail(string id);

        string InsertInvoiceOverhead(VoucherViewModel voucherVM, IEnumerable<ServiceChargesViewModel> voucherDetailVMList, IEnumerable<ServiceChargesTaxViewModel> taxDetailVMList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList);
        string InsertInvoiceOverheadPost(VoucherViewModel voucherVM, IEnumerable<ServiceChargesViewModel> voucherDetailVMList);
        void DeleteInvoiceOverhead(string invoiceId, string voucherId);

        string InsertMultiplePaymnet(MultiplePayment entity, IEnumerable<MultiplePaymentDetail> multiplePaymentDetailList);
        void DeleteInventoryPayable(string grnId, string invoiceId, string voucherId);
        void DeleteServicePayable(string serviceAckId, string invoiceId, string voucherId);
        void DeleteInventorySales(string salesId, string voucherId, string InventoryVoucherId);
    }
}