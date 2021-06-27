#region Using

using Library.Core;
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

#endregion Using

namespace Library.Service.IssueTracker
{
    public class IssueFollowUpResponsibleService : Service<IssueFollowUpAudit>, IIssueFollowUpResponsibleService
    {
        #region Constructor

        private readonly IRepositoryAsync<IssueFollowUpAudit> _issueFollowUpAuditRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public IssueFollowUpResponsibleService(
            IRepositoryAsync<IssueFollowUpAudit> IssueFollowUpResponsibleRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(IssueFollowUpResponsibleRepository, unitOfWork, pkGeneratorService)
        {
            _issueFollowUpAuditRepository = IssueFollowUpResponsibleRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            
        }

        #endregion Constructor

       
        
        public string GetPK()
        {
            return GetAutoNumber(nameof(IssueFollowUpAudit), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private void Check(IssueFollowUpAudit entity)
        {
           // CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
           // CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }
        public override void Insert(IssueFollowUpAudit entity)
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
        public IssueFollowUpAudit IsFollowUpAuditReleased(string issueTransactionId)
        {
            try
            {
                var sql = @" select * from [dbo].[IssueFollowUpAudit] where [IssueTransactionId]  ='" + issueTransactionId + "'";
                IssueFollowUpAudit issueFollowUpAudit = _issueFollowUpAuditRepository.SelectQuery(sql).FirstOrDefault();
                return issueFollowUpAudit;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Dictionary<string, object>> GetById(string IssueFollowUpResponsibleId)
        {
            try
            {
                var sql = @"SELECT ia.* 
	                        FROM [dbo].[IssueFollowUpResponsible] AS ia
	                        LEFT JOIN [dbo].[IssueTransaction] AS itr
	                        ON ia.IssueTransactionId = itr.Id
	                        WHERE ia.Id ='" + IssueFollowUpResponsibleId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetIssueFollowUpResponsibleByIssueTransactionId(string issueTransactionId)
        {
            try
            {
                var sql = @"SELECT * FROM [dbo].[IssueFollowUpResponsible] WHERE IssueTransactionId ='" + issueTransactionId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }

        //public  void Insert(IssueFollowUpResponsible entity, IEnumerable<IssueFollowUpResponsibleDetail> IssueFollowUpResponsibleDetailList)
        //{
        //    var flag = false;
        //    try
        //    {
        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        //Check(entity);
        //        entity.Id = GetPK();
        //        base.Insert(entity);
        //        int detailId = 0;
        //        foreach (var item in issueFollowUpResponsibleDetailList)
        //        {
        //            detailId++;
        //            var issAuditDetail = new IssueFollowUpResponsibleDetail();
        //            issAuditDetail.Id = entity.Id + detailId;
        //            issAuditDetail.EmployeeId = item.EmployeeId;
        //            issAuditDetail.IssueFollowUpResponsibleId = entity.Id;
        //            issAuditDetail.AddedBy = entity.AddedBy;
        //            issAuditDetail.AddedFromIP = entity.AddedFromIP;
        //            issAuditDetail.AddedDate = entity.AddedDate;
        //            _IssueFollowUpResponsibleDetailRepository.Insert(issAuditDetail);
        //        }
        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        public void InsertIssueFollowUpResponsible(IssueFollowUpAudit entity)
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

        //public void InsertIssueFollowUpResponsibleDetail(IEnumerable<IssueFollowUpResponsibleDetail> IssueFollowUpResponsibleDetailList)
        //{
        //    try
        //    {
        //        //Check(entity);
        //        int detailId = 0;
        //        foreach (var item in IssueFollowUpResponsibleDetailList)
        //        {
        //            var issAuditDetail = new IssueFollowUpResponsibleDetail();
        //            if (string.IsNullOrEmpty(item.Id))
        //            {
        //                detailId++;
        //                issAuditDetail.Id = item.IssueFollowUpResponsibleId + detailId;
        //                issAuditDetail.EmployeeId = item.EmployeeId;
        //                issAuditDetail.IssueFollowUpResponsibleId = item.IssueFollowUpResponsibleId;
        //                AuditService.AddedLog(issAuditDetail);
        //                _IssueFollowUpResponsibleDetailRepository.Insert(issAuditDetail);
                       
        //            }
        //            else
        //            {
        //                issAuditDetail.EmployeeId = item.EmployeeId;
        //                issAuditDetail.IssueFollowUpResponsibleId = item.IssueFollowUpResponsibleId;
        //                AuditService.UpdatedLog(issAuditDetail);
        //                _IssueFollowUpResponsibleDetailRepository.Update(issAuditDetail);
                        
        //            }
        //            _unitOfWork.SaveChanges();
        //        }
                
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        public Dictionary<string, object> GetFile(string IssueFollowUpResponsibleId)
        {
            try
            {
                var sql = @"Select Id, Attachment From [dbo].[IssueFollowUpResponsible]  Where Id='" + IssueFollowUpResponsibleId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        
        public override void Update(IssueFollowUpAudit entity)
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
                parameters.CmdText = @"SELECT   ir.* 
                                                ,it.Issue
	                                            ,it.IssueDetail
		                                        FROM [Dbo].[IssueFollowUpResponsible] ir
		                                        LEFT JOIN [Dbo].[IssueTransaction] it
		                                        ON ir.IssueTransactionId = it.Id ";
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

        public GridModel GetListIssueFollowUpResponsible(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT  ia.*
						,itr.Issue 
	                    FROM [dbo].[IssueFollowUpResponsible] AS ia
						LEFT JOIN [dbo].[IssueTransaction] itr
						ON ia.IssueTransactionId = itr.Id
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

    }
}