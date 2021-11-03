#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPFEmployeeVoluntaryValueService : IService<PFEmployeeVoluntaryValue>
    {
        void InsertOrUpdate(IEnumerable<PFEmployeeVoluntaryValue> entities);

        GridModel Query(GridParameter parameters);

        GridModel QueryPFEmpVoluntaryValue(GridParameter parameters, string plantId, string effectiveDate);
        GridModel QueryPFEmpVoluntaryValueChecked(GridParameter parameters, string plantId, string effectiveDate);
        void DeleteGraph(string key);
    }
}