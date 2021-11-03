using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IServiceGroupPartyAccountGroupGLService : IService<ServiceGroupPartyAccountGroupGL>
    {
        ServiceGroupPartyAccountGroupGL FindbyFKId(string key);

        void InsertOrUpdate(IEnumerable<ServiceGroupGL> masterlist, IEnumerable<ServiceGroupPartyAccountGroupGL> entities);

        void DeleteGraph(string id);
    }
}