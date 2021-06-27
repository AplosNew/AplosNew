#region Using

using Library.Core;
using Library.Model.Projects;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Projects
{
    public interface IProjectPlanningCategoryService : IService<ProjectPlanningCategory>
    {
        IEnumerable<object> GetCbo();

        decimal GetAutoSequence();

        void Delete(string key);

        GridModel Query(GridParameter parameters);
    }
}