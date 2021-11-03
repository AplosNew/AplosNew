using System.Collections.Generic;

namespace Library.Service.Accounts
{
    public interface IAccountDashboardService
    {
        IEnumerable<object> OverAllReceivableWithPartyCurrency(string companyId, string partyId, string currencyId);

        IEnumerable<object> OverAllPayableWithPartyCurrency(string companyId, string partyId, string currencyId);

        IEnumerable<object> OverDueReceivableModal(string companyId, string partyId, string currencyId, string matureDate);
    }
}