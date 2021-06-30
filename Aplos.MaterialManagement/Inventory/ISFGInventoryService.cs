#region Using
using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.MaterialManagement.Inventory
{
    public interface ISFGInventoryService : IService<SFGInventory>
    {
        GridModel Query(GridParameter parameters);
        IEnumerable<object> GetCbo();
        decimal GetAutoSequence();
    }
}