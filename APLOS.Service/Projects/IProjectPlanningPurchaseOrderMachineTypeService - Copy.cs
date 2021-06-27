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
    public interface IProjectPlanningPurchaseOrderMachineTypeService : IService<ProjectPlanningPurchaseOrderMachineType>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningPurchaseOrderMachineType> entity, string projectPlanningPurchaseOrderId);

        ProjectPlanningPurchaseOrderMachineType Get(string ProjectPlanningPurchaseOrderId);

        void DeleteGraph(string projectPlanningPurchaseOrderDetailId);

        IEnumerable<object> QueryForProjectPlanningPurchaseOrderMachineType(string plantId, string ProjectPlanningPurchaseOrderId);

        GridModel ProjectplanninPurchaseOrderMachineTypeMasterList(GridParameter parameters, string projectPlanningPurchaseOrderDetailId);
    }
}