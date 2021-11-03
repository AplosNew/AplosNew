#region Using

using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Machines
{
    public interface IOperationSubProcessService : IService<OperationSubProcess>
    {
        void InsertOrDeleteGraph(string operationId, string operationProcessId, IEnumerable<OperationSubProcess> entities, IEnumerable<OperationSubProcess> dbData);

        void DeleteGraph(string operationProcessId, IEnumerable<OperationSubProcess> dbData);
    }
}