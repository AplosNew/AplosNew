using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialMasterVendorReconGLService : IService<MaterialMasterVendorReconGL>
    {
        MaterialMasterVendorReconGL FindbyFKId(string key);

        //void InsertOrUpdate(FixedAssetGL master, IEnumerable<FixedAssetVendorReconGL> entities);
        void InsertOrUpdate(IEnumerable<MaterialMasterGL> masterlist, IEnumerable<MaterialMasterVendorReconGL> entities);

        void DeleteGraph(string masterId);
    }
}