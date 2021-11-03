#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IEmployeeAttendanceGroupService : IService<EmployeeAttendanceGroup>
    {
        void InSertOrUpdate(EmployeeAttendanceGroup entity);
        void SalaryProcessDelete(string employeesId, string month, string year);
        void InsertOrUpdateGraph(IEnumerable<EmployeeAttendanceGroup> entities);
        void DeleteGraph(string Id);
        GridModel Query(GridParameter parameters, string companyGroupId, string attendanceGroupId, string plantId);

        GridModel QueryWithEmployee(GridParameter parameters, string companyGroupId, string employeeId, string[] attendanceGroupIds);
        IEnumerable<object> AttendanceGroupQuery(string companyGroupId, string attendanceGroupId, string plantId);
        GridModel QueryWithUser(GridParameter parameters, string companyGroupId, string userId);
        
    }
}