#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IResignationReqcruitmentPlanningService : IService<RecruitmentPlanningProcessSet>
    {
        GridModel ResignedEmployeeQuery(GridParameter parameters, string companyId, string plantID, bool isControlAdmin, bool isSysAdmin, string employeeId);

        GridModel ResignedEmployeeQueryByEmpId(GridParameter parameters, string companyId, string plantID, string empId, bool isControlAdmin, bool isSysAdmin, string employeeId);

        void ProcessSetInsert(IEnumerable<RecruitmentPlanningProcessSet> entities);

        //void Save(RecruitmentPlanningProcessSet ui, out string masterid);
    }
}