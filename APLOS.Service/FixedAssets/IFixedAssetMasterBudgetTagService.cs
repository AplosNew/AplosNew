using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetMasterBudgetTagService : IService<FixedAssetMasterBudgetTag>
    {
        GridModel Query(GridParameter parameters, string coaId);

        void InsertOrUpdateGraph(IEnumerable<FixedAssetMasterBudgetTag> entities);
    }
}