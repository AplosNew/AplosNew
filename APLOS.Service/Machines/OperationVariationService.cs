#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Machines;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Machines
{
    public partial class OperationVariationService : Service<OperationVariation>, IOperationVariationService
    {
        #region Constructor

        private readonly IRepositoryAsync<OperationVariationAttributeValue> _valueRepository;
        private readonly IRepositoryAsync<OperationAttribute> _attributeRepository;
        private readonly IRepositoryAsync<OperationVariationSizeGroup> _operationVariationSizeGroupRepository;
        private readonly IRepositoryAsync<OperationVariationProductMaster> _operationVariationProductMasterRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public OperationVariationService(
            IRepositoryAsync<OperationVariation> operationRepository
            , IRepositoryAsync<OperationVariationAttributeValue> valueRepository
            , IRepositoryAsync<OperationAttribute> attributeRepository
            , IRepositoryAsync<OperationVariationSizeGroup> operationVariationSizeGroupRepository
            , IRepositoryAsync<OperationVariationProductMaster> operationVariationProductMasterRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(operationRepository, unitOfWork, pkGeneratorService)
        {
            _attributeRepository = attributeRepository;
            _valueRepository = valueRepository;
            _operationVariationSizeGroupRepository = operationVariationSizeGroupRepository;
            _operationVariationProductMasterRepository = operationVariationProductMasterRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetCbo(string operationId)
        {
            try
            {
                return from a in Query(t => t.OperationId == operationId).Select().OrderBy(t => t.UserName)
                       select new { Text = a.UserName, Value = a.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public decimal GetAutoSequence(string companyGroupId, string operationId)
        {
            try
            {
                return base.Query(t => t.CompanyGroupId == companyGroupId && t.OperationId == operationId).Select().Max(t => t.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public GridModel Query(GridParameter parameters, string groupId, string operationId)
        {
            try
            {
                parameters.CmdText = @"SELECT OS.*, ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime, ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime
                                            , ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance
                                            , OP.OperationLength, OP.IsMachineRequired
	                                        , ART.StandardName AS ArticleName, SK.UserName AS SkillName,MM.UserName MaterialName,OM.Code OperationMasterCode
                                           ,OP.Code OperationCode,OP.UserName OperationName
                            FROM [MST].[OperationVariation] AS OS
                            JOIN [MST].[Operation] AS OP ON OP.Id = OS.OperationId
                           -- LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON OP.ArticleId = ART.Id
                            LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON OS.ArticleId = ART.Id
                            LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=ART.MaterialMasterId
                            LEFT JOIN [MST].[OperationMaster]  OM ON OM.Id=OS.OperationMasterId
                            JOIN [HKP].[Skill] AS SK ON OP.SkillId = SK.Id
                            WHERE OS.CompanyGroupId='" + groupId + "'AND OS.OperationId='" + operationId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        /// <summary>
        /// For Operation variation
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyGroupId"></param>
        /// <param name="operationId"></param>
        /// <returns></returns>
        public GridModel GetMachineListByOperation(GridParameter parameters, string companyGroupId, string operationId)
        {
            parameters.CmdText = @";WITH CTE AS
                            (
                                SELECT MGP.UserName AS MaterialGroupMaster, MM.Id AS MaterialMasterId
			                            , MM.Code, MM.ShortName, MM.StandardName,MM.UserName AS MaterialMasterName
                                        , FAM.UserName AS AssetMaster, B.UserName AS BudgetName
                                , COUNT(*) OVER (PARTITION BY MP.MaterialMasterId) AS RN
		                        FROM [MST].[MaterialMaster] AS MM
		                        LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
		                        LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId = FAM.Id
		                        LEFT JOIN MST.BudgetMaster AS BM ON MM.BudgetMasterId=BM.Id
		                        LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
		                        LEFT JOIN MST.MaterialMasterMachineProcess AS MP ON MP.MaterialMasterId=MM.Id
		                        WHERE MM.CompanyGroupId = '" + companyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1
		                        AND MM.Id IN(SELECT MaterialMasterId FROM MST.MaterialMasterBusinessProcess AS A 
                                            JOIN SCS.BusinessProcess AS B ON A.BusinessProcessId=B.Id
					                        WHERE B.BusinessProcessName='" + BusinessProcessEnum.MachineDefinition + @"')
		                        AND MP.ProcessId IN(SELECT ProcessId FROM [MST].[OperationProcess] WHERE OperationId='" + operationId + @"')
                            ) SELECT DISTINCT *, COUNT(*) OVER () AS TotalRows FROM CTE WHERE RN>1";
            return _sqlRepository.GetDifferentGridData(parameters);
        }

        public void InsertGraph(OperationVariation entity, IEnumerable<OperationVariationAttributeValue> valueList, IEnumerable<OperationVariationSizeGroup> operationVariationSizeGroupDataList, IEnumerable<OperationVariationProductMaster> operationVariationPMDataList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                CheckCombination(entity.Id, entity.OperationId, entity.ArticleId, entity.SkillId, valueList);

                Check(entity);
                entity.Id = GetAutoNumber(nameof(OperationVariation), PKGeneratorEnum.Auto, entity.CompanyGroupId, DateTime.Now);
                base.InsertGraph(entity);

                InsertUpdateOrDeleteValue(entity, valueList, operationVariationSizeGroupDataList, operationVariationPMDataList);
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
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        private void CheckCombination(string variationId, string operationId, string articleId, string skillId, IEnumerable<OperationVariationAttributeValue> valueList)
        {
            var isChanged = false;
            if (!Any(t => t.Id != variationId && t.OperationId == operationId && t.ArticleId == articleId && t.SkillId == skillId))
                isChanged = true;
            if (valueList!=null)
            {
                if (!isChanged)
                {
                    foreach (var item in valueList)
                    {
                        if (item.OperationAttributeValueId.IsNotNull() || item.AttributeValueFreeText.IsNotNull())
                        {
                            if (!_valueRepository.Any(t => t.Id != item.Id && t.OperationId == operationId && t.OperationAttributeId == item.OperationAttributeId
                                                    && t.OperationAttributeValueId == item.OperationAttributeValueId))
                                isChanged = true;
                        }
                        
                            isChanged = true;
                        
                    }
                }

                if (!isChanged) throw new CustomException("This combination already exist."); 
            }
        }

        public void UpdateGraph(OperationVariation entity, IEnumerable<OperationVariationAttributeValue> valueList, IEnumerable<OperationVariationSizeGroup> operationVariationSizeGroupDataList, IEnumerable<OperationVariationProductMaster> operationVariationPMDataList)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                base.UpdateGraph(entity);
                InsertUpdateOrDeleteValue(entity, valueList, operationVariationSizeGroupDataList, operationVariationPMDataList);
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
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                DeleteValue(id);

                var dbList = _operationVariationSizeGroupRepository.Query(t => t.OperationVariationId == id).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        _operationVariationSizeGroupRepository.Delete(item.Id);
                    }
                }

                base.Delete(entity.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void DeleteOperationVariationSizeGroup(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = _operationVariationSizeGroupRepository.Find(id);
                _operationVariationSizeGroupRepository.Delete(data);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        private void Check(OperationVariation entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Id != entity.Id && t.Code == entity.Code && !t.Archive && t.CompanyGroupId == entity.CompanyGroupId);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.Id != entity.Id && t.UserName == entity.UserName && !t.Archive && t.CompanyGroupId == entity.CompanyGroupId);
        }

        public object GetOperationUtilityData(string operationId, string articleId, string skillId)
        {
            try
            {
                var wc = "";
                if (articleId != "null")
                {
                    wc = @" AND OV.ArticleId = '" + articleId + "'";
                }
                if (!string.IsNullOrEmpty(wc) && skillId != "null")
                {
                    wc += @" AND OV.SkillId = '" + skillId + "'";
                }
                else if (skillId != "null")
                {
                    wc = @" AND OV.SkillId = '" + skillId + "'";
                }

                var sql = @"SELECT OV.ArticleId,ART.StandardName AS ArticleName, OV.SkillId, SK.UserName AS SkillName,MM.UserName MaterialName
				,OPP.BasicProcessTime,OPP.AssociateProcessTime,OPP.PersonalAllowance,OPP.MachineAllowance,OPP.Frequency,OPP.SPI,OV.TotalSAM, OV.AdditionalSAMSymbol,OV.SubOperationSAM,OV.AdditionalSAM
				 from [MST].[OperationVariation] OV
				LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON OV.ArticleId=ART.Id
                LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=ART.MaterialMasterId
                LEFT JOIN [HKP].[Skill] AS SK ON OV.SkillId=Sk.Id
				LEFT JOIN (SELECT OP.Id,ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime, ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime
                ,ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance, ISNULL(OP.MachineAllowance, 0) AS MachineAllowance
                ,OP.Frequency, OP.SPI FROM [MST].[Operation] OP Where OP.Id='" + operationId + @"') OPP ON OPP.Id =OV.OperationId
				WHERE OV.OperationId='" + operationId + @"' " + wc + "";

                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CheckCode(OperationVariation entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Id != entity.Id && t.Code == entity.Code && !t.Archive && t.CompanyGroupId == entity.CompanyGroupId);
        }

        public void UpdateOperationVaiationCode(OperationVariation entity)
        {
            
            try
            {
                CheckCode(entity);

                var dbData = Find(entity.Id);

                dbData.Code = entity.Code;

                AuditService.UpdatedLog(dbData);
                Update(dbData);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region Attribute Value

        private void InsertUpdateOrDeleteValue(OperationVariation operationVariation, IEnumerable<OperationVariationAttributeValue> valueList, IEnumerable<OperationVariationSizeGroup> operationVariationSizeGroupDataList, IEnumerable<OperationVariationProductMaster> operationVariationPMDataList)
        {
            try
            {
                if (valueList != null)
                {
                    //CheckCombination(operationVariation.Id, operationVariation.OperationId, operationVariation.ArticleId, operationVariation.SkillId, valueList);
                    foreach (var item in valueList)
                    {
                        if (string.IsNullOrEmpty(item.Id))//Insert
                        {
                            if (string.IsNullOrEmpty(item.OperationAttributeValueId) && string.IsNullOrEmpty(item.AttributeValueFreeText))
                                return;
                            else
                            {
                                SetMaterialAttributeValueId(item);
                                item.Id = GetAutoNumber(nameof(OperationVariationAttributeValue), PKGeneratorEnum.Auto, null, DateTime.Now);
                                item.OperationVariationId = operationVariation.Id;
                                item.OperationId = operationVariation.OperationId;
                                AuditService.AddedLog(item);
                                _valueRepository.Insert(item);
                            }
                        }
                        else
                        {
                            //Edit
                            if (string.IsNullOrEmpty(item.OperationAttributeValueId) && string.IsNullOrEmpty(item.AttributeValueFreeText))
                                _valueRepository.Delete(item);
                            else
                            {
                                SetMaterialAttributeValueId(item);
                                AuditService.UpdatedLog(item);
                                _valueRepository.Update(item);
                            }
                        }
                    }
                }

                if (operationVariationSizeGroupDataList != null)
                {
                    foreach (var item in operationVariationSizeGroupDataList)
                    {
                        if (string.IsNullOrEmpty(item.Id))//Insert
                        {

                            item.Id = GetAutoNumber(nameof(OperationVariationSizeGroup), PKGeneratorEnum.Auto, null, DateTime.Now);
                            item.OperationVariationId = operationVariation.Id;
                            AuditService.AddedLog(item);
                            _operationVariationSizeGroupRepository.Insert(item);

                        }
                        else
                        {
                            //Edit

                            AuditService.UpdatedLog(item);
                            _operationVariationSizeGroupRepository.Update(item);

                        }
                    }
                }

                if (operationVariationPMDataList != null)
                {
                    foreach (var item in operationVariationPMDataList)
                    {
                        if (string.IsNullOrEmpty(item.Id))//Insert
                        {

                            item.Id = GetAutoNumber(nameof(OperationVariationProductMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
                            item.OperationVariationId = operationVariation.Id;
                            AuditService.AddedLog(item);
                            _operationVariationProductMasterRepository.Insert(item);

                        }
                        else
                        {
                            //Edit

                            AuditService.UpdatedLog(item);
                            _operationVariationProductMasterRepository.Update(item);

                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteValue(string masterId)
        {
            try
            {
                var dbList = _valueRepository.Query(t => t.OperationVariationId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        _valueRepository.Delete(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void DeleteOperationVariationSizeGroupData(string masterId)
        {
            try
            {
                var dbList = _operationVariationSizeGroupRepository.Query(t => t.OperationVariationId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        _operationVariationSizeGroupRepository.Delete(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetMaterialAttributeValueId(OperationVariationAttributeValue item)
        {
            if (item.OperationAttributeValueId != null)//
                item.AttributeValueFreeText = null;
            else
            {
                if (item.AttributeValueFreeText != null)
                    CheckPropertiesAndCharLength(item);
            }
        }

        private void CheckPropertiesAndCharLength(OperationVariationAttributeValue entity)
        {
            var attribute = _attributeRepository.Query(t => t.Id == entity.OperationAttributeId).Select().FirstOrDefault();
            if (attribute != null)
            {
                if (attribute.AttributeProperty == AttributePropertiesEnum.Integer.ToString())
                {
                    if (!int.TryParse(entity.AttributeValueFreeText, out int userName))
                        throw new CustomException("Attribute Value is not integer");
                }
                else if (attribute.AttributeProperty == AttributePropertiesEnum.Decimal.ToString())
                {
                    if (!decimal.TryParse(entity.AttributeValueFreeText, out decimal userName))
                        throw new CustomException("Attribute Value is not decimal");
                }
                else
                {
                    if (attribute.IsFixedNoOfCharacter &&
                       (attribute.NoOfCharacter < entity.AttributeValueFreeText.Count() || attribute.NoOfCharacter > entity.AttributeValueFreeText.Count()))
                        throw new CustomException("Attribute Value must be [" + attribute.NoOfCharacter + "] character");
                }
            }
        }

        public IEnumerable<object> GetVairiationValue(string operationId, string masterId)
        {
            try
            {
                var sql = @"SELECT OVAV.Id, OA.OperationId, OA.Id AS OperationAttributeId, OA.UserName AS OperationAttributeName, OVAV.OperationAttributeValueId
	                        , OVAV.OperationVariationId, AttributeValueFreeText=CASE WHEN OVAV.OperationAttributeValueId<>'' THEN OAV.UserName ELSE OVAV.AttributeValueFreeText END
	                        , OA.AttributeProperty, OA.IsFixedNoOfCharacter, OA.NoOfCharacter, OA.IsFreeField
	                        , OA.IsPreDefinedField, OA.IsMandatory, OA.ValueAssignmentLevel
                        FROM [MST].[OperationAttribute] AS OA
                        LEFT JOIN (SELECT * FROM [MST].[OperationVariationAttributeValue] WHERE OperationVariationId='" + masterId + @"') AS OVAV ON OVAV.OperationAttributeId=OA.Id
                        LEFT JOIN [MST].[OperationAttributeValue] AS OAV  ON OVAV.OperationAttributeValueId=OAV.Id
                        WHERE OA.OperationId='" + operationId + "' ORDER BY OA.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        #endregion Attribute Value
    }
}