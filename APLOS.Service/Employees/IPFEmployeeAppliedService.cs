#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPFEmployeeAppliedService : IService<PFEligibleEmployee>
    {
        void InsertOrUpdate(IEnumerable<PFEligibleEmployee> entities);

        GridModel Query(GridParameter parameters);

        GridModel QueryForPFMandatoryEmployee(GridParameter parameters, string plantId);

        GridModel QueryForPFOptionalEmployee(GridParameter parameters, string plantId);

        void DeleteGraph(string key);
    }
}