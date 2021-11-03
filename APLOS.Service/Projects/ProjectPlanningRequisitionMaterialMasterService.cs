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
using Library.Service.Materials;
using Library.Service.Properties;
using Library.Service.Setups;
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
    public partial class ProjectPlanningRequisitionMaterialMasterService : Service<ProjectPlanningRequisitionMaterialMaster>, IProjectPlanningRequisitionMaterialMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IProjectPlanningRequisitionMaterialMasterArticleService _projectPlanningRequisitionMaterialMasterArticleService;
        private readonly IProjectPlanningPORequisitionMaterialMasterService _projectPlanningPORequisitionMaterialMasterService;
        private readonly IRepositoryAsync<ProjectPlanningMaterialMaster> _projectPlanningMaterialMasterService;
        private readonly IUOMConversionService _uOMConversionService;

        public ProjectPlanningRequisitionMaterialMasterService(
            IRepositoryAsync<ProjectPlanningRequisitionMaterialMaster> projectPlanningRequisitionMaterialRepository,
            IPKGeneratorService pkGeneratorService,
            IMaterialMasterService materialMasterService,
            IProjectPlanningRequisitionMaterialMasterArticleService projectPlanningRequisitionMaterialMasterArticleService,
            IProjectPlanningPORequisitionMaterialMasterService projectPlanningPORequisitionMaterialMasterService,
            IRepositoryAsync<ProjectPlanningMaterialMaster> projectPlanningMaterialMasterService,
            IUOMConversionService uOMConversionService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningRequisitionMaterialRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _projectPlanningRequisitionMaterialMasterArticleService = projectPlanningRequisitionMaterialMasterArticleService;
            _projectPlanningPORequisitionMaterialMasterService = projectPlanningPORequisitionMaterialMasterService;
            _materialMasterService = materialMasterService;
            _projectPlanningMaterialMasterService = projectPlanningMaterialMasterService;
            _uOMConversionService = uOMConversionService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningRequisitionMaterialMaster> entity, string projectPlanningRequisitionId, string projectPlanningId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var dbList = Query(r => r.ProjectPlanningRequisitionId == projectPlanningRequisitionId).Select().ToList();
                decimal dbSumQuantity;
                if (entity != null)
                {
                    var dbPPMaterialMasterList = Query(r => r.ProjectPlanningId == projectPlanningId).Select().ToList();
                    var pk = GetMaxNumber(nameof(ProjectPlanningRequisitionMaterialMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
                    decimal uomConvertedValue = 0;
                    decimal baseUoMConvertedValue = 0;
                    foreach (var item in entity)
                    {
                        if (item.Quantity <= 0)
                        {
                            throw new CustomException("Quantiy must be greater than 0");
                        }
                        var materialMasterName = _materialMasterService.Find(item.MaterialMasterId).UserName;
                        var planningInfo = _projectPlanningMaterialMasterService.Query(t => t.Id == item.ProjectPlanningMaterialMasterId).Select().FirstOrDefault();
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            if (Any(r => r.ProjectPlanningRequisitionId == item.ProjectPlanningRequisitionId && r.ProjectPlanningMaterialMasterId == item.ProjectPlanningMaterialMasterId))
                            {
                                throw new CustomException(materialMasterName + " - This asset item already added");
                            }
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.ProjectPlanningRequisitionId = projectPlanningRequisitionId;
                            item.ProjectPlanningId = projectPlanningId;
                            dbSumQuantity = dbPPMaterialMasterList.Where(r => r.ProjectPlanningId == projectPlanningId && r.ProjectPlanningMaterialMasterId == item.ProjectPlanningMaterialMasterId).Sum(r => r.Quantity);
                            uomConvertedValue = GetUomConversionValue(item.BaseUoMId, item.AlternativeUomId, item.Quantity, item.MaterialMasterId, planningInfo.PlanningUOMId);
                            baseUoMConvertedValue = GetUomConversionValue(item.AlternativeUomId, item.BaseUoMId, item.Quantity, item.MaterialMasterId);
                            if (item.BaseUoMId != item.AlternativeUomId)
                            {
                                item.BaseUoMQuantity = baseUoMConvertedValue;
                            }
                            else
                            {
                                item.BaseUoMQuantity = item.Quantity;
                            }
                            if (uomConvertedValue + dbSumQuantity > planningInfo.Quantity)
                            {
                                throw new CustomException(materialMasterName + " Requisition Quantity can not be greater than planning quantity ");
                            }
                            if (planningInfo.PlanningUOMId != item.AlternativeUomId)
                            {
                                item.PlanningQuantity = uomConvertedValue;
                            }
                            else
                            {
                                item.PlanningQuantity = item.Quantity;
                            }

                            InsertGraph(item);
                        }
                        else
                        {
                            if (dbList.Any(r => r.ProjectPlanningRequisitionId == projectPlanningRequisitionId && r.Id == item.Id))
                            {
                                var poM = _projectPlanningPORequisitionMaterialMasterService.Query(r => r.ProjectPlanningRequsitionMaterialMasterId == item.Id).Select().FirstOrDefault();
                                if (poM != null)
                                {
                                    throw new CustomException("This asset item is already used in PO " + poM.ProjectPlanningPurchaseOrderId);
                                }
                                dbSumQuantity = dbPPMaterialMasterList.Where(r => r.ProjectPlanningId == projectPlanningId && r.ProjectPlanningMaterialMasterId == item.ProjectPlanningMaterialMasterId).Sum(r => r.Quantity);
                                var dbItem = dbList.FirstOrDefault(r => r.ProjectPlanningRequisitionId == projectPlanningRequisitionId && r.Id == item.Id);
                                //var planningQ = _projectPlanningMaterialMasterService.Query(t => t.Id == item.ProjectPlanningMaterialMasterId).Select(t => t.Quantity).FirstOrDefault();
                                uomConvertedValue = GetUomConversionValue(item.BaseUoMId, item.AlternativeUomId, item.Quantity, item.MaterialMasterId, planningInfo.PlanningUOMId);
                                baseUoMConvertedValue = GetUomConversionValue(item.BaseUoMId, item.AlternativeUomId, item.Quantity, item.MaterialMasterId);
                                if ((uomConvertedValue + dbSumQuantity) - dbItem.Quantity > planningInfo.Quantity)
                                {
                                    throw new CustomException(materialMasterName + "Requisition Quantity can not be greater than planning quantity ");
                                }
                                if (item.BaseUoMId != item.AlternativeUomId)
                                {
                                    item.BaseUoMQuantity = baseUoMConvertedValue;
                                }
                                else
                                {
                                    item.BaseUoMQuantity = item.Quantity;
                                }
                                if (planningInfo.PlanningUOMId != item.AlternativeUomId)
                                {
                                    item.PlanningQuantity = uomConvertedValue;
                                }
                                else
                                {
                                    item.PlanningQuantity = item.Quantity;
                                }
                                UpdateGraph(item);
                            }
                            else
                            {
                                throw new CustomException(ServiceResources.RecordNoLonger);
                            }
                        }
                        //base.InsertOrUpdateGraph(item);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private decimal GetUomConversionValue(string baseUomId, string selectedUomId, decimal quantity, string materialMasterId, string planningUomId = null)
        {
            decimal qv = 0;
            Dictionary<string, object> uomConvertedInfo = (Dictionary<string, object>)GetMaterialUOMValueConversation(baseUomId, selectedUomId, Convert.ToInt32(quantity), materialMasterId, planningUomId).FirstOrDefault();
            if (uomConvertedInfo != null)
            {
                if (uomConvertedInfo.ContainsKey("ConvertedQuantity"))
                {
                    var a = uomConvertedInfo["ConvertedQuantity"].ToString();
                    qv = Convert.ToDecimal(a);
                }
            }
            return qv;
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                var data = Query(r => r.Id == id).Select().FirstOrDefault();
                if (data != null)
                {
                    var poReHas = _projectPlanningPORequisitionMaterialMasterService.Query(r => r.ProjectPlanningRequsitionMaterialMasterId == id).Select().FirstOrDefault();
                    if (poReHas != null)
                    {
                        throw new CustomException("This asset item is already used on PO " + poReHas.ProjectPlanningPurchaseOrderId);
                    }
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _projectPlanningRequisitionMaterialMasterArticleService.DeleteGraph(data.Id);
                    base.DeleteGraph(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteWithMaster(string Id)
        {
            try
            {
                var data = Query(r => r.ProjectPlanningRequisitionId == Id).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        Delete(data[i]);
                    }
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

        public ProjectPlanningRequisitionMaterialMaster Get(string projectPlanningId)
        {
            return Query(r => r.ProjectPlanningRequisitionId == projectPlanningId).Select().FirstOrDefault();
        }

        public GridModel ProjectPlanningRequisitionMaterialMasterList(GridParameter parameters)
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

        public IEnumerable<object> ProjectPlanningRequisitionMaterialMasterSavedList(string projectPlanningRequisitionId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //parameters.searchBy = "PlanningUOM";
                var sql = @"SELECT PPPOMM.Id
                                           ,PPPOMM.ProjectPlanningRequisitionId
                                           ,PPPOMM.ProjectPlanningMaterialMasterId, PPPOMM.AlternativeUomId
                                           ,PPPOMM.Quantity, PPMM.Quantity AS PlanningQuantity,PPPOMM.BaseUoMQuantity,PPPOMM.PlanningQuantity PlanningConvertedQuantity
										   ,RUOM.UserName RequsitionUoM
                                            ,PPPOMM.ProjectPlanningId
                                           ,MM.Id AS MaterialMasterId
                                           ,FAM.UserName AS FixedAssetName
										   ,FAM.AssetType
                                           ,PPMM.PlanningUOMId
										   ,UOMPP.UserName AS PlanningUOM
                                           ,MM.Code, MM.UserName
										   ,MA.ArticleCount
                                           ,MT.Description AS MaterialType, MGP.UserName AS MaterialGroupMaster
                                            ,PM.UserName AS ProductMaster
                                           ,UOMB.UserName AS BaseUom,PPPOMM.BaseUOMId, MM.[Description]
										   --,RQ.RaisedQuantity/UOMC.ToUOMFactor RaisedQuantity
                                            --,RaisedQuantity =case when PPPOMM.AlternativeUomId = PPPOMM.PlanningUoMId THEN RQ.RaisedQuantity ELSE RQ.RaisedQuantity/UOMC.BaseUOMFactor END
										   ,RQ.RaisedQuantity WithoutRaisedQuantity
										  ,RQ.RaisedQuantity
										   ,RARC.ReACount
										   FROM MST.ProjectPlanningRequisitionMaterialMaster AS PPPOMM
										   LEFT OUTER JOIN [MST].[ProjectPlanningMaterialMaster] AS PPMM ON PPPOMM.ProjectPlanningMaterialMasterId=PPMM.Id
										   LEFT JOIN (SELECT SUM(PlanningQuantity) RaisedQuantity,ProjectPlanningMaterialMasterId,ProjectPlanningId From [MST].[ProjectPlanningRequisitionMaterialMaster]
									GROUP BY ProjectPlanningMaterialMasterId,ProjectPlanningId) RQ ON PPPOMM.ProjectPlanningMaterialMasterId = RQ.ProjectPlanningMaterialMasterId AND PPPOMM.ProjectPlanningId=RQ.ProjectPlanningId
									LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPPOMM.MaterialMasterId= MM.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPMM.PlanningUOMId = UOMPP.Id
                                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS RUOM ON PPPOMM.AlternativeUomId = RUOM.Id
                                    LEFT OUTER JOIN [MST].[FixedAssetMaster] AS FAM ON MM.FixedAssetMasterId= FAM.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
									LEFT JOIN (SELECT SUM(Quantity) ReACount,PPRequisitionMaterialMasterId FROM [MST].[ProjectPlanningRequisitionMaterialMasterArticle] GROUP BY PPRequisitionMaterialMasterId) RARC ON PPPOMM.Id=RARC.PPRequisitionMaterialMasterId
	                                    LEFT JOIN (SELECT MaterialMasterId,COUNT(Id) ArticleCount
                                      FROM [MST].[MaterialMasterArticle]
                                       GROUP BY MaterialMasterId)  AS MA ON PPPOMM.MaterialMasterId=MA.MaterialMasterId
                                                                        WHERE PPPOMM.ProjectPlanningRequisitionId='" + projectPlanningRequisitionId + "' order by MM.UserName";

                return _sqlRepository.GetDataCollection(sql, null);
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

        public IEnumerable<Object> ProjectPlanningRequisitionMaterialMasterArticleSavedList(string projectPlanningRequisitionId, string projectPlanningMaterialMasterId)
        {
            var sql = @"SELECT A.Id,A.ProjectPlanningRequisitionId,A.PPRequisitionMaterialMasterId,UOM.UserName RequisitionUoM,A.MaterialMasterId,A.PPReuisitionArticleId,A.Quantity,MA.Code,MA.ShortName,MA.StandardName FROM [MST].[ProjectPlanningRequisitionMaterialMasterArticle] A
LEFT JOIN MST.MaterialMasterArticle MA ON A.PPReuisitionArticleId=MA.Id
LEFT JOIN MST.ProjectPlanningRequisitionMaterialMaster PPRMM ON A.PPRequisitionMaterialMasterId=PPRMM.Id
LEFT JOIN SCS.UnitOfMeasurement UOM ON PPRMM.AlternativeUomId=UOM.Id
                        WHERE A.ProjectPlanningRequisitionId='" + projectPlanningRequisitionId + "' AND A.PPRequisitionMaterialMasterId='" + projectPlanningMaterialMasterId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<Object> ProjectPlanningRequisitionMaterialMasterArticleSavedListForPO(string projectPlanningRequisitionId, string projectPlanningMaterialMasterId)
        {
            var sql = @"SELECT A.Id,A.ProjectPlanningRequisitionId,A.PPRequisitionMaterialMasterId,A.MaterialMasterId,A.PPReuisitionArticleId,A.Quantity,Isnull(q.RaisedQuantity,0) RaisedQuantity,MA.Code,MA.ShortName,MA.StandardName
 FROM [MST].[ProjectPlanningRequisitionMaterialMasterArticle] A
LEFT JOIN MST.MaterialMasterArticle MA ON A.PPReuisitionArticleId=MA.Id
left outer join (--1
SELECT Sum(Quantity) RaisedQuantity,ProjectPlanningRequisitionMaterialMasterId

						 FROM MST.ProjectPlanningPORequisitionMaterialMasterArticle

						WHERE ProjectPlanningRequisitionMaterialMasterId='" + projectPlanningMaterialMasterId + @"'
						group by ProjectPlanningRequisitionMaterialMasterId
						)-- 1
						q on A.PPRequisitionMaterialMasterId=q.ProjectPlanningRequisitionMaterialMasterId
                        WHERE A.ProjectPlanningRequisitionId='" + projectPlanningRequisitionId + "' AND A.PPRequisitionMaterialMasterId='" + projectPlanningMaterialMasterId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMaterialUOMValueConversation(string baseUomId, string selectedUomId, int quantity, string materialMasterId, string planningUomId = null)
        {
            try
            {
                var sql = "";
                if (planningUomId != null && baseUomId != selectedUomId)
                {
                    if (baseUomId != planningUomId)
                    {
                        sql = @"SELECT BaseUOMFactor*" + quantity + @"/(SELECT BaseUOMFactor FROM MST.MaterialMasterAlternativeUOM
                            WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + planningUomId + @"' and MaterialMasterId='" + materialMasterId + @"') ConvertedQuantity  FROM MST.MaterialMasterAlternativeUOM
                            WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + selectedUomId + "' and MaterialMasterId='" + materialMasterId + "'";
                    }
                    else
                    {
                        sql = @"SELECT BaseUOMFactor*" + quantity + @" ConvertedQuantity FROM MST.MaterialMasterAlternativeUOM
                                WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + selectedUomId + "' and MaterialMasterId='" + materialMasterId + "'";
                    }
                }
                if (baseUomId == selectedUomId)
                {
                    if (baseUomId == selectedUomId && selectedUomId == planningUomId)
                    {
                        sql = @"SELECT top 1 AlternativeUOMFactor*" + quantity + @" ConvertedQuantity FROM MST.MaterialMasterAlternativeUOM WHERE BaseUOMId='" + selectedUomId + "'  and MaterialMasterId='" + materialMasterId + "'";
                    }
                    else
                    {
                        sql = @"SELECT top 1 AlternativeUOMFactor *" + quantity + @"/(SELECT BaseUOMFactor FROM MST.MaterialMasterAlternativeUOM
                            WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + planningUomId + "'  and MaterialMasterId='" + materialMasterId + "') ConvertedQuantity FROM MST.MaterialMasterAlternativeUOM WHERE BaseUOMId='" + baseUomId + "'  and MaterialMasterId='" + materialMasterId + "'";
                    }
                }
                if (planningUomId == null)
                {
                    sql = @"select BaseUOMFactor*" + quantity + @" ConvertedQuantity from mst.MaterialMasterAlternativeUOM
                            WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + selectedUomId + "' and MaterialMasterId='" + materialMasterId + "'";
                }
                //var sql = @"SELECT UOMC.Id
                //             ,UOMC.BaseUOMId
                //             ,UOMC.BaseUOMFactor
                //             ,UOMC.AlternativeUOMId
                //             ,UOMC.AlternativeUOMFactor
                //             ,(UOMC.AlternativeUOMFactor *"+ quantity + @" ) ConvertedQuantity
                //           ,(" + quantity + @"  /UOMC.BaseUOMFactor) ReverseQuantity
                //            FROM [MST].[MaterialMasterAlternativeUOM] AS UOMC
                //                WHERE UOMC.BaseUOMId = '" + fromUOMId + "' AND UOMC.AlternativeUOMId = '"+ toUOMId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}