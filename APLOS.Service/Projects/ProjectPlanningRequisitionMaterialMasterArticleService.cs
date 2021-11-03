#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
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
    public partial class ProjectPlanningRequisitionMaterialMasterArticleService : Service<ProjectPlanningRequisitionMaterialMasterArticle>, IProjectPlanningRequisitionMaterialMasterArticleService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<ProjectPlanningRequisitionMaterialMaster> _projectPlanningRequisitionMaterialMasterService;
        private readonly IRepositoryAsync<ProjectPlanningPORequisitionMaterialMasterArticle> _projectPlanningPORequisitionMaterialMasterArticle;
        private readonly IRepositoryAsync<MaterialMasterArticle> _materialMasterArticle;
        private readonly ISqlRepository _sqlRepository;

        public ProjectPlanningRequisitionMaterialMasterArticleService(
            IRepositoryAsync<ProjectPlanningRequisitionMaterialMasterArticle> projectPlanningRequisitionMaterialRepository,
            IPKGeneratorService pkGeneratorService,
            IRepositoryAsync<ProjectPlanningRequisitionMaterialMaster> projectPlanningRequisitionMaterialMasterService,
            IRepositoryAsync<ProjectPlanningPORequisitionMaterialMasterArticle> projectPlanningPORequisitionMaterialMasterArticle,
            IRepositoryAsync<MaterialMasterArticle> materialMasterArticle,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningRequisitionMaterialRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _projectPlanningRequisitionMaterialMasterService = projectPlanningRequisitionMaterialMasterService;
            _projectPlanningPORequisitionMaterialMasterArticle = projectPlanningPORequisitionMaterialMasterArticle;
            _materialMasterArticle = materialMasterArticle;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningRequisitionMaterialMasterArticle> entity, string requisitionMaterialMasterId)
        {
            var flag = false;
            try
            {
                //IEnumerable<ProjectPlanningPORequisitionMaterialMasterArticle> dbList = base.Query(r => r.ProjectPlanningPORequisitionMaterialMasterId == poMaterialMasterId).Select();
                _unitOfWork.BeginTransaction();
                flag = true;
                var dbList = Query(r => r.PPRequisitionMaterialMasterId == requisitionMaterialMasterId).Select().ToList();
                if (entity != null)
                {
                    //var requisitionMaterialMasterId = entity.First().ProjectPlanningRequisitionMaterialMasterId;
                    var uiTotalQuantity = entity.Sum(r => r.Quantity);
                    var requisitionMasterQ = _projectPlanningRequisitionMaterialMasterService.Query(r => r.Id == requisitionMaterialMasterId).Select(r => r.Quantity).FirstOrDefault();
                    var pk = GetMaxNumber(nameof(ProjectPlanningRequisitionMaterialMasterArticle), PKGeneratorEnum.Auto, null, DateTime.Now);
                    if (uiTotalQuantity > requisitionMasterQ)
                    {
                        throw new CustomException("Article total quantity can not be greater than requisitioin material quantity (" + requisitionMasterQ + ")");
                    }
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
                            InsertGraph(item);
                        }
                        else
                        {
                            if (dbList.Any(r => r.PPRequisitionMaterialMasterId == requisitionMaterialMasterId && r.Id == item.Id))
                            {
                                var poMArticle = _projectPlanningPORequisitionMaterialMasterArticle.Query(r => r.PPlanningRequisitionMaterialMasterArticleId == item.Id).Select().FirstOrDefault();
                                if (poMArticle != null)
                                {
                                    throw new CustomException(_materialMasterArticle.Find(item.PPReuisitionArticleId).StandardName + " This article is already used on PO " + poMArticle.ProjectPlanningPurchaseOrderId);
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

            //try
            //{
            //    if (entity != null)
            //    {
            //        var pk = base.GetMaxNumber("ProjectPlanningRequisitionMaterialMasterArticle", PKGeneratorEnum.Auto, null, DateTime.Now);
            //        foreach (var item in entity)
            //        {
            //            if (string.IsNullOrEmpty(item.Id))
            //            {
            //                pk.MaxNumber++;
            //                item.Id = pk.MaxNumber.ToString();
            //                item.ProjectPlanningRequisitionId = projectPlanningId;
            //                base.InsertGraph(item);
            //            }
            //            else if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ProjectPlanningRequisitionId))
            //            {
            //                base.UpdateGraph(item);
            //            }
            //            //base.InsertOrUpdateGraph(item);
            //        }
            //    }
            //}
            //catch (CustomException)
            //{
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    throw new CustomException(ex.Message, ex,
            //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
            //        null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            //}
        }

        public void DeleteGraph(string masterId)
        {
            try
            {
                var data = Query(r => r.PPRequisitionMaterialMasterId == masterId).Select().ToList();
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

        public ProjectPlanningRequisitionMaterialMasterArticle Get(string projectPlanningId)
        {
            return Query(r => r.ProjectPlanningRequisitionId == projectPlanningId).Select().FirstOrDefault();
        }

        public GridModel ProjectPlanningRequisitionMaterialMasterArticleList(GridParameter parameters)
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

        public GridModel ProjectPlanningRequisitionMaterialMasterArticleSavedList(GridParameter parameters, string projectPlanningRequisitionId)
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
										   FROM MST.ProjectPlanningRequisitionMaterialMasterArticle AS PPPOMM
										   LEFT OUTER JOIN [MST].[ProjectPlanningMaterialMaster] AS PPMM ON PPPOMM.ProjectPlanningMaterialMasterId=PPMM.Id
									LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPPOMM.MaterialMasterId= MM.Id
                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPMM.PlanningUOMId = UOMPP.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                                    LEFT OUTER JOIN (SELECT   pppo.ProjectPlanningMaterialMasterId,SUM(PPPO.ReverseQuantity) AS ReverseTotalQuantity FROM [MST].[ProjectPlanningMaterialMaster] AS PPMT
	                                    Right OUTER JOIN MST.ProjectPlanningRequisitionMaterialMasterArticle AS PPPO ON PPMT.Id=PPPO.ProjectPlanningMaterialMasterId
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

        public IEnumerable<Object> getUomList(string materialMasterId)
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