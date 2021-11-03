using Library.Model.Projects;
using Library.Service.Core;

namespace Library.Service.Projects
{
    public interface ICompanyGroupWiseProjectPlanningCategoryService : IService<CompanyGroupWiseProjectPlanningCategory>
    {
        CompanyGroupWiseProjectPlanningCategory FindbyFKId(string key);
    }
}