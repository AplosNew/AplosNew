#region Using

using Library.Core;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IssueTracker
{
    public interface IIssueFollowUpResponsibleService : IService<IssueFollowUpAudit>
    {
        Dictionary<string, object> GetFile(string systemId);
        GridModel Query(GridParameter parameters);
        //void Insert(IssueFollowUpResponsible entity, IEnumerable<IssueFollowUpResponsibleDetail> IssueFollowUpResponsibleDetailList);
        List<Dictionary<string, object>> GetById(string IssueFollowUpResponsibleId);
        List<Dictionary<string, object>> GetIssueFollowUpResponsibleByIssueTransactionId(string issueTransactionId);
        void InsertIssueFollowUpResponsible(IssueFollowUpAudit entity);
        //void InsertIssueFollowUpResponsibleDetail(IEnumerable<IssueFollowUpResponsibleDetail> IssueFollowUpResponsibleDetailList);
        GridModel GetListIssueFollowUpResponsible(GridParameter parameters);
        IssueFollowUpAudit IsFollowUpAuditReleased(string issueTransactionId);
    }
}