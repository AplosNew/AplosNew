using System.Collections.Generic;

using Library.Core;
using Library.Model.EmployeeServices;
using Library.Service.Core;

namespace Library.Service.EmployeeServices
{
   
    public interface IServiceTypeAndCategoryService : IService<ServiceTypeAndCategory>
    {

        IEnumerable<object> GetEmpName(string Empcode);
        IEnumerable<object> GetAllServices();
        List<ServiceTypeAndCategory> GetList(string Service);
        List<ServiceCategory> GetCategoryList(string Service);

    }
}
