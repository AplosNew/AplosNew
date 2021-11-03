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
    public class ServiceGroupService : Service<ServiceGroup>, IServiceGroupService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ServiceGroupService(
            IRepositoryAsync<ServiceGroup> projectPlanningCategoryRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence(string serviceTypeId)
        {
            try
            {
                return base.Query(t => t.ServiceTypeId == serviceTypeId).Select(t => t.Sequence).Max() + 1;
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ServiceGroup), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(ServiceGroup entity)
        {
            try
            {
                CheckUnique(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public override void Update(ServiceGroup entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void Delete(string id)
        {
            try
            {
                var data = base.Find(id);
                base.Delete(id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        private void CheckUnique(ServiceGroup entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Code == entity.Code && t.Id != entity.Id && t.ServiceTypeId == entity.ServiceTypeId && t.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.UserName == entity.UserName && t.Id != entity.Id && t.ServiceTypeId == entity.ServiceTypeId && t.Active);
        }

        public GridModel Query(GridParameter parameters, string serviceTypeId)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id, A.ServiceTypeId, A.HSNCodeId
                    --, C.Code AS HSNCodeName
                    , A.[Sequence], A.Code, A.ShortName, A.StandardName, A.UserName, A.[Description], A.Remarks, A.Active
                    FROM [HKP].[ServiceGroup] AS A JOIN [HKP].[ServiceType] AS B ON A.ServiceTypeId=B.Id
                    --JOIN [HKP].[HSNCode] AS C ON A.HSNCodeId=C.Id  
                    WHERE A.ServiceTypeId='" + serviceTypeId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query(t => t.Active).Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }
    }
}