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
    public class GroupBalanceReportService
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
        public GroupBalanceReportService(ISqlRepository sqlRepository
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
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'";
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
                        FROM TRN.VoucherDetail AS VD
                        LEFT JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                        LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                        LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].BudgetGroup BG ON BG.Id=BM.BudgetGroupId
                        LEFT JOIN [HKP].BudgetCategory BCT ON BCT.Id=BM.BudgetCategoryId
                        LEFT JOIN [HKP].BudgetSubCategory BSCT ON BSCT.Id=BM.BudgetSubCategoryId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                        LEFT JOIN [ORG].[Company] AS CO ON CO.Id=V.CompanyId
                        LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=V.PlantId
                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
                        LEFT JOIN [MST].BankMaster AS BKM ON BKM.Id=VD.BankMasterId
                        LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
					    left join ORG.CostCenter CCE ON CCE.Id =VD.CostCenterId

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
                        WHERE V.IsPark=0 and V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId ='" + companyId + "' AND V.PlantId='" + plantId + "' " + wcEmpStatus + @" ";
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


        public List<Dictionary<string, object>> GetGeneralOpeningBalanceLedgerData(string companyGroupId, string companyId, string plantId, string glId, string budgetMasterId, string activityId, string fromDate)
        {
            var budgetFilter = string.Empty;
            if (!string.IsNullOrEmpty(budgetMasterId))
                budgetFilter = " AND VD.BudgetMasterId='" + budgetMasterId + "' ";
            if (!string.IsNullOrEmpty(activityId))
                budgetFilter = " AND VD.ActivityId='" + activityId + "' ";
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
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.GLGeneralInfoId='" + glId + "' " + budgetFilter + " AND V.PostingDate < '" + fromDate.ToDbDate() + @"' AND V.SourceType!='OpeningBalance'
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
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.GLGeneralInfoId='" + glId + "' " + budgetFilter + " AND V.PostingDate <='" + fromDate.ToDbDate() + @"' AND V.SourceType='OpeningBalance'
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
                parameters.sort = "EmployeeCodeNumeric";
                parameters.CmdText = @"SELECT Emp.SystemId,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE EMP.CompanyId='" + companyId + @"' and EMP.EmployeeStatus='Active' 
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
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
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
                var ledgerData = GetGeneralLedgerData(companyGroupId, companyId, plantId, null, null, null, null, null, true, fiscalYearId);
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
                if (active == true)
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref.", 14); colDocRef = col; col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date.", 14); colDocDate = col; col++;
                }

                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 30); int colNarration = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Party", 15); int colParty = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 18); int colParticulars = col; col++;
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
                    reportUtility.SetHeaderText(ref sheet, row, col, "Activity Balance", 16, ExcelHAlign.HAlignRight); colActivityBalance = col; col++;
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
                var ledgerData = GetGeneralLedgerGroupByData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, toDate, false, null);
                var obVal = GetGeneralOpeningBalanceLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate);


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
                            sheet.Range[row, colParty, row, colCurrency].Merge();
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet[row, colActivityBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                            sheet.Range[row, colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            row++;
                            sheet.Range[row, 1, row, colLast].Merge();
                            row++;
                            reportUtility.SetText(ref sheet, row, colParty, "Opening Balance", true);
                            sheet.Range[row, colVoucherNo, row, colCurrency].Merge();
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
                    sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                row--;
                sheet.Range[row, colActivityBalance].Formula = "=" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1);
                //sheet[row, colActivityBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colActivityBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colLast - 4) + row + "-" + reportUtility.GetColumnNameForXls(colLast - 3) + row + ")";
                sheet.Range[row, colActivityBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                sheet.Range[row, colActivityBalance].CellStyle.Font.Bold = true;
                row++;
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



        public IWorkbook GetGeneralLedgerReportWithDocRef(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool active)
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
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); int colVoucherNo = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 14); int colPostingDate = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref.", 14); int colDocRef = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 14); int colDocDate = col; col++;

                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 30); int colNarration = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Party", 15); int colParty = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 18); int colParticulars = col; col++;
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
                var obVal = GetGeneralOpeningBalanceLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate);
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
                var ledgerData = GetGeneralLedgerData(companyGroupId, companyId, plantId, glId, budgetMasterId, activityId, fromDate, toDate, false, null);
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
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId=BGM.Id  AND V.PostingDate < '" + toDate + @"' AND V.SourceType!='OpeningBalance'
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
							GROUP BY GLGI.AccountCode,GLGI.UserName,  BG.UserName,BGM.Id";
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
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + identity.CompanyGroupId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'   AND VD.ActivityId=A.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
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
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.GLGeneralInfoId='" + glId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.ActivityId=A.Id  AND V.PostingDate < '" + toDate + @"' AND V.SourceType!='OpeningBalance'
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
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.BudgetMasterId='" + budgetMasterId + @"'  AND VD.GLGeneralInfoId='" + glId + @"' AND V.SourceType!='OpeningBalance' AND   V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' 
							GROUP BY GLGI.AccountCode,GLGI.UserName,  BG.UserName,A.Id, A.UserName";
        }

        public IWorkbook GetGeneralLedgerReport2(string companyGroupId, string companyId, string plantId, string plantName, string glId, string budgetMasterId, string fromDate, string toDate)
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
                DataTable dtGroupBalanceBudget = _sqlRepository.GetDataTable(Budgetsql);
                if (dtGroupBalance.Rows.Count == 0)
                    throw new Exception("No data found");

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

                if (budgetMasterId != "null")
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
                if (budgetMasterId != "null")
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
                if (budgetMasterId!="null")
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
                        sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                        sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                        sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;

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

                        sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                        sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                        sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                        ROW++;

                    }
                }

            

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
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Group Balance Report.xls";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }

}
