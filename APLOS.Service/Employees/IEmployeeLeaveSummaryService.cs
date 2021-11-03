#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmployeeLeaveSummaryService : IService<EmployeeLeaveSummary>
    {
        GridModel GetYearCboList(string plantId);
        void Save(string CompanyGroupId);

        void TSSave();

        // void Edit(EmployeeLeaveSummary ui, out string masterid);
        void UpdateLeaveBalance(EmployeeLeaveSummary entity, string PlantId, string CompanyGroupId);

        void UpdateCarryForward(EmployeeLeaveSummary entity, string PlantId, string CompanyGroupId);

        GridModel ActiveEmpListByPlantId(GridParameter parameters, string plantID, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);

        GridModel GetLeaveTypeList(string CompanyGroupId);

        GridModel GetLeaveTypeCumulativeList(string CompanyGroupId);

        GridModel GetYearList(string CompanyGroupId);
    }
}