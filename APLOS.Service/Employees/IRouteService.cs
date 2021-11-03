#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IRouteService : IService<Route>
    {
        IEnumerable<object> GetCbo();

        GridModel GetAllStoppage(GridParameter parameters);

        GridModel GetAllDriver(GridParameter parameters);

        void Insert(Route entity, IEnumerable<RouteStoppage> routeStoppage);
        
    }
}