using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetDepreciationRuleService : IService<FixedAssetDepreciationRule>
    {
        IEnumerable<object> GetCbo();

        void DeleteGraph(string id);

        GridModel Query(GridParameter parameters);
    }
}