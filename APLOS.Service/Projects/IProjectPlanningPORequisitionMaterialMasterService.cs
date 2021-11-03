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
    public interface IProjectPlanningPORequisitionMaterialMasterService : IService<ProjectPlanningPORequisitionMaterialMaster>
    {
        IEnumerable<object> GetCbo();

        IEnumerable<object> GetCompanyCurrencyCountryWise();

        IEnumerable<object> GetCoaIdByCompany();

        void DeleteGraph(string Id);

        void DeleteWithMaster(string Id);

        GridModel FindById(GridParameter parameters, string id);

        void InsertORUpdate(ProjectPlanningPurchaseOrder projectplanningPurchaseOrder, IEnumerable<ProjectPlanningPORequisitionMaterialMaster> projectPlanningPORequisitionMaterial);
    }
}