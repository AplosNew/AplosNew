using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Advances;
using Library.Model.Banks;
using Library.Model.ChartOfAccounts;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.FixedAssets;
using Library.Model.Inventory;
using Library.Model.Invoices;
using Library.Model.Materials;
using Library.Model.OpeningBalances;
using Library.Model.Parties;
using Library.Model.SecurityDeposits;
using Library.Model.Vouchers;
using Library.Service.Advances;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Finances;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.SecurityDeposits;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.OpeningBalances
{
    public class OpeningBalanceService : Service<OpeningBalance>, IOpeningBalanceService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<OpeningBalance> _openingBalanceRepository;
        private readonly IRepositoryAsync<OpeningBalanceDetail> _openingBalanceDetailRepository;
        private readonly IRepositoryAsync<OpeningBalanceDetailCurrency> _openingBalanceDetailCurrencyRepository;
        private readonly IRepositoryAsync<MaterialMasterOpeningBalanceDetail> _materialMasterOpeningBalanceDetailRepository;
        private readonly IRepositoryAsync<MaterialMasterOpeningBalanceDetailCurrency> _materialMasterOpeningBalanceDetailCurrencyRepository;
        private readonly IRepositoryAsync<MaterialMasterOpeningBalanceDetailDirectIndirect> _materialMasterOpeningBalanceDetailDirectIndirectRepository;
        private readonly IRepositoryAsync<InventoryReceive> _inventoryReceiveRepository;
        private readonly IRepositoryAsync<InventoryMaterial> _inventoryMaterialRepository;
        private readonly IRepositoryAsync<InventoryReceiveDetail> _inventoryReceiveDetailRepository;
        private readonly IRepositoryAsync<FinancingTypeGL> _financingTypeGLRepository;
        private readonly IRepositoryAsync<Financing> _financingRepository;
        private readonly IRepositoryAsync<SecurityDeposit> _securityDepositRepository;
        private readonly IRepositoryAsync<SecurityDepositDetail> _securityDepositDetailRepository;
        private readonly IRepositoryAsync<FinancingDetail> _financingDetailRepository;
        private readonly IRepositoryAsync<GLGeneralInfo> _glRepository;
        private readonly IRepositoryAsync<BankMaster> _bankMasterRepository;
        private readonly IRepositoryAsync<CashMaster> _cashMasterRepository;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly IRepositoryAsync<CompanyPartyGL> _companyPartyGLRepository;
        private readonly IRepositoryAsync<PartyPlant> _partyPlantRepository;
        private readonly ICurrencyTransactionService _currencyTransactionService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly ICurrencyService _currencyService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly IFiscalYearService _fiscalYearService;
        private readonly IVoucherService _voucherService;
        private readonly IInvoiceService _invoiceService;
        private readonly IAdvanceService _advanceService;
        private readonly ISecurityDepositService _securityDepositService;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IInvestmentService _investmentService;
        private readonly IEmployeeTransactionTypeGLService _employeeTransactionTypeGLService;
        private readonly IFinancingService _financingService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;


        public OpeningBalanceService(
              IRepositoryAsync<OpeningBalance> openingBalanceRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<OpeningBalanceDetail> openingBalanceDetailRepository
            , IRepositoryAsync<OpeningBalanceDetailCurrency> openingBalanceDetailCurrencyRepository
            , IRepositoryAsync<MaterialMasterOpeningBalanceDetail> materialMasterOpeningBalanceDetailRepository
            , IRepositoryAsync<MaterialMasterOpeningBalanceDetailCurrency> materialMasterOpeningBalanceDetailCurrencyRepository
            , IRepositoryAsync<MaterialMasterOpeningBalanceDetailDirectIndirect> materialMasterOpeningBalanceDetailDirectIndirectRepository
            , IRepositoryAsync<InventoryReceive> inventoryReceiveRepository
            , IRepositoryAsync<InventoryMaterial> inventoryMaterialRepository
            , IRepositoryAsync<InventoryReceiveDetail> inventoryReceiveDetailRepository
            , IRepositoryAsync<BankMaster> bankMasterRepository
            , IRepositoryAsync<FinancingTypeGL> financingTypeGLRepository
            , IRepositoryAsync<Financing> financingRepository
            , IRepositoryAsync<SecurityDeposit> securityDepositRepository
            , IRepositoryAsync<SecurityDepositDetail> securityDepositDetailRepository
            , IRepositoryAsync<FinancingDetail> financingDetailRepository
            , IRepositoryAsync<GLGeneralInfo> glRepository
            , IRepositoryAsync<CashMaster> cashMasterRepository
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , IRepositoryAsync<CompanyPartyGL> companyPartyGLRepository
            , IRepositoryAsync<PartyPlant> partyPlantRepository
            , ICurrencyTransactionService currencyTransactionService
            , ICurrencyService currencyService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , ICompanyTaxYearService companyTaxYearService
            , IFiscalYearService fiscalYearService
            , IVoucherService voucherService
            , IInvoiceService invoiceService
            , IAdvanceService advanceService
            , ISecurityDepositService securityDepositService
            , IEmployeePayableService employeePayableService
            , IInvestmentService investmentService
            , IEmployeeTransactionTypeGLService employeeTransactionTypeGLService
            , IFinancingService financingService
            , ICompanyFiscalYearService companyFiscalYearService
            ) : base(openingBalanceRepository, unitOfWork, pkGeneratorService)
        {
            _investmentService = investmentService;
            _employeePayableService = employeePayableService;
            _securityDepositService = securityDepositService;
            _securityDepositRepository = securityDepositRepository;
            _securityDepositDetailRepository = securityDepositDetailRepository;
            _advanceService = advanceService;
            _invoiceService = invoiceService;
            _financingTypeGLRepository = financingTypeGLRepository;
            _openingBalanceRepository = openingBalanceRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
            _openingBalanceDetailRepository = openingBalanceDetailRepository;
            _openingBalanceDetailCurrencyRepository = openingBalanceDetailCurrencyRepository;
            _materialMasterOpeningBalanceDetailRepository = materialMasterOpeningBalanceDetailRepository;
            _materialMasterOpeningBalanceDetailCurrencyRepository = materialMasterOpeningBalanceDetailCurrencyRepository;
            _materialMasterOpeningBalanceDetailDirectIndirectRepository = materialMasterOpeningBalanceDetailDirectIndirectRepository;
            _inventoryReceiveRepository = inventoryReceiveRepository;
            _inventoryMaterialRepository = inventoryMaterialRepository;
            _inventoryReceiveDetailRepository = inventoryReceiveDetailRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _bankMasterRepository = bankMasterRepository;
            _glRepository = glRepository;
            _cashMasterRepository = cashMasterRepository;
            _financingRepository = financingRepository;
            _financingDetailRepository = financingDetailRepository;
            _companyPartyRepository = companyPartyRepository;
            _companyPartyGLRepository = companyPartyGLRepository;
            _partyPlantRepository = partyPlantRepository;
            _currencyTransactionService = currencyTransactionService;
            _currencyService = currencyService;
            _companyTaxYearService = companyTaxYearService;
            _fiscalYearService = fiscalYearService;
            _employeeTransactionTypeGLService = employeeTransactionTypeGLService;
            _financingService = financingService;
            _companyFiscalYearService = companyFiscalYearService;
        }

        #endregion Constructor

        #region Journal

        public List<Dictionary<string, object>> GetSummaryData(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                        DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        SELECT OB.SourceType, ACT.BalanceType
                        , OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName, OBD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, OBD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , SUM(CC.CCDr) AS CompanyCurrencyAmountDr
                        , SUM(CC.CCCr) AS CompanyCurrencyAmountCr
                        , SUM(GC.GCDr) AS CompanyGroupCurrencyAmountDr
                        , SUM(GC.GCCr) AS CompanyGroupCurrencyAmountCr
                        , SUM(HC.HCDr) AS HardCurrencyAmountDr
                        , SUM(HC.HCCr) AS HardCurrencyAmountCr
                        FROM [TRN].[OpeningBalanceDetail] AS OBD
                        JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                        JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                        JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=OBD.OpeningBalanceId
                        LEFT JOIN (
	                        SELECT CCDr=CASE WHEN OBDC.Amount > 0 AND ACT.BalanceType='Debit' THEN OBDC.Amount ELSE 0 END
	                        ,CCCr=CASE WHEN OBDC.Amount < 0 AND ACT.BalanceType='Debit' THEN ABS(OBDC.Amount) ELSE CASE WHEN OBDC.Amount > 0 AND ACT.BalanceType='Credit' THEN OBDC.Amount ELSE 0 END END
	                        , ABS(OBDC.Amount) AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        LEFT JOIN [TRN].[OpeningBalanceDetail] AS OBD ON OBD.Id=OBDC.OpeningBalanceDetailId
	                        JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
	                        JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
	                        JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        )AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                        LEFT JOIN (
	                        SELECT GCDr=CASE WHEN OBDC.Amount > 0 THEN OBDC.Amount ELSE 0 END
	                        ,GCCr=CASE WHEN OBDC.Amount < 0 THEN ABS(OBDC.Amount) ELSE 0 END
	                        , ABS(OBDC.Amount) AS CompanyGroupCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.OpeningBalanceDetailId=OBD.Id
                        LEFT JOIN (
	                        SELECT HCDr=CASE WHEN OBDC.Amount > 0 THEN OBDC.Amount ELSE 0 END
	                        ,HCCr=CASE WHEN OBDC.Amount < 0 THEN ABS(OBDC.Amount) ELSE 0 END
	                        , ABS(OBDC.Amount) AS HardCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.OpeningBalanceDetailId=OBD.Id
                        WHERE OB.IsPark=1 AND OB.IsPosted=1 AND OB.CompanyGroupId=@companyGroupId AND OB.CompanyId=@companyId AND OB.PlantId=@plantId
                        GROUP BY OB.SourceType, ACT.BalanceType, OBD.GLGeneralInfoId, GGI.AccountCode, GGI.UserName, OBD.BudgetMasterId, B.Code, B.UserName, OBD.ActivityId, A.Code, A.UserName
                        UNION ALL
                        SELECT OB.SourceType, ACT.BalanceType
                        , FOBD.AssetGLId AS GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName, FOBD.AssetBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, FOBD.AssetActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountDr, 0 CompanyCurrencyAmountCr
                        , SUM(GC.CompanyGroupCurrencyAmount) AS CompanyGroupCurrencyAmountDr, 0 CompanyGroupCurrencyAmountCr
                        , SUM(HC.HardCurrencyAmount) AS HardCurrencyAmountDr, 0 HardCurrencyAmountCr
                        FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                        JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.AssetGLId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=FOBD.AssetBudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=FOBD.AssetActivityId
                        JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                        JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=FOBD.OpeningBalanceId
                        LEFT JOIN (
	                        SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='FA'
                        ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        LEFT JOIN (
	                        SELECT OBDC.Amount AS CompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='FA'
                        ) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        LEFT JOIN (
	                        SELECT OBDC.Amount AS HardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='FA'
                        ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        WHERE OB.IsPark=1 AND OB.IsPosted=1 AND OB.CompanyGroupId=@companyGroupId AND OB.CompanyId=@companyId AND OB.PlantId=@plantId
                        GROUP BY OB.SourceType, ACT.BalanceType, FOBD.AssetGLId, GGI.AccountCode, GGI.UserName, FOBD.AssetBudgetMasterId, B.Code, B.UserName, FOBD.AssetActivityId, A.Code, A.UserName
                        UNION ALL
                        SELECT OB.SourceType, ACT.BalanceType
                        , FOBD.AccumulatedDepreciationGLId AS GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName, FOBD.AccumulatedDepreciationBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, FOBD.AccumulatedDepreciationActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , 0 CompanyCurrencyAmountDr, SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountCr
                        , 0 CompanyGroupCurrencyAmountDr, SUM(GC.CompanyGroupCurrencyAmount) AS  CompanyGroupCurrencyAmountCr
                        , 0 HardCurrencyAmountDr, SUM(HC.HardCurrencyAmount) AS HardCurrencyAmountCr
                        FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                        JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.AccumulatedDepreciationGLId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=FOBD.AccumulatedDepreciationBudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=FOBD.AccumulatedDepreciationActivityId
                        JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                        JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=FOBD.OpeningBalanceId
                        LEFT JOIN (
	                        SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='AD'
                        ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        LEFT JOIN (
	                        SELECT OBDC.Amount AS CompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='AD'
                        ) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        LEFT JOIN (
	                        SELECT OBDC.Amount AS HardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='AD'
                        ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        WHERE OB.IsPark=1 AND OB.IsPosted=1 AND OB.CompanyGroupId=@companyGroupId AND OB.CompanyId=@companyId AND OB.PlantId=@plantId
                        GROUP BY OB.SourceType, ACT.BalanceType, FOBD.AccumulatedDepreciationGLId, GGI.AccountCode, GGI.UserName, FOBD.AccumulatedDepreciationBudgetMasterId, B.Code, B.UserName, FOBD.AccumulatedDepreciationActivityId, A.Code, A.UserName
                        ORDER BY 1, 5, 8, 11, 2;";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetAvailableForJournal(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                        DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        SELECT OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, OBD.OpeningBalanceId, OBD.Id AS OpeningBalanceDetailId, OB.EmployeeTransactionTypeId, OB.FinancingTypeId, OB.MaterialStorageId, OB.SourceType, OB.PartyType
                        , OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName, OBD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, OBD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , OBD.PartyType, OBD.CompanyId AS InterCompanyId, OBD.PlantId AS InterPlantId, OBD.EntityId AS InterEntityId, OBD.PartyId, OBD.BankMasterId, OBD.CashMasterId, OBD.EmployeeId, OBD.BaseNoOfDays, OBD.BaseOnDueDate
						, OBD.PartyPlantId, OBD.LifeOfYear, OBD.NoOfInstallmentPerYear, OBD.NoOfPaidInstallment, OBD.ProfitRate, OBD.RefId, OBD.RepaymentStartDate, OBD.SanctionAmount, OBD.TotalNoOfInstallment
                        , REPLACE(CONVERT(CHAR(11), OBD.DocDate, 106),' ','-') AS DocDate, OBD.DocRefNo, OBD.Narration
                        , [TrnType]=CASE WHEN OBD.Amount < 0 AND ACT.BalanceType='Debit' THEN 'Credit' ELSE ACT.BalanceType END, ABS(OBD.Amount) AS Amount, CAST(1 AS bit) AS IsOB
                        , OBD.CurrencyId, C.Code AS CurrencyCode, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, CC.CompanyCurrencyAmount
                        , GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, GC.CompanyGroupCurrencyAmount
                        , HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion, HC.HardCurrencyAmount
                        FROM [TRN].[OpeningBalanceDetail] AS OBD
                        JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=OBD.OpeningBalanceId
                        JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                        JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                        JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        JOIN [SCS].[Currency] AS C ON C.Id=OBD.CurrencyId
                        LEFT JOIN (
	                        SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.ToCurrencyConversion AS CompanyCurrencyConversion, ABS(OBDC.Amount) AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                        LEFT JOIN (
                        SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS CompanyGroupCurrencyRate, OBDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, ABS(OBDC.Amount) AS CompanyGroupCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.OpeningBalanceDetailId=OBD.Id
                        LEFT JOIN (
	                        SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS HardCurrencyRate, OBDC.ToCurrencyConversion AS HardCurrencyConversion, ABS(OBDC.Amount) AS HardCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.OpeningBalanceDetailId=OBD.Id
                        WHERE OB.IsPark=1 AND OB.IsPosted=1 AND OB.CompanyGroupId=@companyGroupId AND OB.CompanyId=@companyId AND OB.PlantId=@plantId
                        UNION ALL
                        SELECT OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, FOBD.OpeningBalanceId, FOBD.Id AS OpeningBalanceDetailId, OB.EmployeeTransactionTypeId, NULL AS FinancingTypeId, OB.MaterialStorageId, OB.SourceType, OB.PartyType
                        , FOBD.AssetGLId AS GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName,  FOBD.AssetBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, FOBD.AssetActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , NULL AS PartyType, NULL AS InterCompanyId, FOBD.PlantId AS InterPlantId, FOBD.EntityId AS InterEntityId, NULL AS PartyId, NULL AS BankMasterId, NULL AS CashMasterId, NULL AS EmployeeId, NULL BaseNoOfDays, NULL BaseOnDueDate
						, NULL PartyPlantId, NULL LifeOfYear, NULL NoOfInstallmentPerYear, NULL NoOfPaidInstallment, NULL ProfitRate, NULL RefId, NULL RepaymentStartDate, NULL SanctionAmount, NULL TotalNoOfInstallment
                        , REPLACE(CONVERT(CHAR(11), OB.DocDate, 106),' ','-') AS DocDate, OB.DocRefNo, OB.Narration
                        , ACT.BalanceType AS [TrnType], FOBD.Amount AS Amount, CAST(1 AS bit) AS IsOB
                        , FOBD.CurrencyId, C.Code AS CurrencyCode, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, CC.CompanyCurrencyAmount
                        , GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, GC.CompanyGroupCurrencyAmount
                        , HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion, HC.HardCurrencyAmount
                        FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                        JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=FOBD.OpeningBalanceId
                        JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.AssetGLId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=FOBD.AssetBudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=FOBD.AssetActivityId
                        JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                        JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
                        LEFT JOIN (
	                        SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.ToCurrencyConversion AS CompanyCurrencyConversion, ABS(OBDC.Amount) AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='FA'
                        ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        LEFT JOIN (
                        SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS CompanyGroupCurrencyRate, OBDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, ABS(OBDC.Amount) AS CompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='FA'
                        ) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        LEFT JOIN (
	                        SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS HardCurrencyRate, OBDC.ToCurrencyConversion AS HardCurrencyConversion, ABS(OBDC.Amount) AS HardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='FA'
                        ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        WHERE OB.IsPark=1 AND OB.IsPosted=1 AND OB.CompanyGroupId=@companyGroupId AND OB.CompanyId=@companyId AND OB.PlantId=@plantId
                        UNION ALL
                        SELECT OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, FOBD.OpeningBalanceId, FOBD.Id AS OpeningBalanceDetailId, OB.EmployeeTransactionTypeId, NULL AS FinancingTypeId, OB.MaterialStorageId, OB.SourceType, OB.PartyType
                        , FOBD.AccumulatedDepreciationGLId AS GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName, FOBD.AccumulatedDepreciationBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, FOBD.AccumulatedDepreciationActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , NULL AS PartyType, NULL AS InterCompanyId, FOBD.PlantId AS InterPlantId, FOBD.EntityId AS InterEntityId, NULL AS PartyId, NULL AS BankMasterId, NULL AS CashMasterId, NULL AS EmployeeId, NULL BaseNoOfDays, NULL BaseOnDueDate
						, NULL PartyPlantId, NULL LifeOfYear, NULL NoOfInstallmentPerYear, NULL NoOfPaidInstallment, NULL ProfitRate, NULL RefId, NULL RepaymentStartDate, NULL SanctionAmount, NULL TotalNoOfInstallment
                        , REPLACE(CONVERT(CHAR(11), OB.DocDate, 106),' ','-') AS DocDate, OB.DocRefNo, OB.Narration
                        , ACT.BalanceType AS [TrnType], CC.CompanyCurrencyAmount AS Amount, CAST(1 AS bit) AS IsOB
                        , FOBD.CurrencyId, C.Code AS CurrencyCode, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, CC.CompanyCurrencyAmount
                        , GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, GC.CompanyGroupCurrencyAmount
                        , HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion, HC.HardCurrencyAmount
                        FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                        JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=FOBD.OpeningBalanceId
                        JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.AccumulatedDepreciationGLId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=FOBD.AccumulatedDepreciationBudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=FOBD.AccumulatedDepreciationActivityId
                        JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                        JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
                        LEFT JOIN (
	                        SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.ToCurrencyConversion AS CompanyCurrencyConversion, ABS(OBDC.Amount) AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='AD'
                        ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        LEFT JOIN (
                        SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS CompanyGroupCurrencyRate, OBDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, ABS(OBDC.Amount) AS CompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='AD'
                        ) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        LEFT JOIN (
	                        SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS HardCurrencyRate, OBDC.ToCurrencyConversion AS HardCurrencyConversion, ABS(OBDC.Amount) AS HardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.GLType='AD'
                        ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        WHERE CC.CompanyCurrencyAmount > 0 AND OB.IsPark=1 AND OB.IsPosted=1 AND OB.CompanyGroupId=@companyGroupId AND OB.CompanyId=@companyId AND OB.PlantId=@plantId
                        ORDER BY SourceType, GLGeneralInfoName, BudgetName, ActivityName;";
            return _sqlRepository.GetDataCollection(sql);
        }

        public void InsertJournal(Voucher voucher, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            var fiscalYear = _fiscalYearService.Find(voucher.FiscalYearId);
            try
            {
                var voucherVM = new VoucherViewModel
                {
                    CompanyGroupId = voucher.CompanyGroupId,
                    CompanyId = voucher.CompanyId,
                    PlantId = voucher.PlantId,
                    EntityId = voucher.EntityId,
                    PostingDate = voucher.PostingDate
                };

                _companyParallelCurrencyService.GetParallelCurrency(voucher.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Voucher TABLE
                voucher.IsPark = false;
                voucher.VoucherDate = DateTime.Now;
                voucher.TaxYearId = voucherVM.TaxYearId;
                voucher.TaxYearPeriodId = voucherVM.TaxYearPeriodId;
                voucher.SourceType = SourceType.OpeningBalance.ToString();
                _voucherService.InsertVoucher(voucher, fiscalYear.YearPrefix);

                var advancePk = _advanceService.GetMaxNumber(nameof(Advance), PKGeneratorEnum.Yearly, null, voucher.VoucherDate);
                var invoicePk = _invoiceService.GetMaxNumber(nameof(Invoice), PKGeneratorEnum.Yearly, null, voucher.VoucherDate);
                var securityDepositPk = _securityDepositService.GetMaxNumber(nameof(SecurityDeposit), PKGeneratorEnum.Yearly, null, voucher.VoucherDate);
                var employeePayablePk = _employeePayableService.GetMaxNumber();
                var financingPk = _financingService.GetMaxNumber();

                var currentRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    // Set to currency
                    voucherDetailVM.ToCurrencyId = companyCurrencyId;

                    // INSERT INTO VOUCHER DETAIL
                    var voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.Id,
                        PlantId = voucherDetailVM.PlantId,
                        EntityId = voucherDetailVM.IsOB ? voucherDetailVM.EntityId : voucher.EntityId,
                        FiscalYearId = voucher.FiscalYearId,
                        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                        PartyType = voucherDetailVM.PartyType,
                        EmployeeId = voucherDetailVM.EmployeeId,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        DrAmount = voucherDetailVM.TrnType == "Debit" ? voucherDetailVM.Amount : 0,
                        CrAmount = voucherDetailVM.TrnType == "Credit" ? voucherDetailVM.Amount : 0,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    currentRecord++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentRecord);

                    if (!string.IsNullOrEmpty(voucherDetailVM.OpeningBalanceId))
                    {
                        // INSERT INTO Financing
                        if (voucherDetailVM.SourceType == SourceType.Loan.ToString() ||
                            voucherDetailVM.SourceType == SourceType.Investment.ToString())
                        {
                            financingPk.MaxNumber++;
                            var investment = new Financing
                            {
                                Id = voucher.VoucherDate.Year + financingPk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                VoucherDate = voucher.AddedDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                Amount = voucherDetailVM.Amount,
                                SourceType = voucherDetailVM.SourceType,
                                PaymentSource = voucherDetailVM.PaymentSource,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                BankMasterId = voucherDetailVM.BankMasterId,
                                LifeOfYear = voucherDetailVM.LifeOfYear,
                                NoOfInstallmentPerYear = voucherDetailVM.NoOfInstallmentPerYear,
                                ProfitRate = voucherDetailVM.ProfitRate,
                                RepaymentStartDate = voucherDetailVM.RepaymentStartDate,
                                InterCompanyId = voucherDetailVM.InterCompanyId,
                                InterPlantId = voucherDetailVM.InterPlantId,
                                TotalNoOfInstallment = voucherDetailVM.TotalNoOfInstallment,
                                IsPark = true
                            };
                            _financingService.InsertFinancing(investment);

                            // INSERT INTO FinancingDetail
                            var financingDetail = new FinancingDetail
                            {
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                BankMasterId = voucherDetailVM.BankMasterId,
                                CashMasterId = voucherDetailVM.CashMasterId,
                                Amount = investment.Amount
                            };
                            _financingService.InsertFinancingDetail(investment, financingDetail);
                            // Set FinancingDetail detail to voucher detail.
                            voucherDetail.FinancingDetailId = financingDetail.Id;
                        }
                        else if (voucherDetailVM.SourceType == SourceType.SecurityDeposit.ToString())
                        {
                            securityDepositPk.MaxNumber++;
                            var securityDeposit = new SecurityDeposit
                            {
                                Id = voucher.VoucherDate.Year + securityDepositPk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                Amount = voucherDetailVM.Amount,
                                SourceType = voucherDetailVM.SourceType,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _securityDepositService.InsertGraph(securityDeposit);

                            // INSERT INTO SecurityDepositDetail
                            var securityDepositDetail = new SecurityDepositDetail
                            {
                                Id = MakePK(securityDeposit.Id, 1, 2),
                                SecurityDepositId = securityDeposit.Id,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = securityDeposit.Amount,
                                AddedBy = securityDeposit.AddedBy,
                                AddedDate = securityDeposit.AddedDate,
                                AddedFromIP = securityDeposit.AddedFromIP
                            };
                            _securityDepositService.InsertSecurityDepositDetail(securityDepositDetail);
                            // Set SecurityDepositDetail detail to voucher detail.
                            voucherDetail.SecurityDepositDetailId = securityDepositDetail.Id;
                        }
                        else if (voucherDetailVM.SourceType == SourceType.CustomerAdvance.ToString() || voucherDetailVM.SourceType == SourceType.VendorAdvance.ToString() || voucherDetailVM.SourceType == SourceType.EmployeeAdvance.ToString())
                        {
                            advancePk.MaxNumber++;
                            var advance = new Advance
                            {
                                Id = voucher.VoucherDate.Year + advancePk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                EmployeeTransactionTypeId = voucherDetailVM.EmployeeTransactionTypeId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                Amount = voucherDetailVM.Amount,
                                SourceType = voucherDetailVM.SourceType,
                                PaymentSource = voucherDetailVM.PaymentSource,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                IsPosted = true,
                                AdvanceNo = voucher.VoucherNo
                            };
                            _advanceService.InsertGraph(advance);

                            // INSERT INTO AdvanceDetail
                            var advanceDetail = new AdvanceDetail
                            {
                                Id = _advanceService.MakeAdvanceDetailPK(advance.Id, 1),
                                AdvanceId = advance.Id,
                                CompanyId = advance.CompanyId,
                                PlantId = advance.PlantId,
                                EmployeeId = advance.EmployeeId,
                                Archive = advance.Archive,
                                IsWrittenOff = advance.IsWrittenOff,
                                ModelState = advance.ModelState,
                                Narration = advance.Narration,
                                NetAmount = advance.Amount,
                                PartyId = advance.PartyId,
                                PartyPlantId = advance.PartyPlantId,
                                PartyType = advance.PartyType,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = advance.Amount,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            };
                            _advanceService.InsertAdvanceDetail(advanceDetail);
                            // Set Advance detail to voucher detail.
                            voucherDetail.AdvanceDetailId = advanceDetail.Id;
                        }
                        else if (voucherDetailVM.SourceType == SourceType.CustomerInvoice.ToString() || voucherDetailVM.SourceType == SourceType.VendorInvoice.ToString())
                        {
                            var invoice = new Invoice
                            {
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                BaseNoOfDays = voucherDetailVM.BaseNoOfDays,
                                BaseOnDueDate = voucherDetailVM.BaseOnDueDate,
                                Amount = voucherDetailVM.Amount,
                                SourceType = voucherDetailVM.SourceType,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            invoicePk.MaxNumber++;
                            _invoiceService.InsertInvoice(invoice, invoicePk.MaxNumber);

                            var invoiceDetail = new InvoiceDetail
                            {
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = voucherDetailVM.Amount,
                                NetAmount = invoice.Amount
                            };
                            _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, 1);
                            // Set InvoiceDetail Id to voucher detail.
                            voucherDetail.InvoiceDetailId = invoiceDetail.Id;
                        }
                        else if (voucherDetailVM.SourceType == SourceType.BankJournal.ToString() || voucherDetailVM.SourceType == SourceType.CashJournal.ToString())
                        {
                            var glTransactionDetail = new GLTransactionDetail
                            {
                                VoucherDetailId = voucherDetail.Id,
                                BankMasterId = voucherDetail.BankMasterId,
                                CashMasterId = voucherDetail.CashMasterId,
                                SourceType = voucherDetailVM.SourceType,
                                DrAmount = voucherDetail.DrAmount,
                                CrAmount = voucherDetail.CrAmount,
                            };
                            // Set BankMasterId/CashMasterId in voucher detail.
                            voucherDetail.BankMasterId = voucherDetailVM.BankMasterId;
                            voucherDetail.CashMasterId = voucherDetailVM.CashMasterId;
                            _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                        }
                        else if (voucherDetailVM.SourceType == SourceType.EmployeePayable.ToString())
                        {
                            employeePayablePk.MaxNumber++;
                            var employeePayable = new EmployeePayable
                            {
                                Id = voucher.VoucherDate.Year + employeePayablePk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                EmployeeTransactionTypeId = voucherDetailVM.EmployeeTransactionTypeId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                Amount = voucherDetailVM.Amount,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                SourceType = voucherDetailVM.SourceType,
                                PartyType = PartyType.Employee.ToString(),
                                VoucherDate = voucher.VoucherDate
                            };
                            _employeePayableService.InsertEmployeePayable(employeePayable);

                            var employeePayableDetail = new EmployeePayableDetail
                            {
                                EmployeePayableId = employeePayable.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount,
                                NetAmount = employeePayable.Amount
                            };
                            _employeePayableService.InsertEmployeePayableDetail(employeePayable, employeePayableDetail, 1);

                            // Set InvoiceDetail Id to voucher detail.
                            voucherDetail.EmployeePayableDetailId = employeePayableDetail.Id;
                            voucherDetail.PartyType = employeePayable.PartyType;
                        }
                        else if (voucherDetailVM.SourceType == SourceType.InterCompanyTransactionGiven.ToString() ||
                            voucherDetailVM.SourceType == SourceType.InterCompanyTransactionTaken.ToString() ||
                            voucherDetailVM.SourceType == SourceType.InterCompanyTransactionTaken.ToString()
                            )
                        {
                            advancePk.MaxNumber++;
                            var interTransaction = new Advance
                            {
                                Id = voucher.VoucherDate.Year + advancePk.MaxNumber.ToString(),
                                AddedBy = voucherDetail.AddedBy,
                                AddedDate = voucherDetail.AddedDate,
                                AddedFromIP = voucherDetail.AddedFromIP,
                                Amount = voucherDetailVM.Amount,
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                EntityId = voucherDetailVM.EntityId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                VoucherId = voucher.Id,
                                VoucherDate = voucher.VoucherDate,
                                IsInterTransaction = true,
                                Narration = voucherDetailVM.Narration,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                SourceType = voucherDetailVM.SourceType,
                                PostingDate = voucher.PostingDate,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                VoucherTypeId = voucher.VoucherTypeId,
                                IsPosted = true
                            };
                            // _interTransactionService.InsertGraph(interTransaction);

                            // INSERT INTO AdvanceDetail
                            var interTransactionDetail = new AdvanceDetail
                            {
                                AdvanceId = interTransaction.Id,
                                CompanyId = interTransaction.CompanyId,
                                PlantId = interTransaction.PlantId,
                                Archive = interTransaction.Archive,
                                IsWrittenOff = interTransaction.IsWrittenOff,
                                ModelState = interTransaction.ModelState,
                                Narration = interTransaction.Narration,
                                NetAmount = interTransaction.Amount,
                                PartyId = interTransaction.PartyId,
                                PartyPlantId = interTransaction.PartyPlantId,
                                PartyType = interTransaction.PartyType,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = interTransaction.Amount,
                                AddedBy = interTransaction.AddedBy,
                                AddedDate = interTransaction.AddedDate,
                                AddedFromIP = interTransaction.AddedFromIP
                            };
                            //_interTransactionService.InsertInterTransactionDetail(interTransaction, interTransactionDetail, 1);
                            // Set InterTransaction detail to voucher detail.
                            voucherDetail.InterTransactionDetailId = interTransactionDetail.Id;
                        }
                    }

                    // Making currency exchange rate and conversion.
                    if (!voucherDetailVM.IsOB)
                    {
                        if (voucherDetailVM.CurrencyId == companyCurrencyId)
                        {
                            voucherDetailVM.CompanyCurrencyRate = 1;
                            voucherDetailVM.CompanyCurrencyConversion = 1;

                            if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                            {
                                voucherDetailVM.CompanyGroupCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.CompanyGroupCurrencyAmount;
                                voucherDetailVM.CompanyGroupCurrencyConversion = voucherDetailVM.CompanyCurrencyConversion / voucherDetailVM.CompanyGroupCurrencyRate;
                            }
                            if (!string.IsNullOrEmpty(hardCurrencyId))
                            {
                                voucherDetailVM.HardCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.HardCurrencyAmount;
                                voucherDetailVM.HardCurrencyConversion = voucherDetailVM.CompanyCurrencyConversion / voucherDetailVM.HardCurrencyRate;
                            }
                        }
                        else if (!string.IsNullOrEmpty(companyGroupCurrencyId) && voucherDetailVM.CurrencyId == companyGroupCurrencyId)
                        {
                            voucherDetailVM.CompanyGroupCurrencyRate = 1;
                            voucherDetailVM.CompanyGroupCurrencyConversion = 1;
                            voucherDetailVM.CompanyFromCurrencyId = voucherDetailVM.CurrencyId;

                            voucherDetailVM.CompanyCurrencyRate = 1 / (voucherDetailVM.CompanyGroupCurrencyAmount / voucherDetailVM.CompanyCurrencyAmount);
                            voucherDetailVM.CompanyCurrencyConversion = voucherDetailVM.CompanyGroupCurrencyConversion / voucherDetailVM.CompanyCurrencyRate;
                            if (!string.IsNullOrEmpty(hardCurrencyId))
                            {
                                voucherDetailVM.HardCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.HardCurrencyAmount;
                                voucherDetailVM.HardCurrencyConversion = voucherDetailVM.CompanyCurrencyConversion / voucherDetailVM.HardCurrencyRate;
                            }
                        }
                        else if (!string.IsNullOrEmpty(hardCurrencyId) && voucherDetailVM.CurrencyId == hardCurrencyId)
                        {
                            voucherDetailVM.HardCurrencyRate = 1;
                            voucherDetailVM.HardCurrencyConversion = 1;
                            voucherDetailVM.CompanyFromCurrencyId = voucherDetailVM.CurrencyId;

                            voucherDetailVM.CompanyCurrencyRate = 1 / (voucherDetailVM.HardCurrencyAmount / voucherDetailVM.CompanyCurrencyAmount);
                            voucherDetailVM.CompanyCurrencyConversion = voucherDetailVM.HardCurrencyConversion * voucherDetailVM.HardCurrencyRate;

                            voucherDetailVM.CompanyGroupCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.CompanyGroupCurrencyAmount;
                            voucherDetailVM.CompanyCurrencyConversion = voucherDetailVM.CompanyCurrencyConversion / voucherDetailVM.CompanyGroupCurrencyRate;
                        }
                        else
                        {
                            voucherDetailVM.CompanyCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.Amount;
                            voucherDetailVM.CompanyCurrencyConversion = 1 / voucherDetailVM.CompanyCurrencyRate;
                            voucherDetailVM.CompanyFromCurrencyId = voucherDetailVM.CurrencyId;
                            if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                            {
                                voucherDetailVM.CompanyGroupCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.CompanyGroupCurrencyAmount;
                                voucherDetailVM.CompanyGroupCurrencyConversion = 1 / voucherDetailVM.CompanyGroupCurrencyRate;
                            }
                            if (!string.IsNullOrEmpty(hardCurrencyId))
                            {
                                voucherDetailVM.HardCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.HardCurrencyAmount;
                                voucherDetailVM.HardCurrencyConversion = 1 / voucherDetailVM.HardCurrencyRate;
                            }
                        }
                    }

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        if (voucherDetailVM.CompanyCurrencyAmount <= 0)
                            throw new CustomException($"{voucherDetailVM.GLGeneralInfoName} GL {companyCurrencyId} {voucherDetailVM.TrnType} amount must have to greater than zero!");
                        else
                        if (voucherDetailVM.CurrencyId == companyCurrencyId && voucherDetailVM.Amount != voucherDetailVM.CompanyCurrencyAmount)
                            throw new CustomException($"{voucherDetailVM.GLGeneralInfoName} GL {companyCurrencyId} {voucherDetailVM.TrnType} amount and Transaction amount is not equal!");

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            DrAmount = voucherDetailVM.TrnType == "Debit" ? voucherDetailVM.CompanyCurrencyAmount : 0,
                            CrAmount = voucherDetailVM.TrnType == "Credit" ? voucherDetailVM.CompanyCurrencyAmount : 0,
                            FromCurrencyId = voucherDetailVM.CompanyFromCurrencyId,
                            ParallelCurrencyId = voucherDetailVM.CompanyCurrencyId,
                            ToCurrencyId = voucherDetailVM.ToCurrencyId,
                            ToCurrencyConversion = voucherDetailVM.CompanyCurrencyConversion,
                            ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate
                        });
                    }

                    // Set company Group currency.
                    if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                    {
                        if (voucherDetailVM.CompanyGroupCurrencyAmount <= 0)
                            throw new CustomException($"{voucherDetailVM.GLGeneralInfoName} GL {companyGroupCurrencyId} {voucherDetailVM.TrnType} amount must have to greater than zero!");
                        else if (voucherDetailVM.CurrencyId == companyGroupCurrencyId && voucherDetailVM.Amount != voucherDetailVM.CompanyGroupCurrencyAmount)
                            throw new CustomException($"{voucherDetailVM.GLGeneralInfoName} GL {companyGroupCurrencyId} {voucherDetailVM.TrnType} amount and Transaction amount is not equal!");
                        _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            VoucherId = voucher.Id,
                            VoucherDetailId = voucherDetail.Id,
                            AddedBy = voucherDetail.AddedBy,
                            AddedDate = voucherDetail.AddedDate,
                            AddedFromIP = voucherDetail.AddedFromIP,
                            DrAmount = voucherDetailVM.TrnType == "Debit" ? voucherDetailVM.CompanyGroupCurrencyAmount : 0,
                            CrAmount = voucherDetailVM.TrnType == "Credit" ? voucherDetailVM.CompanyGroupCurrencyAmount : 0,
                            FromCurrencyId = voucherDetailVM.CompanyGroupFromCurrencyId,
                            ParallelCurrencyId = voucherDetailVM.CompanyGroupCurrencyId,
                            ToCurrencyId = voucherDetailVM.ToCurrencyId,
                            ToCurrencyConversion = voucherDetailVM.CompanyGroupCurrencyConversion,
                            ToCurrencyRate = voucherDetailVM.CompanyGroupCurrencyRate
                        });
                    }

                    // Set hard currency.
                    if (!string.IsNullOrEmpty(hardCurrencyId))
                    {
                        if (voucherDetailVM.HardCurrencyAmount <= 0)
                            throw new CustomException($"{voucherDetailVM.GLGeneralInfoName} GL {hardCurrencyId} {voucherDetailVM.TrnType} amount must have to greater than zero!");
                        else if (voucherDetailVM.CurrencyId == hardCurrencyId && voucherDetailVM.Amount != voucherDetailVM.HardCurrencyAmount)
                            throw new CustomException($"{voucherDetailVM.GLGeneralInfoName} GL {hardCurrencyId} {voucherDetailVM.TrnType} amount and Transaction amount is not equal!");
                        _voucherService.InsertVoucherDetailHardCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            VoucherId = voucher.Id,
                            VoucherDetailId = voucherDetail.Id,
                            ModelState = ModelState.Added,
                            AddedBy = voucherDetail.AddedBy,
                            AddedDate = voucherDetail.AddedDate,
                            AddedFromIP = voucherDetail.AddedFromIP,
                            DrAmount = voucherDetailVM.TrnType == "Debit" ? voucherDetailVM.HardCurrencyAmount : 0,
                            CrAmount = voucherDetailVM.TrnType == "Credit" ? voucherDetailVM.HardCurrencyAmount : 0,
                            FromCurrencyId = voucherDetailVM.HardFromCurrencyId,
                            ParallelCurrencyId = voucherDetailVM.HardCurrencyId,
                            ToCurrencyId = voucherDetailVM.ToCurrencyId,
                            ToCurrencyConversion = voucherDetailVM.HardCurrencyConversion,
                            ToCurrencyRate = voucherDetailVM.HardCurrencyRate,
                        });
                    }
                }

                // Update OpeningBalance IsPark flag
                var openingBalanceIds = voucherDetailVMList.Where(r => r.OpeningBalanceId != null).Select(r => r.OpeningBalanceId).Distinct();
                foreach (var openingBalanceId in openingBalanceIds)
                {
                    var openingBalance = Find(openingBalanceId);
                    openingBalance.IsPark = false;
                    openingBalance.VoucherId = voucher.Id;
                    AuditService.UpdatedLog(openingBalance);
                    UpdateGraph(openingBalance);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, voucher.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetJournalList(GridParameter parameters, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT V.Id, V.CompanyGroupId, V.CompanyId, V.FiscalYearId, FY.FiscalYearName, V.FiscalYearPeriodId, FYP.PeriodName AS FiscalYearPeriodName, V.VoucherNo, V.PostingDate, V.DocRefNo, V.DocDate, V.Narration, V.IsPark
                                FROM [TRN].[Voucher] AS V
                                JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                                JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                                WHERE V.Archive=0 AND V.[SourceType]='" + SourceType.OpeningBalance + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        #endregion Journal

        #region AdvanceJournal

        public string InsertAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                // _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                //_companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.SourceType = SourceType.OpeningBalance.ToString();
                var openingBalance = new OpeningBalance
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    RefId = null,
                    SourceType = voucherVM.SourceType,
                    Narration = voucherVM.Narration,
                    PartyType = voucherVM.PartyType,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    PostingDate = voucherVM.PostingDate,
                    IsPark = true,
                    IsPosted = false,
                    VoucherId = null,
                };
                AuditService.AddedLog(openingBalance);
                openingBalance.Id = GetOpeningBalancePK(openingBalance);
                Insert(openingBalance);

                var currentVoucherDetailId = 0;
                foreach (var openingBalanceDetailVM in voucherDetailVMList)
                {
                    currentVoucherDetailId++;
                    var openingBalanceDetail = new OpeningBalanceDetail
                    {
                        Id = MakePK(openingBalance.Id, currentVoucherDetailId, 4),
                        //OpeningBalanceId = openingBalanceDetailVM.OpeningBalanceId==null? openingBalance.Id: openingBalanceDetailVM.OpeningBalanceId,
                        OpeningBalanceId = openingBalance.Id,
                        ModelState = ModelState.Added,
                        CompanyId = openingBalance.CompanyId,
                        PlantId = openingBalance.PlantId,
                        DrAmount = openingBalanceDetailVM.DrAmount,
                        CrAmount = openingBalanceDetailVM.CrAmount,
                        CurrencyId = voucherVM.CurrencyId,
                        DocDate = openingBalance.DocDate,
                        DocRefNo = openingBalance.DocRefNo,
                        Narration = openingBalance.Narration,
                        EntityId = openingBalance.EntityId,
                        BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                        BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,
                        GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId,
                        BudgetMasterId = openingBalanceDetailVM.BudgetMasterId,
                        ActivityId = openingBalanceDetailVM.ActivityId,
                        BankMasterId = openingBalanceDetailVM.BankMasterId,
                        CashMasterId = openingBalanceDetailVM.CashMasterId,
                        CashCurrencyId = openingBalanceDetailVM.CashCurrencyId,
                        BankCurrencyId = openingBalanceDetailVM.BankCurrencyId,
                        BankAmount = 0,
                        PartyType = openingBalanceDetailVM.PartyType,
                        RefId = openingBalanceDetailVM.RefId,
                        PartyId = openingBalanceDetailVM.PartyId,
                        PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                        EmployeeId = openingBalanceDetailVM.EmployeeId,
                        RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                        LifeOfYear = openingBalanceDetailVM.LifeOfYear,
                        NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear,
                        TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment,
                        NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment,
                        ProfitRate = openingBalanceDetailVM.ProfitRate,
                        SanctionAmount = openingBalanceDetailVM.SanctionAmount,
                        TransactionTypeId = openingBalanceDetailVM.TransactionTypeId,
                        FAType = openingBalanceDetailVM.FAType,
                        //FixedAssetMasterId= openingBalanceDetailVM.FixedAssetMasterId,
                        MaterialMasterId = openingBalanceDetailVM.MaterialMasterId,
                        MaterialMasterOpeningBalanceDetailId = openingBalanceDetailVM.MaterialMasterOpeningBalanceDetailId,
                        LoanOpeningBalanceDetailId = openingBalanceDetailVM.LoanOpeningBalanceDetailId,
                        SecurityOpeningBalanceDetailId = openingBalanceDetailVM.SecurityOpeningBalanceDetailId,
                        EquityOpeningBalanceDetailId = openingBalanceDetailVM.EquityOpeningBalanceDetailId,
                        InvestmentOpeningBalanceDetailId = openingBalanceDetailVM.InvestmentOpeningBalanceDetailId
                    };
                    if (openingBalanceDetailVM.BankMasterId != null)
                    {
                        openingBalanceDetail.BankAmount = openingBalanceDetailVM.BankCurrencyId == voucherVM.CurrencyId ? openingBalanceDetailVM.DrAmount : openingBalanceDetailVM.BankAmount;
                    }
                    if (openingBalanceDetailVM.CashMasterId != null)
                    {
                        openingBalanceDetail.BankAmount = openingBalanceDetailVM.CashCurrencyId == voucherVM.CurrencyId ? openingBalanceDetailVM.DrAmount : openingBalanceDetailVM.BankAmount;
                    }
                    AuditService.AddedLog(openingBalanceDetail);
                    _openingBalanceDetailRepository.Insert(openingBalanceDetail);
                    var companyCurrency = new OpeningBalanceDetailCurrency
                    {
                        ModelState = ModelState.Added,
                        Id = openingBalanceDetail.Id + 1,
                        OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                        OpeningBalanceDetailId = openingBalanceDetail.Id,
                        ParallelCurrencyId = openingBalanceDetail.CurrencyId,
                        FromCurrencyId = openingBalanceDetail.CurrencyId,
                        ToCurrencyId = openingBalanceDetail.CurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                        DrAmount = openingBalanceDetailVM.DrAmount,
                        CrAmount = openingBalanceDetailVM.CrAmount,
                        AddedBy = openingBalanceDetail.AddedBy,
                        AddedDate = openingBalanceDetail.AddedDate,
                        AddedFromIP = openingBalanceDetail.AddedFromIP
                    };
                    _openingBalanceDetailCurrencyRepository.Insert(companyCurrency);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return openingBalance.DocRefNo;
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public string UpdateAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                //_companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.SourceType = SourceType.OpeningBalance.ToString();
                var openingBalance = new OpeningBalance
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    RefId = null,
                    SourceType = voucherVM.SourceType,
                    Narration = voucherVM.Narration,
                    PartyType = voucherVM.PartyType,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    PostingDate = voucherVM.PostingDate,
                    IsPark = true,
                    IsPosted = false,
                    VoucherId = null,
                    Id = voucherVM.Id,
                    AddedBy = voucherVM.AddedBy,
                    AddedDate = voucherVM.AddedDate,
                    AddedFromIP = voucherVM.AddedFromIP
                };
                AuditService.UpdatedLog(openingBalance);
                Update(openingBalance);

                var currentRecord = _openingBalanceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.OpeningBalanceDetail WHERE OpeningBalanceId='{openingBalance.Id}'").First();
                foreach (var openingBalanceDetailVM in voucherDetailVMList)
                {
                    if (!string.IsNullOrEmpty(openingBalanceDetailVM.Id))
                    {
                        var openingBalanceDetail = new OpeningBalanceDetail
                        {
                            Id = openingBalanceDetailVM.Id,
                            OpeningBalanceId = openingBalance.Id,
                            //OpeningBalanceId = openingBalanceDetailVM.OpeningBalanceId,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = openingBalance.DocDate,
                            DocRefNo = openingBalance.DocRefNo,
                            Narration = openingBalance.Narration,
                            EntityId = openingBalance.EntityId,
                            BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                            BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,
                            GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId,
                            BudgetMasterId = openingBalanceDetailVM.BudgetMasterId,
                            ActivityId = openingBalanceDetailVM.ActivityId,
                            BankMasterId = openingBalanceDetailVM.BankMasterId,
                            CashMasterId = openingBalanceDetailVM.CashMasterId,
                            CashCurrencyId = openingBalanceDetailVM.CashCurrencyId,
                            BankCurrencyId = openingBalanceDetailVM.BankCurrencyId,
                            BankAmount = 0,
                            PartyType = openingBalanceDetailVM.PartyType,
                            RefId = openingBalanceDetailVM.RefId,
                            PartyId = openingBalanceDetailVM.PartyId,
                            PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                            EmployeeId = openingBalanceDetailVM.EmployeeId,
                            TransactionTypeId = openingBalanceDetailVM.TransactionTypeId,
                            RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                            LifeOfYear = openingBalanceDetailVM.LifeOfYear,
                            NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear,
                            TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment,
                            NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment,
                            ProfitRate = openingBalanceDetailVM.ProfitRate,
                            SanctionAmount = openingBalanceDetailVM.SanctionAmount,
                            FAType = openingBalanceDetailVM.FAType,
                            //FixedAssetMasterId= openingBalanceDetailVM.FixedAssetMasterId,
                            UpdatedBy = openingBalance.UpdatedBy,
                            UpdatedDate = openingBalance.UpdatedDate,
                            UpdatedFromIP = openingBalance.UpdatedFromIP,
                            MaterialMasterOpeningBalanceDetailId = openingBalanceDetailVM.MaterialMasterOpeningBalanceDetailId,
                            LoanOpeningBalanceDetailId = openingBalanceDetailVM.LoanOpeningBalanceDetailId,
                            SecurityOpeningBalanceDetailId = openingBalanceDetailVM.SecurityOpeningBalanceDetailId,
                            EquityOpeningBalanceDetailId = openingBalanceDetailVM.EquityOpeningBalanceDetailId,
                            InvestmentOpeningBalanceDetailId = openingBalanceDetailVM.InvestmentOpeningBalanceDetailId
                        };
                        if (openingBalanceDetailVM.BankMasterId != null)
                        {
                            openingBalanceDetail.BankAmount = openingBalanceDetailVM.BankCurrencyId == voucherVM.CurrencyId ? openingBalanceDetailVM.DrAmount : openingBalanceDetailVM.BankAmount;
                        }
                        if (openingBalanceDetailVM.CashMasterId != null)
                        {
                            openingBalanceDetail.BankAmount = openingBalanceDetailVM.CashCurrencyId == voucherVM.CurrencyId ? openingBalanceDetailVM.DrAmount : openingBalanceDetailVM.BankAmount;
                        }
                        _openingBalanceDetailRepository.Update(openingBalanceDetail);

                        var companyCurrency = new OpeningBalanceDetailCurrency
                        {
                            Id = openingBalanceDetailVM.OpeningBalanceDetailCurrencyId,
                            OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                            OpeningBalanceDetailId = openingBalanceDetail.Id,
                            ParallelCurrencyId = openingBalanceDetail.CurrencyId,
                            FromCurrencyId = openingBalanceDetail.CurrencyId,
                            ToCurrencyId = openingBalanceDetail.CurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            UpdatedBy = openingBalanceDetail.UpdatedBy,
                            UpdatedDate = openingBalanceDetail.UpdatedDate,
                            UpdatedFromIP = openingBalanceDetail.UpdatedFromIP
                        };
                        _openingBalanceDetailCurrencyRepository.Update(companyCurrency);
                    }
                    else
                    {
                        currentRecord++;
                        var openingBalanceDetail = new OpeningBalanceDetail
                        {
                            Id = MakePK(openingBalance.Id, currentRecord, 4),
                            OpeningBalanceId = openingBalance.Id,
                            // OpeningBalanceId = openingBalanceDetailVM.OpeningBalanceId == null ? openingBalance.Id : openingBalanceDetailVM.OpeningBalanceId,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = openingBalance.DocDate,
                            DocRefNo = openingBalance.DocRefNo,
                            Narration = openingBalance.Narration,
                            EntityId = openingBalance.EntityId,
                            BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                            BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,
                            GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId,
                            BudgetMasterId = openingBalanceDetailVM.BudgetMasterId,
                            ActivityId = openingBalanceDetailVM.ActivityId,
                            BankMasterId = openingBalanceDetailVM.BankMasterId,
                            CashMasterId = openingBalanceDetailVM.CashMasterId,
                            CashCurrencyId = openingBalanceDetailVM.CashCurrencyId,
                            BankCurrencyId = openingBalanceDetailVM.BankCurrencyId,
                            BankAmount = 0,
                            PartyType = openingBalanceDetailVM.PartyType,
                            RefId = openingBalanceDetailVM.RefId,
                            PartyId = openingBalanceDetailVM.PartyId,
                            PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                            EmployeeId = openingBalanceDetailVM.EmployeeId,
                            RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                            LifeOfYear = openingBalanceDetailVM.LifeOfYear,
                            NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear,
                            TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment,
                            NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment,
                            ProfitRate = openingBalanceDetailVM.ProfitRate,
                            SanctionAmount = openingBalanceDetailVM.SanctionAmount,
                            TransactionTypeId = openingBalanceDetailVM.TransactionTypeId,
                            FAType = openingBalanceDetailVM.FAType,
                            //FixedAssetMasterId= openingBalanceDetailVM.FixedAssetMasterId,
                            MaterialMasterOpeningBalanceDetailId = openingBalanceDetailVM.MaterialMasterOpeningBalanceDetailId,
                            LoanOpeningBalanceDetailId = openingBalanceDetailVM.LoanOpeningBalanceDetailId,
                            SecurityOpeningBalanceDetailId = openingBalanceDetailVM.SecurityOpeningBalanceDetailId,
                            EquityOpeningBalanceDetailId = openingBalanceDetailVM.EquityOpeningBalanceDetailId,
                            InvestmentOpeningBalanceDetailId = openingBalanceDetailVM.InvestmentOpeningBalanceDetailId

                        };
                        if (openingBalanceDetailVM.BankMasterId != null)
                        {
                            openingBalanceDetail.BankAmount = openingBalanceDetailVM.BankCurrencyId == voucherVM.CurrencyId ? openingBalanceDetailVM.DrAmount : openingBalanceDetailVM.BankAmount;
                        }
                        if (openingBalanceDetailVM.CashMasterId != null)
                        {
                            openingBalanceDetail.BankAmount = openingBalanceDetailVM.CashCurrencyId == voucherVM.CurrencyId ? openingBalanceDetailVM.DrAmount : openingBalanceDetailVM.BankAmount;
                        }
                        AuditService.AddedLog(openingBalanceDetail);
                        _openingBalanceDetailRepository.Insert(openingBalanceDetail);

                        var companyCurrency = new OpeningBalanceDetailCurrency
                        {
                            Id = openingBalanceDetail.Id + 1,
                            OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                            OpeningBalanceDetailId = openingBalanceDetail.Id,
                            ParallelCurrencyId = openingBalanceDetail.CurrencyId,
                            FromCurrencyId = openingBalanceDetail.CurrencyId,
                            ToCurrencyId = openingBalanceDetail.CurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            AddedBy = openingBalanceDetail.AddedBy,
                            AddedDate = openingBalanceDetail.AddedDate,
                            AddedFromIP = openingBalanceDetail.AddedFromIP
                        };
                        _openingBalanceDetailCurrencyRepository.Insert(companyCurrency);
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return openingBalance.DocRefNo;
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public string InsertGLAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.SourceType = SourceType.OpeningBalance.ToString();
                var openingBalance = new OpeningBalance
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    RefId = null,
                    SourceType = voucherVM.SourceType,
                    Narration = voucherVM.Narration,
                    PartyType = voucherVM.PartyType,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    PostingDate = voucherVM.PostingDate,
                    IsPark = true,
                    IsPosted = false,
                    VoucherId = null,
                    Id = voucherVM.Id,
                    AddedBy = voucherVM.AddedBy,
                    AddedDate = voucherVM.AddedDate,
                    AddedFromIP = voucherVM.AddedFromIP
                };
                
                if (voucherVM.Id == null)
                {
                    openingBalance.Id = GetOpeningBalancePK(openingBalance);
                    Insert(openingBalance);
                }
                else
                {
                    AuditService.UpdatedLog(openingBalance);
                    Update(openingBalance);
                }

                var currentRecord = _openingBalanceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.OpeningBalanceDetail WHERE OpeningBalanceId='{openingBalance.Id}'").First();
                foreach (var openingBalanceDetailVM in voucherDetailVMList)
                {

                    currentRecord++;
                    var openingBalanceDetail = new OpeningBalanceDetail
                    {
                        Id = MakePK(openingBalance.Id, currentRecord, 4),
                        OpeningBalanceId = openingBalance.Id,
                        // OpeningBalanceId = openingBalanceDetailVM.OpeningBalanceId == null ? openingBalance.Id : openingBalanceDetailVM.OpeningBalanceId,
                        DrAmount = openingBalanceDetailVM.DrAmount,
                        CrAmount = openingBalanceDetailVM.CrAmount,
                        CurrencyId = voucherVM.CurrencyId,
                        DocDate = openingBalance.DocDate,
                        DocRefNo = openingBalance.DocRefNo,
                        Narration = openingBalance.Narration,
                        EntityId = openingBalance.EntityId,
                        BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                        BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,
                        GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId,
                        BudgetMasterId = openingBalanceDetailVM.BudgetMasterId,
                        ActivityId = openingBalanceDetailVM.ActivityId,
                        BankMasterId = openingBalanceDetailVM.BankMasterId,
                        CashMasterId = openingBalanceDetailVM.CashMasterId,
                        CashCurrencyId = openingBalanceDetailVM.CashCurrencyId,
                        BankCurrencyId = openingBalanceDetailVM.BankCurrencyId,
                        BankAmount = 0,
                        PartyType = openingBalanceDetailVM.PartyType,
                        RefId = openingBalanceDetailVM.RefId,
                        PartyId = openingBalanceDetailVM.PartyId,
                        PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                        EmployeeId = openingBalanceDetailVM.EmployeeId,
                        RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                        LifeOfYear = openingBalanceDetailVM.LifeOfYear,
                        NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear,
                        TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment,
                        NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment,
                        ProfitRate = openingBalanceDetailVM.ProfitRate,
                        SanctionAmount = openingBalanceDetailVM.SanctionAmount,
                        TransactionTypeId = openingBalanceDetailVM.TransactionTypeId,
                        FAType = openingBalanceDetailVM.FAType,
                        //FixedAssetMasterId= openingBalanceDetailVM.FixedAssetMasterId,
                        MaterialMasterOpeningBalanceDetailId = openingBalanceDetailVM.MaterialMasterOpeningBalanceDetailId,
                        LoanOpeningBalanceDetailId = openingBalanceDetailVM.LoanOpeningBalanceDetailId,
                        SecurityOpeningBalanceDetailId = openingBalanceDetailVM.SecurityOpeningBalanceDetailId,
                        EquityOpeningBalanceDetailId = openingBalanceDetailVM.EquityOpeningBalanceDetailId,
                        InvestmentOpeningBalanceDetailId = openingBalanceDetailVM.InvestmentOpeningBalanceDetailId

                    };
                    if (openingBalanceDetailVM.BankMasterId != null)
                    {
                        openingBalanceDetail.BankAmount = openingBalanceDetailVM.BankCurrencyId == voucherVM.CurrencyId ? openingBalanceDetailVM.DrAmount : openingBalanceDetailVM.BankAmount;
                    }
                    if (openingBalanceDetailVM.CashMasterId != null)
                    {
                        openingBalanceDetail.BankAmount = openingBalanceDetailVM.CashCurrencyId == voucherVM.CurrencyId ? openingBalanceDetailVM.DrAmount : openingBalanceDetailVM.BankAmount;
                    }
                    AuditService.AddedLog(openingBalanceDetail);
                    _openingBalanceDetailRepository.Insert(openingBalanceDetail);

                    var companyCurrency = new OpeningBalanceDetailCurrency
                    {
                        Id = openingBalanceDetail.Id + 1,
                        OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                        OpeningBalanceDetailId = openingBalanceDetail.Id,
                        ParallelCurrencyId = openingBalanceDetail.CurrencyId,
                        FromCurrencyId = openingBalanceDetail.CurrencyId,
                        ToCurrencyId = openingBalanceDetail.CurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                        DrAmount = openingBalanceDetailVM.DrAmount,
                        CrAmount = openingBalanceDetailVM.CrAmount,
                        AddedBy = openingBalanceDetail.AddedBy,
                        AddedDate = openingBalanceDetail.AddedDate,
                        AddedFromIP = openingBalanceDetail.AddedFromIP
                    };
                    _openingBalanceDetailCurrencyRepository.Insert(companyCurrency);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return openingBalance.DocRefNo;
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public string PostAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.SourceType = SourceType.OpeningBalance.ToString();
                var openingBalance = Find(voucherVM.Id);

                var voucher = _voucherService.InsertVoucher(voucherVM);

                openingBalance.VoucherId = voucher.Id;
                openingBalance.IsPark = voucherVM.IsPark;
                openingBalance.IsPosted = voucherVM.IsPosted;
                _openingBalanceRepository.Update(openingBalance);

                var currentVoucherDetailId = 0;
                var currentOpeningDetailId = _openingBalanceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.OpeningBalanceDetail WHERE OpeningBalanceId='{openingBalance.Id}'").First();
                foreach (var openingBalanceDetailVM in voucherDetailVMList)
                {
                    if (openingBalanceDetailVM.Id != null)
                    {
                        var openingBalanceDetail = new OpeningBalanceDetail
                        {
                            Id = openingBalanceDetailVM.Id,
                            //OpeningBalanceId = openingBalanceDetailVM.OpeningBalanceId == null ? openingBalance.Id : openingBalanceDetailVM.OpeningBalanceId,
                            OpeningBalanceId = openingBalance.Id,
                            ModelState = ModelState.Modified,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = openingBalance.DocDate,
                            DocRefNo = openingBalance.DocRefNo,
                            Narration = openingBalance.Narration,
                            EntityId = openingBalance.EntityId,
                            BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                            BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,
                            GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId,
                            BudgetMasterId = openingBalanceDetailVM.BudgetMasterId,
                            ActivityId = openingBalanceDetailVM.ActivityId,
                            BankMasterId = openingBalanceDetailVM.BankMasterId,
                            CashMasterId = openingBalanceDetailVM.CashMasterId,
                            PartyType = openingBalanceDetailVM.PartyType,
                            RefId = openingBalanceDetailVM.RefId,
                            PartyId = openingBalanceDetailVM.PartyId,
                            PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                            EmployeeId = openingBalanceDetailVM.EmployeeId,
                            RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                            LifeOfYear = openingBalanceDetailVM.LifeOfYear,
                            NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear,
                            TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment,
                            NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment,
                            ProfitRate = openingBalanceDetailVM.ProfitRate,
                            SanctionAmount = openingBalanceDetailVM.SanctionAmount,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            MaterialMasterOpeningBalanceDetailId = openingBalanceDetailVM.MaterialMasterOpeningBalanceDetailId
                        };
                        AuditService.UpdatedLog(openingBalanceDetail);
                        _openingBalanceDetailRepository.InsertOrUpdateGraph(openingBalanceDetail);

                        var voucherDetail = new VoucherDetail
                        {
                            ModelState = ModelState.Modified,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = openingBalance.DocDate,
                            DocRefNo = openingBalance.DocRefNo,
                            Narration = openingBalance.Narration,
                            EntityId = openingBalance.EntityId,
                            GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId,
                            BudgetMasterId = openingBalanceDetailVM.BudgetMasterId,
                            ActivityId = openingBalanceDetailVM.ActivityId,
                            BankMasterId = openingBalanceDetailVM.BankMasterId,
                            CashMasterId = openingBalanceDetailVM.CashMasterId,
                            PartyType = openingBalanceDetailVM.PartyType,
                            PartyId = openingBalanceDetailVM.PartyId,
                            PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                            EmployeeId = openingBalanceDetailVM.EmployeeId,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

                        if (voucherDetail.BankMasterId != null || voucherDetail.CashMasterId != null)
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetail, new GLTransactionDetail
                            {
                                SourceType = voucher.SourceType,
                                BankMasterId = voucherDetail.BankMasterId,
                                CashMasterId = voucherDetail.CashMasterId,
                                DrAmount = voucherDetail.DrAmount * voucherVM.CompanyCurrencyRate,
                                CrAmount = voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate
                            });
                        }

                        var companyCurrency = new OpeningBalanceDetailCurrency
                        {
                            ModelState = ModelState.Modified,
                            //Id = openingBalanceDetailVM.OpeningBalanceDetailCurrencyId,
                            OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                            OpeningBalanceDetailId = openingBalanceDetail.Id,
                            ParallelCurrencyId = openingBalanceDetail.CurrencyId,
                            FromCurrencyId = openingBalanceDetail.CurrencyId,
                            ToCurrencyId = openingBalanceDetail.CurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            AddedBy = openingBalanceDetail.AddedBy,
                            AddedDate = openingBalanceDetail.AddedDate,
                            AddedFromIP = openingBalanceDetail.AddedFromIP
                        };
                        _openingBalanceDetailCurrencyRepository.InsertOrUpdateGraph(companyCurrency);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetail.DrAmount * voucherVM.CompanyCurrencyRate,
                            CrAmount = voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate
                        });
                    }
                    else
                    {
                        currentOpeningDetailId++;
                        var openingBalanceDetail = new OpeningBalanceDetail
                        {
                            Id = MakePK(openingBalance.Id, currentOpeningDetailId, 4),
                            //OpeningBalanceId = openingBalanceDetailVM.OpeningBalanceId == null ? openingBalance.Id : openingBalanceDetailVM.OpeningBalanceId,
                            OpeningBalanceId = openingBalance.Id,
                            ModelState = ModelState.Added,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = openingBalance.DocDate,
                            DocRefNo = openingBalance.DocRefNo,
                            Narration = openingBalance.Narration,
                            EntityId = openingBalance.EntityId,
                            BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                            BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,
                            GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId,
                            BudgetMasterId = openingBalanceDetailVM.BudgetMasterId,
                            ActivityId = openingBalanceDetailVM.ActivityId,
                            BankMasterId = openingBalanceDetailVM.BankMasterId,
                            CashMasterId = openingBalanceDetailVM.CashMasterId,
                            PartyType = openingBalanceDetailVM.PartyType,
                            RefId = openingBalanceDetailVM.RefId,
                            PartyId = openingBalanceDetailVM.PartyId,
                            PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                            EmployeeId = openingBalanceDetailVM.EmployeeId,
                            RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                            LifeOfYear = openingBalanceDetailVM.LifeOfYear,
                            NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear,
                            TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment,
                            NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment,
                            ProfitRate = openingBalanceDetailVM.ProfitRate,
                            SanctionAmount = openingBalanceDetailVM.SanctionAmount,
                            MaterialMasterOpeningBalanceDetailId = openingBalanceDetailVM.MaterialMasterOpeningBalanceDetailId
                        };
                        AuditService.AddedLog(openingBalanceDetail);
                        _openingBalanceDetailRepository.Insert(openingBalanceDetail);

                        var voucherDetail = new VoucherDetail
                        {
                            Id = openingBalanceDetailVM.Id,
                            ModelState = ModelState.Modified,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = openingBalance.DocDate,
                            DocRefNo = openingBalance.DocRefNo,
                            Narration = openingBalance.Narration,
                            EntityId = openingBalance.EntityId,
                            GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId,
                            BudgetMasterId = openingBalanceDetailVM.BudgetMasterId,
                            ActivityId = openingBalanceDetailVM.ActivityId,
                            BankMasterId = openingBalanceDetailVM.BankMasterId,
                            CashMasterId = openingBalanceDetailVM.CashMasterId,
                            PartyType = openingBalanceDetailVM.PartyType,
                            PartyId = openingBalanceDetailVM.PartyId,
                            PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                            EmployeeId = openingBalanceDetailVM.EmployeeId,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);
                        if (voucherDetail.BankMasterId != null || voucherDetail.CashMasterId != null)
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetail, new GLTransactionDetail
                            {
                                SourceType = voucher.SourceType,
                                BankMasterId = voucherDetail.BankMasterId,
                                CashMasterId = voucherDetail.CashMasterId,
                                DrAmount = voucherDetail.DrAmount * voucherVM.CompanyCurrencyRate,
                                CrAmount = voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate
                            });
                        }

                        var companyCurrency = new OpeningBalanceDetailCurrency
                        {
                            ModelState = ModelState.Added,
                            Id = openingBalanceDetail.Id + 1,
                            OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                            OpeningBalanceDetailId = openingBalanceDetail.Id,
                            ParallelCurrencyId = openingBalanceDetail.CurrencyId,
                            FromCurrencyId = openingBalanceDetail.CurrencyId,
                            ToCurrencyId = openingBalanceDetail.CurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                            DrAmount = openingBalanceDetailVM.DrAmount,
                            CrAmount = openingBalanceDetailVM.CrAmount,
                            AddedBy = openingBalanceDetail.AddedBy,
                            AddedDate = openingBalanceDetail.AddedDate,
                            AddedFromIP = openingBalanceDetail.AddedFromIP
                        };
                        _openingBalanceDetailCurrencyRepository.Insert(companyCurrency);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetail.DrAmount * voucherVM.CompanyCurrencyRate,
                            CrAmount = voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate
                        });
                    }

                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return openingBalance.DocRefNo;
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public string PostInsertAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            var fiscalYear = _fiscalYearService.Find(voucherVM.FiscalYearId);
            try
            {

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Voucher TABLE
                voucherVM.IsPark = false;

                var voucher = new Voucher
                {
                    IsPark = false,
                    VoucherDate = DateTime.Now,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    CurrencyId = voucherVM.CurrencyId,
                    PostingDate = voucherVM.PostingDate,
                    DocRefNo = voucherVM.DocRefNo,
                    DocDate = voucherVM.DocDate,
                    Narration = voucherVM.Narration
                };
                voucher.CurrencyId = voucherVM.CurrencyId;
                voucher.SourceType = SourceType.OpeningBalance.ToString();
                _voucherService.InsertVoucher(voucher, fiscalYear.YearPrefix);

                var advancePk = _advanceService.GetMaxNumber(nameof(Advance), PKGeneratorEnum.Yearly, null, voucherVM.VoucherDate);
                var invoicePk = _invoiceService.GetMaxNumber(nameof(Invoice), PKGeneratorEnum.Yearly, null, voucherVM.VoucherDate);
                var securityDepositPk = _securityDepositService.GetMaxNumber(nameof(SecurityDeposit), PKGeneratorEnum.Yearly, null, voucherVM.VoucherDate);
                var employeePayablePk = _employeePayableService.GetMaxNumber();
                var financingPk = _financingService.GetMaxNumber();
                 
                
                var currentRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    // Set to currency
                    voucherDetailVM.ToCurrencyId = companyCurrencyId;

                    // INSERT INTO VOUCHER DETAIL
                    var voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucherVM.Id,
                        PlantId = voucherDetailVM.PlantId,
                        OpeningBalanceDetailId = voucherDetailVM.Id,
                        EntityId = voucherDetailVM.IsOB ? voucherDetailVM.EntityId : voucherVM.EntityId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        PartyType = voucherDetailVM.PartyType,
                        EmployeeId = voucherDetailVM.EmployeeId,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        CashMasterId = voucherDetailVM.CashMasterId,
                        BankMasterId = voucherDetailVM.BankMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        DrAmount = voucherDetailVM.DrAmount,
                        CrAmount = voucherDetailVM.CrAmount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        AddedBy = voucherVM.AddedBy,
                        AddedDate = voucherVM.AddedDate,
                        AddedFromIP = voucherVM.AddedFromIP,
                        //FixedAssetMasterId= voucherDetailVM.FixedAssetMasterId,
                        FAType = voucherDetailVM.FAType,
                    };
                    currentRecord++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentRecord);
                    if (voucherDetailVM.MaterialMasterOpeningBalanceDetailId != null)
                    {
                        var materialOB = _materialMasterOpeningBalanceDetailRepository.Find(voucherDetailVM.MaterialMasterOpeningBalanceDetailId);
                        if (materialOB != null)
                        {
                            var ob = _openingBalanceRepository.Find(materialOB.OpeningBalanceId);
                            ob.IsPark = false;
                            ob.IsPosted = true;
                            _openingBalanceRepository.Update(ob);
                            var grn = _inventoryReceiveRepository.Query(r => r.OpeningBalanceId == materialOB.OpeningBalanceId).Select().FirstOrDefault();
                            if (grn != null)
                            {
                                grn.Status = "Posting";
                                grn.IsApproved = true;
                                //grn.CheckedBy = "";
                                grn.CheckedByStatus = "Checked";
                                //grn.AuthorizedBy = "";
                                grn.AuthorizedByStatus = "Approval";
                                _inventoryReceiveRepository.Update(grn);
                            }
                        }
                    }
                    if (voucherDetailVM.LoanOpeningBalanceDetailId != null)
                    {
                        var loanOB = _openingBalanceDetailRepository.Find(voucherDetailVM.LoanOpeningBalanceDetailId);
                        if (loanOB != null)
                        {
                            var ob = _openingBalanceRepository.Query(r => r.Id == loanOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financing = _financingRepository.Query(r => r.OpeningBalanceId == ob.Id).Select().FirstOrDefault();
                            var financingDetail = _financingDetailRepository.Query(r => r.FinancingId == financing.Id).Select().FirstOrDefault();
                            if (ob != null)
                            {
                                ob.VoucherId = voucher.Id;
                                ob.ModelState = ModelState.Modified;
                                _openingBalanceRepository.Update(ob);
                            }
                            if (financing != null)
                            {
                                financing.VoucherId = voucher.Id;
                                financing.IsPosted = true;
                                financing.IsPark = false;
                                financing.ModelState = ModelState.Modified;
                                _financingRepository.Update(financing);
                            }
                            if (financingDetail != null)
                            {
                                voucherDetail.FinancingDetailId = financingDetail.Id;
                                voucherDetail.BankMasterId = financing.BankMasterId;
                                if (voucherDetail.BankMasterId != null)
                                {
                                    var glTransactionDetail = new GLTransactionDetail
                                    {
                                        VoucherDetailId = voucherDetail.Id,
                                        BankMasterId = voucherDetail.BankMasterId,
                                        CashMasterId = voucherDetail.CashMasterId,
                                        SourceType = voucher.SourceType,
                                        DrAmount = voucherDetail.DrAmount,
                                        CrAmount = voucherDetail.CrAmount,
                                    };
                                    _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                                }
                            }
                        }
                    }
                    if (voucherDetailVM.SecurityOpeningBalanceDetailId != null)
                    {
                        var securityOB = _openingBalanceDetailRepository.Find(voucherDetailVM.SecurityOpeningBalanceDetailId);
                        if (securityOB != null)
                        {
                            var ob = _openingBalanceRepository.Query(r => r.Id == securityOB.OpeningBalanceId).Select().FirstOrDefault();
                            var securityDeposit = _securityDepositRepository.Query(r => r.OpeningBalanceId == securityOB.OpeningBalanceId).Select().FirstOrDefault();
                            var securityDepositDetail = _securityDepositDetailRepository.Query(r => r.SecurityDepositId == securityDeposit.Id).Select().FirstOrDefault();

                            if (ob != null)
                            {
                                ob.VoucherId = voucher.Id;
                                ob.ModelState = ModelState.Modified;
                                _openingBalanceRepository.Update(ob);
                            }
                            if (securityDeposit != null)
                            {
                                securityDeposit.VoucherId = voucher.Id;
                                securityDeposit.IsPark = false;
                                securityDeposit.ModelState = ModelState.Modified;
                                _securityDepositRepository.Update(securityDeposit);
                            }
                            if (securityDepositDetail != null)
                            {
                                voucherDetail.SecurityDepositDetailId = securityDepositDetail.Id;
                                voucherDetail.BankMasterId = securityDeposit.BankMasterId;
                                if (voucherDetail.BankMasterId != null)
                                {
                                    var glTransactionDetail = new GLTransactionDetail
                                    {
                                        VoucherDetailId = voucherDetail.Id,
                                        BankMasterId = voucherDetail.BankMasterId,
                                        CashMasterId = voucherDetail.CashMasterId,
                                        SourceType = voucher.SourceType,
                                        DrAmount = voucherDetail.DrAmount,
                                        CrAmount = voucherDetail.CrAmount,
                                    };
                                    _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                                }
                            }
                        }
                    }
                    if (voucherDetailVM.EquityOpeningBalanceDetailId != null)
                    {
                        var equityOB = _openingBalanceDetailRepository.Find(voucherDetailVM.EquityOpeningBalanceDetailId);
                        if (equityOB != null)
                        {
                            var ob = _openingBalanceRepository.Query(r => r.Id == equityOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financing = _financingRepository.Query(r => r.Id == equityOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financingDetail = _financingDetailRepository.Query(r => r.Id == financing.Id).Select().FirstOrDefault();
                            if (ob != null)
                            {
                                ob.VoucherId = voucher.Id;
                                ob.ModelState = ModelState.Modified;
                                _openingBalanceRepository.Update(ob);
                            }
                            if (financing != null)
                            {
                                financing.VoucherId = voucher.Id;
                                financing.IsPosted = true;
                                financing.IsPark = false;
                                financing.ModelState = ModelState.Modified;
                                _financingRepository.Update(financing);
                            }
                            if (financingDetail != null)
                            {
                                voucherDetail.FinancingDetailId = financingDetail.Id;
                            }
                        }
                    }
                    if (voucherDetailVM.InvestmentOpeningBalanceDetailId != null)
                    {
                        var investmentiOB = _openingBalanceDetailRepository.Find(voucherDetailVM.InvestmentOpeningBalanceDetailId);
                        if (investmentiOB != null)
                        {
                            var ob = _openingBalanceRepository.Query(r => r.Id == investmentiOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financing = _financingRepository.Query(r => r.Id == investmentiOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financingDetail = _financingDetailRepository.Query(r => r.Id == financing.Id).Select().FirstOrDefault();
                            if (ob != null)
                            {
                                ob.VoucherId = voucher.Id;
                                ob.ModelState = ModelState.Modified;
                                _openingBalanceRepository.Update(ob);
                            }
                            if (financing != null)
                            {
                                financing.VoucherId = voucher.Id;
                                financing.IsPosted = true;
                                financing.IsPark = false;
                                financing.ModelState = ModelState.Modified;
                                _financingRepository.Update(financing);
                            }
                            if (financingDetail != null)
                            {
                                voucherDetail.FinancingDetailId = financingDetail.Id;
                                voucherDetail.BankMasterId = financing.BankMasterId;
                                if (voucherDetail.BankMasterId != null)
                                {
                                    var glTransactionDetail = new GLTransactionDetail
                                    {
                                        VoucherDetailId = voucherDetail.Id,
                                        BankMasterId = voucherDetail.BankMasterId,
                                        CashMasterId = voucherDetail.CashMasterId,
                                        SourceType = voucher.SourceType,
                                        DrAmount = voucherDetail.DrAmount,
                                        CrAmount = voucherDetail.CrAmount,
                                    };
                                    _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(voucherDetailVM.OpeningBalanceId))
                    {
                        //Customer/Vendor invoice
                        if ((voucherDetailVM.PartyType == PartyType.Customer.ToString() && voucherDetailVM.TransactionTypeId == null && voucherDetailVM.CrAmount == 0) ||
                           voucherDetailVM.PartyType == PartyType.Vendor.ToString() && voucherDetailVM.TransactionTypeId == null && voucherDetailVM.DrAmount == 0)
                        {
                            var invoice = new Invoice
                            {
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucher.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucher.Narration,
                                BaseNoOfDays = voucherDetailVM.BaseNoOfDays,
                                BaseOnDueDate = voucherDetailVM.BaseOnDueDate,
                                Amount = voucherDetailVM.Amount,
                                SourceType = voucher.SourceType,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            if (voucherDetailVM.PartyType == PartyType.Customer.ToString())
                            {
                                invoice.SourceType = SourceType.CustomerInvoice.ToString();
                                invoice.Amount = voucherDetailVM.DrAmount;
                            }
                            if (voucherDetailVM.PartyType == PartyType.Vendor.ToString())
                            {
                                invoice.SourceType = SourceType.VendorInvoice.ToString();
                                invoice.Amount = voucherDetailVM.CrAmount;
                            }
                            invoicePk.MaxNumber++;
                            _invoiceService.InsertInvoice(invoice, invoicePk.MaxNumber);

                            var invoiceDetail = new InvoiceDetail
                            {
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = invoice.Amount,
                                NetAmount = invoice.Amount
                            };
                            _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, 1);
                            // Set InvoiceDetail Id to voucher detail.
                            voucherDetail.InvoiceDetailId = invoiceDetail.Id;
                        }
                        //Customer/Vendor Advance
                        if ((voucherDetailVM.PartyType == PartyType.Customer.ToString() && voucherDetailVM.TransactionTypeId == null && voucherDetailVM.DrAmount == 0) ||
                         voucherDetailVM.PartyType == PartyType.Vendor.ToString() && voucherDetailVM.TransactionTypeId == null && voucherDetailVM.CrAmount == 0)
                        {
                            advancePk.MaxNumber++;
                            var advance = new Advance
                            {
                                Id = voucher.VoucherDate.Year + advancePk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                EmployeeTransactionTypeId = voucherDetailVM.EmployeeTransactionTypeId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucher.Narration,
                                Amount = voucherDetailVM.Amount,
                                SourceType = voucherDetailVM.SourceType,
                                PaymentSource = voucherDetailVM.PaymentSource,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                IsPosted = true,
                                AdvanceNo = voucher.VoucherNo
                            };


                            if (voucherDetailVM.PartyType == PartyType.Customer.ToString())
                            {
                                advance.SourceType = SourceType.CustomerAdvance.ToString();
                                advance.Amount = voucherDetailVM.CrAmount;

                            }
                            if (voucherDetailVM.PartyType == PartyType.Vendor.ToString())
                            {
                                advance.SourceType = SourceType.VendorAdvance.ToString();
                                advance.Amount = voucherDetailVM.DrAmount;
                            }
                            _advanceService.InsertGraph(advance);


                            var advanceDetail = new AdvanceDetail
                            {
                                Id = _advanceService.MakeAdvanceDetailPK(advance.Id, 1),
                                AdvanceId = advance.Id,
                                CompanyId = advance.CompanyId,
                                PlantId = advance.PlantId,
                                EmployeeId = advance.EmployeeId,
                                Archive = advance.Archive,
                                IsWrittenOff = advance.IsWrittenOff,
                                ModelState = advance.ModelState,
                                Narration = advance.Narration,
                                NetAmount = advance.Amount,
                                PartyId = advance.PartyId,
                                PartyPlantId = advance.PartyPlantId,
                                PartyType = advance.PartyType,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = advance.Amount,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            };
                            _advanceService.InsertAdvanceDetail(advanceDetail);
                            // Set Advance detail to voucher detail.
                            voucherDetail.AdvanceDetailId = advanceDetail.Id;
                        }
                        //Bank/Cash
                        else if (voucherDetailVM.PartyType == PartyType.Bank.ToString() || voucherDetailVM.PartyType == PartyType.Cash.ToString())
                        {
                            var glTransactionDetail = new GLTransactionDetail
                            {
                                VoucherDetailId = voucherDetail.Id,
                                BankMasterId = voucherDetail.BankMasterId,
                                CashMasterId = voucherDetail.CashMasterId,
                                SourceType = voucher.SourceType,
                                DrAmount = voucherDetail.DrAmount,
                                CrAmount = voucherDetail.CrAmount,
                            };
                            // Set BankMasterId/CashMasterId in voucher detail.
                            voucherDetail.BankMasterId = voucherDetailVM.BankMasterId;
                            voucherDetail.CashMasterId = voucherDetailVM.CashMasterId;
                            _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                        }
                        //Employee Payable
                        else if (voucherDetailVM.PartyType == PartyType.Employee.ToString() && voucherDetailVM.TransactionTypeId != null && voucherDetailVM.DrAmount == 0)
                        {
                            employeePayablePk.MaxNumber++;
                            var employeePayable = new EmployeePayable
                            {
                                Id = voucher.VoucherDate.Year + employeePayablePk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                EmployeeTransactionTypeId = voucherDetailVM.EmployeeTransactionTypeId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                Amount = voucherDetailVM.CrAmount,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                SourceType = SourceType.EmployeePayable.ToString(),
                                PartyType = PartyType.Employee.ToString(),
                                VoucherDate = voucher.VoucherDate
                            };
                            _employeePayableService.InsertEmployeePayable(employeePayable);

                            var employeePayableDetail = new EmployeePayableDetail
                            {
                                EmployeePayableId = employeePayable.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount,
                                NetAmount = employeePayable.Amount
                            };
                            _employeePayableService.InsertEmployeePayableDetail(employeePayable, employeePayableDetail, 1);

                            // Set InvoiceDetail Id to voucher detail.
                            voucherDetail.EmployeePayableDetailId = employeePayableDetail.Id;
                            voucherDetail.PartyType = employeePayable.PartyType;
                        }
                        //Employee Advance
                        else if (voucherDetailVM.PartyType == PartyType.Employee.ToString() && voucherDetailVM.TransactionTypeId != null && voucherDetailVM.CrAmount == 0)
                        {
                            advancePk.MaxNumber++;
                            var advance = new Advance
                            {
                                Id = voucher.VoucherDate.Year + advancePk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                EmployeeTransactionTypeId = voucherDetailVM.TransactionTypeId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucher.Narration,
                                Amount = voucherDetailVM.DrAmount,
                                SourceType = voucher.SourceType,
                                PaymentSource = voucherDetailVM.PaymentSource,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                IsPosted = true,
                                AdvanceNo = voucher.VoucherNo
                            };
                            if (voucherDetailVM.PartyType == PartyType.Employee.ToString())
                                advance.SourceType = SourceType.EmployeeAdvance.ToString();
                            _advanceService.InsertGraph(advance);

                            // INSERT INTO AdvanceDetail
                            var advanceDetail = new AdvanceDetail
                            {
                                Id = _advanceService.MakeAdvanceDetailPK(advance.Id, 1),
                                AdvanceId = advance.Id,
                                CompanyId = advance.CompanyId,
                                PlantId = advance.PlantId,
                                EmployeeId = advance.EmployeeId,
                                Archive = advance.Archive,
                                IsWrittenOff = advance.IsWrittenOff,
                                ModelState = advance.ModelState,
                                Narration = advance.Narration,
                                NetAmount = advance.Amount,
                                PartyId = advance.PartyId,
                                PartyPlantId = advance.PartyPlantId,
                                PartyType = advance.PartyType,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = advance.Amount,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            };
                            _advanceService.InsertAdvanceDetail(advanceDetail);
                            // Set Advance detail to voucher detail.
                            voucherDetail.AdvanceDetailId = advanceDetail.Id;
                        }
                        //Inter Transaction
                        else if (voucherDetailVM.PartyType == PartyType.InterTransaction.ToString())
                        {
                            advancePk.MaxNumber++;
                            var interTransaction = new Advance
                            {
                                Id = voucher.VoucherDate.Year + advancePk.MaxNumber.ToString(),
                                AddedBy = voucherDetail.AddedBy,
                                AddedDate = voucherDetail.AddedDate,
                                AddedFromIP = voucherDetail.AddedFromIP,
                                Amount = voucherDetailVM.Amount,
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                EntityId = voucherDetailVM.EntityId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                VoucherId = voucher.Id,
                                VoucherDate = voucher.VoucherDate,
                                IsInterTransaction = true,
                                Narration = voucherDetailVM.Narration,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                SourceType = voucher.SourceType,
                                PostingDate = voucher.PostingDate,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                VoucherTypeId = voucher.VoucherTypeId,
                                IsPosted = true
                            };
                            // _interTransactionService.InsertGraph(interTransaction);

                            // INSERT INTO AdvanceDetail
                            var interTransactionDetail = new AdvanceDetail
                            {
                                AdvanceId = interTransaction.Id,
                                CompanyId = interTransaction.CompanyId,
                                PlantId = interTransaction.PlantId,
                                Archive = interTransaction.Archive,
                                IsWrittenOff = interTransaction.IsWrittenOff,
                                ModelState = interTransaction.ModelState,
                                Narration = interTransaction.Narration,
                                NetAmount = interTransaction.Amount,
                                PartyId = interTransaction.PartyId,
                                PartyPlantId = interTransaction.PartyPlantId,
                                PartyType = interTransaction.PartyType,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = interTransaction.Amount,
                                AddedBy = interTransaction.AddedBy,
                                AddedDate = interTransaction.AddedDate,
                                AddedFromIP = interTransaction.AddedFromIP
                            };
                            //_interTransactionService.InsertInterTransactionDetail(interTransaction, interTransactionDetail, 1);
                            // Set InterTransaction detail to voucher detail.
                            voucherDetail.InterTransactionDetailId = interTransactionDetail.Id;
                        }
                    }

                    // Making currency exchange rate and conversion.
                    if (!voucherDetailVM.IsOB)
                    {
                        if (voucherDetailVM.CurrencyId == companyCurrencyId)
                        {
                            voucherDetailVM.CompanyCurrencyRate = 1;
                            voucherDetailVM.CompanyCurrencyConversion = 1;

                            if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                            {
                                voucherDetailVM.CompanyGroupCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.CompanyGroupCurrencyAmount;
                                voucherDetailVM.CompanyGroupCurrencyConversion = voucherDetailVM.CompanyCurrencyConversion / voucherDetailVM.CompanyGroupCurrencyRate;
                            }

                        }
                        else if (!string.IsNullOrEmpty(companyGroupCurrencyId) && voucherDetailVM.CurrencyId == companyGroupCurrencyId)
                        {
                            voucherDetailVM.CompanyGroupCurrencyRate = 1;
                            voucherDetailVM.CompanyGroupCurrencyConversion = 1;
                            voucherDetailVM.CompanyFromCurrencyId = voucherDetailVM.CurrencyId;

                            voucherDetailVM.CompanyCurrencyRate = 1 / (voucherDetailVM.CompanyGroupCurrencyAmount / voucherDetailVM.CompanyCurrencyAmount);
                            voucherDetailVM.CompanyCurrencyConversion = voucherDetailVM.CompanyGroupCurrencyConversion / voucherDetailVM.CompanyCurrencyRate;
                        }
                        else
                        {
                            voucherDetailVM.CompanyCurrencyRate = voucherDetailVM.CompanyCurrencyAmount / voucherDetailVM.Amount;
                            voucherDetailVM.CompanyCurrencyConversion = 1 / voucherDetailVM.CompanyCurrencyRate;
                            voucherDetailVM.CompanyFromCurrencyId = voucherDetailVM.CurrencyId;
                        }
                    }

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            DrAmount = voucherDetailVM.DrAmount,
                            CrAmount = voucherDetailVM.CrAmount,
                            FromCurrencyId = voucherDetailVM.ToCurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            ToCurrencyId = voucherDetailVM.ToCurrencyId,
                            ToCurrencyConversion = 1 / voucherDetailVM.CompanyCurrencyRate,
                            ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate
                        });
                    }

                    // Set company Group currency.
                    if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                    {
                        if (voucherDetailVM.CompanyGroupCurrencyAmount <= 0)
                            throw new CustomException($"{voucherDetailVM.GLGeneralInfoName} GL {companyGroupCurrencyId} {voucherDetailVM.TrnType} amount must have to greater than zero!");
                        else if (voucherDetailVM.CurrencyId == companyGroupCurrencyId && voucherDetailVM.Amount != voucherDetailVM.CompanyGroupCurrencyAmount)
                            throw new CustomException($"{voucherDetailVM.GLGeneralInfoName} GL {companyGroupCurrencyId} {voucherDetailVM.TrnType} amount and Transaction amount is not equal!");
                        _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            VoucherId = voucher.Id,
                            VoucherDetailId = voucherDetail.Id,
                            AddedBy = voucherDetail.AddedBy,
                            AddedDate = voucherDetail.AddedDate,
                            AddedFromIP = voucherDetail.AddedFromIP,
                            DrAmount = voucherDetailVM.DrAmount,
                            CrAmount = voucherDetailVM.CrAmount,
                            FromCurrencyId = voucherDetailVM.CompanyGroupFromCurrencyId,
                            ParallelCurrencyId = voucherDetailVM.CompanyGroupCurrencyId,
                            ToCurrencyId = voucherDetailVM.ToCurrencyId,
                            ToCurrencyConversion = voucherDetailVM.CompanyGroupCurrencyConversion,
                            ToCurrencyRate = voucherDetailVM.CompanyGroupCurrencyRate
                        });
                    }
                }

                // Update OpeningBalance IsPark flag
                var openingBalanceIds = voucherDetailVMList.Where(r => r.OpeningBalanceId != null).Select(r => r.OpeningBalanceId).Distinct();

                foreach (var openingBalanceId in openingBalanceIds)
                {
                    var openingBalance = Find(openingBalanceId);
                    openingBalance.IsPark = false;
                    openingBalance.VoucherId = voucher.Id;
                    AuditService.UpdatedLog(openingBalance);
                    UpdateGraph(openingBalance);


                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, voucherVM.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        private List<Dictionary<string, object>> GeOBPartyType(string OpeningBalanceId)
        {
            var CmdText = @" select distinct PartyType from trn.OpeningBalanceDetail ob where ob.OpeningBalanceid='"+ OpeningBalanceId + "'";
            return _sqlRepository.GetDataCollection(CmdText);
        }

        private List<Dictionary<string, object>> GeOBVendorInvoice(string OpeningBalanceId)
        {
            var CmdText = @" select * from trn.OpeningBalanceDetail OB where OB.OpeningBalanceid='" + OpeningBalanceId + "' and OB.PartyType='" + PartyType.Vendor.ToString()+ "' AND OB.CrAmount>0 ";
            return _sqlRepository.GetDataCollection(CmdText);
        }

        public Dictionary<string, object> CheckingFiscalYearPeriod(string companyId, DateTime postingDate)
        {
            var sql = @"SELECT CFY.FiscalYearId, FY.FiscalYearName, CFYP.FiscalYearPeriodId, FYP.PeriodName, FYP.StartDate, FYP.EndDate, CFYP.IsBudgetLocked
                        , CFYP.IsTransationLocked, CFYP.IsExchangeRateConfirmed, FY.YearPrefix
                        FROM [SCS].[CompanyFiscalYearPeriod] AS CFYP
                        INNER JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=CFYP.FiscalYearPeriodId
                        INNER JOIN [SCS].[CompanyFiscalYear] AS CFY ON CFY.Id=CFYP.CompanyFiscalYearId
                        INNER JOIN [SCS].[FiscalYear] AS FY ON FY.Id=CFY.FiscalYearId
                        WHERE CFY.CompanyId='" + companyId + "' AND FYP.StartDate <= '" + postingDate.ToDbDate() + "' AND FYP.EndDate >= '" + postingDate.ToDbDate() + "' ";
            var data = _sqlRepository.GetData(sql);
            if (null == data || data.Count == 0)
                throw new CustomException(ResourcesCore.FYNotFound);
            if (Convert.ToBoolean(data["IsTransationLocked"].ToString()))
                throw new CustomException($"This period ({data["PeriodName"]}) transation is locked! Please contact with Administrator.");
            if (!Convert.ToBoolean(data["IsExchangeRateConfirmed"].ToString()))
                throw new CustomException($"This period ({data["PeriodName"]}) exchange rate is not confirmed! Please contact with Administrator.");
            return data;
        }
        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }
        
        public string PostOpeningBalanceJournal(VoucherViewModel voucherVM)
        {
            var flag = false;
            
            var fiscalYear = _fiscalYearService.Find("5");
            try
            {

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Voucher TABLE
                voucherVM.IsPark = false;

                var voucher = new Voucher
                {
                    IsPark = false,
                    VoucherDate = DateTime.Now,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    CurrencyId = voucherVM.CurrencyId,
                    PostingDate = voucherVM.PostingDate,
                    DocRefNo = voucherVM.DocRefNo,
                    DocDate = voucherVM.DocDate,
                    Narration = voucherVM.Narration
                };
                voucher.CurrencyId = voucherVM.CurrencyId;
                voucher.SourceType = SourceType.OpeningBalance.ToString();
                _voucherService.InsertVoucher(voucher, fiscalYear.YearPrefix);

                var advancePk = _advanceService.GetMaxNumber(nameof(Advance), PKGeneratorEnum.Yearly, null, voucherVM.VoucherDate);
                var invoicePk = _invoiceService.GetMaxNumber(nameof(Invoice), PKGeneratorEnum.Yearly, null, voucherVM.VoucherDate);
                var securityDepositPk = _securityDepositService.GetMaxNumber(nameof(SecurityDeposit), PKGeneratorEnum.Yearly, null, voucherVM.VoucherDate);
                var employeePayablePk = _employeePayableService.GetMaxNumber();
                var financingPk = _financingService.GetMaxNumber();

                var currentRecord = 0;
                //DataView dvopeningBalanceDetailData;
                //DataSet voucherdetailTemp;
                //DataSet voucherDetailCurrencyTemp;
                //ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                //con.OpenDataSetThroughAdapter("select * from [TRN].[VoucherDetail] where 1=2", out voucherdetailTemp, false, "1");
                //con.OpenDataSetThroughAdapter("select * from [TRN].[ProductionBulletinTemplateMaster] where 1=2", out voucherDetailCurrencyTemp, false, "1");

                DataTable openingBalanceDetailData = _sqlRepository.GetDataTable("SELECT obd.*,NULL ModelState,NULL GLGeneralInfo,NULL OpeningBalance,NULL OpeningBalanceDetailCurrency,NULL MaterialMasterOpeningBalanceDetail FROM [TRN].OpeningBalanceDetail obd left join trn.VoucherDetail vd on vd.OpeningBalanceDetailId=obd.id WHERE obd.OpeningBalanceid='" + voucherVM.Id + "' ");
                List<OpeningBalanceDetail> voucherDetailVMList1 = openingBalanceDetailData.ToList<OpeningBalanceDetail>();

                //List<OpeningBalanceDetail> objBList = voucherDetailVMList1.Select(x => x.Copy()).ToList();

                List<VoucherDetailViewModel> voucherDetailVMList = voucherDetailVMList1.Select(s => new VoucherDetailViewModel
                {
                    VoucherId = s.Id,
                    PlantId = s.PlantId,
                    OpeningBalanceDetailId = s.Id,
                    OpeningBalanceId = s.OpeningBalanceId,
                    Id = s.Id,
                    EntityId = s.EntityId,
                    PartyType = s.PartyType,
                    EmployeeId = s.EmployeeId,
                    PartyId = s.PartyId,
                    PartyPlantId = s.PartyPlantId,
                    GLGeneralInfoId = s.GLGeneralInfoId,
                    BudgetMasterId = s.BudgetMasterId,
                    CashMasterId = s.CashMasterId,
                    BankMasterId = s.BankMasterId,
                    ActivityId = s.ActivityId,
                    CurrencyId = s.CurrencyId,
                    DrAmount = s.DrAmount,
                    CrAmount = s.CrAmount,
                    DocDate = s.DocDate,
                    DocRefNo = s.DocRefNo,
                    Narration = s.Narration,
                    FAType = s.FAType,
                    MaterialMasterOpeningBalanceDetailId = s.MaterialMasterOpeningBalanceDetailId,
                    CompanyId=s.CompanyId,
                    RefId=s.RefId,
                    BaseOnDueDate=s.BaseOnDueDate,
                    BaseNoOfDays=s.BaseNoOfDays,
                    RepaymentStartDate=s.RepaymentStartDate,
                    LifeOfYear=s.LifeOfYear,
                    NoOfInstallmentPerYear=s.NoOfInstallmentPerYear,
                    TransactionTypeId=s.TransactionTypeId,
                    FixedAssetMasterId=s.FixedAssetMasterId,
                    LoanOpeningBalanceDetailId=s.LoanOpeningBalanceDetailId,
                    SecurityDepositDetailId=s.LoanOpeningBalanceDetailId,
                    InvestmentOpeningBalanceDetailId=s.InvestmentOpeningBalanceDetailId,
                    EquityOpeningBalanceDetailId=s.EquityOpeningBalanceDetailId,
                    BankCurrencyId=s.BankCurrencyId,
                    CashCurrencyId=s.CashCurrencyId,
                    BankAmount=s.BankAmount,
                    MaterialMasterId=s.MaterialMasterId
                }).ToList();

                // name_list2.AddRange(name_list1.ToArray());

                //List<VoucherDetailViewModel> clonedList = voucherDetailVMList1.GetClone();

                //List<VoucherDetailViewModel> voucherDetailVMList = new List<VoucherDetailViewModel>();

                //var voucherDetailVMList = _sqlRepository.GetModelCollection<VoucherDetailViewModel>(@"SELECT * FROM [TRN].OpeningBalanceDetail WHERE OpeningBalanceid='"+voucherVM.Id+ "' ");

                //for (int i = 0; i < openingBalanceDetailData.Rows.Count; i++)
                //{
                //    DataRow drDetailDestination = voucherdetailTemp.Tables[0].NewRow();
                //    CopyRow(openingBalanceDetailData.Rows[i], ref drDetailDestination);
                //    drDetailDestination["Id"] = voucher.Id + "-" + (i + 1);
                //    drDetailDestination["VoucherId"] = voucher.Id;
                //    voucherdetailTemp.Tables[0].Rows.Add(drDetailDestination);


                //    dvopeningBalanceDetailData = new DataView(openingBalanceDetailData);

                //    dvopeningBalanceDetailData.RowFilter = "MaterialMasterOpeningBalanceDetailId='" + openingBalanceDetailData.Rows[i]["MaterialMasterOpeningBalanceDetailId"].ToString() + "'";

                //    if (dvopeningBalanceDetailData.Count>0)
                //    {
                //        DataTable openingBalanceMaterialData = _sqlRepository.GetDataTable("SELECT * FROM [TRN].MaterialMasterOpeningBalanceDetail WHERE Id in ('"+ openingBalanceDetailData.Rows[i]["MaterialMasterOpeningBalanceDetailId"].ToString() +"')");

                //        if (openingBalanceMaterialData.Rows.Count>0)
                //        {
                //            var ob = _openingBalanceRepository.Find(materialOB.OpeningBalanceId);
                //            ob.IsPark = false;
                //            ob.IsPosted = true;
                //            _openingBalanceRepository.Update(ob);
                //            var grn = _inventoryReceiveRepository.Query(r => r.OpeningBalanceId == materialOB.OpeningBalanceId).Select().FirstOrDefault();
                //            if (grn != null)
                //            {
                //                grn.Status = "Posting";
                //                grn.IsApproved = true;
                //                //grn.CheckedBy = "";
                //                grn.CheckedByStatus = "Checked";
                //                //grn.AuthorizedBy = "";
                //                grn.AuthorizedByStatus = "Approval";
                //                _inventoryReceiveRepository.Update(grn);
                //            }
                //        }
                //    }
                //}






                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    // Set to currency
                    voucherDetailVM.ToCurrencyId = companyCurrencyId;

                    // INSERT INTO VOUCHER DETAIL
                    var voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucherVM.Id,
                        PlantId = voucherDetailVM.PlantId,
                        OpeningBalanceDetailId = voucherDetailVM.Id,
                        EntityId = voucherDetailVM.IsOB ? voucherDetailVM.EntityId : voucherVM.EntityId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        PartyType = voucherDetailVM.PartyType,
                        EmployeeId = voucherDetailVM.EmployeeId,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        CashMasterId = voucherDetailVM.CashMasterId,
                        BankMasterId = voucherDetailVM.BankMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        DrAmount = voucherDetailVM.DrAmount,
                        CrAmount = voucherDetailVM.CrAmount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        AddedBy = voucherVM.AddedBy,
                        AddedDate = voucherVM.AddedDate,
                        AddedFromIP = voucherVM.AddedFromIP,
                        //FixedAssetMasterId= voucherDetailVM.FixedAssetMasterId,
                        FAType = voucherDetailVM.FAType,
                    };
                    currentRecord++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentRecord);

                    if (voucherDetailVM.MaterialMasterOpeningBalanceDetailId != null)
                    {
                        var materialOB = _materialMasterOpeningBalanceDetailRepository.Find(voucherDetailVM.MaterialMasterOpeningBalanceDetailId);
                        if (materialOB != null)
                        {
                            var ob = _openingBalanceRepository.Find(materialOB.OpeningBalanceId);
                            ob.IsPark = false;
                            ob.IsPosted = true;
                            _openingBalanceRepository.Update(ob);
                            var grn = _inventoryReceiveRepository.Query(r => r.OpeningBalanceId == materialOB.OpeningBalanceId).Select().FirstOrDefault();
                            if (grn != null)
                            {
                                grn.Status = "Posting";
                                grn.IsApproved = true;
                                //grn.CheckedBy = "";
                                grn.CheckedByStatus = "Checked";
                                //grn.AuthorizedBy = "";
                                grn.AuthorizedByStatus = "Approval";
                                _inventoryReceiveRepository.Update(grn);
                            }
                        }
                    }
                    if (voucherDetailVM.LoanOpeningBalanceDetailId != null)
                    {
                        var loanOB = _openingBalanceDetailRepository.Find(voucherDetailVM.LoanOpeningBalanceDetailId);
                        if (loanOB != null)
                        {
                            var ob = _openingBalanceRepository.Query(r => r.Id == loanOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financing = _financingRepository.Query(r => r.OpeningBalanceId == ob.Id).Select().FirstOrDefault();
                            var financingDetail = _financingDetailRepository.Query(r => r.FinancingId == financing.Id).Select().FirstOrDefault();
                            if (ob != null)
                            {
                                ob.VoucherId = voucher.Id;
                                ob.ModelState = ModelState.Modified;
                                _openingBalanceRepository.Update(ob);
                            }
                            if (financing != null)
                            {
                                financing.VoucherId = voucher.Id;
                                financing.IsPosted = true;
                                financing.IsPark = false;
                                financing.ModelState = ModelState.Modified;
                                _financingRepository.Update(financing);
                            }
                            if (financingDetail != null)
                            {
                                voucherDetail.FinancingDetailId = financingDetail.Id;
                                voucherDetail.BankMasterId = financing.BankMasterId;
                                if (voucherDetail.BankMasterId != null)
                                {
                                    var glTransactionDetail = new GLTransactionDetail
                                    {
                                        VoucherDetailId = voucherDetail.Id,
                                        BankMasterId = voucherDetail.BankMasterId,
                                        CashMasterId = voucherDetail.CashMasterId,
                                        SourceType = voucher.SourceType,
                                        DrAmount = voucherDetail.DrAmount,
                                        CrAmount = voucherDetail.CrAmount,
                                    };
                                    _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                                }
                            }
                        }
                    }
                    if (voucherDetailVM.SecurityOpeningBalanceDetailId != null)
                    {
                        var securityOB = _openingBalanceDetailRepository.Find(voucherDetailVM.SecurityOpeningBalanceDetailId);
                        if (securityOB != null)
                        {
                            var ob = _openingBalanceRepository.Query(r => r.Id == securityOB.OpeningBalanceId).Select().FirstOrDefault();
                            var securityDeposit = _securityDepositRepository.Query(r => r.OpeningBalanceId == securityOB.OpeningBalanceId).Select().FirstOrDefault();
                            var securityDepositDetail = _securityDepositDetailRepository.Query(r => r.SecurityDepositId == securityDeposit.Id).Select().FirstOrDefault();

                            if (ob != null)
                            {
                                ob.VoucherId = voucher.Id;
                                ob.ModelState = ModelState.Modified;
                                _openingBalanceRepository.Update(ob);
                            }
                            if (securityDeposit != null)
                            {
                                securityDeposit.VoucherId = voucher.Id;
                                securityDeposit.IsPark = false;
                                securityDeposit.ModelState = ModelState.Modified;
                                _securityDepositRepository.Update(securityDeposit);
                            }
                            if (securityDepositDetail != null)
                            {
                                voucherDetail.SecurityDepositDetailId = securityDepositDetail.Id;
                                voucherDetail.BankMasterId = securityDeposit.BankMasterId;
                                if (voucherDetail.BankMasterId != null)
                                {
                                    var glTransactionDetail = new GLTransactionDetail
                                    {
                                        VoucherDetailId = voucherDetail.Id,
                                        BankMasterId = voucherDetail.BankMasterId,
                                        CashMasterId = voucherDetail.CashMasterId,
                                        SourceType = voucher.SourceType,
                                        DrAmount = voucherDetail.DrAmount,
                                        CrAmount = voucherDetail.CrAmount,
                                    };
                                    _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                                }
                            }
                        }
                    }
                    if (voucherDetailVM.EquityOpeningBalanceDetailId != null)
                    {
                        var equityOB = _openingBalanceDetailRepository.Find(voucherDetailVM.EquityOpeningBalanceDetailId);
                        if (equityOB != null)
                        {
                            var ob = _openingBalanceRepository.Query(r => r.Id == equityOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financing = _financingRepository.Query(r => r.Id == equityOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financingDetail = _financingDetailRepository.Query(r => r.Id == financing.Id).Select().FirstOrDefault();
                            if (ob != null)
                            {
                                ob.VoucherId = voucher.Id;
                                ob.ModelState = ModelState.Modified;
                                _openingBalanceRepository.Update(ob);
                            }
                            if (financing != null)
                            {
                                financing.VoucherId = voucher.Id;
                                financing.IsPosted = true;
                                financing.IsPark = false;
                                financing.ModelState = ModelState.Modified;
                                _financingRepository.Update(financing);
                            }
                            if (financingDetail != null)
                            {
                                voucherDetail.FinancingDetailId = financingDetail.Id;
                            }
                        }
                    }
                    if (voucherDetailVM.InvestmentOpeningBalanceDetailId != null)
                    {
                        var investmentiOB = _openingBalanceDetailRepository.Find(voucherDetailVM.InvestmentOpeningBalanceDetailId);
                        if (investmentiOB != null)
                        {
                            var ob = _openingBalanceRepository.Query(r => r.Id == investmentiOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financing = _financingRepository.Query(r => r.Id == investmentiOB.OpeningBalanceId).Select().FirstOrDefault();
                            var financingDetail = _financingDetailRepository.Query(r => r.Id == financing.Id).Select().FirstOrDefault();
                            if (ob != null)
                            {
                                ob.VoucherId = voucher.Id;
                                ob.ModelState = ModelState.Modified;
                                _openingBalanceRepository.Update(ob);
                            }
                            if (financing != null)
                            {
                                financing.VoucherId = voucher.Id;
                                financing.IsPosted = true;
                                financing.IsPark = false;
                                financing.ModelState = ModelState.Modified;
                                _financingRepository.Update(financing);
                            }
                            if (financingDetail != null)
                            {
                                voucherDetail.FinancingDetailId = financingDetail.Id;
                                voucherDetail.BankMasterId = financing.BankMasterId;
                                if (voucherDetail.BankMasterId != null)
                                {
                                    var glTransactionDetail = new GLTransactionDetail
                                    {
                                        VoucherDetailId = voucherDetail.Id,
                                        BankMasterId = voucherDetail.BankMasterId,
                                        CashMasterId = voucherDetail.CashMasterId,
                                        SourceType = voucher.SourceType,
                                        DrAmount = voucherDetail.DrAmount,
                                        CrAmount = voucherDetail.CrAmount,
                                    };
                                    _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(voucherDetailVM.OpeningBalanceId))
                    {
                        //Customer/Vendor invoice
                        if ((voucherDetailVM.PartyType == PartyType.Customer.ToString() && voucherDetailVM.TransactionTypeId == null && voucherDetailVM.CrAmount == 0) ||
                           voucherDetailVM.PartyType == PartyType.Vendor.ToString() && voucherDetailVM.TransactionTypeId == null && voucherDetailVM.DrAmount == 0)
                        {
                            var invoice = new Invoice
                            {
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucher.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucher.Narration,
                                BaseNoOfDays = voucherDetailVM.BaseNoOfDays,
                                BaseOnDueDate = voucherDetailVM.BaseOnDueDate,
                                Amount = voucherDetailVM.Amount,
                                SourceType = voucher.SourceType,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            if (voucherDetailVM.PartyType == PartyType.Customer.ToString())
                            {
                                invoice.SourceType = SourceType.CustomerInvoice.ToString();
                                invoice.Amount = voucherDetailVM.DrAmount;
                            }
                            if (voucherDetailVM.PartyType == PartyType.Vendor.ToString())
                            {
                                invoice.SourceType = SourceType.VendorInvoice.ToString();
                                invoice.Amount = voucherDetailVM.CrAmount;
                            }
                            invoicePk.MaxNumber++;
                            _invoiceService.InsertInvoice(invoice, invoicePk.MaxNumber);

                            var invoiceDetail = new InvoiceDetail
                            {
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = invoice.Amount,
                                NetAmount = invoice.Amount
                            };
                            _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, 1);
                            // Set InvoiceDetail Id to voucher detail.
                            voucherDetail.InvoiceDetailId = invoiceDetail.Id;
                        }
                        //Customer/Vendor Advance
                        if ((voucherDetailVM.PartyType == PartyType.Customer.ToString() && voucherDetailVM.TransactionTypeId == null && voucherDetailVM.DrAmount == 0) ||
                         voucherDetailVM.PartyType == PartyType.Vendor.ToString() && voucherDetailVM.TransactionTypeId == null && voucherDetailVM.CrAmount == 0)
                        {
                            advancePk.MaxNumber++;
                            var advance = new Advance
                            {
                                Id = voucher.VoucherDate.Year + advancePk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                EmployeeTransactionTypeId = voucherDetailVM.EmployeeTransactionTypeId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucher.Narration,
                                Amount = voucherDetailVM.Amount,
                                SourceType = voucherDetailVM.SourceType,
                                PaymentSource = voucherDetailVM.PaymentSource,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                IsPosted = true,
                                AdvanceNo = voucher.VoucherNo
                            };


                            if (voucherDetailVM.PartyType == PartyType.Customer.ToString())
                            {
                                advance.SourceType = SourceType.CustomerAdvance.ToString();
                                advance.Amount = voucherDetailVM.CrAmount;

                            }
                            if (voucherDetailVM.PartyType == PartyType.Vendor.ToString())
                            {
                                advance.SourceType = SourceType.VendorAdvance.ToString();
                                advance.Amount = voucherDetailVM.DrAmount;
                            }
                            _advanceService.InsertGraph(advance);


                            var advanceDetail = new AdvanceDetail
                            {
                                Id = _advanceService.MakeAdvanceDetailPK(advance.Id, 1),
                                AdvanceId = advance.Id,
                                CompanyId = advance.CompanyId,
                                PlantId = advance.PlantId,
                                EmployeeId = advance.EmployeeId,
                                Archive = advance.Archive,
                                IsWrittenOff = advance.IsWrittenOff,
                                ModelState = advance.ModelState,
                                Narration = advance.Narration,
                                NetAmount = advance.Amount,
                                PartyId = advance.PartyId,
                                PartyPlantId = advance.PartyPlantId,
                                PartyType = advance.PartyType,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = advance.Amount,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            };
                            _advanceService.InsertAdvanceDetail(advanceDetail);
                            // Set Advance detail to voucher detail.
                            voucherDetail.AdvanceDetailId = advanceDetail.Id;
                        }
                        //Bank/Cash
                       else if (voucherDetailVM.PartyType == PartyType.Bank.ToString() || voucherDetailVM.PartyType == PartyType.Cash.ToString())
                        {
                            var glTransactionDetail = new GLTransactionDetail
                            {
                                VoucherDetailId = voucherDetail.Id,
                                BankMasterId = voucherDetail.BankMasterId,
                                CashMasterId = voucherDetail.CashMasterId,
                                SourceType = voucher.SourceType,
                                DrAmount = voucherDetail.DrAmount,
                                CrAmount = voucherDetail.CrAmount,
                            };
                            // Set BankMasterId/CashMasterId in voucher detail.
                            voucherDetail.BankMasterId = voucherDetailVM.BankMasterId;
                            voucherDetail.CashMasterId = voucherDetailVM.CashMasterId;
                            _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetail);
                        }
                        //Employee Payable
                        else if (voucherDetailVM.PartyType == PartyType.Employee.ToString() && voucherDetailVM.TransactionTypeId != null && voucherDetailVM.DrAmount == 0)
                        {
                            employeePayablePk.MaxNumber++;
                            var employeePayable = new EmployeePayable
                            {
                                Id = voucher.VoucherDate.Year + employeePayablePk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                EmployeeTransactionTypeId = voucherDetailVM.TransactionTypeId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                Amount = voucherDetailVM.CrAmount,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                SourceType = SourceType.EmployeePayable.ToString(),
                                PartyType = PartyType.Employee.ToString(),
                                VoucherDate = voucher.VoucherDate
                            };
                            _employeePayableService.InsertEmployeePayable(employeePayable);

                            var employeePayableDetail = new EmployeePayableDetail
                            {
                                EmployeePayableId = employeePayable.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount,
                                NetAmount = employeePayable.Amount
                            };
                            _employeePayableService.InsertEmployeePayableDetail(employeePayable, employeePayableDetail, 1);

                            // Set InvoiceDetail Id to voucher detail.
                            voucherDetail.EmployeePayableDetailId = employeePayableDetail.Id;
                            voucherDetail.PartyType = employeePayable.PartyType;
                        }
                        //Employee Advance
                        else if (voucherDetailVM.PartyType == PartyType.Employee.ToString() && voucherDetailVM.TransactionTypeId != null && voucherDetailVM.CrAmount == 0)
                        {
                            advancePk.MaxNumber++;
                            var advance = new Advance
                            {
                                Id = voucher.VoucherDate.Year + advancePk.MaxNumber.ToString(),
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                VoucherId = voucher.Id,
                                VoucherTypeId = voucher.VoucherTypeId,
                                PartyType = voucherDetailVM.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                EmployeeId = voucherDetailVM.EmployeeId,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                EmployeeTransactionTypeId = voucherDetailVM.TransactionTypeId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                VoucherDate = voucher.VoucherDate,
                                PostingDate = voucher.PostingDate,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucher.Narration,
                                Amount = voucherDetailVM.DrAmount,
                                SourceType = voucher.SourceType,
                                PaymentSource = voucherDetailVM.PaymentSource,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                IsPosted = true,
                                AdvanceNo = voucher.VoucherNo
                            };
                            if (voucherDetailVM.PartyType == PartyType.Employee.ToString())
                                advance.SourceType = SourceType.EmployeeAdvance.ToString();
                            _advanceService.InsertGraph(advance);

                            // INSERT INTO AdvanceDetail
                            var advanceDetail = new AdvanceDetail
                            {
                                Id = _advanceService.MakeAdvanceDetailPK(advance.Id, 1),
                                AdvanceId = advance.Id,
                                CompanyId = advance.CompanyId,
                                PlantId = advance.PlantId,
                                EmployeeId = advance.EmployeeId,
                                Archive = advance.Archive,
                                IsWrittenOff = advance.IsWrittenOff,
                                ModelState = advance.ModelState,
                                Narration = advance.Narration,
                                NetAmount = advance.Amount,
                                PartyId = advance.PartyId,
                                PartyPlantId = advance.PartyPlantId,
                                PartyType = advance.PartyType,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = advance.Amount,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            };
                            _advanceService.InsertAdvanceDetail(advanceDetail);
                            // Set Advance detail to voucher detail.
                            voucherDetail.AdvanceDetailId = advanceDetail.Id;
                        }
                        //Inter Transaction
                        else if (voucherDetailVM.PartyType == PartyType.InterTransaction.ToString())
                        {
                            advancePk.MaxNumber++;
                            var interTransaction = new Advance
                            {
                                Id = voucher.VoucherDate.Year + advancePk.MaxNumber.ToString(),
                                AddedBy = voucherDetail.AddedBy,
                                AddedDate = voucherDetail.AddedDate,
                                AddedFromIP = voucherDetail.AddedFromIP,
                                Amount = voucherDetailVM.Amount,
                                CompanyGroupId = voucher.CompanyGroupId,
                                CompanyId = voucher.CompanyId,
                                PlantId = voucher.PlantId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                EntityId = voucherDetailVM.EntityId,
                                FinancingTypeId = voucherDetailVM.FinancingTypeId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                VoucherId = voucher.Id,
                                VoucherDate = voucher.VoucherDate,
                                IsInterTransaction = true,
                                Narration = voucherDetailVM.Narration,
                                OpeningBalanceId = voucherDetailVM.OpeningBalanceId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                SourceType = voucher.SourceType,
                                PostingDate = voucher.PostingDate,
                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                VoucherTypeId = voucher.VoucherTypeId,
                                IsPosted = true
                            };
                            // _interTransactionService.InsertGraph(interTransaction);

                            // INSERT INTO AdvanceDetail
                            var interTransactionDetail = new AdvanceDetail
                            {
                                AdvanceId = interTransaction.Id,
                                CompanyId = interTransaction.CompanyId,
                                PlantId = interTransaction.PlantId,
                                Archive = interTransaction.Archive,
                                IsWrittenOff = interTransaction.IsWrittenOff,
                                ModelState = interTransaction.ModelState,
                                Narration = interTransaction.Narration,
                                NetAmount = interTransaction.Amount,
                                PartyId = interTransaction.PartyId,
                                PartyPlantId = interTransaction.PartyPlantId,
                                PartyType = interTransaction.PartyType,
                                GLGeneralInfoId = voucherDetail.GLGeneralInfoId,
                                BudgetMasterId = voucherDetail.BudgetMasterId,
                                ActivityId = voucherDetail.ActivityId,
                                Amount = interTransaction.Amount,
                                AddedBy = interTransaction.AddedBy,
                                AddedDate = interTransaction.AddedDate,
                                AddedFromIP = interTransaction.AddedFromIP
                            };
                            //_interTransactionService.InsertInterTransactionDetail(interTransaction, interTransactionDetail, 1);
                            // Set InterTransaction detail to voucher detail.
                            voucherDetail.InterTransactionDetailId = interTransactionDetail.Id;
                        }
                    }

                    
                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            DrAmount = voucherDetailVM.DrAmount,
                            CrAmount = voucherDetailVM.CrAmount,
                            FromCurrencyId = voucherDetailVM.ToCurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            ToCurrencyId = voucherDetailVM.ToCurrencyId,
                            ToCurrencyConversion = 1 ,
                            ToCurrencyRate = 1
                        });
                    }

                }

                // Update OpeningBalance IsPark flag
                var openingBalanceIds = voucherDetailVMList.Where(r => r.OpeningBalanceId != null).Select(r => r.OpeningBalanceId).Distinct();

                foreach (var openingBalanceId in openingBalanceIds)
                {
                    var openingBalance = Find(openingBalanceId);
                    openingBalance.IsPark = false;
                    openingBalance.VoucherId = voucher.Id;
                    AuditService.UpdatedLog(openingBalance);
                    UpdateGraph(openingBalance);


                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, voucherVM.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public GridModel GetMaterialMasterOB(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount,OB.MaterialStorageId
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE OBDC.GLType='FA' AND C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.IsFinancial=1  AND OB.Id NOT IN (SELECT OpeningBalanceId FROM TRN.OpeningBalanceDetail WHERE MaterialMasterOpeningBalanceDetailId in 
								( SELECT Id FROM [TRN].[MaterialMasterOpeningBalanceDetail]) and MaterialMasterOpeningBalanceDetailId<>'')
AND OB.IsPark=1 AND OB.SourceType='" + sourceType + "' AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetNonFinancialMaterialMasterOB(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount,OB.MaterialStorageId
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE OBDC.GLType='FA' AND C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.IsFinancial=0 AND OB.IsPark=1 AND OB.SourceType='" + sourceType + "' AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public List<Dictionary<string, object>> GetMaterialMasterOBGL(string openingBalanceId, string companyGroupId, string companyId, string plantId)
        {
            var CmdText = @"
                        SELECT OB.SourceType, ACT.BalanceType,MM.MaterialGroupMasterId, BM.COAId, MM.Id MaterialMasterId,bm.RefNo,MM.UserName AS MaterialMasterName,MGM.UserName MaterialGroupMasterName
                        ,MMA.StandardName ArticleName,FOBD.ArticleId
						, FOBD.AssetGLId AS GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName, FOBD.AssetBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, FOBD.AssetActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountDr, 0 CompanyCurrencyAmountCr,FOBD.OpeningBalanceId,FOBD.Id MaterialMasterOpeningBalanceDetailId
                        ,SUM(FOBD.Quantity) Quantity,FOBD.LotNumber,FOBD.Diameter,FOBD.[Type]
                        FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.AssetGLId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=FOBD.AssetBudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=FOBD.AssetActivityId
                        LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                        LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
						LEFT JOIN [MST].MaterialMaster AS MM ON MM.Id=FOBD.MaterialMasterId
						LEFT JOIN [MST].MaterialMasterArticle AS MMA ON MMA.Id=FOBD.ArticleId
						LEFT JOIN [MST].MaterialGroupMaster AS MGM ON MGM.Id=MM.MaterialGroupMasterId
                        JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=FOBD.OpeningBalanceId
                        LEFT JOIN (
	                        SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                        FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='FA'
                        ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        WHERE OB.IsPark=1 AND OB.IsPosted=1 AND OB.Id='" + openingBalanceId + "' AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + @"' AND OB.PlantId='" + plantId + @"'
                        --AND FOBD.Id NOT IN(SELECT OBD.MaterialMasterOpeningBalanceDetailId FROM [TRN].OpeningBalanceDetail AS OBD WHERE OBD.MaterialMasterOpeningBalanceDetailId <> '')
                       
                        GROUP BY OB.SourceType, ACT.BalanceType, FOBD.AssetGLId, GGI.AccountCode, GGI.UserName, FOBD.AssetBudgetMasterId, B.Code, B.UserName, FOBD.AssetActivityId, A.Code, A.UserName
                        ,MM.MaterialGroupMasterId, BM.COAId, MM.Id ,bm.RefNo,MM.UserName ,MGM.UserName ,FOBD.OpeningBalanceId,FOBD.Id,MMA.StandardName,FOBD.ArticleId,FOBD.LotNumber,FOBD.Diameter,FOBD.[Type]
						--ORDER BY 1, 5, 8, 11, 2;
                        ";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        public string DeleteOBDetailRow(OpeningBalanceDetail OBDetailVM)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var ob = _openingBalanceRepository.Find(OBDetailVM.OpeningBalanceId);
                if (ob.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var obDetailAppen = new System.Text.StringBuilder();
                var obDetailsql = "";
                obDetailsql = @"DELETE FROM [TRN].[OpeningBalanceDetailCurrency] WHERE OpeningBalanceDetailId='"+ OBDetailVM.Id + "'";
                obDetailAppen.Append(obDetailsql);
                obDetailsql = @"DELETE FROM [TRN].[OpeningBalanceDetail] WHERE Id='"+ OBDetailVM.Id + "'";
                obDetailAppen.Append(obDetailsql);
                _sqlRepository.ExecuteSqlCommand(obDetailAppen.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                return "Deleted";
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        #endregion

        #region FixedAsset

        public List<Dictionary<string, object>> GetMaterialMasterOpeningBalanceDetailList(string companyId, string plantId, string openinngBalanceId)
        {
            try
            {
                var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + "',@plantId VARCHAR(10)='" + plantId + @"';
                        SELECT 
									distinct IM.Id as InventoryReceivedId
									,FOBD.Id,IRDD.Id InventoryReceiveDetailId, FOBD.OpeningBalanceId,AGL.AccountCode+' - '+AGL.UserName AS AssetGLName, ACGL.AccountCode+' - '+ACGL.UserName AS AccDepreciation
							       ,FOBD.AccumulatedDepreciationGLId,FOBD.AccumulatedDepreciationBudgetMasterId,FOBD.AccumulatedDepreciationActivityId,AB.UserName BudgetName,AC.UserName AssetActivityName,BM.BudgetCategoryId,BM.BudgetSubCategoryId,ACB.UserName ACUBudgetName
								   ,FOBD.FixedAssetMasterId,FOBD.AssetBudgetMasterId,FOBD.AssetActivityId, FAM.UserName AS FixedAssetMasterName, FOBD.MaterialMasterId, FOBD.BaseUOMId, UOM.UserName AS BaseUoM, FOBD.AssetGLId, FOBD.AccumulatedDepreciationGLId, FOBD.CurrencyId, FOBD.Quantity,FOBD.Quantity QuantityOld
                                    ,CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.FACompanyCurrencyRate, CC.FACompanyCurrencyAmount,CC.FACompanyCurrencyAmount FACompanyCurrencyAmountOld, ADCC.ADCompanyCurrencyRate, ADCC.ADCompanyCurrencyAmount,
                                    GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.FACompanyGroupCurrencyRate, GC.FACompanyGroupCurrencyAmount, ADGC.ADCompanyGroupCurrencyRate, ADGC.ADCompanyGroupCurrencyAmount,
                                    HC.HardCurrencyId, HC.HardFromCurrencyId, HC.FAHardCurrencyRate, HC.FAHardCurrencyAmount, ADHC.ADHardCurrencyRate, ADHC.ADHardCurrencyAmount
									,CCD.DirectQuantity,CCD.FACompanyCurrencyDirectRate, CCD.FACompanyCurrencyDirectAmount
									,CCID.InDirectQuantity,CCID.FACompanyCurrencyInDirectRate, CCID.FACompanyCurrencyInDirectAmount
									,GCD.DirectQuantity,GCD.FACompanyGroupCurrencyDirectRate, GCD.FACompanyGroupCurrencyDirectAmount
									,GCID.InDirectQuantity,GCID.FACompanyGroupCurrencyInDirectRate, GCID.FACompanyGroupCurrencyInDirectAmount
									,HCD.DirectQuantity,HCD.FAHardCurrencyDirectRate, HCD.FAHardCurrencyDirectAmount
									,HCID.InDirectQuantity,HCID.FAHardCurrencyInDirectRate, HCID.FAHardCurrencyInDirectAmount
									,ADCCD.DirectQuantity,ADCCD.ADCompanyCurrencyDirectRate, ADCCD.ADCompanyCurrencyDirectAmount
									,ADCCID.InDirectQuantity,ADCCID.ADCompanyCurrencyInDirectRate, ADCCID.ADCompanyCurrencyInDirectAmount
									,ADGCD.DirectQuantity,ADGCD.ADCompanyGroupCurrencyDirectRate, ADGCD.ADCompanyGroupCurrencyDirectAmount
									,ADGCID.InDirectQuantity,ADGCID.ADCompanyGroupCurrencyInDirectRate, ADGCID.ADCompanyGroupCurrencyInDirectAmount
									,ADHCD.DirectQuantity,ADHCD.ADHardCurrencyDirectRate, ADHCD.ADHardCurrencyDirectAmount
									,ADHCID.InDirectQuantity,ADHCID.ADHardCurrencyInDirectRate, ADHCID.ADHardCurrencyInDirectAmount
									,FOBD.MaterialMasterId, MM.UserName MaterialMasterName,FOBD.ArticleId, MMA.StandardName ArticleName,FOBD.MaterialStorageId,FOBD.FirstCharacteristicsId,FOBD.FirstCharacteristicsValueId,FOBD.SecondCharacteristicsId,FOBD.SecondCharacteristicsValueId,FOBD.ThirdCharacteristicsId,FOBD.ThirdCharacteristicsValueId
									,IR.Id InventoryReceivedId
									,IR.MaterialStorageId
                                    , FOBD.FirstCharacteristicsId
									, FC.UserName AS FirstCharacteristics
									, FOBD.FirstCharacteristicsValueId
									, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue

									, FOBD.SecondCharacteristicsId
									, FC.UserName AS SecondCharacteristics
									, FOBD.SecondCharacteristicsValueId
									, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue

									, FOBD.ThirdCharacteristicsId
									, FC.UserName AS ThirdCharacteristics
									, FOBD.ThirdCharacteristicsValueId
									, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue,FOBD.LotNumber,FOBD.Diameter,FOBD.Type
                                    FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                                    LEFT JOIN [TRN].[OpeningBalance] AS FOB ON FOBD.OpeningBalanceId=FOB.Id
                                    --LEFT JOIN MST.MaterialMaster AS FAT ON FAT.Id = FOBD.MaterialMasterId
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
                                    LEFT JOIN HKP.Characteristics AS FC ON FOBD.FirstCharacteristicsId=FC.Id
									LEFT JOIN HKP.Characteristics AS SC ON FOBD.SecondCharacteristicsId=SC.Id
									LEFT JOIN HKP.Characteristics AS TC ON FOBD.ThirdCharacteristicsId=TC.Id
									LEFT JOIN HKP.CharacteristicsValue AS FCV ON FOBD.FirstCharacteristicsValueId=FCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS SCV ON FOBD.SecondCharacteristicsValueId=SCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS TCV ON FOBD.ThirdCharacteristicsValueId=TCV.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyRate, OBDC.Amount AS FACompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                                    ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyRate, OBDC.Amount AS FACompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                                    ) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FAHardCurrencyRate, OBDC.Amount AS FAHardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                                    ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyCurrencyRate, OBDC.Amount AS ADCompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADCC ON ADCC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyGroupCurrencyRate, OBDC.Amount AS ADCompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADGC ON ADGC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADHardCurrencyRate, OBDC.Amount AS ADHardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADHC ON ADHC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									--DirectInDirect
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyDirectRate, OBDC.Amount AS FACompanyCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS CCD ON CCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyInDirectRate, OBDC.Amount AS FACompanyCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS CCID ON CCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyDirectRate, OBDC.Amount AS FACompanyGroupCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS GCD ON GCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyInDirectRate, OBDC.Amount AS FACompanyGroupCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS GCID ON GCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity DirectQuantity,OBDC.ToCurrencyRate AS FAHardCurrencyDirectRate, OBDC.Amount AS FAHardCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS HCD ON HCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
																		 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity InDirectQuantity,OBDC.ToCurrencyRate AS FAHardCurrencyInDirectRate, OBDC.Amount AS FAHardCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS HCID ON HCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyCurrencyDirectRate, OBDC.Amount AS ADCompanyCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADCCD ON ADCCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyCurrencyInDirectRate, OBDC.Amount AS ADCompanyCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADCCID ON ADCCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyGroupCurrencyDirectRate, OBDC.Amount AS ADCompanyGroupCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADGCD ON ADGCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyGroupCurrencyInDirectRate, OBDC.Amount AS ADCompanyGroupCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADGCID ON ADGCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity DirectQuantity,OBDC.ToCurrencyRate AS ADHardCurrencyDirectRate, OBDC.Amount AS ADHardCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADHCD ON ADHCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
																		 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity InDirectQuantity,OBDC.ToCurrencyRate AS ADHardCurrencyInDirectRate, OBDC.Amount AS ADHardCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADHCID ON ADHCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT JOIN TRN.InventoryReceive IR ON IR.OpeningBalanceId=FOB.Id
									--LEFT JOIN (select Distinct IM.Id,IM.MaterialMasterId  
									--		from TRN.InventoryReceiveDetail IRD  									
									--		left JOIN TRN.InventoryReceive IR ON IR.id = IRD.InventoryReceiveId
									--		LEft JOIn TRN.InventoryMaterial IM ON IM.id = IRD.InventoryMaterialId 
									--		--LEFT JOIN  trn.InventoryReceiveDetail IRD1 On IRD1.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									--		ANd IR.OpeningBalanceId='20191')IRD1  ON IRD1.MaterialMasterId = MM.Id 

									left join trn.InventoryReceiveDetail IRDD ON IRDD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									Left join trn.InventoryMaterial IM ON IM.Id=IRDD.InventoryMaterialId
									WHERE FOB.CompanyId=@companyId AND FOB.PlantId=@plantId AND FOB.Id='" + openinngBalanceId + "'  Order BY FOBD.Id ASC";// ORDER BY AGL.AccountCode,AB.UserName,FAM.UserName,ACGL.AccountCode,ACB.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void InsertFixedAsset(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                Check(openingBalance);

                // Duplicate Bank checking.
                var duplicates = materialMasterOpeningBalanceDetailVMList.GroupBy(x => new { x.AssetGLId, x.AssetBudgetMasterId, x.AssetActivityId }).Where(x => x.Count() > 1).Select(x => x.Key);
                if (duplicates.Any())
                {
                    var bm = _glRepository.Find(duplicates.FirstOrDefault().AssetGLId);
                    throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "GL (" + bm.UserName + ")"));
                }
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO OpeningBalance TABLE
                openingBalance.Id = GetOpeningBalancePK(openingBalance);
                openingBalance.Archive = false;
                openingBalance.EmployeeTransactionTypeId = null;
                openingBalance.FinancingTypeId = null;
                openingBalance.IsPark = true;
                openingBalance.IsPosted = true;
                openingBalance.SourceType = SourceType.FixedAsset.ToString();
                openingBalance.UpdatedBy = null;
                openingBalance.UpdatedDate = null;
                openingBalance.UpdatedFromIP = null;
                openingBalance.VoucherId = null;
                InsertGraph(openingBalance);

                var fixedAssetOBDetailList = (from fd in _materialMasterOpeningBalanceDetailRepository.Query().Select()
                                              join f in _openingBalanceRepository.Query(r => r.CompanyGroupId == openingBalance.CompanyGroupId && r.CompanyId == openingBalance.CompanyId).Select() on fd.OpeningBalanceId equals f.Id
                                              select fd).ToList();
                var currentRecord = 0;
                foreach (var materialMasterOpeningBalanceDetailVM in materialMasterOpeningBalanceDetailVMList)
                {
                    currentRecord++;
                    // INSERT INTO OPENING BALANCE DETAIL
                    var sql = @"SELECT TOP(1) FAGL.* FROM [HKP].[FixedAssetMasterGL] AS FAGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=FAGL.COAId
								INNER JOIN HKP.FixedAssetMasterBudgetTag FAT ON FAGL.FixedAssetMasterId=FAT.FixedAssetMasterId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND FAT.FixedAssetMasterId='" + materialMasterOpeningBalanceDetailVM.FixedAssetMasterId + "'";
                    var glTemp = _openingBalanceRepository.SqlQuery<FixedAssetMasterGL>(sql).FirstOrDefault();
                    if (null == glTemp || string.IsNullOrEmpty(glTemp.AccumulatedDepreciationGLId))
                        throw new CustomException($"This {materialMasterOpeningBalanceDetailVM.FixedAssetMasterName} Fixed Asset Account Determinate GL not Found!");

                    var materialMasterOpeningBalanceDetail = new MaterialMasterOpeningBalanceDetail
                    {
                        AccumulatedDepreciationActivityId = glTemp.AccumulatedDepreciationActivityId,
                        AccumulatedDepreciationBudgetMasterId = glTemp.AccumulatedDepreciationBudgetMasterId,
                        AccumulatedDepreciationGLId = glTemp.AccumulatedDepreciationGLId,
                        AddedBy = openingBalance.AddedBy,
                        AddedDate = openingBalance.AddedDate,
                        AddedFromIP = openingBalance.AddedFromIP,
                        AssetActivityId = materialMasterOpeningBalanceDetailVM.AssetActivityId,
                        AssetBudgetMasterId = materialMasterOpeningBalanceDetailVM.AssetBudgetMasterId,
                        AssetGLId = materialMasterOpeningBalanceDetailVM.AssetGLId,
                        Id = MakePK(openingBalance.Id, currentRecord, 4),
                        PlantId = openingBalance.PlantId,
                        EntityId = openingBalance.EntityId,
                        ModelState = ModelState.Added,
                        OpeningBalanceId = openingBalance.Id,
                        Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                        CurrencyId = materialMasterOpeningBalanceDetailVM.CurrencyId,
                        BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                        MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
                        FixedAssetMasterId = materialMasterOpeningBalanceDetailVM.FixedAssetMasterId,
                        Quantity = materialMasterOpeningBalanceDetailVM.Quantity
                    };

                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, materialMasterOpeningBalanceDetailVM);
                    // Set company currency.
                    if (!string.IsNullOrWhiteSpace(companyCurrencyId))
                    {
                        //DetailCurrency
                        //FA
                        _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "FA",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 1, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
                        });
                        // AD
                        _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "AD",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 2, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                        });
                        //Detail Direct InDirect
                        //FA
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "FA",
                            Type = "Direct",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 3, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectRate
                        });
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "FA",
                            Type = "InDirect",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 4, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectRate
                        });
                        // AD
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "AD",
                            Type = "Direct",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 5, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectRate
                        });
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "AD",
                            Type = "InDirect",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 6, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectRate
                        });
                    }

                    // Set company group currency.
                    if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
                    {
                        //Detail Currency
                        // FA
                        _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                            GLType = "FA",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 7, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
                        });

                        // AD
                        _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                            GLType = "AD",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 8, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate
                        });
                        //Detail Direct InDirect
                        //FA
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "FA",
                            Type = "Direct",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 9, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectRate
                        });
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "FA",
                            Type = "InDirect",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 10, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectRate
                        });
                        //AD
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "AD",
                            Type = "Direct",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 11, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectRate
                        });
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                            GLType = "AD",
                            Type = "InDirect",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 12, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectRate
                        });
                    }

                    // Set company hard currency.
                    if (!string.IsNullOrWhiteSpace(hardCurrencyId))
                    {
                        // FA
                        _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                            GLType = "FA",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 13, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
                        });

                        // AD
                        _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                            GLType = "AD",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 14, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
                        });

                        //Detail Direct InDirect
                        //FA
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                            GLType = "FA",
                            Type = "Direct",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 15, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectRate
                        });
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                            GLType = "FA",
                            Type = "InDirect",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 16, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectRate
                        });
                        //AD
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                            GLType = "AD",
                            Type = "Direct",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 17, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectRate
                        });
                        _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                        {
                            AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                            AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                            AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                            Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                            Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectAmount,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                            GLType = "AD",
                            Type = "InDirect",
                            Id = MakePK(materialMasterOpeningBalanceDetail.Id, 18, 2),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                            ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectConversion,
                            ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                            ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectRate
                        });
                    }
                    _materialMasterOpeningBalanceDetailRepository.Insert(materialMasterOpeningBalanceDetail);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateFixedAsset(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                ModifyCheck(openingBalance.Id);
                Check(openingBalance);

                var duplicates = materialMasterOpeningBalanceDetailVMList.GroupBy(x => new { x.AssetGLId, x.AssetBudgetMasterId, x.AssetActivityId }).Where(x => x.Count() > 1).Select(x => x.Key);
                if (duplicates.Any())
                {
                    var bm = _glRepository.Find(duplicates.FirstOrDefault().AssetGLId);
                    throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "GL (" + bm.UserName + ")"));
                }

                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO OpeningBalance TABLE
                openingBalance.Archive = false;
                openingBalance.EmployeeTransactionTypeId = null;
                openingBalance.FinancingTypeId = null;
                openingBalance.IsPark = true;
                openingBalance.IsPosted = true;
                openingBalance.SourceType = SourceType.FixedAsset.ToString();
                openingBalance.VoucherId = null;
                UpdateGraph(openingBalance);

                var fixedAssetOBDetailList = (from fd in _materialMasterOpeningBalanceDetailRepository.Query().Select()
                                              join f in _openingBalanceRepository.Query(r => r.CompanyGroupId == openingBalance.CompanyGroupId && r.CompanyId == openingBalance.CompanyId).Select() on fd.OpeningBalanceId equals f.Id
                                              select fd).ToList();
                var faDetailIds = fixedAssetOBDetailList.Select(t => t.Id);
                var fixedAssetOBDetailCurrencyList = _materialMasterOpeningBalanceDetailCurrencyRepository.Query(r => faDetailIds.Contains(r.MaterialMasterOpeningBalanceDetailId)).Select().ToList();
                var fixedAssetOBDetailDirectList = _materialMasterOpeningBalanceDetailDirectIndirectRepository.Query(r => faDetailIds.Contains(r.MaterialMasterOpeningBalanceDetailId)).Select().ToList();
                var currentRecord = _openingBalanceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.MaterialMasterOpeningBalanceDetail WHERE OpeningBalanceId='{openingBalance.Id}'").First();
                foreach (var materialMasterOpeningBalanceDetailVM in materialMasterOpeningBalanceDetailVMList)
                {
                    var sql = @"SELECT TOP(1) FAGL.* FROM [HKP].[FixedAssetMasterGL] AS FAGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=FAGL.COAId
								INNER JOIN HKP.FixedAssetMasterBudgetTag FAT ON FAGL.FixedAssetMasterId=FAT.FixedAssetMasterId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND FAT.FixedAssetMasterId='" + materialMasterOpeningBalanceDetailVM.FixedAssetMasterId + "'";
                    var glTemp = _openingBalanceRepository.SqlQuery<FixedAssetMasterGL>(sql).FirstOrDefault();
                    if (null == glTemp || string.IsNullOrEmpty(glTemp.AccumulatedDepreciationGLId))
                        throw new CustomException($"This {materialMasterOpeningBalanceDetailVM.FixedAssetMasterName} Fixed Asset Account Determinate GL not Found!");

                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, materialMasterOpeningBalanceDetailVM);

                    if (string.IsNullOrEmpty(materialMasterOpeningBalanceDetailVM.Id))
                    {
                        currentRecord++;
                        var MaterialMasterOpeningBalanceDetail = new MaterialMasterOpeningBalanceDetail
                        {
                            AccumulatedDepreciationActivityId = glTemp.AccumulatedDepreciationActivityId,
                            AccumulatedDepreciationBudgetMasterId = glTemp.AccumulatedDepreciationBudgetMasterId,
                            AccumulatedDepreciationGLId = glTemp.AccumulatedDepreciationGLId,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            AssetActivityId = materialMasterOpeningBalanceDetailVM.AssetActivityId,
                            AssetBudgetMasterId = materialMasterOpeningBalanceDetailVM.AssetBudgetMasterId,
                            AssetGLId = materialMasterOpeningBalanceDetailVM.AssetGLId,
                            Id = MakePK(openingBalance.Id, currentRecord, 4),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            FixedAssetMasterId = materialMasterOpeningBalanceDetailVM.FixedAssetMasterId,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            CurrencyId = materialMasterOpeningBalanceDetailVM.CurrencyId,
                            BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
                            Quantity = materialMasterOpeningBalanceDetailVM.Quantity
                        };

                        // Set company currency.
                        if (!string.IsNullOrWhiteSpace(companyCurrencyId))
                        {
                            //Detail Currency
                            //FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 1, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
                            });
                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 2, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                            });
                            //Detail Direct InDirect
                            //FA
                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "FA",
                                Type = "Direct",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 3, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectRate
                            });

                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "FA",
                                Type = "InDirect",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 4, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectRate
                            });
                            // AD
                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "AD",
                                Type = "Direct",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 5, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectRate
                            });

                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "AD",
                                Type = "InDirect",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 6, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectRate
                            });
                        }

                        // Set company group currency.
                        if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 7, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
                            });

                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 8, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate
                            });
                            //Detail Direct InDirect
                            //FA
                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "FA",
                                Type = "Direct",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 9, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectRate
                            });

                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "FA",
                                Type = "InDirect",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 10, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectRate
                            });

                            //AD
                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "AD",
                                Type = "Direct",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 11, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectRate
                            });

                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "AD",
                                Type = "InDirect",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 12, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectRate
                            });
                        }

                        // Set company hard currency.
                        if (!string.IsNullOrWhiteSpace(hardCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 13, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
                            });

                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 14, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
                            });

                            //Detail Direct InDirect
                            //FA
                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "FA",
                                Type = "Direct",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 15, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectRate
                            });

                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "FA",
                                Type = "InDirect",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 16, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectRate
                            });
                            //AD
                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "AD",
                                Type = "Direct",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 17, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectRate
                            });

                            _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "AD",
                                Type = "InDirect",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 18, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectRate
                            });
                        }
                        _materialMasterOpeningBalanceDetailRepository.Insert(MaterialMasterOpeningBalanceDetail);
                    }
                    else
                    {
                        var materialMasterOpeningBalanceDetailDb = fixedAssetOBDetailList.First(r => r.Id == materialMasterOpeningBalanceDetailVM.Id);
                        materialMasterOpeningBalanceDetailDb.AccumulatedDepreciationActivityId = glTemp.AccumulatedDepreciationActivityId;
                        materialMasterOpeningBalanceDetailDb.AccumulatedDepreciationBudgetMasterId = glTemp.AccumulatedDepreciationBudgetMasterId;
                        materialMasterOpeningBalanceDetailDb.AccumulatedDepreciationGLId = glTemp.AccumulatedDepreciationGLId;
                        materialMasterOpeningBalanceDetailDb.Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount;
                        materialMasterOpeningBalanceDetailDb.BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId;
                        materialMasterOpeningBalanceDetailDb.AssetActivityId = materialMasterOpeningBalanceDetailVM.AssetActivityId;
                        materialMasterOpeningBalanceDetailDb.AssetBudgetMasterId = materialMasterOpeningBalanceDetailVM.AssetBudgetMasterId;
                        materialMasterOpeningBalanceDetailDb.AssetGLId = materialMasterOpeningBalanceDetailVM.AssetGLId;
                        materialMasterOpeningBalanceDetailDb.Quantity = materialMasterOpeningBalanceDetailVM.Quantity;
                        materialMasterOpeningBalanceDetailDb.UpdatedBy = openingBalance.UpdatedBy;
                        materialMasterOpeningBalanceDetailDb.UpdatedDate = openingBalance.UpdatedDate;
                        materialMasterOpeningBalanceDetailDb.UpdatedFromIP = openingBalance.UpdatedFromIP;
                        _materialMasterOpeningBalanceDetailRepository.Update(materialMasterOpeningBalanceDetailDb);

                        // Set company currency.
                        if (!string.IsNullOrWhiteSpace(companyCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            var ccDetailFA = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId && r.GLType == "FA");
                            if (null != ccDetailFA)
                            {
                                ccDetailFA.Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount;
                                ccDetailFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId;
                                ccDetailFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId;
                                ccDetailFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion;
                                ccDetailFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                ccDetailFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate;
                                ccDetailFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                ccDetailFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                ccDetailFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(ccDetailFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "FA",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 1, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
                                });
                            }

                            // AD
                            var ccDetailAD = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId && r.GLType == "AD");
                            if (null != ccDetailAD)
                            {
                                ccDetailAD.Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount;
                                ccDetailAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId;
                                ccDetailAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId;
                                ccDetailAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion;
                                ccDetailAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                ccDetailAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate;
                                ccDetailAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                ccDetailAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                ccDetailAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(ccDetailAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "AD",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 2, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                                });
                            }
                            //Detail Direct InDirect
                            // FA
                            var directDetailFA = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId && r.GLType == "FA" && r.Type == "Direct");
                            if (null != directDetailFA)
                            {
                                directDetailFA.Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity;
                                directDetailFA.Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectAmount;
                                directDetailFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId;
                                directDetailFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId;
                                directDetailFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectConversion;
                                directDetailFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                directDetailFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectRate;
                                directDetailFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                directDetailFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                directDetailFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(directDetailFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "FA",
                                    Type = "Direct",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 3, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyDirectRate
                                });
                            }
                            var inDirectDetailFA = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId && r.GLType == "FA" && r.Type == "InDirect");
                            if (null != directDetailFA)
                            {
                                inDirectDetailFA.Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity;
                                inDirectDetailFA.Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectAmount;
                                inDirectDetailFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId;
                                inDirectDetailFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId;
                                inDirectDetailFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectConversion;
                                inDirectDetailFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                inDirectDetailFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectRate;
                                inDirectDetailFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                inDirectDetailFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                inDirectDetailFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(inDirectDetailFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "FA",
                                    Type = "InDirect",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 4, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyInDirectRate
                                });
                            }
                            // AD
                            var directDetailAD = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId && r.GLType == "AD" && r.Type == "Direct");
                            if (null != directDetailAD)
                            {
                                directDetailAD.Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity;
                                directDetailAD.Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectAmount;
                                directDetailAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId;
                                directDetailAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId;
                                directDetailAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectConversion;
                                directDetailAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                directDetailAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectRate;
                                directDetailAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                directDetailAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                directDetailAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(directDetailAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "AD",
                                    Type = "Direct",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 5, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyDirectRate
                                });
                            }
                            var inDirectDetailAD = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId && r.GLType == "AD" && r.Type == "InDirect");
                            if (null != inDirectDetailAD)
                            {
                                inDirectDetailAD.Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity;
                                inDirectDetailAD.Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectAmount;
                                inDirectDetailAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId;
                                inDirectDetailAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId;
                                inDirectDetailAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectConversion;
                                inDirectDetailAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                inDirectDetailAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectRate;
                                inDirectDetailAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                inDirectDetailAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                inDirectDetailAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(inDirectDetailAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "AD",
                                    Type = "InDirect",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 6, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyInDirectRate
                                });
                            }
                        }

                        // Set company Group currency.
                        if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            var cgDetailFA = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyGroupCurrencyId && r.GLType == "FA");
                            if (null != cgDetailFA)
                            {
                                cgDetailFA.Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount;
                                cgDetailFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetailFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                cgDetailFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion;
                                cgDetailFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                cgDetailFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate;
                                cgDetailFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                cgDetailFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                cgDetailFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(cgDetailFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                    GLType = "FA",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 7, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
                                });
                            }

                            // AD
                            var cgDetailAD = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyGroupCurrencyId && r.GLType == "AD");
                            if (null != cgDetailAD)
                            {
                                cgDetailAD.Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount;
                                cgDetailAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetailAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                cgDetailAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion;
                                cgDetailAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                cgDetailAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate;
                                cgDetailAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                cgDetailAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                cgDetailAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(cgDetailAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "AD",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 8, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                                });
                            }
                            //Detail Direct InDirect
                            // FA
                            var cgDetailDirectFA = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyGroupCurrencyId && r.GLType == "FA" && r.Type == "Direct");
                            if (null != cgDetailDirectFA)
                            {
                                cgDetailDirectFA.Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity;
                                cgDetailDirectFA.Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectAmount;
                                cgDetailDirectFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetailDirectFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                cgDetailDirectFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectConversion;
                                cgDetailDirectFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                cgDetailDirectFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectRate;
                                cgDetailDirectFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                cgDetailDirectFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                cgDetailDirectFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(cgDetailDirectFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                    GLType = "FA",
                                    Type = "Direct",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 9, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyDirectRate
                                });
                            }
                            var cgDetailInDirectFA = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyGroupCurrencyId && r.GLType == "FA" && r.Type == "InDirect");
                            if (null != cgDetailInDirectFA)
                            {
                                cgDetailInDirectFA.Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity;
                                cgDetailInDirectFA.Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectAmount;
                                cgDetailInDirectFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetailInDirectFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                cgDetailInDirectFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectConversion;
                                cgDetailInDirectFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                cgDetailInDirectFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectRate;
                                cgDetailInDirectFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                cgDetailInDirectFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                cgDetailInDirectFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(cgDetailInDirectFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                    GLType = "FA",
                                    Type = "InDirect",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 10, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyInDirectRate
                                });
                            }
                            // AD
                            var cgDetailDirectAD = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyGroupCurrencyId && r.GLType == "AD" && r.Type == "Direct");
                            if (null != cgDetailDirectAD)
                            {
                                cgDetailDirectAD.Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity;
                                cgDetailDirectAD.Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectAmount;
                                cgDetailDirectAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetailDirectAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                cgDetailDirectAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectConversion;
                                cgDetailDirectAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                cgDetailDirectAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectRate;
                                cgDetailDirectAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                cgDetailDirectAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                cgDetailDirectAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(cgDetailDirectAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                    GLType = "AD",
                                    Type = "Direct",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 11, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyDirectRate
                                });
                            }
                            var cgDetailInDirectAD = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyGroupCurrencyId && r.GLType == "AD" && r.Type == "InDirect");
                            if (null != cgDetailInDirectAD)
                            {
                                cgDetailInDirectAD.Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity;
                                cgDetailInDirectAD.Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectAmount;
                                cgDetailInDirectAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetailInDirectAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                cgDetailInDirectAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectConversion;
                                cgDetailInDirectAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                cgDetailInDirectAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectRate;
                                cgDetailInDirectAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                cgDetailInDirectAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                cgDetailInDirectAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(cgDetailInDirectAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                    GLType = "AD",
                                    Type = "InDirect",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 12, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyInDirectRate
                                });
                            }
                        }

                        // Set company hard currency.
                        if (!string.IsNullOrWhiteSpace(hardCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            var hcDetailFA = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == hardCurrencyId && r.GLType == "FA");
                            if (null != hcDetailFA)
                            {
                                hcDetailFA.Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount;
                                hcDetailFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                hcDetailFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                hcDetailFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion;
                                hcDetailFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                hcDetailFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate;
                                hcDetailFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                hcDetailFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                hcDetailFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(hcDetailFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                    GLType = "FA",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 13, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
                                });
                            }

                            // AD
                            var hcDetailAD = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == hardCurrencyId && r.GLType == "AD");
                            if (null != hcDetailAD)
                            {
                                hcDetailAD.Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount;
                                hcDetailAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                hcDetailAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                hcDetailAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion;
                                hcDetailAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                hcDetailAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate;
                                hcDetailAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                hcDetailAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                hcDetailAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(hcDetailAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                    GLType = "AD",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 14, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
                                });
                            }
                            //Detail Direct InDirect
                            // FA
                            var hcDetailDirectFA = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == hardCurrencyId && r.GLType == "FA" && r.Type == "Direct");
                            if (null != hcDetailDirectFA)
                            {
                                hcDetailDirectFA.Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity;
                                hcDetailDirectFA.Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectAmount;
                                hcDetailDirectFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                hcDetailDirectFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                hcDetailDirectFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectConversion;
                                hcDetailDirectFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                hcDetailDirectFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectRate;
                                hcDetailDirectFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                hcDetailDirectFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                hcDetailDirectFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(hcDetailDirectFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                    GLType = "FA",
                                    Type = "Direct",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 15, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyDirectRate
                                });
                            }
                            var hcDetailInDirectFA = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == hardCurrencyId && r.GLType == "FA" && r.Type == "InDirect");
                            if (null != hcDetailInDirectFA)
                            {
                                hcDetailInDirectFA.Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity;
                                hcDetailInDirectFA.Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectAmount;
                                hcDetailInDirectFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                hcDetailInDirectFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                hcDetailInDirectFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectConversion;
                                hcDetailInDirectFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                hcDetailInDirectFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectRate;
                                hcDetailInDirectFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                hcDetailInDirectFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                hcDetailInDirectFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(hcDetailInDirectFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Quantity = materialMasterOpeningBalanceDetailVM.InDirectQuantity,
                                    Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                    GLType = "FA",
                                    Type = "InDirect",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 16, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyInDirectRate
                                });
                            }
                            // AD
                            var hcDetailDirectAD = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == hardCurrencyId && r.GLType == "AD" && r.Type == "Direct");
                            if (null != hcDetailDirectAD)
                            {
                                hcDetailDirectAD.Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity;
                                hcDetailDirectAD.Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectAmount;
                                hcDetailDirectAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                hcDetailDirectAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                hcDetailDirectAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectConversion;
                                hcDetailDirectAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                hcDetailDirectAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectRate;
                                hcDetailDirectAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                hcDetailDirectAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                hcDetailDirectAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(hcDetailDirectAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                    GLType = "AD",
                                    Type = "Direct",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 17, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectRate
                                });
                            }
                            var hcDetailInDirectAD = fixedAssetOBDetailDirectList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == hardCurrencyId && r.GLType == "AD" && r.Type == "InDirect");
                            if (null != hcDetailInDirectAD)
                            {
                                hcDetailInDirectAD.Quantity = materialMasterOpeningBalanceDetailVM.DirectQuantity;
                                hcDetailInDirectAD.Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectAmount;
                                hcDetailInDirectAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                hcDetailInDirectAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                hcDetailInDirectAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectConversion;
                                hcDetailInDirectAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                hcDetailInDirectAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyDirectRate;
                                hcDetailInDirectAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                hcDetailInDirectAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                hcDetailInDirectAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Update(hcDetailInDirectAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailDirectIndirectRepository.Insert(new MaterialMasterOpeningBalanceDetailDirectIndirect
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                    GLType = "AD",
                                    Type = "InDirect",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 18, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyInDirectRate
                                });
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteFixedAsset(string id)
        {
            var flag = false;
            try
            {
                ModifyCheck(id.ToString());
                _unitOfWork.BeginTransaction();
                flag = true;
                var sql = $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] WHERE OpeningBalanceId='{id}';" +
                    $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] WHERE OpeningBalanceId='{id}';" +
                    $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetail] WHERE OpeningBalanceId='{id}';" +
                    $"DELETE FROM [TRN].[OpeningBalance] WHERE Id='{id}'";
                _openingBalanceDetailRepository.ExecuteSqlCommand(sql);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #region Material Master
        public List<Dictionary<string, object>> GetMaterialMasterOBDetailList(string companyId, string plantId, string openinngBalanceId)
        {
            try
            {
                var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + "',@plantId VARCHAR(10)='" + plantId + @"';
                        SELECT 
									distinct IRDD.InventoryMaterialId as InventoryReceivedId,IR.Id InventoryReceivedId,
									FOBD.Id, FOBD.OpeningBalanceId,AGL.AccountCode+' - '+AGL.UserName AS AssetGLName
							       ,FOBD.AccumulatedDepreciationGLId,FOBD.AccumulatedDepreciationBudgetMasterId,FOBD.AccumulatedDepreciationActivityId,AB.UserName BudgetName,AC.UserName AssetActivityName,BM.BudgetCategoryId,BM.BudgetSubCategoryId
								   ,FOBD.FixedAssetMasterId,FOBD.AssetBudgetMasterId,FOBD.AssetActivityId, FOBD.MaterialMasterId, FOBD.BaseUOMId, UOM.UserName AS BaseUoM, FOBD.AssetGLId, FOBD.AccumulatedDepreciationGLId, FOBD.CurrencyId, FOBD.Quantity,FOBD.Quantity QuantityOld
									,FOBD.MaterialMasterId, MM.UserName MaterialMasterName,FOBD.ArticleId, MMA.StandardName ArticleName,FOBD.MaterialStorageId,FOBD.FirstCharacteristicsId,FOBD.FirstCharacteristicsValueId,FOBD.SecondCharacteristicsId,FOBD.SecondCharacteristicsValueId,FOBD.ThirdCharacteristicsId,FOBD.ThirdCharacteristicsValueId
									
									,IR.MaterialStorageId
                                    , FOBD.FirstCharacteristicsId
									, FC.UserName AS FirstCharacteristics
									, FOBD.FirstCharacteristicsValueId
									, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue

									, FOBD.SecondCharacteristicsId
									, FC.UserName AS SecondCharacteristics
									, FOBD.SecondCharacteristicsValueId
									, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue

									, FOBD.ThirdCharacteristicsId
									, FC.UserName AS ThirdCharacteristics
									, FOBD.ThirdCharacteristicsValueId
									, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue
                                    FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                                    LEFT JOIN [TRN].[OpeningBalance] AS FOB ON FOBD.OpeningBalanceId=FOB.Id
									LEFT JOIN [SCS].[UnitOfMeasurement] AS UOM ON UOM.Id=FOBD.BaseUOMId
									LEFT JOIN HKP.GLGeneralInfo AGL ON FOBD.AssetGLId=AGL.Id
									LEFT JOIN MST.BudgetMaster BM ON FOBD.AssetBudgetMasterId=BM.Id
									LEFT JOIN HKP.Budget AB ON BM.BudgetId=AB.Id
                                    LEFT JOIN HKP.Activity AC ON FOBD.AssetActivityId=AC.Id
									LEFT JOIN MST.MaterialMaster MM ON FOBD.MaterialMasterId=MM.Id
									LEFT JOIN MST.MaterialMasterArticle MMA ON FOBD.ArticleId = MMA.Id
                                    LEFT JOIN HKP.Characteristics AS FC ON FOBD.FirstCharacteristicsId=FC.Id
									LEFT JOIN HKP.Characteristics AS SC ON FOBD.SecondCharacteristicsId=SC.Id
									LEFT JOIN HKP.Characteristics AS TC ON FOBD.ThirdCharacteristicsId=TC.Id
									LEFT JOIN HKP.CharacteristicsValue AS FCV ON FOBD.FirstCharacteristicsValueId=FCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS SCV ON FOBD.SecondCharacteristicsValueId=SCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS TCV ON FOBD.ThirdCharacteristicsValueId=TCV.Id
                                    
                                    LEFT JOIN TRN.InventoryReceive IR ON IR.OpeningBalanceId=FOB.Id
									left join trn.InventoryReceiveDetail IRDD ON IRDD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									WHERE FOB.CompanyId=@companyId AND FOB.PlantId=@plantId AND FOB.Id='" + openinngBalanceId + "'  ";// ORDER BY AGL.AccountCode,AB.UserName,FAM.UserName,ACGL.AccountCode,ACB.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        //public void InsertMaterialMaster(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList)
        //{
        //    var flagStatus = false;
        //    var flag = false;
        //    var rdBuilder = new System.Text.StringBuilder();
        //    var builderSql = "";
        //    try
        //    {
        //        Check(openingBalance);

        //        _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        // INSERT INTO OpeningBalance TABLE
        //        openingBalance.Id = GetOpeningBalancePK(openingBalance);
        //        openingBalance.Archive = false;
        //        openingBalance.EmployeeTransactionTypeId = null;
        //        openingBalance.FinancingTypeId = null;
        //        openingBalance.IsPark = true;
        //        openingBalance.IsPosted = true;
        //        openingBalance.SourceType = SourceType.MaterialMaster.ToString();
        //        openingBalance.UpdatedBy = null;
        //        openingBalance.UpdatedDate = null;
        //        openingBalance.UpdatedFromIP = null;
        //        openingBalance.VoucherId = null;
        //        InsertGraph(openingBalance);

        //        var invReceivePk = GetAutoNumber(nameof(InventoryReceive), PKGeneratorEnum.Auto, null, openingBalance.PostingDate);
        //        var inventoryMaterialPk = GetMaxNumber(nameof(InventoryMaterial), PKGeneratorEnum.Auto, null, openingBalance.PostingDate);
        //        var MaterialId = Convert.ToInt32(inventoryMaterialPk.MaxNumber) - 1;
        //        // Insert Inventory Receive
        //        var inventoryReceive = new InventoryReceive
        //        {
        //            Id = invReceivePk,
        //            OpeningBalanceId = openingBalance.Id,
        //            CompanyGroupId = openingBalance.CompanyGroupId,
        //            CompanyId = openingBalance.CompanyId,
        //            PlantId = openingBalance.PlantId,
        //            MaterialStorageId = openingBalance.MaterialStorageId,
        //            CurrencyId = materialMasterOpeningBalanceDetailVMList.FirstOrDefault().CompanyCurrencyId,
        //            DocRefNo = openingBalance.DocRefNo,
        //            DocDate = openingBalance.DocDate,
        //            GateEntryNo = null,
        //            EntryDate = openingBalance.AddedDate,
        //            GRNDate = openingBalance.PostingDate,//AddedDate,
        //            FixedAssetOrInventory = nameof(Inventory),
        //            PODepended = false,
        //            AlongwithInvoice = false,
        //            BaseNoOfDays = 0,
        //            BaseCurrencyId = materialMasterOpeningBalanceDetailVMList.FirstOrDefault().CompanyCurrencyId,
        //            IsNonCreditable = false,
        //            ToCurrencyRate = 0,
        //            IsTaxApplicable = false,
        //            AddedBy = openingBalance.AddedBy,
        //            AddedDate = openingBalance.AddedDate,
        //            AddedFromIP = openingBalance.AddedFromIP,
        //            GRNType = "OpeningBalance"

        //        };
        //        _inventoryReceiveRepository.Insert(inventoryReceive);

        //        var fixedAssetOBDetailList = (from fd in _materialMasterOpeningBalanceDetailRepository.Query().Select()
        //                                      join f in _openingBalanceRepository.Query(r => r.CompanyGroupId == openingBalance.CompanyGroupId && r.CompanyId == openingBalance.CompanyId).Select() on fd.OpeningBalanceId equals f.Id
        //                                      select fd).ToList();
        //        var currentRecord = 0;
        //        var currentReceiveDetailId = _inventoryReceiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId='{inventoryReceive.Id}'").First();

        //        var materialIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.MaterialMasterId);
        //        var articleIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.ArticleId);
        //        var firstValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.FirstCharacteristicsValueId);
        //        var secondValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.SecondCharacteristicsValueId);
        //        var thirdValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.ThirdCharacteristicsValueId);
        //        //var LotNums = materialMasterOpeningBalanceDetailVMList.Select(t => t.LotNumber);
        //        //var Diameters = materialMasterOpeningBalanceDetailVMList.Select(t => t.Diameter);
        //        //var Types = materialMasterOpeningBalanceDetailVMList.Select(t => t.Type);
        //        //LotNums.Contains(t.LotNumber) && 
        //        //Diameters.Contains(t.Diameter) && 
        //        //Types.Contains(t.Type) && 
        //        var materialDbData = _inventoryMaterialRepository.Query(t => materialIds.Contains(t.MaterialMasterId) && articleIds.Contains(t.ArticleId) &&
        //                         firstValueIds.Contains(t.FirstCharacteristicsValueId) && 
        //                         secondValueIds.Contains(t.SecondCharacteristicsValueId) &&
        //                         thirdValueIds.Contains(t.ThirdCharacteristicsValueId) &&                                  
        //                         t.CompanyId == openingBalance.CompanyId && t.PlantId == openingBalance.PlantId
        //                         ).Select().ToList();

        //        foreach (var materialMasterOpeningBalanceDetailVM in materialMasterOpeningBalanceDetailVMList)
        //        {
        //            currentRecord++;
        //            // INSERT INTO OPENING BALANCE DETAIL
        //            var sql = @"SELECT TOP(1) FAGL.* FROM [HKP].MaterialGroupGL AS FAGL
        //                        INNER JOIN [ORG].[Company] AS C ON C.COAId=FAGL.COAId
        //                     JOIN MST.MaterialMaster MM ON FAGL.MaterialGroupMasterId = MM.MaterialGroupMasterId
        //                        WHERE C.Id='" + openingBalance.CompanyId + "' AND MM.Id='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "'";
        //            var glTemp = _openingBalanceRepository.SqlQuery<MaterialGroupGL>(sql).FirstOrDefault();
        //            if (null == glTemp || string.IsNullOrEmpty(glTemp.InventoryGLId))
        //                throw new CustomException($"This {materialMasterOpeningBalanceDetailVM.MaterialMasterName} Material Group Account Determinate GL not Found!");
        //            //&& t.LotNumber == materialMasterOpeningBalanceDetailVM.LotNumber
        //            //&& t.Diameter == materialMasterOpeningBalanceDetailVM.Diameter
        //            //&& t.Type == materialMasterOpeningBalanceDetailVM.Type
        //            if (materialDbData.Any(t => t.MaterialMasterId == materialMasterOpeningBalanceDetailVM.MaterialMasterId
        //                         && t.ArticleId == materialMasterOpeningBalanceDetailVM.ArticleId
        //                         && t.FirstCharacteristicsId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId
        //                         && t.FirstCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId
        //                         && t.SecondCharacteristicsId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId
        //                         && t.SecondCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId
        //                         && t.ThirdCharacteristicsId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId
        //                         && t.ThirdCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId                                 
        //                         )
        //                )


        //            //If Material Exist Modify Material Inventory
        //            {
        //                flagStatus = true;
        //                //throw new CustomException(materialMasterOpeningBalanceDetailVM.MaterialMasterName + " already add.");
        //                var materialMasterOpeningBalanceDetail = new MaterialMasterOpeningBalanceDetail
        //                {
        //                    AddedBy = openingBalance.AddedBy,
        //                    AddedDate = openingBalance.AddedDate,
        //                    AddedFromIP = openingBalance.AddedFromIP,
        //                    AssetActivityId = glTemp.InventoryActivityId,
        //                    AssetBudgetMasterId = glTemp.InventoryBudgetMasterId,
        //                    AssetGLId = glTemp.InventoryGLId,
        //                    Id = MakePK(openingBalance.Id, currentRecord, 4),
        //                    PlantId = openingBalance.PlantId,
        //                    EntityId = openingBalance.EntityId,
        //                    ModelState = ModelState.Added,
        //                    OpeningBalanceId = openingBalance.Id,
        //                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                    CurrencyId = materialMasterOpeningBalanceDetailVM.CurrencyId,
        //                    BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
        //                    MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
        //                    ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
        //                    FixedAssetMasterId = materialMasterOpeningBalanceDetailVM.FixedAssetMasterId,
        //                    MaterialStorageId = openingBalance.MaterialStorageId,
        //                    Quantity = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    FirstCharacteristicsId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId,
        //                    FirstCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId,
        //                    SecondCharacteristicsId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId,
        //                    SecondCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId,
        //                    ThirdCharacteristicsId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId,
        //                    ThirdCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId,
        //                    LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
        //                    Diameter= materialMasterOpeningBalanceDetailVM.Diameter,
        //                    Type = materialMasterOpeningBalanceDetailVM.Type,
        //                };

        //                // Insert Inventory Material
        //                //inventoryMaterialPk.MaxNumber++;
        //                //var inventoryMaterial = new InventoryMaterial
        //                //{
        //                //	Id = inventoryMaterialPk.MaxNumber.ToString(),
        //                //	CompanyGroupId = inventoryReceive.CompanyGroupId,
        //                //	CompanyId = inventoryReceive.CompanyId,
        //                //	PlantId = inventoryReceive.PlantId,
        //                //	OpeningBalanceId = openingBalance.Id,
        //                //	MaterialStorageId = inventoryReceive.MaterialStorageId,
        //                //	MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
        //                //	ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
        //                //	FirstCharacteristicsId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId,
        //                //	FirstCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId,
        //                //	SecondCharacteristicsId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId,
        //                //	SecondCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId,
        //                //	ThirdCharacteristicsId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId,
        //                //	ThirdCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId,
        //                //	TotalQty = materialMasterOpeningBalanceDetailVM.Quantity,
        //                //	AvgRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
        //                //	AddedBy = openingBalance.AddedBy,
        //                //	AddedDate = openingBalance.AddedDate,
        //                //	AddedFromIP = openingBalance.AddedFromIP
        //                //};
        //                //_inventoryMaterialRepository.Insert(inventoryMaterial);

        //                // Insert Receive Details
        //                //var sql1 = @"SELECT Id TRN.InventoryMaterial
        //                //                          WHERE MaterialMasterId='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "' " +
        //                //		"AND ArticleId='" + materialMasterOpeningBalanceDetailVM.ArticleId + "' " +
        //                //		"AND Isnull(FirstCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId + "' " +
        //                //		"AND Isnull(FirstCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId + "' " +
        //                //		"AND Isnull(SecondCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId + "' " +
        //                //		"AND Isnull(SecondCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId + "' " +
        //                //		"AND Isnull(ThirdCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId + "' " +
        //                //		"AND Isnull(ThirdCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId + "'";
        //                //var glTemp1 = _openingBalanceRepository.SqlQuery<MaterialGroupGL>(sql1).FirstOrDefault();
        //                currentReceiveDetailId++;
        //                //&& r.LotNumber == materialMasterOpeningBalanceDetailVM.LotNumber
        //                //&& r.Diameter == materialMasterOpeningBalanceDetailVM.Diameter 
        //                //&& r.Type == materialMasterOpeningBalanceDetailVM.Type 
        //                var inventoryMaterial = materialDbData.Where(r => r.MaterialMasterId == materialMasterOpeningBalanceDetailVM.MaterialMasterId && r.ArticleId == materialMasterOpeningBalanceDetailVM.ArticleId
        //                && r.FirstCharacteristicsId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId && r.FirstCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId
        //                && r.SecondCharacteristicsId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId && r.SecondCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId
        //                && r.ThirdCharacteristicsId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId && r.ThirdCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId                        
        //                ).FirstOrDefault();
        //                var inventoryReceiveDetails = new InventoryReceiveDetail
        //                {
        //                    Id = MakePK(inventoryReceive.Id + 1, currentReceiveDetailId, 2),
        //                    MaterialStorageId = inventoryReceive.MaterialStorageId,
        //                    InventoryReceiveId = inventoryReceive.Id,
        //                    InventoryMaterialId = inventoryMaterial.Id,//inventoryMaterial.Id,
        //                    TransactionQty = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    BaseQty = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    TransactionUoMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
        //                    BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
        //                    BaseUoMFactor = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    MaterialTranRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
        //                    MaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                    TotalMaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                    TotalMaterialBooksCurrencyAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                    TotalTaxAmount = 0,
        //                    ChargesTranAmount = 0,
        //                    BaseIssueQty = 0,
        //                    TrnCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
        //                    BooksCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
        //                    AddedBy = openingBalance.AddedBy,
        //                    AddedDate = openingBalance.AddedDate,
        //                    AddedFromIP = openingBalance.AddedFromIP,
        //                    PostDrGLGeneralInfoId = glTemp.InventoryGLId,
        //                    PostDrBudgetMasterId = glTemp.InventoryBudgetMasterId,
        //                    PostDrActivityId = glTemp.InventoryActivityId,
        //                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                    LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
        //                    Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
        //                    Type = materialMasterOpeningBalanceDetailVM.Type,
        //                };
        //                _inventoryReceiveDetailRepository.Insert(inventoryReceiveDetails);

        //                CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, materialMasterOpeningBalanceDetailVM);
        //                // Set company currency.
        //                if (!string.IsNullOrWhiteSpace(companyCurrencyId))
        //                {
        //                    //DetailCurrency
        //                    //FA
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
        //                        GLType = "FA",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 1, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
        //                    });
        //                    // AD
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
        //                        GLType = "AD",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 2, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
        //                    });
        //                }

        //                // Set company group currency.
        //                if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
        //                {
        //                    //Detail Currency
        //                    // FA
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
        //                        GLType = "FA",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 7, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
        //                    });

        //                    // AD
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
        //                        GLType = "AD",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 8, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate
        //                    });
        //                }

        //                // Set company hard currency.
        //                if (!string.IsNullOrWhiteSpace(hardCurrencyId))
        //                {
        //                    // FA
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
        //                        GLType = "FA",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 13, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
        //                    });

        //                    // AD
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
        //                        GLType = "AD",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 14, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
        //                    });
        //                }
        //                _materialMasterOpeningBalanceDetailRepository.Insert(materialMasterOpeningBalanceDetail);
        //                var TotalPreviuousAmountForMAtArtChar = ((inventoryMaterial.TotalQty * inventoryMaterial.AvgRate) + inventoryReceiveDetails.TotalMaterialTranAmount);
        //                var TotalQty = (inventoryMaterial.TotalQty + inventoryReceiveDetails.TransactionQty);
        //                var AvgRate = TotalPreviuousAmountForMAtArtChar / TotalQty;

        //                //builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + (materialDbData[0].TotalQty + materialMasterOpeningBalanceDetailVM.Quantity) + @"' 
        //                //				 WHERE MaterialMasterId='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "' " +
        //                //					"AND ArticleId='" + materialMasterOpeningBalanceDetailVM.ArticleId + "' " +
        //                //					"AND Isnull(FirstCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId + "' " +
        //                //					"AND Isnull(FirstCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId + "' " +
        //                //					"AND Isnull(SecondCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId + "' " +
        //                //					"AND Isnull(SecondCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId + "' " +
        //                //					"AND Isnull(ThirdCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId + "' " +
        //                //					"AND Isnull(ThirdCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId + "'";
        //                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + TotalQty + "' , AvgRate='" + AvgRate + @"' 
        //		  WHERE MaterialMasterId='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "' " +
        //                                    "AND ArticleId='" + materialMasterOpeningBalanceDetailVM.ArticleId + "' " +
        //                                    "AND Isnull(FirstCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId + "' " +
        //                                    "AND Isnull(FirstCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId + "' " +
        //                                    "AND Isnull(SecondCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId + "' " +
        //                                    "AND Isnull(SecondCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId + "' " +
        //                                    "AND Isnull(ThirdCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId + "' " +
        //                                    "AND Isnull(ThirdCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId + "' ";
        //                                    //"AND Isnull(LotNumber,'')='" + materialMasterOpeningBalanceDetailVM.LotNumber + "' " +
        //                                    //"AND Isnull(Diameter,'')='" + materialMasterOpeningBalanceDetailVM.Diameter + "' " +
        //                                    //"AND Isnull(Type,'')='" + materialMasterOpeningBalanceDetailVM.Type + "'";
        //                rdBuilder.Append(builderSql);
        //                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());




        //            }
        //            else
        //            {
        //                flagStatus = false;
        //                var materialMasterOpeningBalanceDetail = new MaterialMasterOpeningBalanceDetail
        //                {
        //                    AddedBy = openingBalance.AddedBy,
        //                    AddedDate = openingBalance.AddedDate,
        //                    AddedFromIP = openingBalance.AddedFromIP,
        //                    AssetActivityId = glTemp.InventoryActivityId,
        //                    AssetBudgetMasterId = glTemp.InventoryBudgetMasterId,
        //                    AssetGLId = glTemp.InventoryGLId,
        //                    Id = MakePK(openingBalance.Id, currentRecord, 4),
        //                    PlantId = openingBalance.PlantId,
        //                    EntityId = openingBalance.EntityId,
        //                    ModelState = ModelState.Added,
        //                    OpeningBalanceId = openingBalance.Id,
        //                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                    CurrencyId = materialMasterOpeningBalanceDetailVM.CurrencyId,
        //                    BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
        //                    MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
        //                    ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
        //                    FixedAssetMasterId = materialMasterOpeningBalanceDetailVM.FixedAssetMasterId,
        //                    MaterialStorageId = openingBalance.MaterialStorageId,
        //                    Quantity = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    FirstCharacteristicsId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId,
        //                    FirstCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId,
        //                    SecondCharacteristicsId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId,
        //                    SecondCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId,
        //                    ThirdCharacteristicsId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId,
        //                    ThirdCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId,
        //                    LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
        //                    Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
        //                    Type = materialMasterOpeningBalanceDetailVM.Type,
        //                };

        //                // Insert Inventory Material
        //                inventoryMaterialPk.MaxNumber++;
        //                var inventoryMaterial = new InventoryMaterial
        //                {
        //                    Id = inventoryMaterialPk.MaxNumber.ToString(),
        //                    CompanyGroupId = inventoryReceive.CompanyGroupId,
        //                    CompanyId = inventoryReceive.CompanyId,
        //                    PlantId = inventoryReceive.PlantId,
        //                    OpeningBalanceId = openingBalance.Id,
        //                    MaterialStorageId = inventoryReceive.MaterialStorageId,
        //                    MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
        //                    ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
        //                    FirstCharacteristicsId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId,
        //                    FirstCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId,
        //                    SecondCharacteristicsId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId,
        //                    SecondCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId,
        //                    ThirdCharacteristicsId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId,
        //                    ThirdCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId,

        //                    LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
        //                    Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
        //                    Type= materialMasterOpeningBalanceDetailVM.Type,

        //                    TotalQty = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    AvgRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
        //                    AddedBy = openingBalance.AddedBy,
        //                    AddedDate = openingBalance.AddedDate,
        //                    AddedFromIP = openingBalance.AddedFromIP
        //                };
        //                _inventoryMaterialRepository.Insert(inventoryMaterial);

        //                // Insert Receive Details
        //                currentReceiveDetailId++;
        //                var inventoryReceiveDetails = new InventoryReceiveDetail
        //                {
        //                    Id = MakePK(inventoryReceive.Id + 1, currentReceiveDetailId, 2),
        //                    MaterialStorageId = inventoryReceive.MaterialStorageId,
        //                    InventoryReceiveId = inventoryReceive.Id,
        //                    InventoryMaterialId = inventoryMaterial.Id,
        //                    TransactionQty = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    BaseQty = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    TransactionUoMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
        //                    BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
        //                    BaseUoMFactor = materialMasterOpeningBalanceDetailVM.Quantity,
        //                    MaterialTranRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
        //                    MaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                    TotalMaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                    TotalMaterialBooksCurrencyAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                    TotalTaxAmount = 0,
        //                    ChargesTranAmount = 0,
        //                    BaseIssueQty = 0,
        //                    TrnCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
        //                    BooksCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
        //                    AddedBy = openingBalance.AddedBy,
        //                    AddedDate = openingBalance.AddedDate,
        //                    AddedFromIP = openingBalance.AddedFromIP,
        //                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                    LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
        //                    Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
        //                    Type = materialMasterOpeningBalanceDetailVM.Type,
        //                };
        //                _inventoryReceiveDetailRepository.Insert(inventoryReceiveDetails);

        //                CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, materialMasterOpeningBalanceDetailVM);
        //                // Set company currency.
        //                if (!string.IsNullOrWhiteSpace(companyCurrencyId))
        //                {
        //                    //DetailCurrency
        //                    //FA
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
        //                        GLType = "FA",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 1, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
        //                    });
        //                    // AD
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
        //                        GLType = "AD",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 2, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
        //                    });
        //                }

        //                // Set company group currency.
        //                if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
        //                {
        //                    //Detail Currency
        //                    // FA
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
        //                        GLType = "FA",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 7, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
        //                    });

        //                    // AD
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
        //                        GLType = "AD",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 8, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate
        //                    });
        //                }

        //                // Set company hard currency.
        //                if (!string.IsNullOrWhiteSpace(hardCurrencyId))
        //                {
        //                    // FA
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
        //                        GLType = "FA",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 13, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
        //                    });

        //                    // AD
        //                    _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
        //                    {
        //                        AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
        //                        AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
        //                        AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
        //                        Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
        //                        MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
        //                        FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
        //                        GLType = "AD",
        //                        Id = MakePK(materialMasterOpeningBalanceDetail.Id, 14, 2),
        //                        ModelState = ModelState.Added,
        //                        OpeningBalanceId = openingBalance.Id,
        //                        ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
        //                        ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
        //                        ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
        //                        ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
        //                    });
        //                }
        //                _materialMasterOpeningBalanceDetailRepository.Insert(materialMasterOpeningBalanceDetail);

        //            }
        //        }
        //        _unitOfWork.SaveChanges();
        //        //if(flagStatus==true)
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (CustomException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
        //    }
        //}
        public void InsertMaterialMaster(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList)
        {
            var flagStatus = false;
            var flag = false;
            var rdBuilder = new System.Text.StringBuilder();
            var builderSql = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var inventoryReceive = new InventoryReceive
            {

            };
            var invReceivePk = GetAutoNumber(nameof(InventoryReceive), PKGeneratorEnum.Auto, null, openingBalance.PostingDate);
            var inventoryMaterialPk = GetMaxNumber(nameof(InventoryMaterial), PKGeneratorEnum.Auto, null, openingBalance.PostingDate);
            var MaterialId = Convert.ToInt32(inventoryMaterialPk.MaxNumber) - 1;
            var InventoryRcvId = "";

            try
            {

                Check(openingBalance);
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(openingBalance.Id))
                {


                    // INSERT INTO OpeningBalance TABLE
                    openingBalance.Id = GetOpeningBalancePK(openingBalance);
                    openingBalance.Archive = false;
                    openingBalance.EmployeeTransactionTypeId = null;
                    openingBalance.FinancingTypeId = null;
                    openingBalance.IsPark = true;
                    openingBalance.IsPosted = true;
                    openingBalance.SourceType = SourceType.MaterialMaster.ToString();
                    openingBalance.UpdatedBy = null;
                    openingBalance.UpdatedDate = null;
                    openingBalance.UpdatedFromIP = null;
                    openingBalance.VoucherId = null;
                    InsertGraph(openingBalance);

                    //var invReceivePk = GetAutoNumber(nameof(InventoryReceive), PKGeneratorEnum.Auto, null, openingBalance.PostingDate);
                    //var inventoryMaterialPk = GetMaxNumber(nameof(InventoryMaterial), PKGeneratorEnum.Auto, null, openingBalance.PostingDate);
                    //var MaterialId = Convert.ToInt32(inventoryMaterialPk.MaxNumber) - 1;

                    // Insert Inventory Receive
                    inventoryReceive = new InventoryReceive
                    {
                        Id = invReceivePk,
                        OpeningBalanceId = openingBalance.Id,
                        CompanyGroupId = openingBalance.CompanyGroupId,
                        CompanyId = openingBalance.CompanyId,
                        PlantId = openingBalance.PlantId,
                        MaterialStorageId = openingBalance.MaterialStorageId,
                        CurrencyId = materialMasterOpeningBalanceDetailVMList.FirstOrDefault().CompanyCurrencyId,
                        DocRefNo = openingBalance.DocRefNo,
                        DocDate = openingBalance.DocDate,
                        GateEntryNo = null,
                        EntryDate = openingBalance.AddedDate,
                        GRNDate = openingBalance.PostingDate,//AddedDate,
                        FixedAssetOrInventory = "Inventory",
                        PODepended = false,
                        AlongwithInvoice = false,
                        BaseNoOfDays = 0,
                        BaseCurrencyId = materialMasterOpeningBalanceDetailVMList.FirstOrDefault().CompanyCurrencyId,
                        IsNonCreditable = false,
                        ToCurrencyRate = 0,
                        IsTaxApplicable = false,
                        AddedBy = openingBalance.AddedBy,
                        AddedDate = openingBalance.AddedDate,
                        AddedFromIP = openingBalance.AddedFromIP,
                        GRNType = "OpeningBalance"

                    };


                    _inventoryReceiveRepository.Insert(inventoryReceive);
                    openingBalance.InventoryReceivedId = inventoryReceive.Id;
                    openingBalance.openingBalanceId = openingBalance.Id;
                    openingBalance.MaterialStorageId = openingBalance.MaterialStorageId;

                }
                else
                {
                    if (string.IsNullOrEmpty(openingBalance.openingBalanceId))
                        openingBalance.openingBalanceId = openingBalance.Id;
                    var OpeningDataInfo = _openingBalanceRepository.Find(openingBalance.openingBalanceId);
                    openingBalance.AddedBy = OpeningDataInfo.AddedBy;
                    openingBalance.AddedDate = OpeningDataInfo.AddedDate;
                    openingBalance.AddedFromIP = OpeningDataInfo.AddedFromIP;

                }
                var fixedAssetOBDetailList = (from fd in _materialMasterOpeningBalanceDetailRepository.Query().Select()
                                              join f in _openingBalanceRepository.Query(r => r.CompanyGroupId == openingBalance.CompanyGroupId && r.CompanyId == openingBalance.CompanyId).Select() on fd.OpeningBalanceId equals f.Id
                                              select fd).ToList();
                //var currentRecord1 = 0;
                var currentReceiveDetailId = _inventoryReceiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId='{openingBalance.InventoryReceivedId}'").First();

                var materialIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.MaterialMasterId);
                var articleIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.ArticleId);
                var firstValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.FirstCharacteristicsValueId);
                var secondValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.SecondCharacteristicsValueId);
                var thirdValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.ThirdCharacteristicsValueId);

                var materialDbData = _inventoryMaterialRepository.Query(t => materialIds.Contains(t.MaterialMasterId) && articleIds.Contains(t.ArticleId) &&
                                 firstValueIds.Contains(t.FirstCharacteristicsValueId) && secondValueIds.Contains(t.SecondCharacteristicsValueId) &&
                                 thirdValueIds.Contains(t.ThirdCharacteristicsValueId) && t.CompanyId == openingBalance.CompanyId && t.PlantId == openingBalance.PlantId
                                 ).Select().ToList();
                openingBalance.currentRecord = openingBalance.currentRecord + 1;
                foreach (var materialMasterOpeningBalanceDetailVM in materialMasterOpeningBalanceDetailVMList)
                {

                    // INSERT INTO OPENING BALANCE DETAIL
                    var sql = @"SELECT TOP(1) FAGL.* FROM [HKP].MaterialGroupGL AS FAGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=FAGL.COAId
	                            JOIN MST.MaterialMaster MM ON FAGL.MaterialGroupMasterId = MM.MaterialGroupMasterId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND MM.Id='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "'";
                    var glTemp = _openingBalanceRepository.SqlQuery<MaterialGroupGL>(sql).FirstOrDefault();
                    if (null == glTemp || string.IsNullOrEmpty(glTemp.InventoryGLId))
                        throw new CustomException($"This {materialMasterOpeningBalanceDetailVM.MaterialMasterName} Material Group Account Determinate GL not Found!");

                    if (materialDbData.Any(t => t.MaterialMasterId == materialMasterOpeningBalanceDetailVM.MaterialMasterId
                                 && t.ArticleId == materialMasterOpeningBalanceDetailVM.ArticleId
                                 && t.FirstCharacteristicsId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId
                                 && t.FirstCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId
                                 && t.SecondCharacteristicsId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId
                                 && t.SecondCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId
                                 && t.ThirdCharacteristicsId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId
                                 && t.ThirdCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId))


                    //If Material Exist Modify Material Inventory
                    {
                        flagStatus = true;
                        //throw new CustomException(materialMasterOpeningBalanceDetailVM.MaterialMasterName + " already add.");
                        var materialMasterOpeningBalanceDetail = new MaterialMasterOpeningBalanceDetail
                        {
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            AssetActivityId = glTemp.InventoryActivityId,
                            AssetBudgetMasterId = glTemp.InventoryBudgetMasterId,
                            AssetGLId = glTemp.InventoryGLId,
                            Id = MakePK(openingBalance.Id, openingBalance.currentRecord, 4),
                            PlantId = openingBalance.PlantId,
                            EntityId = openingBalance.EntityId,
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            CurrencyId = materialMasterOpeningBalanceDetailVM.CurrencyId,
                            BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
                            ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
                            FixedAssetMasterId = materialMasterOpeningBalanceDetailVM.FixedAssetMasterId,
                            MaterialStorageId = openingBalance.MaterialStorageId,
                            Quantity = materialMasterOpeningBalanceDetailVM.Quantity,
                            FirstCharacteristicsId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId,
                            LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
                            Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
                            Type = materialMasterOpeningBalanceDetailVM.Type,
                        };

                        // Insert Inventory Material
                        //inventoryMaterialPk.MaxNumber++;
                        //var inventoryMaterial = new InventoryMaterial
                        //{
                        //	Id = inventoryMaterialPk.MaxNumber.ToString(),
                        //	CompanyGroupId = inventoryReceive.CompanyGroupId,
                        //	CompanyId = inventoryReceive.CompanyId,
                        //	PlantId = inventoryReceive.PlantId,
                        //	OpeningBalanceId = openingBalance.Id,
                        //	MaterialStorageId = inventoryReceive.MaterialStorageId,
                        //	MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
                        //	ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
                        //	FirstCharacteristicsId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId,
                        //	FirstCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId,
                        //	SecondCharacteristicsId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId,
                        //	SecondCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId,
                        //	ThirdCharacteristicsId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId,
                        //	ThirdCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId,
                        //	TotalQty = materialMasterOpeningBalanceDetailVM.Quantity,
                        //	AvgRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                        //	AddedBy = openingBalance.AddedBy,
                        //	AddedDate = openingBalance.AddedDate,
                        //	AddedFromIP = openingBalance.AddedFromIP
                        //};
                        //_inventoryMaterialRepository.Insert(inventoryMaterial);

                        // Insert Receive Details
                        //var sql1 = @"SELECT Id TRN.InventoryMaterial
                        //                          WHERE MaterialMasterId='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "' " +
                        //		"AND ArticleId='" + materialMasterOpeningBalanceDetailVM.ArticleId + "' " +
                        //		"AND Isnull(FirstCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId + "' " +
                        //		"AND Isnull(FirstCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId + "' " +
                        //		"AND Isnull(SecondCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId + "' " +
                        //		"AND Isnull(SecondCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId + "' " +
                        //		"AND Isnull(ThirdCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId + "' " +
                        //		"AND Isnull(ThirdCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId + "'";
                        //var glTemp1 = _openingBalanceRepository.SqlQuery<MaterialGroupGL>(sql1).FirstOrDefault();
                        currentReceiveDetailId++;

                        var inventoryMaterial = materialDbData.Where(r => r.MaterialMasterId == materialMasterOpeningBalanceDetailVM.MaterialMasterId && r.ArticleId == materialMasterOpeningBalanceDetailVM.ArticleId
                        && r.FirstCharacteristicsId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId && r.FirstCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId
                        && r.ThirdCharacteristicsId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId && r.ThirdCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId
                        ).FirstOrDefault();
                        var inventoryReceiveDetails = new InventoryReceiveDetail
                        {
                            Id = MakePK(openingBalance.InventoryReceivedId + 1, currentReceiveDetailId, 2),
                            MaterialStorageId = openingBalance.MaterialStorageId,//inventoryReceive.MaterialStorageId,
                            InventoryReceiveId = openingBalance.InventoryReceivedId,//inventoryReceive.Id,
                            InventoryMaterialId = inventoryMaterial.Id,//inventoryMaterial.Id,
                            TransactionQty = materialMasterOpeningBalanceDetailVM.Quantity,
                            BaseQty = materialMasterOpeningBalanceDetailVM.Quantity,
                            TransactionUoMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            BaseUoMFactor = materialMasterOpeningBalanceDetailVM.Quantity,
                            MaterialTranRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            MaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalMaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalMaterialBooksCurrencyAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalTaxAmount = 0,
                            ChargesTranAmount = 0,
                            BaseIssueQty = 0,
                            TrnCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            BooksCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            PostDrGLGeneralInfoId = glTemp.InventoryGLId,
                            PostDrBudgetMasterId = glTemp.InventoryBudgetMasterId,
                            PostDrActivityId = glTemp.InventoryActivityId,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
                            Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
                            Type = materialMasterOpeningBalanceDetailVM.Type,
                        };
                        AuditService.AddedLog(inventoryReceiveDetails);
                        _inventoryReceiveDetailRepository.Insert(inventoryReceiveDetails);

                        CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, materialMasterOpeningBalanceDetailVM);
                        // Set company currency.
                        if (!string.IsNullOrWhiteSpace(companyCurrencyId))
                        {
                            //DetailCurrency
                            //FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 1, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
                            });
                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = materialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = materialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = materialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 2, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                            });
                        }

                        // Set company group currency.
                        if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 7, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
                            });

                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 8, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate
                            });
                        }

                        // Set company hard currency.
                        if (!string.IsNullOrWhiteSpace(hardCurrencyId))
                        {
                            // FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 13, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
                            });

                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 14, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
                            });
                        }
                        AuditService.AddedLog(materialMasterOpeningBalanceDetail);
                        _materialMasterOpeningBalanceDetailRepository.Insert(materialMasterOpeningBalanceDetail);
                        var TotalPreviuousAmountForMAtArtChar = ((inventoryMaterial.TotalQty * inventoryMaterial.AvgRate) + inventoryReceiveDetails.TotalMaterialTranAmount);
                        var TotalQty = (inventoryMaterial.TotalQty + inventoryReceiveDetails.TransactionQty);
                        var AvgRate = TotalPreviuousAmountForMAtArtChar / TotalQty;

                        //builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + (materialDbData[0].TotalQty + materialMasterOpeningBalanceDetailVM.Quantity) + @"' 
                        //				 WHERE MaterialMasterId='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "' " +
                        //					"AND ArticleId='" + materialMasterOpeningBalanceDetailVM.ArticleId + "' " +
                        //					"AND Isnull(FirstCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId + "' " +
                        //					"AND Isnull(FirstCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId + "' " +
                        //					"AND Isnull(SecondCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId + "' " +
                        //					"AND Isnull(SecondCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId + "' " +
                        //					"AND Isnull(ThirdCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId + "' " +
                        //					"AND Isnull(ThirdCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId + "'";
                        builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + TotalQty + "' , AvgRate='" + AvgRate + @"' 
										  WHERE MaterialMasterId='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "' " +
                                            "AND ArticleId='" + materialMasterOpeningBalanceDetailVM.ArticleId + "' " +
                                            "AND Isnull(FirstCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId + "' " +
                                            "AND Isnull(FirstCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId + "' " +
                                            "AND Isnull(SecondCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId + "' " +
                                            "AND Isnull(SecondCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId + "' " +
                                            "AND Isnull(ThirdCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId + "' " +
                                            "AND Isnull(ThirdCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());




                    }
                    else
                    {
                        flagStatus = false;
                        var materialMasterOpeningBalanceDetail = new MaterialMasterOpeningBalanceDetail
                        {
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            AssetActivityId = glTemp.InventoryActivityId,
                            AssetBudgetMasterId = glTemp.InventoryBudgetMasterId,
                            AssetGLId = glTemp.InventoryGLId,
                            Id = MakePK(openingBalance.Id, openingBalance.currentRecord, 4),
                            PlantId = openingBalance.PlantId,
                            EntityId = openingBalance.EntityId,
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            CurrencyId = materialMasterOpeningBalanceDetailVM.CurrencyId,
                            BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
                            ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
                            FixedAssetMasterId = materialMasterOpeningBalanceDetailVM.FixedAssetMasterId,
                            MaterialStorageId = openingBalance.MaterialStorageId,
                            Quantity = materialMasterOpeningBalanceDetailVM.Quantity,
                            FirstCharacteristicsId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId,
                            LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
                            Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
                            Type = materialMasterOpeningBalanceDetailVM.Type,
                        };

                        // Insert Inventory Material
                        inventoryMaterialPk.MaxNumber++;
                        var inventoryMaterial = new InventoryMaterial
                        {
                            Id = inventoryMaterialPk.MaxNumber.ToString(),
                            CompanyGroupId = identity.CompanyGroupId,
                            CompanyId = identity.CompanyId,
                            PlantId = identity.PlantId,
                            OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                            MaterialStorageId = inventoryReceive.MaterialStorageId,
                            MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
                            ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
                            FirstCharacteristicsId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId,
                            TotalQty = materialMasterOpeningBalanceDetailVM.Quantity,
                            AvgRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP
                        };
                        _inventoryMaterialRepository.Insert(inventoryMaterial);

                        // Insert Receive Details
                        currentReceiveDetailId++;
                        var inventoryReceiveDetails = new InventoryReceiveDetail
                        {
                            Id = MakePK(openingBalance.InventoryReceivedId + 1, currentReceiveDetailId, 2),
                            MaterialStorageId = openingBalance.MaterialStorageId,//inventoryReceive.MaterialStorageId,
                            InventoryReceiveId = openingBalance.InventoryReceivedId,//inventoryReceive.Id,
                            InventoryMaterialId = inventoryMaterial.Id,
                            TransactionQty = materialMasterOpeningBalanceDetailVM.Quantity,
                            BaseQty = materialMasterOpeningBalanceDetailVM.Quantity,
                            TransactionUoMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            BaseUoMFactor = materialMasterOpeningBalanceDetailVM.Quantity,
                            MaterialTranRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            MaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalMaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalMaterialBooksCurrencyAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalTaxAmount = 0,
                            ChargesTranAmount = 0,
                            BaseIssueQty = 0,
                            TrnCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            BooksCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                            LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
                            Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
                            Type = materialMasterOpeningBalanceDetailVM.Type,
                        };
                        AuditService.AddedLog(inventoryReceiveDetails);
                        _inventoryReceiveDetailRepository.Insert(inventoryReceiveDetails);

                        CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, materialMasterOpeningBalanceDetailVM);
                        // Set company currency.
                        if (!string.IsNullOrWhiteSpace(companyCurrencyId))
                        {
                            //DetailCurrency
                            //FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 1, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
                            });
                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 2, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                            });
                        }

                        // Set company group currency.
                        if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 7, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
                            });

                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 8, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate
                            });
                        }

                        // Set company hard currency.
                        if (!string.IsNullOrWhiteSpace(hardCurrencyId))
                        {
                            // FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 13, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
                            });

                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = openingBalance.AddedBy,
                                AddedDate = openingBalance.AddedDate,
                                AddedFromIP = openingBalance.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(materialMasterOpeningBalanceDetail.Id, 14, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.openingBalanceId,//openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
                            });

                        }
                        AuditService.AddedLog(materialMasterOpeningBalanceDetail);
                        _materialMasterOpeningBalanceDetailRepository.Insert(materialMasterOpeningBalanceDetail);

                    }
                }
                _unitOfWork.SaveChanges();
                //if(flagStatus==true)
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void UpdateMaterialMaster(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                ModifyCheck(openingBalance.Id);
                Check(openingBalance);

                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                var duplicates = materialMasterOpeningBalanceDetailVMList.GroupBy(x => new { x.AssetGLId, x.AssetBudgetMasterId }).Where(x => x.Count() > 1).Select(x => x.Key);
                //if (duplicates.Any())
                //{
                //    var bm = _glRepository.Find(duplicates.FirstOrDefault().AssetGLId);
                //    throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "GL (" + bm.UserName + ")"));
                //}

                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO OpeningBalance TABLE
                openingBalance.Archive = false;
                openingBalance.EmployeeTransactionTypeId = null;
                openingBalance.FinancingTypeId = null;
                openingBalance.IsPark = true;
                openingBalance.IsPosted = true;
                openingBalance.SourceType = SourceType.MaterialMaster.ToString();
                openingBalance.VoucherId = null;
                UpdateGraph(openingBalance);

                var fixedAssetOBDetailList = (from fd in _materialMasterOpeningBalanceDetailRepository.Query().Select()
                                              join f in _openingBalanceRepository.Query(r => r.CompanyGroupId == openingBalance.CompanyGroupId && r.CompanyId == openingBalance.CompanyId).Select() on fd.OpeningBalanceId equals f.Id
                                              select fd).ToList();
                var faDetailIds = fixedAssetOBDetailList.Select(t => t.Id);
                var fixedAssetOBDetailCurrencyList = _materialMasterOpeningBalanceDetailCurrencyRepository.Query(r => faDetailIds.Contains(r.MaterialMasterOpeningBalanceDetailId)).Select().ToList();
                var currentRecord = _openingBalanceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.MaterialMasterOpeningBalanceDetail WHERE OpeningBalanceId='{openingBalance.Id}'").First();

                var materialIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.MaterialMasterId);
                var articleIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.ArticleId);
                var firstValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.FirstCharacteristicsValueId);
                var secondValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.SecondCharacteristicsValueId);
                var thirdValueIds = materialMasterOpeningBalanceDetailVMList.Select(t => t.ThirdCharacteristicsValueId);
                var materialDbData = _inventoryMaterialRepository.Query(t => materialIds.Contains(t.MaterialMasterId) && articleIds.Contains(t.ArticleId) &&
                             firstValueIds.Contains(t.FirstCharacteristicsValueId) && secondValueIds.Contains(t.SecondCharacteristicsValueId) &&
                             thirdValueIds.Contains(t.ThirdCharacteristicsValueId) && t.CompanyId == openingBalance.CompanyId && t.PlantId == openingBalance.PlantId
                             ).Select().ToList();

                var MaterialMasterOpeningBalanceDetailNewId = "";
                foreach (var materialMasterOpeningBalanceDetailVM in materialMasterOpeningBalanceDetailVMList)
                {
                    var sql = @"SELECT TOP(1) FAGL.* FROM [HKP].MaterialGroupGL AS FAGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=FAGL.COAId
	                            JOIN MST.MaterialMaster MM ON FAGL.MaterialGroupMasterId = MM.MaterialGroupMasterId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND MM.Id='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "'";
                    var glTemp = _openingBalanceRepository.SqlQuery<MaterialGroupGL>(sql).FirstOrDefault();
                    if (null == glTemp || string.IsNullOrEmpty(glTemp.InventoryGLId))
                        throw new CustomException($"This {materialMasterOpeningBalanceDetailVM.MaterialMasterName} Material Group Account Determinate GL not Found!");

                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, materialMasterOpeningBalanceDetailVM);

                    if (string.IsNullOrEmpty(materialMasterOpeningBalanceDetailVM.Id))
                    {
                        currentRecord++;
                        var MaterialMasterOpeningBalanceDetail = new MaterialMasterOpeningBalanceDetail
                        {
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            AssetActivityId = materialMasterOpeningBalanceDetailVM.AssetActivityId,
                            AssetBudgetMasterId = materialMasterOpeningBalanceDetailVM.AssetBudgetMasterId,
                            AssetGLId = materialMasterOpeningBalanceDetailVM.AssetGLId,
                            Id = MakePK(openingBalance.Id, currentRecord, 4),
                            ModelState = ModelState.Added,
                            OpeningBalanceId = openingBalance.Id,
                            FixedAssetMasterId = materialMasterOpeningBalanceDetailVM.FixedAssetMasterId,
                            Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            CurrencyId = materialMasterOpeningBalanceDetailVM.CurrencyId,
                            BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            MaterialMasterId = materialMasterOpeningBalanceDetailVM.MaterialMasterId,
                            ArticleId = materialMasterOpeningBalanceDetailVM.ArticleId,
                            MaterialStorageId = openingBalance.MaterialStorageId,
                            Quantity = materialMasterOpeningBalanceDetailVM.Quantity
                        };
                        // Set company currency.
                        if (!string.IsNullOrWhiteSpace(companyCurrencyId))
                        {
                            //Detail Currency
                            //FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 1, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
                            });
                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 2, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                            });
                        }

                        // Set company group currency.
                        if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 7, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
                            });

                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 8, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate
                            });
                        }

                        // Set company hard currency.
                        if (!string.IsNullOrWhiteSpace(hardCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "FA",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 13, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
                            });

                            // AD
                            _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                            {
                                AddedBy = MaterialMasterOpeningBalanceDetail.AddedBy,
                                AddedDate = MaterialMasterOpeningBalanceDetail.AddedDate,
                                AddedFromIP = MaterialMasterOpeningBalanceDetail.AddedFromIP,
                                Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
                                MaterialMasterOpeningBalanceDetailId = MaterialMasterOpeningBalanceDetail.Id,
                                FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                GLType = "AD",
                                Id = MakePK(MaterialMasterOpeningBalanceDetail.Id, 14, 2),
                                ModelState = ModelState.Added,
                                OpeningBalanceId = openingBalance.Id,
                                ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
                                ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
                            });
                        }

                        _materialMasterOpeningBalanceDetailRepository.Insert(MaterialMasterOpeningBalanceDetail);
                    }
                    else
                    {
                        var materialMasterOpeningBalanceDetailDb = fixedAssetOBDetailList.First(r => r.Id == materialMasterOpeningBalanceDetailVM.Id);
                        materialMasterOpeningBalanceDetailDb.AccumulatedDepreciationActivityId = null;
                        materialMasterOpeningBalanceDetailDb.AccumulatedDepreciationBudgetMasterId = null;
                        materialMasterOpeningBalanceDetailDb.AccumulatedDepreciationGLId = null;
                        materialMasterOpeningBalanceDetailDb.Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount;
                        materialMasterOpeningBalanceDetailDb.BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId;
                        materialMasterOpeningBalanceDetailDb.AssetActivityId = materialMasterOpeningBalanceDetailVM.AssetActivityId;
                        materialMasterOpeningBalanceDetailDb.AssetBudgetMasterId = materialMasterOpeningBalanceDetailVM.AssetBudgetMasterId;
                        materialMasterOpeningBalanceDetailDb.AssetGLId = materialMasterOpeningBalanceDetailVM.AssetGLId;
                        materialMasterOpeningBalanceDetailDb.MaterialStorageId = openingBalance.MaterialStorageId;
                        materialMasterOpeningBalanceDetailDb.Quantity = materialMasterOpeningBalanceDetailVM.Quantity;
                        materialMasterOpeningBalanceDetailDb.UpdatedBy = openingBalance.UpdatedBy;
                        materialMasterOpeningBalanceDetailDb.UpdatedDate = openingBalance.UpdatedDate;
                        materialMasterOpeningBalanceDetailDb.UpdatedFromIP = openingBalance.UpdatedFromIP;
                        _materialMasterOpeningBalanceDetailRepository.Update(materialMasterOpeningBalanceDetailDb);

                        //var InventoryReceivedId = materialMasterOpeningBalanceDetailDb.Where(r => r.MaterialMasterId == materialMasterOpeningBalanceDetailVM.MaterialMasterId && r.ArticleId == materialMasterOpeningBalanceDetailVM.ArticleId
                        //&& r.FirstCharacteristicsId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId && r.FirstCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId
                        //&& r.SecondCharacteristicsId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId && r.SecondCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId
                        //&& r.ThirdCharacteristicsId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId && r.ThirdCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId
                        //).FirstOrDefault();
                        //var sql1 = @"Select InventoryReceiveId from [TRN].[OpeningBalance] AS FOB 
                        //		LEFT JOIN TRN.InventoryReceive IR ON IR.OpeningBalanceId=FOB.Id
                        //		LEFT JOIN TRN.InventoryReceiveDetail IRD ON IR.Id=IRD.InventoryReceiveId 
                        //		where FOB.Id='" + openingBalance.Id +"'";
                        //var InventoryReceivesId = _inventoryReceiveRepository.ExecuteSqlCommand(sql1.ToString());
                        var InventoryReceiveId = _inventoryReceiveRepository.SqlQuery<string>($"Select InventoryReceiveId from [TRN].[OpeningBalance] AS FOB LEFT JOIN TRN.InventoryReceive IR ON IR.OpeningBalanceId = FOB.Id LEFT JOIN TRN.InventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId where FOB.Id = '{openingBalance.Id}'").First();
                        var InventoryReceiveDetailId = _inventoryReceiveDetailRepository.SqlQuery<string>($"Select Id from TRN.InventoryReceiveDetail where InventoryReceiveId = '" + InventoryReceiveId + "' AND InventoryMaterialId = '" + materialMasterOpeningBalanceDetailVM.InventoryReceivedId + "'").First();
                        var InventoryMaterialId = _inventoryReceiveDetailRepository.SqlQuery<string>($"Select InventoryMaterialId from TRN.InventoryReceiveDetail where Id = '{InventoryReceiveDetailId}'").First();


                        var inventoryReceive = new InventoryReceive
                        {
                            Id = InventoryReceiveId.ToString(),
                            OpeningBalanceId = openingBalance.Id,
                            CompanyGroupId = openingBalance.CompanyGroupId,
                            CompanyId = openingBalance.CompanyId,
                            PlantId = openingBalance.PlantId,
                            MaterialStorageId = openingBalance.MaterialStorageId,
                            CurrencyId = materialMasterOpeningBalanceDetailVMList.FirstOrDefault().CompanyCurrencyId,
                            DocRefNo = openingBalance.DocRefNo,
                            DocDate = openingBalance.DocDate,
                            GateEntryNo = null,
                            EntryDate = openingBalance.AddedDate,
                            GRNDate = openingBalance.PostingDate,//AddedDate,
                            FixedAssetOrInventory = "Inventory",
                            PODepended = false,
                            AlongwithInvoice = false,
                            BaseNoOfDays = 0,
                            BaseCurrencyId = materialMasterOpeningBalanceDetailVMList.FirstOrDefault().CompanyCurrencyId,
                            IsNonCreditable = false,
                            ToCurrencyRate = 0,
                            IsTaxApplicable = false,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            GRNType = "OpeningBalance"
                        };
                        _inventoryReceiveRepository.Update(inventoryReceive);

                        //var fixedAssetOBDetailList = (from fd in _materialMasterOpeningBalanceDetailRepository.Query().Select()
                        //							  join f in _openingBalanceRepository.Query(r => r.CompanyGroupId == openingBalance.CompanyGroupId && r.CompanyId == openingBalance.CompanyId).Select() on fd.OpeningBalanceId equals f.Id
                        //							  select fd).ToList();
                        //var currentRecord = 0;
                        var inventoryReceiveDetails = new InventoryReceiveDetail
                        {
                            Id = materialMasterOpeningBalanceDetailVM.InventoryReceiveDetailId,//InventoryReceiveDetailId.ToString(),
                            MaterialStorageId = openingBalance.MaterialStorageId,
                            InventoryReceiveId = InventoryReceiveId.ToString(),
                            InventoryMaterialId = InventoryMaterialId.ToString(),
                            TransactionQty = materialMasterOpeningBalanceDetailVM.Quantity,
                            BaseQty = materialMasterOpeningBalanceDetailVM.Quantity,
                            TransactionUoMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            BaseUOMId = materialMasterOpeningBalanceDetailVM.BaseUOMId,
                            BaseUoMFactor = materialMasterOpeningBalanceDetailVM.Quantity,
                            MaterialTranRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            MaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalMaterialTranAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalMaterialBooksCurrencyAmount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                            TotalTaxAmount = 0,
                            ChargesTranAmount = 0,
                            BaseIssueQty = 0,
                            TrnCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            BooksCurrencyBaseRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount / materialMasterOpeningBalanceDetailVM.Quantity,
                            AddedBy = openingBalance.AddedBy,
                            AddedDate = openingBalance.AddedDate,
                            AddedFromIP = openingBalance.AddedFromIP,
                            MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailVM.Id,
                            LotNumber = materialMasterOpeningBalanceDetailVM.LotNumber,
                            Diameter = materialMasterOpeningBalanceDetailVM.Diameter,
                            Type = materialMasterOpeningBalanceDetailVM.Type
                        };
                        _inventoryReceiveDetailRepository.Update(inventoryReceiveDetails);
                        var Res = materialDbData.Where(r => r.MaterialMasterId == materialMasterOpeningBalanceDetailVM.MaterialMasterId && r.ArticleId == materialMasterOpeningBalanceDetailVM.ArticleId
                        && r.FirstCharacteristicsId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId && r.FirstCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId
                        && r.SecondCharacteristicsId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId && r.SecondCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId
                        && r.ThirdCharacteristicsId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId && r.ThirdCharacteristicsValueId == materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId).FirstOrDefault();

                        var TotalPreviuousAmountForMAtArtChar = (((Res.TotalQty * Res.AvgRate) - materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmountOld) + inventoryReceiveDetails.TotalMaterialTranAmount);
                        var TotalQty = ((Res.TotalQty - materialMasterOpeningBalanceDetailVM.QuantityOld) + inventoryReceiveDetails.TransactionQty);
                        var AvgRate = TotalPreviuousAmountForMAtArtChar / TotalQty;
                        //builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + ((Res.TotalQty - materialMasterOpeningBalanceDetailVM.QuantityOld) + materialMasterOpeningBalanceDetailVM.Quantity) + @"' 
                        //					 WHERE MaterialMasterId='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "' " +
                        //						"AND ArticleId='" + materialMasterOpeningBalanceDetailVM.ArticleId + "' " +
                        //						"AND Isnull(FirstCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId + "' " +
                        //						"AND Isnull(FirstCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId + "' " +
                        //						"AND Isnull(SecondCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId + "' " +
                        //						"AND Isnull(SecondCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId + "' " +
                        //						"AND Isnull(ThirdCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId + "' " +
                        //						"AND Isnull(ThirdCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId + "'";
                        builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + TotalQty + "',AvgRate='" + AvgRate + @"'
											 WHERE MaterialMasterId='" + materialMasterOpeningBalanceDetailVM.MaterialMasterId + "' " +
                                                "AND ArticleId='" + materialMasterOpeningBalanceDetailVM.ArticleId + "' " +
                                                "AND Isnull(FirstCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsId + "' " +
                                                "AND Isnull(FirstCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.FirstCharacteristicsValueId + "' " +
                                                "AND Isnull(SecondCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsId + "' " +
                                                "AND Isnull(SecondCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.SecondCharacteristicsValueId + "' " +
                                                "AND Isnull(ThirdCharacteristicsId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsId + "' " +
                                                "AND Isnull(ThirdCharacteristicsValueId,'')='" + materialMasterOpeningBalanceDetailVM.ThirdCharacteristicsValueId + "'";
                        rdBuilder.Append(builderSql);
                        //_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        // Set company currency.
                        if (!string.IsNullOrWhiteSpace(companyCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            var ccDetailFA = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId && r.GLType == "FA");
                            if (null != ccDetailFA)
                            {
                                ccDetailFA.Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount;
                                ccDetailFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId;
                                ccDetailFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId;
                                ccDetailFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion;
                                ccDetailFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                ccDetailFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate;
                                ccDetailFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                ccDetailFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                ccDetailFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(ccDetailFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "FA",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 1, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyCurrencyRate
                                });
                            }

                            // AD
                            var ccDetailAD = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId && r.GLType == "AD");
                            if (null != ccDetailAD)
                            {
                                ccDetailAD.Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount;
                                ccDetailAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId;
                                ccDetailAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId;
                                ccDetailAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion;
                                ccDetailAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                ccDetailAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate;
                                ccDetailAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                ccDetailAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                ccDetailAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(ccDetailAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "AD",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 2, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                                });
                            }
                        }

                        // Set company Group currency.
                        if (!string.IsNullOrWhiteSpace(companyGroupCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            var cgDetailFA = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyGroupCurrencyId && r.GLType == "FA");
                            if (null != cgDetailFA)
                            {
                                cgDetailFA.Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount;
                                cgDetailFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetailFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                cgDetailFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion;
                                cgDetailFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                cgDetailFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate;
                                cgDetailFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                cgDetailFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                cgDetailFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(cgDetailFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId,
                                    GLType = "FA",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 7, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FACompanyGroupCurrencyRate
                                });
                            }

                            // AD
                            var cgDetailAD = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == companyGroupCurrencyId && r.GLType == "AD");
                            if (null != cgDetailAD)
                            {
                                cgDetailAD.Amount = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyAmount;
                                cgDetailAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetailAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                cgDetailAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyConversion;
                                cgDetailAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                cgDetailAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyGroupCurrencyRate;
                                cgDetailAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                cgDetailAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                cgDetailAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(cgDetailAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyFromCurrencyId,
                                    GLType = "AD",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 8, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADCompanyCurrencyRate
                                });
                            }
                        }

                        // Set company hard currency.
                        if (!string.IsNullOrWhiteSpace(hardCurrencyId))
                        {
                            //Detail Currency
                            // FA
                            var hcDetailFA = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == hardCurrencyId && r.GLType == "FA");
                            if (null != hcDetailFA)
                            {
                                hcDetailFA.Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount;
                                hcDetailFA.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                hcDetailFA.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                hcDetailFA.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion;
                                hcDetailFA.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                hcDetailFA.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate;
                                hcDetailFA.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                hcDetailFA.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                hcDetailFA.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(hcDetailFA);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.FAHardCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                    GLType = "FA",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 13, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.FAHardCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.FAHardCurrencyRate
                                });
                            }

                            // AD
                            var hcDetailAD = fixedAssetOBDetailCurrencyList.FirstOrDefault(r => r.MaterialMasterOpeningBalanceDetailId == materialMasterOpeningBalanceDetailVM.Id && r.ParallelCurrencyId == hardCurrencyId && r.GLType == "AD");
                            if (null != hcDetailAD)
                            {
                                hcDetailAD.Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount;
                                hcDetailAD.FromCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupFromCurrencyId;
                                hcDetailAD.ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.CompanyGroupCurrencyId;
                                hcDetailAD.ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion;
                                hcDetailAD.ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId;
                                hcDetailAD.ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate;
                                hcDetailAD.UpdatedBy = materialMasterOpeningBalanceDetailDb.UpdatedBy;
                                hcDetailAD.UpdatedDate = materialMasterOpeningBalanceDetailDb.UpdatedDate;
                                hcDetailAD.UpdatedFromIP = materialMasterOpeningBalanceDetailDb.UpdatedFromIP;
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Update(hcDetailAD);
                            }
                            else
                            {
                                _materialMasterOpeningBalanceDetailCurrencyRepository.Insert(new MaterialMasterOpeningBalanceDetailCurrency
                                {
                                    AddedBy = materialMasterOpeningBalanceDetailDb.AddedBy,
                                    AddedDate = materialMasterOpeningBalanceDetailDb.AddedDate,
                                    AddedFromIP = materialMasterOpeningBalanceDetailDb.AddedFromIP,
                                    Amount = materialMasterOpeningBalanceDetailVM.ADHardCurrencyAmount,
                                    MaterialMasterOpeningBalanceDetailId = materialMasterOpeningBalanceDetailDb.Id,
                                    FromCurrencyId = materialMasterOpeningBalanceDetailVM.HardFromCurrencyId,
                                    GLType = "AD",
                                    Id = MakePK(materialMasterOpeningBalanceDetailDb.Id, 14, 2),
                                    ModelState = ModelState.Added,
                                    OpeningBalanceId = openingBalance.Id,
                                    ParallelCurrencyId = materialMasterOpeningBalanceDetailVM.HardCurrencyId,
                                    ToCurrencyConversion = materialMasterOpeningBalanceDetailVM.ADHardCurrencyConversion,
                                    ToCurrencyId = materialMasterOpeningBalanceDetailVM.ToCurrencyId,
                                    ToCurrencyRate = materialMasterOpeningBalanceDetailVM.ADHardCurrencyRate
                                });
                            }
                        }
                    }


                }
                _unitOfWork.SaveChanges();
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteMaterialMaster(string id)
        {
            var flag = false;
            try
            {
                ModifyCheck(id.ToString());
                _unitOfWork.BeginTransaction();
                flag = true;
                var sql = $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] WHERE OpeningBalanceId='{id}';" +
                    $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] WHERE OpeningBalanceId='{id}';" +
                    $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetail] WHERE OpeningBalanceId='{id}';" +
                    $"DELETE FROM [TRN].[OpeningBalance] WHERE Id='{id}'";
                _openingBalanceDetailRepository.ExecuteSqlCommand(sql);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public string PostNonFinancialMaterialOB(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.MaterialMasterOpeningBalanceDetailId != null)
                    {
                        var materialOB = _materialMasterOpeningBalanceDetailRepository.Find(voucherDetailVM.MaterialMasterOpeningBalanceDetailId);
                        if (materialOB != null)
                        {

                            var ob = _openingBalanceRepository.Find(materialOB.OpeningBalanceId);
                            ob.IsPark = false;
                            ob.IsPosted = true;
                            _openingBalanceRepository.Update(ob);
                            var grn = _inventoryReceiveRepository.Query(r => r.OpeningBalanceId == materialOB.OpeningBalanceId).Select().FirstOrDefault();

                            if (grn != null)
                            {
                                grn.Status = "Posting";
                                grn.IsApproved = true;
                                //grn.CheckedBy = "";
                                grn.CheckedByStatus = "Checked";
                                //grn.AuthorizedBy = "";
                                grn.AuthorizedByStatus = "Approval";
                                AuditService.UpdatedLog(grn);
                                _inventoryReceiveRepository.Update(grn);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return "Material Opening Balance";
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, voucherVM.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public GridModel GetNonFinancialMaterialPostedList(GridParameter parameters, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT distinct  OB.Id, OB.Id AS OpeningBalanceId, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, OBD.CurrencyId,  OB.PostingDate, OB.DocRefNo, OB.DocDate
                                , OB.Narration, OB.IsPark, SUM(OBD.DrAmount) DrAmount, SUM(OBD.CrAmount) CrAmount, OB.AddedBy, OB.AddedDate, OB.AddedFromIP, V.VoucherNo
                                FROM [TRN].[OpeningBalanceDetail] AS OBD
								LEFT JOIN TRN.OpeningBalance AS OB ON OB.Id= OBD.OpeningBalanceId
                                LEFT JOIN TRN.Voucher AS V ON V.Id=OB.VoucherId
                                WHERE OB.Archive=0 AND OB.[SourceType]='" + SourceType.OpeningBalance + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + @"'
                                AND OB.IsFinancial=0 and OB.IsPark=0
                                GROUP BY OB.Id, OB.CompanyGroupId, OB.CompanyId,  OB.PostingDate, OB.DocRefNo, OB.DocDate, OB.Narration
								, OB.IsPark, OBD.CurrencyId, OB.PlantId, OB.EntityId ,OB.AddedBy, OB.AddedDate, OB.AddedFromIP, V.VoucherNo ";
            return _sqlRepository.GetGridData(parameters);
        }

        #endregion Material Master

        public void DeleteOPDetail(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var sql = $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] WHERE MaterialMasterOpeningBalanceDetailId='{id}';" +
                    $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] WHERE MaterialMasterOpeningBalanceDetailId='{id}';" +
                    $"DELETE FROM [TRN].[MaterialMasterOpeningBalanceDetail] WHERE Id='{id}';";
                _openingBalanceDetailRepository.ExecuteSqlCommand(sql);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion FixedAsset

        public List<Dictionary<string, object>> GetMMOpeningBalanceDetailList(string companyId, string openingBalanceId)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                          SELECT FOBD.Id, FOBD.OpeningBalanceId,AGL.AccountCode+' - '+AGL.UserName AS AssetGLName, ACGL.AccountCode+' - '+ACGL.UserName AS AccDepreciation
							       ,FOBD.AccumulatedDepreciationGLId,FOBD.AccumulatedDepreciationBudgetId,FOBD.AccumulatedDepreciationActivityId,AB.UserName BudgetName,ACB.UserName ACUBudgetName
								   , FOBD.FixedAssetMasterId, FAM.UserName AS FixedAssetMasterName, FOBD.MaterialMasterId, FOBD.BaseUOMId, UOM.UserName AS BaseUOMName, FOBD.AssetGLId, FOBD.AccumulatedDepreciationGLId, FOBD.CurrencyId, FOBD.Quantity,
                                    CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.FACompanyCurrencyRate, CC.FACompanyCurrencyAmount, ADCC.ADCompanyCurrencyRate, ADCC.ADCompanyCurrencyAmount,
                                    GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.FACompanyGroupCurrencyRate, GC.FACompanyGroupCurrencyAmount, ADGC.ADCompanyGroupCurrencyRate, ADGC.ADCompanyGroupCurrencyAmount,
                                    HC.HardCurrencyId, HC.HardFromCurrencyId, HC.FAHardCurrencyRate, HC.FAHardCurrencyAmount, ADHC.ADHardCurrencyRate, ADHC.ADHardCurrencyAmount
									,CCD.DirectQuantity,CCD.FACompanyCurrencyDirectRate, CCD.FACompanyCurrencyDirectAmount
									,CCID.InDirectQuantity,CCID.FACompanyCurrencyInDirectRate, CCID.FACompanyCurrencyInDirectAmount
									,GCD.DirectQuantity,GCD.FACompanyGroupCurrencyDirectRate, GCD.FACompanyGroupCurrencyDirectAmount
									,GCID.InDirectQuantity,GCID.FACompanyGroupCurrencyInDirectRate, GCID.FACompanyGroupCurrencyInDirectAmount
									,HCD.DirectQuantity,HCD.FAHardCurrencyDirectRate, HCD.FAHardCurrencyDirectAmount
									,HCID.InDirectQuantity,HCID.FAHardCurrencyInDirectRate, HCID.FAHardCurrencyInDirectAmount
									,ADCCD.DirectQuantity,ADCCD.FACompanyCurrencyDirectRate, ADCCD.FACompanyCurrencyDirectAmount
									,ADCCID.InDirectQuantity,ADCCID.FACompanyCurrencyInDirectRate, ADCCID.FACompanyCurrencyInDirectAmount
									,ADGCD.DirectQuantity,GCD.FACompanyGroupCurrencyDirectRate, GCD.FACompanyGroupCurrencyDirectAmount
									,ADGCID.InDirectQuantity,ADGCID.FACompanyGroupCurrencyInDirectRate, ADGCID.FACompanyGroupCurrencyInDirectAmount
									,ADHCD.DirectQuantity,ADHCD.FAHardCurrencyDirectRate, ADHCD.FAHardCurrencyDirectAmount
									,ADHCID.InDirectQuantity,ADHCID.FAHardCurrencyInDirectRate, ADHCID.FAHardCurrencyInDirectAmount
                                    FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                                    LEFT JOIN [TRN].[OpeningBalance] AS FOB ON FOBD.OpeningBalanceId=FOB.Id
                                    --LEFT JOIN MST.MaterialMaster AS FAT ON FAT.Id = FOBD.MaterialMasterId
                                    LEFT JOIN MST.FixedAssetMaster AS FAM ON FAM.Id=FOBD.FixedAssetMasterId
									LEFT JOIN [SCS].[UnitOfMeasurement] AS UOM ON UOM.Id=FOBD.BaseUOMId
									LEFT JOIN HKP.GLGeneralInfo AGL ON FOBD.AssetGLId=AGL.Id
									LEFT JOIN HKP.GLGeneralInfo ACGL ON FOBD.AccumulatedDepreciationGLId=ACGL.Id
									LEFT JOIN MST.BudgetMaster BM ON FOBD.AssetBudgetMasterId=BM.Id
                                    LEFT JOIN HKP.Budget AB ON BM.BudgetId=AB.Id
									LEFT JOIN HKP.Budget ACB ON FOBD.AccumulatedDepreciationBudgetId=ACB.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyRate, OBDC.Amount AS FACompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                                    ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyRate, OBDC.Amount AS FACompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                                    ) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FAHardCurrencyRate, OBDC.Amount AS FAHardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                                    ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyCurrencyRate, OBDC.Amount AS ADCompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADCC ON ADCC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyGroupCurrencyRate, OBDC.Amount AS ADCompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADGC ON ADGC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADHardCurrencyRate, OBDC.Amount AS ADHardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADHC ON ADHC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									--DirectInDirect
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyDirectRate, OBDC.Amount AS FACompanyCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS CCD ON CCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyInDirectRate, OBDC.Amount AS FACompanyCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS CCID ON CCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyDirectRate, OBDC.Amount AS FACompanyGroupCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS GCD ON GCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyInDirectRate, OBDC.Amount AS FACompanyGroupCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS GCID ON GCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity DirectQuantity,OBDC.ToCurrencyRate AS FAHardCurrencyDirectRate, OBDC.Amount AS FAHardCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS HCD ON HCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
																		 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity InDirectQuantity,OBDC.ToCurrencyRate AS FAHardCurrencyInDirectRate, OBDC.Amount AS FAHardCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS HCID ON HCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyDirectRate, OBDC.Amount AS FACompanyCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADCCD ON ADCCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyInDirectRate, OBDC.Amount AS FACompanyCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADCCID ON ADCCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyDirectRate, OBDC.Amount AS FACompanyGroupCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADGCD ON ADGCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyInDirectRate, OBDC.Amount AS FACompanyGroupCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADGCID ON ADGCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity DirectQuantity,OBDC.ToCurrencyRate AS FAHardCurrencyDirectRate, OBDC.Amount AS FAHardCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADHCD ON ADHCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
																		 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity InDirectQuantity,OBDC.ToCurrencyRate AS FAHardCurrencyInDirectRate, OBDC.Amount AS FAHardCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADHCID ON ADHCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                        WHERE FOBD.OpeningBalanceId='" + openingBalanceId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetOpeningBalanceDetailList(string companyId, string openingBalanceId, string sort)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT OBD.Id, OBD.OpeningBalanceId, OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName, OBD.CurrencyId, C.Code AS CurrencyCode
                            , OBD.PartyId, OBD.PartyType, P.Code AS PartyCode, P.UserName AS PartyName, OBD.BankMasterId, B.UserName AS BankName, BM.AccountTitle, EN.UserName AS EntityName
                            , OBD.EntityId, PL.UserName AS PlantName, OBD.PlantId, CO.UserName As CompanyName, OBD.CompanyId, OBD.CashMasterId, CM.UserName AS CashName, OBD.EmployeeId
                            , EI.EmployeeName AS EmployeeName, EI.EmployeeCode, Replace(CONVERT(VARCHAR(11), OBD.DocDate, 106), ' ', '-') AS DocDate, OBD.DocRefNo, OBD.Narration, OBD.Amount
                            , OBD.BaseOnDueDate, OBD.BaseNoOfDays, Replace(CONVERT(VARCHAR(11), OBD.RepaymentStartDate, 106), ' ', '-') AS RepaymentStartDate, OBD.LifeOfYear, OBD.NoOfInstallmentPerYear
                            , OBD.NoOfPaidInstallment, OBD.TotalNoOfInstallment, OBD.ProfitRate,  OBD.SanctionAmount, OBD.PartyPlantId,
                            CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyAmount,
                            GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyAmount,
                            HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyAmount, OBD.BankCurrencyId, OBD.CashCurrencyId, OBD.BankAmount
                            FROM [TRN].[OpeningBalanceDetail] AS OBD
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=OBD.CurrencyId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=OBD.PartyId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=OBD.BankMasterId
                            LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=OBD.CashMasterId
                            LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=OBD.EmployeeId
                            LEFT JOIN [ORG].Plant AS PL ON PL.Id=OBD.PlantId
							LEFT JOIN [ORG].Entity AS EN ON EN.Id=OBD.EntityId
                            LEFT JOIN [ORG].Company AS CO ON CO.Id=OBD.CompanyId
                            LEFT OUTER JOIN (
	                        SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.Amount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                        LEFT OUTER JOIN (
                        SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS CompanyGroupCurrencyRate, OBDC.Amount AS CompanyGroupCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.OpeningBalanceDetailId=OBD.Id
                        LEFT OUTER JOIN (
	                        SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId, OBDC.ToCurrencyId,
	                        OBDC.ToCurrencyRate AS HardCurrencyRate, OBDC.Amount AS HardCurrencyAmount, OBDC.OpeningBalanceDetailId
	                        FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
	                        INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.OpeningBalanceDetailId=OBD.Id
                        WHERE OBD.OpeningBalanceId='" + openingBalanceId + "' ORDER BY " + sort;
            return _sqlRepository.GetDataCollection(sql);
        }

        public GridModel QueryAsset(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.IsFinancial, OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount,OB.MaterialStorageId
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE OBDC.GLType='FA' AND C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.SourceType='" + sourceType + "' AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel Query(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId, string transactionType)
        {
            parameters.CmdText = @"SELECT OB.Id,OB.Id OpeningBalanceId, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.DocRefNo,OB.TransactionType, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.SourceType='" + sourceType + "' AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            if (!string.IsNullOrEmpty(transactionType))
                parameters.CmdText += " AND OB.TransactionType='" + transactionType + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetInterPlantInvestmentTakenList(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.SourceType='" + sourceType + "' AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }



        public GridModel GetInterLoanGivenList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.SourceType IN ('" + SourceType.InterCompanyLoanGiven + "', '" + SourceType.InterPlantLoanGiven + "') AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetInterInvestmentGivenList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.SourceType IN ('" + SourceType.InterCompanyInvestmentGiven + "', '" + SourceType.InterPlantInvestmentGiven + "') AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }


        public void Insert(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                Check(openingBalance);

                if (openingBalance.SourceType == SourceType.BankJournal.ToString())
                {
                    // Duplicate Bank checking.
                    var duplicate = openingBalanceDetailVMList.GroupBy(x => new { x.BankMasterId }).Where(x => x.Skip(1).Any());
                    if (duplicate.Any())
                    {
                        var bm = _bankMasterRepository.Find(duplicate.FirstOrDefault().Key.BankMasterId);
                        throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "bank account (" + bm.AccountTitle + ")"));
                    }
                }
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                openingBalance.Id = GetOpeningBalancePK(openingBalance);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO OpeningBalance TABLE
                openingBalance.IsPark = true;
                openingBalance.IsPosted = true;
                InsertGraph(openingBalance);

                AccountDeterminateGL(openingBalance, out string gl, out string budgetId, out string activityId);

                var existBankIds = _openingBalanceDetailRepository.Query(r => r.BankMasterId != null).Select(r => r.BankMasterId);
                var existCashIds = _openingBalanceDetailRepository.Query(r => r.CashMasterId != null).Select(r => r.CashMasterId);

                var currentRecord = 0;
                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    currentRecord++;
                    openingBalanceDetailVM.EntityId = openingBalance.EntityId;
                    openingBalanceDetailVM.PlantId = openingBalance.PlantId;

                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);
                    Insert(openingBalance, currentRecord, companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, existBankIds, existCashIds, gl, budgetId, activityId, openingBalanceDetailVM);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertInterLoanGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                Check(openingBalance);
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO OpeningBalance Asset side
                openingBalance.Id = GetOpeningBalancePK(openingBalance);
                openingBalance.IsPark = true;
                openingBalance.IsPosted = false;

                // INSERT INTO OpeningBalance Asset side
                var openingBalanceLiability = openingBalance.Copy<OpeningBalance>();
                openingBalanceLiability.Id = GetOpeningBalancePK(openingBalance);
                openingBalanceLiability.CompanyId = openingBalanceDetailVMList.FirstOrDefault()?.CompanyId;
                openingBalanceLiability.PlantId = openingBalanceDetailVMList.FirstOrDefault()?.PlantId;
                openingBalanceLiability.EntityId = openingBalanceDetailVMList.FirstOrDefault()?.EntityId;
                if (openingBalance.PartyType == PartyType.Company.ToString())
                {
                    var currencyId = openingBalanceDetailVMList.FirstOrDefault()?.CurrencyId;
                    // Checking right side company transaction currency
                    if (!_currencyTransactionService.Any(r => r.CompanyId == openingBalanceLiability.CompanyId && r.CurrencyId == currencyId))
                        throw new CustomException($"Inter company does not have ({_currencyService.Find(currencyId)?.Code}) as transaction currency!");
                    openingBalance.SourceType = SourceType.InterCompanyLoanGiven.ToString();
                    openingBalanceLiability.SourceType = SourceType.InterCompanyLoanTaken.ToString();
                }
                else
                {
                    openingBalance.SourceType = SourceType.InterPlantLoanGiven.ToString();
                    openingBalanceLiability.SourceType = SourceType.InterPlantLoanTaken.ToString();
                }

                openingBalance.RefId = openingBalanceLiability.Id;
                openingBalanceLiability.RefId = openingBalance.Id;

                InsertGraph(openingBalance);
                InsertGraph(openingBalanceLiability);

                var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND LTGGL.FinancingTypeId='" + openingBalance.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(glTemp?.AssetGLId) || string.IsNullOrEmpty(glTemp?.LiabilityGLId))
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");

                var currentRecord = 0;
                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    currentRecord++;
                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);

                    // INSERT INTO VOUCHER DETAIL
                    var openingBalanceDetail = new OpeningBalanceDetail
                    {
                        Id = MakePK(openingBalance.Id, currentRecord, 4),
                        OpeningBalanceId = openingBalance.Id,
                        PartyType = openingBalance.PartyType,
                        CompanyId = openingBalanceDetailVM.CompanyId,
                        PlantId = openingBalanceDetailVM.PlantId,
                        EntityId = openingBalanceDetailVM.EntityId,
                        PartyId = openingBalanceDetailVM.PartyId,
                        PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                        ModelState = ModelState.Added,
                        Amount = openingBalanceDetailVM.Amount,
                        CurrencyId = openingBalanceDetailVM.CurrencyId,
                        DocDate = openingBalanceDetailVM.DocDate,
                        DocRefNo = openingBalanceDetailVM.DocRefNo,
                        Narration = openingBalanceDetailVM.Narration,
                        BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                        BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,

                        RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                        LifeOfYear = openingBalanceDetailVM.LifeOfYear,
                        NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear,
                        TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment,
                        NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment,
                        ProfitRate = openingBalanceDetailVM.ProfitRate,
                        SanctionAmount = openingBalanceDetailVM.SanctionAmount,
                        GLGeneralInfoId = glTemp.AssetGLId,
                        BudgetMasterId = glTemp.AssetBudgetMasterId,
                        ActivityId = glTemp.AssetActivityId
                    };
                    AuditService.AddedLog(openingBalanceDetail);

                    var openingBalanceDetailLibility = openingBalanceDetail.Copy<OpeningBalanceDetail>();
                    openingBalanceDetailLibility.Id = MakePK(openingBalanceLiability.Id, currentRecord, 4);
                    openingBalanceDetailLibility.OpeningBalanceId = openingBalanceLiability.Id;
                    openingBalanceDetailLibility.CompanyId = openingBalance.PartyType == PartyType.Company.ToString() ? openingBalance.CompanyId : openingBalanceDetail.CompanyId;
                    openingBalanceDetailLibility.PlantId = openingBalance.PlantId;
                    openingBalanceDetailLibility.EntityId = openingBalance.EntityId;
                    openingBalanceDetailLibility.GLGeneralInfoId = glTemp.LiabilityGLId;
                    openingBalanceDetailLibility.BudgetMasterId = glTemp.LiabilityBudgetMasterId;
                    openingBalanceDetailLibility.ActivityId = glTemp.LiabilityActivityId;

                    openingBalanceDetail.RefId = openingBalanceDetailLibility.Id;
                    openingBalanceDetailLibility.RefId = openingBalanceDetail.Id;

                    _openingBalanceDetailRepository.Insert(openingBalanceDetail);
                    _openingBalanceDetailRepository.Insert(openingBalanceDetailLibility);

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        openingBalanceDetailVM.CompanyCurrencyId = companyCurrencyId;
                        var companyCurrencyLiability = InsertCompanyCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        companyCurrencyLiability.Id = openingBalanceDetailLibility.Id + 1;
                        companyCurrencyLiability.OpeningBalanceId = openingBalanceDetailLibility.OpeningBalanceId;
                        companyCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLibility.Id;
                        _openingBalanceDetailCurrencyRepository.Insert(companyCurrencyLiability);
                    }
                    // Set company Group currency.
                    if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                    {
                        var companyGroupCurrencyLiability = InsertCompanyGroupCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        companyGroupCurrencyLiability.Id = openingBalanceDetailLibility.Id + 2;
                        companyGroupCurrencyLiability.OpeningBalanceId = openingBalanceDetailLibility.OpeningBalanceId;
                        companyGroupCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLibility.Id;
                        _openingBalanceDetailCurrencyRepository.Insert(companyGroupCurrencyLiability);
                    }
                    // Set hard currency.
                    if (!string.IsNullOrEmpty(hardCurrencyId))
                    {
                        var hardCurrencyLiability = InsertHardCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        hardCurrencyLiability.Id = openingBalanceDetailLibility.Id + 3;
                        hardCurrencyLiability.OpeningBalanceId = openingBalanceDetailLibility.OpeningBalanceId;
                        hardCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLibility.Id;
                        _openingBalanceDetailCurrencyRepository.Insert(hardCurrencyLiability);
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateInterLoanGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _unitOfWork.BeginTransaction();
                flag = true;
                var openingBalanceDb = Find(openingBalance.Id);

                // Checking and validation
                CheckIsPosted(openingBalanceDb);
                CheckWithPlant(openingBalance);

                openingBalanceDb.DocDate = openingBalance.DocDate;
                openingBalanceDb.DocRefNo = openingBalance.DocRefNo;
                openingBalanceDb.Narration = openingBalance.Narration;
                openingBalanceDb.FinancingTypeId = openingBalance.FinancingTypeId;
                openingBalanceDb.EntityId = openingBalance.EntityId;
                UpdateGraph(openingBalanceDb);

                var openingBalanceLiability = Find(openingBalanceDb.RefId);
                openingBalanceLiability.DocDate = openingBalance.DocDate;
                openingBalanceLiability.DocRefNo = openingBalance.DocRefNo;
                openingBalanceLiability.Narration = openingBalance.Narration;
                openingBalanceLiability.FinancingTypeId = openingBalance.FinancingTypeId;
                openingBalanceLiability.CompanyId = openingBalanceDetailVMList.FirstOrDefault()?.CompanyId;
                openingBalanceLiability.PlantId = openingBalanceDetailVMList.FirstOrDefault()?.PlantId;
                openingBalanceLiability.EntityId = openingBalanceDetailVMList.FirstOrDefault()?.EntityId;
                UpdateGraph(openingBalanceLiability);

                var openingBalanceDetailList = _openingBalanceDetailRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                var openingBalanceDetailCurrencyList = _openingBalanceDetailCurrencyRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND LTGGL.FinancingTypeId='" + openingBalance.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(glTemp?.AssetGLId) || string.IsNullOrEmpty(glTemp?.LiabilityGLId))
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");

                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);

                    var openingBalanceDetail = openingBalanceDetailList.First(r => r.Id == openingBalanceDetailVM.Id);
                    openingBalanceDetail.CompanyId = openingBalanceDetailVM.CompanyId;
                    openingBalanceDetail.PlantId = openingBalanceDetailVM.PlantId;
                    openingBalanceDetail.EntityId = openingBalanceDetailVM.EntityId;
                    openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                    openingBalanceDetail.PartyPlantId = openingBalanceDetailVM.PartyPlantId;
                    openingBalanceDetail.DocDate = openingBalanceDb.DocDate;
                    openingBalanceDetail.DocRefNo = openingBalanceDb.DocRefNo;
                    openingBalanceDetail.Narration = openingBalanceDetailVM.Narration;
                    openingBalanceDetail.RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate;
                    openingBalanceDetail.LifeOfYear = openingBalanceDetailVM.LifeOfYear;
                    openingBalanceDetail.NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear;
                    openingBalanceDetail.TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment;
                    openingBalanceDetail.NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment;
                    openingBalanceDetail.ProfitRate = openingBalanceDetailVM.ProfitRate;
                    openingBalanceDetail.CurrencyId = openingBalanceDetailVM.CurrencyId;
                    openingBalanceDetail.SanctionAmount = openingBalanceDetailVM.SanctionAmount;
                    openingBalanceDetail.Amount = openingBalanceDetailVM.Amount;
                    openingBalanceDetail.GLGeneralInfoId = glTemp.AssetGLId;
                    openingBalanceDetail.BudgetMasterId = glTemp.AssetBudgetMasterId;
                    openingBalanceDetail.ActivityId = glTemp.AssetActivityId;
                    AuditService.UpdatedLog(openingBalanceDetail);
                    _openingBalanceDetailRepository.Update(openingBalanceDetail);

                    var openingBalanceDetailLiability = _openingBalanceDetailRepository.Find(openingBalanceDetail.RefId);
                    openingBalanceDetailLiability.EntityId = openingBalanceDb.EntityId;
                    openingBalanceDetailLiability.DocDate = openingBalanceDb.DocDate;
                    openingBalanceDetailLiability.DocRefNo = openingBalanceDb.DocRefNo;
                    openingBalanceDetailLiability.Narration = openingBalanceDetailVM.Narration;
                    openingBalanceDetailLiability.RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate;
                    openingBalanceDetailLiability.LifeOfYear = openingBalanceDetailVM.LifeOfYear;
                    openingBalanceDetailLiability.NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear;
                    openingBalanceDetailLiability.TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment;
                    openingBalanceDetailLiability.NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment;
                    openingBalanceDetailLiability.ProfitRate = openingBalanceDetailVM.ProfitRate;
                    openingBalanceDetailLiability.CurrencyId = openingBalanceDetailVM.CurrencyId;
                    openingBalanceDetailLiability.SanctionAmount = openingBalanceDetailVM.SanctionAmount;
                    openingBalanceDetailLiability.Amount = openingBalanceDetailVM.Amount;
                    openingBalanceDetailLiability.GLGeneralInfoId = glTemp.LiabilityGLId;
                    openingBalanceDetailLiability.BudgetMasterId = glTemp.LiabilityBudgetMasterId;
                    openingBalanceDetailLiability.ActivityId = glTemp.LiabilityActivityId;
                    AuditService.UpdatedLog(openingBalanceDetailLiability);
                    _openingBalanceDetailRepository.Update(openingBalanceDetailLiability);

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        var ccDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId);
                        if (null != ccDetail)
                        {
                            ccDetail.FromCurrencyId = openingBalanceDetailVM.CompanyFromCurrencyId;
                            ccDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            ccDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyCurrencyRate;
                            ccDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion;
                            ccDetail.Amount = openingBalanceDetailVM.CompanyCurrencyAmount;
                            ccDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            ccDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            ccDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(ccDetail);
                        }
                    }

                    // Set company Group currency.
                    if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                    {
                        var cgDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId);
                        if (null != cgDetail)
                        {
                            cgDetail.FromCurrencyId = openingBalanceDetailVM.CompanyGroupFromCurrencyId;
                            cgDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            cgDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyGroupCurrencyRate;
                            cgDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyGroupCurrencyConversion;
                            cgDetail.Amount = openingBalanceDetailVM.CompanyGroupCurrencyAmount;
                            cgDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            cgDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            cgDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(cgDetail);
                        }
                    }

                    // Set hard currency.
                    if (!string.IsNullOrEmpty(hardCurrencyId))
                    {
                        var hcDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.HardCurrencyId);
                        if (null != hcDetail)
                        {
                            hcDetail.FromCurrencyId = openingBalanceDetailVM.HardFromCurrencyId;
                            hcDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            hcDetail.ToCurrencyRate = openingBalanceDetailVM.HardCurrencyRate;
                            hcDetail.ToCurrencyConversion = openingBalanceDetailVM.HardCurrencyConversion;
                            hcDetail.Amount = openingBalanceDetailVM.HardCurrencyAmount;
                            hcDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            hcDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            hcDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(hcDetail);
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertInterInvestmentGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                Check(openingBalance);
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO OpeningBalance Asset side
                openingBalance.Id = GetOpeningBalancePK(openingBalance);
                openingBalance.IsPark = true;
                openingBalance.IsPosted = false;

                // INSERT INTO OpeningBalance Asset side
                var openingBalanceLiability = openingBalance.Copy<OpeningBalance>();
                openingBalanceLiability.Id = GetOpeningBalancePK(openingBalance);
                openingBalanceLiability.CompanyId = openingBalanceDetailVMList.FirstOrDefault()?.CompanyId;
                openingBalanceLiability.PlantId = openingBalanceDetailVMList.FirstOrDefault()?.PlantId;
                openingBalanceLiability.EntityId = openingBalanceDetailVMList.FirstOrDefault()?.EntityId;

                if (openingBalance.PartyType == PartyType.Company.ToString())
                {
                    var currencyId = openingBalanceDetailVMList.FirstOrDefault()?.CurrencyId;
                    // Checking right side company transaction currency
                    if (!_currencyTransactionService.Any(r => r.CompanyId == openingBalanceLiability.CompanyId && r.CurrencyId == currencyId))
                        throw new CustomException($"Inter company does not have ({_currencyService.Find(currencyId)?.Code}) as transaction currency!");
                    openingBalance.SourceType = SourceType.InterCompanyInvestmentGiven.ToString();
                    openingBalanceLiability.SourceType = SourceType.InterCompanyInvestmentTaken.ToString();
                }
                else
                {
                    openingBalance.SourceType = SourceType.InterPlantInvestmentGiven.ToString();
                    openingBalanceLiability.SourceType = SourceType.InterPlantInvestmentTaken.ToString();
                }

                openingBalance.RefId = openingBalanceLiability.Id;
                openingBalanceLiability.RefId = openingBalance.Id;

                InsertGraph(openingBalance);
                InsertGraph(openingBalanceLiability);

                var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND LTGGL.FinancingTypeId='" + openingBalance.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(glTemp?.AssetGLId) || string.IsNullOrEmpty(glTemp?.LiabilityGLId))
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");

                var currentRecord = 0;
                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    currentRecord++;
                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);

                    // INSERT INTO VOUCHER DETAIL
                    var openingBalanceDetail = new OpeningBalanceDetail
                    {
                        Id = MakePK(openingBalance.Id, currentRecord, 4),
                        OpeningBalanceId = openingBalance.Id,
                        PartyType = openingBalance.PartyType,
                        CompanyId = openingBalanceDetailVM.CompanyId,
                        PlantId = openingBalanceDetailVM.PlantId,
                        EntityId = openingBalanceDetailVM.EntityId,
                        PartyId = openingBalanceDetailVM.PartyId,
                        PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                        CurrencyId = openingBalanceDetailVM.CurrencyId,
                        ModelState = ModelState.Added,
                        Amount = openingBalanceDetailVM.Amount,
                        DocDate = openingBalanceDetailVM.DocDate,
                        DocRefNo = openingBalanceDetailVM.DocRefNo,
                        Narration = openingBalanceDetailVM.Narration,
                        BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                        BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,

                        RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                        GLGeneralInfoId = glTemp.AssetGLId,
                        BudgetMasterId = glTemp.AssetBudgetMasterId,
                        ActivityId = glTemp.AssetActivityId
                    };
                    AuditService.AddedLog(openingBalanceDetail);

                    var openingBalanceDetailLiability = openingBalanceDetail.Copy<OpeningBalanceDetail>();
                    openingBalanceDetailLiability.Id = MakePK(openingBalanceLiability.Id, currentRecord, 4);
                    openingBalanceDetailLiability.OpeningBalanceId = openingBalanceLiability.Id;
                    openingBalanceDetailLiability.CompanyId = openingBalance.PartyType == PartyType.Company.ToString() ? openingBalance.CompanyId : openingBalanceDetail.CompanyId;
                    openingBalanceDetailLiability.PlantId = openingBalance.PlantId;
                    openingBalanceDetailLiability.EntityId = openingBalance.EntityId;
                    openingBalanceDetailLiability.GLGeneralInfoId = glTemp.LiabilityGLId;
                    openingBalanceDetailLiability.BudgetMasterId = glTemp.LiabilityBudgetMasterId;
                    openingBalanceDetailLiability.ActivityId = glTemp.LiabilityActivityId;

                    openingBalanceDetail.RefId = openingBalanceDetailLiability.Id;
                    openingBalanceDetailLiability.RefId = openingBalanceDetail.Id;

                    _openingBalanceDetailRepository.Insert(openingBalanceDetail);
                    _openingBalanceDetailRepository.Insert(openingBalanceDetailLiability);

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        openingBalanceDetailVM.CompanyCurrencyId = companyCurrencyId;
                        var companyCurrencyLiability = InsertCompanyCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        if (openingBalance.PartyType != PartyType.Company.ToString())
                        {
                            companyCurrencyLiability.Id = openingBalanceDetailLiability.Id + 1;
                            companyCurrencyLiability.OpeningBalanceId = openingBalanceDetailLiability.OpeningBalanceId;
                            companyCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLiability.Id;
                            _openingBalanceDetailCurrencyRepository.Insert(companyCurrencyLiability);
                        }
                    }
                    // Set company Group currency.
                    if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                    {
                        var companyGroupCurrencyLiability = InsertCompanyGroupCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        if (openingBalance.PartyType != PartyType.Company.ToString())
                        {
                            companyGroupCurrencyLiability.Id = openingBalanceDetailLiability.Id + 2;
                            companyGroupCurrencyLiability.OpeningBalanceId = openingBalanceDetailLiability.OpeningBalanceId;
                            companyGroupCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLiability.Id;
                            _openingBalanceDetailCurrencyRepository.Insert(companyGroupCurrencyLiability);
                        }
                    }
                    // Set hard currency.
                    if (!string.IsNullOrEmpty(hardCurrencyId))
                    {
                        var hardCurrencyLiability = InsertHardCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        if (openingBalance.PartyType != PartyType.Company.ToString())
                        {
                            hardCurrencyLiability.Id = openingBalanceDetailLiability.Id + 3;
                            hardCurrencyLiability.OpeningBalanceId = openingBalanceDetailLiability.OpeningBalanceId;
                            hardCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLiability.Id;
                            _openingBalanceDetailCurrencyRepository.Insert(hardCurrencyLiability);
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateInterInvestmentGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;
                var openingBalanceDb = Find(openingBalance.Id);

                // Checking and validation
                CheckIsPosted(openingBalanceDb);
                CheckWithPlant(openingBalance);

                openingBalanceDb.DocDate = openingBalance.DocDate;
                openingBalanceDb.DocRefNo = openingBalance.DocRefNo;
                openingBalanceDb.EntityId = openingBalance.EntityId;
                openingBalanceDb.FinancingTypeId = openingBalance.FinancingTypeId;
                openingBalanceDb.Narration = openingBalance.Narration;
                UpdateGraph(openingBalanceDb);

                var openingBalanceLiability = Find(openingBalanceDb.RefId);
                openingBalanceLiability.DocDate = openingBalance.DocDate;
                openingBalanceLiability.DocRefNo = openingBalance.DocRefNo;
                openingBalanceLiability.Narration = openingBalance.Narration;
                openingBalanceLiability.FinancingTypeId = openingBalance.FinancingTypeId;
                openingBalanceLiability.CompanyId = openingBalanceDetailVMList.FirstOrDefault()?.CompanyId;
                openingBalanceLiability.PlantId = openingBalanceDetailVMList.FirstOrDefault()?.PlantId;
                openingBalanceLiability.EntityId = openingBalanceDetailVMList.FirstOrDefault()?.EntityId;
                UpdateGraph(openingBalanceLiability);

                var openingBalanceDetailList = _openingBalanceDetailRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                var openingBalanceDetailCurrencyList = _openingBalanceDetailCurrencyRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND LTGGL.FinancingTypeId='" + openingBalance.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(glTemp?.AssetGLId) || string.IsNullOrEmpty(glTemp?.LiabilityGLId))
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");

                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);

                    var openingBalanceDetail = openingBalanceDetailList.First(r => r.Id == openingBalanceDetailVM.Id);
                    openingBalanceDetail.CompanyId = openingBalanceDetailVM.CompanyId;
                    openingBalanceDetail.PlantId = openingBalanceDetailVM.PlantId;
                    openingBalanceDetail.EntityId = openingBalanceDetailVM.EntityId;
                    openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                    openingBalanceDetail.PartyPlantId = openingBalanceDetailVM.PartyPlantId;
                    openingBalanceDetail.Narration = openingBalanceDetailVM.Narration;
                    openingBalanceDetail.CurrencyId = openingBalanceDetailVM.CurrencyId;
                    openingBalanceDetail.Amount = openingBalanceDetailVM.Amount;
                    openingBalanceDetail.DocDate = openingBalanceDb.DocDate;
                    openingBalanceDetail.DocRefNo = openingBalanceDb.DocRefNo;
                    openingBalanceDetail.GLGeneralInfoId = glTemp.AssetGLId;
                    openingBalanceDetail.BudgetMasterId = glTemp.AssetBudgetMasterId;
                    openingBalanceDetail.ActivityId = glTemp.AssetActivityId;
                    AuditService.UpdatedLog(openingBalanceDetail);
                    _openingBalanceDetailRepository.Update(openingBalanceDetail);

                    var openingBalanceDetailLiability = _openingBalanceDetailRepository.Find(openingBalanceDetail.RefId);
                    openingBalanceDetailLiability.CompanyId = openingBalance.PartyType == PartyType.Company.ToString() ? openingBalance.CompanyId : openingBalanceDetail.CompanyId;
                    openingBalanceDetailLiability.PlantId = openingBalanceDetailVM.PlantId;
                    openingBalanceDetailLiability.EntityId = openingBalanceDetailVM.EntityId;
                    openingBalanceDetailLiability.Narration = openingBalanceDetailVM.Narration;
                    openingBalanceDetailLiability.CurrencyId = openingBalanceDetailVM.CurrencyId;
                    openingBalanceDetailLiability.Amount = openingBalanceDetailVM.Amount;
                    openingBalanceDetailLiability.DocDate = openingBalanceDb.DocDate;
                    openingBalanceDetailLiability.DocRefNo = openingBalanceDb.DocRefNo;
                    openingBalanceDetailLiability.GLGeneralInfoId = glTemp.LiabilityGLId;
                    openingBalanceDetailLiability.BudgetMasterId = glTemp.LiabilityBudgetMasterId;
                    openingBalanceDetailLiability.ActivityId = glTemp.LiabilityActivityId;
                    AuditService.UpdatedLog(openingBalanceDetailLiability);
                    _openingBalanceDetailRepository.Update(openingBalanceDetailLiability);

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        var ccDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId);
                        if (null != ccDetail)
                        {
                            ccDetail.FromCurrencyId = openingBalanceDetailVM.CompanyFromCurrencyId;
                            ccDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            ccDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyCurrencyRate;
                            ccDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion;
                            ccDetail.Amount = openingBalanceDetailVM.CompanyCurrencyAmount;
                            ccDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            ccDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            ccDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(ccDetail);
                        }
                    }

                    // Set company Group currency.
                    if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                    {
                        var cgDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId);
                        if (null != cgDetail)
                        {
                            cgDetail.FromCurrencyId = openingBalanceDetailVM.CompanyGroupFromCurrencyId;
                            cgDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            cgDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyGroupCurrencyRate;
                            cgDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyGroupCurrencyConversion;
                            cgDetail.Amount = openingBalanceDetailVM.CompanyGroupCurrencyAmount;
                            cgDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            cgDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            cgDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(cgDetail);
                        }
                    }

                    // Set hard currency.
                    if (!string.IsNullOrEmpty(hardCurrencyId))
                    {
                        var hcDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.HardCurrencyId);
                        if (null != hcDetail)
                        {
                            hcDetail.FromCurrencyId = openingBalanceDetailVM.HardFromCurrencyId;
                            hcDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            hcDetail.ToCurrencyRate = openingBalanceDetailVM.HardCurrencyRate;
                            hcDetail.ToCurrencyConversion = openingBalanceDetailVM.HardCurrencyConversion;
                            hcDetail.Amount = openingBalanceDetailVM.HardCurrencyAmount;
                            hcDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            hcDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            hcDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(hcDetail);
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertInterTransactionGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                Check(openingBalance);
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO OpeningBalance Asset side
                openingBalance.Id = GetOpeningBalancePK(openingBalance);
                openingBalance.IsPark = true;
                openingBalance.IsPosted = false;

                // INSERT INTO OpeningBalance Asset side
                var openingBalanceLiability = openingBalance.Copy<OpeningBalance>();
                openingBalanceLiability.Id = GetOpeningBalancePK(openingBalance);
                openingBalanceLiability.CompanyId = openingBalanceDetailVMList.FirstOrDefault()?.CompanyId;
                openingBalanceLiability.PlantId = openingBalanceDetailVMList.FirstOrDefault()?.PlantId;
                openingBalanceLiability.EntityId = openingBalanceDetailVMList.FirstOrDefault()?.EntityId;

                if (openingBalance.PartyType == PartyType.Company.ToString())
                {
                    var currencyId = openingBalanceDetailVMList.FirstOrDefault()?.CurrencyId;
                    // Checking right side company transaction currency
                    if (!_currencyTransactionService.Any(r => r.CompanyId == openingBalanceLiability.CompanyId && r.CurrencyId == currencyId))
                        throw new CustomException($"Inter company does not have ({_currencyService.Find(currencyId)?.Code}) as transaction currency!");
                    openingBalance.SourceType = SourceType.InterCompanyTransactionGiven.ToString();
                    openingBalanceLiability.SourceType = SourceType.InterCompanyTransactionTaken.ToString();
                }
                else
                {
                    openingBalance.SourceType = SourceType.InterPlantTransactionGiven.ToString();
                    openingBalanceLiability.SourceType = SourceType.InterPlantTransactionTaken.ToString();
                }

                openingBalance.RefId = openingBalanceLiability.Id;
                openingBalanceLiability.RefId = openingBalance.Id;

                InsertGraph(openingBalance);
                InsertGraph(openingBalanceLiability);

                var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND LTGGL.FinancingTypeId='" + openingBalance.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(glTemp?.AssetGLId) || string.IsNullOrEmpty(glTemp?.LiabilityGLId))
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");

                var currentRecord = 0;
                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    currentRecord++;
                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);

                    // INSERT INTO VOUCHER DETAIL
                    var openingBalanceDetail = new OpeningBalanceDetail
                    {
                        Id = MakePK(openingBalance.Id, currentRecord, 4),
                        OpeningBalanceId = openingBalance.Id,
                        PartyType = openingBalance.PartyType,
                        CompanyId = openingBalanceDetailVM.CompanyId,
                        PlantId = openingBalanceDetailVM.PlantId,
                        EntityId = openingBalanceDetailVM.EntityId,
                        PartyId = openingBalanceDetailVM.PartyId,
                        PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                        CurrencyId = openingBalanceDetailVM.CurrencyId,
                        ModelState = ModelState.Added,
                        Amount = openingBalanceDetailVM.Amount,
                        DocDate = openingBalanceDetailVM.DocDate,
                        DocRefNo = openingBalanceDetailVM.DocRefNo,
                        Narration = openingBalanceDetailVM.Narration,
                        BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                        BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,

                        RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                        GLGeneralInfoId = glTemp.AssetGLId,
                        BudgetMasterId = glTemp.AssetBudgetMasterId,
                        ActivityId = glTemp.AssetActivityId
                    };
                    AuditService.AddedLog(openingBalanceDetail);

                    var openingBalanceDetailLiability = openingBalanceDetail.Copy<OpeningBalanceDetail>();
                    openingBalanceDetailLiability.Id = MakePK(openingBalanceLiability.Id, currentRecord, 4);
                    openingBalanceDetailLiability.OpeningBalanceId = openingBalanceLiability.Id;
                    openingBalanceDetailLiability.CompanyId = openingBalance.PartyType == PartyType.Company.ToString() ? openingBalance.CompanyId : openingBalanceDetail.CompanyId;
                    openingBalanceDetailLiability.PlantId = openingBalance.PlantId;
                    openingBalanceDetailLiability.EntityId = openingBalance.EntityId;
                    openingBalanceDetailLiability.GLGeneralInfoId = glTemp.LiabilityGLId;
                    openingBalanceDetailLiability.BudgetMasterId = glTemp.LiabilityBudgetMasterId;
                    openingBalanceDetailLiability.ActivityId = glTemp.LiabilityActivityId;

                    openingBalanceDetail.RefId = openingBalanceDetailLiability.Id;
                    openingBalanceDetailLiability.RefId = openingBalanceDetail.Id;

                    _openingBalanceDetailRepository.Insert(openingBalanceDetail);
                    _openingBalanceDetailRepository.Insert(openingBalanceDetailLiability);

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        openingBalanceDetailVM.CompanyCurrencyId = companyCurrencyId;
                        var companyCurrencyLiability = InsertCompanyCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        if (openingBalance.PartyType != PartyType.Company.ToString())
                        {
                            companyCurrencyLiability.Id = openingBalanceDetailLiability.Id + 1;
                            companyCurrencyLiability.OpeningBalanceId = openingBalanceDetailLiability.OpeningBalanceId;
                            companyCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLiability.Id;
                            _openingBalanceDetailCurrencyRepository.Insert(companyCurrencyLiability);
                        }
                    }
                    // Set company Group currency.
                    if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                    {
                        var companyGroupCurrencyLiability = InsertCompanyGroupCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        if (openingBalance.PartyType != PartyType.Company.ToString())
                        {
                            companyGroupCurrencyLiability.Id = openingBalanceDetailLiability.Id + 2;
                            companyGroupCurrencyLiability.OpeningBalanceId = openingBalanceDetailLiability.OpeningBalanceId;
                            companyGroupCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLiability.Id;
                            _openingBalanceDetailCurrencyRepository.Insert(companyGroupCurrencyLiability);
                        }
                    }
                    // Set hard currency.
                    if (!string.IsNullOrEmpty(hardCurrencyId))
                    {
                        var hardCurrencyLiability = InsertHardCurrency(openingBalanceDetail, openingBalanceDetailVM).Copy<OpeningBalanceDetailCurrency>();
                        if (openingBalance.PartyType != PartyType.Company.ToString())
                        {
                            hardCurrencyLiability.Id = openingBalanceDetailLiability.Id + 3;
                            hardCurrencyLiability.OpeningBalanceId = openingBalanceDetailLiability.OpeningBalanceId;
                            hardCurrencyLiability.OpeningBalanceDetailId = openingBalanceDetailLiability.Id;
                            _openingBalanceDetailCurrencyRepository.Insert(hardCurrencyLiability);
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateInterTransactionGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;
                var openingBalanceDb = Find(openingBalance.Id);

                // Checking and validation
                CheckIsPosted(openingBalanceDb);
                CheckWithPlant(openingBalance);

                openingBalanceDb.DocDate = openingBalance.DocDate;
                openingBalanceDb.DocRefNo = openingBalance.DocRefNo;
                openingBalanceDb.EntityId = openingBalance.EntityId;
                openingBalanceDb.FinancingTypeId = openingBalance.FinancingTypeId;
                openingBalanceDb.Narration = openingBalance.Narration;
                UpdateGraph(openingBalanceDb);

                var openingBalanceLiability = Find(openingBalanceDb.RefId);
                openingBalanceLiability.DocDate = openingBalance.DocDate;
                openingBalanceLiability.DocRefNo = openingBalance.DocRefNo;
                openingBalanceLiability.Narration = openingBalance.Narration;
                openingBalanceLiability.FinancingTypeId = openingBalance.FinancingTypeId;
                openingBalanceLiability.CompanyId = openingBalanceDetailVMList.FirstOrDefault()?.CompanyId;
                openingBalanceLiability.PlantId = openingBalanceDetailVMList.FirstOrDefault()?.PlantId;
                openingBalanceLiability.EntityId = openingBalanceDetailVMList.FirstOrDefault()?.EntityId;
                UpdateGraph(openingBalanceLiability);

                var openingBalanceDetailList = _openingBalanceDetailRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                var openingBalanceDetailCurrencyList = _openingBalanceDetailCurrencyRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                                WHERE C.Id='" + openingBalance.CompanyId + "' AND LTGGL.FinancingTypeId='" + openingBalance.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (string.IsNullOrEmpty(glTemp?.AssetGLId) || string.IsNullOrEmpty(glTemp?.LiabilityGLId))
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");

                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);

                    var openingBalanceDetail = openingBalanceDetailList.First(r => r.Id == openingBalanceDetailVM.Id);
                    openingBalanceDetail.CompanyId = openingBalanceDetailVM.CompanyId;
                    openingBalanceDetail.PlantId = openingBalanceDetailVM.PlantId;
                    openingBalanceDetail.EntityId = openingBalanceDetailVM.EntityId;
                    openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                    openingBalanceDetail.PartyPlantId = openingBalanceDetailVM.PartyPlantId;
                    openingBalanceDetail.Narration = openingBalanceDetailVM.Narration;
                    openingBalanceDetail.CurrencyId = openingBalanceDetailVM.CurrencyId;
                    openingBalanceDetail.Amount = openingBalanceDetailVM.Amount;
                    openingBalanceDetail.DocDate = openingBalanceDb.DocDate;
                    openingBalanceDetail.DocRefNo = openingBalanceDb.DocRefNo;
                    openingBalanceDetail.GLGeneralInfoId = glTemp.AssetGLId;
                    openingBalanceDetail.BudgetMasterId = glTemp.AssetBudgetMasterId;
                    openingBalanceDetail.ActivityId = glTemp.AssetActivityId;
                    AuditService.UpdatedLog(openingBalanceDetail);
                    _openingBalanceDetailRepository.Update(openingBalanceDetail);

                    var openingBalanceDetailLiability = _openingBalanceDetailRepository.Find(openingBalanceDetail.RefId);
                    openingBalanceDetailLiability.CompanyId = openingBalance.PartyType == PartyType.Company.ToString() ? openingBalance.CompanyId : openingBalanceDetail.CompanyId;
                    openingBalanceDetailLiability.PlantId = openingBalanceDetailVM.PlantId;
                    openingBalanceDetailLiability.EntityId = openingBalanceDetailVM.EntityId;
                    openingBalanceDetailLiability.Narration = openingBalanceDetailVM.Narration;
                    openingBalanceDetailLiability.CurrencyId = openingBalanceDetailVM.CurrencyId;
                    openingBalanceDetailLiability.Amount = openingBalanceDetailVM.Amount;
                    openingBalanceDetailLiability.DocDate = openingBalanceDb.DocDate;
                    openingBalanceDetailLiability.DocRefNo = openingBalanceDb.DocRefNo;
                    openingBalanceDetailLiability.GLGeneralInfoId = glTemp.LiabilityGLId;
                    openingBalanceDetailLiability.BudgetMasterId = glTemp.LiabilityBudgetMasterId;
                    openingBalanceDetailLiability.ActivityId = glTemp.LiabilityActivityId;
                    AuditService.UpdatedLog(openingBalanceDetailLiability);
                    _openingBalanceDetailRepository.Update(openingBalanceDetailLiability);

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        var ccDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId);
                        if (null != ccDetail)
                        {
                            ccDetail.FromCurrencyId = openingBalanceDetailVM.CompanyFromCurrencyId;
                            ccDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            ccDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyCurrencyRate;
                            ccDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion;
                            ccDetail.Amount = openingBalanceDetailVM.CompanyCurrencyAmount;
                            ccDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            ccDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            ccDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(ccDetail);
                        }
                    }

                    // Set company Group currency.
                    if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                    {
                        var cgDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId);
                        if (null != cgDetail)
                        {
                            cgDetail.FromCurrencyId = openingBalanceDetailVM.CompanyGroupFromCurrencyId;
                            cgDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            cgDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyGroupCurrencyRate;
                            cgDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyGroupCurrencyConversion;
                            cgDetail.Amount = openingBalanceDetailVM.CompanyGroupCurrencyAmount;
                            cgDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            cgDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            cgDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(cgDetail);
                        }
                    }

                    // Set hard currency.
                    if (!string.IsNullOrEmpty(hardCurrencyId))
                    {
                        var hcDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.HardCurrencyId);
                        if (null != hcDetail)
                        {
                            hcDetail.FromCurrencyId = openingBalanceDetailVM.HardFromCurrencyId;
                            hcDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                            hcDetail.ToCurrencyRate = openingBalanceDetailVM.HardCurrencyRate;
                            hcDetail.ToCurrencyConversion = openingBalanceDetailVM.HardCurrencyConversion;
                            hcDetail.Amount = openingBalanceDetailVM.HardCurrencyAmount;
                            hcDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                            hcDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                            hcDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                            _openingBalanceDetailCurrencyRepository.Update(hcDetail);
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateInterPlantTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                CheckWithPlant(openingBalance);
                _unitOfWork.BeginTransaction();
                flag = true;
                var openingBalanceDb = Find(openingBalance.Id);
                if (openingBalanceDb.IsPosted)
                    throw new CustomException("Update is not allowed after Posted.");

                openingBalanceDb.IsPosted = true;
                openingBalanceDb.Narration = openingBalance.Narration;
                UpdateGraph(openingBalanceDb);

                // Update asset side investment side IsPosted flag.
                var openingBalanceAsset = Find(openingBalanceDb.RefId);
                openingBalanceAsset.IsPosted = true;
                UpdateGraph(openingBalanceAsset);

                var openingBalanceDetailList = _openingBalanceDetailRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    var openingBalanceDetailDb = openingBalanceDetailList.FirstOrDefault(r => r.Id == openingBalanceDetailVM.Id);
                    if (null != openingBalanceDetailDb)
                    {
                        openingBalanceDetailDb.Narration = openingBalanceDetailVM.Narration;
                        openingBalanceDetailDb.UpdatedBy = openingBalanceDb.UpdatedBy;
                        openingBalanceDetailDb.UpdatedDate = openingBalanceDb.UpdatedDate;
                        openingBalanceDetailDb.UpdatedFromIP = openingBalanceDb.UpdatedFromIP;
                        _openingBalanceDetailRepository.Update(openingBalanceDetailDb);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateInterCompanyTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                CheckWithPlant(openingBalance);
                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _unitOfWork.BeginTransaction();
                flag = true;
                var openingBalanceDb = Find(openingBalance.Id);
                if (openingBalanceDb.IsPosted)
                    throw new CustomException("Update is not allowed after Posted.");

                openingBalanceDb.IsPosted = true;
                openingBalanceDb.Narration = openingBalance.Narration;
                UpdateGraph(openingBalanceDb);

                // Update asset side investment side IsPosted flag.
                var openingBalanceAsset = Find(openingBalanceDb.RefId);
                openingBalanceAsset.IsPosted = true;
                UpdateGraph(openingBalanceAsset);

                var openingBalanceDetailList = _openingBalanceDetailRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                var openingBalanceDetailCurrencyList = _openingBalanceDetailCurrencyRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);
                    var openingBalanceDetail = openingBalanceDetailList.FirstOrDefault(r => r.Id == openingBalanceDetailVM.Id);
                    if (null != openingBalanceDetail)
                    {
                        openingBalanceDetail.Narration = openingBalanceDetailVM.Narration;
                        openingBalanceDetail.UpdatedBy = openingBalanceDb.UpdatedBy;
                        openingBalanceDetail.UpdatedDate = openingBalanceDb.UpdatedDate;
                        openingBalanceDetail.UpdatedFromIP = openingBalanceDb.UpdatedFromIP;
                        _openingBalanceDetailRepository.Update(openingBalanceDetail);

                        // Set company currency.
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            var ccDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId);
                            if (null != ccDetail)
                            {
                                ccDetail.FromCurrencyId = openingBalanceDetailVM.CompanyFromCurrencyId;
                                ccDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                                ccDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyCurrencyRate;
                                ccDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion;
                                ccDetail.Amount = openingBalanceDetailVM.CompanyCurrencyAmount;
                                ccDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                                ccDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                                ccDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                                _openingBalanceDetailCurrencyRepository.Update(ccDetail);
                            }
                            else
                                // Set company currency.
                                InsertCompanyCurrency(openingBalanceDetail, openingBalanceDetailVM);
                        }

                        // Set company Group currency.
                        if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                        {
                            var cgDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId);
                            if (null != cgDetail)
                            {
                                cgDetail.FromCurrencyId = openingBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                                cgDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyGroupCurrencyRate;
                                cgDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyGroupCurrencyConversion;
                                cgDetail.Amount = openingBalanceDetailVM.CompanyGroupCurrencyAmount;
                                cgDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                                cgDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                                cgDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                                _openingBalanceDetailCurrencyRepository.Update(cgDetail);
                            }
                            else
                                // Set company Group currency.
                                InsertCompanyGroupCurrency(openingBalanceDetail, openingBalanceDetailVM);
                        }

                        // Set hard currency.
                        if (!string.IsNullOrEmpty(hardCurrencyId))
                        {
                            var hcDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.HardCurrencyId);
                            if (null != hcDetail)
                            {
                                hcDetail.FromCurrencyId = openingBalanceDetailVM.HardFromCurrencyId;
                                hcDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                                hcDetail.ToCurrencyRate = openingBalanceDetailVM.HardCurrencyRate;
                                hcDetail.ToCurrencyConversion = openingBalanceDetailVM.HardCurrencyConversion;
                                hcDetail.Amount = openingBalanceDetailVM.HardCurrencyAmount;
                                hcDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                                hcDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                                hcDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                                _openingBalanceDetailCurrencyRepository.Update(hcDetail);
                            }
                            else
                                // Set hard currency.
                                InsertHardCurrency(openingBalanceDetail, openingBalanceDetailVM);
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteInter(string id)
        {
            var flag = false;
            try
            {
                CheckIsPosted(id.ToString());
                _unitOfWork.BeginTransaction();
                flag = true;
                var sql = $"DECLARE @refId varchar(10);" +
                    $"SELECT @refId=RefId FROM [TRN].[OpeningBalance] WHERE Id='{id}';" +
                    $"DELETE FROM [TRN].[OpeningBalanceDetailCurrency] WHERE OpeningBalanceId IN ('{id}', @refId);" +
                    $"DELETE FROM [TRN].[OpeningBalanceDetail] WHERE OpeningBalanceId IN ('{id}', @refId);" +
                    $"DELETE FROM [TRN].[OpeningBalance] WHERE Id IN ('{id}', @refId);";
                _openingBalanceDetailRepository.ExecuteSqlCommand(sql);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateInterPlantInvestmentTaken(OpeningBalance openingBalance)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var openingBalanceDb = Find(openingBalance.Id);
                openingBalanceDb.IsPosted = true;
                openingBalanceDb.Narration = openingBalance.Narration;
                Update(openingBalanceDb);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateInterCompanyInvestmentTaken(OpeningBalance openingBalance)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var openingBalanceDb = Find(openingBalance.Id);
                openingBalanceDb.IsPosted = true;
                openingBalanceDb.Narration = openingBalance.Narration;
                Update(openingBalanceDb);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetOpeningBalancePK(OpeningBalance openingBalance)
        {
            return GetAutoNumber("OpeningBalance", PKGeneratorEnum.Yearly, null, openingBalance.PostingDate);
        }

        public void Update(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            var flag = false;
            try
            {
                ModifyCheck(openingBalance.Id);
                Check(openingBalance);
                if (openingBalance.SourceType == SourceType.BankJournal.ToString())
                {
                    // Duplicate Bank checking.
                    var duplicate = openingBalanceDetailVMList.GroupBy(x => new { x.BankMasterId }).Where(x => x.Skip(1).Any());
                    if (duplicate.Any())
                    {
                        var bm = _bankMasterRepository.Find(duplicate.FirstOrDefault().Key.BankMasterId);
                        throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "bank account (" + bm.AccountTitle + ")"));
                    }
                }

                if (openingBalance.SourceType == SourceType.CashJournal.ToString())
                {
                    // Duplicate Bank checking.
                    var duplicate = openingBalanceDetailVMList.GroupBy(x => new { x.CashMasterId }).Where(x => x.Skip(1).Any());
                    if (duplicate.Any())
                    {
                        var cash = _cashMasterRepository.Find(duplicate.FirstOrDefault().Key.CashMasterId);
                        throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "cash account (" + cash.UserName + ")"));
                    }
                }

                _companyParallelCurrencyService.GetParallelCurrency(openingBalance.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;
                openingBalance.IsPark = true;
                openingBalance.IsPosted = true;

                AccountDeterminateGL(openingBalance, out string gl, out string budgetId, out string activityId);
                var existBankIds = _openingBalanceDetailRepository.Query(r => r.BankMasterId != null).Select(r => r.BankMasterId);
                var existCashIds = _openingBalanceDetailRepository.Query(r => r.CashMasterId != null).Select(r => r.CashMasterId);
                var openingBalanceDetailList = _openingBalanceDetailRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();
                var openingBalanceDetailCurrencyList = _openingBalanceDetailCurrencyRepository.Query(r => r.OpeningBalanceId == openingBalance.Id).Select().ToList();

                var currentRecord = _openingBalanceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.OpeningBalanceDetail WHERE OpeningBalanceId='{openingBalance.Id}'").First();
                foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
                {
                    openingBalanceDetailVM.EntityId = openingBalance.EntityId;
                    CurrencyExchange(companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, openingBalanceDetailVM);

                    if (string.IsNullOrEmpty(openingBalanceDetailVM.Id))
                    {
                        currentRecord++;
                        Insert(openingBalance, currentRecord, companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, existBankIds, existCashIds, gl, budgetId, activityId, openingBalanceDetailVM);
                    }
                    else
                    {
                        var openingBalanceDetail = openingBalanceDetailList.First(r => r.Id == openingBalanceDetailVM.Id);
                        openingBalanceDetail.Amount = openingBalanceDetailVM.Amount;
                        openingBalanceDetail.CurrencyId = openingBalanceDetailVM.CurrencyId;
                        openingBalanceDetail.BankCurrencyId = openingBalanceDetailVM.BankCurrencyId;
                        if (openingBalanceDetailVM.PartyType == "Bank")
                            openingBalanceDetail.BankAmount = openingBalanceDetailVM.BankCurrencyId == openingBalanceDetailVM.CurrencyId ? openingBalanceDetailVM.Amount : openingBalanceDetailVM.BankAmount;
                        else if (openingBalanceDetailVM.PartyType == "Cash")
                            openingBalanceDetail.BankAmount = openingBalanceDetailVM.CashCurrencyId == openingBalanceDetailVM.CurrencyId ? openingBalanceDetailVM.Amount : openingBalanceDetailVM.BankAmount;
                        else
                            openingBalanceDetail.BankAmount = 0;
                        openingBalanceDetail.CurrencyId = openingBalanceDetailVM.CurrencyId;
                        openingBalanceDetail.DocDate = openingBalanceDetailVM.DocDate;
                        openingBalanceDetail.DocRefNo = openingBalanceDetailVM.DocRefNo;
                        openingBalanceDetail.EntityId = openingBalanceDetailVM.EntityId;
                        openingBalanceDetail.Narration = openingBalanceDetailVM.Narration;
                        openingBalanceDetail.BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate;
                        openingBalanceDetail.BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays;
                        openingBalanceDetail.PartyPlantId = openingBalanceDetailVM.PartyPlantId;
                        openingBalanceDetail.LifeOfYear = openingBalanceDetailVM.LifeOfYear;
                        openingBalanceDetail.NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear;
                        openingBalanceDetail.NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment;
                        openingBalanceDetail.ProfitRate = openingBalanceDetailVM.ProfitRate;
                        openingBalanceDetail.RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate;
                        openingBalanceDetail.SanctionAmount = openingBalanceDetailVM.SanctionAmount;
                        openingBalanceDetail.TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment;
                        openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                        AuditService.UpdatedLog(openingBalanceDetail);



                        GLAssign(openingBalance, null, gl, budgetId, activityId, openingBalanceDetailVM, openingBalanceDetail);

                        // Set company currency.
                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            var ccDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == companyCurrencyId);
                            if (null != ccDetail)
                            {
                                ccDetail.FromCurrencyId = openingBalanceDetailVM.CompanyFromCurrencyId;
                                ccDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                                ccDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyCurrencyRate;
                                ccDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion;
                                ccDetail.Amount = openingBalanceDetailVM.CompanyCurrencyAmount;
                                ccDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                                ccDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                                ccDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                                _openingBalanceDetailCurrencyRepository.Update(ccDetail);
                            }
                            else
                                // Set company currency.
                                InsertCompanyCurrency(openingBalanceDetail, openingBalanceDetailVM);
                        }

                        // Set company Group currency.
                        if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                        {
                            var cgDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId);
                            if (null != cgDetail)
                            {
                                cgDetail.FromCurrencyId = openingBalanceDetailVM.CompanyGroupFromCurrencyId;
                                cgDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                                cgDetail.ToCurrencyRate = openingBalanceDetailVM.CompanyGroupCurrencyRate;
                                cgDetail.ToCurrencyConversion = openingBalanceDetailVM.CompanyGroupCurrencyConversion;
                                cgDetail.Amount = openingBalanceDetailVM.CompanyGroupCurrencyAmount;
                                cgDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                                cgDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                                cgDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                                _openingBalanceDetailCurrencyRepository.Update(cgDetail);
                            }
                            else
                                // Set company Group currency.
                                InsertCompanyGroupCurrency(openingBalanceDetail, openingBalanceDetailVM);
                        }

                        // Set hard currency.
                        if (!string.IsNullOrEmpty(hardCurrencyId))
                        {
                            var hcDetail = openingBalanceDetailCurrencyList.FirstOrDefault(r => r.OpeningBalanceDetailId == openingBalanceDetailVM.Id && r.ParallelCurrencyId == openingBalanceDetailVM.HardCurrencyId);
                            if (null != hcDetail)
                            {
                                hcDetail.FromCurrencyId = openingBalanceDetailVM.HardFromCurrencyId;
                                hcDetail.ToCurrencyId = openingBalanceDetailVM.ToCurrencyId;
                                hcDetail.ToCurrencyRate = openingBalanceDetailVM.HardCurrencyRate;
                                hcDetail.ToCurrencyConversion = openingBalanceDetailVM.HardCurrencyConversion;
                                hcDetail.Amount = openingBalanceDetailVM.HardCurrencyAmount;
                                hcDetail.UpdatedBy = openingBalanceDetail.UpdatedBy;
                                hcDetail.UpdatedDate = openingBalanceDetail.UpdatedDate;
                                hcDetail.UpdatedFromIP = openingBalanceDetail.UpdatedFromIP;
                                _openingBalanceDetailCurrencyRepository.Update(hcDetail);
                            }
                            else
                                // Set hard currency.
                                InsertHardCurrency(openingBalanceDetail, openingBalanceDetailVM);
                        }

                        _openingBalanceDetailRepository.Update(openingBalanceDetail);
                    }
                }

                if (openingBalanceDetailList.Count() > 0)
                {
                    if (openingBalanceDetailVMList == null)
                    {
                        foreach (var openingBalanceDetail in openingBalanceDetailList)
                        {
                            _openingBalanceDetailCurrencyRepository.Delete(openingBalanceDetailCurrencyList.Where(r => r.OpeningBalanceDetailId == openingBalanceDetail.Id));
                            _openingBalanceDetailRepository.Delete(openingBalanceDetail);
                        }
                    }
                    else
                    {
                        foreach (var openingBalanceDetail in openingBalanceDetailList)
                        {
                            if (!openingBalanceDetailVMList.Any(t => t.Id == openingBalanceDetail.Id))
                            {
                                _openingBalanceDetailCurrencyRepository.Delete(openingBalanceDetailCurrencyList.Where(r => r.OpeningBalanceDetailId == openingBalanceDetail.Id));
                                _openingBalanceDetailRepository.Delete(openingBalanceDetail);
                            }
                        }
                    }
                }
                UpdateGraph(openingBalance);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, openingBalance.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetOpeningBalance(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM TRN.OpeningBalance WHERE CompanyId= '" + companyId + @"' AND SourceType='" + SourceType.CustomerInvoice + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public override void Delete(object id)
        {
            var flag = false;
            try
            {
                ModifyCheck(id.ToString());
                _unitOfWork.BeginTransaction();
                flag = true;
                var sql = $"DELETE FROM [TRN].[OpeningBalanceDetailCurrency] WHERE OpeningBalanceId='{id}';" +
                    $"DELETE FROM [TRN].[OpeningBalanceDetail] WHERE OpeningBalanceId='{id}';" +
                    $"DELETE FROM [TRN].[OpeningBalance] WHERE Id='{id}'";
                _openingBalanceDetailRepository.ExecuteSqlCommand(sql);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void AccountDeterminateGL(OpeningBalance entity, out string gl, out string budgetMasterId, out string activityId)
        {
            gl = null;
            budgetMasterId = null;
            activityId = null;
            if (entity.SourceType == SourceType.Loan.ToString() && entity.TransactionType == TransactionType.LoanTaken.ToString())
            {
                var sql = @"SELECT TOP(1) LTTGL.* FROM [HKP].[FinancingTypeGL] AS LTTGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTTGL.COAId
                                WHERE C.Id='" + entity.CompanyId + "' AND LTTGL.FinancingTypeId='" + entity.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (null != glTemp && !string.IsNullOrEmpty(glTemp.LiabilityGLId))
                {
                    gl = glTemp.LiabilityGLId;
                    budgetMasterId = glTemp.LiabilityBudgetMasterId;
                    activityId = glTemp.LiabilityActivityId;
                }
                else
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");
            }
            else if (entity.SourceType == SourceType.Loan.ToString() && entity.TransactionType == TransactionType.LoanGiven.ToString())
            {
                var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                                WHERE C.Id='" + entity.CompanyId + "' AND LTGGL.FinancingTypeId='" + entity.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (null != glTemp && !string.IsNullOrEmpty(glTemp.AssetGLId))
                {
                    gl = glTemp.AssetGLId;
                    budgetMasterId = glTemp.AssetBudgetMasterId;
                    activityId = glTemp.AssetActivityId;
                }
                else
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");
            }
            else if (entity.SourceType == SourceType.Investment.ToString() && entity.TransactionType == TransactionType.InvestmentTaken.ToString())
            {
                var sql = @"SELECT TOP(1) LTTGL.* FROM [HKP].[FinancingTypeGL] AS LTTGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTTGL.COAId
                                WHERE C.Id='" + entity.CompanyId + "' AND LTTGL.FinancingTypeId='" + entity.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (null != glTemp && !string.IsNullOrEmpty(glTemp.LiabilityGLId))
                {
                    gl = glTemp.LiabilityGLId;
                    budgetMasterId = glTemp.LiabilityBudgetMasterId;
                    activityId = glTemp.LiabilityActivityId;
                }
                else
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");
            }
            else if (entity.SourceType == SourceType.Investment.ToString() && entity.TransactionType == TransactionType.InvestmentGiven.ToString())
            {
                var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                                INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                                WHERE C.Id='" + entity.CompanyId + "' AND LTGGL.FinancingTypeId='" + entity.FinancingTypeId + "'";
                var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
                if (null != glTemp && !string.IsNullOrEmpty(glTemp.AssetGLId))
                {
                    gl = glTemp.AssetGLId;
                    budgetMasterId = glTemp.AssetBudgetMasterId;
                    activityId = glTemp.AssetActivityId;
                }
                else
                    throw new CustomException("This Transaction Type Account Determinate GL not Found!");
            }
            else if (entity.SourceType == SourceType.EmployeeAdvance.ToString())
            {
                var glTemp = _employeeTransactionTypeGLService.GetEmployeeAdvanceGL(entity.CompanyId, entity.EmployeeTransactionTypeId);
                gl = glTemp.AdvanceGLId;
                budgetMasterId = glTemp.AdvanceBudgetMasterId;
                activityId = glTemp.AdvanceActivityId;
            }
            else if (entity.SourceType == SourceType.EmployeePayable.ToString())
            {
                var glTemp = _employeePayableService.GetEmployeePayableGL(entity.CompanyId, entity.EmployeeTransactionTypeId);
                gl = glTemp.PayableGLId;
                budgetMasterId = glTemp.PayableBudgetMasterId;
                activityId = glTemp.PayableActivityId;
            }
            else if (entity.SourceType == SourceType.SecurityDeposit.ToString() && entity.TransactionType == TransactionType.SecurityTaken.ToString())
            {
                var glTemp = _securityDepositService.GetLiabilityGL(entity.CompanyId, entity.FinancingTypeId);
                gl = glTemp.LiabilityGLId;
                budgetMasterId = glTemp.LiabilityBudgetMasterId;
                activityId = glTemp.LiabilityActivityId;
            }
            else if (entity.SourceType == SourceType.SecurityDeposit.ToString() && entity.TransactionType == TransactionType.SecurityGiven.ToString())
            {
                var glTemp = _securityDepositService.GetAssetGL(entity.CompanyId, entity.FinancingTypeId);
                gl = glTemp.AssetGLId;
                budgetMasterId = glTemp.AssetBudgetMasterId;
                activityId = glTemp.AssetActivityId;
            }
        }

        private void Insert(OpeningBalance openingBalance, int currentRecord, string companyCurrencyId, string companyGroupCurrencyId, string hardCurrencyId,
            IEnumerable<string> existBankIds, IEnumerable<string> existCashIds, string glId, string budgetMasterId, string activityId, VoucherDetailViewModel openingBalanceDetailVM)
        {
            // INSERT INTO VOUCHER DETAIL
            var openingBalanceDetail = new OpeningBalanceDetail
            {
                Id = MakePK(openingBalance.Id, currentRecord, 4),
                OpeningBalanceId = openingBalance.Id,
                ModelState = ModelState.Added,
                Amount = openingBalanceDetailVM.Amount,
                CurrencyId = openingBalanceDetailVM.CurrencyId,
                DocDate = openingBalanceDetailVM.DocDate,
                DocRefNo = openingBalanceDetailVM.DocRefNo,
                Narration = openingBalanceDetailVM.Narration,
                EntityId = openingBalanceDetailVM.EntityId,
                BaseNoOfDays = openingBalanceDetailVM.BaseNoOfDays,
                BaseOnDueDate = openingBalanceDetailVM.BaseOnDueDate,

                RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate,
                LifeOfYear = openingBalanceDetailVM.LifeOfYear,
                NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear,
                TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment,
                NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment,
                ProfitRate = openingBalanceDetailVM.ProfitRate,
                SanctionAmount = openingBalanceDetailVM.SanctionAmount,
            };
            AuditService.AddedLog(openingBalanceDetail);

            if (openingBalance.SourceType == SourceType.CustomerInvoice.ToString() || openingBalance.SourceType == SourceType.VendorInvoice.ToString())
            {
                openingBalanceDetail.PartyType = openingBalance.SourceType == SourceType.CustomerInvoice.ToString() ? PartyType.Customer.ToString() : PartyType.Vendor.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.GLGeneralInfoId))
                    throw new CustomException($"{openingBalanceDetail.PartyType} ({openingBalanceDetail.PartyId}) Reconciliation GL not Found!");
                openingBalanceDetail.PartyPlantId = openingBalanceDetailVM.PartyPlantId;

                openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                openingBalanceDetail.GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId;
                openingBalanceDetail.BudgetMasterId = openingBalanceDetailVM.BudgetMasterId;
                openingBalanceDetail.ActivityId = openingBalanceDetailVM.ActivityId;
            }
            else if (openingBalance.SourceType == SourceType.CustomerAdvance.ToString() || openingBalance.SourceType == SourceType.VendorAdvance.ToString())
            {
                openingBalanceDetail.PartyType = openingBalance.SourceType == SourceType.CustomerAdvance.ToString() ? PartyType.Customer.ToString() : PartyType.Vendor.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.GLGeneralInfoId))
                    throw new CustomException($"{openingBalanceDetail.PartyType} ({openingBalanceDetail.PartyId}) DownPayment GL not Found!");
                openingBalanceDetail.PartyPlantId = openingBalanceDetailVM.PartyPlantId;

                openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                openingBalanceDetail.GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId;
                openingBalanceDetail.BudgetMasterId = openingBalanceDetailVM.BudgetMasterId;
                openingBalanceDetail.ActivityId = openingBalanceDetailVM.ActivityId;
            }
            else if (openingBalance.SourceType == SourceType.InterCompanyInvestmentGiven.ToString() || openingBalance.SourceType == SourceType.InterPlantInvestmentGiven.ToString())
            {
                openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.GLGeneralInfoId))
                    throw new CustomException($"{openingBalanceDetail.PartyType} ({openingBalanceDetail.PartyId}) DownPayment GL not Found!");
                openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                openingBalanceDetail.GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId;
                openingBalanceDetail.BudgetMasterId = openingBalanceDetailVM.BudgetMasterId;
                openingBalanceDetail.ActivityId = openingBalanceDetailVM.ActivityId;
                openingBalanceDetail.EntityId = openingBalanceDetailVM.EntityId;
                openingBalanceDetail.CompanyId = openingBalanceDetailVM.CompanyId;
            }
            else if (openingBalance.SourceType == SourceType.InterCompanyInvestmentTaken.ToString() ||
                   openingBalance.SourceType == SourceType.InterPlantInvestmentTaken.ToString())
            {
                var givenOBDetaildata = _openingBalanceDetailRepository.Find(openingBalanceDetailVM.Id);
                givenOBDetaildata.RefId = openingBalanceDetail.Id;
                _openingBalanceDetailRepository.Update(givenOBDetaildata);

                openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.GLGeneralInfoId))
                    throw new CustomException($"{openingBalanceDetail.PartyType} ({openingBalanceDetail.PartyId}) DownPayment GL not Found!");
                openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                openingBalanceDetail.GLGeneralInfoId = openingBalanceDetailVM.GLGeneralInfoId;
                openingBalanceDetail.BudgetMasterId = openingBalanceDetailVM.BudgetMasterId;
                openingBalanceDetail.ActivityId = openingBalanceDetailVM.ActivityId;
                openingBalanceDetail.EntityId = openingBalanceDetailVM.EntityId;
                openingBalanceDetail.CompanyId = openingBalanceDetailVM.CompanyId;
                openingBalanceDetail.RefId = openingBalanceDetailVM.Id;
            }
            else if (openingBalance.SourceType == SourceType.BankJournal.ToString())
            {
                openingBalanceDetail.BankMasterId = openingBalanceDetailVM.BankMasterId;
                var bankMaster = _bankMasterRepository.Find(openingBalanceDetail.BankMasterId);
                if (existBankIds.Contains(openingBalanceDetail.BankMasterId))
                    throw new CustomException($"This bank account ({bankMaster.AccountTitle}) opening balance is already exist!");
                if (null != bankMaster && !string.IsNullOrEmpty(bankMaster.GLGeneralInfoId))
                {
                    openingBalanceDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    openingBalanceDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                    openingBalanceDetail.ActivityId = bankMaster.ActivityId;
                    openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                }
                else
                    throw new CustomException($"This bank account ({bankMaster.AccountNumber}) GL not Found!");
            }
            else if (openingBalance.SourceType == SourceType.CashJournal.ToString())
            {
                openingBalanceDetail.CashMasterId = openingBalanceDetailVM.CashMasterId;
                var cashMaster = _cashMasterRepository.Find(openingBalanceDetail.CashMasterId);
                if (existCashIds.Contains(openingBalanceDetail.CashMasterId))
                    throw new CustomException($"This cash account ({cashMaster.UserName}) opening balance is already exist!");
                if (null != cashMaster && !string.IsNullOrEmpty(cashMaster.GLGeneralInfoId))
                {
                    openingBalanceDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    openingBalanceDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                    openingBalanceDetail.ActivityId = cashMaster.ActivityId;
                    openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                }
                else
                    throw new CustomException($"This cash account ({cashMaster.UserName}) GL not Found!");
            }
            else if (openingBalance.SourceType == SourceType.SecurityDeposit.ToString())
            {
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    openingBalanceDetail.PartyPlantId = _partyPlantRepository.Query(r => r.PartyId == openingBalanceDetailVM.PartyId && r.IsDefault).Select().FirstOrDefault()?.Id;
                    openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                }
                else if (!string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                {
                    openingBalanceDetail.BankMasterId = openingBalanceDetailVM.BankMasterId;
                    openingBalanceDetail.BankCurrencyId = openingBalanceDetailVM.BankCurrencyId;
                    openingBalanceDetail.BankAmount = openingBalanceDetailVM.BankCurrencyId == openingBalanceDetailVM.CurrencyId ? openingBalanceDetailVM.Amount : openingBalanceDetailVM.BankAmount;
                }

                openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;


                openingBalanceDetail.GLGeneralInfoId = glId;
                openingBalanceDetail.BudgetMasterId = budgetMasterId;
                openingBalanceDetail.ActivityId = activityId;
                var securityDepositPk = _securityDepositService.GetMaxNumber(nameof(SecurityDeposit), PKGeneratorEnum.Yearly, null, openingBalance.PostingDate);

                securityDepositPk.MaxNumber++;
                var securityDeposit = new SecurityDeposit
                {
                    Id = openingBalance.PostingDate.Year + securityDepositPk.MaxNumber.ToString(),
                    CompanyGroupId = openingBalance.CompanyGroupId,
                    CompanyId = openingBalance.CompanyId,
                    PlantId = openingBalance.PlantId,
                    EntityId = openingBalanceDetailVM.EntityId,
                    BankMasterId = openingBalanceDetailVM.BankMasterId,
                    //FiscalYearPeriodId = openingBalance.FiscalYearPeriodId,
                    //TaxYearId = openingBalance.TaxYearId,
                    ///TaxYearPeriodId = openingBalance.TaxYearPeriodId,
                    CurrencyId = openingBalanceDetailVM.CurrencyId,
                    //VoucherId = openingBalance.Id,
                    //VoucherTypeId = openingBalance.VoucherTypeId,
                    PartyType = openingBalanceDetailVM.PartyType,
                    PartyId = openingBalanceDetailVM.PartyId,
                    PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                    EmployeeId = openingBalanceDetailVM.EmployeeId,
                    FinancingTypeId = openingBalance.FinancingTypeId,
                    OpeningBalanceId = openingBalance.Id,
                    VoucherDate = openingBalance.PostingDate,
                    PostingDate = openingBalance.PostingDate,
                    DocDate = openingBalanceDetailVM.DocDate,
                    DocRefNo = openingBalanceDetailVM.DocRefNo,
                    Narration = openingBalanceDetailVM.Narration,
                    Amount = openingBalanceDetailVM.Amount,
                    SourceType = openingBalance.SourceType,
                    AddedBy = openingBalance.AddedBy,
                    AddedDate = openingBalance.AddedDate,
                    AddedFromIP = openingBalance.AddedFromIP
                };
                _securityDepositService.InsertGraph(securityDeposit);

                // INSERT INTO SecurityDepositDetail
                var securityDepositDetail = new SecurityDepositDetail
                {
                    Id = MakePK(securityDeposit.Id, 1, 2),
                    SecurityDepositId = securityDeposit.Id,
                    GLGeneralInfoId = openingBalanceDetail.GLGeneralInfoId,
                    BudgetMasterId = openingBalanceDetail.BudgetMasterId,
                    ActivityId = openingBalanceDetail.ActivityId,
                    Amount = securityDeposit.Amount,
                    AddedBy = securityDeposit.AddedBy,
                    AddedDate = securityDeposit.AddedDate,
                    AddedFromIP = securityDeposit.AddedFromIP
                };
                _securityDepositService.InsertSecurityDepositDetail(securityDepositDetail);
            }
            else if (openingBalance.SourceType == SourceType.Investment.ToString())
            {
                openingBalanceDetail.GLGeneralInfoId = glId;
                openingBalanceDetail.BudgetMasterId = budgetMasterId;
                openingBalanceDetail.ActivityId = activityId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    openingBalanceDetail.PartyPlantId = _partyPlantRepository.Query(r => r.PartyId == openingBalanceDetailVM.PartyId && r.IsDefault).Select().FirstOrDefault()?.Id;
                    openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                    openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                }
                else if (!string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                {
                    openingBalanceDetail.BankMasterId = openingBalanceDetailVM.BankMasterId;
                    openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                }
                else
                {
                    openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                    openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                }
                var financingPk = _securityDepositService.GetMaxNumber(nameof(Financing), PKGeneratorEnum.Yearly, null, openingBalance.PostingDate);

                financingPk.MaxNumber++;
                var investment = new Financing
                {
                    Id = openingBalance.PostingDate.Year + financingPk.MaxNumber.ToString(),
                    CompanyGroupId = openingBalance.CompanyGroupId,
                    CompanyId = openingBalance.CompanyId,
                    PlantId = openingBalance.PlantId,
                    EntityId = openingBalanceDetailVM.EntityId,
                    //FiscalYearId = openingBalance.FiscalYearId,
                    //FiscalYearPeriodId = openingBalance.FiscalYearPeriodId,
                    //TaxYearId = openingBalance.TaxYearId,
                    ///TaxYearPeriodId = openingBalance.TaxYearPeriodId,
                    CurrencyId = openingBalanceDetailVM.CurrencyId,
                    //VoucherId = openingBalance.Id,
                    //VoucherTypeId = openingBalance.VoucherTypeId,
                    TransactionType = openingBalance.TransactionType,
                    PartyType = openingBalanceDetailVM.PartyType,
                    PartyId = openingBalanceDetailVM.PartyId,
                    PartyPlantId = openingBalanceDetailVM.PartyPlantId,
                    EmployeeId = openingBalanceDetailVM.EmployeeId,
                    FinancingTypeId = openingBalance.FinancingTypeId,
                    OpeningBalanceId = openingBalance.Id,
                    VoucherDate = openingBalance.PostingDate,
                    PostingDate = openingBalance.PostingDate,
                    DocDate = openingBalanceDetailVM.DocDate,
                    DocRefNo = openingBalanceDetailVM.DocRefNo,
                    Narration = openingBalanceDetailVM.Narration,
                    Amount = openingBalanceDetailVM.Amount,
                    SourceType = openingBalance.SourceType,
                    AddedBy = openingBalance.AddedBy,
                    AddedDate = openingBalance.AddedDate,
                    AddedFromIP = openingBalance.AddedFromIP
                };
                _financingService.InsertFinancing(investment);

                // INSERT INTO FinancingDetail
                var financingDetail = new FinancingDetail
                {
                    GLGeneralInfoId = glId,
                    BudgetMasterId = budgetMasterId,
                    ActivityId = activityId,
                    BankMasterId = openingBalanceDetail.BankMasterId,
                    CashMasterId = openingBalanceDetail.CashMasterId,
                    Amount = investment.Amount
                };
                _financingService.InsertFinancingDetail(investment, financingDetail);

            }
            else if (openingBalance.SourceType == SourceType.Loan.ToString())
            {
                openingBalanceDetail.RepaymentStartDate = openingBalanceDetailVM.RepaymentStartDate;
                openingBalanceDetail.LifeOfYear = openingBalanceDetailVM.LifeOfYear;
                openingBalanceDetail.NoOfInstallmentPerYear = openingBalanceDetailVM.NoOfInstallmentPerYear;
                openingBalanceDetail.TotalNoOfInstallment = openingBalanceDetailVM.TotalNoOfInstallment;
                openingBalanceDetail.NoOfPaidInstallment = openingBalanceDetailVM.NoOfPaidInstallment;
                openingBalanceDetail.ProfitRate = openingBalanceDetailVM.ProfitRate;
                openingBalanceDetail.SanctionAmount = openingBalanceDetailVM.SanctionAmount;
                openingBalanceDetail.GLGeneralInfoId = glId;
                openingBalanceDetail.BudgetMasterId = budgetMasterId;
                openingBalanceDetail.ActivityId = activityId;
                openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    openingBalanceDetail.PartyPlantId = _partyPlantRepository.Query(r => r.PartyId == openingBalanceDetailVM.PartyId && r.IsDefault).Select().FirstOrDefault()?.Id;
                    openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                }
                else if (!string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                {
                    openingBalanceDetail.BankMasterId = openingBalanceDetailVM.BankMasterId;
                    openingBalanceDetail.BankCurrencyId = openingBalanceDetailVM.BankCurrencyId;
                    openingBalanceDetail.BankAmount = openingBalanceDetailVM.BankCurrencyId == openingBalanceDetailVM.CurrencyId ? openingBalanceDetailVM.Amount : openingBalanceDetailVM.BankAmount;
                }
                openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;

                //LoanTaken Given
                var financingPk = _financingService.GetMaxNumber();

                financingPk.MaxNumber++;
                var investment = new Financing
                {
                    Id = openingBalance.PostingDate.Year + financingPk.MaxNumber.ToString(),
                    CompanyGroupId = openingBalance.CompanyGroupId,
                    CompanyId = openingBalance.CompanyId,
                    PlantId = openingBalance.PlantId,
                    EntityId = openingBalance.EntityId,
                    // FiscalYearId = openingBalance.FiscalYearId,
                    //FiscalYearPeriodId = openingBalance.FiscalYearPeriodId,
                    //TaxYearId = voucher.TaxYearId,
                    //TaxYearPeriodId = voucher.TaxYearPeriodId,
                    CurrencyId = openingBalanceDetail.CurrencyId,
                    //VoucherId = voucher.Id,
                    //VoucherTypeId = voucher.VoucherTypeId,
                    TransactionType = openingBalance.TransactionType,
                    PartyType = openingBalanceDetail.PartyType,
                    PartyId = openingBalanceDetail.PartyId,
                    PartyPlantId = openingBalanceDetail.PartyPlantId,
                    EmployeeId = openingBalanceDetail.EmployeeId,
                    FinancingTypeId = openingBalance.FinancingTypeId,
                    OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                    VoucherDate = openingBalance.PostingDate,
                    PostingDate = openingBalance.PostingDate,
                    DocDate = openingBalance.DocDate,
                    DocRefNo = openingBalance.DocRefNo,
                    Narration = openingBalance.Narration,
                    Amount = openingBalanceDetail.Amount,
                    SourceType = openingBalance.SourceType,
                    //PaymentSource = openingBalanceDetail.PaymentSource,
                    AddedBy = openingBalance.AddedBy,
                    AddedDate = openingBalance.AddedDate,
                    AddedFromIP = openingBalance.AddedFromIP,
                    BankMasterId = openingBalanceDetail.BankMasterId,
                    LifeOfYear = openingBalanceDetail.LifeOfYear,
                    NoOfInstallmentPerYear = openingBalanceDetail.NoOfInstallmentPerYear,
                    ProfitRate = openingBalanceDetail.ProfitRate,
                    RepaymentStartDate = openingBalanceDetail.RepaymentStartDate,
                    TotalNoOfInstallment = openingBalanceDetail.TotalNoOfInstallment
                };
                _financingService.InsertFinancing(investment);

                // INSERT INTO FinancingDetail
                var financingDetail = new FinancingDetail
                {
                    GLGeneralInfoId = openingBalanceDetail.GLGeneralInfoId,
                    BudgetMasterId = openingBalanceDetail.BudgetMasterId,
                    ActivityId = openingBalanceDetail.ActivityId,
                    BankMasterId = openingBalanceDetail.BankMasterId,
                    CashMasterId = openingBalanceDetail.CashMasterId,
                    Amount = investment.Amount
                };
                _financingService.InsertFinancingDetail(investment, financingDetail);
                // Set FinancingDetail detail to voucher detail.
                //voucherDetail.FinancingDetailId = financingDetail.Id;

            }
            else if (openingBalance.SourceType == SourceType.EmployeeAdvance.ToString() || openingBalance.SourceType == SourceType.EmployeePayable.ToString())
            {
                openingBalanceDetail.PartyType = PartyType.Employee.ToString();
                openingBalanceDetail.EmployeeId = openingBalanceDetailVM.EmployeeId;
                openingBalanceDetail.GLGeneralInfoId = glId;
                openingBalanceDetail.BudgetMasterId = budgetMasterId;
                openingBalanceDetail.ActivityId = activityId;
            }

            // Set company currency.
            if (!string.IsNullOrEmpty(companyCurrencyId))
                InsertCompanyCurrency(openingBalanceDetail, openingBalanceDetailVM);
            // Set company Group currency.
            if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                InsertCompanyGroupCurrency(openingBalanceDetail, openingBalanceDetailVM);
            // Set hard currency.
            if (!string.IsNullOrEmpty(hardCurrencyId))
                InsertHardCurrency(openingBalanceDetail, openingBalanceDetailVM);

            _openingBalanceDetailRepository.Insert(openingBalanceDetail);
        }

        private OpeningBalanceDetailCurrency InsertCompanyCurrency(OpeningBalanceDetail openingBalanceDetail, VoucherDetailViewModel openingBalanceDetailVM)
        {
            var companyCurrency = new OpeningBalanceDetailCurrency
            {
                ModelState = ModelState.Added,
                Id = openingBalanceDetail.Id + 1,
                OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                OpeningBalanceDetailId = openingBalanceDetail.Id,
                ParallelCurrencyId = openingBalanceDetailVM.CompanyCurrencyId,
                FromCurrencyId = openingBalanceDetailVM.CompanyFromCurrencyId,
                ToCurrencyId = openingBalanceDetailVM.ToCurrencyId,
                ToCurrencyRate = openingBalanceDetailVM.CompanyCurrencyRate,
                ToCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion,
                Amount = openingBalanceDetailVM.CompanyCurrencyAmount,
                AddedBy = openingBalanceDetail.AddedBy,
                AddedDate = openingBalanceDetail.AddedDate,
                AddedFromIP = openingBalanceDetail.AddedFromIP
            };
            _openingBalanceDetailCurrencyRepository.Insert(companyCurrency);
            return companyCurrency;
        }

        private OpeningBalanceDetailCurrency InsertCompanyGroupCurrency(OpeningBalanceDetail openingBalanceDetail, VoucherDetailViewModel openingBalanceDetailVM)
        {
            var companyGroupCurrency = new OpeningBalanceDetailCurrency
            {
                Id = openingBalanceDetail.Id + 2,
                OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                OpeningBalanceDetailId = openingBalanceDetail.Id,
                ParallelCurrencyId = openingBalanceDetailVM.CompanyGroupCurrencyId,
                FromCurrencyId = openingBalanceDetailVM.CompanyGroupFromCurrencyId,
                ToCurrencyId = openingBalanceDetailVM.ToCurrencyId,
                ToCurrencyRate = openingBalanceDetailVM.CompanyGroupCurrencyRate,
                ToCurrencyConversion = openingBalanceDetailVM.CompanyGroupCurrencyConversion,
                Amount = openingBalanceDetailVM.CompanyGroupCurrencyAmount,
                AddedBy = openingBalanceDetail.AddedBy,
                AddedDate = openingBalanceDetail.AddedDate,
                AddedFromIP = openingBalanceDetail.AddedFromIP
            };
            _openingBalanceDetailCurrencyRepository.Insert(companyGroupCurrency);
            return companyGroupCurrency;
        }

        private OpeningBalanceDetailCurrency InsertHardCurrency(OpeningBalanceDetail openingBalanceDetail, VoucherDetailViewModel openingBalanceDetailVM)
        {
            var hardCurrency = new OpeningBalanceDetailCurrency
            {
                ModelState = ModelState.Added,
                Id = openingBalanceDetail.Id + 3,
                OpeningBalanceId = openingBalanceDetail.OpeningBalanceId,
                OpeningBalanceDetailId = openingBalanceDetail.Id,
                ParallelCurrencyId = openingBalanceDetailVM.HardCurrencyId,
                FromCurrencyId = openingBalanceDetailVM.HardFromCurrencyId,
                ToCurrencyId = openingBalanceDetailVM.ToCurrencyId,
                ToCurrencyRate = openingBalanceDetailVM.HardCurrencyRate,
                ToCurrencyConversion = openingBalanceDetailVM.HardCurrencyConversion,
                Amount = openingBalanceDetailVM.HardCurrencyAmount,
                AddedBy = openingBalanceDetail.AddedBy,
                AddedDate = openingBalanceDetail.AddedDate,
                AddedFromIP = openingBalanceDetail.AddedFromIP
            };
            _openingBalanceDetailCurrencyRepository.Insert(hardCurrency);
            return hardCurrency;
        }

        private void GLAssign(OpeningBalance entity, IEnumerable<string> existBankIds, string glId, string budgetMasterId, string activityId, VoucherDetailViewModel openingBalanceDetailVM, OpeningBalanceDetail openingBalanceDetail)
        {
            if (entity.SourceType == SourceType.CustomerAdvance.ToString() ||
                entity.SourceType == SourceType.CustomerInvoice.ToString())
            {
                openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                openingBalanceDetail.PartyType = PartyType.Customer.ToString();
                openingBalanceDetail.PartyPlantId = openingBalanceDetailVM.PartyPlantId;
                openingBalanceDetail.BankMasterId = null;
                openingBalanceDetail.CashMasterId = null;
                openingBalanceDetail.EmployeeId = null;

                var partyType = PartyType.Customer.ToString();
                var companyParty = _companyPartyRepository.Query(r => r.CompanyId == entity.CompanyId && r.PlantId == entity.PlantId && r.PartyId == openingBalanceDetail.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                if (null == companyParty)
                    throw new CustomException($"This party Id ({openingBalanceDetailVM.PartyId}) is not mapped as {PartyType.Customer} in this plant.");
                var companyPartyGLList = _companyPartyGLRepository.Query(r => r.CompanyPartyId == companyParty.Id).Select().ToList();
                if (null != companyPartyGLList)
                {
                    if (entity.SourceType == SourceType.CustomerAdvance.ToString())
                    {
                        var partyGLTypeDown = PartyGLType.DownPaymentGL.ToString();
                        var downPartyGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == partyGLTypeDown);
                        if (null != downPartyGL && !string.IsNullOrEmpty(downPartyGL.GLGeneralInfoId))
                        {
                            openingBalanceDetail.GLGeneralInfoId = downPartyGL.GLGeneralInfoId;
                            openingBalanceDetail.BudgetMasterId = downPartyGL.BudgetMasterId;
                            openingBalanceDetail.ActivityId = downPartyGL.ActivityId;
                        }
                        else
                            throw new CustomException($"Customer ({openingBalanceDetail.PartyId}) DownPayment GL not Found!");
                    }
                    else if (entity.SourceType == SourceType.CustomerInvoice.ToString())
                    {
                        var partyGLTypeRecon = PartyGLType.ReconciliationGL.ToString();
                        var reconPartyGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == partyGLTypeRecon);
                        if (null != reconPartyGL && !string.IsNullOrEmpty(reconPartyGL.GLGeneralInfoId))
                        {
                            openingBalanceDetail.GLGeneralInfoId = reconPartyGL.GLGeneralInfoId;
                            openingBalanceDetail.BudgetMasterId = reconPartyGL.BudgetMasterId;
                            openingBalanceDetail.ActivityId = reconPartyGL.ActivityId;
                        }
                        else
                            throw new CustomException($"Customer ({openingBalanceDetail.PartyId}) Reconciliation GL not Found!");
                    }
                }
                else
                    throw new CustomException($"Customer ({openingBalanceDetail.PartyId}) GL data not Assigned!");
            }
            else if (entity.SourceType == SourceType.VendorAdvance.ToString() ||
                     entity.SourceType == SourceType.VendorInvoice.ToString())
            {
                openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                openingBalanceDetail.PartyType = PartyType.Vendor.ToString();
                openingBalanceDetail.PartyPlantId = openingBalanceDetailVM.PartyPlantId;
                openingBalanceDetail.BankMasterId = null;
                openingBalanceDetail.CashMasterId = null;
                openingBalanceDetail.EmployeeId = null;

                var partyType = PartyType.Vendor.ToString();
                var companyParty = _companyPartyRepository.Query(r => r.CompanyId == entity.CompanyId && r.PlantId == entity.PlantId && r.PartyId == openingBalanceDetail.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                if (null == companyParty)
                    throw new CustomException($"This party Id ({openingBalanceDetailVM.PartyId}) is not mapped as {PartyType.Vendor} in this plant.");
                var companyPartyGLList = _companyPartyGLRepository.Query(r => r.CompanyPartyId == companyParty.Id).Select().ToList();
                if (null != companyPartyGLList)
                {
                    if (entity.SourceType == SourceType.VendorAdvance.ToString())
                    {
                        var partyGLTypeDown = PartyGLType.DownPaymentGL.ToString();
                        var downPartyGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == partyGLTypeDown);
                        if (null != downPartyGL && !string.IsNullOrEmpty(downPartyGL.GLGeneralInfoId))
                        {
                            openingBalanceDetail.GLGeneralInfoId = downPartyGL.GLGeneralInfoId;
                            openingBalanceDetail.BudgetMasterId = downPartyGL.BudgetMasterId;
                            openingBalanceDetail.ActivityId = downPartyGL.ActivityId;
                        }
                        else
                            throw new CustomException($"Vendor ({openingBalanceDetail.PartyId}) DownPayment GL not Found!");
                    }
                    else if (entity.SourceType == SourceType.VendorInvoice.ToString())
                    {
                        var partyGLTypeRecon = PartyGLType.ReconciliationGL.ToString();
                        var reconPartyGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == partyGLTypeRecon);
                        if (null != reconPartyGL && !string.IsNullOrEmpty(reconPartyGL.GLGeneralInfoId))
                        {
                            openingBalanceDetail.GLGeneralInfoId = reconPartyGL.GLGeneralInfoId;
                            openingBalanceDetail.BudgetMasterId = reconPartyGL.BudgetMasterId;
                            openingBalanceDetail.ActivityId = reconPartyGL.ActivityId;
                        }
                        else
                            throw new CustomException($"Vendor ({openingBalanceDetail.PartyId}) Reconciliation GL not Found!");
                    }
                }
                else
                    throw new CustomException($"Vendor ({openingBalanceDetail.PartyId}) GL data not Assigned!");
            }
            else if (entity.SourceType == SourceType.BankJournal.ToString())
            {
                openingBalanceDetail.PartyId = null;
                openingBalanceDetail.PartyType = PartyType.Bank.ToString();
                openingBalanceDetail.BankMasterId = openingBalanceDetailVM.BankMasterId;
                openingBalanceDetail.CashMasterId = null;
                openingBalanceDetail.EmployeeId = null;

                var bm = _bankMasterRepository.Find(openingBalanceDetail.BankMasterId);
                if (null != existBankIds && existBankIds.Contains(openingBalanceDetail.BankMasterId))
                    throw new CustomException($"This bank account ({bm.AccountTitle}) opening balance is already exist!");
                if (null != bm && !string.IsNullOrEmpty(bm.GLGeneralInfoId))
                {
                    openingBalanceDetail.GLGeneralInfoId = bm.GLGeneralInfoId;
                    openingBalanceDetail.BudgetMasterId = bm.BudgetMasterId;
                    openingBalanceDetail.ActivityId = bm.ActivityId;
                }
                else
                    throw new CustomException($"This bank account ({bm.AccountNumber}) GL not Found!");
            }
            else if (entity.SourceType == SourceType.CashJournal.ToString())
            {
                openingBalanceDetail.PartyId = null;
                openingBalanceDetail.PartyType = SourceType.CashJournal.ToString();
                openingBalanceDetail.BankMasterId = null;
                openingBalanceDetail.CashMasterId = openingBalanceDetailVM.CashMasterId;
                openingBalanceDetail.EmployeeId = null;

                var cash = _cashMasterRepository.Find(openingBalanceDetail.CashMasterId);
                if (null != cash && !string.IsNullOrEmpty(cash.GLGeneralInfoId))
                {
                    openingBalanceDetail.GLGeneralInfoId = cash.GLGeneralInfoId;
                    openingBalanceDetail.BudgetMasterId = cash.BudgetMasterId;
                    openingBalanceDetail.ActivityId = cash.ActivityId;
                }
                else
                    throw new CustomException($"This cash account ({cash.UserName}) GL not Found!");
            }
            else if (entity.SourceType == SourceType.SecurityDeposit.ToString() ||
                     entity.SourceType == SourceType.Investment.ToString())
            {
                openingBalanceDetail.GLGeneralInfoId = glId;
                openingBalanceDetail.BudgetMasterId = budgetMasterId;
                openingBalanceDetail.ActivityId = activityId;
                openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                openingBalanceDetail.PartyType = openingBalanceDetailVM.PartyType;
                openingBalanceDetail.BankMasterId = openingBalanceDetailVM.BankMasterId;
                openingBalanceDetail.CashMasterId = openingBalanceDetailVM.CashMasterId;
                openingBalanceDetail.EmployeeId = openingBalanceDetailVM.EmployeeId;
            }
            else if (entity.SourceType == SourceType.Loan.ToString() ||
                     entity.SourceType == SourceType.Loan.ToString())
            {
                openingBalanceDetail.GLGeneralInfoId = glId;
                openingBalanceDetail.BudgetMasterId = budgetMasterId;
                openingBalanceDetail.ActivityId = activityId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    openingBalanceDetail.PartyId = openingBalanceDetailVM.PartyId;
                    openingBalanceDetail.BankMasterId = null;
                    openingBalanceDetail.CashMasterId = null;
                    openingBalanceDetail.EmployeeId = null;
                }
                else if (!string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                {
                    openingBalanceDetail.PartyId = null;
                    openingBalanceDetail.BankMasterId = openingBalanceDetailVM.BankMasterId;
                    openingBalanceDetail.EmployeeId = null;
                }
            }
            else if (entity.SourceType == SourceType.EmployeeAdvance.ToString() ||
                     entity.SourceType == SourceType.EmployeePayable.ToString())
            {
                openingBalanceDetail.GLGeneralInfoId = glId;
                openingBalanceDetail.BudgetMasterId = budgetMasterId;
                openingBalanceDetail.ActivityId = activityId;
                openingBalanceDetail.PartyId = null;
                openingBalanceDetail.EmployeeId = openingBalanceDetailVM.EmployeeId;
                openingBalanceDetail.PartyType = PartyType.Employee.ToString();
                openingBalanceDetail.BankMasterId = null;
                openingBalanceDetail.CashMasterId = null;
            }
        }

        private void ModifyCheck(string id)
        {
            if (!Find(id).IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }

        private void CheckIsPosted(string id)
        {
            var openingBalanceDb = Find(id);
            if (openingBalanceDb.IsPosted)
                throw new CustomException(ServiceResources.UpdateNotAllowPosted);
            if (!openingBalanceDb.IsPark)
                throw new CustomException(ServiceResources.UpdateOrDeleteNotAllow);
        }

        private static void CheckIsPosted(OpeningBalance openingBalance)
        {
            if (openingBalance.IsPosted)
                throw new CustomException(ServiceResources.UpdateNotAllowPosted);
            if (!openingBalance.IsPark)
                throw new CustomException(ServiceResources.UpdateOrDeleteNotAllow);
        }

        private void Check(OpeningBalance entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.DocRefNo == entity.DocRefNo && r.Id != entity.Id && r.CompanyId == entity.CompanyId);
        }

        private void CheckWithPlant(OpeningBalance entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.DocRefNo == entity.DocRefNo && r.Id != entity.Id && r.CompanyId == entity.CompanyId && r.PlantId == entity.PlantId);
        }

        private static void CurrencyExchange(string companyCurrencyId, string companyGroupCurrencyId, string hardCurrencyId, VoucherDetailViewModel openingBalanceDetailVM)
        {
            // Set to currency id.
            openingBalanceDetailVM.ToCurrencyId = companyCurrencyId;

            if (openingBalanceDetailVM.CurrencyId == companyCurrencyId)
            {
                openingBalanceDetailVM.CompanyCurrencyRate = 1;
                openingBalanceDetailVM.CompanyCurrencyConversion = 1;
                openingBalanceDetailVM.CompanyFromCurrencyId = openingBalanceDetailVM.CurrencyId;
                if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                {
                    openingBalanceDetailVM.CompanyGroupCurrencyRate = openingBalanceDetailVM.CompanyCurrencyAmount / openingBalanceDetailVM.CompanyGroupCurrencyAmount;
                    openingBalanceDetailVM.CompanyGroupCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion / openingBalanceDetailVM.CompanyGroupCurrencyRate;
                }
                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    openingBalanceDetailVM.HardCurrencyRate = openingBalanceDetailVM.CompanyCurrencyAmount / openingBalanceDetailVM.HardCurrencyAmount;
                    openingBalanceDetailVM.HardCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion / openingBalanceDetailVM.HardCurrencyRate;
                }
            }
            else if (!string.IsNullOrEmpty(companyGroupCurrencyId) && openingBalanceDetailVM.CurrencyId == companyGroupCurrencyId)
            {
                openingBalanceDetailVM.CompanyGroupCurrencyRate = 1;
                openingBalanceDetailVM.CompanyGroupCurrencyConversion = 1;
                openingBalanceDetailVM.CompanyFromCurrencyId = openingBalanceDetailVM.CurrencyId;
                openingBalanceDetailVM.CompanyCurrencyRate = 1 / (openingBalanceDetailVM.CompanyGroupCurrencyAmount / openingBalanceDetailVM.CompanyCurrencyAmount);
                openingBalanceDetailVM.CompanyCurrencyConversion = openingBalanceDetailVM.CompanyGroupCurrencyConversion / openingBalanceDetailVM.CompanyCurrencyRate;
                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    openingBalanceDetailVM.HardCurrencyRate = openingBalanceDetailVM.CompanyCurrencyAmount / openingBalanceDetailVM.HardCurrencyAmount;
                    openingBalanceDetailVM.HardCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion / openingBalanceDetailVM.HardCurrencyRate;
                }
            }
            else if (!string.IsNullOrEmpty(hardCurrencyId) && openingBalanceDetailVM.CurrencyId == hardCurrencyId)
            {
                openingBalanceDetailVM.HardCurrencyRate = 1;
                openingBalanceDetailVM.HardCurrencyConversion = 1;
                openingBalanceDetailVM.CompanyFromCurrencyId = openingBalanceDetailVM.CurrencyId;

                openingBalanceDetailVM.CompanyCurrencyRate = 1 / (openingBalanceDetailVM.HardCurrencyAmount / openingBalanceDetailVM.CompanyCurrencyAmount);
                openingBalanceDetailVM.CompanyCurrencyConversion = openingBalanceDetailVM.HardCurrencyConversion * openingBalanceDetailVM.HardCurrencyRate;

                openingBalanceDetailVM.CompanyGroupCurrencyRate = openingBalanceDetailVM.CompanyCurrencyAmount / openingBalanceDetailVM.CompanyGroupCurrencyAmount;
                openingBalanceDetailVM.CompanyCurrencyConversion = openingBalanceDetailVM.CompanyCurrencyConversion / openingBalanceDetailVM.CompanyGroupCurrencyRate;
            }
            else
            {
                openingBalanceDetailVM.CompanyCurrencyRate = openingBalanceDetailVM.CompanyCurrencyAmount / openingBalanceDetailVM.Amount;
                openingBalanceDetailVM.CompanyCurrencyConversion = 1 / openingBalanceDetailVM.CompanyCurrencyRate;
                openingBalanceDetailVM.CompanyFromCurrencyId = openingBalanceDetailVM.CurrencyId;
                if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                {
                    openingBalanceDetailVM.CompanyGroupCurrencyRate = openingBalanceDetailVM.CompanyCurrencyAmount / openingBalanceDetailVM.CompanyGroupCurrencyAmount;
                    openingBalanceDetailVM.CompanyGroupCurrencyConversion = 1 / openingBalanceDetailVM.CompanyGroupCurrencyRate;
                }
                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    openingBalanceDetailVM.HardCurrencyRate = openingBalanceDetailVM.CompanyCurrencyAmount / openingBalanceDetailVM.HardCurrencyAmount;
                    openingBalanceDetailVM.HardCurrencyConversion = 1 / openingBalanceDetailVM.HardCurrencyRate;
                }
            }
        }

        private static void CurrencyExchange(string companyCurrencyId, string companyGroupCurrencyId, string hardCurrencyId, MaterialMasterOpeningBalanceDetailViewModel openingBalanceDetailVM)
        {
            // Set to currency id.
            openingBalanceDetailVM.ToCurrencyId = companyCurrencyId;

            if (openingBalanceDetailVM.CurrencyId == companyCurrencyId)
            {
                openingBalanceDetailVM.FACompanyCurrencyRate = 1;
                openingBalanceDetailVM.ADCompanyCurrencyRate = 1;
                openingBalanceDetailVM.FACompanyCurrencyConversion = 1;
                openingBalanceDetailVM.ADCompanyCurrencyConversion = 1;
                openingBalanceDetailVM.CompanyFromCurrencyId = openingBalanceDetailVM.CurrencyId;
                if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                {
                    openingBalanceDetailVM.FACompanyGroupCurrencyRate = openingBalanceDetailVM.FACompanyCurrencyAmount / openingBalanceDetailVM.FACompanyGroupCurrencyAmount;
                    openingBalanceDetailVM.ADCompanyGroupCurrencyRate = openingBalanceDetailVM.ADCompanyCurrencyAmount / openingBalanceDetailVM.ADCompanyGroupCurrencyAmount;
                    openingBalanceDetailVM.FACompanyGroupCurrencyConversion = openingBalanceDetailVM.FACompanyCurrencyConversion / openingBalanceDetailVM.FACompanyGroupCurrencyRate;
                    openingBalanceDetailVM.ADCompanyGroupCurrencyConversion = openingBalanceDetailVM.ADCompanyCurrencyConversion / openingBalanceDetailVM.ADCompanyGroupCurrencyRate;
                }
                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    openingBalanceDetailVM.FAHardCurrencyRate = openingBalanceDetailVM.FACompanyCurrencyAmount / openingBalanceDetailVM.FAHardCurrencyAmount;
                    openingBalanceDetailVM.ADHardCurrencyRate = openingBalanceDetailVM.ADCompanyCurrencyAmount / openingBalanceDetailVM.ADHardCurrencyAmount;
                    openingBalanceDetailVM.FAHardCurrencyConversion = openingBalanceDetailVM.FACompanyCurrencyConversion / openingBalanceDetailVM.FAHardCurrencyRate;
                    openingBalanceDetailVM.FAHardCurrencyConversion = openingBalanceDetailVM.FACompanyCurrencyConversion / openingBalanceDetailVM.FAHardCurrencyRate;
                }
            }
            else if (!string.IsNullOrEmpty(companyGroupCurrencyId) && openingBalanceDetailVM.CurrencyId == companyGroupCurrencyId)
            {
                openingBalanceDetailVM.FACompanyGroupCurrencyRate = 1;
                openingBalanceDetailVM.ADCompanyGroupCurrencyRate = 1;
                openingBalanceDetailVM.FACompanyGroupCurrencyConversion = 1;
                openingBalanceDetailVM.ADCompanyGroupCurrencyConversion = 1;
                openingBalanceDetailVM.CompanyFromCurrencyId = openingBalanceDetailVM.CurrencyId;
                openingBalanceDetailVM.FACompanyCurrencyRate = 1 / (openingBalanceDetailVM.FACompanyGroupCurrencyAmount / openingBalanceDetailVM.FACompanyCurrencyAmount);
                openingBalanceDetailVM.ADCompanyCurrencyRate = 1 / (openingBalanceDetailVM.ADCompanyGroupCurrencyAmount / openingBalanceDetailVM.ADCompanyCurrencyAmount);
                openingBalanceDetailVM.FACompanyCurrencyConversion = openingBalanceDetailVM.FACompanyGroupCurrencyConversion / openingBalanceDetailVM.FACompanyCurrencyRate;
                openingBalanceDetailVM.ADCompanyCurrencyConversion = openingBalanceDetailVM.ADCompanyGroupCurrencyConversion / openingBalanceDetailVM.ADCompanyCurrencyRate;
                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    openingBalanceDetailVM.FAHardCurrencyRate = openingBalanceDetailVM.FACompanyCurrencyAmount / openingBalanceDetailVM.FAHardCurrencyAmount;
                    openingBalanceDetailVM.ADHardCurrencyRate = openingBalanceDetailVM.ADCompanyCurrencyAmount / openingBalanceDetailVM.ADHardCurrencyAmount;
                    openingBalanceDetailVM.FAHardCurrencyConversion = openingBalanceDetailVM.FACompanyCurrencyConversion / openingBalanceDetailVM.FAHardCurrencyRate;
                    openingBalanceDetailVM.ADHardCurrencyConversion = openingBalanceDetailVM.ADCompanyCurrencyConversion / openingBalanceDetailVM.ADHardCurrencyRate;
                }
            }
            else if (!string.IsNullOrEmpty(hardCurrencyId) && openingBalanceDetailVM.CurrencyId == hardCurrencyId)
            {
                openingBalanceDetailVM.FAHardCurrencyRate = 1;
                openingBalanceDetailVM.ADHardCurrencyRate = 1;
                openingBalanceDetailVM.FAHardCurrencyConversion = 1;
                openingBalanceDetailVM.ADHardCurrencyConversion = 1;
                openingBalanceDetailVM.CompanyFromCurrencyId = openingBalanceDetailVM.CurrencyId;

                openingBalanceDetailVM.FACompanyCurrencyRate = 1 / (openingBalanceDetailVM.FAHardCurrencyAmount / openingBalanceDetailVM.FACompanyCurrencyAmount);
                openingBalanceDetailVM.ADCompanyCurrencyRate = 1 / (openingBalanceDetailVM.ADHardCurrencyAmount / openingBalanceDetailVM.ADCompanyCurrencyAmount);
                openingBalanceDetailVM.FACompanyCurrencyConversion = openingBalanceDetailVM.FAHardCurrencyConversion * openingBalanceDetailVM.FAHardCurrencyRate;
                openingBalanceDetailVM.ADCompanyCurrencyConversion = openingBalanceDetailVM.ADHardCurrencyConversion * openingBalanceDetailVM.ADHardCurrencyRate;

                openingBalanceDetailVM.FACompanyGroupCurrencyRate = openingBalanceDetailVM.FACompanyCurrencyAmount / openingBalanceDetailVM.FACompanyGroupCurrencyAmount;
                openingBalanceDetailVM.ADCompanyGroupCurrencyRate = openingBalanceDetailVM.ADCompanyCurrencyAmount / openingBalanceDetailVM.ADCompanyGroupCurrencyAmount;
                openingBalanceDetailVM.FACompanyCurrencyConversion = openingBalanceDetailVM.FACompanyCurrencyConversion / openingBalanceDetailVM.FACompanyGroupCurrencyRate;
                openingBalanceDetailVM.ADCompanyCurrencyConversion = openingBalanceDetailVM.ADCompanyCurrencyConversion / openingBalanceDetailVM.ADCompanyGroupCurrencyRate;
            }
            else
            {
                openingBalanceDetailVM.FACompanyCurrencyRate = openingBalanceDetailVM.FACompanyCurrencyAmount / openingBalanceDetailVM.FACompanyCurrencyAmount;

                if (openingBalanceDetailVM.ADCompanyCurrencyAmount != 0)
                {
                    openingBalanceDetailVM.ADCompanyCurrencyRate = openingBalanceDetailVM.ADCompanyCurrencyAmount / openingBalanceDetailVM.ADCompanyCurrencyAmount;
                    openingBalanceDetailVM.ADCompanyCurrencyConversion = 1 / openingBalanceDetailVM.ADCompanyCurrencyRate;
                }
                openingBalanceDetailVM.FACompanyCurrencyConversion = 1 / openingBalanceDetailVM.FACompanyCurrencyRate;
                openingBalanceDetailVM.CompanyFromCurrencyId = openingBalanceDetailVM.CurrencyId;
                if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                {
                    openingBalanceDetailVM.FACompanyGroupCurrencyRate = openingBalanceDetailVM.FACompanyCurrencyAmount / openingBalanceDetailVM.FACompanyGroupCurrencyAmount;
                    openingBalanceDetailVM.ADCompanyGroupCurrencyRate = openingBalanceDetailVM.ADCompanyCurrencyAmount / openingBalanceDetailVM.ADCompanyGroupCurrencyAmount;
                    openingBalanceDetailVM.FACompanyGroupCurrencyConversion = 1 / openingBalanceDetailVM.FACompanyGroupCurrencyRate;
                    openingBalanceDetailVM.ADCompanyGroupCurrencyConversion = 1 / openingBalanceDetailVM.ADCompanyGroupCurrencyRate;
                }
                if (!string.IsNullOrEmpty(hardCurrencyId))
                {
                    openingBalanceDetailVM.FAHardCurrencyRate = openingBalanceDetailVM.FACompanyCurrencyAmount / openingBalanceDetailVM.FAHardCurrencyAmount;
                    openingBalanceDetailVM.ADHardCurrencyRate = openingBalanceDetailVM.ADCompanyCurrencyAmount / openingBalanceDetailVM.ADHardCurrencyAmount;
                    openingBalanceDetailVM.FAHardCurrencyConversion = 1 / openingBalanceDetailVM.FAHardCurrencyRate;
                    openingBalanceDetailVM.ADHardCurrencyConversion = 1 / openingBalanceDetailVM.ADHardCurrencyRate;
                }
            }
        }

        #region Report

        public IWorkbook GetOpeningBalanceJournal(string companyId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            try
            {
                var workbook = oRU.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                CreateSheetMainOpeningBalanceJournal(ref sheet1, oRU, "Opening Balance Journal", "Opening Balance Journal", companyId, plantName, voucherId);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheetMainOpeningBalanceJournal(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyId, string plantName, string voucherId)
        {
            var openingBalanceList = GetJournalData(companyId, voucherId);
            var dtOpeningBalance = openingBalanceList;
            var openingBalanceCheckByCompanyList = GetOpeningBalanceCheckByCompany(companyId);
            var dtOpeningBalanceCheckByCompany = openingBalanceCheckByCompanyList;
            if (dtOpeningBalance.Rows.Count == 0)
                throw (new Exception("No Data Found !!!"));
            using (var dvAccountCode = new DataView(openingBalanceList))
            {
                var dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "GLGeneralInfoId");

                using (DataView dvParallelCurrency = new DataView(openingBalanceList)
                {
                    Sort = "CurrencyCode ASC"
                })
                {
                    var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                    using (DataView dvMainBody = new DataView(openingBalanceList)
                    {
                        Sort = "DRCR, GLGeneralInfoCode, Budget, Value DESC"
                    })
                    {
                        var dtMainBody = dvMainBody.ToTable(true, "VoucherDetailId", "Park/Post", "IsPark", "Bank", "Branch", "AccountNumber", "Account Title", "GLGeneralInfoCode", "GLGeneralInfoName", "DetailNarration", "Ref", "InvoiceNo", "InvoiceDate", "TrnCurrency", "Value", "DRCR", "Entity", "Budget", "Activity", "Budget Fiscal Year", "Budget Fiscal Year Period", "Budget Period No", "Party", "Employee");

                        #region Customer Check By Company

                        using (DataView dvOpeningBalanceCheckByCompanyBody = new DataView(openingBalanceCheckByCompanyList))
                        {
                            var dtOpeningBalanceCheckByCompanyBody = dvOpeningBalanceCheckByCompanyBody.ToTable(false, "IsVoucherFromBudget", "IsBudgetPeriod", "IsCostCenterApplicable", "IsProfitCenterApplicable");
                            var Budget = dtOpeningBalanceCheckByCompanyBody.Rows[0]["IsVoucherFromBudget"].ToString();
                            var BudgetPeriod = dtOpeningBalanceCheckByCompanyBody.Rows[0]["IsBudgetPeriod"].ToString();
                            var ProfitCenter = dtOpeningBalanceCheckByCompanyBody.Rows[0]["IsProfitCenterApplicable"].ToString();

                            #endregion Customer Check By Company

                            var _col = 1;
                            var _row = 5;
                            var shet2EndxlsCol = _col;

                            const int _col3 = 3;

                            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Voucher No");
                            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
                            reportUtility.SetText(ref sheet, _row, _col + 2, dtOpeningBalance.Rows[0]["VoucherNo"].ToString()); _row++;
                            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Doc Date");
                            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
                            reportUtility.SetText(ref sheet, _row, _col + 2, dtOpeningBalance.Rows[0]["DocDate"].ToString()); _row++;
                            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Posting Date");
                            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
                            reportUtility.SetText(ref sheet, _row, _col + 2, dtOpeningBalance.Rows[0]["PostingDate"].ToString()); _row++;
                            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Narration");
                            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
                            reportUtility.SetText(ref sheet, _row, _col + 2, dtOpeningBalance.Rows[0]["Narration"].ToString()); _row++;
                            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Park/ Post");
                            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
                            reportUtility.SetText(ref sheet, _row, _col + 2, dtOpeningBalance.Rows[0]["Park/Post"].ToString()); _row++;
                            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                            var _rowR = 5;
                            const int _colR = 6;
                            const int _col8 = 8;

                            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Voucher Date");
                            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtOpeningBalance.Rows[0]["VoucherDate"].ToString()); _rowR++;
                            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Doc No");
                            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtOpeningBalance.Rows[0]["DocRefNo"].ToString()); _rowR++;
                            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Fiscal Year");
                            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtOpeningBalance.Rows[0]["FiscalYearName"].ToString()); _rowR++;
                            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Fiscal Year Period");
                            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtOpeningBalance.Rows[0]["PeriodName"] + " (" + (dtOpeningBalance.Rows[0]["PeriodNo"].ToString()) + ")"); _rowR++;
                            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                            var _rowL = 11;
                            _rowL++;

                            var headreColIndex = 1;
                            var mainColIndex = 1;

                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL Name", 32);
                            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;

                            if (Budget == "True")
                            {
                                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 26);
                                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 26);
                                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                            }

                            if (ProfitCenter == "True")
                            {
                                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Entity", 26);
                                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                            }

                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Reference", 26);
                            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Detail Narration", 26);
                            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Doc Ref No");
                            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Doc Date");
                            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;

                            if (BudgetPeriod == "True")
                            {
                                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget Fiscal Year", 26);
                                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget Fiscal Year Period", 26);
                                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                            }

                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Currency", 12);
                            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Value", ExcelHAlign.HAlignRight);
                            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;

                            double _Total_Amount = 0;
                            var plCurrencyId = string.Empty;
                            var plCurrencyCode = string.Empty;

                            var alParaCurrency = new ArrayList();

                            for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                            {
                                reportUtility.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                                sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); headreColIndex++;
                                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight); headreColIndex++;

                                var dic = new Dictionary<string, int>
                                {
                                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex-1 }
                                };
                                alParaCurrency.Add(dic);

                                if (n == 0)
                                {
                                    plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                                }
                            }
                            shet2EndxlsCol = headreColIndex - 1;

                            double vAmount = 0;
                            var drcrCol = 0;
                            var totCol = 0;
                            var Row_Total_Start = _rowL + 1;
                            for (int n = 0; n < dtMainBody.Rows.Count; n++)
                            {
                                _rowL++;
                                var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                                var Bank = dtMainBody.Rows[n]["Bank"].ToString();
                                var Party = dtMainBody.Rows[n]["Party"].ToString();
                                var Employee = dtMainBody.Rows[n]["Employee"].ToString();
                                var Branch = dtMainBody.Rows[n]["Branch"].ToString();
                                var ACNumber = dtMainBody.Rows[n]["AccountNumber"].ToString();
                                var ACTitle = dtMainBody.Rows[n]["Account Title"].ToString();
                                var _VoucherDetailId = dtMainBody.Rows[n]["VoucherDetailId"].ToString();

                                if (!string.IsNullOrEmpty(Bank))
                                {
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GLGeneralInfoName"] + " " + Bank + Branch); mainColIndex++;
                                }
                                else
                                {
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GLGeneralInfoName"]); mainColIndex++;
                                }

                                if (Budget == "True")
                                {
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n][nameof(Budget)].ToString()); mainColIndex++;
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;
                                }

                                if (ProfitCenter == "True")
                                {
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["Entity"].ToString()); mainColIndex++;
                                }

                                if (!string.IsNullOrEmpty(Bank))
                                {
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, ACNumber + ACTitle); mainColIndex++;
                                }
                                else if (!string.IsNullOrEmpty(Party))
                                {
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n][nameof(Party)].ToString()); mainColIndex++;
                                }
                                else if (!string.IsNullOrEmpty(Employee))
                                {
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n][nameof(Employee)].ToString()); mainColIndex++;
                                }
                                else
                                {
                                    mainColIndex++;
                                }

                                reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["DetailNarration"].ToString()); mainColIndex++;
                                reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["InvoiceNo"].ToString()); mainColIndex++;
                                reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["InvoiceDate"].ToString()); mainColIndex++;

                                if (BudgetPeriod == "True")
                                {
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["Budget Fiscal Year"].ToString()); mainColIndex++;
                                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["Budget Fiscal Year Period"] + " (" + (dtMainBody.Rows[n]["Budget Period No"].ToString()) + ")"); mainColIndex++;
                                }

                                reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["TrnCurrency"].ToString()); mainColIndex++;
                                reportUtility.SetText(ref sheet, _rowL, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["Value"]));

                                vAmount += Convert.ToDouble(dtMainBody.Rows[n]["Value"].ToString());
                                drcrCol = mainColIndex;
                                totCol = mainColIndex;

                                for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                                {
                                    var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                                    using (DataView dvDrCr = new DataView(openingBalanceList)
                                    {
                                        RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND VoucherDetailId='" + _VoucherDetailId + "'"
                                    })
                                    {
                                        if (p == 0)
                                        {
                                            plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                                        }

                                        var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                                        _pcCol = _pcCol - 1;
                                        var dtDrCr = dvDrCr.ToTable();
                                        if (dtDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            reportUtility.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DrAmount"].ToString())); drcrCol++;
                                            reportUtility.SetText(ref sheet, _rowL, _pcCol + 1, Convert.ToDouble(dtDrCr.Rows[0]["CrAmount"].ToString()));
                                            if (p == 0)
                                            {
                                                _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DrAmount"].ToString());
                                            }
                                        }
                                    }
                                }
                                mainColIndex = 1;
                            }

                            #region sumCalc

                            _rowL++;
                            var sumdrcrCol = totCol;
                            sheet.Range[reportUtility.GetColumnNameForXls(1) + (_rowL) + ":" + reportUtility.GetColumnNameForXls(totCol - 1) + _rowL].Merge();
                            sheet.Range[_rowL, totCol].Text = "Total ";
                            sheet.Range[_rowL, totCol].CellStyle.Font.Bold = true;
                            sheet.Range[_rowL, totCol].BorderAround(ExcelLineStyle.Hair);

                            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                            {
                                sumdrcrCol++;
                                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                                sheet.Range[_rowL, sumdrcrCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                                sumdrcrCol++;
                                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                                sheet.Range[_rowL, sumdrcrCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                            }

                            #endregion sumCalc

                            var _Currency = string.Empty;
                            var _CurrencyId = string.Empty;

                            _CurrencyId = dtOpeningBalance.Rows[0]["CurrencyId"].ToString();

                            shet2EndxlsCol = sumdrcrCol;
                            sheet.Range[(11), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                            #region InWord

                            var _amount = reportUtility.InWord(_Total_Amount, plCurrencyId);
                            _rowL += 1;
                            reportUtility.SetText(ref sheet, _rowL, _col, "In Word:", true);
                            _col = 2;
                            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].Text = _amount;
                            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

                            #endregion InWord

                            _rowL = _rowL + 6;

                            #region Signature

                            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                            sheet.Range[_rowL, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                            sheet.Range[_rowL, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                            sheet.Range[_rowL, 9].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                            reportUtility.SetText(ref sheet, _rowL, 1, "Received By", true); _col += 1;
                            reportUtility.SetText(ref sheet, _rowL, 3, "Prepared By", true); _col += 1;
                            reportUtility.SetText(ref sheet, _rowL, 5, "Checked By", true); _col += 1;
                            reportUtility.SetText(ref sheet, _rowL, 7, "HOD (Finance)", true); _col += 1;
                            reportUtility.SetText(ref sheet, _rowL, 9, "CEO/Director", true); _col += 1;

                            #endregion Signature

                            sheet.Name = sheetName;
                            sheet.UsedRange.WrapText = true;
                            sheet.UsedRange.CellStyle.Font.Size = 8;
                            reportUtility.CompanyPlantHeader(ref sheet, shet2EndxlsCol, sheetHeader, companyId, plantName, null);
                            reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                        }
                    }
                }
            }
        }

        private static int GetCurrencyColIndex(ArrayList al, string paraCar)
        {
            var result = 0;
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    var v = (Dictionary<string, int>)al[i];
                    if (v.ContainsKey(paraCar))
                    {
                        result = v[paraCar];
                        break;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetJournalData(string companyId, string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
                            , Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Park' ELSE 'Post' END, Replace(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
                            , V.DocRefNo, Replace(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, V.VoucherNo, V.Narration, VD.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy AS PreparedBy, VDC.ParallelCurrencyId
                            , CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount+VD.CrAmount AS Value, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') InvoiceDate, VD.DocRefNo AS InvoiceNo, '('+BN.UserName +' - ' AS Bank, BR.UserName+')' AS Branch, BM.AccountNumber AS AccountNumber
                            , BM.AccountTitle AS [Account Title], VD.RefCode AS Ref, VD.Narration AS DetailNarration, CO.UserName AS CompanyName, AM.Address1 AS AddressLine, ENT.UserName AS Entity, BUD.UserName AS Budget, ACT.UserName AS Activity
                            , CST.UserName AS [Cost Center], BFY.FiscalYearName AS [Budget Fiscal Year], BFYP.PeriodName AS [Budget Fiscal Year Period], BFYP.PeriodNo AS [Budget Period No], P.UserName AS Party, EMP.EmployeeCode+' - '+EMP.EmployeeName AS Employee,PL.UserName as  PlantName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=VD.CurrencyId
                            LEFT JOIN [ORG].[Company] AS CO ON CO.Id=V.CompanyId
                            LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=CO.AddressMasterId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                            LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
                            LEFT JOIN [HKP].[BankBranch] BR ON BR.Id=BM.BankBranchId
                            LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [ORG].[CostCenter] AS CST ON CST.Id = VD.CostCenterId
                            LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                            LEFT JOIN [SCS].[FiscalYear] AS BFY ON BFY.Id=VD.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS BFYP ON BFYP.Id=VD.FiscalYearPeriodId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [dbo].[EmployeeInformation] AS EMP ON EMP.SystemId=VD.EmployeeId
                            LEFT JOIN ORG.Plant AS PL ON v.PlantId=PL.Id
                            WHERE V.Archive=0 AND V.SourceType='" + SourceType.OpeningBalance + @"' AND V.Id='" + voucherId + @"' AND V.CompanyId='" + companyId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetOpeningBalanceCheckByCompany(string companyId)
        {
            var sql = @"SELECT IsVoucherFromBudget, IsBudgetPeriod, IsCostCenterApplicable, IsProfitCenterApplicable FROM [ORG].[Company] WHERE Id='" + companyId + @"' AND Active=1 AND Archive=0";
            return _sqlRepository.GetDataTable(sql);
        }

        public IWorkbook GetOpeningBalanceReport(string companyId, string plantName, string[] parallelCurrencies)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var oRU = new ReportUtility();
                var workbook = oRU.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                CreateSheetMain_OpeningBalance(ref sheet1, oRU, "Opening Balance Report", "Opening Balance Report", companyId, plantName, parallelCurrencies);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheetMain_OpeningBalance(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyId, string plantName, string[] parallelCurrencies)
        {
            var openingBalanceList = GetOpeningBalance(companyId, parallelCurrencies);
            var dtOpeningBalance = openingBalanceList;
            if (dtOpeningBalance.Rows.Count == 0)
                throw new CustomException("No Data Found !!!");
            using (DataView dvAccountCode = new DataView(openingBalanceList))
            {
                var dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId");

                using (DataView dvParallelCurrency = new DataView(openingBalanceList))
                {
                    var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                    using (DataView dvMainBody = new DataView(openingBalanceList)
                    {
                        Sort = "TrnType, GLGeneralInfoCode, Value DESC"
                    })
                    {
                        var dtMainBody = dvMainBody.ToTable(true, "OpeningBalanceDetailId", "Park/Post", "IsPark", "Bank", "Branch", "AccountNumber", "GLGeneralInfoCode", "GLGeneralInfoName", "BudgetName", "ActivityName", "DetailNarration", "Ref", "TrnCurrency", "Value", "DetailDocDate", "Amount");

                        var _col = 1;
                        var shet2EndxlsCol = _col;

                        var _rowL = 5;
                        _rowL++;

                        var headreColIndex = 1;

                        reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL Name", 32);
                        sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 15);
                        sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 15);
                        sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Detail Narration", 26);
                        sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Doc Ref No");
                        sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Doc Date");
                        sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Currency", 12);
                        sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Value", ExcelHAlign.HAlignRight);
                        sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;

                        double _Total_Amount = 0;
                        var plCurrencyId = string.Empty;
                        var plCurrencyCode = string.Empty;

                        for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                        {
                            reportUtility.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); headreColIndex++;
                            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight); headreColIndex++;

                            if (n == 0)
                            {
                                plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                            }
                        }
                        shet2EndxlsCol = headreColIndex - 1;

                        double vAmount = 0;
                        var drcrCol = 7;
                        var Row_Total_Start = _rowL + 1;
                        for (int n = 0; n < dtMainBody.Rows.Count; n++)
                        {
                            _rowL++;
                            var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                            var Bank = dtMainBody.Rows[n]["Bank"].ToString();
                            var Branch = dtMainBody.Rows[n]["Branch"].ToString();
                            var ACNumber = dtMainBody.Rows[n]["AccountNumber"].ToString();
                            var _VoucherDetailId = dtMainBody.Rows[n]["OpeningBalanceDetailId"].ToString();
                            if (!string.IsNullOrEmpty(Bank))
                            {
                                reportUtility.SetText(ref sheet, _rowL, 1, dtMainBody.Rows[n]["GLGeneralInfoName"] + " " + Bank + Branch + ACNumber);
                            }
                            else
                            {
                                reportUtility.SetText(ref sheet, _rowL, 1, dtMainBody.Rows[n]["GLGeneralInfoName"].ToString());
                            }
                            reportUtility.SetText(ref sheet, _rowL, 2, dtMainBody.Rows[n]["BudgetName"].ToString());
                            reportUtility.SetText(ref sheet, _rowL, 3, dtMainBody.Rows[n]["ActivityName"].ToString());
                            reportUtility.SetText(ref sheet, _rowL, 4, dtMainBody.Rows[n]["DetailNarration"].ToString());
                            reportUtility.SetText(ref sheet, _rowL, 5, dtMainBody.Rows[n]["Ref"].ToString());
                            reportUtility.SetText(ref sheet, _rowL, 6, dtMainBody.Rows[n]["DetailDocDate"].ToString());
                            reportUtility.SetText(ref sheet, _rowL, 7, dtMainBody.Rows[n]["TrnCurrency"].ToString());
                            reportUtility.SetText(ref sheet, _rowL, 8, Convert.ToDouble(dtMainBody.Rows[n]["Value"]));

                            vAmount += Convert.ToDouble(dtMainBody.Rows[n]["Amount"].ToString());
                            drcrCol = 8;

                            for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                            {
                                var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                                using (DataView dvDrCr = new DataView(openingBalanceList)
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND OpeningBalanceDetailId='" + _VoucherDetailId + "'"
                                })
                                {
                                    if (p == 0)
                                    {
                                        plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                                    }

                                    var dtDrCr = dvDrCr.ToTable();
                                    if (dtDrCr.Rows.Count != 0)
                                    {
                                        drcrCol++;
                                        reportUtility.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(dtDrCr.Rows[0]["DrAmount"].ToString())); drcrCol++;
                                        reportUtility.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(dtDrCr.Rows[0]["CrAmount"].ToString()));
                                        if (p == 0)
                                        {
                                            _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DrAmount"].ToString());
                                        }
                                    }
                                }
                            }
                        }//main

                        #region sumCalc

                        _rowL++;
                        var sumdrcrCol = 8;
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + (_rowL) + ":" + reportUtility.GetColumnNameForXls(5) + _rowL].Merge();
                        sheet.Range[_rowL, 8].Text = "Total ";
                        sheet.Range[_rowL, 8].CellStyle.Font.Bold = true;
                        sheet.Range[_rowL, 8].BorderAround(ExcelLineStyle.Hair);
                        // DR
                        for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                        {
                            sumdrcrCol++;
                            sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                            sheet.Range[_rowL, sumdrcrCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                            sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                            sumdrcrCol++;
                            sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                            sheet.Range[_rowL, sumdrcrCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                            sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                        }

                        #endregion sumCalc

                        var _Currency = string.Empty;
                        var _CurrencyId = string.Empty;

                        _CurrencyId = dtOpeningBalance.Rows[0]["CurrencyId"].ToString();//CurrencyId

                        shet2EndxlsCol = drcrCol;
                        sheet.Range[(7), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                        #region InWord

                        var _amount = reportUtility.InWord(_Total_Amount, plCurrencyId);

                        _rowL += 1;
                        reportUtility.SetText(ref sheet, _rowL, _col, "In Word:", true);
                        _col = 2;
                        sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].Text = _amount;
                        sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                        sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
                        //}

                        #endregion InWord

                        _rowL = _rowL + 6;

                        #region Signature

                        sheet.Range[_rowL, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                        sheet.Range[_rowL, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                        sheet.Range[_rowL, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                        sheet.Range[_rowL, 8].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                        sheet.Range[_rowL, 10].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                        reportUtility.SetText(ref sheet, _rowL, 2, "Received By", true); _col += 1;
                        reportUtility.SetText(ref sheet, _rowL, 4, "Prepared By", true); _col += 1;
                        reportUtility.SetText(ref sheet, _rowL, 6, "Checked By", true); _col += 1;
                        reportUtility.SetText(ref sheet, _rowL, 8, "HOD (Finance)", true); _col += 1;
                        reportUtility.SetText(ref sheet, _rowL, 10, "CEO/Director", true); _col += 1;

                        #endregion Signature

                        sheet.Name = sheetName;
                        sheet.UsedRange.WrapText = true;
                        sheet.UsedRange.CellStyle.Font.Size = 8;
                        reportUtility.CompanyPlantHeader(ref sheet, shet2EndxlsCol, sheetHeader, companyId, plantName, null);
                        reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                    }
                }
            }
        }

        private DataTable GetOpeningBalance(string companyId, string[] parallelCurrencies)
        {
            try
            {
                var parallelCurrency = "";
                parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                var _sql = @"  SELECT OB.Id, GGI.Id AS AccountCodeId, GGI.AccountCode AS GLGeneralInfoCode, GGI.AccountCode+' - '+GGI.UserName AS GLGeneralInfoName, REPLACE(CONVERT(CHAR(11)
		                        , OB.DocDate, 106),' ','-') AS DocDate
		                        , OBDC.OpeningBalanceDetailId
		                        , OB.IsPark
		                        , [Park/Post]=CASE WHEN OB.IsPark=1 THEN 'Park' ELSE 'Post' END
		                        , OB.DocRefNo
		                        , OB.Narration
		                        , OBD.GLGeneralInfoId
		                        , OBD.BankMasterId
		                        , OBD.EmployeeId
		                        , OBD.PartyId
		                        , OBD.PartyType
		                        , OB.SourceType
		                        , OB.EntityId
		                        , REPLACE(CONVERT(CHAR(11)
		                        , OBD.DocDate, 106),' ','-') AS DetailDocDate
		                        , OBD.DocRefNo AS Ref
		                        , OBD.Narration AS DetailNarration
		                        , OBD.CurrencyId
		                        , C.Code AS TrnCurrency
								,B.UserName BudgetName
								,A.UserName ActivityName
                                ,[TrnType]=CASE WHEN OB.SourceType='CustomerInvoice' THEN 'Dr'
                                               WHEN OB.SourceType='CustomerAdvance' THEN 'Cr'
                                               WHEN OB.SourceType='VendorInvoice' THEN 'Cr'
                                               WHEN OB.SourceType='VendorAdvance' THEN 'Dr'
                                               WHEN OB.SourceType='SecurityGiven' THEN 'Dr'
                                               WHEN OB.SourceType='SecurityTaken' THEN 'Cr'
                                               WHEN OB.SourceType='Bank' THEN 'Dr'
                                               WHEN OB.SourceType='Loan' THEN 'Cr'
                                               WHEN OB.SourceType='EmployeeAdvance' THEN 'Dr'
                                               WHEN OB.SourceType='LoanGiven' THEN 'Dr'
                                               WHEN OB.SourceType='LoanTaken' THEN 'Cr'
                                               WHEN OB.SourceType='InvestmentGiven' THEN 'Dr'
                                               WHEN OB.SourceType='InvestmentTaken' THEN 'Cr'
                                               WHEN OB.SourceType='InterPlantLoanGiven ' THEN 'Dr'
                                               WHEN OB.SourceType='InterPlantLoanTaken' THEN 'Cr'
                                               WHEN OB.SourceType='InterCompanyLoanGiven' THEN 'Dr'
                                               WHEN OB.SourceType='InterCompanyLoanTaken' THEN 'Cr'
                                               WHEN OB.SourceType='InterCompanyInvestmentGiven' THEN 'Dr'
                                               WHEN OB.SourceType='InterCompanyInvestmentTaken' THEN 'Cr'
		                                       ELSE 'Dr' END
                                , OBD.Amount
		                        , OBD.OpeningBalanceId
		                        , CAST(1 AS bit) AS IsOB
		                        , OBDC.ParallelCurrencyId
		                        , CU.Code AS CurrencyCode
		                        , OBDC.Amount AS Value
		                        , [DrAmount]=CASE WHEN OB.SourceType='CustomerInvoice' THEN  OBDC.Amount
                                               WHEN OB.SourceType='VendorAdvance' THEN  OBDC.Amount
                                               WHEN OB.SourceType='SecurityGiven' THEN  OBDC.Amount
                                               WHEN OB.SourceType='Bank' THEN  OBDC.Amount
                                               WHEN OB.SourceType='EmployeeAdvance' THEN  OBDC.Amount

                                               WHEN OB.SourceType='LoanGiven' THEN  OBDC.Amount
                                               WHEN OB.SourceType='InvestmentGiven' THEN  OBDC.Amount
                                               WHEN OB.SourceType='InterCompanyLoanGiven' THEN  OBDC.Amount
                                               WHEN OB.SourceType='InterCompanyInvestmentGiven' THEN  OBDC.Amount
		                                       ELSE 0 END
		                        , [CrAmount]=CASE WHEN OB.SourceType='CustomerAdvance' THEN OBDC.Amount
                                               WHEN OB.SourceType='VendorInvoice' THEN OBDC.Amount
                                               WHEN OB.SourceType='SecurityTaken' THEN OBDC.Amount
                                               WHEN OB.SourceType='Loan' THEN OBDC.Amount

                                               WHEN OB.SourceType='LoanTaken' THEN  OBDC.Amount
                                               WHEN OB.SourceType='InvestmentTaken' THEN  OBDC.Amount
                                               WHEN OB.SourceType='InterCompanyLoanTaken' THEN  OBDC.Amount
                                               WHEN OB.SourceType='InterCompanyInvestmentTaken' THEN  OBDC.Amount
		                                       ELSE 0 END
		                        , '('+BN.UserName +' - ' AS Bank
		                        , BR.UserName +' - 'AS Branch
		                        , BM.AccountNumber+')'AS AccountNumber
		                        , CO.UserName AS CompanyName, AM.Address1 AS AddressLine
                                FROM [TRN].[OpeningBalanceDetail] AS OBD
                                LEFT JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=OBD.OpeningBalanceId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
		                        LEFT JOIN [TRN].[OpeningBalanceDetailCurrency] AS OBDC ON OBDC.OpeningBalanceDetailId=OBD.id
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=OBD.CurrencyId
		                        LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=OBDC.ParallelCurrencyId
		                        LEFT JOIN [ORG].[Company] AS CO ON CO.Id=OB.CompanyId
		                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=CO.AddressMasterId
		                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=OBD.BankMasterId
		                        LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
		                        LEFT JOIN [HKP].[BankBranch] BR ON BR.Id=BM.BankBranchId
								LEFT JOIN MST.BudgetMaster BMA ON OBD.BudgetMasterId=BMA.Id
								LEFT JOIN HKP.Budget B ON BMA.BudgetId=B.Id
								LEFT JOIN HKP.Activity A ON OBD.ActivityId=A.Id
                                WHERE OB.Archive = 0 AND OB.CompanyId='" + companyId + @"' AND OBDC.ParallelCurrencyId IN (" + parallelCurrency + @")";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Report

        #region Loan Taken
        public List<Dictionary<string, object>> GetOBLoanTakenDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId)
        {
            var CmdText = @"
                        SELECT  OB.SourceType,OBD.PartyType, ACT.BalanceType, BM.COAId,bm.RefNo
                         , OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName
                         , OBD.BudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                         , OBD.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                         , OBD.BankMasterId,OBD.PartyId,OBD.PartyPlantId,BNM.CurrencyId BankCurrencyId
                         , SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountCr
                         , 0 CompanyCurrencyAmountDr,OBD.OpeningBalanceId
                         ,ParticularName= Case when OBD.PartyId <> '' and OBD.PartyType='Party' then 'Party' 
                        					when OBD.PartyId <> '' and OBD.PartyType='Director' then P.UserName
                        					when OBD.BankMasterId<> '' and OBD.PartyType='Bank' then BNM.AccountTitle
                        					else '' end
                        					,OBD.Id LoanOpeningBalanceDetailId,OB.TransactionType
                        FROM TRN.OpeningBalance OB 

                        LEFT JOIN TRN.OpeningBalanceDetail OBD ON OBD.OpeningBalanceId=OB.Id
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                          LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                          LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                          LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId 
                          LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=OBD.BankMasterId 
                          LEFT JOIN [HKP].[Party] AS P ON P.Id=OBD.PartyId 
                        LEFT JOIN (SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
                              FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
                              JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
                              WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
                          ) AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                         WHERE OB.SourceType='Loan' AND OB.TransactionType='LoanTaken' AND OB.CompanyId='" + companyId + @"' AND OB.Id='" + openingBalanceId + @"'
                        GROUP BY OB.SourceType, ACT.BalanceType, OBD.GLGeneralInfoId, GGI.AccountCode, GGI.UserName, OBD.BudgetMasterId, B.Code, B.UserName, OBD.ActivityId, A.Code, A.UserName
                        , BM.COAId, bm.RefNo ,OBD.OpeningBalanceId,OBD.PartyId,OBD.PartyType,OBD.BankMasterId,BNM.AccountTitle,OBD.Id,OB.TransactionType,OBD.PartyPlantId,BNM.CurrencyId,P.UserName
                        ";
            return _sqlRepository.GetDataCollection(CmdText);
        }

        public List<Dictionary<string, object>> GetOBLoanGivenDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId)
        {
            var CmdText = @"
                        SELECT  OB.SourceType,OBD.PartyType, ACT.BalanceType, BM.COAId,bm.RefNo
                         , OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName
                         , OBD.BudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                         , OBD.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                         , OBD.BankMasterId,OBD.PartyId,OBD.PartyPlantId,BNM.CurrencyId BankCurrencyId
                         , SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountDr
                         , 0 CompanyCurrencyAmountCr,OBD.OpeningBalanceId
                         ,ParticularName= Case when OBD.PartyId <> '' and OBD.PartyType='Party' then 'Party' 
                        					when OBD.PartyId <> '' and OBD.PartyType='Director' then P.UserName
                        					when OBD.BankMasterId<> '' and OBD.PartyType='Bank' then BNM.AccountTitle
                        					else '' end
                        					,OBD.Id LoanOpeningBalanceDetailId,OB.TransactionType
                        FROM TRN.OpeningBalance OB 

                        LEFT JOIN TRN.OpeningBalanceDetail OBD ON OBD.OpeningBalanceId=OB.Id
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                          LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                          LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                          LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId 
                          LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=OBD.BankMasterId 
                          LEFT JOIN [HKP].[Party] AS P ON P.Id=OBD.PartyId 
                        LEFT JOIN (SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
                              FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
                              JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
                              WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
                          ) AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                         WHERE OB.SourceType='" + SourceType.Loan.ToString() + "' AND OB.TransactionType='" + PartyType.LoanGiven.ToString() + "' AND OB.CompanyId='" + companyId + @"' AND OB.Id='" + openingBalanceId + @"'
                        GROUP BY OB.SourceType, ACT.BalanceType, OBD.GLGeneralInfoId, GGI.AccountCode, GGI.UserName, OBD.BudgetMasterId, B.Code, B.UserName, OBD.ActivityId, A.Code, A.UserName
                        , BM.COAId, bm.RefNo ,OBD.OpeningBalanceId,OBD.PartyId,OBD.PartyType,OBD.BankMasterId,BNM.AccountTitle,OBD.Id,OB.TransactionType,OBD.PartyPlantId,BNM.CurrencyId,P.UserName
                        ";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        #endregion

        #region Security Given
        public List<Dictionary<string, object>> GetOBSecurityGivenDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId)
        {
            var CmdText = @"
                        
                        SELECT  OB.SourceType,OBD.PartyType, ACT.BalanceType, BM.COAId,bm.RefNo
                         , OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName
                         , OBD.BudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                         , OBD.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                         , SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountDr
                         , 0 CompanyCurrencyAmountCr,OBD.OpeningBalanceId
                         ,ParticularName= Case when OBD.PartyId <> '' and OBD.PartyType='Party' then P.UserName 
                        					when OBD.PartyId <> '' and OBD.PartyType='Director' then P.UserName
                        					when OBD.BankMasterId<> '' and OBD.PartyType='Bank' then BNM.AccountTitle
                        					else '' end
                        					,OBD.Id SecurityOpeningBalanceDetailId,OB.TransactionType,OBD.PartyId,OBD.PartyPlantId,OBD.BankMasterId,OBD.BankCurrencyId,OBD.BankAmount
                        FROM TRN.OpeningBalance OB 

                        LEFT JOIN TRN.OpeningBalanceDetail OBD ON OBD.OpeningBalanceId=OB.Id
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                          LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                          LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                          LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId 
                          LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=OBD.BankMasterId 
                          LEFT JOIN [HKP].[Party] AS P ON P.Id=OBD.PartyId 
                        LEFT JOIN (SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
                              FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
                              JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
                              WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
                          ) AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                         WHERE OB.SourceType='" + SourceType.SecurityDeposit.ToString() + "' AND OB.TransactionType='SecurityGiven' AND OB.CompanyId='" + companyId + @"' AND OB.Id='" + openingBalanceId + @"'
                        GROUP BY OB.SourceType, ACT.BalanceType, OBD.GLGeneralInfoId, GGI.AccountCode, GGI.UserName, OBD.BudgetMasterId, B.Code, B.UserName, OBD.ActivityId, A.Code, A.UserName
                        , BM.COAId, bm.RefNo ,OBD.OpeningBalanceId,OBD.PartyId,OBD.PartyType,OBD.BankMasterId,BNM.AccountTitle,OBD.Id,OB.TransactionType,P.UserName,OBD.PartyPlantId,OBD.BankCurrencyId,OBD.BankAmount
                        ";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        #endregion

        #region Security Taken
        public List<Dictionary<string, object>> GetOBSecurityTakenDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId)
        {
            var CmdText = @"
                        
                        SELECT  OB.SourceType,OBD.PartyType, ACT.BalanceType, BM.COAId,bm.RefNo
                         , OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName
                         , OBD.BudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                         , OBD.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                         , SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountCr
                         , 0 CompanyCurrencyAmountDr,OBD.OpeningBalanceId
                         ,ParticularName= Case when OBD.PartyId <> '' and OBD.PartyType='Party' then P.UserName 
                        					when OBD.PartyId <> '' and OBD.PartyType='Director' then P.UserName
                        					when OBD.BankMasterId<> '' and OBD.PartyType='Bank' then BNM.AccountTitle
                        					else '' end
                        					,OBD.Id SecurityOpeningBalanceDetailId,OB.TransactionType,OBD.PartyId,OBD.PartyPlantId
                        FROM TRN.OpeningBalance OB 

                        LEFT JOIN TRN.OpeningBalanceDetail OBD ON OBD.OpeningBalanceId=OB.Id
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                          LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                          LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                          LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId 
                          LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=OBD.BankMasterId 
                          LEFT JOIN [HKP].[Party] AS P ON P.Id=OBD.PartyId 
                        LEFT JOIN (SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
                              FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
                              JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
                              WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
                          ) AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                         WHERE OB.SourceType='" + SourceType.SecurityDeposit.ToString() + "' AND OB.TransactionType='SecurityTaken' AND OB.CompanyId='" + companyId + @"' AND OB.Id='" + openingBalanceId + @"'
                        GROUP BY OB.SourceType, ACT.BalanceType, OBD.GLGeneralInfoId, GGI.AccountCode, GGI.UserName, OBD.BudgetMasterId, B.Code, B.UserName, OBD.ActivityId, A.Code, A.UserName
                        , BM.COAId, bm.RefNo ,OBD.OpeningBalanceId,OBD.PartyId,OBD.PartyType,OBD.BankMasterId,BNM.AccountTitle,OBD.Id,OB.TransactionType,P.UserName,OBD.PartyPlantId
                        ";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        #endregion

        #region Equity Investment
        public List<Dictionary<string, object>> GetOBEquityDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId)
        {
            var CmdText = @"
                        
                        SELECT  OB.SourceType,OBD.PartyType, ACT.BalanceType, BM.COAId,bm.RefNo
                         , OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName
                         , OBD.BudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                         , OBD.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                         , SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountCr
                         , 0 CompanyCurrencyAmountDr,OBD.OpeningBalanceId
                         ,ParticularName= Case when OBD.PartyId <> '' and OBD.PartyType='Party' then P.UserName 
                        					when OBD.PartyId <> '' and OBD.PartyType='Director' then P.UserName
                        					when OBD.BankMasterId<> '' and OBD.PartyType='Bank' then BNM.AccountTitle
                        					else '' end
                        					,OBD.Id EquityOpeningBalanceDetailId,OB.TransactionType,OBD.PartyId,OBD.PartyPlantId
                        FROM TRN.OpeningBalance OB 

                        LEFT JOIN TRN.OpeningBalanceDetail OBD ON OBD.OpeningBalanceId=OB.Id
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                          LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                          LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                          LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId 
                          LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=OBD.BankMasterId 
                          LEFT JOIN [HKP].[Party] AS P ON P.Id=OBD.PartyId 
                        LEFT JOIN (SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
                              FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
                              JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
                              WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
                          ) AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                         WHERE OB.SourceType='" + SourceType.Investment.ToString() + "' AND OB.TransactionType='InvestmentTaken' AND OB.CompanyId='" + companyId + @"' AND OB.Id='" + openingBalanceId + @"'
                        GROUP BY OB.SourceType, ACT.BalanceType, OBD.GLGeneralInfoId, GGI.AccountCode, GGI.UserName, OBD.BudgetMasterId, B.Code, B.UserName, OBD.ActivityId, A.Code, A.UserName
                        , BM.COAId, bm.RefNo ,OBD.OpeningBalanceId,OBD.PartyId,OBD.PartyType,OBD.BankMasterId,BNM.AccountTitle,OBD.Id,OB.TransactionType,P.UserName,OBD.PartyPlantId
                        ";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        public List<Dictionary<string, object>> GetOBInvestmentDetailGL(string companyGroupId, string companyId, string plantId, string openingBalanceId)
        {
            var CmdText = @"
                        
                        SELECT  OB.SourceType,OBD.PartyType, ACT.BalanceType, BM.COAId,bm.RefNo
                         , OBD.GLGeneralInfoId, GGI.AccountCode AS GLGeneralInfoCode, GGI.UserName AS GLGeneralInfoName
                         , OBD.BudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                         , OBD.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                         , SUM(CC.CompanyCurrencyAmount) AS CompanyCurrencyAmountDr
                         , 0 CompanyCurrencyAmountCr,OBD.OpeningBalanceId
                         ,ParticularName= Case when OBD.PartyId <> '' and OBD.PartyType='Party' then P.UserName 
                        					when OBD.PartyId <> '' and OBD.PartyType='Director' then P.UserName
                        					when OBD.BankMasterId<> '' and OBD.PartyType='Bank' then BNM.AccountTitle
                        					else '' end
                        					,OBD.Id InvestmentOpeningBalanceDetailId,OB.TransactionType,OBD.PartyId,OBD.PartyPlantId
                        FROM TRN.OpeningBalance OB 

                        LEFT JOIN TRN.OpeningBalanceDetail OBD ON OBD.OpeningBalanceId=OB.Id
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=OBD.GLGeneralInfoId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                          LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                          LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GGI.AccountGroupId
                          LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId 
                          LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=OBD.BankMasterId 
                          LEFT JOIN [HKP].[Party] AS P ON P.Id=OBD.PartyId 
                        LEFT JOIN (SELECT OBDC.Amount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
                              FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
                              JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
                              WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
                          ) AS CC ON CC.OpeningBalanceDetailId=OBD.Id
                         WHERE OB.SourceType='" + SourceType.Investment.ToString() + "' AND OB.TransactionType='InvestmentGiven' AND OB.CompanyId='" + companyId + @"' AND OB.Id='" + openingBalanceId + @"'
                        GROUP BY OB.SourceType, ACT.BalanceType, OBD.GLGeneralInfoId, GGI.AccountCode, GGI.UserName, OBD.BudgetMasterId, B.Code, B.UserName, OBD.ActivityId, A.Code, A.UserName
                        , BM.COAId, bm.RefNo ,OBD.OpeningBalanceId,OBD.PartyId,OBD.PartyType,OBD.BankMasterId,BNM.AccountTitle,OBD.Id,OB.TransactionType,P.UserName,OBD.PartyPlantId
                        ";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        #endregion

        #region Delete Back Data Account CutOffDate

        public List<Dictionary<string, object>> GetCutOffBackDateData(string companyGroupId, string companyId, string plantId, DateTime cutOffDate)
        {
            var CmdText = @"
                        SELECT   V.CompanyGroupId,V.CompanyId,V.PlantId,V.SourceType,ISNULL(SUM(VD.DrAmount),0) DrAmount,ISNULL(SUM(VD.CrAmount),0) CrAmount
                            FROM TRN.VoucherDetail AS VD 
                            LEFT JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                            WHERE V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId ='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.PostingDate < '" + cutOffDate + @"'
                            group by  V.CompanyGroupId,V.CompanyId,V.PlantId,V.SourceType";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        public List<Dictionary<string, object>> GetEmployeePayableCutOffAfterPostingDateData(string companyGroupId, string companyId, string plantId, DateTime cutOffDate)
        {
            var CmdText = @"
                        SELECT VD.VoucherId, V.CompanyId, V.PlantId, V.VoucherNo, V.DocRefNo, Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PaymentPostingDate, Replace(CONVERT(VARCHAR(11), EP.PostingDate, 106), ' ', '-') AS PayablePostingDate
                            , V.SourceType, EW.Id EmployeePayableWriteOffId, SUM(VD.DrAmount) Amount
                            FROM TRN.Voucher V
                            LEFT JOIN trn.Voucherdetail VD on VD.VoucherId=V.Id
                            LEFT JOIN TRN.EmployeePayableWriteOff EW ON EW.VoucherId=V.Id
                            LEFT JOIN TRN.EmployeePayableWriteOffDetail EWD ON EW.Id=EWD.EmployeePayableWriteOffId
                            LEFT JOIN TRN.EmployeePayable EP ON EP.Id=EWD.EmployeePayableId
                            WHERE V.Id IN (SELECT VoucherId FROM trn.EmployeePayableWriteOff WHERE Id in 
                            			(SELECT EmployeePayableWriteOffId FROM trn.EmployeePayableWriteOffDetail where EmployeePayableDetailId in 
                            			(select Id from TRN.EmployeePayableDetail where EmployeePayableId in 
                            			(select Id from TRN.EmployeePayable where VoucherId in 
                            			(select Id from trn.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType= '" + SourceType.EmployeePayable + "' AND PostingDate < '" + cutOffDate + @"')))))
                            AND V.PostingDate >='" + cutOffDate + @"'
                            GROUP BY VD.VoucherId, V.DocRefNo, V.PostingDate, V.SourceType, EW.Id, EP.PostingDate, V.VoucherNo, V.CompanyId, V.PlantId";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        /*
         * * ********voucher level data of GetEmployeePayableCutOffAfterPostingDateData****************
         * 
         * 
         SELECT VD.VoucherId, V.VoucherNo, V.DocRefNo, Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PaymentPostingDate, Replace(CONVERT(VARCHAR(11), EP.PostingDate, 106), ' ', '-') AS PayablePostingDate
                            , V.SourceType, EW.Id EmployeePayableWriteOffId,GL.UserName GL,b.UserName Budget,A.UserName Activity,c.UserName Cash
							,VD.DrAmount,VD.CrAmount--, SUM(VD.DrAmount) Amount
                            FROM TRN.Voucher V
                            LEFT JOIN trn.Voucherdetail VD on VD.VoucherId=V.Id
                            LEFT JOIN TRN.EmployeePayableWriteOff EW ON EW.VoucherId=V.Id
                            LEFT JOIN TRN.EmployeePayableWriteOffDetail EWD ON EW.Id=EWD.EmployeePayableWriteOffId
                            LEFT JOIN TRN.EmployeePayable EP ON EP.Id=EWD.EmployeePayableId
							left join HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
							LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
							LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
							LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
							LEFT JOIN MST.CashMaster C ON C.Id=VD.CashMasterId
							LEFT JOIN MST.BankMaster BK ON BK.Id=VD.BankMasterId
                            WHERE V.Id IN (SELECT VoucherId FROM trn.EmployeePayableWriteOff WHERE Id in 
                            			(SELECT EmployeePayableWriteOffId FROM trn.EmployeePayableWriteOffDetail where EmployeePayableDetailId in 
                            			(select Id from TRN.EmployeePayableDetail where EmployeePayableId in 
                            			(select Id from TRN.EmployeePayable where VoucherId in 
                            			(select Id from trn.Voucher where CompanyId='C20171' AND PlantId='20171' AND SourceType= 'EmployeePayable' AND PostingDate < '9/1/2019')))))
                            AND V.PostingDate >='9/1/2019'
							--GROUP BY VD.VoucherId, V.DocRefNo, V.PostingDate, V.SourceType, EW.Id, EP.PostingDate, V.VoucherNo,V.VoucherDate

            
             * 
             */
        public string DeleteEmployeePayableCutOffAfterPostingDateData(IEnumerable<VoucherDetailViewModel> voucherDetailVM)
        {
            var flag = false;
            try
            {
                foreach (var voucherVM in voucherDetailVM)
                {
                    if (voucherVM.SourceType == SourceType.EmployeePayment.ToString())
                    {
                        // Delete Loan
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        var empAdvance = new System.Text.StringBuilder();
                        var empadsql = "";

                        empadsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayment.ToString() + "' AND Id= '" + voucherVM.VoucherId + "')";
                        empAdvance.Append(empadsql);
                        empadsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayment.ToString() + "' AND Id= '" + voucherVM.VoucherId + "'))";
                        empAdvance.Append(empadsql);
                        empadsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayment.ToString() + "' AND Id= '" + voucherVM.VoucherId + "')";
                        empAdvance.Append(empadsql);
                        empadsql = @"delete TRN.EmployeePayableWriteOffDetail where EmployeePayableWriteOffId in (select Id from TRN.EmployeePayableWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayment.ToString() + "' AND Id= '" + voucherVM.VoucherId + "'))";
                        empAdvance.Append(empadsql);
                        empadsql = @"delete TRN.EmployeePayableWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayment.ToString() + "' AND Id= '" + voucherVM.VoucherId + "')";
                        empAdvance.Append(empadsql);
                        empadsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayment.ToString() + "' AND Id= '" + voucherVM.VoucherId + "')";
                        empAdvance.Append(empadsql);
                        _sqlRepository.ExecuteSqlCommand(empAdvance.ToString());
                        _unitOfWork.SaveChanges();
                        flag = false;
                        _unitOfWork.Commit();
                    }
                }

                return "Delete Successful";
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public List<Dictionary<string, object>> GetVendorPayableCutOffAfterPostingDateData(string companyGroupId, string companyId, string plantId, DateTime cutOffDate)
        {
            var CmdText = @"
                        SELECT VD.VoucherId, V.CompanyId, V.PlantId, V.VoucherNo, V.DocRefNo, Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PaymentPostingDate, Replace(CONVERT(VARCHAR(11), EP.PostingDate, 106), ' ', '-') AS PayablePostingDate
                            , V.SourceType, EW.Id EmployeePayableWriteOffId, SUM(VD.DrAmount) Amount
                            FROM TRN.Voucher V
                            LEFT JOIN trn.Voucherdetail VD on VD.VoucherId=V.Id
                            LEFT JOIN TRN.InvoiceWriteOff EW ON EW.VoucherId=V.Id
                            LEFT JOIN TRN.InvoiceWriteOffDetail EWD ON EW.Id=EWD.InvoiceWriteOffId
                            LEFT JOIN TRN.Invoice EP ON EP.Id=EWD.InvoiceId
                            WHERE V.Id IN (SELECT VoucherId FROM trn.InvoiceWriteOff WHERE Id in 
                            			(SELECT InvoiceWriteOffId FROM trn.InvoiceWriteOffDetail where InvoiceDetailId in 
                            			(select Id from TRN.InvoiceDetail where InvoiceId in 
                            			(select Id from TRN.Invoice where VoucherId in 
                            			(select Id from trn.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType= '" + SourceType.VendorInvoice + "' AND PostingDate < '" + cutOffDate + @"')))))
                            AND V.PostingDate >='" + cutOffDate + @"'
                            GROUP BY VD.VoucherId, V.DocRefNo, V.PostingDate, V.SourceType, EW.Id, EP.PostingDate, V.VoucherNo, V.CompanyId, V.PlantId";
            return _sqlRepository.GetDataCollection(CmdText);
        }
        /*
        * ********voucher level data of GetVendorPayableCutOffAfterPostingDateData****************
        * 
        * 
         SELECT VD.VoucherId, V.VoucherNo, V.DocRefNo, Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PaymentPostingDate, Replace(CONVERT(VARCHAR(11), EP.PostingDate, 106), ' ', '-') AS PayablePostingDate
                           , V.SourceType, EW.Id EmployeePayableWriteOffId
                           ,GL.UserName GL,b.UserName Budget,A.UserName Activity,c.UserName Cash,BK.AccountTitle
                           ,VD.DrAmount,VD.CrAmount--, SUM(VD.DrAmount) Amount
                           FROM TRN.Voucher V
                           LEFT JOIN trn.Voucherdetail VD on VD.VoucherId=V.Id
                           LEFT JOIN TRN.InvoiceWriteOff EW ON EW.VoucherId=V.Id
                           LEFT JOIN TRN.InvoiceWriteOffDetail EWD ON EW.Id=EWD.InvoiceWriteOffId
                           LEFT JOIN TRN.Invoice EP ON EP.Id=EWD.InvoiceId
                           left join HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
                           LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                           LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                           LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
                           LEFT JOIN MST.CashMaster C ON C.Id=VD.CashMasterId
                           LEFT JOIN MST.BankMaster BK ON BK.Id=VD.BankMasterId
                           LEFT JOIN HKP.Party P ON P.Id =VD.PartyId
                           WHERE V.Id IN (SELECT VoucherId FROM trn.InvoiceWriteOff WHERE Id in 
                                       (SELECT InvoiceWriteOffId FROM trn.InvoiceWriteOffDetail where InvoiceDetailId in 
                                       (select Id from TRN.InvoiceDetail where InvoiceId in 
                                       (select Id from TRN.Invoice where VoucherId in 
                                       (select Id from trn.Voucher where CompanyId='C20171' AND PlantId='20171' AND SourceType= 'VendorInvoice' AND PostingDate < '9/1/2019 12:00:00 AM')))))
                                   AND V.PostingDate >='9/1/2019'
                        --   GROUP BY VD.VoucherId, V.DocRefNo, V.PostingDate, V.SourceType, EW.Id, EP.PostingDate, V.VoucherNo
            
             *
             */

        public string DeleteVendorPayableCutOffAfterPostingDateData(IEnumerable<VoucherDetailViewModel> voucherDetailVM)
        {
            var flag = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                foreach (var voucherVM in voucherDetailVM)
                {
                    if (voucherVM.SourceType == SourceType.VendorPayment.ToString())
                    {
                        // Delete Loan
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        var vendorAdWr = new System.Text.StringBuilder();
                        var vendorAdWrsql = "";

                        vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "'))";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"update trn.VoucherDetail set UpdatedBy='" + identity.UserId + @"',  InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "'))";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete TRN.InvoiceWriteOffDetail where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "'))";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete TRN.BankCharge where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "'))";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND Id = '" + voucherVM.VoucherId + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                        _unitOfWork.SaveChanges();
                        flag = false;
                        _unitOfWork.Commit();
                    }
                }

                return "Delete Successful";
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
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public string PostDeleteAccCutOffDateBackData(VoucherViewModel voucherVM)
        {
            var flag = false;
            try
            {
                if (voucherVM.SourceType == SourceType.AdvanceJournalVoucher.ToString())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var jv = new System.Text.StringBuilder();
                    var jvsql = "";
                    jvsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher  where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.AdvanceJournalVoucher.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    jv.Append(jvsql);
                    jvsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.AdvanceJournalVoucher.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    jv.Append(jvsql);
                    jvsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.AdvanceJournalVoucher.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    jv.Append(jvsql);
                    jvsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.AdvanceJournalVoucher.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    jv.Append(jvsql);
                    _sqlRepository.ExecuteSqlCommand(jv.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();

                }

                //Cash Journal
                if (voucherVM.SourceType == SourceType.CashJournal.ToString())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var cashJv = new System.Text.StringBuilder();
                    var cashJvsql = "";
                    cashJvsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.CashJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    cashJv.Append(cashJvsql);
                    cashJvsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where  CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.CashJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    cashJv.Append(cashJvsql);
                    cashJvsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.CashJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    cashJv.Append(cashJvsql);
                    cashJvsql = @"delete TRN.BankJournalDetail where BankJournalId in (select id from TRN.BankJournal where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.CashJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    cashJv.Append(cashJvsql);
                    cashJvsql = @"delete TRN.BankJournal where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.CashJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    cashJv.Append(cashJvsql);
                    cashJvsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.CashJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    cashJv.Append(cashJvsql);
                    _sqlRepository.ExecuteSqlCommand(cashJv.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                //Cash Journal
                if (voucherVM.SourceType == SourceType.BankJournal.ToString())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var bankJv = new System.Text.StringBuilder();
                    var bankJvsql = "";
                    bankJvsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.BankJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    bankJv.Append(bankJvsql);
                    bankJvsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.BankJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    bankJv.Append(bankJvsql);
                    bankJvsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.BankJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    bankJv.Append(bankJvsql);
                    bankJvsql = @"delete TRN.BankJournalDetail where BankJournalId in (select id from TRN.BankJournal where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.BankJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    bankJv.Append(bankJvsql);
                    bankJvsql = @"delete TRN.BankCharge where BankJournalId in (select id from TRN.BankJournal where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.BankJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    bankJv.Append(bankJvsql);
                    bankJvsql = @"delete TRN.BankJournal where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.BankJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    bankJv.Append(bankJvsql);
                    bankJvsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.BankJournal.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    bankJv.Append(bankJvsql);
                    _sqlRepository.ExecuteSqlCommand(bankJv.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }

                if (voucherVM.SourceType == SourceType.LoanPayment.ToString())
                {
                    // Delete Loan Payment
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var loanPayment = new System.Text.StringBuilder();
                    var loanPaymentsql = "";

                    loanPaymentsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.LoanPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.LoanPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.LoanPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from trn.FinancingWriteOff where VoucherId in (select Id from TRN.Voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.LoanPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.FinancingWriteOff where Voucherid in (select Id from TRN.Voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.LoanPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.LoanPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);
                    _sqlRepository.ExecuteSqlCommand(loanPayment.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();

                }
                if (voucherVM.SourceType == SourceType.Loan.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var loanPayment = new System.Text.StringBuilder();
                    var loanPaymentsql = "";

                    loanPaymentsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.Loan.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.Loan.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.Loan.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.FinancingDetail where FinancingId in (select Id from trn.Financing where VoucherId in (select Id from TRN.Voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.Loan.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.FinancingSchedule where FinancingId in (select Id from trn.Financing where VoucherId in (select Id from TRN.Voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.Loan.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    loanPayment.Append(loanPaymentsql);
                    loanPaymentsql = @"delete trn.Financing where Voucherid in (select Id from TRN.Voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.Loan.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);

                    loanPaymentsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.Loan.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    loanPayment.Append(loanPaymentsql);
                    _sqlRepository.ExecuteSqlCommand(loanPayment.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }

                if (voucherVM.SourceType == SourceType.EmployeeAdvanceWriteOff.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var empAdvance = new System.Text.StringBuilder();
                    var empadsql = "";

                    empadsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.AdvanceWriteOffDetail where AdvanceWriteOffId in (select Id from TRN.AdvanceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.AdvanceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.EmployeePayableWriteOffDetail where EmployeePayableWriteOffId in (select Id from TRN.EmployeePayableWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeeAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.EmployeePayableWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeeAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    _sqlRepository.ExecuteSqlCommand(empAdvance.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                if (voucherVM.SourceType == SourceType.EmployeeAdvance.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var empAdvance = new System.Text.StringBuilder();
                    var empadsql = "";

                    empadsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.AdvanceDetail where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeeAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    _sqlRepository.ExecuteSqlCommand(empAdvance.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                if (voucherVM.SourceType == SourceType.EmployeePayment.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var empAdvance = new System.Text.StringBuilder();
                    var empadsql = "";

                    empadsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.EmployeePayableWriteOffDetail where EmployeePayableWriteOffId in (select Id from TRN.EmployeePayableWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.EmployeePayableWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    _sqlRepository.ExecuteSqlCommand(empAdvance.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }

                if (voucherVM.SourceType == SourceType.EmployeePayable.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var empAdvance = new System.Text.StringBuilder();
                    var empadsql = "";

                    empadsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.EmployeePayableDetail where EmployeePayableId in (select Id from TRN.EmployeePayable where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete TRN.EmployeePayable where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.ExpenseBookingApprovalHistory where ExpenseBookingId in (select Id from trn.ExpenseBooking where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.ExpenseActivity where ExpenseBookingId in (select Id from trn.ExpenseBooking where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.ExpenseBookingDetail where ExpenseBookingId in (select Id from trn.ExpenseBooking where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.InvoiceDetail where InvoiceId in (select Id from trn.Invoice where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.Invoice where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.ExpenseBooking where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    empadsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.EmployeePayable.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    empAdvance.Append(empadsql);
                    _sqlRepository.ExecuteSqlCommand(empAdvance.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                if (voucherVM.SourceType == SourceType.InterTransaction.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var vendorAdWr = new System.Text.StringBuilder();
                    var vendorAdWrsql = "";

                    vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update trn.VoucherDetail set UpdatedBy='" + identity.UserId + @"',  InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.InvoiceWriteOffDetail where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.BankCharge where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.AdvanceWriteOffDetail where AdvanceWriteOffId in (select Id from TRN.AdvanceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.AdvanceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.AdvanceDetail where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.BankCharge where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.InterTransaction.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                if (voucherVM.SourceType == SourceType.VendorAdvanceWriteOff.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var vendorAdWr = new System.Text.StringBuilder();
                    var vendorAdWrsql = "";

                    vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.AdvanceWriteOffDetail where AdvanceWriteOffId in (select Id from TRN.AdvanceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.AdvanceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.InvoiceWriteOffDetail where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvanceWriteOff.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                if (voucherVM.SourceType == SourceType.VendorAdvance.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var vendorAdWr = new System.Text.StringBuilder();
                    var vendorAdWrsql = "";

                    vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update trn.VoucherDetail set UpdatedBy='" + identity.UserId + @"',  InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.AdvanceDetail where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.BankCharge where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorAdvance.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                if (voucherVM.SourceType == SourceType.VendorPayment.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var vendorAdWr = new System.Text.StringBuilder();
                    var vendorAdWrsql = "";

                    vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update trn.VoucherDetail set UpdatedBy='" + identity.UserId + @"',  InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.InvoiceWriteOffDetail where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.BankCharge where InvoiceWriteOffId in (select Id from TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.InvoiceWriteOff where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorPayment.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                if (voucherVM.SourceType == SourceType.VendorInvoice.ToString())
                {
                    // Delete Loan
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var vendorAdWr = new System.Text.StringBuilder();
                    var vendorAdWrsql = "";

                    vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update trn.VoucherDetail set UpdatedBy='" + identity.UserId + @"',  InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.InvoiceDetail where InvoiceId in (select Id from TRN.Invoice where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "'))";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete TRN.Invoice where voucherId in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType= '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + voucherVM.CompanyId + "' AND PlantId='" + voucherVM.PlantId + "' AND SourceType = '" + SourceType.VendorInvoice.ToString() + "' AND PostingDate < '" + voucherVM.PostingDate + "')";
                    vendorAdWr.Append(vendorAdWrsql);
                    _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                return "Delete " + voucherVM.SourceType;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, voucherVM.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        /* Mr. Taufiq u do you report from here*/

        public IWorkbook CreatePaybleVSpaymentReportSheet(string companyId, string plantId, string fromDate)//, string fromDate, string toDate, string Type
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                //var sheet2 = workbook.Worksheets[1];
                var Head = "Material Return Register";// +" " + fromDate + " " + "To" + " " + toDate
                CreatePaybleVSpaymentReportSheet(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate);//, fromDate, toDate, Type
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private void CreatePaybleVSpaymentReportSheet(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate)//, string fromDate, string toDate, string Type
        {




            var cmdText = "";
            cmdText = @"SELECT   VP.VoucherNo PayableVoucherNo, Replace(CONVERT(VARCHAR(11), EP.PostingDate, 106), ' ', '-') AS PayablePostingDate,EP.DocRefNo PayableDocRefNo,EM.EmployeeName
                           ,EP.Amount ,EP.WrittenOffAmount Paid,(EP.Amount - EP.WrittenOffAmount) Balance
						   , V.VoucherNo PaymentVoucherNo, V.DocRefNo PaymentDocRefNo, Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PaymentPostingDate
						    ,EW.Amount PaymentAmount ,V.Narration,GL.UserName GL , B.UserName Budget,A.UserName Actiivty
                            FROM TRN.Voucher V
                            LEFT JOIN TRN.EmployeePayableWriteOff EW ON EW.VoucherId=V.Id
                            LEFT JOIN TRN.EmployeePayableWriteOffDetail EWD ON EW.Id=EWD.EmployeePayableWriteOffId
                            LEFT JOIN TRN.EmployeePayable EP ON EP.Id=EWD.EmployeePayableId
                            LEFT JOIN TRN.EmployeePayableDetail EPD ON EPD.EmployeePayableId=EP.Id
							LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=EPD.GLGeneralInfoId
							LEFT JOIN MST.BudgetMaster BM ON BM.Id=EPD.BudgetMasterId
							LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
							LEFT JOIN HKP.Activity A ON A.Id=EPD.ActivityId
							LEFT JOIN TRN.Voucher VP ON VP.Id=EP.VoucherId

							LEFT JOIN dbo.EmployeeInformation EM ON EM.SystemId=EP.EmployeeId
                            WHERE V.Id IN (SELECT VoucherId FROM trn.EmployeePayableWriteOff WHERE Id in 
                            			(SELECT EmployeePayableWriteOffId FROM trn.EmployeePayableWriteOffDetail where EmployeePayableDetailId in 
                            			(select Id from TRN.EmployeePayableDetail where EmployeePayableId in 
                            			(select Id from TRN.EmployeePayable where VoucherId in 

		(select Id from trn.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType= 'EmployeePayable' AND PostingDate < '" + fromDate + @"')))))
                            AND V.PostingDate >='" + fromDate + @"'ORDER BY V.PostingDate ASC";
            //  where  IR.PlantId='" + plantId + "' AND convert(Date,IR.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' ORDER BY IR.POReturnDate ASC";

            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _row = 5;

            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Payable Voucher No");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Payable Posting Date");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Payable DocRef No");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Employee Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Paid");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Balance");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Payment Voucher No");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Payment Doc Ref No");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Payment Posting Date");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Payment Amount");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Narration");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            sheet1headreColIndex++;


            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Actiivty");



            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["PayableVoucherNo"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["PayablePostingDate"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["PayableDocRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["EmployeeName"].ToString());
                report.SetText(ref sheet1, _rowL, 5, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Amount"].ToString()).ToString("F2"));
                report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Balance"].ToString()).ToString("F2"));
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["PaymentVoucherNo"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["PaymentDocRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["PaymentPostingDate"].ToString());
                report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PaymentAmount"].ToString()).ToString("F2"));
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["Narration"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["GL"].ToString());
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["Budget"].ToString());
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["Actiivty"].ToString());

            }



            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            _rowL++;


            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);


            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

        }
        #endregion


        #region Material-Master-Opening-Balance-Report




        public IWorkbook CreateMaterialMasterOpeningBalanceReport(string companyId, string plantId, string fromDate, string toDate)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                //var sheet2 = workbook.Worksheets[1];
                var Head = "Material Master Opening Balance" + " " + fromDate + " " + "To" + " " + toDate;
                CreateMaterialMasterOpeningBalanceReport(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }





        private void CreateMaterialMasterOpeningBalanceReport(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate)
        {

            var cmdText = "";
            cmdText = @"SELECT 
									 FOB.DocRefNo
								     , REPLACE(CONVERT(CHAR(11), FOB.PostingDate, 106),' ','-') AS PostingDate
									 ,MSR.UserName MaterialStorage
									, MM.UserName MaterialMasterName
									, MMA.StandardName ArticleName,FOBD.FirstCharacteristicsId
									,FOBD.FirstCharacteristicsValueId,FOBD.SecondCharacteristicsId
									,FOBD.SecondCharacteristicsValueId
									,FOBD.ThirdCharacteristicsId,FOBD.ThirdCharacteristicsValueId
									,UOM.UserName BaseUOM,FOBD.Quantity,FOBD.Amount
							        ,AGL.AccountCode+' - '+AGL.UserName AS GLName,AB.UserName BudgetName,AC.UserName ActivityName
                                   FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                                    LEFT JOIN [TRN].[OpeningBalance] AS FOB ON FOBD.OpeningBalanceId=FOB.Id
									LEFT JOIN [SCS].[UnitOfMeasurement] AS UOM ON UOM.Id=FOBD.BaseUOMId
									LEFT JOIN HKP.GLGeneralInfo AGL ON FOBD.AssetGLId=AGL.Id
									LEFT JOIN MST.BudgetMaster BM ON FOBD.AssetBudgetMasterId=BM.Id
									LEFT JOIN HKP.Budget AB ON BM.BudgetId=AB.Id
                                    LEFT JOIN HKP.Activity AC ON FOBD.AssetActivityId=AC.Id
									LEFT JOIN MST.MaterialMaster MM ON FOBD.MaterialMasterId=MM.Id
									LEFT JOIN MST.MaterialMasterArticle MMA ON FOBD.ArticleId = MMA.Id
                                    LEFT JOIN HKP.Characteristics AS FC ON FOBD.FirstCharacteristicsId=FC.Id
									LEFT JOIN HKP.Characteristics AS SC ON FOBD.SecondCharacteristicsId=SC.Id
									LEFT JOIN HKP.Characteristics AS TC ON FOBD.ThirdCharacteristicsId=TC.Id
									LEFT JOIN HKP.CharacteristicsValue AS FCV ON FOBD.FirstCharacteristicsValueId=FCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS SCV ON FOBD.SecondCharacteristicsValueId=SCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS TCV ON FOBD.ThirdCharacteristicsValueId=TCV.Id
                                    LEFT JOIN HKP.MaterialStorage MSR ON MSR.Id=FOBD.MaterialStorageId
									where  FOBD.PlantId='" + plantId + "' AND convert(Date,FOB.PostingDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' ORDER BY FOB.PostingDate ASC";

            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _row = 5;

            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "DocRef No");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Posting Date");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Storage");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Master Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Uom");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
            sheet1headreColIndex++;


            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget Name");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity Name");



            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["PostingDate"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialStorage"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValueId"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValueId"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValueId"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["BaseUOM"].ToString());
                report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Quantity"].ToString()));
                report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Amount"].ToString()));
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["GLName"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["BudgetName"].ToString());
                report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ActivityName"].ToString()));


            }




            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);


            _rowL++;


            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);



            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

            //sheet2.Name = sheet2Name;
            //sheet2.UsedRange.WrapText = true;
            //sheet2.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyPlantHeader(ref sheet2, sheet2headreColIndex, sheet2Name, companyId, plantName, null);
            //report.PageSetup(ref sheet2, 5, ExcelPageOrientation.Landscape);
        }


        #endregion Material Stock Ledeger 
    }
}