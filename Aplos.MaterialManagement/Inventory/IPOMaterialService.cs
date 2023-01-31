using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using System.Collections.Generic;

namespace Library.MaterialManagement.Inventory
{
    public interface IPOMaterialService : IService<POMaterial>
    {
        GridModel QueryForPurchaseOrderDetail(GridParameter parameters, string inveReveiveId);
        IEnumerable<object> GetPOTaxUpdateList(string poId);
        GridModel GetPOBOQMAPList(GridParameter parameters, string inveReveiveId);
        GridModel Query(GridParameter parameters, string inveReveiveId);

        IEnumerable<object> GetInventoryMaterialForImprestPayable(string companyId, string plantId, string inveReveiveId);

        IEnumerable<object> GetInventoryMaterialWithoutReversChargePayable(string companyId, string plantId, string inveReveiveId);

        IEnumerable<object> GetInventoryMaterialReversChargePayable(string companyId, string plantId, string inveReveiveId);

        decimal GetStock(InventoryMaterialViewModel entity, string issueDate);

        IEnumerable<object> GetSpecificMaterialStock(InventoryMaterialViewModel entity, string issueDate);

        IEnumerable<object> GetRequisitionList(string issueDetailId);

        void InsertOrUpdateFromReceive(InventoryMaterialViewModel entity);

        POMaterial GetInventoryMaterialByUpToSku(InventoryMaterialViewModel entity);

        IEnumerable<POMaterial> GetInventoryMaterialListByUpToSku(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId);

        void UpdateFromReceive(string id, string receiveDetailId);


        
        GridModel GetInventoryMaterialListPoByReq(GridParameter parameters, string inveReveiveId);
   
        IEnumerable<object> GetInventoryMaterialListForPOUpdate(string inveReveiveId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId);
        IEnumerable<object> GetInventoryMaterialListPoByReqDetail(string inveReveiveId);
    }
}