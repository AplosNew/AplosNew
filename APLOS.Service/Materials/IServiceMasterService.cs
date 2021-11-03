#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Materials
{
    public interface IServiceMasterService : IService<ServiceMaster>
    {
        decimal GetAutoSequence();

        void Delete(string id);

        GridModel Query(GridParameter parameters, string[] ids);
        GridModel QueryServiceMaster(GridParameter parameters);
    }
}