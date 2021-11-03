using Library.Model.Productions.Recipe;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Productions.Recipe
{
    public interface IRecipeGlobalUtilityService : IService<RecipeGlobalUtility>
    {
        IEnumerable<object> GetUtility(string id);

        void DelUtilitylList(string subprocessid, out IEnumerable<RecipeGlobalUtility> from_db);

        string GetPK();

        RecipeGlobalUtility GetDetail(string PK);

        IEnumerable<RecipeGlobalUtility> GetDetailList(string SubprocessId);

        IEnumerable<object> GetList(string MasterId);//

        IEnumerable<object> GetDetailById(string id);//GetDetailById

        void OutDetail(RecipeGlobalUtility from_ui, out RecipeGlobalUtility from_db);

        void DeleteUtility(string UtilityId);

        void CheckDuplicate(RecipeGlobalUtility detail_ui, IEnumerable<object> from_db_detailList);

        void DelUtilitylListByOperationId(string operationid, out IEnumerable<RecipeGlobalUtility> from_db);
    }
}