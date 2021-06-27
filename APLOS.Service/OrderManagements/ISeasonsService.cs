#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ISeasonsService : IService<Seasons>
    {
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
        IEnumerable<object> GetSeasonsCbo();
    }
}