#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IWeeklyAbsentismAssignmentService : IService<WeeklyAbsentismAssignment>
    {
        IEnumerable<object> GetEmployeeData(string yearId, string month, string plantId, string day);
        IEnumerable<object> GetAssignedEmployeeList(string plantId, string month, string yearId);
        void InsertUpdate(IEnumerable<WeeklyAbsentismAssignment> entities);
        GridModel Query(GridParameter parameters, string plantId, string fromDate, string toDate);
        IEnumerable<object> GetOffDayData(string yearId, string month, string plantId);
        IEnumerable<object> GetEmployeesDetailsData(string workDate, string employeeCode);
        GridModel GetAssignedList(GridParameter parameters, string plantId, string month, string year);
        void DeleteMaster(string id);
    }
}