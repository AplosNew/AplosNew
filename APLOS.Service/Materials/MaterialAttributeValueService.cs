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
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

#endregion Using

namespace Library.Service.Materials
{
    public class MaterialAttributeValueService : Service<MaterialAttributeValue>, IMaterialAttributeValueService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MaterialAttributeValue> _charaterValueRepository;
        private readonly IRepositoryAsync<MaterialAttribute> _materialAttributeRepository;

        public MaterialAttributeValueService(
            IRepositoryAsync<MaterialAttributeValue> charaterValueRepository,
            IPKGeneratorService pkGeneratorService,
            IRepositoryAsync<MaterialAttribute> materialAttributeRepository,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(charaterValueRepository, unitOfWork)
        {
            _charaterValueRepository = charaterValueRepository;
            _materialAttributeRepository = materialAttributeRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetCbo(string companyGroupId, string attributeId)
        {
            try
            {
                return from mf in base.Query(t => t.CompanyGroupId == companyGroupId && t.MaterialAttributeId == attributeId && !t.Archive && t.Active).Select().OrderBy(r => r.UserName)
                       select new { Text = mf.UserName, Value = mf.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,

                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string materialAttributeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT MAV.*, MA.UserName AS MaterialAttributeName " +
                                     $"FROM {DbSchema.HKP}.[{DbTable.MaterialAttributeValue}] AS MAV " +
                                     $"LEFT JOIN {DbSchema.HKP}.[{DbTable.MaterialAttribute}] AS MA ON MAV.MaterialAttributeId=MA.Id " +
                                     $"WHERE MAV.MaterialAttributeId='{materialAttributeId}' AND MAV.CompanyGroupId='{identity.CompanyGroupId}' AND MAV.Archive=0 AND SourceType='" + ValueAssignmentEnum.General + "'";
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

        public GridModel GetAttributeValueList(GridParameter parameters, string assignment, string materialMasterId, string attributeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (ValueAssignmentEnum.General.ToString() == assignment)
                {
                    parameters.CmdText = @"SELECT Id AS MaterialAttributeValueId, NULL AS MaterialMasterAttributeValueId, [Sequence], Code, ShortName, StandardName, UserName
                                           FROM HKP.MaterialAttributeValue WHERE CompanyGroupId='" + identity.CompanyGroupId + "' AND MaterialAttributeId = '" + attributeId + "'";

                    ///old code
                    ///
                    //parameters.CmdText = @"SELECT Id AS MaterialAttributeValueId, NULL AS MaterialMasterAttributeValueId, [Sequence], Code, ShortName, StandardName, UserName
                    //                       FROM HKP.MaterialAttributeValue WHERE CompanyGroupId='" + identity.CompanyGroupId + @"' AND MaterialAttributeId = '" + attributeId + "'";
                }
                else
                {
                    parameters.CmdText = @"SELECT Id AS MaterialAttributeValueId, NULL AS MaterialMasterAttributeValueId, [Sequence], Code, ShortName, StandardName, UserName
						FROM HKP.MaterialAttributeValue WHERE CompanyGroupId='" + identity.CompanyGroupId + "' AND MaterialAttributeId = '" + attributeId + "'";

                    ///old code
                    //              parameters.CmdText = @"SELECT Id AS MaterialAttributeValueId, NULL AS MaterialMasterAttributeValueId, [Sequence], Code, ShortName, StandardName, UserName
                    //FROM HKP.MaterialAttributeValue WHERE CompanyGroupId='" + identity.CompanyGroupId + "' AND MaterialAttributeId = '" + attributeId + "' AND MaterialMasterId='" + materialMasterId + "'";
                }
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override void Insert(MaterialAttributeValue entity)
        {
            try
            {
                CheckDefault(entity);
                CheckUnique(entity);
                var parentData = _materialAttributeRepository.Query(t => t.Id == entity.MaterialAttributeId).Select().FirstOrDefault();
                CheckPropertiesAndCharLength(entity, parentData);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override void Update(MaterialAttributeValue entity)
        {
            try
            {
                CheckDefault(entity);
                CheckUnique(entity);
                var parentData = _materialAttributeRepository.Query(t => t.Id == entity.MaterialAttributeId).Select().FirstOrDefault();
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                entity.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private static void CheckPropertiesAndCharLength(MaterialAttributeValue entity, MaterialAttribute parentData)
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

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(MaterialAttributeValue), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(MaterialAttributeValue entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.CompanyGroupId == entity.CompanyGroupId && r.Id != entity.Id && r.MaterialAttributeId == entity.MaterialAttributeId && r.MaterialMasterId == entity.MaterialMasterId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.CompanyGroupId == entity.CompanyGroupId && r.MaterialAttributeId == entity.MaterialAttributeId && r.MaterialMasterId == entity.MaterialMasterId && r.Id != entity.Id && !r.Archive);
        }

        private void CheckDefault(MaterialAttributeValue entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entity.IsDefault)
                {
                    if (entity.Active)
                    {
                        var defaultCheck = Query(t => t.Id != entity.Id && t.CompanyGroupId == identity.CompanyGroupId &&
                                          t.MaterialAttributeId == entity.MaterialAttributeId && t.MaterialMasterId == entity.MaterialMasterId && t.IsDefault && t.Active && !t.Archive).Select().FirstOrDefault();

                        if (defaultCheck != null && defaultCheck.IsDefault)
                            throw (new Exception(string.Format(ServiceResources.MaterialAttributeValue, defaultCheck.Code)));
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

        //public void Delete(MaterialAttributeValue entity)
        //{
        //    //CheckIdUse(id);
        //    var data = base.Find(id);
        //    base.Delete(data);
        //}

        public decimal GetAutoSequence(string materialAttributeId, string materialId)
        {
            try
            {
                return string.IsNullOrEmpty(materialId) ? Query(t => t.MaterialAttributeId == materialAttributeId && !t.Archive).Select().Max(r => r.Sequence + 1) : Query(t => t.MaterialAttributeId == materialAttributeId && t.MaterialMasterId == materialId && !t.Archive).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00m;
            }
        }

        private void CheckIdUse(string id)
        {
            string sql = $"IF EXISTS(SELECT 1 FROM( " +
                            $"SELECT MaterialAttributeValueId AS CheckingColumn FROM {DbSchema.Masters}.[MaterialMasterArticleValue]" +
                            $") A WHERE CheckingColumn = '{id}') SELECT 1 ELSE SELECT 0 RETURN ";
            var data = Convert.ToBoolean(_charaterValueRepository.SqlQuery<int>(sql).Single());
            if (data)
                throw new CustomException("Already value exist in material master, you can't delete....!");
        }

        #region Material Ch Value

        public void InsertGraphFromMaterial(IEnumerable<MaterialAttributeValue> entities, IEnumerable<MaterialAttributeViewModel> materialMasterAttribute, string groupId, string materialMasterId)
        {

            foreach (var item in entities)
            {
                if (item.MaterialAttributeId.StartsWith("n-"))
                    item.MaterialAttributeId = materialMasterAttribute.Where(t => t.TempMaterialAttributeId == item.MaterialAttributeId).Select(t => t.MaterialAttributeId).FirstOrDefault();

                CheckDefault(item);
                CheckUnique(item);
                var parentData = _materialAttributeRepository.Query(t => t.Id == item.MaterialAttributeId).Select().FirstOrDefault();
                CheckPropertiesAndCharLength(item, parentData);
                item.Id = GetPK();
                item.CompanyGroupId = groupId;
                item.MaterialMasterId = materialMasterId;
                item.SourceType = ValueAssignmentEnum.Specific.ToString();
                InsertGraph(item);
            }
        }

        public void InsertUpdateOrDeleteFromMaterial(IEnumerable<MaterialAttributeValue> entities, IEnumerable<MaterialAttributeViewModel> materialMasterAttribute, string groupId
            , string materialMasterId, StringBuilder rdBuilder)
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
                        var parentData = _materialAttributeRepository.Query(t => t.Id == item.MaterialAttributeId).Select().FirstOrDefault();
                        CheckPropertiesAndCharLength(item, parentData);
                        if (item.Id.StartsWith("n-"))
                        {
                            if (item.MaterialAttributeId.StartsWith("n-"))
                                item.MaterialAttributeId = materialMasterAttribute.Where(t => t.TempMaterialAttributeId == item.MaterialAttributeId).Select(t => t.MaterialAttributeId).FirstOrDefault();

                            item.Id = GetPK();
                            item.CompanyGroupId = groupId;
                            item.MaterialMasterId = materialMasterId;
                            item.SourceType = ValueAssignmentEnum.Specific.ToString();
                            base.InsertGraph(item);
                        }
                        else
                            base.UpdateGraph(item);
                    }
                }
                if (dbList != null && dbList.Count() > 0)
                {
                    var builderSql = "";

                    foreach (var db in dbList)
                    {
                        if (!entities.Any(t => t.Id == db.Id))
                        {
                            builderSql = @"DELETE HKP.MaterialAttributeValue WHERE Id='" + db.Id + "';";
                            rdBuilder.Insert(0, builderSql);
                            builderSql = "";
                        }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void InsertOrUpdate(MaterialAttributeValue entity)
        {
            try
            {
                CheckDefault(entity);
                CheckUnique(entity);
                var parentData = _materialAttributeRepository.Query(t => t.Id == entity.MaterialAttributeId).Select().FirstOrDefault();
                CheckPropertiesAndCharLength(entity, parentData);
                if (entity.Id.StartsWith("n-"))
                {
                    entity.Id = GetPK();
                    entity.SourceType = ValueAssignmentEnum.Specific.ToString();
                    base.Insert(entity);
                }
                else
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
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraphByMaterial(string materialMasterId)
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetAttributeValueListByMaterialMaster(string materialMasterId)
        {
            try
            {
                var sql = @"SELECT Id, CompanyGroupId, MaterialAttributeId, MaterialMasterId, SourceType, Code, [Sequence], ShortName, StandardName, UserName, IsDefault, Remarks, [Description], Active
                    FROM HKP.MaterialAttributeValue WHERE MaterialMasterId='" + materialMasterId + "' AND SourceType='" + ValueAssignmentEnum.Specific + "'";
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