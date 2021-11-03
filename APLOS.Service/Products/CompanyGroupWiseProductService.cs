using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Products;
using Library.Service.Core;
using System.Linq;

namespace Library.Service.Products
{
    public partial class CompanyGroupWiseProductService : Service<CompanyGroupWiseProduct>, ICompanyGroupWiseProductService
    {
        #region Constructor

        public CompanyGroupWiseProductService(
            IRepositoryAsync<CompanyGroupWiseProduct> comgroupdesingationgroupRepository,
            IUnitOfWork unitOfWork) :
            base(comgroupdesingationgroupRepository, unitOfWork)
        {
        }

        #endregion Constructor

        public CompanyGroupWiseProduct FindbyFKId(string key)
        {
            return Query(m => m.ProductId == key && !m.Archive).Select().FirstOrDefault();
        }
    }
}