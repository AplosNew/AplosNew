#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Model.Products;
using Library.Service.Core;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    public interface IPurchaseOrderGroupDetailsService : IService<PurchaseOrderGroupDetails>
    {
        string CompanyGroupId { get; set; }
        string Id { get; }

   
    
        void InsertOrUpdateGraphEdit(PurchaseOrderGroupDetails entity);
        void InsertOrUpdateGraph(IEnumerable<PurchaseOrderGroupDetailsViewModel> entity, string id, string Gname);

        IEnumerable<object> GetMaterialGridData();

        void DeletePOGDetails(string id); 


    }
}