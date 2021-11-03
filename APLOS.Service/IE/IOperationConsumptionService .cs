#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IE
{
    public interface IOperationConsumptionService : IService<OperationConsumption>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();
    }
}