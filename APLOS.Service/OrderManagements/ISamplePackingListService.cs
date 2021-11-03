#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
	public interface ISamplePackingListService : IService<SamplePackingList>
	{
		IEnumerable<object> GetPackingListByMaterialGroupMaster(string materialGroupMasterId);

		IEnumerable<object> Get2ndPackingListByMaterialGroupMaster(string firstFormId);

		void InsertPackingForm(IEnumerable<SamplePackingListMaterialDetails> materialList, IEnumerable<SamplePackingListForm> firstPackingList);

		void UpdatePackingForm(IEnumerable<SamplePackingListMaterialDetails> materialList, IEnumerable<SamplePackingListForm> firstPackingList);

		void DeletePackingForm(string firstPackId);

		void DeleteGraph(string id);

		GridModel Query(GridParameter parameters, string plantId);
	}
}