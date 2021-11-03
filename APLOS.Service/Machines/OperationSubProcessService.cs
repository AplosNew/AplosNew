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
    public class OperationSubProcessService : Service<OperationSubProcess>, IOperationSubProcessService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public OperationSubProcessService(
            IRepositoryAsync<OperationSubProcess> operationSubProcessRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(operationSubProcessRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrDeleteGraph(string operationId, string operationProcessId
            , IEnumerable<OperationSubProcess> entities, IEnumerable<OperationSubProcess> dbData)
        {
            try
            {
                if (entities != null)
                {
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = GetPK();
                            item.OperationId = operationId;
                            item.OperationProcessId = operationProcessId;
                            InsertGraph(item);
                        }
                    }
                }
                if (dbData != null && dbData.Count() > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbData)
                        {
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbData)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                                base.DeleteGraph(item);
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

        public void DeleteGraph(string operationProcessId, IEnumerable<OperationSubProcess> dbData)
        {
            try
            {
                if (dbData != null)
                {
                    foreach (var item in dbData)
                    {
                        if (item.OperationProcessId == operationProcessId)
                            base.DeleteGraph(item);
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
            return GetAutoNumber(nameof(OperationSubProcess), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
    }
}