#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IIntermediateItemEntityService : IService<IntermediateItemEntity>
    {
        GridModel Query(GridParameter parameters, string entityId, string companyGroupId);

        void InsertORUpdate(IEnumerable<IntermediateItemEntity> entities);

        void DeleteGraph(string key);
    }
}