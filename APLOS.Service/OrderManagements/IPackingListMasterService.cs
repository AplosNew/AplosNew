#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IPackingListMasterService : IService<PackingListMaster>
    {
        GridModel GetCompanyPartyList(GridParameter parameters, string plantId, string entityId);

        GridModel Query(GridParameter parameters, string entityId);

        GridModel GetSalesOrderList(GridParameter parameters);

        IEnumerable<object> GetEntityCboByPlant(string plantId);

        IEnumerable<object> GetDispatchMasterArticleList(string packingId);

        IEnumerable<object> GetDispatchAllSKUList(string packingId);

        IEnumerable<object> GetDispatchData(string dispatchUnitMasterId);

        IEnumerable<object> GetDispatchArticleList(string dispatchUnitMasterId);

        IEnumerable<object> GetDispatchSKUListByArticle(string dispatchArticleId);

        IEnumerable<object> GetSalesOrderSKUList(string salesOrderId);

        void DeleteGraph(string id);

        void InsertOrUpdateDispatch(DispatchUnitMaster dispatch, IEnumerable<DispatchUnitArticle> articleList);

        void InsertOrUpdateDispatchSku(IEnumerable<DispatchUnitSKU> skuList);

        void DeleteDispatchArticleGraph(string id);

        void DeleteDispatchSkuGraph(string id);
    }
}