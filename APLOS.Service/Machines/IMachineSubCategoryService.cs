using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface IMachineSubCategoryService : IService<MachineSubCategory>
    {
        IEnumerable<object> GetMachineSubCategoryList();
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
    }
}