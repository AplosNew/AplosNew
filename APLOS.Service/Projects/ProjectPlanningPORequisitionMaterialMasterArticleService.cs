#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Projects;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Projects
{
    /// <summary>
    ///  Class ProductService.
    /// </summary>
    public partial class ProjectPlanningPORequisitionMaterialMasterArticleService : Service<ProjectPlanningPORequisitionMaterialMasterArticle>, IProjectPlanningPORequisitionMaterialMasterArticleService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<ProjectPlanningRequisitionMaterialMasterArticle> _projectPlanningRequisitionMaterialMasterArticleService;
        private readonly IRepositoryAsync<ProjectPlanningPORequisitionMaterialMaster> _projectPlanningPurchaseOrderMaterialMasterService;

        public ProjectPlanningPORequisitionMaterialMasterArticleService(
            IRepositoryAsync<ProjectPlanningPORequisitionMaterialMasterArticle> projectPlanningRequisitionMaterialRepository,
            IPKGeneratorService pkGeneratorService,
            IRepositoryAsync<ProjectPlanningRequisitionMaterialMasterArticle> projectPlanningRequisitionMaterialMasterArticleService,
            IRepositoryAsync<ProjectPlanningPORequisitionMaterialMaster> projectPlanningPurchaseOrderMaterialMasterService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningRequisitionMaterialRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _projectPlanningRequisitionMaterialMasterArticleService = projectPlanningRequisitionMaterialMasterArticleService;
            _projectPlanningPurchaseOrderMaterialMasterService = projectPlanningPurchaseOrderMaterialMasterService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningPORequisitionMaterialMasterArticle> entity, string poMaterialMasterId)
        {
            var flag = false;
            try
            {
                //IEnumerable<ProjectPlanningPORequisitionMaterialMasterArticle> dbList = base.Query(r => r.ProjectPlanningPORequisitionMaterialMasterId == poMaterialMasterId).Select();
                _unitOfWork.BeginTransaction();
                flag = true;
                var dbList = Query(r => r.ProjectPlanningPORequisitionMaterialMasterId == poMaterialMasterId).Select().ToList();
                decimal dbSumQuantity;
                if (entity != null)
                {
                    var uiTotalQuantity = entity.Sum(r => r.Quantity);
                    var poMasterQ = _projectPlanningPurchaseOrderMaterialMasterService.Query(r => r.Id == poMaterialMasterId).Select(r => r.Quantity).FirstOrDefault();
                    if (uiTotalQuantity > poMasterQ)
                    {
                        throw new CustomException("Article total quantity can not be greater than PO material quantity (" + poMasterQ + ")");
                    }
                    var requisitionMaterialMasterId = entity.First().ProjectPlanningRequisitionMaterialMasterId;
                    var dbAllArticleList = Query(r => r.ProjectPlanningRequisitionMaterialMasterId == requisitionMaterialMasterId).Select().ToList();
                    var pk = GetMaxNumber(nameof(ProjectPlanningPORequisitionMaterialMasterArticle), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entity)
                    {
                        if (item.Quantity <= 0)
                        {
                            throw new CustomException("Quantity must be greater than 0");
                        }
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            dbSumQuantity = dbAllArticleList.Where(r => r.ProjectPlanningRequisitionMaterialMasterId == requisitionMaterialMasterId && r.PPlanningRequisitionMaterialMasterArticleId == item.PPlanningRequisitionMaterialMasterArticleId).Sum(r => r.Quantity);
                            var reQuisitionQ = _projectPlanningRequisitionMaterialMasterArticleService.Query(t => t.Id == item.PPlanningRequisitionMaterialMasterArticleId).Select(t => t.Quantity).FirstOrDefault();
                            if (item.Quantity + dbSumQuantity > reQuisitionQ)
                            {
                                throw new CustomException("Po Quantity can not be greater than requisition quantity ");
                            }
                            InsertGraph(item);
                        }
                        else
                        {
                            if (dbList.Any(r => r.ProjectPlanningPORequisitionMaterialMasterId == poMaterialMasterId && r.Id == item.Id))
                            {
                                dbSumQuantity = dbAllArticleList.Where(r => r.ProjectPlanningRequisitionMaterialMasterId == requisitionMaterialMasterId && r.PPlanningRequisitionMaterialMasterArticleId == item.PPlanningRequisitionMaterialMasterArticleId).Sum(r => r.Quantity);
                                var dbItem = dbList.FirstOrDefault(r => r.ProjectPlanningPORequisitionMaterialMasterId == poMaterialMasterId && r.Id == item.Id);
                                var reQuisitionQ = _projectPlanningRequisitionMaterialMasterArticleService.Query(t => t.Id == item.PPlanningRequisitionMaterialMasterArticleId).Select(t => t.Quantity).FirstOrDefault();
                                if ((item.Quantity + dbSumQuantity) - dbItem.Quantity > reQuisitionQ)
                                {
                                    throw new CustomException("Po Quantity can not be greater than requisition quantity ");
                                }
                                UpdateGraph(item);
                            }
                            else
                            {
                                throw new CustomException(ServiceResources.RecordNoLonger);
                            }
                        }
                    }
                }
                if (dbList.Count() > 0)
                {
                    if (entity == null)
                    {
                        foreach (var item in dbList)
                        {
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entity.Any(t => t.Id == item.Id))
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string masterId)
        {
            try
            {
                var data = Query(r => r.ProjectPlanningPORequisitionMaterialMasterId == masterId).Select().ToList();
                for (int i = 0; i < data.Count; i++)
                {
                    Delete(data[i]);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<Object> QueryForProjectPlanningRequisitionMaterial(string plantId, string projectPlanningId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPD.*,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem   FROM [MST].[ProjectPlanning] AS PP
LEFT OUTER JOIN [MST].[ProjectPlanningRequisitionMaterial] AS PPD ON PP.Id=PPD.ProjectPlanningId
LEFT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetItem, BG.UserName AS BudgetGroup, GLGI.AccountCode +' - '+GLGI.UserName AS GL FROM [MST].[BudgetMaster] BM
                                        LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                        INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
										) AS BM ON PPD.BudgetMasterId=BM.Id
WHERE PP.Id='" + projectPlanningId + @"' AND PP.PlantId='" + plantId + @"' AND PP.CompanyId='" + identity.CompanyId + "' ";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel ProjectPlanningPORequisitionMaterialMasterArticleList(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT MM.Id,MM.Code,MM.UserName,  MT.Description AS MaterialType, mt.Nature,  mm.IsAsset,
                                           MGP.UserName AS MaterialGroupMaster,
                                           MG.[Description] AS GridName,
                                           PM.UserName AS ProductMaster,
                                           UOMB.UserName AS BaseUom,
                                           MM.[Description],
                                           MM.MaterialGridId,
                                           MM.BaseUOMId
                                    FROM [MST].[MaterialMaster] AS MM
                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT OUTER JOIN[HKP].[MaterialGrid] AS MG ON MM.MaterialGridId = MG.Id
                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                                    WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1
                                    AND MM.MaterialTypeId in (SELECT Id FROM [HKP].[MaterialType] where Nature='Asset')
                                    union select MM.Id, MM.Code,MM.UserName, MT.Description AS MaterialType,  mt.Nature,  mm.IsAsset,
                                          MGP.UserName AS MaterialGroupMaster,
                                            MG.[Description] AS GridName,
                                            PM.UserName AS ProductMaster,
                                           UOMB.UserName AS BaseUom,
                                           MM.[Description],
                                           MM.MaterialGridId,
                                           MM.BaseUOMId
                                    from mst.[MaterialMaster] mm
			                        LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT OUTER JOIN[HKP].[MaterialGrid] AS MG ON MM.MaterialGridId = MG.Id
                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
			                        where IsAsset=1 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel ProjectPlanningPORequisitionMaterialMasterArticleSavedList(GridParameter parameters, string projectPlanningRequisitionId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.searchBy = "PlanningUOM";
                parameters.sort = "PlanningUOM";

                parameters.CmdText = @"SELECT PPPOMM.Id
                                           ,PPPOMM.ProjectPlanningRequisitionId
                                           ,PPPOMM.ProjectPlanningMaterialMasterId, PPPOMM.AlternativeUomId
                                           ,PPPOMM.Quantity, PPMM.Quantity AS PlanningQuantity
                                           ,PPPOMM.Rate, MM.Id AS MaterialMasterId
                                           ,PPMM.PlanningUOMId
										   ,UOMPP.UserName AS PlanningUOM
                                           ,MM.Code, MM.UserName
										   ,MA.ArticleCount
                                           ,MT.Description AS MaterialType, MGP.UserName AS MaterialGroupMaster
                                            ,PM.UserName AS ProductMaster
                                           ,UOMB.UserName AS BaseUom, MM.[Description]
                                           ,PPPOMM.ReverseQuantity
                                           ,sss.ReverseTotalQuantity - PPPOMM.ReverseQuantity as ReverseTotalQuantity
										   FROM MST.ProjectPlanningPORequisitionMaterialMasterArticle AS PPPOMM
										   LEFT OUTER JOIN [MST].[ProjectPlanningMaterialMaster] AS PPMM ON PPPOMM.ProjectPlanningMaterialMasterId=PPMM.Id
									LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPPOMM.MaterialMasterId= MM.Id
                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPMM.PlanningUOMId = UOMPP.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                                    LEFT OUTER JOIN (SELECT   pppo.ProjectPlanningMaterialMasterId,SUM(PPPO.ReverseQuantity) AS ReverseTotalQuantity FROM [MST].[ProjectPlanningMaterialMaster] AS PPMT
	                                    Right OUTER JOIN MST.ProjectPlanningPORequisitionMaterialMasterArticle AS PPPO ON PPMT.Id=PPPO.ProjectPlanningMaterialMasterId
	                                    group by  pppo.ProjectPlanningMaterialMasterId) as sss on PPPOMM.ProjectPlanningMaterialMasterId=sss.ProjectPlanningMaterialMasterId
	                                    LEFT JOIN (SELECT MaterialMasterId,COUNT(Id) ArticleCount
                                      FROM [ODYSSEYPOP].[MST].[MaterialMasterArticle]
                                       GROUP BY MaterialMasterId)  AS MA ON PPPOMM.MaterialMasterId=MA.MaterialMasterId
                                                                        WHERE PPPOMM.ProjectPlanningRequisitionId='" + projectPlanningRequisitionId + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<Object> GetUomList(string materialMasterId)
        {
            var sql = @"SELECT DISTINCT UOM1.Id AS UoMID,UOM1.UserName AS UoM,'' AS IsPo, MM.Id
                        FROM MST.MaterialMaster AS MM
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM1 ON MM.BaseUOMId = UOM1.Id
                        UNION
                        SELECT DISTINCT UOM2.Id AS UoMID,UOM2.UserName AS UoM,
                        IsPo=CASE WHEN MM.RequisitionUOMId IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END, MM.Id
                        FROM MST.MaterialMaster AS MM
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM2 ON MM.RequisitionUOMId = UOM2.Id
                        WHERE MM.RequisitionUOMId IS NOT NULL
                        UNION
                        SELECT DISTINCT UOM3.Id AS UoMID,UOM3.UserName AS UoM,'' AS IsPo, MMALT.MaterialMasterId AS Id
                        FROM  MST.MaterialMasterAlternativeUOM AS MMALT
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM3 ON MMALT.AlternativeUOMId = UOM3.Id";
            return _sqlRepository.GetDataCollection(sql, null);
        }
    }
}