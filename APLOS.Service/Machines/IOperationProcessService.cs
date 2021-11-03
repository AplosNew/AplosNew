#region Using

using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Machines
{
    public interface IOperationProcessService : IService<OperationProcess>
    {
        IEnumerable<object> Query(string operationId);

        void InsertOrDeleteGraph(string operationId, IEnumerable<OperationProcess> operationProcess);

        void DeleteGraph(string operationId);
    }
}