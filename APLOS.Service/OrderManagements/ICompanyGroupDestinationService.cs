using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.OrderManagements
{
    public interface ICompanyGroupDestinationService : IService<CompanyGroupDestination>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string destinationtId, bool active);

        IEnumerable<object> GeDestinationCbo(string portid, string groupId);
        IEnumerable<object> GeDestinationCbobyCountry(string CountryId, string groupId);
        void DeleteGraph(string destinationtId);

        GridModel Query(GridParameter parameters);
    }
}