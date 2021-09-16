using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;
using Library.MaterialManagement.Inventory;
using Library.Model.Inventory;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using Library.ViewModel.Materials;
using System.Collections.Generic;
using System.Linq;
using Library.Model.Enums;
using Library.MaterialManagement.Reports;
using System;
using Library.Data;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Reflection;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Data.Repositories;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Data;
using Library.Service.Currencies;
using Library.Service.Organizations;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Library.MaterialManagement.InventoryManagements;
using Library.Accounting.Accounts;
using Newtonsoft.Json;

namespace Aplos.Areas.Products.Controllers
{
	public class InventoryIssueController : BaseController
	{
		#region Constructor

		private readonly IInventoryIssueService _inventoryIssueService;
		private readonly IInventoryIssueDetailService _inventoryDetailService;
		private readonly IInventoryMaterialService _inventoryMaterialService;
		private readonly IInventoryReceiveService _inventoryReveiveService;
		private readonly ISqlRepository _sqlRepository;
		private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
		private readonly CompanyParallelCurrencyService _companyParallelCurrencyService;
		private readonly IPlantService _plantService;

		public InventoryIssueController(IInventoryIssueService inventoryIssueService
			, IInventoryIssueDetailService inventoryDetailService
			, IInventoryMaterialService inventoryMaterialService
			, IInventoryReceiveService inventoryReveiveService
			, IRepositoryAsync<CompanyParty> companyPartyRepository
			, CompanyParallelCurrencyService companyParallelCurrencyService
			, IPlantService plantService
			, ISqlRepository sqlRepository)
		{
			_inventoryIssueService = inventoryIssueService;
			_inventoryDetailService = inventoryDetailService;
			_inventoryMaterialService = inventoryMaterialService;
			_inventoryReveiveService = inventoryReveiveService;
			_sqlRepository = sqlRepository;
			_companyPartyRepository = companyPartyRepository;
			_companyParallelCurrencyService = companyParallelCurrencyService;
			_plantService = plantService;
		}

		#endregion Constructor

		#region Aplos

		public ActionResult Aplos()
		{
			return View();
		}
		public ActionResult SlipIssue()
		{
			return View();
		}

		[Authorize]
		public ActionResult IssueDelete()
		{
			return View();
		}


		public ActionResult SlipAssetIssue()
		{
			return View();
		}

		public ActionResult IssueReturn()
		{
			return View();
		}


		public ActionResult PhysicalStockAdjustment()
		{
			return View();
		}

		[Authorize]
		public ActionResult InventorySalesChecked()
		{
			return View();
		}

		[Authorize]
		public ActionResult InventorySalesApproved()
		{
			return View();
		}

		public ActionResult InventorySales()
		{
			return View();
		}
		[Authorize]
		public ActionResult InventoryScrapChecked()
		{
			return View();
		}

		[Authorize]
		public ActionResult InventoryScrapApproved()
		{
			return View();
		}

		public ActionResult InventoryScrap()
		{
			return View();
		}


		public ActionResult InventorySalesReport()
		{
			return View();
		}

		public ActionResult InventoryScrapReport()
		{
			return View();
		}
		public ActionResult MaterialTransfer()
		{
			return View();
		}


		public ActionResult MaterialTransferRpt()
		{
			return View();
		}

		[Authorize]
		public ActionResult InventorySalesRnd()
		{
			return View();
		}
		#endregion Aplos

		#region Operations

