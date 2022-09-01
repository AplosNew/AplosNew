using Library.Core;
using Library.Model.WorkCenters;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.WorkCenters
{
    public interface IWorkCenterMasterService : IService<WorkCenterMaster>
    {
        GridModel GetShiftList(GridParameter parameters, string sGroupID, string sPlantID, string[] ShiftDefinationIDs);
        GridModel GetListForSubProcess(GridParameter parameters, string CompanyGroupId, string processId, string WorkCenterMasterId, string[] subProcessIds);
        IEnumerable<object> GetWorkCenterWiseShiftList(string sGroupID, string sPlantID, string workCenterMasterId);
        IEnumerable<object> GetWorkCenterMasterSubProcessList(string workCenterMasterId);
        decimal GetAutoSequence();
        GridModel GetProductMasterList(GridParameter parameters, string groupId);
        GridModel EmployeeListByPlant(GridParameter parameters, string plantId);
        GridModel GetWCByPlant(GridParameter parameters, string plantid);

        GridModel GetSearchData(GridParameter parameters);

        IEnumerable<object> GetList(string masterid, string companyId);

        void InsertORUpdateMaster(WorkCenterMaster master, out string masterid);

        void DeleteMaster(string masterid);

        //IEnumerable<object> GetListByPlant(string plantid);
        GridModel GetListByPlant(GridParameter parameters, string plantid);

        GridModel GetListByPlant(GridParameter parameters, string plantid, string processid);

        IEnumerable<object> GetListByPlant(string plantid, string entityid, string processid);

        GridModel GetListByPlantEntity(GridParameter parameters, string plantid, string EntityId);

        IEnumerable<object> GetListByPlantAndEntity(string plantid, string entityid, string companyId);

        GridModel GetAllWorkCenter(GridParameter parameters);

        GridModel GetSearchLine(GridParameter parameters, string entityId);

        IEnumerable<object> GetCbo();

        GridModel GetCboList(string entityId);

        GridModel GetEmployeeList(GridParameter parameters, string plantId);

        GridModel GetMaterialMasterList(GridParameter parameters, string groupId, string[] ids);

        void InsertUpdateOrDeleteDetails(string masterId, IEnumerable<WorkCenterMasterEffectiveDate> effectiveDateList, IEnumerable<WorkCenterMasterManpowerBudge> budgetCodeList, IEnumerable<WorkCenterMasterProductPriority> productPriorityList, IEnumerable<WorkCenterWiseShift> shiftList, IEnumerable<WorkCenterMasterSubProcess> subProcessList);

        IEnumerable<object> GetEffectiveDateList(string masterId);

        IEnumerable<object> GetManpowerBudgetList(string masterId);

        IEnumerable<object> GetProductPriorityList(string masterId);
    }
}