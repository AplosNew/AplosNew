#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IRunningOrderParametersService : IService<RunningOrderParameter>
    {
        void DeleteGraph(string id);
        IEnumerable<object> Query(string PlantId);
        void Insert(RunningOrderParameter entity);
        void Update(RunningOrderParameter entity);
    }
}