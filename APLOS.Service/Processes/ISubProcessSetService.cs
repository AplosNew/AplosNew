#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface ISubProcessSetService : IService<SubProcessSet>
    {
        GridModel Query(GridParameter parameters, string entityId);

        void InsertGraph(SubProcessSet entity, IEnumerable<SubProcessSetDetail> subprocessSetDetail);

        void UpdateGraph(SubProcessSet entity, IEnumerable<SubProcessSetDetail> subprocessSetDetail);

        void DeleteGraph(string id);
    }
}