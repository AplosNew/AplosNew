#region Using

using Library.Model.Products;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    /// <summary>
    /// </summary>
    public interface IProductGroupService : IService<ProductGroup>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <returns></returns>
        IEnumerable<object> GetProductGroupList();

        /// <summary>
        /// Query auto sequence number.
        /// </summary>
        /// <returns>decimal</returns>
        decimal GetAutoSequence();
    }
}