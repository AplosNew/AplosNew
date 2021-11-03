using Library.Model.Products;
using Library.Service.Core;

namespace Library.Service.Products
{
    public interface IProductMasterAttributeValueService : IService<ProductMasterAttributeValue>
    {
        void DeleteGraph(string key);
    }
}