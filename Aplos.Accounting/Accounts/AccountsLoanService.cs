using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Library.Accounting.Accounts
{
    public class AccountsLoanService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsLoanService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        #region Loan
        public GridModel LoanQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, F.FinancingNo, F.TransactionType, F.Id, F.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, F.PartyPlantId, PP.UserName AS PartyPlantName, F.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , F.VoucherId, F.PostingDate, F.DocDate, F.DocRefNo, F.CurrencyId, C.Code AS CurrencyCode, F.Amount, F.IsWrittenOff, F.WrittenOffAmount, F.IsPark, F.IsPosted,Status=case when V.IsPark=1 then 'Parked' else 'Posted' end 
                                FROM [TRN].[Financing] AS F
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=F.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=F.PartyPlantId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=F.EmployeeId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=F.CurrencyId
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=F.VoucherId
                                WHERE F.OpeningBalanceId IS NULL AND F.Archive=0 AND V.Archive=0 AND F.CompanyGroupId='" + companyGroupId + "'AND F.CompanyId='" + companyId + "' AND F.PlantId='" + plantId + "' AND F.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetLoanWriteOffList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AW.FinancingNo,AW.FinancingId,F.DocRefNo LoanNo, V.Id VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, V.PostingDate, V.DocDate, V.DocRefNo, C.Code AS CurrencyCode, VD.Amount,Status=case when V.IsPark=1 then 'Parked' else 'Posted' end
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, V.IsPark, AW.LoanSetOffGroupNo
                                    FROM [TRN].[Voucher] AS V
                                    LEFT JOIN [TRN].[FinancingWriteOff] AS AW ON V.Id=AW.VoucherId
                                    left join trn.Financing F on F.Id=AW.FinancingId
									--LEFT JOIN (SELECT Id,FinancingWriteOffId,SUM(Amount) Amount FROM [TRN].[FinancingDetailWriteOff] Group BY Id,FinancingWriteOffId ) AS IWD ON IWD.FinancingWriteOffId=AW.Id
									LEFT JOIN(SELECT VoucherId,SUM(DrAmount) Amount FROM [TRN].[VoucherDetail] group by VoucherId) AS VD ON VD.VoucherId=V.Id
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                                WHERE  V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "'AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetLoanInterestPayableList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT  LP.VoucherId, V.VoucherNo, LP.Id, P.Code AS PartyCode, P.UserName AS PartyName, LP.PostingDate, LP.DocDate, LP.DocRefNo, C.Code AS CurrencyCode, LP.Amount
                                    , LP.PartyPlantId, PP.UserName AS PartyPlantName, LP.IsPark,[Status]=case when V.IsPark=1 then 'Parked' when LP.IsPark=0 then 'Posted' else '' End,LP.FinancingId FinancingNo,F.DocRefNo LoanNo,V.SourceType
                                    FROM [TRN].[FinancingSubsequentTransaction] AS LP
                                    LEFT JOIN TRN.Financing F ON F.Id=LP.FinancingId
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LP.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=LP.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=LP.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=LP.CurrencyId
                                WHERE V.Archive=0 AND LP.CompanyGroupId='" + companyGroupId + "'AND LP.CompanyId='" + companyId + "' AND LP.PlantId='" + plantId + "' AND LP.SourceType in ('LoanInterestPayable','AdditionalLoanPayable','OtherExpensesPayable','LoanTax')";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetLoanInterestPayableReserveList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT  LP.VoucherId, V.VoucherNo, LP.Id, P.Code AS PartyCode, P.UserName AS PartyName, LP.PostingDate, LP.DocDate, LP.DocRefNo, C.Code AS CurrencyCode, LP.Amount
                                    , LP.PartyPlantId, PP.UserName AS PartyPlantName, LP.IsPark,LP.FinancingId FinancingNo,F.DocRefNo LoanNo,V.SourceType
                                    FROM [TRN].[FinancingSubsequentTransaction] AS LP
                                    LEFT JOIN TRN.Financing F ON F.Id=LP.FinancingId
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LP.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=LP.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=LP.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=LP.CurrencyId
                                WHERE V.Archive=0 AND  LP.CompanyGroupId='" + companyGroupId + "'AND LP.CompanyId='" + companyId + "' AND LP.PlantId='" + plantId + "' AND LP.SourceType in ('LoanInterestPayableReverse')";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetLoanClosedList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew,I.TransactionType
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND V.Archive=0 AND I.IsPark=0 AND I.IsWrittenOff=1 AND I.OpeningBalanceId IS NULL 
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"'
                                    union 
									SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew,I.TransactionType
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND V.Archive=0 AND I.IsPark=0 AND I.IsWrittenOff=1  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' 
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetInvestmentSetoffList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AW.FinancingNo,AW.FinancingId,F.DocRefNo InvestmentNo, V.Id VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, V.PostingDate, V.DocDate, V.DocRefNo, C.Code AS CurrencyCode, VD.Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, V.IsPark, AW.LoanSetOffGroupNo
                                    FROM [TRN].[Voucher] AS V
                                    LEFT JOIN [TRN].[FinancingWriteOff] AS AW ON V.Id=AW.VoucherId
                                    left join trn.Financing F on F.Id=AW.FinancingId
									--LEFT JOIN (SELECT Id,FinancingWriteOffId,SUM(Amount) Amount FROM [TRN].[FinancingDetailWriteOff] Group BY Id,FinancingWriteOffId ) AS IWD ON IWD.FinancingWriteOffId=AW.Id
									LEFT JOIN(SELECT VoucherId,SUM(DrAmount) Amount FROM [TRN].[VoucherDetail] group by VoucherId) AS VD ON VD.VoucherId=V.Id
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                                WHERE  V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "'AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='InvestmentSetOff' ";
            return _sqlRepository.GetGridData(parameters);
        }
        public DataTable GetAllLoanRegisterReportData(string companyGroupId, string companyId, string plantId, TransactionType transactionType)
        { 
            var sql = @"SELECT F.Id FinancingNo,F.TransactionType,FT.StandardName ,LoanType=case when f.TransactionType='LoanTaken' then FT.LiabilityUserName else FT.AssetUserName end,f.SourceType
                    ,REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration
                    , C.Code AS CurrencyCode, PP.GSTIN,LoanTo=case 
                    when F.BankMasterId<>'' then BM.AccountTitle
					when f.CashMasterId<>'' then CM.UserName 
                    else '' end
					,F.PartyType,LoanFrom=case	when F.PartyId<>'' then P.UserName
									when f.OtherBankMasterId<>'' then OBM.AccountTitle end
					, BAT.UserName FromBankAccountType,BN.UserName FromBankName,BB.UserName FromBankBranch
					,f.PaymentSource
					,IsOpening = case when f.OpeningBalanceId<>'' then 'YES' ELSE 'NO'  END
					,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, BUD.UserName AS Budget,[Activity]= ACT.UserName
					,CASE WHEN F.IsWrittenOff=1 THEN 'LoanClosed' ELSE 'Open' END IsLoanClose
					, ISNULL(F.Amount,0) AS ScantionAmount, ISNULL(FD.AdditionalLoanAmount,0) AdditionalLoanAmount
					, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
					, ISNULL(FD.Amount+ISNULL(FD.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0))),0) AS TotalLoanAmount
					, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS WrittenOffAmount
					, (ISNULL(FD.Amount+ISNULL(FD.AdditionalLoanAmount,0),0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS RemaningBalance
                    FROM [TRN].[Financing] AS F
                    LEFT JOIN HKP.FinancingType FT ON FT.Id=F.FinancingTypeId
	                left join [TRN].[FinancingDetail]  FD ON FD.FinancingId = F.Id
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=F.VoucherId
                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=F.CurrencyId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=F.PartyId
                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=F.PartyPlantId
                   left join MST.BankMaster BM ON BM.Id=F.BankMasterId
                    left join MST.BankMaster OBM ON OBM.Id=F.OtherBankMasterId
					left join MST.CashMaster CM ON CM.Id=F.CashMasterId
					left join hkp.BankAccountType BAT ON BAT.ID = OBM.BankAccountTypeId
					left join hkp.Bank BN ON BN.Id = OBM.BankId
					left join hkp.BankBranch BB ON BB.Id = OBM.BankBranchId
		            LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=FD.GLGeneralInfoId
					LEFT JOIN [MST].[BudgetMaster] AS BUM ON BUM.Id = FD.BudgetMasterId
					LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
		            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = FD.ActivityId
					LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable','LoanTax') group by LP.FinancingId) LIP ON LIP.FinancingId=F.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=F.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=F.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=F.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=F.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=F.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=F.Id		
                    where F.TransactionType='" + transactionType + @"' AND F.Archive=0 AND F.IsPark=0  AND F.OpeningBalanceId<>'' 

                    AND F.VoucherId <> '' AND F.CompanyGroupId = '" + companyGroupId + @"' AND F.CompanyId = '" + companyId + @"'


                    UNION ALL

                    SELECT F.Id FinancingNo, F.TransactionType,FT.StandardName ,LoanType =case when f.TransactionType = 'LoanTaken' then FT.LiabilityUserName else FT.AssetUserName end, f.SourceType
                    ,REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration
                    , C.Code AS CurrencyCode, PP.GSTIN,LoanTo =case 
                    when F.BankMasterId <> '' then BM.AccountTitle
                      when f.CashMasterId <> '' then CM.UserName
                    else '' end
					,F.PartyType,LoanFrom =case	when F.PartyId <> '' then P.UserName
                                       when f.OtherBankMasterId <> '' then OBM.AccountTitle end
                         , BAT.UserName FromBankAccountType, BN.UserName FromBankName, BB.UserName FromBankBranch
                           , f.PaymentSource
					,IsOpening = case when f.OpeningBalanceId <> '' then 'YES' ELSE 'NO'  END
					,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, BUD.UserName AS Budget,[Activity]= ACT.UserName
					,CASE WHEN F.IsWrittenOff = 1 THEN 'LoanClosed' ELSE 'Open' END IsLoanClose
                      , ISNULL(F.Amount, 0) AS ScantionAmount, ISNULL(FD.AdditionalLoanAmount, 0) AdditionalLoanAmount
					, ISNULL(LIP.InterestAmount, 0) - (ISNULL(LPR.InterestReverseAmount, 0) + ISNULL(CPR.ChargesPayableReverse, 0)) AS InterestAmount
                         , ISNULL(FD.Amount + ISNULL(FD.AdditionalLoanAmount, 0) + (ISNULL(LIP.InterestAmount, 0) - (ISNULL(LPR.InterestReverseAmount, 0) + ISNULL(CPR.ChargesPayableReverse, 0))), 0) AS TotalLoanAmount
                                    , (ISNULL(LPY.LoanPayment, 0) + ISNULL(SLPY.LoanPayment, 0) + ISNULL(ASLPY.InterestCashPayment, 0)) AS WrittenOffAmount
                                           , (ISNULL(FD.Amount + ISNULL(FD.AdditionalLoanAmount, 0), 0) + (ISNULL(LIP.InterestAmount, 0) - (ISNULL(LPR.InterestReverseAmount, 0) + ISNULL(CPR.ChargesPayableReverse, 0))) - (ISNULL(LPY.LoanPayment, 0) + ISNULL(SLPY.LoanPayment, 0) + ISNULL(ASLPY.InterestCashPayment, 0))) AS RemaningBalance
                    FROM[TRN].[Financing] AS F
                    LEFT JOIN HKP.FinancingType FT ON FT.Id = F.FinancingTypeId

                    left join[TRN].[FinancingDetail]  FD ON FD.FinancingId = F.Id
                    LEFT JOIN[TRN].[Voucher] AS V ON V.Id = F.VoucherId
                    LEFT JOIN[SCS].[Currency] AS C ON C.Id = F.CurrencyId
                    LEFT JOIN[HKP].[Party] AS P ON P.Id = F.PartyId
                    LEFT JOIN[HKP].[PartyPlant] AS PP ON PP.Id = F.PartyPlantId
                   left join MST.BankMaster BM ON BM.Id = F.BankMasterId
                    left join MST.BankMaster OBM ON OBM.Id = F.OtherBankMasterId

                    left join MST.CashMaster CM ON CM.Id = F.CashMasterId

                    left join hkp.BankAccountType BAT ON BAT.ID = OBM.BankAccountTypeId

                    left join hkp.Bank BN ON BN.Id = OBM.BankId

                    left join hkp.BankBranch BB ON BB.Id = OBM.BankBranchId

                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = FD.GLGeneralInfoId

                    LEFT JOIN[MST].[BudgetMaster] AS BUM ON BUM.Id = FD.BudgetMasterId

                    LEFT JOIN[HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId

                    LEFT JOIN[HKP].[Activity] AS ACT ON ACT.Id = FD.ActivityId

                    LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) InterestAmount

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable','LoanTax') group by LP.FinancingId) LIP ON LIP.FinancingId = F.Id

                                            LEFT JOIN(SELECT LP.SetOffFinancingId, SUM(LP.Amount) LoanPayment

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType = 'Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId = F.Id

                                            LEFT JOIN(SELECT LP.SetOffFinancingId, SUM(LP.Amount) LoanPayment

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType = 'LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId = F.Id

                                            LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) InterestCashPayment

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType = 'LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId = F.Id

                                            LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) InterestReverseAmount

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId = F.Id
                                            LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) OtherExpensesPayable

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId = F.Id

                                            LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) ChargesPayableReverse

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId = F.Id
                    where F.TransactionType = '" + transactionType + "' AND F.Archive = 0 AND F.IsPark = 0  AND F.OpeningBalanceId IS NULL AND F.VoucherId <> '' AND F.CompanyGroupId = '" + companyGroupId + "' AND F.CompanyId = '" + companyId + @"'";
            return _sqlRepository.GetDataTable(sql);
        }
        public DataTable GetAllLoanRegisterSummaryReportData(string companyGroupId, string companyId, string plantId, TransactionType transactionType)
        {
            var sql = @"SELECT X.FromBankName,X.Budget,(SUM(X.ScantionAmount)+SUM(X.AdditionalLoanAmount)) LoanAmount,SUM(X.InterestAmount)InterestAmount
,SUM(X.TotalLoanAmount)TotalLoanAmount,SUM(X.WrittenOffAmount)WrittenOffAmount,SUM(X.RemaningBalance)RemaningBalance
FROM (SELECT F.Id FinancingNo,F.TransactionType,FT.StandardName ,LoanType=case when f.TransactionType='LoanTaken' then FT.LiabilityUserName else FT.AssetUserName end,f.SourceType
                    ,REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration
                    , C.Code AS CurrencyCode, PP.GSTIN,LoanTo=case 
                    when F.BankMasterId<>'' then BM.AccountTitle
					when f.CashMasterId<>'' then CM.UserName 
                    else '' end
					,F.PartyType,LoanFrom=case	when F.PartyId<>'' then P.UserName
									when f.OtherBankMasterId<>'' then OBM.AccountTitle end
					, BAT.UserName FromBankAccountType,BN.UserName FromBankName,BB.UserName FromBankBranch
					,f.PaymentSource
					,IsOpening = case when f.OpeningBalanceId<>'' then 'YES' ELSE 'NO'  END
					,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, BUD.UserName AS Budget,[Activity]= ACT.UserName
					,CASE WHEN F.IsWrittenOff=1 THEN 'LoanClosed' ELSE 'Open' END IsLoanClose
					, ISNULL(F.Amount,0) AS ScantionAmount, ISNULL(FD.AdditionalLoanAmount,0) AdditionalLoanAmount
					, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
					, ISNULL(FD.Amount+ISNULL(FD.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0))),0) AS TotalLoanAmount
					, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS WrittenOffAmount
					, (ISNULL(FD.Amount+ISNULL(FD.AdditionalLoanAmount,0),0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS RemaningBalance
                    FROM [TRN].[Financing] AS F
                    LEFT JOIN HKP.FinancingType FT ON FT.Id=F.FinancingTypeId
	                left join [TRN].[FinancingDetail]  FD ON FD.FinancingId = F.Id
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=F.VoucherId
                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=F.CurrencyId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=F.PartyId
                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=F.PartyPlantId
                   left join MST.BankMaster BM ON BM.Id=F.BankMasterId
                    left join MST.BankMaster OBM ON OBM.Id=F.OtherBankMasterId
					left join MST.CashMaster CM ON CM.Id=F.CashMasterId
					left join hkp.BankAccountType BAT ON BAT.ID = OBM.BankAccountTypeId
					left join hkp.Bank BN ON BN.Id = OBM.BankId
					left join hkp.BankBranch BB ON BB.Id = OBM.BankBranchId
		            LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=FD.GLGeneralInfoId
					LEFT JOIN [MST].[BudgetMaster] AS BUM ON BUM.Id = FD.BudgetMasterId
					LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
		            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = FD.ActivityId
					LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=F.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=F.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=F.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=F.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=F.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=F.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=F.Id		
                    where F.TransactionType='" + transactionType + @"' AND F.Archive=0 AND F.IsPark=0  AND F.OpeningBalanceId<>'' 

                    AND F.VoucherId <> '' AND F.CompanyGroupId = '" + companyGroupId + @"' AND F.CompanyId = '" + companyId + @"'


                    UNION ALL

                    SELECT F.Id FinancingNo, F.TransactionType,FT.StandardName ,LoanType =case when f.TransactionType = 'LoanTaken' then FT.LiabilityUserName else FT.AssetUserName end, f.SourceType
                    ,REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration
                    , C.Code AS CurrencyCode, PP.GSTIN,LoanTo =case 
                    when F.BankMasterId <> '' then BM.AccountTitle
                      when f.CashMasterId <> '' then CM.UserName
                    else '' end
					,F.PartyType,LoanFrom =case	when F.PartyId <> '' then P.UserName
                                       when f.OtherBankMasterId <> '' then OBM.AccountTitle end
                         , BAT.UserName FromBankAccountType, BN.UserName FromBankName, BB.UserName FromBankBranch
                           , f.PaymentSource
					,IsOpening = case when f.OpeningBalanceId <> '' then 'YES' ELSE 'NO'  END
					,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, BUD.UserName AS Budget,[Activity]= ACT.UserName
					,CASE WHEN F.IsWrittenOff = 1 THEN 'LoanClosed' ELSE 'Open' END IsLoanClose
                      , ISNULL(F.Amount, 0) AS ScantionAmount, ISNULL(FD.AdditionalLoanAmount, 0) AdditionalLoanAmount
					, ISNULL(LIP.InterestAmount, 0) - (ISNULL(LPR.InterestReverseAmount, 0) + ISNULL(CPR.ChargesPayableReverse, 0)) AS InterestAmount
                         , ISNULL(FD.Amount + ISNULL(FD.AdditionalLoanAmount, 0) + (ISNULL(LIP.InterestAmount, 0) - (ISNULL(LPR.InterestReverseAmount, 0) + ISNULL(CPR.ChargesPayableReverse, 0))), 0) AS TotalLoanAmount
                                    , (ISNULL(LPY.LoanPayment, 0) + ISNULL(SLPY.LoanPayment, 0) + ISNULL(ASLPY.InterestCashPayment, 0)) AS WrittenOffAmount
                                           , (ISNULL(FD.Amount + ISNULL(FD.AdditionalLoanAmount, 0), 0) + (ISNULL(LIP.InterestAmount, 0) - (ISNULL(LPR.InterestReverseAmount, 0) + ISNULL(CPR.ChargesPayableReverse, 0))) - (ISNULL(LPY.LoanPayment, 0) + ISNULL(SLPY.LoanPayment, 0) + ISNULL(ASLPY.InterestCashPayment, 0))) AS RemaningBalance
                    FROM[TRN].[Financing] AS F
                    LEFT JOIN HKP.FinancingType FT ON FT.Id = F.FinancingTypeId

                    left join[TRN].[FinancingDetail]  FD ON FD.FinancingId = F.Id
                    LEFT JOIN[TRN].[Voucher] AS V ON V.Id = F.VoucherId
                    LEFT JOIN[SCS].[Currency] AS C ON C.Id = F.CurrencyId
                    LEFT JOIN[HKP].[Party] AS P ON P.Id = F.PartyId
                    LEFT JOIN[HKP].[PartyPlant] AS PP ON PP.Id = F.PartyPlantId
                   left join MST.BankMaster BM ON BM.Id = F.BankMasterId
                    left join MST.BankMaster OBM ON OBM.Id = F.OtherBankMasterId

                    left join MST.CashMaster CM ON CM.Id = F.CashMasterId

                    left join hkp.BankAccountType BAT ON BAT.ID = OBM.BankAccountTypeId

                    left join hkp.Bank BN ON BN.Id = OBM.BankId

                    left join hkp.BankBranch BB ON BB.Id = OBM.BankBranchId

                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = FD.GLGeneralInfoId

                    LEFT JOIN[MST].[BudgetMaster] AS BUM ON BUM.Id = FD.BudgetMasterId

                    LEFT JOIN[HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId

                    LEFT JOIN[HKP].[Activity] AS ACT ON ACT.Id = FD.ActivityId

                    LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) InterestAmount

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId = F.Id

                                            LEFT JOIN(SELECT LP.SetOffFinancingId, SUM(LP.Amount) LoanPayment

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType = 'Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId = F.Id

                                            LEFT JOIN(SELECT LP.SetOffFinancingId, SUM(LP.Amount) LoanPayment

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType = 'LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId = F.Id

                                            LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) InterestCashPayment

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType = 'LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId = F.Id

                                            LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) InterestReverseAmount

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId = F.Id
                                            LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) OtherExpensesPayable

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId = F.Id

                                            LEFT JOIN(SELECT LP.FinancingId, SUM(LP.Amount) ChargesPayableReverse

                                            FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId = F.Id
                    where F.TransactionType = '" + transactionType + "' AND F.Archive = 0 AND F.IsPark = 0  AND F.OpeningBalanceId IS NULL AND F.VoucherId <> '' AND F.CompanyGroupId = '" + companyGroupId + "' AND F.CompanyId = '" + companyId + @"' )X
					WHERE X.RemaningBalance>0
					GROUP BY X.FromBankName,X.Budget
					ORDER BY X.FromBankName";
            return _sqlRepository.GetDataTable(sql);
        }
        public IEnumerable<object> GetLoanRegisterDataList(string companyGroupId, string companyId, string plantId, string transactionType)
        {
            try
            {
                var sql = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0)+ISNULL(LTP.TaxAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0)+ISNULL(LTP.TaxAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) TaxAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanTax') group by LP.FinancingId) LTP ON LTP.FinancingId=I.Id 
                                            LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId IS NULL AND I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"'
                                    union 
									SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' AND  I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public Dictionary<string, object> GetLoanById(string id)
        {
            var sql = @"SELECT  ATRN.Id, ATRN.CompanyGroupId, ATRN.CompanyId, ATRN.EntityId, ATRN.CurrencyId, ATRN.PartyId, P.Code+' - '+P.UserName AS PartyName, ATRN.EmployeeId, ATRN.PartyType, ATRN.BankMasterId, V.VoucherTypeId
                    , V.VoucherNo, V.VoucherDate, ATRN.DocDate, ATRN.DocRefNo, ATRN.PostingDate, FY.FiscalYearName, FYP.PeriodName AS FiscalYearPeriodName, ATRN.Narration, ATRN.Amount, PVD.GLGeneralInfoId AS PartyGLGeneralInfoId
                    , PGL.AccountCode+' - '+ PGL.UserName AS PartyGL, PVD.BudgetId AS PartyBudgetId, PB.Code+' - '+ PB.UserName AS PartyBudgetName, PVD.ActivityId AS PartyActivityId, PA.Code+' - '+ PA.UserName AS PartyActivityName
                    , ATRN.BankAmount,  BVD.GLGeneralInfoId AS BankGLGeneralInfoId, BGL.AccountCode+' - '+BGL.UserName AS BankGL, BVD.BudgetId AS BankBudgetId, BB.Code+' - '+ BB.UserName AS BankBudgetName, BVD.ActivityId AS BankActivityId, BA.Code+' - '+ BA.UserName AS BankActivityName
                    , BM.AccountNumber AS BankAccountNumber, BC.Code+' - '+ BC.[Name] AS CurrencyCode, B.UserName AS BankName, BBR.UserName AS BankBranchName
                    FROM [TRN].[AccountTransaction] AS ATRN
                    LEFT JOIN (SELECT * FROM [TRN].[VoucherDetail] AS VD WHERE VD.PartyId IS NOT NULL) AS PVD ON PVD.AccountTransactionId=ATRN.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS PGL ON PGL.Id=PVD.GLGeneralInfoId
                    LEFT JOIN [HKP].[Budget] AS PB ON PB.Id=PVD.BudgetId
                    LEFT JOIN [HKP].[Activity] AS PA ON PA.Id=PVD.ActivityId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=ATRN.PartyId
                    LEFT JOIN(SELECT * FROM [TRN].[VoucherDetail] AS VD WHERE VD.BankMasterId IS NOT NULL) AS BVD ON BVD.AccountTransactionId=ATRN.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS BGL ON BGL.Id=BVD.GLGeneralInfoId
                    LEFT JOIN [HKP].[Budget] AS BB ON BB.Id=BVD.BudgetId
                    LEFT JOIN [HKP].[Activity] AS BA ON BA.Id=BVD.ActivityId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=ATRN.BankMasterId
                    LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                    LEFT JOIN [HKP].[BankBranch] AS BBR ON BBR.Id=BM.BankBranchId
                    LEFT JOIN [SCS].[Currency] AS BC ON BC.Id=BM.CurrencyId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=PVD.VoucherId
                    LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                    LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                    WHERE ATRN.Id='" + id + "'";
            return _sqlRepository.GetData(sql);
        }
        public IEnumerable<object> GetLoanList(string companyGroupId, string companyId, string plantId, string transactionType)
        {
            try
            {
                var sql = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+ISNULL(I.DownPaymentAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0)+ISNULL(LTP.TaxAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, ISNULL(LTP.TaxAmount,0) AS TaxAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(I.DownPaymentAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0)+ISNULL(LTP.TaxAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo,bk.UserName BankName
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0)+ISNULL(I.DownPaymentAmount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                        ,0 isSelected,NULL ExchangeType,0 BaseDrAmount,0 BaseCrAmount
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
                                        LEFT JOIN HKP.Bank AS bk ON bk.Id=OBKM.BankId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') 
											group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) TaxAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanTax') 
											group by LP.FinancingId) LTP ON LTP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.DownPaymentAmount) AdditionalLoanDownPaymentAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AdditionalLoanPayable') group by LP.FinancingId) ADLPD ON ADLPD.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId IS NULL AND I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"' AND I.IsWrittenOff=0
                                    union 
									SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+ISNULL(I.DownPaymentAmount,0) +ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, 0 AS TaxAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(I.DownPaymentAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo,bk.UserName BankName
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0)+ISNULL(I.DownPaymentAmount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                         , 0 isSelected,NULL ExchangeType,0 BaseDrAmount,0 BaseCrAmount
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
                                        LEFT JOIN HKP.Bank AS bk ON bk.Id=OBKM.BankId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.DownPaymentAmount) AdditionalLoanDownPaymentAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AdditionalLoanPayable') group by LP.FinancingId) ADLPD ON ADLPD.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' AND  I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.IsWrittenOff=0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetLoanListMultipleSetoff(string companyGroupId, string companyId, string plantId, string transactionType, string partyType, string bankId)
        {
            string filter = "";

            if (partyType == "Bank")
            {
                //filter = " AND I.PartyType='Bank' AND I.OtherBankMasterId IN (SELECT Id FROM mst.BankMaster WHERE BankId='" + bankId + @"')";
                filter = " AND I.PartyType='Bank' ";
            }
            else
            {
                filter = " AND I.PartyType='" + partyType + @"'";
            }
            try
            {
                var sql = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo,bk.UserName BankName
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                        ,0 isSelected,NULL ExchangeType,0 BaseDrAmount,0 BaseCrAmount
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
                                        LEFT JOIN HKP.Bank AS bk ON bk.Id=OBKM.BankId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId IS NULL AND I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"' AND I.IsWrittenOff=0 " + filter + @"
                                    union 
									SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo,bk.UserName BankName
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                         , 0 isSelected,NULL ExchangeType,0 BaseDrAmount,0 BaseCrAmount
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
                                        LEFT JOIN HKP.Bank AS bk ON bk.Id=OBKM.BankId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' AND  I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.IsWrittenOff=0 " + filter + @" ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetLoanListForSalesRealization(string companyGroupId, string companyId, string plantId, string transactionType)
        {
            try
            {
                var sql = @"SELECT * FROM
                            (SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
										,OBKM.AccountTitle,I.OtherBankMasterId BankMasterId,Bk.UserName AS BankName, OBKM.Code AS BankCode
										, OBKM.AccountNumber
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [HKP].[Bank] AS Bk ON Bk.Id=OBKM.BankId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId IS NULL AND ISNULL(I.OtherBankMasterId,'') <>'' AND I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"'
                                    union 
									SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
										,OBKM.AccountTitle,I.OtherBankMasterId BankMasterId,Bk.UserName AS BankName, OBKM.Code AS BankCode
										, OBKM.AccountNumber
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [HKP].[Bank] AS Bk ON Bk.Id=OBKM.BankId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' AND ISNULL(I.OtherBankMasterId,'') <>''  AND  I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"'
                                    )X
									WHERE X.Balance>0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetLoanRegisterList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string transactionType)
        {
            try
            {
                parameters.CmdText = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received, (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE
										WHEN ID.BankMasterId<>'' THEN BKM.AccountTitle
										WHEN P.UserName<>'' THEN P.UserName 
										WHEN ID.CashMasterId<>'' THEN CM.UserName
										ELSE ''	END
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=ID.BankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=ID.CashMasterId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0   AND I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"'
                                    union 
									SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received, (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE
										WHEN ID.BankMasterId<>'' THEN BKM.AccountTitle
										WHEN P.UserName<>'' THEN P.UserName 
										WHEN ID.CashMasterId<>'' THEN CM.UserName
										ELSE ''	END
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=ID.BankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=ID.CashMasterId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' AND  I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetLoanZeroBalanceList(string companyGroupId, string companyId, string plantId, string transactionType)
        {
            try
            {
                var sql = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0 and I.IsWrittenOff=0  AND I.OpeningBalanceId IS NULL AND I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"'
                                    and (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)))=0
                                    union 
									SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0) AS LoanAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS LoanPayment
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayable','OtherExpensesPayable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0 and I.IsWrittenOff=0  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' AND  I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"'
                            and (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0)))=0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #endregion

        private Dictionary<string, object> GetMultiloanPaymentHeader(string companyGroupId, string companyId, string plantId, string loanWriteOffGroupNo, string sourceType)
        {
            var cmdText = @"SELECT AW.LoanSetOffGroupNo, REPLACE(CONVERT(VARCHAR(11), AW.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , P.Code AS PartyCode, P.UserName AS Customer, REPLACE(CONVERT(VARCHAR(11), AW.PostingDate, 106), ' ', '-') AS PostingDate 
                            , REPLACE(CONVERT(VARCHAR(11), AW.DocDate, 106), ' ', '-') AS DocDate, AW.DocRefNo, C.Code AS CurrencyCode,SUM(IWD.Amount) Amount
                            , AW.PartyPlantId, PP.UserName AS CustomerPlant,   UPPER(AW.Narration) AS Narration
                            , VT.UserName AS VoucherTypeName,UA.FullName AddedBy, CASE WHEN AW.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            ,AW.CurrencyId,U.FullName PostedBy
                             ,VoucherNo=STUFF((SELECT DISTINCT ','+xpo.VoucherNo from
                            			[TRN].Voucher xpo
                            			INNER JOin [TRN].[FinancingWriteOff] xPDAMAP on xpo.Id=xPDAMAP.VoucherId
                            			WHERE AW.LoanSetOffGroupNo=xPDAMAP.LoanSetOffGroupNo for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            FROM [TRN].[FinancingWriteOff] AS AW
                            LEFT JOIN (
                            SELECT FinancingWriteOffId,SUM(Amount) Amount FROM [TRN].[FinancingDetailWriteOff] Group BY FinancingWriteOffId 
                            ) AS IWD ON IWD.FinancingWriteOffId=AW.Id
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=AW.VoucherTypeId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
							left join [TRN].[Voucher] V on V.Id=AW.VoucherId
							left join SEC.[User] UA on UA.UserId=V.AddedBy
							left join SEC.[User] U on U.UserId=V.PostedBy

                            WHERE AW.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.LoanSetOffGroupNo='" + loanWriteOffGroupNo + "' AND AW.[SourceType]='" + sourceType + @"'
                            Group BY AW.LoanSetOffGroupNo, AW.VoucherDate
                            , P.Code , P.UserName, AW.PostingDate,VT.UserName,AW.AddedBy,AW.Narration,V.PostedBy
                            , AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.CurrencyId,UA.FullName,U.FullName";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetMultiloanPaymentVoucher(string loanWriteOffGroupNo)
        {
            try
            {
                var sql = @"SELECT  GL.Id AS AccountCodeId--, VDC.VoucherDetailId
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
							, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo--, V.VoucherNo
							, UPPER(V.Narration) AS Narration
                            , V.CurrencyId
							, REPLACE(CONVERT(VARCHAR(11), IV.VoucherDate, 106), ' ', '-') AS VoucherDate
							, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							 , VDC.ToCurrencyRate, SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, SUM(VDC.DrAmount) AS CompanyCurrencyDrAmount, SUM(VDC.CrAmount) AS CompanyCurrencyCrAmount
							, [DRCR]=CASE WHEN SUM(VDC.DrAmount)>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            --, P.UserName AS Customer, PP.UserName AS CustomerPlant
                            , VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , Activity= CASE WHEN VD.BankMasterId<>'' THEN ACT.UserName+' - '+ BM.AccountTitle 
                            WHEN CM.UserName<>'' THEN ACT.UserName+' - '+ CM.UserName 
                            ELSE ACT.UserName END--,VD.PartyType
                            FROM 
							[TRN].[FinancingWriteOff] AS IV  
                            LEFT JOIN [TRN].[Voucher] AS V  ON IV.VoucherId=V.Id
							LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherId=V.Id
                            LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                            WHERE V.Archive=0 AND IV.LoanSetOffGroupNo='" + loanWriteOffGroupNo + @"' 
							GROUP BY  GL.Id 
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, V.PostingDate
                            , V.IsPark , v.DocDate, V.DocRefNo, V.Narration
                            , V.CurrencyId, IV.VoucherDate, CU1.Code , V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code 
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							, VDC.ToCurrencyRate--, VD.DrAmount, VD.CrAmount, VDC.DrAmount, VDC.CrAmount
                            , VD.GLGeneralInfoId, GL.UserName, GL.AccountCode
                            --, P.UserName, PP.UserName 
                            , VD.Narration, BUD.UserName
                            ,  VD.BankMasterId,ACT.UserName,BM.AccountTitle,CM.UserName --,VD.PartyType
							ORDER BY SUM(VD.DrAmount) DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private bool GetPlantIsShowFCInWord(string plantId)
        {
            return bplib.clsWebLib.GetBoolData(_sqlRepository.GetDataCollection(@"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='" + plantId + "'")[0]["IsShowFCInWord"].ToString());
        }

        public IWorkbook MultiloanPaymentReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string loanWriteOffGroupNo, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetMultiloanPaymentHeader(companyGroupId, companyId, plantId, loanWriteOffGroupNo, sourceType);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetMultiloanPaymentVoucher(loanWriteOffGroupNo);

            var transcationCurrency = header["CurrencyId"].ToString();
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            sheet[row, 1, row + 1, 1].Merge();
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            sheet[row, 2, row + 1, 2].Merge();
            sheet.UsedRange.WrapText = true;

            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row+1, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row+1, 2, header["PostingDate"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());
            row++;

            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
            //reportUtility.SetText(ref sheet, row, 2, header["Customer"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            //reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());
            row++;


            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row - 1, 1, "Narration");
            reportUtility.SetText(ref sheet, row - 1, 2, header["Narration"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row-1, 2, row, 2].Merge();
            sheet.UsedRange.WrapText = true;
            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 3, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 3, row, 4].Merge();
                sheet.Range[row, 3, row, 4].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, 3, row, 4].BorderAround(ExcelLineStyle.Hair);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 3, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 3, row, 4].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 5, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
                sheet.Range[row, 3, row, 4].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, 3, row, 4].BorderAround(ExcelLineStyle.Hair);
            }

            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }
            sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }

                reportUtility.SetText(ref sheet, row, 2, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[row, 1].ColumnWidth = 22;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        public IEnumerable<object> GetloanBankListcbo()
        {
            try
            {
                var sql = @"SELECT Id Value, UserName Text  FROM HKP.Bank WHERE Active=1 and Archive=0 and Id in(SELECT BankId FROM mst.BankMaster WHERE AccountType='Loan') order by UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetInvestmentList(string companyGroupId, string companyId, string plantId, string transactionType)
        {
            try
            {
                var sql = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+ISNULL(I.DownPaymentAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0) AS InvestmentAmount
										, ISNULL(LIP.InterestAmount,0)+ISNULL(LTP.TaxAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, ISNULL(LTP.TaxAmount,0) AS TaxAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(I.DownPaymentAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS InvestmentSetOff
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0)+ISNULL(LTP.TaxAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo,bk.UserName BankName
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0)+ISNULL(I.DownPaymentAmount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                        ,0 isSelected,NULL ExchangeType,0 BaseDrAmount,0 BaseCrAmount
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
                                        LEFT JOIN HKP.Bank AS bk ON bk.Id=OBKM.BankId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InvestmentInterestReceivable') 
											group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) TaxAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanTax') 
											group by LP.FinancingId) LTP ON LTP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InvestmentGiven') and LP.SourceType='InvestmentSetOff' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='LoanPayment' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.DownPaymentAmount) AdditionalLoanDownPaymentAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AdditionalLoanPayable') group by LP.FinancingId) ADLPD ON ADLPD.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId IS NULL AND I.TransactionType='"+ transactionType + @"'
                                    AND I.CompanyId='" + companyId + @"' AND I.IsWrittenOff=0
                                    union ALL
									SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS FinancingId
                                        , ID.Id AS FinancingDetailId, I.FinancingTypeId, I.VoucherId, V.VoucherNo, I.FinancingNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration
                                        , ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+ISNULL(I.DownPaymentAmount,0) +ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0) AS InvestmentAmount
										, ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)) AS InterestAmount
										, 0 AS TaxAmount
										, (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(I.DownPaymentAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0)+ISNULL(ASLPY.InterestCashPayment,0)) AS InvestmentSetOff
										, (ISNULL(ID.Amount+ID.AdditionalLoanAmount,0)+(ISNULL(LIP.InterestAmount,0) - (ISNULL(LPR.InterestReverseAmount,0)+ISNULL(CPR.ChargesPayableReverse,0)))- (ISNULL(LPY.LoanPayment,0)+ISNULL(SLPY.LoanPayment,0)+ISNULL(ASLPY.InterestCashPayment,0))) AS Balance
										,ISNULL(LPR.InterestReverseAmount,0) InterestReverseAmount
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo,bk.UserName BankName
										,[Particulars]=CASE WHEN P.UserName<>'' THEN P.UserName  WHEN I.OtherBankMasterId<>'' THEN OBKM.AccountTitle WHEN I.CashMasterId<>'' THEN CM.UserName ELSE ''	END
                                        , I.OtherBankMasterId ,I.PostingDate PostingDateNew
										,ISNULL(ID.Amount,0)+ISNULL(I.DownPaymentAmount,0) InitialSactionAmount  , ISNULL(ID.AdditionalLoanAmount,0)+ISNULL(ADLPD.AdditionalLoanDownPaymentAmount,0) AdditionalLoanAmount
										, ISNULL(LPE.OtherExpensesPayable,0)- ISNULL(CPR.ChargesPayableReverse,0) OtherExpensesPayable
                                        , Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDateNew
                                         , 0 isSelected,NULL ExchangeType,0 BaseDrAmount,0 BaseCrAmount
                                        FROM [TRN].[FinancingDetail] AS ID
                                        LEFT JOIN [TRN].[Financing] AS I ON I.Id=ID.FinancingId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.FinancingDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=I.BankMasterId
										LEFT JOIN [MST].BankMaster AS OBKM ON OBKM.Id=I.OtherBankMasterId
										LEFT JOIN [MST].CashMaster AS CM ON CM.Id=I.CashMasterId
                                        LEFT JOIN HKP.Bank AS bk ON bk.Id=OBKM.BankId
										LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InvestmentInterestReceivable') group by LP.FinancingId) LIP ON LIP.FinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('LoanPayment') and LP.SourceType='Loan' group by LP.SetOffFinancingId) LPY ON LPY.SetOffFinancingId=I.Id
											LEFT JOIN(SELECT LP.SetOffFinancingId,SUM(LP.Amount) LoanPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InvestmentGiven') and LP.SourceType='InvestmentSetOff' group by LP.SetOffFinancingId) SLPY ON SLPY.SetOffFinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestCashPayment
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AccrulInterestPayment') and LP.SourceType='LoanPayment' group by LP.FinancingId) ASLPY ON ASLPY.FinancingId=I.Id
											
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) InterestReverseAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('InterestPayableReverse') group by LP.FinancingId) LPR ON LPR.FinancingId=I.Id
                                            LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) OtherExpensesPayable
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('OtherExpensesPayable') group by LP.FinancingId) LPE ON LPE.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.Amount) ChargesPayableReverse
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('ChargesPayableReverse') group by LP.FinancingId) CPR ON CPR.FinancingId=I.Id
											LEFT JOIN(SELECT LP.FinancingId,SUM(LP.DownPaymentAmount) AdditionalLoanDownPaymentAmount
											FROM TRN.FinancingSubsequentTransaction LP where lp.TransactionType in ('AdditionalLoanPayable') group by LP.FinancingId) ADLPD ON ADLPD.FinancingId=I.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsPark=0  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' AND  I.TransactionType='" + transactionType + @"'
                                    AND I.CompanyId='" + companyId + @"' AND I.IsWrittenOff=0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetInvestmentInterestReceivableList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT  LP.VoucherId, V.VoucherNo, LP.Id, P.Code AS PartyCode, P.UserName AS PartyName, LP.PostingDate, LP.DocDate, LP.DocRefNo, C.Code AS CurrencyCode, LP.Amount
                                    , LP.PartyPlantId, PP.UserName AS PartyPlantName, LP.IsPark,LP.FinancingId FinancingNo,F.DocRefNo InvestmentNo,V.SourceType
                                    FROM [TRN].[FinancingSubsequentTransaction] AS LP
                                    LEFT JOIN TRN.Financing F ON F.Id=LP.FinancingId
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LP.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=LP.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=LP.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=LP.CurrencyId
                                WHERE V.Archive=0 AND LP.CompanyGroupId='" + companyGroupId + "'AND LP.CompanyId='" + companyId + "' AND LP.PlantId='" + plantId + "' AND LP.SourceType='"+ sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }
    }
}
