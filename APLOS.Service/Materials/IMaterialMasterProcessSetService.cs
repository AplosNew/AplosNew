#region Using

using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialMasterProcessSetService : IService<MaterialMasterProcessSet>
    {
        IEnumerable<object> Query(string materialMasterId);

        void InsertGraph(string materialMasterId, IEnumerable<MaterialMasterProcessSet> entity);

        void InsertOrUpdateGraph(string materialMasterId, IEnumerable<MaterialMasterProcessSet> entity);

        void DeleteGraph(string materialMasterId);
    }
}