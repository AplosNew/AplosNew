#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IApprovalConfigurationService : IService<ApprovalConfiguration>
    {
        GridModel GetEmployeeDataIds(GridParameter parameters, string plantId, string lineIds, string employeeCode, string employeeName, string SubsectionId);
        GridModel GetEmployeeAttendanceGroupDataWithLine(GridParameter parameters, string plantId, string lineIds, string employeeCode, string employeeName, string SubsectionId);
        IEnumerable<object> GetEmployeeWithoutAttendanceGroupData(string plantId);
        GridModel GetEmployeeDataWithfilters(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName);

        GridModel GetEmployeeDataByCompany(GridParameter parameters, string companyId);

        GridModel Query(GridParameter parameters, string plantId, string entityId);

        GridModel GetEmployeeData(GridParameter parameters, string plantId);
        GridModel GetAllEmployeeData(GridParameter parameters);

        GridModel GetEmployeeDataWithIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName);
        GridModel GetEmployeeAttendanceGroupDataWithIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName);

        GridModel GetEmployeeDataWithPaidHoursIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName);

        IEnumerable<object> GetEmployeeDataWithEmployeeCode(string plantId, string employeeCode);

        IEnumerable<object> GetEmployeeWithoutPayrollGroupData(string plantId);

        GridModel GetEmployeeWithoutPaidhoursData(GridParameter parameters, string plantId);

        GridModel GetEmployeeWithSalaryProcessData(GridParameter parameters, string plantId, string Monthid,string YearId);
    }
}