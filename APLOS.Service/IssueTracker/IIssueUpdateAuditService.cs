#region Using

using Library.Core;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IssueTracker
{
    public interface IIssueUpdateAuditService : IService<IssueUpdateAudit>
    {
        Dictionary<string, object> GetFile(string systemId);
        GridModel Query(GridParameter parameters);
        void Insert(IssueUpdateAudit entity, IEnumerable<IssueRefDetail> issueRefDetailList);
        List<Dictionary<string, object>> GetById(string issueRefId);
        List<Dictionary<string, object>> GetIssueUpdateAuditByIssueTransactionId(string issueTransactionId);
        //void InsertIssueRef(IssueRef entity);
        void InsertIssueRefDetail(IEnumerable<IssueRefDetail> issueRefDetailList);
        GridModel GetListIssueRef(GridParameter parameters);
        void InsertIssueUpdateAudit(IssueUpdateAudit entity);
        IssueUpdateAudit IsUpdateAuditReleased(string issueTransactionId);

    }
}