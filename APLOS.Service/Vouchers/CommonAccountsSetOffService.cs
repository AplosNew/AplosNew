using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.Advances;
using Library.Model.Banks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Vouchers;
using Library.Service.Advances;
using Library.Service.Banks;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Invoices;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Banks;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Library.Service.Vouchers
{
    public class CommonAccountsSetOffService : Service<AdvanceWriteOff>, ICommonAccountsSetOffService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IAdvanceService _advanceService;
        private readonly IVoucherService _voucherService;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceWriteOffService _invoiceWriteOffService;
        private readonly IPKGeneratorService _pKGeneratorService;
        private readonly IRepositoryAsync<Advance> _advanceRepository;
        private readonly IRepositoryAsync<AdvanceDetail> _advanceDetailRepository;
        private readonly IRepositoryAsync<AdvanceWriteOff> _advanceWriteOffRepository;
        private readonly IRepositoryAsync<AdvanceWriteOffDetail> _advanceWriteOffDetailRepository;
        private readonly IRepositoryAsync<InvoiceWriteOffDetail> _invoiceWriteOffDetailRepository;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;
        private readonly IRepositoryAsync<AdjustmentNote> _adjustmentNoteRepository;
        private readonly IRepositoryAsync<AdjustmentNoteDetail> _adjustmentNoteDetailRepository;
        private readonly IRepositoryAsync<BankJournal> _bankJournalRepository;
        private readonly IRepositoryAsync<BankJournalDetail> _bankJournalDetailRepository;
        private readonly IRepositoryAsync<BankReconciliationMap> _bankReconciliationMapRepository;
        public CommonAccountsSetOffService(
              IRepositoryAsync<AdvanceWriteOff> advanceWriteOffRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<AdvanceWriteOffDetail> advanceWriteOffDetailRepository
            , IRepositoryAsync<InvoiceWriteOffDetail> invoiceWriteOffDetailRepository
            , IAdvanceService advanceService
            , IVoucherService voucherService
            , IInvoiceService invoiceService
            , IInvoiceWriteOffService invoiceWriteOffService
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository
            , IRepositoryAsync<Advance> advanceRepository
            , IRepositoryAsync<AdvanceDetail> advanceDetailRepository
            , IRepositoryAsync<AdjustmentNote> adjustmentNoteRepository
            , IRepositoryAsync<AdjustmentNoteDetail> adjustmentNoteDetailRepository
            , IRepositoryAsync<BankJournal> bankJournalRepository
            , IRepositoryAsync<BankJournalDetail> bankJournalDetailRepository
            , IRepositoryAsync<BankReconciliationMap> bankReconciliationMapRepository
            ) : base(advanceWriteOffRepository, unitOfWork, pkGeneratorService)
        {
            _advanceWriteOffDetailRepository = advanceWriteOffDetailRepository;
            _invoiceWriteOffDetailRepository = invoiceWriteOffDetailRepository;
            _advanceService = advanceService;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _pKGeneratorService = pkGeneratorService;
            _advanceWriteOffRepository = advanceWriteOffRepository;
            _voucherService = voucherService;
            _invoiceService = invoiceService;
            _invoiceWriteOffService = invoiceWriteOffService;
            _voucherRepository = voucherRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _advanceRepository = advanceRepository;
            _advanceDetailRepository = advanceDetailRepository;
            _adjustmentNoteDetailRepository = adjustmentNoteDetailRepository;
            _adjustmentNoteRepository = adjustmentNoteRepository;
            _bankJournalRepository = bankJournalRepository;
            _bankJournalDetailRepository = bankJournalDetailRepository;
            _bankReconciliationMapRepository = bankReconciliationMapRepository;
        }

        #endregion Constructor

        private AdvanceWriteOff InsertAdvanceWriteOff(AdvanceWriteOff advanceWriteOff)
        {
            advanceWriteOff.Id = GetAutoNumber(nameof(AdvanceWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
            AuditService.AddedLog(advanceWriteOff);
            _advanceWriteOffRepository.Insert(advanceWriteOff);
            return advanceWriteOff;
        }

        private AdvanceWriteOff InsertAdvanceWriteOff(VoucherViewModel voucherVM)
        {
            return InsertAdvanceWriteOff(new AdvanceWriteOff
            {
                CompanyGroupId = voucherVM.CompanyGroupId,
                CompanyId = voucherVM.CompanyId,
                PlantId = voucherVM.PlantId,
                EntityId = voucherVM.EntityId,
                FiscalYearId = voucherVM.FiscalYearId,
                FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                TaxYearId = voucherVM.TaxYearId,
                TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                VoucherTypeId = voucherVM.VoucherTypeId,
                CurrencyId = voucherVM.CurrencyId,
                PartyType = voucherVM.PartyType,
                PartyId = voucherVM.PartyId,
                PartyPlantId = voucherVM.PartyPlantId,
                EmployeeId = voucherVM.EmployeeId,
                Amount = voucherVM.Amount,
                VoucherDate = voucherVM.VoucherDate,
                PostingDate = voucherVM.PostingDate,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                Narration = voucherVM.Narration,
                SourceType = voucherVM.SourceType,
                IsPark = voucherVM.IsPark,
                SettlementType = voucherVM.SettlementType,
                PaymentSource = voucherVM.PaymentSource,
                BankMasterId = voucherVM.BankMasterId,
                CashMasterId = voucherVM.CashMasterId,
                Archive = false
            });
        }
        
        private AdvanceWriteOff InsertAdvanceWriteOffDifferentCurrency(AdvanceWriteOff advanceWriteOffVM)
        {
            var advanceWriteOff = new AdvanceWriteOff
            {
                CompanyGroupId = advanceWriteOffVM.CompanyGroupId,
                CompanyId = advanceWriteOffVM.CompanyId,
                PlantId = advanceWriteOffVM.PlantId,
                EntityId = advanceWriteOffVM.EntityId,
                FiscalYearId = advanceWriteOffVM.FiscalYearId,
                FiscalYearPeriodId = advanceWriteOffVM.FiscalYearPeriodId,
                TaxYearId = advanceWriteOffVM.TaxYearId,
                TaxYearPeriodId = advanceWriteOffVM.TaxYearPeriodId,
                VoucherTypeId = advanceWriteOffVM.VoucherTypeId,
                CurrencyId = advanceWriteOffVM.CurrencyId,
                PartyType = advanceWriteOffVM.PartyType,
                PartyId = advanceWriteOffVM.PartyId,
                PartyPlantId = advanceWriteOffVM.PartyPlantId,
                EmployeeId = advanceWriteOffVM.EmployeeId,
                Amount = advanceWriteOffVM.Amount,
                VoucherDate = advanceWriteOffVM.VoucherDate,
                PostingDate = advanceWriteOffVM.PostingDate,
                DocDate = advanceWriteOffVM.DocDate,
                DocRefNo = advanceWriteOffVM.DocRefNo,
                Narration = advanceWriteOffVM.Narration,
                SourceType = advanceWriteOffVM.SourceType,
                IsPark = advanceWriteOffVM.IsPark,
                SettlementType = advanceWriteOffVM.SettlementType,
                PaymentSource = advanceWriteOffVM.PaymentSource,
                BankMasterId = advanceWriteOffVM.BankMasterId,
                CashMasterId = advanceWriteOffVM.CashMasterId,
                Archive = false
            };
            
            return InsertAdvanceWriteOff(advanceWriteOff);
        }
        private void Check(InvoiceWriteOff entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        }
        private void InsertAdvanceWriteOffDetail(AdvanceWriteOff advanceWriteOff, AdvanceWriteOffDetail advanceWriteOffDetail, int currentId)
        {
            advanceWriteOffDetail.Id = MakePK(advanceWriteOff.Id, currentId, 2);
            advanceWriteOffDetail.AddedBy = advanceWriteOff.AddedBy;
            advanceWriteOffDetail.AddedDate = advanceWriteOff.AddedDate;
            advanceWriteOffDetail.AddedFromIP = advanceWriteOff.AddedFromIP;
            advanceWriteOffDetail.AdvanceWriteOffId = advanceWriteOff.Id;
            advanceWriteOffDetail.Archive = advanceWriteOff.Archive;
            _advanceWriteOffDetailRepository.Insert(advanceWriteOffDetail);
        }

        private InvoiceWriteOff InsertInvoiceWriteOff(VoucherViewModel voucherVM)
        {
            if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                if (string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                else
                    voucherVM.CashMasterId = null;
            else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                    throw new CustomException("Cash Id not found!");
                else
                    voucherVM.BankMasterId = null;

            var invoiceWriteOff = new InvoiceWriteOff
            {
                CompanyGroupId = voucherVM.CompanyGroupId,
                CompanyId = voucherVM.CompanyId,
                PlantId = voucherVM.PlantId,
                FiscalYearId = voucherVM.FiscalYearId,
                FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                TaxYearId = voucherVM.TaxYearId,
                TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                VoucherTypeId = voucherVM.VoucherTypeId,
                CurrencyId = voucherVM.CurrencyId,
                SourceType = voucherVM.SourceType,
                PartyType = voucherVM.PartyType,
                PartyId = voucherVM.PartyId,
                PartyPlantId = voucherVM.PartyPlantId,
                Amount = voucherVM.Amount,
                VoucherDate = voucherVM.VoucherDate,
                PostingDate = voucherVM.PostingDate,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                Narration = voucherVM.Narration,
                AddedBy = voucherVM.AddedBy,
                AddedDate = voucherVM.AddedDate,
                AddedFromIP = voucherVM.AddedFromIP,
                IsPark = voucherVM.IsPark,
                Archive = false,
                BankMasterId = voucherVM.BankMasterId,
                CashMasterId = voucherVM.CashMasterId,
                EmployeeId = voucherVM.EmployeeId,
                PaymentSource = voucherVM.PaymentSource,
                RoundingType = voucherVM.RoundingType,
                RoundingAmount = voucherVM.RoundingAmount,
                InvoiceWriteOffGroupNo = voucherVM.InvoiceWriteOffGroupNo
            };
            if (voucherVM.SourceType != "CustomerBanksReceipt")
            {
                Check(invoiceWriteOff);
            }
            return _invoiceWriteOffService.InsertInvoiceWriteOff(invoiceWriteOff);
        }
        private void InsertInvoiceWriteOffDetail(InvoiceWriteOff invoiceWriteOff, InvoiceWriteOffDetail invoiceWriteOffDetail, int currentId)
        {
            invoiceWriteOffDetail.AddedBy = invoiceWriteOff.AddedBy;
            invoiceWriteOffDetail.AddedDate = invoiceWriteOff.AddedDate;
            invoiceWriteOffDetail.AddedFromIP = invoiceWriteOff.AddedFromIP;
            invoiceWriteOffDetail.Archive = invoiceWriteOff.Archive;
            invoiceWriteOffDetail.InvoiceWriteOffId = invoiceWriteOff.Id;
            invoiceWriteOffDetail.Id = MakePK(invoiceWriteOff.Id, currentId, 2);
            _invoiceWriteOffDetailRepository.Insert(invoiceWriteOffDetail);
        }
        public string InsertDebitNoteAdvanceSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO InvoiceWriteOff
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;
                advanceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;

                var adjustNoteIds = voucherDetailVMList.Select(r => r.AdjustmentNoteId);
                var adjustNoteDbList = _adjustmentNoteRepository.Query(r => adjustNoteIds.Contains(r.Id)).Select().ToList();
                var adjustNoteDetailIds = voucherDetailVMList.Select(r => r.AdjustmentNoteDetailId);
                var adjustNoteDetailDbList = _adjustmentNoteDetailRepository.Query(r => adjustNoteDetailIds.Contains(r.Id)).Select().ToList();
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var adjustNoteDetail = adjustNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdjustmentNoteDetailId);
                    if (null == adjustNoteDetail)
                        throw new CustomException("Invoice not found!");

                    adjustNoteDetail.WrittenOffAmount += voucherVM.Amount;

                    if (adjustNoteDetail.Amount < adjustNoteDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    adjustNoteDetail.IsWrittenOff = adjustNoteDetail.Amount == adjustNoteDetail.WrittenOffAmount;
                    adjustNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    adjustNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    adjustNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteDetailRepository.Update(adjustNoteDetail);

                    // TODO: have a gap here if invoice split
                    var adjustNote = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                    adjustNote.WrittenOffAmount += voucherVM.Amount;
                    adjustNote.IsWrittenOff = adjustNote.Amount == adjustNote.WrittenOffAmount;
                    adjustNote.UpdatedBy = invoiceWriteOff.AddedBy;
                    adjustNote.UpdatedDate = invoiceWriteOff.AddedDate;
                    adjustNote.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteRepository.Update(adjustNote);

                    // INSERT INTO InvoiceDetail
                    currentInvoiceWriteOffDetailId++;
                    var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        AdjustmentNoteId = voucherDetailVM.AdjustmentNoteId,
                        AdjustmentNoteDetailId = voucherDetailVM.AdjustmentNoteDetailId,
                        Amount = voucherVM.Amount,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP,
                        Archive = invoiceWriteOff.Archive,
                        ModelState = invoiceWriteOff.ModelState,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration
                    };
                    InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);
                    if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                        throw new CustomException("ActivityId is not found.");
                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherDetailVM.EntityId,
                        CrAmount = voucherVM.Amount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                        CrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailCr.CrAmount
                    });

                    totalAmountCr += voucherDetailCr.CrAmount;
                    totalCurrencyAmountCr += voucherDetailVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                    if (voucherDetailVM.ExchangeType == "ExchangeLoss" && voucherDetailVM.ExchangeAmount > 0)
                    {
                        var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                        var voucherDtEx = new VoucherDetail
                        {
                            GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            PartyType = voucherDetailVM.ExchangeType
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDtEx.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailVM.ExchangeAmount
                        });
                        totalCurrencyAmountCr -= voucherDetailVM.ExchangeAmount;
                    }

                    if (voucherDetailVM.ExchangeType == "ExchangeGain" && voucherDetailVM.ExchangeAmount > 0)
                    {
                        var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                        var voucherDtExGain = new VoucherDetail
                        {
                            GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            PartyType = voucherDetailVM.ExchangeType
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDtExGain.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherDetailVM.ExchangeAmount
                        });
                        totalCurrencyAmountCr += voucherDetailVM.ExchangeAmount;
                    }
                }

                decimal totalCharges = 0;
                decimal totalCurrencyCharges = 0;

                if (voucherVM.PaymentSource == SettlementType.AdvanceToVendor.ToString())
                {
                    // Advance
                    var advances = voucherDetailInvoiceList.Select(r => r.AdvanceId);
                    var advancesDbList = _advanceService.Query(r => advances.Contains(r.Id)).Select().ToList();
                    var advancesDetailIds = voucherDetailInvoiceList.Select(r => r.AdvanceDetailId);
                    var advancesDetailDbList = _advanceService.GetAdvanceDetailList(r => advancesDetailIds.Contains(r.Id)).Select().ToList();

                    foreach (var voucherDetailVM in voucherDetailInvoiceList)
                    {
                        var advanceDetail = advancesDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdvanceDetailId);
                        if (null == advanceDetail)
                            throw new CustomException("Invoice not found!");

                        advanceDetail.AdditionalAmount += voucherDetailVM.Amount;
                        if (advanceDetail.NetAmount < advanceDetail.WrittenOffAmount)
                            throw new CustomException("Received amount can not cross balance amount.");

                        //advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                        advanceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                        advanceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                        advanceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _advanceService.UpdateAdvanceDetail(advanceDetail);

                        var invoice = advancesDbList.First(r => r.Id == advanceDetail.AdvanceId);
                        invoice.AdditionalAmount = voucherDetailVM.Amount;
                        //invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _advanceService.Update(invoice);

                        // INSERT INTO InvoiceWriteOffDetail
                        currentInvoiceWriteOffDetailId++;
                        var advanceWriteOffDetail = new AdvanceWriteOffDetail
                        {
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            CurrencyId = invoice.CurrencyId,
                            AdvanceWriteOffId = advanceWriteOff.Id,
                            AdvanceId = voucherDetailVM.AdvanceId,
                            AdvanceDetailId = voucherDetailVM.AdvanceDetailId,
                            CompanyId = voucherDetailVM.CompanyId,
                            PlantId = voucherDetailVM.PlantId,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            PartyType = voucherDetailVM.PartyType,
                            Amount = voucherDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP,
                            Archive = invoiceWriteOff.Archive,
                            IsAdditionalAdvance = true

                        };
                        InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentInvoiceWriteOffDetailId);
                        invoiceWriteOff.Amount = advanceWriteOffDetail.Amount;

                        // INSERT INTO VoucherDetail
                        var voucherDetailCr = new VoucherDetail
                        {
                            VoucherId = voucher.Id,
                            AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = voucherDetailVM.Amount,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PartyType = invoiceWriteOff.PartyType,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                        totalAmountDr += voucherDetailCr.DrAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailCr.DrAmount * voucherDetailVM.CompanyCurrencyRate,
                        });

                        if (voucherDetailVM.ExchangeType == "ExchangeGain")
                        {
                            var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                            var voucherDetailGain = new VoucherDetail
                            {
                                GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                                BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                                ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                                CurrencyId = voucher.CurrencyId,
                                PartyType = voucherDetailVM.ExchangeType
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailGain, currentVoucherDetailId);

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailGain, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailGain.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherDetailVM.ExchangeAmount
                            });
                        }
                        else if (voucherDetailVM.ExchangeType == "ExchangeLoss")
                        {
                            var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                            var voucherDetailLoss = new VoucherDetail
                            {
                                GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                                BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                                ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                                CurrencyId = voucher.CurrencyId,
                                PartyType = voucherDetailVM.ExchangeType
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailLoss, currentVoucherDetailId);

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoss, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailLoss.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherDetailVM.ExchangeAmount
                            });
                        }
                    }
                }
                if (voucherVM.PaymentSource == SettlementType.AdvanceToCustomer.ToString())
                {
                    // Advance
                    var advances = voucherDetailInvoiceList.Select(r => r.AdvanceId);
                    var advancesDbList = _advanceService.Query(r => advances.Contains(r.Id)).Select().ToList();
                    var advancesDetailIds = voucherDetailInvoiceList.Select(r => r.AdvanceDetailId);
                    var advancesDetailDbList = _advanceService.GetAdvanceDetailList(r => advancesDetailIds.Contains(r.Id)).Select().ToList();

                    foreach (var voucherDetailVM in voucherDetailInvoiceList)
                    {
                        var advanceDetail = advancesDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdvanceDetailId);
                        if (null == advanceDetail)
                            throw new CustomException("Invoice not found!");

                        advanceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                        if (advanceDetail.NetAmount < advanceDetail.WrittenOffAmount)
                            throw new CustomException("Received amount can not cross balance amount.");

                        advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                        advanceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                        advanceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                        advanceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _advanceService.UpdateAdvanceDetail(advanceDetail);

                        var invoice = advancesDbList.First(r => r.Id == advanceDetail.AdvanceId);
                        invoice.WrittenOffAmount = advanceDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _advanceService.Update(invoice);

                        // INSERT INTO InvoiceWriteOffDetail
                        currentInvoiceWriteOffDetailId++;
                        var advanceWriteOffDetail = new AdvanceWriteOffDetail
                        {
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            CurrencyId = invoice.CurrencyId,
                            AdvanceWriteOffId = advanceWriteOff.Id,
                            AdvanceId = voucherDetailVM.AdvanceId,
                            AdvanceDetailId = voucherDetailVM.AdvanceDetailId,
                            CompanyId = voucherDetailVM.CompanyId,
                            PlantId = voucherDetailVM.PlantId,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            PartyType = voucherDetailVM.PartyType,
                            Amount = voucherDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP,
                            Archive = invoiceWriteOff.Archive

                        };
                        InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentInvoiceWriteOffDetailId);
                        invoiceWriteOff.Amount = advanceWriteOffDetail.Amount;

                        // INSERT INTO VoucherDetail
                        var voucherDetailCr = new VoucherDetail
                        {
                            VoucherId = voucher.Id,
                            AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = voucherDetailVM.Amount,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PartyType = invoiceWriteOff.PartyType,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                        totalAmountDr += voucherDetailCr.DrAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailCr.DrAmount * voucherDetailVM.CompanyCurrencyRate,
                        });

                        if (voucherDetailVM.ExchangeType == "ExchangeGain")
                        {
                            var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                            var voucherDetailGain = new VoucherDetail
                            {
                                GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                                BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                                ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                                CurrencyId = voucher.CurrencyId,
                                PartyType = voucherDetailVM.ExchangeType
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailGain, currentVoucherDetailId);

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailGain, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailGain.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherDetailVM.ExchangeAmount
                            });
                        }
                        else if (voucherDetailVM.ExchangeType == "ExchangeLoss")
                        {
                            var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                            var voucherDetailLoss = new VoucherDetail
                            {
                                GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                                BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                                ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                                CurrencyId = voucher.CurrencyId,
                                PartyType = voucherDetailVM.ExchangeType
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailLoss, currentVoucherDetailId);

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoss, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailLoss.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherDetailVM.ExchangeAmount
                            });
                        }
                    }
                }

                totalCurrencyAmountDr = totalCurrencyAmountCr;
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

        private BankJournal InsertBankJournal(BankJournal bankJournal)
        {
            bankJournal.Id = _pKGeneratorService.GetAutoNumber(nameof(BankJournal), PKGeneratorEnum.Yearly, null, DateTime.Now);
            AuditService.AddedLog(bankJournal);
            _bankJournalRepository.Insert(bankJournal);
            return bankJournal;
        }
        private BankJournalDetail InsertBankJournalDetail(BankJournal bankJournal, BankJournalDetail bankJournalDetail, int currentId)
        {
            bankJournalDetail.Id = _pKGeneratorService.MakePK(bankJournal.Id, currentId, 1);
            bankJournalDetail.BankJournalId = bankJournal.Id;
            bankJournalDetail.AddedBy = bankJournal.AddedBy;
            bankJournalDetail.AddedDate = bankJournal.AddedDate;
            bankJournalDetail.AddedFromIP = bankJournal.AddedFromIP;
            _bankJournalDetailRepository.Insert(bankJournalDetail);
            return bankJournalDetail;
        }

        public string InsertExpenseToBankReconcil(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                string tempVoucherDetailId=null;
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
                    IsPark = bankJournal.IsPark
                }, voucherVM.FiscalYearPrefix);

                // Set VoucherId in BankJournal
                bankJournal.VoucherId = voucher.Id;

                var currentVoucherDetailId = 1;
                var currentBankJournalDetailId = 0;
                var bankMaster = _accountsCommonService.GetBankMaster(bankJournal.BankMasterId); //_bankMasterRepository.Find(bankJournal.BankMasterId);
                // INSERT INTO VoucherDetail Credit
                var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                {
                    GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString(),
                    BudgetMasterId = bankMaster["BudgetMasterId"].ToString(),
                    ActivityId = bankMaster["ActivityId"].ToString(),
                    BankMasterId = bankJournal.BankMasterId,
                    DrAmount = bankJournal.BankJournalType == BankJournalType.ProfitEarn.ToString() ? bankJournal.Amount : 0,
                    CrAmount = bankJournal.BankJournalType != BankJournalType.ProfitEarn.ToString() ? bankJournal.Amount : 0,
                    PaymentSource = bankJournal.PaymentSource,
                    PartyType = bankJournal.PaymentSource,
                    TrnNature = TransactionNature.Bank.ToString()
                }, currentVoucherDetailId);
                tempVoucherDetailId = voucherDetail.Id;
                if (bankJournal.BankJournalType == BankJournalType.BankToGL.ToString())
                {
                    voucherDetail.DrAmount = 0;
                    voucherDetail.CrAmount = bankJournal.Amount;
                }

                var glTransactionDetailCr = new GLTransactionDetail
                {
                    SourceType = voucherDetail.PaymentSource,
                    BankMasterId = voucherDetail.BankMasterId,
                };

                if (bankMaster["CurrencyId"].ToString() == voucher.CurrencyId)
                {
                    glTransactionDetailCr.CrAmount = voucherDetail.CrAmount;
                    glTransactionDetailCr.DrAmount = voucherDetail.DrAmount;
                }
                else
                {
                    glTransactionDetailCr.CrAmount = Math.Round(voucherDetail.CrAmount * voucherVM.CompanyCurrencyRate, 2);
                    glTransactionDetailCr.DrAmount = Math.Round(voucherDetail.DrAmount * voucherVM.CompanyCurrencyRate, 2);
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
                    DrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount, 2),
                    CrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount, 2)
                });

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = voucherDetail.DrAmount;
                var totalAmountCr = voucherDetail.CrAmount;

                // INSERT INTO Debit Side
                 if (bankJournal.BankJournalType == BankJournalType.BankToGL.ToString())
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
                            DrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount, 2)
                        });

                        totalAmountDr += voucherDetailDr.DrAmount;
                        totalAmountCr += voucherDetailDr.CrAmount;
                    }
                }

                if (voucherVM.BankReconciliationUploadedDataId != null)
                {
                    var bankReconciliationMap = new BankReconciliationMap
                    {
                        Id = GetAutoNumber(nameof(BankReconciliationMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        BankReconciliationUploadedDataId = voucherVM.BankReconciliationUploadedDataId,
                        VoucherDetailId = tempVoucherDetailId,
                        GLTransactionDetailId = tempVoucherDetailId,
                        AddedBy= voucher.AddedBy,
                        AddedDate= voucher.AddedDate,
                        AddedFromIP=voucher.AddedFromIP
                    };
                    _bankReconciliationMapRepository.Insert(bankReconciliationMap);
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

    }
}