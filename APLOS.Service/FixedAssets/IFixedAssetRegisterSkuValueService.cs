using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetRegisterSkuValueService : IService<FixedAssetRegisterSkuValue>
    {
        void InsertOrUpdateGraph(IEnumerable<FixedAssetRegisterSkuValue> entity, string assetItemId, string fixedAssetRegisterId);

        void DeleteGraph(string masterId);
    }
}