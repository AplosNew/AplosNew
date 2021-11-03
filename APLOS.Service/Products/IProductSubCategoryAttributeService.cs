#region Using

using Library.Model.Products;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    /// <summary>
    /// IProductSubCategoryAttributeService.
    /// </summary>
    public interface IProductSubCategoryAttributeService : IService<ProductSubCategoryAttribute>
    {
        IEnumerable<object> GetSearchData(string productSubCategoryId);

        void Insert(IEnumerable<ProductSubCategoryAttribute> entites);

        IEnumerable<object> GetAttribute(string productSubCategoryId, string productMasterId);

        void DeleteGraph(string productSubCategoryId);
    }
}