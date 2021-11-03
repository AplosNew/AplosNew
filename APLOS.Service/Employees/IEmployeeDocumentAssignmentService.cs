#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmployeeDocumentAssignmentService : IService<EmployeeInformation>
    {
        void InsertORUpdateMaster(IEnumerable<EmployeeInformation> entities);

        IEnumerable<object> GetDocumentDataList(string empId);

        GridModel GetEmployeeData(GridParameter parameters, string assign, string plantId);
    }
}