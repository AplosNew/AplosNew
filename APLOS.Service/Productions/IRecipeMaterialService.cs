using Library.Model.Productions;
using Library.Service.Core;
using System.Collections.Generic;
using Library.Core;

namespace Library.Service.Productions
{
    public interface IRecipeMaterialService : IService<RecipeMaterial>
    {
        string GetPK();

        //RecipeRawMaterial GetDetail(string PK);
        bool ShouldValidation(string RecipeGlobalMasterId, string MaterialMasterId,string articleId);
        string GetMaterialAtricleName(string RecipeGlobalMasterId, string MaterialMasterId, string articleId);
        IEnumerable<object> GetRecipeMaterialListNew(string masterId);

        IEnumerable<ComboModel> GetRecipeCbo(string entityId);

        void DeleteRecipeMaterial(string id);

        IEnumerable<RecipeMaterial> GetDetailList(string MasterId);

        IEnumerable<object> GetList(string MasterId);

        IEnumerable<RecipeMaterial> GetDetailListByMasterId(string RecipeMasterId);

        IEnumerable<object> GetDetailById(string pk);
    }
}