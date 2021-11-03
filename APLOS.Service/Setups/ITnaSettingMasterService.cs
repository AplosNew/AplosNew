#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ITnaSettingMasterService : IService<TnaSettingMaster>
    {
        void InsertOrUpdate(TnaSettingMaster entity, IEnumerable<TnaSettingDetail> entities);
        void DeleteGraph(string id);
        GridModel Query(GridParameter parameters, string plantId);
    }
}