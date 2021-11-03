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
    public interface IProjectPlanningPurchaseOrderService : IService<ProjectPlanningPurchaseOrder>
    {
        IEnumerable<object> GetCbo();

        IEnumerable<object> GetCompanyCurrencyCountryWise();

        IEnumerable<object> GetCoaIdByCompany();

        void DeleteGraph(string Id);

        GridModel FindById(GridParameter parameters, string id);

        void DeleteWithChild(string Id);

        GridModel ProjectPlanningRequisitionMaterialMasterSavedList(GridParameter parameters, string projectPlanningPODetailId, string materialType, string projectPlanningId);

        string InsertAndUpdate(ProjectPlanningPurchaseOrder projectPlanning);

        GridModel Query(GridParameter parameters);
    }
}