using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System.Collections.Generic;

namespace Library.MaterialManagement.Inventory
{
    public interface IPurchaseOrderDetailService : IService<PurchaseOrderDetail>
    {
        void InsertOrUpdateGraph(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList);
         
        void InsertOrUpdateGraphFGForMasterOrder(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> Materialentity, IEnumerable<PurchaseOrderTax> taxCategoryList, IEnumerable<InventoryMaterialViewModel> ServiceEntity, IEnumerable<PurchaseOrderTax> ServicetaxCategoryList);
        
        void InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList);
        void InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId);

        void Delete(string receiveDetailId,string OrderSpecific);
        void UpdateMaterial(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList);
        void UpdateServiceAndTax(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList); 



        void InsertOrUpdateGraphPoByReq(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId);
        void InsertOrUpdateGraphPoUpdateByReq(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId);
        
        void DeletePOByReq(string receiveDetailId);


        //void DetailCreateServicePOByReq(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId);

        //void InsertServicePODetailByReq(ServicePOMaster entity);

        void InsertServicePODetailByReq(IEnumerable<ServicePODetailsViewModel> entity,string ServicePoMasterId, IEnumerable<ServicePOTax> taxCategoryList);
        void InsertServicePODetail(ServicePODetail entity, string ServicePoMasterId, IEnumerable<ServicePOTax> taxCategoryList);

        void GetUpdateServicePOTax(IEnumerable<ServicePOTaxViewModel> receiveTaxList, string ServicePODetailId, string servicePOid);

        void InsertOrUpdateGraphPoForBOQItem(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId);
        void InsertOrUpdateGraphPoForBOQItemUpdate(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId);
        void DeletePOMaterial(string receiveDetailId, string OrderSpecific);

    }

}