using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Skills
{
    public interface ICompanyGroupSkillCategoryService : IService<CompanyGroupSkillCategory>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string skillCategoryId, bool active);

        void DeleteGraph(string skillCategoryId);

        GridModel Query(GridParameter parameters);
    }
}