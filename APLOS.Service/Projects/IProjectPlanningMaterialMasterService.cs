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
    public interface IProjectPlanningMaterialMasterService : IService<ProjectPlanningMaterialMaster>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningMaterialMaster> entity, string projectPlanningId);

        ProjectPlanningMaterialMaster Get(string projectPlanningId);

        void DeleteGraph(string projectPlanningDetailId);

        IEnumerable<object> QueryForProjectPlanningMaterial(string plantId, string projectPlanningId);

        GridModel ProjectplanninMaterialMasterList(GridParameter parameters, string budgetMstId);

        GridModel ProjectplanninMaterialMasterNonAssetList(GridParameter parameters);

        GridModel ProjectplanninMaterialMasterSavedList(GridParameter parameters, string projectPlanningDetailId);

        GridModel ProjectplanninMaterialMasterSavedListForRequisition(GridParameter parameters, string companyGroupId, string materialType, string projectPlanningId);

        IEnumerable<Object> getUomList(string materialMasterId);
    }
}