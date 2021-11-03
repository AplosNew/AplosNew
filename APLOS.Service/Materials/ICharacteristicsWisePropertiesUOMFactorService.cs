using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface ICharacteristicsWisePropertiesUOMFactorService : IService<CharacteristicsWisePropertiesUOMFactor>
    {
        string GetPK();

        IEnumerable<CharacteristicsWisePropertiesUOMFactor> GetList(string MasterId);

        IEnumerable<CharacteristicsWisePropertiesUOMFactor> GetListByDetailId(string DetailId);

        GridModel GetSearchData(GridParameter parameters);
    }
}