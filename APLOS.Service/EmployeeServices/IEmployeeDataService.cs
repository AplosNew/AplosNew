using System.Collections.Generic;

using Library.Core;
using Library.Model.EmployeeServices;
using Library.Service.Core;

namespace Library.Service.EmployeeServices
{
   
    public interface IEmployeeDataService : IService<EmployeeData>
    {
        IEnumerable<object> EmpCodeId(string CompanyGroupId);
        IEnumerable<object> GetList(string AddedBy);
        IEnumerable<object> GetShiftMaster(string PlantId);

        string Create(IEnumerable<EmployeeData> DataToSave);

        string Delete(IEnumerable<EmployeeData> DataToDelete);
        IEnumerable<object> GetCount(string EmpId, string Service);
        IEnumerable<object> GetDeduction(string EmpId, string Service);

        IEnumerable<object> GetEmpType(string EmpId);
        IEnumerable<object> GetUpdatedDeduction(string EmpId, string Service);
    }
}
