#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Model.Products;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    /// <summary>
    ///
    /// </summary>
    public interface IProductDefinitionService : IService<ProductDefinition>
    {
        void InsertOrUpdateGraph(IEnumerable<ProductDefinition> entities);
        GridModel Query(GridParameter parameters, string[] tempParam);

        GridModel GetMaterialMasterList(GridParameter parameters, string companyGroupId, string[] searchParam);

        void InsertGraph(ProductDefinition entity, IEnumerable<MaterialMasterArticle> articleList, IEnumerable<ProductDefinitionEfficency> efficencyList);

        void UpdateGraph(ProductDefinition entity, IEnumerable<MaterialMasterArticle> articleList, IEnumerable<ProductDefinitionEfficency> efficencyList);

        void DeleteGraph(string id);

        IEnumerable<ProductDefinitionEfficency> GetEfficencyList(string masterId);
        IEnumerable<object> GetMaterialMasterList(string companyGroupId);
        IEnumerable<object> GetSavedData();
    }
}