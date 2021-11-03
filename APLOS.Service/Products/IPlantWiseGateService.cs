#region Using

using Library.Core;
using Library.Model.Products;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    public interface IPlantWiseGateService : IService<PlantWiseGate>
    {
        IEnumerable<object> GetUserGateList(string userId);
        GridModel GetGateData(GridParameter parameters);
        GridModel Query(GridParameter parameters, string plantId);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo(string plantId);
    }
}