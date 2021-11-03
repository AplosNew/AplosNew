using Library.Core;
using Library.Model.Productions.Recipe;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.Productions.Recipe
{
    public interface IRecipeGlobalMasterService : IService<RecipeGlobalMaster>
    {
        IEnumerable<ComboModel> GetRecipeMaterialGroupingMasterMeasurementCbo(string recipeMaterialGroupingMasterId);
        IEnumerable<object> GetRecipeGlobalMaterialGroup(string recipeGlobalSubprocessId);
        IEnumerable<object> GetRecipeMaterialGroup();
        IWorkbook GetRecipeReport(out string reportFileName, string mmId,string companyGroupId, string companyId, string plantId);
        IEnumerable<ComboModel> GetMeasurementCbo(string materialMasterId);
        GridModel GetMaterialArticle(GridParameter parameters, string materialMasterId);
        IEnumerable<ComboModel> GetSubProcessCbo(string companyGrupId, string ProcessId);
        IEnumerable<ComboModel> GetProcessCriteriaCbo(string companyGrupId);
        IEnumerable<object> RecipeDetailsUsedListList(string recipemasterId);

        IEnumerable<object> GetRecipeRawMaterialList(string masterId);
        GridModel GetEntityProductionProcessCbo(bool cadmin, bool sadmin, string userId, string entityId);
        IEnumerable<ComboModel> GetMaterialAttributeCbo();

        IEnumerable<ComboModel> GetCharacteristicsCbo();

        IEnumerable<ComboModel> GetMaterialMasterCbo();

        IEnumerable<ComboModel> GetRecipeOperationCbo(string processId);

        IEnumerable<ComboModel> GetUnitOfMeasurementCbo();
        GridModel RecipeGlobalMasterList(GridParameter parameters, string entityId, string processId);

        IEnumerable<ComboModel> GetRecipeCbo(string recipeId);

        IEnumerable<object> GetMasterId(string materialmasterid);

        IEnumerable<object> GetSkuAsperConfig(string entityid, string MaterialMasterId);

        void InsertORUpdateMaster(RecipeGlobalMaster master, out string masterid);

        void InsertORUpdateDetail(RecipeGlobalUtility recipeSubprocess);

        IEnumerable<object> GetList(string masterid, string companyGroupId, string companyId);

      IEnumerable<object> GetRecipeConfigData(string plantId,string processId);

        GridModel GetListByMMId(GridParameter parameters, string materialmasterid);
        
        void DeleteRecipe(string masterid);

        IEnumerable<object> GetRecipeByPOCbo(string pomid);
    }
}