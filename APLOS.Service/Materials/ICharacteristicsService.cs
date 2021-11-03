using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface ICharacteristicsService : IService<Characteristics>
    {
        decimal GetAutoSequence();

        IEnumerable<object> GetCbo(string valueAssignment, string companyGroupId);

        GridModel GetSearchData(GridParameter parameters);

        GridModel GetCharacteristicsSearch(GridParameter parameters);

        Characteristics GetForCharacteristicsValue(string characteristicsId);
    }
}