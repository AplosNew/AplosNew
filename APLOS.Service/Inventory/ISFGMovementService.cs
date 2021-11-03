#region Using
using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Inventory
{
    public interface ISFGMovementService : IService<SFGMovement>
    {
        IEnumerable<object> GetUserSFGMovementList(string userId);
        GridModel Query(GridParameter parameters);
        IEnumerable<object> GetCbo();
        decimal GetAutoSequence();
    }
}