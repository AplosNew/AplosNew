#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class SOPActivityDocumentService : Service<SOPActivityDocument>, ISOPActivityDocumentService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SOPActivityDocumentService(
              IRepositoryAsync<SOPActivityDocument> documentRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            ) : base(documentRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Operation

        public IEnumerable<object> GetDocumentListMain(string sopItemId)
        {
            try
            {
                var sql = @" SELECT K.*, SD.*, ACT.Name AS Activity FROM HKP.SOPActivityDocument K
                                        LEFT OUTER JOIN HKP.SOPActivity ACT ON ACT.Id=K.SOPActivityId
                                        LEFT OUTER JOIN HKP.SOPItem SI ON SI.Id=ACT.SOPItemId
                                        LEFT OUTER JOIN HKP.SOPDocument SD ON SD.Id=K.SOPDocumentId
                                        Where ACT.SOPItemId='" + sopItemId + "' Order By ACT.Name ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetDocumentList(string activityId)
        {
            try
            {
                var sql = @" SELECT K.*, SD.*, ACT.Name AS Activity FROM HKP.SOPActivityDocument K
                                        LEFT OUTER JOIN HKP.SOPActivity ACT ON ACT.Id=K.SOPActivityId
                                        LEFT OUTER JOIN HKP.SOPDocument SD ON SD.Id=K.SOPDocumentId
                                        Where K.SOPActivityId='" + activityId + "' Order By ACT.Name ";
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