#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IInquiryService : IService<Inquiry>
    {
        GridModel Query(GridParameter parameters, string entityId, string employeeId);

        IEnumerable<object> QueryForCommitmentInquiry(string inquiryId);

        IEnumerable<object> QueryForProductInquiry(string inquiryId);

        GridModel GetProductInquiryWithEntity(GridParameter parameters, string entityId);

        GridModel GetIntermediateItemWithEntity(GridParameter parameters, string entityId);

        GridModel GetProductProcessGroupWithNotId(GridParameter parameters, string processProductionGroupId);

        void InsertAndUpdate(Inquiry entity);

        void InsertCommitmentInquiry(IEnumerable<CommitmentInquiry> entities);

        void InsertProductInquiry(IEnumerable<ProductInquiry> entities);

        void InsertProductInquiryDetail(IEnumerable<ProductInquiryDetail> entities);

        IEnumerable<object> QueryForProductInquiryDetailList(string productInquiryId);

        GridModel QueryForIsPreCostingInquiry(GridParameter parameters);

        GridModel EntityWithInternal(GridParameter parameters, string companyGroupId, string productionProcessGroupId);

        IEnumerable<object> GetEntityCboWithProductionProcessGroup(string productionProcessGroupId);

        void DeleteGraph(string id);
        IEnumerable<object> GetActivityWithBuyerMasterCbo(string buyerMasterId);
        GridModel QueryForResponsible(GridParameter parameters, string entityId, string buyerMasterId, string buyerActivityId);
    }
}