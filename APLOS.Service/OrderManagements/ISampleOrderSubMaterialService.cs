#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ISampleOrderSubMaterialService : IService<SampleOrderSubMaterial>
    {
        IEnumerable<object> Query(string masterId);

        GridModel GetPendingSampleOrderList(GridParameter parameters, string entityId);

        GridModel GetMaterialList(GridParameter parameters, string materialGroupId);

        IEnumerable<object> GetPendingList(string[] ids);

        GridModel GetMaterialMasterByCustomer(GridParameter parameters, string partyId, string[] sampleOrderSubMaterialIds);

        void InsertGraph(string masterId, IEnumerable<SampleOrderSubMaterial> entity);

        void InsertOrUpdateGraph(string masterId, IEnumerable<SampleOrderSubMaterial> suMaterials);

        void DeleteGraph(string masterId);

        void Confirmation(string id, bool flag);

        void MaterialAttach(SampleOrderSubMaterial sampleOrderMaterial);

        void MaterialDetached(string id);

        void DispatchDate(string id, DateTime date);

        void IfUoMExistInMaterialMaster(string materialMasterId, string uomId);
    }
}