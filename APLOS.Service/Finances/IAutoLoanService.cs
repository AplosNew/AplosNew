using Library.Core;
using Library.Model.Enums;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Finances
{
    public interface IAutoLoanService
    {
        string ParkAutoLoan(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList);

    }
}