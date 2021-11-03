using Library.Model.Projects;
using Library.Service.Core;

namespace Library.Service.Projects
{
    public interface ICompanyGroupWiseProjectPlanningSubCategoryService : IService<CompanyGroupWiseProjectPlanningSubCategory>
    {
        CompanyGroupWiseProjectPlanningSubCategory FindbyFKId(string key);
    }
}