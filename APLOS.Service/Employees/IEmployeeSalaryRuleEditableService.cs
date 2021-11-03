#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmployeeSalaryRuleEditableService : IService<EmployeeSalaryRuleEditable>
    {
        void InsertUpdate(IEnumerable<EmployeeSalaryRuleEditable> entities, string plantId);

        GridModel Query(GridParameter parameters, string plantId, string companyId);

        void DeleteGraph(string id);
    }
}