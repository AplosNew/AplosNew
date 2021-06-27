#region Using

using Library.Core;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IssueTracker
{
    public interface IIssueCategoryService : IService<IssueCategory>
    {
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
        IEnumerable<object> GetCbo();
    }
}