#region Using

using Library.Model.Biometrics;
using Library.Service.Core;
using Library.Service.Extension.HumanResource.Leave;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Biometrics
{
    public interface ILeaveTransactionDetailsService : IService<LeaveTransactionDetails>
    {
        void InsertGraph(PolicySandwichVM _policyVM,List<string> listH, List<string> listW, LeaveTransactionDetails details, DateTime fromDate, DateTime toDate, decimal duration, bool halfDay);
    }
}