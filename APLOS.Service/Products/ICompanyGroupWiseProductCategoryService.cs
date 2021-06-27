using Library.Model.Products;
using Library.Service.Core;

namespace Library.Service.Products
{
    public interface ICompanyGroupWiseProductCategoryService : IService<CompanyGroupWiseProductCategory>
    {
        CompanyGroupWiseProductCategory FindbyFKId(string key);
    }
}