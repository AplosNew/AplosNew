#region Using

using Library.Model.Projects;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Projects
{
    /// <summary>
    ///
    /// </summary>
    public interface IProjectPlanningPurchaseOrderDetailService : IService<ProjectPlanningPurchaseOrderDetail>
    {
        void InsertOrUpdate(IEnumerable<ProjectPlanningPurchaseOrderDetail> entity, string fixedAssetPurchaseOrderId);

        ProjectPlanningPurchaseOrderDetail Get(string fixedAssetPurchaseOrderId);

        void DeleteWithMaster(string Id);

        void DeleteWithChild(string Id);

        IEnumerable<object> QueryForProjectPlanningPurchaseOrderDetail(string projectPlanningPurchaseOrderId);
    }
}