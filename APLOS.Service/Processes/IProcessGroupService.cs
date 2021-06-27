#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface IProcessGroupService : IService<ProcessGroup>
    {
        decimal GetAutoSequence(string companyGroupId);

        IEnumerable<object> GetCbo(string companyGroupId);

        GridModel Query(GridParameter parameters, string companyGroupId);
    }
}