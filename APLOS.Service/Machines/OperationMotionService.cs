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
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Machines
{
    public class OperationMotionService : Service<OperationMotion>, IOperationMotionService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public OperationMotionService(
            IRepositoryAsync<OperationMotion> operationMotionRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(operationMotionRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence(string companyGroupId)
        {
            try
            {
                return Query(t => t.CompanyGroupId == companyGroupId).Select().Max(t => t.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        private void Check(OperationMotion entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Id != entity.Id && t.CompanyGroupId == entity.CompanyGroupId && t.Code == entity.Code && t.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.Id != entity.Id && t.CompanyGroupId == entity.CompanyGroupId && t.UserName == entity.UserName && t.Active);
        }

        public IEnumerable<object> GetCbo(string companyGroupId)
        {
            try
            {
                return (from m in base.Query(t => t.CompanyGroupId == companyGroupId && t.Active).Select().OrderBy(t => t.UserName)
                        select new { Text = m.UserName, Value = m.Id }).Distinct();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Menu.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [HKP].[OperationMotion] WHERE CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public override void Insert(OperationMotion entity)
        {
            try
            {
                Check(entity);
                entity.Id = GetAutoNumber(nameof(OperationMotion), PKGeneratorEnum.Auto, entity.CompanyGroupId, DateTime.Now);
                entity.Active = true;
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public override void Update(OperationMotion entity)
        {
            try
            {
                Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

    }
}