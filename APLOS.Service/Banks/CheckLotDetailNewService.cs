using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Banks;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Library.Service.Banks
{
    public class CheckLotDetailNewService : Service<CheckLotDetail>, ICheckLotDetailNewService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CheckLotDetail> _CheckLotDetailRepository;
        private ICheckLotDetailService _checkLotDetailService;

        public CheckLotDetailNewService(
            IRepositoryAsync<CheckLotDetail> CheckLotDetailRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ICheckLotDetailService checkLotDetailService
            , ISqlRepository sqlRepository
            ) : base(CheckLotDetailRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _CheckLotDetailRepository = CheckLotDetailRepository;
            _checkLotDetailService = checkLotDetailService;
        }

        #endregion Constructor

        private DataTable ExistingDetailList(string bankMaster, string chequeLotId, int fromNo, int toNo)
        {
            var sql = @" SELECT CD.* FROM TRn.CheckLotDetail AS CD
                            INNER JOIN TRN.CheckLot AS C ON CD.CheckLotId=C.Id
                            WHERE C.BankMasterId='" + bankMaster + @"'  AND CD.CheckLotId='" + chequeLotId + @"' 
							AND CD.SequenceNumber NOT IN (select SequenceNumber from TRn.CheckLotDetail where SequenceNumber Between '" + fromNo + "' AND  '" + toNo + "')";
            return _sqlRepository.GetDataTable(sql);        }
        public void DetailUpdate(CheckLot checkLot)
        {
            try
            {
                var loop = (checkLot.ToNo - checkLot.FromNo) + 1;
                var count = checkLot.FromNo;
                int sequence = _CheckLotDetailRepository.Query(r => r.CheckLotId == checkLot.Id).Select().Min(r => r.SequenceNumber);
                int sequencemax = _CheckLotDetailRepository.Query(r => r.CheckLotId == checkLot.Id).Select().Max(r => r.SequenceNumber);
                var previousList = ExistingDetailList(checkLot.BankMasterId, checkLot.Id, sequence, loop);
                for (int i = 0; i < loop; i++)
                {
                    var cheLotDetail = _CheckLotDetailRepository.Query(r => r.CheckLotId == checkLot.Id && r.SequenceNumber== sequence).Select().FirstOrDefault();
                    CheckLotDetail chkDetail = new CheckLotDetail
                    {
                        CheckLotId = checkLot.Id,
                    };
                    if (cheLotDetail != null)
                    {
                        chkDetail.Id = cheLotDetail.Id;
                        chkDetail.CheckNumber = count;
                        chkDetail.IsPrint = cheLotDetail.IsPrint;
                        chkDetail.IsCancel = cheLotDetail.IsCancel;
                        chkDetail.SequenceNumber = cheLotDetail.SequenceNumber;
                        base.UpdateGraph(chkDetail);
                        sequence++;
                    }
                    else
                    {
                        sequencemax++;
                        chkDetail.CheckNumber = count;
                        chkDetail.SequenceNumber = sequence;
                        _checkLotDetailService.IsCheckNumberRowExist(checkLot.BankMasterId, count, checkLot.Id);
                        base.InsertGraph(chkDetail);
                    }
                    count++;
                }
                for (int i = 0; i < previousList.Rows.Count; i++)
                {
                    base.Delete(previousList.Rows[i]["Id"]);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

    }
}