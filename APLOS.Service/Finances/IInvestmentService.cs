using Library.Core;
using Library.Model.Enums;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;

namespace Library.Service.Finances
{
    public interface IInvestmentService
    {
        GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType);

        string InsertInvestment(VoucherViewModel voucherVM);
        string InsertInvestmentSetOff(VoucherViewModel voucherVM);
        Dictionary<string, object> GetById(string id);
    }
}