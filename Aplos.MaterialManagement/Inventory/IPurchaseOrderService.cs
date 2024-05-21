using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.MaterialManagement.Inventory
{
	public interface IPurchaseOrderService : IService<PurchaseOrder>
	{
		IEnumerable<object> GetPOMasterById(string plantId, string id);
		GridModel Query(GridParameter parameters, string plantId);
		IEnumerable<object> QueryForCharges(string MasterId);
		void InsertGraphCharge(InventoryMaterialViewModel entity, IEnumerable<ServicePOAckTax> taxCategoryList);
		void UpdateGraphCharge(InventoryMaterialViewModel entity, List<ServicePOAckTax> taxCategoryList);
		GridModel GetPostingList(GridParameter parameters, string plantId);

		IEnumerable<object> GetListForHold(string plantId);
		IEnumerable<object> GetPOTypeList(string plantId, string POTypeStatus,string poType);
		IEnumerable<object> POCheckedRollBack(string plantId, string POTypeStatus);
		IEnumerable<object> GetIndependentPOListByStatus(string plantId, string ApproveRejectHold);
		IEnumerable<object> GetListForHold11BOQ(string plantId, string ApproveRejectHold,string poType);
		IEnumerable<object> PORollBackApproved(string plantId, string ApproveRejectHold);
		
		IEnumerable<object> GetListForAllPOList(string plantId);

		IEnumerable<object> GetListForHold1(string plantId);
		//IEnumerable<object> getCheckedList(string plantId); 
		
		IEnumerable<object> GetListForPOApproval1UnApproved(string plantId);

		IEnumerable<object> GetListForPOApproval1Auth(string plantId);

		IEnumerable<object> getApprovedHoldReject(string plantId);
		
		//IEnumerable<object> GetListForallPo(string plantId);
		//IEnumerable<object> GetListForHold(string plantId);
		IEnumerable<object> GetListForPOApproval(string plantId);
		//IEnumerable<object> getPendingList(string plantId);  
		IEnumerable<object> GetListForPOHoldandReject(string plantId);
		IEnumerable<object> getCheckedHoldReject(string plantId); 
		
		IEnumerable<object> GetListForPOApprovalAuthorized(string plantId, string POTypeApprovalStatus);

		IEnumerable<object> getUNApprovalList(string plantId, string POTypeApprovalStatus);


		IEnumerable<object> GetIssueSlipCheckByCbo();
		IEnumerable<object> GetSupervisorCbo();
		IEnumerable<object> GetSupervisorCboApproved();

		IEnumerable<object> GetSupervisorCboApproved1();

		void POClose(string PoId, string PoValue);
		void POUnClose(string PoId, string PoValue);




		GridModel GetEmployeePurchaseList(GridParameter parameters, string plantId);
		IEnumerable<object> GetListForPOClose(string plantId);
		IEnumerable<object> GetListForPOUnClose(string plantId);

		GridModel GetListForInvPayable(GridParameter parameters, string plantId);
		Dictionary<string, object> GetExpenseBookingFile(string id);
        IEnumerable<object> GetJWServiceTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate);

        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate);

		IEnumerable<object> getserviceTaxByTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate);
		IEnumerable<object> GetTaxCategoryListForSalesService(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string InventorySalesDate);
		IEnumerable<PurchaseOrderTax> GetTaxCategoryList1(string companyGroupId, string receiveId, string plantId, string hsnCodeId);
		IEnumerable<object> GetTaxCategoryListServiceAcknowledgement(string companyGroupId, string serviceId, string plantId, string hsnCodeId);
		IEnumerable<object> GetReceiveTaxList(string receiveDetailId);

		IEnumerable<object> GetTotalReceiveTaxList(string receiveId);

		IEnumerable<object> GetServiceTaxList(string serviceId);
		IEnumerable<object> GetServiceTaxListForTax(string serviceId);

		decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId);

		decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable);

		void Delete(string id);


		void DeleteMaterialTax(string id);

		void GRNApproved(IEnumerable<PurchaseOrder> entities);

		void PaymentHold(IEnumerable<PurchaseOrder> entities);

		IEnumerable<object> GetListByParty(string partyId, string PartyType);

		IEnumerable<object> GetPartyPlantCbo(string partyId, string Id);
		IEnumerable<object> GetMaterialDetails(string MaretialDetailsId);
		IEnumerable<object> GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId);

		DataTable GetPurchaseOrderSqlData(string purchaseOrderId);

		void GePurchaseOrderReport(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderId);
		void GetPurchaseAcceptanceReport(string CompanyGroupId, string plantId, string PDACId);


		void GePurchaseOrderReportByReq(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderId);

		#region shajazan PO Approval
		void PoApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string CheckedRejectReason);
		void PoUnApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy);



		//void PoAllListinC(string PoId, string PoValue);
		void PoApproved1(string PoId, string PoValue);
		void PoApproved1Auth(string PoId, string PoValue);

		#endregion
		void PoApprovedAuth(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string ApproveRejectReason);



		IEnumerable<object> GetLCContractList(bool isProcurementOnBom, string plantId);


		IEnumerable<object> GetalldataPOWithLCMap(string plantId);

		IEnumerable<object> GetalldataPOWithoutLCMap(string plantId);

		IEnumerable<object> GetLCListByContract(string ContractId, string VendorId, string CurrencyId);


		IEnumerable<object> UpdatePOforLC(string POId, string PurchaseLCId, string flag);


		#region FGForMasterOrder 22-Jun-2019


		IEnumerable<object> GetListForMasterOrder(string CompanyId);
		IEnumerable<object> GetMasterItemList(string masterOrderId);
		IEnumerable<object> getTaxCategoryListForFGService(string companyGroupId, string plantId, string hsnCodeId, string partyPlantId);

		//object GetOperationMaster();
		#endregion


		#region PO By Requisition
		IEnumerable<object> GetRequisitionList(string RequisitionId);
		DataTable GetListForRequisition(string CompanyId);
		IEnumerable<object> GetListForRequisition1(string CompanyId);
		IEnumerable<object> GetListForPOBYReq(string plantId, string POTypeStatus);
		IEnumerable<object> GetListForPOBYReq1(string plantId, string ApproveRejectHold);
		IEnumerable<object> GetTaxCategoryListPOBYReq(string receiveDetailId);
		#endregion








		#region Service PO BY Requisition

		void InsertServicePOByReq(ServicePOMaster entity);

		void Update(ServicePOMaster entity);

		void DeleteServicePOReq(string id);

		


		IEnumerable<object> GetListForServicePOBYReq(string plantId, string POTypeStatus,string POType);

		IEnumerable<object> GetListForServicePOBYReqHR(string plantId, string ApproveRejectHold, string POType);

		//IEnumerable<object> GetListForServiceRequisition(string Id);
		
		void ServicePurchaseOrderReport(string CompanyGroupId, string plantId, string purchaseOrderId);
		IEnumerable<object> GetServicePOByReqSupervisorCbo();

		#endregion


		#region servive ack

		void InsertServiceAck(ServiceAcknowledgementMaster entity, IEnumerable<ServiceAcknowledgementViewModel> DetailList,IEnumerable<ServicePOAckTax> ServicePOAndAckTax);
		void InsertIndependentServiceAck(string ServiceAckId, ServiceAcknowledgementViewModel ackDetailModel, IEnumerable<ServicePOAckTax> servicePOAckTax);
		void InsertIndependentServiceAck(ServiceAcknowledgementMaster entity);
		void ServiceAcknowledgementReport(string CompanyGroupId, string plantId, string SurviceAckId);

		//void ServiceAcknowledgementReport(string CompanyGroupId, string plantId, string purchaseOrderId);
		void DeleteServiceAck(string id);
		#endregion


		IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy);
		IEnumerable<object> GetCheckedByAndApprovedBYOutSource(string CheckedBy, string ApprovedBy);
		
		IEnumerable<object> GetCheckedByAndApprovedBYServicePORequisition(string CheckedBy, string ApprovedBy);


        IEnumerable<object> GetCheckedByAndApprovedBYServicePOAcknowledgement(string CheckedBy, string ApprovedBy);

		Dictionary<string, object> GetPOFile(string id);
		void InsertPODocMap(PODocumentMap entity, string POId, out string Id);

		void InsertServicePODocMap(ServicePODocumentMap entity, string POId, out string Id);
		void InsertServicePOAckDocMap(ServicePOAckDocumentMap entity, string POId, out string Id);
		IEnumerable<object> GetAllPOList(string column, string value, string plantId);
		IEnumerable<object> GetLCList(string masterId);
		IEnumerable<object> GetGRNList(string masterId);
		IEnumerable<object> GetAcceptanceList(string masterId);
		void InsertPOBOQMaster(PurchaseOrder entity);
		void SaveTermsData(string TitleId, string POId);

	}
}