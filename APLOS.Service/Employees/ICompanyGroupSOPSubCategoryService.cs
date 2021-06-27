using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Employees
{
    public interface ICompanyGroupSOPSubCategoryService : IService<CompanyGroupSOPSubCategory>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string sopSubCategoryId, bool active);

        void DeleteGraph(string sopSubCategoryId);

        GridModel Query(GridParameter parameters);
    }
}