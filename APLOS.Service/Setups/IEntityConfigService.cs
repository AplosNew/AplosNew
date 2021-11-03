#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IEntityConfigService : IService<EntityConfig>
    {
        void InsertOrUpdateGraph(IEnumerable<EntityConfig> entities, string entityId);
        IEnumerable<object> GetEntityConfigParameterList();
        void DeleteGraph(string Id);
        IEnumerable<object> Query(string entityId);
        IEnumerable<object> GetCbo();
        List<ComboModel> GetCboProduction(string companyGroupId);
    }
}