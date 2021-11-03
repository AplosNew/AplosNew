#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface IEmployeeDisciplinaryActionService : IService<EmployeeDisciplinaryAction>
    {
        IEnumerable<object> Query(string EmpId);
        GridModel QueryActionCount(GridParameter parameters, string plantId);
       

    }
}