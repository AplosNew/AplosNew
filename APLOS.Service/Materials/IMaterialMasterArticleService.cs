#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialMasterArticleService : IService<MaterialMasterArticle>
    {
        IEnumerable<object> Query(string materialMasterId);
        void Comapare(List<MaterialMasterArticleNew> allArticles, List<MaterialMasterArticleValue> currentArticles);
        GridModel GetMaterialArticle(GridParameter parameters, string materialMasterId);
        IEnumerable<object> GetMaterialArticle(string materialMasterId, string[] materialType);
        /// <summary>
        /// use : Product definition
        /// </summary>
        /// <param name="materialMasterId"></param>
        /// <returns></returns>
        IEnumerable<object> GetArticlListByMaterialMaster(string materialMasterId);

        IEnumerable<object> GetMaterialArticleValue(string articleId);

        IEnumerable<object> GetAttributeValueList(string materialMasterId);

        IEnumerable<object> GetArticlValueHead(string materialMasterId);

        void InsertOrUpdateGraph(IEnumerable<MaterialMasterArticle> subMaterials, string materialCode);

        void UpdateGraph(IEnumerable<MaterialMasterArticle> articles);

        void DeleteGraph(string materialMasterId);

        void Delete(string articleId);

        void ProcessInsertGraph(string productDefinitionId, IEnumerable<MaterialMasterArticle> articleList);

        void DeleteArticleProcess(string id);

        void DeleteArticleProcessGraphByProductDefinition(string id);
        IEnumerable<object> getArticleAliaslist(string articleId, string masterOrderItemId);
        void deleteArticleAliasData(string Id);
    }
}