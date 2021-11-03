using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.MaterialManagement.Inventory
{
    public interface IMaterialRequsitionMasterServiceService : IService<MaterialRequsitionMaster>  
    {
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
        IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy);  
        IEnumerable<object> GetEmployee();
        IEnumerable<object> GetAllReqdata(string ReqStatus);
        IEnumerable<object> GetAllReqdataDetails();//string ReqDetailId
        IEnumerable<object> GetAllReqdataDetailsById(string Id);//string ReqDetailId
        
        IEnumerable<object> GetAllReqdata1(string ReqStatusApproval);
        IEnumerable<object> GetReqMaster(string id);
        IEnumerable<object> GetInventoryMaterialListById(string inveReveiveId);
        GridModel QueryForPurchaseOrderDetail(GridParameter parameters,string inveReveiveId);
       
        void UpdateMaterial(IEnumerable<MaterialRequisitionDetailViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList);
        void DeleteReqDetails(string id);  
        void POClose(string PoId, string PoValue);
        void POUnClose(string PoId, string PoValue);
        void DeleteReq(string id);

        void DailySendMailRequisitionCheck(string addedBy, string ip, string appVersion, string requisitionNo, string RequirmentType, string RequisitionType, string AddedBy);
        void DailySendMailRequisitionApproved(string addedBy, string ip, string appVersion, string RequisitionType, string RequirmentType, string CheckedBy, string ApprovedBy, string PoId, string PreparedBY);

        void DailySendMailRequisitionCreator(string addedBy, string ip, string appVersion, string RequisitionType, string RequirmentType, string CheckedBy, string ApprovedBy, string PoId, string PreparedBY, string PreparedBYId);

        void Insert1(MaterialRequsitionMaster entity);


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

        //void GRNApproved(IEnumerable<PurchaseOrder> entities);

        void PaymentHold(IEnumerable<PurchaseOrder> entities);


       // void GePurchaseOrderReport(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderId);
        //void RequisitionReportby(string CompanyGroupId, string RequisitionId);

        IEnumerable<object> GetListByParty(string partyId, string PartyType); 

        IEnumerable<object> GetPartyPlantCbo(string partyId,string Id);
        IEnumerable<object> GetMaterialDetails(string MaretialDetailsId);
        IEnumerable<object> GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId);
       
       DataTable GetPurchaseOrderSqlData(string purchaseOrderId);

        //void GePurchaseOrderReport(string companyGroupId);
        void RequisitionReportby(string CompanyGroupId, string PlantId, string RequisitionId, string startDate, string endDate, string empId);  
        

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


    }
}