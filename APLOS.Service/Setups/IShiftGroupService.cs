#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IShiftGroupService : IService<ShiftGroup>
    {
        decimal GetAutoSequence(string plantId, string joblocationId);
        void DeleteGraph(string id);
        IEnumerable<object> GetCbo(string plantId, string joblocationId);
        GridModel Query(GridParameter parameters, string plantId, string joblocationId);
        IEnumerable<object> JobLocationCbo(string companyGroupId, string plantId);
    }
}