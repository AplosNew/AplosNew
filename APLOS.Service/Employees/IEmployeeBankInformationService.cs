#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmployeeBankInformationService : IService<EmployeeBankInformation>
    {
        GridModel GetEmployees(GridParameter parameters, string plantId);

        GridModel GetEmployeeBankHistory(GridParameter parameters, string empSystemId);

    }
}