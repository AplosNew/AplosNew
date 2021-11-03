#region Using

using Library.Core;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IssueTracker
{
    public interface IIssueTransactionService : IService<IssueTransaction>
    {
        void Delete(string Id);
        IEnumerable<object> GetCbo();
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
        List<Dictionary<string, object>> GetById(string issueTransactionId);
        GridModel GetListIssueTransaction(GridParameter parameters);
        GridModel BuyerList(GridParameter parameters);
        IssueTransaction GetIssueTransaction(string issueTransactionId);
        List<Dictionary<string, object>> GetToDoList();
        List<Dictionary<string, object>> GetTodayTaskList();
        //string GetLogedInUser();
        //void InsertIssueRef(IssueRef entity);
    }
}