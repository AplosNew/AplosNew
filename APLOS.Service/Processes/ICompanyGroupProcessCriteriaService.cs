using Library.Core;
using Library.Model.Processes;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Processes
{
    public interface ICompanyGroupProcessCriteriaService : IService<CompanyGroupProcessCriteria>
    {
        IEnumerable<object> GetCbo(string companyGroupId);

        void UpdateGraph(string processCriteriaId, bool active);

        void DeleteGraph(string processCriteriaId);

        IEnumerable<ComboModel> GetWeightUomCbo(string materialMasterId);

        IEnumerable<ComboModel> GetCbo();

        GridModel Query(GridParameter parameters);
    }
}