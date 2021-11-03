#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IOrderActivityService : IService<OrderActivity>
    {
        GridModel Query(GridParameter parameters, string companyGroupId, string activityType);
        IEnumerable<object> GetCbo(string companyGroupId, string activityType);

        void InsertBuyerActivity(OrderActivity entity);
        void UpdateBuyerActivity(OrderActivity entity);

        void InsertInquiryActivity(OrderActivity entity);
        void UpdateInquiryActivity(OrderActivity entity);
    }
}