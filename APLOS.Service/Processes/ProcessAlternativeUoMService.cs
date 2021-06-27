#region Using

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
    public partial class ProcessAlternativeUoMService : Service<ProcessAlternativeUoM>, IProcessAlternativeUoMService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public ProcessAlternativeUoMService(
            IRepositoryAsync<ProcessAlternativeUoM> processAlternativeUoMRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(processAlternativeUoMRepository, unitOfWork, pkGeneratorService)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetAltUomList(string masterId)
        {
            try
            {
                string _sql = @"SELECT PAU.Id, PAU.ProcessUoMId
                                , PAU.AlternativeUoMId, AUoM.UserName AS AlternativeUoMName
                                , PAU.BaseUoMId, BUoM.UserName AS BaseUoMName
                                , PAU.AlternativeUoMFactor, PAU.BaseUoMFactor
                                FROM SCS.ProcessAlternativeUoM AS PAU
                                LEFT JOIN SCS.UnitOfMeasurement AS BUoM ON PAU.BaseUoMId=BUoM.Id
                                LEFT JOIN SCS.UnitOfMeasurement AS AUoM ON PAU.AlternativeUoMId=AUoM.Id
                                WHERE PAU.ProcessUoMId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void InsertUpdateOrDeleteGraph(string masterId, IEnumerable<ProcessAlternativeUoM> entities)
        {
            try
            {
                var dbList = Query(t => t.ProcessUoMId == masterId).Select().ToList();
                if (entities != null)
                {
                    foreach (var item in entities)
                    {
                        if (item.Id == 0)
                        {
                            item.ProcessUoMId = masterId;
                            InsertGraph(item);
                        }
                        else if (item.Id == 0)
                            UpdateGraph(item);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void DeleteGraph(string masterId)
        {
            var dbList = Query(m => m.ProcessUoMId == masterId).Select();
            if (dbList != null)
            {
                foreach (var item in dbList)
                {
                    base.DeleteGraph(item);
                }
            }
        }
    }
}