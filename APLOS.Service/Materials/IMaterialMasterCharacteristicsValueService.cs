using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialMasterCharacteristicsValueService : IService<MaterialMasterCharacteristicsValue>
    {
        decimal GetAutoSequence();

        IEnumerable<object> Query(string masterId);

        void InsertOrUpdateGraph(MaterialMasterCharacteristics characteristics, IEnumerable<MaterialMasterCharacteristicsValue> entities, IEnumerable<MaterialMasterCharacteristicsValue> dbList);

        void DeleteGraph(IEnumerable<MaterialMasterCharacteristicsValue> characteristicsValueList);
    }
}