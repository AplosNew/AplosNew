using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Employees
{
    public interface ICompanyGroupSOPDocumentSubCategoryService : IService<CompanyGroupSOPDocumentSubCategory>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string sopDocumentSubCategoryId, bool active);

        void DeleteGraph(string sopDocumentSubCategoryId);

        GridModel Query(GridParameter parameters);
    }
}