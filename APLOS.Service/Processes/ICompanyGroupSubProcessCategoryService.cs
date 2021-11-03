using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Processes
{
    public interface ICompanyGroupSubProcessCategoryService : IService<CompanyGroupSubProcessCategory>
    {
        IEnumerable<object> GetCbo();

        void DeleteGraph(string subProcessCategoryId);

        GridModel Query(GridParameter parameters);
    }
}