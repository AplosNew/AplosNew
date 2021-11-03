using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface ICharacteristicsWisePropertiesDetailService : IService<CharacteristicsWisePropertiesDetail>
    {
        string GetPK();

        IEnumerable<CharacteristicsWisePropertiesDetail> GetList(string MasterId);

        CharacteristicsWisePropertiesDetail GetDetail(string PK);

        IEnumerable<object> GetDetailList(string MasterId);

        IEnumerable<object> GetDetailById(string id);

        GridModel GetSearchData(GridParameter parameters);

        IEnumerable<object> GetDetailByMaterialMasterId(string MaterialMasterId);

        IEnumerable<object> GetDetailByCharacteristicsValue(string MaterialMasterId, string Characteristics1ValueId, string Characteristics2ValueId, string Characteristics3ValueId);
    }
}