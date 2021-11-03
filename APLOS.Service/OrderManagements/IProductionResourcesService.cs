#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IProductionResourcesService : IService<ProductionResources>
    {
        void DeleteGraph(string id);
        IEnumerable<object> Query(string PlantId);

    }
}