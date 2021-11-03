#region Using

using Library.Model.Processes;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Processes
{
    public interface IProcessCriteriaService : IService<ProcessCriteria>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}