#region Using

using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Payrolls
{
    public interface ILoanAdvanceChildService : IService<LoanAdvanceChild>
    {
        void InsertOrUpdateGraph(IEnumerable<LoanAdvanceChild> loanAdvanceChild, string masterId);

        IEnumerable<object> GetLoanChildByMaster(string loanMstSystemID);

        IEnumerable<object> GetOpeningBalanceChildByMaster(string loanMstSystemID);

        void InsertOrUpdateGraphOpeningBalance(IEnumerable<LoanAdvanceChild> loanAdvanceChildList, string masterId);
    }
}