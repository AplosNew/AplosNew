#region 
using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;
using Syncfusion.XlsIO;
#endregion

namespace Library.Service.Materials
{
	public interface IMaterialGroupMasterService : IService<MaterialGroupMaster>
	{
		GridModel Query(GridParameter parameters, string companyGroupId);
		/// <summary>
		/// GetMaterialGroupMasterCbo dropdown list.
		/// </summary>
		/// <returns></returns>
		IEnumerable<object> GetMaterialGroupMasterCbo();
		IEnumerable<ComboModel> GetCboByMaterialMaster(string companyGroupId);
		GridModel GetHierarchy(GridParameter parameters, string id);
		GridModel GetListByMaterialType(GridParameter parameters, string materialTypeId, string companyGroupId);
		GridModel GetListByFinishedGoods(GridParameter parameters, string companyGroupId);
		IWorkbook GetMaterialGroupMaster();
		GridModel GetArticleList(GridParameter parameters, string mGroupId);
		void InsertGraph(MaterialGroupMaster entity, IEnumerable<MaterialGroupAlternativeUoM> altUoMList, IEnumerable<MaterialGroupPackingForm> packing, IEnumerable<MaterialGroupProductionProcessGroup> processGroupList);
		void UpdateGraph(MaterialGroupMaster entity, IEnumerable<MaterialGroupAlternativeUoM> altUoMList, IEnumerable<MaterialGroupPackingForm> packing, IEnumerable<MaterialGroupProductionProcessGroup> processGroupList);
		void DeleteGraph(string key);
		GridModel GetProductProcessGroupList(GridParameter parameters, string groupId, string[] ids);
		IEnumerable<object> GetMaterialProductProcessGroupList(string masterId);
		IEnumerable<object> GetMaterialPrdGroupList(string mgMasterId, string articleId);
		IEnumerable<object> GetProcessCriteriaList(string id);
		GridModel GetCriteriaList(GridParameter parameters, string groupId, string[] ids);
		void InsertOrUpdateArticleGraph(MaterialGroupArticle article, IEnumerable<MaterialGroupArticleValue> valueList, IEnumerable<MaterialGroupArticlePrdProcessGroup> processGroupList);
		void DeleteProcessCriteria(string key);
		IEnumerable<object> GetAttributeList(string groupMasterId, string articleId);
		GridModel GetAttributeValueList(GridParameter parameters, string groupId, string attributeId);
	}
}
