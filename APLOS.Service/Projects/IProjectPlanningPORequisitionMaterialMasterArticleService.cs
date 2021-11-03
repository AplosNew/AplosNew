#region Using

using Library.Core;
using Library.Model.Projects;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Projects
{
    public interface IProjectPlanningPORequisitionMaterialMasterArticleService : IService<ProjectPlanningPORequisitionMaterialMasterArticle>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningPORequisitionMaterialMasterArticle> entity, string poMaterialMasterId);

        void DeleteGraph(string projectPlanningDetailId);

        IEnumerable<object> QueryForProjectPlanningRequisitionMaterial(string plantId, string projectPlanningId);

        GridModel ProjectPlanningPORequisitionMaterialMasterArticleList(GridParameter parameters);

        GridModel ProjectPlanningPORequisitionMaterialMasterArticleSavedList(GridParameter parameters, string projectPlanningPODetailId);
    }
}