using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Vouchers;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Service.Finances
{
    public class InvestmentService : IInvestmentService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly IFinancingService _financingService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IFinancingTypeGLService _financingTypeGLService;
        private readonly IRepositoryAsync<BankMaster> _bankMasterRepository;
        private readonly IRepositoryAsync<CashMaster> _cashMasterRepository;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly IRepositoryAsync<CompanyPartyGL> _companyPartyGLRepository;
        private readonly IRepositoryAsync<FinancingSubsequentTransaction> _loanInterestPayableRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        public InvestmentService(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IVoucherService voucherService
            , IFinancingService financingService
            , ICompanyFiscalYearService companyFiscalYearService
            , ICompanyTaxYearService companyTaxYearService
            , IFinancingTypeGLService financingTypeGLService
            , IRepositoryAsync<BankMaster> bankMasterRepository
            , IRepositoryAsync<CashMaster> cashMasterRepository
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IRepositoryAsync<CompanyPartyGL> companyPartyGLRepository
            , IPKGeneratorService pkGeneratorService
             , IRepositoryAsync<FinancingSubsequentTransaction> loanInterestPayableRepository
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
            _financingService = financingService;
            _bankMasterRepository = bankMasterRepository;
            _cashMasterRepository = cashMasterRepository;
            _companyTaxYearService = companyTaxYearService;
            _companyFiscalYearService = companyFiscalYearService;
            _financingTypeGLService = financingTypeGLService;
            _companyPartyRepository = companyPartyRepository;
            _companyPartyGLRepository = companyPartyGLRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _loanInterestPayableRepository = loanInterestPayableRepository;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, A.FinancingNo, A.Id, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId, PP.UserName AS PartyPlantName, A.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , A.VoucherId, A.PostingDate, A.DocDate, A.DocRefNo, A.CurrencyId, C.Code AS CurrencyCode, A.Amount, A.IsWrittenOff, A.WrittenOffAmount, A.IsPark, A.IsPosted, A.TransactionType
                                FROM [TRN].[Financing] AS A
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=A.EmployeeId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                                WHERE A.OpeningBalanceId IS NULL AND A.Archive=0 AND V.Archive=0 AND A.CompanyGroupId='" + companyGroupId + "'AND A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' AND A.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        private string GetSubsequentInvestmentPK()
        {
            return _pkGeneratorService.GetAutoNumber("FinancingSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertInvestment(VoucherViewModel voucherVM)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Financing TABLE
                var financing = new Financing
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    FinancingTypeId = voucherVM.FinancingTypeId,
                    EmployeeId = voucherVM.EmployeeId,
                    PartyType = voucherVM.PartyType,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType,
                    PaymentSource = voucherVM.PaymentSource,
                    Amount = voucherVM.Amount,
                    OtherBankMasterId = voucherVM.OtherBankMasterId,
                    BankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    TransactionType = voucherVM.TransactionType,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    IsPark = voucherVM.IsPark,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    VoucherDate = voucherVM.VoucherDate
                };
                _financingService.InsertFinancing(financing);
                var voucher = _voucherService.InsertVoucher(voucherVM);
                var loanInterestPayable = new FinancingSubsequentTransaction
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FinancingId = financing.Id,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType,
                    CurrencyId = voucherVM.CurrencyId,
                    Amount = voucherVM.Amount,
                    DownPaymentAmount = voucherVM.DownPaymentAmount,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    TransactionType = "Investment",
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType.ToString(),
                    IsPark = voucherVM.IsPark,
                    Id = "SI" + GetSubsequentInvestmentPK(),
                    VoucherId = voucher.Id
                };
                AuditService.AddedLog(loanInterestPayable);
                _loanInterestPayableRepository.Insert(loanInterestPayable);
                // Set to Financing
                financing.VoucherId = voucher.Id;

                // INSERT INTO FinancingDetail
                var investmentDetail = new FinancingDetail
                {
                    Amount = financing.Amount,
                };

                // Investment from side Voucher detail row.
                var voucherDetailFrom = new VoucherDetail
                {
                    PartyType = financing.PartyType,
                    PaymentSource = financing.PaymentSource
                };

                // Investment to side Voucher detail row.
                var voucherDetailTo = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                _financingService.InsertFinancingDetail(financing, investmentDetail);

                if (financing.TransactionType == TransactionType.InvestmentGiven.ToString())
                {
                    #region From

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _bankMasterRepository.Find(financing.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
                        if (null == bankMaster.ActivityId)
                            throw new CustomException("Bank Activity not found!");
                       

                        voucherDetailFrom.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailFrom.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailFrom.ActivityId = bankMaster.ActivityId;

                        voucherDetailFrom.BankMasterId = bankMaster.Id;
                        voucherDetailFrom.TrnNature = TransactionNature.Bank.ToString();

                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        voucherDetailFrom.CashMasterId = voucherVM.CashMasterId;
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash is not configured!");
                        voucherDetailFrom.TrnNature = TransactionNature.ToCash.ToString();
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);

                        if (null == cashMaster)
                            throw new CustomException("Cash data not found!");
                        if (null == cashMaster.ActivityId)
                            throw new CustomException("Cash Activity not found!");
                        voucherDetailFrom.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailFrom.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailFrom.ActivityId = cashMaster.ActivityId;

                        investmentDetail.CashMasterId = cashMaster.Id;
                        investmentDetail.GLGeneralInfoId = voucherDetailFrom.GLGeneralInfoId;
                        investmentDetail.BudgetMasterId = voucherDetailFrom.BudgetMasterId;
                        investmentDetail.ActivityId = voucherDetailFrom.ActivityId;
                        // TODO: have to add cash master.
                    }
                    else
                        throw new CustomException("Payment Source not found!");
                    // Set amount in Voucher detail in Credit side.
                    voucherDetailFrom.CrAmount = investmentDetail.Amount;
                    //voucherDetailFrom.FinancingDetailId = investmentDetail.Id;

                    #endregion From

                    #region To

                    var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                    if (string.IsNullOrEmpty(gl.AssetGLId))
                        throw new CustomException("This Transaction Type GL not Found!");
                    if (string.IsNullOrEmpty(gl.AssetActivityId))
                        throw new CustomException("Activity not found!");
                    voucherDetailTo.GLGeneralInfoId = gl.AssetGLId;
                    voucherDetailTo.BudgetMasterId = gl.AssetBudgetMasterId;
                    voucherDetailTo.ActivityId = gl.AssetActivityId;
                    voucherDetailTo.FinancingDetailId = investmentDetail.Id;
                    investmentDetail.GLGeneralInfoId = gl.AssetGLId;
                    investmentDetail.BudgetMasterId = gl.AssetBudgetMasterId;
                    investmentDetail.ActivityId = gl.AssetActivityId;

                    if (voucherVM.PartyType == PartyType.Vendor.ToString() || voucherVM.PartyType == PartyType.Customer.ToString() || voucherVM.PartyType == PartyType.Director.ToString())
                    {
                        voucherDetailTo.PartyId = voucherVM.PartyId;
                        voucherDetailTo.PartyPlantId = voucherVM.PartyPlantId;
                    }
                    else if (voucherVM.PartyType == PartyType.Bank.ToString())
                    {
                        voucherDetailTo.BankMasterId = voucherVM.OtherBankMasterId;
                    }
                    voucherDetailTo.DrAmount = voucherVM.Amount;

                    #endregion To
                }
                else if (financing.TransactionType == TransactionType.InvestmentTaken.ToString())
                {
                    #region From

                    if (voucherVM.PartyType == PartyType.Customer.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Customer Id not found!");

                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Customer.ToString();
                    }
                    else if (voucherVM.PartyType == PartyType.Vendor.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Vendor Id not found!");
                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Vendor.ToString();
                    }
                    else if (voucherVM.PartyType == PartyType.Director.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Director Id not found!");

                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Director.ToString();
                    }
                    else if (voucherVM.PartyType == PartyType.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherBankMasterId))
                            throw new CustomException("Other Bank Id not found!");
                        voucherDetailFrom.BankMasterId = voucherVM.OtherBankMasterId;
                    }

                    var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                    if (string.IsNullOrEmpty(gl.LiabilityGLId))
                        throw new CustomException("This Transaction Type GL not Found!");

                    if (string.IsNullOrEmpty(gl.LiabilityActivityId))
                        throw new CustomException("This Transaction Type Activity not Found!");

                    investmentDetail.GLGeneralInfoId = gl.LiabilityGLId;
                    investmentDetail.BudgetMasterId = gl.LiabilityBudgetMasterId;
                    investmentDetail.ActivityId = gl.LiabilityActivityId;

                    voucherDetailFrom.GLGeneralInfoId = gl.LiabilityGLId;
                    voucherDetailFrom.BudgetMasterId = gl.LiabilityBudgetMasterId;
                    voucherDetailFrom.ActivityId = gl.LiabilityActivityId;

                    #endregion From

                    #region To

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _bankMasterRepository.Find(financing.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
                        if (null == bankMaster.ActivityId)
                            throw new CustomException("Activity data not found!");
                        voucherDetailTo.BankMasterId = bankMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = bankMaster.ActivityId;

                        voucherDetailTo.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        voucherDetailTo.CashMasterId = voucherVM.CashMasterId;
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash is not configured!");
                        voucherDetailTo.TrnNature = TransactionNature.ToCash.ToString();
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);

                        if (null== cashMaster)
                        throw new CustomException("Cash data not found!");
                        if (null == cashMaster.ActivityId)
                            throw new CustomException(" Cash Activity  not found!");
                        voucherDetailTo.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = cashMaster.ActivityId;
                    }
                    voucherDetailTo.DrAmount = voucherVM.Amount;
                    voucherDetailTo.FinancingDetailId = investmentDetail.Id;
                    
                    #endregion To
                }

                var currentVoucherDetailId = 1;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);

                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency From
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailFrom.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount
                });

                // INSERT INTO VoucherDetailCurrency To
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailTo.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount
                });
                // INSRT INTO GLTransactionDetail TABLE From
                if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                    {
                        BankMasterId = voucherDetailFrom.BankMasterId,
                        CashMasterId = voucherDetailFrom.CashMasterId,
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount,
                        SourceType = voucherDetailFrom.PaymentSource
                    });
                }
                // INSRT INTO GLTransactionDetail TABLE To
                if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                    {
                        BankMasterId = voucherDetailTo.BankMasterId,
                        CashMasterId = voucherDetailTo.CashMasterId,
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount,
                        SourceType = voucherDetailTo.PaymentSource
                    });
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public Dictionary<string, object> GetById(string id)
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

        public string InsertInvestmentSetOff(VoucherViewModel voucherVM)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;


                var financinWriteOff = new FinancingWriteOff
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    BankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FinancingId = voucherVM.FinancingId,
                    FinancingTypeId = voucherVM.FinancingTypeId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType,
                    CurrencyId = voucherVM.CurrencyId,
                    Amount = voucherVM.Amount,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType.ToString(),
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    IsPark = voucherVM.IsPark,
                    TransactionType = voucherVM.TransactionType
                };
                var financing = _financingService.FindFinancing(voucherVM.FinancingId);
                if (voucherVM.Amount > 0)
                {
                    _financingService.InsertFinancingWriteOff(financinWriteOff);
                    // INSERT INTO Financing TABLE
                    financing.WrittenOffAmount += voucherVM.Amount;
                    _financingService.UpdateFinancing(financing);

                }
                // INSERT INTO Voucher

                var voucher = _voucherService.InsertVoucher(voucherVM);
                financinWriteOff.FinancingNo = voucher.VoucherNo;
                // Set to Financing
                financinWriteOff.VoucherId = voucher.Id;

                // INSERT INTO FinancingDetail
                var financingDetailWriteOff = new FinancingDetailWriteOff
                {
                    Amount = voucherVM.Amount,
                    FinancingWriteOffId = financinWriteOff.Id,
                    FinancingId = financinWriteOff.FinancingId,
                    FinancingDetailId = voucherVM.FinancingDetailId,
                    WrittenOffAmount = voucherVM.Amount,
                    BankMasterId = voucherVM.OtherBankMasterId,
                    CashMasterId = voucherVM.OtherCashMasterId
                };
                // Investment from side Voucher detail row.
                var voucherDetailFrom = new VoucherDetail
                {
                    PartyType = financing.PartyType,
                    PaymentSource = financing.PaymentSource
                };

                // Investment to side Voucher detail row.
                var voucherDetailTo = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };

                var voucherExpenses = new VoucherDetail
                {
                    PartyType = PartyType.GL.ToString(),
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                };

                var exchangeloss = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                var voucherDetailLoanInterestPayable = new VoucherDetail
                {
                    PaymentSource = financing.PaymentSource
                };
                var voucherDetailLoanInterestCashExp = new VoucherDetail
                {
                    PaymentSource = financing.PaymentSource
                };
                var exchangeGain = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                //Update Financing Detail
                var financingDetail = _financingService.FindFinancingDetail(voucherVM.FinancingDetailId);
                financingDetail.WrittenOffAmount += voucherVM.Amount;
                
                if (voucherVM.Amount > 0)
                {
                    _financingService.UpdateFinancingDetail(financingDetail);
                }

                if (financing.TransactionType == TransactionType.InvestmentGiven.ToString())
                {
                    #region To

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);

                        voucherDetailTo.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailTo.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailTo.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailTo.ActivityId = bankMaster["ActivityId"].ToString();

                        //voucherDetailFrom.BankMasterId = financingDetailWriteOff.BankMasterId;
                        voucherDetailTo.TrnNature = TransactionNature.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _accountsCommonService.GetCashMaster(voucherVM.CashMasterId);

                        voucherDetailTo.CashMasterId = cashMaster["Id"].ToString();
                        voucherDetailTo.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailTo.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailTo.ActivityId = cashMaster["ActivityId"].ToString();
                        voucherDetailTo.TrnNature = TransactionNature.Bank.ToString();

                    }
                    else
                        throw new CustomException("Payment Source not found!");
                    // Set amount in Voucher detail in Credit side.
                    voucherDetailTo.DrAmount = voucherVM.Amount + voucherVM.ExpenseAmount;

                    #endregion To

                    #region From

                    voucherDetailFrom.GLGeneralInfoId = financingDetail.GLGeneralInfoId;
                    voucherDetailFrom.BudgetMasterId = financingDetail.BudgetMasterId;
                    voucherDetailFrom.ActivityId = financingDetail.ActivityId;

                    financingDetailWriteOff.GLGeneralInfoId = financingDetail.GLGeneralInfoId;
                    financingDetailWriteOff.BudgetMasterId = financingDetail.BudgetMasterId;
                    financingDetailWriteOff.ActivityId = financingDetail.ActivityId;

                    if (voucherVM.PartyType == PartyType.Vendor.ToString() || voucherVM.PartyType == PartyType.Customer.ToString() || voucherVM.PartyType == PartyType.Director.ToString())
                    {
                        voucherDetailFrom.PartyId = voucherVM.PartyId;
                        voucherDetailFrom.PartyPlantId = voucherVM.PartyPlantId;
                    }
                    else if (voucherVM.PartyType == PartyType.Bank.ToString())
                    {
                        voucherDetailFrom.BankMasterId = financingDetail.BankMasterId;
                    }
                    voucherDetailFrom.CrAmount = voucherVM.Amount;
                    _financingService.InsertFinancingWriteOffDetail(financinWriteOff, financingDetailWriteOff, 1);
                    voucherDetailFrom.FinancingDetailWriteOffId = financingDetailWriteOff.Id;

                    #endregion From



                }
                else if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    #region From

                    if (voucherVM.PartyType == PartyType.Vendor.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Vendor Id not found!");

                        voucherDetailFrom.DrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Vendor.ToString();
                    }
                    if (voucherVM.PartyType == PartyType.Party.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Vendor Id not found!");

                        voucherDetailFrom.DrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = "Party";
                    }
                    if (voucherVM.PartyType == PartyType.Customer.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Customer Id not found!");

                        voucherDetailFrom.DrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Customer.ToString();
                    }
                    else if (voucherVM.PartyType == PartyType.Director.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Director Id not found!");

                        voucherDetailFrom.DrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Director.ToString();
                    }
                    else if (voucherVM.PartyType == PartyType.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherBankMasterId))
                            throw new CustomException("Other Bank Id not found!");
                        voucherDetailFrom.BankMasterId = voucherVM.OtherBankMasterId;
                        voucherDetailFrom.DrAmount = voucherVM.Amount;
                    }


                    if (string.IsNullOrEmpty(gl.LiabilityGLId))
                        throw new CustomException("This Transaction Type GL not Found!");

                    financingDetailWriteOff.GLGeneralInfoId = gl.LiabilityGLId;
                    financingDetailWriteOff.BudgetMasterId = gl.LiabilityBudgetMasterId;
                    financingDetailWriteOff.ActivityId = gl.LiabilityActivityId;

                    voucherDetailFrom.GLGeneralInfoId = financingDetail.GLGeneralInfoId;
                    voucherDetailFrom.BudgetMasterId = financingDetail.BudgetMasterId;
                    voucherDetailFrom.ActivityId = financingDetail.ActivityId;
                    if (voucherVM.Amount > 0)
                    {
                        _financingService.InsertFinancingWriteOffDetail(financinWriteOff, financingDetailWriteOff, 1);
                        voucherDetailFrom.FinancingDetailWriteOffId = financingDetailWriteOff.Id;
                    }
                    #endregion From

                    #region To

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);

                        voucherDetailTo.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailTo.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailTo.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailTo.ActivityId = bankMaster["ActivityId"].ToString();

                        voucherDetailTo.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _accountsCommonService.GetCashMaster(voucherVM.CashMasterId);

                        voucherDetailTo.CashMasterId = cashMaster["Id"].ToString();
                        voucherDetailTo.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailTo.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailTo.ActivityId = cashMaster["ActivityId"].ToString();

                        voucherDetailTo.CashMasterId = voucherVM.CashMasterId;
                        voucherDetailTo.TrnNature = TransactionNature.ToCash.ToString();
                    }
                    voucherDetailTo.CrAmount = voucherVM.Amount + voucherVM.ExpenseAmount + voucherVM.InterestPaymentAmount + voucherVM.InterestCashAmount;
                    //voucherDetailTo.FinancingDetailWriteOffId = financingDetailWriteOff.Id;

                    #endregion To
                }
                var currentVoucherDetailId = 1;
                if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    //********************VoucherDetail From******************************
                    if (voucherVM.Amount > 0)
                    {
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                        totalAmountDr += voucherDetailFrom.DrAmount;
                    }
                    //********************VoucherDetail To******************************
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);
                    totalAmountCr += voucherDetailTo.CrAmount;


                    var financingSubsequentTransaction = new FinancingSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        FinancingId = voucherVM.FinancingId,
                        SetOffFinancingId = voucherVM.FinancingId,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = voucherVM.PartyType,
                        CurrencyId = voucherVM.CurrencyId,
                        Amount = voucherVM.Amount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        TransactionType = LoanTransactionType.LoanPayment.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = voucherVM.SourceType.ToString(),
                        IsPark = voucherVM.IsPark,
                        Id = "SL" + GetSubsequentInvestmentPK()

                    };
                    AuditService.AddedLog(financingSubsequentTransaction);
                    _loanInterestPayableRepository.Insert(financingSubsequentTransaction);

                    financingSubsequentTransaction.VoucherDetailId = voucherDetailFrom.Id;
                    financingSubsequentTransaction.VoucherId = voucher.Id;


                }
                if (financing.TransactionType == TransactionType.InvestmentGiven.ToString())
                {
                    //********************VoucherDetail From******************************

                    _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                    totalAmountCr += voucherDetailFrom.CrAmount;
                    //********************VoucherDetail To******************************
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);
                    totalAmountDr += voucherDetailTo.DrAmount;
                }



                if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    if (voucherVM.Amount > 0)
                    {
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailFrom.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = Math.Round((voucherVM.ToCurrencyRate * voucherDetailFrom.DrAmount), 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountDr += Math.Round((voucherVM.ToCurrencyRate * voucherDetailFrom.DrAmount), 2, MidpointRounding.AwayFromZero);
                    }

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount), 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountCr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount), 2, MidpointRounding.AwayFromZero);
                    

                }
                if (financing.TransactionType == TransactionType.InvestmentGiven.ToString())
                {
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailFrom.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailFrom.CrAmount), 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountCr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailFrom.CrAmount), 2, MidpointRounding.AwayFromZero);
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = Math.Round((voucherVM.ToCurrencyRate * voucherDetailTo.DrAmount), 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountDr += Math.Round((voucherVM.ToCurrencyRate * voucherDetailTo.DrAmount), 2, MidpointRounding.AwayFromZero);
                }

                //***********************Exchange Loss*************************************
                if (!string.IsNullOrEmpty(voucherVM.ExchangeType) && voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                {
                    var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);

                    exchangeloss.GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString();
                    exchangeloss.BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString();
                    exchangeloss.ActivityId = lossGL["CompanyCurrencyActivityId"].ToString();
                    exchangeloss.CurrencyId = voucher.CurrencyId;
                    exchangeloss.DocDate = voucher.DocDate;
                    exchangeloss.DocRefNo = voucher.DocRefNo;
                    exchangeloss.Narration = voucher.Narration;
                    exchangeloss.PartyType = voucherVM.ExchangeType;
                    exchangeloss.DrAmount = 0;
                    exchangeloss.CrAmount = 0;

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, exchangeloss, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(exchangeloss, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = exchangeloss.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(exchangeloss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.ExchangeAmount,
                    });
                    totalCurrencyAmountDr += voucherVM.ExchangeAmount;

                }
                //***********************Exchange Gain*************************************
                if (!string.IsNullOrEmpty(voucherVM.ExchangeType) && voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                {
                    var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                    exchangeGain.GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString();
                    exchangeGain.BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString();
                    exchangeGain.ActivityId = gainGL["CompanyCurrencyActivityId"].ToString();
                    exchangeGain.CurrencyId = voucher.CurrencyId;
                    exchangeGain.DocDate = voucher.DocDate;
                    exchangeGain.DocRefNo = voucher.DocRefNo;
                    exchangeGain.Narration = voucher.Narration;
                    exchangeGain.DrAmount = 0;
                    exchangeGain.CrAmount = 0;

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, exchangeGain, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(exchangeGain, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = exchangeGain.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(exchangeGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.ExchangeAmount
                    });
                    totalCurrencyAmountCr += voucherVM.ExchangeAmount;
                }
                //***********************Income *****************************************
                if (!string.IsNullOrEmpty(voucherVM.GLGeneralInfoId) && financing.TransactionType == TransactionType.InvestmentGiven.ToString())
                {
                    voucherExpenses.CrAmount = voucherVM.ExpenseAmount;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherExpenses, currentVoucherDetailId);
                    totalAmountCr += voucherVM.ExpenseAmount;
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherExpenses, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherExpenses.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherExpenses.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount), 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountCr += Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount), 2, MidpointRounding.AwayFromZero);

                }
                //***********************Expenses *****************************************
                if (!string.IsNullOrEmpty(voucherVM.GLGeneralInfoId) && financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    voucherExpenses.DrAmount = voucherVM.ExpenseAmount;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherExpenses, currentVoucherDetailId);
                    totalAmountDr += voucherVM.ExpenseAmount;
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherExpenses, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherExpenses.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherExpenses.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount), 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountDr += Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount), 2, MidpointRounding.AwayFromZero);

                }
                //*********************GLGeneralInfo Dr**********************************
                if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    if (voucherVM.Amount > 0)
                    {
                        if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId))
                        {
                            var bankMasterFrom = _accountsCommonService.GetBankMaster(voucherDetailFrom.BankMasterId);
                            _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailFrom.BankMasterId,
                                CashMasterId = voucherDetailFrom.CashMasterId,
                                DrAmount = bankMasterFrom["CurrencyId"].ToString() == voucher.CurrencyId ? voucherDetailFrom.DrAmount : voucherVM.CompanyCurrencyRate * voucherDetailFrom.DrAmount,
                                SourceType = voucherDetailFrom.PaymentSource
                            });
                        }
                        else
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailFrom.BankMasterId,
                                CashMasterId = voucherDetailFrom.CashMasterId,
                                DrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailFrom.DrAmount), 2, MidpointRounding.AwayFromZero),
                                SourceType = voucherDetailFrom.PaymentSource
                            });
                        }

                    }
                }
                //*********************GLGeneralInfo Cr**********************************
                if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                {
                    if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId))
                    {
                        var bankMasterTo = _accountsCommonService.GetBankMaster(voucherDetailTo.BankMasterId);

                        _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                        {
                            BankMasterId = voucherDetailTo.BankMasterId,
                            CashMasterId = voucherDetailTo.CashMasterId,
                            CrAmount = bankMasterTo["CurrencyId"].ToString() == voucher.CurrencyId ? voucherDetailTo.CrAmount : voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount,
                            SourceType = voucherDetailTo.PaymentSource
                        });
                    }
                    else
                    {
                        _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                        {
                            BankMasterId = voucherDetailTo.BankMasterId,
                            CashMasterId = voucherDetailTo.CashMasterId,
                            CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount), 2, MidpointRounding.AwayFromZero),
                            SourceType = voucherDetailTo.PaymentSource
                        });
                    }

                }
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (totalCurrencyAmountDr != totalCurrencyAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}