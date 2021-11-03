using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface ICompanyGroupOperationActivityService : IService<CompanyGroupOperationActivity>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string operationActivityId, bool active);

        void DeleteGraph(string operationActivityId);

        GridModel Query(GridParameter parameters);
    }
}