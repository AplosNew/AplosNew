#region Using

using Library.Model.Processes;

using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    /// <summary>   Interface for process service. </summary>
    public interface IProcessAlternativeUoMService : IService<ProcessAlternativeUoM>
    {
        IEnumerable<object> GetAltUomList(string masterId);

        void InsertUpdateOrDeleteGraph(string masterId, IEnumerable<ProcessAlternativeUoM> entities);

        void DeleteGraph(string masterId);
    }
}