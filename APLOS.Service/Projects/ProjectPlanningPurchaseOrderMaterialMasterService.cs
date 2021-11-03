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
    public partial class ProjectPlanningPurchaseOrderMaterialMasterService : Service<ProjectPlanningPurchaseOrderMaterialMaster>, IProjectPlanningPurchaseOrderMaterialMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public ProjectPlanningPurchaseOrderMaterialMasterService(
            IRepositoryAsync<ProjectPlanningPurchaseOrderMaterialMaster> projectPlanningPurchaseOrderMaterialRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningPurchaseOrderMaterialRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningPurchaseOrderMaterialMaster> entity, string projectPlanningId)
        {
            try
            {
                if (entity != null)
                {
                    var pk = GetMaxNumber("SubMaterial", PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entity)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.ProjectPlanningPurchaseOrderId = projectPlanningId;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ProjectPlanningPurchaseOrderId))
                        {
                            UpdateGraph(item);
                        }
                        //base.InsertOrUpdateGraph(item);
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string projectPlanningPODetailId)
        {
            try
            {
                var data = Query(r => r.ProjectPlanningPurchaseOrderDetailId == projectPlanningPODetailId).Select().ToList();
                for (int i = 0; i < data.Count; i++)
                {
                    base.DeleteGraph(data[i]);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<Object> QueryForProjectPlanningPurchaseOrderMaterial(string plantId, string projectPlanningId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPD.*,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem   FROM [MST].[ProjectPlanning] AS PP
LEFT OUTER JOIN [MST].[ProjectPlanningPurchaseOrderMaterial] AS PPD ON PP.Id=PPD.ProjectPlanningId
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

        public ProjectPlanningPurchaseOrderMaterialMaster Get(string projectPlanningId)
        {
            return Query(r => r.ProjectPlanningPurchaseOrderId == projectPlanningId).Select().FirstOrDefault();
        }

        public GridModel ProjectPlanningPurchaseOrderMaterialMasterList(GridParameter parameters)
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

        public IEnumerable<object> ProjectPlanningPurchaseOrderMaterialMasterSavedList(string projectPlanningPurchaseOrderId, string ProjectPlanningRequisitionId, string projectPlanningId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT ppprmm.*
                                    ,MM.UserName
                                    ,MM.Code
									,FAM.UserName FixedAssetMaster
									,FAM.AssetType
									,PPRM.Quantity RequisitionQuantity
                                    ,UOMB.UserName AS BaseUom
									,UOMPR.UserName PoRequisitionUoM
									,UOMPP.UserName RequisitionUoM
									,POR.RaisedQuantity
									,avgr.ArtQuantity
									,avgr.ArtRate
									,avgr.ArtRate/avgr.ArtQuantity AvgRate
                                    from [MST].[ProjectPlanningPORequisitionMaterialMaster] AS ppprmm
                                    LEFT OUTER JOIN [MST].[ProjectPlanningRequisitionMaterialMaster] AS PPRM ON PPRM.Id = ppprmm.ProjectPlanningRequsitionMaterialMasterId
                                    LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPRM.MaterialMasterId= MM.Id
									LEFT JOIN MST.FixedAssetMaster AS FAM ON  MM.AssetMasterId=FAM.Id
                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPRM.AlternativeUomId = UOMPP.Id
                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPR ON ppprmm.AlternativeUomId = UOMPR.Id
									LEFT JOIN (SELECT SUM(RequisitionUoMQuantity) RaisedQuantity,ProjectPlanningRequsitionMaterialMasterId FROM MST.ProjectPlanningPORequisitionMaterialMaster   where ProjectPlanningId='PPC-94' group by ProjectPlanningRequsitionMaterialMasterId) POR ON ppprmm.ProjectPlanningRequsitionMaterialMasterId=POR.ProjectPlanningRequsitionMaterialMasterId
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
									LEFT JOIN (SELECT sum(Quantity) ArtQuantity,sum(Rate) ArtRate,ProjectPlanningPORequisitionMaterialMasterId  FROM [MST].[ProjectPlanningPORequisitionMaterialMasterArticle] group by  ProjectPlanningPORequisitionMaterialMasterId) avgr on ppprmm.Id=avgr.ProjectPlanningPORequisitionMaterialMasterId
                                    where ppprmm.projectPlanningPurchaseOrderId='" + projectPlanningPurchaseOrderId + "' and ppprmm.ProjectPlanningRequisitionId='" + ProjectPlanningRequisitionId + "' order by MM.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<Object> ProjectPlanningPORequisitionMaterialMasterArticleSavedList(string projectPlanningRequisitionId, string projectPlanningMaterialMasterId)
        {
            var sql = @"SELECT POA.Id,POA.Quantity,POA.Rate,POA.ProjectPlanningPORequisitionMaterialMasterId,UOM.UserName PoRequisitionUoM,POA.ProjectPlanningRequisitionMaterialMasterId,POA.ProjectPlanningPurchaseOrderId,q.RaisedQuantity,POA.PPlanningRequisitionMaterialMasterArticleId,POA.MaterialMasterArticleId,MA.Quantity RequisitionQuantity
                        ,MA.PPReuisitionArticleId,MA.ProjectPlanningRequisitionId,MMA.Code,MMA.ShortName,MMA.StandardName
                        FROM [MST].[ProjectPlanningPORequisitionMaterialMasterArticle] AS POA
                        LEFT JOIN [MST].[ProjectPlanningRequisitionMaterialMasterArticle] MA ON POA.PPlanningRequisitionMaterialMasterArticleId=MA.Id
                        LEFT JOIN MST.MaterialMasterArticle MMA ON POA.MaterialMasterArticleId=MMA.Id
                        LEFT JOIN [MST].[ProjectPlanningPORequisitionMaterialMaster] PORM ON POA.ProjectPlanningPORequisitionMaterialMasterId=PORM.Id
						LEFT JOIN SCS.UnitOfMeasurement UOM ON PORM.AlternativeUomId=UOM.Id
						left outer join (--1
SELECT Sum(Quantity) RaisedQuantity,ProjectPlanningRequisitionMaterialMasterId,PPlanningRequisitionMaterialMasterArticleId

						 FROM MST.ProjectPlanningPORequisitionMaterialMasterArticle

						WHERE ProjectPlanningPORequisitionMaterialMasterId='" + projectPlanningMaterialMasterId + @"'
						group by ProjectPlanningRequisitionMaterialMasterId,PPlanningRequisitionMaterialMasterArticleId
						)-- 1
						q on POA.ProjectPlanningRequisitionMaterialMasterId=q.ProjectPlanningRequisitionMaterialMasterId and poa.PPlanningRequisitionMaterialMasterArticleId=q.PPlanningRequisitionMaterialMasterArticleId
                        WHERE MA.ProjectPlanningRequisitionId='" + projectPlanningRequisitionId + "' AND POA.ProjectPlanningPORequisitionMaterialMasterId='" + projectPlanningMaterialMasterId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<Object> getUomList(string materialMasterId)
        {
            var sql = @"SELECT DISTINCT UOM1.Id AS UoMID,UOM1.UserName AS UoM,'' AS IsPo, MM.Id
                        FROM MST.MaterialMaster AS MM
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM1 ON MM.BaseUOMId = UOM1.Id
                        UNION
                        SELECT DISTINCT UOM2.Id AS UoMID,UOM2.UserName AS UoM,
                        IsPo=CASE WHEN MM.PurchaseOrderUOMId IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END, MM.Id
                        FROM MST.MaterialMaster AS MM
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM2 ON MM.PurchaseOrderUOMId = UOM2.Id
                        WHERE MM.PurchaseOrderUOMId IS NOT NULL
                        UNION
                        SELECT DISTINCT UOM3.Id AS UoMID,UOM3.UserName AS UoM,'' AS IsPo, MMALT.MaterialMasterId AS Id
                        FROM  MST.MaterialMasterAlternativeUOM AS MMALT
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM3 ON MMALT.AlternativeUOMId = UOM3.Id";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        //        public GridModel ProjectPlanningPurchaseOrderMaterialMasterSavedList(GridParameter parameters, string projectPlanningPODetailId)
        //        {
        //            try
        //            {
        //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //                var sql = @"SELECT PPPOMM.Id, PPPOMM.ProjectPlanningPurchaseOrderDetailId
        //                                           ,PPPOMM.ProjectPlanningPurchaseOrderId AS ProjectPlanningPurchaseOrderId
        //                                           ,PPPOMM.ProjectPlanningMaterialMasterId, PPPOMM.AlternativeUomId
        //                                           ,PPPOMM.Quantity, PPMM.Quantity AS PlanningQuantity
        //                                           ,PPPOMM.Rate, MM.Id AS MaterialMasterId
        //                                           ,PPMM.PlanningUOMId
        //										   ,UOMPP.UserName AS PlanningUOM
        //                                           ,MM.Code, MM.UserName
        //                                           ,MT.Description AS MaterialType, MGP.UserName AS MaterialGroupMaster
        //                                            ,PM.UserName AS ProductMaster
        //                                           ,UOMB.UserName AS BaseUom, MM.[Description]
        //                                           ,PPPOMM.ReverseQuantity
        //                                           ,sss.ReverseTotalQuantity - PPPOMM.ReverseQuantity as ReverseTotalQuantity
        //										   FROM MST.ProjectPlanningPurchaseOrderMaterialMaster AS PPPOMM
        //										   LEFT OUTER JOIN [MST].[ProjectPlanningMaterialMaster] AS PPMM ON PPPOMM.ProjectPlanningMaterialMasterId=PPMM.Id
        //									LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPPOMM.MaterialMasterId= MM.Id
        //                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
        //                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
        //                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
        //                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPMM.PlanningUOMId = UOMPP.Id
        //                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
        //LEFT OUTER JOIN (	SELECT   pppo.ProjectPlanningMaterialMasterId,SUM(PPPO.ReverseQuantity) AS ReverseTotalQuantity FROM [MST].[ProjectPlanningMaterialMaster] AS PPMT
        //	Right OUTER JOIN MST.ProjectPlanningPurchaseOrderMaterialMaster AS PPPO ON PPMT.Id=PPPO.ProjectPlanningMaterialMasterId
        //	group by  pppo.ProjectPlanningMaterialMasterId) as sss on PPPOMM.ProjectPlanningMaterialMasterId=sss.ProjectPlanningMaterialMasterId
        //                                    WHERE PPPOMM.ProjectPlanningPurchaseOrderDetailId='" + projectPlanningPODetailId + "'";
        //                return _sqlRepository.GetGridData(parameters, sql);
        //            }
        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //        }
    }
}