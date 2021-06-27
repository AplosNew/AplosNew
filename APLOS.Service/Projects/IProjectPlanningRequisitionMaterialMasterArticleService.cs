#region Using

using Library.Core;
using Library.Model.Projects;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Projects
{
    public interface IProjectPlanningRequisitionMaterialMasterArticleService : IService<ProjectPlanningRequisitionMaterialMasterArticle>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningRequisitionMaterialMasterArticle> entity, string requisitionMaterialMasterId);

        ProjectPlanningRequisitionMaterialMasterArticle Get(string projectPlanningId);

        void DeleteGraph(string projectPlanningDetailId);

        IEnumerable<object> QueryForProjectPlanningRequisitionMaterial(string plantId, string projectPlanningId);

        IEnumerable<Object> getUomList(string materialMasterId);

        GridModel ProjectPlanningRequisitionMaterialMasterArticleList(GridParameter parameters);

        GridModel ProjectPlanningRequisitionMaterialMasterArticleSavedList(GridParameter parameters, string projectPlanningPODetailId);
    }
}