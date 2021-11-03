#region Using

using Library.Core;
using Library.Model.Productions;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Productions
{
    public interface IProductionBookingPeriodService : IService<ProductionBookingPeriod>
    {
        decimal GetAutoSequence();
        IEnumerable<object> GetCbo();
        void DeleteGraph(string id);
        GridModel Query(GridParameter parameters);
    }
}