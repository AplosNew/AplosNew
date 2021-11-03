using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface ICompanyGroupFixedAssetClassService : IService<CompanyGroupFixedAssetClass>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string fixedAssetClassId, bool active);

        void DeleteGraph(string fixedAssetClassId);

        GridModel Query(GridParameter parameters);
    }
}