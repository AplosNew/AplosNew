#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Processes;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Processes
{
    public class ProcessUoMService : Service<ProcessUoM>, IProcessUoMService
    {
        #region Constructor

        private readonly IProcessAlternativeUoMService _processAlternativeUoMService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public ProcessUoMService(
            IRepositoryAsync<ProcessUoM> processUoMRepository
            , IProcessAlternativeUoMService processAlternativeUoMService
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(processUoMRepository, unitOfWork, pkGeneratorService)
        {
            _processAlternativeUoMService = processAlternativeUoMService;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProcessUoM), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private bool CheckUnique(ProcessUoM entity)
        {
            return base.Query(t => t.Id != entity.Id && t.ProcessId == entity.ProcessId && t.CompanyGroupId == entity.CompanyGroupId).Select().Any();
        }

        public void Insert(ProcessUoM entity, IEnumerable<ProcessAlternativeUoM> alternativeUoMList)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                //InsertGraph(entity);
                Insert(entity);
                _processAlternativeUoMService.InsertUpdateOrDeleteGraph(entity.Id, alternativeUoMList);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
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

        public void Update(ProcessUoM entity, IEnumerable<ProcessAlternativeUoM> alternativeUoMList)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                UpdateGraph(entity);
                _processAlternativeUoMService.InsertUpdateOrDeleteGraph(entity.Id, alternativeUoMList);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
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

        public void Delete(string id)
        {
            var flag = false;
            try
            {
                var data = Find(id);
                if (data != null)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _processAlternativeUoMService.DeleteGraph(id);
                    DeleteGraph(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (CustomException)
            {
                throw;
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

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT PU.Id, PU.CompanyGroupId
                                    , PU.ProcessId,P.UserName AS ProcessName, PU.BaseUoMId, BUoM.UserName AS BaseUoMName, PU.CapacityName
                                    , PU.CapacityFirstUoMId, FUoM.UserName AS CapacityFirstUoMName
                                    , PU.CapacitySecondUoMId , SUoM.UserName AS CapacitySecondUoMName
                                    FROM SCS.ProcessUoM AS PU
                                    LEFT JOIN SCS.UnitOfMeasurement AS BUoM ON PU.BaseUoMId=BUoM.Id
                                    LEFT JOIN SCS.UnitOfMeasurement AS FUoM ON PU.CapacityFirstUoMId=FUoM.Id
                                    LEFT JOIN SCS.UnitOfMeasurement AS SUoM ON PU.CapacitySecondUoMId=SUoM.Id
									INNER JOIN HKP.Process AS P ON PU.ProcessId=P.Id
                                    WHERE PU.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public IEnumerable<object> GetUoMCboByProcess(string processId)
        {
            try
            {
                var _sql = @"SELECT UoM.Id AS[Value], 1 AS IsBaseUom, UoM.UserName AS [Text]
                            FROM SCS.ProcessUoM AS PU
                            INNER JOIN SCS.unitofmeasurement AS UoM ON PU.BaseUoMId=UoM.Id
                            WHERE UoM.Archive = 0  AND PU.ProcessId='" + processId + @"'
                            UNION
                            SELECT UoM.Id AS[Value], 0 AS IsBaseUom, UoM.UserName AS [Text]
                            FROM SCS.ProcessAlternativeUoM AS PAU
                            INNER JOIN SCS.unitofmeasurement AS UoM ON PAU.AlternativeUoMId=UoM.Id
                            WHERE UoM.Archive = 0  AND PAU.ProcessUoMId IN (SELECT Id FROM SCS.ProcessUoM WHERE ProcessId='" + processId + "')";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public IEnumerable<object> GetCapacityUoMCboByProcess(string processId)
        {
            try
            {
                return from cp in base.Query(t => t.ProcessId == processId).Select()
                       select new { Text = cp.CapacityName, Value = cp.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }
    }
}