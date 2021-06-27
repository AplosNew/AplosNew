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
    public partial class ProjectPlanningMaterialMasterService : Service<ProjectPlanningMaterialMaster>, IProjectPlanningMaterialMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public ProjectPlanningMaterialMasterService(
            IRepositoryAsync<ProjectPlanningMaterialMaster> projectPlanningMaterialRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningMaterialRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningMaterialMaster> entity, string projectPlanningId)
        {
            try
            {
                if (entity != null)
                {
                    string _pk = GetPK();
                    var count = 0;
                    foreach (var item in entity)
                    {
                        count++;
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = _pk + "-" + count;
                            item.ProjectPlanningId = projectPlanningId;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ProjectPlanningId))
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

        private string GetPK()
        {
            return "PPMM-" + GetAutoNumber(nameof(ProjectPlanningMaterialMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string projectPlanningDetailId)
        {
            try
            {
                var data = Query(r => r.ProjectPlanningDetailId == projectPlanningDetailId).Select().ToList();
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

        public IEnumerable<Object> QueryForProjectPlanningMaterial(string plantId, string projectPlanningId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPD.*,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem   FROM [MST].[ProjectPlanning] AS PP
LEFT OUTER JOIN [MST].[ProjectPlanningMaterialMaster] AS PPD ON PP.Id=PPD.ProjectPlanningId
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

        public ProjectPlanningMaterialMaster Get(string projectPlanningId)
        {
            return Query(r => r.ProjectPlanningId == projectPlanningId).Select().FirstOrDefault();
        }

        public GridModel ProjectplanninMaterialMasterList(GridParameter parameters, string budgetMstId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //       parameters.CmdText = @"SELECT MM.Id,MM.Code,MM.UserName,  MT.Description AS MaterialType, MTN.Nature,  mm.IsAsset,
                //                                  MGP.UserName AS MaterialGroupMaster,
                //                                  UOMB.UserName AS BaseUom,
                //                                  MM.[Description],
                //                                  MM.BaseUOMId,
                //	   MM.AssetMasterId FixedAssetMasterId,
                //	   FAM.UserName FixedAssetMasterName
                //                           FROM [MST].[MaterialMaster] AS MM
                //                           LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                //LEFT OUTER JOIN HKP.MaterialTypeNature MTN ON MT.Id= MTN.MaterialTypeId
                //                           LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                //                           INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                //INNER JOIN MST.FixedAssetMaster AS FAM ON MM.AssetMasterId= FAM.Id
                //LEFT JOIN HKP.FixedAssetMasterBudgetTag FAT ON FAM.Id=FAT.FixedAssetMasterId
                //                           WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + "' AND MM.Archive = 0 AND MM.Active = 1 AND FAT.BudgetMasterId='" + budgetMstId + "' ";
                parameters.CmdText = @"SELECT MM.Id,MM.Code,MM.UserName,  MT.UserName AS MaterialType
	                                          ,mm.IsAsset,
                                              MGP.UserName AS MaterialGroupMaster,
                                              UOMB.UserName AS BaseUom,
                                              MM.[Description],
                                              MM.BaseUOMId,FAM.UserName FixedAssetMasterName,MMA.StandardName Article
                                     FROM [MST].[MaterialMaster] AS MM
                                     LEFT JOIN [MST].[MaterialMasterArticle] AS MMA ON MMA.MaterialMasterId = MM.Id
                                     LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                     LEFT JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                                     INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                                     LEFT JOIN HKP.FixedAssetMasterBudgetTag FAT ON MM.BudgetMasterId=FAT.BudgetMasterId
                                     LEFT JOIN [MST].[FixedAssetMaster] FAM ON FAM.Id=FAT.FixedAssetMasterId
                                     WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + "' AND MM.Archive = 0 AND MM.Active = 1 AND MM.IsAsset = 1 AND FAT.BudgetMasterId='" + budgetMstId + "' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel ProjectplanninMaterialMasterNonAssetList(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT MM.Id,MM.Code,MM.UserName,  MT.UserName AS MaterialType
	                                  ,mm.IsAsset,
                                      MGP.UserName AS MaterialGroupMaster,
                                      UOMB.UserName AS BaseUom,
                                      MM.[Description],
                                      MM.BaseUOMId,MMA.StandardName Article
                                    FROM [MST].[MaterialMaster] AS MM
                                    LEFT JOIN [MST].[MaterialMasterArticle] AS MMA ON MMA.MaterialMasterId = MM.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                                    WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + "' AND MM.Archive = 0 AND MM.Active = 1  AND MM.IsAsset=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel ProjectplanninMaterialMasterSavedList(GridParameter parameters, string projectPlanningDetailId)
        {
            try
            {
                var pdetail = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (projectPlanningDetailId != "undefined")
                {
                    pdetail = "AND PPMM.ProjectPlanningDetailId='" + projectPlanningDetailId + @"'";
                }
                //       parameters.CmdText = @"SELECT PPMM.Id, PPMM.ProjectPlanningDetailId,PPMM.MaterialMasterType,PPMM.ProjectPlanningId AS ProjectPlanningId,PPMM.Quantity, MM.Id AS MaterialMasterId,MM.Code,MM.UserName,  MT.Description AS MaterialType,
                //                                  PPMM.PlanningUOMId,
                //                                   FAM.UserName AS FixedAssetName,
                //		FAM.AssetType,
                //                                   MGP.UserName AS MaterialGroupMaster,
                //                                  PM.UserName AS ProductMaster,
                //                                  UOMB.UserName AS BaseUom,
                //	   UOMPP.UserName AS PlanningUOM,
                //                                  MM.[Description],
                //                                  MM.BaseUOMId,
                //	   MM.AssetMasterId FixedAssetMasterId,
                //	   FAM.UserName FixedAssetMasterName
                //	  --PPPOMT.ReverseTotalQuantity
                //	   FROM MST.ProjectPlanningMaterialMaster AS PPMM
                //--LEFT OUTER JOIN (SELECT  ProjectPlanningMaterialMasterId,SUM(ReverseQuantity) ReverseTotalQuantity  FROM [MST].ProjectPlanningPurchaseOrderMaterialMaster GROUP BY ProjectPlanningMaterialMasterId) AS PPPOMT ON PPMM.Id=PPPOMT.ProjectPlanningMaterialMasterId
                //LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPMM.MaterialMasterId= MM.Id
                //                           LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                //                           LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                //                           LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                //                           LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPMM.PlanningUOMId = UOMPP.Id
                //                           INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                //LEFT OUTER JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId= FAM.Id
                //                           WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + "'" + pdetail + "";
                parameters.CmdText = @"SELECT distinct PPMM.Id, PPMM.ProjectPlanningDetailId,PPMM.MaterialMasterType,PPMM.ProjectPlanningId AS ProjectPlanningId,PPMM.Quantity, MM.Id AS MaterialMasterId,MM.Code,MM.UserName,  MT.Description AS MaterialType,
                                           PPMM.PlanningUOMId,
                                            MGP.UserName AS MaterialGroupMaster,
                                           PM.UserName AS ProductMaster,
                                           UOMB.UserName AS BaseUom,
										   UOMPP.UserName AS PlanningUOM,
                                           MM.[Description],
                                           MM.BaseUOMId--,MMA.StandardName Article
										   ,FAM.UserName FixedAssetMasterName
										   FROM MST.ProjectPlanningMaterialMaster AS PPMM
									--LEFT OUTER JOIN (SELECT  ProjectPlanningMaterialMasterId,SUM(ReverseQuantity) ReverseTotalQuantity  FROM [MST].ProjectPlanningPurchaseOrderMaterialMaster GROUP BY ProjectPlanningMaterialMasterId) AS PPPOMT ON PPMM.Id=PPPOMT.ProjectPlanningMaterialMasterId
									LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPMM.MaterialMasterId= MM.Id
                                    --INNER JOIN [MST].[MaterialMasterArticle] AS MMA ON MMA.MaterialMasterId = MM.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPMM.PlanningUOMId = UOMPP.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAT ON MM.BudgetMasterId=FAT.BudgetMasterId
									LEFT JOIN [MST].[FixedAssetMaster] FAM ON FAM.Id=FAT.FixedAssetMasterId
                                    WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + "'" + pdetail + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel ProjectplanninMaterialMasterSavedListForRequisition(GridParameter parameters, string companyGroupId, string materialType, string projectPlanningId)
        {
            try
            {
                parameters.CmdText = @"SELECT PPMM.Id, PPMM.ProjectPlanningDetailId,PPMM.MaterialMasterType,PPMM.ProjectPlanningId AS ProjectPlanningId,PPMM.Quantity, MM.Id AS MaterialMasterId,MM.Code,MM.UserName,  MT.Description AS MaterialType,
                                           PPMM.PlanningUOMId,
                                            --FAM.UserName AS FixedAssetName,
											--FAM.AssetType,
                                            MGP.UserName AS MaterialGroupMaster,
                                           PM.UserName AS ProductMaster,
                                           UOMB.UserName AS BaseUom,
										   UOMPP.UserName AS PlanningUOM,
                                           MM.[Description],
                                           MM.BaseUOMId,
										   --MM.AssetMasterId FixedAssetMasterId,
										   --FAM.UserName FixedAssetMasterName,
										   PPRM.RaisedQuantity
										   ,PPRM.RaisedQuantity WithoutRaisedQuantity
										   FROM MST.ProjectPlanningMaterialMaster AS PPMM
									LEFT JOIN (SELECT ProjectPlanningMaterialMasterId,SUM(PlanningQuantity) RaisedQuantity FROM [MST].[ProjectPlanningRequisitionMaterialMaster] where ProjectPlanningId='" + projectPlanningId + @"'  GROUP BY ProjectPlanningMaterialMasterId) PPRM ON PPMM.Id= PPRM.ProjectPlanningMaterialMasterId
									LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPMM.MaterialMasterId= MM.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPMM.PlanningUOMId = UOMPP.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
									--LEFT OUTER JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId= FAM.Id
                                    WHERE MM.CompanyGroupId = '" + companyGroupId + "'	AND PPMM.MaterialMasterType='" + materialType + "' and PPMM.ProjectPlanningId='" + projectPlanningId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public GridModel ProjectplanninMaterialMasterSavedListForRequisition(GridParameter parameters,string companyGroupId, string materialType, string projectPlanningId)
        //{
        //    try
        //    {
        //        var sql = @"SELECT PPMM.Id, PPMM.ProjectPlanningDetailId,PPMM.MaterialMasterType,PPMM.ProjectPlanningId AS ProjectPlanningId,PPMM.Quantity, MM.Id AS MaterialMasterId,MM.Code,MM.UserName,  MT.Description AS MaterialType,
        //                                   PPMM.PlanningUOMId,
        //                                    FAM.UserName AS FixedAssetName,
        //			FAM.AssetType,
        //                                    MGP.UserName AS MaterialGroupMaster,
        //                                   PM.UserName AS ProductMaster,
        //                                   UOMB.UserName AS BaseUom,
        //		   UOMPP.UserName AS PlanningUOM,
        //                                   MM.[Description],
        //                                   MM.BaseUOMId,
        //		   MM.AssetMasterId FixedAssetMasterId,
        //		   FAM.UserName FixedAssetMasterName,
        //		   PPRM.RaisedQuantity
        //		   FROM MST.ProjectPlanningMaterialMaster AS PPMM
        //	LEFT JOIN (SELECT ProjectPlanningMaterialMasterId,SUM(Quantity) RaisedQuantity FROM [MST].[ProjectPlanningRequisitionMaterialMaster] where ProjectPlanningId='"+ projectPlanningId + @"'  GROUP BY ProjectPlanningMaterialMasterId) PPRM ON PPMM.Id= PPRM.ProjectPlanningMaterialMasterId
        //	LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPMM.MaterialMasterId= MM.Id
        //                            LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
        //                            LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
        //                            LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
        //                            LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPMM.PlanningUOMId = UOMPP.Id
        //                            INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
        //	LEFT OUTER JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId= FAM.Id
        //                            WHERE MM.CompanyGroupId = '" + companyGroupId+"'	AND PPMM.MaterialMasterType='"+materialType+ "' and PPMM.ProjectPlanningId='"+ projectPlanningId + "'";
        //        return _sqlRepository.GetGridData(parameters, sql);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
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
    }
}