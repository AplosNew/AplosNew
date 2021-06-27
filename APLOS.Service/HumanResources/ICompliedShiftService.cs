using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.HumanResources
{
    public interface ICompliedShiftService : IService<CompliedShift>
    {
        GridModel Query(GridParameter parameters, string plantId);

        

        IEnumerable<object> GetCbo(string plantId);
    }
}