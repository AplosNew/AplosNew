#region Using

using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialAttributeMasterService : IService<MaterialAttributeMaster>
    {
        IEnumerable<object> Query(string materialGroupMasterId);

        IEnumerable<object> QueryForMaterialMaster(string materialGroupMasterId);

        void Save(IEnumerable<MaterialAttributeMaster> entites);
    }
}