using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Employees
{
    public interface ICompanyGroupSOPCategoryService : IService<CompanyGroupSOPCategory>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string sopCategoryId, bool active);

        void DeleteGraph(string sopCategoryId);

        GridModel Query(GridParameter parameters);
    }
}