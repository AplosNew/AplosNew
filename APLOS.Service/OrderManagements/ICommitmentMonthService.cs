#region Using

using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ICommitmentMonthService : IService<CommitmentMonth>
    {
        IEnumerable<object> Query(string masterId);

        void InsertGraph(string masterId, IEnumerable<CommitmentMonth> entities);

        void UpdateGraph(string masterId, IEnumerable<CommitmentMonth> entities);

        void DeleteMonth(string masterId);
    }
}