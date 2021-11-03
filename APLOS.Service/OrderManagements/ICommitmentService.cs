#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ICommitmentService : IService<Commitment>
    {
        IEnumerable<object> GetCommitmentData();
        IEnumerable<ComboModel> GetCbo();
        GridModel GetProductMasterList(GridParameter parameters, string groupId);
        GridModel Query(GridParameter parameters, string entityId);

        GridModel GetMaterialMasterList(GridParameter parameters, string groupId);

        IEnumerable<ComboModel> GetSalesGroupCbo(string entityId);

        void Insert(Commitment entity, IEnumerable<CommitmentMonth> monthList, IEnumerable<CommitmentValueAddedProcess> cvAddedList);

        void Update(Commitment entity, IEnumerable<CommitmentMonth> monthList, IEnumerable<CommitmentValueAddedProcess> cvAddedList);

        IEnumerable<object> QueryCommitmentValueAdded(string masterId);

        void DeleteMaster(string id);
        void DeleteProcess(string Id);
    }
}