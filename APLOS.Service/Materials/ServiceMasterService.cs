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
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Materials
{
    public class ServiceMasterService : Service<ServiceMaster>, IServiceMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ServiceMasterService(
            IRepositoryAsync<ServiceMaster> projectPlanningCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select(t => t.Sequence).Max() + 1;
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ServiceMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(ServiceMaster entity)
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

        public override void Update(ServiceMaster entity)
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

        private void CheckUnique(ServiceMaster entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Code == entity.Code && t.Id != entity.Id && t.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.UserName == entity.UserName && t.Id != entity.Id && t.Active);
        }

        public GridModel Query(GridParameter parameters, string[] ids)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id, A.ServiceGroupId, B.UserName AS ServiceGroupName,A.TransactionUoMId, A.[Sequence], A.Code, A.UserName, A.StandardName, A.[Description], A.Remarks, A.Active,A.HSNCodeId,A.CompanyId
                            FROM [HKP].[ServiceMaster] AS A JOIN [HKP].[ServiceGroup] AS B ON A.ServiceGroupId=B.Id WHERE A.Id NOT IN (" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public GridModel QueryServiceMaster(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id, A.ServiceGroupId, B.UserName AS ServiceGroupName, A.[Sequence], A.Code, A.UserName, A.StandardName, A.[Description], A.Remarks, A.Active,A.HSNCodeId
                            FROM [HKP].[ServiceMaster] AS A JOIN [HKP].[ServiceGroup] AS B ON A.ServiceGroupId=B.Id WHERE A.Id NOT IN (Select ServiceMasterId from HKP.CompanyServiceMaster)";
                return _sqlRepository.GetGridData(parameters);
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