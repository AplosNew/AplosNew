using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;

namespace Library.Service.Inventory
{
    public interface IPurchaseDocumentAcceptanceService : IService<PurchaseDocAcceptance>
    {
        IEnumerable<object> GetGRNList(string plantId, string purchaseLCId);
        void SaveServiceChargesAndChargesTax(IEnumerable<PurchaseDocAcceptanceChargesViewModel> AcceptancechargesList, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptancechargesTax, PurchaseDocAcceptance entity);
        void SaveServiceAndServiceTax(IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax, string PurchaseDocAcceptanceId);
        IEnumerable<object> GetIsAccepptanceFirstData(string masterId, string plantId);
        void SaveMaterialTax(IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceTax, string PurchaseDocAcceptanceId);
        void SaveOrUpdateServiceTax(IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceTax, string PurchaseDocAcceptanceId, string PurchaseDocAcceptanceServiceId);

        IEnumerable<object> GetServiceTaxList(string serviceId);
        IEnumerable<object> GetPurchaseDocAcceptanceService(string purchaseDocAcceptanceId);
        void InsertOrUpdatePurchaseDocAcceptanceService(PurchaseDocAcceptanceService entity, IEnumerable<PurchaseDocAcceptanceTax> taxCategoryList);
        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId);
        IEnumerable<object> GetPOWithLCList(string plantId, string PoType);

        IEnumerable<object> LCDetails(string plantId, string LCID);

        IEnumerable<object> GetLCWisePOList(string plantId, string PoType, string PurchaseLCNo);

        IEnumerable<object> GetRecordDoubleClickMaster(string plantId, string Id, string PoType);
        void Delete(string id, string POID, string PODetailsID, decimal Qty);
        void DeleteACPOmapTabledata(string id, string POID, string PODetailsID, string Qty);


        IEnumerable<object> GetRecordDoubleClickDetail(string plantId, string Id, string PoType);


        GridModel QueryOnlyPO(GridParameter parameters, string inveReveiveId);
        IEnumerable<object> GetAcceptanceCharges();
        void InsertOrUpdateGraphNew(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail
            //, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceTax
            //, IEnumerable<PurchaseDocAcceptanceChargesViewModel> AcceptancechargesList, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptancechargesTax
            //, IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax
            );

        void InsertOrUpdate(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail
           , IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceServiceDetail
            , IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax
            );

        IEnumerable<object> GetAcceptanceList(string plantId);

        IEnumerable<object> GetAcceptanceDetailList(string plantId, string Id);


        IEnumerable<object> GetMaterialById(string Id, string plantId);

        IEnumerable<object> GetAcceptanceServiceList(string plantId, string Id);
        IEnumerable<object> GetAcceptanceChargesTaxList(string plantId, string Id);
        IEnumerable<object> GetPurchaseDocAcceptanceTax(string Id);
        IEnumerable<object> GetPurchaseDocAcceptanceServiceTax(string Id);

        void InsertOrUpdateServicePOAcceptance(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail);
        #region Document Acceptance Posting
        IEnumerable<object> GetAcceptanceDetailForPost(string companyId, string plantId, string Id, string PoType);
        IEnumerable<object> GetAcceptanceServiceListForPost(string plantId, string Id);
        #endregion
    }
}
