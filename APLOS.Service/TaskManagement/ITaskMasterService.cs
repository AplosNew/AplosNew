#region Using

using Library.Core;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.TaskManagement
{
    public interface ITaskMasterService : IService<TaskMaster>
    {
        void Insert(TaskMaster entity, IEnumerable<TaskNotification> TaskNotificationList);
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
    }
}