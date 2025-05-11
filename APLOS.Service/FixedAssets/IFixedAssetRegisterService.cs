using Library.Core;
using Library.Model.FixedAsset;
using Library.Model.FixedAssets;
using Library.Model.Materials;
using Library.Service.Core;
using Library.ViewModel.Accounts;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetRegisterService : IService<FixedAssetRegister>
    {
        //IEnumerable<object> GetList();

        GridModel GetSearchData(GridParameter parameters, string[] ids);

        GridModel GetRegisterByMaterialMaster(GridParameter parameters, string companyId, string materialMasterId);

        GridModel GetSearch(GridParameter parameters);

        //IEnumerable<object> GetList(string masterid);

        IEnumerable<object> QueryForAttribute(string fixedAssetRegisterId, string assetItemId);

        void InsertORUpdateItem(FixedAssetRegister fixedAssetRegister, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int numberOfQuantity, string companyCurrencyCode, string companyGroupCurrencyCode, string hardCurrencyCode, out string masterId, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
            , string assetGLId, string assetBudgetId, string assetActivityId);

        void DeleteItem(string masterid);

        IEnumerable<object> GetCbo();

        /// <summary>
        /// GetPriceAndCurrencyById used in FixedAssetAccountDetarmainCurrency where this method call when fixedasset item change dropdown.
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetPriceAndCurrencyById(string id);

        GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string[] fixedAssetRegisterIds);

        IEnumerable<object> GetRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string companyId);
      //  IEnumerable<object> GetOpeningBalanceInfoWithFAMId(string assetMasterId, string companyId);

       // IEnumerable<object> GetOpeningBalanceInfoWithAssetItemId(string assetGLId, string assetBudgetId, string assetActivityId, string companyId);
        IEnumerable<object> GetOpeningBalanceInfoWithBudgetMasterId(string assetBudgetId, string assetActivityId, string companyId, string accDepBudgetMasterId, string accDepActivityId);

        IEnumerable<object> GetRegisterSavedTotalRowWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string companyId);

        IEnumerable<object> GetSavedListById(string assetRegisterIdList);

       // GridModel GetAssetItemList(GridParameter parameters);

        IEnumerable<object> GetSkuWithRegister(string assetItemId, string registerId);

        GridModel GetListForBudgetMaster(GridParameter parameters, string companyId, string[] ids);

        IEnumerable<object> CheckMasterIsRegisterApplyByMaterialMasterId(string assetMasterId);
        IEnumerable<object> GetOBFixedAssetList(string companyId, string plantId);
        void InsertORUpdateItemJVOB(FixedAssetRegister master, int NumberOfQuantity, string CompanyCurrencyCode
           , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId);
        IEnumerable<object> CheckFixedMasterIsRegisterApplyByOBJV(string assetMasterId);
        IEnumerable<object> GetJVOpeningBalanceFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId);
        IEnumerable<object> GetJVOBRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId);

        GridModel GetJVFixedAssetRegisterList(GridParameter parameters, string[] ids);
        IEnumerable<object> GetJVFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId);
        IEnumerable<object> GetJVRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId);
        IEnumerable<object> CheckFixedMasterIsRegisterApplyByJV(string assetMasterId);
        IEnumerable<object> GetJVSubAssetList(string fixedAssetRegisterId);
        void InsertORUpdateItemJV(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode
          , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
          , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId);


        GridModel GetAUCJVFixedAssetRegisterList(GridParameter parameters, string[] ids);
        IEnumerable<object> GetAUCJVFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId);
        IEnumerable<object> GetAUCJVRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId);
        IEnumerable<object> CheckFixedMasterIsRegisterApplyByAUCJV(string assetMasterId);
        IEnumerable<object> GetAUCJVSubAssetList(string fixedAssetRegisterId);
        void InsertORUpdateItemAUCJV(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode
          , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
          , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId);
        IEnumerable<object> GetAUCList(string masterId);
        IEnumerable<object> GetGRNFixedAssetList(string plantId);
        IEnumerable<object> GetGRNCapitalizeFixedAssetGL(string companyId, string inventoryDetailId);
        GridModel GetFixedAssetCapitalizeJournalData(GridParameter parameters, string plantId);
        IWorkbook GetFixedAssetCapitalizeJournalReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType);
        IEnumerable<object> GetIssueAssetAUCList(string plantId);
        IEnumerable<object> GetIssueInventoryAUCList(string plantId);

        GridModel GetCapitalizeAssetItem(GridParameter parameters, string faType);
        IEnumerable<object> GetAUCCIExpenseData(string column, string value, string faType);
        string GetAUCCIExpenseReport(DataTable data, string ReportHeader, string reportFileName);
        void InsertORUpdateCapitalizeAsset(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, decimal NumberOfQuantity, string CompanyCurrencyCode
         , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
         , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail);
        IEnumerable<object> GetNonAssetItem(string faType);
        IEnumerable<object> GetAssetRegisterItemForSubAsset();
        void InsertORUpdateCapitalizeNonAsset(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode
         , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
         , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail);

        void InsertORUpdateCapitalizeSubAsset(IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail);
        IEnumerable<object> GetExpensesRegisterItem(string faType);
        GridModel GetOBFARegisterData(GridParameter parameters, string[] ids);
        IEnumerable<object> GetCapitalizeAssetItemValue(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId);
        //IWorkbook FixedAssetRegisterList(string companyGroupId, string companyId, string plantId, string PartyType, string PartyId, string MaterialMasterId, string FixedAssetsId, string FromDate, string ToDate);
        IWorkbook FixedAssetRegisterList(string companyGroupId, string companyId, string plantId, string MaterialMasterId, string MaterialMasterArticleId, string FixedAssetsId, string vendorId);
        IWorkbook FixedAssetRegisterDisposedList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string nonPosted, string posted, string DisposeStatus);
        // List<Dictionary<string, object>> GetFixedAssetRegisterPopUpList(string column, string value, string companyId);
        //GridModel GetFixedAssetAccDepGL(GridParameter parameters, string companyId);

        string InsertFixedAssetLost(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister);
        string InsertFixedAssetSales(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister);
        string InsertFixedAssetScrap(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister);
        string EditFixedAssetScrap(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister);

        string EditFixedAssetSales(string status, FixedAssetRegisterDisposed disposeVM, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister);
        string EditFixedAssetLost(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister);

        void InsertCapitalizeAssetLost(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegisterDisposedDetailViewModel> assetRegisterList, IEnumerable<FixedAssetRegisterDisposedTaxViewModel> disposedTaxList);
        void DeleteCapitalizeAssetRegisterDisposed(string fixedAssetRegisterDisposedId);
        void DeleteDepreciationProcess(string assetDepreciationId);
        IEnumerable<object> GetCapitalizeAssetRegisterApproveByCbo();
    }
}