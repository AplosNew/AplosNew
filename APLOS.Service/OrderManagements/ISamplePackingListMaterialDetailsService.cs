#region Using

using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ISamplePackingListMaterialDetailsService : IService<SamplePackingListMaterialDetails>
    {
        void InsertPackingMaterial(IEnumerable<SamplePackingListMaterialDetails> entities, string id);

        void UpdatePackingMaterial(IEnumerable<SamplePackingListMaterialDetails> entities);

        void UpdatePackLessMaterial(IEnumerable<SamplePackingListMaterialDetails> entities);

        void DeletePackingMaterial(string firstPackId);

        void DeleteGraph(string masterId);

        IEnumerable<object> GetPackingMaterial(string firstFormId);

        IEnumerable<object> GetPackLessMaterialList(string masterId);

        IEnumerable<object> GetAllMaterialList(string masterId);

        IEnumerable<object> GetViewMaterialList(string firstFormId, string smpMaterialId);
    }
}