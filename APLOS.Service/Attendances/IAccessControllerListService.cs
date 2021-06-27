#region Using

using Library.Core;
using Library.Model.Attendances;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Attendances
{
    public interface IAccessControllerListService : IService<AccessControllerList>
    {
        void Insert(AccessControllerList entity, string companyGroupId);

        GridModel Query(GridParameter parameters, string companyGroupId, string plantId);

        IEnumerable<AccessControllerList> LoadAttdnRawData(string plantid, string ip);
        IEnumerable<ComboModel> GetCbo(string plantId);
    }
}