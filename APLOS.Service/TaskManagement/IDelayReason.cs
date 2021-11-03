#region Using

using Library.Core;
using Library.Model.TaskManagement;
using Library.Service.Core;

#endregion Using

namespace Library.Service.TaskManagement
{
    public interface IDelayReasonService : IService<DelayReason>
    {
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
    }
}