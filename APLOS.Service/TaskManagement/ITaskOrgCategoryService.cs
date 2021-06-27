#region Using

using Library.Core;
using Library.Model.TaskManagement;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.TaskManagement
{
    public interface ITaskOrgCategoryService : IService<TaskOrgCategory>
    {
        IEnumerable<object> GetCbo();
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
    }
}