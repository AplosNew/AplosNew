#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.IssueTracker;
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

namespace Library.Service.IssueTracker
{
    public class IssueTransactionService : Service<IssueTransaction>, IIssueTransactionService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<IssueTransaction> _issueTransactionRepository;

        public IssueTransactionService(
            IRepositoryAsync<IssueTransaction> IssueTransactionRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(IssueTransactionRepository, unitOfWork, pkGeneratorService)
        {
            _issueTransactionRepository = IssueTransactionRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

        public string GetPK()
        {
            return GetAutoNumber(nameof(IssueTransaction), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query().Select().OrderBy(r => r.Id)
                       select new { Text = m.Issue, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }
        private string GetEmployeeSystemId(string employeeId)
        {
            try
            {
                string SystemId = @"SELECT SystemId FROM EmployeeInformation WHERE EmployeeId = '" + employeeId + "'";
                return SystemId;

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Insert(IssueTransaction entity)
        {
            if (entity.AssignById == null)
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                entity.AssignById = identity.EmployeeId;
                var x = identity.UserId;

            }

            if (entity.IsUpdateApplicable == false)
            {
                entity.UpdateResponsiblePersonId = null;
                entity.UpdateOneTimeDateTime = null;
                entity.IsUpdateRecurring = false;
            }
            else
            {
                if (entity.IsUpdateRecurring == false)
                {
                }
                else
                {
                    entity.UpdateOneTimeDateTime = null;
                }
            }

            if (entity.IsFollowUpApplicable == false)
            {
                entity.IsFollowUpRecurring = false;
                entity.FollowUpResponsiblePersonId = null;
                entity.FollowUpOneTimeDateTime = null;
            }
            else
            {
                if (entity.IsFollowUpRecurring == false)
                {
                }
                else
                {
                    entity.FollowUpOneTimeDateTime = null;
                }
            }

            if (entity.IsInternalApplicable == false)
            {
                entity.IsInternalRecurring = false;
                entity.InternalResponsiblePersonId = null;
                entity.InternalOneTimeDateTime = null;
            }
            else
            {
                if (entity.IsInternalRecurring == false)
                {
                    //entity.InternalFrequencyType = null;
                    //entity.InternalFrequencyDays = null;
                    //entity.InternalEndDateTime = null;
                }
                else
                {
                    entity.InternalOneTimeDateTime = null;
                }
            }

            if (entity.IsExternalApplicable == false)
            {
                entity.IsExternalRecurring = false;

                //entity.ExternalFrequencyType = null;
                //entity.ExternalFrequencyDays = null;
                //entity.ExternalEndDateTime = null;

                entity.ExternalResponsiblePersonId = null;
                entity.ExternalOneTimeDateTime = null;
            }
            else
            {
                if (entity.IsExternalRecurring == false)
                {
                    //entity.ExternalFrequencyType = null;
                    //entity.ExternalFrequencyDays = null;
                    //entity.ExternalEndDateTime = null;
                }
                else
                {
                    entity.ExternalOneTimeDateTime = null;
                }
            }

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


        public override void Update(IssueTransaction entity)
        {
            if (entity.AssignById == null)
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                entity.AssignById = identity.EmployeeId;

            }

            if (entity.IsUpdateApplicable == false)
            {
                entity.UpdateResponsiblePersonId = null;
                entity.UpdateOneTimeDateTime = null;
                entity.IsUpdateRecurring = false;
            }
            else
            {
                if (entity.IsUpdateRecurring == false)
                {
                    entity.UpdateAuditTaskSchedulerMasterId = null;

                }
            }

            if (entity.IsFollowUpApplicable == false)
            {
                entity.IsFollowUpRecurring = false;
                entity.FollowUpResponsiblePersonId = null;
                entity.FollowUpOneTimeDateTime = null;
            }
            else
            {
                if (entity.IsFollowUpRecurring == false)
                {
                    entity.FollowUpAuditTaskSchedulerMasterId = null;
                }
            }

            if (entity.IsInternalApplicable == false)
            {
                entity.IsInternalRecurring = false;
                entity.InternalResponsiblePersonId = null;
                entity.InternalOneTimeDateTime = null;
            }
            else
            {
                if (entity.IsInternalRecurring == false)
                {
                    entity.InternalAuditTaskSchedulerMasterId = null;
                }
            }

            if (entity.IsExternalApplicable == false)
            {
                entity.IsExternalRecurring = false;
                entity.ExternalResponsiblePersonId = null;
                entity.ExternalOneTimeDateTime = null;
            }
            else
            {
                if (entity.IsExternalRecurring == false)
                {
                    entity.ExternalAuditTaskSchedulerMasterId = null;
                }
            }

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
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"
						SELECT  its.*
						,ie.EmployeeName AS AssignToName
					    ,iap.EmployeeName AS AssignBy
						,urp.EmployeeName AS UpdateResponsiblePerson
						,furp.EmployeeName AS FollowUpResponsiblePerson
						,irp.EmployeeName AS InternalResponsiblePerson
						,erp.EmployeeName AS ExternalResponsiblePersonName
                        ,tc.UserName AS TaskCategory
	                    ,tsc.UserName AS TaskSubCategory
	                     ,iim.UserName AS IssueImportance
						 ,ig.Name AS IssueGroupName
						 ,p.userName as Customer
	                  
						,imn.EmployeeName AS Mentor
	                    FROM [dbo].[IssueTransaction] AS its
						LEFT JOIN [dbo].[IssueStandard] AS isd ON its.IssueStandardId = isd.Id
	                    LEFT JOIN [dbo].[IssueImportance] AS iim ON its.IssueImportanceId = iim.Id
						LEFT JOIN [dbo].[EmployeeInformation] AS ie ON its.AssignToId = ie.SystemId
						LEFT JOIN [dbo].[EmployeeInformation] AS imn ON its.MentorId = imn.SystemId
						LEFT JOIN [dbo].[EmployeeInformation] AS iap ON its.AssignById = iap.SystemId
						LEFT JOIN [dbo].[EmployeeInformation] AS urp ON its.UpdateResponsiblePersonId = urp.SystemId
						LEFT JOIN [dbo].[EmployeeInformation] AS furp ON its.FollowUpResponsiblePersonId = furp.SystemId
						LEFT JOIN [dbo].[EmployeeInformation] AS irp ON its.InternalResponsiblePersonId = irp.SystemId
						LEFT JOIN [dbo].[EmployeeInformation] AS erp ON its.ExternalResponsiblePersonId = erp.SystemId
						LEFT JOIN [HKP].TaskCategory AS tc ON its.TaskCategoryId = tc.Id
						LEFT JOIN [HKP].TaskSubCategory AS tsc ON its.TaskSubCategoryId = tsc.Id
						LEFT JOIN IssueGroup AS ig ON ig.Id = its.IssueGroupId
						left join hkp.Party p on p.Id = its.CustomerId
                        WHERE its.AddedBy = '" + identity.UserId + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel GetListIssueTransaction(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"
						SELECT  its.*,its.Id AS IssueTransactionId
						,ie.EmployeeName AS AssignToName
					    ,iap.EmployeeName AS AuthorisedPersonName
                        ,tc.UserName AS TaskCategory
	                    ,tsc.UserName AS TaskSubCategory
	                     ,iim.UserName AS IssueImportance
						 ,ig.Name AS IssueGroupName
	                    
						,imn.EmployeeName AS Mentor
	                    FROM [dbo].[IssueTransaction] AS its
						LEFT JOIN [dbo].[IssueStandard] AS isd
	                    ON its.IssueStandardId = isd.Id

	                   LEFT JOIN HKP.TaskCategory tc ON tc.Id = its.TaskCategoryId
                       LEFT JOIN HKP.TaskSubCategory tsc ON tsc.Id = its.TaskSubCategoryId
	                LEFT JOIN [dbo].[IssueImportance] AS iim ON its.IssueImportanceId = iim.Id
					LEFT JOIN [dbo].[EmployeeInformation] AS ie ON its.AssignToId = ie.SystemId
					LEFT JOIN [dbo].[EmployeeInformation] AS imn ON its.MentorId = imn.SystemId
					LEFT JOIN [dbo].[EmployeeInformation] AS iap ON its.AssignById = iap.SystemId
					LEFT JOIN IssueGroup AS ig ON ig.Id = its.IssueGroupId

						";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetById(string issueTransactionId)
        {
            try
            {
                var sql = @"SELECT itr.* 
                            ,tc.UserName AS IssueCategory
                            ,tsc.UserName AS IssueSubCategory
                            ,iim.UserName AS IssueImportance
							,ig.Name AS IssueGroupName
                            FROM [dbo].[IssueTransaction] AS itr
                            LEFT JOIN HKP.TaskCategory tc ON tc.Id = itr.TaskCategoryId
                            LEFT JOIN HKP.TaskSubCategory tsc ON tsc.Id = itr.TaskSubCategoryId
                            LEFT JOIN [dbo].[IssueImportance] iim ON iim.Id = itr.IssueImportanceId
							LEFT JOIN IssueGroup ig ON ig.Id = itr.IssueGroupId
                            WHERE itr.Id='" + issueTransactionId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetToDoList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var assignToId = identity.EmployeeId;
            try
            {
                var sql = @"SELECT itr.* 
                            ,isc.UserName AS IssueSubCategory
                            ,iim.UserName AS IssueImportance
							,asby.EmpPicPath
							,asby.EmployeeName AS AssignBy
							,asto.EmployeeName AS AssignTo
                            
                            FROM [dbo].[IssueTransaction] AS itr
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

							WHERE AssignToId ='" + assignToId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetTodayTaskList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var assignToId = identity.EmployeeId;
            try
            {
                var sql = @"SELECT itr.* 
                            ,ic.UserName AS IssueCategory
                            ,isc.UserName AS IssueSubCategory
                            ,iim.UserName AS IssueImportance
							,asby.EmpPicPath
							,asby.EmployeeName AS AssignBy
							,asto.EmployeeName AS AssignTo
                            
                            FROM [dbo].[IssueTransaction] AS itr
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

							WHERE AssignToId ='" + assignToId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }

        public void Delete(string Id)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(Id);
                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM [dbo].[IssueExternalAudit] WHERE IssueTransactionId = '" + Id + "' ");
                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM [dbo].[IssueFollowUpAudit] WHERE IssueTransactionId = '" + Id + "' ");
                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM [dbo].[IssueInternalAudit] WHERE IssueTransactionId = '" + Id + "' ");
                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM [dbo].[IssueUpdateAudit] WHERE IssueTransactionId = '" + Id + "' ");
                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM [dbo].[IssueSubTask] WHERE IssueTransactionId = '" + Id + "' ");
                base.Delete(data);
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

        public decimal GetAutoSequence()
        {
            throw new NotImplementedException();
        }
        public GridModel BuyerList(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @" SELECT * FROM [HKP].[Buyer]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IssueTransaction GetIssueTransaction(string issueTransactionId)
        {
            try
            {
                var _sql = "select * from [dbo].[IssueTransaction] where id ='" + issueTransactionId + "'";
                return _issueTransactionRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Dictionary<string, object>> GetTaskScheduleForAllAudit(string issueTransactionId)
        {
            try
            {
                var sql = @"SELECT itr.* 
                            ,ic.UserName AS IssueCategory
                            ,isc.UserName AS IssueSubCategory
                            ,iim.UserName AS IssueImportance
                            FROM [dbo].[IssueTransaction] AS itr
                            LEFT JOIN [dbo].[IssueCategory] ic
                            ON ic.Id = itr.IssueCategoryId
                            LEFT JOIN [dbo].[IssueSubCategory] isc
                            ON isc.Id = itr.IssueSubCategoryId
                            LEFT JOIN [dbo].[IssueImportance] iim
                            ON iim.Id = itr.IssueImportanceId

                            WHERE itr.Id='" + issueTransactionId + "'";
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