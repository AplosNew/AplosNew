#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IServiceGroupGLService : IService<ServiceGroupGL>
    {
        // void DeleteGraph(string id);
        void InsertUpdateServiceGroupDeterminate(IEnumerable<ServiceGroupGL> entities, IEnumerable<ServiceGroupPartyAccountGroupGL> materialGroupVendorReconGL);

        void InsertOrUpdate(string masterId, ServiceGroupGL entity);

        //GridModel GetServiceGroupTypeListById(GridParameter parameters, string fixedassetenum, string Id,string coaId);
        GridModel GetDataByServiceGroupId(GridParameter parameters, string fixedAssetMasterId, string coaId);

        GridModel GetSearchWithCombine(GridParameter parameters, string coaId);

        //GridModel GetSearchWithCombineCoa(GridParameter parameters, string coaId);
        GridModel GetSearchWithCombineCoa(GridParameter parameters);

        GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string coaId);

        GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string coaId);

        GridModel GetPartyAccountGroup(GridParameter parameters);

        GridModel GetPartyAccountVD(GridParameter parameters);

        void DeleteGraph(string id);
        void ServiceGroupGlReport();
    }
}