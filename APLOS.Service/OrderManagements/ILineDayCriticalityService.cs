#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ILineDayCriticalityService : IService<LineDayCriticality>
    {
        void InsertOrUpdate(IEnumerable<LineDayCriticality> entities);

        GridModel Query(GridParameter parameters);

        void DeleteGraph(string workday);
    }
}