#region Using

using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Machines
{
    public interface IOperationFgComponentService : IService<OperationFgComponent>
    {
        IEnumerable<object> Query(string operationId);

        void InsertUpdateOrDeleteGraph(string operationId, IEnumerable<OperationFgComponent> entity);

        void DeleteGraph(string operationId);
    }
}