#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Machines;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Machines
{
    public class OperationProcessService : Service<OperationProcess>, IOperationProcessService
    {
        #region Constructor

        private readonly IOperationSubProcessService _operationSubProcessService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public OperationProcessService(
            IRepositoryAsync<OperationProcess> operationProcessRepository
            , IOperationSubProcessService operationSubProcessService
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(operationProcessRepository, unitOfWork, pkGeneratorService)
        {
            _operationSubProcessService = operationSubProcessService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        /// <summary>
        /// use i operation
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        public IEnumerable<object> Query(string operationId)
        {
            try
            {
                var dataList = base.Query(t => t.OperationId == operationId)
                    .Include(t => t.Process)
                    .Include(t => t.SubProcesses.Select(a => a.SubProcess)).Select();
                var listData = new List<object>();
                foreach (var item in dataList)
                {
                    var childListData = new List<object>();
                    foreach (var child in item.SubProcesses)
                    {
                        var data = new
                        {
                            child.Id,
                            child.OperationId,
                            child.OperationProcessId,
                            child.ProcessId,
                            child.SubProcessId,
                            child.SubProcess.Sequence,
                            child.SubProcess.Code,
                            SubProcessName = child.SubProcess.UserName
                        };
                        childListData.Add(data);
                    }
                    var row = new
                    {
                        item.Id,
                        item.OperationId,
                        item.ProcessId,
                        item.Process.Sequence,
                        item.Process.Code,
                        item.Process.ShortName,
                        item.Process.StandardName,
                        item.Process.UserName,
                        SubProcesses = childListData
                    };
                    listData.Add(row);
                }
                return listData;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void InsertOrDeleteGraph(string operationId, IEnumerable<OperationProcess> entities)
        {
            try
            {
                var dbList = base.Query(t => t.OperationId == operationId).Select().ToList();
                var subProcessList = _operationSubProcessService.Query(t => t.OperationId == operationId).Select().AsEnumerable();
                if (entities != null)
                {
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = GetPK();
                            item.OperationId = operationId;
                            _operationSubProcessService.InsertOrDeleteGraph(operationId, item.Id, item.SubProcesses, subProcessList);
                            InsertGraph(item);
                        }
                        else
                            _operationSubProcessService.InsertOrDeleteGraph(operationId, item.Id, item.SubProcesses, subProcessList);
                    }
                }
                if (dbList != null)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void DeleteGraph(string operationId)
        {
            try
            {
                var dbList = base.Query(t => t.OperationId == operationId).Select().AsEnumerable();
                var subProcessList = _operationSubProcessService.Query(t => t.OperationId == operationId).Select().AsEnumerable();
                if (dbList != null)
                {
                    foreach (var item in dbList)
                    {
                        base.DeleteGraph(item);
                        _operationSubProcessService.DeleteGraph(item.Id, subProcessList);
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(OperationProcess), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
    }
}