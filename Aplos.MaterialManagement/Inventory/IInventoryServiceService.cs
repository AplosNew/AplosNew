using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using System.Collections.Generic;

namespace Library.MaterialManagement.Inventory
{
    public interface IInventoryServiceService : IService<InventoryService>
    {
        void InsertGraph(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList); 
        void InsertGraphUpdate(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList); 
        void InsertGraphNew(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList,string id, string AcceptanceId);
        void InsertGraphNewEdit(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string id); 
        void Delete(string serviceId);

        IEnumerable<object> Query(string receiveId);
        IEnumerable<object> QueryPurchaseReturnCharges(string receiveId);
       
        IEnumerable<object> Query1(string receiveId,string AcceptanceId);
        IEnumerable<object> getTCSData(string receiveId);
    }
}