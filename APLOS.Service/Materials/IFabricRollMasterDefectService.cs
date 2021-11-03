#region Using

using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IFabricRollMasterDefectService : IService<FabricRollMasterDefect>
    {
        IEnumerable<FabricRollMasterDefect> QueryList(string value);
        void DeleteGraph(string id);
    }
}