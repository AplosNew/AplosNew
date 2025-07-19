using Library.Model.Inventory;
using Library.Model.Products;
using Library.Service.Core;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System.Collections.Generic;

namespace Library.MaterialManagement.Inventory
{
    public interface IInventoryReceiveDetailService : IService<InventoryReceiveDetail>
    {
        void InsertOrUpdateGraphNew(InventoryReceive entity,IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList,string id,string MaterialStorageId,string GRNType, IEnumerable<GRNPORequisitionMap> requisitionDetailList, IEnumerable<GRNBinAllocationMap> grnBinAllocationMap);
        void UpdateGRNBYPOMaster(InventoryReceive entity, string GRNType);
        void InsertOrUpdateGraphNewGRNBOQ(InventoryReceive entity,IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList,string id,string MaterialStorageId,string GRNType, IEnumerable<InventoryMaterialViewModel> BOQAllocationSave);
        void BOQInsertOrUpdateGraphNew(InventoryReceive entity,IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList,string id,string MaterialStorageId,string GRNType,List<InventoryMaterialViewModel> List);
        void InsertFOCDetail(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> List);
        void UpdateFOCDetail(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType);
        void InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList);
        void UpdateGRNBOQTax(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList);
        void InsertOrUpdateGraphNewEdits(InventoryReceive entity,IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId,string GRNType);
		void InsertOrUpdateGraphNewEditsOnlyGRN(IEnumerable<InventoryMaterialViewModel> entityMat,string Id);
		
        void InsertOrUpdateGraph(InventoryMaterialViewModel entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, IEnumerable<GRNBinAllocationMap> gRNBinAllocationMapList);
        void InsertFOCMaterial(InventoryMaterialViewModel itemDetail, IEnumerable<InventoryReceiveTax> taxCategoryList);
        void Delete(string receiveDetailId);
        void JWDelete(string receiveDetailId);
        void InsertOrUpdateGraphForPurchaseReturn(PurchaseReturn entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<PurchaseReturnTax> taxCategoryList, IEnumerable<GRNPORequisitionAllocation> grnBoqList, string id, string MaterialStorageId, string GRNType,IEnumerable<InventoryMaterialViewModel> chargesList, IEnumerable<PurchaseReturnTax> ServicetaxCategoryList); 
        void DeletePurchaseReturnRow1(string PurchaseReturnDetailId, string inventoryReceiveDetailId, string InventoryMaterial, decimal Trasantionqty);
        IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy);
        IEnumerable<object> GetProductionRecipeMaterialList(string productionOrderId);
        IEnumerable<object> GetProcessByProductionOrder(string productionOrderId); 
        
        IEnumerable<object> GetCheckedByAndApprovedBYForPurchaserReturn(string CheckedBy, string ApprovedBy);
        
        void OSReceiptGRNInsertOrUpdateGraphNew(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> entityMatByProduct);

        // Job Work Receipt
        void JobWorkInsertOrUpdateNew(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> entityMatByProduct);


        void IssueSlipDelete(string receiveDetailId);
        void IssueSlipDeleteFn(string receiveDetailId);
        void GRNBOQDetailDelete(string receiveId, string receiveDetailId);
    }
}