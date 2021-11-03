using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.OrderManagements
{
    public interface ICompanyGroupOrderCategoryService : IService<CompanyGroupOrderCategory>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string productionStatusId, bool active);

        void DeleteGraph(string productionStatusId);

        GridModel Query(GridParameter parameters);
    }
}