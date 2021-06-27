#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Accounts;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Materials
{
    public partial class MaterialGroupMasterService : Service<MaterialGroupMaster>, IMaterialGroupMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<MaterialGroupProductionProcessGroup> _processGroupRepository;
        private readonly IRepositoryAsync<MaterialGroupArticle> _articleRepository;
        private readonly IRepositoryAsync<MaterialGroupArticleValue> _articleValueRepository;
        private readonly IRepositoryAsync<MaterialGroupArticlePrdProcessGroup> _articleProcessRepository;
        private readonly IRepositoryAsync<MaterialGroupProcessCriteria> _processCriteriaRepository;
        private readonly ICompanyGroupWiseMaterialGroupMasterService _companyGroupWiseMaterialGroupMasterService;
        private readonly IMaterialGroupAlternativeUoMService _alternativeUoMService;
        private readonly IMaterialGroupPackingFormService _packingFormService;
        private readonly ISqlRepository _sqlRepository;

        public MaterialGroupMasterService(
            IRepositoryAsync<MaterialGroupMaster> materialGroup4Repository
            , IRepositoryAsync<MaterialGroupArticle> articleRepository
            , IRepositoryAsync<MaterialGroupArticleValue> articleValueRepository
            , IRepositoryAsync<MaterialGroupArticlePrdProcessGroup> articleProcessRepository
            , IRepositoryAsync<MaterialGroupProductionProcessGroup> processGroupRepository
            , IRepositoryAsync<MaterialGroupProcessCriteria> processCriteriaRepository
            , IPKGeneratorService pkGeneratorService
            , ICompanyGroupWiseMaterialGroupMasterService companyGroupWiseMaterialGroupMasterService
            , IMaterialGroupAlternativeUoMService alternativeUoMService
            , IMaterialGroupPackingFormService packingFormService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialGroup4Repository, unitOfWork, pkGeneratorService)
        {
            _processGroupRepository = processGroupRepository;
            _articleRepository = articleRepository;
            _articleValueRepository = articleValueRepository;
            _articleProcessRepository = articleProcessRepository;
            _processCriteriaRepository = processCriteriaRepository;
            _pkGeneratorService = pkGeneratorService;
            _companyGroupWiseMaterialGroupMasterService = companyGroupWiseMaterialGroupMasterService;
            _unitOfWork = unitOfWork;
            _alternativeUoMService = alternativeUoMService;
            _packingFormService = packingFormService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Material Group

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(MaterialGroupMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void InsertGraph(MaterialGroupMaster entity, IEnumerable<MaterialGroupAlternativeUoM> altUoMList, IEnumerable<MaterialGroupPackingForm> packing, IEnumerable<MaterialGroupProductionProcessGroup> processGroupList)
        {
            var flag = false;
            try
            {
                Check(entity);
                entity.Id = GetPK();
                _companyGroupWiseMaterialGroupMasterService.InsertGraph(entity.Id);
                _alternativeUoMService.InsertOrUpdateGraph(altUoMList, entity.Id);
                _packingFormService.InsertOrUpdateGraph(packing, entity.Id);
                base.InsertGraph(entity);
                if (processGroupList != null)
                {
                    var count = _processGroupRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[MaterialGroupProductionProcessGroup] WHERE MaterialGroupMasterId='{entity.Id}'").First();
                    foreach (var item in processGroupList)
                    {
                        count++;
                        item.Id = MakePK(entity.Id, count, 2);
                        item.MaterialGroupMasterId = entity.Id;
                        AuditService.AddedLog(item);
                        _processGroupRepository.Insert(item);
                    }
                }
                _unitOfWork.BeginTransaction();
                flag = true;
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
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name,
                false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        private void Check(MaterialGroupMaster entity)
        {
            CheckUniqueColumn(UniqueColumnName.UserName, entity.Code, r => r.Id != entity.Id && r.MaterialTypeId == entity.MaterialTypeId && r.Code == entity.Code && r.Active && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.MaterialTypeId == entity.MaterialTypeId && r.UserName == entity.UserName && r.Active && !r.Archive);
        }

        public void UpdateGraph(MaterialGroupMaster entity, IEnumerable<MaterialGroupAlternativeUoM> altUoMList, IEnumerable<MaterialGroupPackingForm> packing, IEnumerable<MaterialGroupProductionProcessGroup> processGroupList)
        {
            var flag = false;
            try
            {
                Check(entity);
                _alternativeUoMService.InsertOrUpdateGraph(altUoMList, entity.Id);
                _packingFormService.InsertOrUpdateGraph(packing, entity.Id);

                InsertOrUpdateProcessGroup(entity, processGroupList);
                base.UpdateGraph(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
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
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name,
                false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void DeleteGraph(string key)
        {
            var flag = false;
            try
            {
                var entity = Find(key);
                _companyGroupWiseMaterialGroupMasterService.DeleteGraph(entity.Id);
                _alternativeUoMService.DeleteGraph(entity.Id);
                _packingFormService.DeleteGraph(entity.Id);
                var groupDbList = _processGroupRepository.SqlQuery<MaterialGroupProductionProcessGroup>($"SELECT * FROM [MST].[MaterialGroupProductionProcessGroup] WHERE MaterialGroupMasterId='{key}'").ToList();
                foreach (var item in groupDbList)
                {
                    item.ModelState = ModelState.Deleted;
                    _processGroupRepository.Delete(item);
                }
                DeleteGraph(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name,
                    false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public IEnumerable<object> GetMaterialGroupMasterCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = $"SELECT op.Id AS Value, op.UserName as Text FROM {DbSchema.Masters}.[{DbTable.MaterialGroupMaster}] AS op " +
                          $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseMaterialGroupMaster}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgu " +
                          $"ON op.Id = cgu.MaterialGroupMasterId  WHERE ISNULL(cgu.Id, '')<> '' AND  op.Archive=0 AND op.Active=1 ORDER BY op.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false,
                    ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetHierarchy(GridParameter parameters, string id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = "SELECT MG1.UserName + ISNULL(+' > '+ MG2.UserName,'') + ISNULL(+' > '+ MG3.UserName,'') + ISNULL(+' > '+ MG4.UserName,'') AS Hierarchy " +
                                     $"FROM {DbSchema.Masters}.[{DbTable.MaterialGroupMaster}] AS MGM " +
                                     $"LEFT JOIN {DbSchema.HKP}.[{DbTable.MaterialGroup1}] AS MG1 ON MG1.Id=MGM.MaterialGroup1Id " +
                                     $"LEFT JOIN {DbSchema.HKP}.[{DbTable.MaterialGroup2}] AS MG2 ON MG2.Id=MGM.MaterialGroup2Id " +
                                     $"LEFT JOIN {DbSchema.HKP}.[{DbTable.MaterialGroup3}] AS MG3 ON MG3.Id=MGM.MaterialGroup3Id " +
                                     $"LEFT JOIN {DbSchema.HKP}.[{DbTable.MaterialGroup4}] AS MG4 ON MG4.Id=MGM.MaterialGroup4Id " +
                                     $"LEFT OUTER JOIN(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseMaterialGroupMaster}] " +
                                                       $"WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgu  ON MGM.Id = cgu.MaterialGroupMasterId  " +
                                     $"WHERE ISNULL(cgu.Id, '')<> '' AND  MGM.Archive= 0 AND MGM.Id= '{id}' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false,
                    ModuleEnum.Material.ToString()));
            }
        }

        public override IQueryFluent<MaterialGroupMaster> Query()
        {
            return base.Query(r => r.Active && !r.Archive).Include(r => r.MaterialAttributeMasters);
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT MGM.*,UM.UserName AS BaseUOM
							, MG1.UserName AS MaterialGroup1Name, MG2.UserName AS MaterialGroup2Name
							, MG3.UserName AS MaterialGroup3Name, MG4.UserName AS MaterialGroup4Name
							--, MT.Description AS MaterialTypeName
							, MT.UserName AS MaterialTypeName
						FROM MST.[MaterialGroupMaster] AS MGM
						LEFT JOIN [HKP].[CompanyGroupWiseMaterialGroupMaster] AS CMGM ON CMGM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN [HKP].[MaterialType] AS MT ON MT.Id=MGM.MaterialTypeId
						LEFT JOIN SCS.UnitOfMeasurement UM ON MGM.BaseUoMId=UM.Id
						LEFT JOIN [HKP].[MaterialGroup1] AS MG1 ON MGM.MaterialGroup1Id=MG1.Id
						LEFT JOIN [HKP].[MaterialGroup2] AS MG2 ON MGM.MaterialGroup2Id=MG2.Id
						LEFT JOIN [HKP].[MaterialGroup3] AS MG3 ON MGM.MaterialGroup3Id=MG3.Id
						LEFT JOIN [HKP].[MaterialGroup4] AS MG4 ON MGM.MaterialGroup4Id=MG4.Id
						WHERE CMGM.CompanyGroupId='" + companyGroupId + "' AND MGM.Archive = 0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false,
                    ModuleEnum.Material.ToString()));
            }
        }

        /// <summary>
        /// This cbo go to fabric roll management.
        /// </summary>
        /// <param name="companyGroupId"></param>
        /// <returns></returns>
        public IEnumerable<ComboModel> GetCboByMaterialMaster(string companyGroupId)
        {
            var _sql = @"SELECT Id, UserName FROM MST.MaterialGroupMaster WHERE Id IN (SELECT DISTINCT MaterialGroupMasterId FROM MST.MaterialMaster WHERE CompanyGroupId='" + companyGroupId + "') ORDER BY UserName";
            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public GridModel GetListByMaterialType(GridParameter parameters, string materialTypeId, string companyGroupId)
        {
            try
            {
                var str = "";
                if (!string.IsNullOrEmpty(materialTypeId))
                    str = " AND MGM.MaterialTypeId='" + materialTypeId + "'";
                parameters.CmdText = @"SELECT MGM.Id
									, MGM.Code, MGM.UserName, MGM.HSNCodeId
									, MGM.MaterialGroup1Id, MG1.UserName AS MaterialGroup1Name
									, MGM.MaterialGroup2Id, MG2.UserName AS MaterialGroup2Name
									, MGM.MaterialGroup3Id, MG3.UserName AS MaterialGroup3Name
									, MGM.MaterialGroup4Id, MG4.UserName AS MaterialGroup4Name
									, MGM.MaterialTypeId, MT.[Description] AS MaterialTypeName
									, MGM.Active
							FROM MST.[MaterialGroupMaster] AS MGM
							LEFT JOIN [HKP].[CompanyGroupWiseMaterialGroupMaster] AS CMGM ON CMGM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MT.Id=MGM.MaterialTypeId
							LEFT JOIN [HKP].[MaterialGroup1] AS MG1 ON MGM.MaterialGroup1Id=MG1.Id
							LEFT JOIN [HKP].[MaterialGroup2] AS MG2 ON MGM.MaterialGroup2Id=MG2.Id
							LEFT JOIN [HKP].[MaterialGroup3] AS MG3 ON MGM.MaterialGroup3Id=MG3.Id
							LEFT JOIN [HKP].[MaterialGroup4] AS MG4 ON MGM.MaterialGroup4Id=MG4.Id
							WHERE CMGM.CompanyGroupId='" + companyGroupId + "' AND MGM.Archive = 0 " + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetListByFinishedGoods(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT MGM.Id
	                              ,MGM.Code
	                              ,MGM.UserName
                                  ,MGM.HSNCodeId
	                              ,MGM.MaterialGroup1Id
	                              ,(SELECT UserName FROM HKP.[MaterialGroup1] WHERE MGM.MaterialGroup1Id = Id
                                   AND Archive = 0) AS MaterialGroup1Name
	                              ,MGM.MaterialGroup2Id
                                  ,(SELECT UserName FROM HKP.[MaterialGroup2] WHERE MGM.MaterialGroup2Id = Id
                                   AND Archive = 0) AS MaterialGroup2Name
	                              ,MGM.MaterialGroup3Id
                                  ,(SELECT UserName FROM HKP.[MaterialGroup3] WHERE MGM.MaterialGroup3Id = Id
                                   AND Archive = 0) AS MaterialGroup3Name
	                              ,MGM.MaterialGroup4Id
                                  ,(SELECT UserName FROM HKP.[MaterialGroup4] WHERE MGM.MaterialGroup4Id = Id
                                   AND Archive = 0) AS MaterialGroup4Name
	                              ,MGM.MaterialTypeId
                                  ,MT.Description AS MaterialTypeName
	                              ,MGM.Active
                            FROM MST.[MaterialGroupMaster] AS MGM
                            LEFT OUTER JOIN (SELECT * FROM HKP.[CompanyGroupWiseMaterialGroupMaster] WHERE CompanyGroupId = '" + companyGroupId + @"') cgu ON MGM.Id = cgu.MaterialGroupMasterId
                            LEFT OUTER JOIN HKP.MaterialType AS MT ON MT.Id=MGM.MaterialTypeId
                            LEFT OUTER JOIN HKP.MaterialTypeNature AS MTN ON MTN.MaterialTypeId=MT.Id
                            WHERE ISNULL(cgu.Id, '') <> '' AND MGM.Archive = 0 AND MTN.Nature='" + EnumMaterialTypeNatureList.FinishedGoods + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        #endregion Material Group

        #region Article

        private string GetArticlePK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(MaterialGroupArticle), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertOrUpdateArticleGraph(MaterialGroupArticle article, IEnumerable<MaterialGroupArticleValue> valueList
            , IEnumerable<MaterialGroupArticlePrdProcessGroup> processGroupList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (valueList == null) throw new CustomException("Please insert attribute.");

                if (string.IsNullOrEmpty(article.Id))
                {
                    article.Id = GetArticlePK();
                    AuditService.AddedLog(article);
                    _articleRepository.Insert(article);
                }
                else
                {
                    AuditService.AddedLog(article);
                    _articleRepository.Update(article);
                }
                var count = _processCriteriaRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[MaterialGroupArticleValue] WHERE MaterialGroupArticleId='{article.Id}'").First();
                var localValueList = valueList.ToList();
                foreach (var value in valueList)
                {
                    //if (string.IsNullOrEmpty(value.Id) && string.IsNullOrEmpty(value.Id))
                    //{
                    //    count++;
                    //    value.Id = MakePK(article.Id, count, 2);
                    //    value.MaterialGroupArticleId = article.Id;
                    //    AuditService.AddedLog(value);
                    //    _articleValueRepository.Insert(value);
                    //}
                    //else
                    //{
                    //    AuditService.UpdatedLog(value);
                    //    _articleValueRepository.Update(value);
                    //}

                    if (string.IsNullOrEmpty(value.Id))//Insert
                    {
                        if (!string.IsNullOrEmpty(value.MaterialAttributeValueId) && !string.IsNullOrEmpty(value.ValueFreeText))
                        {
                            count++;
                            SetAttributeValueId(value);
                            value.Id = MakePK(article.Id, count, 2);
                            value.MaterialGroupArticleId = article.Id;
                            AuditService.AddedLog(value);
                            _articleValueRepository.Insert(value);
                        }
                    }
                    else //Edit or Delete
                    {
                        if (string.IsNullOrEmpty(value.MaterialAttributeValueId) && string.IsNullOrEmpty(value.ValueFreeText))
                            _articleValueRepository.Delete(value);
                        else
                        {
                            SetAttributeValueId(value);
                            AuditService.UpdatedLog(value);
                            _articleValueRepository.Update(value);
                        }
                    }
                }
                if (processGroupList.IsNotNull())
                {
                    var articleProcessId = _articleProcessRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[MaterialGroupArticlePrdProcessGroup] WHERE MaterialGroupArticleId='{article.Id}'").First();
                    var dbCriteriaList = _processCriteriaRepository.Query(t => t.MaterialGroupArticleId == article.Id).Select().ToList();
                    foreach (var group in processGroupList)
                    {
                        if (string.IsNullOrEmpty(group.Id))
                        {
                            articleProcessId++;
                            group.Id = MakePK(article.Id, articleProcessId, 2);
                            group.MaterialGroupArticleId = article.Id;
                            AuditService.AddedLog(group);
                            if (group.CriteriaList != null && group.CriteriaList.Count() > 0)
                            {
                                var id = _processCriteriaRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[MaterialGroupProcessCriteria] WHERE MaterialGroupArticlePrdProcessGroupId='{articleProcessId}'").First();
                                foreach (var item in group.CriteriaList)
                                {
                                    id++;
                                    item.Id = MakePK(group.Id, id, 2);
                                    item.MaterialGroupArticlePrdProcessGroupId = group.Id;
                                    item.MaterialGroupArticleId = article.Id;
                                    AuditService.AddedLog(item);
                                    _processCriteriaRepository.Insert(item);
                                }
                            }
                            _articleProcessRepository.Insert(group);
                        }
                        else
                        {
                            AuditService.UpdatedLog(group);
                            if (group.CriteriaList != null && group.CriteriaList.Count() > 0)
                            {
                                var id = _processCriteriaRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[MaterialGroupProcessCriteria] WHERE MaterialGroupArticlePrdProcessGroupId='{group.Id}'").First();
                                foreach (var item in group.CriteriaList)
                                {
                                    if (string.IsNullOrEmpty(item.Id))
                                    {
                                        id++;
                                        item.Id = MakePK(group.Id, id, 2);
                                        item.MaterialGroupArticlePrdProcessGroupId = group.Id;
                                        item.MaterialGroupArticleId = article.Id;
                                        item.ModelState = ModelState.Added;
                                        AuditService.AddedLog(item);
                                        _processCriteriaRepository.Insert(item);
                                    }
                                    else
                                    {
                                        item.ModelState = ModelState.Modified;
                                        AuditService.UpdatedLog(item);
                                        _processCriteriaRepository.Update(item);
                                    }
                                }
                            }
                            _articleProcessRepository.Update(group);
                        }

                        //var filterdbCriteriaList = _processCriteriaRepository.Query(t => t.MaterialGroupArticlePrdProcessGroupId == group.Id).Select().ToList();
                        //if (filterdbCriteriaList.Count()>0)
                        //{
                        //    foreach (var item in filterdbCriteriaList)
                        //    {
                        //        if (!group.CriteriaList.Any(t => t.Id == item.Id))
                        //            base.DeleteGraph(item);
                        //    }
                        //}
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
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public GridModel GetArticleList(GridParameter parameters, string mGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id, A.MaterialGroupMasterId, A.Code, A.ShortName, A.StandardName, B.UserName AS MaterialGroupMasterName
                    FROM [MST].[MaterialGroupArticle] AS A JOIN [MST].[MaterialGroupMaster] AS B ON A.MaterialGroupMasterId=B.Id WHERE B.Id='" + mGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private static void SetAttributeValueId(MaterialGroupArticleValue item)
        {
            if (item.MaterialAttributeValueId.IsNotNull()) item.ValueFreeText = null;
            else
            {
                if (item.ValueFreeText.IsNull()) throw new CustomException("Free Text can not be null");
            }
        }

        #endregion Article

        #region Production Process Group

        public GridModel GetProductProcessGroupList(GridParameter parameters, string groupId, string[] ids)
        {
            try
            {
                parameters.CmdText = @"SELECT NULL AS Id,Id AS ProductionProcessGroupId,Code,ShortName,UserName AS ProdProcessGroupName
                    FROM HKP.ProductionProcessGroup WHERE CompanyGroupId='" + groupId + "' AND Id NOT IN(" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetMaterialProductProcessGroupList(string masterId)
        {
            try
            {
                var sql = @"SELECT A.Id, A.MaterialGroupMasterId, A.ProductionProcessGroupId, A.InputId, C.UserName AS Input, B.Code
                    , B.ShortName, B.UserName AS ProdProcessGroupName, NULL CriteriaList FROM MST.MaterialGroupProductionProcessGroup AS A
                    JOIN HKP.ProductionProcessGroup AS B ON A.ProductionProcessGroupId=B.Id
                    LEFT JOIN HKP.ProductionProcessGroup AS C ON A.InputId=C.Id WHERE A.MaterialGroupMasterId='" + masterId + "' ORDER BY A.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetMaterialPrdGroupList(string mgMasterId, string articleId)
        {
            try
            {
                var sql = @"SELECT D.Id, A.Id AS MaterialGroupProductionProcessGroupId, D.MaterialGroupArticleId, A.MaterialGroupMasterId
                            , A.ProductionProcessGroupId, A.InputId, C.UserName AS Input
                            , B.Code, B.ShortName, B.UserName AS ProdProcessGroupName, D.OutSourceCost, D.Wastage, NULL CriteriaList
                        FROM [MST].[MaterialGroupProductionProcessGroup] AS A
                        JOIN [HKP].[ProductionProcessGroup] AS B ON A.ProductionProcessGroupId=B.Id
                        LEFT JOIN [HKP].[ProductionProcessGroup] AS C ON A.InputId=C.Id
                        LEFT JOIN (SELECT * FROM [MST].[MaterialGroupArticlePrdProcessGroup] WHERE MaterialGroupArticleId='" + articleId + @"') AS D ON D.MaterialGroupProductionProcessGroupId=A.Id
                        WHERE A.MaterialGroupMasterId='" + mgMasterId + "' ORDER BY A.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private void InsertOrUpdateProcessGroup(MaterialGroupMaster entity, IEnumerable<MaterialGroupProductionProcessGroup> processGroupList)
        {
            if (processGroupList != null)
            {
                var count = _processGroupRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[MaterialGroupProductionProcessGroup] WHERE MaterialGroupMasterId='{entity.Id}'").First();
                foreach (var item in processGroupList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        count++;
                        item.Id = MakePK(entity.Id, count, 2);
                        item.MaterialGroupMasterId = entity.Id;
                        AuditService.AddedLog(item);
                        _processGroupRepository.Insert(item);
                    }
                    else _processGroupRepository.Update(item);
                }
            }
            var groupDbList = _processGroupRepository.SqlQuery<MaterialGroupProductionProcessGroup>($"SELECT * FROM [MST].[MaterialGroupProductionProcessGroup] WHERE MaterialGroupMasterId='{entity.Id}'").ToList();
            if (groupDbList != null && groupDbList.Count() > 0)
            {
                if (processGroupList == null)
                {
                    foreach (var item in groupDbList)
                    {
                        _processGroupRepository.Delete(item);
                    }
                }
                else
                {
                    foreach (var item in groupDbList)
                    {
                        if (!processGroupList.Any(t => t.Id == item.Id))
                            _processGroupRepository.Delete(item);
                    }
                }
            }
        }

        #endregion Production Process Group

        #region Process Criteria

        public IEnumerable<object> GetProcessCriteriaList(string id)
        {
            try
            {
                var sql = @"SELECT  A.Id, A.MaterialGroupArticleId, A.MaterialGroupArticlePrdProcessGroupId, A.ProcessCriteriaId, A.Wastage, A.Rate
                            , B.Code, B.ShortName, B.StandardName, B.UserName FROM [MST].[MaterialGroupProcessCriteria] AS A
                        JOIN [HKP].[ProcessCriteria] AS B ON A.ProcessCriteriaId=B.Id WHERE A.MaterialGroupArticlePrdProcessGroupId='" + id + "' ORDER BY B.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetCriteriaList(GridParameter parameters, string groupId, string[] ids)
        {
            try
            {
                parameters.CmdText = @"SELECT B.Id AS ProcessCriteriaId, B.Code, B.ShortName, B.StandardName, B.UserName FROM [HKP].[ProcessCriteria] AS B
                JOIN [HKP].[CompanyGroupProcessCriteria] AS C ON C.ProcessCriteriaId=B.Id WHERE B.Id NOT IN(" + ReturnStringArray(ids) + ") AND C.CompanyGroupId='" + groupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteProcessCriteria(string key)
        {
            try
            {
                var entity = _processCriteriaRepository.Query(t => t.Id == key).Select().FirstOrDefault();
                if (entity != null)
                {
                    _processCriteriaRepository.Delete(entity);
                    _unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name,
                    false, ModuleEnum.Material.ToString()));
            }
        }

        #endregion Process Criteria

        #region Material Group Article Value

        public IEnumerable<object> GetAttributeList(string groupMasterId, string articleId)
        {
            try
            {
                var _sql = @"SELECT MGAV.Id
                            , MGAV.MaterialGroupArticleId
                            , MAM.MaterialAttributeId
                            , MA.UserName AS MaterialAttributeName
                            , MaterialAttributeValueId = CASE WHEN MGAV.Id<>'' THEN MGAV.MaterialAttributeValueId WHEN (MA.ValueAssignmentLevel='" + ValueAssignmentEnum.General + @"' AND MAV.IsDefault=1) THEN MAV.Id ELSE NULL END
                            , ValueFreeText =CASE WHEN MGAV.Id<>'' AND MGAV.MaterialAttributeValueId<>'' THEN MAV.UserName
							                      WHEN MGAV.Id<>'' AND MGAV.MaterialAttributeValueId IS NULL THEN MGAV.ValueFreeText
							                      WHEN (MA.ValueAssignmentLevel='" + ValueAssignmentEnum.General + @"' AND MAV.IsDefault=1) THEN MAV.UserName ELSE NULL END
                            , MA.IsFreeField, MA.IsPreDefinedField, MA.IsMandatory, MA.ValueAssignmentLevel
                    FROM [MST].[MaterialAttributeMaster] AS MAM
                    LEFT JOIN (SELECT * FROM [MST].[MaterialGroupArticleValue] WHERE MaterialGroupArticleId='" + articleId + @"')
		                    AS MGAV ON MGAV.MaterialAttributeId=MAM.MaterialAttributeId
                    LEFT JOIN [HKP].[MaterialAttribute] AS MA ON MAM.MaterialAttributeId=MA.Id
                    LEFT JOIN (SELECT * FROM [HKP].[MaterialAttributeValue] WHERE Active=1) AS MAV ON MAV.MaterialAttributeId=MA.Id AND MAV.MaterialAttributeId=MAM.MaterialAttributeId
                    AND MAV.MaterialAttributeId=MGAV.MaterialAttributeId AND MAV.Id=MGAV.MaterialAttributeValueId
                    WHERE MAM.MaterialGroupMasterId='" + groupMasterId + @"' ORDER BY MAM.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetAttributeValueList(GridParameter parameters, string groupId, string attributeId)
        {
            try
            {
                parameters.CmdText = @"SELECT Id AS MaterialAttributeValueId, [Sequence], Code, ShortName, StandardName, UserName
                                       FROM HKP.MaterialAttributeValue WHERE CompanyGroupId='" + groupId + "' AND MaterialAttributeId = '" + attributeId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        #endregion Material Group Article Value

        #region Report

        public IWorkbook GetMaterialGroupMaster()
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = obj.MaterialGroupMaster_Report(excelEngine);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Report
    }
}