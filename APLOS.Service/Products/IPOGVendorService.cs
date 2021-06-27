#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Model.Products;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    public interface IPOGVendorService : IService<POGVendor>
    {
        string CompanyGroupId { get; set; }
        string Id { get; }

        void POGVendorDelete(string id);
        //void InsertOrUpdateGraphEdit(PurchaseOrderGroupDetails entity);
        ////void Insert(IPurchaseOrderGroupDetailsService entity);
        //void InsertOrUpdateGraphEdit(PurchaseOrderGroupDetails entity);
        //void InsertOrUpdateGraph(PurchaseOrderGroupDetails entity, string id);
        void InsertOrUpdateGraphPOGVendor(POGVendor entity, string id);
        //IEnumerable<object> GetAllPOGVendor(string Id);


    }
}