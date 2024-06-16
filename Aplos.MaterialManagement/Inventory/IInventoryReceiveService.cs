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
        IEnumerable<object> GetRecentApprovedData(string grnId);
        IEnumerable<object> Posted();
        GridModel QueryEmpGrn(GridParameter parameters, string plantId); 
        IEnumerable<object> GetListForHold(string plantId,string PoType,string Status, string vendorId);
        IEnumerable<object> LoadAcceptanceDetails(string AcceptanceId);
        IEnumerable<object> LoadAcceptanceDetailsBOQ(string AcceptanceId);
        IEnumerable<object> GetSavedPOList(string GRNId);
        IEnumerable<object> GetSavedPOListBOQ(string GRNId);
        IEnumerable<object> GetSavedPOList1(string GRNId);
        IEnumerable<object> GetListForREqPOGRN(string plantId, string PoType, string Status);
        IEnumerable<object> GetListOfPOGateEntry(string CompanyGroupId, string CompanyId, string PlantId, string partyCode); 
        void Insert(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMaterial);
        IEnumerable<object> GetListOfPOGateEntryEmployee(string CompanyGroupId, string CompanyId, string PlantId, string EmployeeId);
		IEnumerable<object> GetListForInvShortagePayable(string plantId);
        IEnumerable<object> GetListForInvRejectPayable(string plantId);
        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string GRNDate);
        IEnumerable<object> GetTaxCategoryListForSales(string companyGroupId, string receiveId, string plantId, string hsnCodeId); 

        IEnumerable<object> GetReceiveTaxList(string receiveDetailId);
        IEnumerable<object> GetReceiveTaxListBOQ(string receiveDetailId);
        IEnumerable<object> GetReceiveTaxListPO(string receiveDetailId);
        IEnumerable<object> GetMaterialLedger(string fromDate, string toDate);

        IEnumerable<object> GetPurchaseReturnRegister(string fromDate, string toDate, string Type);
        IEnumerable<object> GetTotalReceiveTaxList(string receiveId);

        IEnumerable<object> GetServiceTaxList(string serviceId);
        IEnumerable<object> GetServiceTaxListBOQ(string serviceId);
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
        void GetFocReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void PurchaseReturnReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        void InventoryIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        
        // Outsource Transformation Issue
        void JWIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void JWValAddedIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        // JobWork Transformation Template

        void JobWorkTransformationIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        // Jobwork Value added template
        void JobWorkValAddedIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

        void InventorySalesReportPrint(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        void InventoryPreSalesReportPrint(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);

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

        void PoApproved(string PoId, string PoValue);

      
        void PoApproved1(string PoId, string PoValue);


        //IEnumerable<object> GetListForGRNCheck(string plantId);
        //IEnumerable<object> GetListForGRNUnCheck(string plantId);
        //void GRNCheck(string PoId, string PoValue);
        //void GRNUnCheck(string PoId, string PoValue);

        void GRNChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string CheckedRejectReason);


        IEnumerable<object> IssueSlipFilter();
		IEnumerable<object> GetIssueSlipFilterData(string column, string value, string plantId);
        IEnumerable<object> GetStockForMaterialIssue(string plantId, string materialMasterId, string articleId);
        IEnumerable<object> GetAssetIssueSlipFilterData();
        

        IEnumerable<object> IssueFilter();

        void IssueRequestReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string issueId);

        void IssueWithReqPOGRNReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string Id);

        IEnumerable<object> GetRequisitionIssueDetail(string issueId);


        IEnumerable<object> IssueDetailData(string status,string Preparedby);
        IEnumerable<object> GetSupervisorCbo();
        IEnumerable<object> GetSupervisorCboApproved();
        IEnumerable<object> GetTaxCategoryListByPartyPlant(string companyGroupId, string partyPlantId, string plantId, string hsnCodeId);

        void DeletePurchaseReturnfinal(string id);
        IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy);
        IEnumerable<object> GetCheckedByAndApprovedBYBOQ(string CheckedBy, string ApprovedBy);
        // void InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId);
        Dictionary<string, object> GetACCCutOffDate(string companyGroupId, string companyId);
        void InsertPODocMap(GRNDocumentMap entity, string POId, out string Id);

        IEnumerable<object> GetJWApproving(string plantId, string GRNbyPOApprovedStatus);

        IEnumerable<object> GetJobWorkApproving(string plantId, string GRNbyPOApprovedStatus);
        IEnumerable<object> GetJWCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy);

        void FGInventoryReceive(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId);
        void GrnBOQPORep(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnBOQPOId);
    }
}
