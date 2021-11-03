#region Using

using Library.Core;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.TaskManagement
{
    public interface ITaskManagerMasterService : IService<TaskManagerMaster>
    {
        GridModel Query(GridParameter parameters);
        List<Dictionary<string, object>> GetToDoList();
        TaskManagerMaster GetTaskManagerMaster(string issueTransactionId, string tasktype);
        TaskManagerMaster GetTaskManagerMasterByIssueTransactionId(string issueTransactionId);
        List<Dictionary<string, object>> GetTaskAccordingToRresponsiblePersonList(string authorizationType);
        void InsertTaskManagerMasterForIssue(TaskManagerMaster entity, out string Id);
    }
}