#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.External
{
    public class KPIService : Service<KPI>, IKPIService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public KPIService(
              IRepositoryAsync<KPI> kPIRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            ) : base(kPIRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Operation

        public IEnumerable<object> GetKPIList(string activityId)
        {
            try
            {
                var sql = @" SELECT K.*, ACT.Name AS Activity FROM dbo.KPI K
                                        LEFT OUTER JOIN dbo.ActivityEmp ACT ON ACT.Id=K.ActivityId
                                        Where K.ActivityId='" + activityId + "' Order By ACT.Name ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        #endregion Operation
    }
}