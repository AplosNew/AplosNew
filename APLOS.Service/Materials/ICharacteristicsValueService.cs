using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface ICharacteristicsValueService : IService<CharacteristicsValue>
    {
        void InsertBOMSKU(CharacteristicsValue entity);
        IEnumerable<object> GetCharacteristicsValueCboByCharacteristicsId(string materialMasterId, string characteristicsId, string valueAssignmentLevel);
        GridModel Query(GridParameter parameters);

        IEnumerable<object> GetCharacteristicsValueList();
		IEnumerable<object> GetCbo(string companyGroupId, string characteristicsId);

		decimal GetAutoSequence(string characteristicsId, string materialId);

        GridModel Query(GridParameter parameters, string groupId, string characteristicsId, string[] ids);

        //IEnumerable<object> GetListBySelectedId(string[] ids);

        IEnumerable<object> GetCharacteristicsValueByCharacteristicsId(string CharacteristicsId);

        //GridModel CharacteristicsValueSearh(GridParameter parameters, string characteristicsId);

        GridModel GetCharacteristicsValueSearchData(GridParameter parameters, string groupId, string assignment, string materialMasterId, string charId);
		IEnumerable<object> GetCharacteristicsValueSearchData1(string groupId, string assignment, string materialMasterId, string charId);
		void DeleteGraph(string id);

        void InsertGraphFromMaterial(IEnumerable<CharacteristicsValue> entities, string groupId, string materialMasterId);

        void InsertUpdateOrDeleteFromMaterial(IEnumerable<CharacteristicsValue> entities, string groupId, string materialMasterId);

        void DeleteGraphFromMaterial(string materialMasterId);

        IEnumerable<object> GetCharacteristicsValueListByMaterialMaster(string materialMasterId);
    }
}