using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface IMachineCategoryService : IService<MachineCategory>
    {
        IEnumerable<object> GetmachineCategoryList();
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
    }
}