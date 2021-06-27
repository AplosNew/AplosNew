#region Using

using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface ISubProcessSetDetailService : IService<SubProcessSetDetail>
    {
        IEnumerable<object> Query(string subProcessSetId);

        void InsertUpdateOrDeleteGraph(string subProcessSetId, IEnumerable<SubProcessSetDetail> subProcessSetDetail);

        void DeleteGraph(string subProcessSetId);
    }
}