#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IRouteEmployeeService : IService<RouteEmployee>
    {
        GridModel Query(GridParameter parameters, string plantId);

        GridModel GetAllEmployee(GridParameter parameters, string plantId);

        void InsertOrUpdateGraph(IEnumerable<RouteEmployee> routeEmployeeList, string plantId, string routeId);

        IEnumerable<object> GetSavedData(string plantId, string routeId);

        void DeleteGraph(string routeId);
    }
}