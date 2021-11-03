using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.OrderManagements
{
    public interface IProductionOrderSubprocessSetService : IService<ProductionOrderSubprocessSet>
    {
        string GetPK();

        IEnumerable<object> GetCharacteristicsSetting(string entityid, string mmid);

        IEnumerable<ProductionOrderSubprocessSet> ProductionOrderSubprocessSetList(string MasterId);

        void ProcessSetOut(string ProductionOrderMasterId, ProductionOrderSubprocessSet from_ui, out ProductionOrderSubprocessSet from_db);

        IEnumerable<ComboModel> GetProcessTypeCbo(string Orderid);

        IEnumerable<ComboModel> GetProcessCbo(string Orderid);

        GridModel GetListProcessAndProcessTypeWise(GridParameter parameters, string entityid, string processid, string processtypeid);

        IEnumerable<object> GetDetailGridData(string Id);

        void SaveProcessSetChild(string ProductionOrderSubprocessCritariaId, IEnumerable<ProductionOrderSubprocessSet> ui_pmaterial);

        void ProcessSetOutChild(string ProductionOrderSubprocessCritariaId, IEnumerable<ProductionOrderSubprocessSet> from_ui, out List<ProductionOrderSubprocessSet> from_db);

        IEnumerable<object> GetDetailChildList(string productionOrderProcessCriteriaId, string entityid, string processid, string processtypeid);
    }
}