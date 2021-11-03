#region Using

using Library.Core;
using Library.Crosscutting.Security;
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
using System.Threading;

#endregion Using

namespace Library.Service.TaskManagement
{
    public class TaskManagerMasterService : Service<TaskManagerMaster>, ITaskManagerMasterService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<TaskManagerMaster> _taskManagerMasterRepository;

        public TaskManagerMasterService(
            IRepositoryAsync<TaskManagerMaster> TaskManagerMasterRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository

            ) : base(TaskManagerMasterRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            _taskManagerMasterRepository = TaskManagerMasterRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return GetAutoNumber(nameof(TaskManagerMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private void Check(TaskManagerMaster entity)
        {
            //CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            //CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }
        public void InsertTaskManagerMasterForIssue(TaskManagerMaster entity, out string Id)
        {
            try
            {
                Check(entity);
                entity.Id = GetPK();
                base.Insert(entity);
                Id = entity.Id;

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
          Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
          ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public override void Insert(TaskManagerMaster entity)
        {
            try
            {
                Check(entity);
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

        public override void Update(TaskManagerMaster entity)
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
                parameters.CmdText = @"SELECT * FROM [dbo].[TaskManagerMaster] ";
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

        public TaskManagerMaster GetTaskManagerMasterByIssueTransactionId(string issueTransactionId)
        {
            try
            {
                var taskType = TaskTypeEnum.Issue.ToString();
                var _sql = "select * from [dbo].[TaskManagerMaster] where TaskType = '" + taskType + "' AND IssueTransactionId ='" + issueTransactionId + "'";
                return _taskManagerMasterRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TaskManagerMaster GetTaskManagerMaster(string issueTransactionId, string tasktype)
        {
            try
            {
                var _sql = "select * from [dbo].[TaskManagerMaster] where TaskType = '" + tasktype + "' AND IssueTransactionId ='" + issueTransactionId + "'";
                return _taskManagerMasterRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Dictionary<string, object>> GetToDoList()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var assignToId = identity.EmployeeId;

            try
            {
                var sql = @"SELECT tmm.*
							
							
                            ,b.StandardName AS BuyerName 
                            ,ic.UserName AS IssueCategory
                            ,isc.UserName AS IssueSubCategory
                            ,iim.UserName AS IssueImportance
							,asby.EmpPicPath
							,asby.EmployeeName AS AssignBy
							
                            
                            FROM [dbo].[TaskManagerMaster] AS tmm
							LEFT JOIN [dbo].[IssueTransaction] itr
							on tmm.IssueTransactionId = itr.Id
                            LEFT JOIN [HKP].[Buyer] AS b 
							ON itr.BuyerId = b.Id
                            LEFT JOIN [dbo].[IssueCategory] ic
                            ON ic.Id = itr.IssueCategoryId
                            LEFT JOIN [dbo].[IssueSubCategory] isc
                            ON isc.Id = itr.IssueSubCategoryId
                            LEFT JOIN [dbo].[IssueImportance] iim
                            ON iim.Id = itr.IssueImportanceId
							LEFT JOIN [dbo].[EmployeeInformation] asby
							ON asby.SystemId = itr.AssignById 
							LEFT JOIN [dbo].[EmployeeInformation] asto
							ON asto.SystemId = itr.AssignToId

							WHERE itr.AssignToId ='" + assignToId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetTaskAccordingToRresponsiblePersonList(string authorizationType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var logedInUser = identity.EmployeeId;

            try
            {
                var sql = @"SELECT tmm.*
							
                            
                            ,ic.UserName AS IssueCategory
                            ,isc.UserName AS IssueSubCategory
                            ,iim.UserName AS IssueImportance
							,asby.EmpPicPath
							,asby.EmployeeName AS AssignBy
							,ta.Id AS TaskAuditId
							,ta.ResponsiblePersonId
							,ta.AuthorizationType
							,ta.CommitmentDate
							,ta.Remarks
							,ta.RevisedCommitmentDate
							
                            
                            FROM [dbo].[TaskManagerMaster] AS tmm
							LEFT JOIN [dbo].[IssueTransaction] itr
							on tmm.IssueTransactionId = itr.Id
                            
                            LEFT JOIN [dbo].[IssueCategory] ic
                            ON ic.Id = itr.IssueCategoryId
                            LEFT JOIN [dbo].[IssueSubCategory] isc
                            ON isc.Id = itr.IssueSubCategoryId
                            LEFT JOIN [dbo].[IssueImportance] iim
                            ON iim.Id = itr.IssueImportanceId
							LEFT JOIN [dbo].[EmployeeInformation] asby
							ON asby.SystemId = itr.AssignById 
							LEFT JOIN [dbo].[EmployeeInformation] asto
							ON asto.SystemId = itr.AssignToId
							LEFT JOIN [dbo].[TaskAudit] ta
							ON ta.TaskManagerMasterId = tmm.Id

							WHERE ta.ResponsiblePersonId ='" + logedInUser + "' AND  ta.AuthorizationType = '" + authorizationType + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }
    }
}