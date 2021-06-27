using Library.Core;
using Library.Model.Productions;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Productions
{
    public interface ICompanyGroupProductionStatusService : IService<CompanyGroupProductionStatus>
    {
        IEnumerable<object> GetCbo();

        IEnumerable<object> GetStatusCbo(string companyGroupId);

        void UpdateGraph(string productionStatusId, bool active);

        void DeleteGraph(string productionStatusId);

        GridModel Query(GridParameter parameters);
    }
}