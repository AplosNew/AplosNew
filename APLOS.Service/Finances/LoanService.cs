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
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Data;

namespace Library.Service.Finances
{
    public class LoanService : ILoanService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly IVoucherService _voucherService;
        private readonly IFinancingTypeGLService _financingTypeGLService;
        private readonly IRepositoryAsync<BankMaster> _bankMasterRepository;
        private readonly IRepositoryAsync<CashMaster> _cashMasterRepository;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly IRepositoryAsync<CompanyPartyGL> _companyPartyGLRepository;
        private readonly IRepositoryAsync<FinancingSubsequentTransaction> _loanInterestPayableRepository;
        private readonly IFinancingService _financingService;
        private readonly IExchangeGainLossService _exchangeGainLossService;
        private readonly IPKGeneratorService _pkGeneratorService;

        public LoanService(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , ICompanyFiscalYearService companyFiscalYearService
            , ICompanyTaxYearService companyTaxYearService
            , IVoucherService voucherService
            , IFinancingTypeGLService financingTypeGLService
            , IRepositoryAsync<BankMaster> bankMasterRepository
            , IRepositoryAsync<CashMaster> cashMasterRepository
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , IRepositoryAsync<CompanyPartyGL> companyPartyGLRepository
            , IRepositoryAsync<FinancingSubsequentTransaction> loanInterestPayableRepository
            , IFinancingService financingService
            , IExchangeGainLossService exchangeGainLossService
            , IPKGeneratorService pkGeneratorService
            )
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _companyFiscalYearService = companyFiscalYearService;
            _companyTaxYearService = companyTaxYearService;
            _voucherService = voucherService;
            _financingTypeGLService = financingTypeGLService;
            _bankMasterRepository = bankMasterRepository;
            _cashMasterRepository = cashMasterRepository;
            _companyPartyRepository = companyPartyRepository;
            _companyPartyGLRepository = companyPartyGLRepository;
            _financingService = financingService;
            _exchangeGainLossService = exchangeGainLossService;
            _loanInterestPayableRepository = loanInterestPayableRepository;
        }

        #endregion Constructor

        public string InsertLoan(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
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
                var financing = _financingService.InsertFinancing(new Financing
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    FinancingTypeId = voucherVM.FinancingTypeId,
                    BankMasterId = voucherVM.BankMasterId,
                    OtherBankMasterId = voucherVM.OtherBankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    EmployeeId = voucherVM.EmployeeId,
                    PartyId = voucherVM.PartyId,
                    PartyType = voucherVM.PartyType,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType,
                    PaymentSource = voucherVM.PaymentSource,
                    Amount = voucherVM.Amount,
                    LifeOfYear = voucherVM.LifeOfYear,
                    NoOfInstallmentPerYear = voucherVM.NoOfInstallmentPerYear,
                    TotalNoOfInstallment = voucherVM.TotalNoOfInstallment,
                    ProfitRate = voucherVM.ProfitRate,
                    ProfitAmount = voucherVM.ProfitAmount,
                    RepaymentStartDate = voucherVM.RepaymentStartDate,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    IsPark = voucherVM.IsPark,
                    PartyPlantId = voucherVM.PartyPlantId,
                    TransactionType = voucherVM.TransactionType,
                    IsSchedule = voucherVM.IsSchedule
                });

                // INSERT INTO Voucher
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
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    TransactionType = LoanTransactionType.Loan.ToString(),
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType.ToString(),
                    IsPark = voucherVM.IsPark,
                    Id = "SL" + GetLoanInterestPayablePK(),
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
                var voucherDetailLoanPayment = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                _financingService.InsertFinancingDetail(financing, investmentDetail);

                if (financing.TransactionType == TransactionType.LoanGiven.ToString())
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
                            throw new CustomException("Activity not found!");
                        investmentDetail.BankMasterId = bankMaster.Id;
                        voucherDetailFrom.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailFrom.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailFrom.ActivityId = bankMaster.ActivityId;

                        voucherDetailFrom.BankMasterId = investmentDetail.BankMasterId;
                        voucherDetailFrom.TrnNature = TransactionNature.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _cashMasterRepository.Find(financing.CashMasterId);
                        if (null == cashMaster)
                            throw new CustomException("Cash data not found!");
                        if (null == cashMaster.ActivityId)
                            throw new CustomException("Activity not found!");
                        investmentDetail.CashMasterId = cashMaster.Id;
                        //investmentDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailFrom.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailFrom.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailFrom.ActivityId = cashMaster.ActivityId;
                        voucherDetailFrom.CashMasterId = investmentDetail.CashMasterId;
                        voucherDetailFrom.TrnNature = TransactionNature.Bank.ToString();

                    }
                    else
                        throw new CustomException("Payment Source not found!");
                    // Set amount in Voucher detail in Credit side.
                    voucherDetailFrom.CrAmount = investmentDetail.Amount;

                    #endregion From

                    #region To

                    var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                    if (string.IsNullOrEmpty(gl.AssetGLId))
                        throw new CustomException("This Transaction Type GL not Found!");
                    if (string.IsNullOrEmpty(gl.AssetActivityId))
                        throw new CustomException("This Transaction Type Activity not Found!");
                    voucherDetailTo.GLGeneralInfoId = gl.AssetGLId;
                    voucherDetailTo.BudgetMasterId = gl.AssetBudgetMasterId;
                    voucherDetailTo.ActivityId = gl.AssetActivityId;

                    investmentDetail.GLGeneralInfoId = gl.AssetGLId;
                    investmentDetail.BudgetMasterId = gl.AssetBudgetMasterId;
                    investmentDetail.ActivityId = gl.AssetActivityId;
                    voucherDetailTo.FinancingDetailId = investmentDetail.Id;

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
                else if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    #region From

                    if (voucherVM.PartyType == PartyType.Vendor.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Vendor Id not found!");

                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Vendor.ToString();
                    }
                    if (voucherVM.PartyType == PartyType.Customer.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Customer Id not found!");

                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Customer.ToString();
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
                        voucherDetailFrom.CrAmount = voucherVM.Amount;

                    }

                    var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                    if (string.IsNullOrEmpty(gl.LiabilityGLId))
                        throw new CustomException("This Transaction Type GL not Found!");
                    if (string.IsNullOrEmpty(gl.LiabilityActivityId))
                        throw new CustomException("This Transaction Type Activity not Found!");
                    investmentDetail.GLGeneralInfoId = gl.LiabilityGLId;
                    investmentDetail.BudgetMasterId = gl.LiabilityBudgetMasterId;
                    investmentDetail.ActivityId = gl.LiabilityActivityId;
                    voucherDetailFrom.FinancingDetailId = investmentDetail.Id;

                    #endregion From

                    #region To

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.IsPayment == true)
                    {
                        if (string.IsNullOrEmpty(financing.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _bankMasterRepository.Find(financing.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
                        if (null == bankMaster.ActivityId)
                            throw new CustomException("Activity not found!");
                        voucherDetailTo.BankMasterId = bankMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = bankMaster.ActivityId;

                        voucherDetailTo.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.IsPayment == true)
                    {
                        if (string.IsNullOrEmpty(financing.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _cashMasterRepository.Find(financing.CashMasterId);
                        if (null == cashMaster)
                            throw new CustomException("Bank data not found!");
                        if (null == cashMaster.ActivityId)
                            throw new CustomException("Activity not found!");
                        voucherDetailTo.CashMasterId = cashMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = cashMaster.ActivityId;

                        voucherDetailTo.CashMasterId = voucherVM.CashMasterId;
                        voucherDetailTo.TrnNature = TransactionNature.ToCash.ToString();
                    }
                    if (voucherVM.IsLoanSetOff && voucherVM.IsPayment == true)
                    {
                        if (existingLoanList != null)
                        {
                        voucherDetailTo.DrAmount = voucherVM.Amount - existingLoanList.Sum(r=>r.LoanSetOffAmount);
                        }

                    }
                    else
                        voucherDetailTo.DrAmount = voucherVM.Amount;

                    voucherDetailFrom.GLGeneralInfoId = investmentDetail.GLGeneralInfoId;
                    voucherDetailFrom.BudgetMasterId = investmentDetail.BudgetMasterId;
                    voucherDetailFrom.ActivityId = investmentDetail.ActivityId;

                    #endregion To
                }


                var currentVoucherDetailId = 1;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailFrom.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount
                });
                if (financing.TransactionType == TransactionType.LoanTaken.ToString() && voucherVM.IsPayment == true)
                {
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount
                    });
                    if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                    {
                        if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId))
                        {
                            var bankMasterTo = _bankMasterRepository.Find(voucherDetailTo.BankMasterId);
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = bankMasterTo.CurrencyId == voucher.CurrencyId ? voucherVM.Amount : voucherVM.CompanyCurrencyRate * voucherVM.Amount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                        else
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount =  voucherVM.CompanyCurrencyRate * voucherVM.Amount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                       
                    }
                }
                if (financing.TransactionType == TransactionType.LoanGiven.ToString())
                {
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount
                    });
                    if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                    {
                        if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId))
                        {
                            var bankMasterTo = _bankMasterRepository.Find(voucherDetailTo.BankMasterId);

                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = bankMasterTo.CurrencyId == voucher.CurrencyId ? voucherVM.Amount : voucherVM.CompanyCurrencyRate * voucherVM.Amount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                        else
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                       
                    }
                }


                if (voucherVM.IsLoanSetOff)
                {
                    var currentDetailId = 0;
                    foreach (var item in existingLoanList)
                    {
                        var financinWriteOff = new FinancingWriteOff
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            FinancingId = item.FinancingId,
                            FinancingTypeId = item.FinancingTypeId,
                            PartyId = item.PartyId,
                            PartyPlantId = item.PartyPlantId,
                            PartyType = item.PartyType,
                            CurrencyId = item.CurrencyId,
                            Amount = item.LoanSetOffAmount,
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
                            IsPark = voucherVM.IsPark
                        };
                        financinWriteOff.FinancingNo = voucher.VoucherNo;
                        // Set to Financing
                        financinWriteOff.VoucherId = voucher.Id;
                        var FinancingSubsequentPayment = new FinancingSubsequentTransaction
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            FinancingId = financing.Id,
                            SetOffFinancingId = item.FinancingId,
                            PartyId = item.PartyId,
                            PartyPlantId = item.PartyPlantId,
                            PartyType = item.PartyType,
                            CurrencyId = item.CurrencyId,
                            Amount = item.LoanSetOffAmount,
                            VoucherDate = voucherVM.VoucherDate,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            TransactionType = LoanTransactionType.LoanPayment.ToString(),
                            Narration = voucherVM.Narration,
                            SourceType = voucherVM.SourceType.ToString(),
                            IsPark = voucherVM.IsPark,
                            Id = "SL" + GetLoanInterestPayablePK(),
                            VoucherId = voucher.Id
                        };
                        AuditService.AddedLog(FinancingSubsequentPayment);
                        _loanInterestPayableRepository.Insert(FinancingSubsequentPayment);

                        var financingExisting = _financingService.FindFinancing(item.FinancingId);
                        if (item.LoanSetOffAmount > 0)
                        {
                            AuditService.AddedLog(financinWriteOff);
                            _financingService.InsertFinancingWriteOff(financinWriteOff);
                            // INSERT INTO Financing TABLE
                            financingExisting.WrittenOffAmount += item.LoanSetOffAmount;
                            //financing.IsWrittenOff = financing.Amount == financing.WrittenOffAmount;
                            //if (financingExisting.Amount < financingExisting.WrittenOffAmount)
                            //    throw new CustomException("Settlement amount can not greater than loan amount.");
                            _financingService.UpdateFinancing(financingExisting);

                        }
                        var financingDetailWriteOff = new FinancingDetailWriteOff
                        {
                            Amount = item.LoanSetOffAmount,
                            FinancingWriteOffId = financinWriteOff.Id,
                            FinancingId = financinWriteOff.FinancingId,
                            FinancingDetailId = item.FinancingDetailId,
                            WrittenOffAmount = item.LoanSetOffAmount,
                            BankMasterId = item.OtherBankMasterId,
                            CashMasterId = item.OtherCashMasterId
                        };

                        var financingDetail = _financingService.FindFinancingDetail(item.FinancingDetailId);
                        financingDetail.WrittenOffAmount += item.LoanSetOffAmount;
                        ////financingDetail.IsWrittenOff = financingDetail.Amount == financingDetail.WrittenOffAmount;
                        //if (financingDetail.Amount < financingDetail.WrittenOffAmount)
                        //    throw new CustomException("Settlement amount can not greater than loan amount.");
                        if (item.LoanSetOffAmount > 0)
                        {
                            _financingService.UpdateFinancingDetail(financingDetail);
                        }


                        financingDetailWriteOff.GLGeneralInfoId = financingDetail.GLGeneralInfoId;
                        financingDetailWriteOff.BudgetMasterId = financingDetail.BudgetMasterId;
                        financingDetailWriteOff.ActivityId = financingDetail.ActivityId;

                        var voucherDetailExistingLoanSsetoff = new VoucherDetail
                        {
                            PartyType = voucherVM.PartyType,
                            GLGeneralInfoId = financingDetail.GLGeneralInfoId,
                            BudgetMasterId = financingDetail.BudgetMasterId,
                            ActivityId = financingDetail.ActivityId,
                            DrAmount = financingDetailWriteOff.Amount
                        };


                        currentDetailId++;
                        _financingService.InsertFinancingWriteOffDetail(financinWriteOff, financingDetailWriteOff, currentDetailId);
                        voucherDetailExistingLoanSsetoff.FinancingDetailWriteOffId = financingDetailWriteOff.Id;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailExistingLoanSsetoff, currentVoucherDetailId);
                        FinancingSubsequentPayment.VoucherDetailId = voucherDetailExistingLoanSsetoff.Id;
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailExistingLoanSsetoff, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailExistingLoanSsetoff.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = item.ToCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailExistingLoanSsetoff.CurrencyId, companyCurrencyId, item.ToCurrencyRate),
                            DrAmount = item.ToCurrencyRate * item.LoanSetOffAmount
                        });
                    }


                }

                // INSRT INTO GLTransactionDetail TABLE From
                if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId))
                    {
                        var bankMasterFrom = _bankMasterRepository.Find(voucherDetailFrom.BankMasterId);

                        _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                        {
                            BankMasterId = voucherDetailFrom.BankMasterId,
                            CashMasterId = voucherDetailFrom.CashMasterId,
                            CrAmount = bankMasterFrom.CurrencyId == voucher.CurrencyId ? voucherVM.Amount : voucherVM.CompanyCurrencyRate * voucherVM.Amount,
                            SourceType = voucherDetailFrom.PaymentSource
                        });
                    }
                    else
                    {
                        _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                        {
                            BankMasterId = voucherDetailFrom.BankMasterId,
                            CashMasterId = voucherDetailFrom.CashMasterId,
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount,
                            SourceType = voucherDetailFrom.PaymentSource
                        });
                    }
                   

                }
                // INSRT INTO GLTransactionDetail TABLE To

                if (voucherVM.IsSchedule)
                {
                    foreach (var item in financingScheduleVMList)
                    {
                        var financingSchedule = new FinancingSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance
                        };
                        _financingService.InsertFinancingSchedule(financing, financingSchedule);
                    }
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

        public string InsertLoanWriteOff(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

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
                    IsPark = voucherVM.IsPark
                };
                var financing = _financingService.FindFinancing(voucherVM.FinancingId);
                if (voucherVM.Amount > 0)
                {
                    _financingService.InsertFinancingWriteOff(financinWriteOff);
                    // INSERT INTO Financing TABLE
                    financing.WrittenOffAmount += voucherVM.Amount;
                    //financing.IsWrittenOff = financing.Amount+ financing.AdditionalLoanAmount == financing.WrittenOffAmount;
                    //if (financing.Amount + financing.AdditionalLoanAmount < financing.WrittenOffAmount)
                    //    throw new CustomException("Settlement amount can not greater than loan amount.");
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
                //financingDetail.IsWrittenOff = financingDetail.Amount + financingDetail.AdditionalLoanAmount == financingDetail.WrittenOffAmount;
                //if (financingDetail.Amount + financingDetail.AdditionalLoanAmount < financingDetail.WrittenOffAmount)
                //    throw new CustomException("Settlement amount can not greater than loan amount.");
                if (voucherVM.Amount > 0)
                {
                    _financingService.UpdateFinancingDetail(financingDetail);
                }

                if (financing.TransactionType == TransactionType.LoanGiven.ToString())
                {
                    #region To

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
                        voucherDetailTo.BankMasterId = bankMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = bankMaster.ActivityId;

                        //voucherDetailFrom.BankMasterId = financingDetailWriteOff.BankMasterId;
                        voucherDetailTo.TrnNature = TransactionNature.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                        if (null == cashMaster)
                            throw new CustomException("Cash data not found!");
                        voucherDetailTo.CashMasterId = cashMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = cashMaster.ActivityId;
                        // voucherDetailFrom.CashMasterId = financingDetailWriteOff.CashMasterId;
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

                    //voucherDetailFrom.GLGeneralInfoId = gl.LiabilityGLId;
                    //voucherDetailFrom.BudgetMasterId = gl.LiabilityBudgetMasterId;
                    //voucherDetailFrom.ActivityId = gl.LiabilityActivityId;
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
                        var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
                        voucherDetailTo.BankMasterId = bankMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = bankMaster.ActivityId;

                        voucherDetailTo.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                        if (null == cashMaster)
                            throw new CustomException("Cash data not found!");
                        voucherDetailTo.CashMasterId = cashMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = cashMaster.ActivityId;

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
                        SetOffFinancingId= voucherVM.FinancingId,
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
                        Id = "SL" + GetLoanInterestPayablePK()
                    };
                    AuditService.AddedLog(financingSubsequentTransaction);
                    _loanInterestPayableRepository.Insert(financingSubsequentTransaction);


                    //currentVoucherDetailId++;
                    //voucherDetailLoanInterestPayable.DrAmount = voucherVM.InterestPaymentAmount;
                    //if (string.IsNullOrEmpty(gl.ExpensesPayableGLId))
                    //    throw new CustomException("This Expenses Payable GL not Found!");

                    //voucherDetailLoanInterestPayable.GLGeneralInfoId = gl.ExpensesPayableGLId;
                    //voucherDetailLoanInterestPayable.BudgetMasterId = gl.ExpensesPayableBudgetMasterId;
                    //voucherDetailLoanInterestPayable.ActivityId = gl.ExpensesPayableActivityId;
                    //_voucherService.InsertVoucherDetail(voucher, voucherDetailLoanInterestPayable, currentVoucherDetailId);
                    //totalAmountDr += voucherDetailLoanInterestPayable.DrAmount;
                    financingSubsequentTransaction.VoucherDetailId = voucherDetailFrom.Id;
                    financingSubsequentTransaction.VoucherId = voucher.Id;


                }
                if (financing.TransactionType == TransactionType.LoanGiven.ToString())
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
                            DrAmount = voucherVM.ToCurrencyRate * voucherDetailFrom.DrAmount
                        });
                        totalCurrencyAmountDr += voucherVM.ToCurrencyRate * voucherDetailFrom.DrAmount;
                    }

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount
                    });
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount;
                    //if (voucherVM.InterestPaymentAmount > 0)
                    //{
                    //    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoanInterestPayable, new VoucherDetailCurrency
                    //    {
                    //        ParallelCurrencyId = companyCurrencyId,
                    //        FromCurrencyId = voucherDetailLoanInterestPayable.CurrencyId,
                    //        ToCurrencyId = companyCurrencyId,
                    //        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    //        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoanInterestPayable.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    //        DrAmount = voucherVM.ToCurrencyRate * voucherDetailLoanInterestPayable.DrAmount
                    //    });
                    //    totalCurrencyAmountDr += voucherVM.ToCurrencyRate * voucherDetailLoanInterestPayable.DrAmount;

                    //}
                    //if (voucherVM.InterestCashAmount > 0)
                    //{
                    //    currentVoucherDetailId++;
                    //    _voucherService.InsertVoucherDetail(voucher, voucherDetailLoanInterestCashExp, currentVoucherDetailId);
                    //    totalAmountDr += voucherDetailFrom.DrAmount;

                    //    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoanInterestCashExp, new VoucherDetailCurrency
                    //    {
                    //        ParallelCurrencyId = companyCurrencyId,
                    //        FromCurrencyId = voucherDetailLoanInterestCashExp.CurrencyId,
                    //        ToCurrencyId = companyCurrencyId,
                    //        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    //        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoanInterestCashExp.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    //        DrAmount = voucherVM.ToCurrencyRate * voucherDetailLoanInterestCashExp.DrAmount
                    //    });
                    //    totalCurrencyAmountDr += voucherVM.ToCurrencyRate * voucherDetailLoanInterestCashExp.DrAmount;

                    //}

                }
                if (financing.TransactionType == TransactionType.LoanGiven.ToString())
                {
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailFrom.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailFrom.CrAmount
                    });
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailFrom.CrAmount;
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.ToCurrencyRate * voucherDetailTo.DrAmount
                    });
                    totalCurrencyAmountDr += voucherVM.ToCurrencyRate * voucherDetailTo.DrAmount;
                }

                //***********************Exchange Loss*************************************
                if (!string.IsNullOrEmpty(voucherVM.ExchangeType) && voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                {
                    var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);

                    exchangeloss.GLGeneralInfoId = lossGL.CompanyCurrencyGLId;
                    exchangeloss.BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId;
                    exchangeloss.ActivityId = lossGL.CompanyCurrencyActivityId;
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
                    var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                    exchangeGain.GLGeneralInfoId = gainGL.CompanyCurrencyGLId;
                    exchangeGain.BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId;
                    exchangeGain.ActivityId = gainGL.CompanyCurrencyActivityId;
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
                if (!string.IsNullOrEmpty(voucherVM.GLGeneralInfoId) && financing.TransactionType == TransactionType.LoanGiven.ToString())
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
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount
                    });
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount;

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
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount
                    });
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount;

                }
                //*********************GLGeneralInfo Dr**********************************
                if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    if (voucherVM.Amount > 0)
                    {
                        if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId))
                        {
                            var bankMasterFrom = _bankMasterRepository.Find(voucherDetailFrom.BankMasterId);
                            _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailFrom.BankMasterId,
                                CashMasterId = voucherDetailFrom.CashMasterId,
                                DrAmount = bankMasterFrom.CurrencyId == voucher.CurrencyId ? voucherDetailFrom.DrAmount : voucherVM.CompanyCurrencyRate * voucherDetailFrom.DrAmount,
                                SourceType = voucherDetailFrom.PaymentSource
                            });
                        }
                        else
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailFrom.BankMasterId,
                                CashMasterId = voucherDetailFrom.CashMasterId,
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailFrom.DrAmount,
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
                        var bankMasterTo = _bankMasterRepository.Find(voucherDetailTo.BankMasterId);

                        _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                        {
                            BankMasterId = voucherDetailTo.BankMasterId,
                            CashMasterId = voucherDetailTo.CashMasterId,
                            CrAmount = bankMasterTo.CurrencyId == voucher.CurrencyId ? voucherDetailTo.CrAmount : voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount,
                            SourceType = voucherDetailTo.PaymentSource
                        });
                    }
                    else
                    {
                        _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                        {
                            BankMasterId = voucherDetailTo.BankMasterId,
                            CashMasterId = voucherDetailTo.CashMasterId,
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount,
                            SourceType = voucherDetailTo.PaymentSource
                        });
                    }
                   
                }
                //***********************Loan Schedule***********************************
                if (voucherVM.IsSchedule)
                {
                    foreach (var item in financingScheduleVMList)
                    {
                        var financingSchedule = new FinancingSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance
                        };
                        _financingService.InsertFinancingSchedule(financing, financingSchedule);
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
        private string GetLoanInterestPayablePK()
        {
            return _pkGeneratorService.GetAutoNumber("FinancingSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertLoanInterestPayable(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;

                var loanInterestPayable = new FinancingSubsequentTransaction
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FinancingId = voucherVM.FinancingId,
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
                    IsPark = voucherVM.IsPark,
                    Id = "SL" + GetLoanInterestPayablePK()
                };
                AuditService.AddedLog(loanInterestPayable);
                if (voucherVM.SourceType == "AdditionalLoanPayable")
                    loanInterestPayable.TransactionType = LoanTransactionType.AdditionalLoanPayable.ToString();
                if (voucherVM.SourceType == SourceType.LoanInterestPayable.ToString())
                    loanInterestPayable.TransactionType = LoanTransactionType.InterestPayable.ToString();
                if (voucherVM.SourceType == "OtherExpensesPayable")
                    loanInterestPayable.TransactionType = LoanTransactionType.OtherExpensesPayable.ToString();

                _loanInterestPayableRepository.Insert(loanInterestPayable);

                // INSERT INTO Financing TABLE
                var financing = _financingService.FindFinancing(voucherVM.FinancingId);

                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set to Financing
                loanInterestPayable.VoucherId = voucher.Id;

                // INSERT INTO FinancingDetail

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
                };

                var exchangeloss = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                var exchangeGain = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                //Update Financing Detail
                var financingDetail = _financingService.FindFinancingDetail(voucherVM.FinancingDetailId);


                if (financing.TransactionType == TransactionType.LoanGiven.ToString())
                {
                    #region To

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
                        voucherDetailTo.BankMasterId = bankMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = bankMaster.ActivityId;

                        //voucherDetailFrom.BankMasterId = financingDetailWriteOff.BankMasterId;
                        voucherDetailTo.TrnNature = TransactionNature.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                        if (null == cashMaster)
                            throw new CustomException("Cash data not found!");
                        voucherDetailTo.CashMasterId = cashMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = cashMaster.ActivityId;
                        // voucherDetailFrom.CashMasterId = financingDetailWriteOff.CashMasterId;
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

                    #endregion From



                }
                else if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    #region From

                    if (voucherVM.PartyType == PartyType.Vendor.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Vendor Id not found!");

                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Vendor.ToString();
                    }
                    if (voucherVM.PartyType == PartyType.Party.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Vendor Id not found!");

                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = "Party";
                    }
                    if (voucherVM.PartyType == PartyType.Customer.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.PartyId))
                            throw new CustomException("Customer Id not found!");

                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                        voucherDetailFrom.PartyId = financing.PartyId;
                        voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                        voucherDetailFrom.TrnNature = TransactionNature.Customer.ToString();
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
                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                    }

                    var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                    if (string.IsNullOrEmpty(gl.ExpensesPayableGLId))
                        throw new CustomException("Loan Type Interest Payable GL not Found!");
                    if (string.IsNullOrEmpty(gl.ExpensesActivityId))
                        throw new CustomException("Loan Type Expenses  GL not Found!");


                    voucherDetailTo.GLGeneralInfoId = gl.ExpensesPayableGLId;
                    voucherDetailTo.BudgetMasterId = gl.ExpensesPayableBudgetMasterId;
                    voucherDetailTo.ActivityId = gl.ExpensesPayableActivityId;

                    voucherExpenses.GLGeneralInfoId = gl.ExpensesGLId;
                    voucherExpenses.BudgetMasterId = gl.ExpensesBudgetMasterId;
                    voucherExpenses.ActivityId = gl.ExpensesActivityId;
                    if (voucherVM.SourceType == LoanTransactionType.OtherExpensesPayable.ToString())
                    {
                        if (string.IsNullOrEmpty(gl.ChargesPayableGLId))
                            throw new CustomException("Loan Type Charges Payable GL not Found!");
                        if (string.IsNullOrEmpty(gl.ChargesPayableBudgetMasterId))
                            throw new CustomException("Loan Type Charges Budget   not Found!");
                        if (string.IsNullOrEmpty(gl.ChargesPayableActivityId))
                            throw new CustomException("Loan Type Charges Activity   not Found!");

                        voucherDetailTo.GLGeneralInfoId = gl.ChargesPayableGLId;
                        voucherDetailTo.BudgetMasterId = gl.ChargesPayableBudgetMasterId;
                        voucherDetailTo.ActivityId = gl.ChargesPayableActivityId;

                        voucherExpenses.GLGeneralInfoId = voucherVM.GLGeneralInfoId;
                        voucherExpenses.BudgetMasterId = voucherVM.BudgetMasterId;
                        voucherExpenses.ActivityId = voucherVM.ActivityId;
                    }
                    if (voucherVM.SourceType == LoanTransactionType.AdditionalLoanPayable.ToString())
                    {

                        voucherDetailFrom.GLGeneralInfoId = financingDetail.GLGeneralInfoId;
                        voucherDetailFrom.BudgetMasterId = financingDetail.BudgetMasterId;
                        voucherDetailFrom.ActivityId = financingDetail.ActivityId;
                    }

                    #endregion From

                    #region To

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.SourceType == LoanTransactionType.AdditionalLoanPayable.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
                        voucherDetailTo.BankMasterId = bankMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = bankMaster.ActivityId;
                        voucherDetailTo.PaymentSource = voucherVM.PaymentSource;

                        voucherDetailTo.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.SourceType == LoanTransactionType.AdditionalLoanPayable.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                        if (null == cashMaster)
                            throw new CustomException("Cash data not found!");
                        voucherDetailTo.CashMasterId = cashMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = cashMaster.ActivityId;

                        voucherDetailTo.CashMasterId = voucherVM.CashMasterId;
                        voucherDetailTo.TrnNature = TransactionNature.ToCash.ToString();
                        voucherDetailTo.PaymentSource = voucherVM.PaymentSource;
                    }
                    voucherDetailTo.CrAmount = voucherVM.Amount;
                    //voucherDetailTo.FinancingDetailWriteOffId = financingDetailWriteOff.Id;

                    #endregion To
                }
                var currentVoucherDetailId = 1;
                if (financing.TransactionType == TransactionType.LoanTaken.ToString() && voucherVM.SourceType == LoanTransactionType.LoanInterestPayable.ToString()
                    || voucherVM.SourceType == LoanTransactionType.OtherExpensesPayable.ToString())
                {
                    //********************VoucherDetail From******************************
                    //_voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                    //totalAmountDr += voucherDetailFrom.DrAmount;
                    //********************VoucherDetail To******************************
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);
                    totalAmountCr += voucherDetailTo.CrAmount;
                    loanInterestPayable.VoucherDetailId = voucherDetailTo.Id;
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount
                    });
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount;
                }
                if (financing.TransactionType == TransactionType.LoanTaken.ToString() && voucherVM.SourceType == LoanTransactionType.AdditionalLoanPayable.ToString())
                {
                    voucherDetailTo.DrAmount = voucherVM.Amount;
                    voucherDetailTo.CrAmount = 0;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);
                    totalAmountDr += voucherDetailTo.DrAmount;

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount
                    });
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount;

                    if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                    {
                        if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId))
                        {
                            var bankMasterTo = _bankMasterRepository.Find(voucherDetailTo.BankMasterId);
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = bankMasterTo.CurrencyId == voucher.CurrencyId ? voucherDetailTo.DrAmount : voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                        else
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                       
                    }
                }
                //***********************Exchange Loss*************************************
                if (!string.IsNullOrEmpty(voucherVM.ExchangeType) && voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                {
                    var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);

                    exchangeloss.GLGeneralInfoId = lossGL.CompanyCurrencyGLId;
                    exchangeloss.BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId;
                    exchangeloss.ActivityId = lossGL.CompanyCurrencyActivityId;
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
                        DrAmount = voucherVM.ExchangeAmount
                    });
                    totalCurrencyAmountDr += voucherVM.ExchangeAmount;

                }
                //***********************Exchange Gain*************************************
                if (!string.IsNullOrEmpty(voucherVM.ExchangeType) && voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                {
                    var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                    exchangeGain.GLGeneralInfoId = gainGL.CompanyCurrencyGLId;
                    exchangeGain.BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId;
                    exchangeGain.ActivityId = gainGL.CompanyCurrencyActivityId;
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
                if (!string.IsNullOrEmpty(voucherVM.GLGeneralInfoId) && financing.TransactionType == TransactionType.LoanGiven.ToString())
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
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount
                    });
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount;

                }
                //***********************Expenses *****************************************
                if (!string.IsNullOrEmpty(voucherExpenses.GLGeneralInfoId) && financing.TransactionType == TransactionType.LoanTaken.ToString()
                    && voucherVM.SourceType == LoanTransactionType.LoanInterestPayable.ToString() || voucherVM.SourceType == LoanTransactionType.OtherExpensesPayable.ToString())
                {
                    voucherExpenses.DrAmount = voucherVM.Amount;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherExpenses, currentVoucherDetailId);
                    totalAmountDr += voucherVM.Amount;
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherExpenses, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherExpenses.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherExpenses.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount
                    });
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherVM.Amount;

                }
                if (financing.TransactionType == TransactionType.LoanTaken.ToString() && voucherVM.SourceType == LoanTransactionType.AdditionalLoanPayable.ToString())
                {
                    voucherDetailFrom.CrAmount = voucherVM.Amount;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                    totalAmountCr += voucherVM.Amount;
                    loanInterestPayable.VoucherDetailId = voucherDetailFrom.Id;
                    financing.AdditionalLoanAmount += voucherDetailFrom.CrAmount;
                    financingDetail.AdditionalLoanAmount += voucherDetailFrom.CrAmount;
                    _financingService.UpdateFinancing(financing);
                    _financingService.UpdateFinancingDetail(financingDetail);
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailFrom.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount
                    });
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherVM.Amount;

                }

                if (voucherVM.IsSchedule)
                {
                    foreach (var item in financingScheduleVMList)
                    {
                        var financingSchedule = new FinancingSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance
                        };
                        _financingService.InsertFinancingSchedule(financing, financingSchedule);
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

        public string InsertLoanInterestPayableReverse(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;

                var loanInterestPayable = new FinancingSubsequentTransaction
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FinancingId = voucherVM.FinancingId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType,
                    CurrencyId = voucherVM.CurrencyId,
                    Amount = voucherVM.Amount,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    TransactionType = voucherVM.SettlementType,
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType.ToString(),
                    IsPark = voucherVM.IsPark,
                    Id = "SL" + GetLoanInterestPayablePK()
                };
                AuditService.AddedLog(loanInterestPayable);
                _loanInterestPayableRepository.Insert(loanInterestPayable);

                // INSERT INTO Financing TABLE
                var financing = _financingService.FindFinancing(voucherVM.FinancingId);

                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set to Financing
                loanInterestPayable.VoucherId = voucher.Id;

                // INSERT INTO FinancingDetail

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
                };

                var exchangeloss = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                var exchangeGain = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                //Update Financing Detail
                var financingDetail = _financingService.FindFinancingDetail(voucherVM.FinancingDetailId);


                if (financing.TransactionType == TransactionType.LoanGiven.ToString())
                {
                    #region To

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                        var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
                        voucherDetailTo.BankMasterId = bankMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = bankMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = bankMaster.ActivityId;

                        //voucherDetailFrom.BankMasterId = financingDetailWriteOff.BankMasterId;
                        voucherDetailTo.TrnNature = TransactionNature.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                        if (null == cashMaster)
                            throw new CustomException("Cash data not found!");
                        voucherDetailTo.CashMasterId = cashMaster.Id;
                        voucherDetailTo.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailTo.ActivityId = cashMaster.ActivityId;
                        // voucherDetailFrom.CashMasterId = financingDetailWriteOff.CashMasterId;
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

                    #endregion From



                }
                else if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    #region From


                    var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);


                    if (voucherVM.SettlementType == LoanTransactionType.ChargesPayableReverse.ToString())
                    {
                        if (string.IsNullOrEmpty(gl.ChargesPayableGLId))
                            throw new CustomException("Loan Type Charges Payable GL not Found!");
                        if (string.IsNullOrEmpty(gl.ChargesPayableBudgetMasterId))
                            throw new CustomException("Loan Type Charges  Budget not Found!");
                        if (string.IsNullOrEmpty(gl.ChargesPayableActivityId))
                            throw new CustomException("Loan Type Charges  Activity not Found!");
                        voucherDetailTo.GLGeneralInfoId = gl.ChargesPayableGLId;
                        voucherDetailTo.BudgetMasterId = gl.ChargesPayableBudgetMasterId;
                        voucherDetailTo.ActivityId = gl.ChargesPayableActivityId;

                        voucherExpenses.GLGeneralInfoId = voucherVM.GLGeneralInfoId;
                        voucherExpenses.BudgetMasterId = voucherVM.BudgetMasterId;
                        voucherExpenses.ActivityId = voucherVM.ActivityId;
                    }

                    if (voucherVM.SettlementType == LoanTransactionType.InterestPayableReverse.ToString())
                    {
                        if (string.IsNullOrEmpty(gl.ExpensesPayableGLId))
                            throw new CustomException("Loan Type Interest Payable GL not Found!");
                        if (string.IsNullOrEmpty(gl.ExpensesPayableBudgetMasterId))
                            throw new CustomException("Loan Type Interest Payable Budget not Found!");
                        if (string.IsNullOrEmpty(gl.ExpensesPayableActivityId))
                            throw new CustomException("Loan Type Interest Payable Activity not Found!");
                        voucherDetailTo.GLGeneralInfoId = gl.ExpensesPayableGLId;
                        voucherDetailTo.BudgetMasterId = gl.ExpensesPayableBudgetMasterId;
                        voucherDetailTo.ActivityId = gl.ExpensesPayableActivityId;

                        if (string.IsNullOrEmpty(gl.ExpensesGLId))
                            throw new CustomException("Loan Type Expenses GL not Found!");
                        if (string.IsNullOrEmpty(gl.ExpensesBudgetMasterId))
                            throw new CustomException("Loan Type Expenses  Budget not Found!");
                        if (string.IsNullOrEmpty(gl.ExpensesActivityId))
                            throw new CustomException("Loan Type Expenses  Activity not Found!");
                        voucherExpenses.GLGeneralInfoId = gl.ExpensesGLId;
                        voucherExpenses.BudgetMasterId = gl.ExpensesBudgetMasterId;
                        voucherExpenses.ActivityId = gl.ExpensesActivityId;
                    }


                    #endregion From
                    voucherDetailTo.DrAmount = voucherVM.Amount;
                }
                var currentVoucherDetailId = 1;
                if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {

                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);
                    totalAmountDr += voucherDetailTo.DrAmount;
                    loanInterestPayable.VoucherDetailId = voucherDetailTo.Id;

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount
                    });
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount;
                }
                //***********************Exchange Loss*************************************
                if (!string.IsNullOrEmpty(voucherVM.ExchangeType) && voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                {
                    var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);

                    exchangeloss.GLGeneralInfoId = lossGL.CompanyCurrencyGLId;
                    exchangeloss.BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId;
                    exchangeloss.ActivityId = lossGL.CompanyCurrencyActivityId;
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
                        DrAmount = voucherVM.ExchangeAmount
                    });
                    totalCurrencyAmountDr += voucherVM.ExchangeAmount;

                }
                //***********************Exchange Gain*************************************
                if (!string.IsNullOrEmpty(voucherVM.ExchangeType) && voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                {
                    var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                    exchangeGain.GLGeneralInfoId = gainGL.CompanyCurrencyGLId;
                    exchangeGain.BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId;
                    exchangeGain.ActivityId = gainGL.CompanyCurrencyActivityId;
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
                if (!string.IsNullOrEmpty(voucherVM.GLGeneralInfoId) && financing.TransactionType == TransactionType.LoanGiven.ToString())
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
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount
                    });
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherVM.ExpenseAmount;

                }
                //***********************Expenses *****************************************
                if (!string.IsNullOrEmpty(voucherExpenses.GLGeneralInfoId) && financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    voucherExpenses.CrAmount = voucherVM.Amount;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherExpenses, currentVoucherDetailId);
                    totalAmountCr += voucherVM.Amount;
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherExpenses, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherExpenses.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherExpenses.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherVM.Amount
                    });
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherVM.Amount;

                }

                if (voucherVM.IsSchedule)
                {
                    foreach (var item in financingScheduleVMList)
                    {
                        var financingSchedule = new FinancingSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance
                        };
                        _financingService.InsertFinancingSchedule(financing, financingSchedule);
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

        public string InsertLoanClose(IEnumerable<VoucherViewModel> existingLoanList)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var item in existingLoanList)
                {
                    var financing = _financingService.FindFinancing(item.FinancingId);
                    financing.IsWrittenOff = true;
                    _financingService.UpdateFinancing(financing);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return "Successfully Saved";
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