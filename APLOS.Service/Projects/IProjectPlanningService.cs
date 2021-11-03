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
    public interface IProjectPlanningService : IService<ProjectPlanning>
    {
        IEnumerable<object> GetCbo();

        IEnumerable<object> GetCompanyCurrencyCountryWise();

        IEnumerable<object> GetCoaIdByCompany();

        void DeleteGraph(string Id);

        GridModel FindById(GridParameter parameters, string id);

        string InsertAndUpdate(ProjectPlanning projectPlanning, IEnumerable<ProjectPlanningDetail> ProjectPlanningDetail, IEnumerable<ProjectPlanningMachineType> projectPlanningFixedAsset, IEnumerable<ProjectPlanningMaterialMaster> projectPlanningMaterial);

        GridModel Query(GridParameter parameters);
    }
}