using Library.Model.Products;
using Library.Service.Core;

namespace Library.Service.Products
{
    public interface ICompanyGroupWiseProductSubCategoryService : IService<CompanyGroupWiseProductSubCategory>
    {
        CompanyGroupWiseProductSubCategory FindbyFKId(string key);
    }
}