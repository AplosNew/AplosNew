#region Using

using Library.Core;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.TaskManagement
{
    public interface ITaskAuditService : IService<TaskAudit>
    {

        GridModel Query(GridParameter parameters);
        TaskAudit GetTaskAudit(string taskManagerMasterId, string assignById);

        TaskAudit GetTaskAuditByTaskAuditId(string taskAuditId);
        IEnumerable<object> GetAuditOfReleasedIssue(string issueTransactionId, int audit);


    }
}