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
    public class IssueInternalAuditService : Service<IssueInternalAudit>, IIssueInternalAuditService
    {
        #region Constructor

        //private readonly IRepositoryAsync<IssueAuditDetail> _issueAuditDetailRepository;
        private readonly IRepositoryAsync<IssueInternalAudit> _issueInternalAuditRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public IssueInternalAuditService(
            IRepositoryAsync<IssueInternalAudit> IssueInternalAuditRepository
            //, IRepositoryAsync<IssueAuditDetail> issueAuditDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(IssueInternalAuditRepository, unitOfWork, pkGeneratorService)
        {
            _issueInternalAuditRepository = IssueInternalAuditRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            //_issueAuditDetailRepository = issueAuditDetailRepository;
        }

        #endregion Constructor

       
        
        public string GetPK()
        {
            return GetAutoNumber(nameof(IssueInternalAudit), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private void Check(IssueInternalAudit entity)
        {
           // CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
           // CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }
        public override void Insert(IssueInternalAudit entity)
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

        public IssueInternalAudit IsInternalAuditReleased(string issueTransactionId)
        {
            try
            {
                var sql = @" select * from [dbo].[IssueInternalAudit] where [IssueTransactionId]  ='" + issueTransactionId + "'";
                IssueInternalAudit issueInternalAudit = _issueInternalAuditRepository.SelectQuery(sql).FirstOrDefault();
                return issueInternalAudit;

            }
            catch (Exception)
            {
                throw;
            }
        }
        //public List<Dictionary<string, object>> GetById(string issueAuditId)
        //{
        //    try
        //    {
        //        var sql = @"SELECT ia.* 
        //                 FROM [dbo].[IssueAudit] AS ia
        //                 LEFT JOIN [dbo].[IssueTransaction] AS itr
        //                 ON ia.IssueTransactionId = itr.Id
        //                 WHERE ia.Id ='" + issueAuditId + "'";
        //        return _sqlRepository.GetDataCollection(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
        //    }
        //}

        //public List<Dictionary<string, object>> GetIssueAuditByIssueTransactionId(string issueTransactionId)
        //{
        //    try
        //    {
        //        var sql = @"SELECT * FROM [dbo].[IssueAudit] WHERE IssueTransactionId ='" + issueTransactionId + "'";
        //        return _sqlRepository.GetDataCollection(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
        //    }
        //}

        //public  void Insert(IssueAudit entity, IEnumerable<IssueAuditDetail> issueAuditDetailList)
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
        //        foreach (var item in issueAuditDetailList)
        //        {
        //            detailId++;
        //            var issAuditDetail = new IssueAuditDetail();
        //            issAuditDetail.Id = entity.Id + detailId;
        //            issAuditDetail.EmployeeId = item.EmployeeId;
        //            issAuditDetail.IssueAuditId = entity.Id;
        //            issAuditDetail.AddedBy = entity.AddedBy;
        //            issAuditDetail.AddedFromIP = entity.AddedFromIP;
        //            issAuditDetail.AddedDate = entity.AddedDate;
        //            _issueAuditDetailRepository.Insert(issAuditDetail);
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

        //public void InsertIssueAudit(IssueInternalAudit entity)
        //{
        //    try
        //    {
        //        //Check(entity);
        //        entity.Id = GetPK();

        //        base.Insert(entity);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        //public void InsertIssueAuditDetail(IEnumerable<IssueAuditDetail> issueAuditDetailList)
        //{
        //    try
        //    {
        //        //Check(entity);
        //        int detailId = 0;
        //        foreach (var item in issueAuditDetailList)
        //        {
        //            var issAuditDetail = new IssueAuditDetail();
        //            if (string.IsNullOrEmpty(item.Id))
        //            {
        //                detailId++;
        //                issAuditDetail.Id = item.IssueAuditId + detailId;
        //                issAuditDetail.EmployeeId = item.EmployeeId;
        //                issAuditDetail.IssueAuditId = item.IssueAuditId;
        //                AuditService.AddedLog(issAuditDetail);
        //                _issueAuditDetailRepository.Insert(issAuditDetail);

        //            }
        //            else
        //            {
        //                issAuditDetail.EmployeeId = item.EmployeeId;
        //                issAuditDetail.IssueAuditId = item.IssueAuditId;
        //                AuditService.UpdatedLog(issAuditDetail);
        //                _issueAuditDetailRepository.Update(issAuditDetail);

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

        //public Dictionary<string, object> GetFile(string issueAuditId)
        //{
        //    try
        //    {
        //        var sql = @"Select Id, Attachment From [dbo].[IssueAudit]  Where Id='" + issueAuditId + "'";
        //        return _sqlRepository.GetData(sql, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        public override void Update(IssueInternalAudit entity)
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
		                                        FROM [Dbo].[IssueAudit] ir
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

        public GridModel GetListIssueAudit(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT  ia.*
						,itr.Issue 
	                    FROM [dbo].[IssueAudit] AS ia
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