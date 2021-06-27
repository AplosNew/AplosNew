#region Using

using Library.Core;
using Library.Model.IssueTracker;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IssueTracker
{
    public interface IIssueSubTaskService : IService<IssueSubTask>
    {
        GridModel Query(GridParameter parameters);
        List<Dictionary<string, object>> GetSubTaskByIssueTransactionId(string issueTransactionId);
        //void UpdateSubTask(List<IssueSubTask> SubTasks);
    }
}