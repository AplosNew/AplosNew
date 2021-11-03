#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Materials;
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

namespace Library.Service.Materials
{
    public class CharacteristicsValueService : Service<CharacteristicsValue>, ICharacteristicsValueService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ICharacteristicsService _characteristicsService;
        private readonly ISqlRepository _sqlRepository;

        public CharacteristicsValueService(
            IRepositoryAsync<CharacteristicsValue> charaterValueRepository,
            IPKGeneratorService pkGeneratorService,
            ICharacteristicsService characteristicsService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(charaterValueRepository, unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _characteristicsService = characteristicsService;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT * FROM {DbSchema.HKP}.[{DbTable.CharacteristicsValue}] WHERE CompanyGroupId='{identity.CompanyGroupId}' AND Archive=0 AND SourceType='" + ValueAssignmentEnum.General + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string groupId, string characteristicsId, string[] ids)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM {DbSchema.HKP}.[{DbTable.CharacteristicsValue}] " +
                          $" WHERE CompanyGroupId='{groupId}' AND Archive=0 AND CharacteristicsId='{characteristicsId}' AND Id NOT IN (" + ReturnStringArray(ids) + ") AND SourceType='" + ValueAssignmentEnum.General + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        //public IEnumerable<object> GetListBySelectedId(string[] ids)
        //{
        //    try
        //    {
        //        var id = "";
        //        id = ids != null && ids.Length > 0 ? string.Join(",", ids.Select(item => "'" + item + "'")) : "' '";
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        var sql = @"SELECT NULL AS Id
        //                               ,NULL AS MaterialMasterId
        //                               ,NULL AS MaterialGridId
        //                               ,CharacteristicsId
        //                               ,Id AS CharacteristicsValueId
        //                               ,Code
        //                               ,[Description]
        //                            FROM HKP.CharacteristicsValue
        //                            WHERE CompanyGroupId='" + identity.CompanyGroupId + "' AND Archive=0 AND Id IN (" + id + ")";
        //        return _sqlRepository.GetDataCollection(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
        //                        null, ErrorType.ServiceError, null,
        //                        ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
        //    }
        //}

        //public GridModel CharacteristicsValueSearh(GridParameter parameters, string characteristicsId)
        //{
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        parameters.CmdText = "SELECT Code, " +
        //                         "[Description], " +
        //                         "Id " +
        //                  $" FROM {DbSchema.HKP}.[{DbTable.CharacteristicsValue}] " +
        //                  $" WHERE CompanyGroupId='{identity.CompanyGroupId}' AND Active=1 AND Archive=0 AND CharacteristicsId='{characteristicsId}'";
        //        return _sqlRepository.GetGridData(parameters);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public override void Insert(CharacteristicsValue entity)
        {
            try
            {
                CheckDefault(entity);
                CheckUnique(entity);
                var parentData = _characteristicsService.GetForCharacteristicsValue(entity.CharacteristicsId);
                CheckPropertiesAndCharLength(entity, parentData);
                entity.Id = GetPK();
                entity.MaterialMasterId = null;
                entity.SourceType = ValueAssignmentEnum.General.ToString();
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
        public void InsertBOMSKU(CharacteristicsValue entity)
        {
            try
            {
                CheckDefault(entity);
                CheckUnique(entity);
                var parentData = _characteristicsService.GetForCharacteristicsValue(entity.CharacteristicsId);
                CheckPropertiesAndCharLength(entity, parentData);
                entity.Id = GetPK();
               // entity.SourceType = ValueAssignmentEnum.Specific.ToString();
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
        private static void CheckPropertiesAndCharLength(CharacteristicsValue entity, Characteristics parentData)
        {
            if (parentData != null)
            {
                if (parentData.AttributeProperty == AttributePropertiesEnum.Integer.ToString())
                {
                    if (!int.TryParse(entity.UserName, out int userName))
                        throw new CustomException("User name is not integer");
                }
                else if (parentData.AttributeProperty == AttributePropertiesEnum.Decimal.ToString())
                {
                    if (!decimal.TryParse(entity.UserName, out decimal userName))
                        throw new CustomException("User name is not decimal");
                }
                else
                {
                    if (parentData.IsFixedNoOfCharacter &&
                        (parentData.NoOfCharacter < entity.UserName.Count() || parentData.NoOfCharacter > entity.UserName.Count()))
                        throw new Exception("User name must be [" + parentData.NoOfCharacter + "] character");
                }
            }
        }

        public override void Update(CharacteristicsValue entity)
        {
            try
            {
                CheckDefault(entity);
                CheckUnique(entity);
                var parentData = _characteristicsService.GetForCharacteristicsValue(entity.CharacteristicsId);
                CheckPropertiesAndCharLength(entity, parentData);
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        private void CheckDefault(CharacteristicsValue entity)
        {
            try
            {
                if (entity.IsDefault)
                {
                    if (entity.Active)
                    {
                        var defaultCheck = base.Query(t => t.Id != entity.Id && t.CompanyGroupId == entity.CompanyGroupId
                                    && t.CharacteristicsId == entity.CharacteristicsId && t.MaterialMasterId == entity.MaterialMasterId && t.IsDefault && t.Active && !t.Archive).Select().FirstOrDefault();

                        if (defaultCheck != null && defaultCheck.IsDefault)
                            throw (new Exception(string.Format(ServiceResources.CharacteristicsValue, defaultCheck.Code)));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private void CheckUnique(CharacteristicsValue entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.CompanyGroupId == entity.CompanyGroupId && r.Id != entity.Id && r.CharacteristicsId == entity.CharacteristicsId && r.MaterialMasterId == entity.MaterialMasterId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.CompanyGroupId == entity.CompanyGroupId && r.CharacteristicsId == entity.CharacteristicsId && r.MaterialMasterId == entity.MaterialMasterId && r.Id != entity.Id && !r.Archive);
        }

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(CharacteristicsValue), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string id)
        {
            try
            {
                var entity = base.Query(t => t.Id == id).Select().FirstOrDefault();
                Delete(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public decimal GetAutoSequence(string characteristicsId, string materialId)
        {
            try
            {
                if (string.IsNullOrEmpty(materialId))
                    return Query(t => t.CharacteristicsId == characteristicsId && !t.Archive).Select().Max(r => r.Sequence + 1);
                else
                    return Query(t => t.CharacteristicsId == characteristicsId && t.MaterialMasterId == materialId && !t.Archive).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception ex)
            {
                return 1.00m;
            }
        }

        public IEnumerable<object> GetCharacteristicsValueList()
        {
            try
            {
                return from m in base.Query(m => !m.Archive).Select()
                       select new { Text = m.Code, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
		public IEnumerable<object> GetCbo(string companyGroupId, string characteristicsId)
		{
			try
			{
				return from mf in base.Query(t => t.CompanyGroupId == companyGroupId && t.CharacteristicsId == characteristicsId && !t.Archive && t.Active).Select().OrderBy(r => r.UserName)
					   select new { Text = mf.UserName, Value = mf.Id };
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
		}
        public IEnumerable<object> GetCharacteristicsValueCboByCharacteristicsId(string materialMasterId, string characteristicsId, string valueAssignmentLevel)
        {
            try
            {
                var _sql = string.Empty;
                if (valueAssignmentLevel == ValueAssignmentEnum.Specific.ToString())
                
                     _sql = @"SELECT CV.Id AS [Value], CV.UserName AS [Text] FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                            Where CV.MaterialMasterId='" + materialMasterId + "' AND CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "'  Order by CV.UserName"; 
               
                else
                
                     _sql = @"SELECT CV.Id AS [Value], CV.UserName AS [Text] FROM [HKP].[Characteristics] C
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                            Where CV.CharacteristicsId='" + characteristicsId + "' AND  C.ValueAssignmentLevel='" + valueAssignmentLevel + "' AND  CV.SourceType='" + valueAssignmentLevel + "' Order by CV.UserName";
                    return _sqlRepository.GetDataCollection(_sql, null);
                
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
        public IEnumerable<object> GetCharacteristicsValueByCharacteristicsId(string CharacteristicsId)
        {
            try
            {
                var _sql = @"SELECT  TOP(100) cv.Id AS [Value], cv.Code, cv.Description AS [Text], cv.[Sequence],
                                cv.IsDefault, cv.Active,
                                cv.CharacteristicsId, c.StandardName AS CharacteristicsName
                                FROM HKP.CharacteristicsValue cv
                                LEFT OUTER JOIN HKP.Characteristics c ON cv.CharacteristicsId=c.Id
                                WHERE cv.CharacteristicsId='" + CharacteristicsId + @"'
                                AND cv.Archive=0
                                ORDER BY cv.Description";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public GridModel GetCharacteristicsValueSearchData(GridParameter parameters, string MaterialMasterId, string CharacteristicsId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    GridModel result = null;
        //    try
        //    {
        //        var sqlmm = @"SELECT TOP(100) cv.Id, cv.Code, cv.Description, cv.[Sequence],
        //                        cv.IsDefault, cv.Active,
        //                        cv.CharacteristicsId, c.StandardName AS CharacteristicsName
        //                        FROM HKP.CharacteristicsValue cv
        //                        LEFT OUTER JOIN HKP.Characteristics c ON cv.CharacteristicsId=c.Id
        //                        WHERE cv.CharacteristicsId='" + CharacteristicsId + @"'
        //                        AND cv.Archive=0 and cv.Id in
        // ( select CharacteristicsValueId from mst.MaterialMasterCharacteristicsValue
        //  where MaterialMasterId='" + MaterialMasterId + @"' and CharacteristicsId='" + CharacteristicsId + @"'
        //)
        //                        ORDER BY cv.Description";
        //        result = _sqlRepository.GetGridData(parameters, sqlmm);
        //        if (result.total == 0)
        //        {
        //            var sql = @"SELECT TOP(100) cv.Id, cv.Code, cv.Description, cv.[Sequence],
        //                        cv.IsDefault, cv.Active,
        //                        cv.CharacteristicsId, c.StandardName AS CharacteristicsName
        //                        FROM HKP.CharacteristicsValue cv
        //                        LEFT OUTER JOIN HKP.Characteristics c ON cv.CharacteristicsId=c.Id
        //                        WHERE cv.CharacteristicsId='" + CharacteristicsId + @"'
        //                        AND cv.Archive=0
        //                        ORDER BY cv.Description";
        //            result = _sqlRepository.GetGridData(parameters, sql);
        //        }
        //        return result;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public GridModel GetCharacteristicsValueSearchData(GridParameter parameters, string groupId, string assignment, string materialMasterId, string charId)
        {
            try
            {
                if (assignment == ValueAssignmentEnum.General.ToString())
                    parameters.CmdText = @"SELECT Id AS CharacteristicsValueId, CompanyGroupId, CharacteristicsId, MaterialMasterId, SourceType, Code, [Sequence], ShortName, StandardName, UserName, IsDefault, Remarks, [Description], Active
                    FROM HKP.CharacteristicsValue WHERE CompanyGroupId='" + groupId + "' AND CharacteristicsId='" + charId + "' AND SourceType='" + assignment + "'";
                else
                    parameters.CmdText = @"SELECT Id AS CharacteristicsValueId, CompanyGroupId, CharacteristicsId, MaterialMasterId, SourceType, Code, [Sequence], ShortName, StandardName, UserName, IsDefault, Remarks, [Description], Active
                    FROM HKP.CharacteristicsValue WHERE CompanyGroupId='" + groupId + "' AND CharacteristicsId='" + charId + "' AND MaterialMasterId='" + materialMasterId + "' AND SourceType='" + assignment + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }




		public IEnumerable<object> GetCharacteristicsValueSearchData1( string groupId, string assignment, string materialMasterId, string charId)
		
		{
			try
			{
				var sql = "";
				if (assignment == ValueAssignmentEnum.General.ToString())
					sql = @"SELECT Id AS CharacteristicsValueId, CompanyGroupId, CharacteristicsId, MaterialMasterId, SourceType, Code, [Sequence], ShortName, StandardName, UserName, IsDefault, Remarks, [Description], Active
                    FROM HKP.CharacteristicsValue WHERE CompanyGroupId='" + groupId + "' AND CharacteristicsId='" + charId + "' AND SourceType='" + assignment + "'";
				else
					sql = @"SELECT Id AS CharacteristicsValueId, CompanyGroupId, CharacteristicsId, MaterialMasterId, SourceType, Code, [Sequence], ShortName, StandardName, UserName, IsDefault, Remarks, [Description], Active
                    FROM HKP.CharacteristicsValue WHERE CompanyGroupId='" + groupId + "' AND CharacteristicsId='" + charId + "' AND MaterialMasterId='" + materialMasterId + "' AND SourceType='" + assignment + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
		}

		#region Material Ch Value

		public void InsertGraphFromMaterial(IEnumerable<CharacteristicsValue> entities, string groupId, string materialMasterId)
        {
            foreach (var item in entities)
            {
                CheckDefault(item);
                CheckUnique(item);
                var parentData = _characteristicsService.GetForCharacteristicsValue(item.CharacteristicsId);
                CheckPropertiesAndCharLength(item, parentData);
                item.Id = GetPK();
                item.CompanyGroupId = groupId;
                item.MaterialMasterId = materialMasterId;
                item.SourceType = ValueAssignmentEnum.Specific.ToString();
                InsertGraph(item);
            }
        }

        public void InsertUpdateOrDeleteFromMaterial(IEnumerable<CharacteristicsValue> entities, string groupId, string materialMasterId)
        {
            try
            {
                var dbList = Query(t => t.MaterialMasterId == materialMasterId).Select().ToList();
                if (entities != null && entities.Count() > 0)
                {
                    foreach (var item in entities)
                    {
                        CheckDefault(item);
                        CheckUnique(item);
                        var parentData = _characteristicsService.GetForCharacteristicsValue(item.CharacteristicsId);
                        CheckPropertiesAndCharLength(item, parentData);
						if (item.Id.StartsWith("n-"))
						{
                            item.Id = GetPK();
                            item.CompanyGroupId = groupId;
                            item.MaterialMasterId = materialMasterId;
                            item.SourceType = ValueAssignmentEnum.Specific.ToString();
                            InsertGraph(item);
                        }
                        else
                            UpdateGraph(item);
                    }
                }
                //if (dbList != null && dbList.Count() > 0)
                //{
                //    foreach (var db in dbList)
                //    {
                //        if (!entities.Any(t => t.Id == db.Id))
                //            DeleteGraph(db);
                //    }
                //}
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public void DeleteGraphFromMaterial(string materialMasterId)
        {
            try
            {
                var dbList = Query(t => t.MaterialMasterId == materialMasterId).Select().ToList();
                if (dbList != null && dbList.Count() > 0)
                {
                    foreach (var db in dbList)
                    {
                        DeleteGraph(db);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IEnumerable<object> GetCharacteristicsValueListByMaterialMaster(string materialMasterId)
        {
            try
            {
                var sql = @"SELECT Id, CompanyGroupId, CharacteristicsId, MaterialMasterId, SourceType, Code, [Sequence], ShortName, StandardName, UserName, IsDefault, Remarks, [Description], Active
                    FROM HKP.CharacteristicsValue WHERE MaterialMasterId='" + materialMasterId + "' AND SourceType='" + ValueAssignmentEnum.Specific + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        #endregion Material Ch Value
    }
}