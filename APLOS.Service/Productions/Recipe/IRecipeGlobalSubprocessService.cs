using Library.Model.Productions.Recipe;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Productions.Recipe
{
    public interface IRecipeGlobalSubprocessService : IService<RecipeGlobalSubprocess>
    {
        string GetPK();

        IEnumerable<RecipeGlobalSubprocess> GetDetailList(string MasterId);

        IEnumerable<object> GetList(string MasterId);//

        IEnumerable<object> GetDetailById(string id);//GetDetailById

        void OutDetail(RecipeGlobalSubprocess from_ui, out RecipeGlobalSubprocess from_db);

        void CreateRecipeSubprocess(RecipeGlobalSubprocess recipeSubprocess);

        void CheckDuplicate(RecipeGlobalSubprocess detail_ui, IEnumerable<object> from_db_detailList);

        void DeleteDetail(string detailid);
    }
}