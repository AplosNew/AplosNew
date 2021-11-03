using Library.Model.Products;
using Library.Service.Core;

namespace Library.Service.Products
{
    public interface ICompanyGroupWiseProductService : IService<CompanyGroupWiseProduct>
    {
        CompanyGroupWiseProduct FindbyFKId(string key);
    }
}