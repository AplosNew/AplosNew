#region Using

using Library.Core;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.TaskManagement
{
    public interface ITaskManagerSubTasksService : IService<TaskManagerSubTasks>
    {
        GridModel Query(GridParameter parameters);
        List<Dictionary<string, object>> GetSubTaskByTaskManagerMasterId(string taskManagerMasterId);

        List<Dictionary<string, object>> GetTaskManagerSubTasksByResponsiblePersonId(string responsiblePersonId, string taskManagerMasterId);
    }
}