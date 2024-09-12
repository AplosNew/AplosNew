using Library.Core;
using Library.Model.Employees;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;

namespace Library.Service.Employees
{
    public interface IEmployeePayableWriteOffService
    {
        EmployeePayableWriteOff InsertEmployeePayableWriteOff(EmployeePayableWriteOff employeePayableWriteOff);

        EmployeePayableWriteOffDetail InsertEmployeePayableWriteOffDetail(EmployeePayableWriteOff employeePayableWriteOff, EmployeePayableWriteOffDetail employeePayableWriteOffDetail, int currentId);

        GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId);

        string InsertEmployeePayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        string InsertMultipleEmployeePayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> employeeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        void Post(string employeePayableWriteOffId);
        void DeletePayableWriteOff(string employeePayableWriteOffId, string voucherId);
        void DeleteEmployeePayable(string employeePayableId, string voucherId);

    }
}