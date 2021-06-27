using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface ICompanyFixedAssetDepreciationRuleService : IService<CompanyFixedAssetDepreciationRule>
    {
        GridModel Query(GridParameter parameters, string companyId);

        GridModel GetSearchWithCombine(GridParameter parameters, string companyId);

        GridModel GetSearchWithCombineAll(GridParameter parameters, string companyId, string fixedAssetCategoryIds, string FixedAssetSubCategoryIds);

        GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string companyId, string fixedAssetCategoryIds, string FixedAssetSubCategoryIds);

        GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string companyId, string fixedAssetCategoryIds, string FixedAssetSubCategoryIds);

        void InsertUpdateCDepreciation(IEnumerable<CompanyFixedAssetDepreciationRule> entities);

        void DeleteGraph(string id);
    }
}