using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.OrderManagements
{
    public interface IProductionOrderProcessCriteriaService : IService<ProductionOrderProcessCriteria>
    {
        string GetPK();

        IEnumerable<ProductionOrderProcessCriteria> ProductionOrderProcessCriteriaList(string MasterId);

        IEnumerable<object> GetListOrderWise(string ProductionOrderMasterID);//

        IEnumerable<object> GetList(string ProcessId, string ProcessTypeId, string ProductionOrderMasterId);

        void SaveDetail(ProductionOrderProcessCriteria ui_detail);

        void DeleteGraph(string id);

        IEnumerable<ProductionOrderSubprocessSet> ProductionOrderSubprocessSetList(string masterId);
    }
}