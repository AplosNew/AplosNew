using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.OrderManagements
{
    public interface ICompanyGroupShipModeService : IService<CompanyGroupShipMode>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string shipModeId, bool active);

        IEnumerable<object> GeShipModeCbo(string portid);

        void DeleteGraph(string shipModeId);

        GridModel Query(GridParameter parameters);
    }
}