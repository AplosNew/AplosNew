using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface ICharacteristicsWisePropertiesMasterService : IService<CharacteristicsWisePropertiesMaster>
    {
        IEnumerable<object> GetList();

        IEnumerable<object> GetList(string masterid);

        IEnumerable<object> GetUOMList(string masterid);

        IEnumerable<object> GetUOMFactorList(string masterid);

        void InsertORUpdate(CharacteristicsWisePropertiesDetail detail, IEnumerable<CharacteristicsWisePropertiesUOMFactor> characteristicswisepropertiesuomfactor, IEnumerable<CharacteristicsWisePropertiesUOM> characteristicswisepropertiesuom);

        void InsertORUpdate(CharacteristicsWisePropertiesMaster master, out string masterid);

        GridModel GetSearchData(GridParameter parameters);

        void DeleteMasterDetail(string masterid);

        void DeleteDetail(string detailid);

        IEnumerable<object> GetMasterId(string materialmasterid);
    }
}