using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.IEnumerable
{
    public class OperationVideoUploadService : Service<OperationVideoUpload>, IOperationVideoUploadService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public OperationVideoUploadService(
            IRepositoryAsync<OperationVideoUpload> areaRepository,
            IUnitOfWork unitOfWork,
            IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            ) : base(areaRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM {DbSchema.Transaction}.[{DbTable.Operation }] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public override void Insert(OperationVideoUpload operationvideoupload)
        {
            try
            {
                operationvideoupload.Id = "OV" + GetAutoNumber(nameof(OperationVideoUpload), PKGeneratorEnum.Yearly, null, DateTime.Now);
                base.Insert(operationvideoupload);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                operationvideoupload.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        /// <summary>
        /// Query auto sequence number.
        /// </summary>
        /// <returns>decimal</returns>
        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00m;
            }
        }

        public IEnumerable<object> GetOperationVideoUploadList()
        {
            try
            {
                return from m in base.Query(m => !m.Archive).Select().OrderBy(r => r.Sequence)
                       select new { Text = m.FileName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
    }
}