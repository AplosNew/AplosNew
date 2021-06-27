using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Employees
{
    public interface ICompanyGroupSOPDocumentCategoryService : IService<CompanyGroupSOPDocumentCategory>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string sopDocumentCategoryId, bool active);

        void DeleteGraph(string sopDocumentCategoryId);

        GridModel Query(GridParameter parameters);
    }
}