using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Core;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Materials.Controllers
{
	public class MaterialGroupMasterController : BaseController
	{
		#region -- constrator
		private readonly IMaterialGroupMasterService _materialGroupMasterService;
		private readonly IMaterialGroupAlternativeUoMService _alternativeUoMService;
		private readonly IMaterialGroupPackingFormService _packingFormService;
		public MaterialGroupMasterController(
			IMaterialGroupMasterService materialGroupMasterService
			, IMaterialGroupAlternativeUoMService alternativeUoMService
			, IMaterialGroupPackingFormService packingFormService
			)
		{
			_materialGroupMasterService = materialGroupMasterService;
			_alternativeUoMService = alternativeUoMService;
			_packingFormService = packingFormService;
		}
		#endregion

		#region -- pages

		[HttpGet, Authorize]
		public ActionResult Aplos()
		{
			return View();
		}
		
		#endregion

		#region -- Operations

		[HttpGet, Authorize]
		public ActionResult GetList(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialGroupMasterService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetMGMAlternativeUoMList(string masterId)
		{
			return Json(_alternativeUoMService.GetAltUomListMasterId(masterId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetMGMAlternativeUoMAllList()
		{
			return Json(_alternativeUoMService.GetAltUomList(), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetPackingFormList(string masterId)
		{
			return Json(_packingFormService.Query(masterId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetListByMaterialType(GridParameter parameters, string materialTypeId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialGroupMasterService.GetListByMaterialType(parameters, materialTypeId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetListByFinishedGoods(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialGroupMasterService.GetListByFinishedGoods(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetHierarchy(GridParameter parameters, string id)
		{
			return Json(_materialGroupMasterService.GetHierarchy(parameters, id), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetCboByMaterialMaster()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(new SelectList(_materialGroupMasterService.GetCboByMaterialMaster(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetCbo()
		{
			return Json(_materialGroupMasterService.GetMaterialGroupMasterCbo(), JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult Create(MaterialGroupMaster entity, IEnumerable<MaterialGroupAlternativeUoM> altUoMList, IEnumerable<MaterialGroupPackingForm> packing, IEnumerable<MaterialGroupProductionProcessGroup> processGroupList)
		{
			_materialGroupMasterService.InsertGraph(entity, altUoMList, packing, processGroupList);
			return Json(new { MaterialGroupMaster = entity, Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Edit(MaterialGroupMaster entity, IEnumerable<MaterialGroupAlternativeUoM> altUoMList, IEnumerable<MaterialGroupPackingForm> packing, IEnumerable<MaterialGroupProductionProcessGroup> processGroupList)
		{
			_materialGroupMasterService.UpdateGraph(entity, altUoMList, packing, processGroupList);
			return Json(new { Message = AplosMessage.Updated });
		}

		[HttpPost]
		public ActionResult Delete(string id)
		{
			_materialGroupMasterService.DeleteGraph(id);
			return Json(new { Message = AplosMessage.Deleted });
		}
		
		#endregion

		#region Product Process Group

		[HttpGet, Authorize]
		public ActionResult GetProductProcessGroupList(GridParameter parameters, string ids)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialGroupMasterService.GetProductProcessGroupList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetMaterialProductProcessGroupList(string masterId)
		{
			return Json(_materialGroupMasterService.GetMaterialProductProcessGroupList(masterId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetMaterialPrdGroupList(string mgMasterId, string articleId)
		{
			return Json(_materialGroupMasterService.GetMaterialPrdGroupList(mgMasterId, articleId), JsonRequestBehavior.AllowGet);
		}
		
		#endregion

		#region Article

		[HttpGet, Authorize]
		public ActionResult Article()
		{
			return View();
		}
		[HttpGet, Authorize]
		public ActionResult GetArticleList(GridParameter parameters, string mGroupId)
		{
			return Json(_materialGroupMasterService.GetArticleList(parameters, mGroupId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetProcessCriteriaList(string id)
		{
			return Json(_materialGroupMasterService.GetProcessCriteriaList(id), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetAttributeList(string groupMasterId, string articleId)
		{
			return Json(_materialGroupMasterService.GetAttributeList(groupMasterId, articleId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetAttributeValueList(GridParameter parameters, string attributeId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialGroupMasterService.GetAttributeValueList(parameters, identity.CompanyGroupId, attributeId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetCriteriaList(GridParameter parameters, string ids)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialGroupMasterService.GetCriteriaList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
		}
		[HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
		public ActionResult DeleteProcessCriteria(string id)
		{
			_materialGroupMasterService.DeleteProcessCriteria(id);
			return Json(new { Message = AplosMessage.Deleted });
		}
		[HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
		public ActionResult CreateOrEditArticle(MaterialGroupArticle article, IEnumerable<MaterialGroupArticleValue> valueList, IEnumerable<MaterialGroupArticlePrdProcessGroup> processGroupList)
		{
			_materialGroupMasterService.InsertOrUpdateArticleGraph(article, valueList, processGroupList);
			return Json(new { Message = AplosMessage.Deleted });
		}
		
		#endregion

		#region Report

		public ActionResult MaterialGroupMasterReport()
		{
			var fileName = "Material Group Master Report " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
			var workbook = _materialGroupMasterService.GetMaterialGroupMaster();
			workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
			return null;
		}

		#endregion
	}
}