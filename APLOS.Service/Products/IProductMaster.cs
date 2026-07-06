using Library.Core;
using Library.Model.Products;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Products
{
    public interface IProductMasterService : IService<ProductMaster>
    {
        /// <summary>
        /// Query auto sequence number.
        /// </summary>
        /// <returns>decimal</returns>
        decimal GetAutoSequence();
        GridModel GetPMCbo();
        GridModel GetCbo(string companyGroupId);

        void Insert(ProductMaster entity, IEnumerable<ProductMasterAttributeValue> productMasterAttributeValue, IEnumerable<ProductMasterEfficency> efficencyList, IEnumerable<ProductMasterAlternativeUOM> materialMasterAlternativeUOM);

        IEnumerable<object> ProductMasterWithDetails(string productMasterId);

        IEnumerable<object> ProductMasterComminationData(string productMasterId);

        GridModel Query(GridParameter parameters);
        IEnumerable<ProductMasterEfficency> GetEfficencyList(string masterId);
        IEnumerable<object> GetProductMasterAltUomList(string productMasterId);
    }
}