		[Authorize, HttpGet]
		public JsonResult GetList(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.Query(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public JsonResult GetDataByInventoryIssue()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryIssueService.GetDataByInventoryIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
			//return Json(_inventoryIssueService.GetDataByInventoryIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public JsonResult GetDataByInventoryReturnIssue()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryIssueService.GetDataByInventoryReturnIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
			//return Json(_inventoryIssueService.GetDataByInventoryIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetIssueDetailByIssueId(string issueId)
		{
			return Json(_inventoryDetailService.GetIssueDetailByIssueId(issueId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		public JsonResult GetStock(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetStock(entity, issueDate), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult GetStockCountryWise(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetStockCountryWise(entity, issueDate), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult GetInvMaterialId(InventoryMaterialViewModel entity)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			var id = _inventoryMaterialService.Query(t => t.MaterialMasterId == entity.MaterialMasterId && t.ArticleId == entity.ArticleId
								&& t.FirstCharacteristicsId == entity.FirstCharacteristicsId && t.FirstCharacteristicsValueId == entity.FirstCharacteristicsValueId
								&& t.SecondCharacteristicsId == entity.SecondCharacteristicsId && t.SecondCharacteristicsValueId == entity.SecondCharacteristicsValueId
								&& t.ThirdCharacteristicsId == entity.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == entity.ThirdCharacteristicsValueId
								&& t.CompanyId == entity.CompanyId && t.PlantId == entity.PlantId).Select(t => t.Id).FirstOrDefault();
			return Json(id, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// use in inventory issue journel
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		[Authorize, HttpGet]
		public JsonResult GetIssueList(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.GetIssueList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		///  use in inventory issue journel
		/// </summary>
		/// <param name="issueId"></param>
		/// <returns></returns>
		[Authorize, HttpGet]
		public JsonResult GetIssueWithGl(string issueId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDetailService.GetIssueWithGl(identity.CompanyId, issueId), JsonRequestBehavior.AllowGet);
		}

		
		[Authorize, HttpGet]
		public JsonResult GetInventoryMaterialIssueList(GridParameter parameters, string issueId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryMaterialService.GetIssueMaterial(parameters, issueId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetBudgetActivityInIssueMaterial(string materialGroupMasterId)
		{
			return Json(_inventoryDetailService.GetBudgetActivityInIssueMaterial(materialGroupMasterId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetCostCenterLoadNewFun(string EntityId)
		{
			return Json(_inventoryDetailService.GetCostCenterLoadNewFun(EntityId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult GetSpecificMaterialStock(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetSpecificMaterialStock(entity, issueDate), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		public JsonResult GetSpecificMaterialStockForAdjustment(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetSpecificMaterialStockForAdjustment(entity, issueDate), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpPost]
		public JsonResult GetApprovedStockDetail(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetApprovedStockDetail(entity, issueDate), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		public JsonResult GetApprovedStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetApprovedStockDetailBeyondIssueDate(entity, issueDate), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		public JsonResult GetUnApprovedStockDetail(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetUnApprovedStockDetail(entity, issueDate), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		public JsonResult GetUnApprovedStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetUnApprovedStockDetailBeyondIssueDate(entity, issueDate), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		public JsonResult GetPostingStockDetail(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetPostingStockDetail(entity, issueDate), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		public JsonResult GetPostingStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetPostingStockDetailBeyondIssueDate(entity, issueDate), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpPost]
		public JsonResult GetRequisitionList(string issueDetailId)
		{

			return Json(_inventoryMaterialService.GetRequisitionList(issueDetailId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		//public JsonResult Create(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll)
		public JsonResult Create(string entities, string specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, string entitiesAll)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
			inventoryIssue.CompanyId = identity.CompanyId;
			inventoryIssue.PlantId = identity.PlantId;
			List<InventoryMaterialViewModel> entitiesVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entities); 
			List<InventoryMaterialViewModel> specificStockListVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(specificStockList); 
			List<InventoryMaterialViewModel> entitiesAllVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entitiesAll); 


			_inventoryIssueService.InsertGraph(entitiesVM, specificStockListVM, inventoryIssue, IssueTypeStatus, entitiesAllVM);
			return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		public JsonResult Delete(string issueDetailId)
		{
			_inventoryIssueService.DeleteIssueDetail(issueDetailId);
			return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public ActionResult IssueReport(string grnId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			_inventoryReveiveService.InventoryIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);
			return null;
		}

        // Job Work Transformation Issue
        [Authorize, HttpGet]
        public ActionResult JobWorkIssueReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryReveiveService.JWIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);
            return null;
        }


        [Authorize, HttpGet]
		public ActionResult AssetIssueReport(string grnId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			_inventoryReveiveService.AssetIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

			return null;
		}
		#endregion Operations

		#region Posted and not posted Issue Delete permanently
		[Authorize, HttpGet]
		public JsonResult GetDeletableIssueList(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.GetDeletableIssueList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult NonPostedIssueDelete(string issueId)
		{
			_inventoryIssueService.NonPostedIssueDelete(issueId);
			return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult PostedIssueDelete(string issueId)
		{
			_inventoryIssueService.PostedIssueDelete(issueId);
			return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
		}
		#endregion


		#region Slip Iissue Popup

		[Authorize, HttpGet]
		public JsonResult GetApprovedIssueSlip()
		{

			return Json(_inventoryIssueService.GetApprovedIssueSlip(), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public JsonResult GetAssetIssueSlip()
		{

			return Json(_inventoryIssueService.GetAssetIssueSlip(), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public JsonResult GetApprovedIssueSlipDetails(string Id, string StorageLocationId,string OrderSpecific)
		{

			return Json(_inventoryIssueService.GetApprovedIssueSlipDetails(Id, StorageLocationId, OrderSpecific), JsonRequestBehavior.AllowGet);
		}

		#endregion


		#region AssetIssue Code

		public ActionResult AssetIssue()
		
		{
			return View("~/Areas/Products/Views/InventoryIssue/AssetIssue.cshtml");
		}

		[Authorize, HttpGet]
		public JsonResult GetAssetInventoryIssue(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.GetAssetInventoryIssue(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetAssetInventoryIssueNew()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
				return Json(obj.GetAssetInventoryIssueNew(identity.PlantId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[Authorize, HttpGet]
		public JsonResult GetGRNFixedAssetList(string materialStorageId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.GetGRNFixedAssetList(identity.PlantId, materialStorageId), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public JsonResult GetAssetIssueSlipWithGRN(string materialStorageId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.GetAssetIssueSlipWithGRN(identity.PlantId, materialStorageId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult InsertAssetIssue(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
			inventoryIssue.CompanyId = identity.CompanyId;
			inventoryIssue.PlantId = identity.PlantId;
			_inventoryIssueService.InsertAssetInventoryIssue(entities, specificStockList, inventoryIssue);
			return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
		}

		#endregion
		#region Issue Return
		[Authorize, HttpGet]
		public JsonResult IssueSlipMaterialAndArticleList(string fromDate, string toDate, string CostCenterId, string MaterialStorageId)
		{
			string paramter = "";
			try
			{
				var sql = @"select distinct CC.Id CostCenterId,CC.UserName AS CostCenterName ,MT.UserName MaterialType
                            ,MGM.UserName AS MaterialGroupMasterName
                            ,IM.MaterialMasterId
                            ,MM.UserName MaterialMasterName
                            , IM.ArticleId
                            , ART.StandardName ArticleName
                            , IM.FirstCharacteristicsId
                            , FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId
                            , ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId
                            , SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId
                            , ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId
                            , TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId
                            , ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
                            ,0 Active,'Slip Article' ArticleType
                            From TRN.InventoryMaterial AS IM
							Left join TRN.InventoryIssueDetail IID ON IID.InventoryMaterialId=IM.Id
                            left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                            LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                            LEFT JOIN [TRN].[InventoryIssue] IRM ON  IRM.Id=IID.InventoryIssueId
                            LEFT join[ORG].[CostCenter] CC On CC.Id=IID.CostCenterId
                            Where CAST(IRM.IssueDate AS DATE) between '" + fromDate + @"' and '" + toDate + "' and CC.Id='" + CostCenterId + "' AND IRM.MaterialStorageId='" + MaterialStorageId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		[Authorize, HttpGet]
		public JsonResult IssueSlipMaterialAndArticleListForIssued(string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId, string MaterialStorageId, string CostCenterId, string fromDate, string toDate)
		{
			string paramter = "";
			if (MaterialMasterId != "")
			{
				if (paramter == "")
					paramter += "IM.MaterialMasterId in(" + MaterialMasterId + ")";
				else
					paramter += " AND IM.MaterialMasterId in(" + MaterialMasterId + ")";
			}
			if (ArticleId != "")
			{
				if (paramter == "")
					paramter += "IM.ArticleId in(" + ArticleId + ")";
				else
					paramter += " AND IM.ArticleId in(" + ArticleId + ")";
			}
			if (FirstCharacteristicsValueId != "")
			{
				if (FirstCharacteristicsValueId == "'','null'")
					FirstCharacteristicsValueId = "'',''";
				if (paramter == "")
					paramter += "isnull(IM.FirstCharacteristicsValueId,'') in(" + FirstCharacteristicsValueId + ")";
				else
					paramter += " AND isnull(IM.FirstCharacteristicsValueId,'') in(" + FirstCharacteristicsValueId + ")";
			}
			if (SecondCharacteristicsValueId != "")
			{
				if (SecondCharacteristicsValueId == "'','null'")
					SecondCharacteristicsValueId = "'',''";
				if (paramter == "")
					paramter += "isnull(IM.SecondCharacteristicsValueId,'') in(" + SecondCharacteristicsValueId + ")";
				else
					paramter += " AND isnull(IM.SecondCharacteristicsValueId,'') in(" + SecondCharacteristicsValueId + ")";
			}
			if (ThirdCharacteristicsValueId != "")
			{
				if (ThirdCharacteristicsValueId == "'','null'")
					ThirdCharacteristicsValueId = "'',''";
				if (paramter == "")
					paramter += "isnull(IM.ThirdCharacteristicsValueId,'') in(" + ThirdCharacteristicsValueId + ")";
				else
					paramter += " AND isnull(IM.ThirdCharacteristicsValueId,'') in(" + ThirdCharacteristicsValueId + ")";
			}
			try
			{
				var sql = "";
				if (string.IsNullOrEmpty(MaterialStorageId))
				{

					sql = @"select cc.Id CostCenterId,cc.UserName CostCenterName, a.InventoryReceiveDetailId,IM.Id InventoryMaterialId,c.Id As IssuedId, REPLACE(CONVERT(CHAR(11), C.IssueDate, 106),' ','-') AS IssueDate,a.IssueRequestDetailId
                            ,IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                    ,IM.FirstCharacteristicsId, CH1.UserName AS Sku1, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicsValue
		                    ,IM.SecondCharacteristicsId, CH2.UserName AS Sku2, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicsValue
		                    ,IM.ThirdCharacteristicsId, CH3.UserName AS Sku3, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicsValue			
		                    ,b.BaseUOMId, UoM.UserName AS TransactionUoM--, b.AvgRate, b.AvgAmount, b.PolicyRate, b.PolicyAmount, b.[Policy]
		                    ,a.qty AS IssuedQty                          
							,FORMAT(a.Rate,'N4') BaseRate
							,a.TotalAmount
                            ,Isnull(a.IssueReturnQty,0)  IssueRerutnQty
                            ,Isnull(a.qty,0)-Isnull(a.IssueReturnQty,0) TransactionQty
                            ,Isnull(a.qty,0)-Isnull(a.IssueReturnQty,0) Balance
                            ,0 Active
		                    --,IRD.TransactionQty Rcvd, IRD.IssueQty IssueQty,PurchaseRerutnQty PurchaseRerutnQty
		                    --, sum(a.qty) qty 
		                    --,sum(IRD.TransactionQty) Rcvd, sum(IRD.IssueQty) IssueQty,sum(PurchaseRerutnQty) PurchaseRerutnQty
                            ,c.MaterialStorageId,MS.UserName MaterialStorage,a.Id InventoryIssueHistoryId
                    from trn.InventoryIssueHistory a
                    left join trn.InventoryIssueDetail b on b.id=a.InventoryIssueDetailId
                    left join trn.InventoryIssue c on c.id=b.InventoryIssueId
                    left join [TRN].[InventoryMaterial] AS IM ON IM.Id=b.InventoryMaterialId
                    left JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IM.ArticleId=AR.Id
                    LEFT JOIN [HKP].[Characteristics] AS CH1 ON IM.FirstCharacteristicsId=CH1.Id
                    LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IM.FirstCharacteristicsValueId=CHV1.Id
                    LEFT JOIN [HKP].[Characteristics] AS CH2 ON IM.SecondCharacteristicsId=CH2.Id
                    LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IM.SecondCharacteristicsValueId=CHV2.Id
                    LEFT JOIN [HKP].[Characteristics] AS CH3 ON IM.ThirdCharacteristicsId=CH3.Id
                    LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IM.ThirdCharacteristicsValueId=CHV3.Id
                    LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=b.CostCenterId
                    left JOIN [SCS].[UnitOfMeasurement] AS UoM ON b.BaseUOMId=UoM.Id
                    left join [HKP].[MaterialStorage] MS on MS.id=c.MaterialStorageId
                    --Left JOin (Select Id, sum(TransactionQty) TransactionQty,sum(IssueQty) IssueQty,sum(PurchaseRerutnQty) PurchaseRerutnQty,sum(IssueRerutnQty) IssueRerutnQty from trn.InventoryReceiveDetail group by Id) IRD ON IRD.id=a.InventoryReceiveDetailId
                    --where a.InventoryReceiveDetailId in('19304-1','19429-2','19633-3','19796-1')
                    --where IM.MaterialMasterId='" + MaterialMasterId + @"' ANd IM.ArticleId='" + ArticleId + @"' AND isnull(FirstCharacteristicsValueId,'')='" + FirstCharacteristicsValueId + @"' and isnull(SecondCharacteristicsValueId,'')='" + SecondCharacteristicsValueId + @"' AND isnull(ThirdCharacteristicsValueId,'')='" + ThirdCharacteristicsValueId + @"'
                    Where " + paramter + @"
                    --and a.InventoryReceiveDetailId='19304-1'
                    AND CC.Id='" + CostCenterId + @"' --AND c.MaterialStorageId='" + MaterialStorageId + @"' AND IssueDate Between '" + fromDate + @"' and '" + toDate + @"'
                    --group by a.InventoryReceiveDetailId
                    --, IM.MaterialMasterId, MM.UserName , IM.ArticleId, AR.StandardName 
                    --, IM.FirstCharacteristicsId, CH1.UserName , IM.FirstCharacteristicsValueId, CHV1.UserName 
                    --, IM.SecondCharacteristicsId, CH2.UserName , IM.SecondCharacteristicsValueId, CHV2.UserName 
                    --, IM.ThirdCharacteristicsId, CH3.UserName , IM.ThirdCharacteristicsValueId, CHV3.UserName 
                    --, b.BaseUOMId, UoM.UserName,cc.UserName --, b.AvgRate, b.AvgAmount, b.PolicyRate, b.PolicyAmount, b.[Policy]
                    Order by IssueDate,Im.ArticleId DESC";
				}
				else
				{

					sql = @"select cc.Id CostCenterId,cc.UserName CostCenterName, a.InventoryReceiveDetailId,IM.Id InventoryMaterialId,c.Id As IssuedId, REPLACE(CONVERT(CHAR(11), C.IssueDate, 106),' ','-') AS IssueDate,a.IssueRequestDetailId
                            ,IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                    ,IM.FirstCharacteristicsId, CH1.UserName AS Sku1, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicsValue
		                    ,IM.SecondCharacteristicsId, CH2.UserName AS Sku2, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicsValue
		                    ,IM.ThirdCharacteristicsId, CH3.UserName AS Sku3, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicsValue			
		                    ,b.BaseUOMId, UoM.UserName AS TransactionUoM--, b.AvgRate, b.AvgAmount, b.PolicyRate, b.PolicyAmount, b.[Policy]
		                    ,a.qty AS IssuedQty
							,FORMAT(a.Rate,'N4') BaseRate
							,a.TotalAmount
                            ,Isnull(a.IssueReturnQty,0)  IssueReturnQty
                            ,Isnull(a.qty,0)-Isnull(a.IssueReturnQty,0) TransactionQty
                            ,Isnull(a.qty,0)-Isnull(a.IssueReturnQty,0) Balance
                            ,0 Active
		                    --,IRD.TransactionQty Rcvd, IRD.IssueQty IssueQty,PurchaseRerutnQty PurchaseRerutnQty
		                    --, sum(a.qty) qty 
		                    --,sum(IRD.TransactionQty) Rcvd, sum(IRD.IssueQty) IssueQty,sum(PurchaseRerutnQty) PurchaseRerutnQty
                            ,c.MaterialStorageId,MS.UserName MaterialStorage,a.Id InventoryIssueHistoryId
                    from trn.InventoryIssueHistory a
                    left join trn.InventoryIssueDetail b on b.id=a.InventoryIssueDetailId
                    left join trn.InventoryIssue c on c.id=b.InventoryIssueId
                    left join [TRN].[InventoryMaterial] AS IM ON IM.Id=b.InventoryMaterialId
                    left JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IM.ArticleId=AR.Id
                    LEFT JOIN [HKP].[Characteristics] AS CH1 ON IM.FirstCharacteristicsId=CH1.Id
                    LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IM.FirstCharacteristicsValueId=CHV1.Id
                    LEFT JOIN [HKP].[Characteristics] AS CH2 ON IM.SecondCharacteristicsId=CH2.Id
                    LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IM.SecondCharacteristicsValueId=CHV2.Id
                    LEFT JOIN [HKP].[Characteristics] AS CH3 ON IM.ThirdCharacteristicsId=CH3.Id
                    LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IM.ThirdCharacteristicsValueId=CHV3.Id
                    LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=b.CostCenterId
                    left JOIN [SCS].[UnitOfMeasurement] AS UoM ON b.BaseUOMId=UoM.Id
                      left join [HKP].[MaterialStorage] MS on MS.id=c.MaterialStorageId
                    --Left JOin (Select Id, sum(TransactionQty) TransactionQty,sum(IssueQty) IssueQty,sum(PurchaseRerutnQty) PurchaseRerutnQty,sum(IssueRerutnQty) IssueRerutnQty from trn.InventoryReceiveDetail group by Id) IRD ON IRD.id=a.InventoryReceiveDetailId
                    --where a.InventoryReceiveDetailId in('19304-1','19429-2','19633-3','19796-1')
                     --where IM.MaterialMasterId='" + MaterialMasterId + @"' ANd IM.ArticleId='" + ArticleId + @"' AND isnull(FirstCharacteristicsValueId,'')='" + FirstCharacteristicsValueId + @"' and isnull(SecondCharacteristicsValueId,'')='" + SecondCharacteristicsValueId + @"' AND isnull(ThirdCharacteristicsValueId,'')='" + ThirdCharacteristicsValueId + @"'
                    Where " + paramter + @"
                    --and a.InventoryReceiveDetailId='19304-1'
                    AND CC.Id='" + CostCenterId + @"' AND c.MaterialStorageId='" + MaterialStorageId + @"' AND IssueDate Between '" + fromDate + @"' and '" + toDate + @"'
                    --group by a.InventoryReceiveDetailId
                    --, IM.MaterialMasterId, MM.UserName , IM.ArticleId, AR.StandardName 
                    --, IM.FirstCharacteristicsId, CH1.UserName , IM.FirstCharacteristicsValueId, CHV1.UserName 
                    --, IM.SecondCharacteristicsId, CH2.UserName , IM.SecondCharacteristicsValueId, CHV2.UserName 
                    --, IM.ThirdCharacteristicsId, CH3.UserName , IM.ThirdCharacteristicsValueId, CHV3.UserName 
                    --, b.BaseUOMId, UoM.UserName,cc.UserName --, b.AvgRate, b.AvgAmount, b.PolicyRate, b.PolicyAmount, b.[Policy]
                    Order by IssueDate,Im.ArticleId DESC";
				}

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		[HttpPost]
		public JsonResult CreateIssueReturn(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssueReturn inventoryIssue, string IssueTypeStatus)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
			inventoryIssue.CompanyId = identity.CompanyId;
			inventoryIssue.PlantId = identity.PlantId;
			_inventoryIssueService.InsertGraphIssueReturn(entities, specificStockList, inventoryIssue, IssueTypeStatus);
			return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult IssueReturnForUpdate(string Id, string toDate, string CostCenterId)
		{
			string paramter = "";

			try
			{
				var sql = @"Select a.Id IssueREturnHistoryId
                              ,a.InventoryIssueReturnId
                            --,InventoryMaterialId
                            --,InventoryReceiveDetailId
                            --,Qty,CostCenterId
                            --,StorageLocationId
                            --,BaseUOMId
                            --,TransactionUoMId 
                            ,a.CostCenterId CostCenterId
                            ,cc.UserName CostCenterName
                            , a.InventoryReceiveDetailId
                            ,IM.Id InventoryMaterialId
                            ,a.Id As IssueReturnId
                            --, REPLACE(CONVERT(CHAR(11), C.IssueDate, 106),' ','-') AS IssueDate
                            ,a.IssueRequestDetailId
                            ,IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
                            ,IM.FirstCharacteristicsId, CH1.UserName AS Sku1, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicsValue
                            ,IM.SecondCharacteristicsId, CH2.UserName AS Sku2, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicsValue
                            ,IM.ThirdCharacteristicsId, CH3.UserName AS Sku3, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicsValue			
                            ,a.BaseUOMId, UoM.UserName AS TransactionUoM
                            ,IIH.qty AS IssuedQty
                            ,Isnull(IIH.IssueReturnQty,0)  IssueReturnQty
                            
                            ,Isnull(a.qty,0) TransactionQty
                            ,Isnull(a.qty,0) oldReturnQty
                            --,(Isnull(IIH.qty ,0)-(Isnull(IIH.IssueReturnQty,0)+Isnull(a.qty,0))) Balance
                            ,(Isnull(IIH.qty ,0)-(Isnull(IIH.IssueReturnQty,0))) Balance
                            ,0 Active
                            ,a.StorageLocationId,MS.UserName MaterialStorage
                            FROM TRN.InventoryIssueReturnHistory a
                            left join [TRN].[InventoryMaterial] AS IM ON IM.Id=a.InventoryMaterialId
                            left JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                            LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IM.ArticleId=AR.Id
                            LEFT JOIN [HKP].[Characteristics] AS CH1 ON IM.FirstCharacteristicsId=CH1.Id
                            LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IM.FirstCharacteristicsValueId=CHV1.Id
                            LEFT JOIN [HKP].[Characteristics] AS CH2 ON IM.SecondCharacteristicsId=CH2.Id
                            LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IM.SecondCharacteristicsValueId=CHV2.Id
                            LEFT JOIN [HKP].[Characteristics] AS CH3 ON IM.ThirdCharacteristicsId=CH3.Id
                            LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IM.ThirdCharacteristicsValueId=CHV3.Id
                            LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=a.CostCenterId
                            left JOIN [SCS].[UnitOfMeasurement] AS UoM ON a.BaseUOMId=UoM.Id
                            left join [HKP].[MaterialStorage] MS on MS.id=a.StorageLocationId
                            LEFT JOIN trn.InventoryIssueReturn IIR ON IIR.Id=a.InventoryIssueReturnId
                            LEFT join(select sum(qty) qty,sum(IssueReturnQty) IssueReturnQty,InventoryReceiveDetailId from trn.InventoryIssueHistory group by InventoryReceiveDetailId) IIH On IIH.InventoryReceiveDetailId=a.InventoryReceiveDetailId
                            
                            Where IIR.Id='" + Id + @"'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		#endregion

		#region inventory issue return report

		[Authorize, HttpGet]
		public ActionResult InventoryIssueReturnReport(string grnId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			_inventoryReveiveService.InventoryIssueReturnReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

			return null;
		}

		#endregion

		#region Physical-Stock-Adjustment Code Start here 

		[Authorize, HttpGet]
		public JsonResult GetDataByPhysicalStockAdjustment()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryIssueService.GetDataByPhysicalStockAdjustment(identity.PlantId), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
			//return Json(_inventoryIssueService.GetDataByInventoryIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public ActionResult PhysicalStockAdjustmentReport(string grnId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			_inventoryReveiveService.PhysicalStockAdjustmentReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

			return null;
		}

		[Authorize, HttpPost]
		public JsonResult GetStockForPhysicalStock(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetStockForPhysicalStock(entity, issueDate), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult GetSpecificMaterialStockForPhysicalStock(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetSpecificMaterialStockForPhysicalStock(entity, issueDate), JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult InsertPhysicalStockAdjustment(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, PhysicalStockAdjustmentMaster inventoryIssue, string IssueTypeStatus)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
			inventoryIssue.CompanyId = identity.CompanyId;
			inventoryIssue.PlantId = identity.PlantId;
			_inventoryIssueService.InsertPhysicalStockAdjustment(entities, specificStockList, inventoryIssue, IssueTypeStatus);
			return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult MaterialAdjustmentDetailsData(string inveReveiveId, string POID)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryIssueService.MaterialAdjustmentDetailsData(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;

		}
		[Authorize, HttpGet]
		public JsonResult GetAdjustmentDetailByIssueId(string issueId)
		{
			return Json(_inventoryDetailService.GetAdjustmentDetailByIssueId(issueId), JsonRequestBehavior.AllowGet);
		}

		#endregion

		#region Inventory Sales--------------------------
		[Authorize, HttpGet]
		public JsonResult GetDataByInventorySales(string tabType)
		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			//var jsondata = Json(_inventoryIssueService.GetDataByInventorySales(identity.PlantId, tabType), JsonRequestBehavior.AllowGet);
			//jsondata.MaxJsonLength = int.MaxValue;
			//return jsondata;
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (tabType == "1")
				{
					sql = @"SELECT * FROM(SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	                        ,ROUND(sum(ISH.SalesRate), 2) SalesRate
	                        ,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo							
			                ,PPI.Id InvoicingPartyPlantId    ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',p.UserName PartyName,P.Id PartyId
                            ,EI2.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName,II.ApprovedBy,II.PaymentTermId,FORMAT(II.BaseOnDueDate, 'dd-MMM-yyyy') BaseOnDueDate,II.BaseNoOfDays,FORMAT(II.MatureDate, 'dd-MMM-yyyy') MatureDatec
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id

			                LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
							Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
							Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy

							WHERE II.PlantId= '" + identity.PlantId + @"' 
			                         AND II.CheckedByStatus='For Checking'
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				            ,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) ,p.UserName ,P.Id 
                            ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName ,II.ApprovedBy,PPI.Id,II.PaymentTermId,II.BaseOnDueDate,II.BaseNoOfDays,II.MatureDate

			                UNION ALL
			                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	                        ,ROUND(sum(ISH.SalesRate), 2) SalesRate
	                        ,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							
			                    ,PPI.Id InvoicingPartyPlantId    ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',p.UserName PartyName,P.Id PartyId
                            ,EI2.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName,II.ApprovedBy,II.PaymentTermId,FORMAT(II.BaseOnDueDate, 'dd-MMM-yyyy') BaseOnDueDate,II.BaseNoOfDays,FORMAT(II.MatureDate, 'dd-MMM-yyyy') MatureDate
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id

			                         LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
							Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
							Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
			                         AND II.CheckedByStatus IS NULL
			                         AND II.ApprovedByStatus IS NULL
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) ,p.UserName ,P.Id
                             ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName ,II.ApprovedBy,PPI.Id,II.PaymentTermId,II.BaseOnDueDate,II.BaseNoOfDays,II.MatureDate
							UNION ALL
			                        SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	                        ,ROUND(sum(ISH.SalesRate), 2) SalesRate
	                        ,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							
			                    ,PPI.Id InvoicingPartyPlantId    ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',p.UserName PartyName,P.Id PartyId
                            ,EI2.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName,II.ApprovedBy,II.PaymentTermId,FORMAT(II.BaseOnDueDate, 'dd-MMM-yyyy') BaseOnDueDate,II.BaseNoOfDays,FORMAT(II.MatureDate, 'dd-MMM-yyyy') MatureDate
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id

			                LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
							Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
							Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy

							WHERE II.PlantId= '" + identity.PlantId + @"' 
			                         AND II.CheckedByStatus IS NULL
			                         AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)),p.UserName ,P.Id 
                            ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName ,II.ApprovedBy,PPI.Id,II.PaymentTermId,II.BaseOnDueDate,II.BaseNoOfDays,II.MatureDate
							)x  
							Order BY IssueDate DESC";
				}
				else if (tabType == "2")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	,ROUND(sum(ISH.SalesRate), 2) SalesRate
	,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo							
				            ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate , CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
							,II.CurrencyId
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id

				            LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND (II.CheckedByStatus='Hold' OR II.CheckedByStatus='Reject')                           
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName,II.ToCurrencyRate 
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName  ,PPI.UserName 
							,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,II.CurrencyId
							Order BY II.SalesDate DESC";
				}
				else if (tabType == "3")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	,ROUND(sum(ISH.SalesRate), 2) SalesRate
	,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							
				                       ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate , CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',II.CurrencyId
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id

				            LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='For Approval'    
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName,II.ToCurrencyRate 
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName  ,PPI.UserName 
							,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,II.CurrencyId
							Order BY II.SalesDate DESC";
				}
				else if (tabType == "4")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	,ROUND(sum(ISH.SalesRate), 2) SalesRate
	,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							
				                       ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate,  CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
							,II.CurrencyId
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id

				                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
							WHERE II.PlantId= '" + identity.PlantId + @"'
							AND II.CheckedByStatus='Checked' AND (II.ApprovedByStatus='Hold' OR II.ApprovedByStatus='Reject')
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName,II.ToCurrencyRate 
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName  ,PPI.UserName 
							,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,II.CurrencyId
							Order BY II.SalesDate DESC";
				}
				else if (tabType == "5")
				{
					sql = @"select * from(SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	,ROUND(sum(ISH.SalesRate), 2) SalesRate
	,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							
				            ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',II.CurrencyId
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id

				                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
							WHERE II.PlantId=  '" + identity.PlantId + @"'
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName,II.ToCurrencyRate 
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName  ,PPI.UserName 
							,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,II.CurrencyId
                            UNION ALL
                            SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	,ROUND(sum(ISH.SalesRate), 2) SalesRate
	,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							
				            ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',II.CurrencyId
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id


				             LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
							WHERE II.PlantId=  '" + identity.PlantId + @"'
							AND II.CheckedByStatus IS NULL AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName,II.ToCurrencyRate 
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName  ,PPI.UserName 
							,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,II.CurrencyId
                             UNION ALL
                            SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	,ROUND(sum(ISH.SalesRate), 2) SalesRate
	,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							
				            ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',II.CurrencyId
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id


				            LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
							WHERE II.PlantId=  '" + identity.PlantId + @"'
							AND II.CheckedByStatus IS NULL AND II.ApprovedByStatus IS NULL
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName,II.ToCurrencyRate 
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName  ,PPI.UserName 
							,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,II.CurrencyId)x
							
							Order BY IssueDate DESC";
				}
				if (tabType == "6")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,ROUND(sum(ISH.Qty), 2) Qty	
	,ROUND(sum(ISH.SalesRate), 2) SalesRate
	,Sum(ISH.TotalAmount) TotalAmount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							
				            ,PPI.UserName BillTo
							,PPI1.UserName ShipTo,II.ToCurrencyRate, II.DocRefNo, II.DocDate , CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
							,II.CurrencyId
							FROM[TRN].[InventorySales] AS II
							left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IID.Id

				                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
							LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
							WHERE II.PlantId=  '" + identity.PlantId + @"'
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') ='Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.SalesDate, MS.UserName,II.ToCurrencyRate 
							,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
							,PPI.UserName ,PPI1.UserName  ,PPI.UserName 
							,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,II.CurrencyId
							Order BY II.SalesDate DESC";
				}
				//return _sqlRepository.GetDataCollection(sql);


				//return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

				var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
				jsondata.MaxJsonLength = int.MaxValue;
				return jsondata;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}

		}

		[Authorize, HttpGet]
		public JsonResult MaterialSalesDetails(string inveReveiveId, string POID)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryIssueService.MaterialSalesDetails(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
		}
		[Authorize, HttpGet]
		public JsonResult LoadCustomer(string Id, string toDate, string CostCenterId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"SELECT P.Id AS PartyId, P.Id, P.Code, P.UserName, P.PartyType, NULL AS PartyAccountGroupId, NULL AS PartyAccountGroupCode
                                    , NULL AS PartyAccountGroupName, NULL AS CurrencyId, NULL AS CurrencyCode, NULL AS CurrencyName
                                    , NULL AS PaymentTermId, NULL AS PaymentTermCode, NULL AS PaymentTermName, 0 AS IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, NULL AS GSTIN
                                    FROM [HKP].[Party] AS P
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + identity.CompanyGroupId + "'";// AND P.PartyType='Customer'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		[HttpGet, Authorize]
		public JsonResult NotificationSetting()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			try
			{

				var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='MaterialInventorySales' and plantId='" + identity.PlantId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}



		[Authorize, HttpGet]
		public JsonResult GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.GetCheckedByAndApprovedBY(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult InventorySalesCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventorySales inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<InventorySalesTax> taxCategoryList, string productNewId,decimal ToCurrencyRate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
			inventoryIssue.CompanyId = identity.CompanyId;
			inventoryIssue.PlantId = identity.PlantId;
			_inventoryIssueService.InsertGraphInventorySales(entities, specificStockList, inventoryIssue, IssueTypeStatus, CheckedByStatusForNoti, ApprovedByStatusForNoti, taxCategoryList, productNewId,ToCurrencyRate);
			return Json(new { inventoryIssue, Message = AplosMessage.Success + "Sales No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult GetStockSales(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetStockSales(entity, issueDate), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public ActionResult InventorySalesReportPrint(string grnId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			_inventoryReveiveService.InventorySalesReportPrint(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

			return null;
		}

		
		[Authorize, HttpGet]
		public JsonResult GetInventoryMaterialReceivableList(GridParameter parameters, string inveReveiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(GetReceivableMaterial(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		private GridModel GetReceivableMaterial(GridParameter parameters, string inveReveiveId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"',@companyId varchar(10)='" + identity.CompanyId + @"',@plantId varchar(10)='" + identity.PlantId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SELECT IM.Id, ISD.Id AS InventoryReceiveDetailId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                            , ISD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                            , ISH.SalesRate AS TransactionRate
                            , CU.Code AS CurrencyName, IVS.ToCurrencyRate
                            , ISH.Amount
                            ,ISH.Qty                         
							                  
					        ,ISD.TransactionUoMId
							,ISD.BaseUOMId 
                            ,MM.IsAsset  
							,HSNC.Code HSNCode
					  from TRN.InventoryMaterial AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						LEFT JOIN [TRN].[InventorySalesDetail] ISD ON ISD.InventoryMaterialId=IM.Id AND ISD.InventorySalesId=@inventoryReceiveId
                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN TRN.InventorySales IVS ON IVS.Id=ISD.InventorySalesId
                        JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                        LEFT JOIN HKP.HSNCode AS HSNC ON HSNC.Id=MM.HSNCodeId
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(SDH.Qty) Qty,sum(SDH.Qty*SD.SalesRate) Amount,SD.SalesRate SalesRate
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId,SD.SalesRate
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
                        WHERE ISD.InventorySalesId=@inventoryReceiveId";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		
		
		[Authorize, HttpGet]
		private IEnumerable<object> GetInventortGLBudgetActivityData(string companyId, string plantId, string inveReveiveId, string partyId)
		{
			try
			{
				var companyParty = _companyPartyRepository.Query(r => r.PartyId == partyId && r.PlantId == plantId).Select().FirstOrDefault();
				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty.PartyAccountGroupId + @"',@countryId varchar(10)

						SELECT  'CostOfGoodsSold' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId
							,GLGeneralInfoId =MGGL.InventoryGLId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =MGGL.InventoryBudgetMasterId 
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = MGGL.InventoryActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							
							, SUM(ISH.Qty*IRD.BooksCurrencyBaseRate) AS Dr
							, NULL Cr
							, SUM(ISH.Qty*IRD.BooksCurrencyBaseRate) AS Amount
                            --,IRD.Id AS  InventoryReceiveDetailId
						FROM  [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IVS ON ISD.InventorySalesId=IVS.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    UNION
						SELECT  'Inventory' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId

							,GLGeneralInfoId =IRD.PostDrGLGeneralInfoId 
							,GLGeneralInfoCode =GL.AccountCode
							,GLGeneralInfoName =GL.UserName 
							,BudgetMasterId =IRD.PostDrBudgetMasterId 
							,BudgetCode =B.Code 
							,BudgetName =B.UserName
							,ActivityId =IRD.PostDrActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName 
							, NULL Dr
							, SUM(ISH.Qty*IRD.BooksCurrencyBaseRate) AS Cr
							, SUM(ISH.Qty*IRD.BooksCurrencyBaseRate) AS Amount
                            --,IRD.Id AS  InventoryReceiveDetailId
						FROM  [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IVS ON ISD.InventorySalesId=IVS.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		[Authorize, HttpGet]
		public JsonResult GetInventortGLBudgetActivity(string inventorysalesId, string customerId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			return Json(GetInventortGLBudgetActivityData(identity.CompanyId, identity.PlantId, inventorysalesId, customerId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult ReportSalesPosting(ReportFormat reportFormat, string voucherId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService, _plantService);
			var workbook = _accountsSalesReportService.GetSalesPostingReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
			switch (reportFormat)
			{
				case ReportFormat.Pdf:
					return RenderReportAsPdf(workbook, reportFileName);

				case ReportFormat.Excel:
					return RenderReportAsExcel(workbook, reportFileName);

				default:
					return RenderReportAsExcel(workbook, reportFileName);
			}
		}


		[HttpGet, Authorize]
		public ActionResult ReportInventorySalesPosting(ReportFormat reportFormat, string voucherId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			AccountsSalesReportService _accountsSalesReportService = new AccountsSalesReportService(_sqlRepository, _companyParallelCurrencyService, _plantService);
			var workbook = _accountsSalesReportService.GetInventorySalesPostingReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
			switch (reportFormat)
			{
				case ReportFormat.Pdf:
					return RenderReportAsPdf(workbook, reportFileName);

				case ReportFormat.Excel:
					return RenderReportAsExcel(workbook, reportFileName);

				default:
					return RenderReportAsExcel(workbook, reportFileName);
			}
		}



		[Authorize, HttpGet]
		public ActionResult InventorySalesReportExcel(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory,string Summery,bool WithTax)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			plantId = identity.PlantId;
			var reportFileName = "Sales Register.xls" + fromDate + "To" + toDate + "";
			ExcelEngine excelEngine = new ExcelEngine();

			IWorkbook workbook = InventorySalesReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, Qty, Amount, Summery,WithTax);
			switch (reportFormat)
			{
				case ReportFormat.Pdf:
					return RenderReportAsPdf(workbook, reportFileName);

				case ReportFormat.Excel:
					return RenderReportAsExcel(workbook, reportFileName);

				default:
					return View();
			}
		}

		[HttpPost, Authorize]
//		public DataTable GetInventorySalesReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate, string Qty, string Amount, string Summery)
//		{
//			var sql = "";
//			try
//			{
//				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//				if (Summery == "Details")
//				{
//					if (fromDate != "" && toDate != "")
//					{
//						sql = @"SELECT 
//						ROW_NUMBER() Over(Order by   SM.Id) As[S.N]
//						,SA.SourceType
//						,SM.Id
//						,SM.SalesId
//						,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
//						,SM.SalesOrderId
//						,MO.Id MasterOrderId
//						,SO.Id SONo
//						,po.PONumber
//						,PPI.UserName AS BillTo
//						,AM.Address1 as  BillToAddress
//						,ST.UserName as  BillToState
//						,PPI.GSTIN as BillToGSTNo
//						,PPD.UserName AS ShipTo
//						,AMD.Address1 as ShipToAddress
//						,STD.UserName as ShipToState
//                        ,PPD.GSTIN as ShipToGSTNo
//						, SA.ToCurrencyRate
//						, SA.DocRefNo
//						,'' DocDate
//						, P.UserName AS PartyName,p.Code
//						,MGM.UserName AS MaterialGroupMasterName
//						,MM.UserName MaterialMasterName
//						,ART.StandardName AS MaterialMasterArticleName
//						,FCV.UserName FirstCharacteristicsValue
//						,SCV.UserName SecondCharacteristicsValue
//						,TCV.UserName ThirdCharacteristicsValue
//						,ISNULL( TAxInfo.HSCode,'')  HSNCode
//						,SM.BaseRate
//						,SM.BaseUoMFactor
//						,SM.TransactionRate
//						,SM.TransactionQty
//						,SM.TransactionAmount
//						,SM.TaxAmount
//						,SM.NetAmount
//						,v.VoucherNo VoucherDetailId
//						,BUoM.UserName AS BaseUoM
//						,TUoM.UserName AS TransactionUoM
//						,CU.Code AS Currency
//						,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
//						,DT.UserName DestinationName
//						,SO.SOType
//						,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
//						,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
//						,'' Entity
//						,'' CheckedByName
//						,'' CheckedBy
//						,'' ApprovedByName
//						,'' ApprovedBy
//						,Posted=CASE WHEN SA.VoucherId IS NULL THEN 'No' ELSE 'YES'  END
//						,'' 'NoteForAccounts'
//						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
//						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
//						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
//						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
//						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

//                        ,PSI.CNFContainerNo ContainerNo,PSI.TransportDriverName as TransporterName,PSI.TransportDocRefNo 
//						,FORMAT( PSI.TransportDocDate, 'dd-MMM-yyyy')TransportDocDate,Agent.UserName as AgentName
//						,''AgentCommission
//						,'' Insurance
//,PSI.CargoGrossWt GrossWeight,''LoTNo
//						FROM TRN.SalesMaterial AS SM 
//						LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
//						LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
//						LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
//						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
//						LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
//						LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId
//						LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
//						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
//						LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
//						LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId AND SM.SalesOrderId=FC.SalesOrderId
//						LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId
//						LEFT JOIN [HKP].[Characteristics] AS CH ON FC.CharacteristicsId=CH.Id
//						LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId AND SM.SalesOrderId=SC.SalesOrderId
//						LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
//						LEFT JOIN [HKP].[Characteristics] AS CH2 ON SC.CharacteristicsId=CH2.Id
//						LEFT JOIN TRN.ThirdCharacteristics AS TC ON TC.Id=SM.ThirdCharacteristicsId AND SM.SalesOrderId=TC.SalesOrderId
//						LEFT JOIN HKP.CharacteristicsValue AS TCV ON TCV.Id=SM.ThirdCharacteristicsValueId
//						LEFT JOIN [HKP].[Characteristics] AS CH3 ON TC.CharacteristicsId=CH3.Id
//						LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
//						LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
//						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
//						LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
//						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
//						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
//						LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
//						LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
//						LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
//						LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
//						LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
//						LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST' and A.SalesServiceId IS NULL
//									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
//								   ) TAxInfo	ON TAxInfo.SalesMaterialId=SM.Id 
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST' and A.SalesServiceId IS NULL	
//									) TAxInfo1	ON TAxInfo1.SalesMaterialId=SM.Id 
							  		 
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST' and A.SalesServiceId IS NULL	
//									) TAxInfo2	ON TAxInfo2.SalesMaterialId=SM.Id 

//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TDS' and A.SalesServiceId IS NULL									
//									) TAxInfo3	ON TAxInfo3.SalesMaterialId=SM.Id 


							
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='VAT' and A.SalesServiceId IS NULL		
//						) TAxInfo4 ON TAxInfo4.SalesMaterialId=SM.Id 

//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='AIT' and A.SalesServiceId IS NULL		
//						) TAxInfo5 ON TAxInfo5.SalesMaterialId=SM.Id 
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TCS' and A.SalesServiceId IS NULL		 
								
//						) TAxInfo6 ON TAxInfo6.SalesMaterialId=SM.Id 
//						LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
//LEFT JOIN PostSalesInvoice PSI On PSI.SalesId=SA.Id
//						LEFT JOIN HKP.Party as Agent on Agent.Id=PSI.TransportAgentId

//						WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
//						UNION ALL
						
//						Select                  
//						ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
//						,IR.SourceType
//						,ISs.Id
//						,IR.Id SalesId
//						,FORMAT(IR.EntryDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
//						,'' SalesOrderId
//						,'' MasterOrderId
//						,'' SONo
//						,'' PONumber
//						,'' AS BillTo
//						,'' as BillToAddress
//						,'' as BillToState
//						,'' as BillToGSTNo
//						,'' AS ShipTo
//						,'' AS ShipToAddress
//						,'' AS ShipToState
//						,'' as ShipToGSTNo
//						, 0 ToCurrencyRate
//						, '' DocRefNo
//						,'' DocDate
//						, P.UserName AS PartyName,p.Code
//						,'' AS MaterialGroupMasterName
//						,SM.UserName MaterialMasterName
//						,'' AS MaterialMasterArticleName
//						,''FirstCharacteristicsValue
//						,'' SecondCharacteristicsValue
//						,'' ThirdCharacteristicsValue
//						, '' HSNCode
//						,0 BaseRate
//						,0 BaseUoMFactor
//						,0 TransactionRate
//						,0 TransactionQty
//						,ISs.Amount TransactionAmount
//						,ISs.TaxAmount
//						,0 NetAmount
//						,'' VoucherDetailId
//						,''  BaseUoM
//						,''  TransactionUoM
//						,''  Currency
//						,'' DeliveryDate
//						,'' DestinationName
//						,'' SOType
//						,0 ServiceCharge
//						, 0 ServiceTax
//						,'' Entity
//						,'' CheckedByName
//						,'' CheckedBy
//						,'' ApprovedByName
//						,'' ApprovedBy
//						,'' Posted
//						,'' 'NoteForAccounts'

//						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
//						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
//						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
//						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
//						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
//,''ContainerNo ,''TransporterName,''TransportDocRefNo 
//						,''TransportDocDate,''AgentName
//						,''AgentCommission
//						,'' Insurance
//,''GrossWeight,''LoTNo
//						from trn.SalesService AS ISs
//						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
//						left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
//						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
//						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
//						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
//						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
//						left join trn.Voucher V on V.Id=I.VoucherId
//						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
//						left join trn.Voucher V1 on V1.Id=ep.VoucherId
//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST'  
									
//									) TAxInfo	ON TAxInfo.SalesServiceId=ISs.Id AND TAxInfo.SalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST'  
//									) TAxInfo1	ON TAxInfo1.SalesServiceId=ISs.Id AND TAxInfo1.SalesServiceId IS NOT NULL 
							  		 
//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST'  

//									) TAxInfo2	ON TAxInfo2.SalesServiceId=ISs.Id AND TAxInfo2.SalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TDS'  
//									) TAxInfo3	ON TAxInfo3.SalesServiceId=ISs.Id AND TAxInfo3.SalesServiceId IS NOT NULL


							
//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='VAT'  
								
//						) TAxInfo4 ON TAxInfo4.SalesServiceId=ISs.Id AND TAxInfo4.SalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='AIT'  
//						) TAxInfo5 ON TAxInfo5.SalesServiceId=ISs.Id AND TAxInfo5.SalesServiceId IS NOT NULL
//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TCS'
//						) TAxInfo6 ON TAxInfo6.SalesServiceId=ISs.Id AND TAxInfo6.SalesServiceId IS NOT NULL

//						WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.InvoiceDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
//						UNION ALL

//						SELECT 

//						ROW_NUMBER() Over(Order by   II.Id) As[S.N]
//						,'InventorySales' SourceType
//						,IID.Id
//						,II.Id SalesId
//						,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
//						,'' SalesOrderId
//						,'' MasterOrderId
//						,'' SONo
//						,'' PONumber
//						,PPI.UserName AS BillTo
//						,AM.Address1 as BillToAddress
//						,ST.UserName as BillToState				
//						,PPI.GSTIN as BillToGSTNo
//						,PPI1.UserName ShipTo
//						,AM1.Address1 ShipToAddress
//						,ST1.UserName ShipToState
//						,PPI1.GSTIN ShipToGSTNo
//						,II.ToCurrencyRate
//						, II.DocRefNo
//						,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocDate
//						, P.UserName AS PartyName,p.Code
//						,MGM.UserName AS MaterialGroupMasterName
//						,MM.UserName MaterialMasterName
//						,ART.StandardName AS MaterialMasterArticleName
//						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
//						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
//						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
//						, ISNULL(TAxInfo.HSCode,'') HSNCode

//						,Sum(IID.PolicyRate) BaseRate
//						,0 BaseUoMFactor
//						,sum(IID.PolicyRate) TransactionRate
//						,Sum(IID.Qty) TransactionQty
//						,Sum(IID.Qty *IID.PolicyRate) TransactionAmount
//						,sum(SCr1.TaxAmount) TaxAmount
//						,0 NetAmount
//						,II.VoucherId VoucherDetailId
//						,TUoM.UserName AS BaseUoM
//						,TUoM.UserName AS TransactionUoM
//						,'' AS Currency
//						,'' DeliveryDate
//						,'' DestinationName
//						,'' SOType
//						,sum(SCr.Amount) ServiceCharge
//						,sum(SCr.TotalTaxAmount) ServiceTax

//						,E.UserName AS Entity 
//						,EI2.EmployeeName CheckedByName
//						,II.CheckedBy
//						,EI1.EmployeeName ApprovedByName
//						,II.ApprovedBy
//						,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
//						,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'

//						,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST,sum(TAxInfo.Percentage) CGSTTaxPercentage--MaterialTaxPer						
//						,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST,sum(TAxInfo2.Percentage) SGSTTaxPercentage
//						,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST,sum(TAxInfo1.Percentage) IGSTTaxPercentage
//						,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS,sum(TAxInfo3.Percentage) TDSTaxPercentage
//						,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS,sum(TAxInfo6.Percentage) TCSTaxPercentage
//,''ContainerNo ,''TransporterName,''TransportDocRefNo 
//						,''TransportDocDate,''AgentName
//						,''AgentCommission
//						,'' Insurance
//,''GrossWeight,''LoTNo
//						FROM[TRN].[InventorySales] AS II
//						left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,IsAsset,BaseUOMId from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
//						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
//						left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
//						left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
//						Left JOIN [ORG].[Entity] E On E.id= II.EntityId

//						--left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
//						--left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
//						--left JOIN SCS.Country c ON C.Id=IR.CountryId

//						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
//						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
//						LEFT JOIN [SCS].[State] as ST on ST.Id=AM.StateId

//						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
//						LEFT JOIN [MST].[AddressMaster] AS AM1 ON AM1.Id=PPI1.AddressMasterId
//						LEFT JOIN [SCS].[State] as ST1 on ST1.Id=AM1.StateId
//						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
//						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
//						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
//						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
//						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
//						--Left Join [HKP].[Party] Par As Par.Id=II.P
//						LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
//						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
//						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
//						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
//						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id			
//						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
//						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
//						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
//						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
//						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
//						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
//						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
//						LEFT JOIN(Select sum(Amount) Amount, sum(TotalTaxAmount) TotalTaxAmount, InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
//						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id
//			LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST' and A.InventorySalesServiceId IS NULL								
//								   ) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST' and A.InventorySalesServiceId IS NULL									
//									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 
							  		 
//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST' and A.InventorySalesServiceId IS NULL 									
//									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									WHERE B.Code='TDS' and A.InventorySalesServiceId IS NULL 					
//									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 							
//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='VAT' and A.InventorySalesServiceId IS NULL 
								
//						) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.InventorySalesId 

//						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='AIT' and A.InventorySalesServiceId IS NULL 
							
//						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IID.InventorySalesId 
//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM 
//									[TRN].InventorySalesAdditionalTax A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='TCS' 								
//						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
						
//						WHERE II.PlantId='" + identity.PlantId + "' AND convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
//						GROUP BY p.Code	,II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
//						,II.SalesDate, MS.UserName
//						,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo 
//						,PPI.UserName , AM.Address1,ST.UserName,PPI.GSTIN, PPI1.UserName,AM1.Address1,ST1.UserName,PPI1.GSTIN,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
//						, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) ,p.UserName ,P.Id 
//						,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName ,II.ApprovedBy,TAxInfo.HSCode
//						,MT.UserName ,MGM.UserName,IM.MaterialMasterId,MM.UserName, ART.StandardName 
//						, ISNULL(FCV.UserName,''), ISNULL(SCV.UserName,''), ISNULL(TCV.UserName,''),II.[Status]
//						,Pnt.UserName,HSNC.Code ,Com.UserName,TUoM.UserName	,ComG.UserName,II.VoucherId,IID.Id

//						UNION ALL
//						Select                  
//						ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
//						,'InventorySales' SourceType
//						,SM.Id
//						,IR.Id SalesId
//						,FORMAT(IR.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
//						,'' SalesOrderId
//						,'' MasterOrderId
//						,'' SONo
//						,'' PONumber
//						,'' AS BillTo
//						,'' AS BillToAddress
//						,'' AS BillToState
//						,'' as BillToGSTNo
//						,'' AS ShipTo
//						,'' AS ShipToAddress
//						,'' AS ShipToState	
//						,'' as ShipToGSTNo
//						, 0 ToCurrencyRate
//						, '' DocRefNo
//						,'' DocDate
//						, P.UserName AS PartyName,p.Code
//						,'' AS MaterialGroupMasterName
//						,SM.UserName MaterialMasterName
//						,'' AS MaterialMasterArticleName
//						,''FirstCharacteristicsValue
//						,'' SecondCharacteristicsValue
//						,'' ThirdCharacteristicsValue
//						, '' HSNCode

//						,0 BaseRate
//						,0 BaseUoMFactor
//						,0 TransactionRate
//						,0 TransactionQty
//						,ISs.Amount TransactionAmount
//						,ISs.Amount TaxAmount
//						,0 NetAmount
//						,'' VoucherDetailId
//						,'' AS BaseUoM
//						,'' AS TransactionUoM
//						,'' AS Currency
//						,'' DeliveryDate
//						,'' DestinationName
//						,'' SOType
//						,0 ServiceCharge
//						,0 ServiceTax
//						,'' Entity
//						,'' CheckedByName
//						,'' CheckedBy
//						,'' ApprovedByName
//						,'' ApprovedBy
//						,'' Posted
//						,'' 'NoteForAccounts'

//						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
//						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
//						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
//						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
//						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
//,''ContainerNo ,''TransporterName,''TransportDocRefNo 
//						,''TransportDocDate,''AgentName
//						,''AgentCommission
//						,'' Insurance
//,''GrossWeight,''LoTNo
//						from trn.InventoryService AS ISS
//						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
//						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventoryReceiveId
//						LEFT JOIN HKP.Party AS P ON P.Id=IR.CustomerId
//						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
//						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
//						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
//						left join trn.Voucher V on V.Id=I.VoucherId
//						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
//						left join trn.Voucher V1 on V1.Id=ep.VoucherId
//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST'  
//									--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
//									) TAxInfo	ON TAxInfo.InventorySalesServiceId=ISs.Id AND TAxInfo.InventorySalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST'  

//									) TAxInfo1	ON TAxInfo1.InventorySalesServiceId=ISs.Id AND TAxInfo1.InventorySalesServiceId IS NOT NULL 
							  		 
//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST'  

//									) TAxInfo2	ON TAxInfo2.InventorySalesServiceId=ISs.Id AND TAxInfo2.InventorySalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TDS' 
//									) TAxInfo3	ON TAxInfo3.InventorySalesServiceId=ISs.Id AND TAxInfo3.InventorySalesServiceId IS NOT NULL
							
//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='VAT' 
								
//						) TAxInfo4 ON TAxInfo4.InventorySalesServiceId=ISs.Id AND TAxInfo4.InventorySalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='AIT' 
//						) TAxInfo5 ON TAxInfo5.InventorySalesServiceId=ISs.Id AND TAxInfo5.InventorySalesServiceId IS NOT NULL
//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TCS' 
//						) TAxInfo6 ON TAxInfo6.InventorySalesServiceId=ISs.Id AND TAxInfo6.InventorySalesServiceId IS NOT NULL

//						WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.SalesDate) BETWEEN  '" + fromDate + "' AND '" + toDate + @"'";
//						return _sqlRepository.GetDataTable(sql);
//					}
//					else
//					{
//						sql = @"	SELECT 
//						ROW_NUMBER() Over(Order by   SM.Id) As[S.N]
//						,SA.SourceType
//						,SM.Id
//						,SM.SalesId
//						,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
//						,SM.SalesOrderId
//						,MO.Id MasterOrderId
//						,SO.Id SONo
//						,po.PONumber
//						,PPI.UserName AS BillTo
//						,AM.Address1 as BillToAddress
//						,ST.UserName as BillToState
//						,PPI.GSTIN as BillToGSTNo
//						,PPD.UserName AS ShipTo
//						,AMD.Address1 as ShipToAddress
//						,STD.UserName as ShipToState
//						,PPD.GSTIN as ShipToGSTNo					
//						, SA.ToCurrencyRate
//						, SA.DocRefNo
//						,'' DocDate
//						, P.UserName AS PartyName
//						,MGM.UserName AS MaterialGroupMasterName
//						,MM.UserName MaterialMasterName
//						,ART.StandardName AS MaterialMasterArticleName
//						,FCV.UserName FirstCharacteristicsValue
//						,SCV.UserName SecondCharacteristicsValue
//						,TCV.UserName ThirdCharacteristicsValue
//						, TAxInfo.HSCode HSNCode

//						,SM.BaseRate
//						,SM.BaseUoMFactor
//						,SM.TransactionRate
//						,SM.TransactionQty
//						,SM.TransactionAmount
//						,SM.TaxAmount
//						,SM.NetAmount
//						,v.VoucherNo VoucherDetailId
//						,BUoM.UserName AS BaseUoM
//						,TUoM.UserName AS TransactionUoM
//						,CU.Code AS Currency
//						,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
//						,DT.UserName DestinationName
//						,SO.SOType
//						,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
//						,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
//						,'' Entity
//						,'' CheckedByName
//						,'' CheckedBy
//						,'' ApprovedByName
//						,'' ApprovedBy
//						,Posted=CASE WHEN SA.VoucherId IS NULL THEN 'No' ELSE 'YES'  END
//						,'' 'NoteForAccounts'
//						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
//						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
//						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
//						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
//						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
//,PSI.CNFContainerNo ContainerNo,PSI.TransportDriverName as TransporterName,PSI.TransportDocRefNo 
//						,FORMAT( PSI.TransportDocDate, 'dd-MMM-yyyy')TransportDocDate,Agent.UserName as AgentName
//						,''AgentCommission
//						,'' Insurance
//,PSI.CargoGrossWt GrossWeight,''LoTNo
//						FROM TRN.SalesMaterial AS SM 
//						LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
//						LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
//						LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
//						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
//						LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
//						LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId
//						LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
//						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
//						LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
//						LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId AND SM.SalesOrderId=FC.SalesOrderId
//						LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId
//						LEFT JOIN [HKP].[Characteristics] AS CH ON FC.CharacteristicsId=CH.Id
//						LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId AND SM.SalesOrderId=SC.SalesOrderId
//						LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
//						LEFT JOIN [HKP].[Characteristics] AS CH2 ON SC.CharacteristicsId=CH2.Id
//						LEFT JOIN TRN.ThirdCharacteristics AS TC ON TC.Id=SM.ThirdCharacteristicsId AND SM.SalesOrderId=TC.SalesOrderId
//						LEFT JOIN HKP.CharacteristicsValue AS TCV ON TCV.Id=SM.ThirdCharacteristicsValueId
//						LEFT JOIN [HKP].[Characteristics] AS CH3 ON TC.CharacteristicsId=CH3.Id
//						LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
//						LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
//						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
//						LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
//						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
//						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
//						LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
//						LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
//						LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
//						LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
//						LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
//						LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST' and A.SalesServiceId IS NULL
//									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
//								   ) TAxInfo	ON TAxInfo.SalesMaterialId=SM.Id 
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST' and A.SalesServiceId IS NULL	
//									) TAxInfo1	ON TAxInfo1.SalesMaterialId=SM.Id 
							  		 
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST' and A.SalesServiceId IS NULL	
//									) TAxInfo2	ON TAxInfo2.SalesMaterialId=SM.Id 

//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TDS' and A.SalesServiceId IS NULL									
//									) TAxInfo3	ON TAxInfo3.SalesMaterialId=SM.Id 


							
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='VAT' and A.SalesServiceId IS NULL		
//						) TAxInfo4 ON TAxInfo4.SalesMaterialId=SM.Id 

//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='AIT' and A.SalesServiceId IS NULL		
//						) TAxInfo5 ON TAxInfo5.SalesMaterialId=SM.Id 
//						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TCS' and A.SalesServiceId IS NULL		 
								
//						) TAxInfo6 ON TAxInfo6.SalesMaterialId=SM.Id 
//						LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
// LEFT JOIN PostSalesInvoice PSI On PSI.SalesId=SA.Id
//						LEFT JOIN HKP.Party as Agent on Agent.Id=PSI.TransportAgentId
//						WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) <= '" + toDate + @"'
//						UNION ALL
						
//						Select                  
//						ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
//						,IR.SourceType
//						,ISs.Id
//						,IR.Id SalesId
//						,FORMAT(IR.EntryDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
//						,'' SalesOrderId
//						,'' MasterOrderId
//						,'' SONo
//						,'' PONumber
//						,'' AS BillTo							
//						,''BillToAddress
//						,'' BillToState
//						,'' BillToGSTNo
//						,'' ShipTo
//						,'' ShipToAddress
//						,''ShipToState
//						,''ShipToGSTNo						
//						, 0 ToCurrencyRate
//						, '' DocRefNo
//						,'' DocDate
//						, P.UserName AS PartyName
//						,'' AS MaterialGroupMasterName
//						,SM.UserName MaterialMasterName
//						,'' AS MaterialMasterArticleName
//						,''FirstCharacteristicsValue
//						,'' SecondCharacteristicsValue
//						,'' ThirdCharacteristicsValue
//						, '' HSNCode

//						,0 BaseRate
//						,0 BaseUoMFactor
//						,0 TransactionRate
//						,0 TransactionQty
//						,ISs.Amount TransactionAmount
//						,ISs.TaxAmount
//						,0 NetAmount
//						,'' VoucherDetailId
//						,''  BaseUoM
//						,''  TransactionUoM
//						,''  Currency
//						,'' DeliveryDate
//						,'' DestinationName
//						,'' SOType
//						,0 ServiceCharge
//						, 0 ServiceTax
//						,'' Entity
//						,'' CheckedByName
//						,'' CheckedBy
//						,'' ApprovedByName
//						,'' ApprovedBy
//						,'' Posted
//						,'' 'NoteForAccounts'

//						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
//						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
//						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
//						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
//						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
//,''ContainerNo,''TransporterName,''TransportDocRefNo 
//						,''TransportDocDate,''AgentName
//						,''AgentCommission
//						,'' Insurance
//,''GrossWeight,''LoTNo
//						from trn.SalesService AS ISs
//						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
//						left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
//						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
//						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
//						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
//						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
//						left join trn.Voucher V on V.Id=I.VoucherId
//						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
//						left join trn.Voucher V1 on V1.Id=ep.VoucherId
//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST'  
									
//									) TAxInfo	ON TAxInfo.SalesServiceId=ISs.Id AND TAxInfo.SalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST'  
//									) TAxInfo1	ON TAxInfo1.SalesServiceId=ISs.Id AND TAxInfo1.SalesServiceId IS NOT NULL 
							  		 
//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST'  

//									) TAxInfo2	ON TAxInfo2.SalesServiceId=ISs.Id AND TAxInfo2.SalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TDS'  
//									) TAxInfo3	ON TAxInfo3.SalesServiceId=ISs.Id AND TAxInfo3.SalesServiceId IS NOT NULL


							
//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='VAT'  
								
//						) TAxInfo4 ON TAxInfo4.SalesServiceId=ISs.Id AND TAxInfo4.SalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='AIT'  
//						) TAxInfo5 ON TAxInfo5.SalesServiceId=ISs.Id AND TAxInfo5.SalesServiceId IS NOT NULL
//						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.Amount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[SalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TCS'
//						) TAxInfo6 ON TAxInfo6.SalesServiceId=ISs.Id AND TAxInfo6.SalesServiceId IS NOT NULL

//						WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.InvoiceDate) <= '" + toDate + @"'
//						UNION ALL

//						SELECT 

//						ROW_NUMBER() Over(Order by   II.Id) As[S.N]
//						,'InventorySales' SourceType
//						,IID.Id
//						,II.Id SalesId
//						,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
//						,'' SalesOrderId
//						,'' MasterOrderId
//						,'' SONo
//						,'' PONumber
//						,PPI.UserName AS BillTo							
//						,AM.Address1 as  BillToAddress
//						,ST.UserName as  BillToState
//						,PPI.GSTIN    as BillToGSTNo
//					    ,PPI1.UserName as ShipTo
//						,AM1.Address1 as ShipToAddress
//						,ST1.UserName as ShipToState
//						,PPI1.GSTIN as ShipToGSTNo						
//						,II.ToCurrencyRate
//						, II.DocRefNo
//						,FORMAT( II.DocDate,'dd-MMM-yyyy') DocDate
//						, P.UserName AS PartyName
//						,MGM.UserName AS MaterialGroupMasterName
//						,MM.UserName MaterialMasterName
//						,ART.StandardName AS MaterialMasterArticleName
//						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
//						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
//						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
//						, TAxInfo.HSCode HSNCode

//						,Sum(IID.PolicyRate) BaseRate
//						,0 BaseUoMFactor
//						,sum(IID.PolicyRate) TransactionRate
//						,Sum(IID.Qty) TransactionQty
//						,Sum(IID.Qty *IID.PolicyRate) TransactionAmount
//						,sum(SCr1.TaxAmount) TaxAmount
//						,0 NetAmount
//						,II.VoucherId VoucherDetailId
//						,TUoM.UserName AS BaseUoM
//						,TUoM.UserName AS TransactionUoM
//						,'' AS Currency
//						,'' DeliveryDate
//						,'' DestinationName
//						,'' SOType
//						,sum(SCr.Amount) ServiceCharge
//						,sum(SCr.TotalTaxAmount) ServiceTax

//						,E.UserName AS Entity 
//						,EI2.EmployeeName CheckedByName
//						,II.CheckedBy
//						,EI1.EmployeeName ApprovedByName
//						,II.ApprovedBy
//						,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
//						,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'

//						,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST,sum(TAxInfo.Percentage) CGSTTaxPercentage--MaterialTaxPer						
//						,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST,sum(TAxInfo2.Percentage) SGSTTaxPercentage
//						,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST,sum(TAxInfo1.Percentage) IGSTTaxPercentage
//						,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS,sum(TAxInfo3.Percentage) TDSTaxPercentage
//						,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS,sum(TAxInfo6.Percentage) TCSTaxPercentage
//,''ContainerNo,''TransporterName,''TransportDocRefNo 
//						,''TransportDocDate,''AgentName
//						,''AgentCommission
//						,'' Insurance
//,''GrossWeight,''LoTNo
//						FROM[TRN].[InventorySales] AS II
//						left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,IsAsset,BaseUOMId from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
//						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
//						left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
//						left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
//						Left JOIN [ORG].[Entity] E On E.id= II.EntityId

//						--left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
//						--left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
//						--left JOIN SCS.Country c ON C.Id=IR.CountryId

//						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
//						LEFT JOIN MST.AddressMaster as AM on AM.Id=PPI.AddressMasterId
//						LEFT JOIN SCS.[State] as ST on ST.Id=AM.StateId
						
//						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
//						LEFT JOIN MST.AddressMaster as AM1 on AM1.Id=PPI1.AddressMasterId
//						LEFT JOIN SCS.[State] as ST1 on ST1.Id=AM1.StateId
//						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
//						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
//						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
//						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
//						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
//						--Left Join [HKP].[Party] Par As Par.Id=II.P
//						LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
//						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
//						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
//						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
//						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id			
//						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
//						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
//						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
//						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
//						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
//						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
//						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
//						LEFT JOIN(Select sum(Amount) Amount, sum(TotalTaxAmount) TotalTaxAmount, InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
//						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id
//LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
//								   FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST' and A.InventorySalesServiceId IS NULL								
//								   ) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST' and A.InventorySalesServiceId IS NULL									
//									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 
							  		 
//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST' and A.InventorySalesServiceId IS NULL 									
//									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									WHERE B.Code='TDS' and A.InventorySalesServiceId IS NULL 					
//									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 							
//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='VAT' and A.InventorySalesServiceId IS NULL 
								
//						) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.InventorySalesId 

//						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='AIT' and A.InventorySalesServiceId IS NULL 
							
//						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IID.InventorySalesId 
//						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM 
//									[TRN].InventorySalesAdditionalTax A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='TCS' 								
//						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
						
//						WHERE II.PlantId='" + identity.PlantId + "' AND convert(Date,II.SalesDate) <= '" + toDate + @"'
//						GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
//						,II.SalesDate, MS.UserName
//						,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo 
//						,PPI.UserName,AM.Address1,ST.UserName,PPI.GSTIN ,PPI1.UserName ,PPI1.GSTIN,ST1.UserName,AM1.Address1,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
//						, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) ,p.UserName ,P.Id 
//						,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName ,II.ApprovedBy
//						,MT.UserName ,MGM.UserName,IM.MaterialMasterId,MM.UserName, ART.StandardName 
//						, ISNULL(FCV.UserName,''), ISNULL(SCV.UserName,''), ISNULL(TCV.UserName,''),II.[Status]
//						,Pnt.UserName,HSNC.Code ,Com.UserName,TUoM.UserName	,ComG.UserName,II.VoucherId,IID.Id,TAxInfo.HSCode
//						UNION ALL
//						Select                  
//						ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
//						,'InventorySales' SourceType
//						,SM.Id
//						,IR.Id SalesId
//						,FORMAT(IR.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
//						,'' SalesOrderId
//						,'' MasterOrderId
//						,'' SONo
//						,'' PONumber
//						,''  BillTo							
//						,''BillToAddress
//						,'' BillToState
//						,'' BillToGSTNo
//						,'' ShipTo
//						,'' ShipToAddress
//						,''ShipToState
//						,''ShipToGSTNo		
//						, 0 ToCurrencyRate
//						, '' DocRefNo
//						,'' DocDate
//						, P.UserName AS PartyName
//						,'' AS MaterialGroupMasterName
//						,SM.UserName MaterialMasterName
//						,'' AS MaterialMasterArticleName
//						,''FirstCharacteristicsValue
//						,'' SecondCharacteristicsValue
//						,'' ThirdCharacteristicsValue
//						, '' HSNCode

//						,0 BaseRate
//						,0 BaseUoMFactor
//						,0 TransactionRate
//						,0 TransactionQty
//						,ISs.Amount TransactionAmount
//						,ISs.Amount TaxAmount
//						,0 NetAmount
//						,'' VoucherDetailId
//						,'' AS BaseUoM
//						,'' AS TransactionUoM
//						,'' AS Currency
//						,'' DeliveryDate
//						,'' DestinationName
//						,'' SOType
//						,0 ServiceCharge
//						,0 ServiceTax
//						,'' Entity
//						,'' CheckedByName
//						,'' CheckedBy
//						,'' ApprovedByName
//						,'' ApprovedBy
//						,'' Posted
//						,'' 'NoteForAccounts'

//						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
//						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
//						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
//						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
//						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
//,''ContainerNo,''TransporterName,''TransportDocRefNo 
//						,''TransportDocDate,''AgentName
//						,''AgentCommission
//						,'' Insurance
//,''GrossWeight,''LoTNo
//						from trn.InventoryService AS ISS
//						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
//						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventoryReceiveId
//						LEFT JOIN HKP.Party AS P ON P.Id=IR.CustomerId
//						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
//						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
//						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
//						left join trn.Voucher V on V.Id=I.VoucherId
//						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
//						left join trn.Voucher V1 on V1.Id=ep.VoucherId
//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST'  
//									--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
//									) TAxInfo	ON TAxInfo.InventorySalesServiceId=ISs.Id AND TAxInfo.InventorySalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST'  

//									) TAxInfo1	ON TAxInfo1.InventorySalesServiceId=ISs.Id AND TAxInfo1.InventorySalesServiceId IS NOT NULL 
							  		 
//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST'  

//									) TAxInfo2	ON TAxInfo2.InventorySalesServiceId=ISs.Id AND TAxInfo2.InventorySalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TDS' 
//									) TAxInfo3	ON TAxInfo3.InventorySalesServiceId=ISs.Id AND TAxInfo3.InventorySalesServiceId IS NOT NULL


							
//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='VAT' 
								
//						) TAxInfo4 ON TAxInfo4.InventorySalesServiceId=ISs.Id AND TAxInfo4.InventorySalesServiceId IS NOT NULL

//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='AIT' 
//						) TAxInfo5 ON TAxInfo5.InventorySalesServiceId=ISs.Id AND TAxInfo5.InventorySalesServiceId IS NOT NULL
//						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
//									,A.TaxAmount TaxAmount,HS.Code HSCode 
//									FROM  [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='TCS' 
//						) TAxInfo6 ON TAxInfo6.InventorySalesServiceId=ISs.Id AND TAxInfo6.InventorySalesServiceId IS NOT NULL
//						WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.SalesDate) <= '" + toDate + @"'";
//						return _sqlRepository.GetDataTable(sql);
//					}
//				}
//				else
//				{
//					if (fromDate != "" && toDate != "")
//					{
//						sql = @"SELECT 
//							ROW_NUMBER() Over(Order by SA.Id) As[S.N]
//							,SA.Id SalesId
//							,SA.SourceType
//							--SM.Id	
//							,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
//							--,SMD.SalesOrderId
//							--,MO.Id MasterOrderId
//							--,SO.Id SONo
//							--,po.PONumber
//							,PPI.UserName AS BillTo
//							,PPD.UserName AS ShipTo
//							, SA.ToCurrencyRate
//							, SA.DocRefNo
//							,'' DocDate
//							, P.UserName AS PartyName,p.Code	
//							--, '' HSNCode
//							--,SM.BaseRate
//							--,SM.BaseUoMFactor
//							--,SM.TransactionRate
//							--,SM.TransactionQty
//							,Sum(SMD.TransactionAmount) TransactionAmount
//							--,SM.TaxAmount
//							--,SM.NetAmount
//							,v.VoucherNo VoucherId
//							--,BUoM.UserName AS BaseUoM
//							--,TUoM.UserName AS TransactionUoM
//							,CU.Code AS Currency
//							--,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
//							--,DT.UserName DestinationName
//							,''SOType
//							,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
//							,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
//							--TransactionAmount
//							,'' Entity
//							,'' CheckedByName
//							,'' CheckedBy
//							,'' ApprovedByName
//							,'' ApprovedBy
//							,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
//							,'' 'NoteForAccounts'
	
//							,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
//							,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
//							,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
//							,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
//							,round(isnull(TAxInfo6.TaxAmount,0),2) TCS
							
//							,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
//							,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
//							,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
//							,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

//							,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
//							,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//							,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

//							,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
//							,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
							
//							,SONumber=STUFF((select distinct ','+XSO.Id 
//                                         from trn.SalesMaterial SMX									 
//										 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
//							                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
//							, PONumber=STUFF((select distinct ','+CPO.PONumber
//                                         from trn.SalesMaterial SMX									 
//										 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
//										  LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
//							                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
//							, MasterOrder=STUFF((select distinct ','+MO.MasterOrderNo
//                                         from trn.SalesMaterial SMX									 
//										 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
//										  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
//										  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
//							                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							
//							FROM TRN.Sales AS SA
//							LEFT JOIN (select Id,SalesId,SalesOrderId, Sum(TransactionAmount) TransactionAmount,Sum(NetAmount) NetAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from TRN.SalesMaterial Group BY SalesId,SalesOrderId,Id)SMD  ON SA.Id=SMD.SalesId
//							--LEFT JOIN [TRN].[SalesOrder] AS SO ON SMD.SalesOrderId=SO.Id
//							--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
//							--LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
//							--LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
//							--LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId	
//							LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
//							--LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
//							--LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
//							LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
//							LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
//							LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
//							LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
//							LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
//							LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
//							LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
//							LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
//							LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
//										Group by A.salesMaterialId
//										) TAxInfo	ON TAxInfo.salesMaterialId=SMD.Id
//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
//										Group by A.salesMaterialId
//										) TAxInfo1	ON TAxInfo1.salesMaterialId=SMD.Id 
							  		 
//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='SGST' --and A.SalesServiceId IS NULL
//										Group by A.salesMaterialId
//										) TAxInfo2	ON TAxInfo2.salesMaterialId=SMD.Id 

//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
//										Group by A.salesMaterialId
//										) TAxInfo3	ON TAxInfo3.salesMaterialId=SMD.Id 
							
//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
//										Group by A.salesMaterialId
//							) TAxInfo4 ON TAxInfo4.salesMaterialId=SMD.Id 

//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
//										Group by A.salesMaterialId
//							) TAxInfo5 ON TAxInfo5.salesMaterialId=SMD.Id 
//							LEFT JOIN (SELECT A.SalesId,A.BooksCurrencyTaxAmount BooksTaxAmount,TaxAmount TaxAmount
//										FROM trn.SalesAdditionalTax A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
//										WHERE B.Code='TCS'  
//										--Group BY A.SalesId				
//							) TAxInfo6 ON TAxInfo6.SalesId=SA.Id 
//							LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//									from trn.SalesService AS ISS
//									LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
//									left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
//									group by ISS.SalesId
//									)ServiceData on ServiceData.SalesId=SA.Id
//							LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
//							WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) BETWEEN  '" + fromDate + @"'AND '" + toDate + @"'-- and sm.SalesId='202110'
//							Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code
//						UNION ALL
//						SELECT 

//						ROW_NUMBER() Over(Order by   II.Id) As[S.N]
//						,II.Id SalesId
//						,'InventorySales' SourceType
//						--,IID.Id						
//						,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
//						--,'' SalesOrderId
//						--,'' MasterOrderId
//						--,'' SONo
//						--,'' PONumber
//						,PPI.UserName AS BillTo
//						,PPI1.UserName ShipTo
//						,II.ToCurrencyRate
//						, II.DocRefNo
//						,II.DocDate
//						, P.UserName AS PartyName,p.Code
//						--,MGM.UserName AS MaterialGroupMasterName
//						--,MM.UserName MaterialMasterName
//						--,ART.StandardName AS MaterialMasterArticleName
//						--, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
//						--, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
//						--, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
//						--, '' HSNCode

//						--,Sum(IID.PolicyRate) BaseRate
//						--,0 BaseUoMFactor
//						--,sum(IID.PolicyRate) TransactionRate
//						--,Sum(IID.Qty) TransactionQty
//						,Sum(IID.Qty *IID.SalesRate) TransactionAmount
//						--,sum(SCr1.TaxAmount) TaxAmount
//						--,0 NetAmount
//						,v.VoucherNo VoucherId
//						--,TUoM.UserName AS BaseUoM
//						--,TUoM.UserName AS TransactionUoM
//						,'' AS Currency
//						--,'' DeliveryDate
//						--,'' DestinationName
//						,'' SOType
//						,sum(SCr.ServiceAmount) ServiceCharge
//						,sum(SCr.TotalTaxAmount) ServiceTax

//						,E.UserName AS Entity 
//						,EI2.EmployeeName CheckedByName
//						,II.CheckedBy
//						,EI1.EmployeeName ApprovedByName
//						,II.ApprovedBy
//						,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
//						,'' 'NoteForAccounts'

//						,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
//						,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
//						,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
//						,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
//						,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
//						,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
//						,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
//						,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
//						,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
//						,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
//						,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//						,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
//						,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
//						,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt


//						,'' SONumber
//						,'' PONumber
//						,'' MasterOrder
//						FROM[TRN].[InventorySales] AS II
//						left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
//						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
//						left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
//						left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
//						Left JOIN [ORG].[Entity] E On E.id= II.EntityId
//						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
//						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
//						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
//						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
//						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
//						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
//						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
						
//						LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
//						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST' --and A.InventorySalesServiceId IS NULL
//									GROUP BY A.InventorySalesId
//									) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST' --and A.InventorySalesServiceId IS NULL
//									GROUP BY A.InventorySalesId
//									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 
							  		 
//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST' --and A.InventorySalesServiceId IS NULL 
//									GROUP BY A.InventorySalesId
//									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									WHERE B.Code='TDS' --and A.InventorySalesServiceId IS NULL
//									GROUP BY A.InventorySalesId
//									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 
							
//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='VAT' --and A.InventorySalesServiceId IS NULL
//									GROUP BY A.InventorySalesId
								
//						) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.Id 

//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//								FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='AIT' --and A.InventorySalesServiceId IS NULL 
//									GROUP BY A.InventorySalesId
							
//						) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
//									FROM [TRN].InventorySalesAdditionalTax A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='TCS'
//									GROUP BY A.InventorySalesId
//						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
//						LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
//						WHERE II.PlantId='" + identity.PlantId + @"' AND convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
//						GROUP BY p.Code	,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy";
//						return _sqlRepository.GetDataTable(sql);
//					}
//					else
//					{
//						sql = @"SELECT 
//							ROW_NUMBER() Over(Order by SA.Id) As[S.N]
//							,SA.Id SalesId
//							,SA.SourceType
//							--SM.Id	
//							,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate ,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
//							--,SMD.SalesOrderId
//							--,MO.Id MasterOrderId
//							--,SO.Id SONo
//							--,po.PONumber
//							,PPI.UserName AS BillTo
//							,PPD.UserName AS ShipTo
//							, SA.ToCurrencyRate
//							, SA.DocRefNo
//							,'' DocDate
//							, P.UserName AS PartyName,p.Code	
//							--, '' HSNCode
//							--,SM.BaseRate
//							--,SM.BaseUoMFactor
//							--,SM.TransactionRate
//							--,SM.TransactionQty
//							,Sum(SMD.TransactionAmount) TransactionAmount
//							--,SM.TaxAmount
//							--,SM.NetAmount
//							,v.VoucherNo VoucherId
//							--,BUoM.UserName AS BaseUoM
//							--,TUoM.UserName AS TransactionUoM
//							,CU.Code AS Currency
//							--,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
//							--,DT.UserName DestinationName
//							,''SOType
//							,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
//							,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
//							--TransactionAmount
//							,'' Entity
//							,'' CheckedByName
//							,'' CheckedBy
//							,'' ApprovedByName
//							,'' ApprovedBy
//							,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
//							,'' 'NoteForAccounts'
	
//							,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
//							,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
//							,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
//							,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
//							,round(isnull(TAxInfo6.TaxAmount,0),2) TCS
							
//							,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
//							,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
//							,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
//							,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

//							,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
//							,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//							,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

//							,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
//							,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
							
//							,SONumber=STUFF((select distinct ','+XSO.Id 
//                                         from trn.SalesMaterial SMX									 
//										 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
//							                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
//							, PONumber=STUFF((select distinct ','+CPO.PONumber
//                                         from trn.SalesMaterial SMX									 
//										 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
//										  LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
//							                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
//							, MasterOrder=STUFF((select distinct ','+MO.MasterOrderNo
//                                         from trn.SalesMaterial SMX									 
//										 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
//										  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
//										  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
//							                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							
//							FROM TRN.Sales AS SA
//							LEFT JOIN (select Id, SalesId,SalesOrderId, Sum(TransactionAmount) TransactionAmount,Sum(NetAmount) NetAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from TRN.SalesMaterial Group BY SalesId,SalesOrderId,Id)SMD  ON SA.Id=SMD.SalesId
//							--LEFT JOIN [TRN].[SalesOrder] AS SO ON SMD.SalesOrderId=SO.Id
//							--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
//							--LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
//							--LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
//							--LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId	
//							LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
//							--LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
//							--LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
//							LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
//							LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
//							LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
//							LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
//							LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
//							LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
//							LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
//							LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
//							LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
//										Group by A.salesMaterialId
//										) TAxInfo	ON TAxInfo.salesMaterialId=SMD.Id
//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
//										Group by A.salesMaterialId
//										) TAxInfo1	ON TAxInfo1.salesMaterialId=SMD.Id 
							  		 
//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='SGST' --and A.SalesServiceId IS NULL
//										Group by A.salesMaterialId
//										) TAxInfo2	ON TAxInfo2.salesMaterialId=SMD.Id 

//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
//										Group by A.salesMaterialId
//										) TAxInfo3	ON TAxInfo3.salesMaterialId=SMD.Id 
							
//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
//										Group by A.salesMaterialId
//							) TAxInfo4 ON TAxInfo4.salesMaterialId=SMD.Id 

//							LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//										FROM [TRN].[SalesTax] A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//										left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//										WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
//										Group by A.salesMaterialId
//							) TAxInfo5 ON TAxInfo5.salesMaterialId=SMD.Id 
//							LEFT JOIN (SELECT A.SalesId,A.BooksCurrencyTaxAmount BooksTaxAmount,TaxAmount TaxAmount
//										FROM trn.SalesAdditionalTax A
//										LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
//										WHERE B.Code='TCS'  
//										--Group BY A.SalesId				
//							) TAxInfo6 ON TAxInfo6.SalesId=SA.Id 
//							LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//									from trn.SalesService AS ISS
//									LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
//									left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
//									group by ISS.SalesId
//									)ServiceData on ServiceData.SalesId=SA.Id
//							LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
//							WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) <= '" + toDate + @"'-- and sm.SalesId='202110'
//							Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code
//						UNION ALL
//						SELECT 

//						ROW_NUMBER() Over(Order by   II.Id) As[S.N]
//						,II.Id SalesId
//						,'InventorySales' SourceType
//						--,IID.Id						
//						,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
//						--,'' SalesOrderId
//						--,'' MasterOrderId
//						--,'' SONo
//						--,'' PONumber
//						,PPI.UserName AS BillTo
//						,PPI1.UserName ShipTo
//						,II.ToCurrencyRate
//						, II.DocRefNo
//						,II.DocDate
//						, P.UserName AS PartyName,p.Code
//						--,MGM.UserName AS MaterialGroupMasterName
//						--,MM.UserName MaterialMasterName
//						--,ART.StandardName AS MaterialMasterArticleName
//						--, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
//						--, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
//						--, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
//						--, '' HSNCode

//						--,Sum(IID.PolicyRate) BaseRate
//						--,0 BaseUoMFactor
//						--,sum(IID.PolicyRate) TransactionRate
//						--,Sum(IID.Qty) TransactionQty
//						,Sum(IID.Qty *IID.SalesRate) TransactionAmount
//						--,sum(SCr1.TaxAmount) TaxAmount
//						--,0 NetAmount
//						,v.VoucherNo VoucherId
//						--,TUoM.UserName AS BaseUoM
//						--,TUoM.UserName AS TransactionUoM
//						,'' AS Currency
//						--,'' DeliveryDate
//						--,'' DestinationName
//						,'' SOType
//						,sum(SCr.ServiceAmount) ServiceCharge
//						,sum(SCr.TotalTaxAmount) ServiceTax

//						,E.UserName AS Entity 
//						,EI2.EmployeeName CheckedByName
//						,II.CheckedBy
//						,EI1.EmployeeName ApprovedByName
//						,II.ApprovedBy
//						,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
//						,'' 'NoteForAccounts'

//						,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
//						,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
//						,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
//						,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
//						,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
//						,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
//						,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
//						,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
//						,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
//						,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
//						,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
//						,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
//						,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
//						,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt


//						,'' SONumber
//						,'' PONumber
//						,'' MasterOrder
//						FROM[TRN].[InventorySales] AS II
//						left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
//						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
//						left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
//						left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
//						Left JOIN [ORG].[Entity] E On E.id= II.EntityId
//						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
//						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
//						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
//						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
//						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
//						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
//						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
						
//						LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
//						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='CGST' --and A.InventorySalesServiceId IS NULL
//									GROUP BY A.InventorySalesId
//									) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='IGST' --and A.InventorySalesServiceId IS NULL
//									GROUP BY A.InventorySalesId
//									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 
							  		 
//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
//									WHERE B.Code='SGST' --and A.InventorySalesServiceId IS NULL 
//									GROUP BY A.InventorySalesId
//									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
//									WHERE B.Code='TDS' --and A.InventorySalesServiceId IS NULL
//									GROUP BY A.InventorySalesId
//									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 
							
//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//									FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='VAT' --and A.InventorySalesServiceId IS NULL
//									GROUP BY A.InventorySalesId
								
//						) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.Id 

//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
//								FROM [TRN].[InventorySalesTax] A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='AIT' --and A.InventorySalesServiceId IS NULL 
//									GROUP BY A.InventorySalesId
							
//						) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
//						LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
//									FROM [TRN].InventorySalesAdditionalTax A
//									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
//									WHERE B.Code='TCS'
//									GROUP BY A.InventorySalesId
//						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
//						LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
//						WHERE II.PlantId='" + identity.PlantId + @"' AND convert(Date,II.SalesDate) <= '" + toDate + @"'
//						GROUP BY p.Code	,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy";
//						return _sqlRepository.GetDataTable(sql);
//					}
//				}

//			}

//			catch (Exception ex)
//			{
//				throw ex;
//			}
//		}
		public DataTable GetInventorySalesReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate, string Qty, string Amount, string Summery)
		{
			var sql = "";
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				if (Summery == "Details")
				{
					if (fromDate != "" && toDate != "")
					{
						sql = @"SELECT 
								ROW_NUMBER() Over(Order by   SM.Id) As[S.N]
								,SA.SourceType
								,SM.Id
								,SM.SalesId
								,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
								,SM.SalesOrderId
								,MO.Id MasterOrderId
								,SO.Id SONo
								,po.PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as  BillToAddress
								,ST.UserName as  BillToState
								,PPI.GSTIN as BillToGSTNo
								,PPD.UserName AS ShipTo
								,AMD.Address1 as ShipToAddress
								,STD.UserName as ShipToState
		                        ,PPD.GSTIN as ShipToGSTNo
								, SA.ToCurrencyRate
								, SA.DocRefNo
								,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								,FCV.UserName FirstCharacteristicsValue
								,SCV.UserName SecondCharacteristicsValue
								,TCV.UserName ThirdCharacteristicsValue
								--,'' HSNCode
								,SM.BaseRate
								,SM.BaseUoMFactor
								,SM.TransactionRate
								,SM.TransactionQty
								,SM.TransactionAmount
								,SM.TaxAmount
								,SM.NetAmount
								,v.VoucherNo VoucherDetailId
								,BUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								,CU.Code AS Currency
								,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
								,DT.UserName DestinationName
								,SO.SOType
								,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
								,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,Posted=CASE WHEN SA.VoucherId IS NULL THEN 'No' ELSE 'YES'  END
								,ISNULL(SA.Narration,'') NoteForAccounts
								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

		                        ,PSI.CNFContainerNo ContainerNo,PSI.TransportDriverName as TransporterName,PSI.TransportDocRefNo 
								,FORMAT( PSI.TransportDocDate, 'dd-MMM-yyyy')TransportDocDate,Agent.UserName as AgentName
								,''AgentCommission
								,'' Insurance
								,PSI.CargoGrossWt GrossWeight,''LoTNo
								,CON.ContractNo
								,ML.LCRef MasterLcNo
								,SA.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(SA.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,SA.BaseNoOfDays NoOfDays
							    ,FORMAT(SA.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								,TA.UserName TransportAgent	

								,CNfA.UserName CNFAgent
								,PSI.CNFContainerNo
								,PSI.CNFVesselTrackingNo
								, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													, RealizeAmount=isnull(I.WrittenOffAmount,0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=SA.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									--, BalanceAmount=isnull(ISNULL(SM.TransactionAmount,0) - ISNULL(I.WrittenOffAmount,0),0)


								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId

									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join [Contract] CON on CON.Id=MOI.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId
									left outer join PostSalesInvoice PSI on PSI.SalesId=SA.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=SA.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=SA.PartyId
									left outer join HKP.Party TA on TA.Id=SA.PartyId


						--LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
						--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
						LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
						LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId
						LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
						LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId AND SM.SalesOrderId=FC.SalesOrderId
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId
						LEFT JOIN [HKP].[Characteristics] AS CH ON FC.CharacteristicsId=CH.Id
						LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId AND SM.SalesOrderId=SC.SalesOrderId
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
						LEFT JOIN [HKP].[Characteristics] AS CH2 ON SC.CharacteristicsId=CH2.Id
						LEFT JOIN TRN.ThirdCharacteristics AS TC ON TC.Id=SM.ThirdCharacteristicsId AND SM.SalesOrderId=TC.SalesOrderId
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON TCV.Id=SM.ThirdCharacteristicsValueId
						LEFT JOIN [HKP].[Characteristics] AS CH3 ON TC.CharacteristicsId=CH3.Id
						LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
						LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
						LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
						LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
						LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
						LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
						LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
						LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
						Left JOIN [ORG].[Entity] E On E.id= SA.EntityId
						LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,VoucherId 
										FROM TRN.Invoice GROUP BY VoucherId) I ON I.VoucherId=SA.VoucherId
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST' and A.SalesServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								   ) TAxInfo	ON TAxInfo.SalesMaterialId=SM.Id 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.SalesServiceId IS NULL	
									) TAxInfo1	ON TAxInfo1.SalesMaterialId=SM.Id 
							  		 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.SalesServiceId IS NULL	
									) TAxInfo2	ON TAxInfo2.SalesMaterialId=SM.Id 

						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' and A.SalesServiceId IS NULL									
									) TAxInfo3	ON TAxInfo3.SalesMaterialId=SM.Id 


							
					
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' and A.SalesServiceId IS NULL		 
								
						) TAxInfo6 ON TAxInfo6.SalesMaterialId=SM.Id 
						LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
                        --LEFT JOIN PostSalesInvoice PSI On PSI.SalesId=SA.Id
						LEFT JOIN HKP.Party as Agent on Agent.Id=PSI.TransportAgentId

								WHERE SA.PlantId='" + identity.PlantId + @"' AND convert(Date,SA.InvoiceDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 

									UNION ALL

														Select                  
								ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
								,IR.SourceType
								,ISs.Id
								,IR.Id SalesId
								,FORMAT(IR.EntryDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,'' AS BillTo
								,'' as BillToAddress
								,'' as BillToState
								,'' as BillToGSTNo
								,'' AS ShipTo
								,'' AS ShipToAddress
								,'' AS ShipToState
								,'' as ShipToGSTNo
								, 0 ToCurrencyRate
								, '' DocRefNo
								,FORMAT(IR.InvoiceDate,'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,'' AS MaterialGroupMasterName
								,SM.UserName MaterialMasterName
								,'' AS MaterialMasterArticleName
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								--, '' HSNCode
								,0 BaseRate
								,0 BaseUoMFactor
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount TransactionAmount
								,ISs.TaxAmount
								,0 NetAmount
								,'' VoucherDetailId
								,''  BaseUoM
								,''  TransactionUoM
								,''  Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								, 0 ServiceTax
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
		,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
		,''GrossWeight,''LoTNo
		,CON.ContractNo
								,ML.LCRef MasterLcNo
								,IR.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,IR.BaseNoOfDays NoOfDays
							    ,FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								,TA.UserName TransportAgent	

								,CNfA.UserName CNFAgent
								,PSI.CNFContainerNo
								,PSI.CNFVesselTrackingNo
								, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=IR.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													, RealizeAmount=isnull(I.WrittenOffAmount,0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=IR.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									--, BalanceAmount=isnull(ISNULL(ISs.Amount,0)- ISNULL(I.WrittenOffAmount,0),0)

								from trn.SalesService AS ISs
								LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
								left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
								left outer join trn.SalesMaterial IRM on IRM.SalesId=IR.Id
									left outer join TRN.SalesOrder So on SO.Id=IRM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join [Contract] CON on CON.Id=MOI.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId
									left outer join PostSalesInvoice PSI on PSI.SalesId=IR.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=IR.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=IR.PartyId
									left outer join HKP.Party TA on TA.Id=IR.PartyId

						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						Left JOIN [ORG].[Entity] E On E.id= IR.EntityId
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									
									) TAxInfo	ON TAxInfo.SalesServiceId=ISs.Id AND TAxInfo.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  
									) TAxInfo1	ON TAxInfo1.SalesServiceId=ISs.Id AND TAxInfo1.SalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  

											) TAxInfo2	ON TAxInfo2.SalesServiceId=ISs.Id AND TAxInfo2.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS'  
									) TAxInfo3	ON TAxInfo3.SalesServiceId=ISs.Id AND TAxInfo3.SalesServiceId IS NOT NULL


						
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS'
						) TAxInfo6 ON TAxInfo6.SalesServiceId=ISs.Id AND TAxInfo6.SalesServiceId IS NOT NULL

								WHERE IR.PlantId='" + identity.PlantId + @"' AND convert(Date,IR.InvoiceDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
								union ALL

								SELECT 
								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,'InventorySales' SourceType
								,IID.Id
								,II.Id SalesId
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as BillToAddress
								,ST.UserName as BillToState				
								,PPI.GSTIN as BillToGSTNo
								,PPI1.UserName ShipTo
								,AM1.Address1 ShipToAddress
								,ST1.UserName ShipToState
								,PPI1.GSTIN ShipToGSTNo
								,II.ToCurrencyRate
								, II.DocRefNo
								,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								--, ISNULL(TAxInfo.HSCode,'') HSNCode

								,IID.PolicyRate BaseRate
								,0 BaseUoMFactor
								,IID.PolicyRate TransactionRate
								,IID.TransactionQty 
								,IID.TransactionQty *IID.PolicyRate TransactionAmount
								,SCr1.TaxAmount TaxAmount
								,0 NetAmount
								,II.VoucherId VoucherDetailId
								,TUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								,'' AS Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,SCr.Amount ServiceCharge
								,SCr.TotalTaxAmount ServiceTax

								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'

								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
		,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,''TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
		,''GrossWeight,''LoTNo
		,''ContractNo
								,''MasterLcNo
								,''ComercialInvoiceNo
								,''ExpiryDate
								,''BLAWBNo,''BLAWBDate
								,''PaymentTerm,''BaseOnDueDate
								,0NoOfDays
							    ,''MatureDate
								,0LCAmount
								,''ExFactoryDate
								,''TransportAgent	

								,''CNFAgent
								,''CNFContainerNo
								,''CNFVesselTrackingNo
								,''OwnReferenceNo
													,0 RealizeAmount

									,''RealizeDate

									--,0BalanceAmount

								FROM[TRN].[InventorySalesDetail] AS IID
								left outer join [TRN].[InventorySales] AS II on II.Id=IID.InventorySalesId

								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId

								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
								LEFT JOIN [SCS].[State] as ST on ST.Id=AM.StateId

						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
						LEFT JOIN [MST].[AddressMaster] AS AM1 ON AM1.Id=PPI1.AddressMasterId
						LEFT JOIN [SCS].[State] as ST1 on ST1.Id=AM1.StateId
						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
						--Left Join [HKP].[Party] Par As Par.Id=II.P
						LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					--	LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id			
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						LEFT JOIN(Select sum(Amount) Amount, sum(TotalTaxAmount) TotalTaxAmount, InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id
			LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount--hs.Code HSCode 
								   FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST' and A.InventorySalesServiceId IS NULL								
								   ) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.InventorySalesServiceId IS NULL									
									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 
							  		 
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.InventorySalesServiceId IS NULL 									
									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InventorySalesServiceId IS NULL 					
									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 							
					
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM 
									[TRN].InventorySalesAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' 								
						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
						
						WHERE II.PlantId='"+identity.PlantId+@"' AND convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
					

								UNION ALL

								Select                  
								ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
								,'InventorySales' SourceType
								,SM.Id
								,IR.Id SalesId
								,FORMAT(IR.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,'' AS BillTo
								,'' AS BillToAddress
								,'' AS BillToState
								,'' as BillToGSTNo
								,'' AS ShipTo
								,'' AS ShipToAddress
								,'' AS ShipToState	
								,'' as ShipToGSTNo
								, 0 ToCurrencyRate
								, '' DocRefNo
								,FORMAT(IR.DocDate,'') DocDate
								, P.UserName AS PartyName,p.Code
								,'' AS MaterialGroupMasterName
								,SM.UserName MaterialMasterName
								,'' AS MaterialMasterArticleName
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								--, '' HSNCode

								,0 BaseRate
								,0 BaseUoMFactor
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount TransactionAmount
								,ISs.Amount TaxAmount
								,0 NetAmount
								,'' VoucherDetailId
								,'' AS BaseUoM
								,'' AS TransactionUoM
								,'' AS Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								,0 ServiceTax
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

						,round(isnull(TAxInfo.TaxAmount,0),2)  CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
,''ContainerNo ,''TransporterName,''TransportDocRefNo 
						,''TransportDocDate,''AgentName
						,''AgentCommission
						,'' Insurance
,''GrossWeight,''LoTNo
,''ContractNo
						,''MasterLcNo
						,''ComercialInvoiceNo
						,''ExpiryDate
						,''BLAWBNo,''BLAWBDate
						,''PaymentTerm,''BaseOnDueDate
						,0NoOfDays
					    ,''MatureDate
						,0LCAmount
						,''ExFactoryDate
						,''TransportAgent	
						
						,''CNFAgent
						,''CNFContainerNo
						,''CNFVesselTrackingNo
						,''OwnReferenceNo
						,0RealizeAmount
					    ,''RealizeDate

							--,0BalanceAmount
						from trn.InventoryService AS ISS
						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventoryReceiveId
						LEFT JOIN HKP.Party AS P ON P.Id=IR.CustomerId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						Left JOIN [ORG].[Entity] E On E.id= IR.EntityId
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
									) TAxInfo	ON TAxInfo.InventorySalesServiceId=ISs.Id AND TAxInfo.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  

									) TAxInfo1	ON TAxInfo1.InventorySalesServiceId=ISs.Id AND TAxInfo1.InventorySalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  

											) TAxInfo2	ON TAxInfo2.InventorySalesServiceId=ISs.Id AND TAxInfo2.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' 
									) TAxInfo3	ON TAxInfo3.InventorySalesServiceId=ISs.Id AND TAxInfo3.InventorySalesServiceId IS NOT NULL
							
					
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' 
						) TAxInfo6 ON TAxInfo6.InventorySalesServiceId=ISs.Id AND TAxInfo6.InventorySalesServiceId IS NOT NULL

								WHERE IR.PlantId='" + identity.PlantId + @"' AND convert(Date,IR.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' ";
						return _sqlRepository.GetDataTable(sql);
					}
					else
					{
						sql = @" SELECT 
								ROW_NUMBER() Over(Order by   SM.Id) As[S.N]
								,SA.SourceType
								,SM.Id
								,SM.SalesId
								,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
								,SM.SalesOrderId
								,MO.Id MasterOrderId
								,SO.Id SONo
								,po.PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as BillToAddress
								,ST.UserName as BillToState
								,PPI.GSTIN as BillToGSTNo
								,PPD.UserName AS ShipTo
								,AMD.Address1 as ShipToAddress
								,STD.UserName as ShipToState
								,PPD.GSTIN as ShipToGSTNo					
								, SA.ToCurrencyRate
								, SA.DocRefNo
								,'' DocDate
								, P.UserName AS PartyName
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								,FCV.UserName FirstCharacteristicsValue
								,SCV.UserName SecondCharacteristicsValue
								,TCV.UserName ThirdCharacteristicsValue
								, TAxInfo.HSCode HSNCode

						,SM.BaseRate
						,SM.BaseUoMFactor
						,SM.TransactionRate
						,SM.TransactionQty
						,SM.TransactionAmount
						,SM.TaxAmount
						,SM.NetAmount
						,v.VoucherNo VoucherDetailId
						,BUoM.UserName AS BaseUoM
						,TUoM.UserName AS TransactionUoM
						,CU.Code AS Currency
						,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
						,DT.UserName DestinationName
						,SO.SOType
						,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
						,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
						,'' Entity
						,'' CheckedByName
						,'' CheckedBy
						,'' ApprovedByName
						,'' ApprovedBy
						,Posted=CASE WHEN SA.VoucherId IS NULL THEN 'No' ELSE 'YES'  END
						,'' 'NoteForAccounts'
						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
,PSI.CNFContainerNo ContainerNo,PSI.TransportDriverName as TransporterName,PSI.TransportDocRefNo 
						,FORMAT( PSI.TransportDocDate, 'dd-MMM-yyyy')TransportDocDate,Agent.UserName as AgentName
						,''AgentCommission
						,'' Insurance
,PSI.CargoGrossWt GrossWeight,''LoTNo
						FROM TRN.SalesMaterial AS SM 
						LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
						LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
						LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
						LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
						LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId
						LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
						LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId AND SM.SalesOrderId=FC.SalesOrderId
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId
						LEFT JOIN [HKP].[Characteristics] AS CH ON FC.CharacteristicsId=CH.Id
						LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId AND SM.SalesOrderId=SC.SalesOrderId
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
						LEFT JOIN [HKP].[Characteristics] AS CH2 ON SC.CharacteristicsId=CH2.Id
						LEFT JOIN TRN.ThirdCharacteristics AS TC ON TC.Id=SM.ThirdCharacteristicsId AND SM.SalesOrderId=TC.SalesOrderId
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON TCV.Id=SM.ThirdCharacteristicsValueId
						LEFT JOIN [HKP].[Characteristics] AS CH3 ON TC.CharacteristicsId=CH3.Id
						LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
						LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
						LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
						LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
						LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
						LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
						LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
						LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST' and A.SalesServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								   ) TAxInfo	ON TAxInfo.SalesMaterialId=SM.Id 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.SalesServiceId IS NULL	
									) TAxInfo1	ON TAxInfo1.SalesMaterialId=SM.Id 
							  		 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.SalesServiceId IS NULL	
									) TAxInfo2	ON TAxInfo2.SalesMaterialId=SM.Id 

						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' and A.SalesServiceId IS NULL									
									) TAxInfo3	ON TAxInfo3.SalesMaterialId=SM.Id 


							
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='VAT' and A.SalesServiceId IS NULL		
						) TAxInfo4 ON TAxInfo4.SalesMaterialId=SM.Id 

						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='AIT' and A.SalesServiceId IS NULL		
						) TAxInfo5 ON TAxInfo5.SalesMaterialId=SM.Id 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' and A.SalesServiceId IS NULL		 
								
						) TAxInfo6 ON TAxInfo6.SalesMaterialId=SM.Id 
						LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
 LEFT JOIN PostSalesInvoice PSI On PSI.SalesId=SA.Id
						LEFT JOIN HKP.Party as Agent on Agent.Id=PSI.TransportAgentId
						WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) <= '" + toDate + @"'
						UNION ALL
						
						Select                  
						ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
						,IR.SourceType
						,ISs.Id
						,IR.Id SalesId
						,FORMAT(IR.EntryDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
						,'' SalesOrderId
						,'' MasterOrderId
						,'' SONo
						,'' PONumber
						,'' AS BillTo							
						,''BillToAddress
						,'' BillToState
						,'' BillToGSTNo
						,'' ShipTo
						,'' ShipToAddress
						,''ShipToState
						,''ShipToGSTNo						
						, 0 ToCurrencyRate
						, '' DocRefNo
						,'' DocDate
						, P.UserName AS PartyName
						,'' AS MaterialGroupMasterName
						,SM.UserName MaterialMasterName
						,'' AS MaterialMasterArticleName
						,''FirstCharacteristicsValue
						,'' SecondCharacteristicsValue
						,'' ThirdCharacteristicsValue
						, '' HSNCode

								,0 BaseRate
								,0 BaseUoMFactor
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount TransactionAmount
								,ISs.TaxAmount
								,0 NetAmount
								,'' VoucherDetailId
								,''  BaseUoM
								,''  TransactionUoM
								,''  Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								, 0 ServiceTax
								,'' Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
,''ContainerNo,''TransporterName,''TransportDocRefNo 
						,''TransportDocDate,''AgentName
						,''AgentCommission
						,'' Insurance
,''GrossWeight,''LoTNo
						from trn.SalesService AS ISs
						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
						left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									
									) TAxInfo	ON TAxInfo.SalesServiceId=ISs.Id AND TAxInfo.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  
									) TAxInfo1	ON TAxInfo1.SalesServiceId=ISs.Id AND TAxInfo1.SalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  

											) TAxInfo2	ON TAxInfo2.SalesServiceId=ISs.Id AND TAxInfo2.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS'  
									) TAxInfo3	ON TAxInfo3.SalesServiceId=ISs.Id AND TAxInfo3.SalesServiceId IS NOT NULL


							
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='VAT'  
								
						) TAxInfo4 ON TAxInfo4.SalesServiceId=ISs.Id AND TAxInfo4.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='AIT'  
						) TAxInfo5 ON TAxInfo5.SalesServiceId=ISs.Id AND TAxInfo5.SalesServiceId IS NOT NULL
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS'
						) TAxInfo6 ON TAxInfo6.SalesServiceId=ISs.Id AND TAxInfo6.SalesServiceId IS NOT NULL

								WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.InvoiceDate) <= '" + toDate + @"'
								UNION ALL

								SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,'InventorySales' SourceType
								,IID.Id
								,II.Id SalesId
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,PPI.UserName AS BillTo							
								,AM.Address1 as  BillToAddress
								,ST.UserName as  BillToState
								,PPI.GSTIN    as BillToGSTNo
							    ,PPI1.UserName as ShipTo
								,AM1.Address1 as ShipToAddress
								,ST1.UserName as ShipToState
								,PPI1.GSTIN as ShipToGSTNo						
								,II.ToCurrencyRate
								, II.DocRefNo
								,FORMAT( II.DocDate,'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								, TAxInfo.HSCode HSNCode

								,Sum(IID.PolicyRate) BaseRate
								,0 BaseUoMFactor
								,sum(IID.PolicyRate) TransactionRate
								,Sum(IID.Qty) TransactionQty
								,Sum(IID.Qty *IID.PolicyRate) TransactionAmount
								,sum(SCr1.TaxAmount) TaxAmount
								,0 NetAmount
								,II.VoucherId VoucherDetailId
								,TUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								,'' AS Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,sum(SCr.Amount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax

								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'

								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST,sum(TAxInfo.Percentage) CGSTTaxPercentage--MaterialTaxPer						
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST,sum(TAxInfo2.Percentage) SGSTTaxPercentage
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST,sum(TAxInfo1.Percentage) IGSTTaxPercentage
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS,sum(TAxInfo3.Percentage) TDSTaxPercentage
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS,sum(TAxInfo6.Percentage) TCSTaxPercentage
		,''ContainerNo,''TransporterName,''TransportDocRefNo 
								,''TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
		,''GrossWeight,''LoTNo
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,IsAsset,BaseUOMId from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId

								--left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
								--left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
								--left JOIN SCS.Country c ON C.Id=IR.CountryId

						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
						LEFT JOIN MST.AddressMaster as AM on AM.Id=PPI.AddressMasterId
						LEFT JOIN SCS.[State] as ST on ST.Id=AM.StateId
						
						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
						LEFT JOIN MST.AddressMaster as AM1 on AM1.Id=PPI1.AddressMasterId
						LEFT JOIN SCS.[State] as ST1 on ST1.Id=AM1.StateId
						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
						--Left Join [HKP].[Party] Par As Par.Id=II.P
						LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id			
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						LEFT JOIN(Select sum(Amount) Amount, sum(TotalTaxAmount) TotalTaxAmount, InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id
LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
								   FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST' and A.InventorySalesServiceId IS NULL								
								   ) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.InventorySalesServiceId IS NULL									
									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 
							  		 
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.InventorySalesServiceId IS NULL 									
									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InventorySalesServiceId IS NULL 					
									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 							
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='VAT' and A.InventorySalesServiceId IS NULL 
								
						) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.InventorySalesId 

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='AIT' and A.InventorySalesServiceId IS NULL 
							
						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IID.InventorySalesId 
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM 
									[TRN].InventorySalesAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' 								
						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
						
						WHERE II.PlantId='" + identity.PlantId + "' AND convert(Date,II.SalesDate) <= '" + toDate + @"'
						GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
						,II.SalesDate, MS.UserName
						,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo 
						,PPI.UserName,AM.Address1,ST.UserName,PPI.GSTIN ,PPI1.UserName ,PPI1.GSTIN,ST1.UserName,AM1.Address1,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
						, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) ,p.UserName ,P.Id 
						,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName ,II.ApprovedBy
						,MT.UserName ,MGM.UserName,IM.MaterialMasterId,MM.UserName, ART.StandardName 
						, ISNULL(FCV.UserName,''), ISNULL(SCV.UserName,''), ISNULL(TCV.UserName,''),II.[Status]
						,Pnt.UserName,HSNC.Code ,Com.UserName,TUoM.UserName	,ComG.UserName,II.VoucherId,IID.Id,TAxInfo.HSCode
						UNION ALL
						Select                  
						ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
						,'InventorySales' SourceType
						,SM.Id
						,IR.Id SalesId
						,FORMAT(IR.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
						,'' SalesOrderId
						,'' MasterOrderId
						,'' SONo
						,'' PONumber
						,''  BillTo							
						,''BillToAddress
						,'' BillToState
						,'' BillToGSTNo
						,'' ShipTo
						,'' ShipToAddress
						,''ShipToState
						,''ShipToGSTNo		
						, 0 ToCurrencyRate
						, '' DocRefNo
						,'' DocDate
						, P.UserName AS PartyName
						,'' AS MaterialGroupMasterName
						,SM.UserName MaterialMasterName
						,'' AS MaterialMasterArticleName
						,''FirstCharacteristicsValue
						,'' SecondCharacteristicsValue
						,'' ThirdCharacteristicsValue
						, '' HSNCode

								,0 BaseRate
								,0 BaseUoMFactor
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount TransactionAmount
								,ISs.Amount TaxAmount
								,0 NetAmount
								,'' VoucherDetailId
								,'' AS BaseUoM
								,'' AS TransactionUoM
								,'' AS Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								,0 ServiceTax
								,'' Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
,''ContainerNo,''TransporterName,''TransportDocRefNo 
						,''TransportDocDate,''AgentName
						,''AgentCommission
						,'' Insurance
,''GrossWeight,''LoTNo
						from trn.InventoryService AS ISS
						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventoryReceiveId
						LEFT JOIN HKP.Party AS P ON P.Id=IR.CustomerId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
									) TAxInfo	ON TAxInfo.InventorySalesServiceId=ISs.Id AND TAxInfo.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  

									) TAxInfo1	ON TAxInfo1.InventorySalesServiceId=ISs.Id AND TAxInfo1.InventorySalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  

											) TAxInfo2	ON TAxInfo2.InventorySalesServiceId=ISs.Id AND TAxInfo2.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' 
									) TAxInfo3	ON TAxInfo3.InventorySalesServiceId=ISs.Id AND TAxInfo3.InventorySalesServiceId IS NOT NULL


							
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='VAT' 
								
						) TAxInfo4 ON TAxInfo4.InventorySalesServiceId=ISs.Id AND TAxInfo4.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='AIT' 
						) TAxInfo5 ON TAxInfo5.InventorySalesServiceId=ISs.Id AND TAxInfo5.InventorySalesServiceId IS NOT NULL
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.TaxAmount TaxAmount,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.TaxCategoryType='TCS' 
						) TAxInfo6 ON TAxInfo6.InventorySalesServiceId=ISs.Id AND TAxInfo6.InventorySalesServiceId IS NOT NULL
						WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.SalesDate) <= '" + toDate + @"'";
						return _sqlRepository.GetDataTable(sql);
					}
				}
				else
				{
					if (fromDate != "" && toDate != "")
					{
						sql = @"SELECT 
									ROW_NUMBER() Over(Order by SA.Id) As[S.N]
									,SA.Id SalesId
									,SA.SourceType
									--SM.Id	
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate						
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.ToCurrencyRate
									, SA.DocRefNo
									,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocDate
									, P.UserName AS PartyName,p.Code	

									,SMD.TransactionAmount

									,v.VoucherNo VoucherId

									,CU.Code AS Currency

									,''SOType
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
									,E.UserName Entity
									,'' CheckedByName
									,'' CheckedBy
									,'' ApprovedByName
									,'' ApprovedBy
									,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
									,iSNUll( SA.Narration,'') NoteForAccounts	
									--,sum(round(isnull(SMD.TaxAmount,0),2)) CGST			
									--,sum(round(isnull(SMD.TaxAmount,0),2)) SGST
									--,sum(round(isnull(SMD.TaxAmount,0),2)) IGST
									--,sum(round(isnull(SMD.TaxAmount,0),2)) TDS
									,SMD.CGST
									,SMD.SGST
									,SMD.IGST
									,SMD.TDS
									,round(isnull(TAxInfo6.TaxAmount,0),2) TCS

									--,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))  BooksCGST		
									--,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									--,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,SMD.BooksCGST
									,SMD.BooksSGST
									,SMD.BooksIGST
									,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
									,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt

									,SONumber=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, PONumber=STUFF((select distinct ','+CPO.PONumber
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, MasterOrder=STUFF((select distinct ','+MO.MasterOrderNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                 where smx.SalesId=SA.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, InvoiceAmount=isnull(I.Amount,0)
									, RealizeAmount=isnull(I.WrittenOffAmount,0)						
		                            , BalanceAmount=isnull(isnull(SMD.NetAmount,0) -isnull(I.WrittenOffAmount,0),0)
									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=SA.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpDate,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') TransportDocDate
									,CNfA.UserName CNFAgent
									,TA.UserName TransportAgent							
									,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
									,PSI.CNFContainerNo,PSI.CNFVesselTrackingNo
									,PTM.UserName PaymentTerm,FORMAT(SA.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
									,SA.BaseNoOfDays NoOfDays
									,FORMAT(SA.MatureDate,'dd-MMM-yyyy') MatureDate
									,SA.EXPFromNo,SA.ComercialInvoiceNo
									,SMD.LCAmount,SMD.ContractNo
									,SMD.MasterLcNo

									FROM TRN.Sales AS SA
									--left outer join TRN.SalesMaterial SM on SM.SalesId=SA.Id
									-----------------------------------------------------------
									LEFT JOIN (

									select SM.SalesId, Sum(SM.TransactionAmount) TransactionAmount,Sum(SM.NetAmount) NetAmount
									,Sum(SM.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount 
									,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
									,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
									,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
									,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
									,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
									,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,PL.Amount LCAmount,CON.ContractNo
									,ML.LCRef MasterLcNo

									from TRN.SalesMaterial SM 
									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join [Contract] CON on CON.Id=MOI.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId


									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo	ON TAxInfo.salesMaterialId=SM.Id
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
												Group by A.salesMaterialId
												) TAxInfo1	ON TAxInfo1.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.TaxCategoryType='SGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo2	ON TAxInfo2.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo3	ON TAxInfo3.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo4 ON TAxInfo4.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo5 ON TAxInfo5.salesMaterialId=SM.Id 


									--where SM.SalesId='MS2021596'
									Group BY SM.SalesId,PL.Amount ,CON.ContractNo
									,ML.LCRef 

									)SMD  ON SA.Id=SMD.SalesId

									left outer join PostSalesInvoice PSI on PSI.SalesId=SA.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=SA.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=SA.PartyId
									left outer join HKP.Party TA on TA.Id=SA.PartyId

									LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
									--LEFT JOIN TRN.Invoice I ON I.VoucherId=SA.VoucherId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
									Left JOIN [ORG].[Entity] E On E.id= SA.EntityId
									LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,VoucherId 
												FROM TRN.Invoice GROUP BY VoucherId) I ON I.VoucherId=SA.VoucherId

									LEFT JOIN (SELECT A.SalesId,A.BooksCurrencyTaxAmount BooksTaxAmount,TaxAmount TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												--Group BY A.SalesId				
									) TAxInfo6 ON TAxInfo6.SalesId=SA.Id 
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id

									WHERE SA.PlantId='" + identity.PlantId + @"' AND convert(Date,SA.InvoiceDate) BETWEEN '" + fromDate + @"' AND '" + toDate + @"'-- and sm.SalesId='202110'
									Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName
									,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code,E.UserName,SA.VoucherId,I.Amount,I.WrittenOffAmount,PSI.ExpDate,PSI.CNFBLAWB,PSI.CNFBLAWBDate 
									,PSI.ExFactoryDate,PSI.TransportDocRefNo
									,PSI.CNFContainerNo,PSI.CNFVesselTrackingNo,SMD.TransactionAmount

									,PTM.UserName ,SA.BaseOnDueDate,SA.BaseNoOfDays,SA.MatureDate,SA.EXPFromNo,SA.ComercialInvoiceNo
									,CNfA.UserName,TA.UserName 

									,SMD.LCAmount,SMD.ContractNo
									,SMD.MasterLcNo,PSI.TransportDocDate,SA.Narration
									,SMD.CGST
									,SMD.SGST
									,SMD.IGST
									,SMD.TDS
									,SMD.BooksCGST
									,SMD.BooksSGST
									,SMD.BooksIGST
									,SMD.NetAmount

									UNION ALL
									SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'InventorySales' SourceType
								--,IID.Id						
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								,II.ToCurrencyRate
								, II.DocRefNo
								,FORMAT(II.DocDate,'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code

								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								--,sum(SCr1.TaxAmount) TaxAmount
								--,0 NetAmount
								,v.VoucherNo VoucherId

								,'' AS Currency

								,'' SOType
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax

								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,'' 'NoteForAccounts'
								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt


								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								, InvoiceAmount=isnull(I.Amount,0)
								, RealizeAmount=isnull(I.WrittenOffAmount,0)

		, BalanceAmount=isnull(isnull(IID.TransactionAmount,0) -isnull(I.WrittenOffAmount,0),0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
									                                where XI.VoucherId=II.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,'' OwnReferenceNo
									,''ExpDate,''BLAWBNo,''BLAWBDate,''TransportDocDate
									,''CNFAgent
									,''TransportAgent

									,''ExFactoryDate
									,''CNFContainerNo,''CNFVesselTrackingNo

									,''PaymentTerm,''BaseOnDueDate
									,0 NoOfDays
									,''MatureDate
									,''EXPFromNo,''ComercialInvoiceNo		

									,0 LCAmount,''ContractNo
									,''MasterLcNo
								FROM [TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId
								,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate
								,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId
								,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0

								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
								Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
								Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
								Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
								Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
									LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,InventorySalesId 
												FROM TRN.Invoice GROUP BY InventorySalesId) I ON I.InventorySalesId=II.Id
								LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
								LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='CGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='IGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='SGST' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId
											) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TDS' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='VAT' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId

								) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.Id 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
										FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='AIT' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId

								) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
											FROM [TRN].InventorySalesAdditionalTax A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='TCS'
											GROUP BY A.InventorySalesId
								) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								WHERE II.PlantId='" + identity.PlantId + @"' AND convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
								GROUP BY p.Code,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,IID.TransactionAmount
								,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName 
								,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy,II.VoucherId,I.Amount,I.WrittenOffAmount";
						return _sqlRepository.GetDataTable(sql);
					}
					else
					{
						sql = @"SELECT 
									ROW_NUMBER() Over(Order by SA.Id) As[S.N]
									,SA.Id SalesId
									,SA.SourceType
									--SM.Id	
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate ,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
									--,SMD.SalesOrderId
									--,MO.Id MasterOrderId
									--,SO.Id SONo
									--,po.PONumber
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.ToCurrencyRate
									, SA.DocRefNo
									,'' DocDate
									, P.UserName AS PartyName,p.Code	
									--, '' HSNCode
									--,SM.BaseRate
									--,SM.BaseUoMFactor
									--,SM.TransactionRate
									--,SM.TransactionQty
									,Sum(SMD.TransactionAmount) TransactionAmount
									--,SM.TaxAmount
									--,SM.NetAmount
									,v.VoucherNo VoucherId
									--,BUoM.UserName AS BaseUoM
									--,TUoM.UserName AS TransactionUoM
									,CU.Code AS Currency
									--,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
									--,DT.UserName DestinationName
									,''SOType
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
									--TransactionAmount
									,'' Entity
									,'' CheckedByName
									,'' CheckedBy
									,'' ApprovedByName
									,'' ApprovedBy
									,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
									,'' 'NoteForAccounts'

									,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
									,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
									,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
									,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
									,round(isnull(TAxInfo6.TaxAmount,0),2) TCS

									,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
									,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
									,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt

									,SONumber=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, PONumber=STUFF((select distinct ','+CPO.PONumber
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, MasterOrder=STUFF((select distinct ','+MO.MasterOrderNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									FROM TRN.Sales AS SA
									LEFT JOIN (select Id, SalesId,SalesOrderId, Sum(TransactionAmount) TransactionAmount,Sum(NetAmount) NetAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from TRN.SalesMaterial Group BY SalesId,SalesOrderId,Id)SMD  ON SA.Id=SMD.SalesId
									--LEFT JOIN [TRN].[SalesOrder] AS SO ON SMD.SalesOrderId=SO.Id
									--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
									--LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
									--LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
									--LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId	
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo	ON TAxInfo.salesMaterialId=SMD.Id
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
												Group by A.salesMaterialId
												) TAxInfo1	ON TAxInfo1.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='SGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo2	ON TAxInfo2.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo3	ON TAxInfo3.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo4 ON TAxInfo4.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo5 ON TAxInfo5.salesMaterialId=SMD.Id 
									LEFT JOIN (SELECT A.SalesId,A.BooksCurrencyTaxAmount BooksTaxAmount,TaxAmount TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												--Group BY A.SalesId				
									) TAxInfo6 ON TAxInfo6.SalesId=SA.Id 
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id
									LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
									WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) <= '" + toDate + @"'-- and sm.SalesId='202110'
									Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code
								UNION ALL
								SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'InventorySales' SourceType
								--,IID.Id						
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								--,'' SalesOrderId
								--,'' MasterOrderId
								--,'' SONo
								--,'' PONumber
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								,II.ToCurrencyRate
								, II.DocRefNo
								,II.DocDate
								, P.UserName AS PartyName,p.Code
								--,MGM.UserName AS MaterialGroupMasterName
								--,MM.UserName MaterialMasterName
								--,ART.StandardName AS MaterialMasterArticleName
								--, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								--, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								--, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								--, '' HSNCode

								--,Sum(IID.PolicyRate) BaseRate
								--,0 BaseUoMFactor
								--,sum(IID.PolicyRate) TransactionRate
								--,Sum(IID.Qty) TransactionQty
								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								--,sum(SCr1.TaxAmount) TaxAmount
								--,0 NetAmount
								,v.VoucherNo VoucherId
								--,TUoM.UserName AS BaseUoM
								--,TUoM.UserName AS TransactionUoM
								,'' AS Currency
								--,'' DeliveryDate
								--,'' DestinationName
								,'' SOType
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax

								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,'' 'NoteForAccounts'

								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt


								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
								Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
								Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
								Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
								Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId

								LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
								LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='CGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='IGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='SGST' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId
											) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TDS' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='VAT' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId

								) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.Id 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
										FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.TaxCategoryType='AIT' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId

								) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
											FROM [TRN].InventorySalesAdditionalTax A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='TCS'
											GROUP BY A.InventorySalesId
								) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								WHERE II.PlantId='" + identity.PlantId + @"' AND convert(Date,II.SalesDate) <= '" + toDate + @"'
								GROUP BY p.Code	,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy";
						return _sqlRepository.GetDataTable(sql);
					}
				}

			}

			catch (Exception ex)
			{
				throw ex;
			}
		}

		public string NumberFormatZeroDecimal = "#,##0.00;(#,##0)";
		public string NumberFormatTwoDecimal = "#,##0.00;(#,##0.00)";
		public string NumberFormatFourDecimal = "#,####0.0000;(#,####0.0000)";
		[Authorize, HttpGet]
		private IWorkbook InventorySalesReportList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount,string Summery,bool WithTax)
		{

			//Start EmployeeAdvanceDueList
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				ExcelEngine excelEngine = new ExcelEngine();
				//Instantiate the Excel application object
				IApplication application = excelEngine.Excel;

				//Set the default application version
				application.DefaultVersion = ExcelVersion.Excel2013;

				//Load the existing Excel workbook into IWorkbook
				IWorkbook workbook = application.Workbooks.Create(1);

				//Get the first worksheet in the workbook into IWorksheet
				IWorksheet worksheet = workbook.Worksheets[0];
				DataTable dtInventorySalesReportList = GetInventorySalesReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, Qty, Amount,Summery);

				if (dtInventorySalesReportList.Rows.Count == 0)
					throw new Exception("No data found");
				// throw new Exception("To date must be above or equal to From Date.");



				worksheet.Name = Summery;

				var _rowd = 4;
				if (fromDate != "" && toDate != "")
				{

					worksheet.Range[_rowd, 3, _rowd, 6].Text = fromDate + " " + "To" + " " + toDate;
					worksheet.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Size = 8;
					worksheet.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Bold = false;
					worksheet.Range[_rowd, 3, _rowd, 6].Merge();
				}

				else
				{

					worksheet[_rowd, 4].Text = toDate;
					worksheet[_rowd, 4].CellStyle.Font.Size = 8;
					worksheet.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
					worksheet.Range[_rowd, 3, _rowd, 4].Merge();
					//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

				}

				var _rows = 5;
				worksheet.Range[_rows, 3, _rows, 6].Text = "Report Ref No: ";
				worksheet.Range[_rows, 3, _rows, 6].CellStyle.Font.Size = 8;
				worksheet.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
				worksheet.Range[_rows, 3, _rows, 6].Merge();
				worksheet.Range[_rows, 3, _rows, 6].CellStyle.Font.Bold = false;
				_rows++;



				int COL = 1; int ROW = 7;
				int startCol = COL;

				if(Summery=="Details")
				{
					worksheet[ROW, COL].Text = "SL";
					int colSL = COL;
					worksheet[ROW, COL].ColumnWidth = 5;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Id";
					int colId = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SourceType";
					int colSourceType = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Sales Invoice No.";
					int colSalesId = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Enrty Date";
					int colSalesDate = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Sales Invoice Date";
					int colInvoiceDate = COL; 
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Sales Order Id";
					int colSalesOrderId = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Master Order Id";
					int colMasterOrderId = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "SO No";
					int colSONO = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Customer PONo";
					int colPONo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Bill To";
					int colBillTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To Address";
					int colBillToAddress = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To State";
					int colBillToState = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Bill To GST No.";
					int colBillToGstNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To";
					int colShipTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To Address";
					int colShipToAddress = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ship To State";
					int colShipToState = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ship To GST No.";
					int colShipToGSTNo = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					
					worksheet[ROW, COL].Text = "Container No.";
					int colContainer = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transporter Name";
					int colTransporterName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transpoter Doc Ref No.";
					int colTranspoterDocRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transporter Doc Ref No. Date";
					int colTransporterDocRefDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Insurance Y/N";
					int colInsurance = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Agent Name";
					int colAgentName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Agent Commission %";
					int colAgentCommission = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					
					worksheet[ROW, COL].Text = "Doc Ref No";
					int colDocRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Doc Date";
					int colDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Customer Name";
					int colPartyName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Customer Code";
					int colPartyCode = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Material Group Master Name";
					int colMaterialGroupMasterName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Material Master Name";
					int colMaterialMasterName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Article Name";
					int colArticleName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SKU1";
					int colSKU1 = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SKU2";
					int colSKU2 = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SKU3";
					int colSKU3 = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//COL++;
					//worksheet[ROW, COL].Text = "HSN No";
					//int colHSNCode = COL;
					//worksheet[ROW, COL].ColumnWidth = 12;
					//worksheet[ROW, COL].CellStyle.Font.Bold = true;
					//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					//worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base Rate";
					int colBaseRate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base UoM Factor";
					int colBaseUoMFactor = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Transaction Rate";
					int colTransactionRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Gross Weight";
					int colGrossWeight = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LOT No";
					int colLOTNo = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Transaction Qty";
					int colTransactionQty = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transaction Amount";
					int colTransactionAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Tax Amount";
					int colTaxAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					int colCGST = 0;
					int colCGSTTax = 0;
					int colSGST = 0;
					int colSGSTTax = 0;
					int colIGST = 0;
					int colIGSTTax = 0;
					
					if (WithTax==true)
                    {

						worksheet[ROW, COL].Text = "CGST";
						 colCGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "CGST Tax (%)";
						 colCGSTTax = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;

						worksheet[ROW, COL].Text = "SGST";
						 colSGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "SGST Tax (%)";
						 colSGSTTax = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;

						worksheet[ROW, COL].Text = "IGST";
						 colIGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "IGST Tax (%)";
						 colIGSTTax = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;

					}
					worksheet[ROW, COL].Text = "Service Charge";
					int colServiceCharge = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Service Tax";
					int colServiceTax = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Net Amount";
					int colNetAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Voucher Detail Id";
					int colVoucherDetailId = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base UoM";
					int colBaseUoM = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transaction UoM";
					int colTransactionUoM = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Currency";
					int colCurrency = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "ToCurrency Rate";
					int colToCurrencyRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Delivery Date";
					int colDeliveryDate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Destination Name";
					int colDestinationName = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SO Type";
					int colSOType = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Entity";
					int colEntity = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Checked By Name";
					int colCheckedByName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Approved By Name";
					int colApprovedByName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Is Posted";
					int colPosted = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Note For Accounts";
					int colNoteForAccounts = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Contract";
					int colContract = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "MastrerLC Ref No";
					int colMastrerLCRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Commercial Invoice No";
					int colComercialInvoiceNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Expiry Date";
					int colExpiryDatet = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "BL/AWB No.";
					int colBLAWBNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "BL/AWB Date";
					int colBLAWBDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Payment Term";
					int colPaymentTerm = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base on Due Date";
					int colBaseOnDueDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "No Of Days";
					int colNoOfDays = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Mature Date";
					int colMatureDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LC Amount";
					int colLCAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "ExFactory Date";
					int colExFactoryDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Agent";
					int colTransportAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Doc Date";
					int colTransportDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "CNF Agent";
					int colCNFAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Container No.";
					int colContainerNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Vessel Tracking No.";
					int colVesselTrackingNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
				

					//worksheet[ROW, COL].Text = "Own Order Ref.";
					//int colOwnOrderRef = COL;
					//worksheet[ROW, COL].ColumnWidth = 30;
					//worksheet[ROW, COL].CellStyle.Font.Bold = true;
					//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					//worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//COL++;

					//worksheet[ROW, COL].Text = "Realize date";
					//int colRealizeDate = COL;
					//worksheet[ROW, COL].ColumnWidth = 30;
					//worksheet[ROW, COL].CellStyle.Font.Bold = true;
					//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					//worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//COL++;

					//worksheet[ROW, COL].Text = "Realize amount";
					//int colRealizeAmount = COL;
					//worksheet[ROW, COL].ColumnWidth = 30;
					//worksheet[ROW, COL].CellStyle.Font.Bold = true;
					//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					//worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//COL++;

					//worksheet[ROW, COL].Text = "Balance";
					//int colBalance = COL;
					//worksheet[ROW, COL].ColumnWidth = 30;
					//worksheet[ROW, COL].CellStyle.Font.Bold = true;
					//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					//worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;


					int endCol = COL;
					worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
					worksheet.Range[ROW, 1, ROW, endCol].RowHeight = 22;
					worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
					worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
					worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
					ROW++;
					try
					{
						if (Summery == "Details")
						{
							for (int i = 0; i < dtInventorySalesReportList.Rows.Count; i++)
							{

								// int i = 0; i < dtMasterOrderItem.Rows.Count; i++
								worksheet[ROW, colSL].Text = dtInventorySalesReportList.Rows[i]["S.N"].ToString();
								worksheet[ROW, colId].Text = dtInventorySalesReportList.Rows[i]["Id"].ToString();
								worksheet[ROW, colSourceType].Text = dtInventorySalesReportList.Rows[i]["SourceType"].ToString();

								worksheet[ROW, colSalesId].Text = dtInventorySalesReportList.Rows[i]["SalesId"].ToString();
								worksheet[ROW, colSalesDate].Text = dtInventorySalesReportList.Rows[i]["SalesDate"].ToString();
								worksheet[ROW, colInvoiceDate].Text = dtInventorySalesReportList.Rows[i]["InvoiceDate"].ToString();
								
								worksheet[ROW, colSalesOrderId].Text = dtInventorySalesReportList.Rows[i]["SalesOrderId"].ToString();
								worksheet[ROW, colMasterOrderId].Text = dtInventorySalesReportList.Rows[i]["MasterOrderId"].ToString();
								worksheet[ROW, colSONO].Text = dtInventorySalesReportList.Rows[i]["SONo"].ToString();
								worksheet[ROW, colPONo].Text = dtInventorySalesReportList.Rows[i]["PONumber"].ToString();
								worksheet[ROW, colBillTo].Text = dtInventorySalesReportList.Rows[i]["BillTo"].ToString();
								worksheet[ROW, colBillToAddress].Text = dtInventorySalesReportList.Rows[i]["BillToAddress"].ToString();
								worksheet[ROW, colBillToState].Text = dtInventorySalesReportList.Rows[i]["BillToState"].ToString();
								worksheet[ROW, colBillToGstNo].Text = dtInventorySalesReportList.Rows[i]["BillToGSTNo"].ToString();
								worksheet[ROW, colShipTo].Text = dtInventorySalesReportList.Rows[i]["ShipTo"].ToString();
								worksheet[ROW, colShipToAddress].Text = dtInventorySalesReportList.Rows[i]["ShipToAddress"].ToString();
								worksheet[ROW, colShipToState].Text = dtInventorySalesReportList.Rows[i]["ShipToState"].ToString();
								worksheet[ROW, colShipToGSTNo].Text = dtInventorySalesReportList.Rows[i]["ShipToGSTNo"].ToString();
								
								worksheet[ROW, colToCurrencyRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ToCurrencyRate"].ToString());
								worksheet[ROW, colDocRefNo].Text = dtInventorySalesReportList.Rows[i]["DocRefNo"].ToString();
								worksheet[ROW, colDocDate].Text = dtInventorySalesReportList.Rows[i]["DocDate"].ToString();
								worksheet[ROW, colPartyName].Text = dtInventorySalesReportList.Rows[i]["PartyName"].ToString();
								worksheet[ROW, colPartyCode].Text = dtInventorySalesReportList.Rows[i]["Code"].ToString();
								worksheet[ROW, colMaterialGroupMasterName].Text = dtInventorySalesReportList.Rows[i]["MaterialGroupMasterName"].ToString();
								worksheet[ROW, colMaterialMasterName].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterName"].ToString();
								//worksheet[ROW, colMaterialMasterId].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterId"].ToString();
								worksheet[ROW, colArticleName].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterArticleName"].ToString();
								worksheet[ROW, colSKU1].Text = dtInventorySalesReportList.Rows[i]["FirstCharacteristicsValue"].ToString();
								worksheet[ROW, colSKU2].Text = dtInventorySalesReportList.Rows[i]["SecondCharacteristicsValue"].ToString();
								worksheet[ROW, colSKU3].Text = dtInventorySalesReportList.Rows[i]["ThirdCharacteristicsValue"].ToString();
								//worksheet[ROW, colHSNCode].Text = dtInventorySalesReportList.Rows[i]["HSNCode"].ToString();
								worksheet[ROW, colBaseRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BaseRate"].ToString());
								worksheet.Range[ROW, colBaseRate].NumberFormat = NumberFormatFourDecimal;
								worksheet[ROW, colBaseUoMFactor].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BaseUoMFactor"].ToString());
								worksheet.Range[ROW, colBaseUoMFactor].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colTransactionRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionRate"].ToString());
								worksheet.Range[ROW, colTransactionRate].NumberFormat = NumberFormatFourDecimal;
								worksheet[ROW, colTransactionQty].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionQty"].ToString());
								worksheet.Range[ROW, colTransactionQty].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionAmount"].ToString());
								worksheet.Range[ROW, colTransactionAmount].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colTaxAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TaxAmount"].ToString());
								worksheet.Range[ROW, colTaxAmount].NumberFormat = NumberFormatTwoDecimal;

                                if (WithTax==true)
                                {
									worksheet[ROW, colCGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["CGST"].ToString());
									worksheet.Range[ROW, colCGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colCGSTTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["CGSTTaxPercentage"].ToString());
									worksheet.Range[ROW, colCGSTTax].NumberFormat = NumberFormatFourDecimal;
									worksheet[ROW, colSGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["SGST"].ToString());
									worksheet.Range[ROW, colSGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colSGSTTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["SGSTTaxPercentage"].ToString());
									worksheet.Range[ROW, colSGSTTax].NumberFormat = NumberFormatFourDecimal;
									worksheet[ROW, colIGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["IGST"].ToString());
									worksheet.Range[ROW, colIGST].NumberFormat = NumberFormatFourDecimal;
									worksheet[ROW, colIGSTTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["IGSTTaxPercentage"].ToString());
									worksheet.Range[ROW, colIGSTTax].NumberFormat = NumberFormatFourDecimal;

								}								
								//worksheet[ROW, colTDS].Text = dtInventorySalesReportList.Rows[i]["TDS"].ToString();
								//worksheet[ROW, colTDSTax].Text = dtInventorySalesReportList.Rows[i]["TDSTaxPercentage"].ToString();
								//worksheet[ROW, colTCS].Text = dtInventorySalesReportList.Rows[i]["TCS"].ToString();
								//worksheet[ROW, colTCSTax].Text = dtInventorySalesReportList.Rows[i]["TCSTaxPercentage"].ToString(); 
								worksheet[ROW, colServiceCharge].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceCharge"].ToString());
								worksheet.Range[ROW, colServiceCharge].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colServiceTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceTax"].ToString());
								worksheet.Range[ROW, colServiceTax].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colNetAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["NetAmount"].ToString());
								worksheet.Range[ROW, colNetAmount].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colVoucherDetailId].Text = dtInventorySalesReportList.Rows[i]["VoucherDetailId"].ToString();
								worksheet[ROW, colBaseUoM].Text = dtInventorySalesReportList.Rows[i]["BaseUoM"].ToString();
								worksheet[ROW, colTransactionUoM].Text = dtInventorySalesReportList.Rows[i]["TransactionUoM"].ToString();

								worksheet[ROW, colCurrency].Text = dtInventorySalesReportList.Rows[i]["Currency"].ToString();
								worksheet[ROW, colDeliveryDate].Text = dtInventorySalesReportList.Rows[i]["DeliveryDate"].ToString();
								worksheet[ROW, colDestinationName].Text = dtInventorySalesReportList.Rows[i]["DestinationName"].ToString();
								worksheet[ROW, colSOType].Text = dtInventorySalesReportList.Rows[i]["SOType"].ToString();

								worksheet[ROW, colEntity].Text = dtInventorySalesReportList.Rows[i]["Entity"].ToString();
								worksheet[ROW, colCheckedByName].Text = dtInventorySalesReportList.Rows[i]["CheckedByName"].ToString();
								worksheet[ROW, colApprovedByName].Text = dtInventorySalesReportList.Rows[i]["ApprovedByName"].ToString();
								worksheet[ROW, colPosted].Text = dtInventorySalesReportList.Rows[i]["Posted"].ToString();
								worksheet[ROW, colNoteForAccounts].Text = dtInventorySalesReportList.Rows[i]["NoteForAccounts"].ToString();
								worksheet[ROW, colContainer].Text = dtInventorySalesReportList.Rows[i]["ContainerNo"].ToString();
								worksheet[ROW, colTransporterName].Text = dtInventorySalesReportList.Rows[i]["TransporterName"].ToString();
								worksheet[ROW, colTranspoterDocRefNo].Text = dtInventorySalesReportList.Rows[i]["TransportDocRefNo"].ToString();
								worksheet[ROW, colTransporterDocRefDate].Text = dtInventorySalesReportList.Rows[i]["TransportDocDate"].ToString();
								worksheet[ROW, colAgentName].Text = dtInventorySalesReportList.Rows[i]["AgentName"].ToString();
								worksheet[ROW, colAgentCommission].Text = dtInventorySalesReportList.Rows[i]["AgentCommission"].ToString();
								worksheet[ROW, colInsurance].Text = dtInventorySalesReportList.Rows[i]["Insurance"].ToString();
								worksheet[ROW, colGrossWeight].Text = dtInventorySalesReportList.Rows[i]["GrossWeight"].ToString();
								worksheet[ROW, colLOTNo].Text = dtInventorySalesReportList.Rows[i]["LoTNo"].ToString();


								////worksheet[ROW, colNoteForAccounts].Text = dtInventorySalesReportList.Rows[i]["NoteForAccounts"].ToString();
								//worksheet[ROW, colRealizeAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["RealizeAmount"].ToString());
								//worksheet.Range[ROW, colRealizeAmount].NumberFormat = NumberFormatTwoDecimal;

								//worksheet[ROW, colRealizeDate].Text = dtInventorySalesReportList.Rows[i]["RealizeDate"].ToString();
								//worksheet[ROW, colBalance].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BalanceAmount"].ToString());
								//worksheet.Range[ROW, colBalance].NumberFormat = NumberFormatTwoDecimal;

								//worksheet[ROW, colOwnOrderRef].Text = dtInventorySalesReportList.Rows[i]["OwnReferenceNo"].ToString();
								worksheet[ROW, colContract].Text = dtInventorySalesReportList.Rows[i]["ContractNo"].ToString();
								worksheet[ROW, colMastrerLCRefNo].Text = dtInventorySalesReportList.Rows[i]["MasterLcNo"].ToString();
								worksheet[ROW, colComercialInvoiceNo].Text = dtInventorySalesReportList.Rows[i]["ComercialInvoiceNo"].ToString();
								worksheet[ROW, colExpiryDatet].Text = dtInventorySalesReportList.Rows[i]["ExpiryDate"].ToString();
								worksheet[ROW, colBLAWBNo].Text = dtInventorySalesReportList.Rows[i]["BLAWBNo"].ToString();
								worksheet[ROW, colBLAWBDate].Text = dtInventorySalesReportList.Rows[i]["BLAWBDate"].ToString();
								worksheet[ROW, colPaymentTerm].Text = dtInventorySalesReportList.Rows[i]["PaymentTerm"].ToString();
								worksheet[ROW, colBaseOnDueDate].Text = dtInventorySalesReportList.Rows[i]["BaseOnDueDate"].ToString();
								worksheet[ROW, colNoOfDays].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["NoOfDays"].ToString());
								worksheet[ROW, colNoOfDays].NumberFormat = NumberFormatZeroDecimal;
								worksheet[ROW, colMatureDate].Text = dtInventorySalesReportList.Rows[i]["MatureDate"].ToString();
								worksheet[ROW, colLCAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["LCAmount"].ToString());
								worksheet[ROW, colExFactoryDate].Text = dtInventorySalesReportList.Rows[i]["ExFactoryDate"].ToString();
								worksheet[ROW, colTransportAgent].Text = dtInventorySalesReportList.Rows[i]["TransportAgent"].ToString();
								worksheet[ROW, colTransportDocDate].Text = dtInventorySalesReportList.Rows[i]["TransportDocDate"].ToString();
								worksheet[ROW, colCNFAgent].Text = dtInventorySalesReportList.Rows[i]["CNFAgent"].ToString();
								worksheet[ROW, colContainerNo].Text = dtInventorySalesReportList.Rows[i]["CNFContainerNo"].ToString();
								worksheet[ROW, colVesselTrackingNo].Text = dtInventorySalesReportList.Rows[i]["CNFVesselTrackingNo"].ToString();


								worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
								worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
								worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
								ROW++;
							}
							worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
							//worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
							worksheet["A" + 7].FreezePanes();
							ReportUtility reportUtility = new ReportUtility();
							reportUtility.PlantHeader(ref worksheet, endCol, " Sales Register", identity.PlantId);
							reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
							worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
							// worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
							worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

						}
					}
					catch (Exception ex)
					{

						throw ex;
					}
				}
				else
				{
					worksheet[ROW, COL].Text = "SL";
					int colSL = COL;
					worksheet[ROW, COL].ColumnWidth = 5;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Id";
					int colId = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SourceType";
					int colSourceType = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;					
					worksheet[ROW, COL].Text = "Entry Date";
					int colSalesDate = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Invloice Date";
					int colInvoiceDate = COL;  
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Bill To";
					int colBillTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To";
					int colShipTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Doc Ref No";
					int colDocRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Doc Date";
					int colDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;	

					worksheet[ROW, COL].Text = "Customer Name";
					int colPartyName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Customer Code";
					int colPartyCode = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Customer PO Number";
					int colCustomerPONumber = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Master Order Number";
					int colMasterOrderNumber = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Sales Order Number";
					int colSalesOrderNumber = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Tran. Currency";
					int colCurrency = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Exchange Rate";
					int colToCurrencyRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Mat.Amt";
					int colMatAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Serv. Amt";
					int colServAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ttl. Taxable Amt.";
					int colTransactionAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					int colCGST = 0;
					int colSGST = 0;
					int colIGST = 0;
					int colTCS = 0;
					int colBooksCGST = 0;
					int colBooksSGST = 0;
					int colBooksIGST = 0;
					int colBooksTCS = 0;



					if (WithTax==true)
                    {
						worksheet[ROW, COL].Text = "CGST";
						 colCGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;

						worksheet[ROW, COL].Text = "SGST";
						 colSGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "IGST";
						 colIGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "TCS";
						 colTCS = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL ++;
					}
					
					worksheet[ROW, COL].Text = "Books Mat.Amt";
					int colBooksMatAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Books Serv. Amt";
					int colBooksServAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Books Ttl. Taxable Amt.";
					int colBooksTtlTaxableAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

                    if (WithTax==true)
                    {
						worksheet[ROW, COL].Text = "Books CGST";
						colBooksCGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books SGST";
						colBooksSGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books IGST";
						colBooksIGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books TCS";
						colBooksTCS = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
					}

					worksheet[ROW, COL].Text = "VoucherNo";
					int colVoucherDetailId = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;		
				

					worksheet[ROW, COL].Text = "Entity";
					int colEntity = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Checked By Name";
					int colCheckedByName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Approved By Name";
					int colApprovedByName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Is Posted";
					int colPosted = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Note For Accounts";
					int colNoteForAccounts = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Contract";
					int colContract = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "MastrerLC Ref No";
					int colMastrerLCRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Commercial Invoice No";
					int colComercialInvoiceNo= COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Expiry Date";
					int colExpiryDatet = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "BL/AWB No.";
					int colBLAWBNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "BL/AWB Date";
					int colBLAWBDate= COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Payment Term";
					int colPaymentTerm = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base on Due Date";
					int colBaseOnDueDate= COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "No Of Days";
					int colNoOfDays = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Mature Date";
					int colMatureDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LC Amount";
					int colLCAmount= COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "ExFactory Date";
					int colExFactoryDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Agent";
					int colTransportAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Doc Date";
					int colTransportDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "CNF Agent";
					int colCNFAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Container No.";
					int colContainerNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Vessel Tracking No.";
					int colVesselTrackingNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Own Order Ref.";
					int colOwnOrderRef = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Realize date";
					int colRealizeDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Realize amount";
					int colRealizeAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Balance";
					int colBalance = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;

					//COL++;

					int endCol = COL;
					worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
					worksheet.Range[ROW, 1, ROW, endCol].RowHeight = 22;
					worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
					worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
					worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
					ROW++;
					try
					{
						if (Summery == "Summery")
						{ 
							for (int i = 0; i < dtInventorySalesReportList.Rows.Count; i++)
							{

								// int i = 0; i < dtMasterOrderItem.Rows.Count; i++
								worksheet[ROW, colSL].Text = dtInventorySalesReportList.Rows[i]["S.N"].ToString();
								worksheet[ROW, colId].Text = dtInventorySalesReportList.Rows[i]["SalesId"].ToString();
								worksheet[ROW, colSourceType].Text = dtInventorySalesReportList.Rows[i]["SourceType"].ToString();

								worksheet[ROW, colSalesDate].Text = dtInventorySalesReportList.Rows[i]["SalesDate"].ToString();
								worksheet[ROW, colInvoiceDate].Text = dtInventorySalesReportList.Rows[i]["InvoiceDate"].ToString();
								worksheet[ROW, colBillTo].Text = dtInventorySalesReportList.Rows[i]["BillTo"].ToString();
								worksheet[ROW, colShipTo].Text = dtInventorySalesReportList.Rows[i]["ShipTo"].ToString();
								worksheet[ROW, colToCurrencyRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ToCurrencyRate"].ToString());
								worksheet[ROW, colToCurrencyRate].NumberFormat = NumberFormatFourDecimal;

								worksheet[ROW, colDocRefNo].Text = dtInventorySalesReportList.Rows[i]["DocRefNo"].ToString();
                                worksheet[ROW, colDocDate].Text = dtInventorySalesReportList.Rows[i]["DocDate"].ToString();
                                worksheet[ROW, colCustomerPONumber].Text = dtInventorySalesReportList.Rows[i]["PONumber"].ToString();
								worksheet[ROW, colMasterOrderNumber].Text = dtInventorySalesReportList.Rows[i]["MasterOrder"].ToString();
								worksheet[ROW, colSalesOrderNumber].Text = dtInventorySalesReportList.Rows[i]["SONumber"].ToString();
								worksheet[ROW, colPartyName].Text = dtInventorySalesReportList.Rows[i]["PartyName"].ToString();

								worksheet[ROW, colPartyCode].Text = dtInventorySalesReportList.Rows[i]["Code"].ToString();

								worksheet[ROW, colCurrency].Text = dtInventorySalesReportList.Rows[i]["Currency"].ToString();
							

								//worksheet[ROW, colBaseRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BaseRate"].ToString());
								//worksheet.Range[ROW, colBaseRate].NumberFormat = NumberFormatFourDecimal;
								//worksheet[ROW, colBaseUoMFactor].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BaseUoMFactor"].ToString());
								//worksheet.Range[ROW, colBaseUoMFactor].NumberFormat = NumberFormatTwoDecimal;
								//worksheet[ROW, colTransactionRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionRate"].ToString());
								//worksheet.Range[ROW, colTransactionRate].NumberFormat = NumberFormatTwoDecimal;
								//worksheet[ROW, colTransactionQty].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionQty"].ToString());
								//worksheet.Range[ROW, colTransactionQty].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colMatAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionAmount"].ToString());
								worksheet.Range[ROW, colMatAmt].NumberFormat = NumberFormatTwoDecimal;
								//worksheet[ROW, colTaxAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TaxAmount"].ToString());
								//worksheet.Range[ROW, colTaxAmount].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colServAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceCharge"].ToString());
								worksheet.Range[ROW, colServAmt].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TotalTaxableAmt"].ToString());
								worksheet.Range[ROW, colTransactionAmount].NumberFormat = NumberFormatTwoDecimal;
                                if (WithTax==true)
                                {
									worksheet[ROW, colCGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["CGST"].ToString());
									worksheet.Range[ROW, colCGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colSGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["SGST"].ToString());
									worksheet.Range[ROW, colSGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colIGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["IGST"].ToString());
									worksheet.Range[ROW, colIGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colTCS].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TCS"].ToString());
									worksheet.Range[ROW, colTCS].NumberFormat = NumberFormatTwoDecimal;

									worksheet[ROW, colBooksCGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksCGST"].ToString());
									worksheet.Range[ROW, colBooksCGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colBooksSGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksSGST"].ToString());
									worksheet.Range[ROW, colBooksSGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colBooksIGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksIGST"].ToString());
									worksheet.Range[ROW, colBooksIGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colBooksTCS].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksTCS"].ToString());
									worksheet.Range[ROW, colBooksTCS].NumberFormat = NumberFormatTwoDecimal;
								}
								worksheet[ROW, colBooksMatAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksCurrencyTransactionAmount"].ToString());
								worksheet.Range[ROW, colBooksMatAmt].NumberFormat = NumberFormatTwoDecimal;								
								worksheet[ROW, colBooksServAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksServiceCharge"].ToString());
								worksheet.Range[ROW, colBooksServAmt].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colBooksTtlTaxableAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksTotalTaxableAmt"].ToString());
								worksheet.Range[ROW, colBooksTtlTaxableAmt].NumberFormat = NumberFormatTwoDecimal;

								//worksheet[ROW, colServiceTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceTax"].ToString());
								//worksheet.Range[ROW, colServiceTax].NumberFormat = NumberFormatTwoDecimal;								
								//worksheet[ROW, colBooksCurrencyTransactionAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksCurrencyTransactionAmount"].ToString());
								//worksheet.Range[ROW, colBooksCurrencyTransactionAmount].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colVoucherDetailId].Text = dtInventorySalesReportList.Rows[i]["VoucherId"].ToString();



								//worksheet[ROW, colSOType].Text = dtInventorySalesReportList.Rows[i]["SOType"].ToString();

								worksheet[ROW, colEntity].Text = dtInventorySalesReportList.Rows[i]["Entity"].ToString();
								worksheet[ROW, colCheckedByName].Text = dtInventorySalesReportList.Rows[i]["CheckedByName"].ToString();
								worksheet[ROW, colApprovedByName].Text = dtInventorySalesReportList.Rows[i]["ApprovedByName"].ToString();
								worksheet[ROW, colPosted].Text = dtInventorySalesReportList.Rows[i]["Posted"].ToString();
								worksheet[ROW, colNoteForAccounts].Text = dtInventorySalesReportList.Rows[i]["NoteForAccounts"].ToString();
								worksheet[ROW, colRealizeAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["RealizeAmount"].ToString());
								worksheet.Range[ROW, colRealizeAmount].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colRealizeDate].Text = dtInventorySalesReportList.Rows[i]["RealizeDate"].ToString();
								worksheet[ROW, colBalance].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BalanceAmount"].ToString());
								worksheet.Range[ROW, colBalance].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colOwnOrderRef].Text = dtInventorySalesReportList.Rows[i]["OwnReferenceNo"].ToString();
								worksheet[ROW, colContract].Text = dtInventorySalesReportList.Rows[i]["ContractNo"].ToString();
								worksheet[ROW, colMastrerLCRefNo].Text = dtInventorySalesReportList.Rows[i]["MasterLcNo"].ToString();
								worksheet[ROW, colComercialInvoiceNo].Text = dtInventorySalesReportList.Rows[i]["ComercialInvoiceNo"].ToString();
								worksheet[ROW, colExpiryDatet].Text = dtInventorySalesReportList.Rows[i]["ExpDate"].ToString();
								worksheet[ROW, colBLAWBNo].Text = dtInventorySalesReportList.Rows[i]["BLAWBNo"].ToString();
								worksheet[ROW, colBLAWBDate].Text = dtInventorySalesReportList.Rows[i]["BLAWBDate"].ToString();
								worksheet[ROW, colPaymentTerm].Text = dtInventorySalesReportList.Rows[i]["PaymentTerm"].ToString();
								worksheet[ROW, colBaseOnDueDate].Text = dtInventorySalesReportList.Rows[i]["BaseOnDueDate"].ToString();
								worksheet[ROW, colNoOfDays].Number = clsStaticInfo.dbl( dtInventorySalesReportList.Rows[i]["NoOfDays"].ToString());
								worksheet[ROW, colNoOfDays].NumberFormat = NumberFormatZeroDecimal;
								worksheet[ROW, colMatureDate].Text = dtInventorySalesReportList.Rows[i]["MatureDate"].ToString();
								worksheet[ROW, colLCAmount].Number =clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["LCAmount"].ToString());
								worksheet[ROW, colExFactoryDate].Text = dtInventorySalesReportList.Rows[i]["ExFactoryDate"].ToString();
								worksheet[ROW, colTransportAgent].Text = dtInventorySalesReportList.Rows[i]["TransportAgent"].ToString();
								worksheet[ROW, colTransportDocDate].Text = dtInventorySalesReportList.Rows[i]["TransportDocDate"].ToString();
								worksheet[ROW, colCNFAgent].Text = dtInventorySalesReportList.Rows[i]["CNFAgent"].ToString();
								worksheet[ROW, colContainerNo].Text = dtInventorySalesReportList.Rows[i]["CNFContainerNo"].ToString();
								worksheet[ROW, colVesselTrackingNo].Text = dtInventorySalesReportList.Rows[i]["CNFVesselTrackingNo"].ToString();
								worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
								worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
								worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
								ROW++;
							}

							worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
							//worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
							worksheet["A" + 7].FreezePanes();
							ReportUtility reportUtility = new ReportUtility();
							reportUtility.PlantHeader(ref worksheet, endCol, " Sales Register", identity.PlantId);
							reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
							worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
							// worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
							worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

						}
					}
					catch (Exception ex)
					{
						throw ex;
					}
				}

				//}

				worksheet.UsedRange.CellStyle.Font.FontName = "Tahoma";
				//worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
				worksheet.IsGridLinesVisible = false;
				//worksheet.UsedRange.CellStyle.Font.Size = 8;
				#region Freeze Panes

				worksheet.IsDisplayZeros = false;
				worksheet.UsedRange["A8"].FreezePanes();
				worksheet.FirstVisibleColumn = 1;
				//worksheet.FirstVisibleRow = 8;

				#endregion Freeze Panes


				return workbook;
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}
		[HttpPost, Authorize]
		public JsonResult DeleteSalesDetail(string issueDetailId)
		{
			_inventoryIssueService.DeleteSalesDetail(issueDetailId);
			return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
		public JsonResult SalesServiceChargesCreate(InventoryMaterialViewModel entity, IEnumerable<InventorySalesTax> taxCategoryList)
		{
			//if(entity != null)            {

			//        if (entity.Amount == 0)
			//            throw new CustomException("Enter Service Amount!");                  

			//}
			_inventoryIssueService.InsertGraph(entity, taxCategoryList);
			return Json(new { entity.Id, Message = AplosMessage.Success });
		}

		[Authorize, HttpGet]
		public JsonResult GetServiceChargeList(string receiveId)
		{
			try
			{

				var sql = @"SELECT A.Id, A.InventorySalesId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.Amount
                            --, A.TotalTaxAmount
                            ,POT.TaxAmount As TotalTaxAmount
                            --,TaxAmount
                            ,null ChargeTaxList
                            ,A.Description 
                            FROM 
                            [TRN].[InventorySalesService] AS A 
                            INner JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                            left JOIN (select InventorySalesServiceId,Sum(TaxAmount) as TaxAmount  from TRN.InventorySalesTax group by InventorySalesServiceId) AS POT on A.id=POT.InventorySalesServiceId
                            WHERE A.InventorySalesId='" + receiveId + "'";
				//return _sqlRepository.GetDataCollection(sql);
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		[Authorize, HttpGet]
		public JsonResult GetServiceTaxList(string serviceId)
		{
			try
			{
				var sql = @"SELECT A.Id,A.InventorySalesServiceId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM [TRN].[InventorySalesTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.InventorySalesId='" + serviceId + "' AND A.InventoryReceiveDetailId IS NULL ORDER BY TC.[Sequence]";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		[Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
		public JsonResult ServiceChargesDelete(string serviceId)
		{
			_inventoryIssueService.ServiceChargesDelete(serviceId);
			return Json(new { Message = AplosMessage.Deleted });
		}
		#endregion

		#region Inventory Sales Checked And Approved------------------------

		#region Gate Pass Checked And Approved
		[Authorize, HttpGet]
		public JsonResult GetCheckedApprovedList(string tabType)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";


				if (tabType == "UnCheckedList")
				{
					sql = @"
                        SELECT E.UserName AS Entity 
                        ,isnull(II.IssueType,'') issuetype
                        , II.Id, II.CompanyGroupId
                        , II.CompanyId, II.PlantId
                        , II.EntityId, II.MaterialStorageId
                        ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                        , MS.UserName AS MaterialStorage 
                        ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                       ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                        ,II.Remarks,II.Id AS IssueId
                        ,II.OrderRefNo
                        ,EI1.EmployeeName CheckedByName
                        ,II.CheckedByStatus
                        ,EI2.EmployeeName ApprovedByName
                        ,II.ApprovedByStatus
						,P.UserName CustomerName
						,P.Code CustomerCode
                        FROM[TRN].[InventorySales] AS II
                         left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                        left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                        left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                        Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                        left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                        left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                        WHERE II.CheckedBy='" + identity.EmployeeId + @"' AND II.CheckedByStatus ='For Checking' 
                        AND II.ApprovedByStatus IS NULL
                        
                        AND ISNULL(II.[Status],'') <>'Posting' 

                        GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus
	,P.UserName,P.Code";
				}
				else if (tabType == "HoldRejectCheckedList")
				{
					sql = @"SELECT E.UserName AS Entity 
                            ,isnull(II.IssueType,'') issuetype
                            , II.Id, II.CompanyGroupId
                            , II.CompanyId, II.PlantId
                            , II.EntityId, II.MaterialStorageId
                            ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                            , MS.UserName AS MaterialStorage 
                            ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                        ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                            ,II.Remarks,II.Id AS IssueId
                            ,II.OrderRefNo
                            ,EI1.EmployeeName CheckedByName
                            ,II.CheckedByStatus
                            ,EI2.EmployeeName ApprovedByName
                            ,II.ApprovedByStatus
							,P.UserName CustomerName
						    ,P.Code CustomerCode
                            FROM[TRN].[InventorySales] AS II
                            left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                            left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                            left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                            left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                            WHERE II.CheckedByStatus ='Hold' OR II.CheckedByStatus ='Reject' 
                            AND II.ApprovedByStatus IS NULL
                            AND II.CheckedBy='" + identity.EmployeeId + @"'
                            AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus 
							,P.UserName,P.Code
							";
				}
				else if (tabType == "CheckedList")
				{
					sql = @"SELECT E.UserName AS Entity 
                            ,isnull(II.IssueType,'') issuetype
                            , II.Id, II.CompanyGroupId
                            , II.CompanyId, II.PlantId
                            , II.EntityId, II.MaterialStorageId
                            ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                            , MS.UserName AS MaterialStorage 
                            ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                         ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                            ,II.Remarks,II.Id AS IssueId
                            ,II.OrderRefNo
                            ,EI1.EmployeeName CheckedByName
                            ,II.CheckedByStatus
                            ,EI2.EmployeeName ApprovedByName
                            ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                            FROM[TRN].[InventorySales] AS II
                            left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                            left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                            left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                            left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                            WHERE   II.CheckedBy= '" + identity.EmployeeId + @"'  
                            AND II.CheckedByStatus ='Checked' 
                            AND II.ApprovedByStatus= 'For Approval'                            
                            AND ISNULL(II.[Status],'') <>'Posting' 
                           GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus 
,P.UserName,P.Code
							";
				}
				else if (tabType == "UnApprovedList")
				{
					sql = @"SELECT * FROM(
                                SELECT E.UserName AS Entity 
                                ,isnull(II.IssueType,'') issuetype
                                , II.Id, II.CompanyGroupId
                                , II.CompanyId, II.PlantId
                                , II.EntityId, II.MaterialStorageId
                                ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                                , MS.UserName AS MaterialStorage 
                                ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                          ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount  
                                ,II.Remarks,II.Id AS IssueId
                                ,II.OrderRefNo
                                ,EI1.EmployeeName CheckedByName
                                ,II.CheckedByStatus
                                ,EI2.EmployeeName ApprovedByName
                                ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                                FROM[TRN].[InventorySales] AS II
                                left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                                left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                                left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                                WHERE   II.ApprovedBy= '" + identity.EmployeeId + @"' 
                                AND II.CheckedByStatus ='Checked' 
                                AND II.ApprovedByStatus ='For Approval'                                
                                AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus
							,P.UserName,P.Code

					UNION ALL

                                SELECT E.UserName AS Entity 
                                ,isnull(II.IssueType,'') issuetype
                                , II.Id, II.CompanyGroupId
                                , II.CompanyId, II.PlantId
                                , II.EntityId, II.MaterialStorageId
                                ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                                , MS.UserName AS MaterialStorage 
                                ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                            ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                                ,II.Remarks,II.Id AS IssueId
                                ,II.OrderRefNo
                                ,EI1.EmployeeName CheckedByName
                                ,II.CheckedByStatus
                                ,EI2.EmployeeName ApprovedByName
                                ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                                FROM[TRN].[InventorySales] AS II
                                left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                                left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                                left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                                WHERE  II.ApprovedBy= '" + identity.EmployeeId + @"' 
                                AND II.CheckedByStatus IS NULL
                                AND II.ApprovedByStatus ='For Approval'                               
                                AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus
							,P.UserName,P.Code
                                )X
                                Order BY IssueDate DESC";
				}
				else if (tabType == "HoldRejectApprovedList")
				{
					sql = @"SELECT E.UserName AS Entity 
                            ,isnull(II.IssueType,'') issuetype
                            , II.Id, II.CompanyGroupId
                            , II.CompanyId, II.PlantId
                            , II.EntityId, II.MaterialStorageId
                            ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                            , MS.UserName AS MaterialStorage 
                            ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                            ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                            ,II.Remarks,II.Id AS IssueId
                            ,II.OrderRefNo
                            ,EI1.EmployeeName CheckedByName
                            ,II.CheckedByStatus
                            ,EI2.EmployeeName ApprovedByName
                            ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                            FROM[TRN].[InventorySales] AS II
                             left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                            left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                            left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                            left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                            WHERE  II.ApprovedBy= '" + identity.EmployeeId + @"' 
                            AND II.CheckedByStatus ='Checked' 
                            AND (II.ApprovedByStatus ='Hold' OR II.ApprovedByStatus ='Reject')                             
                            AND ISNULL(II.[Status],'') <>'Posting' 
                            GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus,P.UserName,P.Code							";
				}
				else if (tabType == "ApprovedList")
				{
					sql = @"SELECT E.UserName AS Entity 
                            ,isnull(II.IssueType,'') issuetype
                            , II.Id, II.CompanyGroupId
                            , II.CompanyId, II.PlantId
                            , II.EntityId, II.MaterialStorageId
                            ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                            , MS.UserName AS MaterialStorage 
                            ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                          ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                            ,II.Remarks,II.Id AS IssueId
                            ,II.OrderRefNo
                            ,EI1.EmployeeName CheckedByName
                            ,II.CheckedByStatus
                            ,EI2.EmployeeName ApprovedByName
                            ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                            FROM[TRN].[InventorySales] AS II
                              left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                            left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                            left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                            left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                            WHERE  II.ApprovedBy= '" + identity.EmployeeId + @"' 
                            AND II.CheckedByStatus ='Checked' 
                            AND II.ApprovedByStatus= 'Approved'                            
                            AND ISNULL(II.[Status],'') <>'Posting' 
                            GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus,P.UserName,P.Code
							";//II.PlantId= '" + identity.PlantId + @"'  AND 
				}


				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public void GatePassCheckedAndApproved(string Id, string PoValue, string CheckedApprovedStataus, string CheckedApprovedBy, string RejectReason, string UIType)
		{
			var ApprovedById = "";
			var ApprovedByStatus = "";
			if (UIType == "inventory-sales-checking")
			{
				try
				{


					PoValue = "0";
					//  var Id = GetPK();
					if (CheckedApprovedStataus == "Checked")
					{
						if (CheckedApprovedBy == null || CheckedApprovedBy == "")
						{
							throw new CustomException("Select Approved By");
						}
						ApprovedById = CheckedApprovedBy;
						ApprovedByStatus = "For Approval";
						//DailySendMailRequisitionApproved(RequisitionType, RequirmentType, CheckedBy, AuthorizedById, PoId, PreparedBY);

					}
					else
					{
						ApprovedById = null;

					}
					var Status = CheckedApprovedStataus;
					var UpdatedBy = "";
					var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
					var ip = identity.IPAddress;
					var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var AddedBy = identity.Name;
					var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var CompanyGroupId = identity.CompanyGroupId;
					var CompanyId = identity.CompanyId;
					var PlantId = identity.PlantId;
					string _sql = "Update TRN.InventorySales set CheckedByStatus='" + Status + "',ApprovedBy='" + ApprovedById + "',ApprovedByStatus='" + ApprovedByStatus + "',CheckedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";
					_sqlRepository.ExecuteSqlCommand(_sql);
					string _sql1 = "Insert into TRN.InventorySalesApprovalLog(" +
					"CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,ReqValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,GatePassId) " +
					"values ('" + CompanyGroupId + "'," +
					"'" + CompanyId + "'," +
					"'" + PlantId + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + PoValue + "'," +
					"'" + Status + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + ip + "'," +
					"'" + UpdatedBy + "'," +
					"'" + updatedDate + "', " +
					"'" + ip + "','" + Id + "')";
					_sqlRepository.ExecuteSqlCommand(_sql1);
				}
				catch (Exception ex)
				{
					throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
				}

			}
			else if (UIType == "inventory-sales-Approval")
			{
				try
				{
					var IsApproved = 0;

					PoValue = "0";
					//  var Id = GetPK();
					if (CheckedApprovedStataus == "Approved")
					{
						IsApproved = 1;
						ApprovedById = CheckedApprovedBy;
					}
					else
					{
						IsApproved = 0;
						ApprovedById = null;
					}
					var Status = CheckedApprovedStataus;
					var UpdatedBy = "";
					var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
					var ip = identity.IPAddress;
					var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var AddedBy = identity.Name;
					var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var CompanyGroupId = identity.CompanyGroupId;
					var CompanyId = identity.CompanyId;
					var PlantId = identity.PlantId;
					string _sql = "Update TRN.InventorySales set ApprovedByStatus='" + Status + "',ApprovedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";
					_sqlRepository.ExecuteSqlCommand(_sql);
					string _sql1 = "Insert into TRN.InventorySalesApprovalLog(" +
					"CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,ReqValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,GatePassId) " +
					"values ('" + CompanyGroupId + "'," +
					"'" + CompanyId + "'," +
					"'" + PlantId + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + PoValue + "'," +
					"'" + Status + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + ip + "'," +
					"'" + UpdatedBy + "'," +
					"'" + updatedDate + "', " +
					"'" + ip + "','" + Id + "')";
					_sqlRepository.ExecuteSqlCommand(_sql1);
				}
				catch (Exception ex)
				{
					throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
				}
			}

		}
		[Authorize, HttpGet]
		public JsonResult GetGatePassCheckedBy()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				string sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseOrderCheckedBy'";//A.PlantId='" + identity.PlantId + "' AND
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetSalesApproveddBy()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				string sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventorySalesApproveBy'";//A.PlantId='" + identity.PlantId + "' AND
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		[Authorize, HttpGet]
		public JsonResult GetSalesDetailByIssueId(string issueId)
		{
			return Json(_inventoryDetailService.GetSalesDetailByIssueId(issueId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetTaxInfo(string Id)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				string sql = @"select A.Id
							, A.InventorySalesHistoryId
							, A.InventoryReceiveDetailId
							, A.TaxCategoryId
							, A.HSnCodeId
							, HC.Code HSNCode
							, A.Percentage
							, A.TaxAmount
							FROM  TRN.InventorySalesTax A
							LEFT JOIN TRN.InventorySalesHistory B ON B.Id= A.InventorySalesHistoryId
							LEFT JOIN TRN.InventorySalesDetail C ON C.Id= B.InventorySalesDetailId
							LEFT JOIN [TRN].[InventorySales] D ON D.Id= C.InventorySalesId
							LEFT JOIN [HKP].[HSNCode] HC ON HC.Id= A.HSnCodeId
							where D.Id= '" + Id + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		[Authorize, HttpGet]
		public JsonResult GetTaxInfoRowWise(string InventorySalesId, string InventorySalesHistoryId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				string sql = @"SELECT distinct A.InventorySalesHistoryId,A.InventorySalesId,A.TaxCategoryId,A.HSNCodeId,A.Percentage,A.TaxAmount ,B.Code HSNCode,B.Description
                                FROM trn.InventorySalesTax A
                                Left JOIN [HKP].[HSNCode] B On A.HSNCodeId=B.Id   
                                where A.InventorySalesId='" + InventorySalesId + @"' 
                                AND A.InventorySalesHistoryId = '" + InventorySalesHistoryId + @"'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}



		#endregion
		#endregion


		#region Inventory Scrap-------------------------- 
		[Authorize, HttpGet]
		public JsonResult GetDataByInventoryScrap(string tabType)
		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			//var jsondata = Json(_inventoryIssueService.GetDataByInventorySales(identity.PlantId, tabType), JsonRequestBehavior.AllowGet);
			//jsondata.MaxJsonLength = int.MaxValue;
			//return jsondata;
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (tabType == "1")
				{
					sql = @" Select * from(SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
			                AND II.CheckedByStatus='For Checking'
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus									

			                UNION ALL
			                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id
							 LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"'
			                         AND II.CheckedByStatus IS NULL
			                         AND II.ApprovedByStatus IS NULL
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus				
							UNION ALL
			                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id
							 LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
			                         AND II.CheckedByStatus IS NULL
			                         AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus)x  
							Order BY IssueDate DESC";
				}
				else if (tabType == "2")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND (II.CheckedByStatus='Hold' OR II.CheckedByStatus='Reject')                           
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus";
				}
				else if (tabType == "3")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='For Approval'    
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus";
				}
				else if (tabType == "4")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND (II.ApprovedByStatus='Hold' OR II.ApprovedByStatus='Reject')
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus";
				}
				else if (tabType == "5")
				{
					sql = @"select * from(SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX))  
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
                            UNION ALL
                            SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus IS NULL AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
                             UNION ALL
                            SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus IS NULL AND II.ApprovedByStatus IS NULL
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
                             )x
							Order BY IssueDate DESC";
				}
				if (tabType == "6")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'	,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus

							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') ='Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus

							Order BY II.ScrapDate DESC";
				}
				//return _sqlRepository.GetDataCollection(sql);

				//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
				jsondata.MaxJsonLength = int.MaxValue;
				return jsondata;
				//return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}

		}

		[Authorize, HttpGet]
		public JsonResult MaterialScrapDetails(string inveReveiveId, string POID)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryIssueService.MaterialScrapDetails(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
		}
		[Authorize, HttpGet]
		public JsonResult LoadCustomer1(string Id, string toDate, string CostCenterId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"SELECT P.Id AS PartyId, P.Id, P.Code, P.UserName, P.PartyType, NULL AS PartyAccountGroupId, NULL AS PartyAccountGroupCode
                                    , NULL AS PartyAccountGroupName, NULL AS CurrencyId, NULL AS CurrencyCode, NULL AS CurrencyName
                                    , NULL AS PaymentTermId, NULL AS PaymentTermCode, NULL AS PaymentTermName, 0 AS IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, NULL AS GSTIN
                                    FROM [HKP].[Party] AS P
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + identity.CompanyGroupId + "'";// AND P.PartyType='Customer'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		[HttpGet, Authorize]
		public JsonResult NotificationSettingScrap()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			try
			{

				var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='MaterialInventoryScrap' and plantId='" + identity.PlantId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}



		[Authorize, HttpGet]
		public JsonResult GetCheckedByAndApprovedBYScrap(string CheckedBy, string ApprovedBy)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.GetCheckedByAndApprovedBYScrap(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult InventoryScrapCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryScrap inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
			inventoryIssue.CompanyId = identity.CompanyId;
			inventoryIssue.PlantId = identity.PlantId;
			_inventoryIssueService.InsertGraphInventoryScrap(entities, specificStockList, inventoryIssue, IssueTypeStatus, CheckedByStatusForNoti, ApprovedByStatusForNoti);
			return Json(new { inventoryIssue, Message = AplosMessage.Success + "Scrap Sales No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult GetStockScrap(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetStockScrap(entity, issueDate), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public ActionResult InventoryScrapReportPrint(string grnId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			_inventoryReveiveService.InventoryScrapReportPrint(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

			return null;
		}
		[Authorize, HttpGet]
		public JsonResult GetBudgetActivityInScrapMaterial(string materialGroupMasterId)
		{
			return Json(_inventoryDetailService.GetBudgetActivityInScrapMaterial(materialGroupMasterId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		//InventoryScrapReport
		public ActionResult InventoryScrapExcel(string reportFormat, string fromDate, string toDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				ExcelEngine excelEngine = new ExcelEngine();

				IWorkbook workbook = InventoryScrapReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);

				string strFileName = "Inventory Scrap Report.xlsx";
				workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
				workbook.Close();
			}
			catch (CustomException ex)
			{
				return Json(ex.Message, JsonRequestBehavior.AllowGet);

			}
			return null;
		}

		[Authorize, HttpGet]
		//pdf  InventoryScrapPdf
		public ActionResult InventoryScrapPdf(string reportFormat, string fromDate, string toDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				// if (string.IsNullOrEmpty(MasterLCList))
				//   throw new Exception("Please select at least one master Order");

				ExcelEngine excelEngine = new ExcelEngine();

				IWorkbook workbook = InventoryScrapReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);
				string strFileName = "Inventory Scrap Report.pdf";
				ExcelToPdfConverter convert = new ExcelToPdfConverter(workbook);
				PdfDocument pdfDoc = convert.Convert();
				workbook.Close();
				pdfDoc.Save(strFileName, System.Web.HttpContext.Current.Response, HttpReadType.Save);
				//workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

			}
			catch (Exception ex)
			{
				return Json(ex.Message, JsonRequestBehavior.AllowGet);

			}
			return null;
		}


		[HttpPost, Authorize]
		public DataTable GetInventoryScrapReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				if (fromDate != "" && toDate != "")
				{
					var sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							,c.UserName as Company
							,p.UserName as Plant
							
							
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, format(II.DocDate,'dd-MMM-yyyy')DocDate, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus


							--,MT.UserName MaterialType
							--,MGM.UserName AS MaterialGroupMasterName
							,IM.MaterialMasterId
							,MM.UserName MaterialMasterName
						-- , IM.ArticleId
							, ART.StandardName ArticleName
							
							--, IM.FirstCharacteristicsId
							--, FC.UserName AS FirstCharacteristics
							--, IM.FirstCharacteristicsValueId
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							--, IM.SecondCharacteristicsId
							--, SC.UserName AS SecondCharacteristics
							--, IM.SecondCharacteristicsValueId
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							--, IM.ThirdCharacteristicsId
							--, TC.UserName AS ThirdCharacteristics
							--, IM.ThirdCharacteristicsValueId
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						
						    ,Posted=CASE WHEN II.[Status]='Posted' then 'YES' else 'NO' END



							FROM [TRN].[InventoryScrap] AS II
							left join org.company c on c.id= ii.companyid
							left join org.Plant p on p.id= ii.PlantId
							
							

							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy



							LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
							left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id



							WHERE II.PlantId='" + identity.PlantId + @"' 
                         AND convert(Date,II.ScrapDate) BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
			               -- AND II.CheckedByStatus='For Checking'
							--AND ISNULL(II.[Status],'') <>'Posting' 
							--AND II.ScrapDate Between '1-Jan-2020' ANd '1-Jan-2020'
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id 
							,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,
							 II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , 
							EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus	
							,c.UserName  
							,p.UserName

							,IM.MaterialMasterId
							,MM.UserName 
						-- , IM.ArticleId
							, ART.StandardName 
							
							, ISNULL(FCV.UserName,'')  
							--, IM.SecondCharacteristicsId
							--, SC.UserName AS SecondCharacteristics
							--, IM.SecondCharacteristicsValueId
							, ISNULL(SCV.UserName,'')  
							--, IM.ThirdCharacteristicsId
							--, TC.UserName AS ThirdCharacteristics
							--, IM.ThirdCharacteristicsValueId
							, ISNULL(TCV.UserName,''),II.[Status] ";
					return _sqlRepository.GetDataTable(sql);
				}


				else
				{
					var sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							,c.UserName as Company
							,p.UserName as Plant
							
							
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, format(II.DocDate,'dd-MMM-yyyy')DocDate, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus


							--,MT.UserName MaterialType
							--,MGM.UserName AS MaterialGroupMasterName
							,IM.MaterialMasterId
							,MM.UserName MaterialMasterName
						-- , IM.ArticleId
							, ART.StandardName ArticleName
							
							--, IM.FirstCharacteristicsId
							--, FC.UserName AS FirstCharacteristics
							--, IM.FirstCharacteristicsValueId
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							--, IM.SecondCharacteristicsId
							--, SC.UserName AS SecondCharacteristics
							--, IM.SecondCharacteristicsValueId
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							--, IM.ThirdCharacteristicsId
							--, TC.UserName AS ThirdCharacteristics
							--, IM.ThirdCharacteristicsValueId
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						
						    ,Posted=CASE WHEN II.[Status]='Posted' then 'YES' else 'NO' END



							FROM [TRN].[InventoryScrap] AS II
							left join org.company c on c.id= ii.companyid
							left join org.Plant p on p.id= ii.PlantId
							
							

							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy



							LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
							left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id



							WHERE II.PlantId='" + identity.PlantId + @"' 

                            AND convert(Date,II.ScrapDate) <= '" + toDate + @"'
			               -- AND II.CheckedByStatus='For Checking'
							--AND ISNULL(II.[Status],'') <>'Posting' 
							--AND II.ScrapDate Between '1-Jan-2020' ANd '1-Jan-2020'
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id 
							,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,
							 II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , 
							EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus	
							,c.UserName  
							,p.UserName

							,IM.MaterialMasterId
							,MM.UserName 
						-- , IM.ArticleId
							, ART.StandardName 
							
							, ISNULL(FCV.UserName,'')  
							--, IM.SecondCharacteristicsId
							--, SC.UserName AS SecondCharacteristics
							--, IM.SecondCharacteristicsValueId
							, ISNULL(SCV.UserName,'')  
							--, IM.ThirdCharacteristicsId
							--, TC.UserName AS ThirdCharacteristics
							--, IM.ThirdCharacteristicsValueId
							, ISNULL(TCV.UserName,''),II.[Status] ";
					return _sqlRepository.GetDataTable(sql);
				}





			}

			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpPost, Authorize]
		private IWorkbook InventoryScrapReportList(string companyGroupId, string companyId, string plantId, string FromDate, string ToDate)
		{

			//Start EmployeeAdvanceDueList

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			ExcelEngine excelEngine = new ExcelEngine();
			//Instantiate the Excel application object
			IApplication application = excelEngine.Excel;

			//Set the default application version
			application.DefaultVersion = ExcelVersion.Excel2013;

			//Load the existing Excel workbook into IWorkbook
			IWorkbook workbook = application.Workbooks.Create(1);

			//Get the first worksheet in the workbook into IWorksheet
			IWorksheet worksheet = workbook.Worksheets[0];
			DataTable dtInventoryScrapReportList = GetInventoryScrapReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, FromDate, ToDate);



			if (dtInventoryScrapReportList.Rows.Count == 0)
				throw new Exception("No data found");
			// throw new Exception("To date must be above or equal to From Date.");

			worksheet.Name = "InventroyScrapReport";
			var _rowd = 4;
			if (FromDate != "" && ToDate != "")
			{


				worksheet[_rowd, 4].Text = ToDate + " " + "To" + " " + ToDate;

				worksheet.UsedRange.CellStyle.Font.Size = 8;
				//sheet1.UsedRange.CellStyle.Font.Bold = true;
				worksheet.Range[_rowd, 3, _rowd, 5].Merge();
				//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

			}

			else
			{

				worksheet[_rowd, 4].Text = ToDate;
				worksheet.UsedRange.CellStyle.Font.Size = 8;
				worksheet.UsedRange.CellStyle.Font.Bold = false;
				worksheet.Range[_rowd, 3, _rowd, 4].Merge();
				//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

			}

			var _rows = 5;
			worksheet[_rows, 5].Text = "Report Ref No: ";
			worksheet.Range[_rows, 3, _rows, 6].CellStyle.Font.Size = 8;
			worksheet.Range[_rows, 3, _rows, 6].Merge();
			worksheet.UsedRange.CellStyle.Font.Bold = false;
			_rows++;

			int COL = 1; int ROW = 7;
			int startCol = COL;

			worksheet[ROW, COL].Text = "SL.No";
			int colSLNO = COL;
			worksheet[ROW, COL].ColumnWidth = 7;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Entity";
			int colEntity = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Company";
			int colCompany = COL;
			worksheet[ROW, COL].ColumnWidth = 18;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Plant";
			int colPlant = COL;
			worksheet[ROW, COL].ColumnWidth = 18;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Type";
			int colissuetype = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Material Storage";
			int colMaterialStorage = COL;
			worksheet[ROW, COL].ColumnWidth = 18;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Issue Date";
			int colIssueDate = COL;
			worksheet[ROW, COL].ColumnWidth = 14;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;



			worksheet[ROW, COL].Text = "Remarks";
			int colRemarks = COL;
			worksheet[ROW, COL].ColumnWidth = 15;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "DocRefNo";
			int colDocRefNo = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Doc Date";
			int colDocDate = COL;
			worksheet[ROW, COL].ColumnWidth = 15;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "NoteForAccounts";
			int colNoteForAccounts = COL;
			worksheet[ROW, COL].ColumnWidth = 15;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;


			worksheet[ROW, COL].Text = "Checked By";
			int colCheckedByName = COL;
			worksheet[ROW, COL].ColumnWidth = 17;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Checked Status";
			int colCheckedByStatus = COL;
			worksheet[ROW, COL].ColumnWidth = 17;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Approved By";
			int colApprovedByName = COL;
			worksheet[ROW, COL].ColumnWidth = 15;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Approved Status";
			int colApprovedByStatus = COL;
			worksheet[ROW, COL].ColumnWidth = 15;
			worksheet[ROW - 1, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Material Master";
			int colMaterialMasterName = COL;
			worksheet[ROW, COL].ColumnWidth = 15;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Article";
			int colArticleName = COL;
			worksheet[ROW, COL].ColumnWidth = 15;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;


			worksheet[ROW, COL].Text = "SKU1";
			int colFirstCharacteristicsValue = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;


			worksheet[ROW, COL].Text = "SKU2";
			int colSecondCharacteristicsValue = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

			COL++;

			worksheet[ROW, COL].Text = "SKU3";
			int colThirdCharacteristicsValue = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;


			worksheet[ROW, COL].Text = "Posted";
			int colPosted = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Qty";
			int colQty = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			COL++;

			worksheet[ROW, COL].Text = "Amount";
			int colAmount = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			int endCol = COL;
			worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
			worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
			worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;

			worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
			ROW++;

			for (int i = 0; i < dtInventoryScrapReportList.Rows.Count; i++)
			{

				worksheet[ROW, colSLNO].Number = (i + 1);

				worksheet[ROW, colEntity].Text = dtInventoryScrapReportList.Rows[i]["Entity"].ToString();
				worksheet[ROW, colCompany].Text = dtInventoryScrapReportList.Rows[i]["Company"].ToString();

				worksheet[ROW, colPlant].Text = dtInventoryScrapReportList.Rows[i]["Plant"].ToString();
				worksheet[ROW, colissuetype].Text = dtInventoryScrapReportList.Rows[i]["issuetype"].ToString();
				worksheet[ROW, colIssueDate].Text = dtInventoryScrapReportList.Rows[i]["IssueDate"].ToString();
				worksheet[ROW, colMaterialStorage].Text = dtInventoryScrapReportList.Rows[i]["MaterialStorage"].ToString();
				worksheet[ROW, colQty].Number = clsStaticInfo.dbl(dtInventoryScrapReportList.Rows[i]["Qty"].ToString());
				worksheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtInventoryScrapReportList.Rows[i]["Amount"].ToString());
				worksheet[ROW, colRemarks].Text = dtInventoryScrapReportList.Rows[i]["Remarks"].ToString();


				worksheet[ROW, colDocRefNo].Text = dtInventoryScrapReportList.Rows[i]["DocRefNo"].ToString();
				worksheet[ROW, colDocDate].Text = dtInventoryScrapReportList.Rows[i]["DocDate"].ToString();
				worksheet[ROW, colNoteForAccounts].Text = dtInventoryScrapReportList.Rows[i]["NoteForAccounts"].ToString();
				worksheet[ROW, colCheckedByName].Text = dtInventoryScrapReportList.Rows[i]["CheckedByName"].ToString();
				worksheet[ROW, colApprovedByName].Text = dtInventoryScrapReportList.Rows[i]["ApprovedByName"].ToString();
				worksheet[ROW, colCheckedByStatus].Text = dtInventoryScrapReportList.Rows[i]["CheckedByStatus"].ToString();
				worksheet[ROW, colApprovedByStatus].Text = dtInventoryScrapReportList.Rows[i]["ApprovedByStatus"].ToString();
				worksheet[ROW, colMaterialMasterName].Text = dtInventoryScrapReportList.Rows[i]["MaterialMasterName"].ToString();
				worksheet[ROW, colArticleName].Text = dtInventoryScrapReportList.Rows[i]["ArticleName"].ToString();
				worksheet[ROW, colFirstCharacteristicsValue].Text = dtInventoryScrapReportList.Rows[i]["FirstCharacteristicsValue"].ToString();
				worksheet[ROW, colSecondCharacteristicsValue].Text = dtInventoryScrapReportList.Rows[i]["SecondCharacteristicsValue"].ToString();
				worksheet[ROW, colThirdCharacteristicsValue].Text = dtInventoryScrapReportList.Rows[i]["ThirdCharacteristicsValue"].ToString();

				worksheet[ROW, colPosted].Text = dtInventoryScrapReportList.Rows[i]["Posted"].ToString();
				// worksheet[row, colpurchaseprice].numberformat = clsstaticinfo.numberformat();




				worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
				worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
				worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
				ROW++;

			}

			worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
			//worksheet.UsedRange.CellStyle.Font.Size = 8f;



			ReportUtility reportUtility = new ReportUtility();

			reportUtility.PlantHeader(ref worksheet, endCol, "Inventory Scrap", identity.PlantId);
			reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			// worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

			worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
			worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
			worksheet.IsGridLinesVisible = false;

			#region Freeze Panes

			worksheet.IsDisplayZeros = false;
			worksheet.UsedRange["A8"].FreezePanes();
			worksheet.FirstVisibleColumn = 1;
			worksheet.FirstVisibleRow = 8;

			#endregion Freeze Panes


			return workbook;
		}

		#endregion

		#region Inventory Scrap Checked And Approved------------------------

		#region Inventory Scrap Checked And Approved
		[Authorize, HttpGet]
		public JsonResult GetCheckedApprovedListScrap(string tabType)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";


				if (tabType == "UnCheckedList")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.CheckedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus='For Checking' 
							AND II.ApprovedByStatus IS NULL 
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
				}
				else if (tabType == "HoldRejectCheckedList")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.CheckedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus='Hold' OR II.CheckedByStatus='Reject'
							AND II.ApprovedByStatus IS NULL 
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
				}
				else if (tabType == "CheckedList")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.CheckedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus='Checked' 
							AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
				}
				else if (tabType == "UnApprovedList")
				{
					sql = @"SELECT * FROM(
                                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.ApprovedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus='Checked' 
							AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					
							

					UNION ALL

                                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.ApprovedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus IS NULL
							AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus				
							
                                )X
                                Order BY IssueDate DESC";
				}
				else if (tabType == "HoldRejectApprovedList")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.ApprovedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus ='Checked'
							AND (II.ApprovedByStatus ='Hold' OR II.ApprovedByStatus ='Reject')
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
				}
				else if (tabType == "ApprovedList")
				{
					sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.ApprovedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus ='Checked'
							AND II.ApprovedByStatus ='Approved'
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
				}


				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public void CheckedAndApprovedScrap(string Id, string PoValue, string CheckedApprovedStataus, string CheckedApprovedBy, string RejectReason, string UIType)
		{
			var ApprovedById = "";
			var ApprovedByStatus = "";
			if (UIType == "inventory-scrap-checking")
			{
				try
				{


					PoValue = "0";
					//  var Id = GetPK();
					if (CheckedApprovedStataus == "Checked")
					{
						if (CheckedApprovedBy == null || CheckedApprovedBy == "")
						{
							throw new CustomException("Select Approved By");
						}
						ApprovedById = CheckedApprovedBy;
						ApprovedByStatus = "For Approval";
						//DailySendMailRequisitionApproved(RequisitionType, RequirmentType, CheckedBy, AuthorizedById, PoId, PreparedBY);

					}
					else
					{
						ApprovedById = null;

					}
					var Status = CheckedApprovedStataus;
					var UpdatedBy = "";
					var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
					var ip = identity.IPAddress;
					var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var AddedBy = identity.Name;
					var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var CompanyGroupId = identity.CompanyGroupId;
					var CompanyId = identity.CompanyId;
					var PlantId = identity.PlantId;
					string _sql = "Update TRN.InventoryScrap set CheckedByStatus='" + Status + "',ApprovedBy='" + ApprovedById + "',ApprovedByStatus='" + ApprovedByStatus + "',CheckedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";
					_sqlRepository.ExecuteSqlCommand(_sql);
					string _sql1 = "Insert into TRN.InventoryScrapApprovalLog(" +
					"CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,ReqValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,GatePassId) " +
					"values ('" + CompanyGroupId + "'," +
					"'" + CompanyId + "'," +
					"'" + PlantId + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + PoValue + "'," +
					"'" + Status + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + ip + "'," +
					"'" + UpdatedBy + "'," +
					"'" + updatedDate + "', " +
					"'" + ip + "','" + Id + "')";
					_sqlRepository.ExecuteSqlCommand(_sql1);
				}
				catch (Exception ex)
				{
					throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
				}

			}
			else if (UIType == "inventory-scrap-approval")
			{
				try
				{
					var IsApproved = 0;

					PoValue = "0";
					//  var Id = GetPK();
					if (CheckedApprovedStataus == "Approved")
					{
						IsApproved = 1;
						ApprovedById = CheckedApprovedBy;
					}
					else
					{
						IsApproved = 0;
						ApprovedById = null;
					}
					var Status = CheckedApprovedStataus;
					var UpdatedBy = "";
					var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
					var ip = identity.IPAddress;
					var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var AddedBy = identity.Name;
					var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var CompanyGroupId = identity.CompanyGroupId;
					var CompanyId = identity.CompanyId;
					var PlantId = identity.PlantId;
					string _sql = "Update TRN.InventoryScrap set ApprovedByStatus='" + Status + "',ApprovedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";
					_sqlRepository.ExecuteSqlCommand(_sql);
					string _sql1 = "Insert into TRN.InventoryScrapApprovalLog(" +
					"CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,ReqValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,GatePassId) " +
					"values ('" + CompanyGroupId + "'," +
					"'" + CompanyId + "'," +
					"'" + PlantId + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + PoValue + "'," +
					"'" + Status + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + ip + "'," +
					"'" + UpdatedBy + "'," +
					"'" + updatedDate + "', " +
					"'" + ip + "','" + Id + "')";
					_sqlRepository.ExecuteSqlCommand(_sql1);
				}
				catch (Exception ex)
				{
					throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
				}
			}

		}
		[Authorize, HttpGet]
		public JsonResult GetCheckedByScrap()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				string sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventoryScrapCheckedBy'";//A.PlantId='" + identity.PlantId + "' AND
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetScrapApprovedBy()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				string sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventoryScrapApproveBy'";//A.PlantId='" + identity.PlantId + "' AND
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		[Authorize, HttpGet]
		public JsonResult GetScrapDetailByIssueId(string issueId)
		{
			return Json(_inventoryDetailService.GetScrapDetailByIssueId(issueId), JsonRequestBehavior.AllowGet);
		}

		#endregion
		#endregion

		#region Material Transfer
		[Authorize, HttpGet]
		public JsonResult GetListForMaterialTransferGridFun(string POTypeStatus)

		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDetailService.GetListForMaterialTransferGridFun(identity.PlantId, POTypeStatus), JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult MaterialTransferCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryReceive inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
			inventoryIssue.CompanyId = identity.CompanyId;
			inventoryIssue.ToPlantId = inventoryIssue.PlantId;
			inventoryIssue.PlantId = identity.PlantId;

			if (specificStockList == null)
			{

				throw new CustomException("Please select GRN.");

			}
			else if (identity.EmployeeId == inventoryIssue.CheckedBy)
			{
				throw new CustomException("Please select another employee for Check by.");
			}
			else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
			{

				inventoryIssue.AuthorizedBy = inventoryIssue.CheckedBy;
				inventoryIssue.AuthorizedByStatus = "For Approval";
				inventoryIssue.CheckedBy = null;
				inventoryIssue.CheckedByStatus = null;

			}
			else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
			{
				inventoryIssue.CheckedByStatus = null;
				inventoryIssue.AuthorizedByStatus = null;
				inventoryIssue.CheckedBy = null;
				inventoryIssue.AuthorizedBy = null;
				inventoryIssue.IsApproved = true;
			}
			else
			{
				inventoryIssue.CheckedBy = inventoryIssue.CheckedBy;
				inventoryIssue.CheckedByStatus = "ForChecked";
				inventoryIssue.AuthorizedBy = null;
				inventoryIssue.AuthorizedByStatus = null;
				//inventoryIssue.IsApproved = false;

			}

			_inventoryIssueService.MaterialTransferCreateInsertGraph(entities, specificStockList, inventoryIssue, IssueTypeStatus, CheckedByStatusForNoti, ApprovedByStatusForNoti);
			return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult GetMaterialTransferStock(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetMaterialTransferStock(entity, issueDate), JsonRequestBehavior.AllowGet);
		}

		#region Material Transfer  Report  

		[Authorize, HttpGet]
		public ActionResult MaterialTransferReport(string grnId)

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			_inventoryDetailService.MaterialTransferReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

			return null;
		}
		[Authorize, HttpGet]
		public JsonResult GetInventoryMaterialListwithoutpo(GridParameter parameters, string inveReveiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryIssueService.Querywithoutpo(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult GetSpecificMaterialTransferStock(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetSpecificMaterialTransferStock(entity, issueDate), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult StorageWisePlant(string StorageId)
		{
			try
			{
				var sql = @"SELECT P.Id Value,P.UserName Text FROM [HKP].[MaterialStorage] MS LEFT JOIN org.plant P ON P.Id=MS.PlantId WHERE MS.id='" + StorageId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		[Authorize, HttpGet]
		public JsonResult PlatByStorage(string PlantId)
		{
			try
			{
				var sql = @"SELECT  MS.Id Value,MS.UserName Text from org.plant P LEFT JOIN  [HKP].[MaterialStorage] MS  ON P.Id=MS.PlantId where P.Id='" + PlantId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		[Authorize, HttpGet]
		public JsonResult CompanyPlant()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"select Id Value, UserName Text from org.Plant where CompanyId='" + identity.CompanyId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		#endregion
		#endregion
		#region Material Transfer Excel Report

		[Authorize, HttpGet]
		public ActionResult MaterialTransferExcelReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			plantId = identity.PlantId;
			var reportFileName = "Material Transfer" + fromDate + "To" + toDate + "";
			//var workbook = _materialMasterService.CreatePurchaseRegisterReturnReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);
			InventoryCommonService _MaterialTransferexcelReportQueryService = new InventoryCommonService(_sqlRepository);
			IWorkbook workbook = null;
			workbook = _MaterialTransferexcelReportQueryService.CreateMaterialTransferExcelReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);


			switch (reportFormat)
			{
				case ReportFormat.Pdf:
					return RenderReportAsPdf(workbook, reportFileName);

				case ReportFormat.Excel:
					return RenderReportAsExcel(workbook, reportFileName);

				default:
					return View();
			}
		}



		[Authorize, HttpPost]
		public JsonResult GetMaterialTransferRegister(string fromDate, string toDate, string Type)
		{

			DateTime fDate = DateTime.Parse(fromDate);
			DateTime tDate = DateTime.Parse(toDate);
			if (fromDate == null || fromDate == "")
			{
				throw new CustomException("Select From Date");
			}
			else if (toDate == null || toDate == "")
			{
				throw new CustomException("Select To Date");
			}

			//else if (tDate  < fDate)
			//{
			//	throw new CustomException("To Date can not less than From date");
			//}
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			InventoryCommonService _MaterialTransferRegisterService = new InventoryCommonService(_sqlRepository);
			var jsondata = Json(_MaterialTransferRegisterService.GetMaterialTransferRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);

			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
		}

		#endregion

		#region Others Code

		[Authorize, HttpGet]
		public JsonResult MaterialIssueDetailsData1(string inveReveiveId, string POID)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryIssueService.MaterialIssueDetailsData1(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
		}
		[Authorize, HttpGet]
		public JsonResult MaterialIssueDetailsData(string inveReveiveId, string POID)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryIssueService.MaterialIssueDetailsData(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;

		}
		#region Order Ref No
		[HttpGet, Authorize]
		public JsonResult GetMasterOrderList(string contractId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = @"SELECT distinct A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId	
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' )
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus
                                    ,A.OwnReferenceNo [BuyerOrd],A.BuyerReferenceNo [OwnOrd]
                                    ,[BuyerItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    ,CNT.ContractNo  ContractNo,MLC.LCRef MasterLCNo
                            FROM [TRN].[MasterOrder] AS A
                            left JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            left join [TRN].[MasterOrderItem] MOI ON MOI.MasterOrderId=A.Id
							LEFT JOIN dbo.Contract CNT ON CNT.Id=MOI.ContractId
							LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId                                              
                            WHERE A.CompanyId='" + identity.CompanyId + "' AND OrderType='ExternalOrder'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		[HttpGet, Authorize]
		public JsonResult GetMasterOrderDetailsList(string MasterOrderId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = @"SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId	
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' )
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus
                                    ,A.OwnReferenceNo [BuyerOrd],A.BuyerReferenceNo [OwnOrd]
                                    ,[BuyerItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    ,CNT.ContractNo  ContractNo,MLC.Id MasterLCNo
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN dbo.Contract CNT ON CNT.Id=A.MasterOrderNo
							left join [TRN].[MasterOrderItem] MOI ON MOI.ContractId=CNT.Id
							LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId
                            WHERE A.CompanyId='" + identity.CompanyId + "' AND OrderType='ExternalOrder' AND A.Id='" + MasterOrderId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		[HttpPost, Authorize]
		public JsonResult CountryLoad(InventoryMaterialViewModel entity)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = @"select distinct C.Id Value,C.UserName Text from trn.InventoryMaterial IM
                            left join scs.Country C ON C.Id=IM.CountryId
                            where IM.CountryId is not NULL AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                            AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                            AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		#endregion Others Code

		#region Additional Tax
		[Authorize, HttpPost]//
		public ActionResult SaveAdditinalTaxInGRN(string InventoryReceiveId, List<Dictionary<string, object>> UserSendData,string ToCurrencyRate)
		{
			try
			{
				Library.MaterialManagement.InventoryManagements.InventorySalesService obj = new Library.MaterialManagement.InventoryManagements.InventorySalesService();
				obj.SaveAdditinalTaxInGRN(InventoryReceiveId, UserSendData, ToCurrencyRate);
				return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}


		}
		public JsonResult GetAdvanceTaxInfo(string InventoryReceiveId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.InventorySalesService obj = new Library.MaterialManagement.InventoryManagements.InventorySalesService();
				return Json(obj.GetAdvanceTaxInfo(InventoryReceiveId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		[Authorize, HttpPost]
		public ActionResult AdditionalTaxDelete(string Id)
		{

			try
			{
				Library.MaterialManagement.InventoryManagements.InventorySalesService obj = new Library.MaterialManagement.InventoryManagements.InventorySalesService();
				obj.AdditionalTaxDelete(Id);
				return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{

				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}


		}

		#endregion

		[Authorize, HttpPost]
		public JsonResult GetPopUpShowStorageLocation(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetPopUpShowStorageLocation(entity, issueDate), JsonRequestBehavior.AllowGet);
		}

        [Authorize, HttpPost]
        public JsonResult StorageLocationStockWise(string MaterialMstId,string ArticleId, string issueDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //entity.CompanyGroupId = identity.CompanyGroupId;
            //entity.CompanyId = identity.CompanyId;
            //entity.PlantId = identity.PlantId;
            return Json(_inventoryMaterialService.StorageLocationStockWise(MaterialMstId, ArticleId, issueDate), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
		public ActionResult GetEntityWiseConsumption(string EntityId)
		{
			try
			{
				Library.MaterialManagement.InventoryManagements.InventoryIssueService obj = new Library.MaterialManagement.InventoryManagements.InventoryIssueService();				
				return Json(obj.GetEntityWiseConsumption(EntityId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}


		}

		#region JW Issue

		[Authorize, HttpPost]
		public JsonResult GetJWStock(InventoryMaterialViewModel entity, string issueDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			return Json(_inventoryMaterialService.GetJWStock(entity, issueDate), JsonRequestBehavior.AllowGet);
		}
		//[Authorize, HttpPost]
		//public JsonResult GetRequisitionList(string issueDetailId)
		//{

		//	return Json(_inventoryMaterialService.GetRequisitionList(issueDetailId), JsonRequestBehavior.AllowGet);
		//}

		[HttpPost]
		public JsonResult JWIssueCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
			inventoryIssue.CompanyId = identity.CompanyId;
			inventoryIssue.PlantId = identity.PlantId;
			_inventoryIssueService.JWInsertGraph(entities, specificStockList, inventoryIssue, IssueTypeStatus, entitiesAll);
			return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
		}
		#endregion

		
		[Authorize, HttpPost]
		public JsonResult ConverttedBOQUOMData(Dictionary<string, object> data)
		{
			Library.General.Conversions.UOMConversion conversion = new Library.General.Conversions.UOMConversion();
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				data["RequisitionQty"] = conversion.Convert(data["MaterialMasterId"].ToString(), data["consumptionUoMId"].ToString(), data["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(data["RequestedQtyOrginal"].ToString())).ToString("F2");
				//data["IssuedQty"] = conversion.Convert(data["MaterialMasterId"].ToString(), data["consumptionUoMId"].ToString(), data["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(data["IssuedQty"].ToString())).ToString("F2");
				//data["OtherPOQty"] = conversion.Convert(data["MaterialMasterId"].ToString(), data["FromPoUomId"].ToString(), data["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(data["OtherPOQty"].ToString())).ToString("F2"); 
				return Json(new { data, Message = AplosMessage.Success });
			}
			catch (global::System.Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });


			}


		}
	}
}