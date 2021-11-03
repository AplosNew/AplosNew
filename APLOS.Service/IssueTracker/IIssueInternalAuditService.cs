#region Using

using Library.Core;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IssueTracker
{
    public interface IIssueInternalAuditService : IService<IssueInternalAudit>
    {
        //Dictionary<string, object> GetFile(string systemId);
        GridModel Query(GridParameter parameters);
        //void Insert(IssueInternalAudit entity, IEnumerable<IssueAuditDetail> issueAuditDetailList);
        //List<Dictionary<string, object>> GetById(string issueAuditId);
        //List<Dictionary<string, object>> GetIssueAuditByIssueTransactionId(string issueTransactionId);
        //void InsertIssueAudit(IssueInternalAudit entity);
        //void InsertIssueAuditDetail(IEnumerable<IssueAuditDetail> issueAuditDetailList);
        GridModel GetListIssueAudit(GridParameter parameters);
        IssueInternalAudit IsInternalAuditReleased(string issueTransactionId);
    }
}