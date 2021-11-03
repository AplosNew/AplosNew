#region Using

using Library.Model.Productions;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Productions
{
    public interface IProductionSettingsService : IService<ProductionSettings>
    {
        IEnumerable<object> Query(string plantId);

        void InsertGraph(ProductionSettings productionSettings, IEnumerable<ProcessCapacityUOM> processCapacityUOM);

        void UpdateGraph(ProductionSettings productionSettings, IEnumerable<ProcessCapacityUOM> processCapacityUOM);

        void DeleteGraph(string plantId);
    }
}