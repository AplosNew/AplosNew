using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Projects;
using Library.Service.Core;
using Library.Service.Systems;
using System.Linq;

namespace Library.Service.Projects
{
    public partial class CompanyGroupWiseProjectPlanningCategoryService : Service<CompanyGroupWiseProjectPlanningCategory>, ICompanyGroupWiseProjectPlanningCategoryService
    {
        #region Constructor

        public CompanyGroupWiseProjectPlanningCategoryService(
            IRepositoryAsync<CompanyGroupWiseProjectPlanningCategory> comgroupdesingationgroupRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork) :
            base(comgroupdesingationgroupRepository, unitOfWork)
        {
        }

        #endregion Constructor

        public CompanyGroupWiseProjectPlanningCategory FindbyFKId(string key)
        {
            return Query(m => m.ProjectPlanningCategoryId == key).Select().FirstOrDefault();
        }
    }
}