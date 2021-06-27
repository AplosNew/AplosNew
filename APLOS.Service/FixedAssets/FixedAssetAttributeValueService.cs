#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.FixedAssets
{
    public class FixedAssetAttributeValueService : Service<FixedAssetAttributeValue>, IFixedAssetAttributeValueService
    {
        #region Constructor

        private readonly IRepositoryAsync<FixedAssetAttributeValue> _fixedAssetAttributeValueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public FixedAssetAttributeValueService(
            IRepositoryAsync<FixedAssetAttributeValue> fixedAssetAttributeValueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(fixedAssetAttributeValueRepository, unitOfWork)
        {
            _fixedAssetAttributeValueRepository = fixedAssetAttributeValueRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string fixedAssetAttributeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT MAV.*, MA.UserName AS FixedAssetAttributeName " +
                                     $"FROM [HKP].[FixedAssetAttributeValue] AS MAV " +
                                     $"LEFT OUTER JOIN [HKP].[FixedAssetAttribute] AS MA ON MAV.FixedAssetAttributeId=MA.Id " +
                                     $"WHERE MAV.FixedAssetAttributeId='{fixedAssetAttributeId}' AND MAV.CompanyGroupId='{identity.CompanyGroupId}' AND MAV.Archive=0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public override void Insert(FixedAssetAttributeValue entity)
        {
            try
            {
                CheckDefault(entity);
                Check(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(FixedAssetAttributeValue), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(FixedAssetAttributeValue entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
        }

        private void CheckDefault(FixedAssetAttributeValue entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entity.IsDefault)
                {
                    if (entity.Active)
                    {
                        var defaultCheck = base.Query(m => m.Id != entity.Id && m.CompanyGroupId == identity.CompanyGroupId &&
                               m.FixedAssetAttributeId == entity.FixedAssetAttributeId && m.IsDefault && m.Active && !m.Archive).Select().FirstOrDefault();
                        if (defaultCheck != null && defaultCheck.IsDefault)
                            throw (new Exception(string.Format(ServiceResources.MaterialAttributeValue, defaultCheck.Code)));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public override void Update(FixedAssetAttributeValue entity)
        {
            try
            {
                CheckDefault(entity);
                Check(entity);
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public void Delete(string id)
        {
            try
            {
                if (_fixedAssetAttributeValueRepository.FKDependency("HKP.FixedAssetAttributeValue", id))
                    throw new CustomException("This value already used.");
                base.Delete(id);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
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
                return 1.00m;
            }
        }
    }
}