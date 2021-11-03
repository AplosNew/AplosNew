using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialMasterProcessRoutingService : IService<MaterialMasterProcessRouting>
    {
        IEnumerable<object> GetProcessRoutingList(string groupId, string materialMasterId);

        void DeleteGraph(string materialMasterId);
    }
}