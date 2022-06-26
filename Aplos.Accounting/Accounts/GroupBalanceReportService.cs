using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Service.ChartOfAccounts;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.ManagementChartOfAccounts;
using Library.Service.Organizations;
using Library.Service.Properties;
using Library.Service.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class GroupBalanceReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IGLGeneralInfoService _gLGeneralInfoService;
        private readonly IBudgetMasterService _budgetMasterService;
       
        public GroupBalanceReportService(ISqlRepository sqlRepository
            , IGLGeneralInfoService gLGeneralInfoService
            , IBudgetMasterService budgetMasterService
            )
        {
            _sqlRepository = sqlRepository;
            _gLGeneralInfoService = gLGeneralInfoService;
            _budgetMasterService = budgetMasterService;
        }
        public DataTable GetGeneralLedgerData(string companyGroupId, string companyId, string plantId, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool isOpeningBalance, string fiscalYearId)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, VD.CurrencyId
							, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName,p.username as Party
                            ,Particular =concat( STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.UserName+')' from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
                                                        JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												
                                                    ,STUFF((select distinct ','+xp.AccountTitle from
														TRN.VoucherDetail XVD JOIN MST.BankMaster AS XP ON XP.Id=XVD.BankMasterId
													where	XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN MST.CashMaster AS XP ON XP.Id=XVD.CashMasterId
													where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 ,STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.EmployeeName+')' from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
														JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                        ,NULL PartyName
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "'";
            if (!string.IsNullOrEmpty(budgetMasterId))
                cmdText += " AND VD.BudgetMasterId='" + budgetMasterId + "' ";
            if (!string.IsNullOrEmpty(activityId))
                cmdText += " AND VD.ActivityId='" + activityId + "' ";
            cmdText += isOpeningBalance ? " AND V.SourceType='OpeningBalance' AND V.FiscalYearId='" + fiscalYearId + "' AND VD.GLGeneralInfoId IS NOT NULL" : " AND VD.GLGeneralInfoId='" + glId + "' AND V.SourceType!='OpeningBalance' AND CONVERT(VARCHAR, V.PostingDate, 23) BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate.ToDbDate() + "'";
            cmdText += " ORDER BY V.PostingDate ASC, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        private string GLSql(string glId, string budgetMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"DECLARE @companyId VARCHAR(10)='" + identity.CompanyId + @"';
                            SELECT 
                             SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0)) AS CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0)) AS CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName 
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)BudgetOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId

                        UNION

                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)BudgetClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'   AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType!='OpeningBalance' AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' 
							GROUP BY GLGI.AccountCode,GLGI.UserName,  BG.UserName,BGM.Id

union

							SELECT * FROM 
							  (SELECT 
                              0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName 
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId+ @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)BudgetOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)BudgetClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'   AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType!='OpeningBalance' AND V.PostingDate < '" + fromDate + @"'
							AND BGM.Id NOT IN(SELECT  VDO.BudgetMasterId 
							 FROM [TRN].[VoucherDetail] AS VDO
                            LEFT JOIN [TRN].[Voucher] VO ON VO.Id=VDO.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VDO.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VDO.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            WHERE VO.Archive=0 AND VO.IsPark=0 AND VO.CompanyGroupId='" + identity.CompanyGroupId + @"' AND VO.CompanyId='" + identity.CompanyId + @"' AND VO.PlantId='" + identity.PlantId + @"'  AND VDO.GLGeneralInfoId='" + glId + @"' AND VO.SourceType!='OpeningBalance' AND   VO.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' )
							GROUP BY GLGI.AccountCode,GLGI.UserName,  BG.UserName,BGM.Id)T
							WHERE T.BudgetOpeningBalance<>0
