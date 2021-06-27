#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Payrolls
{
    public interface ILoanAdvanceMasterService : IService<LoanAdvanceMaster>
    {
        IEnumerable<ComboModel> GetSalaryHeadCbo(string currencyRuleSystemID);
        GridModel GetCbo(string currencyRuleSystemID);

        void InsertOrUpdate(LoanAdvanceMaster entity, IEnumerable<LoanAdvanceChild> loanAdvanceChild);

        IEnumerable<object> GetLoanMasterByEmployee(string employeeId);

        IEnumerable<object> GetYear(string plantId);

        GridModel GetLoanAdvanceInfoPlantWise(GridParameter parameters, string plantId, bool isControlAdmin, bool isSysAdmin, string employeeId);

        void UpdateSalApprovals(IEnumerable<LoanAdvanceMaster> entities, string name);

        IEnumerable<object> GetOpeningBalanceByEmployee(string employeeId);

        void InsertOrUpdateOpeningBalance(LoanAdvanceMaster entity, IEnumerable<LoanAdvanceChild> loanAdvanceChild);
    }
}