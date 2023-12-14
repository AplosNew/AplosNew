#region Using

using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface IProcessSetDetailService : IService<ProcessSetDetail>
    {
        IEnumerable<object> Query(string processSetId);

		IEnumerable<object> GetProcessSetList(string processSetId,string entityId);

		void InsertGraph(string processSetId, IEnumerable<ProcessSetDetail> processSetDetail);

        void InsertUpdateOrDeleteGraph(string processSetId, IEnumerable<ProcessSetDetail> processSetDetail);

        void DeleteGraph(string processSetId);
    }
}