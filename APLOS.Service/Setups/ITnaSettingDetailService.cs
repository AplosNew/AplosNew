using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Setups
{
    public interface ITnaSettingDetailService : IService<TnaSettingDetail>
    {
        void  InsertUpdate(IEnumerable<TnaSettingDetail> entities,string masterId);
        void DeleteGraph(string id);
        GridModel Query(GridParameter parameters, string shiftGroupId, string plantId);
    }
}