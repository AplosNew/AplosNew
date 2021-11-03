#region Using

using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ISamplePackingListFormService : IService<SamplePackingListForm>
    {
        void InsertFirstPackingForm(IEnumerable<SamplePackingListForm> firstPackingList, string id);

        void UpdateFirstPackingForm(IEnumerable<SamplePackingListForm> entities);

        void InsertOrUpdateSecondPackingForm(IEnumerable<SamplePackingListForm> entities);

        void DeletePackingForm(string firstPackId);

        void DeleteGraph(string masterId);

        IEnumerable<object> GetPackingFormList(string masterId);

        IEnumerable<object> GetFirstPackingForm(string id, string samplePackingMaterialId, string materialGroupMstId);

        IEnumerable<object> GetSecondPackByFirstPackId(string firstFormId, string samplePackingListMaterialId);
    }
}