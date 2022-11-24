#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface ICompanySubProcessService : IService<CompanySubProcess>
    {
        void Insert(IEnumerable<CompanySubProcess> companySubProcess, string[] ids);

        GridModel Query(GridParameter parameters, string companyId, string processId);

        GridModel Query(GridParameter parameters, string companyId, string processId, string[] subProcessIds);

        void DeleteGraph(string id);

        IEnumerable<ComboModel> GetCbo(string ProcessId, string companyId);
    }
}