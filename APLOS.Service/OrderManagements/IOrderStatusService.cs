#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IOrderStatusService : IService<OrderStatus>
    {
        GridModel Query(GridParameter parameters);

        IEnumerable<object> GetCbo();

        decimal GetAutoSequence();
    }
}