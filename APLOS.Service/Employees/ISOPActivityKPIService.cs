#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPActivityKPIService : IService<SOPActivityKPI>
    {
        IEnumerable<object> GetKPIListMain(string sopItemId);

        IEnumerable<object> GetKPIList(string activityId);
    }
}