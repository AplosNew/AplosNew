#region Using

using Library.Core;
using Library.Model.Inventory;
using Library.Model.OrderManagements;
using Library.Model.Products;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Products
{
    public interface IQualityStdSetService : IService<QualityStdSet>
    {

      

        IEnumerable<object> GetQualityStdSetGridData();

        IEnumerable<object> GetAllPurchaseOrderGroupDetails(string Id);


        IEnumerable<object> GetAllPOGVendor(string Id);

        IEnumerable<object> GetVendorCbo(string partyId, string Id);


        IEnumerable<object> GetAllReqdata1();
        //object GetAutoSequence();
        void DeleteReqDetails(string id);

       
        void DeleteQStd(string id);
        //IEnumerable<object> GetReqMaster(string id);
        //void Insert1(PurchaseOrderGroupMaster entity);

 
        void Insert(QualityStdSet entity);
        object SqlQuery<T>(string v);
        decimal GetAutoSequence();
        void UpdateMaterial(IEnumerable<PurchaseOrderGroupDetails> entity, IEnumerable<PurchaseOrderTax> receiveTaxList);
       
        decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId);

    }
}