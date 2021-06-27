#region Using

using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Machines
{
    public interface IOperationTypeService : IService<OperationType>
    {
        GridModel Query(GridParameter parameters);

        IEnumerable<object> GetCbo();

        decimal GetAutoSequence();

        void Delete(string key);
    }
}