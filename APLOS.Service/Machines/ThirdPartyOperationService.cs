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
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Machines
{
    public partial class ThirdPartyOperationService : Service<ThirdPartyOperation>, IThirdPartyOperationService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyGroupThirdPartyOperationService _comgroupThridPartyService;
        private readonly ISqlRepository _sqlRepository;

        public ThirdPartyOperationService(
            IRepositoryAsync<ThirdPartyOperation> thirdPartyRepository,
            IPKGeneratorService pkGeneratorService, ICompanyGroupThirdPartyOperationService comgroupThridPartyService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(thirdPartyRepository, unitOfWork, pkGeneratorService)
        {
            _comgroupThridPartyService = comgroupThridPartyService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(ThirdPartyOperation entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                InsertGraph(entity);
                _comgroupThridPartyService.Insert(new CompanyGroupThirdPartyOperation { ThirdPartyOperationId = entity.Id, Active = entity.Active });
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

        private string GetPK()
        {
            return GetAutoNumber(nameof(ThirdPartyOperation), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(ThirdPartyOperation entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && !r.Archive);
            //CheckUniqueColumn(UniqueColumnName.Description, entity.Description, r => r.Id != entity.Id && r.Description == entity.Description && !r.Archive);
        }

        public override void Update(ThirdPartyOperation entity)
        {
            try
            {
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
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void DeleteGraph(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(key);
                _comgroupThridPartyService.DeleteGraph(entity.Id);
                base.DeleteGraph(entity.Id);
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
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = $"SELECT mt.Id AS Value, mt.Code  FROM {DbSchema.Masters}.[{DbTable.ThirdPartyOperation}] AS mt  " +
                          $"left outer join(SELECT * FROM HKP.[{DbTable.CompanyGroupThirdPartyOperation}] " +
                          $"WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgu ON mt.Id = cgu.ThirdPartyOperationId " +
                          "WHERE ISNULL(cgu.Id, '')<> '' AND mt.Archive = 0 AND mt.Active=1 ORDER BY mt.Code";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT mt.Id, mt.Code, mt.TMU, mt.IsMachine, mt.[Grouping], mt.[Description], mt.Active  FROM MST.[ThirdPartyOperation] AS mt
                            LEFT OUTER JOIN(SELECT * FROM HKP.[CompanyGroupThirdPartyOperation] WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgu ON mt.Id = cgu.ThirdPartyOperationId
                            WHERE ISNULL(cgu.Id, '')<> '' AND mt.Archive = 0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }
    }
}