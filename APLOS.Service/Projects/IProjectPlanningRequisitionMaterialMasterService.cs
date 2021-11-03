#region Using

using Library.Core;
using Library.Model.Projects;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Projects
{
    public interface IProjectPlanningRequisitionMaterialMasterService : IService<ProjectPlanningRequisitionMaterialMaster>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningRequisitionMaterialMaster> entity, string projectPlanningRequisitionId, string projectPlanningId);

        ProjectPlanningRequisitionMaterialMaster Get(string projectPlanningId);

        void DeleteGraph(string projectPlanningDetailId);

        void DeleteWithMaster(string Id);

        IEnumerable<object> QueryForProjectPlanningRequisitionMaterial(string plantId, string projectPlanningId);

        IEnumerable<Object> GetUomList(string materialMasterId);

        GridModel ProjectPlanningRequisitionMaterialMasterList(GridParameter parameters);

        IEnumerable<object> ProjectPlanningRequisitionMaterialMasterSavedList(string projectPlanningRequisitionId);

        IEnumerable<Object> ProjectPlanningRequisitionMaterialMasterArticleSavedList(string projectPlanningRequisitionId, string projectPlanningMaterialMasterId);

        IEnumerable<Object> ProjectPlanningRequisitionMaterialMasterArticleSavedListForPO(string projectPlanningRequisitionId, string projectPlanningMaterialMasterId);

        IEnumerable<object> GetMaterialUOMValueConversation(string baseUoMId, string selectedUoMId, int quantity, string materialMasterId, string planningUoMId);
    }
}