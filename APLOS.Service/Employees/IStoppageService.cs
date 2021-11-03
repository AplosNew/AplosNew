#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IStoppageService : IService<Stoppage>
    {
        //GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();

        GridModel GetCbo(string routeId);

        IEnumerable<object> GetCityByCompanyCbo(string companyId);
    }
}