using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.MaterialManagement.Inventory
{
    public interface IServiceRequsitionMasterService : IService<ServiceRequsitionMaster>
    {
        IEnumerable<object> Query(string receiveId);
        void DeleteServiceCharge(string id);
        void ServiceRequisitionReportby(string CompanyGroupId, string plantId, string RequisitionId, string startDate, string endDate, string empId);
        IEnumerable<object> GetListForHold(string plantId);
        IEnumerable<object> GetListForPOApproval1Auth(string plantId);
        IEnumerable<object> GetListForPOApprovalAuthorized(string plantId);
        IEnumerable<object> GetEntity();
        IEnumerable<object> GetSupervisorCbo();
        //IEnumerable<object> GetAllReqdata(string ReqStatus);
        IEnumerable<object> GetAllReqdataDetails();
        IEnumerable<object> GetAllReqdataDetailsById(string Id);//string ReqDetailId
        IEnumerable<object> GetAllReqdata1(string ReqStatusApproval);
        IEnumerable<object> GetReqMaster(string id);
        GridModel QueryForPurchaseOrderDetail(GridParameter parameters, string inveReveiveId);
        void UpdateMaterial(IEnumerable<MaterialRequisitionDetailViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList);
        void DeleteReq(string id);
        IEnumerable<object> GetAllServiceReqdataDetails();//string ReqDetailId
        void Insert1(ServiceRequsitionMaster entity);
        
        void InsertSerReqDetail(ServiceRequsitionDetail entity);
        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId);
        IEnumerable<object> GetReceiveTaxList(string receiveDetailId);
        IEnumerable<object> GetTotalReceiveTaxList(string receiveId);
        IEnumerable<object> GetServiceTaxList(string serviceId);
        decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable);
        void DeleteMaterialTax(string id);
        IEnumerable<object> GetPartyPlantCbo(string partyId, string Id);
        IEnumerable<object> GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId);
        DataTable GetPurchaseOrderSqlData(string purchaseOrderId);
        #region shajazan PO Approval
        void PoApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy);
        void PoApproved1(string PoId, string PoValue);
        void PoApproved1Auth(string PoId, string PoValue);
        #endregion
        void PoApprovedAuth(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy);

        IEnumerable<object> GetCheckedByAndApprovedBYServiceRequisitionCreation(string CheckedBy, string ApprovedBy);
        IEnumerable<object> GetListIndependentServiceAcknowledgementData(string tabType);

    }
}