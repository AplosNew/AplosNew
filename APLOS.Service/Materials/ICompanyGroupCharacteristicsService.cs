using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface ICompanyGroupCharacteristicsService : IService<CompanyGroupCharacteristics>
    {
        IEnumerable<object> GetCompanyGroupWiseCharacteristicsList();

        GridModel GetSearchData(GridParameter parameters);

        void DeleteGraph(string charaterId);
    }
}