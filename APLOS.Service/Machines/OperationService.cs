#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Machines;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Machines
{
    public class OperationService : Service<Operation>, IOperationService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IOperationFgComponentService _operationFgComponentService;
        private readonly IOperationProcessService _operationProcessService;
        private readonly IRepositoryAsync<OperationAttribute> _attributeRepository;
        private readonly IRepositoryAsync<OperationAttributeValue> _valueRepository;
        private readonly IRepositoryAsync<Operation> _operationRepository;

        public OperationService(
            IRepositoryAsync<Operation> operationRepository
            , IPKGeneratorService pkGeneratorService
            , IOperationFgComponentService operationFgComponentService
            , IOperationProcessService operationProcessService
            , IRepositoryAsync<OperationAttribute> attributeRepository
            , IRepositoryAsync<OperationAttributeValue> valueRepository
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(operationRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _operationFgComponentService = operationFgComponentService;
            _operationProcessService = operationProcessService;
            _attributeRepository = attributeRepository;
            _valueRepository = valueRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _operationRepository = operationRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(Operation), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(Operation entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && r.Active && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && r.Active && !r.Archive);
        }

        public void InsertGraph(Operation entity
            , IEnumerable<OperationProcess> operationProcessList
            , IEnumerable<OperationFgComponent> operationFgComponent
            , IEnumerable<OperationAttribute> attributeList
            , IEnumerable<OperationAttributeValue> valueList)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                base.InsertGraph(entity);
                _operationProcessService.InsertOrDeleteGraph(entity.Id, operationProcessList);
                _operationFgComponentService.InsertUpdateOrDeleteGraph(entity.Id, operationFgComponent);
                InsertUpdateOrDeleteAttribute(entity.Id, attributeList, valueList);
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

        public void UpdateGraph(Operation entity
            , IEnumerable<OperationProcess> operationProcessList
            , IEnumerable<OperationFgComponent> operationFgComponent
            , IEnumerable<OperationAttribute> attributeList
            , IEnumerable<OperationAttributeValue> valueList)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                _operationProcessService.InsertOrDeleteGraph(entity.Id, operationProcessList);
                _operationFgComponentService.InsertUpdateOrDeleteGraph(entity.Id, operationFgComponent);
                InsertUpdateOrDeleteAttribute(entity.Id, attributeList, valueList);
                base.UpdateGraph(entity);
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

        public bool CheckUsing(object id)
        {
            try
            {
                var sql = @"IF EXISTS(SELECT 1 FROM(
                          SELECT OperationId AS CheckingColumn FROM [MST].[OperationVariation]
                          ) A WHERE CheckingColumn = '"+ id + @"') SELECT 1 ELSE SELECT 0 RETURN";
                return Convert.ToBoolean(_operationRepository.SqlQuery<int>(sql).Single());
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (CheckUsing(id))
                    throw new CustomException("This Operation is using in Operation Variation!");

                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                _operationProcessService.DeleteGraph(id);
                _operationFgComponentService.DeleteGraph(id);
                DeleteAttribute(id);
                base.DeleteGraph(entity);
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

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetCbo(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT OP.Id AS [Value], OP.UserName as [Text]
                            , ProsessIds=(SELECT STUFF((SELECT DISTINCT ',' +  ProcessId FROM [MST].[OperationProcess] WHERE OperationId=OP.Id FOR XML PATH('')),1,1,''))
                            FROM [" + DbSchema.Masters + "].[" + DbTable.Operation + @"] AS OP
                            WHERE OP.CompanyGroupId='" + companyGroupId + "' AND  OP.Archive=0 AND OP.Active=1 ORDER BY OP.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetOperationCbo(string subprocessid)
        {
            try
            {
                var sql = @"  select distinct o.Id [Value],o.UserName [Text] from mst.OperationSubProcess s
                              left outer join mst.Operation o on o.Id=s.OperationId
                              where s.SubProcessId='" + subprocessid + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Grid List

        public GridModel Query(GridParameter parameters, string companyGroupId, string[] ids)
        {
            try
            {
                parameters.CmdText = @"SELECT O.Id
	                    , Process=STUFF((SELECT DISTINCT ',' + P.UserName FROM [MST].[OperationProcess] AS OPMT
					                    LEFT JOIN HKP.[Process] AS P ON OPMT.ProcessId=P.Id
					                    WHERE OPMT.OperationId=O.Id
					                    GROUP BY P.UserName
					                    FOR XML PATH ('')
					                    ),1,1,'')
                        , O.CompanyGroupId, O.OperationTypeId, ot.UserName AS OperationTypeCode, O.OperationCategoryId, oc.UserName AS OperationCategoryName
                        , O.OperationActivityId, OA.UserName AS OperationActivityName, O.[Sequence], O.Code, O.ShortName
                        , O.StandardName, O.UserName, O.Remarks, IsMachineRequired = CASE WHEN O.IsMachineRequired='M' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, O.Active, O.Archive
                        --, O.StandardName, O.UserName, O.Remarks, IsMachineRequired , O.Active, O.Archive

                        , O.BasicProcessTime, O.AssociateProcessTime, O.PersonalAllowance, O.MachineAllowance
	                    , ART.MaterialMasterId, O.ArticleId, ART.StandardName AS ArticleName, O.SkillId, SK.UserName AS SkillName
	                    , O.OperationLength, O.Frequency, O.ProductionSystemId, O.SPI,O.AdditionalAllowance
                    FROM MST.[Operation] as O
                    LEFT JOIN HKP.[OperationType] as ot ON O.OperationTypeId = ot.Id
                    LEFT JOIN HKP.[OperationCategory] as oc ON O.OperationCategoryId = oc.Id
                    LEFT JOIN HKP.[OperationActivity] AS OA ON O.OperationActivityId=OA.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON O.ArticleId=ART.Id
                    LEFT JOIN [HKP].[Skill] AS SK ON O.SkillId=SK.Id
                    WHERE O.CompanyGroupId = '" + companyGroupId + "' AND O.Archive = 0 AND O.Id NOT IN(" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// For Bulletin
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="fgComponentId"></param>
        /// <param name="groupId">todo: describe groupId parameter on GetSearchData</param>
        /// <param name="processid">todo: describe processid parameter on GetSearchData</param>
        /// <returns></returns>
        public GridModel GetSearchData(GridParameter parameters, string groupId, string processid, string fgComponentId)
        {
            try
            {
                parameters.CmdText = @"SELECT O.Id, O.Code, O.ShortName, O.StandardName, O.UserName, O.Remarks
		                            , O.IsMachineRequired, O.Active
		                            , O.OperationTypeId, OT.UserName AS OperationTypeCode
		                            , O.OperationCategoryId, OC.UserName AS OperationCategoryName
		                            , O.OperationActivityId, OA.UserName AS OperationActivityName
		                            , ART.MaterialMasterId, o.ArticleId AS MaterialMasterArticleId, ART.StandardName AS Machine
		                            , MM.UserName AS AssetItem, FAC.UserName AS FixedAssetCategory, FASC.UserName AS FixedAssetSubCategory, FAM.AssetType
                            FROM [MST].[Operation] AS O 
                            LEFT JOIN [HKP].[OperationType] AS OT ON O.OperationTypeId = OT.Id
                            LEFT JOIN [HKP].[OperationCategory] AS OC ON O.OperationCategoryId = OC.Id
                            LEFT JOIN [HKP].[OperationActivity] AS OA ON O.OperationActivityId=OA.Id
                            LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON O.ArticleId=ART.Id
                            LEFT JOIN [MST].[MaterialMaster] AS MM ON ART.MaterialMasterId=MM.Id
                            LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId = FAM.Id
                            LEFT JOIN [HKP].[FixedAssetCategory] AS FAC ON FAM.FixedAssetCategoryId = FAC.Id
                            LEFT JOIN [HKP].[FixedAssetSubCategory] AS FASC ON FAM.FixedAssetCategoryId = FASC.Id
                            LEFT JOIN MST.[OperationFgComponent] AS FG ON FG.OperationId=O.Id
                            WHERE O.CompanyGroupId='" + groupId + "' AND O.Archive=0 AND OM.ProcessId='" + processid + "' AND FG.FGComponentId='" + fgComponentId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetOperationListByProcess(GridParameter parameters, string processId)
        {
            try
            {
                parameters.CmdText = @"SELECT O.Id, O.Code, O.UserName, OT.UserName AS OperationTypeName
                                        , OC.UserName AS OperationCategoryName, OA.UserName AS OperationActivityName
                                        , O.ArticleId AS MaterialMasterArticleId, ART.StandardName AS ArticleName
                                        , O.IsMachineRequired, O.Active
                            FROM [MST].[Operation] AS O
                            LEFT JOIN [HKP].[OperationType] AS OT ON O.OperationTypeId = OT.Id
                            LEFT JOIN [HKP].[OperationCategory] AS OC ON O.OperationCategoryId = OC.Id
                            LEFT JOIN [HKP].[OperationActivity] AS OA ON O.OperationActivityId=OA.Id
                            LEFT JOIN [MST].[OperationProcess] AS OM ON OM.OperationId=O.Id
                            LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON O.ArticleId=ART.Id
                            WHERE OM.ProcessId='" + processId + "' AND O.Active=1 AND O.Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetArticleListByMaterialMaster(GridParameter parameters, string materialMasterId)
        {
            parameters.CmdText = @"SELECT ART.Id, ART.Code, ART.ShortName, ART.StandardName, MM.SkillId, SK.UserName AS SkillName, ART.MachineAllowance
                                FROM [MST].[MaterialMasterArticle] AS ART
                                LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=ART.MaterialMasterId
                                LEFT JOIN [HKP].Skill AS Sk ON MM.SkillId=Sk.Id
                                WHERE ART.MaterialMasterId='" + materialMasterId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public object GetOperationUtilityData(string operationId)
        {
            try
            {
                var sql = @"SELECT ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime, ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime
                            , ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance, ISNULL(OP.MachineAllowance, 0) AS MachineAllowance
                            , OP.OperationLength, OP.Frequency, OP.SPI
	                        , OP.ArticleId, ART.StandardName AS ArticleName
	                        , OP.SkillId, SK.UserName AS SkillName, OP.IsMachineRequired,MM.UserName MaterialName,OP.AdditionalAllowance
                        FROM [MST].[Operation] AS OP
                        LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON OP.ArticleId=ART.Id
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=ART.MaterialMasterId
                        LEFT JOIN [HKP].[Skill] AS SK ON OP.SkillId=Sk.Id
                        WHERE OP.Id='" + operationId + "'";
                return _sqlRepository.GetData(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Grid List

        #region Operation Attribute

        private void InsertUpdateOrDeleteAttribute(string masterId, IEnumerable<OperationAttribute> attributeList, IEnumerable<OperationAttributeValue> valueList)
        {
            try
            {
                var count = _attributeRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[OperationAttribute] WHERE OperationId='{masterId}'").First();
                var dbList = _attributeRepository.Query(t => t.OperationId == masterId).Select().ToList();
                if (attributeList.IsNotNull() && attributeList.Count() > 0)
                {
                    foreach (var item in attributeList)
                    {
                        UniqueAttributeCheck(item);
                        //item.IsFixedNoOfCharacter = item.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? true : false;
                        item.NoOfCharacter = item.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? item.NoOfCharacter : 0;
                        var valueFilterList = new List<OperationAttributeValue>();
                        if (item.ValueAssignmentLevel == ValueAssignmentEnum.General.ToString())
                        {
                            valueFilterList = valueList.Where(t => t.OperationAttributeId == item.Id).ToList();
                            if (valueFilterList.IsNull() && valueFilterList.Count() == 0)
                                throw new CustomException("Attribute [" + item.UserName + "] value can't be null.");
                        }

                        if (item.Id.StartsWith("n-"))
                        {
                            count++;
                            item.Id = MakePK(masterId, count, 2);
                            item.OperationId = masterId;
                            AuditService.AddedLog(item);
                            _attributeRepository.Insert(item);
                        }
                        else
                        {
                            AuditService.UpdatedLog(item);
                            _attributeRepository.Update(item);
                        }
                        InsertUpdateOrDeleteValue(masterId, item, valueFilterList);
                    }
                }
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    if (attributeList == null)
                    {
                        foreach (var item in dbList)
                        {
                            _attributeRepository.Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!attributeList.Any(t => t.Id == item.Id))
                                _attributeRepository.Delete(item);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteAttribute(string masterId)
        {
            try
            {
                var dbList = _attributeRepository.Query(t => t.OperationId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        DeleteAttributeValue(item.Id);
                        _attributeRepository.Delete(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UniqueAttributeCheck(OperationAttribute entity)
        {
            if (_attributeRepository.Any(t => t.Id != entity.Id && t.OperationId == entity.OperationId && t.Code == entity.Code))
                throw new CustomException("Code: " + entity.Code + " already exist in this operation.");
            if (_attributeRepository.Any(t => t.Id != entity.Id && t.OperationId == entity.OperationId && t.UserName == entity.UserName))
                throw new CustomException("User Define Name: " + entity.UserName + " already exist in this operation.");
        }

        public decimal GetAttributeSequence(string operationId)
        {
            try
            {
                return _attributeRepository.Query(t => t.OperationId == operationId).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetOperationAttributeList(string operationId)
        {
            try
            {
                var sql = @"SELECT * FROM [MST].[OperationAttribute] WHERE OperationId='" + operationId + "' ORDER BY [Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetOperationAttributeListForSubOperation(string operationId)
        {
            try
            {
                var sql = @"SELECT NULL Id, OA.OperationId, OA.Id AS OperationAttributeId, OA.UserName AS OperationAttributeName
                            , NULL OperationAttributeValueId, NULL OperationVariationId, NULL AttributeValueFreeText
                            , OA.AttributeProperty, OA.IsFixedNoOfCharacter, OA.NoOfCharacter, OA.IsFreeField
                            , OA.IsPreDefinedField, OA.IsMandatory, OA.ValueAssignmentLevel
                        FROM [MST].[OperationAttribute] AS OA 
                        WHERE OA.OperationId='" + operationId + "' ORDER BY OA.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Operation Attribute

        #region Operation Attribute Value

        private void InsertUpdateOrDeleteValue(string masterId, OperationAttribute attribute, IEnumerable<OperationAttributeValue> valueList)
        {
            try
            {
                var dbList = _valueRepository.Query(t => t.OperationId == masterId && t.OperationAttributeId == attribute.Id).Select().ToList();
                if (valueList.IsNotNull() && valueList.Count() > 0)
                {
                    var count = _valueRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[OperationAttributeValue] WHERE OperationAttributeId='{attribute.Id}'").First();

                    foreach (var item in valueList)
                    {
                        UniqueValueCheck(attribute.Id, item);
                        CheckPropertiesAndCharLength(item, attribute);
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(attribute.Id, count, 2);
                            item.OperationId = masterId;
                            item.OperationAttributeId = attribute.Id;
                            AuditService.AddedLog(item);
                            _valueRepository.Insert(item);
                        }
                        else
                        {
                            AuditService.UpdatedLog(item);
                            _valueRepository.Update(item);
                        }
                    }
                }
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    if (valueList == null)
                    {
                        foreach (var item in dbList)
                        {
                            _valueRepository.Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!valueList.Any(t => t.Id == item.Id))
                                _valueRepository.Delete(item);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteAttributeValue(string attributeId)
        {
            try
            {
                var dbList = _valueRepository.Query(t => t.OperationAttributeId == attributeId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        _valueRepository.Delete(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        private static void CheckPropertiesAndCharLength(OperationAttributeValue entity, OperationAttribute parentData)
        {
            if (parentData != null)
            {
                if (parentData.AttributeProperty == AttributePropertiesEnum.Integer.ToString())
                {
                    if (!int.TryParse(entity.UserName, out int userName))
                        throw new CustomException(entity.UserName + " is not integer");
                }
                else if (parentData.AttributeProperty == AttributePropertiesEnum.Decimal.ToString())
                {
                    if (!decimal.TryParse(entity.UserName, out decimal userName))
                        throw new CustomException(entity.UserName + " is not decimal");
                }
                else
                {
                    if (parentData.IsFixedNoOfCharacter &&
                        (parentData.NoOfCharacter < entity.UserName.Count() || parentData.NoOfCharacter > entity.UserName.Count()))
                        throw new Exception(entity.UserName + " must be [" + parentData.NoOfCharacter + "] character");
                }
            }
        }

        private void UniqueValueCheck(string attributeId, OperationAttributeValue entity)
        {
            if (_valueRepository.Any(t => t.Id != entity.Id && t.OperationId == entity.OperationId && t.OperationAttributeId == attributeId && t.Code == entity.Code))
                throw new CustomException("Code: " + entity.Code + " already exist.");
            if (_valueRepository.Any(t => t.Id != entity.Id && t.OperationId == entity.OperationId && t.OperationAttributeId == attributeId && t.UserName == entity.UserName))
                throw new CustomException("User Define Name: " + entity.UserName + " already exist.");
        }

        public decimal GetValueSequence(string operationAttributeId)
        {
            try
            {
                return _valueRepository.Query(t => t.OperationAttributeId == operationAttributeId).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetOperationAttributeValueList(string operationId)
        {
            try
            {
                var sql = @"SELECT * FROM [MST].[OperationAttributeValue] WHERE OperationId='" + operationId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetValueListByAttributeId(GridParameter parameters, string attributeId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [MST].[OperationAttributeValue] WHERE OperationAttributeId='" + attributeId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }



        #endregion Operation Attribute Value

     
    }
}