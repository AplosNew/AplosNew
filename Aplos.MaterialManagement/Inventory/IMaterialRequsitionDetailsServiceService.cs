using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System.Collections.Generic;

namespace Library.MaterialManagement.Inventory
{
    public interface IMaterialRequsitionDetailsServiceService : IService<MaterialRequsitionDetails>
    {
        void InsertOrUpdateGraph(MaterialRequisitionDetailViewModel entity);
		void InsertOrUpdateGraphApprovedQty(IEnumerable<MaterialRequisitionDetailViewModel> entity);

		
		void InsertOrUpdateGraphEdit(MaterialRequisitionDetailViewModel entity);
        void InsertOrUpdateGraphFGForMasterOrder(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> Materialentity, IEnumerable<PurchaseOrderTax> taxCategoryList, IEnumerable<InventoryMaterialViewModel> ServiceEntity, IEnumerable<PurchaseOrderTax> ServicetaxCategoryList);
        
        void InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList);
        void InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId);

        void Delete(string receiveDetailId);
        void UpdateMaterial(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList);
        void UpdateServiceAndTax(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList);

        
    }

}