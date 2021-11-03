#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.TaskScheduler;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.TaskScheduler
{
    public class TaskSchedulerMasterService : Service<TaskSchedulerMaster>, ITaskSchedulerMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<TaskSchedulerMaster> _taskSchedulerMasterRepository;

        public TaskSchedulerMasterService(
            IRepositoryAsync<TaskSchedulerMaster> TaskSchedulerMasterRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(TaskSchedulerMasterRepository, unitOfWork, pkGeneratorService)
        {
            _taskSchedulerMasterRepository = TaskSchedulerMasterRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        

        private void Check(TaskSchedulerMaster entity)
        {
            //CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Id != entity.Id && t.Code == entity.Code && t.Active);
            //CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.Id != entity.Id && t.UserName == entity.UserName && t.Active);
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [Dbo].[TaskSchedulerMaster]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public override void Insert(TaskSchedulerMaster entity)
        {
            try
            {
                //Check(entity);
                entity.Id = GetAutoNumber(nameof(TaskSchedulerMaster), PKGeneratorEnum.Auto, DateTime.Now);
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public override void Update(TaskSchedulerMaster entity)
        {
            try
            {
                //Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public TaskSchedulerMaster GetTaskScheduleByAuditTaskSchedulerMasterId(string auditTaskSchedulerMasterId)
        {
            try
            {
                var _sql = "select * from TaskSchedulerMaster where Id = '" + auditTaskSchedulerMasterId + "'";
                return _taskSchedulerMasterRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}