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
using Aplos.MaterialManagement.MaterialQuery;

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
        public ActionResult inventoryIssueBOQ()
        {
            return View();
        }
        public ActionResult POWiseMaterialIssue()
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


        [Authorize, HttpPost]
        public JsonResult GetDataByInventoryIssue(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var jsondata = Json(inventoryIssueQueryService.GetDataByInventoryIssue(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [Authorize, HttpGet]
        public JsonResult GetDataByInventoryReturnIssue()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var jsondata = Json(inventoryIssueQueryService.GetDataByInventoryReturnIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
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

        [HttpPost, Authorize]
        public JsonResult GetIssueList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryIssueService.GetIssueList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryIssueReturnListForPosting(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryIssueService.GetInventoryIssueReturnListForPosting(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
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
        public JsonResult GetInventoryMaterialIssueReturnList(GridParameter parameters, string issueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.GetIssueReturnMaterial(parameters, issueId, identity.CompanyId), JsonRequestBehavior.AllowGet);
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
        public JsonResult IssueDetailDelete(string issueDetailId,string voucherId)
        {
            _inventoryIssueService.DeleteIssueDetail(issueDetailId, voucherId);
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult UpdateIssueMaster(InventoryIssue inventoryIssue)
        {
            _inventoryIssueService.UpdateIssueMaster(inventoryIssue);
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult IssueReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryReveiveService.InventoryIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);
            return null;
        }

        // Outsource Transformation Issue
        [Authorize, HttpGet]
        public ActionResult JobWorkIssueReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryReveiveService.JWIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);
            return null;
        }

        // Outsource Value Added Issue
        [Authorize, HttpGet]
        public ActionResult JWValAddedIssueReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryReveiveService.JWValAddedIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);
            return null;
        }

        // JobWork Transformation Issue
        [Authorize, HttpGet]
        public ActionResult JobWorkTransformationIssueReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryReveiveService.JobWorkTransformationIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);
            return null;
        }

        // JobWork Value Added Issue
        [Authorize, HttpGet]
        public ActionResult JobWorkValAddedIssueReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryReveiveService.JobWorkValAddedIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);
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

        [HttpGet, Authorize]
        public JsonResult GetNewCompanyConfiguration()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CompanyExtensionService companyExtensionService = new CompanyExtensionService(_sqlRepository);
            return Json(companyExtensionService.GetCompanyConfiguration(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        #region Posted and not posted Issue Delete permanently
        [Authorize, HttpGet]
        public JsonResult GetDeletableIssueList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetDeletableIssueList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetApprovedIssueSlip(), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetAssetIssueSlip()
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetAssetIssueSlip(), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetApprovedIssueSlipDetails(string Id, string StorageLocationId, string OrderSpecific)
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetApprovedIssueSlipDetails(Id, StorageLocationId, OrderSpecific), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetApprovedIssueSlipBOQDetails(string Id, string StorageLocationId, string OrderSpecific)
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetApprovedIssueSlipBOQDetails(Id, StorageLocationId, OrderSpecific), JsonRequestBehavior.AllowGet);
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
        public JsonResult GetGRNFixedAssetList(string materialStorageId, string issueDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetGRNFixedAssetList(identity.PlantId, materialStorageId, issueDate), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetAssetIssueSlipWithGRN(string materialStorageId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetAssetIssueSlipWithGRN(identity.PlantId, materialStorageId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertAssetIssue(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.InsertAssetInventoryIssue(entities, specificStockList, inventoryIssue);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
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
                            LEFT join [ORG].[CostCenter] CC On CC.Id=IID.CostCenterId
                            LEFT JOIN (SELECT DISTINCT InventoryIssueDetailId,MaterialStorageId FROM TRN.InventoryIssueHistory WHERE MaterialStorageId='"+ MaterialStorageId + @"' )IIH ON IIH.InventoryIssueDetailId=IID.Id
                            Where CAST(IRM.IssueDate AS DATE) between '" + fromDate + @"' and '" + toDate + "' and CC.Id='" + CostCenterId + "' AND IIH.MaterialStorageId='" + MaterialStorageId + "'";
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

                    sql = @"select cc.Id CostCenterId,cc.UserName CostCenterName, a.InventoryReceiveDetailId,IM.Id InventoryMaterialId,b.InventoryIssueId, REPLACE(CONVERT(CHAR(11), C.IssueDate, 106),' ','-') AS IssueDate,a.IssueRequestDetailId
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
                            ,c.MaterialStorageId,MS.UserName MaterialStorage,a.Id InventoryIssueHistoryId,a.InventoryIssueDetailId
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
                    AND CC.Id='" + CostCenterId + @"' --AND a.MaterialStorageId='" + MaterialStorageId + @"' AND IssueDate Between '" + fromDate + @"' and '" + toDate + @"'
                    AND Isnull(a.qty,0)-Isnull(a.IssueReturnQty,0)>0 AND Isnull(a.Rate,0)>0
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

                    sql = @"select cc.Id CostCenterId,cc.UserName CostCenterName, a.InventoryReceiveDetailId,IM.Id InventoryMaterialId,b.InventoryIssueId, REPLACE(CONVERT(CHAR(11), C.IssueDate, 106),' ','-') AS IssueDate,a.IssueRequestDetailId
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
                            ,c.MaterialStorageId,MS.UserName MaterialStorage,a.Id InventoryIssueHistoryId,a.InventoryIssueDetailId
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
                    AND CC.Id='" + CostCenterId + @"' AND a.MaterialStorageId='" + MaterialStorageId + @"' AND IssueDate Between '" + fromDate + @"' and '" + toDate + @"'
                    AND Isnull(a.qty,0)-Isnull(a.IssueReturnQty,0)>0 AND Isnull(a.Rate,0)>0
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
            try
            {
                InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
                return Json(inventoryIssueQueryService.IssueReturnForUpdateQuery(Id, toDate, CostCenterId), JsonRequestBehavior.AllowGet);
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(inventoryIssueQueryService.GetDataByPhysicalStockAdjustment(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(inventoryIssueQueryService.MaterialAdjustmentDetailsData(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var jsondata = Json(inventoryIssueQueryService.MaterialSalesDetails(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
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
        public JsonResult InventorySalesCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventorySales inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<InventorySalesTax> taxCategoryList, string productNewId, decimal ToCurrencyRate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.InsertGraphInventorySales(entities, specificStockList, inventoryIssue, IssueTypeStatus, CheckedByStatusForNoti, ApprovedByStatusForNoti, taxCategoryList, productNewId, ToCurrencyRate);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Sales No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult GetStockSales(InventoryMaterialViewModel entity, string issueDate)//dgsdg
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
        public ActionResult inventoryPreSalesReportPrint(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.InventoryPreSalesReportPrint(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

            return null;
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialReceivableList(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetReceivableMaterial(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }



        [Authorize, HttpGet]
        public JsonResult GetInventortGLBudgetActivity(string inventorysalesId, string customerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetInventortGLBudgetActivityData(identity.CompanyId, identity.PlantId, inventorysalesId, customerId), JsonRequestBehavior.AllowGet);
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



       


        [HttpPost, Authorize]
        public JsonResult DeleteSalesDetail(string issueDetailId)
        {
            _inventoryIssueService.DeleteSalesDetail(issueDetailId);
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult SalesServiceChargesCreate(InventoryMaterialViewModel entity, IEnumerable<InventorySalesTax> taxCategoryList)
        {
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
                            ,POT.TaxAmount As SalesServiceTaxAmount,0Amount,0 TotalTaxAmount
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

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(IssueDetailDelete))]
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetCheckedApprovedListQuery(tabType), JsonRequestBehavior.AllowGet);
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
                          where  A.ActionStatus='PurchaseOrderCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
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
                          where  A.ActionStatus='InventorySalesApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
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




        #endregion
        #endregion


        #region Inventory Scrap-------------------------- 
        [Authorize, HttpGet]
        public JsonResult GetDataByInventoryScrap(string tabType)
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            try
            {
                var jsondata = Json(inventoryIssueQueryService.GetDataByInventoryScrapQuery(tabType), JsonRequestBehavior.AllowGet);
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
        public JsonResult MaterialScrapDetails(string inveReveiveId, string POID)
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var jsondata = Json(inventoryIssueQueryService.MaterialScrapDetails(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
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
                InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = inventoryIssueQueryService.InventoryScrapReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);

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
                InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository); // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = inventoryIssueQueryService.InventoryScrapReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);
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

        #endregion

        #region Inventory Scrap Checked And Approved------------------------

        #region Inventory Scrap Checked And Approved
        [Authorize, HttpGet]
        public JsonResult GetCheckedApprovedListScrap(string tabType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
                return Json(inventoryIssueQueryService.GetCheckedApprovedListScrapQuery(tabType), JsonRequestBehavior.AllowGet);

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
                          where  A.ActionStatus='InventoryScrapCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
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
                          where  A.ActionStatus='InventoryScrapApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
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
            if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
            {
                CheckedByStatusForNoti = "False";
                ApprovedByStatusForNoti = "False";

            }


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
                //inventoryIssue.CheckedBy = null;
                inventoryIssue.CheckedByStatus = "ForChecked";
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryIssueQueryService.Querywithoutpo(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var jsondata = Json(inventoryIssueQueryService.MaterialIssueDetailsData1(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [Authorize, HttpGet]
        public JsonResult MaterialIssueDetailsData(string inveReveiveId, string POID)
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var jsondata = Json(inventoryIssueQueryService.MaterialIssueDetailsData(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveAdditinalTaxInGRN(string InventoryReceiveId, List<Dictionary<string, object>> UserSendData, string ToCurrencyRate)
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
        public JsonResult StorageLocationStockWise(string MaterialMstId, string ArticleId, string issueDate)
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
        public JsonResult JWIssueCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll, string TabType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.JWInsertGraph(entities, specificStockList, inventoryIssue, IssueTypeStatus, entitiesAll, TabType);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // Job Work Issue Save

        [HttpPost]
        public JsonResult JobWorkIssueCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll, string TabType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.JobWorkIssueCreate(entities, specificStockList, inventoryIssue, IssueTypeStatus, entitiesAll, TabType);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryIssueBOQ()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var jsondata = Json(inventoryIssueQueryService.GetInventoryIssueBOQ(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_inventoryIssueService.GetDataByInventoryIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

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
        [Authorize, HttpPost]
        public JsonResult GETBoqFilter(string materialStorageId, List<Dictionary<string, object>> parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string materialIds = "''";
                string articleIds = "''";
                string tempQuery = "";
                
                if (parameters!=null)
                {
                    foreach (var item in parameters)
                    {
                        materialIds += ",'" + item["MaterialMasterId"].ToString() + "'";
                        articleIds += ",'" + item["ArticleId"].ToString() + "'";
                    }

                    if (articleIds!=null)
                    {
                        tempQuery = "AND IM.MaterialMasterId in ("+ materialIds + ") AND IM.ArticleId in ("+ articleIds + ")";
                    }
                }
                
                

                var sql = "";
                sql = @"SELECT DISTINCT Convert(BIT, 'False') IsActives
                            	,ISNULL(POD.InventoryReceiveId, '') POId
                            	,ISNULL(P.UserName, '') CustomerName
                            	,ISNULL(PO.ContractId, '') ContractId
                            	,ISNULL(mo.Id, '') MasterOrderId
                            	,ISNULL(cbi.SalesOrderId, '') SalesOrderId
								,ISNULL(CPO.PONumber,'') CustomerPONumber
                            	,ISNULL(boq.RMCustomerSpec, '') CustomerRefNo
                            	,ISNULL(boq.RMVendorSpec, '') VendorRefNo
                            	,ISNULL(boq.OwnReferenceNo, '') OwnReferenceNo
                            	,ISNULL(mo.PartyId,'')PartyId
                            	,v.UserName VendorName
                            FROM TRN.InventoryReceiveDetail IRD
							JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
							JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
							JOIN TRN.GRNPORequisitionAllocation GRA ON GRA.InventoryReceiveDetailId=IRD.Id
							JOIN BOQ boq ON boq.Id=GRA.BOQDetailId
                            LEFT JOIN MST.MaterialMaster mm ON mm.Id = boq.MaterialMasterId
                            LEFT JOIN MST.MaterialMasterArticle mma ON mma.Id = boq.ArticleId
                            LEFT JOIN TRN.MasterOrderItem moi ON moi.Id = boq.MasterOrderItemId
                            LEFT JOIN TRN.MasterOrder mo ON mo.Id = moi.MasterOrderId
							LEFT JOIN CostingBOQMaster cboqm on cboqm.Id=boq.CostingBOQMasterId
							LEFT JOIN CostingBOQItems cbi on cbi.CostingBOQMasterId=cboqm.Id
							LEFT JOIN TRN.SalesOrder SO ON SO.Id=cbi.SalesOrderId
							LEFT OUTER JOIN [TRN].[CustomerPO] CPO ON CPO.Id=SO.CustomerPOId
							LEFT JOIN TRN.ProductionOrderDetail PROD ON PROD.SalesOrderId=SO.Id
                            JOIN TRN.POBOQMAP pomap ON pomap.BOQDetailId = boq.Id
                            LEFT JOIN HKP.Party P ON P.Id = mo.PartyId
                            LEFT JOIN HKP.Party V ON V.Id = boq.VendorId
                            LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id = pomap.PODetailId
                            LEFT JOIN TRN.PurchaseOrder PO ON PO.Id = POD.InventoryReceiveId
							WHERE IR.[Status]='Posting' AND (IRD.BaseQty-IRD.BaseIssueQty)>0 --AND   SO.OrderStatusId='Active' 
                            AND IRD.MaterialStorageId='" + materialStorageId + "' and IR.PlantId='" + identity.PlantId + @"'
                          "+ tempQuery + @"   ";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpPost]
        public JsonResult GetSearchDistinctMaterialBOQ( string materialStorageId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            BOQQueryService bOQQueryService = new BOQQueryService(_sqlRepository);
            return Json(bOQQueryService.GetSearchDistinctMaterialBOQ(identity.CompanyId, identity.PlantId, materialStorageId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetSpecificMaterialStockBOQ(string pOId, string contractId, string masterOrderitemId, string salesOrderId, string issueDate, string materialStorageId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            BOQQueryService bOQQueryService = new BOQQueryService(_sqlRepository);
            return Json(bOQQueryService.GetSpecificMaterialStockBOQ(identity.CompanyId, identity.PlantId, pOId, contractId, masterOrderitemId, salesOrderId, issueDate, materialStorageId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateBOQIssue(string entities, string specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, string entitiesAll, string BoqAllocationList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            List<InventoryMaterialViewModel> entitiesVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entities);
            List<InventoryMaterialViewModel> specificStockListVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(specificStockList);
            List<InventoryMaterialViewModel> entitiesAllVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entitiesAll);
            List<InventoryIssueHistoryBOQ> BoqAllocationListVM = JsonConvert.DeserializeObject<List<InventoryIssueHistoryBOQ>>(BoqAllocationList);

            _inventoryIssueService.InsertGraphBOQ(entitiesVM, specificStockListVM, inventoryIssue, IssueTypeStatus, entitiesAllVM, BoqAllocationListVM);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult IssueDetailBOQDelete(string issueDetailId, string voucherId)
        {
            _inventoryIssueService.DeleteIssueDetailBOQ(issueDetailId, voucherId);
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
    }
}