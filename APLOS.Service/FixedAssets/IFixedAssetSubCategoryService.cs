using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetSubCategoryService : IService<FixedAssetSubCategory>
    {
        IEnumerable<object> GetCbo();

        decimal GetAutoSequence();

        void Delete(string id);

        GridModel Query(GridParameter parameters);
    }
}