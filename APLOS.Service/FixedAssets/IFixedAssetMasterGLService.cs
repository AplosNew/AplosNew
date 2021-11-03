#region Using

using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetMasterGLService : IService<FixedAssetMasterGL>
    {
        void DeleteGraph(string id);

        void InsertUpdateFixedAssetMasterGL(IEnumerable<FixedAssetMasterGL> entities, IEnumerable<FixedAssetMasterVendorReconGL> fixedAssetVendorReconGL);

        void InsertOrUpdate(string masterId, FixedAssetMasterGL entity, IEnumerable<FixedAssetMasterVendorReconGL> fixedAssetVendorReconGL);

        GridModel GetDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId);

        GridModel GetVendorReconDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId);

       
        IEnumerable<object> GetFixedAssetItemCbo();


        GridModel GetPartyAccountGroup(GridParameter parameters);

        GridModel GetPartyAccountVD(GridParameter parameters);

        IEnumerable<object> GetPartyAccountWithAssignList(string partyAcId, string FixedAssetMasterGlId);

        IEnumerable<object> GetBudgetActivityCbo(string budgetId);

        IEnumerable<object> GetAccountGroupData();

        IEnumerable<object> GetAccountGroupData2();
        void FixedAssetMasterReport();
    }
}