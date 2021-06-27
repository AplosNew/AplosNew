#region Using

using Library.Core;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IssueTracker
{
    public interface IIssueGroupService : IService<IssueGroup>
    {
        
        GridModel Query(GridParameter parameters);
       
       
    }
}