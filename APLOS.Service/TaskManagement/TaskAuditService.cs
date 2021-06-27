#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.TaskManagement;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.TaskManagement
{
    public class TaskAuditService : Service<TaskAudit>, ITaskAuditService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<TaskAudit> _taskAuditRepository;

        public TaskAuditService(
            IRepositoryAsync<TaskAudit> TaskAuditRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(TaskAuditRepository, unitOfWork, pkGeneratorService)
        {
            _taskAuditRepository = TaskAuditRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

        public string GetPK()
        {
            return GetAutoNumber(nameof(TaskAudit), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private void Check(TaskAudit entity)
        {
            //CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            //CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }
        public override void Insert(TaskAudit entity)
        {
            try
            {
                //Check(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(TaskAudit entity)
        {
            try
            {
                //Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [dbo].[TaskAudit] ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void Delete(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                base.Delete(Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public TaskAudit GetTaskAudit(string taskManagerMasterId, string assignById)
        {
            try
            {


                var _sql = "select * from [dbo].[TaskAudit] where AuthorizationType = '" + assignById + "' AND TaskManagerMasterId ='" + taskManagerMasterId + "'";
                return _taskAuditRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TaskAudit GetTaskAuditByTaskAuditId(string taskAuditId)
        {
            try
            {
                var _sql = "select * from [dbo].[TaskAudit] where id ='" + taskAuditId + "'";
                return _taskAuditRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region GetAuditOfReleasedIssue
        public enum AuditPerson
        {
            Issue = 1,
            UpdateAudit,
            FollowUpAudit,
            InternalAudit,
            ExternalAudit

        }
        public IEnumerable<object> GetAuditOfReleasedIssue(string issueTransactionId, int audit)
        {
            AuditPerson x = ((AuditPerson)audit);
            string auditType = x.ToString();

            try
            {
                //var _sql = @"SELECT TA.*, E.EmployeeName AS ResponsiblePerson FROM TaskAudit TA LEFT JOIN TaskManagerMaster TMM ON TMM.Id = TA.TaskManagerMasterId
                //           left join dbo.EmployeeInformation E ON E.SystemId = TA.ResponsiblePersonId  WHERE IssueTransactionId = '" + issueTransactionId + "' AND AuthorizationType = '"+ auditType + "'";

                var _sql = @"select TA.* ,E.EmployeeName AS ResponsiblePerson from TaskManagerMaster  TMM 
                            left join TaskAudit TA ON TA.TaskManagerMasterId = TMM.Id 
                            left join EmployeeInformation E ON E.SystemId = TA.ResponsiblePersonId
                            where IssueTransactionId = '" + issueTransactionId + "' and TaskType = '" + auditType + "' and TA.AuthorizationType = 'AssignTo'";


                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion 
    }
}