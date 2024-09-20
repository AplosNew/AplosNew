using Library.Core;
using Library.Data.Sql;
using Library.Model.Enums;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Extension.Accounts
{
    public class BankExtensionService
    {
        SqlRepository _sqlRepository;
        public BankExtensionService()
        {
            _sqlRepository = new SqlRepository();
        }


        public Dictionary<string, object> GetBankJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
                            --, V.AddedBy, V.PostedBy
                            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , B.UserName AS BankName, BB.UserName AS BankBranchName, BM.AccountNumber, BM.AccountTitle, BJ.CurrencyId, C.Code AS CurrencyCode, BJ.BankJournalType

							,CLD1.Id CheckLotDetailId, CLD1.CheckNumber, CLH.CheckDate, CL.LotNumber,p.UserName Party

							, AddedBy =case when u.FullName<>'' then u.FullName else v.AddedBy end
							,PostedBy = case when up.FullName<>'' then up.FullName else v.PostedBy end
							,ET.UserName Entity
                            FROM [TRN].[BankJournal] AS BJ
							left join org.Entity ET on ET.Id = BJ.EntityId
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=BJ.BankMasterId
                            LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                            LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId

						    LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.BankMasterId<>''
						    left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId

							left join trn.CheckLot CL on CL.Id=BM.Id
							left join trn.CheckLotDetail CLD ON CLD.CheckLotId=CL.Id
							--left join trn.CheckLotDetail CLD on CLD.Id=CL.Id
							--left join trn.CheckLotDetailHistory CLDH ON CLDH.Id=CLD.Id

							left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD1 ON CLD1.Id=CLH.CheckLotDetailId

						   -- LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
		                    left join [sec].[User] U on U.UserId=v.AddedBy
							left join sec.[User] up on up.UserId=v.PostedBy
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public DataTable GetBankJournalDetail(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, GLGI.Id AS AccountCodeId, GLGI.AccountCode, VD.Id AS VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GLGI.UserName AS GL, GLGI.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId, B.UserName AS BudgetName
                            , ActivityName=case when vd.BankMasterId<>'' then BM.AccountTitle when vd.CashMasterId<>'' then CM.UserName else A.UserName end
                            , UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType,BM.AccountTitle as AccountTitleName
							,[ParticularName]=CASE
								WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
								WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
								WHEN P.UserName<>'' THEN P.UserName 
								WHEN CM.UserName<>'' THEN CM.UserName
								ELSE ''	END
                           FROM [TRN].[VoucherDetailCurrency] AS VDC
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [TRN].[BankJournalDetail] AS BJD ON BJD.Id=VD.BankJournalDetailId
                            LEFT JOIN [TRN].[BankJournal] AS BJ ON BJ.Id=BJD.BankJournalId
                            LEFT JOIN [HKP].[FinancingType] AS BJDFT ON BJDFT.Id=BJD.FinancingTypeId
                            LEFT JOIN [TRN].[BankCharge] AS BC ON BC.Id=VD.BankChargeId
                            LEFT JOIN [HKP].[FinancingType] AS BCFT ON BCFT.Id=BC.FinancingTypeId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS B ON B.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=BJD.PartyId
							LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=BJ.EmployeeId
							 LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
							 LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
							LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }


        public GridModel GetBankJournalDetail(GridParameter parameters, string companyGroupId, string companyId, string plantId, string voucherId, string voucherDetailId)
        {
            parameters.CmdText = @"	SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId,VDC.Id AS VoucherDetailCurrencyId, V.EntityId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Park' ELSE 'Post' END, Replace(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106)
                            , ' ', '-') AS VoucherDate, V.VoucherNo, v.Narration, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy AS PreparedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.ToCurrencyRate, VD.DrAmount+VD.CrAmount AS Value, VDC.DrAmount, VDC.CrAmount, V.SourceType, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId
                            , GL.UserName AS GLGeneralInfoName, GL.AccountCode AS GLGeneralInfoCode,CM.UserName AS CashName, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate
                            , VD.DocRefNo AS InvoiceNo, VD.RefCode AS Ref, VD.Narration AS DetailNarration, CO.UserName AS CompanyName, AM.Address1 AS AddressLine,BUD.Code AS BudgetCode, BUD.UserName AS BudgetName, ACT.UserName AS ActivityName, ACT.Code AS ActivityCode
							,EI.[EmployeeName] AS [Employee]
							,VDC.FromCurrencyId, VDC.CrAmount AS CompanyCurrencyCr, VDC.DrAmount AS CompanyCurrencyDr
							,CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [ORG].[Company] AS CO ON CO.Id=V.CompanyId
							LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=CO.AddressMasterId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
							LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							WHERE V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"'
                                      AND V.Id= '" + voucherId + "'  AND VD.Id<> '" + voucherDetailId + @"' ";
            return _sqlRepository.GetGridData(parameters);
        }

        public Dictionary<string, object> GetCashJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
							--, V.AddedBy, V.PostedBy
                            , CM.UserName AS CashName, BJ.CurrencyId, C.Code AS CurrencyCode, BJ.BankJournalType

						   ,CLD1.Id CheckLotDetailId, CLD1.CheckNumber, CLH.CheckDate, p.UserName Party
						  -- , CL.LotNumber
				            , AddedBy =case when u.FullName<>'' then u.FullName else v.AddedBy end
                            ,PostedBy = case when up.FullName<>'' then up.FullName else v.PostedBy end

                            FROM [TRN].[BankJournal] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=BJ.CashMasterId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId

						    LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id -- AND VD.BankMasterId<>''
					        left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
							--left join trn.CheckLot CL on CL.Id=BM.Id
							left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD1 ON CLD1.Id=CLH.CheckLotDetailId
							left join [sec].[User] U on U.UserId=v.AddedBy
							left join sec.[User] up on up.UserId=v.PostedBy

                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

       // public DataTable GetCashJournalDetail(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
       // {
       //     var cmdText = @"SELECT V.Id, GLGI.Id AS AccountCodeId, GLGI.AccountCode, VD.Id AS VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
       //                     , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
       //                     , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
       //                     , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GLGI.UserName AS GL, GLGI.AccountCode AS GLGeneralInfoCode
       //                     , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
       //                     , VD.Id AS BudgetMasterId, B.UserName AS BudgetName, A.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType,BM.AccountTitle as AccountTitleName
							//,[ParticularName]=CASE
							//	WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
							//	WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
							//	WHEN P.UserName<>'' THEN P.UserName 
							//	WHEN CM.UserName<>'' THEN CM.UserName
							//	ELSE ''	END
       //                    FROM [TRN].[VoucherDetailCurrency] AS VDC
       //                     INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
       //                     INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
       //                     LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
       //                     LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
       //                     LEFT JOIN [TRN].[BankJournalDetail] AS BJD ON BJD.Id=VD.BankJournalDetailId
       //                     LEFT JOIN [TRN].[BankJournal] AS BJ ON BJ.Id=BJD.BankJournalId
       //                     LEFT JOIN [HKP].[FinancingType] AS BJDFT ON BJDFT.Id=BJD.FinancingTypeId
       //                     LEFT JOIN [TRN].[BankCharge] AS BC ON BC.Id=VD.BankChargeId
       //                     LEFT JOIN [HKP].[FinancingType] AS BCFT ON BCFT.Id=BC.FinancingTypeId
       //                     LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
       //                     LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
       //                     LEFT JOIN [HKP].[Budget] AS B ON B.Id=BGM.BudgetId
       //                     LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
       //                     LEFT JOIN [HKP].[Party] AS P ON P.Id=BJD.PartyId
							//LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
							//LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=BJ.EmployeeId
							// LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
       //                     LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
							// LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
       //                     LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
							//LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
       //                     WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "' ORDER BY VD.DrAmount DESC";
       //     return _sqlRepository.GetDataTable(cmdText);
       // }

        public DataTable GetCashJournalDetail(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            return GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, sourceType);
        }

        public List<Dictionary<string, object>> GetBankOpeningBalanceLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate)
        {
            var sql = @"SELECT SUM(DrAmount) - SUM(CrAmount) AS OB
                        , CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB
                        , CompanyGroupCurrencyId, SUM(CompanyGroupCurrencyDrAmount)-SUM(CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyOB
                        , HardCurrencyId, SUM(HardCurrencyDrAmount)-SUM(HardCurrencyCrAmount) AS HardCurrencyOB FROM (
                        SELECT SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.BankMasterId='" + bankMasterId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + @"'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        UNION
                        SELECT SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.BankMasterId='" + bankMasterId + "' AND V.PostingDate ='" + fromDate.ToDbDate() + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId, X.CompanyGroupCurrencyId, X.HardCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }

        public DataTable GetBankLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                    DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                    DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                    DECLARE @bankMasterId VARCHAR(10)='" + bankMasterId + @"';
                    SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                    GLT.DrAmount AS DrAmount,
                    GLT.CrAmount AS CrAmount
                    , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, V.Narration,v.AddedDate
                    ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.VoucherId=V.Id AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId !=VD.BankMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                    ,GLT.VoucherDetailId,case when isnull(GLT.ReconcileDate,'')<>'' then FORMAT(GLT.ReconcileDate,'dd-MMM-yyyy') else '' end ReconcileDate
					,case when isnull(GLT.ReconcileDate,'')<>'' then 'Yes' else 'No' end ReconciliationStatus
                    FROM [TRN].[GLTransactionDetail] AS GLT
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLT.VoucherDetailId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND VD.BankMasterId=@bankMasterId AND V.SourceType!='OpeningBalance' AND VD.LoanSetOffGroupNo IS NULL
                    AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"'
                    
                    UNION ALL
                    
                    SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                    GLT.DrAmount AS DrAmount,
                    GLT.CrAmount AS CrAmount
                    , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, V.Narration,v.AddedDate
                    ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.VoucherId=V.Id AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId !=VD.BankMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                    ,GLT.VoucherDetailId,case when isnull(GLT.ReconcileDate,'')<>'' then FORMAT(GLT.ReconcileDate,'dd-MMM-yyyy') else '' end ReconcileDate
					,case when isnull(GLT.ReconcileDate,'')<>'' then 'Yes' else 'No' end ReconciliationStatus
                    FROM [TRN].[GLTransactionDetail] AS GLT
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLT.VoucherDetailId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND VD.BankMasterId=@bankMasterId
                    AND V.SourceType='OpeningBalance' AND VD.LoanSetOffGroupNo IS NULL
                    AND V.PostingDate > '" + fromDate + @"'
                    UNION ALL
                    SELECT   
					VoucherNo=STUFF((select distinct ','+XV.VoucherNo from
					trn.VoucherDetail XVD 
					LEFT JOIN TRN.Voucher XV ON XVD.VoucherId=XV.Id
                    where  XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
					,V.PostingDate, V.CurrencyId, 
                    SUM(GLT.DrAmount) AS DrAmount,
                    SUM(GLT.CrAmount) AS CrAmount
                    ,SUM(CC.CompanyCurrencyDrAmount) CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount)CompanyCurrencyCrAmount , V.Narration,NULL AddedDate
                   
				   ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.BankMasterId !=VD.BankMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo  AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo  AND XVD.CashMasterId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo  AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                    ,NULL VoucherDetailId
					,NULL ReconcileDate--case when isnull(GLT.ReconcileDate,'')<>'' then FORMAT(GLT.ReconcileDate,'dd-MMM-yyyy') else '' end ReconcileDate
					,NULL ReconcileDate--case when isnull(GLT.ReconcileDate,'')<>'' then 'Yes' else 'No' end ReconciliationStatus
                    FROM 
					
					[TRN].[GLTransactionDetail] AS GLT
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLT.VoucherDetailId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND VD.BankMasterId=@bankMasterId AND V.SourceType!='OpeningBalance'
                    AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND VD.LoanSetOffGroupNo<>''
                    group by  V.PostingDate, V.CurrencyId, V.Narration,VD.LoanSetOffGroupNo,VD.BankMasterId
                    ORDER BY V.PostingDate,V.AddedDate, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }



    }
}
