#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ILSDService : IService<LSD>
    {
        void DeleteGraph(string id);

        IEnumerable<object> LsdList(string buyerId);

        GridModel Query(GridParameter parameters, string buyerId);
    }
}