using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface ICompanyGroupFixedAssetCategoryService : IService<CompanyGroupFixedAssetCategory>
    {
        void DeleteGraph(string fixedAssetCategoryId);

        GridModel Query(GridParameter parameters, string companyGroupId);

        IEnumerable<object> GetCbo();
    }
}