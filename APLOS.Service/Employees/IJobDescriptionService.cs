#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IJobDescriptionService : IService<JobDescription>
    {
        IEnumerable<object> GetSOPDocumentList(string SOPItemId);
        GridModel Query(GridParameter parameters, string companyGroupId);

        void InsertGraph(JobDescription entity, IEnumerable<JobDescriptionDetail> jobDescriptionDetail);

        void UpdateGraph(JobDescription entity, IEnumerable<JobDescriptionDetail> jobDescriptionDetail);

        GridModel Query(GridParameter parameters, string companyGroupId, string[] jobDescriptionIds);

        IEnumerable<object> GetEmployeeJobDescription(string employeeId);

        IEnumerable<object> GetActivityDocumentList(string SOPActivityId);
        IEnumerable<object> GetFileByJDId(string jdId);
    }
}