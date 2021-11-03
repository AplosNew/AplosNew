#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialMasterGLService : IService<MaterialMasterGL>
    {
        void DeleteGraph(string id);

        void InsertUpdateMaterialMasterGL(IEnumerable<MaterialMasterGL> entities, IEnumerable<MaterialMasterVendorReconGL> fixedAssetVendorReconGL);

        void InsertOrUpdate(string masterId, MaterialMasterGL entity, IEnumerable<MaterialMasterVendorReconGL> fixedAssetVendorReconGL);

        GridModel GetDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId);

        GridModel GetVendorReconDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId);

        GridModel GetSearchWithCombine(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds);

        //GridModel GetSearchWithCombineCoa(GridParameter parameters, string coaId);
        GridModel GetSearchWithCombineCoa(GridParameter parameters);

        IEnumerable<object> GetFixedAssetItemCbo();

        GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds);

        GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds);

        GridModel GetPartyAccountGroup(GridParameter parameters);

        GridModel GetPartyAccountVD(GridParameter parameters);

        IEnumerable<object> GetPartyAccountWithAssignList(string partyAcId, string materialMasterGlId);

        IEnumerable<object> GetBudgetActivityCbo(string budgetId);

        IEnumerable<object> GetAccountGroupData();

        IEnumerable<object> GetAccountGroupData2();
    }
}