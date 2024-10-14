using Library.Core;
using Library.Crosscutting.Security;
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
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Banks;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Library.Service.Banks
{
    public class BankJournalNewService : IBankJournalNewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IRepositoryAsync<BankJournal> _bankJournalRepository;
        private readonly IRepositoryAsync<BankJournalDetail> _bankJournalDetailRepository;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;
        private readonly IRepositoryAsync<GLTransactionDetail> _gLTransactionDetailRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<FinancingTypeGL> _financingTypeGLRepository;
        private readonly IRepositoryAsync<BankMaster> _bankMasterRepository;
        private readonly IRepositoryAsync<CashMaster> _cashMasterRepository;
        private readonly IBankChargeService _bankChargeService;
        private readonly IRepositoryAsync<BankCharge> _bankChargeRepository;
        private readonly IEmployeeTransactionTypeGLService _employeeTransactionTypeGLService;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly IRepositoryAsync<CompanyPartyGL> _companyPartyGLRepository;

        public BankJournalNewService(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IVoucherService voucherService
            , ICompanyTaxYearService companyTaxYearService
            , ICompanyFiscalYearService companyFiscalYearService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IRepositoryAsync<BankJournal> bankJournalRepository
            , IRepositoryAsync<BankJournalDetail> bankJournalDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<FinancingTypeGL> financingTypeGLRepository
            , IRepositoryAsync<BankMaster> bankMasterRepository
            , IRepositoryAsync<CashMaster> cashMasterRepository
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository
            , IRepositoryAsync<GLTransactionDetail> gLTransactionDetailRepository
            , IBankChargeService bankChargeService
            , IEmployeeTransactionTypeGLService employeeTransactionTypeGLService
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , IRepositoryAsync<CompanyPartyGL> companyPartyGLRepository
            , IRepositoryAsync<BankCharge> bankChargeRepository)
        {
            _unitOfWork = unitOfWork;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _sqlRepository = sqlRepository;
            _bankJournalRepository = bankJournalRepository;
            _bankJournalDetailRepository = bankJournalDetailRepository;
            _voucherService = voucherService;
            _pkGeneratorService = pkGeneratorService;
            _financingTypeGLRepository = financingTypeGLRepository;
            _companyFiscalYearService = companyFiscalYearService;
            _companyTaxYearService = companyTaxYearService;
            _bankMasterRepository = bankMasterRepository;
            _cashMasterRepository = cashMasterRepository;
            _bankChargeService = bankChargeService;
            _employeeTransactionTypeGLService = employeeTransactionTypeGLService;
            _companyPartyRepository = companyPartyRepository;
            _companyPartyGLRepository = companyPartyGLRepository;
            _voucherRepository = voucherRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _gLTransactionDetailRepository = gLTransactionDetailRepository;
            _bankChargeRepository = bankChargeRepository;
        }

        public BankJournal InsertBankJournal(BankJournal bankJournal)
        {
            bankJournal.Id = _pkGeneratorService.GetAutoNumber(nameof(BankJournal), PKGeneratorEnum.Yearly, null, DateTime.Now);
            AuditService.AddedLog(bankJournal);
            _bankJournalRepository.Insert(bankJournal);
            return bankJournal;
        }

        public string InsertBankJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                if (voucherVM.BankMasterId == voucherVM.OtherBankMasterId)
                    throw new CustomException("Same to same bank transfer is not allowed!");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.SourceType = SourceType.BankJournal.ToString();

                var bankJournal = InsertBankJournal(new BankJournal
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    BankMasterId = voucherVM.BankMasterId,
                    IsPark = voucherVM.IsPark,
                    SourceType = voucherVM.SourceType,
                    PaymentSource = PaymentSource.Bank.ToString(),
                    BankJournalType = voucherVM.BankJournalType,
                    Amount = voucherVM.Amount,
                    IsReverse = voucherVM.IsReverse
                    
                });

                // INSERT INTO Voucher TABLE
                var voucher = _voucherService.InsertVoucher(new Voucher
                {
                    CompanyGroupId = bankJournal.CompanyGroupId,
                    CompanyId = bankJournal.CompanyId,
                    PlantId = bankJournal.PlantId,
                    EntityId = bankJournal.EntityId,
                    CurrencyId = bankJournal.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = bankJournal.PostingDate,
                    DocDate = bankJournal.DocDate,
                    DocRefNo = bankJournal.DocRefNo,
                    Narration = bankJournal.Narration,
                    SourceType = bankJournal.SourceType,
                    AddedBy = bankJournal.AddedBy,
                    AddedFromIP = bankJournal.AddedFromIP,
                    AddedDate = bankJournal.AddedDate,
                    Archive = bankJournal.Archive,
                    IsPark = bankJournal.IsPark,
                    ApprovedById=voucherVM.ApprovedById,
                    ApprovedByStatus=voucherVM.ApprovedByStatus
                }, voucherVM.FiscalYearPrefix);

                // Set VoucherId in BankJournal
                bankJournal.VoucherId = voucher.Id;

                var currentVoucherDetailId = 1;
                var currentBankJournalDetailId = 0;
                var bankMaster = _bankMasterRepository.Find(bankJournal.BankMasterId);
                // INSERT INTO VoucherDetail Credit
                var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                {
                    GLGeneralInfoId = bankMaster.GLGeneralInfoId,
                    BudgetMasterId = bankMaster.BudgetMasterId,
                    ActivityId = bankMaster.ActivityId,
                    BankMasterId = bankJournal.BankMasterId,
                    DrAmount = bankJournal.BankJournalType == BankJournalType.ProfitEarn.ToString() ? bankJournal.Amount : 0,
                    CrAmount = bankJournal.BankJournalType != BankJournalType.ProfitEarn.ToString() ? bankJournal.Amount : 0,
                    PaymentSource = bankJournal.PaymentSource,
                    PartyType = bankJournal.PaymentSource,
                    TrnNature = TransactionNature.Bank.ToString()
                }, currentVoucherDetailId);

                if (bankJournal.IsReverse &&
                    (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString()
                    || bankJournal.BankJournalType == BankJournalType.BankToCash.ToString()
                    || bankJournal.BankJournalType == BankJournalType.BankCharge.ToString()))
                {
                    voucherDetail.DrAmount = bankJournal.Amount;
                    voucherDetail.CrAmount = 0;
                }
                else if (bankJournal.IsReverse && bankJournal.BankJournalType == BankJournalType.ProfitEarn.ToString())
                {
                    voucherDetail.DrAmount = 0;
                    voucherDetail.CrAmount = bankJournal.Amount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.BankReverse.ToString())
                {
                    voucherDetail.CrAmount = 0;
                    voucherDetail.DrAmount = bankJournal.Amount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.CashExpense.ToString())
                {
                    voucherDetail.DrAmount = 0;
                    voucherDetail.CrAmount = bankJournal.Amount;
                }

                var glTransactionDetailCr = new GLTransactionDetail
                {
                    SourceType = voucherDetail.PaymentSource,
                    BankMasterId = voucherDetail.BankMasterId,
                };
                decimal CompanyCurrencyAmountDr = 0;
                decimal CompanyCurrencyAmountCr = 0;
                if(voucherDetailVMList!=null)
                {
                    if(voucherDetailVMList.Sum(r => r.CompanyCurrencyAmount) == 0)
                    {
                        throw new CustomException(companyCurrencyCode + " is required.");
                    }
                    if(voucherDetail.DrAmount>0)
                    {
                        CompanyCurrencyAmountDr = Math.Round(voucherDetailVMList.Sum(r => r.CompanyCurrencyAmount), 2);

                    }
                    if (voucherDetail.CrAmount > 0)
                    {
                        CompanyCurrencyAmountCr = Math.Round(voucherDetailVMList.Sum(r => r.CompanyCurrencyAmount), 2);

                    }

                }
                else
                {
                    CompanyCurrencyAmountDr = Math.Round(voucherDetail.DrAmount * voucherVM.CompanyCurrencyRate, 2);
                    CompanyCurrencyAmountCr = Math.Round(voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate, 2);
                }
                if (bankMaster.CurrencyId == voucher.CurrencyId)
                {
                    glTransactionDetailCr.CrAmount = voucherDetail.CrAmount;
                    glTransactionDetailCr.DrAmount = voucherDetail.DrAmount;
                }
                else
                {
                    //glTransactionDetailCr.CrAmount = Math.Round(voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate, 2);
                    glTransactionDetailCr.CrAmount = CompanyCurrencyAmountCr;
                    glTransactionDetailCr.DrAmount = CompanyCurrencyAmountDr;
                }
                _voucherService.InsertGLTransactionDetail(voucherDetail, glTransactionDetailCr);



                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = CompanyCurrencyAmountDr,
                    CrAmount = CompanyCurrencyAmountCr
                });

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetail.DrAmount;
                var totalAmountCr = voucherDetail.CrAmount;

                // INSERT INTO Debit Side
                if (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString() ||
                    bankJournal.BankJournalType == BankJournalType.BankToCash.ToString())
                {
                    // INSERT INTO BankJournalDetail
                    var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                    {
                        Amount = bankJournal.Amount,
                        BankJournalId = bankJournal.Id,
                        BankMasterId = voucherVM.OtherBankMasterId,
                        CashMasterId = voucherVM.OtherCashMasterId
                    }, 1);

                    if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        bankJournalDetail.Amount -= bankChargeDetailVMList.Sum(r => r.Amount);

                    var voucherDetailDr = new VoucherDetail
                    {
                        CurrencyId = voucher.CurrencyId,
                        DrAmount = bankJournalDetail.Amount,
                        PaymentSource = bankJournal.PaymentSource,
                        BankJournalDetailId = bankJournalDetail.Id,
                        PartyType = bankJournal.PaymentSource,
                        CashMasterId = voucherVM.OtherCashMasterId,
                        BankMasterId = voucherVM.OtherBankMasterId,
                        Narration = voucherVM.Narration
                    };

                    if (bankJournal.IsReverse)
                    {
                        voucherDetailDr.CrAmount = voucherDetailDr.DrAmount;
                        voucherDetailDr.DrAmount = 0;
                    }

                    var glTransactionDetailDr = new GLTransactionDetail
                    {
                        SourceType = voucherDetailDr.PaymentSource,
                        DrAmount = Math.Round(voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate, 2),
                        BankMasterId = voucherDetailDr.BankMasterId,
                        CashMasterId = voucherDetailDr.CashMasterId
                    };



                    if (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString())
                    {
                        if (string.IsNullOrEmpty(bankJournalDetail.BankMasterId))
                            throw new CustomException("To Bank account is null.");

                        bankJournalDetail.CashMasterId = null;
                        glTransactionDetailDr.CashMasterId = null;

                        var otherBankMaster = _bankMasterRepository.Find(bankJournalDetail.BankMasterId);
                        voucherDetailDr.GLGeneralInfoId = otherBankMaster.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = otherBankMaster.BudgetMasterId;
                        voucherDetailDr.ActivityId = otherBankMaster.ActivityId;
                        voucherDetailDr.TrnNature = TransactionNature.ToBank.ToString();

                        if (otherBankMaster.CurrencyId == voucher.CurrencyId)
                        {
                            glTransactionDetailDr.DrAmount = voucherDetailDr.DrAmount; 
                        }
                        else
                        {
                            glTransactionDetailDr.DrAmount = Math.Round(voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate, 2);
                        }
                    }

                    if (bankJournal.BankJournalType == BankJournalType.BankToCash.ToString())
                    {
                        if (string.IsNullOrEmpty(bankJournalDetail.CashMasterId))
                            throw new CustomException("To Cash is null.");

                        bankJournalDetail.BankMasterId = null;
                        glTransactionDetailDr.BankMasterId = null;

                        var cashMaster = _cashMasterRepository.Find(bankJournalDetail.CashMasterId);
                        voucherDetailDr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = cashMaster.BudgetMasterId;
                        voucherDetailDr.ActivityId = cashMaster.ActivityId;
                        voucherDetailDr.TrnNature = TransactionNature.ToCash.ToString();
                        
                        if (cashMaster.CurrencyId == voucher.CurrencyId)
                        {
                            glTransactionDetailDr.DrAmount = voucherDetailDr.DrAmount;
                        }
                        else
                        {
                            glTransactionDetailDr.DrAmount = Math.Round(voucherDetailDr.DrAmount* voucherVM.CompanyCurrencyRate, 2);
                        }
                    }

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetailDr);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount, 2),
                        CrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount, 2)
                    });

                    // Set Dr/Cr amount to local variable.
                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.ProfitEarn.ToString())
                {
                    // INSERT INTO BankJournalDetail
                    var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                    {
                        Amount = bankJournal.Amount,
                        BankJournalId = bankJournal.Id,
                        BankMasterId = voucherVM.OtherBankMasterId,
                        CashMasterId = voucherVM.OtherCashMasterId,
                        FinancingTypeId = voucherVM.FinancingTypeId
                    }, 1);

                    // Get Revenue GL
                    var revenueGL = GetRevenueGL(bankJournal.CompanyId, bankJournalDetail.FinancingTypeId);

                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = revenueGL.RevenueGLId,
                        CurrencyId = voucher.CurrencyId,
                        ActivityId = revenueGL.RevenueActivityId,
                        BudgetMasterId = revenueGL.RevenueBudgetMasterId,
                        CrAmount = bankJournal.Amount,
                        PaymentSource = bankJournal.PaymentSource,
                        BankJournalDetailId = bankJournalDetail.Id,
                        PartyType = bankJournal.PaymentSource,
                        TrnNature = TransactionNature.Profit.ToString()
                    };

                    if (bankJournal.IsReverse)
                    {
                        voucherDetailCr.DrAmount = voucherDetailCr.CrAmount;
                        voucherDetailCr.CrAmount = 0;
                    }
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetailCr.DrAmount, 2),
                        CrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount, 2)
                    });

                    // Set Dr/Cr amount to local variable.
                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.BankReverse.ToString())
                {
                    // INSERT INTO BankJournalDetail
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        // INSERT INTO BankJournalDetail
                        currentBankJournalDetailId++;
                        var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = voucherDetailVM.Amount
                        }, currentBankJournalDetailId);

                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankJournalDetailId = bankJournalDetail.Id,
                            GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId,
                            BudgetMasterId = bankJournalDetail.BudgetMasterId,
                            ActivityId = bankJournalDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = bankJournalDetail.Amount,
                            PaymentSource = bankJournal.PaymentSource,
                            PartyType = bankJournal.PaymentSource,
                            Narration = voucherVM.Narration,
                            TrnNature = TransactionNature.ToBank.ToString()
                        }, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = Math.Round(voucherDetailVM.CompanyCurrencyAmount, 2)
                        });

                        totalAmountDr += voucherDetailDr.DrAmount;
                        totalAmountCr += voucherDetailDr.CrAmount;
                    }
                }
                else if (bankJournal.BankJournalType == BankJournalType.CashExpense.ToString())
                {
                    if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                        throw new CustomException("Expense GL list not found!");

                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        // INSERT INTO BankJournalDetail
                        currentBankJournalDetailId++;
                        var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = voucherDetailVM.Amount
                        }, currentBankJournalDetailId);

                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankJournalDetailId = bankJournalDetail.Id,
                            GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId,
                            BudgetMasterId = bankJournalDetail.BudgetMasterId,
                            ActivityId = bankJournalDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = bankJournalDetail.Amount,
                            PaymentSource = bankJournal.PaymentSource,
                            PartyType = bankJournal.PaymentSource,
                            Narration = voucherVM.Narration,
                            TrnNature = TransactionNature.ToExpense.ToString()
                        }, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = Math.Round(voucherDetailVM.CompanyCurrencyAmount, 2)
                        });

                        totalAmountDr += voucherDetailDr.DrAmount;
                        totalAmountCr += voucherDetailDr.CrAmount;
                    }
                }

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var currentBankChargeId = 0;
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        currentBankChargeId++;
                        var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                        {
                            BankJournalId = bankJournal.Id,
                            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                            BankMasterId = bankJournal.BankMasterId,
                            CashMasterId = bankJournal.CashMasterId,
                            Amount = bankChargeDetailVM.Amount,
                            Narration = bankJournal.Narration,
                            AddedBy = bankJournal.AddedBy,
                            AddedDate = bankJournal.AddedDate,
                            AddedFromIP = bankJournal.AddedFromIP,
                            Archive = bankJournal.Archive,
                            SourceType = bankJournal.SourceType
                        }, currentBankChargeId);

                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(bankJournal.CompanyId, bankCharge.FinancingTypeId);

                        // Insert Bank charges Debit
                        currentVoucherDetailId++;
                        var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankChargeId = bankCharge.Id,
                            GLGeneralInfoId = expenseGL.ExpensesGLId,
                            BudgetMasterId = expenseGL.ExpensesBudgetMasterId,
                            ActivityId = expenseGL.ExpensesActivityId,
                            DrAmount = bankCharge.Amount,
                            Narration = bankCharge.Narration,
                            PartyType = bankJournal.PaymentSource,
                            PaymentSource = bankJournal.PaymentSource,
                            TrnNature = TransactionNature.Charge.ToString()
                        }, currentVoucherDetailId);

                        if (bankJournal.IsReverse)
                        {
                            voucherDetailChargeDr.CrAmount = voucherDetailChargeDr.DrAmount;
                            voucherDetailChargeDr.DrAmount = 0;
                        }

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailChargeDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = bankJournal.IsReverse ? 0 : bankChargeDetailVM.CompanyCurrencyAmount,
                            CrAmount = !bankJournal.IsReverse ? 0 : bankChargeDetailVM.CompanyCurrencyAmount
                        });

                        // Set Dr/Cr amount to local variable.
                        totalAmountDr += voucherDetailChargeDr.DrAmount;
                        totalAmountCr += voucherDetailChargeDr.CrAmount;
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #region Bank Payment

        public GridModel GetBankCashPaymentList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            try
            {
                parameters.CmdText = @"SELECT BC.Id, BC.Id AS BankJournalId ,V.Id AS VoucherId, VD.Id AS VoucherDetailId, BJD.Id AS BankJournalDetailId, BC.PostingDate, BC.DocDate, BM.AccountTitle, BM.AccountNumber, BM.Id AS BankMasterId
										, V.EntityId, BC.BankJournalType, CM.UserName AS CashAccountTitle, BC.CashMasterId
                                        , V.VoucherDate, V.DocRefNo, V.VoucherNo, V.VoucherTypeId, BC.IsPark, BC.CurrencyId, C.Code AS CurrencyCode
										, V.Narration, BC.IsReverse, BJD.EmployeeId, BJD.PartyId, BJD.PartyPlantId, BJD.PartyType
										, BJD.BankMasterId AS OtherBankMasterId, BJD.CashMasterId AS OtherCashMasterId, EI.EmployeeCode+'-'+ EI.EmployeeName AS EmployeeName
										, P.UserName AS PartyName, CC.CompanyCurrencyRate, BJD.EmployeeTransactionTypeId
										,[Amount]=CASE WHEN ISNULL(BJD.Amount,0)=0 THEN ISNULL(BC.Amount,0)
										ELSE ISNULL(BJD.Amount,0) END ,  ISNULL(BC.Amount,0) AS TotalAmount
                                        FROM [TRN].[BankJournal] AS BC
                                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=BC.BankMasterId
                                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=BC.CashMasterId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BC.VoucherId
										LEFT JOIN(SELECT * FROM  [TRN].[BankJournalDetail] WHERE PartyType<>'GL') AS BJD ON BJD.BankJournalId=BC.Id
										LEFT JOIN TRN.VoucherDetail AS VD ON VD.BankJournalDetailId=BJD.Id
										LEFT JOIN (SELECT DISTINCT VDC.VoucherId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.ToCurrencyConversion,
										VDC.FromCurrencyId, VDC.ParallelCurrencyId,VDC.ToCurrencyRate AS CompanyCurrencyRate
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
										) AS CC ON CC.VoucherId=BC.VoucherId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=BC.CurrencyId
										LEFT JOIN [HKP].[Party] AS P ON P.Id=BJD.PartyId
										LEFT JOIN dbo.EmployeeInformation AS EI ON EI.SystemId=BJD.EmployeeId
                                        WHERE BC.Archive=0 AND BC.CompanyGroupId='" + companyGroupId + "' AND BC.CompanyId='" + companyId + "' AND BC.PlantId='" + plantId + "' AND BC.SourceType='" + sourceType + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public GridModel GetBankCashPaymentDetailList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType, string bankJournalId)
        {
            try
            {
                parameters.CmdText = @"SELECT BJD.Id, BJD.Id AS BankJournalDetailId, BJ.VoucherId, VD.Id AS VoucherDetailId, BJD.BankJournalId, BJD.GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
										,B.UserName AS BudgetName, A.UserName AS ActivityName ,BJD.PartyType, BJD.Amount
										FROM [TRN].[BankJournalDetail] AS BJD
										LEFT JOIN [TRN].[BankJournal] AS BJ ON BJ.Id=BJD.BankJournalId
										LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.BankJournalDetailId=BJD.Id
										LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=BJD.GLGeneralInfoId
										LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=BJD.BudgetMasterId
										LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
										LEFT JOIN HKP.Activity AS A ON A.Id=BJD.ActivityId
                                        WHERE  BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + @"'
                                        AND BJ.PlantId= '" + plantId + "' AND BJ.SourceType= '" + sourceType + "' AND BJD.BankJournalId='" + bankJournalId + "' AND bjd.PartyType='" + PartyType.GL + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public string InsertBankPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                if (voucherVM.BankMasterId == voucherVM.OtherBankMasterId)
                    throw new CustomException("Same to same bank transfer is not allowed!");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                var bankJournal = InsertBankJournal(new BankJournal
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    BankMasterId = voucherVM.BankMasterId,
                    IsPark = voucherVM.IsPark,
                    SourceType = SourceType.PaymentByBank.ToString(),
                    PaymentSource = PaymentSource.Bank.ToString(),
                    BankJournalType = voucherVM.BankJournalType,
                    Amount = voucherVM.Amount,
                    IsReverse = voucherVM.IsReverse,
                    EmployeeId = voucherVM.EmployeeId,
                    EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType
                });

                // INSERT INTO Voucher TABLE
                var voucher = _voucherService.InsertVoucher(new Voucher
                {
                    CompanyGroupId = bankJournal.CompanyGroupId,
                    CompanyId = bankJournal.CompanyId,
                    PlantId = bankJournal.PlantId,
                    EntityId = bankJournal.EntityId,
                    CurrencyId = bankJournal.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = bankJournal.PostingDate,
                    DocDate = bankJournal.DocDate,
                    DocRefNo = bankJournal.DocRefNo,
                    Narration = bankJournal.Narration,
                    SourceType = bankJournal.SourceType,
                    AddedBy = bankJournal.AddedBy,
                    AddedFromIP = bankJournal.AddedFromIP,
                    AddedDate = bankJournal.AddedDate,
                    Archive = bankJournal.Archive,
                    IsPark = bankJournal.IsPark
                }, voucherVM.FiscalYearPrefix);

                // Set VoucherId in BankJournal
                bankJournal.VoucherId = voucher.Id;

                var currentVoucherDetailId = 1;
                var bankMaster = _bankMasterRepository.Find(bankJournal.BankMasterId);
                if (null == bankMaster.ActivityId)
                    throw new CustomException("Activity data not found!");
                // INSERT INTO VoucherDetail Credit
                var voucherDetailCr = new VoucherDetail
                {
                    GLGeneralInfoId = bankMaster.GLGeneralInfoId,
                    BudgetMasterId = bankMaster.BudgetMasterId,
                    ActivityId = bankMaster.ActivityId,
                    BankMasterId = bankJournal.BankMasterId,
                    CrAmount = bankJournal.Amount,
                    PaymentSource = bankJournal.PaymentSource,
                    PartyType = bankJournal.PartyType,
                    TrnNature = TransactionNature.Bank.ToString()
                };
                if (bankJournal.BankJournalType != BankJournalType.BankToGL.ToString() && null != voucherDetailVMList)
                {
                    voucherDetailCr.CrAmount += voucherDetailVMList.Sum(r => r.Amount);
                    bankJournal.Amount = voucherDetailCr.CrAmount;
                }
                _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                // INSRT INTO GLTransactionDetail
                _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                {
                    SourceType = voucher.SourceType,
                    BankMasterId = voucherDetailCr.BankMasterId,
                    CrAmount = bankMaster.CurrencyId == voucher.CurrencyId ? voucherDetailCr.CrAmount : voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount,
                });

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailCr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount
                });

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetailCr.DrAmount;
                var totalAmountCr = voucherDetailCr.CrAmount;

                // INSERT INTO Debit Side
                var currentBankJournalDetailId = 0;
                if (bankJournal.BankJournalType != BankJournalType.BankToGL.ToString())
                {
                    // INSERT INTO BankJournalDetail
                    currentBankJournalDetailId++;
                    var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                    {
                        BankJournalId = bankJournal.Id,
                        Amount = voucherVM.Amount
                    }, currentBankJournalDetailId);

                    var voucherDetailDr = new VoucherDetail
                    {
                        CurrencyId = voucher.CurrencyId,
                        DrAmount = bankJournalDetail.Amount,
                        PaymentSource = bankJournal.PaymentSource,
                        BankJournalDetailId = bankJournalDetail.Id,
                        Narration = voucherVM.Narration
                    };

                    var glTransactionDetailDr = new GLTransactionDetail
                    {
                        SourceType = voucherDetailDr.PaymentSource,
                    };

                    if (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherBankMasterId))
                            throw new CustomException("To Bank account is null.");

                        var otherBankMaster = _bankMasterRepository.Find(voucherVM.OtherBankMasterId);
                        if (string.IsNullOrEmpty(otherBankMaster.ActivityId))
                            throw new CustomException("Activity data not found!");
                        bankJournalDetail.PartyType = PartyType.Bank.ToString();
                        bankJournalDetail.BankMasterId = voucherVM.OtherBankMasterId;
                        bankJournalDetail.GLGeneralInfoId = otherBankMaster.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = otherBankMaster.BudgetMasterId;
                        bankJournalDetail.ActivityId = otherBankMaster.ActivityId;

                        voucherDetailDr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailDr.BankMasterId = bankJournalDetail.BankMasterId;
                        voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailDr.TrnNature = TransactionNature.ToBank.ToString();

                        glTransactionDetailDr.SourceType = voucher.SourceType;
                        glTransactionDetailDr.BankMasterId = voucherDetailDr.BankMasterId;
                        glTransactionDetailDr.DrAmount = otherBankMaster.CurrencyId == voucher.CurrencyId ? voucherVM.Amount : voucherVM.CompanyCurrencyRate * voucherVM.Amount;

                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToCash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherCashMasterId))
                            throw new CustomException("To Cash is null.");

                        var otherCashMaster = _cashMasterRepository.Find(voucherVM.OtherCashMasterId);
                        if (string.IsNullOrEmpty(otherCashMaster.ActivityId))
                            throw new CustomException("Activity data not found!");
                        bankJournalDetail.PartyType = PartyType.Cash.ToString();
                        bankJournalDetail.CashMasterId = voucherVM.OtherCashMasterId;
                        bankJournalDetail.GLGeneralInfoId = otherCashMaster.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = otherCashMaster.BudgetMasterId;
                        bankJournalDetail.ActivityId = otherCashMaster.ActivityId;

                        voucherDetailDr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailDr.CashMasterId = bankJournalDetail.CashMasterId;
                        voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailDr.TrnNature = TransactionNature.ToCash.ToString();

                        glTransactionDetailDr.SourceType = voucher.SourceType;
                        glTransactionDetailDr.CashMasterId = voucherDetailDr.CashMasterId;
                        glTransactionDetailDr.DrAmount = otherCashMaster.CurrencyId == voucher.CurrencyId ? voucherVM.Amount : voucherVM.CompanyCurrencyRate * voucherVM.Amount;

                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToEmployee.ToString())
                    {
                        var payableGL = _employeeTransactionTypeGLService.GetEmployeePayableGL(bankJournal.CompanyId, bankJournal.EmployeeTransactionTypeId);
                        if (string.IsNullOrEmpty(payableGL.PayableActivityId))
                            throw new CustomException("Activity data not found!");
                        bankJournalDetail.EmployeeTransactionTypeId = bankJournal.EmployeeTransactionTypeId;
                        bankJournalDetail.PartyType = PartyType.Employee.ToString();
                        bankJournalDetail.EmployeeId = bankJournal.EmployeeId;
                        bankJournalDetail.GLGeneralInfoId = payableGL.PayableGLId;
                        bankJournalDetail.BudgetMasterId = payableGL.PayableBudgetMasterId;
                        bankJournalDetail.ActivityId = payableGL.PayableActivityId;

                        voucherDetailDr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailDr.EmployeeId = bankJournalDetail.EmployeeId;
                        voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailDr.TrnNature = TransactionNature.ToEmployee.ToString();
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToVendor.ToString())
                    {
                        bankJournalDetail.PartyType = PartyType.Vendor.ToString();
                        bankJournalDetail.PartyId = bankJournal.PartyId;
                        bankJournalDetail.PartyPlantId = bankJournal.PartyPlantId;
                        var companyParty = _companyPartyRepository.Query(r => r.CompanyId == bankJournal.CompanyId && r.PlantId == bankJournal.PlantId && r.PartyId == bankJournalDetail.PartyId && r.PartyType == bankJournalDetail.PartyType).Select().FirstOrDefault();
                        if (null == companyParty)
                            throw new CustomException($"Plant {bankJournalDetail.PartyType} mapping not found!");

                        var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                        if (null == companyPartyGLList)
                            throw new CustomException($"{bankJournalDetail.PartyType} GL not found!");

                        var reconGL = PartyGLType.ReconciliationGL.ToString();
                        var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                        if (null == regularGL)
                            throw new CustomException($"{bankJournalDetail.PartyType} Reconciliation GL not found!");
                        if (string.IsNullOrEmpty(regularGL.ActivityId))
                            throw new CustomException("Activity data not found!");
                        bankJournalDetail.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = regularGL.BudgetMasterId;
                        bankJournalDetail.ActivityId = regularGL.ActivityId;

                        voucherDetailDr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailDr.PartyId = bankJournalDetail.PartyId;
                        voucherDetailDr.PartyPlantId = bankJournalDetail.PartyPlantId;
                        voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailDr.TrnNature = TransactionNature.ToVendor.ToString();
                    }

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetailDr);

                    _voucherService.InsertGLTransactionDetail(voucherDetailDr, new GLTransactionDetail
                    {
                        BankMasterId = voucherDetailDr.BankMasterId,
                        CashMasterId = voucherDetailDr.CashMasterId,
                        DrAmount = glTransactionDetailDr.DrAmount,
                        SourceType = glTransactionDetailDr.SourceType
                    });

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                    });

                    // Set Dr/Cr amount to local variable.
                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.BankToGL.ToString())
                {
                    if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                        throw new CustomException("GL list not found!");
                }

                if (null != voucherDetailVMList)
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");
                        if (voucherDetailVM.ActivityId == null)
                            throw new CustomException("Activity not found");

                        // INSERT INTO BankJournalDetail
                        currentBankJournalDetailId++;
                        var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = voucherDetailVM.Amount,
                            PartyType = PartyType.GL.ToString()
                        }, currentBankJournalDetailId);

                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankJournalDetailId = bankJournalDetail.Id,
                            GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId,
                            BudgetMasterId = bankJournalDetail.BudgetMasterId,
                            ActivityId = bankJournalDetail.ActivityId,
                            DrAmount = bankJournalDetail.Amount,
                            PaymentSource = bankJournal.PaymentSource,
                            PartyType = PartyType.GL.ToString(),
                            Narration = voucherVM.Narration,
                            TrnNature = TransactionNature.ToGL.ToString()
                        }, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                        });

                        totalAmountDr += voucherDetailDr.DrAmount;
                        totalAmountCr += voucherDetailDr.CrAmount;
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public string UpdateBankPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                if (voucherVM.BankMasterId == voucherVM.OtherBankMasterId)
                    throw new CustomException("Same to same bank transfer is not allowed!");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                voucherVM.SourceType = SourceType.PaymentByBank.ToString();

                var bankJournal = FindBankJournal(voucherVM.BankJournalId);
                if (null == bankJournal)
                    throw new CustomException("Bank journal master data not found.");

                // Update validation after Posted.
                CheckIsPosted(bankJournal);

                bankJournal.CurrencyId = voucherVM.CurrencyId;
                bankJournal.EntityId = voucherVM.EntityId;
                bankJournal.DocRefNo = voucherVM.DocRefNo;
                bankJournal.PostingDate = voucherVM.PostingDate;
                bankJournal.Narration = voucherVM.Narration;
                bankJournal.Amount = voucherVM.Amount;
                bankJournal.IsReverse = voucherVM.IsReverse;
                bankJournal.BankMasterId = voucherVM.BankMasterId;
                bankJournal.CashMasterId = voucherVM.CashMasterId;
                bankJournal.PartyType = voucherVM.PartyType;

                var voucher = _voucherService.FindVoucher(bankJournal.VoucherId);
                voucher.CurrencyId = bankJournal.CurrencyId;
                voucher.EntityId = bankJournal.EntityId;
                voucher.DocRefNo = bankJournal.DocRefNo;
                voucher.Narration = bankJournal.Narration;
                voucher.UpdatedBy = bankJournal.UpdatedBy;
                voucher.UpdatedDate = bankJournal.UpdatedDate;
                voucher.UpdatedFromIP = bankJournal.UpdatedFromIP;

                var bankMaster = _bankMasterRepository.Find(bankJournal.BankMasterId);
                var voucherDetailCr = _voucherService.FindVoucherDetailForBankJournalDetail(bankJournal.VoucherId);

                voucherDetailCr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                voucherDetailCr.BudgetMasterId = bankMaster.BudgetMasterId;
                voucherDetailCr.ActivityId = bankMaster.ActivityId;
                voucherDetailCr.BankMasterId = bankJournal.BankMasterId;
                voucherDetailCr.CurrencyId = bankJournal.CurrencyId;
                voucherDetailCr.CrAmount = bankJournal.Amount;
                voucherDetailCr.PaymentSource = bankJournal.PaymentSource;
                voucherDetailCr.PartyType = bankJournal.PartyType;
                voucherDetailCr.TrnNature = TransactionNature.Bank.ToString();

                if (bankJournal.BankJournalType != BankJournalType.BankToGL.ToString() && null != voucherDetailVMList)
                {
                    voucherDetailCr.CrAmount += voucherDetailVMList.Sum(r => r.Amount);
                    bankJournal.Amount = voucherDetailCr.CrAmount;
                }

                UpdateBankJournal(bankJournal);//Bank Journal Update
                _voucherService.UpdateVoucher(voucher);//Voucher Update
                _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);//Voucher Detail (Cr) Update.

                if (voucherDetailCr.CashMasterId != null || voucherDetailCr.BankMasterId != null)
                {
                    var glTransactionDetail = _voucherService.FindGLTransactionDetail(voucherDetailCr.Id);
                    glTransactionDetail.CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate;
                    _voucherService.UpdateGLTransactionDetail(voucherDetailCr, glTransactionDetail);
                }

                var voucherDetailCurrency = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucherDetailCr.VoucherId && r.VoucherDetailId == voucherDetailCr.Id).Select().FirstOrDefault();
                if (null != voucherDetailCurrency)
                {
                    voucherDetailCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                    voucherDetailCurrency.ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate;
                    voucherDetailCurrency.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                    voucherDetailCurrency.DrAmount = 0;
                    _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrency);
                }

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetailCr.DrAmount;
                var totalAmountCr = voucherDetailCr.CrAmount;

                // INSERT INTO Debit Side
                if (bankJournal.BankJournalType != BankJournalType.BankToGL.ToString())
                {
                    var bankJournalDetail = _bankJournalDetailRepository.Query(r => r.Id == voucherVM.BankJournalDetailId).Select().FirstOrDefault();
                    bankJournalDetail.Amount = voucherVM.Amount;
                    bankJournalDetail.UpdatedBy = bankJournal.UpdatedBy;
                    bankJournalDetail.UpdatedDate = bankJournal.UpdatedDate;
                    bankJournalDetail.UpdatedFromIP = bankJournal.UpdatedFromIP;
                    bankJournalDetail.CashMasterId = voucherVM.OtherCashMasterId;
                    bankJournalDetail.BankMasterId = voucherVM.OtherBankMasterId;

                    bankJournalDetail.EmployeeId = null;
                    bankJournalDetail.PartyType = voucherVM.PartyType;
                    bankJournalDetail.UpdatedFromIP = bankJournal.UpdatedFromIP;
                    _bankJournalDetailRepository.Update(bankJournalDetail);//Update BankJournal Update

                    var voucherDetailDr = _voucherService.FindVoucherDetail(voucherVM.VoucherDetailId);
                    voucherDetailDr.CurrencyId = voucher.CurrencyId;
                    voucherDetailDr.DrAmount = bankJournalDetail.Amount;
                    voucherDetailDr.PaymentSource = bankJournal.PaymentSource;
                    voucherDetailDr.BankJournalDetailId = bankJournalDetail.Id;
                    voucherDetailDr.Narration = voucherVM.Narration;
                    voucherDetailDr.PartyId = null;
                    voucherDetailDr.PartyPlantId = null;
                    voucherDetailDr.EmployeeId = null;
                    voucherDetailDr.Narration = voucherVM.Narration;

                    if (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherBankMasterId))
                            throw new CustomException("To Bank account is null.");

                        var otherBankMaster = _bankMasterRepository.Find(voucherVM.OtherBankMasterId);

                        bankJournalDetail.PartyType = PartyType.Bank.ToString();
                        bankJournalDetail.BankMasterId = voucherVM.OtherBankMasterId;
                        bankJournalDetail.GLGeneralInfoId = otherBankMaster.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = otherBankMaster.BudgetMasterId;
                        bankJournalDetail.ActivityId = otherBankMaster.ActivityId;

                        voucherDetailDr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailDr.BankMasterId = bankJournalDetail.BankMasterId;
                        voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailDr.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToCash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherCashMasterId))
                            throw new CustomException("To Cash is null.");

                        var otherCashMaster = _cashMasterRepository.Find(voucherVM.OtherCashMasterId);

                        bankJournalDetail.PartyType = PartyType.Cash.ToString();
                        bankJournalDetail.CashMasterId = voucherVM.OtherCashMasterId;
                        bankJournalDetail.GLGeneralInfoId = otherCashMaster.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = otherCashMaster.BudgetMasterId;
                        bankJournalDetail.ActivityId = otherCashMaster.ActivityId;

                        voucherDetailDr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailDr.CashMasterId = bankJournalDetail.CashMasterId;
                        voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailDr.TrnNature = TransactionNature.ToCash.ToString();
                    }
                    if (voucherVM.OtherBankMasterId != null || voucherVM.OtherCashMasterId != null)
                    {
                        var glTransactionDetailDr = _voucherService.FindGLTransactionDetail(voucherDetailDr.Id);
                        if (glTransactionDetailDr != null)
                        {
                            glTransactionDetailDr.SourceType = voucher.SourceType;
                            glTransactionDetailDr.CashMasterId = voucherDetailDr.CashMasterId;
                            glTransactionDetailDr.BankMasterId = voucherDetailDr.BankMasterId;
                            glTransactionDetailDr.DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate;
                            glTransactionDetailDr.CrAmount = 0;
                            _voucherService.UpdateGLTransactionDetail(voucherDetailDr, glTransactionDetailDr);
                        }
                        else
                            _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetailDr);
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToEmployee.ToString())
                    {
                        var payableGL = _employeeTransactionTypeGLService.GetEmployeePayableGL(bankJournal.CompanyId, bankJournal.EmployeeTransactionTypeId);

                        bankJournalDetail.EmployeeTransactionTypeId = bankJournal.EmployeeTransactionTypeId;
                        bankJournalDetail.PartyType = PartyType.Employee.ToString();
                        bankJournalDetail.EmployeeId = bankJournal.EmployeeId;
                        bankJournalDetail.GLGeneralInfoId = payableGL.PayableGLId;
                        bankJournalDetail.BudgetMasterId = payableGL.PayableBudgetMasterId;
                        bankJournalDetail.ActivityId = payableGL.PayableActivityId;
                        bankJournalDetail.PartyId = null;
                        bankJournalDetail.PartyPlantId = null;
                        bankJournalDetail.BankMasterId = null;
                        bankJournalDetail.CashMasterId = null;

                        voucherDetailDr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailDr.EmployeeId = bankJournalDetail.EmployeeId;
                        voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailDr.PartyId = null;
                        voucherDetailDr.PartyPlantId = null;
                        voucherDetailDr.BankMasterId = null;
                        voucherDetailDr.CashMasterId = null;
                        voucherDetailDr.TrnNature = TransactionNature.ToEmployee.ToString();
                        var glTransactionDetailDr = _voucherService.FindGLTransactionDetail(voucherDetailDr.Id);
                        if (glTransactionDetailDr != null)
                        {
                            _voucherService.DeleteGLTransactionDetail(voucherDetailDr, glTransactionDetailDr);
                        }
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToVendor.ToString())
                    {
                        bankJournalDetail.PartyType = PartyType.Vendor.ToString();
                        bankJournalDetail.PartyId = bankJournal.PartyId;
                        bankJournalDetail.PartyPlantId = bankJournal.PartyPlantId;
                        var companyParty = _companyPartyRepository.Query(r => r.CompanyId == bankJournal.CompanyId && r.PlantId == bankJournal.PlantId && r.PartyId == bankJournalDetail.PartyId && r.PartyType == bankJournalDetail.PartyType).Select().FirstOrDefault();
                        if (null == companyParty)
                            throw new CustomException($"Plant {bankJournalDetail.PartyType} mapping not found!");

                        var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                        if (null == companyPartyGLList)
                            throw new CustomException($"{bankJournalDetail.PartyType} GL not found!");

                        var reconGL = PartyGLType.ReconciliationGL.ToString();
                        var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                        if (null == regularGL)
                            throw new CustomException($"{bankJournalDetail.PartyType} Reconciliation GL not found!");

                        bankJournalDetail.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = regularGL.BudgetMasterId;
                        bankJournalDetail.ActivityId = regularGL.ActivityId;
                        bankJournalDetail.EmployeeId = null;
                        bankJournalDetail.BankMasterId = null;
                        bankJournalDetail.CashMasterId = null;
                        bankJournalDetail.PartyId = voucherVM.PartyId;
                        bankJournalDetail.PartyPlantId = voucherVM.PartyPlantId;

                        voucherDetailDr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailDr.PartyId = bankJournalDetail.PartyId;
                        voucherDetailDr.PartyPlantId = bankJournalDetail.PartyPlantId;
                        voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailDr.BankMasterId = null;
                        voucherDetailDr.CashMasterId = null;
                        voucherDetailDr.EmployeeId = null;
                        voucherDetailDr.TrnNature = TransactionNature.ToVendor.ToString();
                        var glTransactionDetailDr = _voucherService.FindGLTransactionDetail(voucherDetailDr.Id);
                        if (glTransactionDetailDr != null)
                        {
                            _voucherService.DeleteGLTransactionDetail(voucherDetailDr, glTransactionDetailDr);
                        }
                    }

                    _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);//Voucher Detail (Dr) Update.

                    var voucherDetailCurrencyDr = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucherDetailDr.VoucherId && r.VoucherDetailId == voucherDetailDr.Id).Select().FirstOrDefault();
                    if (null != voucherDetailCurrency)
                    {
                        voucherDetailCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyDr.ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyDr.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                        voucherDetailCurrencyDr.CrAmount = 0;
                        _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrencyDr);
                    }
                    // Set Dr/Cr amount to local variable.
                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.BankToGL.ToString())
                {
                    if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                        throw new CustomException("GL list not found!");
                }

                if (null != voucherDetailVMList)
                {
                    var currentBankJournalDetailId = GetBankJournalDetailPK(bankJournal.Id);
                    var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);

                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        if (voucherDetailVM.VoucherDetailId != null)
                        {
                            var bankJournalDetailDr = _bankJournalDetailRepository.Query(r => r.Id == voucherDetailVM.BankJournalDetailId).Select().FirstOrDefault();
                            bankJournalDetailDr.Amount = voucherDetailVM.Amount;
                            bankJournalDetailDr.UpdatedBy = bankJournal.UpdatedBy;
                            bankJournalDetailDr.UpdatedDate = bankJournal.UpdatedDate;
                            bankJournalDetailDr.UpdatedFromIP = bankJournal.UpdatedFromIP;
                            bankJournalDetailDr.PartyType = PartyType.GL.ToString();
                            bankJournalDetailDr.UpdatedFromIP = bankJournal.UpdatedFromIP;
                            _bankJournalDetailRepository.Update(bankJournalDetailDr);//Update BankJournalDetail Update

                            var voucherDetailDr = _voucherService.FindVoucherDetail(voucherDetailVM.VoucherDetailId);
                            voucherDetailDr.CurrencyId = voucher.CurrencyId;
                            voucherDetailDr.DrAmount = voucherDetailVM.Amount;
                            voucherDetailDr.PaymentSource = bankJournal.PaymentSource;
                            voucherDetailDr.BankJournalDetailId = voucherDetailVM.BankJournalDetailId;
                            voucherDetailDr.Narration = voucherVM.Narration;
                            voucherDetailDr.PartyId = voucherVM.PartyId;
                            voucherDetailDr.PartyPlantId = voucherVM.PartyPlantId;
                            voucherDetailDr.EmployeeId = voucherVM.EmployeeId;
                            voucherDetailDr.Narration = voucherVM.Narration;
                            _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);//Voucher Detail (Dr) Update.

                            var voucherDetailCurrencyDr = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucherDetailDr.VoucherId && r.VoucherDetailId == voucherDetailDr.Id).Select().FirstOrDefault();
                            if (null != voucherDetailCurrency)
                            {
                                voucherDetailCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                                voucherDetailCurrencyDr.ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate;
                                voucherDetailCurrencyDr.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                                voucherDetailCurrencyDr.CrAmount = 0;
                                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrencyDr);
                            }
                            totalAmountDr += voucherDetailDr.DrAmount;
                            totalAmountCr += voucherDetailDr.CrAmount;
                        }
                        else
                        {
                            // INSERT INTO BankJournalDetail
                            currentBankJournalDetailId++;
                            var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount,
                                PartyType = PartyType.GL.ToString()
                            }, currentBankJournalDetailId);

                            currentVoucherDetailId++;
                            var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankJournalDetailId = bankJournalDetail.Id,
                                GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId,
                                BudgetMasterId = bankJournalDetail.BudgetMasterId,
                                ActivityId = bankJournalDetail.ActivityId,
                                DrAmount = bankJournalDetail.Amount,
                                PaymentSource = bankJournal.PaymentSource,
                                PartyType = PartyType.GL.ToString(),
                                Narration = voucherVM.Narration,
                                TrnNature = TransactionNature.ToGL.ToString()
                            }, currentVoucherDetailId);

                            // INSERT INTO VoucherDetailCurrency
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                                CrAmount = 0
                            });

                            totalAmountDr += voucherDetailDr.DrAmount;
                            totalAmountCr += voucherDetailDr.CrAmount;
                        }
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion Bank Payment

        public string InsertBankReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                if (voucherVM.BankMasterId == voucherVM.OtherBankMasterId)
                    throw new CustomException("Same to same bank transfer is not allowed!");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                var bankJournal = InsertBankJournal(new BankJournal
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    IsPark = voucherVM.IsPark,
                    SourceType = SourceType.ReceiptByBank.ToString(),
                    PaymentSource = PaymentSource.Bank.ToString(),
                    BankJournalType = voucherVM.BankJournalType,
                    Amount = voucherVM.Amount,
                    IsReverse = voucherVM.IsReverse,
                    BankMasterId = voucherVM.BankMasterId,
                    EmployeeId = voucherVM.EmployeeId,
                    EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType
                });

                // INSERT INTO Voucher TABLE
                var voucher = _voucherService.InsertVoucher(new Voucher
                {
                    CompanyGroupId = bankJournal.CompanyGroupId,
                    CompanyId = bankJournal.CompanyId,
                    PlantId = bankJournal.PlantId,
                    EntityId = bankJournal.EntityId,
                    CurrencyId = bankJournal.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = bankJournal.PostingDate,
                    DocDate = bankJournal.DocDate,
                    DocRefNo = bankJournal.DocRefNo,
                    Narration = bankJournal.Narration,
                    SourceType = bankJournal.SourceType,
                    AddedBy = bankJournal.AddedBy,
                    AddedFromIP = bankJournal.AddedFromIP,
                    AddedDate = bankJournal.AddedDate,
                    Archive = bankJournal.Archive,
                    IsPark = bankJournal.IsPark
                }, voucherVM.FiscalYearPrefix);

                // Set VoucherId in BankJournal
                bankJournal.VoucherId = voucher.Id;

                var currentVoucherDetailId = 1;
                var bankMaster = _bankMasterRepository.Find(bankJournal.BankMasterId);
                // INSERT INTO VoucherDetail Debit
                var voucherDetailDr = new VoucherDetail
                {
                    GLGeneralInfoId = bankMaster.GLGeneralInfoId,
                    BudgetMasterId = bankMaster.BudgetMasterId,
                    ActivityId = bankMaster.ActivityId,
                    PartyType = PartyType.Bank.ToString(),
                    BankMasterId = bankJournal.BankMasterId,
                    PaymentSource = bankJournal.PaymentSource,
                    TrnNature = TransactionNature.Bank.ToString(),
                    DrAmount = bankJournal.Amount
                };
                if (bankJournal.BankJournalType != BankJournalType.BankToGL.ToString() && null != voucherDetailVMList)
                {
                    voucherDetailDr.DrAmount += voucherDetailVMList.Sum(r => r.Amount);
                    bankJournal.Amount = voucherDetailDr.DrAmount;
                }
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                // INSRT INTO GLTransactionDetail (Bank)
                _voucherService.InsertGLTransactionDetail(voucherDetailDr, new GLTransactionDetail
                {
                    SourceType = voucher.SourceType,
                    BankMasterId = voucherDetailDr.BankMasterId,
                    DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate
                });

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                });

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetailDr.DrAmount;
                var totalAmountCr = voucherDetailDr.CrAmount;

                var currentBankJournalDetailId = 0;
                if (bankJournal.BankJournalType != BankJournalType.BankToGL.ToString())
                {
                    // INSERT INTO BankJournalDetail
                    currentBankJournalDetailId++;
                    var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                    {
                        BankJournalId = bankJournal.Id,
                        Amount = voucherVM.Amount
                    }, currentBankJournalDetailId);

                    var voucherDetailCr = new VoucherDetail
                    {
                        CurrencyId = voucher.CurrencyId,
                        CrAmount = voucherVM.Amount,
                        PaymentSource = bankJournal.PaymentSource,
                        BankJournalDetailId = bankJournalDetail.Id,
                        Narration = voucherVM.Narration
                    };

                    var glTransactionDetailCr = new GLTransactionDetail
                    {
                        SourceType = voucherDetailCr.PaymentSource,
                        CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate,
                    };

                    if (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherBankMasterId))
                            throw new CustomException("To Bank account is null.");

                        var otherBankMaster = _bankMasterRepository.Find(voucherVM.OtherBankMasterId);

                        bankJournalDetail.PartyType = PartyType.Bank.ToString();
                        bankJournalDetail.BankMasterId = voucherVM.OtherBankMasterId;
                        bankJournalDetail.GLGeneralInfoId = otherBankMaster.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = otherBankMaster.BudgetMasterId;
                        bankJournalDetail.ActivityId = otherBankMaster.ActivityId;

                        voucherDetailCr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailCr.BankMasterId = bankJournalDetail.BankMasterId;
                        voucherDetailCr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailCr.TrnNature = TransactionNature.ToBank.ToString();

                        glTransactionDetailCr.SourceType = voucher.SourceType;
                        glTransactionDetailCr.BankMasterId = voucherDetailCr.BankMasterId;
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToCash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherCashMasterId))
                            throw new CustomException("To Cash is null.");

                        var otherCashMaster = _cashMasterRepository.Find(voucherVM.OtherCashMasterId);

                        bankJournalDetail.PartyType = PartyType.Cash.ToString();
                        bankJournalDetail.CashMasterId = voucherVM.OtherCashMasterId;
                        bankJournalDetail.GLGeneralInfoId = otherCashMaster.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = otherCashMaster.BudgetMasterId;
                        bankJournalDetail.ActivityId = otherCashMaster.ActivityId;

                        voucherDetailCr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailCr.CashMasterId = bankJournalDetail.CashMasterId;
                        voucherDetailCr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailCr.TrnNature = TransactionNature.ToCash.ToString();

                        glTransactionDetailCr.SourceType = voucher.SourceType;
                        glTransactionDetailCr.CashMasterId = voucherDetailCr.CashMasterId;
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToEmployee.ToString())
                    {
                        var payableGL = _employeeTransactionTypeGLService.GetEmployeePayableGL(bankJournal.CompanyId, bankJournal.EmployeeTransactionTypeId);

                        bankJournalDetail.EmployeeTransactionTypeId = bankJournal.EmployeeTransactionTypeId;
                        bankJournalDetail.PartyType = PartyType.Employee.ToString();
                        bankJournalDetail.EmployeeId = bankJournal.EmployeeId;
                        bankJournalDetail.GLGeneralInfoId = payableGL.PayableGLId;
                        bankJournalDetail.BudgetMasterId = payableGL.PayableBudgetMasterId;
                        bankJournalDetail.ActivityId = payableGL.PayableActivityId;

                        voucherDetailCr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailCr.EmployeeId = bankJournalDetail.EmployeeId;
                        voucherDetailCr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailCr.TrnNature = TransactionNature.ToEmployee.ToString();
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToCustomer.ToString())
                    {
                        bankJournalDetail.PartyType = PartyType.Customer.ToString();
                        bankJournalDetail.PartyId = bankJournal.PartyId;
                        bankJournalDetail.PartyPlantId = bankJournal.PartyPlantId;
                        var companyParty = _companyPartyRepository.Query(r => r.CompanyId == bankJournal.CompanyId && r.PlantId == bankJournal.PlantId && r.PartyId == bankJournalDetail.PartyId && r.PartyType == bankJournalDetail.PartyType).Select().FirstOrDefault();
                        if (null == companyParty)
                            throw new CustomException($"Plant {bankJournalDetail.PartyType} mapping not found!");

                        var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                        if (null == companyPartyGLList)
                            throw new CustomException($"{bankJournalDetail.PartyType} GL not found!");

                        var reconGL = PartyGLType.ReconciliationGL.ToString();
                        var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                        if (null == regularGL)
                            throw new CustomException($"{bankJournalDetail.PartyType} Reconciliation GL not found!");

                        bankJournalDetail.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = regularGL.BudgetMasterId;
                        bankJournalDetail.ActivityId = regularGL.ActivityId;

                        voucherDetailCr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailCr.PartyId = bankJournalDetail.PartyId;
                        voucherDetailCr.PartyPlantId = bankJournalDetail.PartyPlantId;
                        voucherDetailCr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailCr.TrnNature = TransactionNature.ToCustomer.ToString();
                    }

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetailCr);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount
                    });

                    // Set Dr/Cr amount to local variable.
                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.BankToGL.ToString())
                {
                    if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                        throw new CustomException("GL list not found!");
                }

                if (null != voucherDetailVMList)
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        // INSERT INTO BankJournalDetail
                        currentBankJournalDetailId++;
                        var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = voucherDetailVM.Amount,
                            PartyType = PartyType.GL.ToString(),
                        }, currentBankJournalDetailId);

                        currentVoucherDetailId++;
                        var voucherDetailCr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankJournalDetailId = bankJournalDetail.Id,
                            GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId,
                            BudgetMasterId = bankJournalDetail.BudgetMasterId,
                            ActivityId = bankJournalDetail.ActivityId,
                            CrAmount = bankJournalDetail.Amount,
                            PaymentSource = bankJournal.PaymentSource,
                            Narration = voucherVM.Narration,
                            TrnNature = TransactionNature.ToGL.ToString(),
                            PartyType = PartyType.GL.ToString(),
                        }, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount
                        });

                        totalAmountDr += voucherDetailCr.DrAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public string UpdateBankReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                if (voucherVM.BankMasterId == voucherVM.OtherBankMasterId)
                    throw new CustomException("Same to same bank transfer is not allowed!");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                voucherVM.SourceType = SourceType.ReceiptByBank.ToString();

                var bankJournal = FindBankJournal(voucherVM.BankJournalId);
                if (null == bankJournal)
                    throw new CustomException("Bank journal master data not found.");

                // Update validation after Posted.
                CheckIsPosted(bankJournal);

                bankJournal.CurrencyId = voucherVM.CurrencyId;
                bankJournal.EntityId = voucherVM.EntityId;
                bankJournal.DocRefNo = voucherVM.DocRefNo;
                bankJournal.PostingDate = voucherVM.PostingDate;
                bankJournal.Narration = voucherVM.Narration;
                bankJournal.Amount = voucherVM.Amount;
                bankJournal.IsReverse = voucherVM.IsReverse;
                bankJournal.BankMasterId = voucherVM.BankMasterId;
                bankJournal.CashMasterId = voucherVM.CashMasterId;
                bankJournal.PartyType = voucherVM.PartyType;

                var voucher = _voucherService.FindVoucher(bankJournal.VoucherId);
                voucher.CurrencyId = bankJournal.CurrencyId;
                voucher.EntityId = bankJournal.EntityId;
                voucher.DocRefNo = bankJournal.DocRefNo;
                voucher.Narration = bankJournal.Narration;
                voucher.UpdatedBy = bankJournal.UpdatedBy;
                voucher.UpdatedDate = bankJournal.UpdatedDate;
                voucher.UpdatedFromIP = bankJournal.UpdatedFromIP;

                var bankMaster = _bankMasterRepository.Find(bankJournal.BankMasterId);
                var voucherDetailDr = _voucherService.FindVoucherDetailForBankJournalDetail(bankJournal.VoucherId);

                voucherDetailDr.CurrencyId = voucher.CurrencyId;
                voucherDetailDr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                voucherDetailDr.BudgetMasterId = bankMaster.BudgetMasterId;
                voucherDetailDr.ActivityId = bankMaster.ActivityId;
                voucherDetailDr.BankMasterId = bankJournal.BankMasterId;
                voucherDetailDr.DrAmount = bankJournal.Amount;
                voucherDetailDr.PaymentSource = bankJournal.PaymentSource;
                voucherDetailDr.PartyType = bankJournal.PartyType;
                voucherDetailDr.TrnNature = TransactionNature.Bank.ToString();

                if (bankJournal.BankJournalType != BankJournalType.BankToGL.ToString() && null != voucherDetailVMList)
                {
                    voucherDetailDr.DrAmount += voucherDetailVMList.Sum(r => r.Amount);
                    bankJournal.Amount = voucherDetailDr.DrAmount;
                }

                UpdateBankJournal(bankJournal);//Bank Journal Update
                _voucherService.UpdateVoucher(voucher);//Voucher Update
                _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);//Voucher Detail (Cr) Update.

                if (voucherDetailDr.CashMasterId != null || voucherDetailDr.BankMasterId != null)
                {
                    var glTransactionDetail = _voucherService.FindGLTransactionDetail(voucherDetailDr.Id);
                    glTransactionDetail.DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate;
                    _voucherService.UpdateGLTransactionDetail(voucherDetailDr, glTransactionDetail);
                }

                var voucherDetailCurrency = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucherDetailDr.VoucherId && r.VoucherDetailId == voucherDetailDr.Id).Select().FirstOrDefault();
                if (null != voucherDetailCurrency)
                {
                    voucherDetailCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                    voucherDetailCurrency.ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate;
                    voucherDetailCurrency.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                    voucherDetailCurrency.CrAmount = 0;
                    _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrency);
                }

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetailDr.DrAmount;
                var totalAmountCr = voucherDetailDr.CrAmount;

                // INSERT INTO Debit Side
                if (bankJournal.BankJournalType != BankJournalType.BankToGL.ToString())
                {
                    var bankJournalDetail = _bankJournalDetailRepository.Query(r => r.Id == voucherVM.BankJournalDetailId).Select().FirstOrDefault();
                    bankJournalDetail.Amount = voucherVM.Amount;
                    bankJournalDetail.UpdatedBy = bankJournal.UpdatedBy;
                    bankJournalDetail.UpdatedDate = bankJournal.UpdatedDate;
                    bankJournalDetail.UpdatedFromIP = bankJournal.UpdatedFromIP;
                    bankJournalDetail.CashMasterId = voucherVM.OtherCashMasterId;
                    bankJournalDetail.BankMasterId = voucherVM.OtherBankMasterId;

                    bankJournalDetail.EmployeeId = voucherVM.EmployeeId;
                    bankJournalDetail.PartyType = voucherVM.PartyType;
                    bankJournalDetail.UpdatedFromIP = bankJournal.UpdatedFromIP;
                    _bankJournalDetailRepository.Update(bankJournalDetail);//Update BankJournal Update

                    var voucherDetailCr = _voucherService.FindVoucherDetail(voucherVM.VoucherDetailId);
                    voucherDetailCr.CurrencyId = voucher.CurrencyId;
                    voucherDetailCr.CrAmount = bankJournalDetail.Amount;
                    voucherDetailCr.PaymentSource = bankJournal.PaymentSource;
                    voucherDetailCr.BankJournalDetailId = bankJournalDetail.Id;
                    voucherDetailCr.Narration = voucherVM.Narration;
                    voucherDetailCr.PartyId = null;
                    voucherDetailCr.PartyPlantId = null;
                    voucherDetailCr.EmployeeId = null;
                    voucherDetailCr.Narration = voucherVM.Narration;

                    if (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherBankMasterId))
                            throw new CustomException("To Bank account is null.");

                        var otherBankMaster = _bankMasterRepository.Find(voucherVM.OtherBankMasterId);

                        bankJournalDetail.PartyType = PartyType.Bank.ToString();
                        bankJournalDetail.BankMasterId = voucherVM.OtherBankMasterId;
                        bankJournalDetail.GLGeneralInfoId = otherBankMaster.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = otherBankMaster.BudgetMasterId;
                        bankJournalDetail.ActivityId = otherBankMaster.ActivityId;

                        voucherDetailCr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailCr.BankMasterId = bankJournalDetail.BankMasterId;
                        voucherDetailCr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailCr.TrnNature = TransactionNature.ToBank.ToString();
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToCash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.OtherCashMasterId))
                            throw new CustomException("To Cash is null.");

                        var otherCashMaster = _cashMasterRepository.Find(voucherVM.OtherCashMasterId);

                        bankJournalDetail.PartyType = PartyType.Cash.ToString();
                        bankJournalDetail.CashMasterId = voucherVM.OtherCashMasterId;
                        bankJournalDetail.GLGeneralInfoId = otherCashMaster.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = otherCashMaster.BudgetMasterId;
                        bankJournalDetail.ActivityId = otherCashMaster.ActivityId;

                        voucherDetailCr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailCr.CashMasterId = bankJournalDetail.CashMasterId;
                        voucherDetailCr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailCr.TrnNature = TransactionNature.ToCash.ToString();
                    }
                    if (voucherVM.OtherBankMasterId != null || voucherVM.OtherCashMasterId != null)
                    {
                        var glTransactionDetailCr = _voucherService.FindGLTransactionDetail(voucherDetailCr.Id);
                        if (glTransactionDetailCr != null)
                        {
                            glTransactionDetailCr.SourceType = voucher.SourceType;
                            glTransactionDetailCr.CashMasterId = voucherDetailCr.CashMasterId;
                            glTransactionDetailCr.BankMasterId = voucherDetailCr.BankMasterId;
                            glTransactionDetailCr.CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate;
                            glTransactionDetailCr.DrAmount = 0;
                            _voucherService.UpdateGLTransactionDetail(voucherDetailCr, glTransactionDetailCr);
                        }
                        else
                            _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetailCr);
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToEmployee.ToString())
                    {
                        var payableGL = _employeeTransactionTypeGLService.GetEmployeePayableGL(bankJournal.CompanyId, bankJournal.EmployeeTransactionTypeId);

                        bankJournalDetail.EmployeeTransactionTypeId = bankJournal.EmployeeTransactionTypeId;
                        bankJournalDetail.PartyType = PartyType.Employee.ToString();
                        bankJournalDetail.EmployeeId = bankJournal.EmployeeId;
                        bankJournalDetail.GLGeneralInfoId = payableGL.PayableGLId;
                        bankJournalDetail.BudgetMasterId = payableGL.PayableBudgetMasterId;
                        bankJournalDetail.ActivityId = payableGL.PayableActivityId;
                        bankJournalDetail.PartyId = null;
                        bankJournalDetail.PartyPlantId = null;
                        bankJournalDetail.BankMasterId = null;
                        bankJournalDetail.CashMasterId = null;

                        voucherDetailCr.PartyType = bankJournalDetail.PartyType;
                        voucherDetailCr.EmployeeId = bankJournalDetail.EmployeeId;
                        voucherDetailCr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailCr.PartyId = null;
                        voucherDetailCr.PartyPlantId = null;
                        voucherDetailCr.BankMasterId = null;
                        voucherDetailCr.CashMasterId = null;
                        voucherDetailCr.TrnNature = TransactionNature.ToEmployee.ToString();
                        var glTransactionDetailDr = _voucherService.FindGLTransactionDetail(voucherDetailCr.Id);
                        if (glTransactionDetailDr != null)
                        {
                            _voucherService.DeleteGLTransactionDetail(voucherDetailCr, glTransactionDetailDr);
                        }
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToVendor.ToString())
                    {
                        bankJournalDetail.PartyType = PartyType.Vendor.ToString();
                        bankJournalDetail.PartyId = bankJournal.PartyId;
                        bankJournalDetail.PartyPlantId = bankJournal.PartyPlantId;
                        var companyParty = _companyPartyRepository.Query(r => r.CompanyId == bankJournal.CompanyId && r.PlantId == bankJournal.PlantId && r.PartyId == bankJournalDetail.PartyId && r.PartyType == bankJournalDetail.PartyType).Select().FirstOrDefault();
                        if (null == companyParty)
                            throw new CustomException($"Plant {bankJournalDetail.PartyType} mapping not found!");

                        var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                        if (null == companyPartyGLList)
                            throw new CustomException($"{bankJournalDetail.PartyType} GL not found!");

                        var reconGL = PartyGLType.ReconciliationGL.ToString();
                        var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                        if (null == regularGL)
                            throw new CustomException($"{bankJournalDetail.PartyType} Reconciliation GL not found!");

                        bankJournalDetail.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                        bankJournalDetail.BudgetMasterId = regularGL.BudgetMasterId;
                        bankJournalDetail.ActivityId = regularGL.ActivityId;
                        bankJournalDetail.EmployeeId = null;
                        bankJournalDetail.BankMasterId = null;
                        bankJournalDetail.CashMasterId = null;
                        bankJournalDetail.PartyType = voucherVM.PartyType;
                        bankJournalDetail.PartyId = voucherVM.PartyId;
                        bankJournalDetail.PartyPlantId = voucherVM.PartyPlantId;

                        voucherDetailCr.PartyType = voucherVM.PartyType;
                        voucherDetailCr.PartyId = voucherVM.PartyId;
                        voucherDetailCr.PartyPlantId = voucherVM.PartyPlantId;
                        voucherDetailCr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = bankJournalDetail.ActivityId;
                        voucherDetailCr.BankMasterId = null;
                        voucherDetailCr.CashMasterId = null;
                        voucherDetailCr.EmployeeId = null;
                        voucherDetailCr.TrnNature = TransactionNature.ToVendor.ToString();
                        var glTransactionDetailDr = _voucherService.FindGLTransactionDetail(voucherDetailCr.Id);
                        if (glTransactionDetailDr != null)
                        {
                            _voucherService.DeleteGLTransactionDetail(voucherDetailCr, glTransactionDetailDr);
                        }
                    }

                    _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);//Voucher Detail (Dr) Update.

                    var voucherDetailCurrencyDr = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucherDetailCr.VoucherId && r.VoucherDetailId == voucherDetailCr.Id).Select().FirstOrDefault();
                    if (null != voucherDetailCurrency)
                    {
                        voucherDetailCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyDr.ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyDr.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        voucherDetailCurrencyDr.DrAmount = 0;
                        _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyDr);
                    }
                    // Set Dr/Cr amount to local variable.
                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.BankToGL.ToString())
                {
                    if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                        throw new CustomException("GL list not found!");
                }

                if (null != voucherDetailVMList)
                {
                    var currentBankJournalDetailId = GetBankJournalDetailPK(bankJournal.Id);
                    var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);

                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        if (voucherDetailVM.VoucherDetailId != null)
                        {
                            var bankJournalDetailDr = _bankJournalDetailRepository.Query(r => r.Id == voucherDetailVM.BankJournalDetailId).Select().FirstOrDefault();
                            bankJournalDetailDr.Amount = voucherDetailVM.Amount;
                            bankJournalDetailDr.UpdatedBy = bankJournal.UpdatedBy;
                            bankJournalDetailDr.UpdatedDate = bankJournal.UpdatedDate;
                            bankJournalDetailDr.UpdatedFromIP = bankJournal.UpdatedFromIP;
                            bankJournalDetailDr.PartyType = PartyType.GL.ToString();
                            bankJournalDetailDr.UpdatedFromIP = bankJournal.UpdatedFromIP;
                            _bankJournalDetailRepository.Update(bankJournalDetailDr);//Update BankJournalDetail Update

                            var voucherDetailCr = _voucherService.FindVoucherDetail(voucherDetailVM.VoucherDetailId);
                            voucherDetailCr.CurrencyId = voucher.CurrencyId;
                            voucherDetailCr.CrAmount = voucherDetailVM.Amount;
                            voucherDetailCr.PaymentSource = bankJournal.PaymentSource;
                            voucherDetailCr.BankJournalDetailId = voucherDetailVM.BankJournalDetailId;
                            voucherDetailCr.Narration = voucherVM.Narration;
                            voucherDetailCr.PartyId = voucherVM.PartyId;
                            voucherDetailCr.PartyPlantId = voucherVM.PartyPlantId;
                            voucherDetailCr.EmployeeId = voucherVM.EmployeeId;
                            voucherDetailCr.Narration = voucherVM.Narration;
                            _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);//Voucher Detail (Dr) Update.

                            var voucherDetailCurrencyCr = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucherDetailCr.VoucherId && r.VoucherDetailId == voucherDetailCr.Id).Select().FirstOrDefault();
                            if (null != voucherDetailCurrency)
                            {
                                voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                                voucherDetailCurrencyCr.ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate;
                                voucherDetailCurrencyCr.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                                voucherDetailCurrencyCr.DrAmount = 0;
                                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                            }
                            totalAmountDr += voucherDetailCr.DrAmount;
                            totalAmountCr += voucherDetailCr.CrAmount;
                        }
                        else
                        {
                            // INSERT INTO BankJournalDetail
                            currentBankJournalDetailId++;
                            var bankJournalDetail = InsertBankJournalDetail(bankJournal, new BankJournalDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount,
                                PartyType = PartyType.GL.ToString()
                            }, currentBankJournalDetailId);

                            currentVoucherDetailId++;
                            var voucherDetailCr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankJournalDetailId = bankJournalDetail.Id,
                                GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId,
                                BudgetMasterId = bankJournalDetail.BudgetMasterId,
                                ActivityId = bankJournalDetail.ActivityId,
                                CrAmount = bankJournalDetail.Amount,
                                PaymentSource = bankJournal.PaymentSource,
                                PartyType = PartyType.GL.ToString(),
                                Narration = voucherVM.Narration,
                                TrnNature = TransactionNature.ToGL.ToString(),
                                CurrencyId = voucher.CurrencyId
                            }, currentVoucherDetailId);

                            // INSERT INTO VoucherDetailCurrency
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailCr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount,
                                DrAmount = 0
                            });

                            totalAmountDr += voucherDetailCr.DrAmount;
                            totalAmountCr += voucherDetailCr.CrAmount;
                        }
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void UpdateBankJournal(BankJournal bankJournal)
        {
            AuditService.UpdatedLog(bankJournal);
            _bankJournalRepository.Update(bankJournal);
        }

        public string UpdateBankJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                if (voucherVM.BankMasterId == voucherVM.OtherBankMasterId)
                    throw new CustomException("Same to same bank transfer is not allowed.");
                if (voucherVM.Amount <= 0)
                    throw new CustomException("Amount is 0.");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.SourceType = SourceType.BankJournal.ToString();

                var bankJournal = FindBankJournal(voucherVM.Id);
                if (null == bankJournal)
                    throw new CustomException("Bank journal master data not found.");

                // Update validation after Posted.
                CheckIsPosted(bankJournal);

                bankJournal.EntityId = voucherVM.EntityId;
                bankJournal.PostingDate = voucherVM.PostingDate;
                bankJournal.DocDate = voucherVM.DocDate;
                bankJournal.DocRefNo = voucherVM.DocRefNo;
                bankJournal.Narration = voucherVM.Narration;
                bankJournal.Amount = voucherVM.Amount;
                bankJournal.IsReverse = voucherVM.IsReverse;
                bankJournal.BankMasterId = voucherVM.BankMasterId;
                UpdateBankJournal(bankJournal);

                var voucher = _voucherService.FindVoucher(bankJournal.VoucherId);
                voucher.EntityId = bankJournal.EntityId;
                voucher.DocDate = bankJournal.DocDate;
                voucher.PostingDate = bankJournal.PostingDate;
                voucher.DocRefNo = bankJournal.DocRefNo;
                voucher.Narration = bankJournal.Narration;
                voucher.UpdatedBy = bankJournal.UpdatedBy;
                voucher.UpdatedDate = bankJournal.UpdatedDate;
                voucher.UpdatedFromIP = bankJournal.UpdatedFromIP;
                _voucherService.UpdateVoucher(voucher);

                var voucherDetailList = _voucherService.GetVoucherDetailList(r => r.VoucherId == voucher.Id).Select().ToList();
                var voucherDetailCurrencyList = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id).Select().ToList();

                // UPDATE VoucherDetail Debit/Credit
                var voucherDetail = voucherDetailList.FirstOrDefault(r => r.BankMasterId == bankJournal.BankMasterId);
                voucherDetail.DocDate = voucher.DocDate;
                voucherDetail.DocRefNo = voucher.DocRefNo;
                voucherDetail.Narration = voucher.Narration;
                voucherDetail.BankMasterId = voucherVM.BankMasterId;
                voucherDetail.DrAmount = bankJournal.BankJournalType == BankJournalType.ProfitEarn.ToString() ? bankJournal.Amount : 0;
                voucherDetail.CrAmount = bankJournal.BankJournalType != BankJournalType.ProfitEarn.ToString() ? bankJournal.Amount : 0;

                if (bankJournal.IsReverse &&
                    (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString()
                    || bankJournal.BankJournalType == BankJournalType.BankToCash.ToString()
                    || bankJournal.BankJournalType == BankJournalType.BankCharge.ToString()))
                {
                    voucherDetail.DrAmount = bankJournal.Amount;
                    voucherDetail.CrAmount = 0;
                }
                else if (bankJournal.IsReverse && bankJournal.BankJournalType == BankJournalType.ProfitEarn.ToString())
                {
                    voucherDetail.CrAmount = bankJournal.Amount;
                    voucherDetail.DrAmount = 0;
                }
                else if (!bankJournal.IsReverse &&
                   (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString()
                   || bankJournal.BankJournalType == BankJournalType.BankToCash.ToString()
                   || bankJournal.BankJournalType == BankJournalType.BankCharge.ToString()))
                {
                    voucherDetail.CrAmount = bankJournal.Amount;
                    voucherDetail.DrAmount = 0;
                }
                else if (!bankJournal.IsReverse && bankJournal.BankJournalType == BankJournalType.ProfitEarn.ToString())
                {
                    voucherDetail.DrAmount = bankJournal.Amount;
                    voucherDetail.CrAmount = 0;
                }
                _voucherService.UpdateVoucherDetail(voucher, voucherDetail);

                var glTransactionDetail = _voucherService.FindGLTransactionDetail(voucherDetail.Id);
                glTransactionDetail.BankMasterId = voucherDetail.BankMasterId;
                glTransactionDetail.DrAmount = voucherDetail.DrAmount * voucherVM.CompanyCurrencyRate;
                glTransactionDetail.CrAmount = voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate;
                _voucherService.UpdateGLTransactionDetail(voucherDetail, glTransactionDetail);

                var voucherDetailCompanyCurrency = voucherDetailCurrencyList.FirstOrDefault(r => r.VoucherDetailId == voucherDetail.Id && r.ParallelCurrencyId == companyCurrencyId);
                voucherDetailCompanyCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                voucherDetailCompanyCurrency.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                voucherDetailCompanyCurrency.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                voucherDetailCompanyCurrency.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetail, voucherDetailCompanyCurrency);

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetail.DrAmount;
                var totalAmountCr = voucherDetail.CrAmount;

                if (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString() ||
                    bankJournal.BankJournalType == BankJournalType.BankToCash.ToString())
                {
                    var bankJournalDetail = _bankJournalDetailRepository.Query(r => r.BankJournalId == bankJournal.Id).Select().FirstOrDefault();
                    var existingBankMasterId = bankJournalDetail.BankMasterId;
                    var existingCashMasterId = bankJournalDetail.CashMasterId;
                    bankJournalDetail.Amount = bankJournal.Amount;
                    bankJournalDetail.BankMasterId = voucherVM.OtherBankMasterId;
                    bankJournalDetail.CashMasterId = voucherVM.OtherCashMasterId;
                    bankJournalDetail.UpdatedBy = bankJournal.UpdatedBy;
                    bankJournalDetail.UpdatedDate = bankJournal.UpdatedDate;
                    bankJournalDetail.UpdatedFromIP = bankJournal.UpdatedFromIP;
                    _bankJournalDetailRepository.Update(bankJournalDetail);

                    if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        bankJournalDetail.Amount -= bankChargeDetailVMList.Sum(r => r.Amount);
                    if (bankJournal.BankJournalType == BankJournalType.BankToBank.ToString())
                    {
                        if (string.IsNullOrEmpty(bankJournalDetail.BankMasterId))
                            throw new CustomException("To Bank account is null.");

                        var voucherDetailDr = voucherDetailList.FirstOrDefault(r => r.BankMasterId == existingBankMasterId && r.BankJournalDetailId == bankJournalDetail.Id);
                        voucherDetailDr.DrAmount = bankJournalDetail.Amount;
                        voucherDetailDr.Narration = voucherVM.Narration;
                        voucherDetailDr.DocDate = voucherVM.DocDate;
                        voucherDetailDr.DocRefNo = voucherVM.DocRefNo;
                        voucherDetailDr.BankMasterId = voucherVM.OtherBankMasterId;

                        if (bankJournal.IsReverse)
                        {
                            voucherDetailDr.CrAmount = voucherDetailDr.DrAmount;
                            voucherDetailDr.DrAmount = 0;
                        }
                        else if (!bankJournal.IsReverse)
                        {
                            voucherDetailDr.DrAmount = voucherDetailDr.DrAmount;
                            voucherDetailDr.CrAmount = voucherDetailDr.CrAmount;
                        }

                        _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);

                        var glTransactionDetailDr = _voucherService.FindGLTransactionDetail(voucherDetailDr.Id);
                        glTransactionDetailDr.BankMasterId = voucherDetailDr.BankMasterId;
                        glTransactionDetailDr.DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate;
                        _voucherService.UpdateGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                        var voucherDetailCompanyCurrencyDr = voucherDetailCurrencyList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailDr.Id && r.ParallelCurrencyId == companyCurrencyId);
                        voucherDetailCompanyCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCompanyCurrencyDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                        voucherDetailCompanyCurrencyDr.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                        voucherDetailCompanyCurrencyDr.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount;
                        _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCompanyCurrencyDr);

                        totalAmountDr += voucherDetailDr.DrAmount;
                        totalAmountCr += voucherDetailDr.CrAmount;
                    }
                    else if (bankJournal.BankJournalType == BankJournalType.BankToCash.ToString())
                    {
                        if (string.IsNullOrEmpty(bankJournalDetail.CashMasterId))
                            throw new CustomException("To Cash is null.");

                        var voucherDetailDr = voucherDetailList.FirstOrDefault(r => r.CashMasterId == existingCashMasterId && r.BankJournalDetailId == bankJournalDetail.Id);
                        voucherDetailDr.DrAmount = bankJournalDetail.Amount;
                        voucherDetailDr.Narration = voucherVM.Narration;
                        voucherDetailDr.CashMasterId = voucherVM.OtherCashMasterId;
                        _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);

                        var glTransactionDetailDr = _voucherService.FindGLTransactionDetail(voucherDetailDr.Id);
                        glTransactionDetailDr.DrAmount = voucherDetailDr.DrAmount;
                        glTransactionDetailDr.CrAmount = voucherDetailDr.CrAmount;
                        glTransactionDetailDr.CashMasterId = voucherDetailDr.CashMasterId;
                        _voucherService.UpdateGLTransactionDetail(voucherDetailDr, glTransactionDetailDr);

                        var voucherDetailCompanyCurrencyDr = voucherDetailCurrencyList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailDr.Id && r.ParallelCurrencyId == companyCurrencyId);
                        voucherDetailCompanyCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCompanyCurrencyDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                        voucherDetailCompanyCurrencyDr.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                        voucherDetailCompanyCurrencyDr.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount;
                        _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCompanyCurrencyDr);

                        totalAmountDr += voucherDetailDr.DrAmount;
                        totalAmountCr += voucherDetailDr.CrAmount;
                        existingCashMasterId = null;
                        existingBankMasterId = null;
                    }
                }
                else if (bankJournal.BankJournalType == BankJournalType.ProfitEarn.ToString())
                {
                    var bankJournalDetail = _bankJournalDetailRepository.Query(r => r.BankJournalId == bankJournal.Id).Select().FirstOrDefault();
                    bankJournalDetail.Amount = bankJournal.Amount;
                    bankJournalDetail.UpdatedBy = bankJournal.UpdatedBy;
                    bankJournalDetail.UpdatedDate = bankJournal.UpdatedDate;
                    bankJournalDetail.UpdatedFromIP = bankJournal.UpdatedFromIP;
                    _bankJournalDetailRepository.Update(bankJournalDetail);

                    var voucherDetailCr = voucherDetailList.FirstOrDefault(r => r.BankMasterId == bankJournalDetail.BankMasterId && r.BankJournalDetailId == bankJournalDetail.Id);
                    voucherDetailCr.CrAmount = bankJournalDetail.Amount;
                    voucherDetailCr.Narration = voucherVM.Narration;

                    if (bankJournal.IsReverse)
                    {
                        voucherDetailCr.DrAmount = voucherDetailCr.CrAmount;
                        voucherDetailCr.CrAmount = 0;
                    }
                    else if (!bankJournal.IsReverse)
                    {
                        voucherDetailCr.CrAmount = voucherDetailCr.DrAmount;
                        voucherDetailCr.DrAmount = 0;
                    }

                    _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);

                    var voucherDetailCompanyCurrencyCr = voucherDetailCurrencyList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailCr.Id && r.ParallelCurrencyId == companyCurrencyId);
                    voucherDetailCompanyCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                    voucherDetailCompanyCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                    voucherDetailCompanyCurrencyCr.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.DrAmount;
                    voucherDetailCompanyCurrencyCr.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                    _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCompanyCurrencyCr);

                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;
                }
                else if (bankJournal.BankJournalType == BankJournalType.CashExpense.ToString())
                {
                    if (null == voucherDetailVMList && voucherDetailVMList.Count() < 0)
                        throw new CustomException("Expense GL list not found!");
                    var currentBankJournalDetailId = GetBankJournalDetailPK(bankJournal.Id);
                    var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);

                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        var bankJournalDetail = FindBankJournalDetail(voucherDetailVM.Id);
                        if (null == bankJournalDetail)
                        {
                            // INSERT INTO BankJournalDetail
                            currentBankJournalDetailId++;
                            InsertBankJournalDetail(bankJournal, new BankJournalDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                Amount = voucherDetailVM.Amount
                            }, currentBankJournalDetailId);

                            var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankJournalDetailId = bankJournalDetail.Id,
                                GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId,
                                BudgetMasterId = bankJournalDetail.BudgetMasterId,
                                ActivityId = bankJournalDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = bankJournalDetail.Amount,
                                PaymentSource = bankJournal.PaymentSource,
                                PartyType = bankJournal.PaymentSource,
                                DocRefNo = voucherVM.DocRefNo,
                                DocDate = voucherVM.DocDate,
                                Narration = voucherVM.Narration,
                                TrnNature = TransactionNature.ToExpense.ToString()
                            }, currentVoucherDetailId);

                            // INSERT INTO VoucherDetailCurrency
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                            });

                            totalAmountDr += voucherDetailDr.DrAmount;
                            totalAmountCr += voucherDetailDr.CrAmount;
                        }
                        else
                        {
                            bankJournalDetail.Amount = bankJournal.Amount;
                            bankJournalDetail.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                            bankJournalDetail.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                            bankJournalDetail.ActivityId = voucherDetailVM.ActivityId;
                            bankJournalDetail.Amount = voucherDetailVM.Amount;
                            UpdateBankJournalDetail(bankJournal, bankJournalDetail);

                            var voucherDetailDr = voucherDetailList.FirstOrDefault(r => r.Id == voucherDetailVM.VoucherDetailId);
                            if (null == voucherDetailDr)
                                throw new CustomException("Voucher Detail(For Expense) data not found!");

                            voucherDetailDr.GLGeneralInfoId = bankJournalDetail.GLGeneralInfoId;
                            voucherDetailDr.BudgetMasterId = bankJournalDetail.BudgetMasterId;
                            voucherDetailDr.ActivityId = bankJournalDetail.ActivityId;
                            voucherDetailDr.DrAmount = bankJournalDetail.Amount;
                            voucherDetailDr.DocRefNo = voucherVM.DocRefNo;
                            voucherDetailDr.DocDate = voucherVM.DocDate;
                            voucherDetailDr.Narration = voucherVM.Narration;
                            _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);

                            var voucherDetailCompanyCurrencyDr = voucherDetailCurrencyList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailDr.Id && r.ParallelCurrencyId == companyCurrencyId);
                            voucherDetailCompanyCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                            voucherDetailCompanyCurrencyDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                            voucherDetailCompanyCurrencyDr.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                            _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCompanyCurrencyDr);

                            totalAmountDr += voucherDetailDr.DrAmount;
                            totalAmountCr += voucherDetailDr.CrAmount;
                        }
                    }
                }
                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var bankChargeList = _bankChargeService.GetBankChargeList(r => r.BankJournalId == bankJournal.Id).Select().ToList();
                    var currentBankChargeId = _bankChargeService.GetBankChargePKForBankJournal(bankJournal.Id);
                    var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        if (string.IsNullOrEmpty(bankChargeDetailVM.Id))
                        {
                            currentBankChargeId++;
                            var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                            {
                                BankJournalId = bankJournal.Id,
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                BankMasterId = bankJournal.BankMasterId,
                                CashMasterId = bankJournal.CashMasterId,
                                Amount = bankChargeDetailVM.Amount,
                                Narration = bankJournal.Narration,
                                AddedBy = bankJournal.AddedBy,
                                AddedDate = bankJournal.AddedDate,
                                AddedFromIP = bankJournal.AddedFromIP,
                                Archive = bankJournal.Archive,
                                SourceType = bankJournal.SourceType
                            }, currentBankChargeId);

                            // Get Expense GL
                            var expenseGL = _bankChargeService.GetExpensesGL(bankJournal.CompanyId, bankCharge.FinancingTypeId);

                            // Insert Bank charges Debit
                            currentVoucherDetailId++;
                            var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankChargeId = bankCharge.Id,
                                GLGeneralInfoId = expenseGL.ExpensesGLId,
                                BudgetMasterId = expenseGL.ExpensesBudgetMasterId,
                                ActivityId = expenseGL.ExpensesActivityId,
                                DrAmount = bankCharge.Amount,
                                Narration = bankCharge.Narration,
                                DocDate = bankJournal.DocDate,
                                DocRefNo = bankJournal.DocRefNo,
                                PartyType = bankJournal.PaymentSource,
                                PaymentSource = bankJournal.PaymentSource,
                                TrnNature = TransactionNature.Charge.ToString()
                            }, currentVoucherDetailId);

                            if (bankJournal.IsReverse)
                            {
                                voucherDetailChargeDr.CrAmount = voucherDetailChargeDr.DrAmount;
                                voucherDetailChargeDr.DrAmount = 0;
                            }
                            else if (!bankJournal.IsReverse)
                            {
                                voucherDetailChargeDr.DrAmount = voucherDetailChargeDr.CrAmount;
                                voucherDetailChargeDr.CrAmount = 0;
                            }

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailChargeDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = bankJournal.IsReverse ? 0 : bankChargeDetailVM.CompanyCurrencyAmount,
                                CrAmount = !bankJournal.IsReverse ? 0 : bankChargeDetailVM.CompanyCurrencyAmount
                            });

                            // Set Dr/Cr amount to local variable.
                            totalAmountDr += voucherDetailChargeDr.DrAmount;
                            totalAmountCr += voucherDetailChargeDr.CrAmount;
                        }
                        else
                        {
                            var bankCharge = bankChargeList.FirstOrDefault(r => r.Id == bankChargeDetailVM.Id);
                            if (null != bankCharge)
                            {
                                bankCharge.FinancingTypeId = bankChargeDetailVM.FinancingTypeId;
                                bankCharge.Amount = bankChargeDetailVM.Amount;
                                bankCharge.Amount = bankChargeDetailVM.Amount;
                                bankCharge.Narration = bankJournal.Narration;
                                bankCharge.UpdatedBy = bankJournal.UpdatedBy;
                                bankCharge.UpdatedDate = bankJournal.UpdatedDate;
                                bankCharge.UpdatedFromIP = bankJournal.UpdatedFromIP;
                                _bankChargeService.UpdateBankCharge(bankCharge);

                                // Get Expense GL
                                var expenseGL = _bankChargeService.GetExpensesGL(bankJournal.CompanyId, bankCharge.FinancingTypeId);

                                var voucherDetailChargeDr = voucherDetailList.FirstOrDefault(r => r.BankChargeId == bankCharge.Id);
                                voucherDetailChargeDr.GLGeneralInfoId = expenseGL.ExpensesGLId;
                                voucherDetailChargeDr.BudgetMasterId = expenseGL.ExpensesBudgetMasterId;
                                voucherDetailChargeDr.ActivityId = expenseGL.ExpensesActivityId;
                                voucherDetailChargeDr.DrAmount = bankJournal.IsReverse ? 0 : bankCharge.Amount;
                                voucherDetailChargeDr.CrAmount = !bankJournal.IsReverse ? 0 : bankCharge.Amount;
                                voucherDetailChargeDr.DocRefNo = bankJournal.DocRefNo;
                                voucherDetailChargeDr.DocDate = bankJournal.DocDate;
                                voucherDetailChargeDr.Narration = bankCharge.Narration;
                                _voucherService.UpdateVoucherDetail(voucher, voucherDetailChargeDr);

                                var voucherDetailCompanyCurrencyCr = voucherDetailCurrencyList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailChargeDr.Id && r.ParallelCurrencyId == companyCurrencyId);
                                voucherDetailCompanyCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                                voucherDetailCompanyCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                                voucherDetailCompanyCurrencyCr.DrAmount = bankJournal.IsReverse ? 0 : voucherVM.CompanyCurrencyRate * bankCharge.Amount;
                                voucherDetailCompanyCurrencyCr.CrAmount = !bankJournal.IsReverse ? 0 : voucherVM.CompanyCurrencyRate * bankCharge.Amount;
                                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailChargeDr, voucherDetailCompanyCurrencyCr);
                                totalAmountDr += voucherDetailChargeDr.DrAmount;
                                totalAmountCr += voucherDetailChargeDr.CrAmount;
                            }
                        }
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public BankJournal FindBankJournal(string bankJournalId)
        {
            return _bankJournalRepository.Find(bankJournalId);
        }

        public BankJournalDetail FindBankJournalDetail(string bankJournalDetailId)
        {
            return _bankJournalDetailRepository.Find(bankJournalDetailId);
        }

        public BankJournalDetail InsertBankJournalDetail(BankJournal bankJournal, BankJournalDetail bankJournalDetail, int currentId)
        {
            bankJournalDetail.Id = _pkGeneratorService.MakePK(bankJournal.Id, currentId, 1);
            bankJournalDetail.BankJournalId = bankJournal.Id;
            bankJournalDetail.AddedBy = bankJournal.AddedBy;
            bankJournalDetail.AddedDate = bankJournal.AddedDate;
            bankJournalDetail.AddedFromIP = bankJournal.AddedFromIP;
            _bankJournalDetailRepository.Insert(bankJournalDetail);
            return bankJournalDetail;
        }

        public void UpdateBankJournalDetail(BankJournal bankJournal, BankJournalDetail bankJournalDetail)
        {
            bankJournalDetail.UpdatedBy = bankJournal.UpdatedBy;
            bankJournalDetail.UpdatedDate = bankJournal.UpdatedDate;
            bankJournalDetail.UpdatedFromIP = bankJournal.UpdatedFromIP;
            _bankJournalDetailRepository.Update(bankJournalDetail);
        }

        public void PostBankJournal(string journalId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var bankJournal = _bankJournalRepository.Find(journalId);
                CheckIsPosted(bankJournal);

                bankJournal.IsPark = false;
                UpdateBankJournal(bankJournal);
                _voucherService.PostVoucher(bankJournal.VoucherId);
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

        private static void CheckIsPosted(BankJournal bankCharge)
        {
            if (!bankCharge.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }

        public FinancingTypeGL GetRevenueGL(string companyId, string financingTypeId)
        {
            var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                        WHERE C.Id='" + companyId + "' AND LTGGL.FinancingTypeId='" + financingTypeId + "'";
            var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
            if (null == glTemp && string.IsNullOrEmpty(glTemp.RevenueGLId))
                throw new CustomException("This Revenue Type GL not Found!");
            return glTemp;
        }

        public IQueryFluent<BankJournal> GetBankJournalList(Expression<Func<BankJournal, bool>> query)
        {
            return _bankJournalRepository.Query(query);
        }

        public IQueryFluent<BankJournalDetail> GetBankJournalDetailList(Expression<Func<BankJournalDetail, bool>> query)
        {
            return _bankJournalDetailRepository.Query(query);
        }

        public GridModel GetBankJournalList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT BC.Id , V.Id AS VoucherId, BC.PostingDate, BC.DocDate, BC.Amount, BM.AccountTitle, BM.AccountNumber,BM.Id AS BankMasterId, V.EntityId, BC.BankJournalType,
                                        V.VoucherDate, V.DocRefNo, V.VoucherNo, V.VoucherTypeId, VDC.ToCurrencyRate AS CompanyCurrencyRate, BC.IsPark, BC.CurrencyId, C.Code AS CurrencyCode, V.Narration
                                        , BJD.BankMasterId AS OtherBankMasterId, BJD.CashMasterId AS OtherCashMasterId, BJD.FinancingTypeId, BC.IsReverse,[Status]=case when v.IsPark=0 then 'Posted' else 'Parked' end
                                        ,V.ApprovedByStatus,EI.EmployeeName ApprovedBy,V.ApprovedById,V.ApprovedDate
                                        FROM [TRN].[BankJournal] AS BC
                                        LEFT JOIN [TRN].[BankJournalDetail] AS BJD ON BJD.BankJournalId=BC.Id
                                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=BC.BankMasterId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BC.VoucherId
                                        LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherId=V.Id
										LEFT JOIN [SCS].[Currency] AS C ON C.Id=BC.CurrencyId
                                        LEFT JOIN DBO.EmployeeInformation EI ON EI.SystemId=V.ApprovedById
                                        WHERE BC.Archive=0 AND BC.CompanyGroupId='" + companyGroupId + "' AND BC.CompanyId='" + companyId + "' AND BC.PlantId='" + plantId + "' AND BC.SourceType='" + sourceType + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public GridModel GetBankJVList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            try
            {
                parameters.CmdText = @"SELECT BC.Id, V.Id AS VoucherId, BC.PostingDate, BC.DocDate, BM.AccountTitle, BM.AccountNumber, BM.Id AS BankMasterId, V.EntityId, BC.BankJournalType,
                                        V.VoucherDate, V.DocRefNo, V.VoucherNo, V.VoucherTypeId, BC.IsPark, BC.CurrencyId, C.Code AS CurrencyCode, V.Narration, BC.IsReverse, BJD.Amount
                                        FROM [TRN].[BankJournal] AS BC
                                        LEFT JOIN(SELECT BankJournalId, SUM(Amount) AS Amount FROM [TRN].[BankJournalDetail] GROUP BY BankJournalId
                                        )AS BJD ON BJD.BankJournalId=BC.Id
                                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=BC.BankMasterId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BC.VoucherId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=BC.CurrencyId
                                        WHERE BC.Archive=0 AND BC.CompanyGroupId='" + companyGroupId + "' AND BC.CompanyId='" + companyId + "' AND BC.PlantId='" + plantId + "' AND BC.SourceType='" + sourceType + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
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
							,CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
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
							WHERE V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"'
                                      AND V.Id= '" + voucherId + "'  AND VD.Id<> '" + voucherDetailId + @"' ";
            return _sqlRepository.GetGridData(parameters);
        }

        /// <summary>
        /// For geting Report Header data.
        /// </summary>
        /// <param name="companyGroupId"></param>
        /// <param name="companyId"></param>
        /// <param name="plantId"></param>
        /// <param name="voucherId"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetBankJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , B.UserName AS BankName, BB.UserName AS BankBranchName, BM.AccountNumber, BM.AccountTitle, BJ.CurrencyId, C.Code AS CurrencyCode, BJ.BankJournalType
                            FROM [TRN].[BankJournal] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=BJ.BankMasterId
                            LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                            LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        /// <summary>
        /// For Report data
        /// </summary>
        /// <param name="companyGroupId"></param>
        /// <param name="companyId"></param>
        /// <param name="plantId"></param>
        /// <param name="voucherId"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public DataTable GetBankJournalDetail(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, GLGI.Id AS AccountCodeId, GLGI.AccountCode, VD.Id AS VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GLGI.UserName AS GL, GLGI.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId, B.UserName AS BudgetName, A.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType
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

        public DataTable GetBankLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate, bool isOpeningBalance, string fiscalYearId)
        {
            var cmdText = @"SELECT REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, UPPER(VD.Narration) AS Narration, ISNULL(GT.DrAmount,0) AS DrAmount, ISNULL(GT.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, GC.CompanyGroupCurrencyId, GC.CompanyGroupCurrencyDrAmount, GC.CompanyGroupCurrencyCrAmount
                            , HC.HardCurrencyId, HC.HardCurrencyDrAmount, HC.HardCurrencyCrAmount, BM.AccountTitle, BM.AccountNumber, BM.CurrencyId, C.Code AS CurrencyCode, B.UserName AS BankName
                            , BB.UserName AS BankBranchName, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName
                            FROM [TRN].[VoucherDetail] VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[GLTransactionDetail] AS GT ON GT.VoucherDetailId=VD.Id
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=BM.CurrencyId
                            LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                            LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=BM.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=BM.ActivityId
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
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "'";
            cmdText += isOpeningBalance ? " AND V.SourceType='OpeningBalance' AND V.FiscalYearId='" + fiscalYearId + "' AND VD.BankMasterId IS NOT NULL" : " AND VD.BankMasterId='" + bankMasterId + "' AND V.SourceType!='OpeningBalance' AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate.ToDbDate() + "'";
            cmdText += " ORDER BY V.PostingDate, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
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
                        , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, V.Narration
                                   ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName  from
														 TRN.VoucherDetail AS XVD  
														 left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
														 left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
													    where	XVD.VoucherId=V.Id AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
														,STUFF((select distinct ','+XEI.EmployeeName  from
														 TRN.VoucherDetail AS XVD  
														 left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
														 left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
													    where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
														,STUFF((select distinct ','+XCM.UserName  from
														 TRN.VoucherDetail AS XVD  
														 left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
														 left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
													    where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
														,STUFF((select distinct ','+XA.UserName  from
														 TRN.VoucherDetail AS XVD  
														 left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
														 left join HKP.Activity XA ON XA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id  AND XVD.BankMasterId IS NULL   AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                        FROM  [TRN].[GLTransactionDetail] AS GLT 
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
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND VD.BankMasterId=@bankMasterId  AND V.SourceType!='OpeningBalance'
						 AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"'
						 
                        UNION

						 SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                         GLT.DrAmount AS DrAmount,
                         GLT.CrAmount AS CrAmount
                        , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, V.Narration
                        ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName  from
														 TRN.VoucherDetail AS XVD  
														 left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
														 left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
													    where	XVD.VoucherId=V.Id AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
														,STUFF((select distinct ','+XEI.EmployeeName  from
														 TRN.VoucherDetail AS XVD  
														 left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
														 left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
													    where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
														,STUFF((select distinct ','+XCM.UserName  from
														 TRN.VoucherDetail AS XVD  
														 left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
														 left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
													    where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
														,STUFF((select distinct ','+XA.UserName  from
														 TRN.VoucherDetail AS XVD  
														 left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
														 left join HKP.Activity XA ON XA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id  AND XVD.BankMasterId IS NULL   AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                        FROM  [TRN].[GLTransactionDetail] AS GLT 
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
						AND V.SourceType='OpeningBalance'
						 AND V.PostingDate > '" + fromDate + @"' 
                            ORDER BY V.PostingDate,V.Addeddate, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public DataTable GetBankReconcileData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                        DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        DECLARE @bankMasterId VARCHAR(10)='" + bankMasterId + @"';
                          SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                         GLT.DrAmount AS DrAmount,
                         GLT.CrAmount AS CrAmount
                        , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, V.Narration,BR.Id AS ReconcileNo,BR.BankStatementNo,GLT.ReconcileDate, OtherSide=CASE WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
	                        WHEN CM.UserName<>'' THEN CM.UserName
	                        WHEN P.UserName<>'' THEN P.UserName
	                        ELSE ''	END
                        FROM  [TRN].[GLTransactionDetail] AS GLT 
						LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLT.VoucherDetailId
						LEFT JOIN [TRN].[BankReconciliation] AS BR ON BR.Id=GLT.ReconcileId
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId 
						AND VD.BankMasterId=@bankMasterId AND GLT.ReconcileId<>'' 
						 AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"'
						 ORDER BY V.PostingDate, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
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

        public Dictionary<string, object> GetBankMaster(string bankMasterId)
        {
            var sql = @"SELECT BM.Id, BM.AccountTitle, BM.AccountNumber, BM.CurrencyId, C.Code AS CurrencyCode, B.UserName AS BankName, BB.UserName AS BankBranchName, GLGI.AccountCode AS GLGeneralInfoCode
                    , GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName
                    FROM [MST].[BankMaster] AS BM
                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=BM.CurrencyId
                    LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                    LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                    LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=BM.BudgetMasterId
                    LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=BM.ActivityId
                    WHERE BM.Id='" + bankMasterId + "'";
            return _sqlRepository.GetData(sql);
        }

        public Dictionary<string, object> GetBankJournal(string bankJournalId)
        {
            var sql = @"SELECT BJ.Id, BJ.BankMasterId, BJ.CashMasterId, V.VoucherDate, V.VoucherTypeId, BJ.DocDate, BJ.DocRefNo, BJ.VoucherId, BJ.PostingDate, BJ.EntityId, BJ.Narration, BJ.Amount, BJ.BankJournalType
                        , BJD.BankMasterId AS OtherBankMasterId, BJD.CashMasterId AS OtherCashMasterId, BJD.FinancingTypeId, V.AddedBy
                        FROM [TRN].[BankJournal] AS BJ
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                        LEFT JOIN [TRN].[BankJournalDetail] AS BJD ON BJD.BankJournalId=BJ.Id
                        WHERE BJ.Id='" + bankJournalId + "'";
            return _sqlRepository.GetData(sql);
        }

        public List<Dictionary<string, object>> GetBankChargeList(string bankJournalId)
        {
            var sql = @"SELECT BC.Id, BC.BankMasterId, BC.FinancingTypeId, FT.ExpensesUserName AS FinancingTypeName, BC.Narration, BC.Amount
                        ,CompanyCurrencyAmount=CASE WHEN ISNULL(VDC.DrAmount,0)=0 THEN VDC.CrAmount ELSE VDC.DrAmount END
                        FROM [TRN].[BankCharge] AS BC
                        LEFT JOIN [HKP].[FinancingType] AS FT ON FT.Id=BC.FinancingTypeId
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.BankChargeId=BC.Id
                        LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
                        WHERE BC.BankJournalId='" + bankJournalId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetAdvanceBankChargeList(string bankChargeId)
        {
            var sql = @"SELECT BC.Id, BC.Id AS BankChargeId, BC.BankMasterId, BC.AdvanceId, BC.FinancingTypeId, FT.ExpensesUserName AS FinancingTypeName, BC.Narration, BC.Amount
                        ,CompanyCurrencyAmount=CASE WHEN ISNULL(VDC.DrAmount,0)=0 THEN VDC.CrAmount ELSE VDC.DrAmount END
                        FROM [TRN].[BankCharge] AS BC
                        LEFT JOIN [HKP].[FinancingType] AS FT ON FT.Id=BC.FinancingTypeId
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.BankChargeId=BC.Id
                        LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
                        WHERE BC.Id='" + bankChargeId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public int GetBankJournalDetailPK(string bankJournalId)
        {
            return _bankJournalDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(SUBSTRING(Id, LEN(BankJournalId)+1, 2) AS INT)), 0) Id FROM [TRN].[BankJournalDetail] WHERE BankJournalId='{bankJournalId}'").First();
        }

        public GridModel GetAvilabeCustomerPaymentList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT AD.BankJournalId AS AdvanceId, AD.BankJournalId, AD.Id AS BankJournalDetailId, AD.PartyType, AM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , REPLACE(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received
                                , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion
                                FROM [TRN].[BankJournalDetail] AS AD
                                LEFT JOIN [TRN].[BankJournal] AS AM ON AD.BankJournalId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.BankJournalDetailId=AD.Id
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
                                WHERE AM.Archive=0 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AD.PartyType='Customer'";
            return _sqlRepository.GetGridData(parameters);
        }

        public DataTable GetBankBookLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                        DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        DECLARE @bankMasterId VARCHAR(10)='" + bankMasterId + @"';
                        SELECT V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.CurrencyId,
                         VD.CrAmount DrAmount,
                         VD.DrAmount CrAmount
						 , V.Narration
                         ,CC.CompanyCurrencyCrAmount CompanyCurrencyDrAmount, CC.CompanyCurrencyDrAmount CompanyCurrencyCrAmount
						, OtherSide=CASE 
	                        WHEN P.UserName<>'' THEN P.UserName
							WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
	                        WHEN CM.UserName<>'' THEN CM.UserName
							WHEN EI.EmployeeName <>'' THEN EI.EmployeeName
	                        ELSE A.UserName	END
                        FROM  [TRN].[VoucherDetail] AS VD 
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
						LEFT JOIN [MST].[BudgetMaster] AS BDM ON BDM.Id=VD.BudgetMasterId
						LEFT JOIN [HKP].[Budget] AS B ON B.Id=BDM.BudgetId
						LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
						LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=VD.EmployeeId
						 JOIN (SELECT VoucherId FROM TRN.VoucherDetail VVD WHERE VVD.BankMasterId=@bankMasterId ) VDD ON VDD.VoucherId=VD.VoucherId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId  AND (isnull(VD.BankMasterId,'')='' OR (isnull(VD.BankMasterId,'')<>'' AND VD.BankMasterId<>@bankMasterId))
						 AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND V.SourceType!='OpeningBalance'
                            UNION
                        SELECT V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.CurrencyId,
                         VD.CrAmount DrAmount,
                         VD.DrAmount CrAmount
						 , V.Narration
                         ,CC.CompanyCurrencyCrAmount CompanyCurrencyDrAmount, CC.CompanyCurrencyDrAmount CompanyCurrencyCrAmount
						, OtherSide=CASE 
	                        WHEN P.UserName<>'' THEN P.UserName
							WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
	                        WHEN CM.UserName<>'' THEN CM.UserName
							WHEN EI.EmployeeName <>'' THEN EI.EmployeeName
	                        ELSE A.UserName	END
                        FROM  [TRN].[VoucherDetail] AS VD 
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
						LEFT JOIN [MST].[BudgetMaster] AS BDM ON BDM.Id=VD.BudgetMasterId
						LEFT JOIN [HKP].[Budget] AS B ON B.Id=BDM.BudgetId
						LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
						LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=VD.EmployeeId
						 JOIN (SELECT VoucherId FROM TRN.VoucherDetail VVD WHERE VVD.BankMasterId=@bankMasterId ) VDD ON VDD.VoucherId=VD.VoucherId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId  AND (isnull(VD.BankMasterId,'')='' OR (isnull(VD.BankMasterId,'')<>'' AND VD.BankMasterId<>@bankMasterId))
						 AND V.PostingDate > '" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        ORDER BY PostingDate ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        private static void CheckIsPosted(Voucher voucher)
        {
            if (!voucher.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }
        public void DeleteVoucherDetail(string Id, string voucherId, string bankjournalDetailId, string plantId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                var bankjournaldetail = _bankJournalDetailRepository.Find(bankjournalDetailId);
                CheckIsPosted(voucher);
                var voucherDetail = _voucherDetailRepository.Query(r => r.Id == Id && r.VoucherId == voucherId).Select().FirstOrDefault();
                var voucherDetailCurrency = _voucherDetailCurrencyRepository.Query(r => r.VoucherDetailId == Id && r.VoucherId == voucherId).Select().FirstOrDefault();
                if (voucherDetail != null && voucherDetail.Id == Id)
                {
                    if (!string.IsNullOrEmpty(voucherDetail.BankMasterId) || !string.IsNullOrEmpty(voucherDetail.CashMasterId))
                        _gLTransactionDetailRepository.Delete(Id);
                    _voucherDetailCurrencyRepository.Delete(voucherDetailCurrency.Id);

                    _voucherDetailRepository.Delete(Id);
                    _bankJournalDetailRepository.Delete(bankjournalDetailId);
                }
                else
                    throw new CustomException("Data not found!");

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


        public void DeleteBankJournal(string bankJournalId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var voucher = _voucherRepository.Find(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var bankJournal = _bankJournalRepository.Find(bankJournalId);
                var bankJournalDetails = _bankJournalDetailRepository.Query(r => r.BankJournalId == bankJournalId).Select().ToList();
                var bankCharges = _bankChargeRepository.Query(r => r.BankJournalId == bankJournalId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id); 
                }

                foreach (var item in voucherdetail)
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = @"UPDATE [TRN].VoucherDetail SET BankJournalDetailId=NULL,BankChargeId=NULL,UpdatedBy='" + identity.UserId + "' WHERE Id='" + item.Id + "'";
                    rdBuilder.Append(builderSql);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    var glTransactionDetail = _gLTransactionDetailRepository.Query(r => r.VoucherDetailId == item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _gLTransactionDetailRepository.Delete(item.Id);
                    }
                    _voucherDetailRepository.Delete(item.Id);
                }
                if (bankCharges != null)
                {
                    foreach (var item in bankCharges)
                    {
                        _bankChargeRepository.Delete(item.Id);
                    }
                }
                if (bankJournalDetails != null)
                {
                    foreach (var item in bankJournalDetails)
                    {
                        _bankJournalDetailRepository.Delete(item.Id);
                    }
                }
                _bankJournalRepository.Delete(bankJournalId);
                _voucherRepository.Delete(voucher.Id);
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


    }
}