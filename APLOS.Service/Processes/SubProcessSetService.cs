#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Processes;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Processes
{
    public partial class SubProcessSetService : Service<SubProcessSet>, ISubProcessSetService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ISubProcessSetDetailService _subprocessSetDetailService;

        public SubProcessSetService(
             IRepositoryAsync<SubProcessSet> SubProcessSetRepository
           , IPKGeneratorService pkGeneratorService
           , ISubProcessSetDetailService subprocessSetDetailService
           , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(SubProcessSetRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _subprocessSetDetailService = subprocessSetDetailService;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string entityId)
        {
            try
            {
                parameters.order = "asc";
                parameters.sort = "Code";
                parameters.CmdText = @"SELECT PS.Id
	                                    ,PS.CompanyGroupId
	                                    ,PS.CompanyId
	                                    ,PS.EntityId
	                                    ,PS.ProcessId
	                                    ,PS.ProcessTypeId
	                                    ,PS.Code
	                                    ,PS.RequiredTimeUnit
	                                    ,PS.[Description]
	                                    ,E.UserName AS Entity
	                                    ,PCAT.UserName AS Process
	                                    ,PCRI.UserName AS ProcessType
                                    FROM [" + DbSchema.HKP + @"].[" + DbTable.SubProcessSet + @"] AS PS
                                    LEFT OUTER JOIN [" + DbSchema.Organizations + @"].[Entity] AS E ON PS.EntityId=E.Id
                                    LEFT OUTER JOIN [" + DbSchema.HKP + @"].[" + DbTable.Process + @"] AS PCAT ON PS.ProcessId=PCAT.Id
                                    LEFT OUTER JOIN [" + DbSchema.HKP + @"].[" + DbTable.ProcessType + @"] AS PCRI ON PS.ProcessTypeId=PCRI.Id
                                    WHERE PS.EntityId='" + entityId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(SubProcessSet), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void CheckUnique(SubProcessSet entity)
        {
            //CheckUniqueColumn(UniqueColumnName.Entity, entity.EntityId, r => r.EntityId == entity.EntityId && r.Id != entity.Id && r.CompanyId == entity.CompanyId);
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && r.EntityId == entity.EntityId);
        }

        public void InsertGraph(SubProcessSet entity, IEnumerable<SubProcessSetDetail> subProcessSetDetail)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                base.InsertGraph(entity);
                _subprocessSetDetailService.InsertUpdateOrDeleteGraph(entity.Id, subProcessSetDetail);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateGraph(SubProcessSet entity, IEnumerable<SubProcessSetDetail> subProcessSetDetail)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                base.UpdateGraph(entity);
                _subprocessSetDetailService.InsertUpdateOrDeleteGraph(entity.Id, subProcessSetDetail);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "SubProcessSet Id"));
                _unitOfWork.BeginTransaction();
                flag = true;
                SubProcessSet entity = Find(id);
                base.DeleteGraph(entity);
                _subprocessSetDetailService.DeleteGraph(entity.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}