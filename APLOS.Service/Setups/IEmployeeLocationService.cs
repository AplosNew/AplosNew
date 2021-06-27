using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Setups
{
    public interface IEmployeeLocationService : IService<EmployeeLocation>
    {
        GridModel Query(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence(string companyGroupId);

        IEnumerable<object> GetCbo(string companyGroupId);
    }
}