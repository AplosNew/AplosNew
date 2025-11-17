using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.SalesManagements;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;

namespace Library.MaterialManagement.Inventory
{
    public interface IInventoryIssueService : IService<InventoryIssue>
    {
        GridModel Query(GridParameter parameters, string plantId);

        IEnumerable<object> GetIssueList(string column, string value, string plantId); 
        GridModel GetInventoryIssueReturnListForPosting(GridParameter parameters, string plantId); 

        void InsertGraph(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll);
        void InsertGraphBOQ(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll, List<InventoryIssueHistoryBOQ> BoqAllocationListVM);

       
        

        void InsertGraphInventorySales(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventorySales inventoryIssue, string IssueTypeStatus,  string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<InventorySalesTax> taxCategoryList,string productNewId, decimal ToCurrencyRate); 

        void InsertGraphIssueReturn(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssueReturn inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryIssueReturnHistoryBOQ> issueboqList);


        IWorkbook CreateIssueRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate,string Type);
        DataTable GetIssueRegisterBYGRN(string fromDate, string toDate, string Type);
        // Out Source
        IWorkbook CreateOSIssueRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type);

        IWorkbook CreateIssueReturnRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type);

        IWorkbook CreateIssueRegisterGRNIssueReport(string companyId, string plantId, string fromDate, string toDate, string Type);

        // Out source
        IWorkbook CreateOSIssueRegisterGRNIssueReport(string companyId, string plantId, string fromDate, string toDate, string Type);


        void DeleteIssueDetail(string issueDetailId, string voucherId);
        void DeleteSalesDetail(string issueDetailId);
        void DeleteIssueDetailBOQ(string issueDetailId, string voucherId);
        void RequisitionIssueInsert(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
            , InventoryIssue inventoryIssue, IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails);
        void RequisitionIssueUpdate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
           , InventoryIssue inventoryIssue, IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails);

        GridModel GetAssetInventoryIssue(GridParameter parameters, string plantId);

        void InsertAssetInventoryIssue(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue);


        void NonPostedIssueDelete(string issueId);
        void PostedIssueDelete(string issueId);
        void InsertPhysicalStockAdjustment(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, PhysicalStockAdjustmentMaster inventoryIssue, string IssueTypeStatus);

        IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy);
        IEnumerable<object> GetCheckedByAndApprovedBYScrap(string CheckedBy, string ApprovedBy);
        void InsertGraphInventoryScrap(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryScrap inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti);

        void InsertGraph(InventoryMaterialViewModel entity, IEnumerable<InventorySalesTax> taxCategoryList);

        void ServiceChargesDelete(string serviceId);

        void MaterialTransferCreateInsertGraph(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryReceive inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti);
        void JWInsertGraph(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll, string TabType);

        // Job Work Issue Save

        void JobWorkIssueCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll, string TabType);

        void SalesReturnInsert(InventorySalesReturn inventoryIssue, IEnumerable<InventorySalesReturnDetailViewModel> entities, IEnumerable<SalesReturnTaxViewModel> salesReturnTaxList, IEnumerable<InventorySalesReturnServiceViewModel> salesServiceVMList);
        void SalesReturnUpdate(InventorySalesReturn inventoryIssue, IEnumerable<InventorySalesReturnDetailViewModel> entities, IEnumerable<SalesReturnTaxViewModel> salesReturnTaxList, IEnumerable<InventorySalesReturnServiceViewModel> salesServiceVMList);
        void UpdateIssueMaster(InventoryIssue inventoryIssue);
    }
}