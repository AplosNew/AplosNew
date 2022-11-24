using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System.Collections.Generic;

namespace Library.MaterialManagement.Inventory
{
    public interface IInventoryMaterialService : IService<InventoryMaterial>
    {
        GridModel QueryForPurchaseOrderDetail(GridParameter parameters, string inveReveiveId);
		GridModel Querywithoutpo(GridParameter parameters, string inveReveiveId);

		GridModel Query(GridParameter parameters, string inveReveiveId,string POID, string AcceptanceId); 
		GridModel QueryBOQ(GridParameter parameters, string inveReveiveId,string POID, string AcceptanceId); 
         IEnumerable<object> GRNDetailsData( string inveReveiveId, string POID);
        //IEnumerable<object> JWGRNDetailsData(string inveReveiveId, string POID);
        IEnumerable<object> PurchaseReturnDetailsData(string PurchaseReturnId, string POID);

        GridModel Query1(GridParameter parameters, string inveReveiveId);
		
        GridModel GetIssueMaterial(GridParameter parameters, string issueId,string companyId);
        GridModel GetPayableRejectMaterial(GridParameter parameters, string inveReveiveId);
        GridModel GetPayableShortageMaterial(GridParameter parameters, string inveReveiveId);
        IEnumerable<object> GetVendorPayableGLBudgetActivity(string receiveId, string companyId, string plantId);


        IEnumerable<object> GetInventoryMaterialListForPOUpdate(string inveReveiveId, string InventoryReceiveId, string MaterialMasterId,string InventoryReceiveDetailId);
     


        Dictionary<string, object> GetStock(InventoryMaterialViewModel entity, string issueDate);

        Dictionary<string, object> GetMaterialTransferStock(InventoryMaterialViewModel entity, string issueDate);


        Dictionary<string, object> GetStockCountryWise(InventoryMaterialViewModel entity, string issueDate);

        
        IEnumerable<object> GetSpecificMaterialStock(InventoryMaterialViewModel entity, string issueDate);
        IEnumerable<object> GetSpecificMaterialTransferStock(InventoryMaterialViewModel entity, string issueDate); 

        
        IEnumerable<object> GetSpecificMaterialStockForAdjustment(InventoryMaterialViewModel entity, string issueDate); 

        IEnumerable<object> GetApprovedStockDetail(InventoryMaterialViewModel entity, string issueDate);
        IEnumerable<object> GetApprovedStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate);
        IEnumerable<object> GetUnApprovedStockDetail(InventoryMaterialViewModel entity, string issueDate);
        IEnumerable<object> GetUnApprovedStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate);
        IEnumerable<object> GetPostingStockDetail(InventoryMaterialViewModel entity, string issueDate);
        IEnumerable<object> GetPostingStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate);
        IEnumerable<object> GetRequisitionList(string issueDetailId);

        void InsertOrUpdateFromReceive(InventoryMaterialViewModel entity);

        InventoryMaterial GetInventoryMaterialByUpToSku(InventoryMaterialViewModel entity);

        IEnumerable<InventoryMaterial> GetInventoryMaterialListByUpToSku(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId);
        IEnumerable<InventoryMaterial> GetJWInventoryMaterialListByUpToSku(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId);
        void UpdateFromReceive(string inventoryMaterialId, string receiveDetailId);
        IEnumerable<InventoryMaterial> GetInventoryIssueMaterialListByUpToSku(IEnumerable<RequisitionIssueDetailViewModel> entities, string companyId, string plantId);
        IEnumerable<object> GetInventoryTaxList(string inveReveiveId);

        Dictionary<string, object> GetStockForPhysicalStock(InventoryMaterialViewModel entity, string issueDate);

        IEnumerable<object> GetSpecificMaterialStockForPhysicalStock(InventoryMaterialViewModel entity, string issueDate);
        IEnumerable<InventoryMaterial> GetInventoryMaterialListByUpToSkuSales(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId);
        IEnumerable<InventoryMaterial> GetInventoryMaterialListByUpToSkuScrap(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId);  

        Dictionary<string, object> GetStockSales(InventoryMaterialViewModel entity, string issueDate);
        Dictionary<string, object> GetStockScrap(InventoryMaterialViewModel entity, string issueDate);



        IEnumerable<object> GetPopUpShowStorageLocation(InventoryMaterialViewModel entity, string issueDate);

        IEnumerable<object> StorageLocationStockWise(string MaterialMstId, string ArticleId, string issueDate);

        void JWInsertOrUpdateFromReceive(InventoryMaterialViewModel entity);
        InventoryMaterial JWGetInventoryMaterialByUpToSku(InventoryMaterialViewModel entity);
        IEnumerable<object> JWOutPutQuery(string inveReveiveId);

        IEnumerable<object> JobWorkOutPutQuery(string inveReveiveId);
        IEnumerable<object> JWByProductQuery(string inveReveiveId);

        IEnumerable<object> JobWorkByProductQuery(string inveReveiveId);

        Dictionary<string, object> GetJWStock(InventoryMaterialViewModel entity, string issueDate); 
    }
}