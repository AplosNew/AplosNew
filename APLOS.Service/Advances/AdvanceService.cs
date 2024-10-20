using Library.Core;
using Library.Crosscutting.Security;
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
using Library.Model.Taxations;
using Library.Model.Vouchers;
using Library.Service.Banks;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Extension.Accounts;
using Library.Service.Finances;
using Library.Service.Invoices;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Library.Service.Advances
{
    public class AdvanceService : Service<Advance>, IAdvanceService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<EmployeeSubsequentTransaction> _employeeSubsequentTransactionRepository;
        private readonly IRepositoryAsync<AdvanceWriteOff> _advanceWriteOffRepository;
        private readonly IRepositoryAsync<AdvanceWriteOffDetail> _advanceWriteOffDetailRepository;
        private readonly IRepositoryAsync<AdvanceDetail> _advanceDetailRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly IRepositoryAsync<TaxCode> _taxCodeRepository;
        private readonly IRepositoryAsync<TaxCodeGL> _taxCodeGLRepository;
        private readonly IVoucherService _voucherService;
        private readonly IEmployeeTransactionTypeGLService _employeeTransactionTypeGLService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly IRepositoryAsync<BankMaster> _bankMasterRepository;
        private readonly IRepositoryAsync<CashMaster> _cashMasterRepository;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly IRepositoryAsync<CompanyPartyGL> _companyPartyGLRepository;
        private readonly IBankChargeService _bankChargeService;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IFinancingTypeGLService _financingTypeGLService;
        private readonly IInvoiceWriteOffService _invoiceWriteOffService;
        private readonly IInvoiceService _invoiceService;
        private readonly IExchangeGainLossService _exchangeGainLossService;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;
        private readonly IRepositoryAsync<GLTransactionDetail> _gLTransactionDetailRepository;
        private readonly IRepositoryAsync<BankCharge> _bankChargeRepository;
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<InvoiceDetail> _invoiceDetailRepository;
        private readonly IRepositoryAsync<InvoiceWriteOff> _invoiceWriteOffRepository;
        private readonly IRepositoryAsync<InvoiceWriteOffDetail> _invoiceWriteOffDetailRepository;
        private readonly IRepositoryAsync<AdjustmentNote> _adjustmentNoteRepository;
        private readonly IRepositoryAsync<AdjustmentNoteDetail> _adjustmentNoteDetailRepository;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IRepositoryAsync<EmployeePayableWriteOff> _employeePayableWriteOffRepository;
        private readonly IRepositoryAsync<EmployeePayableWriteOffDetail> _employeePayableWriteOffDetailRepository;
        private readonly IRepositoryAsync<EmployeePayable> _employeePayableRepository;
        private readonly IRepositoryAsync<EmployeePayableDetail> _employeePayableDetailRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<EmployeeTransactionTypeGL> _employeeTransactionTypeGLRepository;
        private readonly IRepositoryAsync<EmployeeSalaryAdvance> _employeeSalaryAdvanceRepository;
        private readonly IRepositoryAsync<AdvanceReqSchedule> _advanceReqScheduleRepository;
        private readonly IRepositoryAsync<BankReconciliationMap> _bankReconciliationMapRepository;
        private readonly IEmployeePayableWriteOffService _employeePayableWriteOffService;
        private readonly IFinancingService _financingService;
        private readonly IRepositoryAsync<FinancingSubsequentTransaction> _loanInterestPayableRepository;
        private readonly IRepositoryAsync<FinancingWriteOff> _financingWriteOffRepository;

        public AdvanceService(
              IRepositoryAsync<Advance> advanceRepository
            , IRepositoryAsync<AdvanceDetail> advanceDetailRepository
            , IRepositoryAsync<AdvanceWriteOff> advanceWriteOffRepository
            , IRepositoryAsync<AdvanceWriteOffDetail> advanceWriteOffDetailRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IVoucherService voucherService
            , IBankChargeService bankChargeService
            , ICompanyTaxYearService companyTaxYearService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IEmployeeTransactionTypeGLService employeeTransactionTypeGLService
            , ICompanyFiscalYearService companyFiscalYearService
            , IRepositoryAsync<BankMaster> bankMasterRepository
            , IRepositoryAsync<CashMaster> cashMasterRepository
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , IRepositoryAsync<CompanyPartyGL> companyPartyGLRepository
            , IRepositoryAsync<TaxCode> taxCodeRepository
            , IRepositoryAsync<TaxCodeGL> taxCodeGLRepository
            , IInvoiceTaxService invoiceTaxService
            , IFinancingTypeGLService financingTypeGLService
            , IInvoiceWriteOffService invoiceWriteOffService
            , IInvoiceService invoiceService
            , IExchangeGainLossService exchangeGainLossService
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository
            , IRepositoryAsync<GLTransactionDetail> gLTransactionDetailRepository
            , IRepositoryAsync<BankCharge> bankChargeRepository
            , IRepositoryAsync<Invoice> invoiceRepository
            , IRepositoryAsync<InvoiceDetail> invoiceDetailRepository
            , IRepositoryAsync<InvoiceWriteOff> invoiceWriteOffRepository
            , IRepositoryAsync<InvoiceWriteOffDetail> invoiceWriteOffDetailRepository
            , IRepositoryAsync<AdjustmentNote> adjustmentNoteRepository
            , IRepositoryAsync<AdjustmentNoteDetail> adjustmentNoteDetailRepository
            , IEmployeePayableService employeePayableService
            , IRepositoryAsync<EmployeePayableWriteOff> employeePayableWriteOffRepository
            , IRepositoryAsync<EmployeePayableWriteOffDetail> employeePayableWriteOffDetailRepository
             , IRepositoryAsync<EmployeePayable> employeePayableRepository
            , IRepositoryAsync<EmployeePayableDetail> employeePayableDetailRepository
            , IEmployeePayableWriteOffService employeePayableWriteOffService
            , IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository
            , IRepositoryAsync<EmployeeTransactionTypeGL> employeeTransactionTypeGLRepository
            , IRepositoryAsync<AdvanceReqSchedule> advanceReqScheduleRepository
            , IRepositoryAsync<EmployeeSalaryAdvance> employeeSalaryAdvanceRepository
            , IRepositoryAsync<EmployeeSubsequentTransaction> employeeSubsequentTransactionRepository
            , IRepositoryAsync<BankReconciliationMap> bankReconciliationMapRepository
            , IFinancingService financingService
            , IRepositoryAsync<FinancingSubsequentTransaction> loanInterestPayableRepository
            , IRepositoryAsync<FinancingWriteOff> financingWriteOffRepository
            ) : base(advanceRepository, unitOfWork, pkGeneratorService)
        {
            _employeeTransactionTypeGLService = employeeTransactionTypeGLService;
            _employeeTransactionTypeGLRepository = employeeTransactionTypeGLRepository;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _advanceDetailRepository = advanceDetailRepository;
            _companyTaxYearService = companyTaxYearService;
            _voucherService = voucherService;
            _companyFiscalYearService = companyFiscalYearService;
            _bankMasterRepository = bankMasterRepository;
            _cashMasterRepository = cashMasterRepository;
            _companyPartyRepository = companyPartyRepository;
            _companyPartyGLRepository = companyPartyGLRepository;
            _bankChargeService = bankChargeService;
            _taxCodeRepository = taxCodeRepository;
            _taxCodeGLRepository = taxCodeGLRepository;
            _invoiceTaxService = invoiceTaxService;
            _financingTypeGLService = financingTypeGLService;
            _invoiceWriteOffService = invoiceWriteOffService;
            _invoiceService = invoiceService;
            _exchangeGainLossService = exchangeGainLossService;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherRepository = voucherRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _gLTransactionDetailRepository = gLTransactionDetailRepository;
            _bankChargeRepository = bankChargeRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _invoiceWriteOffRepository = invoiceWriteOffRepository;
            _invoiceWriteOffDetailRepository = invoiceWriteOffDetailRepository;
            _adjustmentNoteRepository = adjustmentNoteRepository;
            _adjustmentNoteDetailRepository = adjustmentNoteDetailRepository;
            _employeePayableService = employeePayableService;
            _employeePayableWriteOffRepository = employeePayableWriteOffRepository;
            _employeePayableWriteOffDetailRepository = employeePayableWriteOffDetailRepository;
            _employeePayableRepository = employeePayableRepository;
            _employeePayableDetailRepository = employeePayableDetailRepository;
            _employeePayableWriteOffService = employeePayableWriteOffService;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _advanceWriteOffRepository = advanceWriteOffRepository;
            _advanceWriteOffDetailRepository = advanceWriteOffDetailRepository;
            _advanceReqScheduleRepository = advanceReqScheduleRepository;
            _employeeSalaryAdvanceRepository = employeeSalaryAdvanceRepository;
            _employeeSubsequentTransactionRepository = employeeSubsequentTransactionRepository;
            _bankReconciliationMapRepository = bankReconciliationMapRepository;
            _financingService = financingService;
            _loanInterestPayableRepository = loanInterestPayableRepository;
            _financingWriteOffRepository = financingWriteOffRepository;
        }

        #endregion Constructor

        public string MakeAdvanceDetailPK(string masterId, int currentId)
        {
            return MakePK(masterId, currentId, 1);
        }

        public Advance InsertAdvance(Advance advance)
        {
            advance.Id = GetAutoNumber(nameof(Advance), PKGeneratorEnum.Yearly, null, DateTime.Now);
            base.InsertGraph(advance);
            return advance;
        }

        public Advance InsertAdvance(VoucherViewModel voucherVM)
        {
            return InsertAdvance(new Advance
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
                EmployeeId = voucherVM.EmployeeId,
                PartyId = voucherVM.PartyId,
                PartyPlantId = voucherVM.PartyPlantId,
                ResponsiblePersonId = voucherVM.ResponsiblePersonId,
                Amount = voucherVM.Amount,
                VoucherDate = voucherVM.VoucherDate,
                PostingDate = voucherVM.PostingDate,
                ReviewDate = voucherVM.ReviewDate,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                Narration = voucherVM.Narration,
                PartyType = voucherVM.PartyType,
                SourceType = voucherVM.SourceType,
                PaymentSource = voucherVM.PaymentSource,
                BankMasterId = voucherVM.BankMasterId,
                CashMasterId = voucherVM.CashMasterId,
                JournalId = voucherVM.PaymentSource == PaymentSource.Journal.ToString() ? voucherVM.JournalId : null,
                BankAmount = voucherVM.BankAmount,
                EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId,
                IsInterTransaction = voucherVM.IsInterTransaction,
                FinancingTypeId = voucherVM.IsInterTransaction ? voucherVM.FinancingTypeId : null,
                IsPark = voucherVM.IsPark,
                JournalType = voucherVM.JournalType,
                SettlementType = voucherVM.SettlementType,
                RequisitionId = voucherVM.RequisitionId,
                CompanyCurrencyRate = voucherVM.CompanyCurrencyRate,
                POId = voucherVM.POId,
                ContractId = voucherVM.ContractId,
                MasterOrderId = voucherVM.MasterOrderId,
                AdvanceGroupNo = voucherVM.AdvanceGroupNo,
                AdditionalAmount = 0
            });
        }

        public InvoiceWriteOff InsertInvoiceWriteOff(InvoiceWriteOff invoiceWriteOff)
        {
            invoiceWriteOff.Id = GetAutoNumber(nameof(InvoiceWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
            _invoiceWriteOffService.InsertGraph(invoiceWriteOff);
            return invoiceWriteOff;
        }

        public InvoiceWriteOff InsertInvoiceWriteOff(VoucherViewModel voucherVM)
        {

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
                RoundingAmount = voucherVM.RoundingAmount
            };
            return InsertInvoiceWriteOff(invoiceWriteOff);
        }
        public void InsertInvoiceWriteOffDetail(InvoiceWriteOff invoiceWriteOff, InvoiceWriteOffDetail invoiceWriteOffDetail, int currentId)
        {
            invoiceWriteOffDetail.AddedBy = invoiceWriteOff.AddedBy;
            invoiceWriteOffDetail.AddedDate = invoiceWriteOff.AddedDate;
            invoiceWriteOffDetail.AddedFromIP = invoiceWriteOff.AddedFromIP;
            invoiceWriteOffDetail.Archive = invoiceWriteOff.Archive;
            invoiceWriteOffDetail.InvoiceWriteOffId = invoiceWriteOff.Id;
            invoiceWriteOffDetail.Id = MakePK(invoiceWriteOff.Id, currentId, 2);
            _invoiceWriteOffDetailRepository.Insert(invoiceWriteOffDetail);
        }


        private Advance UpdateAdvance(VoucherViewModel advanceVM)
        {
            var advance = Find(advanceVM.Id);
            CheckIsPosted(advance);

            advance.PartyId = advanceVM.PartyId;
            advance.PartyPlantId = advanceVM.PartyPlantId;
            advance.PostingDate = advanceVM.PostingDate;
            advance.DocDate = advanceVM.DocDate;
            advance.DocRefNo = advanceVM.DocRefNo;
            advance.EntityId = advanceVM.EntityId;
            advance.ReviewDate = advanceVM.ReviewDate;
            advance.ResponsiblePersonId = advanceVM.ResponsiblePersonId;
            advance.PaymentSource = advanceVM.PaymentSource;
            advance.BankMasterId = advanceVM.BankMasterId;
            advance.CashMasterId = advanceVM.CashMasterId;
            advance.JournalId = advanceVM.JournalId;
            advance.Amount = advanceVM.Amount;
            advance.BankAmount = advanceVM.BankAmount;
            advance.CurrencyId = advanceVM.CurrencyId;
            advance.Narration = advanceVM.Narration;
            advance.IsInterTransaction = advanceVM.IsInterTransaction;
            advance.EmployeeTransactionTypeId = advanceVM.EmployeeTransactionTypeId;
            advance.FinancingTypeId = advanceVM.FinancingTypeId;
            advance.FiscalYearId = advanceVM.FiscalYearId;
            advance.FiscalYearPeriodId = advanceVM.FiscalYearPeriodId;
            advance.TaxYearId = advanceVM.TaxYearId;
            advance.TaxYearPeriodId = advanceVM.TaxYearPeriodId;
            base.UpdateGraph(advance);
            return advance;
        }

        public AdvanceDetail InsertAdvanceDetail(AdvanceDetail advanceDetail)
        {
            _advanceDetailRepository.Insert(advanceDetail);
            return advanceDetail;
        }

        public AdvanceDetail InsertAdvanceDetail(Advance advance, AdvanceDetail advanceDetail, int currentAdvanceDetaiId)
        {
            advanceDetail.Id = MakeAdvanceDetailPK(advance.Id, currentAdvanceDetaiId);
            advanceDetail.AdvanceId = advance.Id;
            advanceDetail.AddedBy = advance.AddedBy;
            advanceDetail.AddedDate = advance.AddedDate;
            advanceDetail.AddedFromIP = advance.AddedFromIP;
            advanceDetail.Archive = advance.Archive;
            _advanceDetailRepository.Insert(advanceDetail);
            return advanceDetail;
        }

        public AdvanceDetail InsertAdvanceDetail(Advance advance, int currentAdvanceDetaiId, VoucherDetailViewModel advanceDetailVM)
        {
            var advanceDetail = new AdvanceDetail
            {
                Id = MakeAdvanceDetailPK(advance.Id, currentAdvanceDetaiId),
                AdvanceId = advance.Id,
                CompanyId = advanceDetailVM.CompanyId,
                PlantId = advanceDetailVM.PlantId,
                PartyId = advanceDetailVM.PartyId,
                PartyPlantId = advanceDetailVM.PartyPlantId,
                EmployeeId = advanceDetailVM.EmployeeId,
                PartyType = advanceDetailVM.PartyType,
                PaymentType = advanceDetailVM.PaymentType,
                GLGeneralInfoId = advanceDetailVM.GLGeneralInfoId,
                BudgetMasterId = advanceDetailVM.BudgetMasterId,
                ActivityId = advanceDetailVM.ActivityId,
                AddedBy = advance.AddedBy,
                AddedDate = advance.AddedDate,
                AddedFromIP = advance.AddedFromIP,
                Archive = advance.Archive,
                Narration = advanceDetailVM.Narration,
                Amount = advanceDetailVM.Amount,
                NetAmount = advanceDetailVM.Amount,
                AdditionalAmount = 0,
                BooksAmount = Math.Round((advanceDetailVM.Amount * advance.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
            };
            if (advanceDetail.PartyType != PartyType.Company.ToString())
            {
                advanceDetail.PartyType = advance.PartyType;
                advanceDetail.CompanyId = advance.CompanyId;
                advanceDetail.EmployeeId = advance.EmployeeId;
                advanceDetail.PlantId = advance.PlantId;
                advanceDetail.PartyId = advance.PartyId;
                advanceDetail.PartyPlantId = advance.PartyPlantId;
            }
            InsertAdvanceDetail(advanceDetail);
            return advanceDetail;
        }

        private AdvanceDetail UpdateAdvanceDetail(Advance advance, AdvanceDetail advanceDetail, VoucherDetailViewModel advanceDetailVM)
        {
            advanceDetail.PartyType = advanceDetailVM.PartyType;
            advanceDetail.CompanyId = advanceDetailVM.CompanyId;
            advanceDetail.PlantId = advanceDetailVM.PlantId;
            advanceDetail.EmployeeId = advanceDetailVM.EmployeeId;
            advanceDetail.PartyId = advanceDetailVM.PartyId;
            advanceDetail.PartyPlantId = advanceDetailVM.PartyPlantId;
            advanceDetail.GLGeneralInfoId = advanceDetailVM.GLGeneralInfoId;
            advanceDetail.BudgetMasterId = advanceDetailVM.BudgetMasterId;
            advanceDetail.ActivityId = advanceDetailVM.ActivityId;
            advanceDetail.Archive = advance.Archive;
            advanceDetail.Narration = advanceDetailVM.Narration;
            advanceDetail.Amount = advanceDetailVM.Amount;
            advanceDetail.NetAmount = advanceDetailVM.Amount;

            if (advanceDetail.PartyType != PartyType.Company.ToString())
            {
                advanceDetail.PartyType = advance.PartyType;
                advanceDetail.CompanyId = advance.CompanyId;
                advanceDetail.EmployeeId = advance.EmployeeId;
                advanceDetail.PlantId = advance.PlantId;
                advanceDetail.PartyId = advance.PartyId;
                advanceDetail.PartyPlantId = advance.PartyPlantId;
            }
            return UpdateAdvanceDetail(advanceDetail);
        }

        public AdvanceDetail UpdateAdvanceDetail(AdvanceDetail advanceDetail)
        {
            _advanceDetailRepository.Update(advanceDetail);
            return advanceDetail;
        }

        public Dictionary<string, object> GetById(string companyGroupId, string companyId, string plantId, string id)
        {
            var sql = @"SELECT A.Id AS AdvanceId, A.Id, BC.Id AS BankChargeId, A.EntityId, A.FiscalYearId, FY.FiscalYearName, A.FiscalYearPeriodId, FYP.PeriodName AS FiscalYearPeriodName, A.TaxYearId, A.TaxYearPeriodId, A.VoucherTypeId, A.VoucherId, A.CurrencyId
                        , A.FinancingTypeId, A.EmployeeTransactionTypeId, A.ResponsiblePersonId, EIRP.EmployeeName AS ResponsiblePerson, A.PartyType, A.EmployeeId, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
                        , A.PartyPlantId, A.PaymentSource, A.BankMasterId, BM.AccountTitle AS BankName, A.CashMasterId, CM.UserName AS CashName, A.JournalId, A.AdvanceNo, A.VoucherDate, A.PostingDate, A.DocDate, A.DocRefNo, A.ReviewDate, A.Narration
                        , A.Amount, A.BankAmount, A.IsPark, A.IsInterTransaction, A.IsPosted, VDC.ToCurrencyRate AS CompanyCurrencyRate
                        FROM [TRN].[Advance] AS A
                        LEFT JOIN [TRN].[AdvanceDetail] AS AD ON AD.AdvanceId=A.Id
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                        LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=A.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=A.FiscalYearPeriodId
                        LEFT JOIN [dbo].[EmployeeInformation] AS EIRP ON EIRP.SystemId=A.ResponsiblePersonId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=A.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=A.CashMasterId
						LEFT JOIN TRN.BankCharge AS BC ON BC.AdvanceId=A.Id
                        WHERE A.Archive=0 AND A.OpeningBalanceId IS NULL AND A.CompanyGroupId='" + companyGroupId + "' AND A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' AND A.Id='" + id + "'";
            return _sqlRepository.GetData(sql);
        }

        public Dictionary<string, object> GetAdvance(string companyGroupId, string companyId, string plantId, string advanceId)
        {
            var sql = @"SELECT A.Id AS AdvanceId, A.EntityId, A.PartyType, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId
                        , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                        , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                        , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                        FROM [TRN].[Advance] AS A
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                        LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                        LEFT JOIN(
                        SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                        , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                        , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                        FROM [HKP].[CompanyPartyGL] AS CPGL
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                        WHERE CPGL.PartyGLType='DownPaymentGL'
                        ) AS DGL ON DGL.CompanyPartyId=CP.Id
                        WHERE A.Archive=0 AND A.CompanyGroupId='" + companyGroupId + "' AND A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' AND A.Id='" + advanceId + "'";
            return _sqlRepository.GetData(sql);
        }

        public Dictionary<string, object> GetReportHeader(string voucherId)
        {
            var sql = @"SELECT V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
                        , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.Narration, P.UserName AS PartyName
                        , PP.UserName AS PartyPlantName, V.AddedBy, V.PostedBy, VT.UserName AS VoucherTypeName
                        FROM [TRN].[Advance] AS A
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                        LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                        WHERE A.Archive=0 AND V.Id='" + voucherId + "'";
            return _sqlRepository.GetData(sql);
        }

        public List<Dictionary<string, object>> GetPartyWiseOutstandingAdvance(string companyGroupId, string companyId, string plantId, string partyId, SourceType sourceType)
        {
            var sql = @"SELECT P.UserName As PartyName, PP.UserName AS PartyPlantName, SUM(AD.Amount-AD.WrittenOffAmount) AS Balance
                        FROM [TRN].[AdvanceDetail] AS AD
                        LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
                        WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='" + sourceType + @"'
                        AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.PartyId='" + partyId + @"'
                        GROUP BY P.UserName, PP.UserName ";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetPartyWiseOutstandingDebitNote(string companyGroupId, string companyId, string plantId, string partyId, SourceType sourceType)
        {
            var sql = @"SELECT P.UserName As PartyName, PP.UserName AS PartyPlantName, SUM(AD.Amount-AD.WrittenOffAmount) AS Balance
                        FROM [TRN].[AdjustmentNoteDetail] AS AD
                        LEFT JOIN [TRN].[AdjustmentNote] AS AM ON AD.AdjustmentNoteId=AM.Id
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
                        WHERE AM.Archive=0 AND AM.IsPark=0 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='DebitNote'
                        AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.PartyId='" + partyId + @"'
                        GROUP BY P.UserName, PP.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }



        public List<Dictionary<string, object>> GetDetail(string advanceId)
        {
            var sql = @"SELECT AD.Id AS AdvanceDetailId, AD.Id, AD.AdvanceId, AD.PartyType, AD.CompanyId, AD.PlantId, AD.EmployeeId, AD.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, AD.PartyPlantId, PP.UserName AS PartyPlantName
                        , AD.GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName, AD.BudgetMasterId, B.UserName AS BudgetName, AD.ActivityId, A.UserName AS ActivityName, AD.RefId
                        , AD.Amount, AD.TaxAmount, AD.NetAmount, AD.Narration, AD.PaymentType, VDC.CrAmount AS CompanyCurrencyAmount
                        FROM [TRN].[AdvanceDetail] AS AD
						LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
						LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=AD.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AD.PartyPlantId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=AD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                        WHERE AD.Archive=0 AND AD.AdvanceId='" + advanceId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, A.Id, A.Id As AdvanceId--,BC.Id AS BankChargeId
                                , A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId, PP.UserName AS PartyPlantName, A.EmployeeId, EI.EmployeeCode
                                 , EI.EmployeeName, EIR.EmployeeCode AS ResponsibleCode,EIR.EmployeeName AS ResponsibleName, A.VoucherId, A.PostingDate, A.DocDate, A.DocRefNo
                                 , A.CurrencyId, C.Code AS CurrencyCode, A.Amount, A.IsWrittenOff, A.WrittenOffAmount, A.IsPark, A.IsInterTransaction, A.IsPosted, AD.NetAmount
                                 , Status = case when A.IsPark = 0 then 'Posted' else 'Parked' end,A.AdvanceGroupNo
                                 FROM [TRN].[Advance] AS A
                                 LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                 LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=A.EmployeeId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=A.ResponsiblePersonId
                                 LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                 LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                                 --LEFT JOIN [TRN].[BankCharge] AS BC ON BC.AdvanceId=A.Id
                                LEFT JOIN (SELECT AdvanceId, PartyId, NetAmount FROM [TRN].[AdvanceDetail]
                                ) AS AD ON AD.AdvanceId=A.Id AND AD.PartyId=A.PartyId
                                WHERE A.OpeningBalanceId IS NULL AND A.Archive=0 AND V.Archive=0 AND A.CompanyGroupId='" + companyGroupId + "'AND A.CompanyId='" + companyId + @"' 
                                AND A.PlantId='" + plantId + "' AND A.SourceType='" + sourceType + @"'  AND A.AdvanceGroupNo IS NULL
                                UNION ALL
								SELECT VoucherNo=STUFF((SELECT DISTINCT ','+xpo.VoucherNo from
                                    			[TRN].Voucher xpo
                                    			INNER JOin trn.[Advance] xPDAMAP on xpo.Id=xPDAMAP.VoucherId
                                    			WHERE A.AdvanceGroupNo=xPDAMAP.AdvanceGroupNo for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												,NULL  Id, NULL AdvanceId, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId, PP.UserName AS PartyPlantName, A.EmployeeId, EI.EmployeeCode
                                 , EI.EmployeeName, EIR.EmployeeCode AS ResponsibleCode,EIR.EmployeeName AS ResponsibleName, NULL VoucherId, A.PostingDate, A.DocDate, A.DocRefNo
                                 , A.CurrencyId, C.Code AS CurrencyCode, SUM(A.Amount) Amount, A.IsWrittenOff, SUM(A.WrittenOffAmount) WrittenOffAmount, A.IsPark
								 , A.IsInterTransaction, A.IsPosted, SUM(AD.NetAmount) NetAmount
                                 , Status = case when A.IsPark = 0 then 'Posted' else 'Parked' end,A.AdvanceGroupNo

                                 FROM [TRN].[Advance] AS A
                                 LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                 LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=A.EmployeeId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=A.ResponsiblePersonId
                                 LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                 LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                                LEFT JOIN (SELECT AdvanceId, PartyId, NetAmount FROM [TRN].[AdvanceDetail]
                                ) AS AD ON AD.AdvanceId=A.Id AND AD.PartyId=A.PartyId
                                WHERE A.OpeningBalanceId IS NULL AND A.Archive=0 AND V.Archive=0 AND A.CompanyGroupId='" + companyGroupId + "'AND A.CompanyId='" + companyId + @"' 
                                AND A.PlantId='" + plantId + "' AND A.SourceType='" + sourceType + @"' and A.AdvanceGroupNo IS NOT NULL

                                Group By A.PartyId, P.Code, P.UserName, A.PartyPlantId, PP.UserName, A.EmployeeId, EI.EmployeeCode
                                 , EI.EmployeeName, EIR.EmployeeCode,EIR.EmployeeName, A.PostingDate, A.DocDate, A.DocRefNo
                                 , A.CurrencyId, C.Code  , A.IsWrittenOff,A.AdvanceGroupNo,A.IsPark , A.IsInterTransaction, A.IsPosted
";
            parameters.sort = " PostingDate DESC, VoucherNo";
            parameters.order = "DESC";
            return _sqlRepository.GetGridData(parameters);
        }


        public GridModel GetCustomerPaymentList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, A.Id, A.Id As AdvanceId, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId, PP.UserName AS PartyPlantName, A.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , A.VoucherId, A.PostingDate, A.DocDate, A.DocRefNo, A.CurrencyId, C.Code AS CurrencyCode, A.Amount, A.IsWrittenOff, A.WrittenOffAmount, A.IsPark, A.IsInterTransaction, A.IsPosted
                                FROM [TRN].[Advance] AS A
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=A.EmployeeId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                                WHERE A.OpeningBalanceId IS NULL AND A.Archive=0 AND A.CompanyGroupId='" + companyGroupId + "'AND A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' AND A.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }



        public Dictionary<string, object> Query(string companyGroupId, string companyId, string plantId, string partyId, string advanceId, SourceType sourceType)
        {
            var cmdText = @"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received
                                , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
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
                                WHERE AM.Archive=0 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType='" + sourceType + @"'
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "' AND AM.PartyId='" + partyId + "' AND AD.AdvanceId='" + advanceId + "'";
            return _sqlRepository.GetData(cmdText);
        }

        public string InsertCrAdvance(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var flag = false;
            try
            {
                if (advanceVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(advanceVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                if (advanceVM.PaymentSource == PaymentSource.Cash.ToString() && string.IsNullOrEmpty(advanceVM.CashMasterId))
                    throw new CustomException("Cash Id not found!");

                if (null == advanceDetailVMList || advanceDetailVMList.Count() < 0)
                    throw new CustomException("Payment Receipt list is null.");

                if (advanceVM.PaymentSource == PaymentSource.Bank.ToString())
                    advanceVM.CashMasterId = null;

                if (advanceVM.PaymentSource == PaymentSource.Cash.ToString())
                    advanceVM.BankMasterId = null;

                _companyParallelCurrencyService.GetParallelCurrency(advanceVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(advanceVM);
                _companyTaxYearService.CheckingTaxYearPeriod(advanceVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Advance
                var advance = InsertAdvance(advanceVM);

                // INSERT INTO Voucher
                var voucher = InsertVoucher(advance, advanceVM.FiscalYearPrefix);

                var currentVoucherDetailId = 0;
                // INSERT INTO VoucherDetail (Bank or cash side Dr)
                var voucherDetailDr = new VoucherDetail
                {
                    VoucherId = voucher.Id,
                    EntityId = voucher.EntityId,
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    CurrencyId = voucher.CurrencyId,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = voucher.Narration,
                    IsPark = voucher.IsPark,
                    AddedBy = voucher.AddedBy,
                    AddedDate = voucher.AddedDate,
                    AddedFromIP = voucher.AddedFromIP,
                    GLGeneralInfoId = advanceVM.GLGeneralInfoId,
                    BudgetMasterId = advanceVM.BudgetMasterId,
                    ActivityId = advanceVM.ActivityId,
                    DrAmount = advance.Amount,
                    BankMasterId = advance.BankMasterId,
                    CashMasterId = advance.CashMasterId,
                    Archive = advance.Archive,
                    PaymentSource = advance.PaymentSource
                };
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                // INSRT INTO GLTransactionDetail
                if (advance.PaymentSource == PaymentSource.Bank.ToString() || advance.PaymentSource == PaymentSource.Cash.ToString())
                {
                    _voucherService.InsertGLTransactionDetail(voucherDetailDr, new GLTransactionDetail
                    {
                        VoucherDetailId = voucherDetailDr.Id,
                        SourceType = advance.PaymentSource,
                        BankMasterId = voucherDetailDr.BankMasterId,
                        CashMasterId = voucherDetailDr.CashMasterId,
                        DrAmount = advance.BankAmount
                    });
                }
                else if (advance.PaymentSource == PaymentSource.Journal.ToString())
                {
                    advance.JournalId = advanceVM.JournalId;
                    var payableAdvance = Find(advanceVM.JournalId);
                    payableAdvance.IsPosted = true;
                    payableAdvance.JournalId = advance.Id;
                    base.UpdateGraph(payableAdvance);
                }

                // INSERT INTO VoucherDetailCurrency
                foreach (var voucherDetailCurrencyVM in voucherDetailCurrencyVMList.Where(r => r.TrnType == "Dr" && r.GLGeneralInfoId == voucherDetailDr.GLGeneralInfoId))
                {
                    InsertVoucherDetailCurrencyDr(advanceVM.CurrencyId, companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, voucherDetailDr, voucherDetailCurrencyVM);
                }

                var currentAdvanceDetaiId = 0;
                var currentInterTransactioneDetaiId = 0;
                foreach (var advanceDetailVM in advanceDetailVMList)
                {
                    InsertAdvanceDetailCr(advanceVM, voucherDetailCurrencyVMList, companyCurrencyId, advanceVM.FiscalYearPrefix, advance, voucher, ref currentVoucherDetailId, ref currentAdvanceDetaiId, ref currentInterTransactioneDetaiId, advanceDetailVM);
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

        private void InsertAdvanceDetailCr(VoucherViewModel advanceVM, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, string companyCurrencyId, string fiscalYearPrefix, Advance advance, Voucher voucher, ref int currentVoucherDetailId, ref int currentAdvanceDetaiId, ref int currentInterTransactioneDetaiId, VoucherDetailViewModel advanceDetailVM)
        {
            currentAdvanceDetaiId++;
            // INSERT INTO AdvanceDetail
            var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, advanceDetailVM);

            // INSERT INTO VoucherDetail (liability side Cr)
            var voucherDetail = new VoucherDetail
            {
                VoucherId = voucher.Id,
                EntityId = voucher.EntityId,
                FiscalYearId = voucher.FiscalYearId,
                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                CurrencyId = voucher.CurrencyId,
                DocDate = voucher.DocDate,
                DocRefNo = voucher.DocRefNo,
                Narration = advanceDetail.Narration,
                IsPark = voucher.IsPark,
                AddedBy = voucher.AddedBy,
                AddedDate = voucher.AddedDate,
                AddedFromIP = voucher.AddedFromIP,
                GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                BudgetMasterId = advanceDetail.BudgetMasterId,
                ActivityId = advanceDetail.ActivityId,
                CrAmount = advanceDetail.NetAmount,
                PartyId = advanceDetail.PartyId,
                PartyType = advanceDetail.PartyType,
                PartyPlantId = advanceDetail.PartyPlantId,
                AdvanceDetailId = advanceDetail.Id
            };

            if (advanceDetail.PartyType == PartyType.Company.ToString())
            {
                if (advance.IsInterTransaction)
                {
                    var interTransaction = new Advance
                    {
                        AddedBy = advance.AddedBy,
                        AddedDate = advance.AddedDate,
                        AddedFromIP = advance.AddedFromIP,
                        Amount = advance.Amount,
                        Archive = advance.Archive,
                        CompanyGroupId = advance.CompanyGroupId,
                        CompanyId = advance.CompanyId,
                        PlantId = advance.PlantId,
                        CurrencyId = advance.CurrencyId,
                        DocDate = advance.DocDate,
                        DocRefNo = advance.DocRefNo,
                        PostingDate = advance.PostingDate,
                        VoucherDate = advance.VoucherDate,
                        EntityId = advance.EntityId,
                        FinancingTypeId = advance.FinancingTypeId,
                        FiscalYearId = advance.FiscalYearId,
                        FiscalYearPeriodId = advance.FiscalYearPeriodId,
                        IsPark = advance.IsPark,
                        IsPosted = advance.IsPosted,
                        IsInterTransaction = advance.IsInterTransaction,
                        Narration = advance.Narration,
                        PartyId = advance.PartyId,
                        PartyType = advance.PartyType,
                        SourceType = advance.SourceType,
                        VoucherId = voucher.Id,
                        VoucherTypeId = voucher.VoucherTypeId,
                        PartyPlantId = advanceDetailVM.PartyPlantId,
                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId
                    };
                    // TODO: have to update here
                    // InsertAdvance(interTransaction, fiscalYearPrefix);

                    var interTransactionDetail = new AdvanceDetail
                    {
                        AddedBy = advanceDetail.AddedBy,
                        AddedDate = advanceDetail.AddedDate,
                        AddedFromIP = advanceDetail.AddedFromIP,
                        AdvanceId = advanceDetail.Id,
                        Amount = advanceDetail.Amount,
                        NetAmount = advanceDetail.NetAmount,
                        Archive = advanceDetail.Archive,
                        CompanyId = advanceDetail.CompanyId,
                        PlantId = advanceDetail.PlantId,
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId,
                        Narration = advanceDetail.Narration,
                        PartyPlantId = advanceDetail.PartyPlantId,
                        PartyId = advanceDetail.PartyId,
                        PartyType = advanceDetail.PartyType,
                        TaxAmount = advanceDetail.TaxAmount
                    };
                    currentInterTransactioneDetaiId++;
                    // TODO: have to update.
                    //_interTransactionService.InsertInterTransactionDetail(interTransaction, interTransactionDetail, currentInterTransactioneDetaiId);

                    voucherDetail.InterTransactionDetailId = interTransactionDetail.Id;
                }
            }
            currentVoucherDetailId++;
            _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

            foreach (var voucherDetailCurrencyVM in voucherDetailCurrencyVMList.Where(r => r.TrnType == "Cr" && r.GLGeneralInfoId == voucherDetail.GLGeneralInfoId))
            {
                InsertVoucherDetailCurrencyCr(advanceVM.CurrencyId, companyCurrencyId, voucherDetail, voucherDetailCurrencyVM);
            }
        }

        public string UpdateCrAdvance(VoucherViewModel advanceVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(advanceVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(advanceVM);
                _companyTaxYearService.CheckingTaxYearPeriod(advanceVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                // UPDATE Advance
                var advance = UpdateAdvance(advanceVM);

                // UPDATE Voucher
                var voucher = UpdateVoucher(advance);

                var voucherDetailList = _voucherService.GetVoucherDetailList(r => r.VoucherId == advance.VoucherId).Select().ToList();
                // INSERT INTO VoucherDetail (Bank or cash side Dr)
                var voucherDetailDr = voucherDetailList.FirstOrDefault(r => r.PaymentSource == advance.PaymentSource);
                voucherDetailDr.EntityId = voucher.EntityId;
                voucherDetailDr.FiscalYearId = voucher.FiscalYearId;
                voucherDetailDr.FiscalYearPeriodId = voucher.FiscalYearPeriodId;
                voucherDetailDr.CurrencyId = voucher.CurrencyId;
                voucherDetailDr.DocDate = voucher.DocDate;
                voucherDetailDr.DocRefNo = voucher.DocRefNo;
                voucherDetailDr.Narration = voucher.Narration;
                voucherDetailDr.IsPark = voucher.IsPark;
                voucherDetailDr.GLGeneralInfoId = advanceVM.GLGeneralInfoId;
                voucherDetailDr.BudgetMasterId = advanceVM.BudgetMasterId;
                voucherDetailDr.ActivityId = advanceVM.ActivityId;
                voucherDetailDr.DrAmount = advance.Amount;
                voucherDetailDr.BankMasterId = advance.BankMasterId;
                voucherDetailDr.CashMasterId = advance.CashMasterId;
                voucherDetailDr.PaymentSource = advance.PaymentSource;
                _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);

                // INSRT INTO GLTransactionDetail
                if (advance.PaymentSource == PaymentSource.Bank.ToString() || advance.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherDetailDr.BankMasterId) || !string.IsNullOrEmpty(voucherDetailDr.CashMasterId))
                    {
                        var glTransactionDetail = _voucherService.GetGLTransactionDetailList(r => r.VoucherDetailId == voucherDetailDr.Id).Select().FirstOrDefault();
                        glTransactionDetail.SourceType = advance.PaymentSource;
                        glTransactionDetail.BankMasterId = voucherDetailDr.BankMasterId;
                        glTransactionDetail.CashMasterId = voucherDetailDr.CashMasterId;
                        glTransactionDetail.DrAmount = advance.BankAmount;
                        _voucherService.UpdateGLTransactionDetail(voucherDetailDr, glTransactionDetail);
                    }
                    else
                        throw new CustomException("Bank or Cash Id not found!");
                }

                // Get voucher currency list data.
                var voucherDetailCurrencyList = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id).Select().ToList();
                // INSERT INTO VoucherDetailCurrency
                foreach (var voucherDetailCurrencyVM in voucherDetailCurrencyVMList.Where(r => r.TrnType == "Dr" && r.GLGeneralInfoId == voucherDetailDr.GLGeneralInfoId))
                {
                    var voucherDetailCurrencyDrList = voucherDetailCurrencyList.Where(r => r.VoucherDetailId == voucherDetailDr.Id).ToList();
                    UpdateVoucherDetailCurrencyDr(advanceVM.CurrencyId, companyCurrencyId, voucherDetailDr, voucherDetailCurrencyVM, voucherDetailCurrencyDrList);
                }
                var advanceDetailDbList = _advanceDetailRepository.Query(r => r.AdvanceId == advance.Id).Select().ToList();
                var currentAdvanceDetaiId = advanceDetailDbList.Count;
                var currentInterTransactioneDetaiId = 0;
                var currentVoucherDetailId = voucherDetailList.Count;
                var interTransactionDbList = Query(r => r.Id == advance.Id).Select().ToList();
                foreach (var advanceDetailVM in advanceDetailVMList)
                {
                    // INSERT INTO AdvanceDetail
                    var advanceDetailCr = advanceDetailDbList.FirstOrDefault(r => r.Id == advanceDetailVM.Id);
                    if (null != advanceDetailCr)
                    {
                        var advanceDetail = UpdateAdvanceDetail(advance, advanceDetailCr, advanceDetailVM);
                        // INSERT INTO VoucherDetail (liability side Cr)

                        var voucherDetail = voucherDetailList.FirstOrDefault(r => r.AdvanceDetailId == advanceDetailCr.Id);
                        voucherDetail.EntityId = voucher.EntityId;
                        voucherDetail.FiscalYearId = voucher.FiscalYearId;
                        voucherDetail.FiscalYearPeriodId = voucher.FiscalYearPeriodId;
                        voucherDetail.CurrencyId = voucher.CurrencyId;
                        voucherDetail.DocDate = voucher.DocDate;
                        voucherDetail.DocRefNo = voucher.DocRefNo;
                        voucherDetail.Narration = advanceDetailCr.Narration;
                        voucherDetail.IsPark = voucher.IsPark;
                        voucherDetail.GLGeneralInfoId = advanceDetailCr.GLGeneralInfoId;
                        voucherDetail.BudgetMasterId = advanceDetailCr.BudgetMasterId;
                        voucherDetail.ActivityId = advanceDetailCr.ActivityId;
                        voucherDetail.CrAmount = advanceDetailCr.NetAmount;
                        voucherDetail.PartyId = advanceDetailCr.PartyId;
                        voucherDetail.PartyType = advanceDetailCr.PartyType;
                        voucherDetail.PartyPlantId = advanceDetailCr.PartyPlantId;

                        if (advanceDetailCr.PartyType == PartyType.Company.ToString())
                        {
                            if (advance.IsInterTransaction)
                            {
                                var interTransaction = interTransactionDbList.FirstOrDefault(r => r.CompanyId == advance.CompanyId && r.PlantId == advance.PlantId);
                                interTransaction.Amount = advance.Amount;
                                interTransaction.Archive = advance.Archive;
                                interTransaction.CurrencyId = advance.CurrencyId;
                                interTransaction.DocDate = advance.DocDate;
                                interTransaction.DocRefNo = advance.DocRefNo;
                                interTransaction.PostingDate = advance.PostingDate;
                                interTransaction.VoucherDate = advance.VoucherDate;
                                interTransaction.EntityId = advance.EntityId;
                                interTransaction.FinancingTypeId = advance.FinancingTypeId;
                                interTransaction.FiscalYearId = advance.FiscalYearId;
                                interTransaction.FiscalYearPeriodId = advance.FiscalYearPeriodId;
                                interTransaction.IsPark = advance.IsPark;
                                interTransaction.IsPosted = advance.IsPosted;
                                interTransaction.IsInterTransaction = advance.IsInterTransaction;
                                interTransaction.Narration = advance.Narration;
                                interTransaction.PartyId = advance.PartyId;
                                //interTransaction.InterTransactionNo = advance.AdvanceNo;
                                interTransaction.PartyType = advance.PartyType;
                                interTransaction.SourceType = advance.SourceType;
                                interTransaction.VoucherId = voucher.Id;
                                interTransaction.VoucherTypeId = voucher.VoucherTypeId;
                                interTransaction.PartyPlantId = advanceDetailVM.PartyPlantId;
                                interTransaction.TaxYearId = voucher.TaxYearId;
                                interTransaction.TaxYearPeriodId = voucher.TaxYearPeriodId;
                                //_interTransactionService.UpdateInterTransaction(interTransaction);

                                // Update inter transaction detail table for inter company/plant
                                //var interTransactionDetail = _interTransactionService.GetInterTransactionDetailList(r => r.InterTransactionId == interTransaction.Id).Select().FirstOrDefault();
                                //interTransactionDetail.Amount = advanceDetail.Amount;
                                //interTransactionDetail.NetAmount = advanceDetail.NetAmount;
                                //interTransactionDetail.Archive = advanceDetail.Archive;
                                //interTransactionDetail.CompanyId = advanceDetail.CompanyId;
                                //interTransactionDetail.PlantId = advanceDetail.PlantId;
                                //interTransactionDetail.GLGeneralInfoId = advanceDetail.GLGeneralInfoId;
                                //interTransactionDetail.BudgetMasterId = advanceDetail.BudgetMasterId;
                                //interTransactionDetail.ActivityId = advanceDetail.ActivityId;
                                //interTransactionDetail.Narration = advanceDetail.Narration;
                                //interTransactionDetail.PartyPlantId = advanceDetail.PartyPlantId;
                                //interTransactionDetail.PartyId = advanceDetail.PartyId;
                                //interTransactionDetail.PartyType = advanceDetail.PartyType;
                                //interTransactionDetail.TaxAmount = advanceDetail.TaxAmount;
                                //_interTransactionService.UpdateInterTransactionDetail(interTransactionDetail);
                            }
                        }
                        // Update voucher detail credit side.
                        _voucherService.UpdateVoucherDetail(voucher, voucherDetail);
                        foreach (var voucherDetailCurrencyVM in voucherDetailCurrencyVMList.Where(r => r.TrnType == "Cr" && r.GLGeneralInfoId == voucherDetail.GLGeneralInfoId))
                        {
                            var voucherDetailCurrencyCrList = voucherDetailCurrencyList.Where(r => r.VoucherDetailId == voucherDetail.Id).ToList();
                            UpdateVoucherDetailCurrencyCr(advanceVM.CurrencyId, companyCurrencyId, voucherDetail, voucherDetailCurrencyVM, voucherDetailCurrencyCrList);
                        }
                    }
                    else
                    {
                        InsertAdvanceDetailCr(advanceVM, voucherDetailCurrencyVMList, companyCurrencyId, advanceVM.FiscalYearPrefix, advance, voucher, ref currentVoucherDetailId, ref currentAdvanceDetaiId, ref currentInterTransactioneDetaiId, advanceDetailVM);
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

        public string UpdateCustomerAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");

                // UPDATE Advance
                var advance = Find(voucherVM.Id);
                CheckIsPosted(advance);
                voucherVM.CompanyId = advance.CompanyId;

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                advance.PartyId = voucherVM.PartyId;
                advance.PartyPlantId = voucherVM.PartyPlantId;
                advance.PostingDate = voucherVM.PostingDate;
                advance.DocDate = voucherVM.DocDate;
                advance.DocRefNo = voucherVM.DocRefNo;
                advance.EntityId = voucherVM.EntityId;
                advance.ReviewDate = voucherVM.ReviewDate;
                advance.ResponsiblePersonId = voucherVM.ResponsiblePersonId;
                advance.PaymentSource = voucherVM.PaymentSource;
                advance.BankMasterId = voucherVM.BankMasterId;
                advance.CashMasterId = voucherVM.CashMasterId;
                advance.JournalId = voucherVM.JournalId;
                advance.Amount = voucherVM.Amount;
                advance.CurrencyId = voucherVM.CurrencyId;
                advance.Narration = voucherVM.Narration;
                advance.IsInterTransaction = voucherVM.IsInterTransaction;
                advance.EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId;
                advance.FinancingTypeId = voucherVM.FinancingTypeId;
                advance.FiscalYearId = voucherVM.FiscalYearId;
                advance.FiscalYearPeriodId = voucherVM.FiscalYearPeriodId;
                advance.TaxYearId = voucherVM.TaxYearId;
                advance.TaxYearPeriodId = voucherVM.TaxYearPeriodId;
                advance.ContractId = voucherVM.ContractId;
                advance.MasterOrderId = voucherVM.MasterOrderId;
                base.UpdateGraph(advance);

                // INSERT INTO Voucher
                var voucher = _voucherService.FindVoucher(advance.VoucherId);
                voucher.CurrencyId = advance.CurrencyId;
                voucher.DocDate = advance.DocDate;
                voucher.DocRefNo = advance.DocRefNo;
                voucher.EntityId = advance.EntityId;
                voucher.FiscalYearId = advance.FiscalYearId;
                voucher.FiscalYearPeriodId = advance.FiscalYearPeriodId;
                voucher.PostingDate = advance.PostingDate;
                voucher.TaxYearId = advance.TaxYearId;
                voucher.TaxYearPeriodId = advance.TaxYearPeriodId;
                _voucherService.UpdateVoucher(voucher);

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                decimal taxDrCurrencyAmount = 0;
                var withholdgl = false;

                var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);
                var voucherDetailDbList = _voucherService.GetVoucherDetailList(r => r.VoucherId == voucher.Id).Select().ToList();
                var voucherDetailCurrencyDbList = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id).Select().ToList();

                var companyPartyGLList = new List<CompanyPartyGL>();
                if (voucherVM.PartyType == PartyType.Customer.ToString())
                {
                    var companyParty = _companyPartyRepository.Query(r => r.CompanyId == advance.CompanyId && r.PlantId == advance.PlantId && r.PartyId == advance.PartyId && r.PartyType == voucherVM.PartyType).Select().FirstOrDefault();
                    if (null == companyParty)
                        throw new CustomException("Plant party mapping not found!");
                    companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                    if (null == companyPartyGLList)
                        throw new CustomException("Party GL not found!");
                }

                var advanceDetailDbList = GetAdvanceDetailList(r => r.AdvanceId == advance.Id).Select().ToList();
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var advanceDetail = advanceDetailDbList.FirstOrDefault();
                    if (null == advanceDetail)
                        throw new CustomException("Payment detail row not found!");

                    advanceDetail.Narration = advance.Narration;
                    if (voucherVM.PartyType == PartyType.Customer.ToString())
                    {
                        var reconGL = PartyGLType.DownPaymentGL.ToString();
                        var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                        if (null == regularGL)
                            throw new CustomException("Party DownPayment GL not found!");
                        voucherDetailVM.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = regularGL.BudgetMasterId;
                        voucherDetailVM.ActivityId = regularGL.ActivityId;
                    }
                    else if (voucherVM.PartyType == PartyType.Employee.ToString())
                    {
                        voucherDetailVM.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                        voucherDetailVM.ActivityId = voucherDetailVM.ActivityId;
                    }
                    else
                        throw new CustomException("Party type is null.");

                    UpdateAdvanceDetail(advance, advanceDetail, voucherDetailVM);

                    var voucherDetail = voucherDetailDbList.FirstOrDefault(r => r.AdvanceDetailId == advanceDetail.Id);
                    voucherDetail.Narration = advanceDetail.Narration;
                    voucherDetail.GLGeneralInfoId = advanceDetail.GLGeneralInfoId;
                    voucherDetail.BudgetMasterId = advanceDetail.BudgetMasterId;
                    voucherDetail.ActivityId = advanceDetail.ActivityId;
                    voucherDetail.CrAmount = advance.Amount;
                    voucherDetail.PartyType = advanceDetail.PartyType;
                    voucherDetail.EmployeeId = advanceDetail.EmployeeId;
                    voucherDetail.PartyId = advanceDetail.PartyId;
                    voucherDetail.PartyPlantId = advanceDetail.PartyPlantId;
                    voucherDetail.AdvanceDetailId = advanceDetail.Id;
                    _voucherService.UpdateVoucherDetail(voucher, voucherDetail);

                    var voucherDetailCurrency = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetail.Id);
                    voucherDetailCurrency.ParallelCurrencyId = companyCurrencyId;
                    voucherDetailCurrency.FromCurrencyId = voucherDetail.CurrencyId;
                    voucherDetailCurrency.ToCurrencyId = companyCurrencyId;
                    voucherDetailCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                    voucherDetailCurrency.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                    voucherDetailCurrency.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;
                    _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetail, voucherDetailCurrency);

                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;
                }

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var bankChargeDbList = _bankChargeService.GetBankChargeList(r => r.AdvanceId == advance.Id).Select().ToList();
                    var currentBankChargeDetailId = _bankChargeService.GetBankChargePKForAdvance(advance.Id);
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(advance.CompanyId, bankChargeDetailVM.FinancingTypeId);

                        if (string.IsNullOrEmpty(bankChargeDetailVM.BankChargeId))
                        {
                            currentBankChargeDetailId++;
                            var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                            {
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                AdvanceId = advance.Id,
                                BankMasterId = advance.BankMasterId,
                                CashMasterId = advance.CashMasterId,
                                Archive = advance.Archive,
                                SourceType = advance.SourceType,
                                Amount = bankChargeDetailVM.Amount,
                                Narration = advance.Narration,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            }, currentBankChargeDetailId);

                            // Insert Bank charges Debit
                            currentVoucherDetailId++;
                            var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankChargeId = bankChargeDetail.Id,
                                DrAmount = bankChargeDetail.Amount,
                                Narration = bankChargeDetail.Narration,
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
                            totalAmountCr += voucherDetailChargeDr.CrAmount;
                        }
                        else
                        {
                            var bankCharge = bankChargeDbList.FirstOrDefault(r => r.Id == bankChargeDetailVM.BankChargeId);
                            if (null == bankCharge)
                                throw new CustomException("Bank Charge row not found!");
                            bankCharge.FinancingTypeId = bankChargeDetailVM.FinancingTypeId;
                            bankCharge.BankMasterId = advance.BankMasterId;
                            bankCharge.CashMasterId = advance.CashMasterId;
                            bankCharge.Archive = advance.Archive;
                            bankCharge.SourceType = advance.SourceType;
                            bankCharge.Amount = bankChargeDetailVM.Amount;
                            bankCharge.Narration = advance.Narration;
                            _bankChargeService.UpdateBankCharge(bankCharge);

                            // Insert Bank charges Debit
                            var voucherDetailChargeDr = voucherDetailDbList.FirstOrDefault(r => r.BankChargeId == bankCharge.Id);
                            if (null == voucherDetailChargeDr)
                                throw new CustomException("Bank Charge voucher detail row not found!");

                            voucherDetailChargeDr.BankChargeId = bankCharge.Id;
                            voucherDetailChargeDr.DrAmount = bankCharge.Amount;
                            voucherDetailChargeDr.Narration = bankCharge.Narration;
                            voucherDetailChargeDr.GLGeneralInfoId = expenseGL.ExpensesGLId;
                            voucherDetailChargeDr.BudgetMasterId = expenseGL.ExpensesBudgetMasterId;
                            voucherDetailChargeDr.ActivityId = expenseGL.ExpensesActivityId;
                            _voucherService.UpdateVoucherDetail(voucher, voucherDetailChargeDr);

                            var voucherDetailCurrencyChargeDr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailChargeDr.Id);
                            if (null == voucherDetailCurrencyChargeDr)
                                throw new CustomException("Bank Charge voucher detail currency row not found!");

                            voucherDetailCurrencyChargeDr.ParallelCurrencyId = companyCurrencyId;
                            voucherDetailCurrencyChargeDr.FromCurrencyId = voucherDetailChargeDr.CurrencyId;
                            voucherDetailCurrencyChargeDr.ToCurrencyId = companyCurrencyId;
                            voucherDetailCurrencyChargeDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                            voucherDetailCurrencyChargeDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                            voucherDetailCurrencyChargeDr.DrAmount = bankChargeDetailVM.CompanyCurrencyAmount;
                            _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailChargeDr, voucherDetailCurrencyChargeDr);

                            totalAmountDr += voucherDetailChargeDr.DrAmount;
                            totalCurrencyAmountDr += bankChargeDetailVM.CompanyCurrencyAmount;
                            totalAmountCr += voucherDetailChargeDr.CrAmount;
                        }
                    }
                }
                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                {
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                    foreach (var invoiceTaxVM in taxDetailVMList)
                    {
                        var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                        if (null == taxCode)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");

                        var invoiceTax = new InvoiceTax
                        {
                            VoucherDetailId = voucherDetailTempId,
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            TaxAmount = invoiceTaxVM.TaxAmount,
                            TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                        };
                        taxDrAmount += invoiceTaxVM.TaxAmount;
                        _invoiceTaxService.InsertInvoiceTax(advance, invoiceTax, invoiceTaxPk);

                        // Insert Into Customer Invoice Tax Detail (Withhold GL)
                        withholdgl = taxCode.IsWithhold;
                        if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Cr"
                            };
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                CrAmount = invoiceTaxDetail.Amount,
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            taxDrCurrencyAmount += voucherDetailCurrencyTax.CrAmount;
                        }
                    }
                }
                // UPDATE VoucherDetail
                var voucherDetailDr = voucherDetailDbList.FirstOrDefault(r => r.PaymentSource == PaymentSource.Bank.ToString() || r.PaymentSource == PaymentSource.Cash.ToString());
                if (null == voucherDetailDr)
                    throw new CustomException("Bank or Cash type voucher detail row not found.");

                voucherDetailDr.Narration = voucher.Narration;
                voucherDetailDr.DrAmount = advance.Amount;
                voucherDetailDr.PaymentSource = advance.PaymentSource;

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    voucherDetailDr.DrAmount += bankChargeDetailVMList.Sum(r => r.Amount);
                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    voucherDetailDr.DrAmount -= taxDetailVMList.Sum(r => r.TaxAmount);
                totalAmountDr += voucherDetailDr.DrAmount;
                // INSRT INTO GLTransactionDetail
                var glTransactionDetail = _voucherService.FindGLTransactionDetail(voucherDetailDr.Id);
                glTransactionDetail.SourceType = voucherDetailDr.PaymentSource;
                glTransactionDetail.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;

                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    voucherDetailDr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    voucherDetailDr.BudgetMasterId = bankMaster.BudgetMasterId;
                    voucherDetailDr.ActivityId = bankMaster.ActivityId;
                    voucherDetailDr.BankMasterId = bankMaster.Id;
                    voucherDetailDr.PartyType = PartyType.Bank.ToString();

                    glTransactionDetail.BankMasterId = voucherDetailDr.BankMasterId;
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    voucherDetailDr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    voucherDetailDr.BudgetMasterId = cashMaster.BudgetMasterId;
                    voucherDetailDr.ActivityId = cashMaster.ActivityId;
                    voucherDetailDr.CashMasterId = cashMaster.Id;
                    voucherDetailDr.PartyType = PartyType.Cash.ToString();

                    glTransactionDetail.CashMasterId = voucherDetailDr.CashMasterId;
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);

                // Update VoucherDetailCurrency
                var voucherDetailCurrencyDr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailDr.Id);
                if (null == voucherDetailCurrencyDr)
                    throw new CustomException("Bank or Cash type voucher detail currency row not found.");

                voucherDetailCurrencyDr.ParallelCurrencyId = companyCurrencyId;
                voucherDetailCurrencyDr.FromCurrencyId = voucherDetailDr.CurrencyId;
                voucherDetailCurrencyDr.ToCurrencyId = companyCurrencyId;
                voucherDetailCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                voucherDetailCurrencyDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                voucherDetailCurrencyDr.DrAmount = (voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount) - taxDrCurrencyAmount;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrencyDr);

                // voucherDetailDr.CrAmount = voucherDetailCurrencyDr.CrAmount;
                _voucherService.UpdateGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                advance.BankAmount = totalCurrencyAmountDr - taxDrCurrencyAmount;
                totalCurrencyAmountCr += totalCurrencyAmountDr;

                //totalAmountDr -= taxDrAmount;
                //totalAmountCr += voucherDetailDr.CrAmount;

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                //if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                //    throw new CustomException("Dr and Cr amount is not equal.");

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

        public string InsertDrAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Advance
                var advance = InsertAdvance(voucherVM);
                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to Advance
                advance.VoucherId = voucher.Id;
                advance.AdvanceNo = voucher.VoucherNo;

                var currentVoucherDetailId = 0;
                var currentAdvanceDetaiId = 0;
                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                decimal taxDrCurrencyAmount = 0;
                var withholdgl = false;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    currentAdvanceDetaiId++;
                    // INSERT INTO AdvanceDetail
                    var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, voucherDetailVM);

                    // INSERT INTO VoucherDetail Party side
                    currentVoucherDetailId++;
                    var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                    {
                        Narration = advanceDetail.Narration,
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId,
                        DrAmount = advanceDetail.NetAmount,
                        PartyType = advanceDetail.PartyType,
                        EmployeeId = advanceDetail.EmployeeId,
                        PartyId = advanceDetail.PartyId,
                        PartyPlantId = advanceDetail.PartyPlantId,
                        AdvanceDetailId = advanceDetail.Id
                    }, currentVoucherDetailId);
                    voucherDetailTempId = voucherDetail.Id;
                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount), 2, MidpointRounding.AwayFromZero),
                    });

                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount), 2, MidpointRounding.AwayFromZero);
                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount), 2, MidpointRounding.AwayFromZero);
                }

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var currentBankChargeId = 0;
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        currentBankChargeId++;
                        var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                        {
                            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                            Amount = bankChargeDetailVM.Amount,
                            Narration = advance.Narration,
                            AddedBy = advance.AddedBy,
                            AddedDate = advance.AddedDate,
                            AddedFromIP = advance.AddedFromIP,
                            Archive = advance.Archive,
                            BankMasterId = advance.BankMasterId,
                            CashMasterId = advance.CashMasterId,
                            SourceType = advance.SourceType,
                            AdvanceId = advance.Id
                        }, currentBankChargeId);

                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(advance.CompanyId, bankChargeDetail.FinancingTypeId);

                        // Insert Bank charges Debit
                        currentVoucherDetailId++;
                        var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankChargeId = bankChargeDetail.Id,
                            DrAmount = bankChargeDetail.Amount,
                            Narration = bankChargeDetail.Narration,
                            GLGeneralInfoId = expenseGL.ExpensesGLId,
                            BudgetMasterId = expenseGL.ExpensesBudgetMasterId,
                            ActivityId = expenseGL.ExpensesActivityId,
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
                        totalAmountCr += voucherDetailChargeDr.CrAmount;
                    }
                }

                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                {
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                    foreach (var invoiceTaxVM in taxDetailVMList)
                    {
                        var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                        if (null == taxCode)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");

                        var invoiceTax = new InvoiceTax
                        {
                            VoucherDetailId = voucherDetailTempId,
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            TaxAmount = invoiceTaxVM.TaxAmount,
                            TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                        };
                        taxDrAmount += invoiceTaxVM.TaxAmount;
                        _invoiceTaxService.InsertInvoiceTax(advance, invoiceTax, invoiceTaxPk);

                        // Insert Into Customer Invoice Tax Detail (Withhold GL)
                        withholdgl = taxCode.IsWithhold;
                        if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Cr"
                            };
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                CrAmount = invoiceTaxDetail.Amount,
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount), 2, MidpointRounding.AwayFromZero),
                                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            taxDrCurrencyAmount += voucherDetailCurrencyTax.CrAmount;
                        }
                    }
                }

                // INSERT INTO VoucherDetail
                var bankVoucherDetail = new VoucherDetail
                {
                    Narration = voucher.Narration,
                    CrAmount = advance.Amount,
                    PaymentSource = advance.PaymentSource
                };

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);
                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    bankVoucherDetail.CrAmount -= taxDetailVMList.Sum(r => r.TaxAmount);
                totalAmountCr += bankVoucherDetail.CrAmount;
                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                    bankVoucherDetail.BankMasterId = bankMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    bankVoucherDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = cashMaster.ActivityId;
                    bankVoucherDetail.CashMasterId = cashMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Cash.ToString();
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = bankVoucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = totalCurrencyAmountDr - taxDrCurrencyAmount/*voucherVM.CompanyCurrencyRate * bankVoucherDetail.CrAmount*/
                });

                // INSRT INTO GLTransactionDetail
                _voucherService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                {
                    SourceType = advance.PaymentSource,
                    BankMasterId = bankVoucherDetail.BankMasterId,
                    CashMasterId = bankVoucherDetail.CashMasterId,
                    CrAmount = totalCurrencyAmountDr - taxDrCurrencyAmount
                });

                advance.BankAmount = totalCurrencyAmountDr - taxDrCurrencyAmount;
                totalCurrencyAmountCr += totalCurrencyAmountDr;

                totalAmountDr += bankVoucherDetail.DrAmount - taxDrAmount;

                if (totalAmountDr != totalAmountCr)
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

        public string InsertCustomerAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Advance
                var advance = InsertAdvance(voucherVM);
                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to Advance
                advance.VoucherId = voucher.Id;
                advance.AdvanceNo = voucher.VoucherNo;

                var currentVoucherDetailId = 0;
                var currentAdvanceDetaiId = 0;
                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    currentAdvanceDetaiId++;
                    // INSERT INTO AdvanceDetail
                    voucherDetailVM.Amount = voucherVM.Amount;
                    var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, voucherDetailVM);

                    // INSERT INTO VoucherDetail Party side
                    currentVoucherDetailId++;
                    var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                    {
                        Narration = advanceDetail.Narration,
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId,
                        CrAmount = advanceDetail.NetAmount,
                        PartyType = advanceDetail.PartyType,
                        EmployeeId = advanceDetail.EmployeeId,
                        PartyId = advanceDetail.PartyId,
                        PartyPlantId = advanceDetail.PartyPlantId,
                        AdvanceDetailId = advanceDetail.Id
                    }, currentVoucherDetailId);
                    voucherDetailTempId = voucherDetail.Id;
                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount), 2, MidpointRounding.AwayFromZero),
                    });

                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount), 2, MidpointRounding.AwayFromZero);
                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount), 2, MidpointRounding.AwayFromZero);
                }

                // INSERT INTO VoucherDetail
                var bankVoucherDetail = new VoucherDetail
                {
                    Narration = voucher.Narration,
                    DrAmount = advance.Amount,
                    PaymentSource = advance.PaymentSource
                };

                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                    bankVoucherDetail.BankMasterId = bankMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    bankVoucherDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = cashMaster.ActivityId;
                    bankVoucherDetail.CashMasterId = cashMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Cash.ToString();
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);
                totalAmountDr += bankVoucherDetail.DrAmount;
                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = bankVoucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = totalCurrencyAmountCr
                });

                // INSRT INTO GLTransactionDetail
                _voucherService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                {
                    SourceType = advance.PaymentSource,
                    BankMasterId = bankVoucherDetail.BankMasterId,
                    CashMasterId = bankVoucherDetail.CashMasterId,
                    DrAmount = totalCurrencyAmountCr
                });

                if (voucherVM.BankReconciliationUploadedDataId != null)
                {
                    var bankReconciliationMap = new BankReconciliationMap
                    {
                        Id = GetAutoNumber(nameof(BankReconciliationMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        BankReconciliationUploadedDataId = voucherVM.BankReconciliationUploadedDataId,
                        VoucherDetailId = bankVoucherDetail.Id,
                        GLTransactionDetailId = bankVoucherDetail.Id,
                        ModelState= ModelState.Added,
                        AddedBy= voucher.AddedBy,
                        AddedDate=voucher.AddedDate,
                        AddedFromIP=voucher.AddedFromIP
                    };
                    _bankReconciliationMapRepository.InsertOrUpdateGraph(bankReconciliationMap);
                }

                if (totalAmountDr != totalAmountCr)
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
        private string GetAdvanceGroupNoPK()
        {
            return base.GetAutoNumber("AdvanceGroupNo", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private string GetLoanInterestPayablePK()
        {
            return base.GetAutoNumber("FinancingSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertMultiBankCustomerAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);
                string _advanceGroupNo = GetAdvanceGroupNoPK();
                _unitOfWork.BeginTransaction();
                flag = true;
                var currentVoucherDetailId = 0;
                var currentAdvanceDetaiId = 0;
                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                var loanWriteoffAmount = 0.0M;
                var currencyAmountDr = 0.0M;
                string voucherDetailTempId = null;
                string displayVoucherNo = null;
                var bankchargeNewList = new List<BankChargeViewModel>();
                int len = banksDetailVMList.Count();
                decimal chargesAmount = 0;
                decimal chargesBooksAmount = 0;
                decimal totalchargesAmount = 0;
                decimal totalbookchargesAmount = 0;
                int count = 0;
                decimal chargesCountAmount = 0;
                decimal chargesBooksCountAmount = 0;
                foreach (var item in banksDetailVMList)
                {
                    count++;
                    if (bankChargeDetailVMList != null)
                    {
                        totalchargesAmount = Math.Round(bankChargeDetailVMList.Sum(r => r.Amount), 2);
                        totalbookchargesAmount = Math.Round(bankChargeDetailVMList.Sum(r => r.CompanyCurrencyAmount), 2);
                        if (len > count)
                        {
                            chargesAmount = Math.Round((bankChargeDetailVMList.Sum(r => r.Amount) * item.Amount / banksDetailVMList.Sum(r => r.Amount)), 2);
                            chargesCountAmount += chargesAmount;
                            chargesBooksAmount = Math.Round((bankChargeDetailVMList.Sum(r => r.CompanyCurrencyAmount) * item.BaseDrAmount / banksDetailVMList.Sum(r => r.BaseDrAmount)), 2);
                            chargesBooksCountAmount += chargesBooksAmount;
                        }
                        else if (len == count)
                        {
                            chargesAmount = Math.Round(totalchargesAmount - chargesCountAmount, 2);
                            chargesBooksAmount = Math.Round(totalbookchargesAmount - chargesBooksCountAmount, 2);
                        }
                    }
                    // INSERT INTO Advance
                    voucherVM.Amount = item.Amount+ chargesAmount;
                    voucherVM.AdvanceGroupNo = _advanceGroupNo;
                    var advance = InsertAdvance(voucherVM);
                    // INSERT INTO Voucher
                    var voucher = _voucherService.InsertVoucher(voucherVM);

                    // Set to Advance
                    advance.VoucherId = voucher.Id;
                    advance.AdvanceNo = voucher.VoucherNo;
                    displayVoucherNo = voucher.VoucherNo;


                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        currentAdvanceDetaiId++;

                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            voucherDetailVM.Amount = item.Amount + chargesAmount;
                        }
                        else
                        {
                            voucherDetailVM.Amount = item.Amount;
                        }
                        var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, voucherDetailVM);

                        // INSERT INTO VoucherDetail Party side
                        currentVoucherDetailId++;
                        var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            Narration = advanceDetail.Narration,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            CrAmount = advanceDetail.NetAmount,
                            PartyType = advanceDetail.PartyType,
                            EmployeeId = advanceDetail.EmployeeId,
                            PartyId = advanceDetail.PartyId,
                            PartyPlantId = advanceDetail.PartyPlantId,
                            AdvanceDetailId = advanceDetail.Id
                        }, currentVoucherDetailId);
                        voucherDetailTempId = voucherDetail.Id;
                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0? Math.Round((item.BaseDrAmount), 2, MidpointRounding.AwayFromZero)+ chargesBooksAmount
                                        : Math.Round((item.BaseDrAmount), 2, MidpointRounding.AwayFromZero),
                        });

                        totalAmountCr += voucherDetail.CrAmount;
                        totalCurrencyAmountCr += Math.Round((item.BaseDrAmount + chargesBooksAmount), 2, MidpointRounding.AwayFromZero);
                        
                    }

                    if (item.SourceType == "Loan")
                    {
                        if (voucherVM.PaymentSource == "MultiBank")
                        {
                            // INSERT INTO VoucherDetail (Bank or cash side Dr)
                            var voucherDetailDr = new VoucherDetail
                            {
                                Narration = voucher.Narration,
                                DrAmount = item.Amount,
                                PaymentSource = "Bank"
                            };
                            totalAmountDr += voucherDetailDr.DrAmount;

                            var glTransactionDetail = new GLTransactionDetail
                            {
                                SourceType = voucherDetailDr.PaymentSource,
                                BankMasterId = item.BankMasterId
                            };

                            #region Loan Writeoff 
                            loanWriteoffAmount = 0;
                            if (item.SourceType == "Loan")
                            {
                                if (companyCurrencyId == item.BankCurrencyId)
                                {
                                    loanWriteoffAmount = item.BaseDrAmount;
                                }
                                else
                                {
                                    loanWriteoffAmount = item.Amount;
                                }
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
                                    PartyId = null,
                                    PartyPlantId = null,
                                    PartyType = "Bank",
                                    CurrencyId = item.BankCurrencyId,
                                    Amount = loanWriteoffAmount,
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
                                var financing = _financingService.FindFinancing(item.FinancingId);
                                if (item.Amount > 0)
                                {
                                    _financingService.InsertFinancingWriteOff(financinWriteOff);
                                    // INSERT INTO Financing TABLE
                                    financing.WrittenOffAmount += loanWriteoffAmount;
                                    _financingService.UpdateFinancing(financing);

                                }
                                // INSERT INTO Voucher


                                financinWriteOff.FinancingNo = voucher.VoucherNo;
                                // Set to Financing
                                financinWriteOff.VoucherId = voucher.Id;

                                // INSERT INTO FinancingDetail
                                var financingDetailWriteOff = new FinancingDetailWriteOff
                                {
                                    Amount = loanWriteoffAmount,
                                    FinancingWriteOffId = financinWriteOff.Id,
                                    FinancingId = financinWriteOff.FinancingId,
                                    FinancingDetailId = item.FinancingDetailId,
                                    WrittenOffAmount = loanWriteoffAmount,
                                    BankMasterId = voucherVM.BankMasterId,
                                    CashMasterId = voucherVM.CashMasterId
                                };


                                //Update Financing Detail
                                var gl = _financingTypeGLService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);
                                var financingDetail = _financingService.FindFinancingDetail(item.FinancingDetailId);
                                financingDetail.WrittenOffAmount += loanWriteoffAmount;
                                if (item.Amount > 0)
                                {
                                    _financingService.UpdateFinancingDetail(financingDetail);
                                }
                                financingDetailWriteOff.GLGeneralInfoId = gl.LiabilityGLId;
                                financingDetailWriteOff.BudgetMasterId = gl.LiabilityBudgetMasterId;
                                financingDetailWriteOff.ActivityId = gl.LiabilityActivityId;

                                if (item.Amount > 0)
                                {
                                    _financingService.InsertFinancingWriteOffDetail(financinWriteOff, financingDetailWriteOff, 1);

                                }
                                voucherDetailDr.FinancingDetailWriteOffId = financingDetailWriteOff.Id;


                            }

                            #endregion

                            if (!string.IsNullOrEmpty(item.BankMasterId))
                            {
                                var bankMaster = _accountsCommonService.GetBankMaster(item.BankMasterId);
                                if (item.SourceType == "Loan")
                                {
                                    voucherDetailDr.GLGeneralInfoId = item.GLGeneralInfoId;
                                    voucherDetailDr.BudgetMasterId = item.BudgetMasterId;
                                    voucherDetailDr.ActivityId = item.ActivityId;
                                    voucherDetailDr.PartyType = PartyType.LoanTaken.ToString();
                                }
                                else
                                {
                                    voucherDetailDr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                                    voucherDetailDr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                                    voucherDetailDr.ActivityId = bankMaster["ActivityId"].ToString();
                                    voucherDetailDr.PartyType = PartyType.Bank.ToString();
                                }

                                if (string.IsNullOrEmpty(voucherDetailDr.ActivityId))
                                    throw new CustomException("ActivityId is not found.");
                                voucherDetailDr.BankMasterId = bankMaster["Id"].ToString();
                                if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                                    glTransactionDetail.DrAmount = item.BankAmount;
                                else if (bankMaster["CurrencyId"].ToString() == companyCurrencyId)
                                    glTransactionDetail.DrAmount = item.BankAmount;
                                else
                                    glTransactionDetail.DrAmount = item.BankAmount;

                            }
                            else
                                throw new CustomException("Bank  Id not found!");
                            // INSRT INTO GLTransactionDetail

                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                            _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                            // INSERT INTO VoucherDetailCurrency
                            currencyAmountDr = 0;
                            if (item.SourceType == "Loan")
                            {
                                var financingSubsequentTransaction = new FinancingSubsequentTransaction
                                {
                                    CompanyGroupId = voucherVM.CompanyGroupId,
                                    CompanyId = voucherVM.CompanyId,
                                    PlantId = voucherVM.PlantId,
                                    EntityId = voucherVM.EntityId,
                                    VoucherTypeId = voucherVM.VoucherTypeId,
                                    FinancingId = item.FinancingId,
                                    SetOffFinancingId = item.FinancingId,
                                    PartyId = voucherVM.PartyId,
                                    PartyPlantId = voucherVM.PartyPlantId,
                                    PartyType = voucherVM.PartyType,
                                    CurrencyId = item.BankCurrencyId,
                                    Amount = loanWriteoffAmount,
                                    VoucherDate = voucherVM.VoucherDate,
                                    PostingDate = voucherVM.PostingDate,
                                    DocDate = voucherVM.DocDate,
                                    DocRefNo = voucherVM.DocRefNo,
                                    TransactionType = LoanTransactionType.LoanPayment.ToString(),
                                    Narration = voucherVM.Narration,
                                    SourceType = "Loan",
                                    IsPark = voucherVM.IsPark,
                                    Id = "SL" + GetLoanInterestPayablePK(),
                                    VoucherId = voucher.Id,
                                    VoucherDetailId = voucherDetailDr.Id
                                };
                                AuditService.AddedLog(financingSubsequentTransaction);
                                _loanInterestPayableRepository.Insert(financingSubsequentTransaction);

                                if (companyCurrencyId != item.BankCurrencyId)
                                {
                                    currencyAmountDr = Math.Round(item.Amount * item.CompanyCurrencyRate, 2);
                                }
                                else
                                {
                                    currencyAmountDr = item.BaseDrAmount;
                                }

                            }
                            var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = item.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, item.CompanyCurrencyRate),
                                DrAmount = currencyAmountDr  //item.BaseDrAmount//Math.Round(voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,2)
                            });
                            totalCurrencyAmountDr += currencyAmountDr;// Math.Round(voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,2);
                        }

                        #region Loan Writeoff Exchange Gain and loss
                        if (item.SourceType == "Loan")
                        {
                            //***********************Exchange Loss*************************************
                            var exchangeloss = new VoucherDetail
                            {
                                PartyType = voucherVM.PartyType
                            };
                            var exchangeGain = new VoucherDetail
                            {
                                PartyType = voucherVM.PartyType
                            };
                            if (item.CompanyCurrencyRate < voucherVM.CompanyCurrencyRate && companyCurrencyId != item.BankCurrencyId)
                            {
                                var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);

                                exchangeloss.GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString();
                                exchangeloss.BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString();
                                exchangeloss.ActivityId = lossGL["CompanyCurrencyActivityId"].ToString();
                                exchangeloss.CurrencyId = voucher.CurrencyId;
                                exchangeloss.DocDate = voucher.DocDate;
                                exchangeloss.DocRefNo = voucher.DocRefNo;
                                exchangeloss.Narration = voucher.Narration;
                                exchangeloss.PartyType = "ExchangeLoss";
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
                                    DrAmount = item.Amount * (voucherVM.CompanyCurrencyRate - item.CompanyCurrencyRate)
                                });
                                totalCurrencyAmountDr += item.Amount * (voucherVM.CompanyCurrencyRate - item.CompanyCurrencyRate);

                            }
                            //***********************Exchange Gain*************************************
                            if (item.CompanyCurrencyRate > voucherVM.CompanyCurrencyRate && companyCurrencyId != item.BankCurrencyId)
                            {
                                var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                                exchangeGain.GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString();
                                exchangeGain.BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString();
                                exchangeGain.ActivityId = gainGL["CompanyCurrencyActivityId"].ToString();
                                exchangeGain.CurrencyId = voucher.CurrencyId;
                                exchangeGain.DocDate = voucher.DocDate;
                                exchangeGain.DocRefNo = voucher.DocRefNo;
                                exchangeGain.Narration = voucher.Narration;
                                exchangeGain.PartyType = "ExchangeGain";
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
                                    CrAmount = Math.Round(item.Amount * (item.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate))
                                });
                                totalCurrencyAmountCr += Math.Round(item.Amount * (item.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate));
                            }
                        }
                        #endregion
                    }
                    else
                    {

                        // INSERT INTO VoucherDetail
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = item.Amount,
                            PaymentSource = advance.PaymentSource
                        };

                        if (!string.IsNullOrEmpty(item.BankMasterId))
                        {
                            var bankMaster = _bankMasterRepository.Find(item.BankMasterId);
                            bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                            bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                            bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                            bankVoucherDetail.BankMasterId = bankMaster.Id;
                            bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                        }

                        else
                            throw new CustomException("Bank or Cash Id not found!");

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);
                        totalAmountDr += bankVoucherDetail.DrAmount;
                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = item.BaseDrAmount
                        });

                        totalCurrencyAmountDr += Math.Round((item.BaseDrAmount), 2, MidpointRounding.AwayFromZero);
                        // INSRT INTO GLTransactionDetail
                        _voucherService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                        {
                            SourceType = advance.PaymentSource,
                            BankMasterId = bankVoucherDetail.BankMasterId,
                            CashMasterId = bankVoucherDetail.CashMasterId,
                            DrAmount = item.BankAmount
                        });

                        if (voucherVM.BankReconciliationUploadedDataId != null)
                        {
                            var bankReconciliationMap = new BankReconciliationMap
                            {
                                Id = GetAutoNumber(nameof(BankReconciliationMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                BankReconciliationUploadedDataId = voucherVM.BankReconciliationUploadedDataId,
                                VoucherDetailId = bankVoucherDetail.Id,
                                GLTransactionDetailId = bankVoucherDetail.Id,
                            };
                            _bankReconciliationMapRepository.Insert(bankReconciliationMap);
                        }
                    }

                    if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    {
                        var currentBankChargeDetailId = 0;
                        foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                        {
                            currentBankChargeDetailId++;


                            var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                            {
                                AdvanceId = advance.Id,
                                BankMasterId = advance.BankMasterId,
                                CashMasterId = advance.CashMasterId,
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                SourceType = advance.SourceType,
                                Narration = voucher.Narration,
                                Archive = advance.Archive,
                                //Amount = Math.Round(bankChargeDetailVM.Amount * item.Amount / banksDetailVMList.Sum(r => r.Amount), 2),
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            }, currentBankChargeDetailId);
                            decimal chargecurrencyAmount = 0;
                            if (len > count)
                            {
                                bankCharge.Amount = Math.Round(bankChargeDetailVM.Amount * item.Amount / banksDetailVMList.Sum(r => r.Amount), 2);
                                chargecurrencyAmount = Math.Round(bankChargeDetailVM.CompanyCurrencyAmount * item.BaseDrAmount / banksDetailVMList.Sum(r => r.BaseDrAmount), 2);
                                var bkCharge = new BankChargeViewModel
                                {
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    Amount = bankCharge.Amount,
                                    CompanyCurrencyAmount = chargecurrencyAmount
                                };
                                bankchargeNewList.Add(bkCharge);
                            }

                            else if (len == count)
                            {
                                bankCharge.Amount = Math.Round(bankChargeDetailVMList.Where(r => r.FinancingTypeId == bankChargeDetailVM.FinancingTypeId).Sum(r => r.Amount) - bankchargeNewList.Where(r => r.FinancingTypeId == bankChargeDetailVM.FinancingTypeId).Sum(r => r.Amount), 2);
                                chargecurrencyAmount = Math.Round(bankChargeDetailVMList.Where(r => r.FinancingTypeId == bankChargeDetailVM.FinancingTypeId).Sum(r => r.CompanyCurrencyAmount) - bankchargeNewList.Where(r => r.FinancingTypeId == bankChargeDetailVM.FinancingTypeId).Sum(r => r.CompanyCurrencyAmount), 2);
                            }

                            // Get Expense GL
                            var expenseGL = _bankChargeService.GetExpensesGL(voucher.CompanyId, bankChargeDetailVM.FinancingTypeId);
                            if (string.IsNullOrEmpty(expenseGL.ExpensesActivityId))
                                throw new CustomException("ActivityId is not found.");
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
                            totalAmountDr += bankCharge.Amount;

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailChargeDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = chargecurrencyAmount
                            });
                            totalCurrencyAmountDr += chargecurrencyAmount;
                        }
                    }

                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if(totalCurrencyAmountDr!= totalCurrencyAmountCr)
                    throw new CustomException("Dr Books Amount and Cr Books Amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return displayVoucherNo;
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

        private string GetEmployeeSubsequentTransactionPK()
        {
            return GetAutoNumber("EmployeeSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertEmployeeAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Advance
                var advance = InsertAdvance(voucherVM);
                var NewemployeeSalaryAdvance = new EmployeeSalaryAdvance();

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to Advance
                advance.VoucherId = voucher.Id;
                advance.AdvanceNo = voucher.VoucherNo;



                var currentVoucherDetailId = 0;
                var currentAdvanceDetaiId = 0;
                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                var voucherDetailDrId = "";

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    currentAdvanceDetaiId++;
                    // INSERT INTO AdvanceDetail
                    var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, voucherDetailVM);

                    // INSERT INTO VoucherDetail Party side
                    currentVoucherDetailId++;
                    var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                    {
                        Narration = advanceDetail.Narration,
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId,
                        DrAmount = advanceDetail.NetAmount,
                        PartyType = advanceDetail.PartyType,
                        EmployeeId = advanceDetail.EmployeeId,
                        PartyId = advanceDetail.PartyId,
                        PartyPlantId = advanceDetail.PartyPlantId,
                        AdvanceDetailId = advanceDetail.Id
                    }, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount,
                    });
                    voucherDetailDrId = voucherDetail.Id;
                    var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        AdvanceId = advance.Id,
                        EmployeeId = advance.EmployeeId,
                        EmployeeTransactionTypeId = advance.EmployeeTransactionTypeId,
                        AdvanceWriteOffId = null,
                        EmployeePayableId = null,
                        PartyType = advance.PartyType,
                        CurrencyId = advance.CurrencyId,
                        Amount = advanceDetail.Amount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        JournalType = voucherVM.JournalType,
                        TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = voucherVM.SourceType.ToString(),
                        IsPark = voucherVM.IsPark,
                        Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                        VoucherId = voucher.Id,
                        VoucherDetailId = voucherDetail.Id,
                        PaymentSource = voucherVM.PaymentSource,
                    };
                    AuditService.AddedLog(EmployeeSubsequentAdvance);
                    _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);


                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;
                }

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var currentBankChargeId = 0;
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        currentBankChargeId++;
                        var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                        {
                            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                            Amount = bankChargeDetailVM.Amount,
                            Narration = advance.Narration,
                            AddedBy = advance.AddedBy,
                            AddedDate = advance.AddedDate,
                            AddedFromIP = advance.AddedFromIP,
                            Archive = advance.Archive,
                            BankMasterId = advance.BankMasterId,
                            CashMasterId = advance.CashMasterId,
                            SourceType = advance.SourceType,
                            AdvanceId = advance.Id
                        }, currentBankChargeId);

                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(advance.CompanyId, bankChargeDetail.FinancingTypeId);

                        // Insert Bank charges Debit
                        currentVoucherDetailId++;
                        var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankChargeId = bankChargeDetail.Id,
                            DrAmount = bankChargeDetail.Amount,
                            Narration = bankChargeDetail.Narration,
                            GLGeneralInfoId = expenseGL.ExpensesGLId,
                            BudgetMasterId = expenseGL.ExpensesBudgetMasterId,
                            ActivityId = expenseGL.ExpensesActivityId,
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
                        totalAmountCr += voucherDetailChargeDr.CrAmount;
                    }
                }

                // INSERT INTO VoucherDetail
                var bankVoucherDetail = new VoucherDetail
                {
                    Narration = voucher.Narration,
                    CrAmount = advance.Amount,
                    PaymentSource = advance.PaymentSource
                };

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);

                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    if (string.IsNullOrEmpty(bankMaster.GLGeneralInfoId.ToString()))
                        throw new CustomException("GL Id not found!");
                    else if (string.IsNullOrEmpty(bankMaster.BudgetMasterId.ToString()))
                        throw new CustomException("Budget Master Id not found!");
                    else if (string.IsNullOrEmpty(bankMaster.ActivityId.ToString()))
                        throw new CustomException("Activity Id not found!");
                    bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                    bankVoucherDetail.BankMasterId = bankMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    if (string.IsNullOrEmpty(cashMaster.GLGeneralInfoId.ToString()))
                        throw new CustomException("GL Id not found!");
                    else if (string.IsNullOrEmpty(cashMaster.BudgetMasterId.ToString()))
                        throw new CustomException("Budget Master Id not found!");
                    else if (string.IsNullOrEmpty(cashMaster.ActivityId.ToString()))
                        throw new CustomException("Activity Id not found!");
                    bankVoucherDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = cashMaster.ActivityId;
                    bankVoucherDetail.CashMasterId = cashMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Cash.ToString();
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = bankVoucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = totalCurrencyAmountDr/*voucherVM.CompanyCurrencyRate * bankVoucherDetail.CrAmount*/
                });

                // INSRT INTO GLTransactionDetail
                _voucherService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                {
                    SourceType = advance.PaymentSource,
                    BankMasterId = bankVoucherDetail.BankMasterId,
                    CashMasterId = bankVoucherDetail.CashMasterId,
                    CrAmount = totalCurrencyAmountDr
                });

                advance.BankAmount = totalCurrencyAmountDr;
                totalCurrencyAmountCr += totalCurrencyAmountDr;

                totalAmountDr += bankVoucherDetail.DrAmount;
                totalAmountCr += bankVoucherDetail.CrAmount;

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (advanceSalarySchedulelist != null)
                {
                    var employeeSalaryAdvance = new EmployeeSalaryAdvance
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        EmployeeAdvanceRequisitionId = voucherVM.RequisitionId,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = voucherVM.PartyType,
                        CurrencyId = voucherVM.CurrencyId,
                        Amount = voucherVM.Amount,
                        EmployeeId = voucherVM.EmployeeId,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        TransactionType = EmployeeSalaryAdvanceType.Advance.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = voucherVM.SourceType.ToString(),
                        IsPark = voucherVM.IsPark,
                        Id = GetEmployeeSalaryAdvancePK(),
                        VoucherId = voucher.Id,
                        VoucherDetailId = voucherDetailDrId,
                    };
                    AuditService.AddedLog(employeeSalaryAdvance);
                    _employeeSalaryAdvanceRepository.Insert(employeeSalaryAdvance);
                    NewemployeeSalaryAdvance = employeeSalaryAdvance;
                    foreach (var item in advanceSalarySchedulelist)
                    {
                        var advanceReqSchedule = new AdvanceReqSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance,
                            YearNo = item.InstallmentDate.Year,
                            MonthNo = item.InstallmentDate.Month
                        };
                        InsertAdvanceReqSchedule(NewemployeeSalaryAdvance, advanceReqSchedule, voucherVM.RequisitionId);
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

        public string UpdateDrAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");

                // UPDATE Advance
                var advance = Find(voucherVM.Id);
                CheckIsPosted(advance);
                voucherVM.CompanyId = advance.CompanyId;

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                advance.PartyId = voucherVM.PartyId;
                advance.PartyPlantId = voucherVM.PartyPlantId;
                advance.PostingDate = voucherVM.PostingDate;
                advance.DocDate = voucherVM.DocDate;
                advance.DocRefNo = voucherVM.DocRefNo;
                advance.EntityId = voucherVM.EntityId;
                advance.ReviewDate = voucherVM.ReviewDate;
                advance.ResponsiblePersonId = voucherVM.ResponsiblePersonId;
                advance.PaymentSource = voucherVM.PaymentSource;
                advance.BankMasterId = voucherVM.BankMasterId;
                advance.CashMasterId = voucherVM.CashMasterId;
                advance.JournalId = voucherVM.JournalId;
                advance.Amount = voucherVM.Amount;
                advance.CurrencyId = voucherVM.CurrencyId;
                advance.Narration = voucherVM.Narration;
                advance.IsInterTransaction = voucherVM.IsInterTransaction;
                advance.EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId;
                advance.FinancingTypeId = voucherVM.FinancingTypeId;
                advance.FiscalYearId = voucherVM.FiscalYearId;
                advance.FiscalYearPeriodId = voucherVM.FiscalYearPeriodId;
                advance.TaxYearId = voucherVM.TaxYearId;
                advance.TaxYearPeriodId = voucherVM.TaxYearPeriodId;
                base.UpdateGraph(advance);

                // INSERT INTO Voucher
                var voucher = _voucherService.FindVoucher(advance.VoucherId);
                voucher.CurrencyId = advance.CurrencyId;
                voucher.DocDate = advance.DocDate;
                voucher.DocRefNo = advance.DocRefNo;
                voucher.EntityId = advance.EntityId;
                voucher.FiscalYearId = advance.FiscalYearId;
                voucher.FiscalYearPeriodId = advance.FiscalYearPeriodId;
                voucher.PostingDate = advance.PostingDate;
                voucher.TaxYearId = advance.TaxYearId;
                voucher.TaxYearPeriodId = advance.TaxYearPeriodId;
                _voucherService.UpdateVoucher(voucher);

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                decimal taxDrCurrencyAmount = 0;
                var withholdgl = false;

                var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);
                var voucherDetailDbList = _voucherService.GetVoucherDetailList(r => r.VoucherId == voucher.Id).Select().ToList();
                var voucherDetailCurrencyDbList = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id).Select().ToList();

                var companyPartyGLList = new List<CompanyPartyGL>();
                if (voucherVM.PartyType == PartyType.Vendor.ToString())
                {
                    var companyParty = _companyPartyRepository.Query(r => r.CompanyId == advance.CompanyId && r.PlantId == advance.PlantId && r.PartyId == advance.PartyId && r.PartyType == voucherVM.PartyType).Select().FirstOrDefault();
                    if (null == companyParty)
                        throw new CustomException("Plant party mapping not found!");
                    companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                    if (null == companyPartyGLList)
                        throw new CustomException("Party GL not found!");
                }

                var advanceDetailDbList = GetAdvanceDetailList(r => r.AdvanceId == advance.Id).Select().ToList();
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var advanceDetail = advanceDetailDbList.FirstOrDefault();
                    if (null == advanceDetail)
                        throw new CustomException("Payment detail row not found!");

                    advanceDetail.Narration = advance.Narration;
                    if (voucherVM.PartyType == PartyType.Vendor.ToString())
                    {
                        var reconGL = PartyGLType.DownPaymentGL.ToString();
                        var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                        if (null == regularGL)
                            throw new CustomException("Party DownPayment GL not found!");
                        voucherDetailVM.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = regularGL.BudgetMasterId;
                        voucherDetailVM.ActivityId = regularGL.ActivityId;
                    }
                    else if (voucherVM.PartyType == PartyType.Employee.ToString())
                    {
                        voucherDetailVM.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                        voucherDetailVM.ActivityId = voucherDetailVM.ActivityId;
                    }
                    else
                        throw new CustomException("Party type is null.");

                    UpdateAdvanceDetail(advance, advanceDetail, voucherDetailVM);

                    var voucherDetail = voucherDetailDbList.FirstOrDefault(r => r.AdvanceDetailId == advanceDetail.Id);
                    voucherDetail.Narration = advanceDetail.Narration;
                    voucherDetail.GLGeneralInfoId = advanceDetail.GLGeneralInfoId;
                    voucherDetail.BudgetMasterId = advanceDetail.BudgetMasterId;
                    voucherDetail.ActivityId = advanceDetail.ActivityId;
                    voucherDetail.DrAmount = advance.Amount;
                    voucherDetail.PartyType = advanceDetail.PartyType;
                    voucherDetail.EmployeeId = advanceDetail.EmployeeId;
                    voucherDetail.PartyId = advanceDetail.PartyId;
                    voucherDetail.PartyPlantId = advanceDetail.PartyPlantId;
                    voucherDetail.AdvanceDetailId = advanceDetail.Id;
                    _voucherService.UpdateVoucherDetail(voucher, voucherDetail);

                    var voucherDetailCurrency = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetail.Id);
                    voucherDetailCurrency.ParallelCurrencyId = companyCurrencyId;
                    voucherDetailCurrency.FromCurrencyId = voucherDetail.CurrencyId;
                    voucherDetailCurrency.ToCurrencyId = companyCurrencyId;
                    voucherDetailCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                    voucherDetailCurrency.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                    voucherDetailCurrency.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetail, voucherDetailCurrency);

                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;
                }

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var bankChargeDbList = _bankChargeService.GetBankChargeList(r => r.AdvanceId == advance.Id).Select().ToList();
                    var currentBankChargeDetailId = _bankChargeService.GetBankChargePKForAdvance(advance.Id);
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(advance.CompanyId, bankChargeDetailVM.FinancingTypeId);

                        if (string.IsNullOrEmpty(bankChargeDetailVM.BankChargeId))
                        {
                            currentBankChargeDetailId++;
                            var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                            {
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                AdvanceId = advance.Id,
                                BankMasterId = advance.BankMasterId,
                                CashMasterId = advance.CashMasterId,
                                Archive = advance.Archive,
                                SourceType = advance.SourceType,
                                Amount = bankChargeDetailVM.Amount,
                                Narration = advance.Narration,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            }, currentBankChargeDetailId);

                            // Insert Bank charges Debit
                            currentVoucherDetailId++;
                            var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankChargeId = bankChargeDetail.Id,
                                DrAmount = bankChargeDetail.Amount,
                                Narration = bankChargeDetail.Narration,
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
                            totalAmountCr += voucherDetailChargeDr.CrAmount;
                        }
                        else
                        {
                            var bankCharge = bankChargeDbList.FirstOrDefault(r => r.Id == bankChargeDetailVM.BankChargeId);
                            if (null == bankCharge)
                                throw new CustomException("Bank Charge row not found!");
                            bankCharge.FinancingTypeId = bankChargeDetailVM.FinancingTypeId;
                            bankCharge.BankMasterId = advance.BankMasterId;
                            bankCharge.CashMasterId = advance.CashMasterId;
                            bankCharge.Archive = advance.Archive;
                            bankCharge.SourceType = advance.SourceType;
                            bankCharge.Amount = bankChargeDetailVM.Amount;
                            bankCharge.Narration = advance.Narration;
                            _bankChargeService.UpdateBankCharge(bankCharge);

                            // Insert Bank charges Debit
                            var voucherDetailChargeDr = voucherDetailDbList.FirstOrDefault(r => r.BankChargeId == bankCharge.Id);
                            if (null == voucherDetailChargeDr)
                                throw new CustomException("Bank Charge voucher detail row not found!");

                            voucherDetailChargeDr.BankChargeId = bankCharge.Id;
                            voucherDetailChargeDr.DrAmount = bankCharge.Amount;
                            voucherDetailChargeDr.Narration = bankCharge.Narration;
                            voucherDetailChargeDr.GLGeneralInfoId = expenseGL.ExpensesGLId;
                            voucherDetailChargeDr.BudgetMasterId = expenseGL.ExpensesBudgetMasterId;
                            voucherDetailChargeDr.ActivityId = expenseGL.ExpensesActivityId;
                            _voucherService.UpdateVoucherDetail(voucher, voucherDetailChargeDr);

                            var voucherDetailCurrencyChargeDr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailChargeDr.Id);
                            if (null == voucherDetailCurrencyChargeDr)
                                throw new CustomException("Bank Charge voucher detail currency row not found!");

                            voucherDetailCurrencyChargeDr.ParallelCurrencyId = companyCurrencyId;
                            voucherDetailCurrencyChargeDr.FromCurrencyId = voucherDetailChargeDr.CurrencyId;
                            voucherDetailCurrencyChargeDr.ToCurrencyId = companyCurrencyId;
                            voucherDetailCurrencyChargeDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                            voucherDetailCurrencyChargeDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                            voucherDetailCurrencyChargeDr.DrAmount = bankChargeDetailVM.CompanyCurrencyAmount;
                            _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailChargeDr, voucherDetailCurrencyChargeDr);

                            totalAmountDr += voucherDetailChargeDr.DrAmount;
                            totalCurrencyAmountDr += bankChargeDetailVM.CompanyCurrencyAmount;
                            totalAmountCr += voucherDetailChargeDr.CrAmount;
                        }
                    }
                }
                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                {
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                    foreach (var invoiceTaxVM in taxDetailVMList)
                    {
                        var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                        if (null == taxCode)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");

                        var invoiceTax = new InvoiceTax
                        {
                            VoucherDetailId = voucherDetailTempId,
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            TaxAmount = invoiceTaxVM.TaxAmount,
                            TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                        };
                        taxDrAmount += invoiceTaxVM.TaxAmount;
                        _invoiceTaxService.InsertInvoiceTax(advance, invoiceTax, invoiceTaxPk);

                        // Insert Into Customer Invoice Tax Detail (Withhold GL)
                        withholdgl = taxCode.IsWithhold;
                        if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Cr"
                            };
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                CrAmount = invoiceTaxDetail.Amount,
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            taxDrCurrencyAmount += voucherDetailCurrencyTax.CrAmount;
                        }
                    }
                }
                // UPDATE VoucherDetail
                var voucherDetailCr = voucherDetailDbList.FirstOrDefault(r => r.PaymentSource == PaymentSource.Bank.ToString() || r.PaymentSource == PaymentSource.Cash.ToString());
                if (null == voucherDetailCr)
                    throw new CustomException("Bank or Cash type voucher detail row not found.");

                voucherDetailCr.Narration = voucher.Narration;
                voucherDetailCr.CrAmount = advance.Amount;
                voucherDetailCr.PaymentSource = advance.PaymentSource;

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    voucherDetailCr.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);
                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    voucherDetailCr.CrAmount -= taxDetailVMList.Sum(r => r.TaxAmount);
                totalAmountCr += voucherDetailCr.CrAmount;
                // INSRT INTO GLTransactionDetail
                var glTransactionDetail = _voucherService.FindGLTransactionDetail(voucherDetailCr.Id);
                glTransactionDetail.SourceType = voucherDetailCr.PaymentSource;
                glTransactionDetail.CrAmount = totalCurrencyAmountDr;

                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    voucherDetailCr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    voucherDetailCr.BudgetMasterId = bankMaster.BudgetMasterId;
                    voucherDetailCr.ActivityId = bankMaster.ActivityId;
                    voucherDetailCr.BankMasterId = bankMaster.Id;
                    voucherDetailCr.PartyType = PartyType.Bank.ToString();

                    glTransactionDetail.BankMasterId = voucherDetailCr.BankMasterId;
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    voucherDetailCr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    voucherDetailCr.BudgetMasterId = cashMaster.BudgetMasterId;
                    voucherDetailCr.ActivityId = cashMaster.ActivityId;
                    voucherDetailCr.CashMasterId = cashMaster.Id;
                    voucherDetailCr.PartyType = PartyType.Cash.ToString();

                    glTransactionDetail.CashMasterId = voucherDetailCr.CashMasterId;
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);

                // Update VoucherDetailCurrency
                var voucherDetailCurrencyCr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailCr.Id);
                if (null == voucherDetailCurrencyCr)
                    throw new CustomException("Bank or Cash type voucher detail currency row not found.");

                voucherDetailCurrencyCr.ParallelCurrencyId = companyCurrencyId;
                voucherDetailCurrencyCr.FromCurrencyId = voucherDetailCr.CurrencyId;
                voucherDetailCurrencyCr.ToCurrencyId = companyCurrencyId;
                voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                voucherDetailCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr - taxDrCurrencyAmount;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);

                voucherDetailCr.CrAmount = voucherDetailCurrencyCr.CrAmount;
                _voucherService.UpdateGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                advance.BankAmount = totalCurrencyAmountDr - taxDrCurrencyAmount;
                totalCurrencyAmountCr += totalCurrencyAmountDr;

                totalAmountDr -= taxDrAmount;
                totalAmountCr += voucherDetailCr.CrAmount;

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

        public string UpdateEmployeeAdvance(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<AdvanceReqSchedule> DetailsList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");

                // UPDATE Advance
                var advance = Find(voucherVM.Id);
                //CheckIsPosted(advance);
                voucherVM.CompanyId = advance.CompanyId;

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                advance.PartyId = voucherVM.PartyId;
                advance.PartyPlantId = voucherVM.PartyPlantId;
                advance.PostingDate = voucherVM.PostingDate;
                advance.DocDate = voucherVM.DocDate;
                advance.DocRefNo = voucherVM.DocRefNo;
                advance.EntityId = voucherVM.EntityId;
                advance.ReviewDate = voucherVM.ReviewDate;
                advance.ResponsiblePersonId = voucherVM.ResponsiblePersonId;
                advance.PaymentSource = voucherVM.PaymentSource;
                advance.BankMasterId = voucherVM.BankMasterId;
                advance.CashMasterId = voucherVM.CashMasterId;
                advance.JournalId = voucherVM.JournalId;
                advance.Amount = voucherVM.Amount;
                advance.CurrencyId = voucherVM.CurrencyId;
                advance.Narration = voucherVM.Narration;
                advance.IsInterTransaction = voucherVM.IsInterTransaction;
                advance.EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId;
                advance.FinancingTypeId = voucherVM.FinancingTypeId;
                advance.FiscalYearId = voucherVM.FiscalYearId;
                advance.FiscalYearPeriodId = voucherVM.FiscalYearPeriodId;
                advance.TaxYearId = voucherVM.TaxYearId;
                advance.TaxYearPeriodId = voucherVM.TaxYearPeriodId;
                base.UpdateGraph(advance);

                // INSERT INTO Voucher
                var voucher = _voucherService.FindVoucher(advance.VoucherId);
                voucher.CurrencyId = advance.CurrencyId;
                voucher.DocDate = advance.DocDate;
                voucher.DocRefNo = advance.DocRefNo;
                voucher.EntityId = advance.EntityId;
                voucher.FiscalYearId = advance.FiscalYearId;
                voucher.FiscalYearPeriodId = advance.FiscalYearPeriodId;
                voucher.PostingDate = advance.PostingDate;
                voucher.TaxYearId = advance.TaxYearId;
                voucher.TaxYearPeriodId = advance.TaxYearPeriodId;
                voucher.Narration = advance.Narration;
                _voucherService.UpdateVoucher(voucher);

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;

                var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);
                var voucherDetailDbList = _voucherService.GetVoucherDetailList(r => r.VoucherId == voucher.Id).Select().ToList();
                var voucherDetailCurrencyDbList = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id).Select().ToList();

                var companyPartyGLList = new List<CompanyPartyGL>();
                if (voucherVM.PartyType == PartyType.Vendor.ToString())
                {
                    var companyParty = _companyPartyRepository.Query(r => r.CompanyId == advance.CompanyId && r.PlantId == advance.PlantId && r.PartyId == advance.PartyId && r.PartyType == voucherVM.PartyType).Select().FirstOrDefault();
                    if (null == companyParty)
                        throw new CustomException("Plant party mapping not found!");
                    companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                    if (null == companyPartyGLList)
                        throw new CustomException("Party GL not found!");
                }

                var advanceDetailDbList = GetAdvanceDetailList(r => r.AdvanceId == advance.Id).Select().ToList();
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var advanceDetail = advanceDetailDbList.FirstOrDefault();
                    if (null == advanceDetail)
                        throw new CustomException("Payment detail row not found!");

                    advanceDetail.Narration = advance.Narration;
                    if (voucherVM.PartyType == PartyType.Vendor.ToString())
                    {
                        var reconGL = PartyGLType.DownPaymentGL.ToString();
                        var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                        if (null == regularGL)
                            throw new CustomException("Party DownPayment GL not found!");
                        voucherDetailVM.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = regularGL.BudgetMasterId;
                        voucherDetailVM.ActivityId = regularGL.ActivityId;
                    }
                    else if (voucherVM.PartyType == PartyType.Employee.ToString())
                    {
                        voucherDetailVM.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                        voucherDetailVM.ActivityId = voucherDetailVM.ActivityId;
                    }
                    else
                        throw new CustomException("Party type is null.");

                    UpdateAdvanceDetail(advance, advanceDetail, voucherDetailVM);

                    var voucherDetail = voucherDetailDbList.FirstOrDefault(r => r.AdvanceDetailId == advanceDetail.Id);
                    voucherDetail.Narration = advanceDetail.Narration;
                    voucherDetail.GLGeneralInfoId = advanceDetail.GLGeneralInfoId;
                    voucherDetail.BudgetMasterId = advanceDetail.BudgetMasterId;
                    voucherDetail.ActivityId = advanceDetail.ActivityId;
                    voucherDetail.DrAmount = advanceDetail.NetAmount;
                    voucherDetail.PartyType = advanceDetail.PartyType;
                    voucherDetail.EmployeeId = advanceDetail.EmployeeId;
                    voucherDetail.PartyId = advanceDetail.PartyId;
                    voucherDetail.PartyPlantId = advanceDetail.PartyPlantId;
                    voucherDetail.AdvanceDetailId = advanceDetail.Id;
                    _voucherService.UpdateVoucherDetail(voucher, voucherDetail);

                    var voucherDetailCurrency = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetail.Id);
                    voucherDetailCurrency.ParallelCurrencyId = companyCurrencyId;
                    voucherDetailCurrency.FromCurrencyId = voucherDetail.CurrencyId;
                    voucherDetailCurrency.ToCurrencyId = companyCurrencyId;
                    voucherDetailCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                    voucherDetailCurrency.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                    voucherDetailCurrency.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetail, voucherDetailCurrency);

                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;
                }

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var bankChargeDbList = _bankChargeService.GetBankChargeList(r => r.AdvanceId == advance.Id).Select().ToList();
                    var currentBankChargeDetailId = _bankChargeService.GetBankChargePKForAdvance(advance.Id);
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(advance.CompanyId, bankChargeDetailVM.FinancingTypeId);

                        if (string.IsNullOrEmpty(bankChargeDetailVM.BankChargeId))
                        {
                            currentBankChargeDetailId++;
                            var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                            {
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                AdvanceId = advance.Id,
                                BankMasterId = advance.BankMasterId,
                                CashMasterId = advance.CashMasterId,
                                Archive = advance.Archive,
                                SourceType = advance.SourceType,
                                Amount = bankChargeDetailVM.Amount,
                                Narration = advance.Narration,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            }, currentBankChargeDetailId);

                            // Insert Bank charges Debit
                            currentVoucherDetailId++;
                            var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankChargeId = bankChargeDetail.Id,
                                DrAmount = bankChargeDetail.Amount,
                                Narration = bankChargeDetail.Narration,
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
                            totalAmountCr += voucherDetailChargeDr.CrAmount;
                        }
                        else
                        {
                            var bankCharge = bankChargeDbList.FirstOrDefault(r => r.Id == bankChargeDetailVM.BankChargeId);
                            if (null == bankCharge)
                                throw new CustomException("Bank Charge row not found!");
                            bankCharge.FinancingTypeId = bankChargeDetailVM.FinancingTypeId;
                            bankCharge.BankMasterId = advance.BankMasterId;
                            bankCharge.CashMasterId = advance.CashMasterId;
                            bankCharge.Archive = advance.Archive;
                            bankCharge.SourceType = advance.SourceType;
                            bankCharge.Amount = bankChargeDetailVM.Amount;
                            bankCharge.Narration = advance.Narration;
                            _bankChargeService.UpdateBankCharge(bankCharge);

                            // Insert Bank charges Debit
                            var voucherDetailChargeDr = voucherDetailDbList.FirstOrDefault(r => r.BankChargeId == bankCharge.Id);
                            if (null == voucherDetailChargeDr)
                                throw new CustomException("Bank Charge voucher detail row not found!");

                            voucherDetailChargeDr.BankChargeId = bankCharge.Id;
                            voucherDetailChargeDr.DrAmount = bankCharge.Amount;
                            voucherDetailChargeDr.Narration = bankCharge.Narration;
                            voucherDetailChargeDr.GLGeneralInfoId = expenseGL.ExpensesGLId;
                            voucherDetailChargeDr.BudgetMasterId = expenseGL.ExpensesBudgetMasterId;
                            voucherDetailChargeDr.ActivityId = expenseGL.ExpensesActivityId;
                            _voucherService.UpdateVoucherDetail(voucher, voucherDetailChargeDr);

                            var voucherDetailCurrencyChargeDr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailChargeDr.Id);
                            if (null == voucherDetailCurrencyChargeDr)
                                throw new CustomException("Bank Charge voucher detail currency row not found!");

                            voucherDetailCurrencyChargeDr.ParallelCurrencyId = companyCurrencyId;
                            voucherDetailCurrencyChargeDr.FromCurrencyId = voucherDetailChargeDr.CurrencyId;
                            voucherDetailCurrencyChargeDr.ToCurrencyId = companyCurrencyId;
                            voucherDetailCurrencyChargeDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                            voucherDetailCurrencyChargeDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                            voucherDetailCurrencyChargeDr.DrAmount = bankChargeDetailVM.CompanyCurrencyAmount;
                            _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailChargeDr, voucherDetailCurrencyChargeDr);

                            totalAmountDr += voucherDetailChargeDr.DrAmount;
                            totalCurrencyAmountDr += bankChargeDetailVM.CompanyCurrencyAmount;
                            totalAmountCr += voucherDetailChargeDr.CrAmount;
                        }
                    }
                }

                // UPDATE VoucherDetail
                var voucherDetailCr = voucherDetailDbList.FirstOrDefault(r => r.PaymentSource == PaymentSource.Bank.ToString() || r.PaymentSource == PaymentSource.Cash.ToString());
                if (null == voucherDetailCr)
                    throw new CustomException("Bank or Cash type voucher detail row not found.");

                voucherDetailCr.Narration = voucher.Narration;
                voucherDetailCr.CrAmount = advance.Amount;
                voucherDetailCr.PaymentSource = advance.PaymentSource;

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    voucherDetailCr.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);

                // INSRT INTO GLTransactionDetail
                var glTransactionDetail = _voucherService.FindGLTransactionDetail(voucherDetailCr.Id);
                glTransactionDetail.SourceType = voucherDetailCr.PaymentSource;
                glTransactionDetail.CrAmount = totalCurrencyAmountDr;

                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    voucherDetailCr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    voucherDetailCr.BudgetMasterId = bankMaster.BudgetMasterId;
                    voucherDetailCr.ActivityId = bankMaster.ActivityId;
                    voucherDetailCr.BankMasterId = bankMaster.Id;
                    voucherDetailCr.PartyType = PartyType.Bank.ToString();

                    glTransactionDetail.BankMasterId = voucherDetailCr.BankMasterId;
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    voucherDetailCr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    voucherDetailCr.BudgetMasterId = cashMaster.BudgetMasterId;
                    voucherDetailCr.ActivityId = cashMaster.ActivityId;
                    voucherDetailCr.CashMasterId = cashMaster.Id;
                    voucherDetailCr.PartyType = PartyType.Cash.ToString();

                    glTransactionDetail.CashMasterId = voucherDetailCr.CashMasterId;
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);
                _voucherService.UpdateGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                // Update VoucherDetailCurrency
                var voucherDetailCurrencyCr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailCr.Id);
                if (null == voucherDetailCurrencyCr)
                    throw new CustomException("Bank or Cash type voucher detail currency row not found.");

                voucherDetailCurrencyCr.ParallelCurrencyId = companyCurrencyId;
                voucherDetailCurrencyCr.FromCurrencyId = voucherDetailCr.CurrencyId;
                voucherDetailCurrencyCr.ToCurrencyId = companyCurrencyId;
                voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                voucherDetailCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);

                advance.BankAmount = totalCurrencyAmountDr;
                totalCurrencyAmountCr += totalCurrencyAmountDr;

                totalAmountDr += voucherDetailCr.DrAmount;
                totalAmountCr += voucherDetailCr.CrAmount;

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (null != DetailsList && DetailsList.Count() > 0)
                {
                    foreach (var item in DetailsList)
                    {
                        if (item.Id != null)
                        {
                            item.YearNo = item.InstallmentDate.Year;
                            item.MonthNo = item.InstallmentDate.Month;
                            _advanceReqScheduleRepository.Update(item);
                        }
                        else
                        {
                            var employeeSalaryAdvance = _employeeSalaryAdvanceRepository.Find(DetailsList.FirstOrDefault().EmployeeSalaryAdvanceId);
                            item.YearNo = item.InstallmentDate.Year;
                            item.MonthNo = item.InstallmentDate.Month;
                            InsertAdvanceReqSchedule(employeeSalaryAdvance, item, DetailsList.FirstOrDefault().RequisitionId);
                        }

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
        private string GetEmployeeSalaryAdvancePK()
        {
            return base.GetAutoNumber("EmployeeSalaryAdvance", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public string InsertEmployeeAdvanceRequisition(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Advance
                // var advance = InsertAdvance(voucherVM);

                // INSERT INTO Voucher


                var voucher = _voucherService.InsertVoucher(voucherVM);
                var NewemployeeSalaryAdvance = new EmployeeSalaryAdvance();
                // Set to Advance
                var advance = new Advance();
                if (voucherVM.JournalType != AdvanceType.Salary.ToString())
                {
                    advance = InsertAdvance(voucherVM);
                    advance.VoucherId = voucher.Id;
                    advance.AdvanceNo = voucher.VoucherNo;
                }


                var currentVoucherDetailId = 0;
                var currentAdvanceDetaiId = 0;
                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                if (voucherVM.JournalType != AdvanceType.Salary.ToString())
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        currentAdvanceDetaiId++;
                        // INSERT INTO AdvanceDetail
                        var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, voucherDetailVM);

                        // INSERT INTO VoucherDetail Party side
                        currentVoucherDetailId++;
                        var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            Narration = advanceDetail.Narration,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            DrAmount = advanceDetail.NetAmount,
                            PartyType = advanceDetail.PartyType,
                            EmployeeId = advanceDetail.EmployeeId,
                            PartyId = advanceDetail.PartyId,
                            PartyPlantId = advanceDetail.PartyPlantId,
                            AdvanceDetailId = advanceDetail.Id
                        }, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount,
                        });

                        totalAmountDr += voucherDetail.DrAmount;
                        totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                        totalAmountCr += voucherDetail.CrAmount;
                        totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;

                        var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            AdvanceId = advance.Id,
                            EmployeeId = voucherVM.EmployeeId,
                            EmployeeTransactionTypeId = advance.EmployeeTransactionTypeId,
                            AdvanceWriteOffId = null,
                            EmployeePayableId = null,
                            PartyType = voucherVM.PartyType,
                            CurrencyId = voucherVM.CurrencyId,
                            Amount = voucherDetail.DrAmount,
                            VoucherDate = voucherVM.VoucherDate,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            JournalType = AdvanceType.General.ToString(),
                            TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                            Narration = voucherVM.Narration,
                            SourceType = voucherVM.SourceType.ToString(),
                            IsPark = voucherVM.IsPark,
                            Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                            VoucherId = voucher.Id,
                            VoucherDetailId = voucherDetail.Id,
                            PaymentSource = voucherVM.PaymentSource,
                        };
                        AuditService.AddedLog(EmployeeSubsequentAdvance);
                        _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);

                    }
                }
                if (voucherVM.JournalType == AdvanceType.Salary.ToString())
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        currentAdvanceDetaiId++;
                        // INSERT INTO AdvanceDetail
                        // var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, voucherDetailVM);
                        var employeeSalaryAdvance = new EmployeeSalaryAdvance
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            EmployeeAdvanceRequisitionId = voucherVM.RequisitionId,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            PartyType = voucherVM.PartyType,
                            CurrencyId = voucherVM.CurrencyId,
                            Amount = voucherVM.Amount,
                            EmployeeId = voucherVM.EmployeeId,
                            VoucherDate = voucherVM.VoucherDate,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            TransactionType = EmployeeSalaryAdvanceType.Advance.ToString(),
                            Narration = voucherVM.Narration,
                            SourceType = voucherVM.SourceType.ToString(),
                            IsPark = voucherVM.IsPark,
                            Id = GetEmployeeSalaryAdvancePK()
                        };
                        AuditService.AddedLog(employeeSalaryAdvance);
                        _employeeSalaryAdvanceRepository.Insert(employeeSalaryAdvance);
                        NewemployeeSalaryAdvance = employeeSalaryAdvance;
                        // INSERT INTO VoucherDetail Party side
                        currentVoucherDetailId++;
                        var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            Narration = voucherDetailVM.Narration,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = employeeSalaryAdvance.Amount,
                            PartyType = voucherVM.PartyType,
                            EmployeeId = voucherVM.EmployeeId,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            AdvanceDetailId = voucherDetailVM.Id
                        }, currentVoucherDetailId);
                        employeeSalaryAdvance.VoucherDetailId = voucherDetail.Id;
                        employeeSalaryAdvance.VoucherId = voucher.Id;
                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount,
                        });

                        totalAmountDr += voucherDetail.DrAmount;
                        totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                        totalAmountCr += voucherDetail.CrAmount;
                        totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;

                        var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            AdvanceId = advance.Id,
                            EmployeeId = voucherVM.EmployeeId,
                            EmployeeTransactionTypeId = advance.EmployeeTransactionTypeId,
                            AdvanceWriteOffId = null,
                            EmployeePayableId = null,
                            PartyType = voucherVM.PartyType,
                            CurrencyId = voucherVM.CurrencyId,
                            Amount = voucherDetail.DrAmount,
                            VoucherDate = voucherVM.VoucherDate,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            JournalType = AdvanceType.Salary.ToString(),
                            TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                            Narration = voucherVM.Narration,
                            SourceType = voucherVM.SourceType.ToString(),
                            IsPark = voucherVM.IsPark,
                            Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                            VoucherId = voucher.Id,
                            VoucherDetailId = voucherDetail.Id,
                            PaymentSource = voucherVM.PaymentSource,
                            EmployeeSalaryAdvanceId= employeeSalaryAdvance.Id
                        };
                        AuditService.AddedLog(EmployeeSubsequentAdvance);
                        _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);

                    }
                }



                // INSERT INTO VoucherDetail
                var bankVoucherDetail = new VoucherDetail
                {
                    Narration = voucher.Narration,
                    CrAmount = voucherVM.Amount,
                    PaymentSource = voucherVM.PaymentSource,
                    PlantId = voucherVM.PlantId
                };

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);

                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                    bankVoucherDetail.BankMasterId = bankMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    bankVoucherDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = cashMaster.ActivityId;
                    bankVoucherDetail.CashMasterId = cashMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Cash.ToString();
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = bankVoucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = totalCurrencyAmountDr/*voucherVM.CompanyCurrencyRate * bankVoucherDetail.CrAmount*/
                });

                // INSRT INTO GLTransactionDetail
                _voucherService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                {
                    SourceType = voucherVM.PaymentSource,
                    BankMasterId = bankVoucherDetail.BankMasterId,
                    CashMasterId = bankVoucherDetail.CashMasterId,
                    CrAmount = totalCurrencyAmountDr
                });

                voucherVM.BankAmount = totalCurrencyAmountDr;
                totalCurrencyAmountCr += totalCurrencyAmountDr;

                totalAmountDr += bankVoucherDetail.DrAmount;
                totalAmountCr += bankVoucherDetail.CrAmount;

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (advanceSalarySchedulelist != null)
                {
                    foreach (var item in advanceSalarySchedulelist)
                    {
                        var advanceReqSchedule = new AdvanceReqSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance,
                            YearNo = item.InstallmentDate.Year,
                            MonthNo = item.InstallmentDate.Month
                        };
                        InsertAdvanceReqSchedule(NewemployeeSalaryAdvance, advanceReqSchedule, voucherVM.RequisitionId);
                    }
                }
                _sqlRepository.GetDataCollection("update  [TRN].[EmployeeAdvanceRequisition] set IsPost=1  where SystemId='" + voucherVM.RequisitionId + "'");
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                if (voucherVM.JournalType == AdvanceType.Salary.ToString() && advanceSalarySchedulelist == null)
                {
                    _sqlRepository.GetDataCollection("update  [dbo].[AdvanceReqSchedule] set EmployeeSalaryAdvanceId='" + NewemployeeSalaryAdvance.Id + "'  where RequisitionId='" + voucherVM.RequisitionId + "'");
                }
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



        public string CreateEmployeeAdvanceHRPark(VoucherViewModel voucherVM, Dictionary<string, object> data, List<Dictionary<string, object>> advanceDetail, IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist)
        {
            try
            {
                AccountCommonExtensionService _accountCommonService = new AccountCommonExtensionService();
                _accountCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountCommonService.CheckingTaxYearPeriod(voucherVM);
                var year = voucherVM.PostingDate.Year;
                var month = voucherVM.PostingDate.Month;
                DataSet dsMaster;
                DataSet dsDetail;
                DataSet dsData=null;
                DataSet dsDrvoucherDetail=null;
                DataSet dsCrvoucherDetail=null;
                DataSet dsDrvoucherDetailCurrency=null;
                DataSet dsCrvoucherDetailCurrency=null;
                DataSet dsGLTransactionDetail = null;
                DataSet _dsAdvanceReqScheduleData = null;
                DataSet dsEmployeeSubsequentTransaction=null;
                var currentVoucherDetailId = 0;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvance] where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("Select * from TRN.EmployeeSubsequentTransaction where 1=2", out dsEmployeeSubsequentTransaction, false, "1");


                string _Id = "";
                string ids =string.Empty;

                #region data update Worker Advance
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("EmployeeAdvance", out _Id);
                    }
                    data["Id"] = _Id;
                    data["YearNo"] = year;
                    data["MonthNo"] = month;
                    data["CurrencyId"] = voucherVM.CurrencyId;
                    data["ToCurrencyRate"] = voucherVM.ToCurrencyRate;
                    data["FiscalYearId"] = voucherVM.FiscalYearId;
                    data["FiscalYearPeriodId"] = voucherVM.FiscalYearPeriodId;
                    data["TaxYearId"] = voucherVM.TaxYearId;
                    data["TaxYearPeriodId"] = voucherVM.TaxYearPeriodId;
                    data["CompanyGroupId"] = voucherVM.CompanyGroupId;
                    data["CompanyId"] = voucherVM.CompanyId;
                    data["PlantId"] = voucherVM.PlantId;
                    data["EntityId"] = voucherVM.EntityId;
                    data["SourceType"] = voucherVM.SourceType;
                    data["UserRef"] = voucherVM.DocRefNo;
                    data["ResponsiblePersonId"] = voucherVM.ResponsiblePersonId;
                    data["RequisitionId"] = voucherVM.RequisitionId;

                    _accountCommonService.AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    _accountCommonService.EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update  Worker Advance

                var advancevoucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = companyCurrencyId,//voucherVM.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = voucherVM.VoucherDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    IsPark = voucherVM.IsPark,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.EmployeeAdvance.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                advancevoucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + advancevoucher.Id;
                //_voucherService.InsertVoucher(packingvoucher, voucherVM.FiscalYearPrefix);
                _accountCommonService.InsertVoucher(advancevoucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);




                var voucherDetailDr = new VoucherDetail
                {
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                    DrAmount = voucherVM.Amount,
                    CurrencyId = companyCurrencyId,// voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PlantId = voucherVM.PlantId,
                    EmployeeId = voucherVM.EmployeeId,
                    PartyType = voucherVM.PartyType,

                    AddedBy = advancevoucher.AddedBy,
                    AddedDate = advancevoucher.AddedDate,
                    AddedFromIP = advancevoucher.AddedFromIP
                };
                currentVoucherDetailId++;
                _accountCommonService.InsertVoucherDetail(advancevoucher, voucherDetailDr, currentVoucherDetailId, ref dsDrvoucherDetail);
                _accountCommonService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = 1,// sales.ToCurrencyRate,
                    ToCurrencyConversion = 1, /// sales.ToCurrencyRate,
                    DrAmount = voucherDetailDr.DrAmount// * sales.ToCurrencyRate
                }, ref dsDrvoucherDetailCurrency);

                #region  Worker Advance Detail
                string detailId = null;
                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceDetail] where  EmployeeAdvanceId='" + _MasterId + "'", out dsDetail, false, "1");
                int ccount = 0;
                if (advanceDetail != null)
                {
                    foreach (var item in advanceDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                             detailId = _accountCommonService.MakePK(_MasterId, ccount, 2);

                            item["Id"] = detailId;
                            item["EmployeeAdvanceId"] = _MasterId;
                            item["EmpSystemId"] = voucherVM.EmployeeId;
                            item["AdvanceAmount"] = voucherVM.Amount;
                            item["IsDisburse"] = true;
                            item["IsApprove"] = true;
                            item["IsWrittenOff"] = false;
                            item["VoucherId"] = advancevoucher.Id;
                            item["VoucherDetailId"] = voucherDetailDr.Id;
                            item["PaymentSource"] = voucherVM.PaymentSource;

                            _accountCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            ccount++;
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();

                            drmo["EmployeeAdvanceId"] = _MasterId;
                            drmo["EmpSystemId"] = item["EmpSystemId"];
                            drmo["PayDays"] = item["PayDays"];
                            drmo["AdvanceAmount"] = item["AdvanceAmount"];
                            drmo["Remarks"] = item["Remarks"];

                            drmo.EndEdit();
                        }
                    }

                    if (advanceSalarySchedulelist != null)
                    {
                        foreach (var item in advanceSalarySchedulelist)
                        {
                            if (item.Id != null)
                            {
                                if (ids == "")
                                    ids = "'" + item.Id + "'";
                                else
                                    ids = ids + ",'" + item.Id + "'";
                            }
                            
                        }
                        if (ids!="")
                        {
                            con.getDataSet("Select Id,EmployeeAdvanceDetailId,UpdatedBy,UpdatedDate,UpdatedFromIP from [dbo].[AdvanceReqSchedule] where Id in (" + ids + @") ", out dsData);
                        }
                        foreach (var item in advanceSalarySchedulelist)
                        {
                            if (item.Id == null)
                            {
                                var advanceReqSchedule = new AdvanceReqSchedule
                                {
                                    InstallmentAmount = item.InstallmentAmount,
                                    InstallmentDate = item.InstallmentDate,
                                    InstallmentNo = item.InstallmentNo,
                                    PrincipalAmount = item.PrincipalAmount,
                                    ProfitAmount = item.ProfitAmount,
                                    ScheduleNo = item.ScheduleNo,
                                    Balance = item.Balance,
                                    YearNo = item.InstallmentDate.Year,
                                    MonthNo = item.InstallmentDate.Month,
                                    EmployeeAdvanceDetailId = detailId
                                };
                                _accountCommonService.InsertAdvanceReqSchedule(advanceReqSchedule, advanceReqSchedule.EmployeeAdvanceDetailId, ref _dsAdvanceReqScheduleData);
                            }
                            else
                            {
                                var advanceReqSchedule = new AdvanceReqSchedule
                                {
                                    Id = item.Id,
                                    EmployeeAdvanceDetailId = detailId
                                };
                                _accountCommonService.UpdateAdvanceReqSchedule(advanceReqSchedule, dsData);
                            }
                           
                        }
                    }
                }

             
                if (dsEmployeeSubsequentTransaction.Tables[0].DefaultView.Count == 0)
                {
                    var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        AdvanceId = voucherVM.Id,
                        EmployeeId = voucherVM.EmployeeId,
                        EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId,
                        AdvanceWriteOffId = null,
                        EmployeePayableId = null,
                        PartyType = voucherVM.PartyType,
                        CurrencyId = voucherVM.CurrencyId,
                        Amount = voucherVM.Amount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        JournalType = AdvanceType.Salary.ToString(),
                        TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = voucherVM.SourceType.ToString(),
                        IsPark = voucherVM.IsPark,
                        Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                        VoucherId = advancevoucher.Id,
                        VoucherDetailId = voucherDetailDr.Id,
                        PaymentSource = voucherVM.PaymentSource,
                        EmployeeAdvanceDetailId = detailId,
                        AddedBy= advancevoucher.AddedBy,
                        AddedDate = advancevoucher.AddedDate,
                        AddedFromIP = advancevoucher.AddedFromIP,
                    };
                    _accountCommonService.AddNewRow(dsEmployeeSubsequentTransaction.Tables[0], EmployeeSubsequentAdvance);
                }

                var bankVoucherDetail = new VoucherDetail
                {
                    Narration = voucherVM.Narration,
                    CrAmount = voucherVM.Amount,
                    PaymentSource = voucherVM.PaymentSource
                };
                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                    bankVoucherDetail.BankMasterId = bankMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    bankVoucherDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    bankVoucherDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                    bankVoucherDetail.ActivityId = cashMaster.ActivityId;
                    bankVoucherDetail.CashMasterId = cashMaster.Id;
                    bankVoucherDetail.PartyType = PartyType.Cash.ToString();
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                          
                currentVoucherDetailId++;
                _accountCommonService.InsertVoucherDetail(advancevoucher, bankVoucherDetail, currentVoucherDetailId, ref dsCrvoucherDetail);
                _accountCommonService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = bankVoucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = 1,// sales.ToCurrencyRate,
                    ToCurrencyConversion = 1, /// sales.ToCurrencyRate,
                    CrAmount = bankVoucherDetail.CrAmount// * sales.ToCurrencyRate
                }, ref dsCrvoucherDetailCurrency);

                _accountCommonService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                {
                    SourceType = voucherVM.PaymentSource,
                    BankMasterId = bankVoucherDetail.BankMasterId,
                    CashMasterId = bankVoucherDetail.CashMasterId,
                    CrAmount = bankVoucherDetail.CrAmount
                }, out dsGLTransactionDetail);


                #endregion  Worker Advance Detail
             
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, _vdataset, dsDrvoucherDetail, dsDrvoucherDetailCurrency, dsDetail, _dsAdvanceReqScheduleData, dsData, dsEmployeeSubsequentTransaction, dsCrvoucherDetail, dsCrvoucherDetailCurrency, dsGLTransactionDetail);

                return "";
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public void InsertAdvanceReqSchedule(EmployeeSalaryAdvance employeeSalaryAdvance, AdvanceReqSchedule financingSchedule, string requisitionId)
        {
            financingSchedule.Id = MakePK(employeeSalaryAdvance.Id, financingSchedule.InstallmentNo, 3);
            financingSchedule.RequisitionId = requisitionId;
            financingSchedule.EmployeeSalaryAdvanceId = employeeSalaryAdvance.Id;
            financingSchedule.AddedBy = employeeSalaryAdvance.AddedBy;
            financingSchedule.AddedDate = employeeSalaryAdvance.AddedDate;
            financingSchedule.AddedFromIP = employeeSalaryAdvance.AddedFromIP;
            _advanceReqScheduleRepository.Insert(financingSchedule);
        }
        public void Post(string advanceId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var advance = Find(advanceId);
                CheckIsPosted(advance);

                advance.IsPosted = true;
                advance.IsPark = false;
                base.UpdateGraph(advance);
                _voucherService.PostVoucher(advance.VoucherId);
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

        public void PostEmployeeAdvanceHR(string voucherId,string requisitionId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var direct = new System.Text.StringBuilder();
                var directsql = "";
                var requisitionsql = "";
                requisitionsql = @" UPDATE [TRN].[EmployeeAdvanceRequisition] SET IsPost=1 where SystemId='" + requisitionId + @"' ";
                directsql = @" UPDATE trn.EmployeeSubsequentTransaction SET IsPark=0 where VoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                direct.Append(requisitionsql);
                _sqlRepository.ExecuteSqlCommand(direct.ToString());
                _voucherService.PostVoucher(voucherId);
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

        public void PostCustomerAdvanceGroupWise(string advanveGroupNo)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var advance = Query(r=>r.AdvanceGroupNo== advanveGroupNo).Select().ToList();
                foreach (var item in advance)
                {
                    CheckIsPosted(item);
                    item.IsPosted = true;
                    item.IsPark = false;
                    base.UpdateGraph(item);
                    _voucherService.PostVoucher(item.VoucherId);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void PostEmployeeAdvanceRequisition(string advanceId, string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var advance = Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                var salaryadvance = _employeeSalaryAdvanceRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                if (advance != null)
                {
                    CheckIsPosted(advance);
                    advance.IsPosted = true;
                    advance.IsPark = false;
                    base.UpdateGraph(advance);
                }
                if (salaryadvance != null)
                {
                    salaryadvance.IsPark = false;
                    salaryadvance.IsPosted = true;
                    _employeeSalaryAdvanceRepository.Update(salaryadvance);
                }

                _voucherService.PostVoucher(voucherId);
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

        public void UnPost(string advanceId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var advance = Find(advanceId);
                advance.IsPark = true;
                advance.IsPosted = false;
                base.UpdateGraph(advance);

                var voucher = _voucherService.FindVoucher(advance.VoucherId);
                voucher.IsPark = advance.IsPark;
                _voucherService.UpdateVoucher(voucher);

                var voucherDetailList = _voucherService.GetVoucherDetailList(r => r.VoucherId == voucher.Id).Select().ToList();
                foreach (var voucherDetail in voucherDetailList)
                {
                    voucherDetail.IsPark = voucher.IsPark;
                    _voucherService.UpdateVoucherDetail(voucher, voucherDetail);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private Voucher InsertVoucher(Advance advance, string fiscalYearPrefix)
        {
            var voucher = new Voucher
            {
                CompanyGroupId = advance.CompanyGroupId,
                CompanyId = advance.CompanyId,
                PlantId = advance.PlantId,
                EntityId = advance.EntityId,
                FiscalYearId = advance.FiscalYearId,
                FiscalYearPeriodId = advance.FiscalYearPeriodId,
                TaxYearId = advance.TaxYearId,
                TaxYearPeriodId = advance.TaxYearPeriodId,
                VoucherTypeId = advance.VoucherTypeId,
                CurrencyId = advance.CurrencyId,
                VoucherDate = advance.VoucherDate,
                PostingDate = advance.PostingDate,
                DocDate = advance.DocDate,
                DocRefNo = advance.DocRefNo,
                Narration = advance.Narration,
                IsPark = advance.IsPark,
                Archive = advance.Archive,
                SourceType = advance.SourceType,
                ModelState = advance.ModelState,
                AddedBy = advance.AddedBy,
                AddedDate = advance.AddedDate,
                AddedFromIP = advance.AddedFromIP
            };
            _voucherService.InsertVoucher(voucher, fiscalYearPrefix);
            // Set to Advance
            advance.VoucherId = voucher.Id;
            advance.AdvanceNo = voucher.VoucherNo;
            return voucher;
        }

        private Voucher UpdateVoucher(Advance advance)
        {
            var voucher = _voucherService.FindVoucher(advance.VoucherId);
            voucher.CurrencyId = advance.CurrencyId;
            voucher.DocDate = advance.DocDate;
            voucher.DocRefNo = advance.DocRefNo;
            voucher.EntityId = advance.EntityId;
            voucher.FiscalYearId = advance.FiscalYearId;
            voucher.FiscalYearPeriodId = advance.FiscalYearPeriodId;
            voucher.PostingDate = advance.PostingDate;
            voucher.TaxYearId = advance.TaxYearId;
            voucher.TaxYearPeriodId = advance.TaxYearPeriodId;
            voucher.UpdatedBy = advance.UpdatedBy;
            voucher.UpdatedDate = advance.UpdatedDate;
            voucher.UpdatedFromIP = advance.UpdatedFromIP;
            _voucherService.UpdateVoucher(voucher);
            return voucher;
        }

        private void InsertVoucherDetailCurrencyDr(string transactionCurrencyId, string companyCurrencyId, string companyGroupCurrencyId, string hardCurrencyId, VoucherDetail voucherDetail, VoucherDetailCurrencyViewModel voucherDetailCurrencyVM)
        {
            _voucherService.CurrencyExchange(transactionCurrencyId, companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, voucherDetailCurrencyVM.CompanyCurrencyDr, voucherDetailCurrencyVM.CompanyGroupCurrencyDr, voucherDetailCurrencyVM);
            if (!string.IsNullOrEmpty(companyCurrencyId))
            {
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = voucherDetailCurrencyVM.CompanyCurrencyId,
                    FromCurrencyId = voucherDetailCurrencyVM.CompanyFromCurrencyId,
                    ToCurrencyId = voucherDetailCurrencyVM.ToCurrencyId,
                    ToCurrencyConversion = voucherDetailCurrencyVM.CompanyCurrencyConversion,
                    ToCurrencyRate = voucherDetailCurrencyVM.CompanyCurrencyRate,
                    DrAmount = voucherDetailCurrencyVM.CompanyCurrencyDr
                });
            }
            if (!string.IsNullOrEmpty(companyGroupCurrencyId))
            {
                _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = voucherDetailCurrencyVM.CompanyGroupCurrencyId,
                    FromCurrencyId = voucherDetailCurrencyVM.CompanyGroupFromCurrencyId,
                    ToCurrencyId = voucherDetailCurrencyVM.ToCurrencyId,
                    ToCurrencyRate = voucherDetailCurrencyVM.CompanyGroupCurrencyRate,
                    ToCurrencyConversion = voucherDetailCurrencyVM.CompanyGroupCurrencyConversion,
                    DrAmount = voucherDetailCurrencyVM.CompanyGroupCurrencyDr
                });
            }
            if (!string.IsNullOrEmpty(hardCurrencyId))
            {
                _voucherService.InsertVoucherDetailHardCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = voucherDetailCurrencyVM.HardCurrencyId,
                    FromCurrencyId = voucherDetailCurrencyVM.HardFromCurrencyId,
                    ToCurrencyId = voucherDetailCurrencyVM.ToCurrencyId,
                    ToCurrencyRate = voucherDetailCurrencyVM.HardCurrencyRate,
                    ToCurrencyConversion = voucherDetailCurrencyVM.HardCurrencyConversion,
                    DrAmount = voucherDetailCurrencyVM.HardCurrencyDr
                });
            }
        }

        private void UpdateVoucherDetailCurrencyDr(string transactionCurrencyId, string companyCurrencyId, VoucherDetail voucherDetail, VoucherDetailCurrencyViewModel voucherDetailCurrencyVM, List<VoucherDetailCurrency> voucherDetailCurrencyList)
        {
            _voucherService.CurrencyExchange(transactionCurrencyId, companyCurrencyId, voucherDetailCurrencyVM.CompanyCurrencyDr, voucherDetailCurrencyVM);
            if (!string.IsNullOrEmpty(companyCurrencyId))
            {
                var voucherDetailCompanyCurrency = voucherDetailCurrencyList.FirstOrDefault(r => r.ParallelCurrencyId == voucherDetailCurrencyVM.CompanyCurrencyId);
                voucherDetailCompanyCurrency.FromCurrencyId = voucherDetailCurrencyVM.CompanyFromCurrencyId;
                voucherDetailCompanyCurrency.ToCurrencyId = voucherDetailCurrencyVM.ToCurrencyId;
                voucherDetailCompanyCurrency.ToCurrencyConversion = voucherDetailCurrencyVM.CompanyCurrencyConversion;
                voucherDetailCompanyCurrency.ToCurrencyRate = voucherDetailCurrencyVM.CompanyCurrencyRate;
                voucherDetailCompanyCurrency.DrAmount = voucherDetailCurrencyVM.CompanyCurrencyDr;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetail, voucherDetailCompanyCurrency);
            }
        }

        private void InsertVoucherDetailCurrencyCr(string transactionCurrencyId, string companyCurrencyId, VoucherDetail voucherDetail, VoucherDetailCurrencyViewModel voucherDetailCurrencyVM)
        {
            _voucherService.CurrencyExchange(transactionCurrencyId, companyCurrencyId, voucherDetailCurrencyVM.CompanyCurrencyCr, voucherDetailCurrencyVM);
            if (!string.IsNullOrEmpty(companyCurrencyId))
            {
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = voucherDetailCurrencyVM.CompanyCurrencyId,
                    FromCurrencyId = voucherDetailCurrencyVM.CompanyFromCurrencyId,
                    ToCurrencyId = voucherDetailCurrencyVM.ToCurrencyId,
                    ToCurrencyRate = voucherDetailCurrencyVM.CompanyCurrencyRate,
                    ToCurrencyConversion = voucherDetailCurrencyVM.CompanyCurrencyConversion,
                    CrAmount = voucherDetailCurrencyVM.CompanyCurrencyCr
                });
            }
        }

        private void UpdateVoucherDetailCurrencyCr(string transactionCurrencyId, string companyCurrencyId, VoucherDetail voucherDetail, VoucherDetailCurrencyViewModel voucherDetailCurrencyVM, List<VoucherDetailCurrency> voucherDetailCurrencyList)
        {
            _voucherService.CurrencyExchange(transactionCurrencyId, companyCurrencyId, voucherDetailCurrencyVM.CompanyCurrencyCr, voucherDetailCurrencyVM);
            if (!string.IsNullOrEmpty(companyCurrencyId))
            {
                var voucherDetailCompanyCurrency = voucherDetailCurrencyList.FirstOrDefault(r => r.ParallelCurrencyId == voucherDetailCurrencyVM.CompanyCurrencyId);
                voucherDetailCompanyCurrency.FromCurrencyId = voucherDetailCurrencyVM.CompanyFromCurrencyId;
                voucherDetailCompanyCurrency.ToCurrencyId = voucherDetailCurrencyVM.ToCurrencyId;
                voucherDetailCompanyCurrency.ToCurrencyRate = voucherDetailCurrencyVM.CompanyCurrencyRate;
                voucherDetailCompanyCurrency.ToCurrencyConversion = voucherDetailCurrencyVM.CompanyCurrencyConversion;
                voucherDetailCompanyCurrency.CrAmount = voucherDetailCurrencyVM.CompanyCurrencyCr;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetail, voucherDetailCompanyCurrency);
            }
        }

        public GridModel GetAvailableJournal(GridParameter parameters, string companyGroupId, string companyId, string plantId, string partyId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT A.Id AS AdvanceId, AD.Id AS AdvanceDetailId, A.AdvanceNo, A.CompanyGroupId, A.CompanyId, CO.UserName AS CompanyName, A.PlantId, P.UserName AS PlantName, A.CurrencyId, A.PartyId, PT.Code AS PartyCode
                        , PT.UserName AS PartyName, A.PartyPlantId, PTP.UserName AS PartyPlantName, AD.Amount, AD.TaxAmount, AD.NetAmount, A.FinancingTypeId, AD.Narration
                        FROM [TRN].[AdvanceDetail] AS AD
                        LEFT JOIN [TRN].[Advance] AS A ON A.Id=AD.AdvanceId
                        LEFT JOIN [ORG].[Company] AS CO ON CO.Id=A.CompanyId
                        LEFT JOIN [ORG].[Plant] AS P ON P.Id=A.PlantId
                        LEFT JOIN [HKP].[Party] AS PT ON PT.Id=A.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PTP ON PTP.Id=A.PartyPlantId
                        WHERE A.CompanyGroupId='" + companyGroupId + "' AND AD.CompanyId='" + companyId + "' AND AD.PlantId='" + plantId + "' AND A.PartyId='" + partyId + "' " +
                        "AND AD.PartyType='" + PartyType.Company + "' AND A.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        /// <summary>
        /// Get by PartyId and AdvanceId for InterCompany Transaction.
        /// </summary>
        /// <param name="companyGroupId"></param>
        /// <param name="companyId"></param>
        /// <param name="plantId"></param>
        /// <param name="advanceId"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetAvailableJournal(string companyGroupId, string companyId, string plantId, string advanceId)
        {
            var sql = @"SELECT A.Id AS AdvanceId, AD.Id AS AdvanceDetailId, A.AdvanceNo, A.CompanyGroupId, A.CompanyId, CO.UserName AS CompanyName, A.PlantId, P.UserName AS PlantName, A.CurrencyId, A.PartyId, PT.Code AS PartyCode
                        , PT.UserName AS PartyName, A.PartyPlantId, PTP.UserName AS PartyPlantName, AD.Amount, AD.TaxAmount, AD.NetAmount, A.FinancingTypeId, AD.Narration, A.DocRefNo
                        FROM [TRN].[AdvanceDetail] AS AD
                        LEFT JOIN [TRN].[Advance] AS A ON A.Id=AD.AdvanceId
                        LEFT JOIN [ORG].[Company] AS CO ON CO.Id=A.CompanyId
                        LEFT JOIN [ORG].[Plant] AS P ON P.Id=A.PlantId
                        LEFT JOIN [HKP].[Party] AS PT ON PT.Id=A.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PTP ON PTP.Id=A.PartyPlantId
                        WHERE A.CompanyGroupId='" + companyGroupId + "' AND AD.CompanyId='" + companyId + "' AND AD.PlantId='" + plantId + "' " +
                        "AND AD.PartyType='" + PartyType.Company + "' AND A.Id='" + advanceId + "'";
            return _sqlRepository.GetData(sql);
        }

        public decimal GetCustomerTotalAdvanceAmount(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId)
        {
            var sql = @"SELECT  ISNULL(Sum((IVD.Amount * CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount * CC.CompanyCurrencyRate)),0) AS TotalAdvanceAmount
                        FROM [TRN].[AdvanceDetail] AS IVD
                        LEFT JOIN [TRN].[Advance] AS IV ON IVD.AdvanceId=IV.Id
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=IVD.Id
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
						LEFT JOIN (
						SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
						VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
						FROM [TRN].[VoucherDetailCurrency] AS VDC
						JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
						WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
					) AS CC ON CC.VoucherDetailId=VD.Id
                    WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND IV.SourceType='CustomerAdvance'
                    AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"' AND IV.PlantId='" + plantId + "' AND IV.PartyId='" + partyId + "' AND IVD.PartyId='" + partyId + "' AND IVD.PartyPlantId='" + partyPlantId + "'";
            return _advanceDetailRepository.SqlQuery<decimal>(sql).FirstOrDefault();
        }

        public GridModel GetEmployeeAvilabePayableList(GridParameter parameters, string companyGroupId, string companyId, string employeeId)
        {
            parameters.CmdText = @"SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId,IV.EmployeeId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, A.UserName AS ActivityName,
                                V.VoucherNo, IV.DocDate, IV.DocRefNo, IV.Narration, IV.Id AS AdvanceId, IVD.Id AS AdvanceDetailId, IV.VoucherId,VD.EmployeePayableDetailId,IVD.EmployeePayableId,
                                VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode,  IVD.Amount AS Receivable,VD.EntityId,EN.UserName AS EntityName,VD.PlantId,
                                IVD.WrittenOffAmount AS Received, IVD.Amount-IVD.WrittenOffAmount AS Balance, 0 DrAmount, 0 CrAmount,
								CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
								GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
								HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion,IV.InventoryReceiveId GRNNo
                                FROM [TRN].[EmployeePayableDetail] AS IVD
                                LEFT JOIN [TRN].[EmployeePayable] AS IV ON IVD.EmployeePayableId=IV.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.EmployeePayableDetailId=IVD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
								LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
								LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
								LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                LEFT JOIN [ORG].Entity AS EN ON EN.Id=VD.EntityId
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
                                WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND IV.SourceType in ('EmployeePayable','InventoryPayable','VendorInvoice')
                                AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"' AND IV.EmployeeId='" + employeeId + "' ";
            return _sqlRepository.GetGridData(parameters);
        }

        private static void CheckIsPosted(Advance advance)
        {
            if (!advance.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }

        public string InsertCustomerPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Advance
                var advance = InsertAdvance(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to advance
                advance.VoucherId = voucher.Id;

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                var currentVoucherDetailId = 0;
                // INSERT INTO VoucherDetail (Bank or cash side Dr)
                var voucherDetailDr = new VoucherDetail
                {
                    Narration = voucher.Narration,
                    DrAmount = advance.Amount,
                    PaymentSource = advance.PaymentSource
                };

                // INSRT INTO GLTransactionDetail
                var glTransactionDetail = new GLTransactionDetail
                {
                    SourceType = voucherDetailDr.PaymentSource,
                    DrAmount = advance.BankAmount,
                };
                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    voucherDetailDr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    voucherDetailDr.BudgetMasterId = bankMaster.BudgetMasterId;
                    voucherDetailDr.ActivityId = bankMaster.ActivityId;
                    voucherDetailDr.BankMasterId = bankMaster.Id;
                    voucherDetailDr.PartyType = PartyType.Bank.ToString();

                    glTransactionDetail.BankMasterId = voucherDetailDr.BankMasterId;
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    voucherDetailDr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    voucherDetailDr.BudgetMasterId = cashMaster.BudgetMasterId;
                    voucherDetailDr.ActivityId = cashMaster.ActivityId;
                    voucherDetailDr.CashMasterId = cashMaster.Id;
                    voucherDetailDr.PartyType = PartyType.Cash.ToString();

                    glTransactionDetail.CashMasterId = voucherDetailDr.CashMasterId;
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                totalAmountDr += voucherDetailDr.DrAmount;
                totalAmountCr += voucherDetailDr.CrAmount;

                // INSERT INTO VoucherDetailCurrency
                var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                });

                var partyType = PartyType.Customer.ToString();
                var companyParty = _companyPartyRepository.Query(r => r.CompanyId == advance.CompanyId && r.PlantId == advance.PlantId && r.PartyId == advance.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                if (null == companyParty)
                    throw new CustomException("Plant party mapping not found!");
                var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                if (null == companyPartyGLList)
                    throw new CustomException("Party GL not found!");

                var currentAdvanceDetaiId = 0;
                foreach (var advanceDetailVM in voucherDetailVMList)
                {
                    currentAdvanceDetaiId++;
                    // INSERT INTO AdvanceDetail
                    advanceDetailVM.Narration = advance.Narration;
                    if (advanceDetailVM.PaymentType == PaymentType.Regular.ToString())
                    {
                        var reconGL = PartyGLType.ReconciliationGL.ToString();
                        var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                        if (null == regularGL)
                            throw new CustomException("Party Reconciliation GL not found!");
                        advanceDetailVM.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                        advanceDetailVM.BudgetMasterId = regularGL.BudgetMasterId;
                        advanceDetailVM.ActivityId = regularGL.ActivityId;
                    }
                    else if (advanceDetailVM.PaymentType == PaymentType.Advance.ToString())
                    {
                        var downGL = PartyGLType.DownPaymentGL.ToString();
                        var advanceGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == downGL);
                        if (null == advanceGL)
                            throw new CustomException("Party DownPayment GL not found!");
                        advanceDetailVM.GLGeneralInfoId = advanceGL.GLGeneralInfoId;
                        advanceDetailVM.BudgetMasterId = advanceGL.BudgetMasterId;
                        advanceDetailVM.ActivityId = advanceGL.ActivityId;
                    }
                    else if (advanceDetailVM.PaymentType == PaymentType.Suspense.ToString())
                    {
                        var susGL = PartyGLType.SuspenseGL.ToString();
                        var suspenseGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == susGL);
                        if (null == suspenseGL)
                            throw new CustomException("Party Suspense GL not found!");
                        advanceDetailVM.GLGeneralInfoId = suspenseGL.GLGeneralInfoId;
                        advanceDetailVM.BudgetMasterId = suspenseGL.BudgetMasterId;
                        advanceDetailVM.ActivityId = suspenseGL.ActivityId;
                    }
                    var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, advanceDetailVM);

                    // INSERT INTO VoucherDetail (liability side Cr)
                    var voucherDetailCr = new VoucherDetail
                    {
                        Narration = advanceDetail.Narration,
                        CrAmount = advanceDetail.NetAmount,
                        PartyId = advanceDetail.PartyId,
                        PartyType = advanceDetail.PartyType,
                        PartyPlantId = advanceDetail.PartyPlantId,
                        AdvanceDetailId = advanceDetail.Id,
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId
                    };

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = advanceDetailVM.CompanyCurrencyAmount
                    });
                }

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var currentBankChargeDetailId = 0;
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        currentBankChargeDetailId++;
                        var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                        {
                            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                            AdvanceId = advance.Id,
                            BankMasterId = advance.BankMasterId,
                            CashMasterId = advance.CashMasterId,
                            Archive = advance.Archive,
                            SourceType = advance.SourceType,
                            Amount = bankChargeDetailVM.Amount,
                            Narration = advance.Narration,
                            AddedBy = advance.AddedBy,
                            AddedDate = advance.AddedDate,
                            AddedFromIP = advance.AddedFromIP
                        }, currentBankChargeDetailId);

                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(advance.CompanyId, bankChargeDetail.FinancingTypeId);

                        // Insert Bank charges Debit
                        currentVoucherDetailId++;
                        var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankChargeId = bankChargeDetail.Id,
                            DrAmount = bankChargeDetail.Amount,
                            Narration = bankChargeDetail.Narration,
                            GLGeneralInfoId = expenseGL.ExpensesGLId,
                            BudgetMasterId = expenseGL.ExpensesBudgetMasterId,
                            ActivityId = expenseGL.ExpensesActivityId
                        }, currentVoucherDetailId);

                        totalAmountDr += voucherDetailChargeDr.DrAmount;
                        totalAmountCr += voucherDetailChargeDr.CrAmount;

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailChargeDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = bankChargeDetailVM.CompanyCurrencyAmount
                        });
                    }
                }
                if (totalAmountDr != totalAmountCr)
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

        public string UpdateCustomerPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                // UPDATE INTO Advance
                var advance = Find(voucherVM.AdvanceId);
                voucherVM.CompanyGroupId = advance.CompanyGroupId;
                voucherVM.CompanyId = advance.CompanyId;
                voucherVM.PlantId = advance.PlantId;

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                advance.PartyPlantId = voucherVM.PartyPlantId;
                advance.PaymentSource = voucherVM.PaymentSource;
                advance.BankMasterId = voucherVM.BankMasterId;
                advance.CashMasterId = voucherVM.CashMasterId;
                advance.Narration = voucherVM.Narration;
                advance.PostingDate = voucherVM.PostingDate;
                advance.DocDate = voucherVM.DocDate;
                advance.Amount = voucherVM.Amount;
                advance.CurrencyId = voucherVM.CurrencyId;
                advance.ReviewDate = voucherVM.ReviewDate;
                advance.EntityId = voucherVM.EntityId;
                advance.DocRefNo = voucherVM.DocRefNo;
                advance.ResponsiblePersonId = voucherVM.ResponsiblePersonId;
                UpdateGraph(advance);

                // INSERT INTO Voucher
                var voucher = _voucherService.FindVoucher(advance.VoucherId);
                voucher.DocDate = advance.DocDate;
                voucher.DocRefNo = advance.DocRefNo;
                voucher.EntityId = advance.EntityId;
                voucher.FiscalYearId = advance.FiscalYearId;
                voucher.FiscalYearPeriodId = advance.FiscalYearPeriodId;
                voucher.Narration = advance.Narration;
                voucher.PostingDate = advance.PostingDate;
                voucher.TaxYearId = advance.TaxYearId;
                voucher.TaxYearPeriodId = advance.TaxYearPeriodId;
                voucher.UpdatedBy = advance.UpdatedBy;
                voucher.UpdatedDate = advance.UpdatedDate;
                voucher.UpdatedFromIP = advance.UpdatedFromIP;
                voucher.VoucherDate = advance.VoucherDate;
                _voucherService.UpdateVoucher(voucher);

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);
                var voucherDetailDbList = _voucherService.GetVoucherDetailList(r => r.VoucherId == voucher.Id).Select().ToList();
                var voucherDetailCurrencyDbList = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id).Select().ToList();

                // UPDATE VoucherDetail
                var voucherDetailDr = voucherDetailDbList.FirstOrDefault(r => r.PaymentSource == nameof(Bank) || r.PaymentSource == "Cash");
                if (null == voucherDetailDr)
                    throw new CustomException("Bank or Cash type voucher detail row not found.");
                voucherDetailDr.Narration = voucher.Narration;
                voucherDetailDr.DrAmount = advance.Amount;
                voucherDetailDr.PaymentSource = advance.PaymentSource;

                // INSRT INTO GLTransactionDetail
                var glTransactionDetail = _voucherService.FindGLTransactionDetail(voucherDetailDr.Id);
                glTransactionDetail.SourceType = voucherDetailDr.PaymentSource;
                glTransactionDetail.DrAmount = advance.BankAmount;

                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                {
                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                    voucherDetailDr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                    voucherDetailDr.BudgetMasterId = bankMaster.BudgetMasterId;
                    voucherDetailDr.ActivityId = bankMaster.ActivityId;
                    voucherDetailDr.BankMasterId = bankMaster.Id;
                    voucherDetailDr.PartyType = PartyType.Bank.ToString();

                    glTransactionDetail.BankMasterId = voucherDetailDr.BankMasterId;
                }
                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                {
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                    voucherDetailDr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                    voucherDetailDr.BudgetMasterId = cashMaster.BudgetMasterId;
                    voucherDetailDr.ActivityId = cashMaster.ActivityId;
                    voucherDetailDr.CashMasterId = cashMaster.Id;
                    voucherDetailDr.PartyType = PartyType.Cash.ToString();

                    glTransactionDetail.CashMasterId = voucherDetailDr.CashMasterId;
                }
                else
                    throw new CustomException("Bank or Cash Id not found!");

                _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);
                _voucherService.UpdateGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                totalAmountDr += voucherDetailDr.DrAmount;
                totalAmountCr += voucherDetailDr.CrAmount;

                // INSERT INTO VoucherDetailCurrency
                var voucherDetailCurrencyDr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailDr.Id);
                if (null == voucherDetailCurrencyDr)
                    throw new CustomException("Bank or Cash type voucher detail currency row not found.");

                voucherDetailCurrencyDr.ParallelCurrencyId = companyCurrencyId;
                voucherDetailCurrencyDr.FromCurrencyId = voucherDetailDr.CurrencyId;
                voucherDetailCurrencyDr.ToCurrencyId = companyCurrencyId;
                voucherDetailCurrencyDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                voucherDetailCurrencyDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                voucherDetailCurrencyDr.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrencyDr);

                var partyType = PartyType.Customer.ToString();
                var companyParty = _companyPartyRepository.Query(r => r.CompanyId == advance.CompanyId && r.PlantId == advance.PlantId && r.PartyId == advance.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                if (null == companyParty)
                    throw new CustomException("Plant party mapping not found!");
                var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                if (null == companyPartyGLList)
                    throw new CustomException("Party GL not found!");

                var currentAdvanceDetaiId = 0;
                var advanceDetailDbList = GetAdvanceDetailList(r => r.AdvanceId == advance.Id).Select().ToList();
                foreach (var advanceDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(advanceDetailVM.AdvanceDetailId))
                    {
                        currentAdvanceDetaiId++;
                        // INSERT INTO AdvanceDetail
                        advanceDetailVM.Narration = advance.Narration;
                        if (advanceDetailVM.PaymentType == PaymentType.Regular.ToString())
                        {
                            var reconGL = PartyGLType.ReconciliationGL.ToString();
                            var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                            if (null == regularGL)
                                throw new CustomException("Party Reconciliation GL not found!");
                            advanceDetailVM.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                            advanceDetailVM.BudgetMasterId = regularGL.BudgetMasterId;
                            advanceDetailVM.ActivityId = regularGL.ActivityId;
                        }
                        else if (advanceDetailVM.PaymentType == PaymentType.Advance.ToString())
                        {
                            var downGL = PartyGLType.DownPaymentGL.ToString();
                            var advanceGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == downGL);
                            if (null == advanceGL)
                                throw new CustomException("Party DownPayment GL not found!");
                            advanceDetailVM.GLGeneralInfoId = advanceGL.GLGeneralInfoId;
                            advanceDetailVM.BudgetMasterId = advanceGL.BudgetMasterId;
                            advanceDetailVM.ActivityId = advanceGL.ActivityId;
                        }
                        else if (advanceDetailVM.PaymentType == PaymentType.Suspense.ToString())
                        {
                            var susGL = PartyGLType.SuspenseGL.ToString();
                            var suspenseGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == susGL);
                            if (null == suspenseGL)
                                throw new CustomException("Party Suspense GL not found!");
                            advanceDetailVM.GLGeneralInfoId = suspenseGL.GLGeneralInfoId;
                            advanceDetailVM.BudgetMasterId = suspenseGL.BudgetMasterId;
                            advanceDetailVM.ActivityId = suspenseGL.ActivityId;
                        }
                        var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, advanceDetailVM);

                        // INSERT INTO VoucherDetail (liability side Cr)
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = advanceDetail.Narration,
                            CrAmount = advanceDetail.NetAmount,
                            PartyId = advanceDetail.PartyId,
                            PartyType = advanceDetail.PartyType,
                            PartyPlantId = advanceDetail.PartyPlantId,
                            AdvanceDetailId = advanceDetail.Id,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId
                        };

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                        totalAmountDr += voucherDetailCr.DrAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = advanceDetailVM.CompanyCurrencyAmount
                        });
                    }
                    else
                    {
                        var advanceDetail = advanceDetailDbList.FirstOrDefault(r => r.Id == advanceDetailVM.AdvanceDetailId);
                        if (null == advanceDetail)
                            throw new CustomException("Payment detail row not found!");

                        advanceDetail.Narration = advance.Narration;
                        if (advanceDetailVM.PaymentType == PaymentType.Regular.ToString())
                        {
                            var reconGL = PartyGLType.ReconciliationGL.ToString();
                            var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                            if (null == regularGL)
                                throw new CustomException("Party Reconciliation GL not found!");
                            advanceDetail.GLGeneralInfoId = regularGL.GLGeneralInfoId;
                            advanceDetail.BudgetMasterId = regularGL.BudgetMasterId;
                            advanceDetail.ActivityId = regularGL.ActivityId;
                        }
                        else if (advanceDetailVM.PaymentType == PaymentType.Advance.ToString())
                        {
                            var downGL = PartyGLType.DownPaymentGL.ToString();
                            var advanceGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == downGL);
                            if (null == advanceGL)
                                throw new CustomException("Party DownPayment GL not found!");
                            advanceDetail.GLGeneralInfoId = advanceGL.GLGeneralInfoId;
                            advanceDetail.BudgetMasterId = advanceGL.BudgetMasterId;
                            advanceDetail.ActivityId = advanceGL.ActivityId;
                        }
                        else if (advanceDetailVM.PaymentType == PaymentType.Suspense.ToString())
                        {
                            var susGL = PartyGLType.SuspenseGL.ToString();
                            var suspenseGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == susGL);
                            if (null == suspenseGL)
                                throw new CustomException("Party Suspense GL not found!");
                            advanceDetail.GLGeneralInfoId = suspenseGL.GLGeneralInfoId;
                            advanceDetail.BudgetMasterId = suspenseGL.BudgetMasterId;
                            advanceDetail.ActivityId = suspenseGL.ActivityId;
                        }
                        UpdateAdvanceDetail(advanceDetail);

                        // UPDATE VoucherDetail (liability side Cr)
                        var voucherDetailCr = voucherDetailDbList.FirstOrDefault(r => r.AdvanceDetailId == advanceDetailVM.AdvanceDetailId);
                        if (null == voucherDetailCr)
                            throw new CustomException("Voucher detail row not found.");

                        voucherDetailCr.Narration = advanceDetail.Narration;
                        voucherDetailCr.CrAmount = advanceDetail.NetAmount;
                        voucherDetailCr.PartyId = advanceDetail.PartyId;
                        voucherDetailCr.PartyType = advanceDetail.PartyType;
                        voucherDetailCr.PartyPlantId = advanceDetail.PartyPlantId;
                        voucherDetailCr.AdvanceDetailId = advanceDetail.Id;
                        voucherDetailCr.GLGeneralInfoId = advanceDetail.GLGeneralInfoId;
                        voucherDetailCr.BudgetMasterId = advanceDetail.BudgetMasterId;
                        voucherDetailCr.ActivityId = advanceDetail.ActivityId;
                        _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);

                        totalAmountDr += voucherDetailCr.DrAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var voucherDetailCurrencyCr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailCr.Id);
                        voucherDetailCurrencyCr.ParallelCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.FromCurrencyId = voucherDetailCr.CurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                        voucherDetailCurrencyCr.CrAmount = advanceDetailVM.CompanyCurrencyAmount;
                        _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                    }
                }

                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var bankChargeDbList = _bankChargeService.GetBankChargeList(r => r.AdvanceId == advance.Id).Select().ToList();
                    var currentBankChargeDetailId = _bankChargeService.GetBankChargePKForAdvance(advance.Id);
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        // Get Expense GL
                        var expenseGL = _bankChargeService.GetExpensesGL(advance.CompanyId, bankChargeDetailVM.FinancingTypeId);

                        if (string.IsNullOrEmpty(bankChargeDetailVM.BankChargeId))
                        {
                            currentBankChargeDetailId++;
                            var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                            {
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                AdvanceId = advance.Id,
                                BankMasterId = advance.BankMasterId,
                                CashMasterId = advance.CashMasterId,
                                Archive = advance.Archive,
                                SourceType = advance.SourceType,
                                Amount = bankChargeDetailVM.Amount,
                                Narration = advance.Narration,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
                            }, currentBankChargeDetailId);

                            // Insert Bank charges Debit
                            currentVoucherDetailId++;
                            var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                BankChargeId = bankChargeDetail.Id,
                                DrAmount = bankChargeDetail.Amount,
                                Narration = bankChargeDetail.Narration,
                                GLGeneralInfoId = expenseGL.ExpensesGLId,
                                BudgetMasterId = expenseGL.ExpensesBudgetMasterId,
                                ActivityId = expenseGL.ExpensesActivityId
                            }, currentVoucherDetailId);

                            totalAmountDr += voucherDetailChargeDr.DrAmount;
                            totalAmountCr += voucherDetailChargeDr.CrAmount;

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailChargeDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = bankChargeDetailVM.CompanyCurrencyAmount
                            });
                        }
                        else
                        {
                            var bankCharge = bankChargeDbList.FirstOrDefault(r => r.Id == bankChargeDetailVM.BankChargeId);
                            if (null == bankCharge)
                                throw new CustomException("Bank Charge row not found!");
                            bankCharge.FinancingTypeId = bankChargeDetailVM.FinancingTypeId;
                            bankCharge.BankMasterId = advance.BankMasterId;
                            bankCharge.CashMasterId = advance.CashMasterId;
                            bankCharge.Archive = advance.Archive;
                            bankCharge.SourceType = advance.SourceType;
                            bankCharge.Amount = bankChargeDetailVM.Amount;
                            bankCharge.Narration = advance.Narration;
                            _bankChargeService.UpdateBankCharge(bankCharge);

                            // Insert Bank charges Debit
                            var voucherDetailChargeDr = voucherDetailDbList.FirstOrDefault(r => r.BankChargeId == bankCharge.Id);
                            if (null == voucherDetailChargeDr)
                                throw new CustomException("Bank Charge voucher detail row not found!");

                            voucherDetailChargeDr.BankChargeId = bankCharge.Id;
                            voucherDetailChargeDr.DrAmount = bankCharge.Amount;
                            voucherDetailChargeDr.Narration = bankCharge.Narration;
                            voucherDetailChargeDr.GLGeneralInfoId = expenseGL.ExpensesGLId;
                            voucherDetailChargeDr.BudgetMasterId = expenseGL.ExpensesBudgetMasterId;
                            voucherDetailChargeDr.ActivityId = expenseGL.ExpensesActivityId;
                            _voucherService.UpdateVoucherDetail(voucher, voucherDetailChargeDr);

                            totalAmountDr += voucherDetailChargeDr.DrAmount;
                            totalAmountCr += voucherDetailChargeDr.CrAmount;

                            var voucherDetailCurrencyChargeDr = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailChargeDr.Id);
                            if (null == voucherDetailCurrencyChargeDr)
                                throw new CustomException("Bank Charge voucher detail currency row not found!");

                            voucherDetailCurrencyChargeDr.ParallelCurrencyId = companyCurrencyId;
                            voucherDetailCurrencyChargeDr.FromCurrencyId = voucherDetailChargeDr.CurrencyId;
                            voucherDetailCurrencyChargeDr.ToCurrencyId = companyCurrencyId;
                            voucherDetailCurrencyChargeDr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                            voucherDetailCurrencyChargeDr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                            voucherDetailCurrencyChargeDr.DrAmount = bankChargeDetailVM.CompanyCurrencyAmount;
                            _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailChargeDr, voucherDetailCurrencyChargeDr);
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

        public IQueryFluent<AdvanceDetail> GetAdvanceDetailList(Expression<Func<AdvanceDetail, bool>> query)
        {
            return _advanceDetailRepository.Query(query);
        }

        public AdvanceDetail FindAdvanceDetail(string advanceDetailId)
        {
            return _advanceDetailRepository.Find(advanceDetailId);
        }

        public string InsertInterTransaction(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> NoteSetOffList
            , IEnumerable<VoucherDetailViewModel> employeePayableVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.SettlementType == SettlementType.Payment.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.SettlementType == SettlementType.Payment.ToString())
                    throw new CustomException("Cash Id not found!");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Advance
                var advance = InsertAdvance(voucherVM);
                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to Advance
                advance.VoucherId = voucher.Id;
                advance.AdvanceNo = voucher.VoucherNo;

                var currentVoucherDetailId = 0;
                var currentAdvanceDetaiId = 0;

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;

                if (voucherVM.SettlementType == SettlementType.Payment.ToString())
                {
                    currentAdvanceDetaiId++;
                    var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);

                    if (null == financingTypeGL)
                        throw new CustomException("Transaction Type GL  not found!");

                    var advanceDetail = new AdvanceDetail
                    {
                        Id = MakeAdvanceDetailPK(advance.Id, currentAdvanceDetaiId),
                        AdvanceId = advance.Id,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        EmployeeId = voucherVM.EmployeeId,
                        PartyType = voucherVM.PartyType,
                        PaymentType = null,
                        AddedBy = advance.AddedBy,
                        AddedDate = advance.AddedDate,
                        AddedFromIP = advance.AddedFromIP,
                        Archive = advance.Archive,
                        Narration = voucherVM.Narration,
                        Amount = voucherVM.Amount,
                        NetAmount = voucherVM.Amount,
                    };
                    if (voucherVM.JournalType == "Payable")
                    {
                        advanceDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                        advanceDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                        advanceDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                    }
                    if (voucherVM.JournalType == "Receivable")
                    {
                        advanceDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        advanceDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        advanceDetail.ActivityId = financingTypeGL.AssetActivityId;

                    }
                    InsertAdvanceDetail(advance, advanceDetail, currentAdvanceDetaiId);

                    // INSERT INTO VoucherDetail Party side
                    currentVoucherDetailId++;
                    var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                    {
                        Narration = advanceDetail.Narration,
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId,
                        PartyType = advanceDetail.PartyType,
                        EmployeeId = advanceDetail.EmployeeId,
                        PartyId = advanceDetail.PartyId,
                        PartyPlantId = advanceDetail.PartyPlantId,
                        AdvanceDetailId = advanceDetail.Id,
                        PlantId = voucherVM.InterPlantId
                    }, currentVoucherDetailId);
                    if (voucherVM.JournalType == "Receivable")
                    {
                        voucherDetail.DrAmount = 0;
                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            voucherDetail.CrAmount = advanceDetail.NetAmount + voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            voucherDetail.CrAmount = advanceDetail.NetAmount - voucherVM.ExchangeAmount;
                        else
                            voucherDetail.CrAmount = advanceDetail.NetAmount;

                    }
                    if (voucherVM.JournalType == "Payable")
                    {
                        voucherDetail.CrAmount = 0;
                        voucherDetail.DrAmount = advanceDetail.NetAmount;
                    }
                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount,
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount,
                    });


                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;

                    var bankVoucherDetail = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        PaymentSource = advance.PaymentSource
                    };
                    if (voucherVM.JournalType == "Receivable")
                    {
                        bankVoucherDetail.DrAmount = advance.Amount;
                        bankVoucherDetail.CrAmount = 0;

                    }
                    if (voucherVM.JournalType == "Payable")
                    {
                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advanceDetail.NetAmount + voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advanceDetail.NetAmount - voucherVM.ExchangeAmount;
                        else
                            bankVoucherDetail.CrAmount = advanceDetail.NetAmount;

                        bankVoucherDetail.DrAmount = 0;

                    }

                    if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    {
                        if (voucherVM.JournalType == "Payable")
                            bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);
                        if (voucherVM.JournalType == "Receivable")
                            bankVoucherDetail.DrAmount -= bankChargeDetailVMList.Sum(r => r.Amount);
                    }
                    totalAmountCr += bankVoucherDetail.CrAmount;
                    totalAmountDr += bankVoucherDetail.DrAmount;

                    if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                    {
                        var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                        bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                        bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                        bankVoucherDetail.BankMasterId = bankMaster.Id;
                        bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                    }
                    else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                    {
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                        bankVoucherDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        bankVoucherDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                        bankVoucherDetail.ActivityId = cashMaster.ActivityId;
                        bankVoucherDetail.CashMasterId = cashMaster.Id;
                        bankVoucherDetail.PartyType = PartyType.Cash.ToString();
                    }
                    else
                        throw new CustomException("Bank or Cash Id not found!");
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = bankVoucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = bankVoucherDetail.CrAmount,
                        DrAmount = bankVoucherDetail.DrAmount
                    });
                    _voucherService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                    {
                        SourceType = advance.PaymentSource,
                        BankMasterId = bankVoucherDetail.BankMasterId,
                        CashMasterId = bankVoucherDetail.CashMasterId,
                        CrAmount = bankVoucherDetail.CrAmount,
                        DrAmount = bankVoucherDetail.DrAmount
                    });

                    decimal totalCharges = 0;
                    if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    {
                        var currentBankChargeDetailId = 0;
                        foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                        {
                            currentBankChargeDetailId++;
                            var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                            {
                                AdvanceId = advance.Id,
                                BankMasterId = advance.BankMasterId,
                                CashMasterId = advance.CashMasterId,
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                SourceType = advance.SourceType,
                                Narration = voucher.Narration,
                                Archive = advance.Archive,
                                Amount = bankChargeDetailVM.Amount,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
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
                            totalCharges += bankCharge.Amount;

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
                    if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                    {
                        var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                        var voucherDtEx = new VoucherDetail
                        {
                            GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                            BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                            ActivityId = lossGL.CompanyCurrencyActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            DrAmount = voucherVM.ExchangeAmount,
                            PartyType = voucherVM.ExchangeType
                        };
                        totalAmountDr += voucherDtEx.DrAmount;

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDtEx.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.ExchangeAmount
                        });
                        totalCurrencyAmountDr += voucherVM.ExchangeAmount;
                    }

                    if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                    {
                        var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                        var voucherDtExGain = new VoucherDetail
                        {
                            GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                            BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                            ActivityId = gainGL.CompanyCurrencyActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            CrAmount = voucherVM.ExchangeAmount,
                            PartyType = voucherVM.ExchangeType
                        };
                        totalAmountCr += voucherDtExGain.CrAmount;

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDtExGain.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherVM.ExchangeAmount
                        });
                        totalCurrencyAmountCr -= voucherVM.ExchangeAmount;
                    }
                }
                else if (voucherVM.SettlementType == SettlementType.SetOff.ToString())
                {
                    if (voucherVM.JournalType == JournalType.Receivable.ToString())
                    {
                        if (null == voucherDetailVMList)
                            throw new CustomException("Detail row is null.");

                        var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;

                        var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                        invoiceWriteOff.VoucherId = voucher.Id;

                        foreach (var voucherDetailVM in voucherDetailVMList)
                        {
                            var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == invoiceDetail)
                                throw new CustomException("Invoice not found!");

                            invoiceDetail.WrittenOffAmount += voucherDetailVM.CrAmount;
                            if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                            invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                            var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                            invoice.WrittenOffAmount = invoiceDetail.WrittenOffAmount;
                            invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                            invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.Update(invoice);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                ActivityId = invoiceDetail.ActivityId,
                                CurrencyId = invoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                InvoiceId = voucherDetailVM.InvoiceId,
                                InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = voucherDetailVM.CompanyId,
                                PlantId = voucherDetailVM.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.CrAmount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceDetail);
                            invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                CrAmount = voucherDetailVM.CrAmount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advance.PartyType,
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
                                CrAmount = voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate,
                            });

                            if (voucherDetailVM.ExchangeType == "ExchangeGain")
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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

                            //ExchangeGain  ExchangeLoss

                            if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            {
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                                var voucherDtEx = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    DrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountDr += voucherDtEx.DrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtEx.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    DrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountDr += voucherVM.ExchangeAmount;
                            }

                            if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                                var voucherDtExGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    CrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountCr += voucherDtExGain.CrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtExGain.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    CrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountCr -= voucherVM.ExchangeAmount;
                            }
                        }

                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource
                        };


                        bankVoucherDetail.CrAmount = 0;
                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.DrAmount = advance.Amount - voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.DrAmount = advance.Amount + voucherVM.ExchangeAmount;
                        else
                            bankVoucherDetail.DrAmount = advance.Amount;

                        totalAmountDr += bankVoucherDetail.DrAmount;

                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = bankVoucherDetail.DrAmount
                        });
                    }
                    if (voucherVM.JournalType == JournalType.Payable.ToString())
                    {
                        if (null == voucherDetailVMList)
                            throw new CustomException("Detail row is null.");

                        var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;

                        var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                        invoiceWriteOff.VoucherId = voucher.Id;

                        foreach (var voucherDetailVM in voucherDetailVMList)
                        {
                            var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == invoiceDetail)
                                throw new CustomException("Invoice not found!");

                            invoiceDetail.WrittenOffAmount += voucherDetailVM.DrAmount;
                            if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                            invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                            var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                            invoice.WrittenOffAmount = invoiceDetail.WrittenOffAmount;
                            invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                            invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.Update(invoice);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                ActivityId = invoiceDetail.ActivityId,
                                CurrencyId = invoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                InvoiceId = voucherDetailVM.InvoiceId,
                                InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = voucherDetailVM.CompanyId,
                                PlantId = voucherDetailVM.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.DrAmount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceDetail);
                            invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.DrAmount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advance.PartyType,
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
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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


                            //ExchangeGain  ExchangeLoss

                            if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            {
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                                var voucherDtEx = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    DrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountDr += voucherDtEx.DrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtEx.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    DrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountDr += voucherVM.ExchangeAmount;
                            }

                            if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                                var voucherDtExGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    CrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountCr += voucherDtExGain.CrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtExGain.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    CrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountCr -= voucherVM.ExchangeAmount;
                            }

                        }
                        decimal totalCharges = 0;
                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            var currentBankChargeDetailId = 0;
                            foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                            {
                                currentBankChargeDetailId++;
                                var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                                {
                                    AdvanceId = advance.Id,
                                    BankMasterId = advance.BankMasterId,
                                    CashMasterId = advance.CashMasterId,
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    SourceType = advance.SourceType,
                                    Narration = voucher.Narration,
                                    Archive = advance.Archive,
                                    Amount = bankChargeDetailVM.Amount,
                                    AddedBy = advance.AddedBy,
                                    AddedDate = advance.AddedDate,
                                    AddedFromIP = advance.AddedFromIP
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
                                totalCharges += bankCharge.Amount;

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
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource,

                        };

                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advance.Amount + voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advance.Amount - voucherVM.ExchangeAmount;
                        else
                            bankVoucherDetail.CrAmount = advance.Amount;

                        bankVoucherDetail.DrAmount = 0;

                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                            bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);

                        totalAmountCr += bankVoucherDetail.CrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;

                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = bankVoucherDetail.CrAmount
                        });
                    }

                }
                else if (voucherVM.SettlementType == SettlementType.EmployeePayableSetOff.ToString())
                {
                    if (voucherVM.JournalType == JournalType.Payable.ToString())
                    {
                        if (null == employeePayableVMList)
                            throw new CustomException("Detail row is null.");


                        var employeePayableIds = employeePayableVMList.Select(r => r.EmployeePayableId);
                        var employeePayableDbList = _employeePayableService.GetEmployeePayableList(r => employeePayableIds.Contains(r.Id)).Select().ToList();
                        var employeePayableDetailIds = employeePayableVMList.Select(r => r.EmployeePayableDetailId);
                        var employeePayableDetailDbList = _employeePayableService.GetEmployeePayableDetailList(r => employeePayableDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceWriteOffDetailId = 0;

                        var employeePayableWriteOff = new EmployeePayableWriteOff
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            CurrencyId = voucherVM.CurrencyId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            Amount = voucherVM.Amount,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            SourceType = SourceType.EmployeePayment.ToString(),
                            PartyType = PartyType.Employee.ToString(),
                            EmployeeId = voucherVM.EmployeeId,
                            SourceFrom = voucherVM.SourceFrom,
                            TaxYearId = voucherVM.TaxYearId,
                            TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                            FiscalYearId = voucherVM.FiscalYearId,
                            FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                            RoundingType = voucherVM.RoundingType,
                            RoundingAmount = voucherVM.RoundingAmount,
                            RowState = RowState.Parked.ToString(),
                            VoucherDate = voucherVM.VoucherDate
                        };
                        _employeePayableWriteOffService.InsertEmployeePayableWriteOff(employeePayableWriteOff);

                        employeePayableWriteOff.VoucherId = voucher.Id;
                        foreach (var voucherDetailVM in employeePayableVMList)
                        {
                            var employeePayableDetail = employeePayableDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.EmployeePayableDetailId);
                            if (null == employeePayableDetail)
                                throw new CustomException("Invoice not found!");

                            employeePayableDetail.WrittenOffAmount += voucherDetailVM.Amount;

                            if (employeePayableDetail.NetAmount < employeePayableDetail.WrittenOffAmount)
                                throw new CustomException("Received Amount can not cross Balance Amount");

                            employeePayableDetail.IsWrittenOff = employeePayableDetail.NetAmount == employeePayableDetail.WrittenOffAmount;
                            employeePayableDetail.UpdatedBy = employeePayableWriteOff.AddedBy;
                            employeePayableDetail.UpdatedDate = employeePayableWriteOff.AddedDate;
                            employeePayableDetail.UpdatedFromIP = employeePayableWriteOff.AddedFromIP;
                            _employeePayableService.UpdateEmployeePayableDetail(employeePayableDetail);

                            // TODO: have a gap here if invoice split
                            var employeePayable = employeePayableDbList.First(r => r.Id == employeePayableDetail.EmployeePayableId);
                            employeePayable.WrittenOffAmount += employeePayableDetail.WrittenOffAmount;
                            employeePayable.NetAmount = employeePayable.Amount - employeePayable.WrittenOffAmount;
                            employeePayable.IsWrittenOff = employeePayable.Amount == employeePayable.WrittenOffAmount;
                            employeePayable.UpdatedBy = employeePayableWriteOff.AddedBy;
                            employeePayable.UpdatedDate = employeePayableWriteOff.AddedDate;
                            employeePayable.UpdatedFromIP = employeePayableWriteOff.AddedFromIP;
                            _employeePayableService.UpdateEmployeePayable(employeePayable);

                            // INSERT INTO InvoiceDetail
                            var employeePayableWriteOffDetail = new EmployeePayableWriteOffDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucherDetailVM.CurrencyId,
                                EmployeePayableWriteOffId = employeePayableWriteOff.Id,
                                EmployeePayableId = voucherDetailVM.EmployeePayableId,
                                EmployeePayableDetailId = voucherDetailVM.EmployeePayableDetailId,
                                Amount = voucherDetailVM.Amount,
                                Archive = employeePayableWriteOff.Archive,
                                ModelState = employeePayableWriteOff.ModelState,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            currentInvoiceWriteOffDetailId++;
                            _employeePayableWriteOffService.InsertEmployeePayableWriteOffDetail(employeePayableWriteOff, employeePayableWriteOffDetail, currentInvoiceWriteOffDetailId);

                            // in libility side Cr.
                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                EntityId = voucherDetailVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                DrAmount = voucherDetailVM.Amount,
                                CrAmount = 0,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                IsPark = voucher.IsPark,
                                Archive = voucher.Archive,
                                ModelState = voucher.ModelState,
                                EmployeeId = employeePayableWriteOff.EmployeeId,
                                PartyType = employeePayableWriteOff.PartyType,
                                EmployeePayableWriteOffDetailId = employeePayableWriteOffDetail.Id,
                                VoucherId = voucher.Id
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetailId);
                            totalAmountDr += voucherDr.DrAmount;
                            totalAmountCr += voucherDr.CrAmount;
                            if (!string.IsNullOrEmpty(companyCurrencyId))
                            {
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                                {
                                    DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate,
                                    FromCurrencyId = voucherVM.CurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate
                                });
                            }

                            if (voucherDetailVM.ExchangeType == "ExchangeGain")
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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


                            //ExchangeGain  ExchangeLoss

                            if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            {
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                                var voucherDtEx = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    DrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountDr += voucherDtEx.DrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtEx.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    DrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountDr += voucherVM.ExchangeAmount;
                            }

                            if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                                var voucherDtExGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    CrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountCr += voucherDtExGain.CrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtExGain.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    CrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountCr -= voucherVM.ExchangeAmount;
                            }

                        }
                        decimal totalCharges = 0;
                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            var currentBankChargeDetailId = 0;
                            foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                            {
                                currentBankChargeDetailId++;
                                var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                                {
                                    AdvanceId = advance.Id,
                                    BankMasterId = advance.BankMasterId,
                                    CashMasterId = advance.CashMasterId,
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    SourceType = advance.SourceType,
                                    Narration = voucher.Narration,
                                    Archive = advance.Archive,
                                    Amount = bankChargeDetailVM.Amount,
                                    AddedBy = advance.AddedBy,
                                    AddedDate = advance.AddedDate,
                                    AddedFromIP = advance.AddedFromIP
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
                                totalCharges += bankCharge.Amount;

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
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource,

                        };

                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advance.Amount + voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advance.Amount - voucherVM.ExchangeAmount;
                        else
                            bankVoucherDetail.CrAmount = advance.Amount;

                        bankVoucherDetail.DrAmount = 0;

                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                            bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);

                        totalAmountCr += bankVoucherDetail.CrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;

                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = bankVoucherDetail.CrAmount
                        });
                    }

                }

                else if (voucherVM.SettlementType == SettlementType.AdvanceToEmployee.ToString())
                {

                    if (voucherVM.JournalType == JournalType.Payable.ToString())
                    {
                        var employeeTransactionGL = _employeeTransactionTypeGLRepository.Query(r => r.EmployeeTransactionTypeId == voucherVM.EmployeeTransactionTypeId && r.IsExpensesBooking == true).Select().FirstOrDefault();

                        if (null == employeeTransactionGL)
                            throw new CustomException("Employee Transaction GL not found!");

                        var advanceDetail = new AdvanceDetail
                        {
                            Id = MakeAdvanceDetailPK(advance.Id, currentAdvanceDetaiId),
                            AdvanceId = advance.Id,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            EmployeeId = voucherVM.EmployeeId,
                            PartyType = voucherVM.PartyType,
                            PaymentType = null,
                            AddedBy = advance.AddedBy,
                            AddedDate = advance.AddedDate,
                            AddedFromIP = advance.AddedFromIP,
                            Archive = advance.Archive,
                            Narration = voucherVM.Narration,
                            Amount = voucherVM.Amount,
                            NetAmount = voucherVM.Amount,
                            GLGeneralInfoId = employeeTransactionGL.AdvanceGLId,
                            BudgetMasterId = employeeTransactionGL.AdvanceBudgetMasterId,
                            ActivityId = employeeTransactionGL.AdvanceActivityId
                        };

                        InsertAdvanceDetail(advance, advanceDetail, currentAdvanceDetaiId);

                        // INSERT INTO VoucherDetail
                        var voucherDetailCr = new VoucherDetail
                        {
                            VoucherId = voucher.Id,
                            //InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = advanceDetail.Amount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            PartyType = advance.PartyType,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            EmployeeId = advance.EmployeeId,
                            AdvanceDetailId = advanceDetail.Id
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
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailCr.DrAmount * voucherVM.CompanyCurrencyRate,
                        });

                        decimal totalCharges = 0;
                        decimal taxDrAmount = 0;
                        decimal taxDrCurrencyAmount = 0;
                        bool withholdgl = false;
                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            var currentBankChargeDetailId = 0;
                            foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                            {
                                currentBankChargeDetailId++;
                                var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                                {
                                    AdvanceId = advance.Id,
                                    BankMasterId = advance.BankMasterId,
                                    CashMasterId = advance.CashMasterId,
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    SourceType = advance.SourceType,
                                    Narration = voucher.Narration,
                                    Archive = advance.Archive,
                                    Amount = bankChargeDetailVM.Amount,
                                    AddedBy = advance.AddedBy,
                                    AddedDate = advance.AddedDate,
                                    AddedFromIP = advance.AddedFromIP
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
                                totalCharges += bankCharge.Amount;

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
                        if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                        {
                            var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                            foreach (var invoiceTaxVM in taxDetailVMList)
                            {
                                var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                                if (null == taxCode)
                                    throw new CustomException("Tax code not found!");

                                var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                                if (null == taxCodeGL)
                                    throw new CustomException("Tax code GL not found!");

                                var invoiceTax = new InvoiceTax
                                {
                                    VoucherDetailId = voucherDetailCr.Id,
                                    TaxCodeId = invoiceTaxVM.TaxCodeId,
                                    TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                    TaxAmount = invoiceTaxVM.TaxAmount,
                                    TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                                };
                                taxDrAmount += invoiceTaxVM.TaxAmount;
                                _invoiceTaxService.InsertInvoiceTax(advance, invoiceTax, invoiceTaxPk);

                                // Insert Into Customer Invoice Tax Detail (Withhold GL)
                                withholdgl = taxCode.IsWithhold;
                                if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                        BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                        ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                        Amount = invoiceTax.TaxAmount,
                                        AType = "Cr"
                                    };
                                    _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                    var voucherDetailTax = new VoucherDetail
                                    {
                                        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                        ActivityId = invoiceTaxDetail.ActivityId,
                                        InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                        CrAmount = invoiceTaxDetail.Amount,
                                    };
                                    currentVoucherDetailId++;
                                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                                    var voucherDetailCurrencyTax = new VoucherDetailCurrency
                                    {
                                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                        ToCurrencyId = companyCurrencyId,
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = companyCurrencyId,
                                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                    };
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                                    taxDrCurrencyAmount += voucherDetailCurrencyTax.CrAmount;
                                }
                            }
                        }
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource,

                        };
                        if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                            bankVoucherDetail.CrAmount = advance.Amount - taxDetailVMList.Sum(r => r.TaxAmount);
                        else
                            bankVoucherDetail.CrAmount = advance.Amount;

                        bankVoucherDetail.DrAmount = 0;

                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                            bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);
                        //TODO:
                        if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                            totalAmountCr = bankVoucherDetail.CrAmount + taxDetailVMList.Sum(r => r.TaxAmount);
                        else
                            totalAmountCr = bankVoucherDetail.CrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;

                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = bankVoucherDetail.CrAmount
                        });
                    }

                }

                else if (voucherVM.SettlementType == SettlementType.AdvanceToVendor.ToString())
                {

                    if (voucherVM.JournalType == JournalType.Payable.ToString())
                    {

                        var partyType = PartyType.Vendor.ToString();
                        var companyParty = _companyPartyRepository.Query(r => r.CompanyId == advance.CompanyId && r.PlantId == advance.PlantId && r.PartyId == advance.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                        if (null == companyParty)
                            throw new CustomException("Plant party mapping not found!");
                        var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                        if (null == companyPartyGLList)
                            throw new CustomException("Party GL not found!");

                        var downGL = PartyGLType.DownPaymentGL.ToString();
                        var advanceGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == downGL);
                        if (null == advanceGL)
                            throw new CustomException("Party DownPayment GL not found!");



                        var advanceDetail = new AdvanceDetail
                        {
                            Id = MakeAdvanceDetailPK(advance.Id, currentAdvanceDetaiId),
                            AdvanceId = advance.Id,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            EmployeeId = voucherVM.EmployeeId,
                            PartyType = voucherVM.PartyType,
                            PaymentType = null,
                            AddedBy = advance.AddedBy,
                            AddedDate = advance.AddedDate,
                            AddedFromIP = advance.AddedFromIP,
                            Archive = advance.Archive,
                            Narration = voucherVM.Narration,
                            Amount = voucherVM.Amount,
                            NetAmount = voucherVM.Amount,
                            GLGeneralInfoId = advanceGL.GLGeneralInfoId,
                            BudgetMasterId = advanceGL.BudgetMasterId,
                            ActivityId = advanceGL.ActivityId
                        };

                        InsertAdvanceDetail(advance, advanceDetail, currentAdvanceDetaiId);

                        // INSERT INTO VoucherDetail
                        var voucherDetailCr = new VoucherDetail
                        {
                            VoucherId = voucher.Id,
                            //InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = advanceDetail.Amount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            PartyType = advance.PartyType,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            AdvanceDetailId = advanceDetail.Id

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
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailCr.DrAmount * voucherVM.CompanyCurrencyRate,
                        });

                        decimal totalCharges = 0;
                        decimal taxDrAmount = 0;
                        decimal taxDrCurrencyAmount = 0;
                        bool withholdgl = false;
                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            var currentBankChargeDetailId = 0;
                            foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                            {
                                currentBankChargeDetailId++;
                                var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                                {
                                    AdvanceId = advance.Id,
                                    BankMasterId = advance.BankMasterId,
                                    CashMasterId = advance.CashMasterId,
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    SourceType = advance.SourceType,
                                    Narration = voucher.Narration,
                                    Archive = advance.Archive,
                                    Amount = bankChargeDetailVM.Amount,
                                    AddedBy = advance.AddedBy,
                                    AddedDate = advance.AddedDate,
                                    AddedFromIP = advance.AddedFromIP
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
                                totalCharges += bankCharge.Amount;

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
                        if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                        {
                            var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                            foreach (var invoiceTaxVM in taxDetailVMList)
                            {
                                var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                                if (null == taxCode)
                                    throw new CustomException("Tax code not found!");

                                var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                                if (null == taxCodeGL)
                                    throw new CustomException("Tax code GL not found!");

                                var invoiceTax = new InvoiceTax
                                {
                                    VoucherDetailId = voucherDetailCr.Id,
                                    TaxCodeId = invoiceTaxVM.TaxCodeId,
                                    TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                    TaxAmount = invoiceTaxVM.TaxAmount,
                                    TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                                };
                                taxDrAmount += invoiceTaxVM.TaxAmount;
                                _invoiceTaxService.InsertInvoiceTax(advance, invoiceTax, invoiceTaxPk);

                                // Insert Into Customer Invoice Tax Detail (Withhold GL)
                                withholdgl = taxCode.IsWithhold;
                                if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                        BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                        ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                        Amount = invoiceTax.TaxAmount,
                                        AType = "Cr"
                                    };
                                    _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                    var voucherDetailTax = new VoucherDetail
                                    {
                                        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                        ActivityId = invoiceTaxDetail.ActivityId,
                                        InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                        CrAmount = invoiceTaxDetail.Amount,
                                    };
                                    currentVoucherDetailId++;
                                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                                    var voucherDetailCurrencyTax = new VoucherDetailCurrency
                                    {
                                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                        ToCurrencyId = companyCurrencyId,
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = companyCurrencyId,
                                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                    };
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                                    taxDrCurrencyAmount += voucherDetailCurrencyTax.CrAmount;
                                }
                            }
                        }
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource,

                        };
                        if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                            bankVoucherDetail.CrAmount = advance.Amount - taxDetailVMList.Sum(r => r.TaxAmount);
                        else
                            bankVoucherDetail.CrAmount = advance.Amount;

                        bankVoucherDetail.DrAmount = 0;

                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                            bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);
                        //TODO:
                        if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                            totalAmountCr = bankVoucherDetail.CrAmount + taxDetailVMList.Sum(r => r.TaxAmount);
                        else
                            totalAmountCr = bankVoucherDetail.CrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;

                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = bankVoucherDetail.CrAmount
                        });
                    }

                }
                else if (voucherVM.SettlementType == SettlementType.DebitNoteSetOff.ToString())
                {
                    if (voucherVM.JournalType == JournalType.Payable.ToString())
                    {
                        if (null == NoteSetOffList)
                            throw new CustomException("Detail row is null.");


                        var adjustNoteIds = NoteSetOffList.Select(r => r.AdjustmentNoteId);
                        var adjustNoteDbList = _adjustmentNoteRepository.Query(r => adjustNoteIds.Contains(r.Id)).Select().ToList();
                        var adjustNoteDetailIds = NoteSetOffList.Select(r => r.AdjustmentNoteDetailId);
                        var adjustNoteDetailDbList = _adjustmentNoteDetailRepository.Query(r => adjustNoteDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceWriteOffDetailId = 0;

                        var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);
                        invoiceWriteOff.VoucherId = voucher.Id;

                        foreach (var voucherDetailVM in NoteSetOffList)
                        {
                            var adjustNoteDetail = adjustNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdjustmentNoteDetailId);
                            if (null == adjustNoteDetail)
                                throw new CustomException("Invoice not found!");

                            adjustNoteDetail.WrittenOffAmount += voucherDetailVM.DrAmount;

                            if (adjustNoteDetail.Amount < adjustNoteDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            adjustNoteDetail.IsWrittenOff = adjustNoteDetail.Amount == adjustNoteDetail.WrittenOffAmount;
                            adjustNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            adjustNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            adjustNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _adjustmentNoteDetailRepository.Update(adjustNoteDetail);

                            // TODO: have a gap here if invoice split
                            var adjustNote = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                            adjustNote.WrittenOffAmount += voucherDetailVM.DrAmount;
                            adjustNote.IsWrittenOff = adjustNote.Amount == adjustNote.WrittenOffAmount;
                            adjustNote.UpdatedBy = invoiceWriteOff.AddedBy;
                            adjustNote.UpdatedDate = invoiceWriteOff.AddedDate;
                            adjustNote.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _adjustmentNoteRepository.Update(adjustNote);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceWriteOffDetailId++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = adjustNoteDetail.GLGeneralInfoId,
                                BudgetMasterId = adjustNoteDetail.BudgetMasterId,
                                ActivityId = adjustNoteDetail.ActivityId,
                                CurrencyId = invoiceWriteOff.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                AdjustmentNoteId = voucherDetailVM.AdjustmentNoteId,
                                AdjustmentNoteDetailId = voucherDetailVM.AdjustmentNoteDetailId,
                                CompanyId = voucherDetailVM.CompanyId,
                                PlantId = voucherDetailVM.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.DrAmount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);
                            invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.DrAmount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advance.PartyType,
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



                            //ExchangeGain  ExchangeLoss

                            if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            {
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                                var voucherDtEx = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    DrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountDr += voucherDtEx.DrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtEx.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    DrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountDr += voucherVM.ExchangeAmount;
                            }

                            if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                                var voucherDtExGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    CrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountCr += voucherDtExGain.CrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtExGain.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    CrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountCr -= voucherVM.ExchangeAmount;
                            }


                            if (voucherDetailVM.ExchangeType == "ExchangeGain")
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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
                        decimal totalCharges = 0;
                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            var currentBankChargeDetailId = 0;
                            foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                            {
                                currentBankChargeDetailId++;
                                var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                                {
                                    AdvanceId = advance.Id,
                                    BankMasterId = advance.BankMasterId,
                                    CashMasterId = advance.CashMasterId,
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    SourceType = advance.SourceType,
                                    Narration = voucher.Narration,
                                    Archive = advance.Archive,
                                    Amount = bankChargeDetailVM.Amount,
                                    AddedBy = advance.AddedBy,
                                    AddedDate = advance.AddedDate,
                                    AddedFromIP = advance.AddedFromIP
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
                                totalCharges += bankCharge.Amount;

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
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource,
                        };


                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advance.Amount + voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advance.Amount - voucherVM.ExchangeAmount;
                        else
                            bankVoucherDetail.CrAmount = advance.Amount;

                        bankVoucherDetail.DrAmount = 0;

                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                            bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);

                        totalAmountCr += bankVoucherDetail.CrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;

                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = bankVoucherDetail.CrAmount
                        });

                    }
                    if (voucherVM.JournalType == JournalType.Receivable.ToString())
                    {
                        if (null == NoteSetOffList)
                            throw new CustomException("Detail row is null.");


                        var adjustNoteIds = NoteSetOffList.Select(r => r.AdjustmentNoteId);
                        var adjustNoteDbList = _adjustmentNoteRepository.Query(r => adjustNoteIds.Contains(r.Id)).Select().ToList();
                        var adjustNoteDetailIds = NoteSetOffList.Select(r => r.AdjustmentNoteDetailId);
                        var adjustNoteDetailDbList = _adjustmentNoteDetailRepository.Query(r => adjustNoteDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceWriteOffDetailId = 0;

                        var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);
                        invoiceWriteOff.VoucherId = voucher.Id;

                        foreach (var voucherDetailVM in NoteSetOffList)
                        {
                            var adjustNoteDetail = adjustNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdjustmentNoteDetailId);
                            if (null == adjustNoteDetail)
                                throw new CustomException("Invoice not found!");

                            adjustNoteDetail.WrittenOffAmount += voucherDetailVM.DrAmount;

                            if (adjustNoteDetail.Amount < adjustNoteDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            adjustNoteDetail.IsWrittenOff = adjustNoteDetail.Amount == adjustNoteDetail.WrittenOffAmount;
                            adjustNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            adjustNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            adjustNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _adjustmentNoteDetailRepository.Update(adjustNoteDetail);

                            // TODO: have a gap here if invoice split
                            var adjustNote = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                            adjustNote.WrittenOffAmount += voucherDetailVM.DrAmount;
                            adjustNote.IsWrittenOff = adjustNote.Amount == adjustNote.WrittenOffAmount;
                            adjustNote.UpdatedBy = invoiceWriteOff.AddedBy;
                            adjustNote.UpdatedDate = invoiceWriteOff.AddedDate;
                            adjustNote.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _adjustmentNoteRepository.Update(adjustNote);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceWriteOffDetailId++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = adjustNoteDetail.GLGeneralInfoId,
                                BudgetMasterId = adjustNoteDetail.BudgetMasterId,
                                ActivityId = adjustNoteDetail.ActivityId,
                                CurrencyId = invoiceWriteOff.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                AdjustmentNoteId = voucherDetailVM.AdjustmentNoteId,
                                AdjustmentNoteDetailId = voucherDetailVM.AdjustmentNoteDetailId,
                                CompanyId = voucherDetailVM.CompanyId,
                                PlantId = voucherDetailVM.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.DrAmount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);
                            invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                CrAmount = voucherDetailVM.DrAmount,
                                DrAmount = 0,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advance.PartyType,
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
                                CrAmount = voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate,
                            });

                            if (voucherDetailVM.ExchangeType == "ExchangeGain")
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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

                            //ExchangeGain  ExchangeLoss

                            if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            {
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                                var voucherDtEx = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    DrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountDr += voucherDtEx.DrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtEx.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    DrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountDr += voucherVM.ExchangeAmount;
                            }

                            if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                                var voucherDtExGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    CrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountCr += voucherDtExGain.CrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtExGain.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    CrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountCr -= voucherVM.ExchangeAmount;
                            }

                        }
                        decimal totalCharges = 0;
                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            var currentBankChargeDetailId = 0;
                            foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                            {
                                currentBankChargeDetailId++;
                                var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                                {
                                    AdvanceId = advance.Id,
                                    BankMasterId = advance.BankMasterId,
                                    CashMasterId = advance.CashMasterId,
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    SourceType = advance.SourceType,
                                    Narration = voucher.Narration,
                                    Archive = advance.Archive,
                                    Amount = bankChargeDetailVM.Amount,
                                    AddedBy = advance.AddedBy,
                                    AddedDate = advance.AddedDate,
                                    AddedFromIP = advance.AddedFromIP
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
                                totalCharges += bankCharge.Amount;

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
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource,
                        };



                        bankVoucherDetail.CrAmount = 0;
                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.DrAmount = advance.Amount - voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.DrAmount = advance.Amount + voucherVM.ExchangeAmount;
                        else
                            bankVoucherDetail.DrAmount = advance.Amount;

                        totalAmountDr += bankVoucherDetail.DrAmount;


                        //if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        //    bankVoucherDetail.DrAmount -= bankChargeDetailVMList.Sum(r => r.Amount);

                        //totalAmountDr += bankVoucherDetail.DrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;

                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = bankVoucherDetail.DrAmount
                        });
                    }
                }

                else if (voucherVM.SettlementType == SettlementType.CreditNoteSetOff.ToString())
                {
                    if (voucherVM.JournalType == JournalType.Payable.ToString())
                    {
                        if (null == NoteSetOffList)
                            throw new CustomException("Detail row is null.");


                        var adjustNoteIds = NoteSetOffList.Select(r => r.AdjustmentNoteId);
                        var adjustNoteDbList = _adjustmentNoteRepository.Query(r => adjustNoteIds.Contains(r.Id)).Select().ToList();
                        var adjustNoteDetailIds = NoteSetOffList.Select(r => r.AdjustmentNoteDetailId);
                        var adjustNoteDetailDbList = _adjustmentNoteDetailRepository.Query(r => adjustNoteDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceWriteOffDetailId = 0;

                        var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);
                        invoiceWriteOff.VoucherId = voucher.Id;

                        foreach (var voucherDetailVM in NoteSetOffList)
                        {
                            var adjustNoteDetail = adjustNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdjustmentNoteDetailId);
                            if (null == adjustNoteDetail)
                                throw new CustomException("Invoice not found!");

                            adjustNoteDetail.WrittenOffAmount += voucherDetailVM.DrAmount;

                            if (adjustNoteDetail.Amount < adjustNoteDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            adjustNoteDetail.IsWrittenOff = adjustNoteDetail.Amount == adjustNoteDetail.WrittenOffAmount;
                            adjustNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            adjustNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            adjustNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _adjustmentNoteDetailRepository.Update(adjustNoteDetail);

                            // TODO: have a gap here if invoice split
                            var adjustNote = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                            adjustNote.WrittenOffAmount += voucherDetailVM.DrAmount;
                            adjustNote.IsWrittenOff = adjustNote.Amount == adjustNote.WrittenOffAmount;
                            adjustNote.UpdatedBy = invoiceWriteOff.AddedBy;
                            adjustNote.UpdatedDate = invoiceWriteOff.AddedDate;
                            adjustNote.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _adjustmentNoteRepository.Update(adjustNote);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceWriteOffDetailId++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = adjustNoteDetail.GLGeneralInfoId,
                                BudgetMasterId = adjustNoteDetail.BudgetMasterId,
                                ActivityId = adjustNoteDetail.ActivityId,
                                CurrencyId = invoiceWriteOff.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                AdjustmentNoteId = voucherDetailVM.AdjustmentNoteId,
                                AdjustmentNoteDetailId = voucherDetailVM.AdjustmentNoteDetailId,
                                CompanyId = voucherDetailVM.CompanyId,
                                PlantId = voucherDetailVM.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.DrAmount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);
                            invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.DrAmount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advance.PartyType,
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



                            //ExchangeGain  ExchangeLoss

                            if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            {
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                                var voucherDtEx = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    DrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountDr += voucherDtEx.DrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtEx.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    DrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountDr += voucherVM.ExchangeAmount;
                            }

                            if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                                var voucherDtExGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    CrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountCr += voucherDtExGain.CrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtExGain.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    CrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountCr -= voucherVM.ExchangeAmount;
                            }


                            if (voucherDetailVM.ExchangeType == "ExchangeGain")
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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
                        decimal totalCharges = 0;
                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            var currentBankChargeDetailId = 0;
                            foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                            {
                                currentBankChargeDetailId++;
                                var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                                {
                                    AdvanceId = advance.Id,
                                    BankMasterId = advance.BankMasterId,
                                    CashMasterId = advance.CashMasterId,
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    SourceType = advance.SourceType,
                                    Narration = voucher.Narration,
                                    Archive = advance.Archive,
                                    Amount = bankChargeDetailVM.Amount,
                                    AddedBy = advance.AddedBy,
                                    AddedDate = advance.AddedDate,
                                    AddedFromIP = advance.AddedFromIP
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
                                totalCharges += bankCharge.Amount;

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
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource,
                        };


                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advance.Amount + voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.CrAmount = advance.Amount - voucherVM.ExchangeAmount;
                        else
                            bankVoucherDetail.CrAmount = advance.Amount;

                        bankVoucherDetail.DrAmount = 0;

                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                            bankVoucherDetail.CrAmount += bankChargeDetailVMList.Sum(r => r.Amount);

                        totalAmountCr += bankVoucherDetail.CrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;

                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = bankVoucherDetail.CrAmount
                        });

                    }
                    if (voucherVM.JournalType == JournalType.Receivable.ToString())
                    {
                        if (null == NoteSetOffList)
                            throw new CustomException("Detail row is null.");


                        var adjustNoteIds = NoteSetOffList.Select(r => r.AdjustmentNoteId);
                        var adjustNoteDbList = _adjustmentNoteRepository.Query(r => adjustNoteIds.Contains(r.Id)).Select().ToList();
                        var adjustNoteDetailIds = NoteSetOffList.Select(r => r.AdjustmentNoteDetailId);
                        var adjustNoteDetailDbList = _adjustmentNoteDetailRepository.Query(r => adjustNoteDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceWriteOffDetailId = 0;

                        var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);
                        invoiceWriteOff.VoucherId = voucher.Id;

                        foreach (var voucherDetailVM in NoteSetOffList)
                        {
                            var adjustNoteDetail = adjustNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdjustmentNoteDetailId);
                            if (null == adjustNoteDetail)
                                throw new CustomException("Invoice not found!");

                            adjustNoteDetail.WrittenOffAmount += voucherDetailVM.DrAmount;

                            if (adjustNoteDetail.Amount < adjustNoteDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            adjustNoteDetail.IsWrittenOff = adjustNoteDetail.Amount == adjustNoteDetail.WrittenOffAmount;
                            adjustNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            adjustNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            adjustNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _adjustmentNoteDetailRepository.Update(adjustNoteDetail);

                            // TODO: have a gap here if invoice split
                            var adjustNote = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                            adjustNote.WrittenOffAmount += voucherDetailVM.DrAmount;
                            adjustNote.IsWrittenOff = adjustNote.Amount == adjustNote.WrittenOffAmount;
                            adjustNote.UpdatedBy = invoiceWriteOff.AddedBy;
                            adjustNote.UpdatedDate = invoiceWriteOff.AddedDate;
                            adjustNote.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _adjustmentNoteRepository.Update(adjustNote);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceWriteOffDetailId++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = adjustNoteDetail.GLGeneralInfoId,
                                BudgetMasterId = adjustNoteDetail.BudgetMasterId,
                                ActivityId = adjustNoteDetail.ActivityId,
                                CurrencyId = invoiceWriteOff.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                AdjustmentNoteId = voucherDetailVM.AdjustmentNoteId,
                                AdjustmentNoteDetailId = voucherDetailVM.AdjustmentNoteDetailId,
                                CompanyId = voucherDetailVM.CompanyId,
                                PlantId = voucherDetailVM.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.DrAmount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);
                            invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                CrAmount = voucherDetailVM.DrAmount,
                                DrAmount = 0,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advance.PartyType,
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
                                CrAmount = voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate,
                            });

                            if (voucherDetailVM.ExchangeType == "ExchangeGain")
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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

                            //ExchangeGain  ExchangeLoss

                            if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            {
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                                var voucherDtEx = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    DrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountDr += voucherDtEx.DrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtEx.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    DrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountDr += voucherVM.ExchangeAmount;
                            }

                            if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                                var voucherDtExGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    DocDate = voucher.DocDate,
                                    DocRefNo = voucher.DocRefNo,
                                    Narration = voucher.Narration,
                                    CrAmount = voucherVM.ExchangeAmount,
                                    PartyType = voucherVM.ExchangeType
                                };
                                totalAmountCr += voucherDtExGain.CrAmount;

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucherDetailId);
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDtExGain.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    CrAmount = voucherVM.ExchangeAmount
                                });
                                totalCurrencyAmountCr -= voucherVM.ExchangeAmount;
                            }

                        }
                        decimal totalCharges = 0;
                        if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        {
                            var currentBankChargeDetailId = 0;
                            foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                            {
                                currentBankChargeDetailId++;
                                var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                                {
                                    AdvanceId = advance.Id,
                                    BankMasterId = advance.BankMasterId,
                                    CashMasterId = advance.CashMasterId,
                                    FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                    SourceType = advance.SourceType,
                                    Narration = voucher.Narration,
                                    Archive = advance.Archive,
                                    Amount = bankChargeDetailVM.Amount,
                                    AddedBy = advance.AddedBy,
                                    AddedDate = advance.AddedDate,
                                    AddedFromIP = advance.AddedFromIP
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
                                totalCharges += bankCharge.Amount;

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
                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource,
                        };



                        bankVoucherDetail.CrAmount = 0;
                        if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.DrAmount = advance.Amount - voucherVM.ExchangeAmount;
                        else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                            bankVoucherDetail.DrAmount = advance.Amount + voucherVM.ExchangeAmount;
                        else
                            bankVoucherDetail.DrAmount = advance.Amount;

                        totalAmountDr += bankVoucherDetail.DrAmount;


                        //if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                        //    bankVoucherDetail.DrAmount -= bankChargeDetailVMList.Sum(r => r.Amount);

                        //totalAmountDr += bankVoucherDetail.DrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                            bankVoucherDetail.PlantId = voucherVM.InterPlantId;

                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = bankVoucherDetail.DrAmount
                        });
                    }
                }

                else if (voucherVM.SettlementType == SettlementType.Others.ToString())
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        currentAdvanceDetaiId++;
                        // INSERT INTO AdvanceDetail
                        voucherDetailVM.Narration = voucherVM.Narration;
                        var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, voucherDetailVM);

                        // INSERT INTO VoucherDetail Party side
                        currentVoucherDetailId++;
                        var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            Narration = advanceDetail.Narration,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            DrAmount = advanceDetail.NetAmount,
                            PartyType = advanceDetail.PartyType,
                            EmployeeId = advanceDetail.EmployeeId,
                            PartyId = advanceDetail.PartyId,
                            PartyPlantId = advanceDetail.PartyPlantId,
                            AdvanceDetailId = advanceDetail.Id,
                        }, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount,
                        });

                        totalAmountDr += voucherDetail.DrAmount;
                        totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                        totalAmountCr += voucherDetail.CrAmount;
                        totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;
                    }

                    var bankVoucherDetail = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        CrAmount = advance.Amount,
                        PaymentSource = advance.PaymentSource
                    };
                    totalAmountCr += bankVoucherDetail.CrAmount;

                    var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                    if (null == financingTypeGL)
                        throw new CustomException("Transaction Type GL  not found!");

                    if (voucherVM.JournalType == "Payable")
                    {
                        bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                        bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                        bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                    }
                    if (voucherVM.JournalType == "Receivable")
                    {
                        bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                    }
                    //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                    //bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                    //bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                    bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = bankVoucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = bankVoucherDetail.CrAmount
                    });
                }
                else if (voucherVM.SettlementType == SettlementType.Transfer.ToString())
                {
                    currentAdvanceDetaiId++;
                    var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);

                    if (null == financingTypeGL)
                        throw new CustomException("Transaction Type GL  not found!");

                    var advanceDetail = new AdvanceDetail
                    {
                        Id = MakeAdvanceDetailPK(advance.Id, currentAdvanceDetaiId),
                        AdvanceId = advance.Id,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        EmployeeId = voucherVM.EmployeeId,
                        PartyType = voucherVM.PartyType,
                        PaymentType = null,
                        AddedBy = advance.AddedBy,
                        AddedDate = advance.AddedDate,
                        AddedFromIP = advance.AddedFromIP,
                        Archive = advance.Archive,
                        Narration = voucherVM.Narration,
                        Amount = voucherVM.Amount,
                        NetAmount = voucherVM.Amount,
                    };
                    if (voucherVM.JournalType == "Payable")
                    {
                        advanceDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                        advanceDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                        advanceDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                    }
                    if (voucherVM.JournalType == "Receivable")
                    {
                        advanceDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        advanceDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        advanceDetail.ActivityId = financingTypeGL.AssetActivityId;
                    }
                    InsertAdvanceDetail(advance, advanceDetail, currentAdvanceDetaiId);

                    // INSERT INTO VoucherDetail Party side
                    currentVoucherDetailId++;
                    var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                    {
                        Narration = advanceDetail.Narration,
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId,
                        PartyType = advanceDetail.PartyType,
                        EmployeeId = advanceDetail.EmployeeId,
                        PartyId = advanceDetail.PartyId,
                        PartyPlantId = advanceDetail.PartyPlantId,
                        AdvanceDetailId = advanceDetail.Id,
                        PlantId = voucherVM.InterPlantId
                    }, currentVoucherDetailId);
                    if (voucherVM.JournalType == "Receivable")
                    {
                        voucherDetail.CrAmount = 0;
                        voucherDetail.DrAmount = advanceDetail.NetAmount;

                    }
                    if (voucherVM.JournalType == "Payable")
                    {
                        voucherDetail.DrAmount = 0;
                        voucherDetail.CrAmount = advanceDetail.NetAmount;


                    }
                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount,
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount,
                    });


                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;

                    var bankVoucherDetail = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        PaymentSource = advance.PaymentSource
                    };
                    if (voucherVM.JournalType == "Receivable")
                    {
                        bankVoucherDetail.CrAmount = advance.Amount;
                        bankVoucherDetail.DrAmount = 0;
                    }
                    if (voucherVM.JournalType == "Payable")
                    {
                        bankVoucherDetail.DrAmount = advance.Amount;
                        bankVoucherDetail.CrAmount = 0;
                    }

                    if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    {
                        if (voucherVM.JournalType == "Payable")
                            bankVoucherDetail.DrAmount += bankChargeDetailVMList.Sum(r => r.Amount);
                        if (voucherVM.JournalType == "Receivable")
                            bankVoucherDetail.CrAmount -= bankChargeDetailVMList.Sum(r => r.Amount);
                    }
                    totalAmountCr += bankVoucherDetail.CrAmount;
                    totalAmountDr += bankVoucherDetail.DrAmount;

                    if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                    {
                        var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                        bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                        bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                        bankVoucherDetail.BankMasterId = bankMaster.Id;
                        bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                    }
                    else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                    {
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                        bankVoucherDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        bankVoucherDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                        bankVoucherDetail.ActivityId = cashMaster.ActivityId;
                        bankVoucherDetail.CashMasterId = cashMaster.Id;
                        bankVoucherDetail.PartyType = PartyType.Cash.ToString();
                    }
                    else
                        throw new CustomException("Bank or Cash Id not found!");
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = bankVoucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = bankVoucherDetail.CrAmount,
                        DrAmount = bankVoucherDetail.DrAmount
                    });
                    _voucherService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                    {
                        SourceType = advance.PaymentSource,
                        BankMasterId = bankVoucherDetail.BankMasterId,
                        CashMasterId = bankVoucherDetail.CashMasterId,
                        CrAmount = bankVoucherDetail.CrAmount,
                        DrAmount = bankVoucherDetail.DrAmount
                    });

                    decimal totalCharges = 0;
                    if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                    {
                        var currentBankChargeDetailId = 0;
                        foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                        {
                            currentBankChargeDetailId++;
                            var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                            {
                                AdvanceId = advance.Id,
                                BankMasterId = advance.BankMasterId,
                                CashMasterId = advance.CashMasterId,
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                SourceType = advance.SourceType,
                                Narration = voucher.Narration,
                                Archive = advance.Archive,
                                Amount = bankChargeDetailVM.Amount,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP
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
                            totalCharges += bankCharge.Amount;

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
                }
                totalCurrencyAmountCr += totalCurrencyAmountDr;

                if (totalAmountDr != totalAmountCr)
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

        public string UpdateInterTransaction(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.SettlementType == SettlementType.Payment.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.SettlementType == SettlementType.Payment.ToString())
                    throw new CustomException("Cash Id not found!");

                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Advance

                var advance = Find(voucherVM.AdvanceId);
                Update(advance);

                // INSERT INTO Voucher

                var voucher = _voucherService.FindVoucher(voucherVM.VoucherId);
                _voucherService.UpdateVoucher(voucher);
                // Set to Advance

                var currentVoucherDetailId = 0;
                var currentAdvanceDetaiId = 0;

                // Set Dr/Cr amount to local variable.
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;

                if (voucherVM.SettlementType == SettlementType.Payment.ToString())
                {
                    currentAdvanceDetaiId++;
                    var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);

                    if (null == financingTypeGL)
                        throw new CustomException("Transaction Type GL  not found!");
                    var advanceDetail = _advanceDetailRepository.Query(r => r.AdvanceId == voucherVM.AdvanceId).Select().FirstOrDefault();
                    _advanceDetailRepository.Update(advanceDetail);

                    var voucherDetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherVM.VoucherId && r.AdvanceDetailId != null).Select().FirstOrDefault();
                    // INSERT INTO VoucherDetail Party side
                    _voucherDetailRepository.Update(voucherDetail);


                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount,
                    });

                    totalAmountDr += voucherDetail.DrAmount;
                    totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                    totalAmountCr += voucherDetail.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;

                    var bankVoucherDetail = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        CrAmount = advance.Amount,
                        PaymentSource = advance.PaymentSource
                    };
                    totalAmountCr += bankVoucherDetail.CrAmount;

                    if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                    {
                        var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                        bankVoucherDetail.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                        bankVoucherDetail.BudgetMasterId = bankMaster.BudgetMasterId;
                        bankVoucherDetail.ActivityId = bankMaster.ActivityId;
                        bankVoucherDetail.BankMasterId = bankMaster.Id;
                        bankVoucherDetail.PartyType = PartyType.Bank.ToString();
                    }
                    else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                    {
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                        bankVoucherDetail.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                        bankVoucherDetail.BudgetMasterId = cashMaster.BudgetMasterId;
                        bankVoucherDetail.ActivityId = cashMaster.ActivityId;
                        bankVoucherDetail.CashMasterId = cashMaster.Id;
                        bankVoucherDetail.PartyType = PartyType.Cash.ToString();
                    }
                    else
                        throw new CustomException("Bank or Cash Id not found!");
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = bankVoucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = bankVoucherDetail.CrAmount
                    });
                    _voucherService.InsertGLTransactionDetail(bankVoucherDetail, new GLTransactionDetail
                    {
                        SourceType = advance.PaymentSource,
                        BankMasterId = bankVoucherDetail.BankMasterId,
                        CashMasterId = bankVoucherDetail.CashMasterId,
                        CrAmount = bankVoucherDetail.CrAmount
                    });
                }
                else if (voucherVM.SettlementType == SettlementType.SetOff.ToString())
                {
                    if (voucherVM.JournalType == JournalType.Receivable.ToString())
                    {
                        if (null == voucherDetailVMList)
                            throw new CustomException("Detail row is null.");

                        var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;

                        var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                        invoiceWriteOff.VoucherId = voucher.Id;

                        foreach (var voucherDetailVM in voucherDetailVMList)
                        {
                            var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == invoiceDetail)
                                throw new CustomException("Invoice not found!");

                            invoiceDetail.WrittenOffAmount += voucherDetailVM.CrAmount;
                            if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                            invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                            var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                            invoice.WrittenOffAmount = invoiceDetail.WrittenOffAmount;
                            invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                            invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.Update(invoice);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                ActivityId = invoiceDetail.ActivityId,
                                CurrencyId = invoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                InvoiceId = voucherDetailVM.InvoiceId,
                                InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = voucherDetailVM.CompanyId,
                                PlantId = voucherDetailVM.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.CrAmount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceDetail);
                            invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                CrAmount = voucherDetailVM.CrAmount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advance.PartyType,
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
                                CrAmount = voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate,
                            });

                            if (voucherDetailVM.ExchangeType == "ExchangeGain")
                            {
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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

                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource
                        };
                        totalAmountDr += bankVoucherDetail.DrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");
                        bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = bankVoucherDetail.DrAmount
                        });
                    }
                    if (voucherVM.JournalType == JournalType.Payable.ToString())
                    {
                        if (null == voucherDetailVMList)
                            throw new CustomException("Detail row is null.");

                        var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;

                        var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                        invoiceWriteOff.VoucherId = voucher.Id;

                        foreach (var voucherDetailVM in voucherDetailVMList)
                        {
                            var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == invoiceDetail)
                                throw new CustomException("Invoice not found!");

                            invoiceDetail.WrittenOffAmount += voucherDetailVM.DrAmount;
                            if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                            invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                            var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                            invoice.WrittenOffAmount = invoiceDetail.WrittenOffAmount;
                            invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                            invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.Update(invoice);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                ActivityId = invoiceDetail.ActivityId,
                                CurrencyId = invoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                InvoiceId = voucherDetailVM.InvoiceId,
                                InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = voucherDetailVM.CompanyId,
                                PlantId = voucherDetailVM.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.DrAmount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceDetail);
                            invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.DrAmount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advance.PartyType,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PlantId = voucherVM.InterPlantId
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
                                var gainGL = _exchangeGainLossService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                                var voucherDetailGain = new VoucherDetail
                                {
                                    GLGeneralInfoId = gainGL.CompanyCurrencyGLId,
                                    BudgetMasterId = gainGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = gainGL.CompanyCurrencyActivityId,
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
                                var lossGL = _exchangeGainLossService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                                var voucherDetailLoss = new VoucherDetail
                                {
                                    GLGeneralInfoId = lossGL.CompanyCurrencyGLId,
                                    BudgetMasterId = lossGL.CompanyCurrencyBudgetMasterId,
                                    ActivityId = lossGL.CompanyCurrencyActivityId,
                                    CurrencyId = voucher.CurrencyId,
                                    PartyType = voucherVM.ExchangeType
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

                        var bankVoucherDetail = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = advance.Amount,
                            PaymentSource = advance.PaymentSource
                        };
                        totalAmountCr += bankVoucherDetail.CrAmount;
                        var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                        if (null == financingTypeGL)
                            throw new CustomException("Transaction Type GL  not found!");

                        if (voucherVM.JournalType == "Payable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                        }
                        if (voucherVM.JournalType == "Receivable")
                        {
                            bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                            bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                            bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        }
                        //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        //bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        //bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = bankVoucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = bankVoucherDetail.CrAmount
                        });
                    }
                }
                else if (voucherVM.SettlementType == SettlementType.Others.ToString())
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        currentAdvanceDetaiId++;
                        // INSERT INTO AdvanceDetail
                        voucherDetailVM.Narration = voucherVM.Narration;
                        var advanceDetail = InsertAdvanceDetail(advance, currentAdvanceDetaiId, voucherDetailVM);

                        // INSERT INTO VoucherDetail Party side
                        currentVoucherDetailId++;
                        var voucherDetail = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            Narration = advanceDetail.Narration,
                            GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceDetail.BudgetMasterId,
                            ActivityId = advanceDetail.ActivityId,
                            DrAmount = advanceDetail.NetAmount,
                            PartyType = advanceDetail.PartyType,
                            EmployeeId = advanceDetail.EmployeeId,
                            PartyId = advanceDetail.PartyId,
                            PartyPlantId = advanceDetail.PartyPlantId,
                            AdvanceDetailId = advanceDetail.Id,
                        }, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount,
                        });

                        totalAmountDr += voucherDetail.DrAmount;
                        totalCurrencyAmountDr += voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount;
                        totalAmountCr += voucherDetail.CrAmount;
                        totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount;
                    }

                    var bankVoucherDetail = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        CrAmount = advance.Amount,
                        PaymentSource = advance.PaymentSource
                    };
                    totalAmountCr += bankVoucherDetail.CrAmount;

                    var financingTypeGL = _financingTypeGLService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                    if (null == financingTypeGL)
                        throw new CustomException("Transaction Type GL  not found!");

                    if (voucherVM.JournalType == "Payable")
                    {
                        bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                        bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                        bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                    }
                    if (voucherVM.JournalType == "Receivable")
                    {
                        bankVoucherDetail.GLGeneralInfoId = financingTypeGL.AssetGLId;
                        bankVoucherDetail.BudgetMasterId = financingTypeGL.AssetBudgetMasterId;
                        bankVoucherDetail.ActivityId = financingTypeGL.AssetActivityId;
                    }
                    //bankVoucherDetail.GLGeneralInfoId = financingTypeGL.LiabilityGLId;
                    //bankVoucherDetail.BudgetMasterId = financingTypeGL.LiabilityBudgetMasterId;
                    //bankVoucherDetail.ActivityId = financingTypeGL.LiabilityActivityId;
                    bankVoucherDetail.PlantId = voucherVM.InterPlantId;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, bankVoucherDetail, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(bankVoucherDetail, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = bankVoucherDetail.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(bankVoucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = bankVoucherDetail.CrAmount
                    });
                }

                totalCurrencyAmountCr += totalCurrencyAmountDr;

                if (totalAmountDr != totalAmountCr)
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

        public void DeleteVendorAdvance(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Loan
                _unitOfWork.BeginTransaction();
                flag = true;
                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                //vendorAdWrsql = @"update trn.VoucherDetail set InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                //vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.AdvanceDetail where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.BankCharge where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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

        public void DeleteMultiVendorAdvance(string companyId, string plantId, string voucherId,string advanceGroupNo)
        {
            var flag = false;
            try
            {
                var advanceList = base.Query(r=>r.AdvanceGroupNo== advanceGroupNo).Select().ToList();
                
                _unitOfWork.BeginTransaction();
                flag = true;
                if (advanceList !=null && advanceList.Count>0)
                {
                    foreach (var item in advanceList)
                    {
                        var vendorAdWr = new System.Text.StringBuilder();
                        var vendorAdWrsql = "";
                        vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "'))";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"update trn.VoucherDetail set InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "'))";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete TRN.AdvanceDetail where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "'))";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete TRN.BankCharge where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "'))";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + item.VoucherId.ToString() + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                        _unitOfWork.SaveChanges();
                    }
                }
                
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

        public void DeleteEmployeeAdvanceWriteOff(string advanceWriteOffId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var advanceWriteOff = _advanceWriteOffRepository.Find(advanceWriteOffId);
                var advanceWriteOffDetail = _advanceWriteOffDetailRepository.Query(r => r.AdvanceWriteOffId == advanceWriteOffId).Select().ToList();
                var employeePayableWriteOff = _employeePayableWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                var employeeSubsequentTransaction = _employeeSubsequentTransactionRepository.Query(r => r.VoucherId == voucherId).Select().ToList();


                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }
                foreach (var item in employeeSubsequentTransaction)
                {
                    _employeeSubsequentTransactionRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var gltransactionDetail = _gLTransactionDetailRepository.Find(item.Id);
                    if (gltransactionDetail != null)
                        _gLTransactionDetailRepository.Delete(gltransactionDetail.Id);
                    _voucherDetailRepository.Delete(item.Id);
                }


                if (employeePayableWriteOff != null)
                {
                    var employeePayableWriteOffDetail = _employeePayableWriteOffDetailRepository.Query(r => r.EmployeePayableWriteOffId == employeePayableWriteOff.Id).Select().ToList();
                    foreach (var item in employeePayableWriteOffDetail)
                    {
                        var employeePayable = _employeePayableRepository.Find(item.EmployeePayableId);
                        var employeePayableDetail = _employeePayableDetailRepository.Find(item.EmployeePayableDetailId);

                        employeePayableDetail.WrittenOffAmount -= item.Amount;
                        employeePayable.WrittenOffAmount -= item.Amount;
                        employeePayableDetail.IsWrittenOff = employeePayableDetail.NetAmount == employeePayableDetail.WrittenOffAmount;
                        employeePayable.IsWrittenOff = employeePayable.Amount == employeePayable.WrittenOffAmount;

                        _employeePayableDetailRepository.Update(employeePayableDetail);
                        _employeePayableRepository.Update(employeePayable);
                        _employeePayableWriteOffDetailRepository.Delete(item.Id);
                    }
                    _employeePayableWriteOffRepository.Delete(employeePayableWriteOff.Id);
                }
                var direct = new System.Text.StringBuilder();
                var directsql = "";
                if (advanceWriteOffDetail != null)
                {
                    foreach (var item in advanceWriteOffDetail)
                    {
                        if(item.AdvanceId != null)
                        {
                            var advance = base.Find(item.AdvanceId);
                            var advanceDetail = _advanceDetailRepository.Find(item.AdvanceDetailId);

                            advanceDetail.WrittenOffAmount -= item.Amount;
                            advance.WrittenOffAmount -= item.Amount;
                            advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                            advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;

                            _advanceDetailRepository.Update(advanceDetail);
                            base.Update(advance);
                        }
                        if (item.EmployeeAdvanceDetailId != null)
                        {
                            directsql = @"DECLARE @newWrittenOffAmount decimal(18,2)=" + item.Amount + @",@employeeAdvanceDetailId varchar(50)='" + item.EmployeeAdvanceDetailId + @"'
                                          update [TRN].[EmployeeAdvanceDetail] set WrittenOffAmount= ISNULL(WrittenOffAmount,0) -  @newWrittenOffAmount, IsWrittenOff=0 where Id=@employeeAdvanceDetailId ";
                            direct.Append(directsql);
                        }

                            _advanceWriteOffDetailRepository.Delete(item.Id);
                    }
                    _advanceWriteOffRepository.Delete(advanceWriteOff.Id);
                }

                _voucherRepository.Delete(voucher.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directsql != "")
                {
                    _sqlRepository.ExecuteSqlCommand(direct.ToString());
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void DeleteEmployeeTotalAdvanceWriteOff(string advanceWriteOffId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherRepository.Find(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var advanceWriteOff = _advanceWriteOffRepository.Find(advanceWriteOffId);
                var advanceWriteOffDetail = _advanceWriteOffDetailRepository.Query(r => r.AdvanceWriteOffId == advanceWriteOffId).Select().ToList();
                var employeePayableWriteOff = _employeePayableWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                var employeeSubsequentTransaction = _employeeSubsequentTransactionRepository.Query(r => r.VoucherId == voucherId).Select().ToList();


                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }
                foreach (var item in employeeSubsequentTransaction)
                {
                    _employeeSubsequentTransactionRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var gltransactionDetail = _gLTransactionDetailRepository.Find(item.Id);
                    if (gltransactionDetail != null)
                        _gLTransactionDetailRepository.Delete(gltransactionDetail.Id);
                    _voucherDetailRepository.Delete(item.Id);
                }


                if (employeePayableWriteOff != null)
                {
                    var employeePayableWriteOffDetail = _employeePayableWriteOffDetailRepository.Query(r => r.EmployeePayableWriteOffId == employeePayableWriteOff.Id).Select().ToList();
                    foreach (var item in employeePayableWriteOffDetail)
                    {
                        var employeePayable = _employeePayableRepository.Find(item.EmployeePayableId);
                        var employeePayableDetail = _employeePayableDetailRepository.Find(item.EmployeePayableDetailId);

                        employeePayableDetail.WrittenOffAmount -= item.Amount;
                        employeePayable.WrittenOffAmount -= item.Amount;
                        employeePayableDetail.IsWrittenOff = employeePayableDetail.NetAmount == employeePayableDetail.WrittenOffAmount;
                        employeePayable.IsWrittenOff = employeePayable.Amount == employeePayable.WrittenOffAmount;

                        _employeePayableDetailRepository.Update(employeePayableDetail);
                        _employeePayableRepository.Update(employeePayable);
                        _employeePayableWriteOffDetailRepository.Delete(item.Id);
                    }
                    _employeePayableWriteOffRepository.Delete(employeePayableWriteOff.Id);
                }

                if (advanceWriteOffDetail != null)
                {
                    foreach (var item in advanceWriteOffDetail)
                    {
                        //var advance = base.Find(item.AdvanceId);
                        //var advanceDetail = _advanceDetailRepository.Find(item.AdvanceDetailId);

                        //advanceDetail.WrittenOffAmount -= item.Amount;
                        //advance.WrittenOffAmount -= item.Amount;
                        //advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                        //advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;

                        //_advanceDetailRepository.Update(advanceDetail);
                        //base.Update(advance);
                        _advanceWriteOffDetailRepository.Delete(item.Id);
                    }
                    _advanceWriteOffRepository.Delete(advanceWriteOff.Id);
                }

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
        public void DeleteInterTransaction(string advanceId, string voucherId)
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
                var advance = base.Find(advanceId);
                var bankCharge = _bankChargeRepository.Query(r => r.AdvanceId == advanceId);
                var advanceDetail = _advanceDetailRepository.Query(r => r.AdvanceId == advanceId).Select().ToList();


                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    if (advance.SettlementType == SettlementType.SetOff.ToString() || advance.SettlementType == SettlementType.DebitNoteSetOff.ToString() || advance.SettlementType == SettlementType.CreditNoteSetOff.ToString())
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"UPDATE [TRN].voucherdetail SET InvoiceWriteOffDetailId=NULL,AdvanceWriteOffDetailId=NULL,AdjustmentNoteDetailId=NULL,UpdatedBy='" + identity.UserId + "' WHERE Id='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }
                    if (advance.SettlementType == SettlementType.Payment.ToString() || advance.SettlementType == SettlementType.Transfer.ToString())
                    {
                        var gltransactionDetail = _gLTransactionDetailRepository.Find(item.Id);
                        if (gltransactionDetail != null)
                            _gLTransactionDetailRepository.Delete(gltransactionDetail.Id);
                    }
                    _voucherDetailRepository.Delete(item.Id);
                }

                foreach (var item in advanceDetail)
                {
                    _advanceDetailRepository.Delete(item.Id);
                }
                if (advance.SettlementType == SettlementType.SetOff.ToString())
                {
                    var invoiceWritOff = _invoiceWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                    var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWritOff.Id).Select().ToList();
                    foreach (var item in invoiceWriteOffDetail)
                    {
                        var invoice = _invoiceRepository.Find(item.InvoiceId);
                        var invoiceDetail = _invoiceDetailRepository.Find(item.InvoiceDetailId);
                        invoiceDetail.WrittenOffAmount -= item.Amount;
                        invoice.WrittenOffAmount -= item.Amount;
                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;

                        _invoiceDetailRepository.Update(invoiceDetail);
                        _invoiceRepository.Update(invoice);
                        _invoiceWriteOffDetailRepository.Delete(item.Id);
                    }
                    _invoiceWriteOffRepository.Delete(invoiceWritOff.Id);
                }
                if (advance.SettlementType == SettlementType.EmployeePayableSetOff.ToString())
                {
                    var invoiceWritOff = _invoiceWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                    var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWritOff.Id).Select().ToList();
                    foreach (var item in invoiceWriteOffDetail)
                    {
                        var invoice = _invoiceRepository.Find(item.InvoiceId);
                        var invoiceDetail = _invoiceDetailRepository.Find(item.InvoiceDetailId);
                        invoiceDetail.WrittenOffAmount -= item.Amount;
                        invoice.WrittenOffAmount -= item.Amount;
                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;

                        _invoiceDetailRepository.Update(invoiceDetail);
                        _invoiceRepository.Update(invoice);
                        _invoiceWriteOffDetailRepository.Delete(item.Id);
                    }
                    _invoiceWriteOffRepository.Delete(invoiceWritOff.Id);
                }
                if (advance.SettlementType == SettlementType.AdvanceToVendor.ToString())
                {
                    var advanceTax = _invoiceTaxService.Query(r => r.AdvanceId == advanceId).Select().FirstOrDefault();

                    var advanceTaxDetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == advanceTax.Id).Select().ToList();
                    foreach (var item in advanceTaxDetail)
                    {
                        _invoiceTaxDetailRepository.Delete(item.Id);
                    }
                    _invoiceTaxService.Delete(advanceTax.Id);
                }
                if (advance.SettlementType == SettlementType.DebitNoteSetOff.ToString() || advance.SettlementType == SettlementType.CreditNoteSetOff.ToString())
                {
                    var invoiceWritOff = _invoiceWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                    var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWritOff.Id).Select().ToList();
                    foreach (var item in invoiceWriteOffDetail)
                    {
                        var adjustment = _adjustmentNoteRepository.Find(item.AdjustmentNoteId);
                        var adjustmentDetail = _adjustmentNoteDetailRepository.Find(item.AdjustmentNoteDetailId);
                        adjustmentDetail.WrittenOffAmount -= item.Amount;
                        adjustment.WrittenOffAmount -= item.Amount;
                        adjustmentDetail.IsWrittenOff = adjustmentDetail.Amount == adjustmentDetail.WrittenOffAmount;
                        adjustment.IsWrittenOff = adjustment.Amount == adjustment.WrittenOffAmount;

                        _adjustmentNoteDetailRepository.Update(adjustmentDetail);
                        _adjustmentNoteRepository.Update(adjustment);
                        _invoiceWriteOffDetailRepository.Delete(item.Id);
                    }
                    _invoiceWriteOffRepository.Delete(invoiceWritOff.Id);
                }
                base.Delete(advanceId);
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

        public void DeleteEmployeeAdvance(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {
                ConnectionManager.DAL.ConManager objCon1;
                DataSet dsMaster1 = null;
                string setOffsql = @"SELECT VoucherNo from trn.AdvanceWriteOffDetail iwd JOIN trn.AdvanceWriteOff iw on iw.Id=iwd.AdvanceWriteOffId LEFT JOIN trn.Voucher v on v.Id = iw.VoucherId
                                            WHERE iwd.AdvanceId in (select Id from trn.Advance where VoucherId = '" + voucherId + "')";
                objCon1 = new ConnectionManager.DAL.ConManager("1");
                objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                if (dsMaster1.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                }

                // Delete Loan
                _unitOfWork.BeginTransaction();
                flag = true;
                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete dbo.AdvanceReqSchedule where EmployeeSalaryAdvanceId in (select Id from trn.EmployeeSalaryAdvance  where voucherId in  ('" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.EmployeeSalaryAdvance  where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.EmployeeSubsequentTransaction  where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"update trn.VoucherDetail set InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.AdvanceDetail where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.BankCharge where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.Advance where voucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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

        public void DeleteEmployeeAdvanceHR(string employeeAdvanceId, string voucherId)
        {
            var flag = false;
            try
            {
                ConnectionManager.DAL.ConManager objCon1;
                DataSet dsMaster1 = null;
                string setOffsql = @"SELECT VoucherNo from trn.AdvanceWriteOffDetail iwd JOIN trn.AdvanceWriteOff iw on iw.Id=iwd.AdvanceWriteOffId LEFT JOIN trn.Voucher v on v.Id = iw.VoucherId
                                            WHERE iwd.EmployeeAdvanceDetailId in (select Id from trn.EmployeeAdvanceDetail where VoucherId = '" + voucherId + "')";
                objCon1 = new ConnectionManager.DAL.ConManager("1");
                objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                if (dsMaster1.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                }

                // Delete Loan
                _unitOfWork.BeginTransaction();
                flag = true;
                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"update [TRN].[EmployeeAdvanceRequisition] set IsPost=0 WHERE  SystemId in (select RequisitionId from TRN.EmployeeAdvance where  Id = '" + employeeAdvanceId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete trn.voucherdetailcurrency where VoucherId in (select Id from trn.voucher where  Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete trn.GLTransactionDetail where VoucherDetailId in (select id from trn.voucherdetail where VoucherId in (select Id from trn.voucher where  Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
               
                vendorAdWrsql = @"delete trn.EmployeeAdvanceDetail  where voucherId in (select Id from trn.voucher where  Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.EmployeeSubsequentTransaction  where voucherId in (select Id from trn.voucher where Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"update trn.VoucherDetail set InvoiceTaxDetailId=NULL  where voucherId in (select Id from trn.voucher where  Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"update trn.InvoiceTax set VoucherDetailId=NULL where voucherId in (select Id from trn.voucher where  Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucherdetail where VoucherId in (select Id from trn.voucher where  Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.InvoiceTaxDetail where InvoiceTaxId in (select Id from trn.InvoiceTax where voucherId in (select Id from trn.voucher where  Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.InvoiceTax where voucherId in (select Id from trn.voucher where  Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.AdvanceDetail where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where  Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.BankCharge where AdvanceId in (select Id from TRN.Advance where voucherId in (select Id from trn.voucher where  Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete TRN.EmployeeAdvance where Id in ('" + employeeAdvanceId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucher where Id in (select Id from trn.voucher where  Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
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