#region Using

using Library.Core;
using Library.Model.Products;

using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    public interface IProductCategoryService : IService<ProductCategory>
    {
        IEnumerable<object> GetCbo();

        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters);
    }
}