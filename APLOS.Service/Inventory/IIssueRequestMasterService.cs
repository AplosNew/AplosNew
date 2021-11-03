using Library.Core;
using Library.Model.Inventory;
using Library.Model.Products;
using Library.Service.Core;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;

namespace Library.Service.Inventory
{
    public interface IIssueRequestMasterService : IService<IssueRequestMaster>    
    {
        GridModel Query(GridParameter parameters, string plantId);        
        IEnumerable<object> QueryGetListForMasterData(string plantId);
        GridModel GetListByGrnno(GridParameter parameters, string plantId, int GRN);        
        IEnumerable<object> GetListForHold(string plantId);
        IEnumerable<object> GetListOfPOGateEntry(string CompanyGroupId, string CompanyId, string PlantId, string partyCode); 
        GridModel GetPostingList(GridParameter parameters, string plantId);
        void Insert(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMaterial);
        GridModel GetListForHold(GridParameter parameters, string plantId);
		GridModel GetEmployeePurchaseList(GridParameter parameters, string plantId);
        GridModel GetListForInvPayable(GridParameter parameters, string plantId);
        IEnumerable<object> GetListForInvShortagePayable(string plantId);
        IEnumerable<object> GetListForInvRejectPayable(string plantId);
        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId);

        IEnumerable<object> GetReceiveTaxList(string receiveDetailId);
        IEnumerable<object> GetReceiveTaxListPO(string receiveDetailId);
        IEnumerable<object> GetOperationMaster();
        IEnumerable<object> GetTotalReceiveTaxList(string receiveId);

        IEnumerable<object> GetServiceTaxList(string serviceId);
        IEnumerable<object> GetServiceTaxListPO(string serviceId);
       

        decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId);

        decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable);

        void Delete(string id);

		void GRNApproved(IEnumerable<InventoryReceive> entities,string GRNStatus);
        void GRNApproved1(IEnumerable<InventoryReceive> entities, string GRNStatus, string GRNNo,string AuthorizedByStatus);

        void PaymentHold(IEnumerable<InventoryReceive> entities);
        
        void InventoryReceive(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        void InventoryIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        //GRN Approved
        
        IEnumerable<object> getListForGRNUnchecked(string plantId);
        IEnumerable<object> getListForGRNChecked(string plantId);
        IEnumerable<object> GetListForGRNAp(string plantId);

       
        IEnumerable<object> GetListForGRNUNApproval(string plantId);

       


        void PoApproved(string PoId, string PoValue);

      
        void PoApproved1(string PoId, string PoValue);


        //IEnumerable<object> GetListForGRNCheck(string plantId);
        //IEnumerable<object> GetListForGRNUnCheck(string plantId);
        //void GRNCheck(string PoId, string PoValue);
        //void GRNUnCheck(string PoId, string PoValue);

        void GRNChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy);


        IEnumerable<object> IssueSlipFilter();

        void IssueRequestReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string issueId);
    }
}
