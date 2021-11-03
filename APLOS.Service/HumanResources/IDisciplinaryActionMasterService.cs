#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface IDisciplinaryActionMasterService : IService<DisciplinaryActionMaster>
    {
        //GridModel Query(GridParameter parameters);
        GridModel GetEmployeeData(GridParameter parameters, string plantId, string empId);

        GridModel Query(GridParameter parameters, string empSystemId);
    }
}