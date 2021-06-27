using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface ICharacteristicsWisePropertiesUOMService : IService<CharacteristicsWisePropertiesUOM>
    {
        string GetPK();

        IEnumerable<CharacteristicsWisePropertiesUOM> GetList(string MasterId);

        IEnumerable<CharacteristicsWisePropertiesUOM> GetListByDetailId(string detailid);

        GridModel GetSearchData(GridParameter parameters);

        IEnumerable<object> GetCharValueUOMByMaterialMasterId(string MaterialMasterId);

        IEnumerable<object> GetUOMByCharacteristicsValue(string MaterialMasterId, string Characteristics1ValueId, string Characteristics2ValueId, string Characteristics3ValueId);

        IEnumerable<object> GetUOMByCharacteristicsValue1st(string MaterialMasterId, string Characteristics1ValueId);

        IEnumerable<object> GetUOMByCharacteristicsValue2nd(string MaterialMasterId, string Characteristics1ValueId, string Characteristics2ValueId);

        IEnumerable<object> GetUOMByCharacteristicsValue3rd(string MaterialMasterId, string Characteristics1ValueId, string Characteristics2ValueId, string Characteristics3ValueId);
    }
}