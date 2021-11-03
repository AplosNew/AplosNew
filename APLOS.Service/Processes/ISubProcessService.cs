#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface ISubProcessService : IService<SubProcess>
    {
        IEnumerable<object> GetCbo();

        GridModel GetCbo(string processid);

        decimal GetAutoSequence(string processId);

        GridModel Query(GridParameter parameters, string processId, string companyGroupId);

        GridModel GetListForCompanySubProcess(GridParameter parameters, string companyId, string processId, string[] subProcessIds);

        GridModel GetListSubProcess(GridParameter parameters, string companyId, string processId, string[] subProcessIds);

		GridModel GetSubProcessListByProductionProcess(GridParameter parameters, string companyGroupId, string processId);

	}
}