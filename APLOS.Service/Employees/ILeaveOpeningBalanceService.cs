#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ILeaveOpeningBalanceService : IService<LeaveOpeningBalance>
    {
        void InsertUpdate(IEnumerable<LeaveOpeningBalance> entities, string plantId);

        GridModel Query(GridParameter parameters, string plantId, string companyId);

        GridModel GetLeaveTypeList(GridParameter parameters, string employeeId, string calendarId, string plantId, string companyGroupId);

        void DeleteGraph(string id);
    }
}