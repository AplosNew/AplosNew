#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Processes;
using Library.Service.Accounts;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Processes
{
    public partial class ProcessSetService : Service<ProcessSet>, IProcessSetService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IProcessSetDetailService _processSetDetailService;

        public ProcessSetService(
              IRepositoryAsync<ProcessSet> ProcessSetRepository
            , IPKGeneratorService pkGeneratorService
            , IProcessSetDetailService processSetDetailService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(ProcessSetRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _processSetDetailService = processSetDetailService;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string companyId, string entityId)
        {
            try
            {
                parameters.order = "asc";
                parameters.sort = "Code";
                parameters.CmdText = @"SELECT PS.Id, PS.CompanyGroupId, PS.CompanyId, PS.EntityId
	                                    , PS.ProcessCategoryId, PCAT.UserName AS ProcessCategory
										, PS.ProcessCriteriaId, PCRI.UserName AS ProcessCriteria
	                                    , PS.Code, PS.[Description]
	                                    , PS.RequiredTimeUnit, E.UserName AS Entity
                    FROM [" + DbSchema.HKP + @"].[" + DbTable.ProcessSet + @"] AS PS
                    LEFT OUTER JOIN [" + DbSchema.Organizations + @"].[Entity] AS E ON PS.EntityId=E.Id
                    LEFT OUTER JOIN [" + DbSchema.HKP + @"].[" + DbTable.ProcessCategory + @"] AS PCAT ON PS.ProcessCategoryId=PCAT.Id
                    LEFT OUTER JOIN [" + DbSchema.HKP + @"].[" + DbTable.ProcessCriteria + @"] AS PCRI ON PS.ProcessCriteriaId=PCRI.Id
                    WHERE PS.CompanyId='" + companyId + "' AND PS.EntityId='" + entityId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT PS.Id, PS.CompanyGroupId, PS.CompanyId, PS.EntityId, E.UserName AS Entity
										, PS.ProcessCategoryId, PCAT.UserName AS ProcessCategory
										, PS.ProcessCriteriaId, PCRI.UserName AS ProcessCriteria
										, PS.Code, PS.[Description]
										, PS.RequiredTimeUnit
								FROM [HKP].[ProcessSet] AS PS
								LEFT OUTER JOIN [ORG].[Entity] AS E ON PS.EntityId=E.Id
								LEFT OUTER JOIN [HKP].[ProcessCategory] AS PCAT ON PS.ProcessCategoryId=PCAT.Id
								LEFT OUTER JOIN [HKP].[ProcessCriteria] AS PCRI ON PS.ProcessCriteriaId=PCRI.Id
								WHERE PS.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public GridModel QueryByCompany(GridParameter parameters, string companyId, string entityId)
        {
            try
            {
                parameters.CmdText = @"SELECT PS.Id, PS.CompanyGroupId, PS.CompanyId, PS.EntityId, E.UserName AS Entity
										, PS.ProcessCategoryId, PCAT.UserName AS ProcessCategory
										, PS.ProcessCriteriaId, PCRI.UserName AS ProcessCriteria
										, PS.Code, PS.[Description]
										, PS.RequiredTimeUnit
								FROM [HKP].[ProcessSet] AS PS
								LEFT OUTER JOIN [ORG].[Entity] AS E ON PS.EntityId=E.Id
								LEFT OUTER JOIN [HKP].[ProcessCategory] AS PCAT ON PS.ProcessCategoryId=PCAT.Id
								LEFT OUTER JOIN [HKP].[ProcessCriteria] AS PCRI ON PS.ProcessCriteriaId=PCRI.Id
								WHERE PS.CompanyId='" + companyId + "' AND PS.EntityId='"+ entityId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }


        public GridModel GetProcessSetListByCompany(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT PS.Id, PS.CompanyGroupId, PS.CompanyId, PS.EntityId, E.UserName AS Entity
										, PS.ProcessCategoryId, PCAT.UserName AS ProcessCategory
										, PS.ProcessCriteriaId, PCRI.UserName AS ProcessCriteria
										, PS.Code, PS.[Description]
										, PS.RequiredTimeUnit
								FROM [HKP].[ProcessSet] AS PS
								LEFT OUTER JOIN [ORG].[Entity] AS E ON PS.EntityId=E.Id
								LEFT OUTER JOIN [HKP].[ProcessCategory] AS PCAT ON PS.ProcessCategoryId=PCAT.Id
								LEFT OUTER JOIN [HKP].[ProcessCriteria] AS PCRI ON PS.ProcessCriteriaId=PCRI.Id
								WHERE PS.CompanyId='" + companyId + "'";
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
            return GetAutoNumber(nameof(ProcessSet), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void CheckUnique(ProcessSet entity)
        {
            //CheckUniqueColumn(UniqueColumnName.Entity, entity.EntityId, r => r.EntityId == entity.EntityId && r.Id != entity.Id && r.CompanyId == entity.CompanyId);
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && r.EntityId == entity.EntityId);
        }

        public void InsertGraph(ProcessSet entity, IEnumerable<ProcessSetDetail> processSetDetail)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                base.InsertGraph(entity);
                _processSetDetailService.InsertGraph(entity.Id, processSetDetail);
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

        public void UpdateGraph(ProcessSet entity, IEnumerable<ProcessSetDetail> processSetDetail)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                base.UpdateGraph(entity);
                _processSetDetailService.InsertUpdateOrDeleteGraph(entity.Id, processSetDetail);
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
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "ProcessSet Id"));
                _unitOfWork.BeginTransaction();
                flag = true;
                ProcessSet entity = Find(id);
                _processSetDetailService.DeleteGraph(entity.Id);
                base.DeleteGraph(entity);
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

        public IWorkbook GetProcessSetReport(string companyId, string entityId, string process)
        {
            try
            {
                ReportGeneralVoucher obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IWorkbook workbook = obj.ProcessSet_Report(excelEngine, companyId, entityId, process);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}