using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.OrderManagements
{
    public interface IProductionOrderService : IService<ProductionOrder>
    {
        GridModel Query(GridParameter parameters, string plantId);

        IEnumerable<object> GetSalesOrderList(GridParameter parameters, string companyId);

        IEnumerable<object> GetProductionRecipeMaterialList(string productionOrderId);
        IEnumerable<object> GetProductionOrderType2MaterialList(string productionOrderId);
        IEnumerable<object> GetProductionOrderProcessSetList(string productionOrderId);
        IEnumerable<object> GetProductionOrderType2ProcessSetList(string productionOrderId);
        IEnumerable<object> GetProductionOrderEntityList(string productionOrderId);

        IEnumerable<object> GetWorkCenterList(string[] entityIds,string processid);

        IEnumerable<object> GetProductionOrderWorkCenterList(string productionOrderId);
        IEnumerable<object> GetProductionOrderType2WorkCenterList(string productionOrderId);
        IEnumerable<object> GetWorkCenterListByEntity(string entityId);

        IEnumerable<object> GetWorkCenterListByEntityandFirstProcess(string entityId, string processId, string productionOrderId);
        IEnumerable<object> GetSavedWorkCenterListByEntityandFirstProcess(string ProductionOrderId);
        IEnumerable<object> GetSavedType2WorkCenterListByEntityandFirstProcess(string ProductionOrderId);
        void InsertGraph(ProductionOrder master, IEnumerable<ProductionOrderDetail> detaillist
            , IEnumerable<ProductionOrderProcessSet> processSetlist
            , IEnumerable<ProductionOrderEntity> entitylist
            , IEnumerable<ProductionOrderWorkCenter> workcenterlist,
            DataTable Runningworkcenterlist);

        void UpdateGraph(ProductionOrder master, IEnumerable<ProductionOrderDetail> detaillist
             , IEnumerable<ProductionOrderProcessSet> processSetlist
             , IEnumerable<ProductionOrderEntity> entitylist
             , IEnumerable<ProductionOrderWorkCenter> workcenterlist,
                DataTable Runningworkcenterlist
            , IEnumerable<ProductionOrderFirstProcessWorkCenter> fpworkcenterlist);

        void DeleteGraph(string id);
    }
}