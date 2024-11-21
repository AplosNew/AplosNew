#region Using

using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Processes
{
    public interface IProcessSetService : IService<ProcessSet>
    {
        GridModel Query(GridParameter parameters, string companyId, string entityId);

		GridModel Query(GridParameter parameters, string companyGroupId);

        GridModel QueryByCompany(GridParameter parameters, string companyId, string entityId);
        GridModel GetProcessSetListByCompany(GridParameter parameters, string companyId);

        void InsertGraph(ProcessSet entity, IEnumerable<ProcessSetDetail> processSetDetail);

        void UpdateGraph(ProcessSet entity, IEnumerable<ProcessSetDetail> processSetDetail);

        void DeleteGraph(string id);

        IWorkbook GetProcessSetReport(string companyId, string entityId, string process);
    }
}