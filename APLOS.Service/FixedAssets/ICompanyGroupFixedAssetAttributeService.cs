using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface ICompanyGroupFixedAssetAttributeService : IService<CompanyGroupFixedAssetAttribute>
    {
        IEnumerable<object> GetCbo(string companyGroupId);

        void UpdateGraph(string fixedAssetAttributeId, bool active);

        void DeleteGraph(string fixedAssetAttributeId);

        GridModel Query(GridParameter parameters);
    }
}