#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IBonusPolicyMonthlyRetainEligibleEmployeeService : IService<BonusPolicyMonthlyRetainEligibleEmployee>
    {
        void InsertOrUpdate(IEnumerable<BonusPolicyMonthlyRetainEligibleEmployee> entities);

        GridModel Query(GridParameter parameters);

        GridModel QueryForOptionalBonusEmployee(GridParameter parameters, string plantId);

        GridModel QueryForMandatoryBonusEmployee(GridParameter parameters, string plantId);

        void DeleteGraph(string key);
    }
}