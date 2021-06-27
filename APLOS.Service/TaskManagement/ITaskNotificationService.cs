#region Using

using Library.Core;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.TaskManagement
{
    public interface ITaskNotificationService : IService<TaskNotification>
    {

        void InsertOrUpdateGraph(IEnumerable<TaskNotification> entitylist, string taskmasterid);
        GridModel Query(GridParameter parameters);
        
    }
}