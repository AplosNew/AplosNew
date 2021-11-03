using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using Library.ViewModel.Materials;
using System.Collections.Generic;
using System.Text;

namespace Library.Service.Materials
{
	public interface IMaterialAttributeValueService : IService<MaterialAttributeValue>
	{
		IEnumerable<object> GetCbo(string companyGroupId, string attributeId);

		decimal GetAutoSequence(string materialAttributeId, string materialId);

		GridModel Query(GridParameter parameters, string materialAttributeId);

		GridModel GetAttributeValueList(GridParameter parameters, string assignment, string materialMasterId, string attributeId);

		void DeleteGraphByMaterial(string materialMasterId);

		void InsertGraphFromMaterial(IEnumerable<MaterialAttributeValue> entities, IEnumerable<MaterialAttributeViewModel> materialMasterAttribute, string groupId, string materialMasterId);

		void InsertUpdateOrDeleteFromMaterial(IEnumerable<MaterialAttributeValue> entities, IEnumerable<MaterialAttributeViewModel> materialMasterAttribute, string groupId, string materialMasterId, StringBuilder rdBuilder);

		IEnumerable<object> GetAttributeValueListByMaterialMaster(string materialMasterId);

        void InsertOrUpdate(MaterialAttributeValue entity);

    }
}