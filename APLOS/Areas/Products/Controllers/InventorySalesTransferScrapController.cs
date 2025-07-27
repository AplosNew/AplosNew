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
using Library.Accounting.Accounts;
using Aplos.MaterialManagement.MaterialQuery;

namespace Aplos.Areas.Products.Controllers
{
    public class InventorySalesTransferScrapController : BaseController 
    {
        #region Constructor

        private readonly IInventoryIssueService _inventoryIssueService;
        private readonly IInventoryIssueDetailService _inventoryDetailService;
        private readonly IInventoryMaterialService _inventoryMaterialService;
        private readonly IInventoryReceiveService _inventoryReveiveService;
        private readonly ISqlRepository _sqlRepository;

        public InventorySalesTransferScrapController(IInventoryIssueService inventoryIssueService 
            , IInventoryIssueDetailService inventoryDetailService
            , IInventoryMaterialService inventoryMaterialService
            , IInventoryReceiveService inventoryReveiveService
            , ISqlRepository sqlRepository)
        {
            _inventoryIssueService = inventoryIssueService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryReveiveService = inventoryReveiveService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Aplos
        [Authorize]
        public ActionResult InventorySales() 
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


        //[HttpGet]
        //public JsonResult GetDataByInventoryIssue()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
        //    var jsondata = Json(inventoryIssueQueryService.GetDataByInventoryIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
        //    jsondata.MaxJsonLength = int.MaxValue;
        //    return jsondata;
        //}


        [HttpGet]
        public JsonResult GetDataByInventoryReturnIssue() 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var jsondata = Json(inventoryIssueQueryService.GetDataByInventoryReturnIssue(identity.PlantId), JsonRequestBehavior.AllowGet);
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
                                && t.CompanyId == entity.CompanyId && t.PlantId == entity.PlantId).Select(t=>t.Id).FirstOrDefault();
            return Json(id, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// use in inventory issue journel
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        //[Authorize, HttpGet]
        //public JsonResult GetIssueList(GridParameter parameters)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryIssueService.GetIssueList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        //}

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
        public JsonResult GetInventoryMaterialIssueGLList(GridParameter parameters, string issueId)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetIssueMaterialGL(parameters, issueId,identity.CompanyId), JsonRequestBehavior.AllowGet);
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
        public JsonResult Create(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue ,string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.InsertGraph(entities, specificStockList, inventoryIssue, IssueTypeStatus, entitiesAll);
            return Json(new { inventoryIssue, Message = AplosMessage.Success +"Issue No="+ inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(string issueDetailId)
        {
            _inventoryIssueService.DeleteIssueDetail(issueDetailId,null);
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult IssueReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.InventoryIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

            return null;
        }


        [Authorize, HttpGet]
        public ActionResult AssetIssueReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.AssetIssueReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

            return null;
        }


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
        #endregion Operations

        #region AssetIssue
        [Authorize]
        public ActionResult AssetIssue()
        {
            return View("~/Areas/Products/Views/InventoryIssue/AssetIssue.cshtml");
        }

        [HttpGet]
        public JsonResult GetAssetInventoryIssue(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryIssueService.GetAssetInventoryIssue(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGRNFixedAssetList(string materialStorageId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetGRNFixedAssetList(identity.PlantId, materialStorageId,null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetAssetIssueSlipWithGRN(string materialStorageId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetAssetIssueSlipWithGRN(identity.PlantId, materialStorageId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertAssetIssue(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue )
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.InsertAssetInventoryIssue(entities, specificStockList, inventoryIssue);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
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
		public JsonResult GetApprovedIssueSlipDetails(string Id,string StorageLocationId,string OrderSpecific)
		{
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            return Json(inventoryIssueQueryService.GetApprovedIssueSlipDetails(Id, StorageLocationId, OrderSpecific), JsonRequestBehavior.AllowGet);
		}


        #endregion

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
        #region Issue Return
        [Authorize, HttpGet]
        public JsonResult IssueSlipMaterialAndArticleList(string fromDate,string toDate,string CostCenterId,string MaterialStorageId)  
        {
            string paramter = "";

            try
            {
                //var sql = @" select distinct CC.Id CostCenterId,CC.UserName AS CostCenterName ,MT.UserName MaterialType
                //            ,MGM.UserName AS MaterialGroupMasterName
                //            ,IM.MaterialMasterId
                //            ,MM.UserName MaterialMasterName
                //            , IM.ArticleId
                //            , ART.StandardName ArticleName
                //            , IM.FirstCharacteristicsId
                //            , FC.UserName AS FirstCharacteristics
                //            , IM.FirstCharacteristicsValueId
                //            , ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
                //            , IM.SecondCharacteristicsId
                //            , SC.UserName AS SecondCharacteristics
                //            , IM.SecondCharacteristicsValueId
                //            , ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
                //            , IM.ThirdCharacteristicsId
                //            , TC.UserName AS ThirdCharacteristics
                //            , IM.ThirdCharacteristicsValueId
                //            , ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
                //            ,0 Active,'Slip Article' ArticleType
                //            From TRN.IssueRequest AS IM
                //            left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                //            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                //            LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                //            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                //            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                //            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                //            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                //            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                //            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                //            LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                //            LEFT JOIN [TRN].[IssueRequestMaster] IRM ON  IRM.Id=IM.IssueRequestMasterId
                //            LEFT join[ORG].[CostCenter] CC On CC.Id=IM.CostCenterId
                //            Where   CAST(IRM.Addeddate AS DATE) between '" + fromDate + @"' and '"+toDate+"' and CC.Id='"+ CostCenterId + "' UNION ALL select distinct '' CostCenterId,'' CostCenterName ,MT.UserName MaterialType  , MGM.UserName AS MaterialGroupMasterName,IM.MaterialMasterId ,MM.UserName MaterialMasterName, IM.ArticleId , ART.StandardName ArticleName , IM.FirstCharacteristicsId , FC.UserName AS FirstCharacteristics , IM.FirstCharacteristicsValueId, ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue, IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId , ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue , IM.ThirdCharacteristicsId  , TC.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue ,0 Active,'All Article' ArticleType From TRN.InventoryMaterial AS IM left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id LEFT JOIN[HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id";//MM.IsAsset=0 And
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
                            Where CAST(IRM.IssueDate AS DATE) between '" + fromDate + @"' and '" + toDate + "' and CC.Id='" + CostCenterId + "' AND IRM.MaterialStorageId='"+ MaterialStorageId + "'";
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
        public JsonResult IssueSlipMaterialAndArticleListForIssued(string MaterialMasterId,string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId,string MaterialStorageId,string CostCenterId, string fromDate, string toDate)
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
                if(FirstCharacteristicsValueId == "'','null'")
                    FirstCharacteristicsValueId="'',''";
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
                    //if (FirstCharacteristicsValueId == "null")
                    //{
                    //    FirstCharacteristicsValueId = "";
                    //}
                    //if (SecondCharacteristicsValueId == "null")
                    //{
                    //    SecondCharacteristicsValueId = "";
                    //}
                    //if (ThirdCharacteristicsValueId == "null")
                    //{
                    //    SecondCharacteristicsValueId = "";
                    //}
                    sql = @"select cc.Id CostCenterId,cc.UserName CostCenterName, a.InventoryReceiveDetailId,IM.Id InventoryMaterialId,c.Id As IssuedId, REPLACE(CONVERT(CHAR(11), C.IssueDate, 106),' ','-') AS IssueDate,a.IssueRequestDetailId
                            ,IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                    ,IM.FirstCharacteristicsId, CH1.UserName AS Sku1, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicsValue
		                    ,IM.SecondCharacteristicsId, CH2.UserName AS Sku2, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicsValue
		                    ,IM.ThirdCharacteristicsId, CH3.UserName AS Sku3, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicsValue			
		                    ,b.BaseUOMId, UoM.UserName AS TransactionUoM--, b.AvgRate, b.AvgAmount, b.PolicyRate, b.PolicyAmount, b.[Policy]
		                    ,a.qty AS IssuedQty
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
                    //if (FirstCharacteristicsValueId=="null")
                    //{
                    //    FirstCharacteristicsValueId = "";
                    //}
                    //if (SecondCharacteristicsValueId == "null")
                    //{
                    //    SecondCharacteristicsValueId = "";
                    //}
                    //if (ThirdCharacteristicsValueId == "null")
                    //{
                    //    ThirdCharacteristicsValueId = "";
                    //}
                    sql = @"select cc.Id CostCenterId,cc.UserName CostCenterName, a.InventoryReceiveDetailId,IM.Id InventoryMaterialId,c.Id As IssuedId, REPLACE(CONVERT(CHAR(11), C.IssueDate, 106),' ','-') AS IssueDate,a.IssueRequestDetailId
                            ,IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                    ,IM.FirstCharacteristicsId, CH1.UserName AS Sku1, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicsValue
		                    ,IM.SecondCharacteristicsId, CH2.UserName AS Sku2, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicsValue
		                    ,IM.ThirdCharacteristicsId, CH3.UserName AS Sku3, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicsValue			
		                    ,b.BaseUOMId, UoM.UserName AS TransactionUoM--, b.AvgRate, b.AvgAmount, b.PolicyRate, b.PolicyAmount, b.[Policy]
		                    ,a.qty AS IssuedQty
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
            //if (specificStockList != null)
            //{
            //    foreach (var item in specificStockList)
            //    {

            //        if (!item.Active)
            //        {
            //            throw new CustomException("Please Select Materials !");

            //        }
            //        else if (item.TransactionQty.ToString() == "0")
            //        {
            //            throw new CustomException("Please Input The Current Qty !");
            //        }

            //    }
            //}
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.InsertGraphIssueReturn(entities, specificStockList, inventoryIssue, IssueTypeStatus);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Issue No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }
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

        [HttpGet]
        public JsonResult GetDataByPhysicalStockAdjustment()
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(inventoryIssueQueryService.GetDataByPhysicalStockAdjustment(identity.PlantId), JsonRequestBehavior.AllowGet);
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
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
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


        #region Order Ref No
        [HttpGet, Authorize]
        public JsonResult GetMasterOrderList(string contractId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //var sql = @"SELECT A.Id AS  MasterOrderId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, A.TotalQty	
                //                        ,A.TotalQtyUOMId,PL.UserName,C.Code Currency, 0 Active,B.UserName Buyer
                //			--, M.Amount
                //			--,M.CM
                //			--,M.SOQty 
                //			--,M.Qty
                //                        FROM [TRN].[MasterOrder] AS A
                //                        JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                //                        LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                //                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                //                        LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
                //                        --LEFT JOIN (SELECT SUM(SO.Amount) Amount,MO.Id,SO.CM,SO.Qty, SUM(SO.Qty) SOQty 
                //			--			FROM [TRN].[MasterOrder] MO
                //			--			LEFT JOIN TRN.MasterOrderItem MOI ON MOI.MasterOrderId=MO.Id
                //			--			LEFT JOIN (SELECT Qty, (Qty*Rate) Amount, MasterOrderItemId,CM
                //			--			FROM TRN.SalesOrder
                //			--			) SO ON SO.MasterOrderItemId=MOI.Id GROUP BY MO.Id,CM,Qty
                //			--) M ON M.Id=A.Id
                //                        WHERE A.CompanyId='" + identity.CompanyId + "'  AND A.PlantId='" + identity.PlantId + "' ORDER BY P.Id";//AND A.ContractId='" + contractId + "'

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
                                    ,A.ContractId ContractNo,MLC.Id MasterLCNo
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
                            LEFT JOIN dbo.Contract CNT ON CNT.Id=A.ContractId
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
                                    ,A.ContractId ContractNo,MLC.Id MasterLCNo
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
                            LEFT JOIN dbo.Contract CNT ON CNT.Id=A.ContractId
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



        //#region New Code Start Here for Inventory Sale/ Scrap / Transfer




        //#endregion





    }
}