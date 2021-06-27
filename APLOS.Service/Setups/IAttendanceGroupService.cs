#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IAttendanceGroupService : IService<AttendanceGroup>
    {
        decimal GetAutoSequence();
        void DeleteGraph(string Id);
        GridModel Query(GridParameter parameters);
        IEnumerable<object> GetCbo();
    }
}