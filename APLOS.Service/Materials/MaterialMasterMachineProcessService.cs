#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Materials
{
    public class MaterialMasterMachineProcessService : Service<MaterialMasterMachineProcess>, IMaterialMasterMachineProcessService
    {
        #region Constructor

        private readonly IRepositoryAsync<MaterialMaster> _materialMasterRepository;
        private readonly IMaterialMasterArticleService _articleService;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public MaterialMasterMachineProcessService(
             IRepositoryAsync<MaterialMasterMachineProcess> baseRepository
            , IRepositoryAsync<MaterialMaster> materialMasterRepository
            , IMaterialMasterArticleService articleService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) :
            base(baseRepository, unitOfWork, pkGeneratorService)
        {
            _materialMasterRepository = materialMasterRepository;
            _articleService = articleService;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetDetailList(string masterId)
        {
            try
            {
                var _sql = @"SELECT A.Id, A.MaterialMasterId, A.ProcessId, B.[Sequence], B.Code,B.ShortName,B.StandardName, B.UserName
                                    FROM MST.MaterialMasterMachineProcess AS A LEFT JOIN HKP.Process AS B ON A.ProcessId=B.Id
                                    WHERE A.MaterialMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private static string CreateWhereCluse(string fieldValue, string sqlField, string conditionVariable)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                var str = " AND ";
                //if (!string.IsNullOrEmpty(conditionVariable)) str = " AND ";
                return str + sqlField + @" LIKE ('%" + fieldValue + "%')";
            }
            return string.Empty;
        }

        public GridModel GetMaterialMasterList(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT MT.UserName AS MaterialType
                                      , MGP.UserName AS MaterialGroupMaster
                                      , PM.UserName AS ProductMaster
                                      , UOMB.UserName AS BaseUom
                                      , MC.UserName MaterialCategory
	                                  , MSC.UserName MaterialSubCategory
                                      , MM.Id AS MaterialMasterId
                                      , MM.Sequence,MM.Code,MM.ShortName,MM.StandardName,MM.UserName, SKU=CASE WHEN MM.WithSKU=1 THEN 'Yes' ELSE 'No' END
                                      , Active=CASE WHEN MM.Active=1 THEN 'Yes' ELSE 'No' END
                                      , FAM.UserName AS AssetMaster, FAM.AssetType
                                      , B.Code AS AssetBudgetCode, MM.SkillId--, MM.MachineAllowance
                                      , Revenue=CASE WHEN (MM.IsInventory=1 OR MM.IsExpenseOut=1) THEN 'Yes' ELSE 'No' END
                FROM [MST].[MaterialMaster] AS MM
                LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                LEFT OUTER JOIN [HKP].[MaterialCategory] AS MC ON MM.MaterialCategoryId = MC.Id
                LEFT OUTER JOIN [HKP].[MaterialSubCategory] AS MSC ON MM.MaterialSubCategoryId = MSC.Id
                LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
				LEFT JOIN MST.BudgetMaster AS BM ON MM.BudgetMasterId=BM.Id
                LEFT JOIN HKP.FixedAssetMasterBudgetTag AS FAMT ON FAMT.BudgetMasterId=BM.Id
                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAMT.FixedAssetMasterId=FAM.Id
                LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                WHERE MM.CompanyGroupId = '" + companyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1
                AND MM.Id IN(SELECT MaterialMasterId FROM MST.MaterialMasterBusinessProcess AS A INNER JOIN SCS.BusinessProcess AS B ON A.BusinessProcessId=B.Id
							WHERE B.BusinessProcessName='" + BusinessProcessEnum.MachineDefinition + "')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                  Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber("Machine", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertGraph(string materialMasterId, IEnumerable<MaterialMasterMachineProcess> entities)
        {
            try
            {
                //if (entities == null && entities.Count() == 0)
                //    throw new CustomException("Can not save without process");
                foreach (var item in entities)
                {
                    item.Id = GetPK();
                    item.MaterialMasterId = materialMasterId;
                    base.InsertGraph(item);
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void InsertUpdateOrDeleteGraph(string materialMasterId, string skillId, IEnumerable<MaterialMasterMachineProcess> entities, IEnumerable<MaterialMasterArticle> articleList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                //TODO: To be continue
                var materialData = _materialMasterRepository.Find(materialMasterId);
                materialData.SkillId = skillId;
                AuditService.UpdatedLog(materialData);
                _materialMasterRepository.Update(materialData);
                if (entities.IsNotNull() && entities.Count() > 0)
                {
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = GetPK();
                           // item.Id= GetAutoNumber(nameof(MaterialMasterMachineProcess), PKGeneratorEnum.Auto, null, DateTime.Now);
                            item.MaterialMasterId = materialMasterId;
                            base.InsertGraph(item);
                        }
                        else
                            UpdateGraph(item);
                    }
                }
                if (articleList.IsNotNull() && articleList.Count() > 0)
                    _articleService.UpdateGraph(articleList);


                var dbDetailList = Query(t => t.MaterialMasterId == materialMasterId).Select().ToList();
                if (dbDetailList != null && dbDetailList.Count() > 0)
                {
                    foreach (var item in dbDetailList)
                    {
                        if (!entities.Any(t => t.Id == item.Id))
                            base.DeleteGraph(item);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertUpdateDeleteGraph(string materialMasterId, IEnumerable<MaterialMasterMachineProcess> entities)
        {
            try
            {
                //if (entities == null && entities.Count() == 0)
                //    throw new CustomException("Can not save without process");
                if (entities != null && entities.Count() > 0)
                {
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = GetPK();
                            item.MaterialMasterId = materialMasterId;
                            base.InsertGraph(item);
                        }
                        else
                            UpdateGraph(item);
                    }
                }
                var dbDetailList = Query(t => t.MaterialMasterId == materialMasterId).Select().ToList();
                if (dbDetailList != null)
                {
                    foreach (var item in dbDetailList)
                    {
                        if (!entities.Any(t => t.Id == item.Id))
                            base.DeleteGraph(item);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void DeleteGraph(string materialMasterId)
        {
            var flag = false;
            try
            {
                var entities = Query(t => t.MaterialMasterId == materialMasterId).Select();
                if (entities != null && entities.Count() > 0)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    foreach (var item in entities)
                    {
                        base.DeleteGraph(item);
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (CustomException)
            {
                throw;
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
    }
}