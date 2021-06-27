#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialGroup2Service : IService<MaterialGroup2>
    {
        IEnumerable<object> GetCboList();

        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters);
    }
}