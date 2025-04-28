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
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class AccountVoucherReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IGLGeneralInfoService _gLGeneralInfoService;
        private readonly IBudgetMasterService _budgetMasterService;
        private readonly IActivityService _activityService;
        private readonly IPlantService _plantService;
        private readonly IVoucherService _voucherService;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IEmployeePayableService _employeePayableService;
        public AccountVoucherReportService(ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IGLGeneralInfoService gLGeneralInfoService
            , IActivityService activityService
            , IPlantService plantService
            , IVoucherService voucherService
            , IEmployeeInformationService employeeInformationService
            , IEmployeePayableService employeePayableService
            , IBudgetMasterService budgetMasterService
            )
        {
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _gLGeneralInfoService = gLGeneralInfoService;
            _activityService = activityService;
            _plantService = plantService;
            _voucherService = voucherService;
            _employeeInformationService = employeeInformationService;
            _employeePayableService = employeePayableService;
            _budgetMasterService = budgetMasterService;
        }


        private bool GetPlantIsShowFCInWord(string plantId)
        {
            return bplib.clsWebLib.GetBoolData(_sqlRepository.GetDataCollection(@"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='" + plantId + "'")[0]["IsShowFCInWord"].ToString());
        }

        public void GetParallelCurrency(string companyId, out string companyCurrencyId, out string companyCurrencyCode)
        {
            var companyParallelCurrency = GetCompanyCurrencyId(companyId);
            if (null == companyParallelCurrency["CurrencyId"].ToString())
                throw new CustomException(ResourcesCore.CompanyParallelCurrencyNotConfigured);
            companyCurrencyId = companyParallelCurrency["CurrencyId"].ToString();
            companyCurrencyCode = companyParallelCurrency["CurrencyCode"].ToString();
        }
        private Dictionary<string, object> GetCompanyCurrencyId(string companyId)
        {
            var cmdText = @"select cpc.CurrencyId,C.Code CurrencyCode from SCS.CompanyParallelCurrency cpc
                            LEFT JOIN SCS.Currency C ON C.Id = CPC.CurrencyId where cpc.ParallelCurrencyType = '" + ParallelCurrencyType.CompanyCurrency.ToString() + "'";
            return _sqlRepository.GetData(cmdText);
        }



        public DataTable GetFixedAssetObData(string companyGroupId, string companyId, string plantId, string fiscalYearId)
        {
            var cmdText = @"SELECT FAM.UserName AS FixedAssetMasterName, AGL.AccountCode+' - '+AGL.UserName AS AssetGLName, v.FiscalYearId, AB.UserName BudgetName, AC.UserName AssetActivityName
                            , CC.FACompanyCurrencyAmount AS FixedAssetValue, ADCC.ADCompanyCurrencyAmount AS AccDepAmount, (CC.FACompanyCurrencyAmount - ADCC.ADCompanyCurrencyAmount) NetBookValue
                            FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                            LEFT JOIN [TRN].[OpeningBalance] AS FOB ON FOBD.OpeningBalanceId=FOB.Id
							LEFT JOIN [TRN].[Voucher] AS V ON V.Id=FOB.VoucherId
                            LEFT JOIN MST.FixedAssetMaster AS FAM ON FAM.Id=FOBD.FixedAssetMasterId
							LEFT JOIN [SCS].[UnitOfMeasurement] AS UOM ON UOM.Id=FOBD.BaseUOMId
							LEFT JOIN HKP.GLGeneralInfo AGL ON FOBD.AssetGLId=AGL.Id
							LEFT JOIN HKP.GLGeneralInfo ACGL ON FOBD.AccumulatedDepreciationGLId=ACGL.Id
							LEFT JOIN MST.BudgetMaster BM ON FOBD.AssetBudgetMasterId=BM.Id
							LEFT JOIN HKP.Budget AB ON BM.BudgetId=AB.Id
							LEFT JOIN MST.BudgetMaster ACBBM ON FOBD.AccumulatedDepreciationBudgetMasterId=ACBBM.Id
							LEFT JOIN HKP.Budget ACB ON ACBBM.BudgetId=ACB.Id
                            LEFT JOIN HKP.Activity AC ON FOBD.AssetActivityId=AC.Id
							LEFT JOIN MST.MaterialMaster MM ON FOBD.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialMasterArticle MMA ON FOBD.ArticleId = MMA.Id
							LEFT JOIN SCS.FiscalYear FY ON v.FiscalYearId=FY.Id
                            LEFT OUTER JOIN (
	                            SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                            OBDC.ToCurrencyRate AS FACompanyCurrencyRate, OBDC.Amount AS FACompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                            FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                            INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                            WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='C20181'
                            ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                            LEFT OUTER JOIN (
                            SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                            OBDC.ToCurrencyRate AS FACompanyGroupCurrencyRate, OBDC.Amount AS FACompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                            FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                            INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                            WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='C20181'
                            ) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                            LEFT OUTER JOIN (
	                            SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId,
	                            OBDC.ToCurrencyRate AS FAHardCurrencyRate, OBDC.Amount AS FAHardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                            FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                            INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                            WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='C20181'
                            ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                            LEFT OUTER JOIN (
	                            SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
	                            OBDC.ToCurrencyRate AS ADCompanyCurrencyRate, OBDC.Amount AS ADCompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                            FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                            INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                            WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='C20181'
                            ) AS ADCC ON ADCC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                            LEFT OUTER JOIN (
                            SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                            OBDC.ToCurrencyRate AS ADCompanyGroupCurrencyRate, OBDC.Amount AS ADCompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                            FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                            INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                            WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='C20181'
                            ) AS ADGC ON ADGC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                            LEFT OUTER JOIN (
	                            SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId,
	                            OBDC.ToCurrencyRate AS ADHardCurrencyRate, OBDC.Amount AS ADHardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                            FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                            INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                            WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='C20181'
                            ) AS ADHC ON ADHC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                            WHERE FOB.CompanyId='" + companyId + "'  AND FOB.PlantId='" + plantId + "' AND v.FiscalYearId='" + fiscalYearId + "'  ORDER BY AGL.AccountCode,AB.UserName,FAM.UserName,ACGL.AccountCode,ACB.UserName";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public DataTable GetBudgetMasterData(string coaId)
        {
            var cmdText = @"SELECT C.UserName AS COA, C1.Id AS Level1Id, C1.UserName AS Level1, C2.Id AS Level2Id, C2.UserName AS Level2, C3.Id AS Level3Id,
                            C3.UserName AS Level3, C4.Id AS Level4Id, C4.UserName AS Level4,GL.RefNo CARefNo, GL.AccountCode AS GLGeneralInfoCode ,GL.Id GLId, GL.UserName AS GLName, BG.UserName AS BudgetGroup,
                            BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BM.RefNo, BM.id BudgetMasterId,B.UserName AS Budget  ,FAM.Code AS FACode,
							FAM.UserName AS FixedAssetMaster, R.UserName AS Register, [Project]=CASE WHEN BM.IsProject=1 THEN 'Yes' ELSE NULL END,
                            [Manufacturing]=CASE WHEN GL.IsManufacturing=1 THEN 'Yes' ELSE NULL END, [Treding]=CASE WHEN GL.IsTreding =1 THEN 'Yes' ELSE NULL END,
                            [Service]=CASE WHEN GL.IsService =1 THEN 'Yes' ELSE NULL END
                            FROM HKP.GLGeneralInfo AS GL
                            LEFT OUTER JOIN HKP.COALevel1 AS C1 ON C1.Id=GL.COALevel1Id
                            LEFT OUTER JOIN HKP.COALevel2 AS C2 ON C2.Id=GL.COALevel2Id
                            LEFT OUTER JOIN HKP.COALevel3 AS C3 ON C3.Id=GL.COALevel3Id
                            LEFT OUTER JOIN HKP.COALevel4 AS C4 ON C4.Id=GL.COALevel4Id
                            LEFT OUTER JOIN MST.BudgetMaster AS BM ON BM.GLGeneralInfoId=GL.Id
                            LEFT OUTER JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                            LEFT OUTER JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id =BM.BudgetSubCategoryId
                            LEFT OUTER JOIN HKP.BudgetCategory AS BC ON BC.Id =BM.BudgetCategoryId
                            LEFT OUTER JOIN HKP.BudgetGroup AS BG ON BG.Id =BM.BudgetGroupId
							LEFT OUTER JOIN HKP.Register AS R ON R.Id =BM.RegisterId
                            LEFT OUTER JOIN HKP.COA AS C ON C.Id =BM.COAId
                            LEFT JOIN [HKP].[FixedAssetMasterBudgetTag] AS FAMT ON FAMT.BudgetMasterId=BM.Id
                            LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=FAMT.FixedAssetMasterId";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public DataTable GetBudgetMasterActivityData(string coaId)
        {
            var cmdText = @"SELECT C.UserName AS COA, C1.Id AS Level1Id, C1.UserName AS Level1, C2.Id AS Level2Id, C2.UserName AS Level2, C3.Id AS Level3Id, C3.UserName AS Level3, C4.Id AS Level4Id, C4.UserName AS Level4
                            ,GL.RefNo CARefNo, GL.AccountCode AS GLGeneralInfoCode, GL.id GLId,GL.UserName AS GLName, BG.UserName AS BudgetGroup, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BM.RefNo,BM.Id BudgetMasterId 
,B.UserName AS Budget
                            , A.UserName AS Activity, BMA.ActivityId
                          --,BMA.Id BudgetMasterActivityId
							,[Default]=CASE WHEN BMA.IsDefault=1 THEN 'Yes' ELSE 'No' END
							,Specific=CASE WHEN CGD.BudgetMasterId IS NULL  THEN 'Yes' ELSE 'No' END
                            , FAM.Code AS FACode, FAM.UserName AS FixedAssetMaster, R.UserName AS Register, [Project]=CASE WHEN BM.IsProject=1 THEN 'Yes' ELSE NULL END
                            , [Manufacturing]=CASE WHEN GL.IsManufacturing=1 THEN 'Yes' ELSE NULL END, [Treding]=CASE WHEN GL.IsTreding=1 THEN 'Yes' ELSE NULL END
                            , [Service]=CASE WHEN GL.IsService =1 THEN 'Yes' ELSE NULL END
                            FROM [HKP].[GLGeneralInfo] AS GL
                            LEFT JOIN [HKP].[COALevel1] AS C1 ON C1.Id=GL.COALevel1Id
                            LEFT JOIN [HKP].[COALevel2] AS C2 ON C2.Id=GL.COALevel2Id
                            LEFT JOIN [HKP].[COALevel3] AS C3 ON C3.Id=GL.COALevel3Id
                            LEFT JOIN [HKP].[COALevel4] AS C4 ON C4.Id=GL.COALevel4Id
                            LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.GLGeneralInfoId=GL.Id
                            LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                            LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=BM.BudgetSubCategoryId
                            LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=BM.BudgetCategoryId
                            LEFT JOIN [HKP].[BudgetGroup] AS BG ON BG.Id=BM.BudgetGroupId
                            LEFT JOIN [HKP].[Register] AS R ON R.Id=BM.RegisterId
                            LEFT JOIN [HKP].[COA] AS C ON C.Id=BM.COAId
                            LEFT JOIN [HKP].[FixedAssetMasterBudgetTag] AS FAMT ON FAMT.BudgetMasterId=BM.Id
                            LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=FAMT.FixedAssetMasterId
                            LEFT Join [MST].[BudgetMasterActivity] AS BMA ON BMA.BudgetMasterId=BM.Id
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=BMA.ActivityId
							LEFT JOIN [HKP].[CompanyGroupActivity] AS CGD ON CGD.ActivityId=A.Id
                            WHERE C.Id='" + coaId + "'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public Dictionary<string, object> GetAdvanceJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ";
            return _sqlRepository.GetData(cmdText);
        }


        public Dictionary<string, object> GetDashboardJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode,EB.BeneficiaryType,EB.EmployeeId
							,Beneficiary=CASE WHEN EB.EmployeeId<>'' THEN 'Employee' WHEN EB.PartyId<>'' THEN 'Party' ELSE NULL end
							,BeneficiaryName=CASE WHEN EB.EmployeeId<>'' THEN EI.EmployeeName WHEN EB.PartyId<>'' THEN P.UserName ELSE NULL end
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN TRN.ExpenseBooking AS EB ON EB.VoucherId=V.Id
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                            WHERE V.Archive=0  AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ";
            return _sqlRepository.GetData(cmdText);
        }
        public DataTable GetGLMaster(string coaId)
        {
            var cmdText = @"SELECT C.UserName AS COA, C1.Id AS Level1Id, C1.UserName AS Level1, C2.Id AS Level2Id, C2.UserName AS Level2, C3.Id AS Level3Id, C3.UserName AS Level3,
                            C4.Id AS Level4Id, C4.UserName AS Level4, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName, AG.UserName AS AccountGroup,
                            [Manufacturing]=CASE WHEN GL.IsManufacturing=1 THEN 'Yes' ELSE NULL END,
                            [Treding]=CASE WHEN GL.IsTreding =1 THEN 'Yes' ELSE NULL END,
                            [Service]=CASE WHEN GL.IsService =1 THEN 'Yes' ELSE NULL END
                            FROM HKP.GLGeneralInfo AS GL
                            LEFT JOIN HKP.COALevel1 AS C1 ON C1.Id=GL.COALevel1Id
                            LEFT JOIN HKP.COALevel2 AS C2 ON C2.Id=GL.COALevel2Id
                            LEFT JOIN HKP.COALevel3 AS C3 ON C3.Id=GL.COALevel3Id
                            LEFT JOIN HKP.COALevel4 AS C4 ON C4.Id=GL.COALevel4Id
                            LEFT JOIN HKP.COA AS C ON C.Id=GL.COAId
                            LEFT JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                            WHERE GL.Archive=0 AND GL.COAId='" + coaId + @"'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public DataTable GetDailyTransactionData(string companyGroupId, string companyId, string plantId, string entityId, DateTime date)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId
                            , BUD.UserName AS BudgetName
                            , Activity= case when CM.UserName<>'' then CM.UserName else  ACT.UserName end 
                            , UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId
							,[ParticularName]=CASE
								WHEN V.VoucherNo<>'' THEN V.VoucherNo
								--WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
								--WHEN P.UserName<>'' THEN P.UserName 
								--WHEN CM.UserName<>'' THEN CM.UserName
                                --WHEN FAM.UserName<>'' THEN FAM.UserName
								ELSE ''	END
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                            LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
							LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
							LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
							LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
							LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
							LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.PostingDate='" + date + "' ";
            if (string.IsNullOrEmpty(entityId) == false && entityId.Contains("Undefined") == false && entityId.Contains("null") == false)
            {
                cmdText += "AND V.EntityId = '" + entityId + "'";
            }


            return _sqlRepository.GetDataTable(cmdText);
        }

        public DataTable GetDayBooksData(string companyGroupId, string companyId, string plantId, DateTime fromDate, DateTime toDate, string dateType)
        {
            string wcEmpStatus = " ";

            if (dateType == "PostingDate")
            {
                wcEmpStatus = " AND CONVERT(DATE,V.PostingDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"' ";
            }
            else
            {
                wcEmpStatus = " AND CONVERT(DATE,V.AddedDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"' ";

            }


            var cmdText = @"SELECT CO.UserName CompanyName, PT.UserName PlantName,EN.UserName AS EntityName
                        , VoucherType=v.SourceType
                        , V.VoucherNo
                        ,Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') PostingDate

                        , Replace(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate
                        ,V.DocRefNo
                        ,GLGI.AccountCode GLCode, GL=GLGI.UserName, b.UserName Budget, BM.RefNo AS BudgetRefNo, A.UserName Activity,VD.ActivityId ActivityCode

                        ,Particular=CASE WHEN VD.PartyId<>'' THEN PP.UserName
                        WHEN VD.BankMasterId<>'' THEN BKM.AccountTitle
                        WHEN VD.CashMasterId<>'' THEN CM.UserName
                        WHEN VD.EmployeeId<>'' THEN ei.EmployeeName ELSE '' END

		                ,isnull( ir.Id,'')GRNNo
						,AcceptanceNo= STUFF((select distinct ','+XPD.Id from
														TRN.PurchasedocAcceptance XPD Left join TRN.Voucher AS XV ON XV.Id=XPD.VoucherId
													where	XV.Id=V.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,Issue= STUFF((select distinct ','+XPD.Id from
														TRN.InventoryIssue XPD Left join TRN.Voucher AS XV ON XV.Id=XPD.VoucherId
													where	XV.Id=V.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        
                        ,C.Code TrnCurrency
                        ,[Amount]=CASE WHEN ISNULL(VD.DrAmount,0)<>0.00 THEN ISNULL(VD.DrAmount,0) WHEN ISNULL(VD.CrAmount,0)<>0.00 THEN ISNULL(VD.CrAmount,0) ELSE 0 END
                        ,ISNULL(VD.DrAmount,0) DrAmount,ISNULL(VD.CrAmount,0) CrAmount
                        ,CB.Code BooksCurrency
                        ,ISNULL(CC.CompanyCurrencyDrAmount,0) CompanyCurrencyDrAmount,ISNULL(CC.CompanyCurrencyCrAmount,0) CompanyCurrencyCrAmount
                        ,BG.UserName BudgetGroup,BCT.UserName BudgetCategory,BSCT.UserName BudgetSubCategory
                        ,V.AddedBy
                        , Replace(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') EntryDate
                        ,v.Narration
                        ,NoteForAccounts= case when isnull(ir.NoteForAccounts,null) is not null then isnull(ir.NoteForAccounts,null) when isnull(ii.Remarks,null) is not null then isnull(ii.Remarks,null) else null end
                        ,ei.EmployeeName
                        ,ACT.Id AS [Type],C1.UserName Level1,C2.UserName Level2,C3.UserName Level3,C4.UserName Level4, CCE.UserName CostCenterName
						,Replace(CONVERT(VARCHAR(11), GLTD.ReconcileDate , 106), ' ', '-') ReconcileDate
						,Reconcile=CASE WHEN VD.BankMasterId<>'' AND GLTD.ReconcileId<>'' THEN 'Yes' WHEN VD.BankMasterId IS NULL THEN '' ELSE 'No' END
                        ,(select COUNT(Id) from [TRN].[VoucherGLUpdateLog] where VoucherDetailId=VD.Id)GLUpdate
                         ,PC.UserName UserCategory,PSC.UserName UserSubCategory,VT.Category VoucherCategory
                        ,PG.UserName PartyGroup,PAG.UserName PartyAccountGroup,IsPark = case when V.IsPark=1 then 'Yes' else 'No' end
                        FROM TRN.VoucherDetail AS VD
                        LEFT JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                        LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                        LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BM.BudgetGroupId
                        LEFT JOIN [HKP].BudgetCategory BCT ON BCT.Id=BM.BudgetCategoryId
                        LEFT JOIN [HKP].BudgetSubCategory BSCT ON BSCT.Id=BM.BudgetSubCategoryId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType=VD.PartyType
                        LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType=VD.PartyType
                        LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
                        LEFT JOIN [HKP].PartyCategory PC ON PC.Id=P.PartyCategoryId
						LEFT JOIN [HKP].PartySubCategory PSC ON PSC.Id=P.PartySubCategoryId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                        LEFT JOIN [ORG].[Company] AS CO ON CO.Id=V.CompanyId
                        LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=V.PlantId
                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
                        LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=VD.BankMasterId
                        LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
					    left join ORG.CostCenter CCE ON CCE.Id =VD.CostCenterId
                        LEFT JOIN SCS.VoucherType VT ON VT.Id=V.VoucherTypeId
                        left join trn.Invoice I on I.VoucherId=V.Id
                        left join trn.InventoryReceive ir on ir.Id=i.InventoryReceiveId
                        left join trn.InventoryIssue ii on ii.VoucherId=v.Id
                        left join dbo.EmployeeInformation ei on ei.SystemId=VD.EmployeeId
                        LEFT JOIN SCS.Currency CB ON CB.Id=CO.BaseCurrencyId

                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                        FROM [TRN].[VoucherDetailCurrency] AS VDC
                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN HKP.COALevel1 C1 ON C1.Id=GLGI.COALevel1Id
                        LEFT JOIN HKP.COALevel2 C2 ON C2.Id=GLGI.COALevel2Id
                        LEFT JOIN HKP.COALevel3 C3 ON C3.Id=GLGI.COALevel3Id
                        LEFT JOIN HKP.COALevel4 C4 ON C4.Id=GLGI.COALevel4Id
						LEFT JOIN TRN.GLTransactionDetail GLTD ON GLTD.VoucherDetailId=VD.Id AND VD.BankMasterId=GLTD.BankMasterId
                        WHERE V.IsPark=0 and V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId ='" + companyId + "' AND V.PlantId='" + plantId + "' " + wcEmpStatus + @" ";
            return _sqlRepository.GetDataTable(cmdText);


        }

        public DataTable GetVoucherParkedData(string companyGroupId, string companyId, string plantId, DateTime fromDate, DateTime toDate)
        {
            var cmdText = @"SELECT CO.UserName CompanyName, PT.UserName PlantName,EN.UserName AS EntityName
                        , VoucherType=v.SourceType , V.VoucherNo
                        ,Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') PostingDate
                        , Replace(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate
                        ,V.DocRefNo ,C.Code TrnCurrency
                       ,ISNULL((select sum(CrAmount) from trn.VoucherDetail where VoucherId=V.Id and CrAmount>0),0) CrAmount
                        ,V.AddedBy , Replace(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') EntryDate ,v.Narration
                       ,IsPark = case when V.IsPark=1 then 'Yes' else 'No' end
                        FROM  TRN.Voucher AS V 
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                        LEFT JOIN [ORG].[Company] AS CO ON CO.Id=V.CompanyId
                        LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=V.PlantId
                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
                        WHERE V.IsPark=1 and V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId ='" + companyId + "' AND V.PlantId='" + plantId +  "' AND CONVERT(DATE, V.PostingDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"'  ";
            return _sqlRepository.GetDataTable(cmdText);


        }

        public DataTable GetAssetDepreciationReportData(string companyGroupId, string companyId, string plantId, DateTime fromDate, DateTime toDate, string assetDepreciationId)
        {
            var cmdText = @"DECLARE @AssetDepreciationId varchar(50)='" + assetDepreciationId + @"'

                        SELECT AssetDepreciationId,AD.ProcessName, REPLACE(CONVERT(CHAR(11), AD.ProcessDate, 106),' ','-') AS ProcessDate, AssetRegisterId, AssetRegisterChildId
	                    , CapitalizationMasterId, CapitalizationChildId, REPLACE(CONVERT(CHAR(11), CapitalizationDate, 106),' ','-') AS CapitalizationDate, ADDS.FixedAssetMasterId
	                    , FixedAssetItemId,FAM.UserName FixedAssetMaster,FAI.UserName FixedAssetItem, DepreciationDays, DepreciationType, DepreciationRate, AssetValue
	                    , DepreciationAmount, AccumulatedDepreciationAmount, NetAssetValue, AD.Remarks
	                    FROM [TRN].[AssetDepreciation] AD
	                    INNER JOIN [TRN].[AssetDepreciationDetail] ADDS  ON  ADDS.AssetDepreciationId = AD.Id
	                    LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=ADDS.FixedAssetItemId
                        LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
	                    WHERE ADDS.AssetDepreciationId= CASE WHEN @AssetDepreciationId<> 'null' THEN @AssetDepreciationId ELSE ADDS.AssetDepreciationId END
                        AND AD.CompanyGroupId='" + companyGroupId + "' AND AD.CompanyId ='" + companyId + "' AND AD.PlantId='" + plantId + "' AND CONVERT(DATE, AD.ProcessDate) BETWEEN '" + fromDate + "' AND '" + toDate + @"'
                        ORDER BY AssetRegisterId, AssetRegisterChildId, CapitalizationMasterId, CapitalizationChildId";
            return _sqlRepository.GetDataTable(cmdText);


        }
        public DataTable GetFixedAssetFinancialRegisterReportData(string companyGroupId, string companyId, string plantId, DateTime fromDate, DateTime toDate)
        {
            var cmdText = @"DECLARE @fromDate datetime='" + fromDate + @"' ,@toDate datetime='" + toDate + @"'

                select x.GL,x.Budget,x.Activity
 ,SUM(x.OpeningAmount)OpeningAmount
 ,SUM(x.OpeningJV) OpeningJV
 ,SUM(x.OpeningAmount)+SUM(x.OpeningJV) TotalOpeningAmount
 ,SUM(x.RegisterItemAmount)RegisterItemAmount
 ,SUM(x.DepreciationAmount)DepreciationAmount
,SUM(x.OpeningAmount)+SUM(x.OpeningJV)+SUM(x.RegisterItemAmount)-SUM(x.DepreciationAmount) NetAssetAmount
,0 JVAmount 
,TotalAmount=SUM(x.OpeningAmount)+SUM(x.OpeningJV)+SUM(x.RegisterItemAmount)-SUM(x.DepreciationAmount)+sum(isnull(x.JVAmount,0))
,x.GLGeneralInfoId,x.BudgetMasterId,x.ActivityId from (
 
 SELECT T.GL,T.Budget,T.Activity
 ,SUM(T.OpeningAmount)-T.OBDepreciationAmount OpeningAmount
 ,isnull(jv.JVDrAmount,0)-Isnull(jv.JVCrAmount,0) OpeningJV
 ,SUM(T.RegisterItemAmount)RegisterItemAmount
 ,SUM(T.DepreciationAmount)DepreciationAmount
,SUM(T.NetAssetAmount)NetAssetAmount
,0 JVAmount 
,TotalAmount=SUM(T.NetAssetAmount)+(isnull(jv.JVDrAmount,0)-Isnull(jv.JVCrAmount,0))
,T.GLGeneralInfoId,t.BudgetMasterId,t.ActivityId
FROM (
--opening
select GL.UserName GL,B.UserName Budget,A.UserName Activity,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
,ARC.Amount OpeningAmount,ISNULL(aDep.DepreciationAmount,0) OBDepreciationAmount, 0 RegisterItemAmount 
,0 DepreciationAmount
,0 NetAssetAmount
from trn.AssetRegisterChild ARC
left join trn.AssetRegister AR on AR.Id = ARC.AssetRegisterId
LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=ARC.FixedAssetItemId
LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
left join trn.VoucherDetail VD on VD.Id = ARC.VoucherdetailId
left join trn.CapitalizationMaster CM on CM.Id = ARC.CapitalizationMasterId
left join TRN.Voucher V on V.Id = CM.VoucherId
left join hkp.GLGeneralInfo GL ON GL.Id=vd.GLGeneralInfoId
left join MST.BudgetMaster BM ON BM.Id=vd.BudgetMasterId
left join hkp.Budget B ON B.Id=BM.BudgetId
left join hkp.Activity A ON A.Id=vd.ActivityId
left join mst.BudgetMasterActivity bma ON bma.BudgetMasterId=VD.BudgetMasterId and bma.ActivityId=VD.ActivityId

left join (
	select VD.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId,SUM(VDC.CrAmount) DepreciationAmount
	FROM trn.VoucherDetail VD  
	LEFT JOIN  trn.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id  
	LEFT JOIN [TRN].Voucher ADV ON ADV.Id=VD.VoucherId
		where ADV.PostingDate< @fromDate  and ADV.Ispark=0  and ADV.SourceType='DepreciationJournal' --AND ADV.IsPark=0  
		group by VD.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
		) aDep ON aDep.GLGeneralInfoId=VD.GLGeneralInfoId and aDep.BudgetMasterId=VD.BudgetMasterId and aDep.ActivityId=VD.ActivityId

where v.PostingDate < @fromDate  and v.Ispark=0 
and ARC.AssetRegisterId NOT IN (select AssetRegisterId from [TRN].[FixedAssetRegisterDisposedDetail] FRDD 
join  [TRN].[FixedAssetRegisterDisposed] FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId 
join trn.Voucher DV ON DV.Id=FRD.DisposedVoucherId and DV.IsPark=0
where DV.PostingDate< @fromDate)
)T

LEFT JOIN (SELECT VD.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId,Sum(ISNULL(vdc.DrAmount,0)) JVDrAmount,sum(ISNULL(vdc.CrAmount,0)) JVCrAmount
FROM TRN.VoucherDetailCurrency VDC JOIN TRN.VoucherDetail VD ON VD.Id=VDC.VoucherDetailId JOIN TRN.Voucher V ON V.Id=VD.VoucherId
where v.PostingDate < @fromDate and V.SourceType='JournalVoucher'  and V.IsPark=0
group by VD.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId) jv ON jv.GLGeneralInfoId=T.GLGeneralInfoId and jv.BudgetMasterId=T.BudgetMasterId and jv.ActivityId=T.ActivityId

GROUP BY T.GL,T.Budget,T.Activity,jv.JVCrAmount,jv.JVDrAmount,T.GLGeneralInfoId,t.BudgetMasterId,t.ActivityId,T.OBDepreciationAmount


UNION ALL

SELECT T.GL,T.Budget,T.Activity
 ,SUM(T.OpeningAmount)OpeningAmount
 ,SUM(T.OpeningJV) OpeningJV
 ,SUM(T.RegisterItemAmount)RegisterItemAmount
 ,SUM(T.DepreciationAmount)DepreciationAmount
,SUM(T.NetAssetAmount)NetAssetAmount
,JVAmount=isnull(jv.JVDrAmount,0)-Isnull(jv.JVCrAmount,0)
,TotalAmount=SUM(T.NetAssetAmount)+(isnull(jv.JVDrAmount,0)-Isnull(jv.JVCrAmount,0))
,T.GLGeneralInfoId,t.BudgetMasterId,t.ActivityId
FROM (

select GL.UserName GL,B.UserName Budget,A.UserName Activity,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
,0 OpeningAmount,0 OpeningJV,ARC.Amount RegisterItemAmount
,0 DepreciationAmount--,ISNULL(aDep.DepreciationAmount,0) DepreciationAmount
,NetAssetAmount=ARC.Amount-ISNULL(aDep.DepreciationAmount,0)
from trn.AssetRegisterChild ARC
left join trn.AssetRegister AR on AR.Id = ARC.AssetRegisterId
LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=ARC.FixedAssetItemId
LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
left join trn.VoucherDetail VD on VD.Id = ARC.VoucherdetailId
left join trn.CapitalizationMaster CM on CM.Id = ARC.CapitalizationMasterId
left join TRN.Voucher V on V.Id = CM.VoucherId
left join hkp.GLGeneralInfo GL ON GL.Id=vd.GLGeneralInfoId
left join MST.BudgetMaster BM ON BM.Id=vd.BudgetMasterId
left join hkp.Budget B ON B.Id=BM.BudgetId
left join hkp.Activity A ON A.Id=vd.ActivityId
left join mst.BudgetMasterActivity bma ON bma.BudgetMasterId=VD.BudgetMasterId and bma.ActivityId=VD.ActivityId
left join (select AssetRegisterChildId,SUM(DepreciationAmount) DepreciationAmount from [TRN].[AssetDepreciationDetail]  ADPD 
	LEFT JOIN [TRN].[AssetDepreciation] ADP ON ADP.Id=ADPD.AssetDepreciationId LEFT JOIN [TRN].Voucher ADV ON ADV.Id=ADP.VoucherId
		where ADV.PostingDate between @fromDate and @toDate AND ADV.IsPark=0  group by AssetRegisterChildId ) aDep ON aDep.AssetRegisterChildId=ARC.Id

where v.PostingDate  between @fromDate and @toDate  and v.Ispark=0 and ARC.AssetRegisterId NOT IN (select AssetRegisterId from [TRN].[FixedAssetRegisterDisposedDetail] FRDD 
join  [TRN].[FixedAssetRegisterDisposed] FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId 
join trn.Voucher DV ON DV.Id=FRD.DisposedVoucherId and DV.IsPark=0
where DV.PostingDate between @fromDate and @toDate)



UNION ALL

select GL.UserName GL,B.UserName Budget,A.UserName Activity,vd.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId
,0 OpeningAmount,0 OpeningJV,0 RegisterItemAmount
,ISNULL(aDep.DepreciationAmount,0) DepreciationAmount
,0 NetAssetAmount
from trn.AssetRegisterChild ARC
left join trn.AssetRegister AR on AR.Id = ARC.AssetRegisterId
LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=ARC.FixedAssetItemId
LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
left join trn.VoucherDetail VD on VD.Id = ARC.VoucherdetailId
left join trn.CapitalizationMaster CM on CM.Id = ARC.CapitalizationMasterId
left join TRN.Voucher V on V.Id = CM.VoucherId
left join hkp.GLGeneralInfo GL ON GL.Id=vd.GLGeneralInfoId
left join MST.BudgetMaster BM ON BM.Id=vd.BudgetMasterId
left join hkp.Budget B ON B.Id=BM.BudgetId
left join hkp.Activity A ON A.Id=vd.ActivityId
left join mst.BudgetMasterActivity bma ON bma.BudgetMasterId=VD.BudgetMasterId and bma.ActivityId=VD.ActivityId
left join (select AssetRegisterChildId,SUM(DepreciationAmount) DepreciationAmount from [TRN].[AssetDepreciationDetail]  ADPD 
	LEFT JOIN [TRN].[AssetDepreciation] ADP ON ADP.Id=ADPD.AssetDepreciationId LEFT JOIN [TRN].Voucher ADV ON ADV.Id=ADP.VoucherId
		where ADV.PostingDate between @fromDate and @toDate  AND ADV.IsPark=0  group by AssetRegisterChildId ) aDep ON aDep.AssetRegisterChildId=ARC.Id

)T

LEFT JOIN (SELECT VD.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId,Sum(ISNULL(vdc.DrAmount,0)) JVDrAmount,sum(ISNULL(vdc.CrAmount,0)) JVCrAmount
FROM TRN.VoucherDetailCurrency VDC JOIN TRN.VoucherDetail VD ON VD.Id=VDC.VoucherDetailId JOIN TRN.Voucher V ON V.Id=VD.VoucherId
where v.PostingDate  between @fromDate and @toDate and V.SourceType='JournalVoucher'  and V.IsPark=0
group by VD.GLGeneralInfoId,vd.BudgetMasterId,vd.ActivityId) jv ON jv.GLGeneralInfoId=T.GLGeneralInfoId and jv.BudgetMasterId=T.BudgetMasterId and jv.ActivityId=T.ActivityId

GROUP BY T.GL,T.Budget,T.Activity,jv.JVCrAmount,jv.JVDrAmount,T.GLGeneralInfoId,t.BudgetMasterId,t.ActivityId
) x
group by x.GL,x.Budget,x.Activity,x.GLGeneralInfoId,x.BudgetMasterId,x.ActivityId ";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public Dictionary<string, object> GetDailyTransactionHeader(string companyGroupId, string companyId, string plantId, DateTime date)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' --AND V.PostingDate='" + date + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public DataTable GetGeneralLedgerData(string companyGroupId, string companyId, string plantId, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool isOpeningBalance, string fiscalYearId, string bankMasterId, string cashMasterId, string partyId)
        {
            var bankCashPartyFilter = string.Empty;
            if (!string.IsNullOrEmpty(bankMasterId))
                bankCashPartyFilter = " AND VD.BankMasterId='" + bankMasterId + "' ";
            if (!string.IsNullOrEmpty(cashMasterId))
                bankCashPartyFilter = " AND VD.CashMasterId='" + cashMasterId + "' ";
            if (!string.IsNullOrEmpty(partyId))
                bankCashPartyFilter = " AND VD.PartyId='" + partyId + "' ";
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                             ,V.SourceType, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
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
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' " + bankCashPartyFilter + " ";
            if (!string.IsNullOrEmpty(budgetMasterId))
                cmdText += " AND VD.BudgetMasterId='" + budgetMasterId + "' ";
            if (!string.IsNullOrEmpty(activityId))
                cmdText += " AND VD.ActivityId='" + activityId + "' ";
            cmdText += isOpeningBalance ? " AND V.SourceType='OpeningBalance' AND V.FiscalYearId='" + fiscalYearId + "' AND VD.GLGeneralInfoId IS NOT NULL" : " AND VD.GLGeneralInfoId='" + glId + "' AND V.SourceType!='OpeningBalance' AND CONVERT(VARCHAR, V.PostingDate, 23) BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate.ToDbDate() + "'";
            cmdText += " ORDER BY V.PostingDate ASC, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public DataTable GetGeneralLedgerGSTData(string companyGroupId, string companyId, string plantId, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool isOpeningBalance, string fiscalYearId)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                              SELECT Vt.UserName VoucherType,REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, VD.CurrencyId
							, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName
                            ,PartyName = STUFF((select distinct ','+ xp.UserName  from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' 
														AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PartyTaxNo = STUFF((select distinct ','+ xp.TINNO  from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' 
														AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,TaxCetegory= CASE WHEN TC.UserName<>'' THEN TC.UserName ELSE TXC.UserName END
                                         ,TaxableAmount=isnull((i.Amount-vd.DrAmount),0)
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT JOIN SCS.[VoucherType] Vt ON Vt.Id=V.VoucherTypeId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
							LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.Id=VD.InvoiceTaxDetailId 
							LEFT JOIN TRN.InvoiceTax IVT ON IVT.Id=ITD.InvoiceTaxId AND ITD.AType=CASE WHEN VD.DrAmount>0 THEN 'Dr' WHEN VD.CrAmount>0 THEN 'Cr' END
							LEFT JOIN MST.TaxCategory TC ON TC.Id=IVT.TaxCategoryId
							LEFT JOIN MST.TaxCode TXC ON TXC.Id=IVT.TaxCodeId
							LEFT JOIN TRN.Invoice i ON I.VoucherId=V.Id
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

        public DataTable GetGeneralLedgerGroupByData(string companyGroupId, string companyId, string plantId, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool isOpeningBalance, string fiscalYearId)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, VD.CurrencyId
							, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName ,A.Id ActivityID, A.UserName AS ActivityName,p.username as Party
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
,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND VD.GLGeneralInfoId='" + glId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        UNION
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND VD.GLGeneralInfoId='" + glId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId, X.CompanyGroupCurrencyId, X.HardCurrencyId ),0)ActivityOpeningBalance
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
            cmdText += " ORDER BY VD.ActivityId,BGM.BudgetId,VD.GLGeneralInfoId,V.PostingDate, V.VoucherNo  ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public DataTable GetGeneralLedgerWithBudgetActivityGroupByData(string companyGroupId, string companyId, string plantId, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool isOpeningBalance, string fiscalYearId)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT SUM(ISNULL(VD.DrAmount,0)) AS DrAmount, SUM(ISNULL(VD.CrAmount,0)) AS CrAmount
							, SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0)) AS CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0)) AS CompanyCurrencyCrAmount
							, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,BGM.BudgetId, BG.UserName AS BudgetName ,A.Id ActivityID, A.UserName AS ActivityName
							,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND VD.GLGeneralInfoId='" + glId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType!='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        UNION
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND VD.GLGeneralInfoId='" + glId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId, X.CompanyGroupCurrencyId, X.HardCurrencyId ),0)ActivityOpeningBalance
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
            cmdText += " GROUP BY GLGI.AccountCode,GLGI.UserName,BGM.BudgetId,  BG.UserName,A.Id, A.UserName ORDER BY BG.UserName";
            return _sqlRepository.GetDataTable(cmdText);
        }


        public List<Dictionary<string, object>> GetGeneralOpeningBalanceLedgerData(string companyGroupId, string companyId, string plantId, string glId, string budgetMasterId, string activityId, string fromDate, string bankMasterId, string cashMasterId, string partyId)
        {
            var budgetFilter = string.Empty;
            if (!string.IsNullOrEmpty(budgetMasterId))
                budgetFilter = " AND VD.BudgetMasterId='" + budgetMasterId + "' ";
            if (!string.IsNullOrEmpty(activityId))
                budgetFilter = " AND VD.ActivityId='" + activityId + "' ";
            var bankCashPartyFilter = string.Empty;
            if (!string.IsNullOrEmpty(bankMasterId))
                bankCashPartyFilter = " AND VD.BankMasterId='" + bankMasterId + "' ";
            if (!string.IsNullOrEmpty(cashMasterId))
                bankCashPartyFilter = " AND VD.CashMasterId='" + cashMasterId + "' ";
            if (!string.IsNullOrEmpty(partyId))
                bankCashPartyFilter = " AND VD.PartyId='" + partyId + "' ";
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB
                        , CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB
                        , CompanyGroupCurrencyId, SUM(CompanyGroupCurrencyDrAmount)-SUM(CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyOB
                        , HardCurrencyId, SUM(HardCurrencyDrAmount)-SUM(HardCurrencyCrAmount) AS HardCurrencyOB FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.GLGeneralInfoId='" + glId + "' " + budgetFilter + " " + bankCashPartyFilter + " AND V.PostingDate < '" + fromDate.ToDbDate() + @"' AND V.SourceType!='OpeningBalance'
                        AND VD.Id NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
										INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
										LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
										LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
										LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
										WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        UNION
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.GLGeneralInfoId='" + glId + "' " + budgetFilter + " " + bankCashPartyFilter + " AND V.PostingDate <='" + fromDate.ToDbDate() + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId, X.CompanyGroupCurrencyId, X.HardCurrencyId ORDER BY OB DESC";

            return _sqlRepository.GetDataCollection(sql);
        }
        public DataTable GetSalaryJournalData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId, BUD.UserName AS BudgetName, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId
							,[ParticularName]=CASE
								WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
								WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
								WHEN P.UserName<>'' THEN P.UserName 
								WHEN CM.UserName<>'' THEN CM.UserName
                                WHEN FAM.UserName<>'' THEN FAM.UserName
								ELSE ''	END
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                            LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
							LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
							LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
							LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
							LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
							LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public DataTable GetEmployeeLedger(string companyGroupId, string companyId, string plantId, string employeeId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount, 0) AS DrAmount, ISNULL(VD.CrAmount, 0) AS CrAmount
                            , CC.CompanyCurrencyId, CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
							, BGM.RefNo, BG.UserName AS BudgetName, V.CurrencyId, A.UserName AS ActivityName, EI.EmployeeCode AS PartyCode, EI.EmployeeName AS PartyName
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
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
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.EmployeeId='" + employeeId + "' AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance' ORDER BY V.PostingDate--, V.VoucherNo 
                                ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public List<Dictionary<string, object>> GetEmployeeOpeningBalance(string companyGroupId, string companyId, string plantId, string employeeId, string fromDate)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB
						FROM (SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.EmployeeId='" + employeeId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + @"'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.EmployeeId='" + employeeId + "' AND V.PostingDate >='" + fromDate.ToDbDate() + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }

        public DataTable GetEmployeeSalaryAdvanceData(string companyGroupId, string companyId, string plantId, string employeeId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount, 0) AS DrAmount, ISNULL(VD.CrAmount, 0) AS CrAmount
                            , CC.CompanyCurrencyId, CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
							, BGM.RefNo, BG.UserName AS BudgetName, V.CurrencyId, A.UserName AS ActivityName, EI.EmployeeCode AS PartyCode, EI.EmployeeName AS PartyName
                            FROM [TRN].[VoucherDetail] AS VD
		                    join TRN.EmployeeSalaryAdvance ESA ON ESA.VoucherDetailId=VD.Id
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
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
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.EmployeeId='" + employeeId + "' AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance' ORDER BY V.PostingDate--, V.VoucherNo 
                                ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public List<Dictionary<string, object>> GetEmployeeSalaryAdvanceLedgerOBeData(string companyGroupId, string companyId, string plantId, string employeeId, string fromDate)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB
						FROM (SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        join TRN.EmployeeSalaryAdvance ESA ON ESA.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.EmployeeId='" + employeeId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + @"'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.EmployeeId='" + employeeId + "' AND V.PostingDate >='" + fromDate.ToDbDate() + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }
        public DataTable GetEmployeePayable(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId,VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
                            , Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, [Park/Post]=CASE WHEN v.IsPark=1 THEN 'Parked' ELSE 'Posted' END
                            , Replace(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') VoucherDate
		                    , V.VoucherNo, V.Narration, V.CurrencyId,CU1.Code AS TrnCurrency, V.AddedBy AS PreparedBy, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode, VDC.FromCurrencyId
		                    , VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount+VD.CrAmount AS Value, VDC.DrAmount, VDC.CrAmount, V.SourceType
                            , [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            , GL.UserName+' - '+BM.AccountTitle+' - '+BM.AccountNumber AS BankMain, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate
		                    , VD.DocRefNo AS InvoiceNo, P.EmployeeName AS Employee, VD.RefCode AS Ref,VD.Narration AS DetailNarration, CO.UserName AS CompanyName
                            , AM.Address1 AS AddressLine, BUD.UserName AS Budget, ACT.UserName AS Activity, ApprovedBy=CASE WHEN EBA.EmployeeName <>'' THEN EBA.EmployeeName END
	                        FROM TRN.VoucherDetailCurrency AS VDC
		                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                    LEFT JOIN TRN.EmployeePayableDetail AS CID ON CID.Id=VD.EmployeePayableDetailId
                            LEFT JOIN TRN.EmployeePayable AS CI ON CI.Id=CID.EmployeePayableId
							LEFT JOIN TRN.ExpenseBooking As EB ON EB.Id=CI.ExpenseBookingId
							LEFT JOIN TRN.ExpenseBookingApprovalHistory As EAH ON EAH.ExpenseBookingId=EB.Id
							LEFT JOIN dbo.EmployeeInformation AS EBA ON EBA.SystemId=EAH.EmployeeId
							LEFT JOIN dbo.EmployeeInformation AS P ON P.SystemId=CI.EmployeeId
		                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
		                    LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
		                    LEFT JOIN SCS.Currency AS CU1 ON CU1.Id=V.CurrencyId
		                    LEFT JOIN ORG.Company AS CO ON CO.Id=V.CompanyId
		                    LEFT JOIN MST.AddressMaster AS AM ON AM.Id=CO.AddressMasterId
                            LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=V.FiscalYearId
							LEFT JOIN SCS.FiscalYearPeriod AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
							LEFT JOIN MST.BudgetMaster BUM ON VD.BudgetMasterId=BUM.Id
							LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
							LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            WHERE V.Archive=0 AND V.SourceType='" + SourceType.EmployeePayable + "' AND V.Id = '" + voucherId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetEmployeeInvoicePayment(string voucherId)
        {
            try
            {
                var sql = @" SELECT V.Id,VDC.VoucherDetailId,
		                                    V.VoucherNo ,
                                            P.UserName AS Vendor, VD.PartyId, VD.EmployeeId, EM.EmployeeName AS Employee
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    LEFT JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN TRN.EmployeePayableWriteOffDetail AS IWD ON IWD.Id=VD.EmployeePayableWriteOffDetailId
		                                    LEFT JOIN TRN.EmployeePayableWriteOff AS IW ON IW.Id=IWD.EmployeePayableWriteOffId
		                                    LEFT JOIN HKP.Party AS P ON P.Id=VD.PartyId
											LEFT JOIN dbo.EmployeeInformation AS EM ON EM.SystemId=VD.EmployeeId
                                            where V.Archive = 0 AND V.Id = '" + voucherId + @"' AND VD.EmployeeId IS NOT NULL";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #region EmployeePayable
        public GridModel GetEmployeeAvailableInvoiceList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string employeeId)
        {
            try
            {
                parameters.CmdText = @" SELECT EPD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, EPD.BudgetMasterId, B.UserName AS BudgetName, EPD.ActivityId, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), EP.DocDate, 106), ' ', '-') DocDate,Replace(CONVERT(VARCHAR(11), EP.PostingDate, 106), ' ', '-') PostingDate,
                                        EP.DocRefNo, EP.Narration, EP.Id AS EmployeePayableId, EPD.Id AS EmployeePayableDetailId, EP.VoucherId, VD.EntityId, E.UserName AS EntityName, VD.PlantId,
                                        VD.Id AS VoucherDetailId, EP.CurrencyId, C.Code AS CurrencyCode, EP.EmployeeId, EPD.NetAmount AS Receivable,
                                        EPD.WrittenOffAmount AS Received, EPD.NetAmount-EPD.WrittenOffAmount AS Balance,EP.InventoryReceiveId GRNNo,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
										HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion, ET.AdvanceType JournalType
                                        ,Particular=REPLACE(REPLACE(
										STUFF((SELECT DISTINCT ','+xpo.UserName from
											hkp.Activity xpo
											INNER JOin TRN.VoucherDetail xPDAMAP on xpo.id=xPDAMAP.ActivityId
											WHERE VD.ActivityId!=xPDAMAP.ActivityId and xPDAMAP.VoucherId=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                        FROM [TRN].[EmployeePayableDetail] AS EPD
                                        LEFT JOIN [TRN].[EmployeePayable] AS EP ON EPD.EmployeePayableId=EP.Id
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.EmployeePayableDetailId=EPD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=EPD.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=EPD.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=EPD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=EP.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS E ON E.Id=VD.EntityId
                                        LEFT JOIN HKP.EmployeeTransactionType ET ON ET.Id=EP.EmployeeTransactionTypeId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
									LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.DrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS HC ON HC.VoucherDetailId=VD.Id
                                        WHERE EP.Archive=0 AND EP.IsPark=0 AND EP.IsWrittenOff=0 AND EPD.IsWrittenOff=0 AND EPD.IsBlock=0 AND EP.SourceType IN ('" + SourceType.EmployeePayable + "','" + SourceType.SalaryPayable + "','" + SourceType.VendorInvoice + "','" + SourceType.InventoryPayable + @"')
                                        AND EP.CompanyGroupId='" + companyGroupId + "' AND EP.CompanyId='" + companyId + "' AND EP.PlantId='" + plantId + @"' AND EP.EmployeeId='" + employeeId + "' AND (EPD.NetAmount-EPD.WrittenOffAmount)>0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel EmployeeListAllPlant(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.sort = "EmployeeCodeNumeric,EmployeeStatus";
                parameters.CmdText = @"SELECT Emp.SystemId,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,EMP.EmployeeStatus
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE EMP.CompanyId='" + companyId + @"' 
                                        ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel EmployeeListByPayable(GridParameter parameters, string companyId, string plantId)
        {
            try
            {
                parameters.sort = "EmployeeCodeNumeric";
                parameters.CmdText = @"SELECT Emp.SystemId,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
										JOIN (select distinct EmployeeId from TRN.EmployeePayable where IsWrittenOff=0) EP ON EP.EmployeeId=EMP.SystemId
                                        WHERE  EMP.CompanyId='" + companyId + @"'  
                                        ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion


        public IWorkbook GetGeneralOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId, bool isCompanyCurrency)
        {
            try
            {
                var row = 5;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Report";

                // Get bank transaction data.
                var fiscalYear = _sqlRepository.GetData("SELECT FiscalYearCode, FiscalYearName, StartDate, EndDate FROM [SCS].[FiscalYear] WHERE Id='" + fiscalYearId + "'");
                var ledgerData = GetGeneralLedgerData(companyGroupId, companyId, plantId, null, null, null, null, null, true, fiscalYearId, null, null, null);
                var colA = 1;
                var colB = 2;
                var colC = 3;
                var colD = 4;
                var colE = 5;
                var colF = 6;
                var colG = 7;
                var colH = 8;
                var colI = 9;
                var colJ = 10;
                var colK = 11;
                var colL = 12;
                if (ledgerData.Rows.Count > 0)
                {
                    // Set Header Column
                    row++;
                    reportUtility.SetHeaderText(ref sheet, row, colI, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(colI) + row + ":" + reportUtility.GetColumnNameForXls(colJ) + row].Merge();

                    colLast = colJ;
                    _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                    if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                    {
                        reportUtility.SetHeaderText(ref sheet, row, colK, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                        sheet.Range[reportUtility.GetColumnNameForXls(colK) + row + ":" + reportUtility.GetColumnNameForXls(colL) + row].Merge();
                        colLast = colL;
                    }

                    // Detail Row Header
                    row++;
                    reportUtility.SetHeaderText(ref sheet, row, colA, "GL", 12);
                    reportUtility.SetHeaderText(ref sheet, row, colB, "Currency", 8);
                    reportUtility.SetHeaderText(ref sheet, row, colC, "Posting Date", 12);
                    reportUtility.SetHeaderText(ref sheet, row, colD, "Voucher No", 12);
                    reportUtility.SetHeaderText(ref sheet, row, colE, "Voucher Date", 12);
                    reportUtility.SetHeaderText(ref sheet, row, colF, "Doc Ref", 12);
                    reportUtility.SetHeaderText(ref sheet, row, colG, "Doc Date", 12);
                    reportUtility.SetHeaderText(ref sheet, row, colH, "Narration", 18);
                    reportUtility.SetHeaderText(ref sheet, row, colI, "Debit", 12);
                    reportUtility.SetHeaderText(ref sheet, row, colJ, "Credit", 12);

                    if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                    {
                        reportUtility.SetHeaderText(ref sheet, row, colK, "Debit", 10, ExcelHAlign.HAlignRight);
                        reportUtility.SetHeaderText(ref sheet, row, colL, "Credit", 10, ExcelHAlign.HAlignRight);
                    }

                    row++;
                    if (ledgerData.Rows.Count > 0)
                    {
                        for (int i = 0; i < ledgerData.Rows.Count; i++)
                        {
                            reportUtility.SetText(ref sheet, row, colA, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]);
                            reportUtility.SetText(ref sheet, row, colB, ledgerData.Rows[i]["CurrencyCode"].ToString());
                            reportUtility.SetText(ref sheet, row, colC, ledgerData.Rows[i]["PostingDate"].ToString());
                            reportUtility.SetText(ref sheet, row, colD, ledgerData.Rows[i]["VoucherNo"].ToString());
                            reportUtility.SetText(ref sheet, row, colE, ledgerData.Rows[i]["VoucherDate"].ToString());
                            reportUtility.SetText(ref sheet, row, colF, ledgerData.Rows[i]["DocRefNo"].ToString());
                            reportUtility.SetText(ref sheet, row, colG, ledgerData.Rows[i]["DocDate"].ToString());
                            reportUtility.SetText(ref sheet, row, colH, ledgerData.Rows[i]["Narration"].ToString());
                            reportUtility.SetText(ref sheet, row, colI, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colJ, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString()));

                            // Base currency checking
                            if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                            {
                                reportUtility.SetText(ref sheet, row, colK, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, colL, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            }
                            row++;
                        }
                    }
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                }
                else
                {
                    reportUtility.SetText(ref sheet, 6, colLast, "Data not found!", ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(colA) + 6 + ":" + reportUtility.GetColumnNameForXls(colLast) + 6].Merge();
                }
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "General Opening Balance Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "Fiscal Year " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        //General ledger report
        public IWorkbook GetGeneralLedgerGroupReportWithBudgetActivity(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string activityId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var StartRow = row;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                //Get GL  heade and data
                var col = 1;
                var ob = 1;

                var colA = 1; //gl name and data
                var colB = 2;
                var colC = 3;

                var colE = 4;//marge

                var colF = 5;//account group

                //var colParticulars = 6;
                var colG = 6; //accout group value6
                var colI = 8; // marge8
                int colBaseCurrencyDebit = 0;
                int colBaseCurrencyCredit = 0;
                int colActivityBalance = 0;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                // Set Header
                var gl = _gLGeneralInfoService.GetGLData(glId);
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account Type");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["AccountTypeName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Account Group");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["AccountGroupName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, colA, "GL Name");
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["GLGeneralInfoCode"] + " - " + gl["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "RefNo");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["RefNo"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                colLast = 10;

                if (!string.IsNullOrEmpty(budgetMasterId))
                {
                    row++;
                    var budgetMaster = _budgetMasterService.GetBudgetMasterData(budgetMasterId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colA, "Budget");
                    sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, budgetMaster["UserName"].ToString());
                    sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();
                    colLast = 12;
                    //borderStartCol = 11;
                }
                if (!string.IsNullOrEmpty(activityId))
                {
                    var activity = _activityService.Find(activityId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Activity");
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, activity.UserName);
                    sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                    colLast = 11;
                }
                if (!string.IsNullOrEmpty(gl["AccountType"].ToString()))
                {
                    colLast += 1;
                }
                row++;
                ob = colLast - 3;


                // Set Row Header
                row++; //row10
                int colBudget = col;
                if (string.IsNullOrEmpty(budgetMasterId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colBudget, "Budget", 30); colBudget = col; col++;
                }
                int colActivity = col;
                if (string.IsNullOrEmpty(activityId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colActivity, "Activity", 40); colActivity = col; col++;
                }
                int colBalance = col;
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    colBaseCurrencyDebit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 30, ExcelHAlign.HAlignRight); col++;
                    colBaseCurrencyCredit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 20, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Budget Balance", 20, ExcelHAlign.HAlignRight); colActivityBalance = col; col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 22, ExcelHAlign.HAlignRight); colBalance = col; col++;
                    sheet.Range[reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (row - 1)].Merge();
                    reportUtility.SetHeaderText(ref sheet, row - 1, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row - 1, colBaseCurrencyDebit, row - 1, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Thin);
                }
                colLast = col;
                int colDrCr = col;
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                sheet[row, col].RowHeight = 22;

                row++;

                reportUtility.SetText(ref sheet, row, colActivity, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colActivity) + row].Merge();

                // Get bank opening balance data.
                var ledgerData = GetGeneralLedgerWithBudgetActivityGroupByData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, toDate, false, null);
                var obVal = GetGeneralOpeningBalanceLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, null, null, null);


                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, colLast - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    sheet[row, colActivityBalance].Number = clsStaticInfo.dbl(ledgerData.Rows[0]["ActivityOpeningBalance"].ToString());
                    sheet.Range[row, colActivityBalance].CellStyle.Font.Bold = true;
                    sheet.Range[row, colLast - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }
                row++;
                string TempBudgetId = ledgerData.Rows[0]["BudgetId"].ToString();

                int formulaStartRow = 0;
                int formulaEndRow = 0;
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    formulaStartRow = row;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {

                        if (TempBudgetId != ledgerData.Rows[i]["BudgetId"].ToString())
                        {

                            reportUtility.SetText(ref sheet, row, colActivity, "Closing Balance", true);
                            sheet.Range[row, colActivity, row, colActivity].Merge();
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet[row, colActivityBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row, colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            row++;
                            sheet.Range[row, 1, row, colLast].Merge();
                            row++;
                            reportUtility.SetText(ref sheet, row, colActivity, "Opening Balance", true);
                            sheet.Range[row, colBudget, row, colActivity].Merge();
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 2) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet[row, colActivityBalance].Number = clsStaticInfo.dbl(ledgerData.Rows[i]["ActivityOpeningBalance"].ToString());
                            sheet.Range[row, colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            row++;
                        }

                        int colBudgetName = col;
                        if (string.IsNullOrEmpty(budgetMasterId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["BudgetName"].ToString()); colBudgetName = col; col++;
                        }
                        int colActivityName = col;
                        if (string.IsNullOrEmpty(activityId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ActivityName"].ToString()); col++;
                        }
                        
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        }

                        sheet.Range[row, colActivityBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";

                        sheet.Range[row, colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                        col = 1;
                        TempBudgetId = ledgerData.Rows[i]["BudgetId"].ToString();
                    }
                }
                reportUtility.SetText(ref sheet, row, colActivity, "Closing Balance", true);
                sheet.Range[row, colActivity, row, colActivity].Merge();
                sheet.Range[row, colActivity, row, colActivity].CellStyle.Font.Bold = true;
                row++;
                formulaEndRow = row - 2;
                reportUtility.SetText(ref sheet, row, colActivity, "Closing Balance", true);
                sheet.Range[row, colActivity, row, colActivity].Merge();
                sheet.Range[row, colActivity, row, colActivity].CellStyle.Font.Bold = true;

                //sheet.Range[reportUtility.GetColumnNameForXls(colLast - 9) + row + ":" + reportUtility.GetColumnNameForXls(colLast - 6) + row].Merge();


                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    sheet.Range[row, colLast - 1].Formula = "=" + reportUtility.GetColumnNameForXls(colLast - 1) + (row - 2);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                row--;
                sheet.Range[row, colActivityBalance].Formula = "=" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1);
                //sheet[row, colActivityBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                sheet.Range[row, colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                sheet.Range[row, colActivityBalance].CellStyle.Font.Bold = true;
                row++;
                

                sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = false;
                sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);


                //sheet.UsedRange.CellStyle.Font.Size = 9;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "General Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();

                sheet.UsedRange.WrapText = true;
                sheet[StartRow, 1, row, colLast].CellStyle.Font.Size = 11;
                reportUtility.PageSetup4(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IWorkbook GetGeneralLedgerGroupByReport(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool active, bool IsGroupBy)
        {
            try
            {
                var row = 6;
                var StartRow = row;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
#pragma warning disable CS0219 // The variable 'borderStartCol' is assigned but its value is never used
                // var borderStartCol = 10;
#pragma warning restore CS0219 // The variable 'borderStartCol' is assigned but its value is never used

                //Get GL  heade and data
                var col = 1;
                var ob = 1;

                var colA = 1; //gl name and data
                var colB = 2;
                var colC = 3;

                var colE = 4;//marge

                var colF = 5;//account group

                //var colParticulars = 6;
                var colG = 6; //accout group value6
                var colI = 8; // marge8
                int colDocRef = 0;
                int colDocDate = 0;
                int colBaseCurrencyDebit = 0;
                int colBaseCurrencyCredit = 0;
                int colTranCurrencyDebit = 0;
                int colTranCurrencyCredit = 0;
                int colActivityBalance = 0;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                // Set Header
                var gl = _gLGeneralInfoService.GetGLData(glId);
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account Type");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["AccountTypeName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Account Group");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["AccountGroupName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, colA, "GL Name");
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["GLGeneralInfoCode"] + " - " + gl["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "RefNo");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["RefNo"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                colLast = 13;

                if (!string.IsNullOrEmpty(budgetMasterId))
                {
                    row++;
                    var budgetMaster = _budgetMasterService.GetBudgetMasterData(budgetMasterId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colA, "Budget");
                    sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, budgetMaster["UserName"].ToString());
                    sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();
                    colLast = 12;
                    //borderStartCol = 11;
                }
                if (!string.IsNullOrEmpty(activityId))
                {
                    var activity = _activityService.Find(activityId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Activity");
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, activity.UserName);
                    sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                    colLast = 11;
                }
                if (!string.IsNullOrEmpty(gl["AccountType"].ToString()))
                {
                    colLast += 1;
                }
                row++;
                ob = colLast - 3;


                // Set Row Header
                row++; //row10
                int colBudget = col;
                if (string.IsNullOrEmpty(budgetMasterId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colBudget, "Budget", 10); colBudget = col; col++;
                }
                int colActivity = col;
                if (string.IsNullOrEmpty(activityId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colActivity, "Activity", 10); colActivity = col; col++;
                }

                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); int colVoucherNo = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 14); int colPostingDate = col; col++;
                if (active==true)
                {
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref.", 14);  colDocRef = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date.", 14); colDocDate = col; col++;
                }

                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 30); int colNarration = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Party", 15); int colParty = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 18); int colParticulars = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 5); int colCurrency = col; col++;
                colTranCurrencyDebit = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 20, ExcelHAlign.HAlignRight); col++;
                colTranCurrencyCredit = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 20, ExcelHAlign.HAlignRight);
                col++;

                reportUtility.SetHeaderText(ref sheet, row - 1, colTranCurrencyDebit, "Transaction", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (row - 1)].Merge();
                sheet.Range[row - 1, colTranCurrencyDebit, row - 1, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Thin);
                int colBalance = col;
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    colBaseCurrencyDebit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 20, ExcelHAlign.HAlignRight); col++;
                    colBaseCurrencyCredit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 20, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Activity Balance", 20, ExcelHAlign.HAlignRight); colActivityBalance = col; col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 22, ExcelHAlign.HAlignRight); colBalance = col; col++;
                    sheet.Range[reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (row - 1)].Merge();
                    reportUtility.SetHeaderText(ref sheet, row - 1, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row - 1, colBaseCurrencyDebit, row - 1, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Thin);
                }
                colLast = col;
                int colDrCr = col;
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                sheet[row, col].RowHeight = 22;

                row++;

                reportUtility.SetText(ref sheet, row, colCurrency, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNo) + row + ":" + reportUtility.GetColumnNameForXls(colCurrency) + row].Merge();

                // Get bank opening balance data.
                var ledgerData = GetGeneralLedgerGroupByData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, toDate, false, null);
                var obVal = GetGeneralOpeningBalanceLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, null, null, null);


                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, colLast - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    sheet[row, colActivityBalance].Number = clsStaticInfo.dbl(ledgerData.Rows[0]["ActivityOpeningBalance"].ToString());
                    sheet.Range[row, colActivityBalance].CellStyle.Font.Bold = true;
                    sheet.Range[row, colLast - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }
                row++;
                string TempActivityId = ledgerData.Rows[0]["ActivityID"].ToString();

                int formulaStartRow = 0;
                int formulaEndRow = 0;
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    formulaStartRow = row;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {

                        if (TempActivityId != ledgerData.Rows[i]["ActivityID"].ToString())
                        {

                            reportUtility.SetText(ref sheet, row, colParty, "Closing Balance", true);
                            sheet.Range[row , colParty, row, colCurrency].Merge();
                            sheet.Range[row , colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row , colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet[row , colActivityBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row , colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            row++;
                            sheet.Range[row, 1, row, colLast].Merge();
                            row++;
                            reportUtility.SetText(ref sheet, row , colParty, "Opening Balance", true);
                            sheet.Range[row, colVoucherNo, row, colCurrency].Merge();
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 2) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet[row , colActivityBalance].Number = clsStaticInfo.dbl(ledgerData.Rows[i]["ActivityOpeningBalance"].ToString());
                            sheet.Range[row , colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            row++;
                        }
                        
                        int colBudgetName = col;
                        if (string.IsNullOrEmpty(budgetMasterId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["BudgetName"].ToString()); colBudgetName = col; col++;
                        }
                        int colActivityName = col;
                        if (string.IsNullOrEmpty(activityId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ActivityName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, colPostingDate, ledgerData.Rows[i]["PostingDate"].ToString()); col++;
                        if (active == true)
                        {
                            reportUtility.SetText(ref sheet, row, colDocRef, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, colDocDate, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, colVoucherNo, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colNarration, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colParty, ledgerData.Rows[i]["Party"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colParticulars, ledgerData.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colCurrency, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        }

                        sheet.Range[row, colActivityBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";

                        sheet.Range[row, colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                        col = 1;
                        TempActivityId = ledgerData.Rows[i]["ActivityID"].ToString();
                    }
                }
                reportUtility.SetText(ref sheet, row, colParty, "Closing Balance", true);
                sheet.Range[row, colParty, row, colCurrency].Merge();
                sheet.Range[row, colParty, row, colCurrency].CellStyle.Font.Bold = true;
                row++;
                formulaEndRow = row - 2;
                reportUtility.SetText(ref sheet, row, colParty, "Closing Balance", true);
                sheet.Range[row, colParty, row, colCurrency].Merge();
                sheet.Range[row, colParty, row, colCurrency].CellStyle.Font.Bold = true;

                //sheet.Range[reportUtility.GetColumnNameForXls(colLast - 9) + row + ":" + reportUtility.GetColumnNameForXls(colLast - 6) + row].Merge();


                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    sheet.Range[row, colLast - 1].Formula = "=" + reportUtility.GetColumnNameForXls(colLast - 1) + (row - 2);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                row--;
                sheet.Range[row, colActivityBalance].Formula = "=" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1);
                //sheet[row, colActivityBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                sheet.Range[row, colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, colActivityBalance].CellStyle.Font.Bold = true;
                row++;
                //General Ledger sum function
                sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);


                //sheet.UsedRange.CellStyle.Font.Size = 9;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "General Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();

                sheet.UsedRange.WrapText = true;
                sheet[StartRow, 1, row, colLast].CellStyle.Font.Size = 11;
                reportUtility.PageSetup4(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IWorkbook GetGeneralLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool active)
        {
            try
            {
                var row = 6;
                var StartRow = row;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
#pragma warning disable CS0219 // The variable 'borderStartCol' is assigned but its value is never used
                // var borderStartCol = 10;
#pragma warning restore CS0219 // The variable 'borderStartCol' is assigned but its value is never used

                //Get GL  heade and data
                var col = 1;
                var ob = 1;

                var colA = 1; //gl name and data
                var colB = 2;
                var colC = 3;

                var colE = 4;//marge

                var colF = 5;//account group

                //var colParticulars = 6;
                var colG = 6; //accout group value6
                var colI = 8; // marge8

                int colBaseCurrencyDebit = 0;
                int colBaseCurrencyCredit = 0;
                int colTranCurrencyDebit = 0;
                int colTranCurrencyCredit = 0;

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                // Set Header
                var gl = _gLGeneralInfoService.GetGLData(glId);
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account Type");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["AccountTypeName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Account Group");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["AccountGroupName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, colA, "GL Name");
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["GLGeneralInfoCode"] + " - " + gl["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "RefNo");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["RefNo"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                colLast = 13;

                if (!string.IsNullOrEmpty(budgetMasterId))
                {
                    row++;
                    var budgetMaster = _budgetMasterService.GetBudgetMasterData(budgetMasterId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colA, "Budget");
                    sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, budgetMaster["UserName"].ToString());
                    sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();
                    colLast = 12;
                    //borderStartCol = 11;
                }
                if (!string.IsNullOrEmpty(activityId))
                {
                    var activity = _activityService.Find(activityId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Activity");
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, activity.UserName);
                    sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                    colLast = 11;
                }
                if (!string.IsNullOrEmpty(gl["AccountType"].ToString()))
                {
                    colLast += 1;
                }
                row++;
                ob = colLast - 3;


                // Set Row Header
                row++; //row10
                int colBudget = col;
                if (string.IsNullOrEmpty(budgetMasterId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colBudget, "Budget", 10); colBudget = col; col++;
                }
                int colActivity = col;
                if (string.IsNullOrEmpty(activityId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colActivity, "Activity", 10); colActivity = col; col++;
                }
                //int colAccountType = col;
                //if (!string.IsNullOrEmpty(gl["AccountType"].ToString()))
                //{
                //    reportUtility.SetHeaderText(ref sheet, row, colAccountType, gl["AccountType"].ToString(), 16); colAccountType = col; col++;
                //}

                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); int colVoucherNo = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 14); int colPostingDate = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 30); int colNarration = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Party", 15); int colParty = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 18); int colParticulars = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 5); int colCurrency = col; col++;
                colTranCurrencyDebit = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 20, ExcelHAlign.HAlignRight); col++;
                colTranCurrencyCredit = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 20, ExcelHAlign.HAlignRight);

                col++;

                reportUtility.SetHeaderText(ref sheet, row - 1, colTranCurrencyDebit, "Transaction", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (row - 1)].Merge();
                sheet.Range[row - 1, colTranCurrencyDebit, row - 1, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Thin);
                int colBalance = col;
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    colBaseCurrencyDebit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 20, ExcelHAlign.HAlignRight); col++;
                    colBaseCurrencyCredit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 20, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 20, ExcelHAlign.HAlignRight); colBalance = col; col++;
                    sheet.Range[reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (row - 1)].Merge();
                    reportUtility.SetHeaderText(ref sheet, row - 1, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row - 1, colBaseCurrencyDebit, row - 1, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Thin);

                }
                colLast = col;
                int colDrCr = col;
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                sheet[row, col].RowHeight = 22;

                row++;

                reportUtility.SetText(ref sheet, row, colCurrency, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNo) + row + ":" + reportUtility.GetColumnNameForXls(colCurrency) + row].Merge();

                // Get bank opening balance data.
                var obVal = GetGeneralOpeningBalanceLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, null, null, null);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, colLast - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                }

                if (!string.IsNullOrEmpty(companyCurrencyId))
                {

                }

                row++;
                // Get GL transaction data.


                int formulaStartRow = 0;
                int formulaEndRow = 0;
                var ledgerData = GetGeneralLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, toDate, false, null, null, null, null);
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    formulaStartRow = row;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        int colBudgetName = col;
                        if (string.IsNullOrEmpty(budgetMasterId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["BudgetName"].ToString()); colBudgetName = col; col++;
                        }
                        int colActivityName = col;
                        if (string.IsNullOrEmpty(activityId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ActivityName"].ToString()); col++;
                        }


                        //sheet[row, col].ColumnWidth = 10;

                        reportUtility.SetText(ref sheet, row, colPostingDate, ledgerData.Rows[i]["PostingDate"].ToString()); col++;


                        reportUtility.SetText(ref sheet, row, colVoucherNo, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colNarration, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colParty, ledgerData.Rows[i]["Party"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colParticulars, ledgerData.Rows[i]["Particular"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colCurrency, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 3) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 2) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        }


                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                        col = 1;
                    }
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colLast - 6, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colLast - 9) + row + ":" + reportUtility.GetColumnNameForXls(colLast - 6) + row].Merge();


                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    sheet.Range[row, colLast - 1].Formula = "=" + reportUtility.GetColumnNameForXls(colLast - 1) + (row - 1);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                //General Ledger sum function
                sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);


                //sheet.UsedRange.CellStyle.Font.Size = 9;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "General Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();

                sheet.UsedRange.WrapText = true;
                sheet[StartRow, 1, row, colLast].CellStyle.Font.Size = 11;
                reportUtility.PageSetup4(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetGeneralLedgerReportWithDocRef(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool active, string bankMasterId, string cashMasterId, string partyId)
        {
            try
            {
                var row = 6;
                var StartRow = row;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
#pragma warning disable CS0219 // The variable 'borderStartCol' is assigned but its value is never used
                // var borderStartCol = 10;
#pragma warning restore CS0219 // The variable 'borderStartCol' is assigned but its value is never used

                //Get GL  heade and data
                var col = 1;
                var ob = 1;

                var colA = 1; //gl name and data
                var colB = 2;
                var colC = 3;

                var colE = 4;//marge

                var colF = 5;//account group

                //var colParticulars = 6;
                var colG = 6; //accout group value6
                var colI = 8; // marge8

                int colBaseCurrencyDebit = 0;
                int colBaseCurrencyCredit = 0;
                int colTranCurrencyDebit = 0;
                int colTranCurrencyCredit = 0;

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                // Set Header
                var gl = _gLGeneralInfoService.GetGLData(glId);
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account Type");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["AccountTypeName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Account Group");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["AccountGroupName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, colA, "GL Name");
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["GLGeneralInfoCode"] + " - " + gl["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "RefNo");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["RefNo"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                colLast = 13;

                if (!string.IsNullOrEmpty(budgetMasterId))
                {
                    row++;
                    var budgetMaster = _budgetMasterService.GetBudgetMasterData(budgetMasterId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colA, "Budget");
                    sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, budgetMaster["UserName"].ToString());
                    sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();
                    colLast = 12;
                    //borderStartCol = 11;
                }
                if (!string.IsNullOrEmpty(activityId))
                {
                    var activity = _activityService.Find(activityId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Activity");
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, activity.UserName);
                    sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                    colLast = 11;
                }
                if (!string.IsNullOrEmpty(gl["AccountType"].ToString()))
                {
                    colLast += 1;
                }
                row++;
                ob = colLast - 3;


                // Set Row Header
                row++; //row10
                int colBudget = col;
                if (string.IsNullOrEmpty(budgetMasterId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colBudget, "Budget", 10); colBudget = col; col++;
                }
                int colActivity = col;
                if (string.IsNullOrEmpty(activityId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colActivity, "Activity", 10); colActivity = col; col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 17); int colVoucherNo = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 14); int colPostingDate = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Source Type", 14); int colSourceType = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref.", 14); int colDocRef = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 14); int colDocDate = col; col++;

                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 30); int colNarration = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Party", 15); int colParty = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 20); int colParticulars = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 5); int colCurrency = col; col++;
                colTranCurrencyDebit = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 20, ExcelHAlign.HAlignRight); col++;
                colTranCurrencyCredit = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 20, ExcelHAlign.HAlignRight);

                col++;

                reportUtility.SetHeaderText(ref sheet, row - 1, colTranCurrencyDebit, "Transaction", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (row - 1)].Merge();
                sheet.Range[row - 1, colTranCurrencyDebit, row - 1, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Thin);
                int colBalance = col;
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    colBaseCurrencyDebit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 20, ExcelHAlign.HAlignRight); col++;
                    colBaseCurrencyCredit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 20, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 20, ExcelHAlign.HAlignRight); colBalance = col; col++;
                    sheet.Range[reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (row - 1)].Merge();
                    reportUtility.SetHeaderText(ref sheet, row - 1, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row - 1, colBaseCurrencyDebit, row - 1, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Thin);

                }
                colLast = col;
                int colDrCr = col;
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                sheet[row, col].RowHeight = 22;

                row++;

                reportUtility.SetText(ref sheet, row, colCurrency, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNo) + row + ":" + reportUtility.GetColumnNameForXls(colCurrency) + row].Merge();

                // Get bank opening balance data.
                var obVal = GetGeneralOpeningBalanceLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, bankMasterId, cashMasterId, partyId);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, colLast - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                }

                if (!string.IsNullOrEmpty(companyCurrencyId))
                {

                }

                row++;
                // Get GL transaction data.


                int formulaStartRow = 0;
                int formulaEndRow = 0;
                var ledgerData = GetGeneralLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, toDate, false, null, bankMasterId, cashMasterId, partyId);
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    formulaStartRow = row;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        int colBudgetName = col;
                        if (string.IsNullOrEmpty(budgetMasterId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["BudgetName"].ToString()); colBudgetName = col; col++;
                        }
                        int colActivityName = col;
                        if (string.IsNullOrEmpty(activityId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ActivityName"].ToString()); col++;
                        }


                        //sheet[row, col].ColumnWidth = 10;

                        reportUtility.SetText(ref sheet, row, colPostingDate, ledgerData.Rows[i]["PostingDate"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colSourceType, ledgerData.Rows[i]["SourceType"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colDocRef, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colDocDate, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colVoucherNo, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colNarration, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colParty, ledgerData.Rows[i]["Party"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colParticulars, ledgerData.Rows[i]["Particular"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colCurrency, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 3) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 2) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        }


                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);

                        row++;
                        col = 1;
                    }
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colLast - 6, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colLast - 9) + row + ":" + reportUtility.GetColumnNameForXls(colLast - 6) + row].Merge();


                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    sheet.Range[row, colLast - 1].Formula = "=" + reportUtility.GetColumnNameForXls(colLast - 1) + (row - 1);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                //General Ledger sum function
                sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);


                //sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);
                //sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);


                //sheet.UsedRange.CellStyle.Font.Size = 9;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "General Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();

                sheet.UsedRange.WrapText = true;
                sheet[StartRow, 1, row, colLast].CellStyle.Font.Size = 11;
                reportUtility.PageSetup4(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IWorkbook GetGeneralLedgerGSTReportWithDocRef(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool active)
        {
            try
            {
                var row = 8;
                var StartRow = row;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "GST Ledger";

                //Get GL  heade and data
                var col = 1;
                var ob = 1;

                var colA = 1; //gl name and data
                var colB = 2;
                var colC = 3;

                var colE = 4;//marge

                var colF = 5;//account group

                //var colParticulars = 6;
                var colG = 6; //accout group value6
                var colI = 8; // marge8

                int colBaseCurrencyDebit = 0;
                int colBaseCurrencyCredit = 0;
                int colTranCurrencyDebit = 0;
                int colTranCurrencyCredit = 0;

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                // Set Header
                var gl = _gLGeneralInfoService.GetGLData(glId);
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account Type");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["AccountTypeName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Account Group");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["AccountGroupName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, colA, "GL Name");
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, gl["GLGeneralInfoCode"] + " - " + gl["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, colF, "RefNo");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, gl["RefNo"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                colLast = 13;

                if (!string.IsNullOrEmpty(budgetMasterId))
                {
                    row++;
                    var budgetMaster = _budgetMasterService.GetBudgetMasterData(budgetMasterId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colA, "Budget");
                    sheet.Range[reportUtility.GetColumnNameForXls(colA) + row + ":" + reportUtility.GetColumnNameForXls(colB) + row].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colC, budgetMaster["UserName"].ToString());
                    sheet.Range[reportUtility.GetColumnNameForXls(colC) + row + ": " + reportUtility.GetColumnNameForXls(colE) + row].Merge();
                    colLast = 12;
                    //borderStartCol = 11;
                }
                if (!string.IsNullOrEmpty(activityId))
                {
                    var activity = _activityService.Find(activityId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, colF, "Activity");
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, colG, activity.UserName);
                    sheet.Range[reportUtility.GetColumnNameForXls(colG) + row + ": " + reportUtility.GetColumnNameForXls(colI) + row].Merge();
                    colLast = 11;
                }
                if (!string.IsNullOrEmpty(gl["AccountType"].ToString()))
                {
                    colLast += 1;
                }
                row++;
                ob = colLast - 3;


                // Set Row Header
                row++; //row10
                int colBudget = col;
                if (string.IsNullOrEmpty(budgetMasterId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colBudget, "Budget", 10); colBudget = col; col++;
                }
                int colActivity = col;
                if (string.IsNullOrEmpty(activityId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colActivity, "Activity", 10); colActivity = col; col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); int colVoucherNo = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher Type", 15); int colVoucherType = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 14); int colPostingDate = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref.", 14); int colDocRef = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 14); int colDocDate = col; col++;

                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 30); int colNarration = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Party", 15); int colParty = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Party Tax No", 18); int colPartyTaxNo = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Tax Category", 15); int colTaxCetegory = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Taxable Amount", 18); int colTaxableAmount = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 5); int colCurrency = col; col++;
                colTranCurrencyDebit = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 13, ExcelHAlign.HAlignRight); col++;
                colTranCurrencyCredit = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 13, ExcelHAlign.HAlignRight);

                col++;

                reportUtility.SetHeaderText(ref sheet, row - 1, colTranCurrencyDebit, "Transaction", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (row - 1)].Merge();
                sheet.Range[row - 1, colTranCurrencyDebit, row - 1, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Thin);
                int colBalance = col;
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    colBaseCurrencyDebit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 13, ExcelHAlign.HAlignRight); col++;
                    colBaseCurrencyCredit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 13, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 16, ExcelHAlign.HAlignRight); colBalance = col; col++;
                    sheet.Range[reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (row - 1)].Merge();
                    reportUtility.SetHeaderText(ref sheet, row - 1, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row - 1, colBaseCurrencyDebit, row - 1, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Thin);

                }
                colLast = col;
                int colDrCr = col;
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                sheet[row, col].RowHeight = 22;

                row++;

                reportUtility.SetText(ref sheet, row, colCurrency, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNo) + row + ":" + reportUtility.GetColumnNameForXls(colCurrency) + row].Merge();

                // Get bank opening balance data.
                var obVal = GetGeneralOpeningBalanceLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, null, null, null);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, colLast - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                }

                if (!string.IsNullOrEmpty(companyCurrencyId))
                {

                }

                row++;
                // Get GL transaction data.


                int formulaStartRow = 0;
                int formulaEndRow = 0;
                var ledgerData = GetGeneralLedgerGSTData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, toDate, false, null);
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    formulaStartRow = row;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        int colBudgetName = col;
                        if (string.IsNullOrEmpty(budgetMasterId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["BudgetName"].ToString()); colBudgetName = col; col++;
                        }
                        int colActivityName = col;
                        if (string.IsNullOrEmpty(activityId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ActivityName"].ToString()); col++;
                        }


                        //sheet[row, col].ColumnWidth = 10;

                        reportUtility.SetText(ref sheet, row, colPostingDate, ledgerData.Rows[i]["PostingDate"].ToString()); col++;


                        reportUtility.SetText(ref sheet, row, colDocRef, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colDocDate, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colVoucherNo, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colVoucherType, ledgerData.Rows[i]["VoucherType"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colNarration, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colParty, ledgerData.Rows[i]["PartyName"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colPartyTaxNo, ledgerData.Rows[i]["PartyTaxNo"].ToString()); col++;

                        reportUtility.SetText(ref sheet, row, colTaxCetegory, ledgerData.Rows[i]["TaxCetegory"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colTaxableAmount, Convert.ToDouble(ledgerData.Rows[i]["TaxableAmount"].ToString())); col++;

                        reportUtility.SetText(ref sheet, row, colCurrency, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 3) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 2) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        }


                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                        col = 1;
                    }
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colLast - 6, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colLast - 9) + row + ":" + reportUtility.GetColumnNameForXls(colLast - 6) + row].Merge();


                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    sheet.Range[row, colLast - 1].Formula = "=" + reportUtility.GetColumnNameForXls(colLast - 1) + (row - 1);
                    sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                //General Ledger sum function
                sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow + 5, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);


                //sheet.UsedRange.CellStyle.Font.Size = 9;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "GST Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colA) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();

                sheet.UsedRange.WrapText = true;
                sheet[StartRow, 1, row, colLast].CellStyle.Font.Size = 11;
                reportUtility.PageSetup4(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetLCLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string lCRef)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "LCLedger";
                var colLast = 6;
                var colLast1 = 6;
                var col = 1;
                var StartRow = 9;
                
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "LC No");
                //sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, lCRef);
                sheet.Range[row, 2, row, 4].Merge();
                // sheet.Range[row, 3, row, 5].RowHeight = 30;
                
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 3].BorderAround(ExcelLineStyle.Thin);

                colLast = colLast + 3;
                
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 4].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 4].BorderAround();
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 25); col++;
                
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref No", 25); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 25); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 25); col++;

                sheet.Range[row, col].WrapText = true;

                
                reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignLeft); col++;

                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
               
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 10, ExcelHAlign.HAlignRight);
                
                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 20;
                // Get party opening balance data.
                var obVal = GetLCOpeningBalance(companyGroupId, companyId, plantId, fromDate, lCRef);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerData = GetLCLedger(companyGroupId, companyId, plantId, fromDate, toDate, lCRef);
                row++;
                int sumStrRow = 0;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;

                        sheet.Range[row, col].WrapText = true;

                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;

                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";

                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                //sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;

                var endCol = col;
                sheet.UsedRange.CellStyle.Font.Size = 8;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);

                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[StartRow, 1, row, endCol].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow, 1, row, endCol].BorderAround(ExcelLineStyle.Thin);

                reportUtility.CompanyPlantHeader(ref sheet, col, "LC Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private List<Dictionary<string, object>> GetLCOpeningBalance(string companyGroupId, string companyId, string plantId, string fromDate, string lCRef)
        {
            var sql = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"',@companyId VARCHAR(10)='" + companyId + @"',@plantId VARCHAR(10)='" + plantId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
                        LEFT JOIN [TRN].[Invoice] AS IV ON IV.Id=IVD.InvoiceId
                        LEFT JOIN [TRN].[PurchaseDocAcceptance] AS PDA ON PDA.Id=IV.PurchaseDocAcceptanceId
                        LEFT JOIN [dbo].PurchaseLC AS PLC ON PLC.Id=PDA.PurchaseLCId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId   AND V.PostingDate < '" + fromDate.ToDbDate() + @"' AND  PLC.LCRef='" + lCRef + @"' GROUP BY CC.CompanyCurrencyId

                        UNION ALL
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [dbo].[PurchaseLCCharges]  AS PLCC ON PLCC.VoucherId=VD.VoucherId
                        LEFT JOIN [dbo].PurchaseLC AS PLC ON PLC.Id=PLCC.PurchaseLCId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId   AND V.PostingDate < '" + fromDate.ToDbDate() + @"' AND  PLC.LCRef='" + lCRef + @"' AND VD.BankMasterId is null  GROUP BY CC.CompanyCurrencyId

                        UNION ALL
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[FinancingDetail]  AS FD ON FD.Id=VD.FinancingDetailId
						LEFT JOIN [TRN].[Financing]  AS F ON F.Id=FD.FinancingId
                        LEFT JOIN [dbo].[InvoiceTaggingWithLCMaster]  AS ITLM ON ITLM.Id=F.InvoiceTaggingWithLCMasterId
                        LEFT JOIN [dbo].PurchaseLC AS PLC ON PLC.Id=ITLM.PurchaseLCId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId   AND V.PostingDate < '" + fromDate.ToDbDate() + @"' AND  PLC.LCRef='" + lCRef + @"'   GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId";
            
            return _sqlRepository.GetDataCollection(sql);
        }
        private DataTable GetLCLedger(string companyGroupId, string companyId, string plantId,  string fromDate, string toDate, string lCRef)
        {
            var cmdText = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"',@companyId VARCHAR(10)='" + companyId + @"',@plantId VARCHAR(10)='" + plantId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
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
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
                            LEFT JOIN [TRN].[Invoice] AS IV ON IV.Id=IVD.InvoiceId
                            LEFT JOIN [TRN].[PurchaseDocAcceptance] AS PDA ON PDA.Id=IV.PurchaseDocAcceptanceId
                            LEFT JOIN [dbo].PurchaseLC AS PLC ON PLC.Id=PDA.PurchaseLCId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND  PLC.LCRef='" + lCRef + @"' AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'

                            UNION ALL                            
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
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
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            LEFT JOIN [dbo].[PurchaseLCCharges]  AS PLCC ON PLCC.VoucherId=VD.VoucherId
                            LEFT JOIN [dbo].PurchaseLC AS PLC ON PLC.Id=PLCC.PurchaseLCId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND  PLC.LCRef='" + lCRef + @"' AND VD.BankMasterId is null AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'

                            UNION ALL                            
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
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
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            LEFT JOIN [TRN].[FinancingDetail]  AS FD ON FD.Id=VD.FinancingDetailId
							LEFT JOIN [TRN].[Financing]  AS F ON F.Id=FD.FinancingId
                            LEFT JOIN [dbo].[InvoiceTaggingWithLCMaster]  AS ITLM ON ITLM.Id=F.InvoiceTaggingWithLCMasterId
                            LEFT JOIN [dbo].PurchaseLC AS PLC ON PLC.Id=ITLM.PurchaseLCId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND  PLC.LCRef='" + lCRef + @"'  AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"' ";

            return _sqlRepository.GetDataTable(cmdText);
        }
        public IWorkbook GetFixedAssetObReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Fixed Asset";

                var fiscalYear = _sqlRepository.GetData("SELECT FiscalYearCode, FiscalYearName, StartDate, EndDate FROM [SCS].[FiscalYear] WHERE Id='" + fiscalYearId + "'");
                var dsLocal = GetFixedAssetObData(companyGroupId, companyId, plantId, fiscalYearId);

                if (dsLocal.Rows.Count > 0)
                {
                    row++;
                    reportUtility.SetHeaderText(ref sheet, row, 1, "Fixed Asset", 28);
                    reportUtility.SetHeaderText(ref sheet, row, 2, "GL", 28);
                    reportUtility.SetHeaderText(ref sheet, row, 3, "Budget", 30);
                    reportUtility.SetHeaderText(ref sheet, row, 4, "Activity", 20);
                    reportUtility.SetHeaderText(ref sheet, row, 5, "Asset Value", 12);
                    reportUtility.SetHeaderText(ref sheet, row, 6, "AccDep Amount", 12);
                    reportUtility.SetHeaderText(ref sheet, row, 7, "Net Book Value", 12);

                    row++;

                    for (int i = 0; i < dsLocal.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, 1, dsLocal.Rows[i]["FixedAssetMasterName"].ToString());
                        reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[i]["AssetGLName"].ToString());
                        reportUtility.SetText(ref sheet, row, 3, dsLocal.Rows[i]["BudgetName"].ToString());
                        reportUtility.SetText(ref sheet, row, 4, dsLocal.Rows[i]["AssetActivityName"].ToString());
                        reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(dsLocal.Rows[i]["FixedAssetValue"].ToString()));
                        reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(dsLocal.Rows[i]["AccDepAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(dsLocal.Rows[i]["NetBookValue"].ToString()));
                        row++;
                    }
                }
                row = row - 1;

                sheet.Range[8, 1, row, 7].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, 7].BorderAround(ExcelLineStyle.Hair);
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, "Fixed Asset Opening Balance", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, 7, "Fiscal Year : " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                sheet.Range[5, 1, 5, 7].Merge();
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //JOurnal old format
        public IWorkbook GetAdvanceJournalVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entry Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

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
                        reportUtility.NumberFormatDecimalFour(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.NumberFormatDecimalFour(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalFour();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalFour();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
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
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }


        //Employee Payment report  New
        public IWorkbook GetAdvanceJournalVoucherReport1(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            sheet[row, 1].ColumnWidth = 20;
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            sheet[row, 2].ColumnWidth = 15;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            // sheet[row, 3].ColumnWidth = 15;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 7, header["VoucherDate"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "DocDate");
            reportUtility.SetText(ref sheet, row, 7, header["DocDate"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;

            row++;
            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee");
            ////reportUtility.SetText(ref sheet, row, 2, header["EmployeeCode"].ToString() + " - " + header["EmployeeName"].ToString());
            //sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;



            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;


            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 7, header["DocRefNo"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            // reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            //reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            //sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Status");
            reportUtility.SetText(ref sheet, row, 7, header["Status"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;

            //row++;
            row++;  //10
            colLast = companyCurrencyId == transcationCurrency ? 7 : 9;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 8, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 8, row, 9].Merge();
            }
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, 6, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 6, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;

            int colGl = 0;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; //clo3

            xlsCol++; //cloDNaration
                      // int colDnaration = 0;
                      // reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Detail Narration"); colDnaration = xlsCol;
            sheet[row, 4].ColumnWidth = 15;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
            // sheet.ShowColumn(4, false); 
            //sheet.HideColumn(5);
            //sheet[1, 5].ColumnWidth = 0; 


            xlsCol++; //clo5
            int colParticulars = 0;
            colParticulars = xlsCol;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars");
            sheet[row, colParticulars].ColumnWidth = 20;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
            xlsCol++;

            //xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol; //col9

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }

            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();



            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    //var glName = dsLocal.Rows[i]["Budget"].ToString();
                    // glName = string.Empty;
                    //reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    // sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();

                    //reportUtility.SetText(ref sheet, row, colDnaration, dsLocal.Rows[i]["DetailNarration"].ToString());
                    //sheet[row, colDnaration].RowHeight = 25;
                    //sheet[row, colDnaration].WrapText = true;

                    //reportUtility.SetText(ref sheet, row, colApprovedBy, dsLocal.Rows[i]["ApprovedBy"].ToString());



                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 3) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());




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

                    // glName = string.Empty;

                    // sheet.AutofitRow(3);



                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 5, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 21;

                // reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["AddedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Received By", true);
                sheet[row, 3].ColumnWidth = 15;



                reportUtility.SetSignatureText(ref sheet, row - 1, 5, header["PostedBy"].ToString());
                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Checked By", true);
                //sheet[row, 5].ColumnWidth = 15;

                sheet.Range[row, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 7, "Authorized By", true);
                sheet[row, 6].ColumnWidth = 15;
                sheet[row, 7].ColumnWidth = 15;

                sheet[row, 8].ColumnWidth = 15;
                sheet[row, 9].ColumnWidth = 15;


                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Journal", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

                //    //else
                //    //{
                //    //    sheet.UsedRange.WrapText = true;
                //    //    sheet.UsedRange.CellStyle.Font.Size = 8;
                //    //    reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                //    //    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 9, "Journal", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 9, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }


        public IWorkbook GetDashboardJournalVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";
            string plantName = _sqlRepository.GetDataTable(@"Select * from ORG.Plant where Id = '" + plantId + @"'").Rows[0]["UserName"].ToString();
            var header = GetDashboardJournalHeader(companyGroupId, companyId, plantId, voucherId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            if (header["BeneficiaryType"].ToString() != null)
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Beneficiary - (" + header["Beneficiary"].ToString() + ")");
                reportUtility.SetText(ref sheet, row, 2, header["BeneficiaryName"].ToString(), ExcelHAlign.HAlignLeft);
                sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
            }
            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

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


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
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
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        #region Voucher Report
        public Dictionary<string, object> GetPrintNonCashCheckVoucherReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, string voucherDetailId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
                           -- , V.AddedBy, V.PostedBy
                            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
                            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END

                            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode,EB.BeneficiaryType,EB.EmployeeId	,p.UserName Party
							,Beneficiary=CASE WHEN EB.EmployeeId<>'' THEN 'Employee' WHEN EB.PartyId<>'' THEN 'Party' ELSE NULL end
							,BeneficiaryName=CASE WHEN EB.EmployeeId<>'' THEN EI.EmployeeName WHEN EB.PartyId<>'' THEN P.UserName ELSE NULL end
							,VD.CheckLotDetailId, CLD.CheckNumber,CLH.CheckDate
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN TRN.ExpenseBooking AS EB ON EB.VoucherId=V.Id 
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
							LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.BankMasterId<>''
							--LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
	                        left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD ON CLD.Id=CLH.CheckLotDetailId
                             LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.Id='" + voucherDetailId + "'  order by CLD.CheckNumber desc ";
            return _sqlRepository.GetData(cmdText);
        }
        public DataTable GetPrintNonCashCheckVoucherReportData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                , VD.Id AS BudgetMasterId, BUD.UserName AS Budget, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId,BM.AccountTitle as AccountTitleName
                ,[ParticularName]=CASE
                WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
                WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                WHEN P.UserName<>'' THEN P.UserName
                WHEN CM.UserName<>'' THEN CM.UserName
                WHEN FAM.UserName<>'' THEN FAM.UserName
                ELSE '' END
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
                LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
                LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
                LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }


        public IWorkbook GetPrintNonCashCheckVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string voucherDetailId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "NonCashPrint";

            // var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);
            // var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            var header = GetPrintNonCashCheckVoucherReportHeader(companyGroupId, companyId, plantId, voucherId, voucherDetailId);
            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //  var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            //var dsLocal = _bankJournalService.GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            var dsLocal = GetPrintNonCashCheckVoucherReportData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colBaseCurrencyDebit = 0;
            int colBaseCurrencyCredit = 0;
            int colTranCurrencyDebit = 0;
            int colTranCurrencyCredit = 0;

            int colVoucherNo = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            sheet[row, colVoucherNo].ColumnWidth = 18;
            sheet.Range[row, colVoucherNo].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());
            sheet[row, colVoucherNoValue].ColumnWidth = 12;
            sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            xlsCol++; //3
            int colReceived = xlsCol;
            xlsCol++;//4
            sheet[row, xlsCol].ColumnWidth = 10;

            xlsCol++; //5
            int colParticulars = xlsCol;

            xlsCol++;//6
            int colVoucherDate = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            sheet.Range[row, colVoucherDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;//7
            int colVoucherDateValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet.Range[row, colVoucherDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colPostingDate = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            sheet.Range[row, colPostingDate].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());
            sheet.Range[row, colPostingDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocDate = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            sheet.Range[row, colDocDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            sheet.Range[row, colDocDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;


            int colCheckNo = colVoucherNo;
            int colCheckNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckNo, "CheckNo");
            reportUtility.SetText(ref sheet, row, colCheckNoValue, header["CheckNumber"].ToString());


            int colCheckDate = colVoucherDate;
            int colCheckDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());
            row++;

            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());




            int colDocRef = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRef, "Doc Ref");
            sheet.Range[row, colDocRef].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocRefValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocRefValue, header["DocRefNo"].ToString());
            sheet.Range[row, colDocRefValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colNaration = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNaration, "Narration");
            int colNarationValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colNarationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

            sheet.Range[row, colNaration].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colNarationValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            int colStatus = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            int colStatusValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());
            sheet.Range[row, colStatus].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colStatusValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;  //10

            //    int colParty = colVoucherNo;
            //    int colPartyValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            //    reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());


            //    int colFiscalYearName = colVoucherNo;
            //    int colFiscalYearNameValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "CheckNo");
            //    reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["CheckNumber"].ToString());


            //    int colCheckDate = colDocRefNo;
            //    int colCheckDateValue = colDocRefNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            //    reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());

            colTranCurrencyDebit = colVoucherDate; //col6
            colTranCurrencyCredit = colVoucherDateValue; //7
            xlsCol++; //8 
            colBaseCurrencyDebit = xlsCol;
            xlsCol++; //9 
            colBaseCurrencyCredit = xlsCol;

            colLast = companyCurrencyId == transcationCurrency ? colTranCurrencyCredit : colBaseCurrencyCredit;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colBaseCurrencyDebit, row, colBaseCurrencyCredit].Merge();
            }
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            int colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();


            //reportUtility.SetHeaderText(ref sheet, row, colParticulars, "Particulars");
            //sheet[row, colParticulars].ColumnWidth = 23;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colusdCradit = xlsCol;
                colLast = colBaseCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colTranCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    // glName = string.Empty;

                    // var glName = dsLocal.Rows[i]["BudgetName"].ToString(); 
                    var glName = dsLocal.Rows[i]["Budget"].ToString();

                    // reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["AccountTitleName"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 4) + row].Merge();

                    // reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colParticulars, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, colVoucherNo, header["AddedBy"].ToString());
                sheet.Range[row, colVoucherNo].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherNo, "Prepared By", true);
                sheet[row, colVoucherNo].ColumnWidth = 18;


                reportUtility.SetTextMiddle(ref sheet, row, colReceived, "Received By", true);
                sheet[row, colReceived].ColumnWidth = 14;
                sheet.Range[row, colReceived].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars, header["PostedBy"].ToString());
                sheet.Range[row, colParticulars].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colParticulars, "Checked By", true);

                reportUtility.SetTextMiddle(ref sheet, row, colTranCurrencyCredit, "Authorized By", true);
                sheet[row, colTranCurrencyDebit].ColumnWidth = 15;
                sheet[row, colTranCurrencyCredit].ColumnWidth = 15;
                sheet.Range[row, colTranCurrencyCredit].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                sheet[row, colBaseCurrencyDebit].ColumnWidth = 15;
                sheet[row, colBaseCurrencyCredit].ColumnWidth = 15;


                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }
        //check management report


        //check void voucher report
        public Dictionary<string, object> GetPrintCheckVoidVoucherReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, string voucherDetailId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
                           -- , V.AddedBy, V.PostedBy
                            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
                            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END

                            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode,EB.BeneficiaryType,EB.EmployeeId	,p.UserName Party
							,Beneficiary=CASE WHEN EB.EmployeeId<>'' THEN 'Employee' WHEN EB.PartyId<>'' THEN 'Party' ELSE NULL end
							,BeneficiaryName=CASE WHEN EB.EmployeeId<>'' THEN EI.EmployeeName WHEN EB.PartyId<>'' THEN P.UserName ELSE NULL end
							,VD.CheckLotDetailId, CLD.CheckNumber,CLH.CheckDate
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN TRN.ExpenseBooking AS EB ON EB.VoucherId=V.Id 
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
							LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.BankMasterId<>''
							--LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
	                        left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD ON CLD.Id=CLH.CheckLotDetailId
                             LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.Id='" + voucherDetailId + "'  order by CLD.CheckNumber desc ";
            return _sqlRepository.GetData(cmdText);
        }
        public DataTable GetPrintCheckVoidVoucherReportData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                , VD.Id AS BudgetMasterId, BUD.UserName AS Budget, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId,BM.AccountTitle as AccountTitleName
                ,[ParticularName]=CASE
                WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
                WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                WHEN P.UserName<>'' THEN P.UserName
                WHEN CM.UserName<>'' THEN CM.UserName
                WHEN FAM.UserName<>'' THEN FAM.UserName
                ELSE '' END
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
                LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
                LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
                LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetPrintCheckVoidVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string voucherDetailId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "NonCashPrint";

            // var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);
            // var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            var header = GetPrintCheckVoidVoucherReportHeader(companyGroupId, companyId, plantId, voucherId, voucherDetailId);
            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //  var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            //var dsLocal = _bankJournalService.GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            var dsLocal = GetPrintCheckVoidVoucherReportData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colBaseCurrencyDebit = 0;
            int colBaseCurrencyCredit = 0;
            int colTranCurrencyDebit = 0;
            int colTranCurrencyCredit = 0;

            int colVoucherNo = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            sheet[row, colVoucherNo].ColumnWidth = 18;
            sheet.Range[row, colVoucherNo].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());
            sheet[row, colVoucherNoValue].ColumnWidth = 12;
            sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            xlsCol++; //3
            int colReceived = xlsCol;
            xlsCol++;//4
            sheet[row, xlsCol].ColumnWidth = 10;

            xlsCol++; //5
            int colParticulars = xlsCol;

            xlsCol++;//6
            int colVoucherDate = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            sheet.Range[row, colVoucherDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;//7
            int colVoucherDateValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet.Range[row, colVoucherDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colPostingDate = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            sheet.Range[row, colPostingDate].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());
            sheet.Range[row, colPostingDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocDate = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            sheet.Range[row, colDocDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            sheet.Range[row, colDocDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;


            int colCheckNo = colVoucherNo;
            int colCheckNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckNo, "CheckNo");
            reportUtility.SetText(ref sheet, row, colCheckNoValue, header["CheckNumber"].ToString());


            int colCheckDate = colVoucherDate;
            int colCheckDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());
            row++;

            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());




            int colDocRef = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRef, "Doc Ref");
            sheet.Range[row, colDocRef].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocRefValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocRefValue, header["DocRefNo"].ToString());
            sheet.Range[row, colDocRefValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colNaration = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNaration, "Narration");
            int colNarationValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colNarationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

            sheet.Range[row, colNaration].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colNarationValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            int colStatus = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            int colStatusValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());
            sheet.Range[row, colStatus].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colStatusValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;  //10

            //    int colParty = colVoucherNo;
            //    int colPartyValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            //    reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());


            //    int colFiscalYearName = colVoucherNo;
            //    int colFiscalYearNameValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "CheckNo");
            //    reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["CheckNumber"].ToString());


            //    int colCheckDate = colDocRefNo;
            //    int colCheckDateValue = colDocRefNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            //    reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());

            colTranCurrencyDebit = colVoucherDate; //col6
            colTranCurrencyCredit = colVoucherDateValue; //7
            xlsCol++; //8 
            colBaseCurrencyDebit = xlsCol;
            xlsCol++; //9 
            colBaseCurrencyCredit = xlsCol;

            colLast = companyCurrencyId == transcationCurrency ? colTranCurrencyCredit : colBaseCurrencyCredit;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colBaseCurrencyDebit, row, colBaseCurrencyCredit].Merge();
            }
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            int colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();


            //reportUtility.SetHeaderText(ref sheet, row, colParticulars, "Particulars");
            //sheet[row, colParticulars].ColumnWidth = 23;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colusdCradit = xlsCol;
                colLast = colBaseCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colTranCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    // glName = string.Empty;

                    // var glName = dsLocal.Rows[i]["BudgetName"].ToString(); 
                    var glName = dsLocal.Rows[i]["Budget"].ToString();

                    // reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["AccountTitleName"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 4) + row].Merge();

                    // reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colParticulars, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, colVoucherNo, header["AddedBy"].ToString());
                sheet.Range[row, colVoucherNo].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherNo, "Prepared By", true);
                sheet[row, colVoucherNo].ColumnWidth = 18;


                reportUtility.SetTextMiddle(ref sheet, row, colReceived, "Received By", true);
                sheet[row, colReceived].ColumnWidth = 14;
                sheet.Range[row, colReceived].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars, header["PostedBy"].ToString());
                sheet.Range[row, colParticulars].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colParticulars, "Checked By", true);

                reportUtility.SetTextMiddle(ref sheet, row, colTranCurrencyCredit, "Authorized By", true);
                sheet[row, colTranCurrencyDebit].ColumnWidth = 15;
                sheet[row, colTranCurrencyCredit].ColumnWidth = 15;
                sheet.Range[row, colTranCurrencyCredit].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                sheet[row, colBaseCurrencyDebit].ColumnWidth = 15;
                sheet[row, colBaseCurrencyCredit].ColumnWidth = 15;


                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }




        public Dictionary<string, object> GetRePrintNonCashCheckVoucherReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, string voucherDetailId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
                           -- , V.AddedBy, V.PostedBy
                            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
                            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END

                            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode,EB.BeneficiaryType,EB.EmployeeId	,p.UserName Party
							,Beneficiary=CASE WHEN EB.EmployeeId<>'' THEN 'Employee' WHEN EB.PartyId<>'' THEN 'Party' ELSE NULL end
							,BeneficiaryName=CASE WHEN EB.EmployeeId<>'' THEN EI.EmployeeName WHEN EB.PartyId<>'' THEN P.UserName ELSE NULL end
							,VD.CheckLotDetailId, CLD.CheckNumber,CLH.CheckDate
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN TRN.ExpenseBooking AS EB ON EB.VoucherId=V.Id 
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
							LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.BankMasterId<>''
							--LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
	                        left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD ON CLD.Id=CLH.CheckLotDetailId
                             LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
							
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.Id='" + voucherDetailId + "'  order by CLD.CheckNumber desc ";
            return _sqlRepository.GetData(cmdText);
        }
        public DataTable GetRePrintNonCashCheckVoucherReportData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                , VD.Id AS BudgetMasterId, BUD.UserName AS Budget, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId,BM.AccountTitle as AccountTitleName
                ,[ParticularName]=CASE
                WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
                WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                WHEN P.UserName<>'' THEN P.UserName
                WHEN CM.UserName<>'' THEN CM.UserName
                WHEN FAM.UserName<>'' THEN FAM.UserName
                ELSE '' END
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
                LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
                LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
                LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }



        public IWorkbook GetRePrintNonCashCheckVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string voucherDetailId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "NonCashRePrint";

            // var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);
            // var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            //var header = GetPrintNonCashCheckVoucherReportHeader(companyGroupId, companyId, plantId, voucherId);
            var header = GetRePrintNonCashCheckVoucherReportHeader(companyGroupId, companyId, plantId, voucherId, voucherDetailId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //  var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            //var dsLocal = _bankJournalService.GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);

            //var dsLocal = GetPrintNonCashCheckVoucherReportData(companyGroupId, companyId, plantId, voucherId);

            var dsLocal = GetRePrintNonCashCheckVoucherReportData(companyGroupId, companyId, plantId, voucherId);
            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colBaseCurrencyDebit = 0;
            int colBaseCurrencyCredit = 0;
            int colTranCurrencyDebit = 0;
            int colTranCurrencyCredit = 0;

            int colVoucherNo = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            sheet[row, colVoucherNo].ColumnWidth = 18;
            sheet.Range[row, colVoucherNo].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());
            sheet[row, colVoucherNoValue].ColumnWidth = 12;
            sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            xlsCol++; //3
            int colReceived = xlsCol;
            xlsCol++;//4
            sheet[row, xlsCol].ColumnWidth = 10;

            xlsCol++; //5
            int colParticulars = xlsCol;

            xlsCol++;//6
            int colVoucherDate = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            sheet.Range[row, colVoucherDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;//7
            int colVoucherDateValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet.Range[row, colVoucherDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colPostingDate = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            sheet.Range[row, colPostingDate].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());
            sheet.Range[row, colPostingDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocDate = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            sheet.Range[row, colDocDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            sheet.Range[row, colDocDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;


            int colCheckNo = colVoucherNo;
            int colCheckNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckNo, "CheckNo");
            reportUtility.SetText(ref sheet, row, colCheckNoValue, header["CheckNumber"].ToString());


            int colCheckDate = colVoucherDate;
            int colCheckDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());
            row++;

            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());




            int colDocRef = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRef, "Doc Ref");
            sheet.Range[row, colDocRef].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocRefValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocRefValue, header["DocRefNo"].ToString());
            sheet.Range[row, colDocRefValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colNaration = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNaration, "Narration");
            int colNarationValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colNarationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

            sheet.Range[row, colNaration].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colNarationValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            int colStatus = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            int colStatusValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());
            sheet.Range[row, colStatus].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colStatusValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;  //10

            //    int colParty = colVoucherNo;
            //    int colPartyValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            //    reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());


            //    int colFiscalYearName = colVoucherNo;
            //    int colFiscalYearNameValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "CheckNo");
            //    reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["CheckNumber"].ToString());


            //    int colCheckDate = colDocRefNo;
            //    int colCheckDateValue = colDocRefNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            //    reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());

            colTranCurrencyDebit = colVoucherDate; //col6
            colTranCurrencyCredit = colVoucherDateValue; //7
            xlsCol++; //8 
            colBaseCurrencyDebit = xlsCol;
            xlsCol++; //9 
            colBaseCurrencyCredit = xlsCol;

            colLast = companyCurrencyId == transcationCurrency ? colTranCurrencyCredit : colBaseCurrencyCredit;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colBaseCurrencyDebit, row, colBaseCurrencyCredit].Merge();
            }
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            int colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();


            //reportUtility.SetHeaderText(ref sheet, row, colParticulars, "Particulars");
            //sheet[row, colParticulars].ColumnWidth = 23;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colusdCradit = xlsCol;
                colLast = colBaseCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colTranCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    // glName = string.Empty;

                    // var glName = dsLocal.Rows[i]["BudgetName"].ToString(); 
                    var glName = dsLocal.Rows[i]["Budget"].ToString();

                    // reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["AccountTitleName"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 4) + row].Merge();

                    // reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colParticulars, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, colVoucherNo, header["AddedBy"].ToString());
                sheet.Range[row, colVoucherNo].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherNo, "Prepared By", true);
                sheet[row, colVoucherNo].ColumnWidth = 18;


                reportUtility.SetTextMiddle(ref sheet, row, colReceived, "Received By", true);
                sheet[row, colReceived].ColumnWidth = 14;
                sheet.Range[row, colReceived].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars, header["PostedBy"].ToString());
                sheet.Range[row, colParticulars].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colParticulars, "Checked By", true);

                reportUtility.SetTextMiddle(ref sheet, row, colTranCurrencyCredit, "Authorized By", true);
                sheet[row, colTranCurrencyDebit].ColumnWidth = 15;
                sheet[row, colTranCurrencyCredit].ColumnWidth = 15;
                sheet.Range[row, colTranCurrencyCredit].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                sheet[row, colBaseCurrencyDebit].ColumnWidth = 15;
                sheet[row, colBaseCurrencyCredit].ColumnWidth = 15;


                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }



        public Dictionary<string, object> GetRePrintCashCheckVoucherReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, string voucherDetailId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
                           -- , V.AddedBy, V.PostedBy
                            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
                            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END

                            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode,EB.BeneficiaryType,EB.EmployeeId	,p.UserName Party
							,Beneficiary=CASE WHEN EB.EmployeeId<>'' THEN 'Employee' WHEN EB.PartyId<>'' THEN 'Party' ELSE NULL end
							,BeneficiaryName=CASE WHEN EB.EmployeeId<>'' THEN EI.EmployeeName WHEN EB.PartyId<>'' THEN P.UserName ELSE NULL end
							,VD.CheckLotDetailId, CLD.CheckNumber,CLH.CheckDate
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN TRN.ExpenseBooking AS EB ON EB.VoucherId=V.Id 
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
							LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.BankMasterId<>''
							--LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
	                        left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD ON CLD.Id=CLH.CheckLotDetailId
                             LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
							
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.Id='" + voucherDetailId + "'  order by CLD.CheckNumber desc ";
            return _sqlRepository.GetData(cmdText);
        }



        public DataTable GetRePrintCashCheckVoucherReportData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                , VD.Id AS BudgetMasterId, BUD.UserName AS Budget, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId,BM.AccountTitle as AccountTitleName
                ,[ParticularName]=CASE
                WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
                WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                WHEN P.UserName<>'' THEN P.UserName
                WHEN CM.UserName<>'' THEN CM.UserName
                WHEN FAM.UserName<>'' THEN FAM.UserName
                ELSE '' END
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
                LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
                LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
                LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }


        public IWorkbook GetRePrintCashCheckVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string voucherDetailId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "CashRePrint";

            // var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);
            // var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            //var header = GetPrintNonCashCheckVoucherReportHeader(companyGroupId, companyId, plantId, voucherId);
            //var header = GetRePrintCashCheckVoucherReportHeader(companyGroupId, companyId, plantId, voucherId);
            var header = GetRePrintCashCheckVoucherReportHeader(companyGroupId, companyId, plantId, voucherId, voucherDetailId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //  var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            //var dsLocal = _bankJournalService.GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);

            //var dsLocal = GetPrintNonCashCheckVoucherReportData(companyGroupId, companyId, plantId, voucherId);

            // var dsLocal = GetRePrintNonCashCheckVoucherReportData(companyGroupId, companyId, plantId, voucherId);

            var dsLocal = GetRePrintCashCheckVoucherReportData(companyGroupId, companyId, plantId, voucherId);
            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colBaseCurrencyDebit = 0;
            int colBaseCurrencyCredit = 0;
            int colTranCurrencyDebit = 0;
            int colTranCurrencyCredit = 0;

            int colVoucherNo = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            sheet[row, colVoucherNo].ColumnWidth = 18;
            sheet.Range[row, colVoucherNo].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());
            sheet[row, colVoucherNoValue].ColumnWidth = 12;
            sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            xlsCol++; //3
            int colReceived = xlsCol;
            xlsCol++;//4
            sheet[row, xlsCol].ColumnWidth = 10;

            xlsCol++; //5
            int colParticulars = xlsCol;

            xlsCol++;//6
            int colVoucherDate = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            sheet.Range[row, colVoucherDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;//7
            int colVoucherDateValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet.Range[row, colVoucherDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colPostingDate = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            sheet.Range[row, colPostingDate].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());
            sheet.Range[row, colPostingDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocDate = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            sheet.Range[row, colDocDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            sheet.Range[row, colDocDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;


            int colCheckNo = colVoucherNo;
            int colCheckNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckNo, "CheckNo");
            reportUtility.SetText(ref sheet, row, colCheckNoValue, header["CheckNumber"].ToString());


            int colCheckDate = colVoucherDate;
            int colCheckDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());
            row++;

            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());




            int colDocRef = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRef, "Doc Ref");
            sheet.Range[row, colDocRef].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocRefValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocRefValue, header["DocRefNo"].ToString());
            sheet.Range[row, colDocRefValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colNaration = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNaration, "Narration");
            int colNarationValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colNarationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

            sheet.Range[row, colNaration].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colNarationValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            int colStatus = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            int colStatusValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());
            sheet.Range[row, colStatus].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colStatusValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;  //10

            //    int colParty = colVoucherNo;
            //    int colPartyValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            //    reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());


            //    int colFiscalYearName = colVoucherNo;
            //    int colFiscalYearNameValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "CheckNo");
            //    reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["CheckNumber"].ToString());


            //    int colCheckDate = colDocRefNo;
            //    int colCheckDateValue = colDocRefNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            //    reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());

            colTranCurrencyDebit = colVoucherDate; //col6
            colTranCurrencyCredit = colVoucherDateValue; //7
            xlsCol++; //8 
            colBaseCurrencyDebit = xlsCol;
            xlsCol++; //9 
            colBaseCurrencyCredit = xlsCol;

            colLast = companyCurrencyId == transcationCurrency ? colTranCurrencyCredit : colBaseCurrencyCredit;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colBaseCurrencyDebit, row, colBaseCurrencyCredit].Merge();
            }
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            int colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();


            //reportUtility.SetHeaderText(ref sheet, row, colParticulars, "Particulars");
            //sheet[row, colParticulars].ColumnWidth = 23;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colusdCradit = xlsCol;
                colLast = colBaseCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colTranCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    // glName = string.Empty;

                    // var glName = dsLocal.Rows[i]["BudgetName"].ToString(); 
                    var glName = dsLocal.Rows[i]["Budget"].ToString();

                    // reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["AccountTitleName"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 4) + row].Merge();

                    // reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colParticulars, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, colVoucherNo, header["AddedBy"].ToString());
                sheet.Range[row, colVoucherNo].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherNo, "Prepared By", true);
                sheet[row, colVoucherNo].ColumnWidth = 18;


                reportUtility.SetTextMiddle(ref sheet, row, colReceived, "Received By", true);
                sheet[row, colReceived].ColumnWidth = 14;
                sheet.Range[row, colReceived].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars, header["PostedBy"].ToString());
                sheet.Range[row, colParticulars].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colParticulars, "Checked By", true);

                reportUtility.SetTextMiddle(ref sheet, row, colTranCurrencyCredit, "Authorized By", true);
                sheet[row, colTranCurrencyDebit].ColumnWidth = 15;
                sheet[row, colTranCurrencyCredit].ColumnWidth = 15;
                sheet.Range[row, colTranCurrencyCredit].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                sheet[row, colBaseCurrencyDebit].ColumnWidth = 15;
                sheet[row, colBaseCurrencyCredit].ColumnWidth = 15;


                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        //Cash cheque voucher report 
        public Dictionary<string, object> GetPrintCashCheckVoucherReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, string voucherDetailId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
                           -- , V.AddedBy, V.PostedBy
                            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
                            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END

                            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode,EB.BeneficiaryType,EB.EmployeeId	,p.UserName Party
							,Beneficiary=CASE WHEN EB.EmployeeId<>'' THEN 'Employee' WHEN EB.PartyId<>'' THEN 'Party' ELSE NULL end
							,BeneficiaryName=CASE WHEN EB.EmployeeId<>'' THEN EI.EmployeeName WHEN EB.PartyId<>'' THEN P.UserName ELSE NULL end
							,VD.CheckLotDetailId, CLD.CheckNumber,CLH.CheckDate
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN TRN.ExpenseBooking AS EB ON EB.VoucherId=V.Id 
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
							LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.BankMasterId<>''
							--LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
	                        left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD ON CLD.Id=CLH.CheckLotDetailId
                             LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.Id='" + voucherDetailId + "'  order by CLD.CheckNumber desc ";
            return _sqlRepository.GetData(cmdText);
        }
        public Dictionary<string, object> GetPrintCashCheckVoucherReportHeaderCashMaster(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode,EB.BeneficiaryType,EB.EmployeeId	,p.UserName Party
							,Beneficiary=CASE WHEN EB.EmployeeId<>'' THEN 'Employee' WHEN EB.PartyId<>'' THEN 'Party' ELSE NULL end
							,BeneficiaryName=CASE WHEN EB.EmployeeId<>'' THEN EI.EmployeeName WHEN EB.PartyId<>'' THEN P.UserName ELSE NULL end
							,VD.CheckLotDetailId, CLD.CheckNumber,CLH.CheckDate
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN TRN.ExpenseBooking AS EB ON EB.VoucherId=V.Id
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
							LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.CashMasterId<>''
							--LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
	                        left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD ON CLD.Id=CLH.CheckLotDetailId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ";
            return _sqlRepository.GetData(cmdText);
        }


        public DataTable GetPrintCashCheckVoucherReportData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                , VD.Id AS BudgetMasterId, BUD.UserName AS Budget, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId ,BM.AccountTitle as AccountTitleName
                ,[ParticularName]=CASE
                WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
                WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                WHEN P.UserName<>'' THEN P.UserName
                WHEN CM.UserName<>'' THEN CM.UserName
                WHEN FAM.UserName<>'' THEN FAM.UserName
                ELSE '' END
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
                LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
                LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
                LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }


        public IWorkbook GetPrintCashCheckVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string voucherDetailId, string bankMasterId, string cashMasterId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "CashPrint";

            // var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);
            // var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            // var header = GetPrintNonCashCheckVoucherReportHeader(companyGroupId, companyId, plantId, voucherId, voucherDetailId);
            var header = (bankMasterId != null) ? GetPrintCashCheckVoucherReportHeader(companyGroupId, companyId, plantId, voucherId, voucherDetailId) : GetPrintCashCheckVoucherReportHeaderCashMaster(companyGroupId, companyId, plantId, voucherId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //  var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            //var dsLocal = _bankJournalService.GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            //var dsLocal = GetPrintNonCashCheckVoucherReportData(companyGroupId, companyId, plantId, voucherId);
            var dsLocal = GetPrintCashCheckVoucherReportData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colBaseCurrencyDebit = 0;
            int colBaseCurrencyCredit = 0;
            int colTranCurrencyDebit = 0;
            int colTranCurrencyCredit = 0;

            int colVoucherNo = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            sheet[row, colVoucherNo].ColumnWidth = 18;
            sheet.Range[row, colVoucherNo].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());
            sheet[row, colVoucherNoValue].ColumnWidth = 12;
            sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            xlsCol++; //3
            int colReceived = xlsCol;
            xlsCol++;//4
            sheet[row, xlsCol].ColumnWidth = 10;

            xlsCol++; //5
            int colParticulars = xlsCol;

            xlsCol++;//6
            int colVoucherDate = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            sheet.Range[row, colVoucherDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;//7
            int colVoucherDateValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet.Range[row, colVoucherDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colPostingDate = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            sheet.Range[row, colPostingDate].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());
            sheet.Range[row, colPostingDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocDate = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            sheet.Range[row, colDocDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            sheet.Range[row, colDocDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;


            int colCheckNo = colVoucherNo;
            int colCheckNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckNo, "CheckNo");
            reportUtility.SetText(ref sheet, row, colCheckNoValue, header["CheckNumber"].ToString());


            int colCheckDate = colVoucherDate;
            int colCheckDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());
            row++;

            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());




            int colDocRef = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRef, "Doc Ref");
            sheet.Range[row, colDocRef].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocRefValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocRefValue, header["DocRefNo"].ToString());
            sheet.Range[row, colDocRefValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colNaration = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNaration, "Narration");
            int colNarationValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colNarationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

            sheet.Range[row, colNaration].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colNarationValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            int colStatus = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            int colStatusValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());
            sheet.Range[row, colStatus].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colStatusValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;  //10

            //    int colParty = colVoucherNo;
            //    int colPartyValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            //    reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());


            //    int colFiscalYearName = colVoucherNo;
            //    int colFiscalYearNameValue = colVoucherNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "CheckNo");
            //    reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["CheckNumber"].ToString());


            //    int colCheckDate = colDocRefNo;
            //    int colCheckDateValue = colDocRefNoValue;
            //    reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            //    reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());

            colTranCurrencyDebit = colVoucherDate; //col6
            colTranCurrencyCredit = colVoucherDateValue; //7
            xlsCol++; //8 
            colBaseCurrencyDebit = xlsCol;
            xlsCol++; //9 
            colBaseCurrencyCredit = xlsCol;

            colLast = companyCurrencyId == transcationCurrency ? colTranCurrencyCredit : colBaseCurrencyCredit;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colBaseCurrencyDebit, row, colBaseCurrencyCredit].Merge();
            }
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            int colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();


            //reportUtility.SetHeaderText(ref sheet, row, colParticulars, "Particulars");
            //sheet[row, colParticulars].ColumnWidth = 23;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colusdCradit = xlsCol;
                colLast = colBaseCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colTranCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    // glName = string.Empty;

                    // var glName = dsLocal.Rows[i]["BudgetName"].ToString(); 
                    var glName = dsLocal.Rows[i]["Budget"].ToString();

                    // reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["AccountTitleName"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 4) + row].Merge();

                    // reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colParticulars, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, colVoucherNo, header["AddedBy"].ToString());
                sheet.Range[row, colVoucherNo].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherNo, "Prepared By", true);
                sheet[row, colVoucherNo].ColumnWidth = 18;


                reportUtility.SetTextMiddle(ref sheet, row, colReceived, "Received By", true);
                sheet[row, colReceived].ColumnWidth = 14;
                sheet.Range[row, colReceived].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars, header["PostedBy"].ToString());
                sheet.Range[row, colParticulars].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colParticulars, "Checked By", true);

                reportUtility.SetTextMiddle(ref sheet, row, colTranCurrencyCredit, "Authorized By", true);
                sheet[row, colTranCurrencyDebit].ColumnWidth = 15;
                sheet[row, colTranCurrencyCredit].ColumnWidth = 15;
                sheet.Range[row, colTranCurrencyCredit].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                sheet[row, colBaseCurrencyDebit].ColumnWidth = 15;
                sheet[row, colBaseCurrencyCredit].ColumnWidth = 15;


                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }



        #endregion Voucher Report

        public IWorkbook GetSalaryJournalVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.SalaryJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetSalaryJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entry Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

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


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
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
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Voucher Report", companyId, plantName, null); // header["VoucherTypeName"].ToString()
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Voucher Report", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        public IWorkbook GetGLReport(string coaId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 2);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet1 = workbook.Worksheets[0];
            var sheet2 = workbook.Worksheets[1];
            CreateGLReportSheet1(ref sheet1, reportUtility, "GL Master Report", "GL Master Report", coaId);
            CreateGLReportSheet2(ref sheet2, reportUtility, "GL Master List", "GL Master Data", coaId);
            return workbook;
        }

        private void CreateGLReportSheet1(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string coaId)
        {
            DataTable dtBudgetMaster = null;

            #region List data

            var BudgetMasterList = GetGLMaster(coaId);
            var dvBudgetMaster = new DataView(BudgetMasterList)
            {
                Sort = "GLGeneralInfoCode"
            };
            dtBudgetMaster = dvBudgetMaster.ToTable();

            var dvGLLevel1 = new DataView(BudgetMasterList)
            {
                Sort = "Level1"
            };
            var dtGLLevel1 = dvGLLevel1.ToTable(true, "Level1", "Level1Id");

            DataView dvGLLevel2 = null;
            DataTable dtGLLevel2 = null;

            DataView dvGLLevel3 = null;
            DataTable dtGLLevel3 = null;

            DataView dvGLLevel4 = null;
            DataTable dtGLLevel4 = null;

            DataView dvGL = null;
            DataTable dtGL = null;

            DataView dvGLCode = null;
            DataTable dtGLCode = null;

            if (dtBudgetMaster.Rows.Count == 0)
            {
                throw new Exception("No Data Found !!!");
            }

            #endregion List data

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;
            var level1ColIndex = 1;
            var level2ColIndex = 2;
            var level3ColIndex = 3;
            var level4ColIndex = 4;
            var glCodeColIndex = 5;
            var glColIndex = 6;
            var budgetColIndex = 7;

            var _col3 = 3;

            reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "COA");
            sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
            reportUtility.SetText(ref sheet, _rowL, _col + 2, dtBudgetMaster.Rows[0]["COA"].ToString()); _rowL++;
            sheet[reportUtility.GetColumnNameForXls(_col3) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

            _rowL = 6;
            _rowL++;

            for (int i = 0; i < dtBudgetMaster.Columns.Count; i++)
            {
                if (dtBudgetMaster.Columns[i].ColumnName != "TotalRows" && dtBudgetMaster.Columns[i].ColumnName != "Level1Id" && dtBudgetMaster.Columns[i].ColumnName != "Level2Id" && dtBudgetMaster.Columns[i].ColumnName != "Level3Id" && dtBudgetMaster.Columns[i].ColumnName != "Level4Id" && dtBudgetMaster.Columns[i].ColumnName != "COA")
                {
                    _colIndex++;
                    reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtBudgetMaster.Columns[i].ColumnName);
                }
            }

            shet2EndxlsCol = _colIndex;

            for (int m = 0; m < dtGLLevel1.Rows.Count; m++)
            {
                _rowL++;
                var level1Id = dtGLLevel1.Rows[m]["Level1Id"].ToString();
                dvGLLevel2 = new DataView(dtBudgetMaster)
                {
                    Sort = "Level2",
                    RowFilter = "Level1Id='" + level1Id + "'"
                };
                dtGLLevel2 = dvGLLevel2.ToTable(true, "Level2", "Level2Id");
                var rowStartLevel1 = _rowL;
                reportUtility.SetText(ref sheet, _rowL, level1ColIndex, dtGLLevel1.Rows[m]["Level1"].ToString(), 26);

                for (int n = 0; n < dtGLLevel2.Rows.Count; n++)
                {
                    var level2Id = dtGLLevel2.Rows[n]["Level2Id"].ToString();
                    dvGLLevel3 = new DataView(dtBudgetMaster)
                    {
                        Sort = "Level3",
                        RowFilter = "Level2Id='" + level2Id + "' and Level1Id='" + level1Id + "'"
                    };
                    dtGLLevel3 = dvGLLevel3.ToTable(true, "Level3", "Level3Id");
                    var rowStartLevel2 = _rowL;
                    reportUtility.SetText(ref sheet, _rowL, level2ColIndex, dtGLLevel2.Rows[n]["Level2"].ToString(), 26);

                    for (int o = 0; o < dtGLLevel3.Rows.Count; o++)
                    {
                        var level3Id = dtGLLevel3.Rows[o]["Level3Id"].ToString();
                        dvGLLevel4 = new DataView(dtBudgetMaster)
                        {
                            Sort = "Level4",
                            RowFilter = "Level3Id='" + level3Id + "' and Level2Id='" + level2Id + "' and Level1Id='" + level1Id + "'"
                        };
                        dtGLLevel4 = dvGLLevel4.ToTable(true, "Level4", "Level4Id");
                        var rowStartLevel3 = _rowL;
                        reportUtility.SetText(ref sheet, _rowL, level3ColIndex, dtGLLevel3.Rows[o]["Level3"].ToString(), 26);

                        for (int p = 0; p < dtGLLevel4.Rows.Count; p++)
                        {
                            var level4Id = dtGLLevel4.Rows[p]["Level4Id"].ToString();
                            dvGLCode = new DataView(dtBudgetMaster)
                            {
                                Sort = "GLGeneralInfoName",
                                RowFilter = "Level4Id='" + level4Id + "' and Level3Id='" + level3Id + "' and Level2Id='" + level2Id + "' and Level1Id='" + level1Id + "'"
                            };
                            dtGLCode = dvGLCode.ToTable(true, "GLGeneralInfoName", "GLGeneralInfoCode");
                            var rowStartGroup4 = _rowL;
                            reportUtility.SetText(ref sheet, _rowL, level4ColIndex, dtGLLevel4.Rows[p]["Level4"].ToString(), 26);

                            for (int r = 0; r < dtGLCode.Rows.Count; r++)
                            {
                                var glCode = dtGLCode.Rows[r]["GLGeneralInfoCode"].ToString();
                                dvGL = new DataView(dtBudgetMaster)
                                {
                                    Sort = "GLGeneralInfoName",
                                    RowFilter = "GLGeneralInfoCode='" + glCode + "' and Level4Id='" + level4Id + "' and Level3Id='" + level3Id + "' and Level2Id='" + level2Id + "' and Level1Id='" + level1Id + "'"
                                };
                                dtGL = dvGL.ToTable(true, "GLGeneralInfoName", "GLGeneralInfoCode", "AccountGroup", "Manufacturing", "Treding", nameof(Service));
                                var rowStartGLCode = _rowL;

                                glColIndex = 6;

                                reportUtility.SetText(ref sheet, _rowL, glCodeColIndex, dtGLCode.Rows[r]["GLGeneralInfoCode"].ToString(), 15);
                                reportUtility.SetText(ref sheet, _rowL, glColIndex, dtGLCode.Rows[r]["GLGeneralInfoName"].ToString(), 26);

                                if (dtGL.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dtGL.Rows.Count; i++)
                                    {
                                        //glColIndex++;

                                        reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["AccountGroup"].ToString(), 26); budgetColIndex++;
                                        reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Manufacturing"].ToString(), 15); budgetColIndex++;
                                        reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Treding"].ToString(), 15); budgetColIndex++;
                                        reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i][nameof(Service)].ToString(), 15); budgetColIndex++;

                                        _rowL++;

                                        glCodeColIndex = 5;
                                        glColIndex = 6;
                                        budgetColIndex = 7;
                                    }
                                }
                                sheet[rowStartGLCode, glCodeColIndex, _rowL - 1, glCodeColIndex].Merge();
                                sheet[rowStartGLCode, glColIndex, _rowL - 1, glColIndex].Merge();
                            }//GL
                            sheet[rowStartGroup4, level4ColIndex, _rowL - 1, level4ColIndex].Merge();
                        }//Level4
                        sheet[rowStartLevel3, level3ColIndex, _rowL - 1, level3ColIndex].Merge();
                    }//Level3
                    sheet[rowStartLevel2, level2ColIndex, _rowL - 1, level2ColIndex].Merge();
                }//Level2
                sheet[rowStartLevel1, level1ColIndex, _rowL - 1, level1ColIndex].Merge();
                _rowL--;
            }//Level1

            sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = sheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            reportUtility.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "GL Master Report", identity.CompanyGroupId);
            reportUtility.FreezePage(ref sheet, 1, 8);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
        }

        private void CreateGLReportSheet2(ref IWorksheet sheet, ReportUtility reportUtility, string SheetHeader, string sheetName, string coaId)
        {
            DataTable dtBudgetMaster = null;

            var BudgetMasterList = GetGLMaster(coaId);
            var dvBudgetMaster = new DataView(BudgetMasterList)
            {
                Sort = "GLGeneralInfoCode"
            };
            dtBudgetMaster = dvBudgetMaster.ToTable();
            if (dtBudgetMaster.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;

            var _col3 = 3;

            reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "COA");
            sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
            reportUtility.SetText(ref sheet, _rowL, _col + 2, dtBudgetMaster.Rows[0]["COA"].ToString()); _rowL++;
            sheet[reportUtility.GetColumnNameForXls(_col3) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

            _rowL = 6;
            _rowL++;

            for (int i = 0; i < dtBudgetMaster.Columns.Count; i++)
            {
                if (dtBudgetMaster.Columns[i].ColumnName != "TotalRows" && dtBudgetMaster.Columns[i].ColumnName != "Level1Id" && dtBudgetMaster.Columns[i].ColumnName != "Level2Id" && dtBudgetMaster.Columns[i].ColumnName != "Level3Id" && dtBudgetMaster.Columns[i].ColumnName != "Level4Id" && dtBudgetMaster.Columns[i].ColumnName != "COA")
                {
                    _colIndex++;
                    reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtBudgetMaster.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;

            for (int i = 0; i < dtBudgetMaster.Rows.Count; i++)
            {
                _rowL++;
                reportUtility.SetText(ref sheet, _rowL, 1, dtBudgetMaster.Rows[i]["Level1"].ToString(), 26);
                reportUtility.SetText(ref sheet, _rowL, 2, dtBudgetMaster.Rows[i]["Level2"].ToString(), 26);
                reportUtility.SetText(ref sheet, _rowL, 3, dtBudgetMaster.Rows[i]["Level3"].ToString(), 26);
                reportUtility.SetText(ref sheet, _rowL, 4, dtBudgetMaster.Rows[i]["Level4"].ToString(), 26);
                reportUtility.SetText(ref sheet, _rowL, 5, dtBudgetMaster.Rows[i]["GLGeneralInfoCode"].ToString(), 15);
                reportUtility.SetText(ref sheet, _rowL, 6, dtBudgetMaster.Rows[i]["GLGeneralInfoName"].ToString(), 26);
                reportUtility.SetText(ref sheet, _rowL, 7, dtBudgetMaster.Rows[i]["AccountGroup"].ToString(), 26);
                reportUtility.SetText(ref sheet, _rowL, 8, dtBudgetMaster.Rows[i]["Manufacturing"].ToString(), 15);
                reportUtility.SetText(ref sheet, _rowL, 9, dtBudgetMaster.Rows[i]["Treding"].ToString(), 15);
                reportUtility.SetText(ref sheet, _rowL, 10, dtBudgetMaster.Rows[i][nameof(Service)].ToString(), 15);
            }

            sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = sheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            reportUtility.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "GL Master List", identity.CompanyGroupId);
            reportUtility.FreezePage(ref sheet, 1, 8);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
        }
        public IWorkbook GetDailyTransactionReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, DateTime date, string entityId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetDailyTransactionHeader(companyGroupId, companyId, plantId, date);

            reportFileName = "DailyTransactions" + date.ToString("dd-MMM-yyyy");

            var dsLocal = GetDailyTransactionData(companyGroupId, companyId, plantId, entityId, date);

            var dtEntityUserName = _sqlRepository.GetDataTable("select UserName Entity from ORG.Entity where Id= '" + entityId + @"'");

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            //reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            //reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            //reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            //sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            //row++;

            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            //reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            //reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            //reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            //sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            //row++;

            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            //reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            //reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            //reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            //sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            //row++;

            //colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            //reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            //sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            //row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

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


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
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
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Transaction Report of " + dtEntityUserName.Rows[0]["Entity"].ToString() + " On " + date.ToString("dd-MMM-yyyy"), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Transaction Report of " + dtEntityUserName.Rows[0]["Entity"].ToString() + " On " + date.ToString("dd-MMM-yyyy"), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }


        public IWorkbook GetDayBooksReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, DateTime fromDate, DateTime toDate, string entityId, string dateType)  //, bool checkbox
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            // DataTable dtIssueReportList = GetOperationReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
            //var dsLocal = GetDayBooksData(companyGroupId, companyId, plantId, fromDate, toDate);
            DataTable dtDayBookData = GetDayBooksData(companyGroupId, companyId, plantId, fromDate, toDate, dateType);

            worksheet.Name = "Day Books";
            //var header = GetDailyTransactionHeader(companyGroupId, companyId, plantId, toDate);
            reportFileName = "Day Books " + toDate.ToString("dd-MMM-yyyy");


            //if (dtDayBookData.Rows.Count == 0)
            //    throw new Exception("No data found");

            int COL = 1; int ROW = 5;
            int startCol = COL;

            if (dateType == "PostingDate")
            {
                worksheet.Range[ROW - 1, 3].Text = "Posting Date:  From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");
            }
            else
            {
                worksheet.Range[ROW - 1, 3].Text = "Entry Date:   From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");
            }


            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher Type";
            int colSourceType = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Posting Date";
            int colPostingDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Entry Date";
            int colEntryDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Date";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "DocRef No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GL Code";
            int colGLCode = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GL";
            int colUserName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget";
            int colBudget = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget Group";
            int colBudgetGroup = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget Category";
            int colBudgetCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget Sub Category";
            int colBudgetSubCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget RefNo";
            int colBudgetRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Activity Id";
            int colActivityId = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Activity";
            int colActivity = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Particular";
            int colParticular = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Tran. Currency";
            int colTrnCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            
            worksheet[ROW, COL].Text = "Tran. Dr.";
            int colDrAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Tran. Cr.";
            int colCrAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "Dr/Cr";
            //int colBooksDrCr = COL;
            //worksheet[ROW, COL].ColumnWidth = 5;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            worksheet[ROW, COL].Text = "Books Dr.";
            int colBooksDrAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Books Cr.";
            int colBooksCrAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Park";
            int colIsPark = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRNNo.";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "AcceptanceNo.";
            int colAcceptanceNo = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Issue";
            int colIssue = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Cost Center";
            int colCostCenterName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Narration";
            int colNarration = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            //worksheet[ROW, COL].Text = "Type";
            //int colType = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            worksheet[ROW, COL].Text = "Level1";
            int colLevel1 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Level2";
            int colLevel2 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Level3";
            int colLevel3 = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Level4";
            int colLevel4 = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Reconcile";
            int colReconcile = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Reconcile Date";
            int colReconcileDate = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GL Update";
            int colGLUpdate = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Party Category";
            int colCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Party Sub Category";
            int colSubCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Voucher Category";
            int colVoucherCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtDayBookData.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);
                worksheet[ROW, colDrAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["DrAmount"].ToString());
                worksheet[ROW, colDrAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                // worksheet[ROW, colBooksDrCr].Text = dtDayBookData.Rows[i]["Dr/Cr"].ToString();
                worksheet[ROW, colBooksDrAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["CompanyCurrencyDrAmount"].ToString());
                worksheet[ROW, colBooksDrAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colBooksCrAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["CompanyCurrencyCrAmount"].ToString());
                worksheet[ROW, colBooksCrAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colSourceType].Text = dtDayBookData.Rows[i]["VoucherType"].ToString();
                worksheet[ROW, colVoucherNo].Text = dtDayBookData.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colPostingDate].Text = dtDayBookData.Rows[i]["PostingDate"].ToString();
                worksheet[ROW, colEntryDate].Text = dtDayBookData.Rows[i]["EntryDate"].ToString();
                worksheet[ROW, colDocDate].Text = dtDayBookData.Rows[i]["DocDate"].ToString();

                worksheet[ROW, colCrAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["CrAmount"].ToString());
                worksheet[ROW, colCrAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colDocRefNo].Text = dtDayBookData.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colGLCode].Text = dtDayBookData.Rows[i]["GLCode"].ToString();
                worksheet[ROW, colUserName].Text = dtDayBookData.Rows[i]["GL"].ToString();
                worksheet[ROW, colBudget].Text = dtDayBookData.Rows[i]["Budget"].ToString();
                worksheet[ROW, colBudgetGroup].Text = dtDayBookData.Rows[i]["BudgetGroup"].ToString();
                worksheet[ROW, colBudgetCategory].Text = dtDayBookData.Rows[i]["BudgetCategory"].ToString();
                worksheet[ROW, colBudgetSubCategory].Text = dtDayBookData.Rows[i]["BudgetSubCategory"].ToString();
                worksheet[ROW, colActivity].Text = dtDayBookData.Rows[i]["Activity"].ToString();
                worksheet[ROW, colActivityId].Text = dtDayBookData.Rows[i]["ActivityCode"].ToString();
                worksheet[ROW, colBudgetRefNo].Text = dtDayBookData.Rows[i]["BudgetRefNo"].ToString();

                worksheet[ROW, colParticular].Text = dtDayBookData.Rows[i]["Particular"].ToString();
                worksheet[ROW, colIsPark].Text = dtDayBookData.Rows[i]["IsPark"].ToString();
                worksheet[ROW, colGRNNo].Text = dtDayBookData.Rows[i]["GRNNo"].ToString();
                worksheet[ROW, colAcceptanceNo].Text = dtDayBookData.Rows[i]["AcceptanceNo"].ToString();
                worksheet[ROW, colIssue].Text = dtDayBookData.Rows[i]["Issue"].ToString();
                worksheet[ROW, colCostCenterName].Text = dtDayBookData.Rows[i]["CostCenterName"].ToString();

                worksheet[ROW, colTrnCurrency].Text = dtDayBookData.Rows[i]["TrnCurrency"].ToString();
                //worksheet[ROW, colType].Text = dtDayBookData.Rows[i]["Type"].ToString();
                worksheet[ROW, colLevel1].Text = dtDayBookData.Rows[i]["Level1"].ToString();
                worksheet[ROW, colLevel2].Text = dtDayBookData.Rows[i]["Level2"].ToString();
                worksheet[ROW, colLevel3].Text = dtDayBookData.Rows[i]["Level3"].ToString();
                worksheet[ROW, colLevel4].Text = dtDayBookData.Rows[i]["Level4"].ToString();
                worksheet[ROW, colNarration].Text = dtDayBookData.Rows[i]["Narration"].ToString();

                worksheet[ROW, colReconcileDate].Text = dtDayBookData.Rows[i]["ReconcileDate"].ToString();
                worksheet[ROW, colReconcile].Text = dtDayBookData.Rows[i]["Reconcile"].ToString();
                worksheet[ROW, colGLUpdate].Text = dtDayBookData.Rows[i]["GLUpdate"].ToString();
                worksheet[ROW, colCategory].Text = dtDayBookData.Rows[i]["UserCategory"].ToString();
                worksheet[ROW, colSubCategory].Text = dtDayBookData.Rows[i]["UserSubCategory"].ToString();
                worksheet[ROW, colVoucherCategory].Text = dtDayBookData.Rows[i]["VoucherCategory"].ToString();

                //if (checkbox == true)
                //{
                //    worksheet[ROW, colTaskDetail].Text = dtIssueReportList.Rows[i]["TaskDetail"].ToString();
                //}

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();

            worksheet[ROW, colTrnCurrency].Text = "Total :";

            worksheet[ROW, colTrnCurrency].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, colTrnCurrency].CellStyle.Font.Bold = true;

            //worksheet.Range[ROW, colDrAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colDrAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colDrAmount) + (ROW-1) + ")";
            //worksheet.Range[ROW, colDrAmount].NumberFormat = report.NumberFormatDecimalTwo();
            //worksheet.Range[ROW, colDrAmount].CellStyle.Font.Bold = true;
            //worksheet.Range[ROW, colDrAmount].BorderAround(ExcelLineStyle.Hair);

            //worksheet.Range[ROW, colCrAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colCrAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colCrAmount) + (ROW - 1) + ")";
            //worksheet.Range[ROW, colCrAmount].NumberFormat = report.NumberFormatDecimalTwo();
            //worksheet.Range[ROW, colCrAmount].CellStyle.Font.Bold = true;
            //worksheet.Range[ROW, colCrAmount].BorderAround(ExcelLineStyle.Hair);

            worksheet.Range[ROW, colBooksDrAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colBooksDrAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colBooksDrAmount) + (ROW - 1) + ")";
            worksheet.Range[ROW, colBooksDrAmount].NumberFormat = report.NumberFormatDecimalTwo();
            worksheet.Range[ROW, colBooksDrAmount].CellStyle.Font.Bold = true;
            worksheet.Range[ROW, colBooksDrAmount].BorderAround(ExcelLineStyle.Hair);

            worksheet.Range[ROW, colBooksCrAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colBooksCrAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colBooksCrAmount) + (ROW - 1) + ")";
            worksheet.Range[ROW, colBooksCrAmount].NumberFormat = report.NumberFormatDecimalTwo();
            worksheet.Range[ROW, colBooksCrAmount].CellStyle.Font.Bold = true;
            worksheet.Range[ROW, colBooksCrAmount].BorderAround(ExcelLineStyle.Hair);
            //sheet1.Range[xlsRow, 3].Text = "GST Recievable Report From " + fromDate + " To " + toDate;

            reportUtility.PlantHeader(ref worksheet, endCol, " Day Books Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }
        public IWorkbook GetVoucherParkedReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, DateTime fromDate, DateTime toDate)  
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            
            DataTable dtDayBookData = GetVoucherParkedData(companyGroupId, companyId, plantId, fromDate, toDate);

            worksheet.Name = "Voucher Parked Report";
            reportFileName = "Voucher Parked Report " + toDate.ToString("dd-MMM-yyyy");

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet.Range[ROW - 1, 3].Text = "Posting Date:  From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");
            
            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher Type";
            int colSourceType = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Posting Date";
            int colPostingDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Entry Date";
            int colEntryDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Date";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "DocRef No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;
            
            worksheet[ROW, COL].Text = "Tran. Currency";
            int colTrnCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Amount";
            int colCrAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtDayBookData.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);
                worksheet[ROW, colSourceType].Text = dtDayBookData.Rows[i]["VoucherType"].ToString();
                worksheet[ROW, colVoucherNo].Text = dtDayBookData.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colPostingDate].Text = dtDayBookData.Rows[i]["PostingDate"].ToString();
                worksheet[ROW, colEntryDate].Text = dtDayBookData.Rows[i]["EntryDate"].ToString();
                worksheet[ROW, colDocDate].Text = dtDayBookData.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtDayBookData.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colTrnCurrency].Text = dtDayBookData.Rows[i]["TrnCurrency"].ToString();
                worksheet[ROW, colCrAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["CrAmount"].ToString());
                worksheet[ROW, colCrAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, " Voucher Parked Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }

        public IWorkbook GetFixedAssetFinancialRegisterReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, DateTime fromDate, DateTime toDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtDayBookData = GetFixedAssetFinancialRegisterReportData(companyGroupId, companyId, plantId, fromDate, toDate);

            worksheet.Name = "Fixed Asset Financial Register Report";
            reportFileName = "Fixed Asset Financial Register Report From" + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

            int COL = 1; int ROW = 6;
            int startCol = COL;

            worksheet.Range[ROW - 2, 3].Text = "Fixed Asset Financial Register Report" ;
            worksheet.Range[ROW - 1, 3].Text = "From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

            worksheet[ROW, COL].Text = "GL";
            int colGL = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget";
            int colBudget = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Activity";
            int colActivity = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "OpeningAmount";
            int colOpeningAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "OpeningJV";
            int colOpeningJV = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "TotalOpenigAmount";
            int colTotalOpeningAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Asset Amount";
            int colAssetAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Depreciation Amount";
            int colDepreciationAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Net Asset Amount";
            int colNetAssetAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "JV Amount";
            int colJVAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Total Financial Amount";
            int colTotalAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;


            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;

            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtDayBookData.Rows.Count; i++)
            {
                worksheet[ROW, colGL].Text = dtDayBookData.Rows[i]["GL"].ToString();
                worksheet[ROW, colBudget].Text = dtDayBookData.Rows[i]["Budget"].ToString();
                worksheet[ROW, colActivity].Text = dtDayBookData.Rows[i]["Activity"].ToString();
                worksheet[ROW, colOpeningAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["OpeningAmount"].ToString());
                worksheet[ROW, colOpeningAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colOpeningJV].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["OpeningJV"].ToString());
                worksheet[ROW, colOpeningJV].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colTotalOpeningAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["TotalOpeningAmount"].ToString());
                worksheet[ROW, colTotalOpeningAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colAssetAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["RegisterItemAmount"].ToString());
                worksheet[ROW, colAssetAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colDepreciationAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["DepreciationAmount"].ToString());
                worksheet[ROW, colDepreciationAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colNetAssetAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["NetAssetAmount"].ToString());
                worksheet[ROW, colNetAssetAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colJVAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["JVAmount"].ToString());
                worksheet[ROW, colJVAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colTotalAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["TotalAmount"].ToString());
                worksheet[ROW, colTotalAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "Fixed Asset Financial Register Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }
        public IWorkbook GetAssetDepreciationReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, DateTime fromDate, DateTime toDate, string assetDepreciationId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtDayBookData = GetAssetDepreciationReportData(companyGroupId, companyId, plantId, fromDate, toDate, assetDepreciationId);

            worksheet.Name = "Capitalize Assets Depreciation Report";
            reportFileName = "Capitalize Assets Depreciation Report From" + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet.Range[ROW - 1, 3].Text = "From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Asset Depreciation Id";
            int colAssetDepreciationId = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "ProcessName";
            int colProcessName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Process Date";
            int colProcessDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Asset Register Id";
            int colAssetRegisterId = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Asset Register Child Id";
            int colAssetRegisterChildId = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Capitalization Master Id";
            int colCapitalizationMasterId = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Capitalization Child Id";
            int colCapitalizationChildId = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Capitalization Date";
            int colCapitalizationDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Asset Master Id";
            int colFixedAssetMasterId = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Asset Item Id";
            int colFixedAssetItemId = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Asset Master";
            int colFixedAssetMaster = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Asset Item";
            int colFixedAssetItem = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Depreciation Days";
            int colDepreciationDays = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Depreciation Type";
            int colDepreciationType = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Depreciation Rate";
            int colDepreciationRate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "AssetValue";
            int colAssetValue = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Depreciation Amount";
            int colDepreciationAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Acc. Depreciation Amount";
            int colAccumulatedDepreciationAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Net Asset Value";
            int colNetAssetValue = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;


            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            
            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtDayBookData.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);
                worksheet[ROW, colAssetDepreciationId].Text = dtDayBookData.Rows[i]["AssetDepreciationId"].ToString();
                worksheet[ROW, colProcessName].Text = dtDayBookData.Rows[i]["ProcessName"].ToString();
                worksheet[ROW, colProcessDate].Text = dtDayBookData.Rows[i]["ProcessDate"].ToString();
                worksheet[ROW, colAssetRegisterId].Text = dtDayBookData.Rows[i]["AssetRegisterId"].ToString();
                worksheet[ROW, colAssetRegisterChildId].Text = dtDayBookData.Rows[i]["AssetRegisterChildId"].ToString();
                worksheet[ROW, colCapitalizationMasterId].Text = dtDayBookData.Rows[i]["CapitalizationMasterId"].ToString();
                worksheet[ROW, colCapitalizationChildId].Text = dtDayBookData.Rows[i]["CapitalizationChildId"].ToString();
                worksheet[ROW, colCapitalizationDate].Text = dtDayBookData.Rows[i]["CapitalizationDate"].ToString();
                worksheet[ROW, colFixedAssetMasterId].Text = dtDayBookData.Rows[i]["FixedAssetMasterId"].ToString();
                worksheet[ROW, colFixedAssetItemId].Text = dtDayBookData.Rows[i]["FixedAssetItemId"].ToString();
                worksheet[ROW, colFixedAssetMaster].Text = dtDayBookData.Rows[i]["FixedAssetMaster"].ToString();
                worksheet[ROW, colFixedAssetItem].Text = dtDayBookData.Rows[i]["FixedAssetItem"].ToString();
                worksheet[ROW, colDepreciationDays].Text = dtDayBookData.Rows[i]["DepreciationDays"].ToString();
                worksheet[ROW, colDepreciationType].Text = dtDayBookData.Rows[i]["DepreciationType"].ToString();
                worksheet[ROW, colDepreciationRate].Text = dtDayBookData.Rows[i]["DepreciationRate"].ToString();
                worksheet[ROW, colAssetValue].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["AssetValue"].ToString());
                worksheet[ROW, colAssetValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colDepreciationAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["DepreciationAmount"].ToString());
                worksheet[ROW, colDepreciationAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colAccumulatedDepreciationAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["AccumulatedDepreciationAmount"].ToString());
                worksheet[ROW, colAccumulatedDepreciationAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colNetAssetValue].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["NetAssetValue"].ToString());
                worksheet[ROW, colNetAssetValue].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "Capitalize Assets Depreciation Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }
        public IWorkbook GetGRNParkedReport(out string reportFileName, string plantId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtGRNParkedData = GetGRNParkedData(plantId);

            worksheet.Name = "GRN Parked Report";
            reportFileName = "GRN Parked Report ";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            //worksheet.Range[ROW - 1, 3].Text = "Posting Date:  From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

            worksheet[ROW, COL].Text = "Type";
            int colType = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Particular";
            int colParticular = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "GRN No";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN Date";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN Ref No";
            int colGRNRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Qty";
            int colQty = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Amount(TRN)";
            int colTRNAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Amount(BC)";
            int colBCAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "FOC";
            int colFOC = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "PO No";
            int colPONo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "PO Date";
            int colPODate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "PO Ref No";
            int colPORefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
         

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtGRNParkedData.Rows.Count; i++)
            {
                //worksheet[ROW, colType].Number = (i + 1);
                worksheet[ROW, colType].Text = dtGRNParkedData.Rows[i]["Type"].ToString();
                worksheet[ROW, colParticular].Text = dtGRNParkedData.Rows[i]["Particular"].ToString();
                worksheet[ROW, colGRNNo].Text = dtGRNParkedData.Rows[i]["Id"].ToString();
                worksheet[ROW, colGRNDate].Text = dtGRNParkedData.Rows[i]["GRNDate"].ToString();
                worksheet[ROW, colGRNRefNo].Text = dtGRNParkedData.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colCurrency].Text = dtGRNParkedData.Rows[i]["CurrencyCode"].ToString();
               
                worksheet[ROW, colQty].Number = clsStaticInfo.dbl(dtGRNParkedData.Rows[i]["TransactionQty"].ToString());
                worksheet[ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colTRNAmount].Number = clsStaticInfo.dbl(dtGRNParkedData.Rows[i]["TransactionAmount"].ToString());
                worksheet[ROW, colTRNAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colBCAmount].Number = clsStaticInfo.dbl(dtGRNParkedData.Rows[i]["BaseAmount"].ToString());
                worksheet[ROW, colBCAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colFOC].Text = dtGRNParkedData.Rows[i]["IsFOC"].ToString();
                worksheet[ROW, colPONo].Text = dtGRNParkedData.Rows[i]["POId"].ToString();
                worksheet[ROW, colPODate].Text = dtGRNParkedData.Rows[i]["PODate"].ToString();
                worksheet[ROW, colPORefNo].Text = dtGRNParkedData.Rows[i]["POVendorRefNo"].ToString();

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "GRN Parked Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }

        public DataTable GetGRNParkedData(string plantId)
        {
            var cmdText = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                            , IR.MaterialStorageId, IR.DocRefNo, IR.DocDate
	                            , IR.GateEntryNo,PG.UserName GateEntryName, REPLACE(CONVERT(CHAR(11), GE.EntryDate, 106),' ','-') AS EntryDate
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo
								, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName,PT.PaymentMode
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate, IR.ToCurrencyRate CompanyCurrencyRate
								,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END
								,IR.NoteForAccounts Narration
                                 ,GRNACC.PurchaseDocumentAcceptanceId AcceptanceId, REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate
								, PDA.AcceptanceNo
								,IsFOC=CASE WHEN IR.IsFOC=1 THEN 'YES' ELSE 'NO' END
								,IR.GRNType,IR.OtherPartyId,IR.OtherPartyPlantId,ISNULL(PLC.IsAccepptanceFirst,0) IsAccepptanceFirst
								,POId=	STUFF((select distinct ','+PO.Id from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PODate=	STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,POVendorRefNo=	STUFF((select distinct ','+PO.DocRefNo from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,LCNo=	STUFF((select distinct ','+LC.LCRef from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								 ,PurchaseLCId=	STUFF((select distinct ','+LC.Id from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=	STUFF((select distinct ','+C.ContractNo from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								 ,CustomerName=	STUFF((select distinct ','+P.UserName from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
														LEFT JOIN HKP.Party P ON P.Id=C.CustomerId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
					LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
					LEFT JOIN TRN.GRNAcceptanceMap GRNACC ON GRNACC.GRNId=IR.Id
					LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=GRNACC.PurchaseDocumentAcceptanceId
					LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
					
                     LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.TotalMaterialTranAmount,4)) AS TransactionAmount, SUM(ROUND(A.TotalMaterialBooksCurrencyAmount,0)) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0 AND IR.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL 
					AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND ISNULL(IR.IsFOC,0)=0 AND IR.GRNType!='MaterialTransfer'
                    order by IR.GRNDate desc";
            return _sqlRepository.GetDataTable(cmdText);


        }

        public IWorkbook GetIssueParkedReport(out string reportFileName, string plantId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtIssueParkedData = GetIssueParkedData(plantId);

            worksheet.Name = "Issue Parked Report";
            reportFileName = "Issue Parked Report ";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            //worksheet.Range[ROW - 1, 3].Text = "Posting Date:  From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

            worksheet[ROW, COL].Text = "Issue No";
            int colIssueNo = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Material Storage";
            int colMaterialStorage = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Issue Date";
            int colIssueDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;
            worksheet[ROW, COL].Text = "Employee";
            int colEmployeeName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Type";
            int colType = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "PO No";
            int colPONo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Order Ref No";
            int colOrderRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Qty";
            int colQty = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Amount";
            int colAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Contract";
            int colContract = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "LC";
            int colLC = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Customer";
            int colCustomer = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtIssueParkedData.Rows.Count; i++)
            {
                //worksheet[ROW, colType].Number = (i + 1);
                worksheet[ROW, colIssueNo].Text = dtIssueParkedData.Rows[i]["IssueNo"].ToString();
                worksheet[ROW, colMaterialStorage].Text = dtIssueParkedData.Rows[i]["MaterialStorage"].ToString();
                worksheet[ROW, colIssueDate].Text = dtIssueParkedData.Rows[i]["IssueDate"].ToString();
                worksheet[ROW, colEmployeeName].Text = dtIssueParkedData.Rows[i]["EmployeeName"].ToString();
                worksheet[ROW, colType].Text = dtIssueParkedData.Rows[i]["Types"].ToString();
                worksheet[ROW, colPONo].Text = dtIssueParkedData.Rows[i]["SourceNo"].ToString();
                worksheet[ROW, colOrderRefNo].Text = dtIssueParkedData.Rows[i]["OrderRefNo"].ToString();

                worksheet[ROW, colQty].Number = clsStaticInfo.dbl(dtIssueParkedData.Rows[i]["Qty"].ToString());
                worksheet[ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtIssueParkedData.Rows[i]["Amount"].ToString());
                worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colContract].Text = dtIssueParkedData.Rows[i]["ContractId"].ToString();
               
                worksheet[ROW, colLC].Text = dtIssueParkedData.Rows[i]["LCRef"].ToString();
                worksheet[ROW, colCustomer].Text = dtIssueParkedData.Rows[i]["Customer"].ToString();

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "Issue Parked Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }

        public DataTable GetIssueParkedData(string plantId)
        {
            var cmdText = @"SELECT II.Id,II.Id IssueNo,Replace(CONVERT(VARCHAR(11),II.IssueDate, 106), ' ', '-') AS IssueDate,II.Remarks, MS.UserName AS MaterialStorage,II.EntityId,E.UserName  EntityName,II.IssueType
                                    ,EI.EmployeeCode+' - '+EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount
                                    ,ii.OrderRefNo, IsOrderSpecificy=  CASE WHEN ii.OrderRefNo <> '' THEN 1 ELSE 0 END,II.[Types]
									,SourceNo=II.JWContractId,JW.ContractId,LC.LCRef,Customer=P.Code+' '+P.UserName 
                                    FROM [TRN].[InventoryIssue] AS II
                                    JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id 
							        JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId=II.Id
								    left join dbo.EmployeeInformation AS EI ON EI.SystemId=II.EmployeeId
                                    left join org.Entity E ON E.Id=II.EntityId
									LEFT JOIN [dbo].[OSTransformationPO] JW ON JW.Id=II.JWContractId
									left join dbo.[Contract] CN ON CN.Id=JW.ContractId
									LEFT JOIN dbo.MasterLC LC ON LC.Id=CN.MasterLCId
									LEFT JOIN HKP.Party P ON P.Id=LC.CustomerId
                            WHERE II.PlantId='" + plantId + @"' AND ISNULL(II.[Status],'')<>'Posting' 
                            AND IID.IsAsset=0 AND II.IsPostingRequired=1
                            GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 , II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.Remarks,II.EntityId,E.UserName,II.IssueType
									 , ii.OrderRefNo,II.[Types],II.JWContractId,JW.ContractId,LC.LCRef,P.Code,p.UserName";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetServiceParkedReport(out string reportFileName, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtServiceParkedData = GetServiceParkedData(plantId);

            worksheet.Name = "Service Parked Report";
            reportFileName = "Service Parked Report ";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            //worksheet.Range[ROW - 1, 3].Text = "Posting Date:  From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

            worksheet[ROW, COL].Text = "Party";
            int colParty = COL;
            worksheet[ROW, COL].ColumnWidth = 35;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Acknowledgement No";
            int colAcknowledgementNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Ref No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;
            worksheet[ROW, COL].Text = "Doc Date";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Amount (TRN)";
            int colTransactionAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Amount (BC)";
            int colBaseAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "PO No";
            int colPONo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "PO Ref No";
            int colPORefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "PO Date";
            int colPODate = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtServiceParkedData.Rows.Count; i++)
            {
                //worksheet[ROW, colType].Number = (i + 1);
                worksheet[ROW, colParty].Text = dtServiceParkedData.Rows[i]["PartyName"].ToString();
                worksheet[ROW, colAcknowledgementNo].Text = dtServiceParkedData.Rows[i]["Id"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtServiceParkedData.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colDocDate].Text = dtServiceParkedData.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colCurrency].Text = dtServiceParkedData.Rows[i]["CurrencyCode"].ToString();

                worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtServiceParkedData.Rows[i]["TransactionAmount"].ToString());
                worksheet[ROW, colTransactionAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colBaseAmount].Number = clsStaticInfo.dbl(dtServiceParkedData.Rows[i]["BaseAmount"].ToString());
                worksheet[ROW, colBaseAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colPONo].Text = dtServiceParkedData.Rows[i]["POId"].ToString();
                worksheet[ROW, colPORefNo].Text = dtServiceParkedData.Rows[i]["PORefNo"].ToString();

                worksheet[ROW, colPODate].Text = dtServiceParkedData.Rows[i]["PODate"].ToString();
               
                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "Service Parked Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }

        public DataTable GetServiceParkedData(string plantId)
        {
            var cmdText = @"SELECT IR.Id,  IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
								, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                   
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId
	                            , IR.PODepended
								, IR.DocRefNo
                                ,Replace(CONVERT(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                                ,Replace(CONVERT(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS AcknolwdgementDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
								,IR.NoteForAccounts Narration
								,POId=STUFF((SELECT DISTINCT ','+xpo.Id from
									trn.ServicePOMaster xpo
									INNER JOin trn.[ServiceAcknowledgementDetail] xPDAMAP on xpo.Id=xPDAMAP.ServicePOMasterId
									left join [TRN].[ServiceAcknowledgementMaster] xir on xir.Id=xPDAMAP.ServiceAcknowledgementMasterId
									WHERE xir.Id=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PORefNo=STUFF((SELECT DISTINCT ','+xpo.DocRefNo from
									trn.ServicePOMaster xpo
									INNER JOin trn.[ServiceAcknowledgementDetail] xPDAMAP on xpo.Id=xPDAMAP.ServicePOMasterId
									left join [TRN].[ServiceAcknowledgementMaster] xir on xir.Id=xPDAMAP.ServiceAcknowledgementMasterId
									WHERE xir.Id=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PODate=STUFF((SELECT DISTINCT ','++REPLACE(CONVERT(CHAR(11), xpo.DocDate, 106),' ','-') from
									trn.ServicePOMaster xpo
									INNER JOin trn.[ServiceAcknowledgementDetail] xPDAMAP on xpo.Id=xPDAMAP.ServicePOMasterId
									left join [TRN].[ServiceAcknowledgementMaster] xir on xir.Id=xPDAMAP.ServiceAcknowledgementMasterId
									WHERE xir.Id=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									
                    FROM [TRN].[ServiceAcknowledgementMaster] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C
					 LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
					
                     LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, SUM(ROUND(A.Amount,4)) AS TransactionAmount, SUM(ROUND(A.TotalAmount,0)) AS BaseAmount 
					 FROM [TRN].[ServiceAcknowledgementDetail] AS A
		                        JOIN [TRN].[ServiceAcknowledgementMaster] AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + plantId + @"' 
								GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                    WHERE IR.PlantId='" + plantId + @"' 
					AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0   AND IR.ApprovedByStatus='Approved'";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetServiceTDSReport(out string reportFileName, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];

            DataTable dtServiceParkedData = GetServiceTDSData(plantId);

            worksheet.Name = "TDS Report";
            reportFileName = "TDS Report ";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "Type";
            int colGRNType = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Particular";
            int colParticular = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN No";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;
            worksheet[ROW, COL].Text = "GRN Date";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Gate Entry No";
            int colGateEntryNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Posting Date";
            int colPostingDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "PO No";
            int colPONo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Ref No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Doc Ref Date";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++; 

            worksheet[ROW, COL].Text = "Storage Location";
            int colMaterialStorageName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Transaction Qty";
            int colTransactionQty = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++; 

            worksheet[ROW, COL].Text = "Currency";
            int colCurrencyCode = COL;
            worksheet[ROW, COL].ColumnWidth = 6;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++; 

            worksheet[ROW, COL].Text = "Transaction Amount";
            int colTransactionAmount  = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++; 

            worksheet[ROW, COL].Text = "TDS";
            int colTDSTax = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++; 

            worksheet[ROW, COL].Text = "TDS Voucher No";
            int colTDSVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Tax Status";
            int colTaxStatus = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;
            int Row_Total_Start = ROW;
            for (int i = 0; i < dtServiceParkedData.Rows.Count; i++)
            {
                worksheet[ROW, colGRNType].Text = dtServiceParkedData.Rows[i]["GRNType"].ToString();
                worksheet[ROW, colParticular].Text = dtServiceParkedData.Rows[i]["Particular"].ToString();
                worksheet[ROW, colGRNNo].Text = dtServiceParkedData.Rows[i]["Id"].ToString();
                worksheet[ROW, colGRNDate].Text = dtServiceParkedData.Rows[i]["GRNDate"].ToString();
                worksheet[ROW, colGateEntryNo].Text = dtServiceParkedData.Rows[i]["GateEntryNo"].ToString();
                worksheet[ROW, colVoucherNo].Text = dtServiceParkedData.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colPostingDate].Text = dtServiceParkedData.Rows[i]["PostingDate"].ToString();
                worksheet[ROW, colPONo].Text = dtServiceParkedData.Rows[i]["POId"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtServiceParkedData.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colDocDate].Text = dtServiceParkedData.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colMaterialStorageName].Text = dtServiceParkedData.Rows[i]["MaterialStorageName"].ToString();

                worksheet[ROW, colTransactionQty].Number = clsStaticInfo.dbl(dtServiceParkedData.Rows[i]["TransactionQty"].ToString());
                worksheet[ROW, colTransactionQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colCurrencyCode].Text = dtServiceParkedData.Rows[i]["CurrencyCode"].ToString();
                worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtServiceParkedData.Rows[i]["TransactionAmount"].ToString());
                worksheet[ROW, colTransactionAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colTDSTax].Number = clsStaticInfo.dbl(dtServiceParkedData.Rows[i]["TDSTax"].ToString());
                worksheet[ROW, colTDSTax].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colTDSVoucherNo].Text = dtServiceParkedData.Rows[i]["TDSVoucherNo"].ToString();
                worksheet[ROW, colTaxStatus].Text = dtServiceParkedData.Rows[i]["IsTDSTaxPost"].ToString();
               
                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            var report = new ReportUtility();
            // var workbook = report.GetWorkbook(ref excelEngine, 1);
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "TDS Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }

        public DataTable GetServiceTDSData(string plantId)
        {
            var cmdText = @"select * from(SELECT IR.Id,IR.Id GRNNo, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
			                        , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                    , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice
                                    , InvoiceId=CASE WHEN IR.EmployeeId<> '' THEN EP.Id ELSE IV.Id END
                                    , IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IR.InvoicingPartyPlantId PartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable
                                    , COUNT(*) OVER () AS TotalRows
									,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
									,IR.GateEntryNo,IR.POId,IR.ToCurrencyRate,IR.NoteForAccounts Narration
									,VoucherNo = CASE WHEN IR.EmployeeId <>'' THEN VE.VoucherNo ELSE V.VoucherNo END
									,VoucherId = CASE WHEN IR.EmployeeId <>'' THEN VE.Id ELSE V.Id END
									,VoucherTypeId = CASE WHEN IR.EmployeeId <>'' THEN VE.VoucherTypeId ELSE V.VoucherTypeId END
									,PostingDate= CASE WHEN IR.EmployeeId <>'' THEN REPLACE(CONVERT(CHAR(11), VE.PostingDate, 106),' ','-') ELSE REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') END
                                    ,MS.UserName MaterialStorageName, IR.IsFOC, ISNULL(ADT.TaxAmount,0) TDSTax, ADT.VoucherId TDSTaxVoucherId, ADT.Id AdditionalTaxId
                                    ,IsTDSTaxPost=CASE WHEN ADT.VoucherId<>'' THEN 'TDSPosted' WHEN  ADT.InventoryReceiveId IS NULL THEN '' ELSE 'TDSParked' end
									,VT.VoucherNo TDSVoucherNo,V.IsPark,IV.WrittenOffAmount
                                    ,IR.OtherPartyId,IR.OtherPartyPlantId
						FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        --LEFT JOIN TRN.GRNAcceptanceMap IGD ON IGD.GRNId=IR.Id
                        LEFT JOIN TRN.Invoice IV ON IV.inventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
						LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher VE ON VE.Id=EP.VoucherId
                        LEFT JOIN HKP.MaterialStorage MS ON MS.Id=IR.MaterialStorageId
                        LEFT JOIN TRN.AdditionalTax ADT ON ADT.InventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher VT ON VT.Id=ADT.VoucherId
                        WHERE IR.PlantId='" + plantId + @"') x
						where x.TDSTax>0 and x.IsTDSTaxPost='TDSParked'";

            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetEmployeeLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string employeeId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 7;
                var colLast1 = 7;
                var col = 1;
                int colCompanyCurrDebit = 0;
                int colCompanyCurrCredit = 0;

                // Get Employee Master
                var employee = _employeeInformationService.Find(employeeId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, employee.EmployeeCode + " - " + employee.EmployeeName);
                sheet.Range[row, 3, row, 5].Merge();

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                }
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher Date", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", ExcelHAlign.HAlignCenter); col++;




                //reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Voucher Date", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 20); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignCenter);int colDebit = col; 
                //reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignCenter);int colCredit = col; 

                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignCenter); colCompanyCurrDebit = col; col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignCenter); colCompanyCurrCredit = col; col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Balance", ExcelHAlign.HAlignCenter); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 5, ExcelHAlign.HAlignRight);

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();

                // Get Employee opening balance data.
                var obVal = GetEmployeeOpeningBalance(companyGroupId, companyId, plantId, employeeId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }
                int StartRow = row;

                row++;

                var ledgerData = GetEmployeeLedger(companyGroupId, companyId, plantId, employeeId, fromDate, toDate);
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";
                            sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;
                        }
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                //if (!string.IsNullOrEmpty(companyCurrencyId))
                //{

                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                    sheet.Range[row, colCompanyCurrDebit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colCompanyCurrDebit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colCompanyCurrDebit) + (row - 1) + ")";
                    sheet.Range[row, colCompanyCurrDebit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet.Range[row, colCompanyCurrCredit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colCompanyCurrCredit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colCompanyCurrCredit) + (row - 1) + ")";
                    sheet.Range[row, colCompanyCurrCredit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, col, "Employee Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook EmployeeSalaryAdvanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string employeeId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "EmployeeSalaryAdvanceLedger";
                var colLast = 7;
                //var colLast1 = 7;
                var colLast1 = 4;
                var col = 1;
                // Get Employee Master
                var employee = _employeeInformationService.Find(employeeId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee");
                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, employee.EmployeeCode + " - " + employee.EmployeeName);
                sheet.Range[row, 2, row, 4].Merge();

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row, 5, row, 5 + 3].Merge();
                }
                // Set Row Header
                row++;
                // reportUtility.SetHeaderText(ref sheet, row, col, "GL", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher Date", ExcelHAlign.HAlignCenter); col++;
                // reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", ExcelHAlign.HAlignCenter); col++;
                // reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", ExcelHAlign.HAlignCenter); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 23, ExcelHAlign.HAlignCenter); col++;




                //reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Voucher Date", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 12); col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 20); col++;
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignCenter); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignCenter); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Balance", ExcelHAlign.HAlignCenter); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 5, ExcelHAlign.HAlignCenter);

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();

                // Get Employee opening balance data.
                var obVal = GetEmployeeSalaryAdvanceLedgerOBeData(companyGroupId, companyId, plantId, employeeId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }
                row++;

                var ledgerData = GetEmployeeSalaryAdvanceData(companyGroupId, companyId, plantId, employeeId, fromDate, toDate);
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                        // reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString()); col++;
                        //reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        //reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            //Balance Plus & Minus
                            sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";
                            sheet[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;


                        }
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;

                        row++;


                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1); // (Closing Balance= 10875)
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, col, "Employee Salary Advance Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetEmployeePayableReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = _employeePayableService.GetEmployeePayableReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.EmployeePayable);

            // Set report Name
            reportFileName = Convert.ToDateTime(headerData["PostingDate"]).ToString("yyMMdd") + " " + headerData["VoucherNo"];

            var _row = 5;
            var shet2EndxlsCol = 1;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Voucher No");
            report.SetText(ref sheet, _row, 2, headerData["VoucherNo"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, 1, "Doc Date");
            report.SetText(ref sheet, _row, 2, headerData["DocDate"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, 1, "Posting Date");
            report.SetText(ref sheet, _row, 2, headerData["PostingDate"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Employee");
            report.SetText(ref sheet, _row, 2, headerData["EmployeeName"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            var _rowL = 11;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Narration");
            report.SetText(ref sheet, _row, 2, headerData["Narration"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            var _rowR = 5;

            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Voucher Date");
            report.SetText(ref sheet, _rowR, 5, headerData["VoucherDate"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Invoice No");
            report.SetText(ref sheet, _rowR, 5, headerData["DocRefNo"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Fiscal Year");
            report.SetText(ref sheet, _rowR, 5, headerData["FiscalYearName"] + "(" + headerData["PeriodNo"] + ")");
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Status");
            report.SetText(ref sheet, _rowR, 5, Convert.ToBoolean(headerData["IsPark"]) ? "Parked" : "Posted");
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            var headreColIndex = 1;

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 24);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 20);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 18);
            headreColIndex++;

            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Currency", 10);
                headreColIndex++;
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Value", 10);
                headreColIndex++;
            }

            double _Total_Amount = 0;
            report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, 12, ExcelHAlign.HAlignCenter);
            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

            shet2EndxlsCol = headreColIndex;

            var dtGeneralVoucher = GetEmployeePayable(voucherId);

            double vAmount = 0;
            var data = _employeePayableService.GetAdvanceWriteOffReportData(companyId, voucherId);

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                var drcrCol = 1;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["GLGeneralInfoCode"] + " - " + data[n]["GLGeneralInfoName"]); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["BudgetName"].ToString()); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["ActivityName"].ToString()); drcrCol++;
                if (companyCurrencyId != headerData["CurrencyId"].ToString())
                {
                    report.SetText(ref sheet, _rowL, drcrCol, data[n]["DrAmount"].ToString()); drcrCol++;
                    report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CrAmount"])); drcrCol++;
                    vAmount += Convert.ToDouble(data[n]["CrAmount"].ToString());
                }

                report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CompanyCurrencyDrAmount"].ToString())); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CompanyCurrencyCrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(data[n]["CompanyCurrencyCrAmount"].ToString());
            }

            _rowL++;
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetText(ref sheet, _rowL, 1, "Total :", true);
                sheet[_rowL, 1, _rowL, shet2EndxlsCol - 3].Merge();
            }
            else
            {
                report.SetText(ref sheet, _rowL, 1, "Total :", true);
                sheet[_rowL, 1, _rowL, shet2EndxlsCol - 2].Merge();
            }

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                sheet.Range[_rowL, shet2EndxlsCol - 2].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol - 2) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol - 2) + (_rowL - 1) + ")";
                sheet.Range[_rowL, shet2EndxlsCol - 2].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, shet2EndxlsCol - 2].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, shet2EndxlsCol - 2].BorderAround(ExcelLineStyle.Hair);
            }
            sheet.Range[_rowL, shet2EndxlsCol - 1].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol - 1) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol - 1) + (_rowL - 1) + ")";
            sheet.Range[_rowL, shet2EndxlsCol - 1].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, shet2EndxlsCol - 1].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, shet2EndxlsCol - 1].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[_rowL, shet2EndxlsCol].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, shet2EndxlsCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, shet2EndxlsCol].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            vAmount = vAmount / 2;
            _rowL += 1;

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            }

            report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(vAmount, headerData["CurrencyId"].ToString());
                sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;
                _rowL += 1;
            }
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(_Total_Amount, companyCurrencyId);
            sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;

            _rowL = _rowL + 4;

            report.SetSignatureText(ref sheet, _rowL - 1, 1, headerData["AddedBy"].ToString());
            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

            report.SetSignatureText(ref sheet, _rowL - 1, 3, headerData["PostedBy"].ToString());
            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

            sheet.Range[_rowL, shet2EndxlsCol].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, shet2EndxlsCol, "Authorized By", true);

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headerData["VoucherTypeName"].ToString(), companyId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }

        public IWorkbook GetEmployeeSalaryPayableReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = _employeePayableService.GetEmployeePayableReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.SalaryPayable);

            // Set report Name
            reportFileName = Convert.ToDateTime(headerData["PostingDate"]).ToString("yyMMdd") + " " + headerData["VoucherNo"];

            var _row = 5;
            var shet2EndxlsCol = 1;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Voucher No");
            report.SetText(ref sheet, _row, 2, headerData["VoucherNo"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, 1, "Doc Date");
            report.SetText(ref sheet, _row, 2, headerData["DocDate"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, 1, "Posting Date");
            report.SetText(ref sheet, _row, 2, headerData["PostingDate"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Employee");
            report.SetText(ref sheet, _row, 2, headerData["EmployeeName"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            var _rowL = 11;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Narration");
            report.SetText(ref sheet, _row, 2, headerData["Narration"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            var _rowR = 5;

            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Voucher Date");
            report.SetText(ref sheet, _rowR, 5, headerData["VoucherDate"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Invoice No");
            report.SetText(ref sheet, _rowR, 5, headerData["DocRefNo"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Fiscal Year");
            report.SetText(ref sheet, _rowR, 5, headerData["FiscalYearName"] + "(" + headerData["PeriodNo"] + ")");
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Status");
            report.SetText(ref sheet, _rowR, 5, Convert.ToBoolean(headerData["IsPark"]) ? "Parked" : "Posted");
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            var headreColIndex = 1;

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 24);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 20);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 18);
            headreColIndex++;

            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Currency", 10);
                headreColIndex++;
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Value", 10);
                headreColIndex++;
            }

            double _Total_Amount = 0;
            report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, 12, ExcelHAlign.HAlignCenter);
            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

            shet2EndxlsCol = headreColIndex;

            var dtGeneralVoucher = GetEmployeePayable(voucherId);

            double vAmount = 0;
            var data = _employeePayableService.GetAdvanceWriteOffReportData(companyId, voucherId);

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                var drcrCol = 1;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["GLGeneralInfoCode"] + " - " + data[n]["GLGeneralInfoName"]); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["BudgetName"].ToString()); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["ActivityName"].ToString()); drcrCol++;
                if (companyCurrencyId != headerData["CurrencyId"].ToString())
                {
                    report.SetText(ref sheet, _rowL, drcrCol, data[n]["DrAmount"].ToString()); drcrCol++;
                    report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CrAmount"])); drcrCol++;
                    vAmount += Convert.ToDouble(data[n]["CrAmount"].ToString());
                }

                report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CompanyCurrencyDrAmount"].ToString())); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CompanyCurrencyCrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(data[n]["CompanyCurrencyCrAmount"].ToString());
            }

            _rowL++;
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetText(ref sheet, _rowL, 1, "Total :", true);
                sheet[_rowL, 1, _rowL, shet2EndxlsCol - 3].Merge();
            }
            else
            {
                report.SetText(ref sheet, _rowL, 1, "Total :", true);
                sheet[_rowL, 1, _rowL, shet2EndxlsCol - 2].Merge();
            }

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                sheet.Range[_rowL, shet2EndxlsCol - 2].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol - 2) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol - 2) + (_rowL - 1) + ")";
                sheet.Range[_rowL, shet2EndxlsCol - 2].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, shet2EndxlsCol - 2].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, shet2EndxlsCol - 2].BorderAround(ExcelLineStyle.Hair);
            }
            sheet.Range[_rowL, shet2EndxlsCol - 1].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol - 1) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol - 1) + (_rowL - 1) + ")";
            sheet.Range[_rowL, shet2EndxlsCol - 1].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, shet2EndxlsCol - 1].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, shet2EndxlsCol - 1].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[_rowL, shet2EndxlsCol].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, shet2EndxlsCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, shet2EndxlsCol].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            vAmount = vAmount / 2;
            _rowL += 1;

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            }

            report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(vAmount, headerData["CurrencyId"].ToString());
                sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;
                _rowL += 1;
            }
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(_Total_Amount, companyCurrencyId);
            sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;

            _rowL = _rowL + 4;

            report.SetSignatureText(ref sheet, _rowL - 1, 1, headerData["AddedBy"].ToString());
            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

            report.SetSignatureText(ref sheet, _rowL - 1, 3, headerData["PostedBy"].ToString());
            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

            sheet.Range[_rowL, shet2EndxlsCol].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, shet2EndxlsCol, "Authorized By", true);

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headerData["VoucherTypeName"].ToString(), companyId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }
        public IWorkbook GetBudgetMasterReport(string companyGroupId, string coaId, bool isActivityLevel)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 2);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                CreateBudgetMasterReportSheet1(ref sheet1, reportUtility, companyGroupId, coaId, isActivityLevel);
                CreateBudgetMasterReportSheet2(ref sheet2, reportUtility, companyGroupId, coaId, isActivityLevel);
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateBudgetMasterReportSheet1(ref IWorksheet sheet, ReportUtility reportUtility, string companyGroupId, string coaId, bool isActivityLevel)
        {

            try
            {
                DataTable dtBudgetMaster = null;

                #region List data

                var budgetMasterList = isActivityLevel ? GetBudgetMasterActivityData(coaId) : GetBudgetMasterData(coaId);
                var dvBudgetMaster = new DataView(budgetMasterList)
                {
                    Sort = "GLGeneralInfoCode"
                };
                dtBudgetMaster = dvBudgetMaster.ToTable();

                var dvGLLevel1 = new DataView(budgetMasterList)
                {
                    Sort = "Level1"
                };
                var dtGLLevel1 = dvGLLevel1.ToTable(true, "Level1", "Level1Id");

                DataView dvGLLevel2 = null;
                DataTable dtGLLevel2 = null;

                DataView dvGLLevel3 = null;
                DataTable dtGLLevel3 = null;

                DataView dvGLLevel4 = null;
                DataTable dtGLLevel4 = null;

                DataView dvGL = null;
                DataTable dtGL = null;

                DataView dvGLCode = null;
                DataTable dtGLCode = null;

                if (dtBudgetMaster.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var level1ColIndex = 1;
                var level2ColIndex = 2;
                var level3ColIndex = 3;
                var level4ColIndex = 4;
                var CARefNo = 5;
                var glCodeColIndex = 6;
                var glColIndex = 7;
                var glCol = 8;
                var budgetColIndex = 9;

                var _col3 = 3;

                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "COA");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtBudgetMaster.Rows[0]["COA"].ToString()); _rowL++;
                sheet[reportUtility.GetColumnNameForXls(_col3) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtBudgetMaster.Columns.Count; i++)
                {

                    if (dtBudgetMaster.Columns[i].ColumnName != "TotalRows" && dtBudgetMaster.Columns[i].ColumnName != "Level1Id" && dtBudgetMaster.Columns[i].ColumnName != "Level2Id" && dtBudgetMaster.Columns[i].ColumnName != "Level3Id" && dtBudgetMaster.Columns[i].ColumnName != "Level4Id" && dtBudgetMaster.Columns[i].ColumnName != "COA")
                    {
                        _colIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtBudgetMaster.Columns[i].ColumnName);
                    }
                }

                shet2EndxlsCol = _colIndex;

                for (int m = 0; m < dtGLLevel1.Rows.Count; m++)
                {
                    _rowL++;
                    var level1Id = dtGLLevel1.Rows[m]["Level1Id"].ToString();
                    dvGLLevel2 = new DataView(dtBudgetMaster)
                    {
                        Sort = "Level2",
                        RowFilter = "Level1Id='" + level1Id + "'"
                    };
                    dtGLLevel2 = dvGLLevel2.ToTable(true, "Level2", "Level2Id");
                    var rowStartLevel1 = _rowL;
                    reportUtility.SetText(ref sheet, _rowL, level1ColIndex, dtGLLevel1.Rows[m]["Level1"].ToString(), 26);

                    for (int n = 0; n < dtGLLevel2.Rows.Count; n++)
                    {
                        var level2Id = dtGLLevel2.Rows[n]["Level2Id"].ToString();
                        dvGLLevel3 = new DataView(dtBudgetMaster)
                        {
                            Sort = "Level3",
                            RowFilter = "Level2Id='" + level2Id + "' and Level1Id='" + level1Id + "'"
                        };
                        dtGLLevel3 = dvGLLevel3.ToTable(true, "Level3", "Level3Id");
                        var rowStartLevel2 = _rowL;
                        reportUtility.SetText(ref sheet, _rowL, level2ColIndex, dtGLLevel2.Rows[n]["Level2"].ToString(), 26);

                        for (int o = 0; o < dtGLLevel3.Rows.Count; o++)
                        {
                            var level3Id = dtGLLevel3.Rows[o]["Level3Id"].ToString();
                            dvGLLevel4 = new DataView(dtBudgetMaster)
                            {
                                Sort = "Level4",
                                RowFilter = "Level3Id='" + level3Id + "' and Level2Id='" + level2Id + "' and Level1Id='" + level1Id + "'"
                            };
                            dtGLLevel4 = dvGLLevel4.ToTable(true, "Level4", "Level4Id");
                            var rowStartLevel3 = _rowL;
                            reportUtility.SetText(ref sheet, _rowL, level3ColIndex, dtGLLevel3.Rows[o]["Level3"].ToString(), 26);

                            for (int p = 0; p < dtGLLevel4.Rows.Count; p++)
                            {
                                var level4Id = dtGLLevel4.Rows[p]["Level4Id"].ToString();
                                dvGLCode = new DataView(dtBudgetMaster)
                                {
                                    Sort = "GLName",
                                    RowFilter = "Level4Id='" + level4Id + "' and Level3Id='" + level3Id + "' and Level2Id='" + level2Id + "' and Level1Id='" + level1Id + "'"
                                };
                                dtGLCode = dvGLCode.ToTable(true, "GLId", "GLName", "GLGeneralInfoCode", "CARefNo");
                                var rowStartGroup4 = _rowL;
                                reportUtility.SetText(ref sheet, _rowL, level4ColIndex, dtGLLevel4.Rows[p]["Level4"].ToString(), 26);

                                for (int r = 0; r < dtGLCode.Rows.Count; r++)
                                {
                                    var glCode = dtGLCode.Rows[r]["GLGeneralInfoCode"].ToString();
                                    dvGL = new DataView(dtBudgetMaster)
                                    {
                                        Sort = "GLName",
                                        RowFilter = "GLGeneralInfoCode='" + glCode + "' and Level4Id='" + level4Id + "' and Level3Id='" + level3Id + "' and Level2Id='" + level2Id + "' and Level1Id='" + level1Id + "'"
                                    };
                                    if (isActivityLevel)
                                    {
                                        dtGL = dvGL.ToTable(true, "GLId", "GLName", "GLGeneralInfoCode", "BudgetMasterId", "Budget", "Activity", "ActivityId", "Default", "Specific", "BudgetCategory", "BudgetSubCategory", "RefNo", "FACode", "FixedAssetMaster", "BudgetGroup", "Register", "Project", "Manufacturing", "Treding", nameof(Service));
                                    }
                                    else
                                    {
                                        dtGL = dvGL.ToTable(true, "GLId", "GLName", "GLGeneralInfoCode", "BudgetMasterId", "Budget", "BudgetCategory", "BudgetSubCategory", "RefNo", "FACode", "FixedAssetMaster", "BudgetGroup", "Register", "Project", "Manufacturing", "Treding", nameof(Service));
                                    }
                                    var rowStartGLCode = _rowL;


                                    reportUtility.SetText(ref sheet, _rowL, CARefNo, dtGLCode.Rows[r]["CARefNo"].ToString(), 10);
                                    reportUtility.SetText(ref sheet, _rowL, glCodeColIndex, dtGLCode.Rows[r]["GLGeneralInfoCode"].ToString(), 15);
                                    reportUtility.SetText(ref sheet, _rowL, glColIndex, dtGLCode.Rows[r]["GLId"].ToString(), 26);
                                    reportUtility.SetText(ref sheet, _rowL, glCol, dtGLCode.Rows[r]["GLName"].ToString(), 26);

                                    if (dtGL.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < dtGL.Rows.Count; i++)
                                        {
                                            //glColIndex++;
                                            var Budget = dtGL.Rows[i]["Budget"].ToString();

                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["BudgetGroup"].ToString(), 26); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["BudgetCategory"].ToString(), 26); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["BudgetSubCategory"].ToString(), 26); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["RefNo"].ToString(), 6); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["BudgetMasterId"].ToString(), 20); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i][nameof(Budget)].ToString(), 26); budgetColIndex++;
                                            if (isActivityLevel)
                                            {
                                                reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Activity"].ToString(), 26); budgetColIndex++;
                                                reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["ActivityId"].ToString(), 16); budgetColIndex++;
                                                reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Default"].ToString(), 16); budgetColIndex++;
                                                reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Specific"].ToString(), 8); budgetColIndex++;

                                            }
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["FACode"].ToString(), 6); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["FixedAssetMaster"].ToString(), 26); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Register"].ToString(), 12); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Project"].ToString(), 8); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Manufacturing"].ToString(), 12); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i]["Treding"].ToString(), 8); budgetColIndex++;
                                            reportUtility.SetText(ref sheet, _rowL, budgetColIndex, dtGL.Rows[i][nameof(Service)].ToString(), 15); budgetColIndex++;

                                            var colorCol = budgetColIndex;

                                            if (string.IsNullOrEmpty(Budget))
                                            {
                                                sheet.Range[_rowL, 7].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                                sheet.Range[_rowL, 8].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                                sheet.Range[_rowL, 9].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                                sheet.Range[_rowL, 10].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                                sheet.Range[_rowL, 11].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                                sheet.Range[_rowL, 12].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                            }

                                            _rowL++;
                                            CARefNo = 5;
                                            glCodeColIndex = 6;
                                            glColIndex = 7;
                                            glCol = 8;
                                            budgetColIndex = 9;
                                        }
                                    }
                                    sheet[rowStartGLCode, CARefNo, _rowL - 1, CARefNo].Merge();
                                    sheet[rowStartGLCode, glCodeColIndex, _rowL - 1, glCodeColIndex].Merge();
                                    sheet[rowStartGLCode, glColIndex, _rowL - 1, glColIndex].Merge();
                                    sheet[rowStartGLCode, glCol, _rowL - 1, glCol].Merge();
                                }//GL
                                try { sheet[rowStartGroup4, level4ColIndex, _rowL - 1, level4ColIndex].Merge(); } catch { }
                            }//Level4

                            try { sheet[rowStartLevel3, level3ColIndex, _rowL - 1, level3ColIndex].Merge(); } catch { }
                        }//Level3
                        try { sheet[rowStartLevel2, level2ColIndex, _rowL - 1, level2ColIndex].Merge(); } catch { }
                    }//Level2
                    try { sheet[rowStartLevel1, level1ColIndex, _rowL - 1, level1ColIndex].Merge(); } catch { }
                    _rowL--;
                }//Level1

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = "Report";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Budget Master Report", companyGroupId);
                reportUtility.FreezePage(ref sheet, 1, 8);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        private void CreateBudgetMasterReportSheet2(ref IWorksheet sheet, ReportUtility reportUtility, string companyGroupId, string coaId, bool isActivityLevel)
        {
            DataTable dtBudgetMaster = null;

            #region List data

            var budgetMasterList = isActivityLevel ? GetBudgetMasterActivityData(coaId) : GetBudgetMasterData(coaId);
            var dvBudgetMaster = new DataView(budgetMasterList)
            {
                Sort = "GLGeneralInfoCode"
            };
            dtBudgetMaster = dvBudgetMaster.ToTable();
            if (dtBudgetMaster.Rows.Count == 0)
            {
                throw new Exception("No Data Found !!!");
            }

            #endregion List data

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;

            var _col3 = 3;

            reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "COA");
            sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
            reportUtility.SetText(ref sheet, _rowL, _col + 2, dtBudgetMaster.Rows[0]["COA"].ToString()); _rowL++;
            sheet[reportUtility.GetColumnNameForXls(_col3) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

            _rowL = 6;
            _rowL++;

            for (int i = 0; i < dtBudgetMaster.Columns.Count; i++)
            {
                if (dtBudgetMaster.Columns[i].ColumnName != "TotalRows" && dtBudgetMaster.Columns[i].ColumnName != "Level1Id" && dtBudgetMaster.Columns[i].ColumnName != "Level2Id" && dtBudgetMaster.Columns[i].ColumnName != "Level3Id" && dtBudgetMaster.Columns[i].ColumnName != "Level4Id" && dtBudgetMaster.Columns[i].ColumnName != "COA")
                {
                    _colIndex++;
                    reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtBudgetMaster.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;

            for (int i = 0; i < dtBudgetMaster.Rows.Count; i++)
            {
                _rowL++;
                int col = 1;

                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Level1"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Level2"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Level3"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Level4"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["CARefNo"].ToString(), 15); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["GLGeneralInfoCode"].ToString(), 15); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["GLId"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["GLName"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["BudgetGroup"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["BudgetCategory"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["BudgetSubCategory"].ToString(), 26); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["RefNo"].ToString(), 6); col++;
                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["BudgetMasterId"].ToString(), 20); col++;

                reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Budget"].ToString(), 26); col++;

                if (isActivityLevel)
                {
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Activity"].ToString(), 26); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["ActivityId"].ToString(), 16); col++;
                    //reportUtility.SetText(ref sheet, _rowL, 15, dtBudgetMaster.Rows[i]["BudgetMasterActivityId"].ToString(), 18);col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Default"].ToString(), 8); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Specific"].ToString(), 8); col++;

                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["FACode"].ToString(), 6); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["FixedAssetMaster"].ToString(), 26); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Register"].ToString(), 26); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Project"].ToString(), 8); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Manufacturing"].ToString(), 12); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Treding"].ToString(), 8); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i][nameof(Service)].ToString(), 15); col++;
                }
                else
                {
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["FACode"].ToString(), 6); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["FixedAssetMaster"].ToString(), 26); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Register"].ToString(), 26); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Project"].ToString(), 8); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Manufacturing"].ToString(), 12); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i]["Treding"].ToString(), 8); col++;
                    reportUtility.SetText(ref sheet, _rowL, col, dtBudgetMaster.Rows[i][nameof(Service)].ToString(), 15); col++;
                }



            }

            sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = "Data";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Budget Master List", companyGroupId);
            reportUtility.FreezePage(ref sheet, 1, 8);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);

        }



        public List<Dictionary<string, object>> GetPartyPaymentStatusData(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"SELECT SUM(X.NoOfInvoice) NoOfInvoice,convert(bit,0) AS isSelected,X.PartyId,X.PartyPlantId,X.PartyCode,X.PartyName,X.PartyPlantName,SUM(X.Gross) Gross ,SUM(X.SetOff) SetOff,SUM(X.Balance) Balance
                ,SUM(X.BooksGross) BooksGross,SUM(X.BooksSetOff) BooksSetOff,SUM(X.BooksBalance) BooksBalance
                FROM (
                SELECT count(IV.PartyId)NoOfInvoice,IV.PartyId, IV.PartyPlantId,P.Code PartyCode,P.UserName PartyName, PP.UserName AS PartyPlantName
                , SUM(ISNULL(IVD.NetAmount,0)) AS Gross,SUM(ISNULL(IVD.WrittenOffAmount,0)) AS SetOff, SUM(ISNULL(IVD.NetAmount-IVD.WrittenOffAmount,0)) AS Balance
                , SUM(ISNULL(IVD.NetAmount*CC.CompanyCurrencyRate,0)) AS BooksGross,SUM(ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0)) AS BooksSetOff, SUM(ISNULL((IVD.NetAmount*CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount*CC.CompanyCurrencyRate),0)) AS BooksBalance
                
                FROM [TRN].[InvoiceDetail] AS IVD
                LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                ) AS CC ON CC.VoucherDetailId=VD.Id
                
                WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('VendorInvoice','PurchaseDocAcceptance','SuspensePayable','EmployeePayable')
                AND IV.CompanyGroupId='" + companyGroupId + "' AND IV.CompanyId='" + companyId + "' AND IV.PlantId='" + plantId + @"'
                GROUP BY IV.PartyId, IV.PartyPlantId, PP.UserName,P.UserName,p.code
                
                UNION ALL
                SELECT count(IV.PartyId)NoOfInvoice,IV.PartyId, IV.PartyPlantId,P.Code PartyCode,P.UserName PartyName, PP.UserName AS PartyPlantName, SUM(ISNULL(IVD.NetAmount,0)) AS Gross,
                SUM(ISNULL(IVD.WrittenOffAmount,0)) AS SetOff, SUM(ISNULL(IVD.NetAmount-IVD.WrittenOffAmount,0)) AS Balance
                , SUM(ISNULL(IVD.NetAmount*CC.CompanyCurrencyRate,0)) AS BooksGross,SUM(ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0)) AS BooksSetOff, SUM(ISNULL((IVD.NetAmount*CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount*CC.CompanyCurrencyRate),0)) AS BooksBalance
                
                FROM [TRN].[InvoiceDetail] AS IVD
                LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IV.InventoryReceiveId
                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                ) AS CC ON CC.VoucherDetailId=VD.Id
                
                WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('InventoryPayable')
                AND IV.CompanyGroupId='" + companyGroupId + "' AND IV.CompanyId='" + companyId + "' AND IV.PlantId='" + plantId + @"'
                AND IR.PurchaseDocumentAcceptanceId IS NULL
                GROUP BY IV.PartyId, IV.PartyPlantId, PP.UserName,P.UserName,P.Code)
                X
                GROUP BY PartyId,PartyPlantId,PartyName,PartyPlantName,PartyCode
                order by X.PartyName";
            return _sqlRepository.GetDataCollection(sql);

        }



        public IWorkbook GetPartyPaymentStatusDetailReport(ExcelEngine excelEngine, string MasterLCList, string CompanyGroupId, string CompanyId, string PlantId) // , string MasterLCList
        {
            excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                worksheet.Name = "PartyPaymentStatusDetailReport";

                int COL = 1; int ROW = 6;

                int startCol = COL;
                worksheet[ROW, COL].Text = "SL. No";
                int colSLNO = COL;
                worksheet[ROW, COL].ColumnWidth = 7;
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                worksheet[ROW, COL].Text = "Party Code";
                int colPartyCode = COL;
                worksheet[ROW, COL].ColumnWidth = 12;
                COL++;

                worksheet[ROW, COL].Text = "Party";
                int colPartyName = COL;
                worksheet[ROW, COL].ColumnWidth = 35;
                COL++;

                worksheet[ROW, COL].Text = "Party Plant";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colPartyPlantName = COL;
                worksheet[ROW, COL].ColumnWidth = 35;
                COL++;


                worksheet[ROW, COL].Text = "Voucher No";
                int colVoucherNo = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Posting Date";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colPostingDate = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Doc RefNo";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colDocRefNo = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Doc Date";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colDocDate = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Due Date";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colActualDueDate = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Books Gross";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                worksheet[ROW, COL].ColumnWidth = 15;
                int colBooksGross = COL;
                COL++;
                worksheet[ROW, COL].Text = "Books DebitNote";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                worksheet[ROW, COL].ColumnWidth = 15;
                int colBooksDebitNote = COL;
                COL++;
                worksheet[ROW, COL].Text = "Books Tax";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                worksheet[ROW, COL].ColumnWidth = 15;
                int colBooksTax = COL;
                COL++;
                worksheet[ROW, COL].Text = "Books Payment";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBooksSetOff = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Books Balance";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBooksBalance = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;


                worksheet[ROW, COL].Text = "Trn Currency";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colTrnCurrency = COL;
                worksheet[ROW, COL].ColumnWidth = 12;
                COL++;


                worksheet[ROW, COL].Text = "Gross";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colGross = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;
                worksheet[ROW, COL].Text = "DebitNote";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDebitNote = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Tax";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTax = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;
                worksheet[ROW, COL].Text = "Payment";
                int colSetOff = COL;
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;


                worksheet[ROW, COL].Text = "Balance";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBalance = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                //COL++;

                int endCol = COL;

                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Size = 12;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Bold = true;

                //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                worksheet.Range[ROW, startCol, ROW, COL].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, startCol, ROW, COL].BorderInside(ExcelLineStyle.Hair);
                // worksheet.Range[ROW,  ROW].BorderInside(ExcelLineStyle.Hair);


                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                string sql = @"SELECT IV.PartyId, IV.PartyPlantId,p.Code PartyCode, P.UserName PartyName, PP.UserName AS PartyPlantName
                ,V.VoucherNo,V.DocRefNo InvoiceNo
		        , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate 
				, REPLACE(CONVERT(VARCHAR(11),iv.DocDate, 106), ' ', '-') AS DocDate
				, REPLACE(CONVERT(VARCHAR(11),iv.ActualDueDate , 106), ' ', '-') AS ActualDueDate 
				 ,C.Code TrnCurrency
                , ISNULL(IVD.NetAmount,0) AS Gross,0 DebitNoteAmount,0 BooksDebitNoteAmount, IWD.TaxAmount TaxAmount,
                 SetOff=ISNULL(IVD.WrittenOffAmount, 0) -ISNULL(IWD.TaxAmount,0), ISNULL(IVD.NetAmount-IVD.WrittenOffAmount,0) AS Balance
				, ISNULL(IVD.NetAmount*IV.CompanyCurrencyRate,0) AS BooksGross,
                   ISNULL(IVD.WrittenOffAmount*IV.CompanyCurrencyRate,0)-ISNULL(IWD.TaxAmount*IV.CompanyCurrencyRate,0) AS BooksSetOff,ISNULL(IWD.TaxAmount*IV.CompanyCurrencyRate,0) BooksTaxAmount, ISNULL((IVD.NetAmount*IV.CompanyCurrencyRate)-(IVD.WrittenOffAmount*IV.CompanyCurrencyRate),0) AS BooksBalance
                            
                FROM [TRN].[InvoiceDetail] AS IVD
                LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id
                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + CompanyId + @"'
                ) AS CC ON CC.VoucherDetailId=VD.Id

                WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('VendorInvoice','PurchaseDocAcceptance','SuspensePayable','EmployeePayable')
                AND IV.CompanyGroupId='" + CompanyGroupId + "' AND IV.CompanyId='" + CompanyId + "'  AND IV.PlantId='" + PlantId + @"'
                --GROUP BY IV.PartyId, IV.PartyPlantId, PP.UserName,P.UserName
                 AND IV.PartyId in(" + MasterLCList + @")

                UNION ALL
                SELECT IV.PartyId, IV.PartyPlantId,p.Code PartyCode, P.UserName PartyName, PP.UserName AS PartyPlantName
                ,V.VoucherNo,V.DocRefNo InvoiceNo
	        	, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
				, REPLACE(CONVERT(VARCHAR(11),iv.DocDate, 106), ' ', '-') AS DocDate
				, REPLACE(CONVERT(VARCHAR(11),iv.ActualDueDate , 106), ' ', '-') AS ActualDueDate ,C.Code TrnCurrency
                , ISNULL(IVD.NetAmount, 0) AS Gross,0 DebitNoteAmount, 0 BooksDebitNoteAmount,IWD.TaxAmount TaxAmount,
                 SetOff=ISNULL(IVD.WrittenOffAmount, 0) -ISNULL(IWD.TaxAmount,0), ISNULL(IVD.NetAmount - IVD.WrittenOffAmount, 0) AS Balance
				 , ISNULL(IVD.NetAmount*IV.CompanyCurrencyRate,0) AS BooksGross,
                   ISNULL(IVD.WrittenOffAmount*IV.CompanyCurrencyRate,0)-ISNULL(IWD.TaxAmount*IV.CompanyCurrencyRate,0) AS BooksSetOff,ISNULL(IWD.TaxAmount*IV.CompanyCurrencyRate,0) BooksTaxAmount, ISNULL((IVD.NetAmount*IV.CompanyCurrencyRate)-(IVD.WrittenOffAmount*IV.CompanyCurrencyRate),0) AS BooksBalance
                           
                FROM[TRN].[InvoiceDetail] AS IVD
                LEFT JOIN[TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id
                LEFT JOIN[HKP].[Party] AS P ON P.Id = IV.PartyId
                LEFT JOIN[HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
                LEFT JOIN[TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
                LEFT JOIN[TRN].[Voucher] AS V ON V.Id = VD.VoucherId
                LEFT JOIN[SCS].[Currency] AS C ON C.Id = IV.CurrencyId
                LEFT JOIN[ORG].[Entity] AS EN ON EN.Id = IV.EntityId
                LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id
                LEFT JOIN TRN.InventoryReceive IR ON IR.Id = IV.InventoryReceiveId
                LEFT JOIN(
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId= VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType= 'CompanyCurrency' AND CPC.CompanyId= '" + CompanyId + @"'
                ) AS CC ON CC.VoucherDetailId = VD.Id

                WHERE IV.Archive = 0 AND IV.IsWrittenOff = 0 AND IVD.IsWrittenOff = 0 AND V.IsPark = 0 AND IVD.IsBlock = 0 AND IV.SourceType in ('InventoryPayable')
                AND IV.CompanyGroupId = '" + CompanyGroupId + "' AND IV.CompanyId = '" + CompanyId + "' AND IV.PlantId = '" + PlantId + @"'
                AND IR.PurchaseDocumentAcceptanceId IS NULL
                AND IV.PartyId in(" + MasterLCList + @")
                order by P.UserName";


                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsData, false, "1"); ;


                if (dsData.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("No Data Found");
                }


                //con.getDataSet(@"Select * from EmployeeInformation", out DataSet dsData);
                //left join EmpDateWiseShiftAssign on ei.EmployeeCode=EmpDateWiseShiftAssign.GroupID
                ROW++;
                int StartDataRow = ROW;

                for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                {

                    worksheet[ROW, colSLNO].Number = (i + 1);
                    worksheet[ROW, colVoucherNo].Text = dsData.Tables[0].Rows[i]["VoucherNo"].ToString();

                    //worksheet[ROW, colPartyId].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["PartyId"].ToString());
                    //worksheet[ROW, colPartyPlantId].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["PartyPlantId"].ToString());
                    worksheet[ROW, colPartyName].Text = dsData.Tables[0].Rows[i]["PartyName"].ToString();
                    worksheet[ROW, colPartyPlantName].Text = dsData.Tables[0].Rows[i]["PartyPlantName"].ToString();
                    worksheet[ROW, colPostingDate].Text = dsData.Tables[0].Rows[i]["PostingDate"].ToString();
                    worksheet[ROW, colPartyCode].Text = dsData.Tables[0].Rows[i]["PartyCode"].ToString();
                    worksheet[ROW, colDocDate].Text = dsData.Tables[0].Rows[i]["DocDate"].ToString();
                    worksheet[ROW, colActualDueDate].Text = dsData.Tables[0].Rows[i]["ActualDueDate"].ToString();

                    worksheet[ROW, colDocRefNo].Text = dsData.Tables[0].Rows[i]["InvoiceNo"].ToString();
                    worksheet[ROW, colTrnCurrency].Text = dsData.Tables[0].Rows[i]["TrnCurrency"].ToString();


                    worksheet[ROW, colGross].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["Gross"].ToString());
                    worksheet[ROW, colGross].NumberFormat = "#,##0.00;(#,##0.00)";


                    worksheet[ROW, colDebitNote].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["DebitNoteAmount"].ToString());
                    worksheet[ROW, colDebitNote].NumberFormat = "#,##0.00;(#,##0.00)";

                    worksheet[ROW, colTax].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["TaxAmount"].ToString());
                    worksheet[ROW, colTax].NumberFormat = "#,##0.00;(#,##0.00)";

                    worksheet[ROW, colSetOff].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["SetOff"].ToString());
                    worksheet[ROW, colSetOff].NumberFormat = "#,##0.00;(#,##0.00)";

                    worksheet[ROW, colBalance].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["Balance"].ToString());
                    worksheet[ROW, colBalance].NumberFormat = "#,##0.00;(#,##0.00)";


                    worksheet[ROW, colBooksGross].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["BooksGross"].ToString());
                    worksheet[ROW, colBooksGross].NumberFormat = "#,##0.00;(#,##0.00)";

                    worksheet[ROW, colBooksDebitNote].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["DebitNoteAmount"].ToString());
                    worksheet[ROW, colBooksDebitNote].NumberFormat = "#,##0.00;(#,##0.00)";

                    worksheet[ROW, colBooksDebitNote].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["DebitNoteAmount"].ToString());
                    worksheet[ROW, colBooksDebitNote].NumberFormat = "#,##0.00;(#,##0.00)";

                    worksheet[ROW, colBooksTax].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["TaxAmount"].ToString());
                    worksheet[ROW, colBooksTax].NumberFormat = "#,##0.00;(#,##0.00)";

                    worksheet[ROW, colBooksBalance].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["BooksBalance"].ToString());
                    worksheet[ROW, colBooksBalance].NumberFormat = "#,##0.00;(#,##0.00)";



                    //    startRowGroup2 = ROW;
                    //    group2 = group1 + dsData.Tables[0].Rows[i]["ContractId"].ToString(); //ContractNo, ContractId

                    //    worksheet[ROW, colContractFundCommission].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "*" + (clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["CommissionPercentage"].ToString())).ToString() + "%";
                    //    worksheet[ROW, colContractFundUtilization].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colContractFundCommission) + ROW.ToString();

                    //    worksheet[ROW, colContractFundPercentage].Formula = clsStaticInfo.GetxlsCol(colContractFundUtilization) + ROW.ToString() + "*" + clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["PurchaseMargin"].ToString()) + "%";

                    //    worksheet[ROW, colContractId].Text = dsData.Tables[0].Rows[i]["ContractId"].ToString(); //ContractNo, ContractId
                    //    worksheet[ROW, colContractNo].Text = dsData.Tables[0].Rows[i]["ContractNo"].ToString(); //ContractNo, ContractId
                    //    worksheet[ROW, colMasterLCCustomerId].Text = dsData.Tables[0].Rows[i]["Buyer"].ToString(); // New
                    //    worksheet[ROW, colMasterOrderCurrencyId].Text = dsData.Tables[0].Rows[i]["MasterOrderCurrency"].ToString();
                    //    worksheet[ROW, colSalesOrderQty].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["ContractOrderQty"].ToString());
                    //    worksheet[ROW, colSalesOrderQty].NumberFormat = clsStaticInfo.NumberFormat();
                    //    worksheet[ROW, colSalesOrderValue].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["ContractOrderValue"].ToString());

                    //}

                    //if (group3 != group2 + dsData.Tables[0].Rows[i]["PurchaseLCRefNo"].ToString()) //PurchaseLCRefNo
                    //{
                    //    StartRowGroup3 = ROW;
                    //    group3 = group2 + dsData.Tables[0].Rows[i]["PurchaseLCRefNo"].ToString();

                    //    worksheet[ROW, colPurchaseLCNo].Text = dsData.Tables[0].Rows[i]["PurchaseLCRefNo"].ToString();

                    //    worksheet[ROW, colPurchaseLCCurrencyId].Text = dsData.Tables[0].Rows[i]["PurchasePLCurrency"].ToString();

                    //    worksheet[ROW, colPurchaseOrderDetailTrnQtyRate].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["POValue"].ToString());
                    //    worksheet[ROW, colPurchaseLCAmount].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["PurchaseLcOpeningValue"].ToString()); // PurchaseLcOpeningValue
                    //    worksheet[ROW, colPartyUserName].Text = dsData.Tables[0].Rows[i]["vendor"].ToString();
                    //    worksheet[ROW, colLastAmendmentDate].Text = dsData.Tables[0].Rows[i]["LastAmendmentDate"].ToString();
                    //    worksheet[ROW, colPresentLCValue].Number = clsStaticInfo.dbl(dsData.Tables[0].Rows[i]["PresentLCValue"].ToString());
                    //    worksheet[ROW, colPurchaseLCLCDate].Text = dsData.Tables[0].Rows[i]["PurchaseLCOpeningDate"].ToString();

                    //}
                    //worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                    ROW++;
                }

                //if (ROW > startRowGroup1 + 1)
                //{
                //    worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                //    worksheet[startRowGroup1, colMasterLCId, ROW - 1, colMasterLCId].Merge();
                //    worksheet[startRowGroup1, colMasterLCRefNo, ROW - 1, colMasterLCRefNo].Merge();
                //    worksheet[startRowGroup1, colMasterLCAmount, ROW - 1, colMasterLCAmount].Merge();
                //    // worksheet[startRowGroup1, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge();
                //    worksheet[startRowGroup1, colCurrencyCode, ROW - 1, colCurrencyCode].Merge();
                //    worksheet[startRowGroup1, colPartyId, ROW - 1, colPartyId].Merge();


                //}
                // worksheet[StartDataRow, colMasterLCAmount, ROW - 1, colMasterLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                //if (ROW > startRowGroup2 + 1)
                //{
                //    worksheet[startRowGroup2, colContractFundPercentage, ROW - 1, colContractFundPercentage].Merge();
                //    worksheet[startRowGroup2, colContractId, ROW - 1, colContractId].Merge();
                //    worksheet[startRowGroup2, colContractNo, ROW - 1, colContractNo].Merge();
                //    worksheet[startRowGroup2, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge(); //new buyer
                //    worksheet[startRowGroup2, colMasterOrderCurrencyId, ROW - 1, colMasterOrderCurrencyId].Merge();
                //    worksheet[startRowGroup2, colSalesOrderQty, ROW - 1, colSalesOrderQty].Merge();
                //    worksheet[startRowGroup2, colSalesOrderValue, ROW - 1, colSalesOrderValue].Merge();
                //    worksheet[startRowGroup2, colContractFundCommission, ROW - 1, colContractFundCommission].Merge();
                //    worksheet[startRowGroup2, colContractFundUtilization, ROW - 1, colContractFundUtilization].Merge();

                //}

                worksheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
                //worksheet[StartDataRow, colSalesOrderValue, ROW - 1, colSalesOrderValue].NumberFormat = "#,##0.00;(#,##0.00)";
                //worksheet[StartDataRow, colContractFundCommission, ROW - 1, colContractFundCommission].NumberFormat = "#,##0.00;(#,##0.00)";
                //worksheet[StartDataRow, colContractFundUtilization, ROW - 1, colContractFundUtilization].NumberFormat = "#,##0.00;(#,##0.00)";
                //worksheet[StartDataRow, colContractFundPercentage, ROW - 1, colContractFundPercentage].NumberFormat = "#,##0.00;(#,##0.00)";

                //  worksheet[ROW, colQty].Formula = "SUM("+ clsStaticInfo.GetxlsCol(colQty) + StartDataRow + ":"+ clsStaticInfo.GetxlsCol(colQty) + (ROW-1).ToString() + ")";

                worksheet[ROW, colBooksGross - 1].Text = "Total";
                worksheet[ROW, colBooksGross - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet[ROW, colBooksGross].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBooksGross) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colBooksGross) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colBooksGross].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, colBooksGross, ROW, colBooksGross].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;


                worksheet[ROW, colBooksDebitNote].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBooksDebitNote) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colBooksDebitNote) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colBooksDebitNote].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, colBooksDebitNote, ROW, colBooksDebitNote].CellStyle.Font.Bold = true;


                worksheet[ROW, colBooksTax].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBooksTax) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colBooksTax) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colBooksTax].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, colBooksTax, ROW, colBooksTax].CellStyle.Font.Bold = true;


                worksheet[ROW, colDebitNote].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colDebitNote) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colDebitNote) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colDebitNote].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, colDebitNote, ROW, colDebitNote].CellStyle.Font.Bold = true;


                worksheet[ROW, colTax].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTax) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colTax) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colTax].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, colTax, ROW, colTax].CellStyle.Font.Bold = true;



                worksheet[ROW, colGross].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colGross) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colGross) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colGross].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colSetOff].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colSetOff) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colSetOff) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colSetOff].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colBalance].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBalance) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colBalance) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colBalance].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[ROW, colBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;


                worksheet[ROW, colBooksGross].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBooksGross) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colBooksGross) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colBooksGross].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colBooksSetOff].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBooksSetOff) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colBooksSetOff) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colBooksSetOff].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colBooksBalance].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBooksBalance) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colBooksBalance) + (ROW - 1).ToString() + ")";
                worksheet[ROW, colBooksBalance].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[ROW, colBooksBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;
                worksheet.Range[ROW, colBooksBalance, ROW, colBooksBalance].CellStyle.Font.Bold = true;



                worksheet.Range[ROW, colGross - 1, ROW, COL].CellStyle.Font.Bold = true;

                // worksheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                //worksheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeader(ref worksheet, endCol, "Party Payment Status Detail", identity.CompanyId, identity.PlantName, "");
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }




        }
         
        #region Expense Register Report
        public IWorkbook GetExpenseRegisterReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string entityId)  //, bool checkbox
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            // DataTable dtIssueReportList = GetOperationReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
            //var dsLocal = GetDayBooksData(companyGroupId, companyId, plantId, fromDate, toDate);
            DataTable dtDayBookData = GetExpenseRegisterReportData(companyGroupId, companyId, plantId, fromDate, toDate);

            worksheet.Name = "Expense Register Report";

            //var header = GetDailyTransactionHeader(companyGroupId, companyId, plantId, toDate);

            reportFileName = "Expense Register" + toDate.ToString();


            if (dtDayBookData.Rows.Count == 0)
                throw new Exception("No data found");

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet.Range[ROW - 1, startCol].Text = "From " + Convert.ToDateTime(fromDate).ToString("dd-MMM-yyyy") + " To " + Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");


            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher Type";
            int colSourceType = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Entity";
            int colEntity = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Cost Centre";
            int colCostCentre = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Posting Date";
            int colPostingDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "DocRef No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Date";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GL Code";
            int colGLCode = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            //GLCode
            worksheet[ROW, COL].Text = "GL";
            int colUserName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget";
            int colBudget = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget RefNo";
            int colBudgetRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Activity Code";
            int colActivityId = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Activity";
            int colActivity = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Particular";
            int colParticular = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            //worksheet[ROW, COL].Text = "Dr/Cr";
            //int colDrCr = COL;
            //worksheet[ROW, COL].ColumnWidth = 5;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Amount";
            int colDrAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            //worksheet[ROW, COL].Text = "CrAmount";
            //int colCrAmount = COL;
            //worksheet[ROW, COL].ColumnWidth = 12;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            worksheet[ROW, COL].Text = "Narration";
            int colNarration = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;



            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            for (int i = 0; i < dtDayBookData.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);

                worksheet[ROW, colDrAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["Amount"].ToString());
                worksheet[ROW, colDrAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colSourceType].Text = dtDayBookData.Rows[i]["VoucherType"].ToString();
                worksheet[ROW, colEntity].Text = dtDayBookData.Rows[i]["EntityName"].ToString();

                worksheet[ROW, colCostCentre].Text = dtDayBookData.Rows[i]["CostCenterName"].ToString();

                worksheet[ROW, colVoucherNo].Text = dtDayBookData.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colPostingDate].Text = dtDayBookData.Rows[i]["PostingDate"].ToString();
                worksheet[ROW, colDocDate].Text = dtDayBookData.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colCurrency].Text = dtDayBookData.Rows[i]["CurrencyCode"].ToString();


                //worksheet[ROW, colCrAmount].Number = clsStaticInfo.dbl(dtDayBookData.Rows[i]["CrAmount"].ToString());
                //worksheet[ROW, colCrAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colDocRefNo].Text = dtDayBookData.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colGLCode].Text = dtDayBookData.Rows[i]["GLCode"].ToString();
                worksheet[ROW, colUserName].Text = dtDayBookData.Rows[i]["GL"].ToString();
                worksheet[ROW, colBudget].Text = dtDayBookData.Rows[i]["Budget"].ToString();
                worksheet[ROW, colActivity].Text = dtDayBookData.Rows[i]["Activity"].ToString();
                worksheet[ROW, colActivityId].Text = dtDayBookData.Rows[i]["ActivityCode"].ToString();

                worksheet[ROW, colBudgetRefNo].Text = dtDayBookData.Rows[i]["BudgetRefNo"].ToString();

                worksheet[ROW, colParticular].Text = dtDayBookData.Rows[i]["Particular"].ToString();
                //worksheet[ROW, colDrCr].Text = dtDayBookData.Rows[i]["Dr/Cr"].ToString();
                //worksheet[ROW, colOperationAttributeValue].Text = dtDayBookData.Rows[i]["CrAmount"].ToString();

                worksheet[ROW, colNarration].Text = dtDayBookData.Rows[i]["Narration"].ToString();


                //if (checkbox == true)
                //{

                //    worksheet[ROW, colTaskDetail].Text = dtIssueReportList.Rows[i]["TaskDetail"].ToString();

                //}


                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;



            ReportUtility reportUtility = new ReportUtility();

            //sheet1.Range[xlsRow, 3].Text = "GST Recievable Report From " + fromDate + " To " + toDate;

            reportUtility.PlantHeader(ref worksheet, endCol, " Expense Register Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }

        public DataTable GetExpenseRegisterReportData(string companyGroupId, string companyId, string plantId, string fromDate, string toDate)
        {
            var cmdText = @"SELECT CO.UserName CompanyName, PT.UserName PlantName,EN.UserName AS EntityName,CC.UserName CostCenterName, v.SourceType VoucherType, V.VoucherNo
                            ,Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') PostingDate, Replace(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate
                            ,V.DocRefNo,C.Code CurrencyCode
                            ,ACT.Id AS [Type]
                            ,GLGI.AccountCode GLCode, GL=GLGI.UserName, b.UserName Budget, BM.RefNo AS BudgetRefNo, A.UserName Activity,VD.ActivityId ActivityCode

                            ,Particular=CASE WHEN VD.PartyId<>'' THEN PP.UserName
                            				 WHEN VD.BankMasterId<>'' THEN BKM.AccountTitle
                            				 WHEN VD.CashMasterId<>'' THEN CM.UserName
                            				 WHEN VD.EmployeeId<>'' THEN ei.EmployeeName ELSE '' END
                            ,[Dr/Cr]=CASE WHEN VD.DrAmount<>0.00 THEN 'Dr'  WHEN VD.CrAmount<>0.00 THEN 'Cr' ELSE ''	END
                            ,[Amount]=CASE WHEN ISNULL(VD.DrAmount,0)<>0.00 THEN ISNULL(VD.DrAmount,0)  WHEN ISNULL(VD.CrAmount,0)<>0.00 THEN ISNULL(VD.CrAmount,0) ELSE 0	END
                            ,ISNULL(VD.DrAmount,0) DrAmount,ISNULL(VD.CrAmount,0) CrAmount
                            ,V.AddedBy, Replace(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') EntryDate
                            ,v.Narration,ir.NoteForAccounts,ei.EmployeeName
                            FROM TRN.VoucherDetail AS VD 
                            LEFT JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                             LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                             LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                             LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                             LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                             LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                             LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                             LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                             LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                             LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                             LEFT JOIN [ORG].[Company] AS CO ON CO.Id=V.CompanyId
                             LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=V.PlantId
                             LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
                            LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=VD.BankMasterId
                            LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                            left join trn.Invoice I on I.VoucherId=V.Id
                            left join trn.InventoryReceive ir on ir.Id=i.InventoryReceiveId
                            left join dbo.EmployeeInformation ei on ei.SystemId=VD.EmployeeId
							left join ORG.CostCenter CC ON CC.Id=VD.CostCenterId
                            WHERE V.IsPark=0 and V.CompanyGroupId='"+ companyGroupId + "' AND V.CompanyId ='"+ companyId + "' AND V.PlantId='"+ plantId + @"' and ACT.Id='Expense' --and VD.DrAmount>0
							AND convert(Date,V.PostingDate) BETWEEN  '" + fromDate + "' AND '"+ toDate + "'";


            return _sqlRepository.GetDataTable(cmdText);
        }

        #endregion

        #region Good work payment Undisburse & Disburse Report
        public string GoodWorkPaymentUndisburseReportxlx(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "GoodWorkPaymentUndisburseReport";
                sheet = workbook.Worksheets[0];
                int ROW = 5; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 11;
                int ColEmployeeCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 18;
                int ColEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "Minute";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColMinute = COL;
                COL++;

                sheet[ROW, COL].Text = "Hour";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColRemarks = COL;

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, ColEmployeeCode].Text = data[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEmployeeName].Text = data[i]["EmployeeName"].ToString();
                    sheet[ROW, ColMinute].Text = data[i]["Minute"].ToString();
                    sheet[ROW, ColMinute].Number = clsStaticInfo.dbl(data[i]["Minute"].ToString());
                    sheet[ROW, ColMinute].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColHour].Number = clsStaticInfo.dbl(data[i]["Hour"].ToString());
                    sheet[ROW, ColHour].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data[i]["Rate"].ToString());
                    sheet[ROW, ColRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data[i]["Amount"].ToString());
                    sheet[ROW, ColAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColRemarks].Text = data[i]["Remarks"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Good Work Payment Undisburse Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GoodWorkPaymentDisburseReportxlx(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "GoodWorkPaymentDisburseReport";
                sheet = workbook.Worksheets[0];
                int ROW = 5; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 11;
                int ColEmployeeCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 18;
                int ColEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "Minute";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColMinute = COL;
                COL++;

                sheet[ROW, COL].Text = "Hour";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColRemarks = COL;

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, ColEmployeeCode].Text = data[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEmployeeName].Text = data[i]["EmployeeName"].ToString();
                    sheet[ROW, ColMinute].Text = data[i]["Minute"].ToString();
                    sheet[ROW, ColMinute].Number = clsStaticInfo.dbl(data[i]["Minute"].ToString());
                    sheet[ROW, ColMinute].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColHour].Number = clsStaticInfo.dbl(data[i]["Hour"].ToString());
                    sheet[ROW, ColHour].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data[i]["Rate"].ToString());
                    sheet[ROW, ColRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data[i]["Amount"].ToString());
                    sheet[ROW, ColAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColRemarks].Text = data[i]["Remarks"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Good Work Payment Disburse Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Good work payment Undisburse & Disburse Report
    }

}
