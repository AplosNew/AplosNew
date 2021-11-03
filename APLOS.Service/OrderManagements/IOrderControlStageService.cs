#region Using

using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IOrderControlStageService : IService<OrderControlStage>
    {
        decimal GetAutoSequence();
        IEnumerable<object> GetOrderControlStageList();
    }
}