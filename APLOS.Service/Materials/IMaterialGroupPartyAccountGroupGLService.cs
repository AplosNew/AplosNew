using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialGroupPartyAccountGroupGLService : IService<MaterialGroupPartyAccountGroupGL>
    {
        MaterialGroupPartyAccountGroupGL FindbyFKId(string key);

        void InsertOrUpdate(IEnumerable<MaterialGroupGL> masterlist, IEnumerable<MaterialGroupPartyAccountGroupGL> entities);

        void DeleteGraph(string id);
    }
}