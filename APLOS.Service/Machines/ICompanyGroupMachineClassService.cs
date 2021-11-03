using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface ICompanyGroupMachineClassService : IService<CompanyGroupMachineClass>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string machineClassId, bool active);

        void DeleteGraph(string machineClassId);

        GridModel Query(GridParameter parameters);
    }
}