union

							SELECT * FROM 
							  (SELECT 
                              0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName 
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)BudgetOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)BudgetClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'   AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType='OpeningBalance' --AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
							AND BGM.Id NOT IN(SELECT  VDO.BudgetMasterId 
							 FROM [TRN].[VoucherDetail] AS VDO
                            LEFT JOIN [TRN].[Voucher] VO ON VO.Id=VDO.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VDO.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VDO.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            WHERE VO.Archive=0 AND VO.IsPark=0 AND VO.CompanyGroupId='" + identity.CompanyGroupId + @"' AND VO.CompanyId='" + identity.CompanyId + @"' AND VO.PlantId='" + identity.PlantId + @"'  AND VDO.GLGeneralInfoId='" + glId + @"' AND VO.SourceType!='OpeningBalance' ) --AND   VO.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' )
							GROUP BY GLGI.AccountCode,GLGI.UserName,  BG.UserName,BGM.Id)T ";
        }

        private string BudgetSql(string glId, string budgetMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"DECLARE @companyId VARCHAR(10)='" + identity.CompanyId + @"';
                            SELECT 
                             SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0)) AS CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0)) AS CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName ,A.Id ActivityID, A.UserName AS ActivityName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" +glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.BudgetMasterId='" +budgetMasterId + @"'  AND VD.GLGeneralInfoId='" +glId + @"' AND V.SourceType!='OpeningBalance' AND   V.PostingDate BETWEEN '" + fromDate + @"' AND '" +toDate + @"' 
							GROUP BY GLGI.AccountCode,GLGI.UserName,  BG.UserName,A.Id, A.UserName


	union
	SELECT * FROM 
							  (SELECT 
                             0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName ,A.Id ActivityID, A.UserName AS ActivityName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" +budgetMasterId+ @"'  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'   AND VD.ActivityId=A.Id  AND V.PostingDate <='" +fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType!='OpeningBalance' AND   V.PostingDate < '" + toDate + @"'
							AND A.Id NOT IN(SELECT  VDO.ActivityId 
							 FROM [TRN].[VoucherDetail] AS VDO
                            LEFT JOIN [TRN].[Voucher] VO ON VO.Id=VDO.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VDO.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VDO.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VDO.ActivityId
                            WHERE VO.Archive=0 AND VO.IsPark=0 AND VO.CompanyGroupId='" + identity.CompanyGroupId + @"' AND VO.CompanyId='" + identity.CompanyId + @"' AND VO.PlantId='" + identity.PlantId + @"' AND VDO.BudgetMasterId='" + budgetMasterId + @"'  AND VDO.GLGeneralInfoId='" + glId + @"' AND VO.SourceType!='OpeningBalance' AND   VO.PostingDate BETWEEN '" + fromDate+ @"' AND '" + toDate + @"' )
							GROUP BY GLGI.AccountCode,GLGI.UserName,  BG.UserName,A.Id, A.UserName)T
							WHERE T.ActivityOpeningBalance<>0
