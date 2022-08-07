using Library.Core;
using Library.Model.Advances;
using Library.Model.Enums;
using Library.Service.Core;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;

namespace Library.Service.Vouchers
{
    public interface ICommonAccountsSetOffService : IService<AdvanceWriteOff>
    {
        string InsertDebitNoteAdvanceSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList);
    }
}