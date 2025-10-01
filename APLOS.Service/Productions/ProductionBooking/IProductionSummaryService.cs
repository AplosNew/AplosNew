#region Using

using Library.Core;
using Library.Model.Productions;
using Library.Model.Productions.ProductionBooking;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Productions
{
    public interface IProductionSummaryService : IService<ProductionSummary>
    {
        //IEnumerable<object> GetPOCust(string POId);
        //IEnumerable<object> GetSOCust(string SOId);
        IEnumerable<ComboModel> GetWCCbo(string plantId, string entityId);
        IEnumerable<ComboModel> GetShiftCbo(string wcId);
        IEnumerable<ComboModel> GetToWCCbo(string plantId, string ProcessId, string entityId, string CompanyId);
        IEnumerable<object> GetListAPIforProduction(string ProdnDate, string EntityId, string ProcessId, string ShiftId);
        IEnumerable<object> GetDetailProductionList(string ProdnDate, string EntityId, string ProcessId, string ShiftId, string WkCenterId, string ProductionOrderId);
        string Delete(IEnumerable<ProductionSummary> DataToDelete);
        string Create(IEnumerable<ProductionSummary> DataToSave);
        IEnumerable<ComboModel> GetCbo(string plantId, string ProcessId);
        IEnumerable<object> GetLineItemGridSFG(string EntityId, string ProcessId, string ProductionDate, string ProductionShiftId, string WorkCenterMasterId, string ProductionLevel);
        IEnumerable<object> GetListAPIforProduction(string ProdnDate, string EntityId);
        List<ProductionSummary> GetListAPIforProduction(string ProdnDate);
        IEnumerable<object> GetProcess(string entityId);
        IEnumerable<object> GetEntity(string CompanyId, string PlantId);
        IEnumerable<object> GetTotalQty(string salesOrderId, string processId);
        IEnumerable<object> GetTotalSOQty(string POId, string salesOrderId, string processId);
        IEnumerable<object> GetTotalMOIQty(string POId, string MasterOrderItemId, string processId);
        IEnumerable<object> GetTotalPCQty(string POId, string MasterOrderItemId, string processId);
        IEnumerable<object> GetMentorAndRespPersonByWCM(string wcmId);
        IEnumerable<object> GetTotalProductionQty(string WorkCenterMasterId, string ProductionDate);
        void SaveSecondDetail(IEnumerable<ProductionSummaryDetail> psd, ProductionSummary productionSummary, string companyGroupId, string plantId);
        IEnumerable<ComboModel> GetCharacteristicsValueCbo(string soid);
        IEnumerable<ComboModel> GetCharacteristicsValueByPrOCbo(string soid);
        IEnumerable<object> Query(string plantId);
        IEnumerable<ComboModel> GetCbo(string plantId, string ProcessId, string entityId, string CompanyId, string shiftId);
        IEnumerable<object> GetCboWC(string plantId, string ProcessId, string entityId,string productionDate,string shiftId,string HeaderResponsiblePersonId);

        IEnumerable<object> GetCboWCPIC(string plantId, string ProcessId, string entityId, string productionDate, string shiftId, string HeaderResponsiblePersonId, string IssueId, string PeriodId);

        IEnumerable<object> GetCboIssueQIC(string plantId, string ProcessId, string entityId, string productionDate, string shiftId, string HeaderResponsiblePersonId, string IssueId, string PeriodId, string PId, string POItemId);

        IEnumerable<object> GetPOWiseData(string ProcessId, string entityId, string POId, string Date, string POStatus, string CustomerId, string IssueId);

        IEnumerable<object> GetQCComplete(string IssueId, string todate, string fromDate, string POId);

        IEnumerable<object> GetQCSummary(string IssueId, string todate, string fromDate, string POId);
        IEnumerable<object> GetWSCWC(string plantId, string ProcessId, string entityId, string Date, string shiftId, string WSMId);
        void Save(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd);
        void DeleteDetail(string masterid);
        IEnumerable<object> GetSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId);
        IEnumerable<object> GetChar1Info(string id, string soid);
        IEnumerable<object> GetChar1InfobyPrO(string id, string soid);
        IEnumerable<object> GetCharInfo(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId);
        IEnumerable<object> GetCharInfoByPrO(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId);
        IEnumerable<ComboModel> GetShiftGroupCbo(string plantId);
        void SaveMaster(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string companyGroupId, string ProcessId, IEnumerable<ProductionSummaryParameterValue> ProcessParaList);
        //void SaveMasterWC(List<Dictionary<string, object>> DataList);

        void SaveMasterWC(ProductionSummary ps, string companyGroupId, string ProcessId);

        void SaveDetentionWC(List<Dictionary<string, object>> DataList);
        void SaveInOutMaster(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string companyGroupId);
        void SaveDetail(string psid, IEnumerable<ProductionSummaryDetail> psd);

    }
}