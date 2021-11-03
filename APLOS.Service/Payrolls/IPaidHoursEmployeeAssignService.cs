#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Payrolls
{
    public interface IPaidHoursEmployeeAssignService : IService<PaidHoursEmployeeAssign>
    {
        void InsertOrUpdateGraph(IEnumerable<PaidHoursEmployeeAssign> entities);
        void DeleteGraph(string Id);
        GridModel Query(GridParameter parameters, string companyGroupId, string paidHours, string plantId);

        GridModel QueryWithEmployee(GridParameter parameters, string companyGroupId, string employeeId, string[] payrollGroupIds);

    }
}