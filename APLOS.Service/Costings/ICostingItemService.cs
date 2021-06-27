#region Using

using Library.Core;
using Library.Model.Costings;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Costings
{
    public interface ICostingItemService : IService<CostingItem>
    {
        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();

        GridModel Query(GridParameter parameters);
    }
}