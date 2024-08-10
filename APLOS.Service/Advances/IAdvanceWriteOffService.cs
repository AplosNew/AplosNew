using Library.Core;
using Library.Model.Advances;
using Library.Model.Enums;
using Library.Service.Core;
using Library.ViewModel.Banks;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;

namespace Library.Service.Advances
{
    public interface IAdvanceWriteOffService : IService<AdvanceWriteOff>
    {
        GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);
        GridModel GetInvoiceCharge(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);
        GridModel QueryEmployee(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);

        string InsertCustomerPaymentWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        string InsertCustomerAdvanceWriteOff(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);
        string InsertMultiCustomerAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailListNew, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList);
        string InsertVendorAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string InsertVendorAdvanceWriteOffDifferentCurrency(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        string InsertEmployeeAdvanceWriteOff(VoucherViewModel advanceVM, VoucherDetailViewModel VoucherDetailVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailGLList);
        string InsertVendorPaymentEmployeeAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
               , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherViewModel> advanceVMList);
        string InsertEmployeeTotalAdvanceWriteOff(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList);

        string UpdateEmployeeAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        string InsertInvoiceChargeWriteOff(VoucherViewModel voucherVM);
        string InsertVendorChargeWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        Dictionary<string, object> GetById(string id);

        List<Dictionary<string, object>> GetEmployeeAdvanceDetail(string companyGroupId, string companyId, string plantId, string voucherId);

        void Post(string advanceWriteOffId);

        Dictionary<string, object> GetAdvanceWriteOffReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType);

        List<Dictionary<string, object>> GetAdvanceWriteOffReportData(string companyId, string voucherId);

        string InsertCustomerInvoiceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string InsertEmployeeSalaryPayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        void DeleteEmployeeSalaryPayable(string payableId, string voucherId);
        void PostEmployeeSalaryPayable(string employeePayableId);
        void PostVendorInvoiceCharge(string invoicewriteOffId);
        string InsertPartyLiabilityReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> invoiceDetailVMList);
        string InsertPartyAssetReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> invoiceDetailVMList);
        string InsertPartyLiabilityAdvanceReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> invoiceDetailVMList);
        string InsertPartyAssetAdvanceReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> invoiceDetailVMList);
        void PostPartyReconciliation(string voucherId);
        void DeleteCustomerAdvanceWriteOff(string voucherId);
        void DeleteVendorInvoiceCharge(string invoiceWriteOffId, string voucherId);
    }
}