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
    }
}