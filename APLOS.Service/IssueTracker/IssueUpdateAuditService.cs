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
    public class IssueUpdateAuditService : Service<IssueUpdateAudit>, IIssueUpdateAuditService
    {
        #region Constructor

        private readonly IRepositoryAsync<IssueRefDetail> _issueRefDetailRepository;
        private readonly IRepositoryAsync<IssueUpdateAudit> _issueUpdateAuditRepository;
        
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public IssueUpdateAuditService(
            IRepositoryAsync<IssueUpdateAudit> issueUpdateAuditRepository
            , IRepositoryAsync<IssueRefDetail> issueRefDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(issueUpdateAuditRepository, unitOfWork, pkGeneratorService)
        {
            _issueUpdateAuditRepository = issueUpdateAuditRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            _issueRefDetailRepository = issueRefDetailRepository;
        }

        #endregion Constructor

       
        
        public string GetPK()
        {
            return GetAutoNumber(nameof(IssueUpdateAudit), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private void Check(IssueUpdateAudit entity)
        {
           // CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
           // CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }
        public override void Insert(IssueUpdateAudit entity)
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
        public IssueUpdateAudit IsUpdateAuditReleased(string issueTransactionId)
        {
            try
            {
                var sql = @" select * from [dbo].[IssueUpdateAudit] where [IssueTransactionId]  ='" + issueTransactionId + "'";
                IssueUpdateAudit issueUpdateAudit = _issueUpdateAuditRepository.SelectQuery(sql).FirstOrDefault();
                return issueUpdateAudit;
               
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Dictionary<string, object>> GetById(string issueRefId)
        {
            try
            {
                var sql = @"SELECT ird.*
                    ,ei.EmployeeName
                    FROM [dbo].[IssueRefDetail] ird
                    LEFT JOIN [dbo].[EmployeeInformation] ei
                    ON ird.EmployeeId = ei.SystemId
                    WHERE ird.IssueRefId ='" + issueRefId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetIssueUpdateAuditByIssueTransactionId(string issueTransactionId)
        {
            try
            {
                var sql = @"SELECT * FROM [dbo].[IssueUpdateAudit] WHERE IssueTransactionId ='" + issueTransactionId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }

        public  void Insert(IssueUpdateAudit entity, IEnumerable<IssueRefDetail> issueRefDetailList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                //Check(entity);
                entity.Id = GetPK();
                base.Insert(entity);
                int detailId = 0;
                foreach (var item in issueRefDetailList)
                {
                    detailId++;
                    var issRefDetail = new IssueRefDetail();
                    issRefDetail.Id = entity.Id + detailId;
                    issRefDetail.EmployeeId = item.EmployeeId;
                    issRefDetail.IssueRefId = entity.Id;
                    issRefDetail.AddedBy = entity.AddedBy;
                    issRefDetail.AddedFromIP = entity.AddedFromIP;
                    issRefDetail.AddedDate = entity.AddedDate;
                    _issueRefDetailRepository.Insert(issRefDetail);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertIssueUpdateAudit(IssueUpdateAudit entity)
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


        public void InsertIssueRefDetail(IEnumerable<IssueRefDetail> issueRefDetailList)
        {
            try
            {
                //Check(entity);
                int detailId = 0;
                foreach (var item in issueRefDetailList)
                {
                    var issRefDetail = new IssueRefDetail();
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        detailId++;
                        issRefDetail.Id = item.IssueRefId + detailId;
                        issRefDetail.EmployeeId = item.EmployeeId;
                        issRefDetail.IssueRefId = item.IssueRefId;
                        AuditService.AddedLog(issRefDetail);
                        _issueRefDetailRepository.Insert(issRefDetail);
                       
                    }
                    else
                    {
                        issRefDetail.EmployeeId = item.EmployeeId;
                        issRefDetail.IssueRefId = item.IssueRefId;
                        AuditService.UpdatedLog(issRefDetail);
                        _issueRefDetailRepository.Update(issRefDetail);
                        
                    }
                    _unitOfWork.SaveChanges();
                }
                
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public Dictionary<string, object> GetFile(string systemId)
        {
            try
            {
                var sql = @"Select Id, Attachment From [dbo].[IssueRef]  Where Id='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        
        public override void Update(IssueUpdateAudit entity)
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
		                                        FROM [Dbo].[IssueRef] ir
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

        public GridModel GetListIssueRef(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT  ir.*
						,its.Issue 
	                    FROM [dbo].[IssueRef] AS ir
						LEFT JOIN [dbo].[IssueTransaction] its
						ON ir.IssueTransactionId = its.Id
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