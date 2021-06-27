using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Projects;
using Library.Service.Core;
using Library.Service.Systems;
using System.Linq;

namespace Library.Service.Projects
{
    public partial class CompanyGroupWiseProjectPlanningSubCategoryService : Service<CompanyGroupWiseProjectPlanningSubCategory>, ICompanyGroupWiseProjectPlanningSubCategoryService
    {
        #region Constructor

        public CompanyGroupWiseProjectPlanningSubCategoryService(
            IRepositoryAsync<CompanyGroupWiseProjectPlanningSubCategory> comgroupdesingationgroupRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork) :
            base(comgroupdesingationgroupRepository, unitOfWork)
        {
        }

        #endregion Constructor

        public CompanyGroupWiseProjectPlanningSubCategory FindbyFKId(string key)
        {
            return Query(m => m.ProjectPlanningSubCategoryId == key).Select().FirstOrDefault();
        }
    }
}