#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Payrolls
{
    public interface IParollsReportService 
    {
        
       
        GridModel Query(GridParameter parameters, string companyGroupId, string payrollGroupId, string plantId);

        GridModel QueryWithEmployee(GridParameter parameters, string companyGroupId, string employeeId, string[] payrollGroupIds);
        IEnumerable<object> PayRollGroupQuery(string companyGroupId, string payrollGroupId, string plantId);
        GridModel QueryWithUser(GridParameter parameters, string companyGroupId, string userId);
        
    }
}