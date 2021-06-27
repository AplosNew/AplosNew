#region Using

using Library.Core;
using Library.Model.Projects;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Projects
{
    public interface IProjectPlanningRequisitionService : IService<ProjectPlanningRequisition>
    {
        GridModel Query(GridParameter parameters);

        IEnumerable<object> GetCbo();

        IEnumerable<object> GetCompanyCurrencyCountryWise();

        IEnumerable<object> GetCoaIdByCompany();

        void DeleteGraph(string Id);

        GridModel FindById(GridParameter parameters, string id);

        GridModel QueryGraph(GridParameter parameters, string projectPlanningId);

        void DeleteMasterWithChild(string Id);

        string InsertAndUpdate(ProjectPlanningRequisition entity);

        IEnumerable<object> GetMaterialMasterAttributeValueList(string materialMasterId);
    }
}