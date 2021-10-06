using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;

namespace Library.MaterialManagement.Inventory
{
    public interface IInventoryReceiveService : IService<InventoryReceive>   
    {
        GridModel Query(GridParameter parameters, string plantId);

        IEnumerable<object> GetListGRN();


        IEnumerable<object> NotApproveChecked();
		IEnumerable<object> CheckedHoldReject();
		IEnumerable<object> ApprovedHoldChecked();
		IEnumerable<object> ApprovedNotPost();

		
		IEnumerable<object> Posted();
        
        IEnumerable<object> GetListEmployeePurchase();
		
			IEnumerable<object> GetListEmpCheckedHoldReject();
		IEnumerable<object> GetListEmpNotApproveChecked();
		
		IEnumerable<object> GetListEmpApprovedHoldReject();
		//IEnumerable<object> GetListEmpApprovedNotPost();

       // IEnumerable<object> GetListEmpPosted();
        GridModel QueryEmpGrn(GridParameter parameters, string plantId); 

		
		//IEnumerable<object> QueryGetListForMasterData(string plantId,string GRNbyPOCheckStatus);
		IEnumerable<object> QueryGetListForGRNSaveData(string plantId,string GRNWithReqPOCheckStatus); 


		
		IEnumerable<object> QueryGetListForMasterData2(string plantId,string GRNbyPOApprovedStatus);

        IEnumerable<object> GetListForGrnByPoReq(string plantId, string GRNWithReqPOApprovedStatus);



        GridModel GetListByGrnno(GridParameter parameters, string plantId, int GRN);        
        IEnumerable<object> GetListForHold(string plantId,string PoType,string Status);
        IEnumerable<object> LoadAcceptanceDetails(string AcceptanceId);

        
        IEnumerable<object> GetSavedPOList(string GRNId);

        IEnumerable<object> GetSavedPOList1(string GRNId);

        IEnumerable<object> GetListForREqPOGRN(string plantId, string PoType, string Status);

        
        IEnumerable<object> GetListOfPOGateEntry(string CompanyGroupId, string CompanyId, string PlantId, string partyCode); 
        GridModel GetPostingList(GridParameter parameters, string plantId);
       
        void Insert(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMaterial);
        GridModel GetListForHold(GridParameter parameters, string plantId);
		GridModel GetEmployeePurchaseList(GridParameter parameters, string plantId);
        GridModel GetListForInvPayable(GridParameter parameters, string plantId);

        IEnumerable<object> GetListOfPOGateEntryEmployee(string CompanyGroupId, string CompanyId, string PlantId, string EmployeeId);
		IEnumerable<object> GetListForInvShortagePayable(string plantId);
        IEnumerable<object> GetListForInvRejectPayable(string plantId);
        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string GRNDate);
        IEnumerable<object> GetTaxCategoryListForSales(string companyGroupId, string receiveId, string plantId, string hsnCodeId); 

        IEnumerable<object> GetReceiveTaxList(string receiveDetailId);
        IEnumerable<object> GetReceiveTaxListPO(string receiveDetailId);
        IEnumerable<object> GetMaterialLedger(string fromDate, string toDate);
		IEnumerable<object> GetPurchaseRegister(string fromDate, string toDate, string Type);

        IEnumerable<object> GetPurchaseReturnRegister(string fromDate, string toDate, string Type);



        //IWorkbook GetDailyTransactionReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, DateTime date, string entityId);
        IEnumerable<object> GetTotalReceiveTaxList(string receiveId);

        IEnumerable<object> GetServiceTaxList(string serviceId);
        IEnumerable<object> GetServiceTaxListPR(string serviceId);
        IEnumerable<object> GetServiceTaxListPO(string serviceId);
       

        decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId);
        decimal GetToCurrencyRateForJWR(string currencyId, string baseCurrencyId, DateTime docDate, string companyId);
        
        decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable);
		
		decimal GetChargesTaxRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable);

		void Delete(string id);
        void JWDelete(string id);
        void GRNApproved(IEnumerable<InventoryReceive> entities,string GRNStatus);
        void GRNApproved1(IEnumerable<InventoryReceive> entities, string GRNStatus, string GRNNo,string AuthorizedByStatus, string RejectApprovedReason);

        void PaymentHold(IEnumerable<InventoryReceive> entities);
        
        void InventoryReceive(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void PurchaseReturnReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        void InventoryIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        
        // Job Work Transformation Issue
        void JWIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void JWValAddedIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void InventorySalesReportPrint(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void InventoryScrapReportPrint(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void PhysicalStockAdjustmentReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        void InventoryIssueReturnReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void AssetIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        //GRN Approved

        IEnumerable<object> getListForGRNUnchecked(string plantId);
        IEnumerable<object> getListForGRNChecked(string plantId);
		IEnumerable<object> getListForGRNRejectHoldList(string plantId); 
		IEnumerable<object> GetListForGRNAp(string plantId);
		IEnumerable<object> GetListForGRNApprovalHoldReject(string plantId);


		IEnumerable<object> GetListForGRNUNApproval(string plantId);

       


        void PoApproved(string PoId, string PoValue);

      
        void PoApproved1(string PoId, string PoValue);


        //IEnumerable<object> GetListForGRNCheck(string plantId);
        //IEnumerable<object> GetListForGRNUnCheck(string plantId);
        //void GRNCheck(string PoId, string PoValue);
        //void GRNUnCheck(string PoId, string PoValue);

        void GRNChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string CheckedRejectReason);


        IEnumerable<object> IssueSlipFilter();
		IEnumerable<object> GetIssueSlipFilterData();
        IEnumerable<object> GetAssetIssueSlipFilterData();
        

        IEnumerable<object> IssueFilter();

        void IssueRequestReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string issueId);

        void IssueWithReqPOGRNReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string Id);

        IEnumerable<object> GetRequisitionIssueDetail(string issueId);


        IEnumerable<object> IssueDetailData(string issueId);
        IEnumerable<object> GetSupervisorCbo();
        IEnumerable<object> GetSupervisorCboApproved();
        IEnumerable<object> GetTaxCategoryListByPartyPlant(string companyGroupId, string partyPlantId, string plantId, string hsnCodeId);

        void DeletePurchaseReturnfinal(string id);
        IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy);
        // void InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId);
        Dictionary<string, object> GetACCCutOffDate(string companyGroupId, string companyId);
        void InsertPODocMap(GRNDocumentMap entity, string POId, out string Id);

        IEnumerable<object> GetJWApproving(string plantId, string GRNbyPOApprovedStatus);
        IEnumerable<object> GetJWCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy);

        void FGInventoryReceive(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

    }
}
