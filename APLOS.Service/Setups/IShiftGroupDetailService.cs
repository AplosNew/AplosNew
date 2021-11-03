using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Setups
{
    public interface IShiftGroupDetailService : IService<ShiftGroupDetail>
    {
        void InsertUpdate(IEnumerable<ShiftGroupDetail> entities);
        void DeleteGraph(string id);
        GridModel Query(GridParameter parameters, string shiftGroupId, string plantId);
    }
}