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
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Library.Service.Extension.Accounts;
using Library.ViewModel.Invoices;
using Library.Model.Invoices;
using Library.Service.Invoices;
using Library.ViewModel.Banks;
using Library.Service.Banks;
using Library.Model.Accounts;
using Library.Model.Taxations;
using Library.Model.Systems;

namespace Library.Service.Finances
{
    public class LoanService : Service<Financing>, ILoanService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly IFinancingTypeGLService _financingTypeGLService;
        private readonly IRepositoryAsync<FinancingSubsequentTransaction> _loanInterestPayableRepository;
        IRepositoryAsync<Financing> _financingRepository;
        private readonly IFinancingService _financingService;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IBankChargeService _bankChargeService;
        private readonly IRepositoryAsync<AdditionalTax> _additionalTaxRepository;
        private readonly IRepositoryAsync<AdditionalTaxDetail> _additionalTaxDetailRepository;
        private readonly IRepositoryAsync<TaxCode> _taxCodeRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;

        public LoanService(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IVoucherService voucherService
            , IFinancingTypeGLService financingTypeGLService
            , IRepositoryAsync<FinancingSubsequentTransaction> loanInterestPayableRepository
            , IRepositoryAsync<Financing> financingRepository
            , IFinancingService financingService
            , IPKGeneratorService pkGeneratorService
            , IInvoiceTaxService invoiceTaxService
            , IBankChargeService bankChargeService
            , IRepositoryAsync<AdditionalTax> additionalTaxRepository
            , IRepositoryAsync<AdditionalTaxDetail> additionalTaxDetailRepository
            , IRepositoryAsync<TaxCode> taxCodeRepository
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            ) : base(financingRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _voucherService = voucherService;
            _financingTypeGLService = financingTypeGLService;
            _financingRepository = financingRepository;
            _financingService = financingService;
            _loanInterestPayableRepository = loanInterestPayableRepository;
            _invoiceTaxService = invoiceTaxService;
            _bankChargeService = bankChargeService;
            _additionalTaxRepository = additionalTaxRepository;
            _additionalTaxDetailRepository = additionalTaxDetailRepository;
            _taxCodeRepository = taxCodeRepository;
            _invoiceTaxRepository = invoiceTaxRepository;
        }

        #endregion Constructor

