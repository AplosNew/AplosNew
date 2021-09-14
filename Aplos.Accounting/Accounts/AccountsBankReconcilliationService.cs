using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Library.Accounting.Accounts
{
    public class AccountsBankReconcilliationService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsBankReconcilliationService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
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
                                       AND (VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate<='" + toDate + @"') --AND V.PostingDate>='" + fromDate + @"'
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
                                       AND (VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate<='" + toDate + @"') --AND V.PostingDate>='" + fromDate + @"'
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

    }
}
