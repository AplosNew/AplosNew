using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System.Collections.Generic;

namespace Library.Service.Inventory
{
    public interface IInventoryIssueDetailService : IService<InventoryIssueDetail>
    {
        void InsertRange(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue);

        IEnumerable<object> GetIssueDetailByIssueId(string issueId);
        IEnumerable<object> GetAdjustmentDetailByIssueId(string issueId);

        
        IEnumerable<object> GetIssueWithGl(string companyId, string issueId);
        IEnumerable<object> GetBudgetActivityInIssueMaterial(string materialGroupMasterId);
        IEnumerable<object> GetCostCenterLoadNewFun(string EntityId);

        void RequisitionIssueDetailInsert(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
            , InventoryIssue inventoryIssue, IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails);
        void RequisitionIssueDetailUpdate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
           , InventoryIssue inventoryIssue, IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails);
        void InsertAssetIssueDetail(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue);
        IEnumerable<object> GetSalesDetailByIssueId(string issueId);
        IEnumerable<object> GetScrapDetailByIssueId(string issueId);
        IEnumerable<object> GetBudgetActivityInSalesMaterial(string materialGroupMasterId);
        IEnumerable<object> GetBudgetActivityInScrapMaterial(string materialGroupMasterId);


        IEnumerable<object> GetListForMaterialTransferGridFun(string plantId, string POTypeStatus);

        void MaterialTransferReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);


    }
}