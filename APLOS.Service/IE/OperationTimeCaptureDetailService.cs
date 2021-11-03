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
using Library.Service.Machines;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.IEnumerable
{
    public class OperationTimeCaptureDetailService : Service<OperationTimeCaptureDetail>, IOperationTimeCaptureDetailService
    {
        #region Constructor

        private readonly IRepositoryAsync<OperationTimeCaptureDetail> _detailRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public OperationTimeCaptureDetailService(
            IRepositoryAsync<OperationTimeCaptureDetail> detailRepository,
            IUnitOfWork unitOfWork,
            IOperationService operationService,
            IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            ) : base(detailRepository, unitOfWork, pkGeneratorService)
        {
            _detailRepository = detailRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public int GetVasVersion(string operationId)
        {
            var _sql = @"SELECT ISNULL(SUM(x.V),0)+1 FROM(
                        SELECT MAX(VASVersion) V,m.OperationId FROM TRN.OperationTimeCaptureDetail AS CH
                        LEFT JOIN (SELECT * FROM TRN.OperationTimeCaptureMaster ) AS M ON CH.OperationTimeCaptureMasterId=M.Id GROUP BY M.OperationId)
                        AS x WHERE OperationId='" + operationId + "'";
            return _detailRepository.SqlQuery<int>(_sql).First();
        }

        public IEnumerable<object> GetAllVersion(string operationId)
        {
            var _sql = @"SELECT DISTINCT CH.[VASVersion],CH.OperationTimeCaptureMasterId
	                     ,M.Id, M.EmpCode, M.EmpName, M.Line, M.Unit, M.FileName, M.OperationId, M.CompanyGroupId, M.OperationVideoUploadId, M.FileExtension, M.Active
                       FROM TRN.OperationTimeCaptureDetail AS CH
                       INNER JOIN TRN.OperationTimeCaptureMaster AS M ON CH.OperationTimeCaptureMasterId=M.Id
                       WHERE M.OperationId='" + operationId + "' ORDER BY CH.VASVersion DESC";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM {DbSchema.Transaction}.[{DbTable.OperationElement}] WHERE Archive=0";
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

        public IEnumerable<object> GetOperationTimeCaptureDetailList()
        {
            try
            {
                return from m in base.Query(m => !m.Archive).Select().OrderBy(r => r.Cycle)
                       select new { Text = m.StartTime, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public void InsertOrUpdateGraph(string masterId, IEnumerable<OperationTimeCaptureDetail> from_ui, out List<OperationTimeCaptureDetail> from_db)
        {
            try
            {
                from_db = null;
                var count = _detailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[OperationTimeCaptureDetail] WHERE OperationTimeCaptureMasterId='{masterId}'").First();

                //from_db = GetOperationTimeCaptureDetailList(MasterId).ToList<OperationTimeCaptureDetail>();
                if (from_db == null)
                    from_db = new List<OperationTimeCaptureDetail>();

                foreach (var ui in from_ui)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db == null)//new
                    {
                        count++;
                        db = new OperationTimeCaptureDetail
                        {
                            Id = MakePK(masterId, count, 2),
                            ModelState = ModelState.Added,

                            Active = ui.Active,
                            Archive = false,
                            Cycle = ui.Cycle,
                            EndTime = ui.EndTime,
                            OperationTimeCaptureMasterId = masterId,
                            Sequence = ui.Sequence,
                            StartTime = ui.StartTime,
                            StepId = ui.StepId,
                            ThirdPartyCodeId = ui.ThirdPartyCodeId,
                            UserDefinedStepCode = ui.UserDefinedStepCode,
                            VASVersion = ui.VASVersion
                        };
                        Insert(db);
                    }
                    else
                    {
                        db.ModelState = ModelState.Modified;

                        db.Active = ui.Active;
                        db.Archive = false;
                        db.Cycle = ui.Cycle;
                        db.EndTime = ui.EndTime;
                        db.OperationTimeCaptureMasterId = masterId;
                        db.Sequence = ui.Sequence;
                        db.StartTime = ui.StartTime;
                        db.StepId = ui.StepId;
                        db.ThirdPartyCodeId = ui.ThirdPartyCodeId;
                        db.UserDefinedStepCode = ui.UserDefinedStepCode;
                        Update(db);
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}