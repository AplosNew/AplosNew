#region Using

using Library.Core;
using Library.Model.External;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public interface IAplosEmpFieldService : IService<AplosEmpField>
    {
        IEnumerable<object> GetCbo();

        GridModel Query(GridParameter parameters);
    }
}