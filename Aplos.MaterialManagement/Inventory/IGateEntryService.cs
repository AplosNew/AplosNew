using Library.Core;
using Library.Model.Inventory;
using Library.Model.Products;
using Library.Service.Core;
using Library.ViewModel.Inventory;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.MaterialManagement.Inventory
{
    public interface IGateEntryService : IService<GateEntry>   
    {
        //Pass
        IEnumerable<object> GetPOMasterById(string plantId, string id);
        GridModel Query(GridParameter parameters, string plantId);

        GridModel GetPostingList(GridParameter parameters, string plantId);

        IEnumerable<object> GetListForHold(string plantId);
        IEnumerable<object> GetListForAllPOList(string plantId);

        IEnumerable<object> GetListForHold1(string plantId);

        IEnumerable<object> GetListForPOApproval1UnApproved(string plantId);
        
        IEnumerable<object> GetListForPOApproval1Auth(string plantId);
        
        //IEnumerable<object> GetListForallPo(string plantId);
        //IEnumerable<object> GetListForHold(string plantId);
        IEnumerable<object> GetListForPOApproval(string plantId);
        IEnumerable<object> GetListForPOApprovalAuthorized(string plantId); 

        
       // IEnumerable<object> GetSupervisorCbo();
        IEnumerable<object> GetEntity();
        IEnumerable<object> GetSupervisorCbo();
        IEnumerable<object> GetEmployee();
        IEnumerable<object> GetAllReqdata(string IsSysAdmin, string UserId, string plantId);
        IEnumerable<object> PlantWiseGateCbo(string IsSysAdmin, string UserId, string plantId);
        IEnumerable<object> GetReqMaster(string id);
        GridModel QueryForPurchaseOrderDetail(GridParameter parameters, string inveReveiveId);
        void UpdateMaterial(IEnumerable<MaterialRequisitionDetailViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList);
        void DeleteReqDetails(string id);  
        void POClose(string PoId, string PoValue);
        void POUnClose(string PoId, string PoValue);
        void DeleteReq(string id);
        void DeleteGateEntry(string id);
        void DeleteGatePass(string id);
        void CancelGateEntry(string id);
        void Insert(GateEntry entity, string PlantWiseGateId);
        void InsertGatePass(GatePassMaster entity, string PlantWiseGateId);
        void UpdateGatePass(GatePassMaster entity); 

        GridModel GetEmployeePurchaseList(GridParameter parameters, string plantId);
        IEnumerable<object> GetListForPOClose(string plantId);
        IEnumerable<object> GetListForPOUnClose(string plantId);

        GridModel GetListForInvPayable(GridParameter parameters, string plantId);

        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId);
      
        
        IEnumerable<object> GetReceiveTaxList(string receiveDetailId);

        IEnumerable<object> GetTotalReceiveTaxList(string receiveId);

        IEnumerable<object> GetServiceTaxList(string serviceId);

        decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId);

        decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable);

        //void Delete(string id);
        void DeleteMaterialTax(string id); 

        IEnumerable<object> GetListByParty(string partyId, string PartyType); 

        IEnumerable<object> GetPartyPlantCbo(string partyId,string Id);
        IEnumerable<object> GetMaterialDetails(string MaretialDetailsId);
        IEnumerable<object> GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId);
       
       DataTable GetPurchaseOrderSqlData(string purchaseOrderId);

        //void GePurchaseOrderReport(string companyGroupId);
        void GateEntryReport(string CompanyGroupId, string plantId, string RequisitionId);
        //void GateEntryReport(string CompanyGroupId, string plantId, string RequisitionId);


        #region shajazan PO Approval
        void PoApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy);
        void PoUnApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy);


        
        //void PoAllListinC(string PoId, string PoValue);
        void PoApproved1(string PoId, string PoValue);
        void PoApproved1Auth(string PoId, string PoValue);
        
        #endregion
        void PoApprovedAuth(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy); 





        #region FGForMasterOrder 22-Jun-2019


        IEnumerable<object> GetListForMasterOrder(string CompanyId);
        IEnumerable<object> GetMasterItemList(string masterOrderId);
        IEnumerable<object> getTaxCategoryListForFGService(string companyGroupId, string plantId, string hsnCodeId,string partyPlantId);
        //object GetOperationMaster();
        #endregion

        GridModel EmployeeListByDepartment(GridParameter parameters, string DepartmentId);
        void InsertOrUpdateGraph(GatePassDetailsViewModel entity, string ChallanNo);
        void InsertOrUpdateGraphDispatch(IEnumerable<GatePassDetailsViewModel> entity, string ChallanNo, string MasterId); 
        GridModel QueryForGatePassDetail(GridParameter parameters, string inveReveiveId,string GatePassNewId);
        void DeleteGatePassDEtails(string id);

        
    }
}