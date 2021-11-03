#region

using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Products;
using Library.Service.Core;
using System.Linq;

#endregion

namespace Library.Service.Products
{
    public partial class CompanyGroupWiseProductSubCategoryService : Service<CompanyGroupWiseProductSubCategory>, ICompanyGroupWiseProductSubCategoryService
    {
        #region Constructor

        public CompanyGroupWiseProductSubCategoryService(
            IRepositoryAsync<CompanyGroupWiseProductSubCategory> comgroupdesingationgroupRepository,
            IUnitOfWork unitOfWork) :
            base(comgroupdesingationgroupRepository, unitOfWork)
        {
        }

        #endregion

        public CompanyGroupWiseProductSubCategory FindbyFKId(string key)
        {
            return Query(m => m.ProductSubCategoryId == key && !m.Archive).Select().FirstOrDefault();
        }
    }
}