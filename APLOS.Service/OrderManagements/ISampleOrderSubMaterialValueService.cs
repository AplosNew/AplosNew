#region Using

using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ISampleOrderSubMaterialValueService : IService<SampleOrderSubMaterialValue>
    {
        IEnumerable<object> Query(string materialGroupMasterId, string sampleOrderSubMaterialId);

        IEnumerable<object> GetAttributeByMgm(string materialGroupMasterId, string subMaterialId);

        void InsertOrUpdateGraph(string masterId, SampleOrderSubMaterial entity);

        void DeleteGraph(SampleOrderSubMaterial subMaterials);
    }
}