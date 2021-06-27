using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.OrderManagements
{
    public interface ICompanyGroupPortService : IService<CompanyGroupPort>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string portId, bool active);

        void DeleteGraph(string portId);

        GridModel Query(GridParameter parameters);
    }
}