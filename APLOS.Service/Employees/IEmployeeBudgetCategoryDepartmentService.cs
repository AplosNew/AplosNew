#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmployeeBudgetCategoryDepartmentService : IService<EmployeeBudgetCategoryDepartment>
    {
        GridModel QueryWithDepartment(GridParameter parameters, string departmentId);

        GridModel QueryDepartmentWithCompany(GridParameter parameters);

        void InsertOrUpdateGraph(IEnumerable<EmployeeBudgetCategoryDepartment> entities);
    }
}