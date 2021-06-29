using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using System.Collections.Generic;

namespace Library.MaterialManagement.Inventory
{
    public interface IPurchaseOrderServiceService : IService<POService>
    {
        void InsertGraph(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList);
        void InsertGraphFG(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> taxCategoryList); 

        void Delete(string serviceId);

        IEnumerable<object> Query(string receiveId);
        IEnumerable<object> GetTerms(string id);
        IEnumerable<object> GetServicePOTerms(string id);
        IEnumerable<object> GetServiceChargePOServiceList(string id);
        IEnumerable<object> LoadServicePoDetails(string id);

        IEnumerable<object> LoadTaxById(string id);

        
        void InsertGraphPOByReq(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList); 
    }
}