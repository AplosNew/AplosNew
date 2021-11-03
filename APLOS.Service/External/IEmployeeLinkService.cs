#region Using

using Library.Model.External;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public interface IEmployeeLinkService : IService<EmployeeLink>
    {
        void EmployeeLinkSend(EmployeeLink entity, IEnumerable<Employee> employeeList);
    }
}