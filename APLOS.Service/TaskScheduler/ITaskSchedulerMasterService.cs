#region Using

using Library.Core;
using Library.Model.TaskScheduler;
using Library.Service.Core;

#endregion Using

namespace Library.Service.TaskScheduler
{
    public interface ITaskSchedulerMasterService : IService<TaskSchedulerMaster>
    {
        //decimal GetAutoSequence();

        //IEnumerable<object> GetCbo();

        GridModel Query(GridParameter parameters);
        TaskSchedulerMaster GetTaskScheduleByAuditTaskSchedulerMasterId(string auditTaskSchedulerMasterId);
    }
}