union
	SELECT * FROM 
							  (SELECT 
                             0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName ,A.Id ActivityID, A.UserName AS ActivityName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType='OpeningBalance' --AND   V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
							AND A.Id NOT IN(SELECT  VDO.ActivityId 
							 FROM [TRN].[VoucherDetail] AS VDO
                            LEFT JOIN [TRN].[Voucher] VO ON VO.Id=VDO.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VDO.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VDO.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VDO.ActivityId
                            WHERE VO.Archive=0 AND VO.IsPark=0 AND VO.CompanyGroupId='" + identity.CompanyGroupId + @"' AND VO.CompanyId='" + identity.CompanyId + @"' AND VO.PlantId='" + identity.PlantId + @"' AND VDO.BudgetMasterId='" + budgetMasterId + @"'  AND VDO.GLGeneralInfoId='" + glId + @"' AND VO.SourceType!='OpeningBalance' ) --AND   VO.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' )
							GROUP BY GLGI.AccountCode,GLGI.UserName,  BG.UserName,A.Id, A.UserName)T ";
        }
        private string BudgetActivitySql(string glId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"DECLARE @companyId VARCHAR(10)='" + identity.CompanyId + @"';
                            SELECT 
                             SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0)) AS CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0)) AS CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName ,A.Id ActivityID, A.UserName AS ActivityName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND VD.ActivityId=A.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'   AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType!='OpeningBalance' AND   V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' 
							GROUP BY GLGI.AccountCode,GLGI.UserName,BGM.Id,  BG.UserName,A.Id, A.UserName


	union
	SELECT * FROM 
							  (SELECT 
                             0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName ,A.Id ActivityID, A.UserName AS ActivityName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND VD.ActivityId=A.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND VD.ActivityId=A.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'   AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType!='OpeningBalance' AND   V.PostingDate < '" + toDate + @"'
							AND A.Id NOT IN(SELECT  VDO.ActivityId 
							 FROM [TRN].[VoucherDetail] AS VDO
                            LEFT JOIN [TRN].[Voucher] VO ON VO.Id=VDO.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VDO.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VDO.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VDO.ActivityId
                            WHERE VO.Archive=0 AND VO.IsPark=0 AND VO.CompanyGroupId='" + identity.CompanyGroupId + @"' AND VO.CompanyId='" + identity.CompanyId + @"' AND VO.PlantId='" + identity.PlantId + @"'   AND VDO.GLGeneralInfoId='" + glId + @"' AND VO.SourceType!='OpeningBalance' AND   VO.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' )
							GROUP BY GLGI.AccountCode,GLGI.UserName,BGM.Id,  BG.UserName,A.Id, A.UserName)T
							WHERE T.ActivityOpeningBalance<>0
union
	SELECT * FROM 
							  (SELECT 
                             0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,  BG.UserName AS BudgetName ,A.Id ActivityID, A.UserName AS ActivityName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND VD.ActivityId=A.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)ActivityClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'   AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType='OpeningBalance' --AND   V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
							AND A.Id NOT IN(SELECT  VDO.ActivityId 
							 FROM [TRN].[VoucherDetail] AS VDO
                            LEFT JOIN [TRN].[Voucher] VO ON VO.Id=VDO.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VDO.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VDO.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VDO.ActivityId
                            WHERE VO.Archive=0 AND VO.IsPark=0 AND VO.CompanyGroupId='" + identity.CompanyGroupId + @"' AND VO.CompanyId='" + identity.CompanyId + @"' AND VO.PlantId='" + identity.PlantId + @"'   AND VDO.GLGeneralInfoId='" + glId + @"' AND VO.SourceType!='OpeningBalance' ) --AND   VO.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' )
							GROUP BY GLGI.AccountCode,GLGI.UserName,BGM.Id,  BG.UserName,A.Id, A.UserName)T ORDER BY BudgetName";
        }

        public IWorkbook GetGeneralLedgerGroupReport(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string fromDate, string toDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                string sql = GLSql(glId, budgetMasterId, fromDate, toDate);
                string Budgetsql = BudgetSql(glId, budgetMasterId, fromDate, toDate);
                var gl = _gLGeneralInfoService.GetGLData(glId);
                var budget = _budgetMasterService.GetBudgetMasterData(budgetMasterId);

                //Instantiate the Excel application object
                DataTable dtGroupBalance = _sqlRepository.GetDataTable(sql);
                DataTable dtGroupBalanceBudgets = _sqlRepository.GetDataTable(Budgetsql);
                if (dtGroupBalance.Rows.Count == 0)
                    throw new Exception("No data found");

                var dtGroupBalanceBudget = dtGroupBalanceBudgets.AsEnumerable()
                        .OrderBy(r => r["ActivityName"])
                        .CopyToDataTable();

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Group Balance Report";


                int ROW = 6;
                int COL = 1;

                #region Header
                sheet[ROW, COL].Text = "Account Type :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;
                sheet[ROW, COL].Text = gl["AccountTypeName"].ToString();
                sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                int colAccountType = COL;
                COL += 2;

                sheet[ROW, COL].Text = "Account Group :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                //sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                COL++;
                sheet[ROW, COL].Text = gl["AccountGroupName"].ToString();
                sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 2) + ROW].Merge();
                int colAccountGroup = COL;
                ROW++;
                COL = 1;
                sheet[ROW, COL].Text = "GL:";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                sheet[ROW, COL].Text = gl["GLGeneralInfoCode"] + " - " + gl["GLGeneralInfoName"];
                sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                int colGL = COL;

                if (budgetMasterId != null)
                {
                    COL += 2;
                    sheet[ROW, COL].Text = "Budget :";
                    sheet[ROW, COL].CellStyle.Font.Bold = true;

                    COL++;
                    sheet[ROW, COL].Text = budget["UserName"].ToString();
                    sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 2) + ROW].Merge();

                }

                ROW++;
                COL = 1;
                #endregion
                int colActivity = 0;
                int colBudget = 0;
                if (budgetMasterId != null)
                {
                    sheet[ROW, COL].Text = "Activity";
                    sheet[ROW, COL].ColumnWidth = 20;
                    colActivity = COL;
                    COL++;
                }
                else
                {
                    sheet[ROW, COL].Text = "Budget";
                    sheet[ROW, COL].ColumnWidth = 20;
                    colBudget = COL;
                    COL++;
                }

                sheet[ROW, COL].Text = "Openning Balance";
                sheet[ROW, COL].ColumnWidth = 18;
                int colOpenningCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Dr/Cr";
                sheet[ROW, COL].ColumnWidth = 5;
                int colOpenningDRCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Periodic Dr.";
                sheet[ROW, COL].ColumnWidth = 18;
                int colPeriodicDr = COL;
                COL++;
                sheet[ROW, COL].Text = "Periodic Cr.";
                sheet[ROW, COL].ColumnWidth = 18;
                int colPeriodicCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].ColumnWidth = 18;
                int colBalanceDrCr = COL;
                COL++;
                sheet[ROW, COL].Text = "Dr/Cr";
                sheet[ROW, COL].ColumnWidth = 5;
                int colCRDR = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                if (budgetMasterId!=null)
                {
                    for (int i = 0; i < dtGroupBalanceBudget.Rows.Count; i++)
                    {

                        sheet[ROW, colActivity].Text = dtGroupBalanceBudget.Rows[i]["ActivityName"].ToString();
                        sheet[ROW, colOpenningCR].Number = clsStaticInfo.dbl(dtGroupBalanceBudget.Rows[i]["ActivityOpeningBalance"].ToString());
                        sheet.Range[ROW, colOpenningCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet[ROW, colPeriodicDr].Number = clsStaticInfo.dbl(dtGroupBalanceBudget.Rows[i]["CompanyCurrencyDrAmount"].ToString());
                        sheet[ROW, colPeriodicDr].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colPeriodicCR].Number = clsStaticInfo.dbl(dtGroupBalanceBudget.Rows[i]["CompanyCurrencyCrAmount"].ToString());
                        sheet[ROW, colPeriodicCR].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colBalanceDrCr].Number = clsStaticInfo.dbl(dtGroupBalanceBudget.Rows[i]["ActivityClosingBalance"].ToString());
                        sheet.Range[ROW, colBalanceDrCr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        if (Convert.ToInt32(dtGroupBalanceBudget.Rows[i]["ActivityClosingBalance"]) != 0)
                        {
                            sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                            sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        }
                        if (Convert.ToInt32(dtGroupBalanceBudget.Rows[i]["ActivityOpeningBalance"]) != 0)
                        {
                            sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                            sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        }

                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                        ROW++;

                    }
                }
                else
                {
                    for (int i = 0; i < dtGroupBalance.Rows.Count; i++)
                    {

                        sheet[ROW, colBudget].Text = dtGroupBalance.Rows[i]["BudgetName"].ToString();
                        sheet[ROW, colOpenningCR].Number = clsStaticInfo.dbl(dtGroupBalance.Rows[i]["BudgetOpeningBalance"].ToString());
                        sheet.Range[ROW, colOpenningCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                        sheet[ROW, colPeriodicDr].Number = clsStaticInfo.dbl(dtGroupBalance.Rows[i]["CompanyCurrencyDrAmount"].ToString());
                        sheet[ROW, colPeriodicDr].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colPeriodicCR].Number = clsStaticInfo.dbl(dtGroupBalance.Rows[i]["CompanyCurrencyCrAmount"].ToString());
                        sheet[ROW, colPeriodicCR].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colBalanceDrCr].Number = Convert.ToDouble(dtGroupBalance.Rows[i]["BudgetClosingBalance"].ToString());
                        sheet.Range[ROW, colBalanceDrCr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        if (Convert.ToInt32(dtGroupBalance.Rows[i]["BudgetOpeningBalance"]) != 0 )
                        {
                            sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                            sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        }
                        if (Convert.ToInt32(dtGroupBalance.Rows[i]["BudgetClosingBalance"]) != 0)
                        {
                            sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                            sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        }

                    
                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                        ROW++;

                    }
                }
                sheet[ROW, 1].Text = "Total :";
                sheet[ROW, colOpenningCR].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colOpenningCR) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colOpenningCR) + (ROW-1)+")";
                sheet.Range[ROW, colOpenningCR].CellStyle.Font.Bold = true;
                //sheet[ROW, colOpenningCR].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet.Range[ROW, colOpenningCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();


                sheet[ROW, colPeriodicDr].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colPeriodicDr) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colPeriodicDr) + (ROW-1)+")";
                sheet.Range[ROW, colPeriodicDr].CellStyle.Font.Bold = true;
                //sheet[ROW, colPeriodicDr].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet.Range[ROW, colPeriodicDr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();


                sheet[ROW, colBalanceDrCr].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalanceDrCr) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colBalanceDrCr) + (ROW-1)+")";
                sheet.Range[ROW, colBalanceDrCr].CellStyle.Font.Bold = true;
                //sheet[ROW, colBalanceDrCr].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet.Range[ROW, colBalanceDrCr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
              //  " IF(B125 > 0, "Dr", IF(B125 < 0, "Cr", IF(B125 = 0, "")));
                sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "> 0, \"Dr\",IF("+ reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "< 0,\"Cr\",IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "= 0 ,\" \")))";
                sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "> 0, \"Dr\", \"Cr\")";
                //sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "> 0, \"Dr\", \"Cr\")";
                sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "> 0, \"Dr\",IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "< 0,\"Cr\",IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "= 0 ,\" \")))";
sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;


                sheet[ROW, colPeriodicCR].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colPeriodicCR) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colPeriodicCR) + (ROW - 1) + ")";
                sheet.Range[ROW, colPeriodicCR].CellStyle.Font.Bold = true;
                //sheet[ROW, colPeriodicCR].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet.Range[ROW, colPeriodicCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();



                //sheet.Range[StartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colPOValue, ROW, colPOValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colAcceptanceValue, ROW, colAcceptanceValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[StartRow, colGRNValue, ROW, colGRNValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                //reportUtility.PlantHeader(ref sheet, endCol, "Group Balance", identity.PlantId);
                reportUtility.CompanyPlantHeader(ref sheet, endCol, "Group Balance", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, 1, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                //sheet.Range[ROW, COL, ROW, endCol].CellStyle.Font.Bold = true;
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Group Balance Report.xls";
                //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //workbook.Close();
                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetGeneralLedgerGroupWithBudgetActivityReport(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string fromDate, string toDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                //string sql = GLSql(glId, budgetMasterId, fromDate, toDate);
                string Budgetsql = BudgetActivitySql(glId, fromDate, toDate);
                var gl = _gLGeneralInfoService.GetGLData(glId);
                var budget = _budgetMasterService.GetBudgetMasterData(budgetMasterId);

                //Instantiate the Excel application object
                //DataTable dtGroupBalance = _sqlRepository.GetDataTable(sql);
                DataTable dtGroupBalanceBudget = _sqlRepository.GetDataTable(Budgetsql);
                //if (dtGroupBalance.Rows.Count == 0)
                //    throw new Exception("No data found");

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Group Balance Report";


                int ROW = 6;
                int COL = 1;

                #region Header
                sheet[ROW, COL].Text = "Account Type :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;
                sheet[ROW, COL].Text = gl["AccountTypeName"].ToString();
                sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                int colAccountType = COL;
                COL += 3;

                sheet[ROW, COL].Text = "Account Group :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                //sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                COL++;
                sheet[ROW, COL].Text = gl["AccountGroupName"].ToString();
                sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 3) + ROW].Merge();
                int colAccountGroup = COL;
                ROW++;
                COL = 1;
                sheet[ROW, COL].Text = "GL:";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                sheet[ROW, COL].Text = gl["GLGeneralInfoCode"] + " - " + gl["GLGeneralInfoName"];
                sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                int colGL = COL;
                if (budgetMasterId != null)
                {
                    COL += 2;
                    sheet[ROW, COL].Text = "Budget :";
                    sheet[ROW, COL].CellStyle.Font.Bold = true;

                    COL++;
                    sheet[ROW, COL].Text = budget["UserName"].ToString();
                    sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 2) + ROW].Merge();

                }

                ROW++;
                COL = 1;
                #endregion
                int colActivity = 0;
                int colBudget = 0;

                sheet[ROW, COL].Text = "Budget";
                sheet[ROW, COL].ColumnWidth = 22;
                colBudget = COL;
                COL++;

                sheet[ROW, COL].Text = "Activity";
                sheet[ROW, COL].ColumnWidth = 28;
                colActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "Openning Balance";
                sheet[ROW, COL].ColumnWidth = 18;
                int colOpenningCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Dr/Cr";
                sheet[ROW, COL].ColumnWidth = 5;
                int colOpenningDRCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Periodic Dr.";
                sheet[ROW, COL].ColumnWidth = 18;
                int colPeriodicDr = COL;
                COL++;
                sheet[ROW, COL].Text = "Periodic Cr.";
                sheet[ROW, COL].ColumnWidth = 18;
                int colPeriodicCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].ColumnWidth = 18;
                int colBalanceDrCr = COL;
                COL++;
                sheet[ROW, COL].Text = "Dr/Cr";
                sheet[ROW, COL].ColumnWidth = 5;
                int colCRDR = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                
                int StartRow = ROW; //row 20
                
                var dt = dtGroupBalanceBudget.AsEnumerable().OrderBy(r => r["BudgetName"])
                            .GroupBy(r => new { BudgetName = r["BudgetName"] })
                            .Select(g => g.OrderBy(r => r["BudgetName"]).First())
                            .CopyToDataTable();
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    var data = dtGroupBalanceBudget.AsEnumerable()
                        .Where(r => r.Field<string>("BudgetName") == dt.Rows[j]["BudgetName"].ToString())
                        .OrderBy(r => r["ActivityName"])
                        .CopyToDataTable();
                    StartRow = ROW;
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        sheet[ROW, colBudget].Text = data.Rows[i]["BudgetName"].ToString();
                        sheet[ROW, colActivity].Text = data.Rows[i]["ActivityName"].ToString();
                        sheet[ROW, colOpenningCR].Number = clsStaticInfo.dbl(data.Rows[i]["ActivityOpeningBalance"].ToString());
                        sheet.Range[ROW, colOpenningCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet[ROW, colPeriodicDr].Number = clsStaticInfo.dbl(data.Rows[i]["CompanyCurrencyDrAmount"].ToString());
                        sheet[ROW, colPeriodicDr].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colPeriodicCR].Number = clsStaticInfo.dbl(data.Rows[i]["CompanyCurrencyCrAmount"].ToString());
                        sheet[ROW, colPeriodicCR].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colBalanceDrCr].Number = clsStaticInfo.dbl(data.Rows[i]["ActivityClosingBalance"].ToString());
                        sheet.Range[ROW, colBalanceDrCr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        if (Convert.ToInt32(data.Rows[i]["ActivityClosingBalance"]) != 0)
                        {
                            sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                            sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        }
                        if (Convert.ToInt32(data.Rows[i]["ActivityOpeningBalance"]) != 0)
                        {
                            sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                            sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        }

                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        ROW++;

                    }
                    sheet[ROW, 1].Text = "Total :";
                    sheet[ROW, colOpenningCR].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colOpenningCR) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colOpenningCR) + (ROW - 1) + ")";
                    sheet.Range[ROW, colOpenningCR].CellStyle.Font.Bold = true;
                    //sheet[ROW, colOpenningCR].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet.Range[ROW, colOpenningCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();


                    sheet[ROW, colPeriodicDr].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colPeriodicDr) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colPeriodicDr) + (ROW - 1) + ")";
                    sheet.Range[ROW, colPeriodicDr].CellStyle.Font.Bold = true;
                    //sheet[ROW, colPeriodicDr].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet.Range[ROW, colPeriodicDr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();


                    sheet[ROW, colBalanceDrCr].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalanceDrCr) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colBalanceDrCr) + (ROW - 1) + ")";
                    sheet.Range[ROW, colBalanceDrCr].CellStyle.Font.Bold = true;
                    //sheet[ROW, colBalanceDrCr].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet.Range[ROW, colBalanceDrCr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //  " IF(B125 > 0, "Dr", IF(B125 < 0, "Cr", IF(B125 = 0, "")));
                    sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "> 0, \"Dr\",IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "< 0,\"Cr\",IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "= 0 ,\" \")))";
                    sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "> 0, \"Dr\", \"Cr\")";
                    //sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    //sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "> 0, \"Dr\", \"Cr\")";
                    sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "> 0, \"Dr\",IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "< 0,\"Cr\",IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "= 0 ,\" \")))";
                    sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;


                    sheet[ROW, colPeriodicCR].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colPeriodicCR) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colPeriodicCR) + (ROW - 1) + ")";
                    sheet.Range[ROW, colPeriodicCR].CellStyle.Font.Bold = true;
                    //sheet[ROW, colPeriodicCR].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet.Range[ROW, colPeriodicCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                    ROW++;
                }

                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                //reportUtility.PlantHeader(ref sheet, endCol, "Group Balance", identity.PlantId);
                reportUtility.CompanyPlantHeader(ref sheet, endCol, "Group Balance", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, 1, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                //sheet.Range[ROW, COL, ROW, endCol].CellStyle.Font.Bold = true;
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Group Balance Report.xls";
                //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //workbook.Close();
                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }

}