        public string InsertLoan(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList, IEnumerable<FinancingMasterOrderViewModel> financingMasterOrderlist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
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
                decimal totalbankCharges = 0;
                decimal totalbankChargesCompanyCurrencyAmount = 0;
                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    totalbankCharges = bankChargeDetailVMList.Sum(r => r.Amount);
                    totalbankChargesCompanyCurrencyAmount = bankChargeDetailVMList.Sum(r => r.CompanyCurrencyAmount);
                }

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
                    ExpectedCloseDate = voucherVM.ExpectedCloseDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType,
                    PaymentSource = voucherVM.PaymentSource,
                    Amount = voucherVM.Amount,
                    DownPaymentAmount = voucherVM.DownPaymentAmount,
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
                    DownPaymentAmount = voucherVM.DownPaymentAmount,
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
                var exchangeloss = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                var exchangeGain = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                var currentVoucherDetailId = 0;
                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var currentBankChargeDetailId = 0;
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        currentBankChargeDetailId++;
                        var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                        {
                            BankMasterId = financing.BankMasterId,
                            CashMasterId = financing.CashMasterId,
                            FinancingId = financing.Id,
                            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                            SourceType = financing.SourceType,
                            Narration = voucher.Narration,
                            Archive = financing.Archive,
                            Amount = bankChargeDetailVM.Amount,
                            AddedBy = financing.AddedBy,
                            AddedDate = financing.AddedDate,
                            AddedFromIP = financing.AddedFromIP
                        }, currentBankChargeDetailId);

                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(voucher.CompanyId, bankChargeDetailVM.FinancingTypeId);

                        // Insert Bank charges Debit
                        currentVoucherDetailId++;
                        var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankChargeId = bankCharge.Id,
                            DrAmount = bankCharge.Amount,
                            Narration = bankCharge.Narration,
                            GLGeneralInfoId = expenseGL.ExpensesGLId,
                            BudgetMasterId = expenseGL.ExpensesBudgetMasterId,
                            ActivityId = expenseGL.ExpensesActivityId
                        }, currentVoucherDetailId);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailChargeDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = bankChargeDetailVM.CompanyCurrencyAmount
                        });
                        totalAmountDr += voucherDetailChargeDr.DrAmount;
                        totalCurrencyAmountDr += bankChargeDetailVM.CompanyCurrencyAmount;
                    }
                }

                _financingService.InsertFinancingDetail(financing, investmentDetail);

                if (financing.TransactionType == TransactionType.LoanGiven.ToString())
                {
                    #region From

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.BankMasterId))
                            throw new CustomException("Bank Id not found!");
                       
                        var bankMaster =  _accountsCommonService.GetBankMaster(financing.BankMasterId);
                       
                        if (null == bankMaster["ActivityId"].ToString())
                            throw new CustomException("Activity not found!");
                        investmentDetail.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailFrom.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailFrom.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailFrom.ActivityId = bankMaster["ActivityId"].ToString();

                        voucherDetailFrom.BankMasterId = investmentDetail.BankMasterId;
                        voucherDetailFrom.TrnNature = TransactionNature.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(financing.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _accountsCommonService.GetCashMaster(financing.CashMasterId);
                        
                        if (null == cashMaster["ActivityId"].ToString())
                            throw new CustomException("Activity not found!");
                        investmentDetail.CashMasterId = cashMaster["Id"].ToString();
                        voucherDetailFrom.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailFrom.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailFrom.ActivityId = cashMaster["ActivityId"].ToString();
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
                        var bankMaster =  _accountsCommonService.GetBankMaster(financing.BankMasterId);
                        
                        if (null == bankMaster["ActivityId"].ToString())
                            throw new CustomException("Activity not found!");
                        voucherDetailTo.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailTo.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailTo.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailTo.ActivityId = bankMaster["ActivityId"].ToString();

                        voucherDetailTo.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.IsPayment == true)
                    {
                        if (string.IsNullOrEmpty(financing.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _accountsCommonService.GetCashMaster(financing.CashMasterId);
                        if (null == cashMaster)
                            throw new CustomException("Bank data not found!");
                        if (null == cashMaster["ActivityId"].ToString())
                            throw new CustomException("Activity not found!");
                        voucherDetailTo.CashMasterId = cashMaster["Id"].ToString();
                        voucherDetailTo.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailTo.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailTo.ActivityId = cashMaster["ActivityId"].ToString();

                        voucherDetailTo.CashMasterId = voucherVM.CashMasterId;
                        voucherDetailTo.TrnNature = TransactionNature.ToCash.ToString();
                    }
                    if (voucherVM.IsLoanSetOff && voucherVM.IsPayment == true)
                    {
                        if (existingLoanList != null)
                        {
                            if(voucherVM.CurrencyId!=existingLoanList.FirstOrDefault().CurrencyId)
                            {
                                voucherDetailTo.DrAmount = voucherVM.Amount - Math.Round((existingLoanList.Sum(r => r.LoanSetOffAmount/r.ToCurrencyRate)),2) - totalbankCharges;
                            }
                            else
                            {
                                voucherDetailTo.DrAmount = voucherVM.Amount - existingLoanList.Sum(r => r.LoanSetOffAmount) - totalbankCharges;
                            }
                        
                        }

                    }
                    else
                        voucherDetailTo.DrAmount = voucherVM.Amount- totalbankCharges;

                    voucherDetailFrom.GLGeneralInfoId = investmentDetail.GLGeneralInfoId;
                    voucherDetailFrom.BudgetMasterId = investmentDetail.BudgetMasterId;
                    voucherDetailFrom.ActivityId = investmentDetail.ActivityId;

                    #endregion To
                }


                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailFrom.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.Amount), 2)
                });
                totalAmountCr += voucherDetailFrom.CrAmount;
                totalCurrencyAmountCr += Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.Amount), 2);
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
                        DrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount), 2) 
                    });
                    totalAmountDr += voucherDetailTo.DrAmount;
                    totalCurrencyAmountDr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount), 2);
                if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                    {
                        if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId))
                        {
                            var bankMasterTo = _accountsCommonService.GetBankMaster(voucherDetailTo.BankMasterId);
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = bankMasterTo["CurrencyId"].ToString() == voucher.CurrencyId ? voucherVM.Amount - totalbankCharges : (voucherVM.CompanyCurrencyRate * voucherVM.Amount) - totalbankChargesCompanyCurrencyAmount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                        else
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.Amount), 2) - totalbankChargesCompanyCurrencyAmount,
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
                        DrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount), 2)
                    });
                    totalAmountDr += voucherDetailTo.DrAmount;
                    totalCurrencyAmountDr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount), 2);
                    if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                    {
                        if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId))
                        {
                            var bankMasterTo = _accountsCommonService.GetBankMaster(voucherDetailTo.BankMasterId);

                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = bankMasterTo["CurrencyId"].ToString() == voucher.CurrencyId ? voucherVM.Amount : Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.Amount), 2),
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                        else
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherVM.Amount), 2),
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }
                       
                    }
                }


                if (voucherVM.IsLoanSetOff)
                {
                    if((voucherVM.Amount!= existingLoanList.Sum(r => r.LoanSetOffAmount)) && voucherVM.BankMasterId==null && voucherVM.CurrencyId == existingLoanList.FirstOrDefault().CurrencyId)
                    {
                        throw new CustomException("Dr Cr Amount Not Match!");
                    }
                    if ((voucherVM.Amount != Math.Round((existingLoanList.Sum(r => r.LoanSetOffAmount / r.ToCurrencyRate)), 2)) && voucherVM.BankMasterId == null && voucherVM.CurrencyId != existingLoanList.FirstOrDefault().CurrencyId)
                    {
                        throw new CustomException("Dr Cr Amount Not Match!");
                    }

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

                        var ExistingLoanSetoffAmount= 0.0M;
                        var ExistingLoanSetoffCurrencyAmount = 0.0M;
                        if (voucherVM.CurrencyId != existingLoanList.FirstOrDefault().CurrencyId)
                        {
                            ExistingLoanSetoffAmount = Math.Round((financingDetailWriteOff.Amount / existingLoanList.FirstOrDefault().ToCurrencyRate), 2);
                            ExistingLoanSetoffCurrencyAmount = item.LoanSetOffAmount;
                        }
                        else
                        {
                            ExistingLoanSetoffAmount = financingDetailWriteOff.Amount;
                            ExistingLoanSetoffCurrencyAmount = Math.Round((item.ToCurrencyRate * item.LoanSetOffAmount), 2);
                        }

                        var voucherDetailExistingLoanSsetoff = new VoucherDetail
                        {
                            PartyType = voucherVM.PartyType,
                            GLGeneralInfoId = financingDetail.GLGeneralInfoId,
                            BudgetMasterId = financingDetail.BudgetMasterId,
                            ActivityId = financingDetail.ActivityId,
                            DrAmount = ExistingLoanSetoffAmount
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
                            DrAmount = ExistingLoanSetoffCurrencyAmount
                        });
                        totalAmountDr += ExistingLoanSetoffAmount;
                        totalCurrencyAmountDr += ExistingLoanSetoffCurrencyAmount;

                        //***********************Exchange Loss*************************************
                        if (!string.IsNullOrEmpty(item.ExchangeType) && item.ExchangeType == "ExchangeLoss" && item.ExchangeAmount > 0)
                        {
                            var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);

                            exchangeloss.GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString();
                            exchangeloss.BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString();
                            exchangeloss.ActivityId = lossGL["CompanyCurrencyActivityId"].ToString();
                            exchangeloss.CurrencyId = voucher.CurrencyId;
                            exchangeloss.DocDate = voucher.DocDate;
                            exchangeloss.DocRefNo = voucher.DocRefNo;
                            exchangeloss.Narration = voucher.Narration;
                            exchangeloss.PartyType = item.ExchangeType;
                            exchangeloss.DrAmount = 0;
                            exchangeloss.CrAmount = 0;

                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, exchangeloss, currentVoucherDetailId);
                            _voucherService.InsertVoucherDetailCompanyCurrency(exchangeloss, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = exchangeloss.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = item.ToCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(exchangeloss.CurrencyId, companyCurrencyId, item.ToCurrencyRate),
                                DrAmount = item.ExchangeAmount,
                            });
                            totalCurrencyAmountDr += item.ExchangeAmount;

                        }
                        //***********************Exchange Gain*************************************
                        if (!string.IsNullOrEmpty(item.ExchangeType) && item.ExchangeType == "ExchangeGain" && item.ExchangeAmount > 0)
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
                                ToCurrencyRate = item.ToCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(exchangeGain.CurrencyId, companyCurrencyId, item.ToCurrencyRate),
                                CrAmount = item.ExchangeAmount
                            });
                            totalCurrencyAmountCr += item.ExchangeAmount;
                        }

                    }

                }

                // INSRT INTO GLTransactionDetail TABLE From
                if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId))
                    {
                        var bankMasterFrom = _accountsCommonService.GetBankMaster(voucherDetailFrom.BankMasterId);

                        _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                        {
                            BankMasterId = voucherDetailFrom.BankMasterId,
                            CashMasterId = voucherDetailFrom.CashMasterId,
                            CrAmount = bankMasterFrom["CurrencyId"].ToString() == voucher.CurrencyId ? voucherVM.Amount : voucherVM.CompanyCurrencyRate * voucherVM.Amount,
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
                var currentfinancingMasterOrderId = 1;
                if (financingMasterOrderlist !=null)
                {
                    foreach (var item in financingMasterOrderlist)
                    {
                        var financingMasterOrder = new FinancingMasterOrder
                        {
                            MasterOrderId = item.MasterOrderId,
                            PartyId = item.PartyId,
                            FinancingId = financing.Id
                        };
                        _financingService.InsertFinancingMasterOrder(financing, financingMasterOrder, currentfinancingMasterOrderId);
                        currentfinancingMasterOrderId++;
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (totalCurrencyAmountCr != totalCurrencyAmountDr)
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
        private InvoiceTax InsertInvoiceTax(FinancingWriteOff invoicewriteoff, InvoiceTax invoiceTax, PKGenerator pKGenerator)
        {
            pKGenerator.MaxNumber++;
            invoiceTax.Id = DateTime.Now.Year + pKGenerator.MaxNumber.ToString();
            invoiceTax.FinancingId = invoicewriteoff.FinancingId;
            invoiceTax.TaxYearId = invoicewriteoff.TaxYearId;
            invoiceTax.TaxYearPeriodId = invoicewriteoff.TaxYearPeriodId;
            invoiceTax.VoucherId = invoicewriteoff.VoucherId;
            invoiceTax.PartyId = invoicewriteoff.PartyId;
            invoiceTax.PartyPlantId = invoicewriteoff.PartyPlantId;
            invoiceTax.SourceType = invoicewriteoff.SourceType;
            invoiceTax.Archive = invoicewriteoff.Archive;
            invoiceTax.AddedBy = invoicewriteoff.AddedBy;
            invoiceTax.AddedDate = invoicewriteoff.AddedDate;
            invoiceTax.AddedFromIP = invoicewriteoff.AddedFromIP;
            _invoiceTaxRepository.Insert(invoiceTax);
            return invoiceTax;
        }
        public string InsertLoanWriteOff(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList)
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
                var invoiceTax = new InvoiceTax
                {

                };
                var invoiceTaxDetail = new InvoiceTaxDetail
                {

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
                    else if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                    {
                        if (null != tdsVMList && tdsVMList.Count() > 0)
                        {
                            var tdstax = new AdditionalTax
                            {

                                TaxYearId = voucher.TaxYearId,
                                TaxYearPeriodId = voucher.TaxYearPeriodId,
                                //TaxAmount = tdsVMList.Sum(r => r.TaxAmount),
                                TaxAmount = financinWriteOff.Amount,
                                TaxAutoAmount = tdsVMList.Sum(r => r.TaxAutoAmount),
                                InventoryReceiveId = null,
                                InvoiceId = null,
                                FinancingWriteOffId = financinWriteOff.Id,
                                EmployeePayableId = null,
                                PartyId = financinWriteOff.PartyId,
                                PartyPlantId = financinWriteOff.PartyPlantId,
                                Id =  base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                VoucherId = voucher.Id
                            };
                            _additionalTaxRepository.Insert(tdstax);


                            var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                            int addtionalTaxDetailId = 0;
                            foreach (var invoiceTaxVM in tdsVMList)
                            {
                                var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                                if (null == taxCode)
                                    throw new CustomException("Tax code not found!");

                                var taxCodeGL = _accountsCommonService.GetTaxCodeGL(taxCode.Id); _accountsCommonService.GetTaxCodeGL(taxCode.Id);
                                if (null == taxCodeGL)
                                    throw new CustomException("Tax code GL not found!");
                               
                                voucherDetailTo.TrnNature = TransactionNature.ToGL.ToString();
                                addtionalTaxDetailId++;
                                var tdsDetail = new AdditionalTaxDetail
                                {
                                    GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                                    BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                                    ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                                    Amount = financinWriteOff.Amount,
                                    AdditionalTaxId = tdstax.Id,
                                    TaxCodeId = invoiceTaxVM.TaxCodeId,
                                    TaxCategoryId = taxCode.TaxCategoryId,
                                    AType = "Cr",
                                    Id = MakePK(tdstax.Id, addtionalTaxDetailId, 3),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _additionalTaxDetailRepository.Insert(tdsDetail);
                                 invoiceTax = new InvoiceTax
                                {
                                    TaxCodeId = invoiceTaxVM.TaxCodeId,
                                    TaxCategoryId = taxCode.TaxCategoryId,
                                    TaxAmount = financinWriteOff.Amount,
                                    TaxAutoAmount = 0,
                                    VoucherId = voucher.Id
                                };
                                InsertInvoiceTax(financinWriteOff, invoiceTax, invoiceTaxPk);

                                 invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    GLGeneralInfoId = tdsDetail.GLGeneralInfoId,
                                    BudgetMasterId = tdsDetail.BudgetMasterId,
                                    ActivityId = tdsDetail.ActivityId,
                                    Amount = tdsDetail.Amount,
                                    AType = "Cr"
                                };
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);
                                voucherDetailTo.GLGeneralInfoId = tdsDetail.GLGeneralInfoId;
                                voucherDetailTo.BudgetMasterId = tdsDetail.BudgetMasterId;
                                voucherDetailTo.ActivityId = tdsDetail.ActivityId;
                            }
                        }
                        
                        
                    }
                    voucherDetailTo.CrAmount = voucherVM.Amount + voucherVM.ExpenseAmount + voucherVM.InterestPaymentAmount + voucherVM.InterestCashAmount;
                    
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
                    if (tdsVMList.Count() > 0)
                    {
                        invoiceTax.VoucherDetailId = voucherDetailTo.Id;
                        voucherDetailTo.InvoiceTaxDetailId = invoiceTaxDetail.Id;
                    }


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
        public string InsertLoanWriteOffChangeBooksAmount(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
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
                        Id = "SL" + GetLoanInterestPayablePK()

                    };
                    AuditService.AddedLog(financingSubsequentTransaction);
                    _loanInterestPayableRepository.Insert(financingSubsequentTransaction);

                    
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
                        CrAmount = voucherVM.LoanSetOffAmount
                    });
                    totalCurrencyAmountCr += voucherVM.LoanSetOffAmount;
                    

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
                        CrAmount = voucherVM.LoanSetOffAmount
                    });
                    totalCurrencyAmountCr += voucherVM.LoanSetOffAmount;
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
                                DrAmount = bankMasterFrom["CurrencyId"].ToString() == voucher.CurrencyId ? voucherDetailFrom.DrAmount : voucherVM.LoanSetOffAmount,
                                SourceType = voucherDetailFrom.PaymentSource
                            });
                        }
                        else
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailFrom.BankMasterId,
                                CashMasterId = voucherDetailFrom.CashMasterId,
                                DrAmount = voucherVM.LoanSetOffAmount,
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
                            CrAmount = bankMasterTo["CurrencyId"].ToString() == voucher.CurrencyId ? voucherDetailTo.CrAmount : voucherVM.LoanSetOffAmount,
                            SourceType = voucherDetailTo.PaymentSource
                        });
                    }
                    else
                    {
                        _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                        {
                            BankMasterId = voucherDetailTo.BankMasterId,
                            CashMasterId = voucherDetailTo.CashMasterId,
                            CrAmount = voucherVM.LoanSetOffAmount,
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
        public string InsertLoanWriteOffLoanAddition(VoucherViewModel voucherVM, VoucherViewModel loanAdditionVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
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
                    Amount = loanAdditionVM.LoanSetOffAmount,
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
                    financing.WrittenOffAmount += loanAdditionVM.LoanSetOffAmount;
                    //financing.IsWrittenOff = financing.Amount+ financing.AdditionalLoanAmount == financing.WrittenOffAmount;
                    //if (financing.Amount + financing.AdditionalLoanAmount < financing.WrittenOffAmount)
                    //    throw new CustomException("Settlement amount can not greater than loan amount.");
                    _financingService.UpdateFinancing(financing);

                }
                // INSERT INTO Voucher

                var voucher = _voucherService.InsertVoucher(voucherVM);
                financinWriteOff.FinancingNo = voucher.VoucherNo;
                voucher.CurrencyId = loanAdditionVM.CurrencyId;
                // Set to Financing
                financinWriteOff.VoucherId = voucher.Id;

                // INSERT INTO FinancingDetail
                var financingDetailWriteOff = new FinancingDetailWriteOff
                {
                    Amount = loanAdditionVM.LoanSetOffAmount,
                    FinancingWriteOffId = financinWriteOff.Id,
                    FinancingId = financinWriteOff.FinancingId,
                    FinancingDetailId = voucherVM.FinancingDetailId,
                    WrittenOffAmount = loanAdditionVM.LoanSetOffAmount,
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
                    PartyType = loanAdditionVM.PartyType
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
                financingDetail.WrittenOffAmount += loanAdditionVM.LoanSetOffAmount;
                var financingDetailloanAddition = _financingService.FindFinancingDetail(loanAdditionVM.FinancingDetailId);
                var financingloanAddition = _financingService.FindFinancing(loanAdditionVM.FinancingId);

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

                   if (voucherVM.PaymentSource == PaymentSource.Loan.ToString())
                    {
                        if (string.IsNullOrEmpty(loanAdditionVM.FinancingId))
                            throw new CustomException("Loan not found!");

                        voucherDetailTo.GLGeneralInfoId = financingDetailloanAddition.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = financingDetailloanAddition.BudgetMasterId;
                        voucherDetailTo.ActivityId = financingDetailloanAddition.ActivityId;
                        voucherDetailTo.TrnNature = LoanTransactionType.AdditionalLoanPayable.ToString();

                        financingloanAddition.AdditionalLoanAmount += voucherVM.Amount;
                        financingDetailloanAddition.AdditionalLoanAmount += voucherVM.Amount;
                        
                        _financingService.UpdateFinancing(financingloanAddition);
                        _financingService.UpdateFinancingDetail(financingDetailloanAddition);
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
                        Amount = loanAdditionVM.LoanSetOffAmount,
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

                    financingSubsequentTransaction.VoucherDetailId = voucherDetailFrom.Id;
                    financingSubsequentTransaction.VoucherId = voucher.Id;

                    var financingSubsequentTransactionloanAddition = new FinancingSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        FinancingId = loanAdditionVM.FinancingId,
                        PartyId = loanAdditionVM.PartyId,
                        PartyPlantId = loanAdditionVM.PartyPlantId,
                        PartyType = loanAdditionVM.PartyType,
                        CurrencyId = loanAdditionVM.CurrencyId,
                        Amount = voucherVM.Amount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        TransactionType = LoanTransactionType.AdditionalLoanPayable.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = voucherVM.SourceType.ToString(),
                        IsPark = voucherVM.IsPark,
                        Id = "SL" + GetLoanInterestPayablePK()

                    };
                    AuditService.AddedLog(financingSubsequentTransactionloanAddition);
                    _loanInterestPayableRepository.Insert(financingSubsequentTransactionloanAddition);

                    financingSubsequentTransactionloanAddition.VoucherDetailId = voucherDetailTo.Id;
                    financingSubsequentTransactionloanAddition.VoucherId = voucher.Id;


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
                            DrAmount = loanAdditionVM.LoanSetOffAmount
                        });
                        totalCurrencyAmountDr += loanAdditionVM.LoanSetOffAmount;
                    }

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailTo.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = loanAdditionVM.Amount
                    });
                    totalCurrencyAmountCr += loanAdditionVM.Amount;
                    

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
        private string GetLoanSetOffGroupNoPK()
        {
            return _pkGeneratorService.GetAutoNumber("LoanSetOffGroupNo", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertMultiLoanWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> loanRepaymentlist)
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
                string _loanSetOffGroupNo = GetLoanSetOffGroupNoPK();
                foreach (var item in loanRepaymentlist)
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
                        CurrencyId = voucherVM.CurrencyId,
                        Amount = item.Amount,
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
                        TransactionType = voucherVM.TransactionType,
                        LoanSetOffGroupNo= _loanSetOffGroupNo
                    };
                    var financing = _financingService.FindFinancing(item.FinancingId);
                    if (item.Amount > 0)
                    {
                        _financingService.InsertFinancingWriteOff(financinWriteOff);
                        // INSERT INTO Financing TABLE
                        financing.WrittenOffAmount += item.Amount;
                        _financingService.UpdateFinancing(financing);

                    }
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    financinWriteOff.FinancingNo = voucher.VoucherNo;
                    financinWriteOff.VoucherId = voucher.Id;


                    var financingDetailWriteOff = new FinancingDetailWriteOff
                    {
                        Amount = item.Amount,
                        FinancingWriteOffId = financinWriteOff.Id,
                        FinancingId = financinWriteOff.FinancingId,
                        FinancingDetailId = item.FinancingDetailId,
                        WrittenOffAmount = item.Amount,
                        BankMasterId = item.OtherBankMasterId,
                        CashMasterId = item.OtherCashMasterId
                    };
                    // Investment from side Voucher detail row.
                    var voucherDetailFrom = new VoucherDetail
                    {
                        PartyType = financing.PartyType,
                        PaymentSource = financing.PaymentSource,
                        LoanSetOffGroupNo= _loanSetOffGroupNo
                    };

                    // Investment to side Voucher detail row.
                    var voucherDetailTo = new VoucherDetail
                    {
                        PartyType = item.PartyType,
                        LoanSetOffGroupNo = _loanSetOffGroupNo

                    };

                   

                    var exchangeloss = new VoucherDetail
                    {
                        PartyType = voucherVM.PartyType,
                        LoanSetOffGroupNo = _loanSetOffGroupNo
                    };
                    var voucherDetailLoanInterestPayable = new VoucherDetail
                    {
                        PaymentSource = financing.PaymentSource,
                        LoanSetOffGroupNo = _loanSetOffGroupNo
                    };
                    var voucherDetailLoanInterestCashExp = new VoucherDetail
                    {
                        PaymentSource = financing.PaymentSource,
                        LoanSetOffGroupNo = _loanSetOffGroupNo
                    };
                    var exchangeGain = new VoucherDetail
                    {
                        PartyType = voucherVM.PartyType,
                        LoanSetOffGroupNo = _loanSetOffGroupNo
                    };
                    var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                    //Update Financing Detail
                    var financingDetail = _financingService.FindFinancingDetail(item.FinancingDetailId);
                    financingDetail.WrittenOffAmount += item.Amount;

                    if (item.Amount > 0)
                    {
                        _financingService.UpdateFinancingDetail(financingDetail);
                    }

                    if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                    {
                        #region From

                        if (item.PartyType == PartyType.Vendor.ToString())
                        {
                            if (string.IsNullOrEmpty(item.PartyId))
                                throw new CustomException("Vendor Id not found!");

                            voucherDetailFrom.DrAmount = item.Amount;
                            voucherDetailFrom.PartyId = financing.PartyId;
                            voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                            voucherDetailFrom.TrnNature = TransactionNature.Vendor.ToString();
                        }
                        if (item.PartyType == PartyType.Party.ToString())
                        {
                            if (string.IsNullOrEmpty(item.PartyId))
                                throw new CustomException("Vendor Id not found!");

                            voucherDetailFrom.DrAmount = item.Amount;
                            voucherDetailFrom.PartyId = financing.PartyId;
                            voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                            voucherDetailFrom.TrnNature = "Party";
                        }
                        if (item.PartyType == PartyType.Customer.ToString())
                        {
                            if (string.IsNullOrEmpty(financing.PartyId))
                                throw new CustomException("Customer Id not found!");

                            voucherDetailFrom.DrAmount = item.Amount;
                            voucherDetailFrom.PartyId = financing.PartyId;
                            voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                            voucherDetailFrom.TrnNature = TransactionNature.Customer.ToString();
                        }
                        else if (item.PartyType == PartyType.Director.ToString())
                        {
                            if (string.IsNullOrEmpty(financing.PartyId))
                                throw new CustomException("Director Id not found!");

                            voucherDetailFrom.DrAmount = item.Amount;
                            voucherDetailFrom.PartyId = financing.PartyId;
                            voucherDetailFrom.PartyPlantId = financing.PartyPlantId;
                            voucherDetailFrom.TrnNature = TransactionNature.Director.ToString();
                        }
                        else if (item.PartyType == PartyType.Bank.ToString())
                        {
                            if (string.IsNullOrEmpty(item.OtherBankMasterId))
                                throw new CustomException("Other Bank Id not found!");
                            voucherDetailFrom.BankMasterId = item.OtherBankMasterId;
                            voucherDetailFrom.DrAmount = item.Amount;
                        }


                        if (string.IsNullOrEmpty(gl.LiabilityGLId))
                            throw new CustomException("This Transaction Type GL not Found!");

                        financingDetailWriteOff.GLGeneralInfoId = gl.LiabilityGLId;
                        financingDetailWriteOff.BudgetMasterId = gl.LiabilityBudgetMasterId;
                        financingDetailWriteOff.ActivityId = gl.LiabilityActivityId;

                        voucherDetailFrom.GLGeneralInfoId = financingDetail.GLGeneralInfoId;
                        voucherDetailFrom.BudgetMasterId = financingDetail.BudgetMasterId;
                        voucherDetailFrom.ActivityId = financingDetail.ActivityId;
                        if (item.Amount > 0)
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
                        voucherDetailTo.CrAmount = item.Amount + item.ExpenseAmount + item.InterestPaymentAmount + item.InterestCashAmount;
                        //voucherDetailTo.FinancingDetailWriteOffId = financingDetailWriteOff.Id;

                        #endregion To
                    }
                    var currentVoucherDetailId = 1;
                    if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                    {
                        //********************VoucherDetail From******************************
                        if (item.Amount > 0)
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
                            FinancingId = item.FinancingId,
                            SetOffFinancingId = item.FinancingId,
                            PartyId = item.PartyId,
                            PartyPlantId = item.PartyPlantId,
                            PartyType = item.PartyType,
                            CurrencyId = voucherVM.CurrencyId,
                            Amount = item.Amount,
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

                        financingSubsequentTransaction.VoucherDetailId = voucherDetailFrom.Id;
                        financingSubsequentTransaction.VoucherId = voucher.Id;


                    }


                    if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                    {
                        if (item.Amount > 0)
                        {
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailFrom.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = item.ToCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = item.ToCurrencyRate * voucherDetailFrom.DrAmount
                            });
                            totalCurrencyAmountDr += item.ToCurrencyRate * voucherDetailFrom.DrAmount;
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
                       

                    }

                    //***********************Exchange Loss*************************************
                    if (!string.IsNullOrEmpty(item.ExchangeType) && item.ExchangeType == "ExchangeLoss" && item.ExchangeAmount > 0)
                    {
                        var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);

                        exchangeloss.GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString();
                        exchangeloss.BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString();
                        exchangeloss.ActivityId = lossGL["CompanyCurrencyActivityId"].ToString();
                        exchangeloss.CurrencyId = voucher.CurrencyId;
                        exchangeloss.DocDate = voucher.DocDate;
                        exchangeloss.DocRefNo = voucher.DocRefNo;
                        exchangeloss.Narration = voucher.Narration;
                        exchangeloss.PartyType = item.ExchangeType;
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
                            DrAmount = item.ExchangeAmount,
                        });
                        totalCurrencyAmountDr += item.ExchangeAmount;

                    }
                    //***********************Exchange Gain*************************************
                    if (!string.IsNullOrEmpty(item.ExchangeType) && item.ExchangeType == "ExchangeGain" && item.ExchangeAmount > 0)
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
                            CrAmount = item.ExchangeAmount
                        });
                        totalCurrencyAmountCr += item.ExchangeAmount;
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
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTo.CrAmount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }

                    }
                    //***********************Loan Schedule***********************************

                }


                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (totalCurrencyAmountDr != totalCurrencyAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return "Hello";
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
        public string InsertLoanInterestPayable(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList)
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

                decimal totalVoucherDetailTaxAmount = 0;
                decimal totalcreditableDrAmount = 0,  totalwithholdCrAmount = 0, taxDrAmount = 0;
                decimal totalBaseCurrencyCrAmount = 0;
                decimal totalBaseCurrencyDrAmount = 0;
                decimal totalAPBaseCurrencyDrAmount = 0;
                var creditablegl = false;
                var withholdgl = false;
                //var merge = false;

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
                    DownPaymentAmount = voucherVM.DownPaymentAmount,
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
                if (voucherVM.SourceType == "LoanTax")
                    loanInterestPayable.TransactionType = LoanTransactionType.LoanTax.ToString();

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
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
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
                        if (null == cashMaster)
                            throw new CustomException("Cash data not found!");
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
                        voucherDetailTo.BankMasterId = voucherVM.OtherBankMasterId;
                        voucherDetailFrom.CrAmount = voucherVM.Amount;
                    }

                    var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                    if (string.IsNullOrEmpty(gl.ExpensesPayableGLId))
                        throw new CustomException("Loan Type Interest Payable GL not Found!");
                    if (string.IsNullOrEmpty(gl.ExpensesActivityId))
                        throw new CustomException("Loan Type Expenses  GL not Found!");


                    voucherDetailTo.GLGeneralInfoId = financingDetail.GLGeneralInfoId;
                    voucherDetailTo.BudgetMasterId = financingDetail.BudgetMasterId;
                    voucherDetailTo.ActivityId = financingDetail.ActivityId;

                    voucherExpenses.GLGeneralInfoId = gl.ExpensesGLId;
                    voucherExpenses.BudgetMasterId = gl.ExpensesBudgetMasterId;
                    voucherExpenses.ActivityId = gl.ExpensesActivityId;
                    if (voucherVM.SourceType == LoanTransactionType.OtherExpensesPayable.ToString())
                    {
                        //if (string.IsNullOrEmpty(gl.ChargesPayableGLId))
                        //    throw new CustomException("Loan Type Charges Payable GL not Found!");
                        //if (string.IsNullOrEmpty(gl.ChargesPayableBudgetMasterId))
                        //    throw new CustomException("Loan Type Charges Budget   not Found!");
                        //if (string.IsNullOrEmpty(gl.ChargesPayableActivityId))
                        //    throw new CustomException("Loan Type Charges Activity   not Found!");

                        voucherDetailTo.GLGeneralInfoId = financingDetail.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = financingDetail.BudgetMasterId;
                        voucherDetailTo.ActivityId = financingDetail.ActivityId;

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
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                     
                        voucherDetailTo.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailTo.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailTo.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailTo.ActivityId = bankMaster["ActivityId"].ToString();
                        voucherDetailTo.PaymentSource = voucherVM.PaymentSource;

                        voucherDetailTo.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.SourceType == LoanTransactionType.AdditionalLoanPayable.ToString())
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
                        voucherDetailTo.PaymentSource = voucherVM.PaymentSource;
                    }
                    voucherDetailTo.CrAmount = voucherVM.Amount;
                    //voucherDetailTo.FinancingDetailWriteOffId = financingDetailWriteOff.Id;

                    #endregion To
                }
                var currentVoucherDetailId = 1;
                if (null != invoiceTaxVMList && voucherVM.SourceType == LoanTransactionType.LoanTax.ToString())
                {
                    taxDrAmount = 0;
                    foreach (var invoiceTaxVM in invoiceTaxVMList)
                    {
                        var taxCode = _accountsCommonService.GetTaxCode(invoiceTaxVM.TaxCodeId);

                        if (voucherVM.IsExcludingTax)
                        {
                            if (Convert.ToBoolean(taxCode["IsWithhold"].ToString()) == false)
                                throw new CustomException("Withhold  is not configured for TaxCode " + taxCode["StandardName"].ToString());
                        }

                        //merge = Convert.ToBoolean(taxCode["IsMerge"].ToString());
                        var taxCodeGL = _accountsCommonService.GetTaxCodeGL(taxCode["Id"].ToString());
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");
                        var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                        var invoice = new Invoice
                        {
                            PartyId = voucherVM.PartyId,
                            SourceType = SourceType.LoanInterestPayable.ToString(),
                            TaxYearId = financing.TaxYearId,
                            TaxYearPeriodId = financing.TaxYearPeriodId
                        };
                        AuditService.AddedLog(invoice);
                        var invoiceTax = new InvoiceTax
                        {
                            TaxYearId = financing.TaxYearId,
                            TaxYearPeriodId = financing.TaxYearPeriodId,
                            FinancingId = voucherVM.FinancingId,
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            TaxAmount = Math.Round(invoiceTaxVM.TaxAmount, 4),
                            TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                            PartyId = voucherVM.PartyId,
                            SourceType = SourceType.LoanInterestPayable.ToString()
                        };
                        taxDrAmount += Math.Round(invoiceTaxVM.TaxAmount, 4);
                        _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);

                        // Insert Into Customer Invoice Tax Detail (Withhold GL)
                        withholdgl = Convert.ToBoolean(taxCode["IsWithhold"]);
                        if (Convert.ToBoolean(taxCode["IsWithhold"]) && string.IsNullOrEmpty(taxCodeGL["WithholdCreditableGLId"].ToString()))
                            throw new CustomException("Withhold GL is not found of TaxCode " + taxCode["StandardName"].ToString());
                        if (Convert.ToBoolean(taxCode["IsWithhold"]) && !string.IsNullOrEmpty(taxCodeGL["WithholdCreditableGLId"].ToString()))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                                BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                                ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                                Amount = invoiceTax.TaxAmount,
                                AType = "Dr"
                            };
                            totalVoucherDetailTaxAmount += totalwithholdCrAmount;
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                DrAmount = invoiceTaxDetail.Amount
                                //PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
                            };
                            totalAmountCr += voucherDetailTax.CrAmount;
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                            var voucherDetailCurrencydb = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                            };
                            totalCurrencyAmountDr += voucherDetailCurrencydb.DrAmount;
                            totalAmountDr += voucherDetailTax.DrAmount;
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencydb);
                        }

                        // Insert Into Customer Invoice Tax Detail (Creditable GL)
                        creditablegl = Convert.ToBoolean(taxCode["IsCreditable"]);
                        if (Convert.ToBoolean(taxCode["IsCreditable"]) && string.IsNullOrEmpty(taxCodeGL["CreditableGLId"].ToString()))
                            throw new CustomException("Creditable GL is not found of TaxCode " + taxCode["StandardName"].ToString());
                        if (Convert.ToBoolean(taxCode["IsCreditable"]) && !string.IsNullOrEmpty(taxCodeGL["CreditableGLId"].ToString()))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL["CreditableGLId"].ToString(),
                                BudgetMasterId = taxCodeGL["CreditableGLBudgetMasterId"].ToString(),
                                ActivityId = taxCodeGL["CreditableGLActivityId"].ToString(),
                                Amount = invoiceTax.TaxAmount,
                                AType = "Dr"
                            };
                            totalcreditableDrAmount += invoiceTaxDetail.Amount;
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 2);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                DrAmount = invoiceTaxDetail.Amount
                                //PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);
                            totalAmountDr += voucherDetailTax.DrAmount;
                            var voucherDetailCurrencybase = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherVM.CurrencyId,
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.DrAmount,
                                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                            };
                            totalCurrencyAmountDr += voucherDetailCurrencybase.DrAmount;
                            totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                        }
                        
                    }
                }
                if (financing.TransactionType == TransactionType.LoanTaken.ToString() && voucherVM.SourceType == LoanTransactionType.LoanInterestPayable.ToString()
                    || voucherVM.SourceType == LoanTransactionType.OtherExpensesPayable.ToString() || voucherVM.SourceType == LoanTransactionType.LoanTax.ToString())
                {
                    //********************VoucherDetail From******************************
                    //_voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                    //totalAmountDr += voucherDetailFrom.DrAmount;
                    //********************VoucherDetail To******************************
                    currentVoucherDetailId++;
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
                            var bankMasterTo = _accountsCommonService.GetBankMaster(voucherDetailTo.BankMasterId);
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = bankMasterTo["CurrencyId"].ToString() == voucher.CurrencyId ? voucherDetailTo.DrAmount : voucherVM.CompanyCurrencyRate * voucherDetailTo.DrAmount,
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
                        DrAmount = voucherVM.ExchangeAmount
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
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                        if (null == bankMaster)
                            throw new CustomException("Bank data not found!");
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
                        DrAmount = voucherVM.ExchangeAmount
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