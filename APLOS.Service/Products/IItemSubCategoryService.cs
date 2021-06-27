#region Using

using Library.Model.Products;

using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    /// <summary>
    ///
    /// </summary>
    public interface IItemSubCategoryService : IService<ItemSubCategory>
    {
        /// <summary>
        /// Query item SubCategory list for dropdown.
        /// </summary>
        /// <returns>IEnumerable<object></returns>
        IEnumerable<object> GetItemSubCategoryList();

        /// <summary>
        /// Query auto sequence number.
        /// </summary>
        /// <returns>decimal</returns>
        decimal GetAutoSequence();
    }
}