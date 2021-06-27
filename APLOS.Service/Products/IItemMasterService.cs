using Library.Model.Products;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Products
{
    public interface IItemMasterService : IService<ItemMaster>
    {
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetItemMasterList();

        /// <summary>
        /// ItemMasterDetailViewModel
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        IEnumerable<ItemMaster> GetAllById(string Id);
    }
}