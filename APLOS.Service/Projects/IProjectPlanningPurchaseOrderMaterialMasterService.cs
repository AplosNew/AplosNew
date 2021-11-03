#region Using

using Library.Core;
using Library.Model.Projects;
using Library.Service.Core;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Projects
{
    /// <summary>
    ///
    /// </summary>
    public interface IProjectPlanningPurchaseOrderMaterialMasterService : IService<ProjectPlanningPurchaseOrderMaterialMaster>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningPurchaseOrderMaterialMaster> entity, string projectPlanningId);

        ProjectPlanningPurchaseOrderMaterialMaster Get(string projectPlanningId);

        void DeleteGraph(string projectPlanningDetailId);

        IEnumerable<object> QueryForProjectPlanningPurchaseOrderMaterial(string plantId, string projectPlanningId);

        IEnumerable<Object> getUomList(string materialMasterId);

        GridModel ProjectPlanningPurchaseOrderMaterialMasterList(GridParameter parameters);

        IEnumerable<object> ProjectPlanningPurchaseOrderMaterialMasterSavedList(string projectPlanningPurchaseOrderId, string ProjectPlanningRequisitionId, string projectPlanningId);

        //GridModel ProjectPlanningPurchaseOrderMaterialMasterSavedList(GridParameter parameters);
        IEnumerable<Object> ProjectPlanningPORequisitionMaterialMasterArticleSavedList(string projectPlanningRequisitionId, string projectPlanningMaterialMasterId);
    }
}