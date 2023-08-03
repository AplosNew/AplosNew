#region Using

using Library.Core;
using Library.Model.Inventory;
using Library.Model.OrderManagements;
using Library.Model.Products;
using Library.Service.Core;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.MaterialManagement.Products
{
    public interface IIssueRequestService : IService<IssueRequest>
    {
        void CreateOrUpdateMaterialControlIssueSlip(IssueRequestMaster Issentry, IEnumerable<IssueRequestViewModel> entity, IEnumerable<IssueRequestViewModel> entityGroupData, string IssueSlipType, IEnumerable<IssueRequestViewModel> SOListSelectedNew);
        IEnumerable<object> GetPurchaseOrderGroupGridData();
        void InsertOrUpdateGraphIssueSlipCreate(IssueRequestMaster Issentity,IEnumerable<IssueRequestViewModel> entity, IEnumerable<IssueRequestViewModel> entityGroupData ,string IssueSlipType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti
            , IEnumerable<IssueRequestViewModel> SOListSelectedNew, IEnumerable<IssueRequestViewModel> MaterialColorListNew, string ProcessId, List<Dictionary<string, object>> machinepopUpDataList);
        void InsertOrUpdateGraphIssueSlipUpdate(IssueRequestMaster Issentity, IEnumerable<IssueRequestViewModel> entity, string Id,string IssueSlipType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti);  
        //IEnumerable<object> IssueListData(string plantId);
        IEnumerable<object> IssueListData(string IssueStatus, string IssueSlipType);
        IEnumerable<object> IssueListDataByProudctionOrder(string IssueStatus, string IssueSlipType, string productionOrderId);
        IEnumerable<object> AssetIssueListData(string IssueStatus, string IssueSlipType);
        
        IEnumerable<object> ApprovedIssueSlipGridData(string IssueStatusApproval, string IssueSlipType);
        IEnumerable<object> IssueSlipDetail(string slipstatus, string employeeId);

        IEnumerable<object> IssueListById(GridParameter parameters, string Id);
        IEnumerable<object> GetAllPurchaseOrderGroupDetails(string Id);

        IEnumerable<object> GetAllReqdata1();
        //object GetAutoSequence();
        void DeleteReqDetails(string id);
        void DeleteReq(string id);
        //IEnumerable<object> GetReqMaster(string id);
        //void Insert1(PurchaseOrderGroupMaster entity);

        void Insert(PurchaseOrderGroup entity);
        object SqlQuery<T>(string v);
        decimal GetAutoSequence();
        void UpdateMaterial(IEnumerable<PurchaseOrderGroupDetails> entity, IEnumerable<PurchaseOrderTax> receiveTaxList);
       
        decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId);



        //----#region IssueSlipCheck---
        IEnumerable<object> IssueSlipUnChecked(string IssuStatus);
        IEnumerable<object> IssueSlipChecked();
        void IssueSlipToChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy);





        //---- #region  ApprovingIssueSlip---

        IEnumerable<object> IssueSlipUnApproved(string IssuAppStatus);
        IEnumerable<object> IssueSlipApproved();

        void IssueSlipToApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy);


        //--------#region  RequisitionIssue
        IEnumerable<object> RequisitionIssueListData();
    }
}