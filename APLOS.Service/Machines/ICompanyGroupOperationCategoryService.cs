using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface ICompanyGroupOperationCategoryService : IService<CompanyGroupOperationCategory>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string operationCategoryId, bool active);

        void DeleteGraph(string operationCategoryId);

        GridModel Query(GridParameter parameters);
    }
}