using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using Library.ViewModel.Materials;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialMasterService : IService<MaterialMaster>
    {
        IEnumerable<object> GetMaterialMasterCharacteristicsUsingInBOM(string masterId);
        IEnumerable<object> GetPOList(string masterId);
        IEnumerable<object> GetGRNList(string masterId);
        IEnumerable<object> GetBOMList(string masterId);
        bool CheckMaterialCVUsingInBOM(string characteristicsValueId);
        GridModel GetNonAssetMaterialList(GridParameter parameters, string groupId);
        void UpdateDocument(string id);
        Dictionary<string, object> GetDocFile(string id);
        GridModel Query(GridParameter parameters, string groupId);
        GridModel MaterialQueryForGLControl(GridParameter parameters,string groupId);

        IEnumerable<MaterialViewModel> GetBaseUoMConvertionFactorByMaterialMaster(string[] materialMasterIds, string[] alternativeUOMIds);

        //decimal GetAutoSequence();
        decimal GetAutoSequence(string groupId);

        IEnumerable<object> GetMMDefaultSetting(string mmid);

        IEnumerable<object> GetOurStyle(string mmid);

        IEnumerable<object> GetMaterialMaster(string MaterialMasterId);

      //  IEnumerable<object> ValidationByMaterialType(string materialTypeId);

        IEnumerable<object> GetUomCboByMaterialMaster(string[] id);

        IEnumerable<object> GetMaterialMasterAttribute(string masterId);

        IEnumerable<object> GetMaterialMasterAttributeList(string materialMasterId);
        IEnumerable<object> GetMaterialMasterCharacteristics(string masterId);

        GridModel GetCommonMachineListByProcess(GridParameter parameters, string companyGroupId, string[] processIds);

        IWorkbook CreateMaterialLedgerReportSheet(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string Unit);
		//IWorkbook CreateMaterialStockBalanceSheet(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory,string Country);

        IWorkbook CreateMaterialStationeryRequestReport(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory);

        


        IWorkbook CreatePhysicalInventoryReport(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory);
        IWorkbook MaterialMasterStatus(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory); 

        
        IWorkbook CreateMaterialStoreLedger(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId, string Sku1, string Sku2,string Sku3);
        //IWorkbook CreateMaterialStoreLedgerAll(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId);

        IWorkbook CreateMaterialConsumptionReport(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue);

        IEnumerable<object> GetMaterialConsumption(string fromDate, string toDate, string Type);

        IEnumerable<object> GetMaterialConsumptionCostCenter(string fromDate, string toDate, string Type);
        IWorkbook CreateMaterialReceiptsReports(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory);

        IWorkbook CreateMaterialIssueReports(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue);


      //  IWorkbook CreatePurchaseRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate ,string Type);
        IWorkbook CreatePurchaseRegisterReturnReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type);
        IEnumerable<object> GetGRNPendingList(string CompanyId, string GRNPendingStatus);

        void InsertGraph(MaterialMaster entity
           , IEnumerable<MaterialMasterAlternativeUOM> materialMasterAlternativeUOM
           //, IEnumerable<SubMaterial> subMaterialList
           , IEnumerable<MaterialAttributeViewModel> materialMasterAttribute
           , IEnumerable<MaterialAttributeValue> attributeValueList
           , IEnumerable<MaterialMasterCharacteristics> materialMasterCharacteristics
           , IEnumerable<CharacteristicsValue> characteristicsValue
           , IEnumerable<MaterialMasterProcessRouting> materialMasterProcessRouting
		   , IEnumerable<MaterialMasterProcessSet> masterProcessSetList
		   , IEnumerable<MaterialMasterBusinessProcess> businessProcesses
           , IEnumerable<MaterialMasterRevenueBudget> revenuList);

        void UpdateGraph(MaterialMaster entity
           , IEnumerable<MaterialMasterAlternativeUOM> materialMasterAlternativeUOM
           //, IEnumerable<SubMaterial> subMaterialList
           , IEnumerable<MaterialAttributeViewModel> materialMasterAttribute
           , IEnumerable<MaterialAttributeValue> attributeValueList
           , IEnumerable<MaterialMasterCharacteristics> materialMasterCharacteristics
           , IEnumerable<CharacteristicsValue> characteristicsValue
           , IEnumerable<MaterialMasterProcessRouting> materialMasterProcessRouting
		   , IEnumerable<MaterialMasterProcessSet> masterProcessSetList
		   , IEnumerable<MaterialMasterBusinessProcess> businessProcesses
           , IEnumerable<MaterialMasterRevenueBudget> revenuList);

        IEnumerable<object> GetCharacteristicsByMaterialMasterId(string materialMasterId);
        IEnumerable<object> GetCharacteristicsWithoutMaterialSKU1();
        IEnumerable<object> GetCharacteristicsWithoutMaterialSKU2();

        GridModel MaterialMasterSearch(GridParameter parameters, string companyGroupId);

        GridModel MaterialSearchByBusinessProcess(GridParameter parameters, string groupId, string type);

		GridModel GetMaterialListByMaterialType(GridParameter parameters, string groupId, string[] materialType);
        GridModel GetMaterialListByMaterialTypeBOM(GridParameter parameters, string groupId, string materialType);

        IEnumerable<object> GetRevenueBudget(string materialMasterId);

        GridModel GetMaterialMasterDeterminateGL(GridParameter parameters, string companyId);

        GridModel GetMaterialMasterWithFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId);

        GridModel GetMaterialMasterWithFixedAssetMaster(GridParameter parameters);
        GridModel GetMaterialMasterAUCFixedAssetMaster(GridParameter parameters);

        IWorkbook GetMaterialMasterReport(string materialTypeId, bool withSubmaterial);
        GridModel GetATypeAssetAndReconAssetGLWithFixedAssetMaster(GridParameter parameters, string companyId, string fixedAssetMasterId);
        GridModel GetMaterialMasterActiveItemPopUp(GridParameter parameters, string groupId);
        IEnumerable<object> CheckItemArticleSKUList();
        GridModel GetFixedAssetMasterBudgetTagForRegister(GridParameter parameters, string BudgetMasterId, string activityId);
        void MaterialMasterReport2(string MaterialTypeId,bool Article);
        IWorkbook CreateRequisitionRegisterReport(string fromDate,string toDate, string employeeId);
        string PurchaseOrderReportxlx(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName);

    }
}