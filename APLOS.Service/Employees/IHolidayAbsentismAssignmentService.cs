#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IHolidayAbsentismAssignmentService : IService<HolidayAbsentismAssignment>
    {
        IEnumerable<ComboModel> GetHolidayCbo(string yearId, string month, string plantId);
        IEnumerable<object> GetEmployeeData(string workDate, string day, string plantId);
        IEnumerable<object> GetAssignedEmployeeList(string plantId, string workDate);
        void InsertUpdate(IEnumerable<HolidayAbsentismAssignment> entities);
        GridModel Query(GridParameter parameters, string plantId, string fromDate, string toDate);
        IEnumerable<object> GetHolidayData(string yearId, string month, string plantId);
        IEnumerable<object> GetEmployeesDetailsData(string workDate, string employeeCode);
        GridModel GetAssignedList(GridParameter parameters, string plantId, string workDate);
        void DeleteMaster(string id);
    }
}