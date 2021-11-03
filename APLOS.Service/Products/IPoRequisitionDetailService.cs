#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Model.Products;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    public interface IPoRequisitionDetailService : IService<PoRequisitionDetail>
    {

        IEnumerable<object> GetPurchaseOrderGroupGridData();

        IEnumerable<object> GetAllPurchaseOrderGroupDetails();

        IEnumerable<object> GetAllReqdata1();
        object GetAutoSequence();

        void DeleteReq(string id);
        IEnumerable<object> GetReqMaster(string id);
        //void Insert1(PurchaseOrderGroupMaster entity);




    }
}