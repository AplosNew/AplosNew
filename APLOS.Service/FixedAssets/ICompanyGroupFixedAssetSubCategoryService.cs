using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface ICompanyGroupFixedAssetSubCategoryService : IService<CompanyGroupFixedAssetSubCategory>
    {
        void DeleteGraph(string fixedAssetSubCategoryId);

        GridModel Query(GridParameter parameters, string companyGroupId);

        IEnumerable<object> GetCbo();
    }
}