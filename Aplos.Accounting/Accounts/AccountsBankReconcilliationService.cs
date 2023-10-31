using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class AccountsBankReconcilliationService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        public AccountsBankReconcilliationService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"select top 1 BR.Id,BM.AccountTitle BankName,BR.BankStatementNo
                                ,REPLACE(CONVERT(CHAR(11), BR.FromDate, 106),' ','-') FromDate
                                ,REPLACE(CONVERT(CHAR(11),  BR.ToDate, 106),' ','-') ToDate
                                ,BR.OpeningBlance,BR.ClosingBalance
                                from trn.BankReconciliation BR
                                LEFT JOIN [MST].[BankMaster] BM ON BM.Id=BR.BankMasterId
                                WHERE BR.CompanyGroupId='" + companyGroupId + "'AND BR.CompanyId='" + companyId + "' order by BR.AddedDate desc";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetAvailableBankReconciliationUploadedDataList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            parameters.CmdText = @"SELECT BRUD.Id,REPLACE(CONVERT(CHAR(11), BankStatementDate, 106),' ','-') AS  BankStatementDate, BankRefNo, BankParticulars, DrAmount, CrAmount, BRUD.Remarks, OwnRefNo
                                FROM TRN.BankReconciliationUploadedData  BRUD
                                INNER JOIN TRN.BankReconciliationUpload BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                                WHERE BRUD.CompanyGroupId='" + companyGroupId + "'AND BRUD.CompanyId='" + companyId + "' AND BRU.BankMasterId='" + bankMasterId + "' ";
            return _sqlRepository.GetGridData(parameters);
        }
        public void DeleteBankreconciliation(string bankReconciliationId)
        {
            var flag = false;
            try
            {
                flag = true;
                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                
                vendorAdWrsql = @"UPDATE trn.GLTransactionDetail SET ReconcileId=NULL,ReconcileDate=NULL where ReconcileId='" + bankReconciliationId + "' ";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"DELETE FROM trn.BankReconciliation WHERE Id='" + bankReconciliationId + "' ";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                
                flag = false;
                

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
                //if (flag)
                    //_unitOfWork.Rollback();
            }
        }
        public IEnumerable<object> GetBankReconciledList(string companyGroupId, string companyId, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (Convert.ToDateTime(fromDate) < Convert.ToDateTime(cutOffDate))
                    throw new CustomException("From date can not be greater then opening balance date.........!");
                string str = "", str2 = "";
                if (Convert.ToDateTime(cutOffDate).Date == fromDate.Date)
                {
                    str = " AND V.[SourceType]='OpeningBalance' AND V.PostingDate=@fromDate";
                    str2 = " AND V.[SourceType]<>'OpeningBalance' ";
                }
                else
                {
                    str = " AND V.PostingDate<@fromDate";
                    str2 = " ";
                }

                var sql = @"DECLARE @companyGroupId AS VARCHAR(MAX)='" + companyGroupId + @"'
                                   ,@companyId AS VARCHAR(MAX)='" + companyId + @"'
                                   ,@bankMasterId AS VARCHAR(MAX)='" + bankMasterId + @"'
	                               ,@fromDate AS DATE='" + fromDate + @"'
	                               ,@toDate AS DATE='" + toDate + @"'
	                               ,@query AS NVARCHAR(MAX)
	                               ,@glBalance AS DECIMAL(18,4)
	                               ,@col2R1 AS DECIMAL(18,4)
	                               ,@col2R2 AS DECIMAL(18,4)
	                               ,@col2R3 AS DECIMAL(18,4)
	                               ,@col2R4 AS DECIMAL(18,4)
                            SELECT @glBalance=(SELECT COALESCE(SUM(ISNULL(GLT.DrAmount,0)) - SUM(ISNULL(GLT.CrAmount,0)),0)
						                       FROM TRN.VoucherDetail AS VD
						                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
						                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
						                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId=@bankMasterId)-- AND (ReconcileId IS NULL))
									           AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId
                                               AND V.IsPark=0 AND VD.BankMasterId=@bankMasterId  AND V.PostingDate<=@toDate --AND V.PostingDate<@fromDate
						        )
                           SELECT Col, [Value] AS [Before], '' AS [ReconciledValue],'' AS [After] FROM
                           (SELECT @glBalance AS BnkGL,*, CONVERT(DECIMAL(18,4),COALESCE((@glBalance+(BnkOthCr+Payable)-(BnkOthDr+Receiveable)),0)) AS Balance  FROM
                              (SELECT CONVERT(DECIMAL(18,4),COALESCE(SUM(GLT.CrAmount),0)) AS BnkOthCr--[Add : BanK other Credit]
                                     ,CONVERT(DECIMAL(18,4),COALESCE(SUM(GLT.DrAmount),0)) AS BnkOthDr--[Less : BanK other Debit]
                               FROM TRN.VoucherDetail AS VD
                               INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                               INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                               WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId=@bankMasterId AND (ReconcileId IS NULL))
   	   			                         AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.IsPark=0
                                         AND (VD.BankMasterId=@bankMasterId  AND V.PostingDate<=@toDate)  --AND V.PostingDate>=@fromDate
   	                           ) AS T1,

   	                          (SELECT CONVERT(DECIMAL(18,4),COALESCE(SUM(DrAmount),0)) AS Payable--[Add : Instrument issued but not yet presented]
			                         ,CONVERT(DECIMAL(18,4),COALESCE(SUM(CrAmount),0)) AS Receiveable--[Less : Instrument received but not yet realized]
	                           FROM TRN.VoucherDetail WHERE VoucherId IN (
	   	                        SELECT VD.VoucherId
	   	                        FROM TRN.VoucherDetail AS VD
	   	                        INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
	   	                        INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
	   	                        WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId=@bankMasterId AND (ReconcileId <>'')) AND
   	     				                    (V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.IsPark=0
                                             AND VD.BankMasterId=@bankMasterId AND  V.PostingDate<=@toDate)  )
	   				                        AND (BankMasterId IS NULL) 
   	                           ) AS T2
                           ) AS UNP
                           UNPIVOT
                           (
                              Value FOR Col IN (BnkGL,Payable,Receiveable,[BnkOthCr],BnkOthDr,Balance)
                           ) AS unpvt";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetIssuedNotPresentList(GridParameter parameters, string companyGroupId, string companyId, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                string str = "";
                if (Convert.ToDateTime(cutOffDate).Date == fromDate.Date)
                {
                    str = " AND V.[SourceType]<>'OpeningBalance'";
                }
                else
                {
                    str = "";
                }
                parameters.CmdText = @"SELECT V.Id AS VoucherId
	                                      ,VD.Id AS VoucherDetailId
	                                      ,V.VoucherNo
	                                      ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                                          ,V.PostingDate
	                                      ,VD.CrAmount AS Amount
	                                      ,BR.Id AS ReconcileNo
	                                      ,BR.BankStatementNo
	                                      ,REPLACE(CONVERT(CHAR(11), CIRD.ReconcileDate, 106),' ','-') AS ReconcileDate
                                  FROM TRN.VoucherDetail AS VD
                                  INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                  INNER JOIN TRN.GLTransactionDetail AS CIRD ON VD.Id=CIRD.Id
                                  INNER JOIN TRN.BankReconciliation AS BR ON BR.Id=CIRD.ReconcileId
                                  WHERE VoucherId IN (SELECT VD.VoucherId FROM TRN.VoucherDetail AS VD
					                                  INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
					                                  INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
					                                  WHERE V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + @"' AND V.IsPark=0 AND
					                                      VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' AND ReconcileId<>'' AND CrAmount<>0
														  )
                                  AND (VD.BankMasterId='" + bankMasterId + "' AND  V.PostingDate<='" + toDate + @"') ) ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetReceivedNotPresentList(GridParameter parameters, string companyGroupId, string companyId, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                string str = "";
                if (Convert.ToDateTime(cutOffDate).Date == fromDate.Date)
                {
                    str = " AND V.[SourceType]<>'OpeningBalance'";
                }
                else
                {
                    str = "";
                }
                parameters.CmdText = @"SELECT V.Id AS VoucherId
	                                      ,VD.Id AS VoucherDetailId
	                                      ,V.VoucherNo
	                                      ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                      ,V.PostingDate
	                                      ,VD.DrAmount AS Amount
	                                      ,BR.Id AS ReconcileNo
	                                      ,BR.BankStatementNo
	                                      ,REPLACE(CONVERT(CHAR(11), CIRD.ReconcileDate, 106),' ','-') AS ReconcileDate
                                  FROM TRN.VoucherDetail AS VD
                                  INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                  INNER JOIN TRN.GLTransactionDetail AS CIRD ON VD.Id=CIRD.Id
                                  INNER JOIN TRN.BankReconciliation AS BR ON BR.Id=CIRD.ReconcileId
                                  WHERE VoucherId IN (SELECT VD.VoucherId FROM TRN.VoucherDetail AS VD
					                                  INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
					                                  INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
					                                  WHERE V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + @"' AND V.IsPark=0 AND
					                                      VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' AND ReconcileId<>''  AND DrAmount<>0
														  )
                                  AND (VD.BankMasterId='" + bankMasterId + "' AND  V.PostingDate<='" + toDate + @"')) ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetBankCrReconList(GridParameter parameters, string companyGroupId, string companyId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                parameters.CmdText = @"SELECT V.Id AS VoucherId
	                                         ,VD.Id AS VoucherDetailId
                                             ,VD.Id AS VoucherDetail
	                                         ,V.VoucherNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                             ,VD.DocRefNo, VD.PartyType, VD.Narration
	                                         ,GLT.CrAmount AS Amount --[Add : BanK other Credit]
	                                         ,'' AS CheckNo
	                                         ,'' EncashmentDate
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' AND (ReconcileId IS NULL))
                                       AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'  AND V.IsPark=0
                                       AND (VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) --AND V.PostingDate>='" + fromDate + @"'
                                       AND (VD.CrAmount<>0.0000) ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBankCrReconListSyncfusion( string companyGroupId, string companyId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var sql = @"SELECT V.Id AS VoucherId
	                                         ,VD.Id AS VoucherDetailId
                                             ,VD.Id AS VoucherDetail
	                                         ,V.VoucherNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                             ,VD.DocRefNo, VD.PartyType, VD.Narration
	                                         ,GLT.CrAmount AS Amount --[Add : BanK other Credit]
	                                         ,'' AS CheckNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS EncashmentDate
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' AND (ReconcileId IS NULL))
                                       AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'  AND V.IsPark=0
                                       AND (VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) --AND V.PostingDate>='" + fromDate + @"'
                                       AND (VD.CrAmount<>0.0000) ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBankDrReconListSyncfusion(string companyGroupId, string companyId, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                string str = "";
                if (Convert.ToDateTime(cutOffDate).Date == fromDate.Date)
                {
                    str = " AND V.[SourceType]<>'OpeningBalance' ";
                }
                else
                {
                    str = " ";
                }
                var sql = @"SELECT V.Id AS VoucherId
	                                         ,VD.Id AS VoucherDetailId
                                             ,VD.Id AS VoucherDetail
	                                         ,V.VoucherNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                             ,VD.DocRefNo, VD.PartyType, VD.Narration
	                                         ,GLT.DrAmount AS Amount --[Add : BanK other Credit]
	                                         ,'' AS CheckNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS EncashmentDate 
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' AND (ReconcileId IS NULL))
                                       AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.IsPark=0
                                       AND (VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) --AND V.PostingDate>='" + fromDate + @"'
                                       AND (VD.DrAmount<>0.0000)" + str;
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetBankDrReconListUploadedData(string companyGroupId, string companyId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var sql = @"SELECT V.Id AS VoucherId
	                                         ,VD.Id AS VoucherDetailId
                                             ,VD.Id AS VoucherDetail
                                             ,GLT.Id AS GLTransactionDetailId
	                                         ,V.VoucherNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                             ,VD.DocRefNo, VD.PartyType, VD.Narration
	                                         ,GLT.DrAmount AS Amount --[Add : BanK other Credit]
	                                         ,'' AS CheckNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS EncashmentDate 
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' )
                                       AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.IsPark=0
                                       AND VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')
                                       AND VD.DrAmount<>0.0000 AND V.Archive=0
                                       AND VD.Id NOT IN(select VoucherDetailId from TRN.BankReconciliationMap) "; 
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetBankCrReconListUploadedData(string companyGroupId, string companyId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var sql = @"SELECT V.Id AS VoucherId
	                                         ,VD.Id AS VoucherDetailId
                                             ,VD.Id AS VoucherDetail
                                             ,GLT.Id AS GLTransactionDetailId
	                                         ,V.VoucherNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                             ,VD.DocRefNo, VD.PartyType, VD.Narration
	                                         ,GLT.CrAmount AS Amount --[Add : BanK other Credit]
	                                         ,'' AS CheckNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS EncashmentDate 
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' )
                                       AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.IsPark=0
                                       AND VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')
                                       AND VD.CrAmount<>0.0000 AND V.Archive=0
                                       AND VD.Id NOT IN(select VoucherDetailId from TRN.BankReconciliationMap) ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetBankDrReconList(GridParameter parameters, string companyGroupId, string companyId, DateTime cutOffDate, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                string str = "";
                if (Convert.ToDateTime(cutOffDate).Date == fromDate.Date)
                {
                    str = " AND V.[SourceType]<>'OpeningBalance' ";
                }
                else
                {
                    str = " ";
                }
                parameters.CmdText = @"SELECT V.Id AS VoucherId
	                                         ,VD.Id AS VoucherDetailId
                                             ,VD.Id AS VoucherDetail
	                                         ,V.VoucherNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                             ,VD.DocRefNo, VD.PartyType, VD.Narration
	                                         ,GLT.DrAmount AS Amount --[Add : BanK other Credit]
	                                         ,'' AS CheckNo
	                                         ,'' EncashmentDate 
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' AND (ReconcileId IS NULL))
                                       AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.IsPark=0
                                       AND (VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) --AND V.PostingDate>='" + fromDate + @"'
                                       AND (VD.DrAmount<>0.0000)" + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public Dictionary<string, object> GetBankReconLastDate(string companyGroupId, string companyId, string bankMasterId)
        {
            return _sqlRepository.GetData(@"SELECT TOP(1) REPLACE(CONVERT(CHAR(11), ToDate, 106),' ','-') AS FromDate, OpeningBlance, ClosingBalance
						   FROM [TRN].[BankReconciliation] where BankMasterId='" + bankMasterId + "' AND CompanyGroupId='" + companyGroupId + "' AND CompanyId='" + companyId + @"'
                            ORDER BY ToDate DESC");
        }
        public Dictionary<string, object> GetBankReconUploadLastDate(string companyGroupId, string companyId, string bankMasterId)
        {
            return _sqlRepository.GetData(@"SELECT TOP(1) REPLACE(CONVERT(CHAR(11), dateadd(DAY,1,ToDate), 106),' ','-') AS FromDate, OpeningBlance, ClosingBalance
						   FROM [TRN].[BankReconciliationUpload] where BankMasterId='" + bankMasterId + "' AND CompanyGroupId='" + companyGroupId + "' AND CompanyId='" + companyId + @"'
                            ORDER BY ToDate DESC");
        }
        public Dictionary<string, object> GetBankReconDrCrTotalAmount(string companyGroupId, string companyId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            return _sqlRepository.GetData(@"select sum(x.DrAmount) bankDrAmmount,sum(x.CrAmount) bankCrAmmount from (
                SELECT SUM(GLT.DrAmount) AS DrAmount ,0 CrAmount 
                		FROM TRN.VoucherDetail AS VD
                        INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                        INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                        WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' AND (ReconcileId IS NULL))
                        AND V.CompanyGroupId='"+ companyGroupId + "' AND V.CompanyId='"+ companyId + @"' AND V.IsPark=0
                        AND (VD.BankMasterId= '"+bankMasterId+"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) 
                        AND (VD.DrAmount<>0.0000) AND V.[SourceType]<>'OpeningBalance' 
                union all
                SELECT  0 DrAmount,sum(GLT.CrAmount) AS CrAmount 
                                FROM TRN.VoucherDetail AS VD
                                INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' AND (ReconcileId IS NULL))
                                AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + @"' AND V.IsPark=0
                                AND (VD.BankMasterId= '" + bankMasterId + "'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"'))
                                AND (VD.CrAmount<>0.0000) 
                				) x");
        }
        public IEnumerable<object> GetBankReconciliationUploadedData(string companyGroupId, string companyId, string plantId, string bankMasterId)
        {
            try
            {
                var sql = @"SELECT BRU.Id,B.UserName  BankName,OpeningBlance, ClosingBalance, BankStatementNo, BRU.Remarks,EI.EmployeeName
                            ,REPLACE(CONVERT(CHAR(11), BRU.FromDate, 106),' ','-') AS FromDate
                            ,REPLACE(CONVERT(CHAR(11), BRU.ToDate, 106),' ','-') AS ToDate
                            FROM TRN.BankReconciliationUpload BRU
                            INNER JOIN [MST].[BankMaster] BM ON BM.Id=BRU.BankMasterId
                            INNER JOIN [HKP].[Bank] B ON B.Id=BM.BankId
                            INNER JOIN [dbo].[EmployeeInformation] EI ON EI.SystemId=BRU.EmployeeId
                            WHERE BRU.CompanyGroupId='" + companyGroupId + @"' AND BRU.CompanyId='" + companyId + @"'  AND BRU.PlantId='" + plantId + @"'
                            AND BRU.BankMasterId='" + bankMasterId + @"'  ORDER BY BRU.AddedDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public void DeleteBankReconciliationUploadedData(string bankReconciliationUploadId)
        {
            var flag = false;
            try
            {
                flag = true;
                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"DELETE FROM TRN.BankReconciliationUploadedData where BankReconciliationUploadId='" + bankReconciliationUploadId + "' ";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"DELETE FROM TRN.BankReconciliationUpload WHERE Id='" + bankReconciliationUploadId + "' ";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());

                flag = false;


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
                //if (flag)
                //_unitOfWork.Rollback();
            }
        }
        public void DeleteBankReconciliationMapData(string voucherDetailId)
        {
            var flag = false;
            try
            {
                flag = true;
                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"DELETE FROM TRN.BankReconciliationMap where VoucherDetailId='" + voucherDetailId + "' ";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());

                flag = false;


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
                //if (flag)
                //_unitOfWork.Rollback();
            }
        }
        public IEnumerable<object> GetAvailableBankReconciliationUploadedDrDataList(string companyGroupId, string companyId, string plantId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                
                var sql = @"SELECT BRUD.Id,REPLACE(CONVERT(CHAR(11), BankStatementDate, 106),' ','-') AS  BankStatementDate, BankRefNo, BankParticulars, DrAmount CrAmount, BRUD.Remarks, OwnRefNo
                                FROM TRN.BankReconciliationUploadedData  BRUD
                                INNER JOIN TRN.BankReconciliationUpload BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                                WHERE BRUD.CompanyGroupId='" + companyGroupId + "' AND BRUD.CompanyId='" + companyId + "' AND BRUD.PlantId='" + plantId + "'  AND BRU.BankMasterId='" + bankMasterId + @"' 
                                AND BankStatementDate BETWEEN CONVERT(DATE,'" + fromDate + "') AND CONVERT(DATE,'" + toDate + @"') AND DrAmount>0 
                                AND BRUD.Id NOT IN(select BankReconciliationUploadedDataId from TRN.BankReconciliationMap)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetBankDrReconciledList(string companyGroupId, string companyId, string plantId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {

                var sql = @"SELECT BRM.Id BankReconciliationMapId,BRM.VoucherDetailId,BRM.BankReconciliationUploadedDataId
								,REPLACE(CONVERT(CHAR(11), BankStatementDate, 106),' ','-') AS  BankStatementDate, BankRefNo, BankParticulars, BRUD.CrAmount UploadedAmount
								,V.VoucherNo,VD.DocRefNo,CASE WHEN VD.DrAmount>0  THEN VD.DrAmount ELSE VD.CrAmount END VoucherAmount
                                FROM TRN.BankReconciliationMap BRM
								INNER JOIN TRN.BankReconciliationUploadedData  BRUD ON BRUD.Id=BRM.BankReconciliationUploadedDataId
                                INNER JOIN TRN.BankReconciliationUpload BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id=BRM.VoucherDetailId
								INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                WHERE V.Archive=0 AND BRUD.CompanyGroupId='" + companyGroupId + "' AND BRUD.CompanyId='" + companyId + "' AND BRUD.PlantId='" + plantId + "'  AND BRU.BankMasterId='" + bankMasterId + @"' 
                                AND BankStatementDate BETWEEN CONVERT(DATE,'" + fromDate + "') AND CONVERT(DATE,'" + toDate + @"') AND BRUD.CrAmount>0  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetAvailableBankReconciliationUploadedCrDataList(string companyGroupId, string companyId, string plantId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {

                var sql = @"SELECT BRUD.Id,REPLACE(CONVERT(CHAR(11), BankStatementDate, 106),' ','-') AS  BankStatementDate, BankRefNo, BankParticulars, CrAmount DrAmount, BRUD.Remarks, OwnRefNo
                                FROM TRN.BankReconciliationUploadedData  BRUD
                                INNER JOIN TRN.BankReconciliationUpload BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                                WHERE BRUD.CompanyGroupId='" + companyGroupId + "' AND BRUD.CompanyId='" + companyId + "' AND BRUD.PlantId='" + plantId + "'  AND BRU.BankMasterId='" + bankMasterId + @"' 
                                AND BankStatementDate BETWEEN CONVERT(DATE,'" + fromDate + "') AND CONVERT(DATE,'" + toDate + @"') AND CrAmount>0 
                                AND BRUD.Id NOT IN(select BankReconciliationUploadedDataId from TRN.BankReconciliationMap)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetBankCrReconciledList(string companyGroupId, string companyId, string plantId, string bankMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {

                var sql = @"SELECT BRM.Id BankReconciliationMapId,BRM.VoucherDetailId,BRM.BankReconciliationUploadedDataId
								,REPLACE(CONVERT(CHAR(11), BankStatementDate, 106),' ','-') AS  BankStatementDate, BankRefNo, BankParticulars, BRUD.DrAmount UploadedAmount
								,V.VoucherNo,VD.DocRefNo,CASE WHEN VD.CrAmount>0  THEN VD.CrAmount ELSE VD.DrAmount END VoucherAmount
                                FROM TRN.BankReconciliationMap BRM
								INNER JOIN TRN.BankReconciliationUploadedData  BRUD ON BRUD.Id=BRM.BankReconciliationUploadedDataId
                                INNER JOIN TRN.BankReconciliationUpload BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id=BRM.VoucherDetailId
								INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                WHERE V.Archive=0 AND BRUD.CompanyGroupId='" + companyGroupId + "' AND BRUD.CompanyId='" + companyId + "' AND BRUD.PlantId='" + plantId + "'  AND BRU.BankMasterId='" + bankMasterId + @"' 
                                AND BankStatementDate BETWEEN CONVERT(DATE,'" + fromDate + "') AND CONVERT(DATE,'" + toDate + @"') AND BRUD.DrAmount>0  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        #region Bank Reconciliation Closing
        public List<Dictionary<string, object>> GetBankReconciliationClosingList(string column, string value, string companyGroupId, string companyId, string plantId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (SELECT  BRC.*,B.UserName AS BankName, FY.FiscalYearName
                        FROM [TRN].[BankReconciliationClosing] BRC
						INNER JOIN [SCS].[FiscalYear] FY ON FY.Id=BRC.FiscalYearId						
						INNER JOIN [MST].[BankMaster] BM ON BM.Id=BRC.BankMasterId
						LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                        WHERE BRC.CompanyGroupId='" + companyGroupId + "' AND BRC.CompanyId='" + companyId + "' AND BRC.PlantId='" + plantId + @"'
                ) AS TEMP WHERE " + strkey + " order by AddedDate DESC   ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public void SaveBankReconciliationClosingData(Dictionary<string, object> data, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string _Id = string.Empty;
            
            try
            {
                bplib.clsGenID genid = new bplib.clsGenID();

                string sql = "SELECT * FROM [TRN].[BankReconciliationClosing] WHERE Id='" + data["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BankReconciliationClosing", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        #endregion

        public IEnumerable<object> GetBankUploadInfoById(string id)
        {
            try
            {
                var sql = @"SELECT BRU.BankMasterId,BM.AccountTitle,BRUD.CrAmount,BRUD.CrAmount BankAmount,BRUD.BankRefNo,BRUD.BankRefNo DocRefNo,BRUD.BankStatementDate
                            ,BRUD.BankStatementDate PostingDate,BRUD.BankStatementDate DocDate ,BM.GLGeneralInfoId,BM.BudgetMasterId,BM.ActivityId,BM.CurrencyId,BRUD.Id BankReconciliationUploadedDataId
                            FROM  [TRN].[BankReconciliationUploadedData] BRUD 
                            JOIN [TRN].[BankReconciliationUpload] BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                            LEFT JOIN MST.BankMaster BM ON BM.Id=BRU.BankMasterId
                            WHERE BRUD.Id='" + id + "'  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetBankDrUploadInfoById(string id)
        {
            try
            {
                var sql = @"SELECT BRU.BankMasterId,BM.AccountTitle,BRUD.DrAmount,BRUD.DrAmount BankAmount,BRUD.BankRefNo,BRUD.BankRefNo DocRefNo,BRUD.BankStatementDate
                            ,BRUD.BankStatementDate PostingDate,BRUD.BankStatementDate DocDate ,BM.GLGeneralInfoId,BM.BudgetMasterId,BM.ActivityId,BM.CurrencyId,BRUD.Id BankReconciliationUploadedDataId
                            FROM  [TRN].[BankReconciliationUploadedData] BRUD 
                            JOIN [TRN].[BankReconciliationUpload] BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                            LEFT JOIN MST.BankMaster BM ON BM.Id=BRU.BankMasterId
                            WHERE BRUD.Id='" + id + "'  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public string InsertExpenseToBankReconcil(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                if (voucherVM.BankMasterId == voucherVM.OtherBankMasterId)
                    throw new CustomException("Same to same bank transfer is not allowed!");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");
                DataSet _bankJournalData = null;
                DataSet _bankJournalDetailData = null;
                DataSet _bankReconciliationMapData = null;
                DataSet _glTransactionDetailData = null;
                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                var voucherDrId = "";
                voucherVM.SourceType = SourceType.BankJournal.ToString();

                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.BankJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);
                var bankJournal = new BankJournal
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    BankMasterId = voucherVM.BankMasterId,
                    IsPark = voucherVM.IsPark,
                    SourceType = voucherVM.SourceType,
                    PaymentSource = voucherVM.PaymentSource,
                    BankJournalType = BankJournalType.BankToGL.ToString(),
                    Amount = voucherVM.Amount,
                    IsReverse = true,
                    VoucherId = voucher.Id,
                    Archive = false
                };
                _accountsCommonService.InsertBankJournal(bankJournal, ref _bankJournalData);
                // INSERT INTO BankJournalDetail
                var bankJournalDetail = new BankJournalDetail
                {
                    Amount = bankJournal.Amount,
                    BankJournalId = bankJournal.Id,
                    BankMasterId = voucherVM.BankMasterId
                };
                _accountsCommonService.InsertBankJournalDetail(bankJournal, bankJournalDetail, 1, ref _bankJournalDetailData);
                var currentVoucherDetailId = 1;
                var currentBankJournalDetailId = 0;
                var bankMaster = _accountsCommonService.GetBankMaster(bankJournal.BankMasterId);
                var currentVoucherDetaiRecord = 0;
                var voucherDetail =  new VoucherDetail
                {
                    GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString(),
                    BudgetMasterId = bankMaster["BudgetMasterId"].ToString(),
                    ActivityId = bankMaster["ActivityId"].ToString(),
                    BankMasterId = bankJournal.BankMasterId,
                    CrAmount =  bankJournal.Amount,
                    PaymentSource = bankJournal.PaymentSource,
                    PartyType = bankJournal.PaymentSource,
                    TrnNature = TransactionNature.Bank.ToString()
                };
                currentVoucherDetaiRecord++;
                _accountsCommonService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetaiRecord, ref _crvDetailData);
                var totalAmountDr = voucherDetail.DrAmount;
                var totalAmountCr = voucherDetail.CrAmount;
                if (bankJournal.BankJournalType == BankJournalType.BankToGL.ToString())
                {
                    voucherDetail.DrAmount = 0;
                    voucherDetail.CrAmount = bankJournal.Amount;
                }

                var glTransactionDetailCr = new GLTransactionDetail
                {
                    SourceType = voucherDetail.PaymentSource,
                    BankMasterId = voucherDetail.BankMasterId,
                };

                if (bankMaster["CurrencyId"].ToString() == voucher.CurrencyId)
                {
                    glTransactionDetailCr.CrAmount = voucherDetail.CrAmount;
                }
                else
                {
                    glTransactionDetailCr.CrAmount = Math.Round(voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate, 2);
                }
                _accountsCommonService.InsertGLTransactionDetail(voucherDetail,glTransactionDetailCr, out _glTransactionDetailData);

                _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = companyCurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = 1,
                    CrAmount = voucherDetail.CrAmount
                }, ref _crvDetailCurrencyData);
                voucherDrId = voucherDetail.Id;


                if (bankJournal.BankJournalType == BankJournalType.BankToGL.ToString())
                {
                    if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                        throw new CustomException("Expense GL list not found!");

                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        // INSERT INTO BankJournalDetail
                        //currentBankJournalDetailId++;
                        //var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                        //{
                        //    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        //    BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        //    ActivityId = voucherDetailVM.ActivityId,
                        //    Amount = voucherDetailVM.Amount
                        //}, currentBankJournalDetailId);

                        var voucherDetailDr =  new VoucherDetail
                        {
                            BankJournalDetailId = bankJournalDetail.Id,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = bankJournalDetail.Amount,
                            PaymentSource = bankJournal.PaymentSource,
                            PartyType = bankJournal.PaymentSource,
                            Narration = voucherVM.Narration,
                            TrnNature = TransactionNature.ToExpense.ToString()
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        // INSERT INTO VoucherDetailCurrency
                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            DrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount, 2)
                        }, ref _drvDetailCurrencyData);

                        totalAmountDr += voucherDetailDr.DrAmount;
                    }
                }

                if (voucherVM.BankReconciliationUploadedDataId != null)
                {
                    var bankReconciliationMap = new BankReconciliationMap
                    {
                        BankReconciliationUploadedDataId = voucherVM.BankReconciliationUploadedDataId,
                        VoucherDetailId = voucherDetail.Id,
                        GLTransactionDetailId = voucherDetail.Id,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _accountsCommonService.InsertBankReconciliationMap(bankReconciliationMap, ref _bankReconciliationMapData); 
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _bankJournalData,_bankJournalDetailData, _crvDetailData, _glTransactionDetailData, _crvDetailCurrencyData, _drvDetailData, _drvDetailCurrencyData, _bankReconciliationMapData);

                return voucher.VoucherNo;
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
