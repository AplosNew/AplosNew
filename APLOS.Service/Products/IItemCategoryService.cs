#region Using

using Library.Model.Products;

using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    /// <summary>
    /// </summary>
    public interface IItemCategoryService : IService<ItemCategory>
    {
        /// <summary>
        /// Query fixed assets category list for dropdown.
        /// </summary>
        /// <returns>IEnumerable<object></returns>
        IEnumerable<object> GetItemCategoryList();

        /// <summary>
        /// Query auto sequence number.
        /// </summary>
        /// <returns>decimal</returns>
        decimal GetAutoSequence();
    }
}