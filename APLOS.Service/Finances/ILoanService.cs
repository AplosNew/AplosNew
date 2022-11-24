using Library.Core;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Finances
{
    public interface ILoanService
    {
        string InsertLoan(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList, IEnumerable<FinancingMasterOrderViewModel> financingMasterOrderlist);

        string InsertLoanWriteOff(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList);
        string InsertLoanWriteOffChangeBooksAmount(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList);
        string InsertLoanWriteOffLoanAddition(VoucherViewModel voucherVM, VoucherViewModel loanAdditionVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList);
        string InsertMultiLoanWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> loanRepaymentlist);
        string InsertLoanInterestPayable(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList);
        string InsertLoanInterestPayableReverse(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList);
        string InsertLoanClose(IEnumerable<VoucherViewModel> existingLoanList);
    }
}