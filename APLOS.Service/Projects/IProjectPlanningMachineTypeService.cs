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
    public interface IProjectPlanningMachineTypeService : IService<ProjectPlanningMachineType>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningMachineType> entity, string projectPlanningId);

        ProjectPlanningMachineType Get(string projectPlanningId);

        void DeleteGraph(string projectPlanningDetailId);

        IEnumerable<object> QueryForProjectPlanningMachineType(string plantId, string projectPlanningId);

        GridModel ProjectplanninMachineTypeMasterList(GridParameter parameters, string projectPlanningDetailId);

        GridModel ProjectplanninMaterialMasterList(GridParameter parameters, string projectPlanningDetailId);

        IEnumerable<Object> getUomList(string assetItemId);
    }
}