using Library.Core;
using Library.Model.Productions.Recipe;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Productions.Recipe
{
    public interface IRecipeGlobalOperationService : IService<RecipeGlobalOperation>
    {
        void DelOperationlList(string subprocessid, out IEnumerable<RecipeGlobalOperation> from_db);

        string GetPK();

        IEnumerable<RecipeGlobalOperation> GetDetailList(string MasterId);

        IEnumerable<object> GetList(string MasterId);//

        IEnumerable<object> GetDetailById(string id);//GetDetailById

        void OutDetail(RecipeGlobalOperation from_ui, out RecipeGlobalOperation from_db);

        void CheckDuplicate(RecipeGlobalOperation detail_ui, IEnumerable<object> from_db_detailList);

        void CreateRecipeOperation(RecipeGlobalOperation ui_ob);

        IEnumerable<object> GetOperation(string pk);

        void DeleteOperation(string OperationId);

        IEnumerable<ComboModel> GetGlobalOperationCbo(string RecipeGlobalSubprocessId);
    }
}