#region Using

using Library.Core;
using Library.Model.Projects;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Projects
{
    /// <summary>
    ///
    /// </summary>
    public interface IProjectPlanningDetailService : IService<ProjectPlanningDetail>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningDetail> entity, string projectPlanningId);

        ProjectPlanningDetail Get(string projectPlanningId);

        void DeleteGraph(string Id);

        void DeleteWithMaster(string Id);

        IEnumerable<object> QueryForProjectPlanningDetail(string plantId, string projectPlanningId);

        GridModel QueryForProjectPlanningDetailWithPPId(GridParameter parameters, string projectPlanningId);

        GridModel QueryForProjectPlanningDetailWithPPIdAndCat(GridParameter parameters, string projectPlanningId, string projectPlanningCategory, string projectPlanningSubCategory);
    }
}