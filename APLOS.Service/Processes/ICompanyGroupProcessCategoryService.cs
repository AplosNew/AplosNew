using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Processes
{
    public interface ICompanyGroupProcessCategoryService : IService<CompanyGroupProcessCategory>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string processCategoryId, bool active);

        void DeleteGraph(string processCategoryId);

        GridModel Query(GridParameter parameters);
    }
}