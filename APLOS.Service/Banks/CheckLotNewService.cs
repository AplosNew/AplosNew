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
using System.Linq;
using System.Reflection;

namespace Library.Service.Banks
{
    public class CheckLotNewService : Service<CheckLot>, ICheckLotNewService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CheckLotDetail> _CheckLotDetailRepository;
        private readonly IRepositoryAsync<CheckLotDetailHistory> _checkLotDetailHistoryRepository;
        private ICheckLotDetailNewService _checkLotDetailNewService;

        public CheckLotNewService(
            IRepositoryAsync<CheckLot> checkLotRepository
            , ICheckLotDetailService checkLotDetailService
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<CheckLotDetail> CheckLotDetailRepository
            , IRepositoryAsync<CheckLotDetailHistory> checkLotDetailHistoryRepository
            , ICheckLotDetailNewService checkLotDetailNewService
            ) : base(checkLotRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _CheckLotDetailRepository = CheckLotDetailRepository;
            _checkLotDetailHistoryRepository = checkLotDetailHistoryRepository;
            _checkLotDetailNewService = checkLotDetailNewService;
        }

        #endregion Constructor

            
        public  void UpdateCheckLot(CheckLot checkLot)
        {
            var flag = false;
            try
            {
                if (!string.IsNullOrEmpty(checkLot.Id))
                {
                    if (checkLot.FromNo > 0 && checkLot.ToNo > 0)
                    {
                        if (checkLot.FromNo > checkLot.ToNo)
                            throw new CustomException("From check number can not be greater then to check number........!");
                        _checkLotDetailNewService.DetailUpdate(checkLot);
                        base.UpdateGraph(checkLot);
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        _unitOfWork.SaveChanges();
                        flag = false;
                        _unitOfWork.Commit();
                    }
                    else
                        throw new CustomException("From or to number can not be null or 0............!");
                }

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<ComboModel> GetCbo(string checkLotId, bool isNonSequential)
        {
            string str = "";
            string _sql = "";
            int checknumber = 0;
            if (isNonSequential)
            {
                _sql = @"SELECT CLD.Id,CLD.CheckNumber FROM TRN.CheckLotDetail AS CLD
                            INNER JOIN TRN.CheckLot AS CL ON CLD.CheckLotId=CL.Id WHERE CL.Id='" + checkLotId + "' AND CLD.IsPrint=0 AND CLD.IsCancel=0";
            }
            else
            {
                checknumber = _checkLotDetailHistoryRepository.SqlQuery<int>(@"IF EXISTS (SELECT top(1) CD.CheckNumber FROM TRN.checklotDetailHistory CDH LEFT JOIN TRN.CheckLotDetail CD ON CD.Id=CDH.CheckLotDetailId WHERE CD.CheckLotId = '" + checkLotId + @"' ORDER BY CheckLotDetailId DESC)
                        SELECT CheckNumber  FROM (  SELECT top(1) CD.CheckNumber FROM TRN.checklotDetailHistory CDH LEFT JOIN TRN.CheckLotDetail CD ON CD.Id=CDH.CheckLotDetailId where CD.CheckLotId='" + checkLotId + @"' ORDER BY CheckLotDetailId DESC ) a
                        GROUP BY CheckNumber ELSE SELECT CheckNumber = 0").First();
                str = "TOP(1)";
                _sql = @"SELECT " + str + @"CLD.Id,CLD.CheckNumber FROM TRN.CheckLotDetail AS CLD
                            INNER JOIN TRN.CheckLot AS CL ON CLD.CheckLotId=CL.Id WHERE CL.Id='" + checkLotId + @"' 
                            AND CLD.IsPrint=0 AND CLD.IsCancel=0 AND CLD.CheckNumber>'" + checknumber + "'";
            }

            return _sqlRepository.GetCombo(_sql, "Id", "CheckNumber");
        }

        public IEnumerable<ComboModel> GetExistingCbo(string checkLotId, bool isNonSequential)
        {
            string str = "";
            string _sql = "";
            int checknumber = 0;
            if (isNonSequential)
            {
               
                _sql = @"SELECT CLD.Id,CLD.CheckNumber FROM TRN.CheckLotDetail AS CLD
                            INNER JOIN TRN.CheckLot AS CL ON CLD.CheckLotId = CL.Id WHERE CL.Id = '" + checkLotId+"' AND CLD.IsPrint = 1 AND CLD.IsCancel = 0";
         

            }
            else
            {
                checknumber = _checkLotDetailHistoryRepository.SqlQuery<int>(@"IF EXISTS (SELECT top(1) CD.CheckNumber FROM TRN.checklotDetailHistory CDH LEFT JOIN TRN.CheckLotDetail CD ON CD.Id=CDH.CheckLotDetailId WHERE CD.CheckLotId = '" + checkLotId + @"' ORDER BY CheckLotDetailId DESC)
                        SELECT CheckNumber  FROM (  SELECT top(1) CD.CheckNumber FROM TRN.checklotDetailHistory CDH LEFT JOIN TRN.CheckLotDetail CD ON CD.Id=CDH.CheckLotDetailId where CD.CheckLotId='" + checkLotId + @"' ORDER BY CheckLotDetailId DESC ) a
                        GROUP BY CheckNumber ELSE SELECT CheckNumber = 0").First();
                str = "TOP(1)";
                _sql = @"SELECT " + str + @"CLD.Id,CLD.CheckNumber FROM TRN.CheckLotDetail AS CLD
                            INNER JOIN TRN.CheckLot AS CL ON CLD.CheckLotId=CL.Id WHERE CL.Id='" + checkLotId + @"' 
                            AND CLD.IsPrint=0 AND CLD.IsCancel=0 AND CLD.CheckNumber>'" + checknumber + "'";
            }

            return _sqlRepository.GetCombo(_sql, "Id", "CheckNumber");
        }


        public IEnumerable<object> GetCbo(string bankMasterId)
        {
            try
            {
                var data = from m in base.Query(t => t.BankMasterId == bankMasterId && t.Active).Select().OrderBy(t => t.LotNumber)
                           select new { Text = m.LotNumber, Value = m.Id, isNonSequential = m.IsNonSequential,fromNo=m.FromNo, toNo=m.ToNo,lotNumber=m.LotNumber
                           };
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCbo1(string checkLotId, bool isNonSequential)
        {
            string str = "";
            string _sql = "";
            int checknumber = 0;
            if (isNonSequential)
            {
                //_sql = @"SELECT CLD.Id,CLD.CheckNumber FROM TRN.CheckLotDetail AS CLD
                //            INNER JOIN TRN.CheckLot AS CL ON CLD.CheckLotId=CL.Id WHERE CL.Id='" + checkLotId + "' AND CLD.IsPrint=0 AND CLD.IsCancel=0";

                _sql = @"select * from trn.CheckLot WHERE Id='" + checkLotId + "' ";
            }
            else
            {
                checknumber = _checkLotDetailHistoryRepository.SqlQuery<int>(@"IF EXISTS (SELECT top(1) CD.CheckNumber FROM TRN.checklotDetailHistory CDH LEFT JOIN TRN.CheckLotDetail CD ON CD.Id=CDH.CheckLotDetailId WHERE CD.CheckLotId = '" + checkLotId + @"' ORDER BY CheckLotDetailId DESC)
                        SELECT CheckNumber  FROM (  SELECT top(1) CD.CheckNumber FROM TRN.checklotDetailHistory CDH LEFT JOIN TRN.CheckLotDetail CD ON CD.Id=CDH.CheckLotDetailId where CD.CheckLotId='" + checkLotId + @"' ORDER BY CheckLotDetailId DESC ) a
                        GROUP BY CheckNumber ELSE SELECT CheckNumber = 0").First();
                str = "TOP(1)";
                _sql = @"SELECT " + str + @"CLD.Id,CLD.CheckNumber FROM TRN.CheckLotDetail AS CLD
                            INNER JOIN TRN.CheckLot AS CL ON CLD.CheckLotId=CL.Id WHERE CL.Id='" + checkLotId + @"' 
                            AND CLD.IsPrint=0 AND CLD.IsCancel=0 AND CLD.CheckNumber>'" + checknumber + "'";
            }

            return _sqlRepository.GetCombo(_sql, "Id", "CheckNumber");
        }

        public IEnumerable<object> GetCbo1(string bankMasterId)
        {
            try
            {
                var data = from m in base.Query(t => t.BankMasterId == bankMasterId && t.Active).Select().OrderBy(t => t.LotNumber)
                           select new { Text = m.LotNumber, Value = m.Id, isNonSequential = m.IsNonSequential };
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #region Non cash Check Print and Update
        public void UpdateGraphAndPrint(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy,string party,string partyBankId,string partyAccount)
        {
            var flag = false;
            try
            {
                if (!string.IsNullOrEmpty(voucherDetailId))
                {
                  
                    var checkLotDetail = _CheckLotDetailRepository.Find(checkLotDetailId);
                    checkLotDetail.IsPrint = true;
                    AuditService.UpdatedLog(checkLotDetail);

                    _CheckLotDetailRepository.Update(checkLotDetail);      //  _checkLotDetailRepository.Insert(checkLot); 
                    var checklotDetailHistory = new CheckLotDetailHistory   // var checklotDetail = new CheckLotDetail
                    {
                        Id = GetAutoNumber(nameof(CheckLotDetailHistory), PKGeneratorEnum.Auto, null, DateTime.Now),
                        CheckLotDetailId = checkLotDetail.Id,
                        VoucherDetailId = voucherDetailId,
                        PrintBy = printBy,
                        CheckDate = checkDate,
                        PrintDate = DateTime.Now.ToString(),
                        ResonForCash = checkLotDetail.ResonForCash,
                        AddedBy = checkLotDetail.UpdatedBy,
                        AddedDate = Convert.ToDateTime(checkLotDetail.UpdatedDate),
                        AddedFromIP = checkLotDetail.AddedFromIP,
                        CheckStatus = CheckLotPrintType.Print.ToString(),
                        PrintStatus = CheckLotPrintStatus.NonCash.ToString(),
                        OtherBeneficiary = party,
                        PartyBankId=partyBankId,
                        PartyAccount=partyAccount
                    };
                    _checkLotDetailHistoryRepository.Insert(checklotDetailHistory);  //_checkLotDetailHistoryRepository.Insert(checklotDetail);
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                    throw new CustomException("Voucher not found");
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }



        //Print cash check Update
        public void UpdateGraphAndPrintCashCheck(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy)
        {
            var flag = false;
            try
            {
                if (!string.IsNullOrEmpty(voucherDetailId))
                {

                    var checkLotDetail = _CheckLotDetailRepository.Find(checkLotDetailId);
                    checkLotDetail.IsPrint = true;
                    AuditService.UpdatedLog(checkLotDetail);

                    _CheckLotDetailRepository.Update(checkLotDetail);      //  _checkLotDetailRepository.Insert(checkLot); 
                    var checklotDetailHistory = new CheckLotDetailHistory   // var checklotDetail = new CheckLotDetail
                    {
                        Id = GetAutoNumber(nameof(CheckLotDetailHistory), PKGeneratorEnum.Auto, null, DateTime.Now),
                        CheckLotDetailId = checkLotDetail.Id,
                        VoucherDetailId = voucherDetailId,
                        PrintBy = printBy,
                        CheckDate = checkDate,
                        PrintDate = DateTime.Now.ToString(),
                        ResonForCash = checkLotDetail.ResonForCash,
                        AddedBy = checkLotDetail.UpdatedBy,
                        AddedDate = Convert.ToDateTime(checkLotDetail.UpdatedDate),
                        AddedFromIP = checkLotDetail.AddedFromIP,
                        CheckStatus = CheckLotPrintType.Print.ToString(),
                        PrintStatus = CheckLotPrintStatus.Cash.ToString()
                    };
                    _checkLotDetailHistoryRepository.Insert(checklotDetailHistory);  //_checkLotDetailHistoryRepository.Insert(checklotDetail);
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                    throw new CustomException("Voucher not found");
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion cash check print




        #region CheckRePrint and Update
        public void UpdateGraphAndRePrint(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy,string party, string partyBankId, string partyAccount)
        {
            var flag = false;
            try
            {
                if (!string.IsNullOrEmpty(voucherDetailId))
                {

                    var checkLotDetail = _CheckLotDetailRepository.Find(checkLotDetailId);
                    checkLotDetail.IsPrint = true;
                    AuditService.UpdatedLog(checkLotDetail);

                    _CheckLotDetailRepository.Update(checkLotDetail);
                    var checklotDetailHistory = new CheckLotDetailHistory
                    {
                        Id = GetAutoNumber(nameof(CheckLotDetailHistory), PKGeneratorEnum.Auto, null, DateTime.Now),
                        CheckLotDetailId = checkLotDetail.Id,
                        VoucherDetailId = voucherDetailId,
                        PrintBy = printBy,
                        CheckDate = checkDate,
                        PrintDate = DateTime.Now.ToString(),
                        ResonForCash = checkLotDetail.ResonForCash,
                        AddedBy = checkLotDetail.UpdatedBy,
                        AddedDate = Convert.ToDateTime(checkLotDetail.UpdatedDate),
                        AddedFromIP = checkLotDetail.AddedFromIP,
                        CheckStatus = CheckLotPrintType.RePrint.ToString(),
                        PrintStatus = CheckLotPrintStatus.NonCash.ToString(),
                        OtherBeneficiary=party,
                        PartyBankId=partyBankId,
                        PartyAccount=partyAccount
                        
                    };
                    _checkLotDetailHistoryRepository.Insert(checklotDetailHistory);
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                    throw new CustomException("Voucher not found");
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion

        #region Cash Check RePrint and Update
        public void UpdateGraphAndCashChequeRePrint(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy)
        {
            var flag = false;
            try
            {
                if (!string.IsNullOrEmpty(voucherDetailId))
                {

                    var checkLotDetail = _CheckLotDetailRepository.Find(checkLotDetailId);
                    checkLotDetail.IsPrint = true;
                    AuditService.UpdatedLog(checkLotDetail);

                    _CheckLotDetailRepository.Update(checkLotDetail);
                    var checklotDetailHistory = new CheckLotDetailHistory
                    {
                        Id = GetAutoNumber(nameof(CheckLotDetailHistory), PKGeneratorEnum.Auto, null, DateTime.Now),
                        CheckLotDetailId = checkLotDetail.Id,
                        VoucherDetailId = voucherDetailId,
                        PrintBy = printBy,
                        CheckDate = checkDate,
                        PrintDate = DateTime.Now.ToString(),
                        ResonForCash = checkLotDetail.ResonForCash,
                        AddedBy = checkLotDetail.UpdatedBy,
                        AddedDate = Convert.ToDateTime(checkLotDetail.UpdatedDate),
                        AddedFromIP = checkLotDetail.AddedFromIP,
                        CheckStatus = CheckLotPrintType.RePrint.ToString(),
                        PrintStatus = CheckLotPrintStatus.Cash.ToString()
                    };
                    _checkLotDetailHistoryRepository.Insert(checklotDetailHistory);
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                    throw new CustomException("Voucher not found");
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion

        #region Check void
        public void UpdateGraphAndCheckVoidPrint(string voucherDetailId, int checkLotDetailId, decimal amount, string checkDate, string printBy)
        {
            var flag = false;
            try
            {
                if (!string.IsNullOrEmpty(voucherDetailId))
                {

                    var checkLotDetail = _CheckLotDetailRepository.Find(checkLotDetailId);
                 
                    checkLotDetail.IsCancel = true;
                    AuditService.UpdatedLog(checkLotDetail);

                    _CheckLotDetailRepository.Update(checkLotDetail);      //  _checkLotDetailRepository.Insert(checkLot); 
                    var checklotDetailHistory = new CheckLotDetailHistory   // var checklotDetail = new CheckLotDetail
                    {
                        Id = GetAutoNumber(nameof(CheckLotDetailHistory), PKGeneratorEnum.Auto, null, DateTime.Now),
                        CheckLotDetailId = checkLotDetail.Id,
                        VoucherDetailId = voucherDetailId,
                        PrintBy = printBy,
                        CheckDate = checkDate,
                        PrintDate = DateTime.Now.ToString(),
                        ResonForCash = checkLotDetail.ResonForCash,
                        AddedBy = checkLotDetail.UpdatedBy,
                        AddedDate = Convert.ToDateTime(checkLotDetail.UpdatedDate),
                        AddedFromIP = checkLotDetail.AddedFromIP,
                        CheckStatus = CheckLotPrintType.Cancle.ToString(),
                        PrintStatus = CheckLotPrintStatus.CheckVoid.ToString()
                    };
                    _checkLotDetailHistoryRepository.Insert(checklotDetailHistory);  //_checkLotDetailHistoryRepository.Insert(checklotDetail);
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                    throw new CustomException("Voucher not found");
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        #endregion check void
    }
}