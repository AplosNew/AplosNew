using Library.Model.Productions.Recipe;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Productions.Recipe
{
    public interface IRecipeGlobalRawMaterialService : IService<RecipeGlobalRawMaterial>
    {
        void DeleteRecipeGlobalMaterialGroup(string id);
        bool RecipeGlobalMaterialGroupValidation(string subpProcessId, string recipeMaterialGroupingMasterId);
        void CreateRecipeGlobalMaterialGroup(RecipeGlobalMaterialGroup entity);
        bool ShouldValidation(string RecipeGlobalMasterId, string MaterialMasterId, string articleId, string subpProcessId);
        IEnumerable<object> GetMaterialMaster(string mmid);

        void DeleteRawMaterial(string rawMaterialId);

        void CreateRecipeRawMaterial(RecipeGlobalRawMaterial ui_ob);

        void DelRawMaterialList(string subprocessid, out IEnumerable<RecipeGlobalRawMaterial> from_db);

        string GetPK();

        RecipeGlobalRawMaterial GetDetail(string PK);

        IEnumerable<RecipeGlobalRawMaterial> GetDetailList(string MasterId);

        IEnumerable<object> GetList(string MasterId);//

        IEnumerable<object> GetDetailById(string id);//GetDetailById

        void OutDetail(RecipeGlobalRawMaterial from_ui, out RecipeGlobalRawMaterial from_db);

        void CheckDuplicate(RecipeGlobalRawMaterial detail_ui, IEnumerable<object> from_db_detailList);

        void DelRawMaterialListByOperationId(string OperationId, out IEnumerable<RecipeGlobalRawMaterial> from_db);

        void DelRawMaterialListByUtilityId(string UtilityId, out IEnumerable<RecipeGlobalRawMaterial> from_db);
    }
}