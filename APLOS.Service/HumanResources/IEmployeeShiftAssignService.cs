#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface IEmployeeShiftAssignService : IService<EmployeeShiftAssign>
    {
        void Insert(EmployeeShiftAssign entity, EmployeeWeekOffByDay employeeWeekOffByDay);

        IEnumerable<object> GetRoasterCboByPlant(string plantId);

        IEnumerable<object> GetRosterWiseShiftName(string plantId, string roasterId);

        GridModel GetEmployeeData(GridParameter parameters, string plantId, string empId);

        GridModel Query(GridParameter parameters, string plantId, string date);
    }
}