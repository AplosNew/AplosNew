using Library.Core;
using Library.Data.Repositories;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Systems;
using Library.Model.Vouchers;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Library.Service.Employees
{
    public interface IEmployeePayableService
    {
        Dictionary<string, object> GetEmployeePayableReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType);

        Dictionary<string, object> GetEmployeePayableExpenseBookingReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType);

        List<Dictionary<string, object>> GetAdvanceWriteOffReportData(string companyId, string voucherId);

        EmployeePayable InsertEmployeePayable(EmployeePayable employeePayable);

        void UpdateEmployeePayable(EmployeePayable employeePayable);

        EmployeePayable InsertEmployeePayable(VoucherViewModel voucherVM);

        void Post(string employeePayableId);
        void PostVoucher(Voucher voucher, string employeePayableId, string type, IEnumerable<VoucherDetailViewModel> voucherDetailList);

        PKGenerator GetMaxNumber();

        string InsertEmployeePayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList);

        string UpdateEmployeePayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        EmployeePayableDetail InsertEmployeePayableDetail(EmployeePayable employeePayable, EmployeePayableDetail employeePayableDetail, int currentId);

        void UpdateEmployeePayableDetail(EmployeePayableDetail employeePayableDetail);

        EmployeeTransactionTypeGL GetEmployeePayableGL(string companyId, string employeeTransactionTypeId);

        GridModel GetEmployeePayableList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);

        IQueryFluent<EmployeePayable> GetEmployeePayableList(Expression<Func<EmployeePayable, bool>> query);

        IQueryFluent<EmployeePayableDetail> GetEmployeePayableDetailList(Expression<Func<EmployeePayableDetail, bool>> query);

        Dictionary<string, object> GetEmployeePayable(string id);

        List<Dictionary<string, object>> GetEmployeePayableDetailList(string voucherId);

        List<Dictionary<string, object>> GetExoenseBookingReportData(string companyId, string voucherId);
        GridModel GetEmployeeReconGLBudgetActivity(GridParameter parameters, string companyGroupId, string companyId);
        GridModel GetEmployeeReconAssetGLBudgetActivity(GridParameter parameters, string companyGroupId, string companyId);
        void DeleteInvoiceBeneficiaryEmployee(string invoiceId, string voucherId, string deletedRemarks);
        void DeleteGRNBeneficiaryEmployee(string grnId, string invoiceId, string voucherId, string deletedRemarks);
        void DeleteServiceBeneficiaryEmployee(string serviceAckId, string invoiceId, string voucherId);
    }
}