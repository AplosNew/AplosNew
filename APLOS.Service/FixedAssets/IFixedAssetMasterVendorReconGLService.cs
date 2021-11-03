using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetMasterVendorReconGLService : IService<FixedAssetMasterVendorReconGL>
    {
        FixedAssetMasterVendorReconGL FindbyFKId(string key);

        void InsertOrUpdate(IEnumerable<FixedAssetMasterGL> masterlist, IEnumerable<FixedAssetMasterVendorReconGL> entities);

        void DeleteGraph(string masterId);
    }
}