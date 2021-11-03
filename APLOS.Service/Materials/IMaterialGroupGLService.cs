using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialGroupGLService : IService<MaterialGroupGL>
    {
        // void DeleteGraph(string id);
        void InsertUpdateMaterialGroupDeterminate(IEnumerable<MaterialGroupGL> entities, IEnumerable<MaterialGroupPartyAccountGroupGL> materialGroupVendorReconGL);

        void InsertOrUpdate(string masterId, MaterialGroupGL entity);

        //GridModel GetMaterialGroupTypeListById(GridParameter parameters, string fixedasset enum, string Id,string coaId);
        GridModel GetDataByMaterialGroupMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId);

        IEnumerable<object> GetSearchWithCombine(string coaId);

        //GridModel GetSearchWithCombineCoa(GridParameter parameters, string coaId);
        GridModel GetSearchWithCombineCoa(GridParameter parameters);

        IEnumerable<object> GetSearchWithCombineWithNotAssign(string coaId);

        IEnumerable<object> GetSearchWithCombineWithAssign(string coaId);

        GridModel GetPartyAccountGroup(GridParameter parameters, string accountType);

        GridModel GetPartyAccountGroup(GridParameter parameters);

        GridModel GetPartyAccountVD(GridParameter parameters);

        void DeleteGraph(string id);
    }
}