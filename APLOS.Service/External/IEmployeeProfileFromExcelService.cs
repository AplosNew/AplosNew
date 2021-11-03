#region Using

using Library.Model.External;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public interface IEmployeeProfileFromExcelService : IService<EmployeeProfileFromExcel>
    {
        void Insert(List<EmployeeProfileFromExcel> entities);
    }
}