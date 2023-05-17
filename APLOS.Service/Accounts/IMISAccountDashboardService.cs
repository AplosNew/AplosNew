using Library.Core;
using System.Collections.Generic;

namespace Library.Service.Accounts
{
    public interface IMISAccountDashboardService
    {
        IEnumerable<object> OrgStructureList(string companyGroupId, string companyId);

        IEnumerable<ComboModel> MISBudgetCategoryCbo(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string fromDate, string toDate);

        IEnumerable<object> GetBudgetWiseAmountListElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetMasterId, string Activity, string fromDate, string toDate, string voucherId);

        IEnumerable<object> GetBudgetWisevarianceElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType);

        IEnumerable<object> GetBudgetMasterWiseAmountElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string dayOrPeriod, string dateType);

        IEnumerable<object> GetActivityWisevarianceElastic(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string fromDate, string toDate, string bType, string[] budgetMasterId, string budgetCategoryId, string dateType);

        IEnumerable<object> GetBudgetMasterWiseAmount(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string periodName, string dateType, string dayOrPeriod, string PostingPeriodId, string EntryPeriodId);

        IEnumerable<object> GetBudgetMasterWiseExceptionAmount(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string[] budgetMasterId, string fromDate, string toDate, string periodName);

        IEnumerable<object> GetBudgetMasterWiseExceptionAmountDetail(string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string budgetMasterId, string fromDate, string toDate, string periodName);

        #region Independent Entity Combo  and List

        IEnumerable<object> GetEntityDetailFromCompanySelection(string companyId);

        IEnumerable<ComboModel> GetEntityWisePlantCbo(string compnayGroupId, string companyId, string plantId);

        IEnumerable<object> GetEntityDetailFromPlantSelection(string plantId);

        IEnumerable<object> GetEntityWiseEntityCbo(string[] entityList, string compnayGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId);

        IEnumerable<object> GetEntityDetailFromEntitySelection(string entityId);

        IEnumerable<ComboModel> GetEntityWiseDivisionCbo(string compnayGroupId, string companyId, string plantId);

        IEnumerable<object> GetEntityDetailFromDivisionCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId);

        IEnumerable<ComboModel> GetEntityWiseSubDivisionCbo(string compnayGroupId, string companyId, string plantId, string divisionId);

        IEnumerable<object> GetEntityDetailFromSubDivisionCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId, string subDivisionId);

        IEnumerable<ComboModel> GetEntityWiseUnitCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId, string subDivisionId);

        IEnumerable<object> GetEntityDetailFromUnitCbo(string compnayGroupId, string companyId, string plantId, string entityId, string divisionId, string subDivisionId, string unitId);

        IEnumerable<object> GetVoucherLatestDate(string compnayGroupId, string companyId, string plantId, string dateType, string itemType);

        #endregion Independent Entity Combo  and List



        //Syncfuison Dashboard
        IEnumerable<object> GetBudgetCategoryWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType);
        IEnumerable<object> GetBudgetSubCategoryWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType);
        IEnumerable<object> GetBudgetItemWisevarianceElastic(string parameterString, string companyGroupId, string companyId, string plantId, string divisionId, string subDivisionId, string unitId, string budgetCategory, string budgetSubCategory, string budget, string Activity, string fromDate, string toDate, string bType, string dateType);

        #region Balance Sheet Tree View
        IEnumerable<object> GetBalanceSheetInfoGLLevel(string parameterString, string companyGroupId, string companyId, string plantId, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId);
        IEnumerable<object> GetBalanceSheetInfoBudgetLevel(string parameterString, string companyGroupId, string companyId, string plantId, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId);
        IEnumerable<object> GetBalanceSheetInfoActivityLevel(string parameterString, string companyGroupId, string companyId, string plantId, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId);
        IEnumerable<object> GetBalanceSheetInfoVoucherLevel(string companyGroupId, string companyId, string plantId, string date, string GLGeneralInfoId, string BudgetMasterId, string ActivityId);
        #endregion

    }
}