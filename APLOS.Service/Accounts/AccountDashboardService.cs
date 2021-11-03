using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Service.Accounts
{
    public class AccountDashboardService : IAccountDashboardService
    {
        private readonly ISqlRepository _sqlRepository;

        public AccountDashboardService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        #region AccountReceivable

        public IEnumerable<object> OverAllReceivableWithPartyCurrency(string companyId, string partyId, string currencyId)
        {
            try
            {
                string str = "";
                if (string.IsNullOrEmpty(partyId))
                    str = "";
                else
                    str = " AND vd.PartyId = '" + partyId + @"' ";
                string strC = "";
                if (string.IsNullOrEmpty(currencyId))
                    strC = "and VDC.ParallelCurrencyId = Com.BaseCurrencyId ";
                else
                    strC = "  and VDC.ParallelCurrencyId = '" + currencyId + @"'  ";

                var sql = @" SELECT VDC.CrAmount,VDC.DrAmount Balance,vd.DocRefNo,com.Id comId
           ,vd.GLGeneralInfoId,vd.PartyId,V.SourceType
          ,c.Code,v.VoucherNo,vdc.ParallelCurrencyId,IVD.Amount,IVD.TaxAmount,IVD.NetAmount,IVD.WrittenOffAmount,
		  IV.BaseOnDueDate,IV.BaseNoOfDays, REPLACE(CONVERT(VARCHAR(11),(IV.BaseNoOfDays+IV.BaseOnDueDate),106),'','_') CMaturedDate
                                        FROM  [TRN].[VoucherDetailCurrency] AS VDC
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
										LEFT OUTER JOIN ORG.[Company] AS Com ON VDC.ParallelCurrencyId = Com.BaseCurrencyId

          left join [TRN].InvoiceDetail AS IVD ON IVD.Id=VD.InvoiceDetailId
          left join [TRN].Invoice AS IV ON IV.Id=IVD.InvoiceId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
          left join HKP.GLGeneralInfo As glgi on glgi.Id=VD.GLGeneralInfoId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
          where
		            v.SourceType='customerinvoice'
					AND vd.PartyId IS NOT NULL
                                AND V.CompanyId='" + companyId + @"'
                           " + strC + @" " + str + @" ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #endregion AccountReceivable

        #region AccountPayable

        public IEnumerable<object> OverAllPayableWithPartyCurrency(string companyId, string partyId, string currencyId)
        {
            try
            {
                string pstr = "";
                if (string.IsNullOrEmpty(partyId))
                    pstr = "";
                else
                    pstr = " AND vd.PartyId = '" + partyId + @"' ";
                string pstrC = "";
                if (string.IsNullOrEmpty(currencyId))
                    pstrC = "and VDC.ParallelCurrencyId = Com.BaseCurrencyId ";
                else
                    pstrC = "  and VDC.ParallelCurrencyId = '" + currencyId + @"'  ";

                var sql = @" SELECT VDC.CrAmount Balance,VDC.DrAmount,vd.DocRefNo,com.Id comId
           ,vd.GLGeneralInfoId,vd.PartyId,V.SourceType
          ,c.Code,v.VoucherNo,vdc.ParallelCurrencyId,IVD.Amount,IVD.TaxAmount,IVD.NetAmount,IVD.WrittenOffAmount,
		  IV.BaseOnDueDate,IV.BaseNoOfDays, REPLACE(CONVERT(VARCHAR(11),(IV.BaseNoOfDays+IV.BaseOnDueDate),106),'','_') PMaturedDate
                                        FROM  [TRN].[VoucherDetailCurrency] AS VDC
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
										LEFT OUTER JOIN ORG.[Company] AS Com ON VDC.ParallelCurrencyId = Com.BaseCurrencyId

          left join [TRN].InvoiceDetail AS IVD ON IVD.Id=VD.InvoiceDetailId
          left join [TRN].Invoice AS IV ON IV.Id=IVD.InvoiceId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
          left join HKP.GLGeneralInfo As glgi on glgi.Id=VD.GLGeneralInfoId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
          where
		            v.SourceType='vendorinvoice'
					AND vd.PartyId IS NOT NULL
                                                  AND V.CompanyId='" + companyId + @"'
                           " + pstrC + @" " + pstr + @"  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #endregion AccountPayable

        #region Recivable Modals

        #region OverdueModal

        public IEnumerable<object> OverDueReceivableModal(string companyId, string partyId, string currencyId, string matureDate)
        {
            try
            {
                string str = "";
                if (string.IsNullOrEmpty(partyId))
                    str = "";
                else
                    str = " AND CI.PartyId = '" + partyId + @"' ";
                string strC = "";
                if (string.IsNullOrEmpty(currencyId))
                    strC = "and vdc.ParallelCurrencyId = com.BaseCurrencyId ";
                else
                    strC = "  and vdc.ParallelCurrencyId = '" + currencyId + @"'  ";

                var sql = @"SELECT
                            CID.CustomerInvoiceId ,vdc.DrAmount AS Balance,vdc.ParallelCurrencyId,
                            CI.BaseOnDueDate ,REPLACE(CONVERT(VARCHAR(11), CI.BaseOnDueDate,106),'','_') FBOD
                            ,CI.PartyId,CI.BaseNoOfDays,  REPLACE(CONVERT(VARCHAR(11),(CI.BaseNoOfDays+CI.BaseOnDueDate),106),'','_') CMaturedDate
                            ,ci.DocRefNo DocRefNo
                            ,REPLACE(convert(varchar(11),ci.DocDate,106),'','_') DocDate
                            ,party.UserName Party
                            FROM TRN.VoucherDetailCurrency AS vdc Left outer join
                            TRN.VoucherDetail as vd ON vd.Id=vdc.VoucherDetailId
                            LEFT OUTER JOIN ORG.[Company] AS Com
                            ON VDC.ParallelCurrencyId = com.BaseCurrencyId
                            left join TRN.[CustomerInvoiceDetail] AS CID ON CID.Id=vd.CustomerInvoiceDetailId
                            LEFT OUTER JOIN TRN.[CustomerInvoice] AS CI
                            ON CID.CustomerInvoiceId = CI.Id
							LEFT OUTER JOIN HKP.[Party] AS  party
                            ON CI.PartyId = Party.Id
                            LEFT OUTER JOIN TRN.[Voucher] AS v
                            ON v.Id = VD.VoucherId
                            LEFT OUTER JOIN SCS.[Currency] AS C
                            ON C.Id = CI.CurrencyId
                            LEFT OUTER JOIN HKP.[GLGeneralInfo] AS GLGI
                            ON GLGI.Id = CID.GLGeneralInfoId
                            LEFT OUTER JOIN (SELECT
                            SUM(CRD.Amount) CrAmount,
                            CRD.CustomerInvoiceDetailId
                            FROM TRN.[CustomerInvoiceReceiveDetail] AS CRD
                            INNER JOIN TRN.[CustomerInvoiceReceive] AS CR
                            ON CR.Id = CRD.CustomerInvoiceReceiveId
                            GROUP BY CRD.CustomerInvoiceDetailId) AS Received
                            ON Received.CustomerInvoiceDetailId = CID.Id
                            WHERE
                            CI.Archive = 0
                            AND CID.Amount - ISNULL(received.CrAmount, 0) != 0
                            AND ((ISNULL(CID.Amount, 0) - ISNULL(CID.TaxAmount, 0)) - ISNULL(received.CrAmount, 0)) != 0
                            AND VD.CustomerInvoiceReceiveDetailId IS NULL
                            AND CID.IsPark=0 AND CI.SourceType='CustomerInvoice'
                            AND V.CompanyId='" + companyId + @"'
                           and vdc.ParallelCurrencyId = com.BaseCurrencyId
                         and (CI.BaseNoOfDays+CI.BaseOnDueDate) < '" + matureDate + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #endregion OverdueModal

        #endregion Recivable Modals
    }
}