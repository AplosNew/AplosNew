using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsAdvanceService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsAdvanceService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public GridModel EmployeeAdvanceQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, AM.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount+ISNULL(SAVW.SalaryWrittenOffAmount,0) AS Received, 0 DrAmount, 0 CrAmount
                                , AD.Amount-AD.WrittenOffAmount-ISNULL(SAVW.SalaryWrittenOffAmount,0)AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion,ETT.UserName EmployeeTransactionTypeName
                                ,CASE WHEN ETT.UserName='Employee Salary' THEN 'Salary' ELSE 'General' END JournalType
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AM.EmployeeId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
                                LEFT JOIN [HKP].[EmployeeTransactionType] AS ETT ON ETT.Id=AM.EmployeeTransactionTypeId
								LEFT JOIN (SELECT SUM(ARS.InstallmentAmount)SalaryWrittenOffAmount,ADV.Id AdvanceId
                                    FROM  [TRN].EmployeeAdvanceDeduction EAD 
                                    LEFT JOIN dbo.AdvanceReqSchedule  ARS ON EAD.AdvanceReqScheduleId=ARS.Id
                                    INNER JOIN [TRN].EmployeeSalaryAdvance ESA ON ESA.Id=ARS.EmployeeSalaryAdvanceId
                                    INNER JOIN [TRN].[Advance] ADV ON ADV.VoucherId=ESA.VoucherId
                                    LEFT JOIN DBO.SalaryLock SL ON SL.YearNo=ARS.YearNo AND SL.MonthNo=ARS.MonthNo AND SL.EmpSystemId=ESA.EmployeeId AND SL.PayableVoucherId IS NULL
									GROUP BY ADV.Id) SAVW ON SAVW.AdvanceId=AD.AdvanceId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType in ('EmployeeAdvance','InterTransaction')
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.EmployeeId<>'' ";
            return _sqlRepository.GetGridData(parameters);
        }
        public IEnumerable<object> EmployeeAvilabeAllAdvanceQuery(string column, string value, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var CmdText = @"SELECT TEMP.* FROM (SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, AM.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount+ISNULL(SAVW.SalaryWrittenOffAmount,0) AS Received, 0 DrAmount, 0 CrAmount
                                , AD.Amount-AD.WrittenOffAmount-ISNULL(SAVW.SalaryWrittenOffAmount,0)AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion,ISNULL(ETT.UserName,'Regular Expense') EmployeeTransactionTypeName
                                ,CASE WHEN ETT.UserName='Employee Salary' THEN 'Salary' ELSE 'General' END JournalType
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AM.EmployeeId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
                                LEFT JOIN [HKP].[EmployeeTransactionType] AS ETT ON ETT.Id=AM.EmployeeTransactionTypeId
								LEFT JOIN (SELECT SUM(ARS.InstallmentAmount)-ADV.WrittenOffAmount SalaryWrittenOffAmount,ADV.Id AdvanceId
                                    FROM  [TRN].EmployeeAdvanceDeduction EAD 
                                    LEFT JOIN dbo.AdvanceReqSchedule  ARS ON EAD.AdvanceReqScheduleId=ARS.Id
                                    INNER JOIN [TRN].EmployeeSalaryAdvance ESA ON ESA.Id=ARS.EmployeeSalaryAdvanceId
                                    INNER JOIN [TRN].[Advance] ADV ON ADV.VoucherId=ESA.VoucherId
                                    LEFT JOIN DBO.SalaryLock SL ON SL.YearNo=ARS.YearNo AND SL.MonthNo=ARS.MonthNo AND SL.EmpSystemId=ESA.EmployeeId AND SL.PayableVoucherId IS NULL
									GROUP BY ADV.Id,ADV.WrittenOffAmount) SAVW ON SAVW.AdvanceId=AD.AdvanceId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType in ('EmployeeAdvance','InterTransaction','FixedAssetDisposeJournal')
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.EmployeeId<>'' ) AS TEMP WHERE " + strkey + @"";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        public IEnumerable<object> EmployeeAdvanceSalaryQuery(string column, string value, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var CmdText = @"SELECT TEMP.* FROM (SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, AM.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount+ISNULL(SAVW.SalaryWrittenOffAmount,0) AS Received, 0 DrAmount, 0 CrAmount
                                , AD.Amount-AD.WrittenOffAmount-ISNULL(SAVW.SalaryWrittenOffAmount,0)AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion,ETT.UserName EmployeeTransactionTypeName
                                ,CASE WHEN ETT.UserName='Employee Salary' THEN 'Salary' ELSE 'General' END JournalType
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AM.EmployeeId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
                                LEFT JOIN [HKP].[EmployeeTransactionType] AS ETT ON ETT.Id=AM.EmployeeTransactionTypeId
								LEFT JOIN (SELECT SUM(ARS.InstallmentAmount)SalaryWrittenOffAmount,ADV.Id AdvanceId
                                    FROM  [TRN].EmployeeAdvanceDeduction EAD 
                                    LEFT JOIN dbo.AdvanceReqSchedule  ARS ON EAD.AdvanceReqScheduleId=ARS.Id
                                    INNER JOIN [TRN].EmployeeSalaryAdvance ESA ON ESA.Id=ARS.EmployeeSalaryAdvanceId
                                    INNER JOIN [TRN].[Advance] ADV ON ADV.VoucherId=ESA.VoucherId
                                    LEFT JOIN DBO.SalaryLock SL ON SL.YearNo=ARS.YearNo AND SL.MonthNo=ARS.MonthNo AND SL.EmpSystemId=ESA.EmployeeId AND SL.PayableVoucherId IS NULL
									GROUP BY ADV.Id) SAVW ON SAVW.AdvanceId=AD.AdvanceId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType in ('EmployeeAdvance','InterTransaction') and ETT.UserName='Employee Salary'
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.EmployeeId<>'') AS TEMP WHERE " + strkey + @"";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        public GridModel EmployeeTotalAdvanceQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT * FROM 
(SELECT AD.CompanyId, AD.PlantId,AD.CurrencyId, C.Code AS CurrencyCode, (select TOP 1 GLGeneralInfoId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId) GLGeneralInfoId, AD.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , DP.UserName Department,LD.UserName Designation
								,  (select TOP 1 BudgetMasterId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId)BudgetMasterId,  (select TOP 1 ActivityId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId)ActivityId
								, SUM(AD.Amount) AS Receivable, ISNULL((select SUM(Amount)WrittenOffAmount 
								from TRN.EmployeeSubsequentTransaction where SourceType='EmployeeAdvanceWriteOff' AND  EmployeeId=AD.EmployeeId AND ISNULL(JournalType,'')<>'Salary'),0)  AS Received
                                , SUM(AD.Amount)-ISNULL((select SUM(Amount)WrittenOffAmount 
								from TRN.EmployeeSubsequentTransaction where SourceType='EmployeeAdvanceWriteOff' AND  EmployeeId=AD.EmployeeId AND ISNULL(JournalType,'')<>'Salary'),0) AS Balance
                                FROM TRN.EmployeeSubsequentTransaction AS AD
                                INNER JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AD.EmployeeId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
								LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
								LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=EI.LegalDesignationId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AD.CurrencyId
                                WHERE  AD.CompanyGroupId='" + companyGroupId + @"' AND AD.CompanyId='" + companyId + @"' AND AD.PlantId='" + plantId + @"' AND AD.EmployeeId<>'' AND ISNULL(AD.AdvanceId,'') <>'' 
                                AND AD.SourceType in ('EmployeeAdvance', 'InterTransaction') AND ISNULL(AD.JournalType,'')<>'Salary'
                                GROUP BY AD.CompanyId, AD.PlantId, AD.CurrencyId, C.Code , AD.EmployeeId, EI.EmployeeCode, EI.EmployeeName, DP.UserName,LD.UserName)X
                                WHERE X.Balance > 0 ";
            
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel EmployeeTotalAdvanceByEmployeeIdQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType, string employeeId)
        {
            parameters.CmdText = @"SELECT * FROM 
(SELECT AD.CompanyId, AD.PlantId,AD.CurrencyId, C.Code AS CurrencyCode, (select TOP 1 GLGeneralInfoId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId) GLGeneralInfoId, AD.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , DP.UserName Department,DSG.UserName Designation
								,  (select TOP 1 BudgetMasterId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId)BudgetMasterId,  (select TOP 1 ActivityId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId)ActivityId
								, SUM(AD.Amount) AS Receivable, ISNULL((select SUM(Amount)WrittenOffAmount 
								from TRN.EmployeeSubsequentTransaction where SourceType='EmployeeAdvanceWriteOff' AND  EmployeeId=AD.EmployeeId AND ISNULL(JournalType,'')<>'Salary'),0)  AS Received
                                , SUM(AD.Amount)-ISNULL((select SUM(Amount)WrittenOffAmount 
								from TRN.EmployeeSubsequentTransaction where SourceType='EmployeeAdvanceWriteOff' AND  EmployeeId=AD.EmployeeId AND ISNULL(JournalType,'')<>'Salary'),0) AS Balance
                                FROM TRN.EmployeeSubsequentTransaction AS AD
                                INNER JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AD.EmployeeId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
								 LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
								 LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AD.CurrencyId
                                WHERE  AD.CompanyGroupId='" + companyGroupId + @"' AND AD.CompanyId='" + companyId + @"' AND AD.PlantId='" + plantId + @"' AND AD.EmployeeId='" + employeeId + @"' AND ISNULL(AD.AdvanceId,'') <>'' 
                                AND AD.SourceType in ('EmployeeAdvance', 'InterTransaction') AND ISNULL(AD.JournalType,'')<>'Salary'
                                GROUP BY AD.CompanyId, AD.PlantId, AD.CurrencyId, C.Code , AD.EmployeeId, EI.EmployeeCode, EI.EmployeeName, DP.UserName,DSG.UserName)X
                                WHERE X.Balance > 0 ";

            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetEmployeeAvilabeAdvanceByIdList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string employeeId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, AM.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received, 0 DrAmount, 0 CrAmount
                                , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AM.EmployeeId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='" + sourceType + @"'
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + @"' 
                                AND AM.EmployeeId='" + employeeId + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public IEnumerable<object> GetData(string Id)
        {
            try
            {
                var str = @"SELECT a.Id
                            	,FORMAT(a.InstallmentDate, 'dd-MMM-yyyy') InstallmentDate
                            	,a.InstallmentNo
                            	,a.InstallmentAmount
                            	,a.ProfitAmount
                            	,a.PrincipalAmount
                            	,a.Balance
                            	,a.EmployeeSalaryAdvanceId
                            	,a.YearNo
                            	,a.MonthNo
                            	,a.ScheduleNo
                            	,a.Arrear,t.Id EmployeeAdvanceDeductionId
                                ,CASE WHEN t.Id IS NULL THEN 'Yes' ELSE 'No' END SaveOrNot
                            FROM AdvanceReqSchedule a
                            LEFT JOIN TRN.EmployeeSalaryAdvance e ON e.Id = a.EmployeeSalaryAdvanceId
                            LEFT JOIN trn.Advance AS ad ON ad.VoucherId = e.VoucherId
                            LEFT JOIN [TRN].[EmployeeAdvanceDeduction] t ON t.AdvanceReqScheduleId = a.Id
                            WHERE ad.Id = '" + Id + "' ORDER BY a.InstallmentNo";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public GridModel GetEmployeeWiseOutstandingAdvance(GridParameter parameters, string companyGroupId, string companyId, string plantId, string employeeId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType,   AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId,  C.Code AS CurrencyCode
								,  V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate
								, AM.DocRefNo, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received , AD.Amount-AD.WrittenOffAmount AS Balance
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='" + sourceType + @"'
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AD.EmployeeId='" + employeeId + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetEmployeeAdvanceRequisition(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo,AdvanceType=CASE WHEN EAR.AdvanceType<>'' THEN EAR.AdvanceType ELSE EARII.AdvanceType END 
								, Id=CASE WHEN A.Id<>'' THEN A.Id ELSE ESA.Id END
								, A.Id As AdvanceId,BC.Id AS BankChargeId, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId, PP.UserName AS PartyPlantName
								,EmployeeId=CASE WHEN  A.EmployeeId<>'' THEN A.EmployeeId ELSE ESA.EmployeeId END
								, EmployeeCode=CASE WHEN  EI.EmployeeCode<>'' THEN EI.EmployeeCode ELSE EIAS.EmployeeCode END
                                 , EmployeeName=CASE WHEN  EI.EmployeeName<>'' THEN EI.EmployeeName ELSE EIAS.EmployeeName END
								 , EIR.EmployeeCode AS ResponsibleCode,EIR.EmployeeName AS ResponsibleName
								 , V.Id VoucherId, V.PostingDate, V.DocDate, V.DocRefNo
                                 , V.CurrencyId, C.Code AS CurrencyCode
								 , Amount=CASE WHEN A.Amount>0 THEN A.Amount ELSE ESA.Amount END, A.IsWrittenOff, A.WrittenOffAmount, v.IsPark, A.IsInterTransaction, A.IsPosted, AD.NetAmount
                                 ,EEC. EmployeeName CheckedBy ,EEA. EmployeeName ApprovedBy         
                                 FROM [TRN].[Voucher] AS V 
                                 LEFT JOIN [TRN].[Advance] AS A ON V.Id=A.VoucherId AND A.OpeningBalanceId IS NULL
								  LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                 LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=A.EmployeeId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=A.ResponsiblePersonId
                                 LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                                 LEFT JOIN [TRN].[BankCharge] AS BC ON BC.AdvanceId=A.Id
								 LEFT JOIN [TRN].[EmployeeSalaryAdvance] AS ESA ON ESA.VoucherId=V.Id
                                 LEFT JOIN [TRN].[EmployeeAdvanceRequisition] EAR ON EAR.SystemId=A.RequisitionId
                                 LEFT JOIN [TRN].[EmployeeAdvanceRequisition] EARII ON EARII.SystemId=ESA.EmployeeAdvanceRequisitionId
								 LEFT JOIN [dbo].[EmployeeInformation] AS EIAS ON EIAS.SystemId=ESA.EmployeeId
								 LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
								LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                                LEFT JOIN (SELECT AdvanceId, PartyId, NetAmount FROM [TRN].[AdvanceDetail]
                                ) AS AD ON AD.AdvanceId=A.Id AND AD.PartyId=A.PartyId
                                WHERE  V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "'AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetAdvance(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId,P.Code AS  PartyCode, P.UserName As PartyName, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received
                                , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='" + sourceType + @"'
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "'  ";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetAvilabeCustomerAdvanceList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AD.PaymentType, AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received
                                , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion,CON.Id ContractId,CON.ContractNo
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN dbo.Contract CON on CON.Id=AM.ContractId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType IN ('CustomerAdvance', 'CustomerReceipt')
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetAvilabeCustomerAdvanceByCustomerList(GridParameter parameters, string CustomerId, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AD.PaymentType, AD.AdvanceId, AD.Id AS AdvanceDetailId,AM.AdvanceGroupNo, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received
                                , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType IN ('CustomerAdvance', 'CustomerReceipt')
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.PartyId='"+CustomerId+"'";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetAvilabeCustomerInterTransactionList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType, PartyType partyType)
        {
            parameters.CmdText = @"SELECT AM.AdvanceNo, AM.Id AS AdvanceId, AD.Id AS AdvanceDetailId, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode
                                , AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                                , AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate, P.Code AS PartyCode,P.UserName AS PartyName ,AM.PartyPlantId, PP.UserName AS PartyPlantName
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration,  AM.PartyId, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received
                                , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.IsPark=0 AND AM.Archive=0 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='" + sourceType + @"' AND AD.PartyType='" + partyType + @"' AND AM.IsPosted=0
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AD.CompanyId='" + companyId + "' AND AD.PlantId='" + plantId + "' ";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetInterTransactionList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT distinct V.VoucherNo, A.Id, A.Id As AdvanceId, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId, PP.UserName AS PartyPlantName, A.EmployeeId, EI.EmployeeCode
                                 , EI.EmployeeName, EIR.EmployeeCode AS ResponsibleCode,EIR.EmployeeName AS ResponsibleName, A.VoucherId, A.PostingDate, A.DocDate, A.DocRefNo
                                 , A.CurrencyId, C.Code AS CurrencyCode, A.Amount, A.IsWrittenOff, A.WrittenOffAmount, A.IsPark, A.IsInterTransaction, A.IsPosted
                                 ,A.JournalType,A.SettlementType,A.PaymentSource,A.FinancingTypeId,VD.PlantId,VD.PlantName,CO.UserName CompanyName,VD.CompanyId,V.EntityId,V.VoucherTypeId,V.Narration,VAD.AdjustmentNoteId          
                                 FROM [TRN].[Advance] AS A
                                 LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                 LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=A.EmployeeId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=A.ResponsiblePersonId
                                 LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                 LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
								 LEFT JOIN (Select VD.AdjustmentNoteDetailId, VD.VoucherId,AD.AdjustmentNoteId from TRN.VoucherDetail VD
								 LEFT JOIN TRN.InvoiceWriteOffDetail AD ON AD.Id=VD.InvoiceWriteOffDetailId 
								  Where VD.InvoiceWriteOffDetailId<> '') VAD ON VAD.VoucherId=V.Id 
                                 LEFT JOIN(select VV.PlantId,VV.VoucherId,PL.CompanyId,PL.UserName PlantName from  [TRN].[VoucherDetail] VV
								 LEFT JOIN ORG.Plant PL ON PL.Id=VV.PlantId where PlantId<>'') AS VD ON VD.VoucherId=A.VoucherId
								 LEFT JOIN ORG.Company CO ON CO.Id=VD.CompanyId
                                 LEFT JOIN [TRN].[BankCharge] AS BC ON BC.AdvanceId=A.Id
                                WHERE A.OpeningBalanceId IS NULL AND A.Archive=0 AND V.Archive=0 AND A.CompanyGroupId='" + companyGroupId + "'AND A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' AND A.SourceType='" + SourceType.InterTransaction + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public IEnumerable<object> GetAvailableAdvanceByVendor(string companyGroupId, string companyId, string plantId, SourceType sourceType,string vendorId)
        {
            var sql = @"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId,P.Code AS  PartyCode, P.UserName As PartyName, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount+AD.AdditionalAmount AS Receivable, AD.WrittenOffAmount AS Received
                                , AD.Amount+AD.AdditionalAmount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion 
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='" + sourceType + @"'
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.PartyId='"+ vendorId + @"'  ";
            return _sqlRepository.GetDataCollection(sql);
        }

        public GridModel GetEmployeeAdvanceHRList(GridParameter parameters, SourceType sourceType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT V.VoucherNo, A.Id, EAD.Id As EmployeeAdvanceAdvanceId--,BC.Id AS BankChargeId
                                ,  EAD.EmpSystemId EmployeeId, EI.EmployeeCode
                                 , EI.EmployeeName, EIR.EmployeeCode AS ResponsibleCode,EIR.EmployeeName AS ResponsibleName, EAD.VoucherId, V.PostingDate, V.DocDate, V.DocRefNo
                                 , A.CurrencyId, C.Code AS CurrencyCode, EAD.AdvanceAmount Amount, EAD.IsWrittenOff, EAD.WrittenOffAmount, V.IsPark
                                 , Status = case when V.IsPark = 0 then 'Posted' else 'Parked' end,A.RequisitionId,EAR.AdvanceType,CB.EmployeeName CheckedBy,AP.EmployeeName ApprovedBy
                                 FROM [TRN].[EmployeeAdvance] AS A
								 LEFT JOIN  [TRN].[EmployeeAdvanceDetail] EAD ON EAD.EmployeeAdvanceId=A.Id
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EAD.EmpSystemId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=A.ResponsiblePersonId
								 LEFT JOIN [TRN].[EmployeeAdvanceRequisition] EAR ON EAR.SystemId=A.RequisitionId
								 LEFT JOIN [dbo].[EmployeeInformation] AS CB ON CB.SystemId=EAR.CheckedBy
                                 LEFT JOIN [dbo].[EmployeeInformation] AS AP ON AP.SystemId=EAR.ApprovedBy
                                 LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                 LEFT JOIN [TRN].[Voucher] AS V ON V.Id=EAD.VoucherId
                                    WHERE  V.Archive=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + "'AND V.CompanyId='" + identity.CompanyId + "' AND V.PlantId='" + identity.PlantId + "' AND V.SourceType='" + SourceType.EmployeeAdvance + "' AND A.FromDate IS NULL";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex);
            }
        }
    }
}
