#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IRouteStoppageService : IService<RouteStoppage>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();

        void InsertOrUpdateGraph(IEnumerable<RouteStoppage> routeStoppageList, string routeId, out List<RouteStoppage> routeStoppageDb_list);

        void DeleteGraph(string routeId);
    }
}