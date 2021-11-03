#region Using

using Library.Model.External;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public interface IKPIService : IService<KPI>
    {
        IEnumerable<object> GetKPIList(string activityId);
    }
}