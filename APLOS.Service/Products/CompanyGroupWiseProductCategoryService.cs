using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Products;
using Library.Service.Core;
using System.Linq;

namespace Library.Service.Products
{
    public partial class CompanyGroupWiseProductCategoryService : Service<CompanyGroupWiseProductCategory>, ICompanyGroupWiseProductCategoryService
    {
        #region Constructor

        public CompanyGroupWiseProductCategoryService(
            IRepositoryAsync<CompanyGroupWiseProductCategory> comgroupdesingationgroupRepository,
            IUnitOfWork unitOfWork) :
            base(comgroupdesingationgroupRepository, unitOfWork)
        {
        }

        #endregion Constructor

        public CompanyGroupWiseProductCategory FindbyFKId(string key)
        {
            return Query(m => m.ProductCategoryId == key && !m.Archive).Select().FirstOrDefault();
        }
    }
}