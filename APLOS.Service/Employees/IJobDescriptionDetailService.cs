#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IJobDescriptionDetailService : IService<JobDescriptionDetail>
    {
        void DeleteGraphByJobDescription(string jobDescriptionId);

        void InsertGraph(IEnumerable<JobDescriptionDetail> entities, string jobDescriptionId);

        void UpdateGraph(IEnumerable<JobDescriptionDetail> entities, string jobDescriptionId);

        GridModel Query(GridParameter parameters, string jobDescriptionId);
    }
}