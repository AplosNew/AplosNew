#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    /// <summary>   Interface for process service. </summary>
    public interface IProcessUoMService : IService<ProcessUoM>
    {
        void Insert(ProcessUoM entity, IEnumerable<ProcessAlternativeUoM> alternativeUoM);

        void Update(ProcessUoM entity, IEnumerable<ProcessAlternativeUoM> alternativeUoM);

        void Delete(string id);

        GridModel Query(GridParameter parameters, string companyGroupId);

        IEnumerable<object> GetUoMCboByProcess(string processId);

        IEnumerable<object> GetCapacityUoMCboByProcess(string processId);
    }
}