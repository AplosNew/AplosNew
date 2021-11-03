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
    public class IssueExternalAuditService : Service<IssueExternalAudit>, IIssueExternalAuditService
    {
        #region Constructor
        private readonly IRepositoryAsync<IssueExternalAudit> _issueExternalAuditRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public IssueExternalAuditService(
            IRepositoryAsync<IssueExternalAudit> IssueExternalAuditRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(IssueExternalAuditRepository, unitOfWork, pkGeneratorService)
        {
            _issueExternalAuditRepository = IssueExternalAuditRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

        //public decimal GetAutoSequence()
        //{
        //    try
        //    {
        //        return base.Query().Select().Max(r => r.Sequence + 1);
        //    }
        //    catch
        //    {
        //        return 1.00M;
        //    }
        //}
        
        public string GetPK()
        {
            return GetAutoNumber(nameof(IssueExternalAudit), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        //private void Check(IssueExternalAudit entity)
        //{
        //    CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
        //    CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        //}
        public override void Insert(IssueExternalAudit entity)
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
        public IssueExternalAudit IsExternalAuditReleased(string issueTransactionId)
        {
            try
            {
                var sql = @" select * from [dbo].[IssueExternalAudit] where [IssueTransactionId]  ='" + issueTransactionId + "'";
                IssueExternalAudit issueExternalAudit = _issueExternalAuditRepository.SelectQuery(sql).FirstOrDefault();
                return issueExternalAudit;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Update(IssueExternalAudit entity)
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
                parameters.CmdText = @"SELECT * FROM [dbo].[IssueExternalAudit] ";
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

        //public IEnumerable<object> GetCbo()
        //{
        //    try
        //    {
        //        return from m in base.Query(r => r.Active)
        //               select new { Text = m.UserName, Value = m.Id };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
        //    }
        //}
    }
}