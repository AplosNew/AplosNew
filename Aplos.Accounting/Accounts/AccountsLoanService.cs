using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Enums;
using Library.Service.Logs;
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
                                , F.VoucherId, F.PostingDate, F.DocDate, F.DocRefNo, F.CurrencyId, C.Code AS CurrencyCode, F.Amount, F.IsWrittenOff, F.WrittenOffAmount, F.IsPark, F.IsPosted
                                FROM [TRN].[Financing] AS F
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=F.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=F.PartyPlantId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=F.EmployeeId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=F.CurrencyId
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=F.VoucherId
                                WHERE F.OpeningBalanceId IS NULL AND F.Archive=0 AND F.CompanyGroupId='" + companyGroupId + "'AND F.CompanyId='" + companyId + "' AND F.PlantId='" + plantId + "' AND F.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetLoanWriteOffList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AW.FinancingNo,AW.FinancingId,F.DocRefNo LoanNo, V.Id VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, V.PostingDate, V.DocDate, V.DocRefNo, C.Code AS CurrencyCode, VD.Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, V.IsPark
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
                                    , LP.PartyPlantId, PP.UserName AS PartyPlantName, LP.IsPark,LP.FinancingId FinancingNo,F.DocRefNo LoanNo,V.SourceType
                                    FROM [TRN].[FinancingSubsequentTransaction] AS LP
                                    LEFT JOIN TRN.Financing F ON F.Id=LP.FinancingId
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LP.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=LP.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=LP.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=LP.CurrencyId
                                WHERE   LP.CompanyGroupId='" + companyGroupId + "'AND LP.CompanyId='" + companyId + "' AND LP.PlantId='" + plantId + "' AND LP.SourceType in ('LoanInterestPayable','AdditionalLoanPayable','OtherExpensesPayable')";
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
                                WHERE   LP.CompanyGroupId='" + companyGroupId + "'AND LP.CompanyId='" + companyId + "' AND LP.PlantId='" + plantId + "' AND LP.SourceType in ('LoanInterestPayableReverse')";
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
                                    WHERE I.Archive=0 AND I.IsPark=0 AND I.IsWrittenOff=1 AND I.OpeningBalanceId IS NULL 
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
                                    WHERE I.Archive=0 AND I.IsPark=0 AND I.IsWrittenOff=1  AND I.OpeningBalanceId<>'' AND I.VoucherId<>'' 
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public DataTable GetAllLoanRegisterReportData(string companyGroupId, string companyId, string plantId, TransactionType transactionType)
        {
            // var sql = "";



            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                    SELECT F.Id FinancingNo,F.TransactionType,FT.StandardName ,LoanType=case when f.TransactionType='LoanTaken' then FT.LiabilityUserName else FT.AssetUserName end,f.SourceType
                    ,REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration

                    , C.Code AS CurrencyCode, PP.GSTIN
                    --, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                     ,LoanTo=case 
                    when F.BankMasterId<>'' then BM.AccountTitle
					when f.CashMasterId<>'' then CM.UserName 
                    else '' end
					,F.PartyType
					,LoanFrom=case	when F.PartyId<>'' then P.UserName
									
									when f.OtherBankMasterId<>'' then OBM.AccountTitle end
					, BAT.UserName FromBankAccountType,BN.UserName FromBankName,BB.UserName FromBankBranch
					,f.PaymentSource
					,IsOpening = case when f.OpeningBalanceId<>'' then 'YES' ELSE 'NO'  END
                    , ISNULL(F.Amount,0) AS ScantionAmount,ISNULL(f.WrittenOffAmount,0) WrittenOffAmount,ISNULL(F.Amount,0)-ISNULL(f.WrittenOffAmount,0) BalancePrinciple
                     ,ISNULL(LIP.InterestAmount,0) InterestAmount,ISNULL(LP.WrittenOffInterestAmount,0) WrittenOffInterestAmount
                    ,ISNULL(LIP.InterestAmount,0) -ISNULL(LP.WrittenOffInterestAmount,0) InterestBalanceAmount,
                    ISNULL(F.Amount,0)-ISNULL(f.WrittenOffAmount,0)+(ISNULL(LIP.InterestAmount,0) -ISNULL(LP.WrittenOffInterestAmount,0)) RemaningBalance


						,GL.UserName AS GL
						, GL.AccountCode AS GLGeneralInfoCode
						, BUD.UserName AS Budget
						 ,[Activity]= CASE 
							WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
							WHEN CM.UserName<>'' THEN CM.UserName 
							WHEN ACT.UserName<>'' THEN ACT.UserName 
							ELSE ''	END


                    FROM
                    [TRN].[Financing] AS F
                    LEFT JOIN HKP.FinancingType FT ON FT.Id=F.FinancingTypeId
	                left join [TRN].[FinancingDetail]  FD ON FD.FinancingId = F.Id
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=F.VoucherId
                    LEFT JOIN(SELECT FinancingId,SUM(ISNULL(Amount,0)) InterestAmount 
                     FROM TRN.FinancingSubsequentTransaction  where IsPark=0 and TransactionType in ('InterestPayable') group by FinancingId) LIP ON LIP.FinancingId=F.Id
					 LEFT JOIN(SELECT FinancingId,SUM(ISNULL(Amount,0)) WrittenOffInterestAmount
                     FROM TRN.FinancingSubsequentTransaction  where IsPark=0 and TransactionType in ('AccrulInterestPayment')  group by FinancingId) LP ON LP.FinancingId=F.Id
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
                    where F.SourceType='" + SourceType.Loan.ToString() + "' and f.TransactionType='" + transactionType + "'";
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

    }
}
