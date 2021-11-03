using Library.Core;
using Library.Crosscutting.Security;
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
using System.Threading;

namespace Library.Service.IEnumerable
{
    public class OperationTimeCaptureMasterService : Service<OperationTimeCaptureMaster>, IOperationTimeCaptureMasterService
    {
        #region Constructor

        private readonly IRepositoryAsync<OperationTimeCaptureMaster> _operationtimecaptureRepository;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;
        private readonly IOperationService _operationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public OperationTimeCaptureMasterService(
            IRepositoryAsync<OperationTimeCaptureMaster> operationtimecaptureRepository,
            IUnitOfWork unitOfWork,
            IOperationService operationService,
            IOperationTimeCaptureDetailService operationtimecapturedetailservice,
            IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            ) : base(operationtimecaptureRepository, unitOfWork, pkGeneratorService)
        {
            _operationtimecaptureRepository = operationtimecaptureRepository;
            _operationtimecapturedetailservice = operationtimecapturedetailservice;
            _operationService = operationService;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetSearchData(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                parameters.CmdText = @"SELECT m.Id
                                 , m.EmpCode
                                 , m.EmpName
                                 , m.Line
                                 , m.Unit
                                 , m.FileName
                                 , p.code Operation
                                 , m.OperationId
                                 , m.OperationVideoUploadId
                                 , m.FileExtension
                                 , m.Active
                                 , m.Archive
                                    FROM [TRN].[OperationTimeCaptureMaster] AS m left outer join
                                    " + DbSchema.Masters + ".[" + DbTable.Operation + @"] p  ON p.Id=m.OperationId
                                    WHERE   m.Archive=0 and m.Active=1
                                            and m.Companygroupid='" + identity.CompanyGroupId + "'";

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

        private void SaveMaster(OperationTimeCaptureMaster from_ui, out OperationTimeCaptureMaster from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetOperationTimeCaptureMaster(from_ui.Id);
                if (from_db == null)
                {
                    from_db = new OperationTimeCaptureMaster
                    {
                        ModelState = ModelState.Added,
                        Id = GetPK(),
                        Active = from_ui.Active,
                        Archive = false,
                        CompanyGroupId = identity.CompanyGroupId,
                        EmpCode = from_ui.EmpCode,
                        EmpName = from_ui.EmpName,
                        FileExtension = from_ui.FileExtension,
                        FileName = from_ui.FileName,
                        Line = from_ui.Line,
                        OperationId = from_ui.OperationId,
                        OperationVideoUploadId = from_ui.OperationVideoUploadId,
                        MaterialMasterArticleId = from_ui.MaterialMasterArticleId,
                        NoOfVariant = from_ui.NoOfVariant,
                        FirstVariant = from_ui.FirstVariant,
                        SecondVariant = from_ui.SecondVariant,
                        ThirdVariant = from_ui.ThirdVariant
                    };
                }
                else
                {
                    #region Edit

                    from_db.ModelState = ModelState.Modified;

                    from_db.FileName = from_ui.FileName;//set
                    from_db.Active = from_ui.Active;
                    from_db.Archive = from_ui.Archive;
                    from_db.EmpCode = from_ui.EmpCode;
                    from_db.EmpName = from_ui.EmpName;
                    from_db.FileExtension = from_ui.FileExtension;
                    from_db.FileName = from_ui.FileName;
                    from_db.Line = from_ui.Line;
                    from_db.OperationId = from_ui.OperationId;
                    from_db.OperationVideoUploadId = from_ui.OperationVideoUploadId;
                    from_db.Unit = from_ui.Unit;
                    from_db.MaterialMasterArticleId = from_ui.MaterialMasterArticleId;
                    from_db.NoOfVariant = from_ui.NoOfVariant;
                    from_db.FirstVariant = from_ui.FirstVariant;
                    from_db.SecondVariant = from_ui.SecondVariant;
                    from_db.ThirdVariant = from_ui.ThirdVariant;

                    #endregion Edit
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Insert(OperationTimeCaptureMaster operationtimecapturemaster, IEnumerable<OperationTimeCaptureDetail> operationtimecapturedetailList)
        {
            OperationTimeCaptureMaster localMaster = null;
            List<OperationTimeCaptureDetail> localDetailList = null;
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                SaveMaster(operationtimecapturemaster, out localMaster);
                AuditService.Log(localMaster);
                InsertOrUpdateGraph(localMaster);
                _operationtimecapturedetailservice.InsertOrUpdateGraph(localMaster.Id, operationtimecapturedetailList, out localDetailList);

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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, operationtimecapturemaster.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        // Query auto sequence number.
        public IEnumerable<object> GetMasterData(string masterId)
        {
            try
            {
                var _sql = @"SELECT  A.Id, A.CompanyGroupId
                            , A.OperationId, OP.UserName AS OperationName
                            , A.MaterialMasterArticleId, ART.StandardName AS ArticleName
                            , A.OperationVideoUploadId
                            , A.Line, A.Unit, A.EmpCode, A.EmpName, A.FileName, A.FileExtension
                            , NoOfVariant=CAST(A.NoOfVariant AS INT), A.FirstVariant, A.SecondVariant, A.ThirdVariant
                            , A.Active, A.Archive
                            FROM TRN.OperationTimeCaptureMaster AS A
                            JOIN MST.Operation AS OP ON A.OperationId=OP.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON A.MaterialMasterArticleId=ART.Id
                            WHERE A.Id='" + masterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailList(string MasterId)
        {
            try
            {
                var _sql = @"SELECT   d.Id
                                        , d.StartTime
	                                    , d.EndTime
	                                    , (d.EndTime - d.StartTime) Duration
	                                    , d.Cycle
	                                    , d.UserDefinedStepCode
	                                    , d.OperationTimeCaptureMasterId
	                                    , d.StepId
	                                    , s.UserName Steps
                                        , t.[Description] ThirdPartyCode
	                                    , d.ThirdPartyCodeId,d.VASVersion
                                    FROM [TRN].[OperationTimeCaptureDetail] d
                                    LEFT JOIN [HKP].[OperationElement] s ON s.Id = d.StepId
                                    LEFT JOIN [MST].[ThirdPartyOperation] t ON t.Id = d.ThirdPartyCodeId
                                    Where d.OperationTimeCaptureMasterId='" + MasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(OperationTimeCaptureMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public OperationTimeCaptureMaster GetOperationTimeCaptureMaster(string Id)
        {
            try
            {
                return base.Query(m => !m.Archive && m.Id == Id).Select().FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<OperationTimeCaptureDetail> GetOperationTimeCaptureDetailList2(string MasterId)
        {
            try
            {
                return _operationtimecapturedetailservice.Query(m => !m.Archive && m.OperationTimeCaptureMasterId == MasterId).Select().OrderBy(r => r.Cycle);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<OperationTimeCaptureDetail> GetOperationTimeCaptureDetailList_tested(string MasterId)
        {
            try
            {
                const string _sql = "select * from trn.OperationTimeCaptureDetail";
                return _operationtimecaptureRepository.SqlQuery<OperationTimeCaptureDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetOperationTimeCaptureDetailList()
        {
            try
            {
                const string _sql = "select * from trn.OperationTimeCaptureDetail";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetOperationTimeCaptureMasterList()
        {
            try
            {
                return from m in base.Query(m => !m.Archive).Select().OrderBy(r => r.FileName)
                       select new { Text = m.FileName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IEnumerable<object> GetOperationList()
        {
            try
            {
                return from m in _operationService.Query(m => !m.Archive).Select().OrderBy(r => r.Sequence)
                       select new { Text = m.Code, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }
    }
}