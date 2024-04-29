using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.Advances;
using Library.Model.Banks;
using Library.Model.Commercial;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Taxations;
using Library.Model.Vouchers;
using Library.Service.Banks;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Extension.Accounts;
using Library.Service.Finances;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Invoices
{
    public class InvoiceWriteOffService : Service<InvoiceWriteOff>, IInvoiceWriteOffService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<InvoiceWriteOff> _invoiceWriteOffRepository;
        private readonly IRepositoryAsync<InvoiceWriteOffDetail> _invoiceWriteOffDetailRepository;
        private readonly IRepositoryAsync<VoucherWriteOff> _voucherWriteOffRepository;
        private readonly IRepositoryAsync<VoucherWriteOffDetail> _voucherWriteOffDetailRepository;
        private readonly IVoucherService _voucherService;
        private readonly IRepositoryAsync<OtherInvoice> _otherInvoiceRepository;
        private readonly IRepositoryAsync<AdditionalInvoice> _additionalInvoiceRepository;
        private readonly IRepositoryAsync<AdditionalInvoiceDetail> _additionalInvoiceDetailRepository;
        private readonly IBankChargeService _bankChargeService;
        private readonly IRepositoryAsync<TaxCode> _taxCodeRepository;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IFinancingTypeGLService _financingTypeGLService;
        private readonly IBankJournalService _bankJournalService;
        private readonly IRepositoryAsync<AdjustmentNote> _adjustmentNoteRepository;
        private readonly IRepositoryAsync<AdjustmentNoteDetail> _adjustmentNoteDetailRepository;
        private readonly IRepositoryAsync<PurchaseLC> _purchaseLCRepository;
        private readonly IRepositoryAsync<PurchaseLCCharges> _purchaseLCChargesRepository;
        private readonly IRepositoryAsync<PurchaseLCTax> _purchaseLCTaxRepository;
        private readonly IRepositoryAsync<FinancingType> _financingTypeRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<AdditionalTax> _additionalTaxRepository;
        private readonly IRepositoryAsync<AdditionalTaxDetail> _additionalTaxDetailRepository;
        private readonly IRepositoryAsync<MultiplePayment> _multiplePaymentRepository;
        private readonly IRepositoryAsync<MultiplePaymentDetail> _multiplePaymentDetailRepository;
        private readonly IInvoiceService _invoiceService;
        private readonly IRepositoryAsync<FinancingTypeGL> _financingTypeGLRepository;
        private readonly IRepositoryAsync<BankCharge> _bankChargeRepository;
        private readonly IFinancingService _financingService;
        private readonly IRepositoryAsync<FinancingSubsequentTransaction> _loanInterestPayableRepository;
        private readonly IRepositoryAsync<FinancingWriteOff> _financingWriteOffRepository;
        private readonly IRepositoryAsync<Advance> _advanceRepository;
        private readonly IRepositoryAsync<AdvanceDetail> _advanceDetailRepository;
        private readonly IRepositoryAsync<AdvanceWriteOff> _advanceWriteOffRepository;
        private readonly IRepositoryAsync<AdvanceWriteOffDetail> _advanceWriteOffDetailRepository;
        private readonly IRepositoryAsync<EmployeeSubsequentTransaction> _employeeSubsequentTransactionRepository;


        public InvoiceWriteOffService(
              IRepositoryAsync<InvoiceWriteOff> invoiceWriteOffRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<InvoiceWriteOffDetail> invoiceWriteOffDetailRepository
            , IRepositoryAsync<VoucherWriteOff> voucherWriteOffRepository
            , IRepositoryAsync<VoucherWriteOffDetail> voucherWriteOffDetailRepository
            , IVoucherService voucherService
            , IRepositoryAsync<OtherInvoice> otherInvoiceRepository
            , ISqlRepository sqlRepository
            , IBankChargeService bankChargeService
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<TaxCode> taxCodeRepository
            , IFinancingTypeGLService financingTypeGLService
            , IBankJournalService bankJournalService
            , IRepositoryAsync<AdjustmentNote> adjustmentNoteRepository
            , IRepositoryAsync<AdjustmentNoteDetail> adjustmentNoteDetailRepository
            , IRepositoryAsync<PurchaseLC> purchaseLCRepository
            , IRepositoryAsync<PurchaseLCCharges> purchaseLCChargesRepository
            , IRepositoryAsync<PurchaseLCTax> purchaseLCTaxRepository
            , IRepositoryAsync<FinancingType> financingTypeRepository
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            , IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository
            , IInvoiceService invoiceService
            , IRepositoryAsync<AdditionalTax> additionalTaxRepository
            , IRepositoryAsync<AdditionalTaxDetail> additionalTaxDetailRepository
            , IRepositoryAsync<MultiplePaymentDetail> multiplePaymentDetailRepository
            , IRepositoryAsync<MultiplePayment> multiplePaymentRepository
            , IRepositoryAsync<FinancingTypeGL> financingTypeGLRepository
            , IRepositoryAsync<BankCharge> bankChargeRepository
            , IFinancingService financingService
            , IRepositoryAsync<FinancingSubsequentTransaction> loanInterestPayableRepository
            , IRepositoryAsync<FinancingWriteOff> financingWriteOffRepository
            , IRepositoryAsync<AdditionalInvoice> additionalInvoiceRepository
            , IRepositoryAsync<AdditionalInvoiceDetail> additionalInvoiceDetailRepository
            , IRepositoryAsync<Advance> advanceRepository
            , IRepositoryAsync<AdvanceDetail> advanceDetailRepository
            , IRepositoryAsync<AdvanceWriteOff> advanceWriteOffRepository
            , IRepositoryAsync<AdvanceWriteOffDetail> advanceWriteOffDetailRepository
            , IRepositoryAsync<EmployeeSubsequentTransaction> employeeSubsequentTransactionRepository

            ) : base(invoiceWriteOffRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _invoiceWriteOffRepository = invoiceWriteOffRepository;
            _invoiceWriteOffDetailRepository = invoiceWriteOffDetailRepository;
            _voucherWriteOffRepository = voucherWriteOffRepository;
            _voucherWriteOffDetailRepository = voucherWriteOffDetailRepository;
            _voucherService = voucherService;
            _otherInvoiceRepository = otherInvoiceRepository;
            _bankChargeService = bankChargeService;
            _invoiceTaxService = invoiceTaxService;
            _taxCodeRepository = taxCodeRepository;
            _financingTypeGLService = financingTypeGLService;
            _bankJournalService = bankJournalService;
            _adjustmentNoteDetailRepository = adjustmentNoteDetailRepository;
            _adjustmentNoteRepository = adjustmentNoteRepository;
            _purchaseLCRepository = purchaseLCRepository;
            _purchaseLCChargesRepository = purchaseLCChargesRepository;
            _financingTypeRepository = financingTypeRepository;
            _invoiceTaxRepository = invoiceTaxRepository;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _purchaseLCTaxRepository = purchaseLCTaxRepository;
            _invoiceService = invoiceService;
            _additionalTaxRepository = additionalTaxRepository;
            _additionalTaxDetailRepository = additionalTaxDetailRepository;
            _multiplePaymentDetailRepository = multiplePaymentDetailRepository;
            _multiplePaymentRepository = multiplePaymentRepository;
            _financingTypeGLRepository = financingTypeGLRepository;
            _bankChargeRepository = bankChargeRepository;
            _financingService = financingService;
            _loanInterestPayableRepository = loanInterestPayableRepository;
            _financingWriteOffRepository = financingWriteOffRepository;
            _additionalInvoiceRepository = additionalInvoiceRepository;
            _additionalInvoiceDetailRepository = additionalInvoiceDetailRepository;
            _advanceRepository = advanceRepository;
            _advanceDetailRepository = advanceDetailRepository;
            _advanceWriteOffRepository = advanceWriteOffRepository;
            _advanceWriteOffDetailRepository = advanceWriteOffDetailRepository;
            _employeeSubsequentTransactionRepository = employeeSubsequentTransactionRepository;
        }

        public InvoiceWriteOff InsertInvoiceWriteOff(InvoiceWriteOff invoiceWriteOff)
        {
            invoiceWriteOff.Id = GetAutoNumber(nameof(InvoiceWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
            base.InsertGraph(invoiceWriteOff);
            return invoiceWriteOff;
        }
        public AdditionalInvoice InsertAdditionalInvoice(AdditionalInvoice additionalInvoice)
        {
            additionalInvoice.Id = GetAutoNumber(nameof(AdditionalInvoice), PKGeneratorEnum.Yearly, null, DateTime.Now);
            AuditService.AddedLog(additionalInvoice);
            _additionalInvoiceRepository.Insert(additionalInvoice);
            return additionalInvoice;
        }
        private void Check(InvoiceWriteOff entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        }
        public IQueryFluent<InvoiceWriteOff> QueryInvoiceWriteOff(string voucherId)
        {
            return base.Query(r => r.VoucherId == voucherId);
        }
        public InvoiceWriteOff FindInvoiceWriteOff(string Id)
        {
            return base.Find(Id);
        }
        public void DeleteInvoiceWriteOff(string id)
        {
            base.Delete(id);
        }
        public IQueryFluent<InvoiceWriteOffDetail> QueryInvoiceWriteOffDetail(string invoiceWriteOffId)
        {
            return _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWriteOffId);
        }
        public void DeleteInvoiceWriteOffDetail(string id)
        {
            _invoiceWriteOffDetailRepository.Delete(id);
        }
        public InvoiceWriteOff InsertInvoiceWriteOff(VoucherViewModel voucherVM)
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
            return InsertInvoiceWriteOff(invoiceWriteOff);
        }
        public InvoiceWriteOff InsertMultipleVendorInvoiceWriteOff(VoucherViewModel voucherVM)
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
            return InsertInvoiceWriteOff(invoiceWriteOff);
        }

        public AdditionalInvoice InsertAdditionalInvoice(VoucherViewModel voucherVM)
        {
            var additionalInvoice = new AdditionalInvoice
            {
                CompanyGroupId = voucherVM.CompanyGroupId,
                CompanyId = voucherVM.CompanyId,
                PlantId = voucherVM.PlantId,
                VoucherTypeId = voucherVM.VoucherTypeId,
                CurrencyId = voucherVM.CurrencyId,
                SourceType = voucherVM.SourceType,
                PartyType = voucherVM.PartyType,
                PartyId = voucherVM.PartyId,
                PartyPlantId = voucherVM.PartyPlantId,
                Amount = voucherVM.Amount,
                PostingDate = voucherVM.PostingDate,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                Narration = voucherVM.Narration,
                AddedBy = voucherVM.AddedBy,
                AddedDate = voucherVM.AddedDate,
                AddedFromIP = voucherVM.AddedFromIP,
                IsPark = voucherVM.IsPark,
            };

            return InsertAdditionalInvoice(additionalInvoice);
        }
        public InvoiceWriteOff InsertInvoiceWriteOffDifferentCurrency(InvoiceWriteOff invoiceWriteOffVM)
        {
            if (invoiceWriteOffVM.PaymentSource == PaymentSource.Bank.ToString())
                if (string.IsNullOrEmpty(invoiceWriteOffVM.BankMasterId))
                    throw new CustomException("Bank Id not found!");
                else
                    invoiceWriteOffVM.CashMasterId = null;
            else if (invoiceWriteOffVM.PaymentSource == PaymentSource.Cash.ToString())
                if (string.IsNullOrEmpty(invoiceWriteOffVM.CashMasterId))
                    throw new CustomException("Cash Id not found!");
                else
                    invoiceWriteOffVM.BankMasterId = null;

            var invoiceWriteOff = new InvoiceWriteOff
            {
                CompanyGroupId = invoiceWriteOffVM.CompanyGroupId,
                CompanyId = invoiceWriteOffVM.CompanyId,
                PlantId = invoiceWriteOffVM.PlantId,
                FiscalYearId = invoiceWriteOffVM.FiscalYearId,
                FiscalYearPeriodId = invoiceWriteOffVM.FiscalYearPeriodId,
                TaxYearId = invoiceWriteOffVM.TaxYearId,
                TaxYearPeriodId = invoiceWriteOffVM.TaxYearPeriodId,
                VoucherTypeId = invoiceWriteOffVM.VoucherTypeId,
                CurrencyId = invoiceWriteOffVM.CurrencyId,
                SourceType = invoiceWriteOffVM.SourceType,
                PartyType = invoiceWriteOffVM.PartyType,
                PartyId = invoiceWriteOffVM.PartyId,
                PartyPlantId = invoiceWriteOffVM.PartyPlantId,
                Amount = invoiceWriteOffVM.Amount,
                VoucherDate = invoiceWriteOffVM.VoucherDate,
                PostingDate = invoiceWriteOffVM.PostingDate,
                DocDate = invoiceWriteOffVM.DocDate,
                DocRefNo = invoiceWriteOffVM.DocRefNo,
                Narration = invoiceWriteOffVM.Narration,
                AddedBy = invoiceWriteOffVM.AddedBy,
                AddedDate = invoiceWriteOffVM.AddedDate,
                AddedFromIP = invoiceWriteOffVM.AddedFromIP,
                IsPark = invoiceWriteOffVM.IsPark,
                Archive = false,
                BankMasterId = invoiceWriteOffVM.BankMasterId,
                CashMasterId = invoiceWriteOffVM.CashMasterId,
                EmployeeId = invoiceWriteOffVM.EmployeeId,
                PaymentSource = invoiceWriteOffVM.PaymentSource,
                RoundingType = invoiceWriteOffVM.RoundingType,
                RoundingAmount = invoiceWriteOffVM.RoundingAmount,
                InvoiceWriteOffGroupNo = invoiceWriteOffVM.InvoiceWriteOffGroupNo
            };
            if (invoiceWriteOffVM.SourceType != "CustomerBanksReceipt")
            {
                Check(invoiceWriteOff);
            }
            return InsertInvoiceWriteOff(invoiceWriteOff);
        }

        public InvoiceWriteOff InsertCustomerInvoiceSetOff(VoucherViewModel voucherVM)
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
                RoundingAmount = voucherVM.RoundingAmount,
                InvoiceWriteOffGroupNo = voucherVM.InvoiceWriteOffGroupNo
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

        public void InsertAdditionalInvoiceDetail(AdditionalInvoice additionalInvoice, AdditionalInvoiceDetail additionalInvoiceDetail, int currentId)
        {
            additionalInvoiceDetail.AddedBy = additionalInvoice.AddedBy;
            additionalInvoiceDetail.AddedDate = additionalInvoice.AddedDate;
            additionalInvoiceDetail.AddedFromIP = additionalInvoice.AddedFromIP;
            additionalInvoiceDetail.AdditionalInvoiceId = additionalInvoice.Id;
            additionalInvoiceDetail.Id = MakePK(additionalInvoice.Id, currentId, 2);
            _additionalInvoiceDetailRepository.Insert(additionalInvoiceDetail);
        }

        private void NoteCheck(AdjustmentNote entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        }


        private AdjustmentNote InsertAdjustmentNote(AdjustmentNote adjustmentNote)
        {
            adjustmentNote.Id = base.GetAutoNumber(nameof(AdjustmentNote), PKGeneratorEnum.Yearly, null, DateTime.Now);
            _adjustmentNoteRepository.Insert(adjustmentNote);
            return adjustmentNote;
        }
        private AdjustmentNote InsertAdjustmentNote(VoucherViewModel voucherVM)
        {
            var adjustmentNote = new AdjustmentNote
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
                Amount = voucherVM.Amount,
                VoucherDate = voucherVM.VoucherDate,
                PostingDate = voucherVM.PostingDate,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                Narration = voucherVM.Narration,
                PartyId = voucherVM.OtherPartyId,
                PartyPlantId = voucherVM.OtherPartyPlantId,
                SourceType = voucherVM.SourceType,
                IsPark = voucherVM.IsPark,
                NoteType = NoteType.VendorCreditNote.ToString(),
                InvoiceId = voucherVM.InvoiceId,
                Archive = false,
                SettlementType = voucherVM.SettlementType,
                PartyType = PartyType.Vendor.ToString(),
                AddedBy = voucherVM.AddedBy,
                AddedDate = voucherVM.AddedDate,
                AddedFromIP = voucherVM.AddedFromIP
            };

            NoteCheck(adjustmentNote);
            return InsertAdjustmentNote(adjustmentNote);
        }
        private AdjustmentNoteDetail InsertAdjustmentNoteDetail(AdjustmentNote adjustmentNote, AdjustmentNoteDetail adjustmentNoteDetail, int currentId)
        {
            adjustmentNoteDetail.Id = MakePK(adjustmentNote.Id, currentId, 1);
            adjustmentNoteDetail.AdjustmentNoteId = adjustmentNote.Id;
            adjustmentNoteDetail.InvoiceId = adjustmentNote.InvoiceId;
            adjustmentNoteDetail.AddedBy = adjustmentNote.AddedBy;
            adjustmentNoteDetail.AddedDate = adjustmentNote.AddedDate;
            adjustmentNoteDetail.AddedFromIP = adjustmentNote.AddedFromIP;
            adjustmentNoteDetail.Archive = adjustmentNote.Archive;
            adjustmentNoteDetail.Amount = adjustmentNote.Amount;
            _adjustmentNoteDetailRepository.Insert(adjustmentNoteDetail);
            return adjustmentNoteDetail;
        }
        private FinancingTypeGL GetCreditNoteGL(string companyId, string financingTypeId)
        {
            var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                        WHERE C.Id='" + companyId + "' AND LTGGL.FinancingTypeId='" + financingTypeId + "'";
            var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
            if (null == glTemp || string.IsNullOrEmpty(glTemp.LiabilityGLId))
                throw new CustomException("This transaction type GL not found!");
            return glTemp;
        }

        public string InsertOtherInvicePost(VoucherViewModel voucherVM, string otherInvoiceId, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _unitOfWork.BeginTransaction();
                flag = true;
                var otherInvoice = _otherInvoiceRepository.Find(otherInvoiceId);
                if (otherInvoice.VoucherId != null)
                    throw new CustomException("Govt Subsidy JV already posted.");
                voucherVM.IsPark = false;
                voucherVM.PartyPlantId = voucherVM.InvoicingPartyPlantId;
                voucherVM.Amount = otherInvoice.Amount;
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher

                var voucher = _voucherService.InsertVoucher(voucherVM);
                AuditService.PostedLog(voucher);
                otherInvoice.VoucherId = voucher.Id;
                otherInvoice.IsPark = false;
                AuditService.UpdatedLog(otherInvoice);
                _otherInvoiceRepository.Update(otherInvoice);
                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                var withholdgl = false;
                var currentInvoiceDetail = 0;
                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);
                var invoiceVM = new VoucherViewModel();
                invoiceVM = voucherVM;
                invoiceVM.PartyId = otherInvoice.PartyId;
                invoiceVM.PartyPlantId = otherInvoice.PartyPlantId;
                invoiceVM.PartyType = "Customer";
                invoiceVM.SourceType = SourceType.CustomerInvoice.ToString();
                var invoicedata = _invoiceService.InsertInvoice(invoiceVM);
                invoicedata.VoucherId = voucher.Id;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.TrnType == "Cr")
                    {
                        var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");

                        invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                        if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                            throw new CustomException("Received amount can not cross balance amount.");

                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                        // TODO: have a gap here if invoice split
                        var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                        invoice.WrittenOffAmount += voucherDetailVM.Amount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.Update(invoice);

                        // INSERT INTO InvoiceDetail
                        currentInvoiceWriteOffDetailId++;
                        var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucherVM.CurrencyId,
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            InvoiceId = voucherDetailVM.InvoiceId,
                            InvoiceDetailId = voucherDetailVM.Id,
                            Amount = voucherDetailVM.CrAmount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP,
                            Archive = invoiceWriteOff.Archive,
                            ModelState = invoiceWriteOff.ModelState,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration
                        };
                        InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                        var voucherDetailCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            EntityId = voucherVM.EntityId,
                            CrAmount = voucherDetailVM.CrAmount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            PartyId = invoiceWriteOff.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            PartyType = invoiceWriteOff.PartyType,
                            InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.ToCurrencyRate),
                            CrAmount = voucherVM.ToCurrencyRate * voucherDetailCr.CrAmount
                        });

                        totalAmountCr += voucherDetailCr.CrAmount;
                        totalCurrencyAmountCr += voucherVM.ToCurrencyRate * voucherDetailCr.CrAmount;



                    }

                    if (voucherDetailVM.TrnType == "Dr")
                    {
                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var invoiceDetail = new InvoiceDetail
                        {
                            InvoiceId = invoicedata.Id,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = voucherDetailVM.DrAmount,
                            NetAmount = voucherDetailVM.DrAmount,
                            TaxAmount = 0,
                            WrittenOffAmount = 0,
                            AddedBy = invoicedata.AddedBy,
                            AddedDate = invoicedata.AddedDate,
                            AddedFromIP = invoicedata.AddedFromIP,
                            Archive = invoicedata.Archive
                        };
                        _invoiceService.InsertInvoiceDetail(invoicedata, invoiceDetail, 1);

                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.DrAmount,
                            EntityId = voucherVM.EntityId,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            PartyType = voucherDetailVM.PartyType,
                            InvoiceDetailId = invoiceDetail.Id
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        totalAmountDr += voucherDetailDr.DrAmount;
                        var voucherDetailCurrencyTax = new VoucherDetailCurrency
                        {
                            ToCurrencyRate = voucherVM.ToCurrencyRate,
                            ToCurrencyId = companyCurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            DrAmount = voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount,
                            ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                        };
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrencyTax);
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

        public string InsertVendorPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
                , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<PurchaseLCChargesViewModel> purchaseLCChargesVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList, IEnumerable<VoucherDetailViewModel> glVMList, IEnumerable<VoucherViewModel> existingLoanList)
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
                var adjustmentNote = new AdjustmentNote
                {
                };
                decimal totalbankChargess = 0;
                decimal totalpurchaseLCCharges = 0;
                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    totalbankChargess = bankChargeDetailVMList.Sum(r => r.Amount);
                }
                if (null != purchaseLCChargesVMList && purchaseLCChargesVMList.Count() > 0)
                {
                    totalpurchaseLCCharges = purchaseLCChargesVMList.Sum(r => r.ChargesValue);
                }
                if (voucherVM.PaymentSource == PaymentSource.Discount.ToString())
                {
                    voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                }

                else
                {
                    voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount) + totalbankChargess + totalpurchaseLCCharges;
                }
                // INSERT INTO InvoiceWriteOff
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);
                if (voucherVM.PaymentSource == PaymentSource.Vendor.ToString())
                {
                    voucherVM.AddedBy = voucher.AddedBy;
                    voucherVM.AddedDate = voucher.AddedDate;
                    voucherVM.AddedFromIP = voucher.AddedFromIP;
                    adjustmentNote = InsertAdjustmentNote(voucherVM);
                    adjustmentNote.VoucherId = voucher.Id;
                }
                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;


                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                    if (null == invoiceDetail)
                        throw new CustomException("Invoice not found!");

                    invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                    if (invoiceDetail.NetAmount + invoiceDetail.AdditionalAmount < invoiceDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount + invoiceDetail.AdditionalAmount == invoiceDetail.WrittenOffAmount;
                    invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                    invoice.WrittenOffAmount += voucherDetailVM.Amount;
                    invoice.IsWrittenOff = invoice.Amount + invoice.AdditionalAmount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.Update(invoice);

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
                        InvoiceId = voucherDetailVM.InvoiceId,
                        InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                        Amount = voucherDetailVM.Amount,
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

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherDetailVM.EntityId,
                        DrAmount = voucherDetailVM.Amount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                        DrAmount = Math.Round((voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount), 3, MidpointRounding.AwayFromZero)
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += Math.Round((voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount), 3, MidpointRounding.AwayFromZero);
                    totalAmountCr += voucherDetailDr.CrAmount;
                    totalCurrencyAmountCr += Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount), 3, MidpointRounding.AwayFromZero);

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
                        totalCurrencyAmountDr += voucherDetailVM.ExchangeAmount;
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
                        totalCurrencyAmountDr -= voucherDetailVM.ExchangeAmount;
                    }


                }
                if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
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
                        DrAmount = (companyCurrencyId == voucherVM.CurrencyId) ? voucherVM.ExchangeAmount : 0,
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
                    totalCurrencyAmountCr += voucherVM.ExchangeAmount;
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
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            BankMasterId = invoiceWriteOff.BankMasterId,
                            CashMasterId = invoiceWriteOff.CashMasterId,
                            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                            SourceType = invoiceWriteOff.SourceType,
                            Narration = voucher.Narration,
                            Archive = invoiceWriteOff.Archive,
                            Amount = bankChargeDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP
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

                if (null != purchaseLCChargesVMList && purchaseLCChargesVMList.Count() > 0)
                {
                    foreach (var purchaseLCChargesVM in purchaseLCChargesVMList)
                    {
                        // Insert LC charges Debit
                        currentVoucherDetailId++;
                        var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            DrAmount = purchaseLCChargesVM.ChargesValue,
                            GLGeneralInfoId = purchaseLCChargesVM.ExpensesGLId,
                            BudgetMasterId = purchaseLCChargesVM.ExpensesBudgetMasterId,
                            ActivityId = purchaseLCChargesVM.ExpensesActivityId
                        }, currentVoucherDetailId);
                        totalCharges += purchaseLCChargesVM.ChargesValue;

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailChargeDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = purchaseLCChargesVM.BankAmount
                        });
                        totalAmountDr += voucherDetailChargeDr.DrAmount;
                        totalCurrencyAmountDr += purchaseLCChargesVM.BankAmount;
                    }
                }

                if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                {
                    if (null != tdsVMList && tdsVMList.Count() > 0)
                    {
                        var tdstax = new AdditionalTax
                        {

                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            //TaxAmount = tdsVMList.Sum(r => r.TaxAmount),
                            TaxAmount = voucherDetailVMList.Sum(r => r.Amount),
                            TaxAutoAmount = tdsVMList.Sum(r => r.TaxAutoAmount),
                            InventoryReceiveId = null,
                            InvoiceId = null,
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            EmployeePayableId = null,
                            PartyId = invoiceWriteOff.PartyId,
                            PartyPlantId = invoiceWriteOff.PartyPlantId,
                            Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
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

                            addtionalTaxDetailId++;
                            var tdsDetail = new AdditionalTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                                BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                                ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                                Amount = voucherDetailVMList.Sum(r => r.Amount),
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
                            var invoiceTax = new InvoiceTax
                            {
                                VoucherDetailId = voucherDetailTempId,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = taxCode.TaxCategoryId,
                                TaxAmount = voucherDetailVMList.Sum(r => r.Amount),
                                TaxAutoAmount = 0,
                                VoucherId = voucher.Id
                            };
                            totalAmountCr += invoiceTax.TaxAmount;
                            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = tdsDetail.GLGeneralInfoId,
                                BudgetMasterId = tdsDetail.BudgetMasterId,
                                ActivityId = tdsDetail.ActivityId,
                                Amount = tdsDetail.Amount,
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
                                CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount), 3, MidpointRounding.AwayFromZero),
                                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            totalCurrencyAmountCr += voucherDetailCurrencyTax.CrAmount;
                        }
                    }
                }

                if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                    {
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                        voucherDetailCr.CrAmount = (voucherVM.BankMasterId != null && bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId) ? voucherVM.BankAmount : voucherVM.Amount;

                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        if (string.IsNullOrEmpty(bankMaster["ActivityId"].ToString()))
                            throw new CustomException("ActivityId  not Found in Bank Master!");
                        voucherDetailCr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = bankMaster["ActivityId"].ToString();
                        voucherDetailCr.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailCr.PartyType = PartyType.Bank.ToString();
                        if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                        else
                            glTransactionDetail.CrAmount = voucherVM.BankAmount;

                        //if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                        //{
                        //    voucherVM.BankBookAmount = Math.Round((voucherVM.BankBookAmount * voucherVM.CompanyCurrencyRate),3, MidpointRounding.AwayFromZero);

                        //}

                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate)
                        };
                        if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                        {


                            voucherDetailCurrencyCr.CrAmount = voucherVM.BankBookAmount;
                            if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                                voucherDetailCurrencyCr.CrAmount -= Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);

                            if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                                voucherDetailCurrencyCr.CrAmount += Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);
                        }

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                        totalCurrencyAmountCr += voucherDetailCurrencyCr.CrAmount;
                    }
                    else
                        throw new CustomException("Bank  Id not found!");
                }

                if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                    {
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };

                        var cashMaster = _accountsCommonService.GetCashMaster(voucherVM.CashMasterId);
                        voucherDetailCr.CrAmount = voucherVM.Amount;


                        // INSERT INTO VoucherDetail (Bank or cash side Dr)

                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                        {
                            voucherDetailCr.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                            voucherDetailCr.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                            voucherDetailCr.ActivityId = cashMaster["ActivityId"].ToString();
                            voucherDetailCr.CashMasterId = cashMaster["Id"].ToString();
                            voucherDetailCr.PartyType = PartyType.Cash.ToString();
                            if (cashMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount), 3, MidpointRounding.AwayFromZero);
                        }

                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate)
                        };
                        if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                        {


                            voucherDetailCurrencyCr.CrAmount = voucherVM.BankBookAmount;
                            if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                                voucherDetailCurrencyCr.CrAmount -= Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);

                            if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                                voucherDetailCurrencyCr.CrAmount += Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);

                        }
                        if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                        {
                            if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                                voucherDetailCurrencyCr.CrAmount = voucherVM.Amount - Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);
                            else if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                                voucherDetailCurrencyCr.CrAmount = voucherVM.Amount + Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);
                            else if (voucherVM.ExchangeType == "ExchangeLoss")
                                voucherDetailCurrencyCr.CrAmount = voucherVM.Amount;
                            else if (voucherVM.ExchangeType == "ExchangeGain")
                                voucherDetailCurrencyCr.CrAmount = voucherVM.Amount - voucherVM.ExchangeAmount;

                            else
                                voucherDetailCurrencyCr.CrAmount = voucherVM.Amount;
                        }

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                        totalCurrencyAmountCr += voucherDetailCurrencyCr.CrAmount;
                    }
                    else
                        throw new CustomException("Cash Id not found!");
                }

                if (voucherVM.PaymentSource == PaymentSource.Vendor.ToString())
                {
                    var gl = GetCreditNoteGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                    var adjustmentNoteDetail = new AdjustmentNoteDetail
                    {
                        GLGeneralInfoId = gl.LiabilityGLId,
                        BudgetMasterId = gl.LiabilityBudgetMasterId,
                        ActivityId = gl.LiabilityActivityId,
                    };


                    // INSERT INTO VoucherDetail (Bank or cash side Dr)
                    var voucherDetailCr = new VoucherDetail
                    {
                        PartyId = voucherVM.OtherPartyId,
                        PartyType = "Vendor",
                        PartyPlantId = voucherVM.OtherPartyPlantId,
                        Narration = voucher.Narration,
                        PaymentSource = invoiceWriteOff.PaymentSource,
                        GLGeneralInfoId = adjustmentNoteDetail.GLGeneralInfoId,
                        BudgetMasterId = adjustmentNoteDetail.BudgetMasterId,
                        ActivityId = adjustmentNoteDetail.ActivityId
                    };

                    voucherDetailCr.CrAmount = voucherVM.Amount;
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;

                    adjustmentNote.Amount = voucherDetailCr.CrAmount;
                    adjustmentNote.Id = adjustmentNote.Id;
                    adjustmentNote.InvoiceId = adjustmentNote.InvoiceId;
                    adjustmentNote.AddedBy = adjustmentNote.AddedBy;
                    adjustmentNote.AddedDate = adjustmentNote.AddedDate;
                    adjustmentNote.AddedFromIP = adjustmentNote.AddedFromIP;
                    adjustmentNote.Archive = adjustmentNote.Archive;

                    InsertAdjustmentNoteDetail(adjustmentNote, adjustmentNoteDetail, 1);

                    voucherDetailCr.AdjustmentNoteDetailId = adjustmentNoteDetail.Id;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                    //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyCr = new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate)
                    };
                    voucherDetailCurrencyCr.CrAmount = Math.Round((voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailCurrencyCr.CrAmount -= Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);

                    if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailCurrencyCr.CrAmount += Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                    totalCurrencyAmountCr += voucherDetailCurrencyCr.CrAmount;
                }
                if (voucherVM.PaymentSource == PaymentSource.GL.ToString())
                {
                    if (null != glVMList && glVMList.Count() > 0)
                    {
                        foreach (var glVM in glVMList)
                        {
                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = glVM.GLGeneralInfoId,
                                BudgetMasterId = glVM.BudgetMasterId,
                                ActivityId = glVM.ActivityId,
                                InvoiceTaxDetailId = glVM.Id,
                                CrAmount = glVM.Amount,
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);
                            totalAmountCr += voucherDetailTax.CrAmount;
                            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount), 3, MidpointRounding.AwayFromZero),
                                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            totalCurrencyAmountCr += voucherDetailCurrencyTax.CrAmount;
                        }
                    }
                }


                if (voucherVM.PaymentSource == PaymentSource.Discount.ToString())
                {
                    // INSERT INTO VoucherDetail (Bank or cash side Dr)
                    var voucherDetailCr = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        CrAmount = voucherDetailVMList.Sum(r => r.Amount) + totalCharges,
                        PaymentSource = invoiceWriteOff.PaymentSource
                    };
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                    if (voucherVM.ExchangeType == "ExchangeLoss")
                        voucherDetailCr.CrAmount += voucherVM.ExchangeAmount;
                    if (voucherVM.ExchangeType == "ExchangeGain")
                        voucherDetailCr.CrAmount -= voucherVM.ExchangeAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;

                    var financeType = _financingTypeRepository.Query(r => r.SourceType == FinancingTypeEnum.PurchaseDiscount.ToString()).Select().FirstOrDefault();
                    if (financeType != null)
                    {
                        var financingTypeGL = _financingTypeGLService.Query(r => r.FinancingTypeId == financeType.Id).Select().FirstOrDefault();
                        if (financingTypeGL == null)
                            throw new CustomException("There is no Purchase Discount GL!");
                        voucherDetailCr.GLGeneralInfoId = financingTypeGL.ExpensesGLId;
                        voucherDetailCr.BudgetMasterId = financingTypeGL.ExpensesBudgetMasterId;
                        voucherDetailCr.ActivityId = financingTypeGL.ExpensesActivityId;

                    }
                    else
                        throw new CustomException("There is no Purchase Discount Type!");
                    // INSRT INTO GLTransactionDetail

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                    //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyCr = new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate)
                    };
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr - Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);
                    else if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr + Math.Round((voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate), 3, MidpointRounding.AwayFromZero);
                    else if (voucherVM.ExchangeType == "ExchangeLoss")
                        voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr;
                    else if (voucherVM.ExchangeType == "ExchangeGain")
                        voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr - voucherVM.ExchangeAmount;

                    else
                        voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr;

                    totalCurrencyAmountCr = voucherDetailCurrencyCr.CrAmount;
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                }
                if (voucherVM.PaymentSource == PaymentSource.Loan.ToString())
                {
                    if (existingLoanList != null)
                    {
                        if ((voucherVM.Amount != existingLoanList.Sum(r => r.LoanSetOffAmount)) && voucherVM.CurrencyId == existingLoanList.FirstOrDefault().CurrencyId)
                        {
                            throw new CustomException("Dr Cr Amount Not Match!");
                        }
                        if ((voucherVM.Amount != Math.Round((existingLoanList.Sum(r => r.LoanSetOffAmount / voucherVM.CompanyCurrencyRate)), 2)) && voucherVM.CurrencyId != existingLoanList.FirstOrDefault().CurrencyId)
                        {
                            throw new CustomException("Dr Cr Amount Not Match!");
                        }

                        //var currentDetailId = 0;
                        foreach (var item in existingLoanList)
                        {
                            var FinancingSubsequentPayment = new FinancingSubsequentTransaction
                            {
                                CompanyGroupId = voucherVM.CompanyGroupId,
                                CompanyId = voucherVM.CompanyId,
                                PlantId = voucherVM.PlantId,
                                EntityId = voucherVM.EntityId,
                                VoucherTypeId = voucherVM.VoucherTypeId,
                                FinancingId = item.FinancingId,
                                SetOffFinancingId = null,
                                PartyId = item.PartyId,
                                PartyPlantId = item.PartyPlantId,
                                PartyType = item.PartyType,
                                CurrencyId = item.CurrencyId,
                                Amount = item.LoanSetOffAmount,
                                VoucherDate = voucherVM.VoucherDate,
                                PostingDate = voucherVM.PostingDate,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                TransactionType = LoanTransactionType.AdditionalLoanPayable.ToString(),
                                Narration = voucherVM.Narration,
                                SourceType = LoanTransactionType.AdditionalLoanPayable.ToString(),
                                IsPark = voucherVM.IsPark,
                                Id = "SL" + GetLoanInterestPayablePK(),
                                VoucherId = voucher.Id
                            };
                            AuditService.AddedLog(FinancingSubsequentPayment);
                            _loanInterestPayableRepository.Insert(FinancingSubsequentPayment);

                            var financing = _financingService.FindFinancing(item.FinancingId);
                            var financingDetail = _financingService.FindFinancingDetail(item.FinancingDetailId);

                            var ExistingLoanSetoffAmount = 0.0M;
                            var ExistingLoanSetoffCurrencyAmount = 0.0M;
                            if (voucherVM.CurrencyId != existingLoanList.FirstOrDefault().CurrencyId)
                            {
                                ExistingLoanSetoffAmount = Math.Round((voucherVM.Amount / voucherVM.CompanyCurrencyRate), 2);
                                ExistingLoanSetoffCurrencyAmount = item.LoanSetOffAmount;
                            }
                            else
                            {
                                ExistingLoanSetoffAmount = item.LoanSetOffAmount;
                                ExistingLoanSetoffCurrencyAmount = Math.Round((item.ToCurrencyRate * item.LoanSetOffAmount), 2);
                            }
                            var voucherDetailCr = new VoucherDetail
                            {
                                Narration = voucher.Narration,
                                CurrencyId = item.CurrencyId,
                                PaymentSource = invoiceWriteOff.PaymentSource,
                                PartyType = voucherVM.PartyType,
                                GLGeneralInfoId = financingDetail.GLGeneralInfoId,
                                BudgetMasterId = financingDetail.BudgetMasterId,
                                ActivityId = financingDetail.ActivityId,
                                CrAmount = ExistingLoanSetoffAmount
                            };

                            totalAmountCr += voucherDetailCr.CrAmount;
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                            // INSERT INTO VoucherDetailCurrency
                            var voucherDetailCurrencyCr = new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = item.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = item.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(item.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = ExistingLoanSetoffCurrencyAmount
                            };

                            totalCurrencyAmountCr = voucherDetailCurrencyCr.CrAmount;
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);

                            financing.AdditionalLoanAmount += voucherDetailCr.CrAmount;
                            financingDetail.AdditionalLoanAmount += voucherDetailCr.CrAmount;
                            _financingService.UpdateFinancing(financing);
                            _financingService.UpdateFinancingDetail(financingDetail);
                        }

                    }
                }


                if (!string.IsNullOrEmpty(invoiceWriteOff.RoundingType))
                {
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString() || invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(invoiceWriteOff.CompanyId);
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountDr += voucherDetailRoundingDr.DrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.DrAmount), 3, MidpointRounding.AwayFromZero)
                            });
                            totalCurrencyAmountDr += voucherDetailCurrencyRoundingDr.DrAmount;
                        }
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountCr += voucherDetailRoundingDr.CrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.CrAmount), 3, MidpointRounding.AwayFromZero)
                            });
                            totalCurrencyAmountCr += voucherDetailCurrencyRoundingDr.CrAmount;
                        }
                    }
                }

                totalAmountCr += taxDrAmount;
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
        //private AdvanceWriteOff InsertAdvanceWriteOff(AdvanceWriteOff advanceWriteOff)
        //{
        //    advanceWriteOff.Id = GetAutoNumber(nameof(AdvanceWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
        //    AuditService.AddedLog(advanceWriteOff);
        //    _advanceWriteOffRepository.Insert(advanceWriteOff);
        //    return advanceWriteOff;
        //}
        //private AdvanceWriteOff InsertAdvanceWriteOff(VoucherViewModel voucherVM)
        //{
        //    return InsertAdvanceWriteOff(new AdvanceWriteOff
        //    {
        //        CompanyGroupId = voucherVM.CompanyGroupId,
        //        CompanyId = voucherVM.CompanyId,
        //        PlantId = voucherVM.PlantId,
        //        EntityId = voucherVM.EntityId,
        //        FiscalYearId = voucherVM.FiscalYearId,
        //        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
        //        TaxYearId = voucherVM.TaxYearId,
        //        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
        //        VoucherTypeId = voucherVM.VoucherTypeId,
        //        CurrencyId = voucherVM.CurrencyId,
        //        PartyType = voucherVM.PartyType,
        //        PartyId = voucherVM.PartyId,
        //        PartyPlantId = voucherVM.PartyPlantId,
        //        EmployeeId = voucherVM.EmployeeId,
        //        Amount = voucherVM.Amount,
        //        VoucherDate = voucherVM.VoucherDate,
        //        PostingDate = voucherVM.PostingDate,
        //        DocDate = voucherVM.DocDate,
        //        DocRefNo = voucherVM.DocRefNo,
        //        Narration = voucherVM.Narration,
        //        SourceType = voucherVM.SourceType,
        //        IsPark = voucherVM.IsPark,
        //        SettlementType = voucherVM.SettlementType,
        //        PaymentSource = voucherVM.PaymentSource,
        //        BankMasterId = voucherVM.BankMasterId,
        //        CashMasterId = voucherVM.CashMasterId,
        //        Archive = false
        //    });
        //}
        //private void InsertAdvanceWriteOffDetail(AdvanceWriteOff advanceWriteOff, AdvanceWriteOffDetail advanceWriteOffDetail, int currentId)
        //{
        //    advanceWriteOffDetail.Id = MakePK(advanceWriteOff.Id, currentId, 2);
        //    advanceWriteOffDetail.AddedBy = advanceWriteOff.AddedBy;
        //    advanceWriteOffDetail.AddedDate = advanceWriteOff.AddedDate;
        //    advanceWriteOffDetail.AddedFromIP = advanceWriteOff.AddedFromIP;
        //    advanceWriteOffDetail.AdvanceWriteOffId = advanceWriteOff.Id;
        //    advanceWriteOffDetail.Archive = advanceWriteOff.Archive;
        //    _advanceWriteOffDetailRepository.Insert(advanceWriteOffDetail);
        //}
        //private string GetEmployeeSubsequentTransactionPK()
        //{
        //    return _pkGeneratorService.GetAutoNumber("EmployeeSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        //}

        public string InsertInvoiceToAcceptancePost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
               , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList, IEnumerable<VoucherDetailViewModel> glVMList)
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

                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);

                // INSERT INTO InvoiceWriteOff
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);
                if (voucherVM.PaymentSource == PaymentSource.Vendor.ToString())
                {
                    voucherVM.AddedBy = voucher.AddedBy;
                    voucherVM.AddedDate = voucher.AddedDate;
                    voucherVM.AddedFromIP = voucher.AddedFromIP;
                }
                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                decimal taxDrAmount = 0;


                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                    if (null == invoiceDetail)
                        throw new CustomException("Invoice not found!");

                    invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                    if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                    invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                    invoice.WrittenOffAmount += voucherDetailVM.Amount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.Update(invoice);

                    voucherVM.PurchaseLCId = invoice.PurchaseLCId;
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
                        InvoiceId = voucherDetailVM.InvoiceId,
                        InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                        Amount = voucherDetailVM.Amount,
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

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherDetailVM.EntityId,
                        DrAmount = voucherDetailVM.Amount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount;

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
                        totalCurrencyAmountDr += voucherDetailVM.ExchangeAmount;
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
                        totalCurrencyAmountDr -= voucherDetailVM.ExchangeAmount;
                    }


                }
                if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
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
                    totalCurrencyAmountCr += voucherVM.ExchangeAmount;
                }


                //if (voucherVM.PaymentSource == PaymentSource.GL.ToString())
                //{
                //    if (null != glVMList && glVMList.Count() > 0)
                //    {
                //        foreach (var glVM in glVMList)
                //        {
                //            var voucherDetailTax = new VoucherDetail
                //            {
                //                GLGeneralInfoId = glVM.GLGeneralInfoId,
                //                BudgetMasterId = glVM.BudgetMasterId,
                //                ActivityId = glVM.ActivityId,
                //                InvoiceTaxDetailId = glVM.Id,
                //                CrAmount = glVM.Amount,
                //            };
                //            currentVoucherDetailId++;
                //            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);
                //            totalAmountCr += voucherDetailTax.CrAmount;
                //            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                //            {
                //                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                //                ToCurrencyId = companyCurrencyId,
                //                ParallelCurrencyId = companyCurrencyId,
                //                FromCurrencyId = companyCurrencyId,
                //                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                //                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                //            };
                //            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                //            totalCurrencyAmountCr += voucherDetailCurrencyTax.CrAmount;
                //        }
                //    }
                //}

                var invoiceNew = _invoiceService.InsertInvoice(voucherVM);
                invoiceNew.VoucherId = voucher.Id;
                var partyType = PartyType.Vendor.ToString();
                var companyParty = _accountsCommonService.GetCompanyParty(invoiceNew.CompanyId, invoiceNew.PlantId, invoiceNew.PartyId, partyType);

                //var companyPartyGLList = _accountsCommonService.GetCompanyPartyGL(companyParty["PartyId"].ToString(), companyParty["Id"].ToString(), PartyGLType.ReconciliationGL.ToString());

                // INSERT INTO InvoiceDetail
                var invoiceDetailNew = new InvoiceDetail
                {
                    GLGeneralInfoId = glVMList.FirstOrDefault().GLGeneralInfoId,
                    BudgetMasterId = glVMList.FirstOrDefault().BudgetMasterId,
                    ActivityId = glVMList.FirstOrDefault().ActivityId,
                    Amount = voucherVM.Amount,
                    NetAmount = voucherVM.Amount,
                    TaxAmount = 0
                };

                _invoiceService.InsertInvoiceDetail(invoiceNew, invoiceDetailNew, 1);
                invoiceNew.Amount = invoiceDetailNew.Amount;

                if (voucherVM.PaymentSource == PaymentSource.GL.ToString())
                {
                    if (null != glVMList && glVMList.Count() > 0)
                    {
                        foreach (var glVM in glVMList)
                        {
                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = glVM.GLGeneralInfoId,
                                BudgetMasterId = glVM.BudgetMasterId,
                                ActivityId = glVM.ActivityId,
                                InvoiceTaxDetailId = glVM.Id,
                                CrAmount = glVM.Amount,
                                PartyId = invoiceWriteOff.PartyId,
                                PartyPlantId = voucherDetailVMList.FirstOrDefault().PartyPlantId,
                                PartyType = invoiceWriteOff.PartyType,
                                InvoiceDetailId = invoiceDetailNew.Id
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);
                            totalAmountCr += voucherDetailTax.CrAmount;
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
                            totalCurrencyAmountCr += voucherDetailCurrencyTax.CrAmount;
                        }
                    }
                }


                if (!string.IsNullOrEmpty(invoiceWriteOff.RoundingType))
                {
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString() || invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(invoiceWriteOff.CompanyId);
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountDr += voucherDetailRoundingDr.DrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.DrAmount
                            });
                            totalCurrencyAmountDr += voucherDetailCurrencyRoundingDr.DrAmount;
                        }
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountCr += voucherDetailRoundingDr.CrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.CrAmount
                            });
                            totalCurrencyAmountCr += voucherDetailCurrencyRoundingDr.CrAmount;
                        }
                    }
                }
                //totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
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


        public string PostMultipleVendorPayment(VoucherViewModel voucherVM, IEnumerable<MultiplePaymentViewModel> mpSummarylist, IEnumerable<MultiplePaymentDetailViewModel> multiplePaymentDetailList
                , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var flag = false;
            try
            {

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                decimal taxDrAmount = 0;

                foreach (var mulpay in mpSummarylist)
                {
                    if (mulpay.IsPark == false)
                        throw new CustomException("Post is not allowed!");
                    voucherVM.PostingDate = mulpay.TentativeDate;
                    voucherVM.DocDate = mulpay.TentativeDate;
                    voucherVM.PartyType = "Vendor";
                    _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                    _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                    //if (voucherVM.PaymentSource == PaymentSource.Discount.ToString())
                    voucherVM.Amount = multiplePaymentDetailList.Where(r => r.PartyId == mulpay.PartyId && r.PartyPlantId == mulpay.PartyPlantId).Sum(r => r.Amount);
                    // INSERT INTO InvoiceWriteOff
                    voucherVM.BankMasterId = mulpay.BankMasterId;
                    voucherVM.PartyId = mulpay.PartyId;
                    voucherVM.PartyPlantId = mulpay.PartyPlantId;
                    var invoiceWriteOff = InsertMultipleVendorInvoiceWriteOff(voucherVM);

                    // INSERT INTO Voucher
                    var voucher = _voucherService.InsertVoucher(voucherVM);

                    // Set Voucher Id to Advance
                    invoiceWriteOff.VoucherId = voucher.Id;

                    var currentVoucherDetailId = 0;
                    var currentInvoiceWriteOffDetailId = 0;


                    string voucherDetailTempId = null;
                    var withholdgl = false;


                    var invoiceIds = multiplePaymentDetailList.Where(r => r.PartyId == mulpay.PartyId && r.PartyPlantId == mulpay.PartyPlantId).Select(r => r.InvoiceId);
                    var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = multiplePaymentDetailList.Where(r => r.PartyId == mulpay.PartyId && r.PartyPlantId == mulpay.PartyPlantId).Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);

                    foreach (var multiplePaymentDetail in multiplePaymentDetailList)
                    {

                        if (multiplePaymentDetail.PartyId == mulpay.PartyId && multiplePaymentDetail.PartyPlantId == mulpay.PartyPlantId)
                        {
                            var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == multiplePaymentDetail.InvoiceDetailId);
                            if (null == invoiceDetail)
                                throw new CustomException("Invoice not found!");

                            invoiceDetail.WrittenOffAmount += multiplePaymentDetail.Amount;

                            if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                            invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                            // TODO: have a gap here if invoice split
                            var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                            invoice.WrittenOffAmount += multiplePaymentDetail.Amount;
                            invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                            invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            _invoiceService.Update(invoice);

                            var multiplePayDetail = _multiplePaymentDetailRepository.Query(r => r.InvoiceDetailId == multiplePaymentDetail.InvoiceDetailId).Select().FirstOrDefault();
                            multiplePayDetail.IsPark = false;
                            _multiplePaymentDetailRepository.Update(multiplePayDetail);

                            // INSERT INTO InvoiceDetail
                            currentInvoiceWriteOffDetailId++;
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                PartyId = invoiceWriteOff.PartyId,
                                PartyPlantId = invoice.PartyPlantId,
                                ActivityId = invoiceDetail.ActivityId,
                                CurrencyId = invoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                InvoiceId = multiplePaymentDetail.InvoiceId,
                                InvoiceDetailId = multiplePaymentDetail.InvoiceDetailId,
                                Amount = multiplePaymentDetail.Amount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP,
                                Archive = invoiceWriteOff.Archive,
                                ModelState = invoiceWriteOff.ModelState,
                                DocDate = invoice.DocDate,
                                DocRefNo = invoice.DocRefNo,
                                Narration = invoice.Narration,
                                MultiplePaymentDetailId = multiplePaymentDetail.Id
                            };
                            InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                            var voucherDetailDr = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceDetail.BudgetMasterId,
                                ActivityId = invoiceDetail.ActivityId,
                                EntityId = invoice.EntityId,
                                DrAmount = multiplePaymentDetail.Amount,
                                DocDate = invoice.DocDate,
                                DocRefNo = invoice.DocRefNo,
                                Narration = invoice.Narration,
                                PartyId = invoiceWriteOff.PartyId,
                                PartyPlantId = invoice.PartyPlantId,
                                PartyType = invoiceWriteOff.PartyType,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                            // INSERT INTO VoucherDetailCurrency
                            var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, multiplePaymentDetail.CompanyCurrencyRate),
                                DrAmount = multiplePaymentDetail.CompanyCurrencyRate * voucherDetailDr.DrAmount
                            });

                            totalAmountDr += voucherDetailDr.DrAmount;
                            totalCurrencyAmountDr += multiplePaymentDetail.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                            totalAmountCr += voucherDetailDr.CrAmount;
                            totalCurrencyAmountCr += multiplePaymentDetail.CompanyCurrencyRate * voucherDetailDr.CrAmount;

                            if (multiplePaymentDetail.ExchangeType == "ExchangeLoss" && multiplePaymentDetail.ExchangeAmount > 0)
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
                                    PartyType = multiplePaymentDetail.ExchangeType
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
                                    DrAmount = multiplePaymentDetail.ExchangeAmount
                                });
                                totalCurrencyAmountDr += multiplePaymentDetail.ExchangeAmount;
                            }

                            if (multiplePaymentDetail.ExchangeType == "ExchangeGain" && multiplePaymentDetail.ExchangeAmount > 0)
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
                                    PartyType = multiplePaymentDetail.ExchangeType
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
                                    CrAmount = multiplePaymentDetail.ExchangeAmount
                                });
                                totalCurrencyAmountDr -= multiplePaymentDetail.ExchangeAmount;
                            }

                            if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
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
                                    DrAmount = voucherVM.ExchangeAmount,
                                    PartyType = multiplePaymentDetail.ExchangeType
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
                                    CrAmount = voucherVM.ExchangeAmount,
                                    PartyType = multiplePaymentDetail.ExchangeType
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
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                BankMasterId = invoiceWriteOff.BankMasterId,
                                CashMasterId = invoiceWriteOff.CashMasterId,
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                SourceType = invoiceWriteOff.SourceType,
                                Narration = voucher.Narration,
                                Archive = invoiceWriteOff.Archive,
                                Amount = bankChargeDetailVM.Amount,
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP
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

                    if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                    {
                        if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                        {
                            var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                            foreach (var invoiceTaxVM in taxDetailVMList)
                            {
                                var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                                if (null == taxCode)
                                    throw new CustomException("Tax code not found!");

                                var taxCodeGL = _accountsCommonService.GetTaxCodeGL(taxCode.Id);
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
                                totalAmountCr += invoiceTaxVM.TaxAmount;
                                _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                                // Insert Into Customer Invoice Tax Detail (Withhold GL)
                                withholdgl = taxCode.IsWithhold;
                                if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL["WithholdCreditableGLId"].ToString()))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                                        BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                                        ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
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
                                        CrAmount = totalCurrencyAmountDr,/*voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,*/
                                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                    };
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                                }
                            }
                        }
                    }

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        // INSERT INTO VoucherDetail (Bank or cash side Dr)
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = multiplePaymentDetailList.Where(r => r.PartyId == mulpay.PartyId && r.PartyPlantId == mulpay.PartyPlantId).Sum(r => r.Amount) + totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                        if (voucherVM.ExchangeType == "ExchangeLoss")
                            voucherDetailCr.CrAmount += voucherVM.ExchangeAmount;
                        if (voucherVM.ExchangeType == "ExchangeGain")
                            voucherDetailCr.CrAmount -= voucherVM.ExchangeAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = mulpay.BankMasterId,
                        };

                        if (!string.IsNullOrEmpty(mulpay.BankMasterId))
                        {
                            var bankMaster = _accountsCommonService.GetBankMaster(mulpay.BankMasterId);
                            voucherDetailCr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                            voucherDetailCr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                            voucherDetailCr.ActivityId = bankMaster["ActivityId"].ToString();
                            voucherDetailCr.BankMasterId = bankMaster["Id"].ToString();
                            voucherDetailCr.PartyType = PartyType.Bank.ToString();
                            if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }

                        else
                            throw new CustomException("Bank  Id not found!");
                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate)
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr - (voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate);
                        else if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr + (voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate);
                        else if (voucherVM.ExchangeType == "ExchangeLoss")
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr;
                        else if (voucherVM.ExchangeType == "ExchangeGain")
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr - voucherVM.ExchangeAmount;

                        else
                            voucherDetailCurrencyCr.CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate;

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                    }

                    if (voucherVM.PaymentSource == PaymentSource.Discount.ToString())
                    {
                        // INSERT INTO VoucherDetail (Bank or cash side Dr)
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = multiplePaymentDetailList.Where(r => r.MultiplePaymentId == mulpay.Id).Sum(r => r.Amount) + totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                        if (voucherVM.ExchangeType == "ExchangeLoss")
                            voucherDetailCr.CrAmount += voucherVM.ExchangeAmount;
                        if (voucherVM.ExchangeType == "ExchangeGain")
                            voucherDetailCr.CrAmount -= voucherVM.ExchangeAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var financeType = _financingTypeRepository.Query(r => r.SourceType == FinancingTypeEnum.PurchaseDiscount.ToString()).Select().FirstOrDefault();
                        if (financeType != null)
                        {
                            var financingTypeGL = _financingTypeGLService.Query(r => r.FinancingTypeId == financeType.Id).Select().FirstOrDefault();
                            if (financingTypeGL == null)
                                throw new CustomException("There is no Purchase Discount GL!");
                            voucherDetailCr.GLGeneralInfoId = financingTypeGL.ExpensesGLId;
                            voucherDetailCr.BudgetMasterId = financingTypeGL.ExpensesBudgetMasterId;
                            voucherDetailCr.ActivityId = financingTypeGL.ExpensesActivityId;

                        }
                        else
                            throw new CustomException("There is no Purchase Discount Type!");
                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                        //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate)
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr - (voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate);
                        else if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr + (voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate);
                        else if (voucherVM.ExchangeType == "ExchangeLoss")
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr;
                        else if (voucherVM.ExchangeType == "ExchangeGain")
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr - voucherVM.ExchangeAmount;

                        else
                            voucherDetailCurrencyCr.CrAmount = totalCurrencyAmountDr;

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                    }

                    if (!string.IsNullOrEmpty(invoiceWriteOff.RoundingType))
                    {
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString() || invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var gl = _financingTypeGLService.GetRoundingGL(invoiceWriteOff.CompanyId);
                            if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            {
                                var voucherDetailRoundingDr = new VoucherDetail
                                {
                                    GLGeneralInfoId = gl.ExpensesGLId,
                                    BudgetMasterId = gl.ExpensesBudgetMasterId,
                                    ActivityId = gl.ExpensesActivityId,
                                    EntityId = voucher.EntityId,
                                    DrAmount = invoiceWriteOff.RoundingAmount,
                                    DocDate = invoiceWriteOff.DocDate,
                                    DocRefNo = invoiceWriteOff.DocRefNo,
                                    Narration = invoiceWriteOff.Narration,
                                    PartyType = invoiceWriteOff.PartyType
                                };
                                currentVoucherDetailId++;
                                totalAmountDr += voucherDetailRoundingDr.DrAmount;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                                var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.DrAmount
                                });
                            }
                            if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            {
                                var voucherDetailRoundingDr = new VoucherDetail
                                {
                                    GLGeneralInfoId = gl.ExpensesGLId,
                                    BudgetMasterId = gl.ExpensesBudgetMasterId,
                                    ActivityId = gl.ExpensesActivityId,
                                    EntityId = voucher.EntityId,
                                    CrAmount = invoiceWriteOff.RoundingAmount,
                                    DocDate = invoiceWriteOff.DocDate,
                                    DocRefNo = invoiceWriteOff.DocRefNo,
                                    Narration = invoiceWriteOff.Narration,
                                    PartyType = invoiceWriteOff.PartyType
                                };
                                currentVoucherDetailId++;
                                totalAmountCr += voucherDetailRoundingDr.CrAmount;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                                var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                    CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.CrAmount
                                });
                            }
                        }
                    }

                    var multiplePay = _multiplePaymentRepository.Query(r => r.Id == voucherVM.Id).Select().FirstOrDefault();
                    multiplePay.IsPark = false;
                    _multiplePaymentRepository.Update(multiplePay);
                }

                totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                //if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                //    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return "Save Success";
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

        public string DeleteMultipleVendorRow(IEnumerable<MultiplePayment> multiplePaymentlist, IEnumerable<MultiplePaymentDetail> multiplePaymentDetailList)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                foreach (var item in multiplePaymentDetailList)
                {
                    _multiplePaymentDetailRepository.Delete(item);
                }
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


        public string InsertAdditionalTaxPayable(VoucherViewModel voucherVM, string additionalTaxId)
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
                var additionalTax = _additionalTaxRepository.Find(additionalTaxId);
                if (additionalTax.VoucherId != null)
                    throw new CustomException("Additional Tax already posted.");
                var voucherDetailVMList = _invoiceService.QueryInvoiceDetail(additionalTax.InvoiceId).Select().ToList();
                voucherVM.PartyId = additionalTax.PartyId;
                voucherVM.PartyPlantId = additionalTax.PartyPlantId;
                voucherVM.IsPark = false;
                voucherVM.Amount = additionalTax.TaxAmount;
                voucherVM.Narration = "TDS Of" + voucherVM.PartyName;
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher

                var voucher = _voucherService.InsertVoucher(voucherVM);
                AuditService.PostedLog(voucher);
                additionalTax.VoucherId = voucher.Id;
                AuditService.UpdatedLog(additionalTax);
                _additionalTaxRepository.Update(additionalTax);
                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                var withholdgl = false;

                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.Id);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.Id);
                    if (null == invoiceDetail)
                        throw new CustomException("Invoice not found!");

                    invoiceDetail.WrittenOffAmount += additionalTax.TaxAmount;

                    if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                    invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                    invoice.WrittenOffAmount += additionalTax.TaxAmount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.Update(invoice);

                    // INSERT INTO InvoiceDetail
                    currentInvoiceWriteOffDetailId++;
                    var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherVM.InvoicingPartyPlantId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherVM.CurrencyId,
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        InvoiceId = voucherDetailVM.InvoiceId,
                        InvoiceDetailId = voucherDetailVM.Id,
                        Amount = additionalTax.TaxAmount,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP,
                        Archive = invoiceWriteOff.Archive,
                        ModelState = invoiceWriteOff.ModelState,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration
                    };
                    InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherVM.EntityId,
                        DrAmount = additionalTax.TaxAmount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.ToCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.ToCurrencyRate),
                        DrAmount = voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += voucherVM.ToCurrencyRate * voucherDetailDr.DrAmount;

                }

                var taxDetailVMList = _additionalTaxDetailRepository.Query(r => r.AdditionalTaxId == additionalTaxId).Select().ToList();
                if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                {
                    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                        foreach (var invoiceTaxVM in taxDetailVMList)
                        {
                            var invoiceTax = new InvoiceTax
                            {
                                VoucherDetailId = voucherDetailTempId,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = invoiceTaxVM.Amount,
                                TaxAutoAmount = 0
                            };
                            totalAmountCr += invoiceTaxVM.Amount;
                            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = invoiceTaxVM.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxVM.BudgetMasterId,
                                ActivityId = invoiceTaxVM.ActivityId,
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
                                ToCurrencyRate = voucherVM.ToCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                CrAmount = voucherVM.ToCurrencyRate * voucherDetailTax.CrAmount,
                                ToCurrencyConversion = 1 / voucherVM.ToCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            totalCurrencyAmountCr += voucherVM.ToCurrencyRate * voucherDetailTax.CrAmount;
                        }
                    }
                }

                totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
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

        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {

            parameters.CmdText = @"SELECT AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode, SUM(IWD.Amount) AS Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark, AW.BankJournalId,IWD.MultiplePaymentNo
                                    ,Status=case when AW.IsPark=1 then 'Parked' else 'Posted' end
                                    FROM [TRN].[InvoiceWriteOff] AS AW
									LEFT JOIN (SELECT WD.Id,WD.InvoiceWriteOffId,MPD.MultiplePaymentId MultiplePaymentNo,SUM(WD.Amount) Amount 
											FROM [TRN].[InvoiceWriteOffDetail] WD 
											LEFT JOIN TRN.Invoice IV ON WD.InvoiceId=IV.Id
											LEFT JOIN TRN.MultiplePaymentDetail MPD ON MPD.InvoiceId=IV.Id
											Group BY WD.Id,WD.InvoiceWriteOffId,IV.Id ,MPD.MultiplePaymentId) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
									LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND V.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.[SourceType]='" + sourceType + @"'
                                    Group BY AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code , P.UserName, AW.PostingDate
									, AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId, IWD.MultiplePaymentNo";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetMultiplePaymentVoucherList(GridParameter parameters, string plantId, SourceType sourceType)
        {

            parameters.CmdText = @"SELECT AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode, SUM(IWD.Amount) AS Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark, AW.BankJournalId,IWD.MultiplePaymentNo--,IWD.MultiplePaymentDetailId
                                    ,Status=case when AW.IsPark=1 then 'Parked' else 'Posted' end
                                    FROM [TRN].[InvoiceWriteOff] AS AW
									 JOIN (SELECT WD.Id,WD.InvoiceWriteOffId,MPD.MultiplePaymentId MultiplePaymentNo,WD.MultiplePaymentDetailId,SUM(WD.Amount) Amount 
											FROM [TRN].[InvoiceWriteOffDetail] WD 
											LEFT JOIN TRN.Invoice IV ON WD.InvoiceId=IV.Id
											LEFT JOIN TRN.MultiplePaymentDetail MPD ON MPD.InvoiceId=IV.Id AND WD.MultiplePaymentDetailId=MPD.Id
											where WD.MultiplePaymentDetailId<>''
											Group BY WD.Id,WD.InvoiceWriteOffId,IV.Id ,MPD.MultiplePaymentId,WD.MultiplePaymentDetailId) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
									LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND V.Archive=0 
									AND AW.PlantId='" + plantId + @"' AND AW.[SourceType]='VendorPayment' AND AW.IsPark=0
                                    Group BY AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code , P.UserName, AW.PostingDate
									, AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId
									, IWD.MultiplePaymentNo--,IWD.MultiplePaymentDetailId
                                    ";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetVendorPaymentParkedNonPostedList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode, SUM(IWD.Amount) AS Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark, AW.BankJournalId,IWD.MultiplePaymentNo
                                    ,Status=case when AW.IsPark=1 then 'Parked' else 'Posted' end,AW.ApprovalStatus,AW.ApproveRemark
                                    FROM [TRN].[InvoiceWriteOff] AS AW
									LEFT JOIN (SELECT WD.Id,WD.InvoiceWriteOffId,MPD.MultiplePaymentId MultiplePaymentNo,SUM(WD.Amount) Amount 
											FROM [TRN].[InvoiceWriteOffDetail] WD 
											LEFT JOIN TRN.Invoice IV ON WD.InvoiceId=IV.Id
											LEFT JOIN TRN.MultiplePaymentDetail MPD ON MPD.InvoiceId=IV.Id
											Group BY WD.Id,WD.InvoiceWriteOffId,IV.Id ,MPD.MultiplePaymentId) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
									LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.[SourceType]='" + sourceType + @"' AND AW.IsPark=1 AND AW.ApprovalStatus!='Approved'
                                    Group BY AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code , P.UserName, AW.PostingDate
									, AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId, IWD.MultiplePaymentNo,AW.ApprovalStatus,AW.ApproveRemark";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetNoteSetOff(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            string wc, wcc = string.Empty;
            if (parameters.searchBy == "Status" && parameters.search.ToUpper() == "POSTED")
            {
                wc = "(case when TAB.IsPark = 1 then 'Parked' else 'Posted' end)";
                wcc = "Posted";

                parameters.searchBy = wc;
                parameters.search = wcc;
            }
            else if (parameters.searchBy == "Status" && parameters.search.ToUpper() == "PARKED")
            {
                wc = "(case when TAB.IsPark = 1 then 'Parked' else 'Posted' end)";
                wcc = "Parked";

                parameters.searchBy = wc;
                parameters.search = wcc;
            }
            else
            {

            }

            parameters.CmdText = @"SELECT AW.InvoiceWriteOffNo, vd.VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode, SUM(iwd.Amount) Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark, AW.BankJournalId
                                    , Status = case when AW.IsPark = 1 then 'Parked' else 'Posted' end
                                    FROM [TRN].[InvoiceWriteOff] AS AW
									LEFT JOIN (SELECT Id,InvoiceWriteOffId,SUM(Amount) Amount,AdjustmentNoteDetailId 
												FROM [TRN].[InvoiceWriteOffDetail] Group BY Id,InvoiceWriteOffId,AdjustmentNoteDetailId ) AS IWD ON IWD.InvoiceWriteOffId=AW.Id and IWD.AdjustmentNoteDetailId<>''
									LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=AW.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND V.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.[SourceType]='" + sourceType + @"'
                                    Group BY AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code , P.UserName, AW.PostingDate
									, AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId";
            return _sqlRepository.GetGridData(parameters);
        }

        public string InsertReceived(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
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
                // INSERT INTO InvoiceWriteOff TABLE
                var invoiceWriteOff = new InvoiceWriteOff
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    PartyId = voucherVM.PartyId,
                    PlantId = voucherVM.PlantId,
                    PartyType = PartyType.Customer.ToString(),
                    Amount = voucherVM.Amount,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    SourceType = SourceType.CustomerReceipt.ToString(),
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    BankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId
                };
                InsertInvoiceWriteOff(invoiceWriteOff);

                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = invoiceWriteOff.CompanyGroupId,
                    CompanyId = invoiceWriteOff.CompanyId,
                    PlantId = invoiceWriteOff.PlantId,
                    CurrencyId = invoiceWriteOff.CurrencyId,
                    FiscalYearId = invoiceWriteOff.FiscalYearId,
                    FiscalYearPeriodId = invoiceWriteOff.FiscalYearPeriodId,
                    TaxYearId = invoiceWriteOff.TaxYearId,
                    TaxYearPeriodId = invoiceWriteOff.TaxYearPeriodId,
                    VoucherTypeId = invoiceWriteOff.VoucherTypeId,
                    AddedBy = invoiceWriteOff.AddedBy,
                    AddedDate = invoiceWriteOff.AddedDate,
                    AddedFromIP = invoiceWriteOff.AddedFromIP,
                    VoucherDate = invoiceWriteOff.VoucherDate,
                    PostingDate = invoiceWriteOff.PostingDate,
                    DocDate = invoiceWriteOff.DocDate,
                    DocRefNo = invoiceWriteOff.DocRefNo,
                    Narration = invoiceWriteOff.Narration,
                    Archive = invoiceWriteOff.Archive,
                    SourceType = invoiceWriteOff.SourceType
                };
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);
                // Set to InvoiceWriteOff
                invoiceWriteOff.VoucherId = voucher.Id;

                decimal BaseDeductcurrAmount = 0;
                decimal BaseDeductAmount = 0;
                decimal BaseBankAmount = 0;
                string TempVoucherDetailId = null;
                string trnCurrencyId = null;
                // INSERT INTO VoucherDetail
                var currentVoucheDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;
                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                    if (null == invoiceDetail)
                        throw new CustomException("Invoice not found!");

                    invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                    if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                        throw new CustomException("Received Amount can not cross Balance Amount");

                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                    invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                    invoice.WrittenOffAmount += invoiceDetail.WrittenOffAmount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.Update(invoice);

                    // INSERT INTO InvoiceDetail
                    currentInvoiceWriteOffDetailId++;
                    var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                    {
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        InvoiceId = voucherDetailVM.InvoiceId,
                        InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        Amount = voucherDetailVM.Amount,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        CompanyId = voucherDetailVM.CompanyId,
                        PlantId = voucherDetailVM.PlantId,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = voucherDetailVM.PartyType
                    };
                    InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                    // in libility side Cr.
                    var voucherCr = new VoucherDetail
                    {
                        VoucherId = voucher.Id,
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        EntityId = voucherDetailVM.EntityId,
                        FiscalYearId = voucher.FiscalYearId,
                        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        DrAmount = 0,
                        CrAmount = voucherDetailVM.Amount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = PartyType.Customer.ToString(),
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                    };
                    BaseBankAmount += voucherDetailVM.Amount;
                    trnCurrencyId = voucherDetailVM.CurrencyId;
                    currentVoucheDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucheDetailId);

                    var voucherDetailCurrency = voucherDetailCurrencyVMList.FirstOrDefault(r => r.TrnType == "Cr" && r.GLGeneralInfoId == voucherCr.GLGeneralInfoId && r.InvoiceDetailId == voucherDetailVM.InvoiceDetailId);
                    if (null != voucherDetailCurrency)
                    {
                        // INSERT INTO VoucherDetailCurrency

                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            if (voucherDetailCurrency.CompanyCurrencyCr <= 0)
                                throw new CustomException($"{voucherDetailCurrency.GLGeneralInfoName} GL {voucherDetailCurrency.CompanyCurrencyName} Cr amount must have to greater than zero!");
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                            {
                                CrAmount = voucherDetailCurrency.CompanyCurrencyCr,
                                FromCurrencyId = voucherVM.CurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrency.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherDetailCurrency.CompanyCurrencyRate
                            });
                        }

                    }
                    TempVoucherDetailId = voucherCr.Id;

                    #region Exchange Loss

                    var voucherDetailCurrencyExchangeLosslist = voucherDetailCurrencyVMList.Where(r => r.TrnType == "Dr" && r.InvoiceDetailId == voucherDetailVM.InvoiceDetailId && r.ExchangeStatus == "ExchangeLoss");
                    foreach (var voucherDetailCurrencyExchangeLoss in voucherDetailCurrencyExchangeLosslist)
                    {
                        if (null != voucherDetailCurrencyExchangeLoss)
                        {
                            var voucherDtEx = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailCurrencyExchangeLoss.GLGeneralInfoId,
                                ActivityId = voucherDetailCurrencyExchangeLoss.ActivityId,
                                BudgetMasterId = voucherDetailCurrencyExchangeLoss.BudgetMasterId,
                                CurrencyId = trnCurrencyId,
                                EntityId = voucherVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                Archive = voucher.Archive,
                                DrAmount = 0,
                                CrAmount = 0,
                                DocDate = voucher.DocDate,
                                DocRefNo = voucherDetailCurrencyExchangeLoss.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                VoucherId = voucher.Id,
                                IsPark = voucher.IsPark
                            };
                            currentVoucheDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucheDetailId);
                            //CompanyCurrency
                            if (voucherDetailCurrencyExchangeLoss.Exchange == "Base")
                            {
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    DrAmount = voucherDetailCurrencyExchangeLoss.CompanyCurrencyDr,
                                    FromCurrencyId = voucherDetailCurrencyExchangeLoss.CompanyFromCurrencyId,
                                    ParallelCurrencyId = voucherDetailCurrencyExchangeLoss.CompanyCurrencyId,
                                    ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeLoss.CompanyCurrencyRate,
                                    ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                    ToCurrencyRate = voucherDetailCurrencyExchangeLoss.CompanyCurrencyRate
                                });
                            }
                            if (voucherDetailCurrencyExchangeLoss.Exchange == "Group")
                            {
                                _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    DrAmount = voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyDr,
                                    FromCurrencyId = voucherDetailCurrencyExchangeLoss.CompanyGroupFromCurrencyId,
                                    ParallelCurrencyId = voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyId,
                                    ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyRate,
                                    ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                    ToCurrencyRate = voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyRate
                                });
                            }
                            if (voucherDetailCurrencyExchangeLoss.Exchange == "Hard")
                            {
                                _voucherService.InsertVoucherDetailHardCurrency(voucherDtEx, new VoucherDetailCurrency
                                {
                                    DrAmount = voucherDetailCurrencyExchangeLoss.HardCurrencyDr,
                                    FromCurrencyId = voucherDetailCurrencyExchangeLoss.HardFromCurrencyId,
                                    ParallelCurrencyId = voucherDetailCurrencyExchangeLoss.HardCurrencyId,
                                    ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeLoss.HardCurrencyRate,
                                    ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                    ToCurrencyRate = voucherDetailCurrencyExchangeLoss.HardCurrencyRate
                                });
                            }
                        }
                    }

                    #endregion Exchange Loss

                    #region Exchange Gain

                    var voucherDetailCurrencyExchangeGainlist = voucherDetailCurrencyVMList.Where(r => r.TrnType == "Cr" && r.InvoiceDetailId == voucherDetailVM.InvoiceDetailId && r.ExchangeStatus == "ExchangeGain");
                    foreach (var voucherDetailCurrencyExchangeGain in voucherDetailCurrencyExchangeGainlist)
                    {
                        if (null != voucherDetailCurrencyExchangeGain)
                        {
                            var voucherDtExGain = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailCurrencyExchangeGain.GLGeneralInfoId,
                                ActivityId = voucherDetailCurrencyExchangeGain.ActivityId,
                                BudgetMasterId = voucherDetailCurrencyExchangeGain.BudgetMasterId,
                                CurrencyId = trnCurrencyId,
                                EntityId = voucherVM.EntityId,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                Archive = voucher.Archive,
                                DrAmount = 0,
                                CrAmount = 0,
                                DocDate = voucher.DocDate,
                                DocRefNo = voucherDetailCurrencyExchangeGain.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                VoucherId = voucher.Id,
                                IsPark = voucher.IsPark
                            };
                            currentVoucheDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, currentVoucheDetailId);
                            //CompanyCurrency
                            if (voucherDetailCurrencyExchangeGain.Exchange == "Base")
                            {
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    CrAmount = voucherDetailCurrencyExchangeGain.CompanyCurrencyCr,
                                    FromCurrencyId = voucherDetailCurrencyExchangeGain.CompanyFromCurrencyId,
                                    ParallelCurrencyId = voucherDetailCurrencyExchangeGain.CompanyCurrencyId,
                                    ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeGain.CompanyCurrencyRate,
                                    ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                    ToCurrencyRate = voucherDetailCurrencyExchangeGain.CompanyCurrencyRate
                                });
                            }
                            if (voucherDetailCurrencyExchangeGain.Exchange == "Group")
                            {
                                _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    CrAmount = voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyCr,
                                    FromCurrencyId = voucherDetailCurrencyExchangeGain.CompanyGroupFromCurrencyId,
                                    ParallelCurrencyId = voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyId,
                                    ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyRate,
                                    ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                    ToCurrencyRate = voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyRate
                                });
                            }
                            if (voucherDetailCurrencyExchangeGain.Exchange == "Hard")
                            {
                                _voucherService.InsertVoucherDetailHardCurrency(voucherDtExGain, new VoucherDetailCurrency
                                {
                                    CrAmount = voucherDetailCurrencyExchangeGain.HardCurrencyCr,
                                    FromCurrencyId = voucherDetailCurrencyExchangeGain.HardFromCurrencyId,
                                    ParallelCurrencyId = voucherDetailCurrencyExchangeGain.HardCurrencyId,
                                    ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeGain.HardCurrencyRate,
                                    ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                    ToCurrencyRate = voucherDetailCurrencyExchangeGain.HardCurrencyRate
                                });
                            }
                        }
                    }

                    #endregion Exchange Gain
                }

                var voucherDetailCurrency1 = voucherDetailCurrencyVMList.FirstOrDefault(r => r.TrnType == "Dr" && r.GLGeneralInfoId == voucherVM.GLGeneralInfoId);
                // _voucherService.CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, voucherDetailCurrency1.CompanyCurrencyDr, voucherDetailCurrency1.CompanyGroupCurrencyDr, voucherDetailCurrency1);

                var voucherDr = new VoucherDetail
                {
                    VoucherId = voucher.Id,
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    ActivityId = voucherVM.ActivityId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    CurrencyId = trnCurrencyId,
                    EntityId = voucherVM.EntityId,
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    AddedBy = voucher.AddedBy,
                    AddedDate = voucher.AddedDate,
                    AddedFromIP = voucher.AddedFromIP,
                    Archive = voucher.Archive,
                    DrAmount = BaseBankAmount - BaseDeductAmount,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = invoiceWriteOff.Narration,
                    BankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    IsPark = voucher.IsPark
                };
                currentVoucheDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucheDetailId);

                if (null != voucherDetailCurrency1)
                {
                    // INSERT INTO voucherDetailCurrency1

                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        if (voucherDetailCurrency1.CompanyCurrencyDr <= 0)
                            throw new CustomException($"{voucherDetailCurrency1.GLGeneralInfoName} GL {voucherDetailCurrency1.CompanyCurrencyName} Dr amount must have to greater than zero!");

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            DrAmount = (voucherDetailCurrency1.CompanyCurrencyDr - BaseDeductcurrAmount),
                            FromCurrencyId = voucherDetailCurrency1.CompanyFromCurrencyId,
                            ParallelCurrencyId = voucherDetailCurrency1.CompanyCurrencyId,
                            ToCurrencyConversion = 1 / voucherDetailCurrency1.CompanyCurrencyRate,
                            ToCurrencyId = voucherDetailCurrency1.ToCurrencyId,
                            ToCurrencyRate = voucherDetailCurrency1.CompanyCurrencyRate
                        });

                        if (companyCurrencyId == voucherVM.BankCurrencyId)
                            voucherVM.BankAmount = voucherDetailCurrency1.CompanyCurrencyDr;
                    }

                }

                if (!string.IsNullOrEmpty(voucherDr.BankMasterId))
                {
                    // INSRT INTO GLTransactionDetail TABLE
                    _voucherService.InsertGLTransactionDetail(voucherDr, new GLTransactionDetail
                    {
                        Id = voucherDr.Id,
                        BankMasterId = voucherDr.BankMasterId,
                        DrAmount = voucherVM.BankAmount - BaseDeductcurrAmount,
                        SourceType = PaymentSource.Bank.ToString(),
                        VoucherDetailId = voucherDr.Id
                    });
                }

                if (!string.IsNullOrEmpty(voucherDr.CashMasterId))
                {
                    // INSRT INTO GLTransactionDetail TABLE
                    _voucherService.InsertGLTransactionDetail(voucherDr, new GLTransactionDetail
                    {
                        Id = voucherDr.Id,
                        AddedBy = voucherDr.AddedBy,
                        AddedDate = voucherDr.AddedDate,
                        AddedFromIP = voucherDr.AddedFromIP,
                        CashMasterId = voucherDr.CashMasterId,
                        DrAmount = voucherVM.BankAmount - BaseDeductcurrAmount,
                        SourceType = SourceType.CashJournal.ToString(),
                        VoucherDetailId = voucherDr.Id
                    });
                }
                base.InsertGraph(invoiceWriteOff);
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

        public void Post(string invoiceWriteOffId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                ConnectionManager.DAL.ConManager objCon1;
                DataSet dsMaster1 = null;
                string setOffsql = @"select AWD.InvoiceWriteOffId,A.IsPark,V.IsPark,AW.IsPark,V.AddedDate ,VW.VoucherNo,VW.AddedDate ,A.SourceType,A.DocRefNo,V.VoucherNo
                                     from trn.AdjustmentNote A
                                     LEFT JOIN TRN.Voucher V ON V.Id=A.VoucherId
                                     LEFT JOIN TRN.InvoiceWriteOffDetail AWD ON AWD.AdjustmentNoteId=A.Id
                                     LEFT JOIN TRN.InvoiceWriteOff AW ON AW.Id=AWD.InvoiceWriteOffId
                                     LEFT JOIN TRN.Voucher VW ON VW.Id=AW.VoucherId
                                     where  A.IsPark=1 AND AWD.InvoiceWriteOffId = '" + invoiceWriteOffId + "' ";
                objCon1 = new ConnectionManager.DAL.ConManager("1");
                objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                if (dsMaster1.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException(dsMaster1.Tables[0].Rows[0]["SourceType"].ToString() + " VoucherNo " + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + " have to Post first!");
                }
                flag = true;
                var financing = _invoiceWriteOffRepository.Find(invoiceWriteOffId);
                CheckIsPosted(financing);

                financing.IsPark = false;
                AuditService.UpdatedLog(financing);
                //AuditService.PostedLog(financing);
                _invoiceWriteOffRepository.Update(financing);
                _voucherService.PostVoucher(financing.VoucherId, financing.UpdatedBy, financing.UpdatedFromIP);
                if (financing.VoucherId != null)
                {
                    var adjustmentNote = _adjustmentNoteRepository.Query(r => r.VoucherId == financing.VoucherId).Select().FirstOrDefault();
                    if (adjustmentNote != null)
                    {
                        adjustmentNote.IsPark = false;
                        _adjustmentNoteRepository.Update(adjustmentNote);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void ApproveVendorPayment(InvoiceWriteOff invoiceWriteOff, OTSBD.IdentityParameter para)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var financing = _invoiceWriteOffRepository.Find(invoiceWriteOff.Id);

                financing.ApprovalStatus = invoiceWriteOff.ApprovalStatus;
                financing.ApproveRemark = invoiceWriteOff.ApproveRemark;
                financing.ApprovedBy = para.AddedBy;
                financing.ApprovedDate = DateTime.Now;
                AuditService.UpdatedLog(financing);
                _invoiceWriteOffRepository.Update(financing);

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

        public void PostInvoiceToAcceptance(string invoiceWriteOffId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var financing = _invoiceWriteOffRepository.Find(invoiceWriteOffId);
                CheckIsPosted(financing);

                financing.IsPark = false;
                AuditService.UpdatedLog(financing);
                _invoiceWriteOffRepository.Update(financing);
                _voucherService.PostVoucher(financing.VoucherId);
                var grnBuilder = new System.Text.StringBuilder();
                var buildergrnSql = @"UPDATE TRN.Invoice set IsPark =0 WHERE VoucherId='" + financing.VoucherId + "'";
                grnBuilder.Append(buildergrnSql);
                _sqlRepository.ExecuteSqlCommand(grnBuilder.ToString());
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

        private static void CheckIsPosted(InvoiceWriteOff invoiceWriteOff)
        {
            if (!invoiceWriteOff.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }
        private static void CheckIsInvoicePosted(Invoice invoice)
        {
            if (!invoice.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }

        private static void CheckIsPostedInvoice(Invoice invoice)
        {
            if (!invoice.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }
        public string InsertCustomerInvoiceReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
               , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
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
                if (voucherVM.PaymentSource == PaymentSource.Discount.ToString())
                    voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                // INSERT INTO InvoiceWriteOff
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;


                var withholdgl = false;

                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                    if (null == invoiceDetail)
                        throw new CustomException("Invoice not found!");

                    invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                    if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                    invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                    invoice.WrittenOffAmount += voucherDetailVM.Amount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.Update(invoice);

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
                        InvoiceId = voucherDetailVM.InvoiceId,
                        InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                        Amount = voucherDetailVM.Amount,
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
                        CrAmount = voucherDetailVM.Amount,
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
                        var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
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
                        var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
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
                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    var currentBankChargeDetailId = 0;
                    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                    {
                        currentBankChargeDetailId++;
                        var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                        {
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            BankMasterId = invoiceWriteOff.BankMasterId,
                            CashMasterId = invoiceWriteOff.CashMasterId,
                            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                            SourceType = invoiceWriteOff.SourceType,
                            Narration = voucher.Narration,
                            Archive = invoiceWriteOff.Archive,
                            Amount = bankChargeDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP
                        }, currentBankChargeDetailId);

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
                        totalCurrencyCharges += bankChargeDetailVM.CompanyCurrencyAmount;
                    }
                }

                if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                {
                    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                        foreach (var invoiceTaxVM in taxDetailVMList)
                        {
                            var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                            if (null == taxCode)
                                throw new CustomException("Tax code not found!");

                            var taxCodeGL = _accountsCommonService.GetTaxCodeGL(taxCode.Id);
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
                            totalAmountDr += invoiceTaxVM.TaxAmount;
                            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                            // Insert Into Customer Invoice Tax Detail (Withhold GL)
                            withholdgl = taxCode.IsCreditable;
                            if (taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL["CreditableGLId"].ToString()))
                            {
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    GLGeneralInfoId = taxCodeGL["CreditableGLId"].ToString(),
                                    BudgetMasterId = taxCodeGL["CreditableGLBudgetMasterId"].ToString(),
                                    ActivityId = taxCodeGL["CreditableGLActivityId"].ToString(),
                                    Amount = invoiceTax.TaxAmount,
                                    AType = "Dr"
                                };
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    DrAmount = invoiceTaxDetail.Amount,
                                };
                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                                var voucherDetailCurrencyTax = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = companyCurrencyId,
                                    DrAmount = voucherDetailTax.DrAmount * voucherVM.CompanyCurrencyRate,//totalCurrencyAmountDr,/*voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,*/
                                    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                };
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                                totalCurrencyAmountDr += voucherDetailTax.DrAmount * voucherVM.CompanyCurrencyRate;
                            }
                        }
                    }
                }

                if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                    {
                        var voucherDetailDr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = voucherDetailVMList.Sum(r => r.Amount) - totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailDr.DrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailDr.DrAmount += voucherVM.RoundingAmount;
                        totalAmountDr += voucherDetailDr.DrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailDr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                        voucherDetailDr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailDr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailDr.ActivityId = bankMaster["ActivityId"].ToString();
                        if (string.IsNullOrEmpty(voucherDetailDr.ActivityId))
                            throw new CustomException("ActivityId is not found.");
                        voucherDetailDr.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailDr.PartyType = PartyType.Bank.ToString();
                        if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                        else
                            glTransactionDetail.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;


                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = totalCurrencyAmountCr - totalCurrencyCharges
                        });
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            voucherDetailCurrencyCr.DrAmount -= voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate;
                            totalCurrencyAmountDr += totalCurrencyAmountCr - totalCurrencyCharges;

                        }
                        else if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            voucherDetailCurrencyCr.DrAmount += voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate;
                            totalCurrencyAmountDr += totalCurrencyAmountCr + (voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate) - totalCurrencyCharges;

                        }
                        else
                        {
                            totalCurrencyAmountDr += totalCurrencyAmountCr - totalCurrencyCharges;

                        }
                    }
                    else
                        throw new CustomException("Bank Id not found!");
                }

                if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                    {
                        var voucherDetailDr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = voucherDetailVMList.Sum(r => r.Amount) - totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailDr.DrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailDr.DrAmount += voucherVM.RoundingAmount;
                        totalAmountDr += voucherDetailDr.DrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailDr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };


                        var cashMaster = _accountsCommonService.GetCashMaster(voucherVM.CashMasterId);
                        voucherDetailDr.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailDr.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailDr.ActivityId = cashMaster["ActivityId"].ToString();
                        voucherDetailDr.CashMasterId = cashMaster["Id"].ToString();
                        voucherDetailDr.PartyType = PartyType.Cash.ToString();
                        if (cashMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                        else
                            glTransactionDetail.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;

                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = totalCurrencyAmountCr - totalCurrencyCharges
                        });
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            voucherDetailCurrencyCr.DrAmount -= voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate;
                            totalCurrencyAmountDr += totalCurrencyAmountCr - totalCurrencyCharges;

                        }
                        else if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            voucherDetailCurrencyCr.DrAmount += voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate;
                            totalCurrencyAmountDr += totalCurrencyAmountCr + (voucherVM.RoundingAmount * voucherVM.CompanyCurrencyRate) - totalCurrencyCharges;

                        }
                        else
                        {
                            totalCurrencyAmountDr += totalCurrencyAmountCr - totalCurrencyCharges;
                        }
                    }
                    else
                        throw new CustomException("Cash Id not found!");
                }

                if (voucherVM.PaymentSource == PaymentSource.Discount.ToString())
                {
                    // INSERT INTO VoucherDetail (Bank or cash side Dr)
                    var voucherDetailDr = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        DrAmount = voucherDetailVMList.Sum(r => r.Amount) - totalCharges,
                        PaymentSource = invoiceWriteOff.PaymentSource
                    };
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailDr.DrAmount -= voucherVM.RoundingAmount;
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailDr.DrAmount += voucherVM.RoundingAmount;
                    totalAmountDr += voucherDetailDr.DrAmount;

                    var financeType = _financingTypeRepository.Query(r => r.SourceType == FinancingTypeEnum.SalesDiscount.ToString()).Select().FirstOrDefault();
                    if (financeType != null)
                    {
                        var financingTypeGL = _financingTypeGLService.Query(r => r.FinancingTypeId == financeType.Id).Select().FirstOrDefault();
                        if (financingTypeGL == null)
                            throw new CustomException("There is no Sales Discount GL!");
                        voucherDetailDr.GLGeneralInfoId = financingTypeGL.ExpensesGLId;
                        voucherDetailDr.BudgetMasterId = financingTypeGL.ExpensesBudgetMasterId;
                        voucherDetailDr.ActivityId = financingTypeGL.ExpensesActivityId;

                    }
                    else
                        throw new CustomException("There is no Sales Discount Type!");
                    // INSRT INTO GLTransactionDetail

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = totalCurrencyAmountCr - totalCurrencyCharges
                    });
                    totalCurrencyAmountDr += totalCurrencyAmountCr - totalCurrencyCharges;
                }


                if (!string.IsNullOrEmpty(invoiceWriteOff.RoundingType))
                {
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString() || invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(invoiceWriteOff.CompanyId);
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountCr += voucherDetailRoundingDr.CrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.CrAmount
                            });
                            totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.CrAmount;
                        }
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountDr += voucherDetailRoundingDr.DrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.DrAmount
                            });

                        }
                    }
                }
                //totalCurrencyAmountDr = totalCurrencyAmountCr;
                totalAmountCr += taxDrAmount;
                totalAmountDr += totalCharges;
                totalCurrencyAmountDr += totalCurrencyCharges;
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

        public string InsertInvoiceRoundOffJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _unitOfWork.BeginTransaction();
                flag = true;
                    voucherVM.Amount = 0;
                voucherVM.PartyType = "Customer";
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);
                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.PartyId != null)
                    {
                        currentInvoiceWriteOffDetailId++;
                        var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucherVM.CurrencyId,
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            InvoiceId = voucherDetailVM.InvoiceId,
                            InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                            Amount = 0,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP,
                            Archive = invoiceWriteOff.Archive,
                            ModelState = invoiceWriteOff.ModelState,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration
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
                            CrAmount = 0,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            PartyType = voucherDetailVM.PartyType,
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
                            CrAmount = voucherDetailVM.CrAmount
                        });

                        totalAmountCr += 0;
                        totalCurrencyAmountCr += voucherDetailVM.CrAmount;

                    }
                    else if(voucherDetailVM.PartyId==null)
                    {
                        var voucherDetailCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            EntityId = voucherDetailVM.EntityId,
                            CrAmount = 0,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PartyId = invoiceWriteOff.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            PartyType = invoiceWriteOff.PartyType,
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
                            DrAmount = voucherDetailVM.DrAmount
                        });

                        totalCurrencyAmountDr +=  voucherDetailVM.DrAmount;

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
        private string GetInvoiceWriteOffGroupNoPK()
        {
            return base.GetAutoNumber("InvoiceWriteOffGroupNo", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private string GetInvoiceGroupNoPK()
        {
            return base.GetAutoNumber("InvoiceGroupNo", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertCustomerInvoiceBanksReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
              , IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                string _invoiceWriteOffGroupNo = GetInvoiceWriteOffGroupNoPK();

                _unitOfWork.BeginTransaction();
                flag = true;
                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;
                var currencyAmountDr = 0.0M;
                var loanWriteoffAmount = 0.0M;
                var totalAmountDr = 0.0M;
                var currencyAmountCr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);
                int exlosslen = banksDetailVMList.Count();
                int len = banksDetailVMList.Count();
                int exgainlen = exlosslen;
                decimal chargesAmount = 0;
                decimal chargesBooksAmount = 0;
                decimal totalchargesAmount = 0;
                decimal totalbookchargesAmount = 0;
                int count = 0;
                decimal chargesCountAmount = 0;
                decimal chargesBooksCountAmount = 0;
                decimal totalexLossAmount = voucherDetailVMList.Where(r => r.ExchangeType == "ExchangeLoss").Sum(r => r.ExchangeAmount);
                decimal totalexGainAmount = voucherDetailVMList.Where(r => r.ExchangeType == "ExchangeGain").Sum(r => r.ExchangeAmount);
                var invoiceList = new List<Invoice>();
                var bankchargeNewList = new List<BankChargeViewModel>();
                var exchangeLossList = new List<VoucherViewModel>();
                var exchangeGainList = new List<VoucherViewModel>();
                //Dictionary<string, decimal> invoiceList = new Dictionary<string,decimal>();
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

                    voucherVM.Amount = item.Amount;
                    voucherVM.BankMasterId = item.BankMasterId;
                    voucherVM.InvoiceWriteOffGroupNo = _invoiceWriteOffGroupNo;
                    var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    invoiceWriteOff.VoucherId = voucher.Id;



                    //decimal invoiceAmount = 0;
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {

                        //var invoiceRow = voucherDetailVMList.SingleOrDefault();
                        currencyAmountCr = 0.0M;
                        var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);

                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");
                        if (len > count)
                        {
                            voucherDetailVM.CrAmount = Math.Round((voucherDetailVM.Amount * (item.Amount + chargesAmount) / (banksDetailVMList.Sum(r => r.Amount) + totalchargesAmount)), 2);
                            invoiceDetail.WrittenOffAmount += (voucherDetailVM.CrAmount);
                            currencyAmountCr = Math.Round((voucherDetailVM.BaseCrAmount * (item.BaseDrAmount + chargesBooksAmount) / (banksDetailVMList.Sum(r => r.BaseDrAmount) + totalbookchargesAmount)), 2);
                            var inv = new Invoice
                            {
                                Id = invoiceDetail.InvoiceId,
                                Amount = voucherDetailVM.CrAmount,
                                BaseCrAmount = currencyAmountCr
                            };
                            invoiceList.Add(inv);
                        }

                        if (len == count)
                        {
                            voucherDetailVM.CrAmount = voucherDetailVMList.Where(r => r.InvoiceId == voucherDetailVM.InvoiceId).Sum(r => r.Amount) - invoiceList.Where(r => r.Id == voucherDetailVM.InvoiceId).Sum(r => r.Amount);
                            invoiceDetail.WrittenOffAmount += voucherDetailVM.CrAmount;
                            currencyAmountCr = voucherDetailVMList.Where(r => r.InvoiceId == voucherDetailVM.InvoiceId).Sum(r => r.BaseCrAmount) - invoiceList.Where(r => r.Id == voucherDetailVM.InvoiceId).Sum(r => r.BaseCrAmount);
                        }

                        if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                            throw new CustomException("Received amount can not cross balance amount.");

                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                        // TODO: have a gap here if invoice split
                        var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                        invoice.WrittenOffAmount += voucherDetailVM.CrAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.Update(invoice);

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
                            InvoiceId = voucherDetailVM.InvoiceId,
                            InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                            Amount = voucherDetailVM.CrAmount,
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
                            CrAmount = voucherDetailVM.CrAmount,
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
                            CrAmount = currencyAmountCr
                        });

                        totalAmountCr += voucherDetailCr.CrAmount;
                        totalCurrencyAmountCr += currencyAmountCr;
                        //totalCurrencyAmountCr += Math.Round(voucherDetailVM.CompanyCurrencyRate * voucherDetailCr.CrAmount, 3);
                    }

                    foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.ExchangeType == "ExchangeLoss"))
                    {
                        if (voucherDetailVM.ExchangeType == "ExchangeLoss" && voucherDetailVM.ExchangeAmount > 0)
                        {
                            var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
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
                            decimal exchangeDrAmount = 0;
                            if (len > count)
                            {
                                exchangeDrAmount = Math.Round(voucherDetailVM.ExchangeAmount * (item.Amount + chargesAmount) / (banksDetailVMList.Sum(r => r.Amount) + totalchargesAmount), 2);
                                var exloss = new VoucherViewModel
                                {
                                    ExchangeType = voucherDetailVM.ExchangeType,
                                    ExchangeAmount = exchangeDrAmount,
                                    InvoiceId = voucherDetailVM.InvoiceId
                                };
                                exchangeLossList.Add(exloss);
                            }
                            if (len == count)
                            {
                                exchangeDrAmount = voucherDetailVM.ExchangeAmount - exchangeLossList.Where(r => r.InvoiceId == voucherDetailVM.InvoiceId && r.ExchangeType == "ExchangeLoss").Sum(r => r.ExchangeAmount);
                            }

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDtEx.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = Math.Round(exchangeDrAmount, 2)
                            });
                            totalCurrencyAmountDr += Math.Round(exchangeDrAmount, 2);
                        }

                    }

                    foreach (var voucherDetailVM in voucherDetailVMList.Where(r => r.ExchangeType == "ExchangeGain"))
                    {
                        if (voucherDetailVM.ExchangeType == "ExchangeGain" && voucherDetailVM.ExchangeAmount > 0)
                        {
                            var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
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
                            decimal exchangeCrAmount = 0;

                            if (len > count)
                            {
                                exchangeCrAmount = Math.Round(voucherDetailVM.ExchangeAmount * (item.Amount + chargesAmount) / (banksDetailVMList.Sum(r => r.Amount) + totalchargesAmount), 2);
                                var exgain = new VoucherViewModel
                                {
                                    ExchangeType = voucherDetailVM.ExchangeType,
                                    ExchangeAmount = exchangeCrAmount,
                                    InvoiceId = voucherDetailVM.InvoiceId
                                };
                                exchangeGainList.Add(exgain);
                            }
                            if (len == count)
                            {
                                exchangeCrAmount = voucherDetailVM.ExchangeAmount - exchangeGainList.Where(r => r.InvoiceId == voucherDetailVM.InvoiceId && r.ExchangeType == "ExchangeGain").Sum(r => r.ExchangeAmount);
                            }

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDtExGain.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = Math.Round(exchangeCrAmount, 2)
                            });
                            totalCurrencyAmountCr += exchangeCrAmount;
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
                                InvoiceWriteOffId = invoiceWriteOff.Id,
                                BankMasterId = invoiceWriteOff.BankMasterId,
                                CashMasterId = invoiceWriteOff.CashMasterId,
                                FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                                SourceType = invoiceWriteOff.SourceType,
                                Narration = voucher.Narration,
                                Archive = invoiceWriteOff.Archive,
                                //Amount = Math.Round(bankChargeDetailVM.Amount * item.Amount / banksDetailVMList.Sum(r => r.Amount), 2),
                                AddedBy = invoiceWriteOff.AddedBy,
                                AddedDate = invoiceWriteOff.AddedDate,
                                AddedFromIP = invoiceWriteOff.AddedFromIP
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

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        // INSERT INTO VoucherDetail (Bank or cash side Dr)
                        var voucherDetailDr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = item.Amount,
                            PaymentSource = invoiceWriteOff.PaymentSource
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
                            if (voucherVM.Amount > 0)
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
                            if (voucherVM.Amount > 0)
                            {
                                _financingService.UpdateFinancingDetail(financingDetail);
                            }
                            financingDetailWriteOff.GLGeneralInfoId = gl.LiabilityGLId;
                            financingDetailWriteOff.BudgetMasterId = gl.LiabilityBudgetMasterId;
                            financingDetailWriteOff.ActivityId = gl.LiabilityActivityId;

                            if (voucherVM.Amount > 0)
                            {
                                _financingService.InsertFinancingWriteOffDetail(financinWriteOff, financingDetailWriteOff, 1);

                            }
                            voucherDetailDr.FinancingDetailWriteOffId = financingDetailWriteOff.Id;


                        }

                        #endregion

                        if (!string.IsNullOrEmpty(item.BankMasterId))
                        {
                            var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
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

                            //if(companyCurrencyId != item.BankCurrencyId)
                            //{
                            //    currencyAmountDr =  Math.Round(item.Amount * item.CompanyCurrencyRate, 2);
                            //}
                            //else
                            //{
                            //    currencyAmountDr = item.BaseDrAmount;
                            //}
                            currencyAmountDr = item.BaseDrAmount;
                        }
                        else
                        {
                            currencyAmountDr = item.BaseDrAmount;
                        }
                        var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
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
                        //if (item.CompanyCurrencyRate < voucherVM.CompanyCurrencyRate && companyCurrencyId != item.BankCurrencyId)
                        //{
                        //    var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);

                        //    exchangeloss.GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString();
                        //    exchangeloss.BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString();
                        //    exchangeloss.ActivityId = lossGL["CompanyCurrencyActivityId"].ToString();
                        //    exchangeloss.CurrencyId = voucher.CurrencyId;
                        //    exchangeloss.DocDate = voucher.DocDate;
                        //    exchangeloss.DocRefNo = voucher.DocRefNo;
                        //    exchangeloss.Narration = voucher.Narration;
                        //    exchangeloss.PartyType = "ExchangeLoss";
                        //    exchangeloss.DrAmount = 0;
                        //    exchangeloss.CrAmount = 0;

                        //    currentVoucherDetailId++;
                        //    _voucherService.InsertVoucherDetail(voucher, exchangeloss, currentVoucherDetailId);
                        //    _voucherService.InsertVoucherDetailCompanyCurrency(exchangeloss, new VoucherDetailCurrency
                        //    {
                        //        ParallelCurrencyId = companyCurrencyId,
                        //        FromCurrencyId = exchangeloss.CurrencyId,
                        //        ToCurrencyId = companyCurrencyId,
                        //        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        //        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(exchangeloss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        //        DrAmount = voucherVM.Amount * (voucherVM.CompanyCurrencyRate - item.CompanyCurrencyRate)
                        //    });
                        //    totalCurrencyAmountDr += voucherVM.Amount * (voucherVM.CompanyCurrencyRate - item.CompanyCurrencyRate);

                        //}
                        ////***********************Exchange Gain*************************************
                        //if (item.CompanyCurrencyRate > voucherVM.CompanyCurrencyRate && companyCurrencyId != item.BankCurrencyId)
                        //{
                        //    var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                        //    exchangeGain.GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString();
                        //    exchangeGain.BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString();
                        //    exchangeGain.ActivityId = gainGL["CompanyCurrencyActivityId"].ToString();
                        //    exchangeGain.CurrencyId = voucher.CurrencyId;
                        //    exchangeGain.DocDate = voucher.DocDate;
                        //    exchangeGain.DocRefNo = voucher.DocRefNo;
                        //    exchangeGain.Narration = voucher.Narration;
                        //    exchangeGain.PartyType = "ExchangeGain";
                        //    exchangeGain.DrAmount = 0;
                        //    exchangeGain.CrAmount = 0;

                        //    currentVoucherDetailId++;
                        //    _voucherService.InsertVoucherDetail(voucher, exchangeGain, currentVoucherDetailId);
                        //    _voucherService.InsertVoucherDetailCompanyCurrency(exchangeGain, new VoucherDetailCurrency
                        //    {
                        //        ParallelCurrencyId = companyCurrencyId,
                        //        FromCurrencyId = exchangeGain.CurrencyId,
                        //        ToCurrencyId = companyCurrencyId,
                        //        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        //        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(exchangeGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        //        CrAmount = Math.Round(voucherVM.Amount * (item.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate))
                        //    });
                        //    totalCurrencyAmountCr += Math.Round(voucherVM.Amount * (item.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate));
                        //}
                    }
                    #endregion

                }

                //totalCurrencyAmountDr = totalCurrencyAmountCr;
                //totalAmountCr += taxDrAmount;
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return "Successfully";
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
        #region purchase realization
        public string InsertPurchaseRealizationService(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
       , IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                string _invoiceGroupNo = GetInvoiceGroupNoPK();

                _unitOfWork.BeginTransaction();
                flag = true;
                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;
                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                //Dictionary<string, decimal> invoiceList = new Dictionary<string,decimal>();
                foreach (var item in banksDetailVMList)
                {

                    voucherVM.Amount = item.Amount;
                    voucherVM.BankMasterId = item.BankMasterId;

                    var invoice = new Invoice
                    {
                        Amount = voucherVM.Amount,
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        CurrencyId = voucherVM.CurrencyId,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        EntityId = voucherVM.EntityId,
                        PlantId = voucherVM.PlantId,
                        IsExcludingTax = voucherVM.IsExcludingTax,
                        IsSplit = voucherVM.IsSplit,
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = PartyType.Vendor.ToString(),
                        EmployeeId = voucherVM.EmployeeId,
                        PaymentTermId = voucherVM.PaymentTermId,
                        PostingDate = voucherVM.PostingDate,
                        SourceType = SourceType.SuspensePayable.ToString(),
                        BaseNoOfDays = 0,
                        BaseOnDueDate = null,
                        RevisedDueDate = null,
                        ActualDueDate = null,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        TaxYearId = voucherVM.TaxYearId,
                        VoucherDate = DateTime.Now,
                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                        CompanyCurrencyRate = voucherVM.CompanyCurrencyRate,
                        InvoiceGroupNo = _invoiceGroupNo,
                        IsPark = true
                    };
                    _invoiceService.InsertInvoice(invoice);
                    voucherVM.IsPark = true;
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    invoice.VoucherId = voucher.Id;



                    // INSERT INTO InvoiceDetail
                    currentInvoiceWriteOffDetailId++;
                    var invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId = invoice.Id,
                        GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                        BudgetMasterId = voucherVM.BudgetMasterId,
                        ActivityId = voucherVM.ActivityId,
                        Amount = voucherVM.Amount,
                        NetAmount = voucherVM.Amount,
                        TaxAmount = 0,
                        WrittenOffAmount = 0,
                        AddedBy = invoice.AddedBy,
                        AddedDate = invoice.AddedDate,
                        AddedFromIP = invoice.AddedFromIP,
                        Archive = invoice.Archive
                    };
                    _invoiceService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceWriteOffDetailId);
                    if (string.IsNullOrEmpty(voucherVM.ActivityId))
                        throw new CustomException("ActivityId is not found.");
                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                        BudgetMasterId = voucherVM.BudgetMasterId,
                        ActivityId = voucherVM.ActivityId,
                        EntityId = voucherVM.EntityId,
                        CrAmount = voucherVM.Amount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyId = invoice.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PartyType = invoice.PartyType,
                        InvoiceDetailId = invoiceDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round(voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount, 2)
                    });

                    totalAmountCr += voucherDetailCr.CrAmount;
                    totalCurrencyAmountCr += Math.Round(voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount, 2);


                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        // INSERT INTO VoucherDetail (Bank or cash side Dr)
                        var voucherDetailDr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = item.Amount,
                            PaymentSource = invoice.PaymentSource
                        };
                        totalAmountDr += voucherDetailDr.DrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailDr.PaymentSource,
                            BankMasterId = item.BankMasterId
                        };

                        if (!string.IsNullOrEmpty(item.BankMasterId))
                        {
                            var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                            voucherDetailDr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                            voucherDetailDr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                            voucherDetailDr.ActivityId = bankMaster["ActivityId"].ToString();
                            if (string.IsNullOrEmpty(voucherDetailDr.ActivityId))
                                throw new CustomException("ActivityId is not found.");
                            voucherDetailDr.BankMasterId = bankMaster["Id"].ToString();
                            voucherDetailDr.PartyType = PartyType.Bank.ToString();
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
                        var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = item.BaseDrAmount//Math.Round(voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,2)
                        });
                        totalCurrencyAmountDr += item.BaseDrAmount;// Math.Round(voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,2);
                    }

                }

                //totalCurrencyAmountDr = totalCurrencyAmountCr;
                //totalAmountCr += taxDrAmount;
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (totalCurrencyAmountCr != totalCurrencyAmountDr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return "Successfully";
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
        #endregion purchase realization

        public GridModel CustomerInvoiceBanksQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AW.InvoiceWriteOffGroupNo--, VD.VoucherId, V.VoucherNo, AW.Id
                                    , P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode,SUM(IWD.Amount) Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, IsPark=case when AW.IsPark=0 then 'Posted' else 'Parked' end, AW.BankJournalId
                                    
                                    ,VoucherNo=STUFF((SELECT DISTINCT ','+xpo.VoucherNo from
                                    			[TRN].Voucher xpo
                                    			INNER JOin trn.[InvoiceWriteOff] xPDAMAP on xpo.Id=xPDAMAP.VoucherId
                                    			WHERE AW.InvoiceWriteOffGroupNo=xPDAMAP.InvoiceWriteOffGroupNo for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    
                                    FROM [TRN].[InvoiceWriteOff] AS AW
                                    LEFT JOIN (
                                    SELECT InvoiceWriteOffId,SUM(Amount) Amount FROM [TRN].[InvoiceWriteOffDetail] Group BY InvoiceWriteOffId 
                                    ) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
                                    --LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    --LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.[SourceType]='" + sourceType + @"'
                                    Group BY AW.InvoiceWriteOffGroupNo--, VD.VoucherId, V.VoucherNo, AW.Id
                                    , P.Code , P.UserName, AW.PostingDate
                                    , AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId";
            return _sqlRepository.GetGridData(parameters);
        }
        public void CustomerBanksPost(string invoiceWriteOffNo)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var financing = _invoiceWriteOffRepository.Query(r => r.InvoiceWriteOffGroupNo == invoiceWriteOffNo).Select().ToList();
                foreach (var item in financing)
                {
                    CheckIsPosted(item);

                    item.IsPark = false;
                    AuditService.UpdatedLog(item);
                    if (item.AddedBy == item.UpdatedBy && item.CompanyId != "C20171")
                    {
                        throw new CustomException("You are not authorized to Post!,Prepared by and posted by are same!");
                    }

                    _invoiceWriteOffRepository.Update(item);
                    _voucherService.PostVoucher(item.VoucherId);
                    var financingwriteOff = _loanInterestPayableRepository.Query(r => r.VoucherId == item.VoucherId).Select().FirstOrDefault();
                    if (financingwriteOff != null)
                    {
                        financingwriteOff.IsPark = false;
                        AuditService.UpdatedLog(financingwriteOff);
                        _loanInterestPayableRepository.Update(financingwriteOff);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void SuspensePayablePost(string invoiceGroupNo)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var financing = _invoiceService.Query(r => r.InvoiceGroupNo == invoiceGroupNo).Select().ToList();
                foreach (var item in financing)
                {
                    CheckIsPostedInvoice(item);

                    item.IsPark = false;
                    AuditService.UpdatedLog(item);
                    _invoiceService.Update(item);
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
        public void InsertCustomerInvoiceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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
                // INSERT INTO InvoiceWriteOff TABLE
                var invoiceWriteOff = new InvoiceWriteOff
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = PartyType.Customer.ToString(),
                    CurrencyId = voucherVM.CurrencyId,
                    Amount = voucherDetailVMList.Sum(r => r.DrAmount),
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
                    BankJournalId = voucherVM.BankJournalId,
                    BankJournalDetailId = voucherVM.BankJournalDetailId
                };
                InsertInvoiceWriteOff(invoiceWriteOff);

                //********Voucher WriteOff**********************
                var currentvoucherWriteOffId = _voucherWriteOffRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.VoucherWriteOff WHERE VoucherDetailId='{voucherVM.VoucherDetailId}'").First();
                var voucherDetail = _voucherService.FindVoucherDetail(voucherVM.VoucherDetailId);
                currentvoucherWriteOffId++;
                var voucherWriteOff = new VoucherWriteOff
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
                    SourceType = SourceType.PartyReconcilliation.ToString(),
                    PaymentSource = PaymentSource.Journal.ToString(),
                    Amount = invoiceWriteOff.Amount * voucherVM.CompanyCurrencyRate,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType
                };
                _voucherService.InsertVoucherWriteOff(voucherDetail, voucherWriteOff, currentvoucherWriteOffId);


                // Bank Journal Table update
                var bankJournal = _bankJournalService.FindBankJournal(invoiceWriteOff.BankJournalId);
                bankJournal.WrittenOffAmount += voucherDetailVMList.Sum(r => r.DrAmount);
                bankJournal.IsWrittenOff = bankJournal.Amount == bankJournal.WrittenOffAmount;
                if (bankJournal.Amount < bankJournal.WrittenOffAmount)
                    throw new CustomException("Settlement amount can not greater than receipt amount.");
                _bankJournalService.UpdateBankJournal(bankJournal);

                var bankJournalDetail = _bankJournalService.FindBankJournalDetail(invoiceWriteOff.BankJournalDetailId);
                bankJournalDetail.WrittenOffAmount += voucherDetailVMList.Sum(r => r.DrAmount);
                bankJournalDetail.IsWrittenOff = bankJournalDetail.Amount == bankJournalDetail.WrittenOffAmount;
                if (bankJournalDetail.Amount < bankJournalDetail.WrittenOffAmount)
                    throw new CustomException("Settlement amount can not greater than receipt amount.");
                _bankJournalService.UpdateBankJournalDetail(bankJournal, bankJournalDetail);

                // INSERT INTO VoucherDetail
                var currentInvoiceWriteOffDetailId = 0;
                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.QueryInvoiceDetailEnumerable(invoiceDetailIds);
                var voucher = new Voucher();
                var extrype = voucherDetailVMList.Select(r => r.ExchangeType == "ExchangeGain" || r.ExchangeType == "ExchangeLoss");
                if (extrype != null)
                {
                    voucher = _voucherService.InsertVoucher(voucherVM);
                }
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                    if (null == invoiceDetail)
                        throw new CustomException("Invoice not found!");

                    invoiceDetail.WrittenOffAmount += voucherDetailVM.DrAmount;

                    if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                        throw new CustomException("Received Amount can not cross Balance Amount");

                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                    invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                    invoice.WrittenOffAmount += voucherDetailVM.DrAmount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.Update(invoice);

                    // INSERT INTO InvoiceDetail
                    currentInvoiceWriteOffDetailId++;
                    var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                    {
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        InvoiceId = voucherDetailVM.InvoiceId,
                        InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        Amount = voucherDetailVM.DrAmount,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        CompanyId = voucherDetailVM.CompanyId,
                        PlantId = voucherDetailVM.PlantId,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = voucherDetailVM.PartyType
                    };
                    InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                    var currentvoucherWriteOffDetailId = _voucherWriteOffDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.VoucherWriteOffDetail WHERE VoucherWriteOffId='{voucherWriteOff.Id}'").First();
                    if (voucherDetailVM.Amount < 0)
                        throw new CustomException("Please ensure all line item have amount.");

                    // INSERT INTO BankJournalDetail
                    currentvoucherWriteOffDetailId++;
                    var voucherWriteOffDetail = new VoucherWriteOffDetail
                    {
                        VoucherDetailId = voucherDetailVM.VoucherDetailId,
                        VoucherWriteOffId = voucherWriteOff.Id,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        Amount = voucherDetailVM.DrAmount * voucherDetailVM.CompanyCurrencyRate,
                        PartyType = PartyType.Customer.ToString()
                    };
                    _voucherService.InsertVoucherWriteOffDetail(voucherWriteOff, voucherWriteOffDetail, currentvoucherWriteOffDetailId);

                    var currentVoucherDetailId = 0;
                    if (voucherDetailVM.ExchangeType == "ExchangeLoss" && voucherDetailVM.ExchangeAmount > 0)
                    {
                        var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                        var voucherDtEx = new VoucherDetail
                        {
                            GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = companyCurrencyId,

                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            DrAmount = voucherDetailVM.ExchangeAmount,
                            PartyType = voucherDetailVM.ExchangeType
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDtEx, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDtEx.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailVM.ExchangeAmount
                        });

                        var voucherCtExGain = new VoucherDetail
                        {
                            GLGeneralInfoId = invoiceWriteOffDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceWriteOffDetail.BudgetMasterId,
                            ActivityId = invoiceWriteOffDetail.ActivityId,
                            InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                            PartyId = invoiceWriteOffDetail.PartyId,
                            PartyPlantId = invoiceWriteOffDetail.PartyPlantId,
                            PartyType = voucherDetailVM.PartyType,
                            CurrencyId = companyCurrencyId,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            CrAmount = voucherDetailVM.ExchangeAmount
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCtExGain, currentVoucherDetailId);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCtExGain, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherDetailVM.ExchangeAmount
                        });
                    }

                    if (voucherDetailVM.ExchangeType == "ExchangeGain" && voucherDetailVM.ExchangeAmount > 0)
                    {
                        var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                        var voucherCtExGain = new VoucherDetail
                        {
                            GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = companyCurrencyId,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            CrAmount = voucherDetailVM.ExchangeAmount,
                            PartyType = voucherDetailVM.ExchangeType
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCtExGain, currentVoucherDetailId);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCtExGain, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCtExGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherDetailVM.ExchangeAmount
                        });

                        var voucherDtExGain = new VoucherDetail
                        {
                            GLGeneralInfoId = invoiceWriteOffDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceWriteOffDetail.BudgetMasterId,
                            ActivityId = invoiceWriteOffDetail.ActivityId,
                            InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                            PartyId = invoiceWriteOffDetail.PartyId,
                            PartyPlantId = invoiceWriteOffDetail.PartyPlantId,
                            PartyType = voucherDetailVM.PartyType,
                            CurrencyId = companyCurrencyId,
                            DocDate = voucher.DocDate,
                            DocRefNo = voucher.DocRefNo,
                            Narration = voucher.Narration,
                            DrAmount = voucherDetailVM.ExchangeAmount
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
                            DrAmount = voucherDetailVM.ExchangeAmount
                        });
                    }
                    voucher.CurrencyId = companyCurrencyId;
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


        public List<Dictionary<string, object>> GetVoucherWriteOffList(string companyGroupId, string companyId, string plantId, string voucherWriteOffId)
        {
            var sql = @"SELECT  VD.VoucherId,VD.Id, VD.PartyType, VD.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, VD.PartyPlantId, PP.UserName AS PartyPlantName, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, VD.CurrencyId, C.Code AS CurrencyCode, VD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, VD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, VD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS DocDate
                                , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, VD.Narration, CC.CompanyCurrencyAmount AS Receivable, ISNULL(AM.Amount,0) AS Received
                                ,(ISNULL(CC.CompanyCurrencyAmount,0)-ISNULL(AM.Amount,0)) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion
                                FROM 
								[TRN].[VoucherDetail] AS VD
                                LEFT JOIN(select VW.VoucherDetailId,SUM(VW.Amount) AS Amount from  [TRN].[VoucherWriteOff] VW  where vw.IsWrittenOff=0 AND Vw.Id!='" + voucherWriteOffId + @"' GROUP BY VW.VoucherDetailId) AS AM ON VD.Id=AM.VoucherDetailId
								LEFT JOIN (SELECT * FROM TRN.VoucherWriteOff WHERE Id='" + voucherWriteOffId + @"') AS VDW ON VDW.VoucherDetailId=VD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
                                WHERE  V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyType='" + PartyType.Customer.ToString() + @"' 
                                AND VD.PartyId<>'' AND VD.DrAmount=0 AND (CC.CompanyCurrencyAmount-ISNULL(AM.Amount,0))!=0 AND VDW.Id='" + voucherWriteOffId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public List<Dictionary<string, object>> GetVoucherWriteOffDetailList(string companyGroupId, string companyId, string plantId, string voucherWriteOffId)
        {
            var sql = @"SELECT  VD.VoucherId, VWD.Id, VWD.VoucherWriteOffId,VWD.VoucherDetailId
, VD.PartyType, VD.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, VD.PartyPlantId, PP.UserName AS PartyPlantName,  VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, VD.CurrencyId, C.Code AS CurrencyCode, VD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, VD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, VD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS DocDate
                                 , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, VD.Narration, CC.CompanyCurrencyAmount AS Receivable
								, ISNULL(AD.Amount,0) AS Received
                               ,(ISNULL(CC.CompanyCurrencyAmount,0)-ISNULL(AD.Amount,0)) AS Balance
							, VWD.Amount AS DrAmount
								, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion
                                FROM [TRN].[VoucherWriteOffDetail] AS VWD 
								LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VWD.VoucherDetailId
                                LEFT JOIN(SELECT VW.VoucherDetailId, SUM(ISNULL(VW.Amount,0)) Amount FROM  [TRN].[VoucherWriteOffDetail] 
								AS VW WHERE VW.VoucherWriteOffId != '" + voucherWriteOffId + @"'  GROUP BY VW.VoucherDetailId) AS AD  ON VWD.VoucherDetailId=AD.VoucherDetailId
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
                                WHERE  V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyType='" + PartyType.Customer.ToString() + @"' 
                                AND VD.PartyId<>'' AND VD.CrAmount=0 AND (CC.CompanyCurrencyAmount-ISNULL(VWD.Amount,0))!=0 AND VWD.VoucherWriteOffId='" + voucherWriteOffId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }



        public string InsertPartyReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {

                if (voucherDetailVMList.Sum(r => r.DrAmount) <= 0)
                    throw new CustomException("Amount is 0.");

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                var currentvoucherWriteOffId = _voucherWriteOffRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.VoucherWriteOff WHERE VoucherDetailId='{voucherVM.VoucherDetailId}'").First();

                var voucherDetail = _voucherService.FindVoucherDetail(voucherVM.VoucherDetailId);
                currentvoucherWriteOffId++;
                var voucherWriteOff = new VoucherWriteOff
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
                    SourceType = SourceType.PartyReconcilliation.ToString(),
                    PaymentSource = PaymentSource.Journal.ToString(),
                    Amount = voucherDetailVMList.Sum(r => r.DrAmount),
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType
                };
                _voucherService.InsertVoucherWriteOff(voucherDetail, voucherWriteOff, currentvoucherWriteOffId);

                if (null != voucherDetailVMList)
                {
                    var currentvoucherWriteOffDetailId = _voucherWriteOffDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.VoucherWriteOffDetail WHERE VoucherWriteOffId='{voucherWriteOff.Id}'").First();
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {

                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");

                        // INSERT INTO BankJournalDetail
                        currentvoucherWriteOffDetailId++;
                        var voucherWriteOffDetail = new VoucherWriteOffDetail
                        {
                            VoucherDetailId = voucherDetailVM.VoucherDetailId,
                            VoucherWriteOffId = voucherWriteOff.Id,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            Amount = voucherDetailVM.DrAmount,
                            PartyType = PartyType.Customer.ToString()
                        };
                        _voucherService.InsertVoucherWriteOffDetail(voucherWriteOff, voucherWriteOffDetail, currentvoucherWriteOffDetailId);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucherWriteOff.DocRefNo;
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

        public string UpdatePartyReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {

                if (voucherDetailVMList.Sum(r => r.DrAmount) <= 0)
                    throw new CustomException("Amount is 0.");
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucherWriteOff = _voucherWriteOffRepository.Find(voucherVM.Id);
                voucherWriteOff.Amount = voucherDetailVMList.Sum(r => r.DrAmount);
                _voucherService.UpdateVoucherWriteOff(voucherWriteOff);

                if (null != voucherDetailVMList)
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.Amount < 0)
                            throw new CustomException("Please ensure all line item have amount.");
                        var voucherWriteOffDetail = new VoucherWriteOffDetail
                        {
                            Id = voucherDetailVM.Id,
                            VoucherDetailId = voucherDetailVM.VoucherDetailId,
                            VoucherWriteOffId = voucherWriteOff.Id,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            Amount = voucherDetailVM.DrAmount,
                            PartyType = PartyType.Customer.ToString()
                        };
                        _voucherService.UpdateVoucherWriteOffDetail(voucherWriteOff, voucherWriteOffDetail);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucherWriteOff.DocRefNo;
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

        public string InsertDebitNoteSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
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
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                decimal taxDrAmount = 0;
                decimal taxDrCurrencyAmount = 0;

                var adjustNoteIds = voucherDetailVMList.Select(r => r.AdjustmentNoteId);
                var adjustNoteDbList = _adjustmentNoteRepository.Query(r => adjustNoteIds.Contains(r.Id)).Select().ToList();
                var adjustNoteDetailIds = voucherDetailVMList.Select(r => r.AdjustmentNoteDetailId);
                var adjustNoteDetailDbList = _adjustmentNoteDetailRepository.Query(r => adjustNoteDetailIds.Contains(r.Id)).Select().ToList();
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var adjustNoteDetail = adjustNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdjustmentNoteDetailId);
                    if (null == adjustNoteDetail)
                        throw new CustomException("Invoice not found!");

                    adjustNoteDetail.WrittenOffAmount += voucherDetailVM.Amount;

                    if (adjustNoteDetail.Amount < adjustNoteDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    adjustNoteDetail.IsWrittenOff = adjustNoteDetail.Amount == adjustNoteDetail.WrittenOffAmount;
                    adjustNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    adjustNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    adjustNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteDetailRepository.Update(adjustNoteDetail);

                    // TODO: have a gap here if invoice split
                    var adjustNote = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                    adjustNote.WrittenOffAmount += voucherDetailVM.Amount;
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
                        Amount = voucherDetailVM.Amount,
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
                        CrAmount = voucherDetailVM.Amount,
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
                        CrAmount = Math.Round(voucherDetailVM.CompanyCurrencyRate * voucherDetailCr.CrAmount, 2)
                    });

                    totalAmountCr += voucherDetailCr.CrAmount;
                    totalCurrencyAmountCr += Math.Round(voucherDetailVM.CompanyCurrencyRate * voucherDetailCr.CrAmount, 2);

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
                if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                {
                    totalCharges = bankChargeDetailVMList.Sum(r => r.Amount);
                    totalCurrencyCharges = bankChargeDetailVMList.Sum(r => r.CompanyCurrencyAmount);
                }

                if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                    {
                        var voucherDetailDr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = voucherDetailVMList.Sum(r => r.Amount) - totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailDr.DrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailDr.DrAmount += voucherVM.RoundingAmount;
                        totalAmountDr += voucherDetailDr.DrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailDr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                        voucherDetailDr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailDr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailDr.ActivityId = bankMaster["ActivityId"].ToString();
                        if (string.IsNullOrEmpty(voucherDetailDr.ActivityId))
                            throw new CustomException("ActivityId is not found.");
                        voucherDetailDr.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailDr.PartyType = PartyType.Bank.ToString();
                        if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                        else
                            glTransactionDetail.DrAmount = Math.Round((totalCurrencyAmountCr - totalCurrencyCharges), 2);

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = Math.Round((totalCurrencyAmountCr - totalCurrencyCharges), 2)
                        }); ;
                        totalCurrencyAmountDr += Math.Round((totalCurrencyAmountCr - totalCurrencyCharges), 2);
                    }
                    else
                        throw new CustomException("Bank Id not found!");
                }
                if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                    {
                        var voucherDetailDr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            DrAmount = voucherDetailVMList.Sum(r => r.Amount) - totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailDr.DrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailDr.DrAmount += voucherVM.RoundingAmount;
                        totalAmountDr += voucherDetailDr.DrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailDr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };


                        var cashMaster = _accountsCommonService.GetCashMaster(voucherVM.CashMasterId);
                        voucherDetailDr.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailDr.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailDr.ActivityId = cashMaster["ActivityId"].ToString();
                        voucherDetailDr.CashMasterId = cashMaster["Id"].ToString();
                        voucherDetailDr.PartyType = PartyType.Cash.ToString();
                        if (cashMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                        else
                            glTransactionDetail.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;

                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = totalCurrencyAmountCr - totalCurrencyCharges
                        });
                        totalCurrencyAmountDr += totalCurrencyAmountCr - totalCurrencyCharges;
                    }
                    else
                        throw new CustomException("Cash Id not found!");
                }

                if (!string.IsNullOrEmpty(invoiceWriteOff.RoundingType))
                {
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString() || invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(invoiceWriteOff.CompanyId);
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountDr += voucherDetailRoundingDr.DrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.DrAmount
                            });
                        }
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountCr += voucherDetailRoundingDr.CrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.CrAmount
                            });
                        }
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
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            BankMasterId = invoiceWriteOff.BankMasterId,
                            CashMasterId = invoiceWriteOff.CashMasterId,
                            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                            SourceType = invoiceWriteOff.SourceType,
                            Narration = voucher.Narration,
                            Archive = invoiceWriteOff.Archive,
                            Amount = bankChargeDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP
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
                        totalCurrencyAmountDr += bankChargeDetailVM.CompanyCurrencyAmount;
                    }
                }
                totalAmountCr += taxDrAmount;
                totalAmountDr += totalCharges;
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
        public string InsertDebitNoteInvoiceSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
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
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

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

                if (voucherVM.PaymentSource == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO InvoiceWriteOff


                    // Invoice
                    var invoiceIds = voucherDetailInvoiceList.Select(r => r.InvoiceId);
                    var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = voucherDetailInvoiceList.Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                    foreach (var voucherDetailVM in voucherDetailInvoiceList)
                    {
                        var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");

                        invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;
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
                        currentInvoiceWriteOffDetailId++;
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
                            Amount = voucherDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP,
                            Archive = invoiceWriteOff.Archive,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration
                        };
                        InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);
                        invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                        // INSERT INTO VoucherDetail
                        var voucherDetailCr = new VoucherDetail
                        {
                            VoucherId = voucher.Id,
                            InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
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

        public string InsertCreditNoteSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> tdsVMList)
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
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                //var withholdgl = false;
                var voucherIds = voucherDetailVMList.FirstOrDefault().VoucherId;
                var adjustNoteIds = voucherDetailVMList.Select(r => r.AdjustmentNoteId);
                var adjustNoteDbList = _adjustmentNoteRepository.Query(r => adjustNoteIds.Contains(r.Id)).Select().ToList();
                var adjustNoteDetailIds = voucherDetailVMList.Select(r => r.AdjustmentNoteDetailId);
                var adjustNoteDetailDbList = _adjustmentNoteDetailRepository.Query(r => adjustNoteDetailIds.Contains(r.Id)).Select().ToList();


                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var adjustNoteDetail = adjustNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdjustmentNoteDetailId);
                    if (null == adjustNoteDetail)
                        throw new CustomException("Invoice not found!");

                    adjustNoteDetail.WrittenOffAmount += voucherDetailVM.Amount;

                    if (adjustNoteDetail.Amount < adjustNoteDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    adjustNoteDetail.IsWrittenOff = adjustNoteDetail.Amount == adjustNoteDetail.WrittenOffAmount;
                    adjustNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    adjustNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    adjustNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteDetailRepository.Update(adjustNoteDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                    invoice.WrittenOffAmount += voucherDetailVM.Amount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteRepository.Update(invoice);

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
                        Amount = voucherDetailVM.Amount,
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

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherDetailVM.EntityId,
                        DrAmount = voucherDetailVM.Amount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                    voucherDetailTempId = voucherDetailDr.Id;
                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount;

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
                        totalCurrencyAmountDr += voucherDetailVM.ExchangeAmount;
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
                        totalCurrencyAmountDr -= voucherDetailVM.ExchangeAmount;
                    }
                }

                decimal totalCharges = 0;
                //if (null != bankChargeDetailVMList && bankChargeDetailVMList.Count() > 0)
                //{
                //    var currentBankChargeDetailId = 0;
                //    foreach (var bankChargeDetailVM in bankChargeDetailVMList)
                //    {
                //        currentBankChargeDetailId++;
                //        var bankCharge = _bankChargeService.InsertBankCharge(new BankCharge
                //        {
                //            InvoiceWriteOffId = invoiceWriteOff.Id,
                //            BankMasterId = invoiceWriteOff.BankMasterId,
                //            CashMasterId = invoiceWriteOff.CashMasterId,
                //            FinancingTypeId = bankChargeDetailVM.FinancingTypeId,
                //            SourceType = invoiceWriteOff.SourceType,
                //            Narration = voucher.Narration,
                //            Archive = invoiceWriteOff.Archive,
                //            Amount = bankChargeDetailVM.Amount,
                //            AddedBy = invoiceWriteOff.AddedBy,
                //            AddedDate = invoiceWriteOff.AddedDate,
                //            AddedFromIP = invoiceWriteOff.AddedFromIP
                //        }, currentBankChargeDetailId);

                //        // Get Expense GL
                //        var expenseGL = _bankChargeService.GetExpensesGL(voucher.CompanyId, bankChargeDetailVM.FinancingTypeId);

                //        // Insert Bank charges Debit
                //        currentVoucherDetailId++;
                //        var voucherDetailChargeDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                //        {
                //            BankChargeId = bankCharge.Id,
                //            DrAmount = bankCharge.Amount,
                //            Narration = bankCharge.Narration,
                //            GLGeneralInfoId = expenseGL.ExpensesGLId,
                //            BudgetMasterId = expenseGL.ExpensesBudgetMasterId,
                //            ActivityId = expenseGL.ExpensesActivityId
                //        }, currentVoucherDetailId);
                //        totalCharges += bankCharge.Amount;

                //        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeDr, new VoucherDetailCurrency
                //        {
                //            ParallelCurrencyId = companyCurrencyId,
                //            FromCurrencyId = voucherDetailChargeDr.CurrencyId,
                //            ToCurrencyId = companyCurrencyId,
                //            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                //            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                //            DrAmount = bankChargeDetailVM.CompanyCurrencyAmount
                //        });
                //        totalAmountDr += voucherDetailChargeDr.DrAmount;
                //        totalCurrencyAmountDr += bankChargeDetailVM.CompanyCurrencyAmount;
                //    }
                //}

                //if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                //{
                //    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                //    {
                //        var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                //        foreach (var invoiceTaxVM in taxDetailVMList)
                //        {
                //            var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                //            if (null == taxCode)
                //                throw new CustomException("Tax code not found!");

                //            var taxCodeGL = _accountsCommonService.GetTaxCodeGL(taxCode.Id);
                //            if (null == taxCodeGL)
                //                throw new CustomException("Tax code GL not found!");

                //            var invoiceTax = new InvoiceTax
                //            {
                //                VoucherDetailId = voucherDetailTempId,
                //                TaxCodeId = invoiceTaxVM.TaxCodeId,
                //                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                //                TaxAmount = invoiceTaxVM.TaxAmount,
                //                TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                //            };
                //            totalAmountCr += invoiceTaxVM.TaxAmount;
                //            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                //            // Insert Into Customer Invoice Tax Detail (Withhold GL)
                //            withholdgl = taxCode.IsWithhold;
                //            if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL["WithholdCreditableGLId"].ToString()))
                //            {
                //                var invoiceTaxDetail = new InvoiceTaxDetail
                //                {
                //                    GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                //                    BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                //                    ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                //                    Amount = invoiceTax.TaxAmount,
                //                    AType = "Cr"
                //                };
                //                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                //                var voucherDetailTax = new VoucherDetail
                //                {
                //                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                //                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                //                    ActivityId = invoiceTaxDetail.ActivityId,
                //                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                //                    CrAmount = invoiceTaxDetail.Amount,
                //                };
                //                currentVoucherDetailId++;
                //                _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                //                var voucherDetailCurrencyTax = new VoucherDetailCurrency
                //                {
                //                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                //                    ToCurrencyId = companyCurrencyId,
                //                    ParallelCurrencyId = companyCurrencyId,
                //                    FromCurrencyId = companyCurrencyId,
                //                    CrAmount = totalCurrencyAmountDr,/*voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,*/
                //                    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                //                };
                //                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                //            }
                //        }
                //    }
                //}
                if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                {
                    if (null != tdsVMList && tdsVMList.Count() > 0)
                    {
                        var tdstax = new AdditionalTax
                        {

                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            //TaxAmount = tdsVMList.Sum(r => r.TaxAmount),
                            TaxAmount = voucherDetailVMList.Sum(r => r.Amount),
                            TaxAutoAmount = tdsVMList.Sum(r => r.TaxAutoAmount),
                            InventoryReceiveId = null,
                            InvoiceId = null,
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            EmployeePayableId = null,
                            PartyId = invoiceWriteOff.PartyId,
                            PartyPlantId = invoiceWriteOff.PartyPlantId,
                            Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
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

                            var taxCodeGL = _accountsCommonService.GetTaxCodeGL(taxCode.Id);

                            addtionalTaxDetailId++;
                            var tdsDetail = new AdditionalTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                                BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                                ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                                Amount = voucherDetailVMList.Sum(r => r.Amount),
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
                            var invoiceTax = new InvoiceTax
                            {
                                VoucherDetailId = voucherDetailTempId,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = taxCode.TaxCategoryId,
                                TaxAmount = voucherDetailVMList.Sum(r => r.Amount),
                                TaxAutoAmount = 0,
                                VoucherId = voucher.Id
                            };
                            totalAmountCr += invoiceTax.TaxAmount;
                            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = tdsDetail.GLGeneralInfoId,
                                BudgetMasterId = tdsDetail.BudgetMasterId,
                                ActivityId = tdsDetail.ActivityId,
                                Amount = tdsDetail.Amount,
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
                            totalCurrencyAmountCr += voucherDetailCurrencyTax.CrAmount;
                        }
                    }
                }

                if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                    {
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = voucherDetailVMList.Sum(r => r.Amount) + totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };


                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                        voucherDetailCr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = bankMaster["ActivityId"].ToString();
                        voucherDetailCr.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailCr.PartyType = PartyType.Bank.ToString();
                        if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                        else
                            glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;



                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = totalCurrencyAmountDr
                        });
                    }
                    else
                        throw new CustomException("Bank Id not found!");
                }

                if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                    {
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = voucherDetailVMList.Sum(r => r.Amount) + totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        var cashMaster = _accountsCommonService.GetCashMaster(voucherVM.CashMasterId);
                        voucherDetailCr.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = cashMaster["ActivityId"].ToString();
                        voucherDetailCr.CashMasterId = cashMaster["Id"].ToString();
                        voucherDetailCr.PartyType = PartyType.Cash.ToString();
                        if (cashMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                        else
                            glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = totalCurrencyAmountDr
                        });
                    }
                    else
                        throw new CustomException("Bank or Cash Id not found!");
                }
                if (voucherVM.PaymentSource == PaymentSource.Reverse.ToString())
                {
                    var voucherDetailCr = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        CrAmount = voucherDetailVMList.Sum(r => r.Amount) + totalCharges,
                        PaymentSource = invoiceWriteOff.PaymentSource
                    };
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;

                    //var glTransactionDetail = new GLTransactionDetail
                    //{
                    //    SourceType = voucherDetailCr.PaymentSource,
                    //    BankMasterId = voucherVM.BankMasterId,
                    //    CashMasterId = voucherVM.CashMasterId
                    //};

                    var reverseGL = _accountsCommonService.GetReverseGL(voucherIds);
                    voucherDetailCr.GLGeneralInfoId = reverseGL["GLGeneralInfoId"].ToString();
                    voucherDetailCr.BudgetMasterId = reverseGL["BudgetMasterId"].ToString();
                    voucherDetailCr.ActivityId = reverseGL["ActivityId"].ToString();
                    voucherDetailCr.PartyType = "Reverse";
                    //if (cashMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                    //    glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                    //else
                    //    glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                    // INSRT INTO GLTransactionDetail

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    //_voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = totalCurrencyAmountDr
                    });
                }

                if (!string.IsNullOrEmpty(invoiceWriteOff.RoundingType))
                {
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString() || invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(invoiceWriteOff.CompanyId);
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountDr += voucherDetailRoundingDr.DrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.DrAmount
                            });
                        }
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountCr += voucherDetailRoundingDr.CrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.CrAmount
                            });
                        }
                    }
                }
                totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
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
        public string InsertCreditNoteSetOffDifferentCurrency(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<InvoiceTaxViewModel> tdsVMList)
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
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                //var withholdgl = false;
                var voucherIds = voucherDetailVMList.FirstOrDefault().VoucherId;
                var adjustNoteIds = voucherDetailVMList.Select(r => r.AdjustmentNoteId);
                var adjustNoteDbList = _adjustmentNoteRepository.Query(r => adjustNoteIds.Contains(r.Id)).Select().ToList();
                var adjustNoteDetailIds = voucherDetailVMList.Select(r => r.AdjustmentNoteDetailId);
                var adjustNoteDetailDbList = _adjustmentNoteDetailRepository.Query(r => adjustNoteDetailIds.Contains(r.Id)).Select().ToList();


                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var adjustNoteDetail = adjustNoteDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdjustmentNoteDetailId);
                    if (null == adjustNoteDetail)
                        throw new CustomException("Invoice not found!");

                    adjustNoteDetail.WrittenOffAmount += voucherDetailVM.Amount;

                    if (adjustNoteDetail.Amount < adjustNoteDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    adjustNoteDetail.IsWrittenOff = adjustNoteDetail.Amount == adjustNoteDetail.WrittenOffAmount;
                    adjustNoteDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    adjustNoteDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    adjustNoteDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteDetailRepository.Update(adjustNoteDetail);

                    // TODO: have a gap here if invoice split
                    var invoice = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                    invoice.WrittenOffAmount += voucherDetailVM.Amount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteRepository.Update(invoice);

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
                        Amount = voucherDetailVM.ConvertedAmount,
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

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherDetailVM.EntityId,
                        DrAmount = voucherDetailVM.ConvertedAmount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                    voucherDetailTempId = voucherDetailDr.Id;
                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailVM.Amount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += voucherDetailVM.CompanyCurrencyRate * voucherDetailVM.Amount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount;

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
                        totalCurrencyAmountDr += voucherDetailVM.ExchangeAmount;
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
                        totalCurrencyAmountDr -= voucherDetailVM.ExchangeAmount;
                    }
                }

                decimal totalCharges = 0;

                if (voucherVM.PaymentSource == PaymentSource.Tax.ToString())
                {
                    if (null != tdsVMList && tdsVMList.Count() > 0)
                    {
                        var tdstax = new AdditionalTax
                        {

                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            //TaxAmount = tdsVMList.Sum(r => r.TaxAmount),
                            TaxAmount = voucherDetailVMList.Sum(r => r.Amount),
                            TaxAutoAmount = tdsVMList.Sum(r => r.TaxAutoAmount),
                            InventoryReceiveId = null,
                            InvoiceId = null,
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            EmployeePayableId = null,
                            PartyId = invoiceWriteOff.PartyId,
                            PartyPlantId = invoiceWriteOff.PartyPlantId,
                            Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
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

                            var taxCodeGL = _accountsCommonService.GetTaxCodeGL(taxCode.Id);

                            addtionalTaxDetailId++;
                            var tdsDetail = new AdditionalTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                                BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                                ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                                Amount = voucherDetailVMList.Sum(r => r.Amount),
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
                            var invoiceTax = new InvoiceTax
                            {
                                VoucherDetailId = voucherDetailTempId,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = taxCode.TaxCategoryId,
                                TaxAmount = voucherDetailVMList.Sum(r => r.Amount),
                                TaxAutoAmount = 0,
                                VoucherId = voucher.Id
                            };
                            totalAmountCr += invoiceTax.TaxAmount;
                            _invoiceTaxService.InsertInvoiceTax(invoiceWriteOff, invoiceTax, invoiceTaxPk);

                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = tdsDetail.GLGeneralInfoId,
                                BudgetMasterId = tdsDetail.BudgetMasterId,
                                ActivityId = tdsDetail.ActivityId,
                                Amount = tdsDetail.Amount,
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
                            totalCurrencyAmountCr += voucherDetailCurrencyTax.CrAmount;
                        }
                    }
                }

                if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                    {
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = voucherDetailVMList.Sum(r => r.ConvertedAmount) + totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };


                        var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                        voucherDetailCr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = bankMaster["ActivityId"].ToString();
                        voucherDetailCr.BankMasterId = bankMaster["Id"].ToString();
                        voucherDetailCr.PartyType = PartyType.Bank.ToString();
                        if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.CrAmount = voucherVM.BankAmount + totalCharges;
                        else
                            glTransactionDetail.CrAmount = voucherVM.BankAmount + totalCharges;



                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = totalCurrencyAmountDr
                        });
                    }
                    else
                        throw new CustomException("Bank Id not found!");
                }

                if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                    {
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = voucherDetailVMList.Sum(r => r.Amount) + totalCharges,
                            PaymentSource = invoiceWriteOff.PaymentSource
                        };
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                            voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                            voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        var cashMaster = _accountsCommonService.GetCashMaster(voucherVM.CashMasterId);
                        voucherDetailCr.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = cashMaster["ActivityId"].ToString();
                        voucherDetailCr.CashMasterId = cashMaster["Id"].ToString();
                        voucherDetailCr.PartyType = PartyType.Cash.ToString();
                        if (cashMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                            glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                        else
                            glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = totalCurrencyAmountDr
                        });
                    }
                    else
                        throw new CustomException("Bank or Cash Id not found!");
                }
                if (voucherVM.PaymentSource == PaymentSource.Reverse.ToString())
                {
                    var voucherDetailCr = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        CrAmount = voucherDetailVMList.Sum(r => r.Amount) + totalCharges,
                        PaymentSource = invoiceWriteOff.PaymentSource
                    };
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailCr.CrAmount -= voucherVM.RoundingAmount;
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailCr.CrAmount += voucherVM.RoundingAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;

                    //var glTransactionDetail = new GLTransactionDetail
                    //{
                    //    SourceType = voucherDetailCr.PaymentSource,
                    //    BankMasterId = voucherVM.BankMasterId,
                    //    CashMasterId = voucherVM.CashMasterId
                    //};

                    var reverseGL = _accountsCommonService.GetReverseGL(voucherIds);
                    voucherDetailCr.GLGeneralInfoId = reverseGL["GLGeneralInfoId"].ToString();
                    voucherDetailCr.BudgetMasterId = reverseGL["BudgetMasterId"].ToString();
                    voucherDetailCr.ActivityId = reverseGL["ActivityId"].ToString();
                    voucherDetailCr.PartyType = "Reverse";
                    //if (cashMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                    //    glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                    //else
                    //    glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                    // INSRT INTO GLTransactionDetail

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    //_voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyCr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = totalCurrencyAmountDr
                    });
                }

                if (!string.IsNullOrEmpty(invoiceWriteOff.RoundingType))
                {
                    if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString() || invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(invoiceWriteOff.CompanyId);
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountDr += voucherDetailRoundingDr.DrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.DrAmount
                            });
                        }
                        if (invoiceWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = invoiceWriteOff.RoundingAmount,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = invoiceWriteOff.Narration,
                                PartyType = invoiceWriteOff.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountCr += voucherDetailRoundingDr.CrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailRoundingDr.CrAmount
                            });
                        }
                    }
                }
                totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
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
        public string InsertCreditNoteInvoiceSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
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
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                //string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                //var withholdgl = false;
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
                    var invoice = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                    invoice.WrittenOffAmount += voucherVM.Amount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteRepository.Update(invoice);

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

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherDetailVM.EntityId,
                        DrAmount = voucherVM.Amount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount;

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
                        totalCurrencyAmountDr += voucherDetailVM.ExchangeAmount;
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
                        totalCurrencyAmountDr -= voucherDetailVM.ExchangeAmount;
                    }
                }



                if (voucherVM.PaymentSource == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO InvoiceWriteOff


                    // Invoice
                    var invoiceIds = voucherDetailInvoiceList.Select(r => r.InvoiceId);
                    var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = voucherDetailInvoiceList.Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                    foreach (var voucherDetailVM in voucherDetailInvoiceList)
                    {
                        var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");

                        invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;
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
                        currentInvoiceWriteOffDetailId++;
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
                            Amount = voucherDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP,
                            Archive = invoiceWriteOff.Archive,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration
                        };
                        InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);
                        invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                        // INSERT INTO VoucherDetail
                        var voucherDetailCr = new VoucherDetail
                        {
                            VoucherId = voucher.Id,
                            InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = voucherDetailVM.Amount,
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
                            CrAmount = voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate,
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


                totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
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

        public string InsertVendorCreditNoteSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
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
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set Voucher Id to Advance
                invoiceWriteOff.VoucherId = voucher.Id;

                var currentVoucherDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;

                var totalAmountDr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                //string voucherDetailTempId = null;
                decimal taxDrAmount = 0;
                //var withholdgl = false;
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
                    var invoice = adjustNoteDbList.First(r => r.Id == adjustNoteDetail.AdjustmentNoteId);
                    invoice.WrittenOffAmount += voucherVM.Amount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _adjustmentNoteRepository.Update(invoice);

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

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        EntityId = voucherDetailVM.EntityId,
                        DrAmount = voucherVM.Amount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyId = invoiceWriteOff.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = invoiceWriteOff.PartyType,
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    var voucherDetailCurrencyDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                    });

                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalCurrencyAmountDr += voucherDetailVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;
                    totalCurrencyAmountCr += voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount;

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
                        totalCurrencyAmountDr += voucherDetailVM.ExchangeAmount;
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
                        totalCurrencyAmountDr -= voucherDetailVM.ExchangeAmount;
                    }
                }



                if (voucherVM.PaymentSource == SettlementType.SetOff.ToString())
                {
                    voucherVM.VoucherId = voucher.Id;
                    voucherVM.VoucherTypeId = voucher.VoucherTypeId;
                    voucherVM.DocRefNo = voucher.DocRefNo;
                    voucherVM.PostingDate = voucher.PostingDate;
                    voucherVM.AddedDate = voucher.AddedDate;
                    voucherVM.AddedFromIP = voucher.AddedFromIP;
                    voucherVM.AddedBy = voucher.AddedBy;
                    var additionalInvoice = InsertAdditionalInvoice(voucherVM);


                    // Invoice
                    var invoiceIds = voucherDetailInvoiceList.Select(r => r.InvoiceId);
                    var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = voucherDetailInvoiceList.Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                    foreach (var voucherDetailVM in voucherDetailInvoiceList)
                    {
                        var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");

                        invoiceDetail.AdditionalAmount += voucherDetailVM.Amount;

                        invoiceDetail.UpdatedBy = additionalInvoice.AddedBy;
                        invoiceDetail.UpdatedDate = additionalInvoice.AddedDate;
                        invoiceDetail.UpdatedFromIP = additionalInvoice.AddedFromIP;
                        _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                        var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                        invoice.AdditionalAmount = invoiceDetail.AdditionalAmount;
                        invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.Update(invoice);

                        // INSERT INTO InvoiceWriteOffDetail
                        currentInvoiceWriteOffDetailId++;
                        var additionalInvoiceDetail = new AdditionalInvoiceDetail
                        {
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            AdditionalInvoiceId = additionalInvoice.Id,
                            InvoiceId = voucherDetailVM.InvoiceId,
                            InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                            Amount = voucherDetailVM.Amount,
                            AddedBy = invoiceWriteOff.AddedBy,
                            AddedDate = invoiceWriteOff.AddedDate,
                            AddedFromIP = invoiceWriteOff.AddedFromIP,
                        };
                        InsertAdditionalInvoiceDetail(additionalInvoice, additionalInvoiceDetail, currentInvoiceWriteOffDetailId);

                        // INSERT INTO VoucherDetail
                        var voucherDetailCr = new VoucherDetail
                        {
                            VoucherId = voucher.Id,
                            AdditionalInvoiceDetailId = additionalInvoiceDetail.Id,
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = voucherDetailVM.Amount,
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
                            CrAmount = voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate,
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


                totalCurrencyAmountCr = totalCurrencyAmountDr;
                totalAmountCr += taxDrAmount;
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
        public void DeleteAdjustmentNoteWriteOff(string invoiceWriteOffId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var voucherdetail = _voucherService.QueryVoucherDetail(voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherService.QueryVoucherDetailCurrency(voucherId).Select().ToList();
                var invoiceWriteOff = _invoiceWriteOffRepository.Find(invoiceWriteOffId);
                var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWriteOffId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var advanceWriteOff = _advanceWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();


                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _voucherService.QueryGLTransactionDetail(item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _voucherService.DeleteGLTransactionDetail(item.Id);
                    }
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"UPDATE [TRN].InvoiceTax SET VoucherDetailId=NULL WHERE Id='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                        var invoicetaxDdetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == item.Id).Select().ToList();
                        foreach (var item1 in invoicetaxDdetail)
                        {
                            _invoiceTaxDetailRepository.Delete(item1.Id);
                        }
                        _invoiceTaxRepository.Delete(item.Id);
                    }
                }
                foreach (var item in invoiceWriteOffDetail)
                {
                    if (item.InvoiceId != null)
                    {
                        var invoice = _invoiceService.Find(item.InvoiceId);
                        var invoiceDetail = _invoiceService.FindInvoiceDetail(item.InvoiceDetailId);
                        invoiceDetail.WrittenOffAmount -= item.Amount;
                        invoice.WrittenOffAmount -= item.Amount;
                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;

                        _invoiceService.UpdateInvoiceDetail(invoiceDetail);
                        _invoiceService.Update(invoice);
                    }
                    if (item.AdjustmentNoteId != null)
                    {
                        var adjustmentNote = _adjustmentNoteRepository.Find(item.AdjustmentNoteId);
                        var adjustmentNoteDetail = _adjustmentNoteDetailRepository.Find(item.AdjustmentNoteDetailId);
                        adjustmentNoteDetail.WrittenOffAmount -= item.Amount;
                        adjustmentNote.WrittenOffAmount -= item.Amount;
                        adjustmentNoteDetail.IsWrittenOff = adjustmentNoteDetail.Amount == adjustmentNoteDetail.WrittenOffAmount;
                        adjustmentNote.IsWrittenOff = adjustmentNote.Amount == adjustmentNote.WrittenOffAmount;

                        _adjustmentNoteDetailRepository.Update(adjustmentNoteDetail);
                        _adjustmentNoteRepository.Update(adjustmentNote);
                    }

                    _invoiceWriteOffDetailRepository.Delete(item.Id);
                }
                if (advanceWriteOff != null)
                {
                    var advanceWriteOffDetail = _advanceWriteOffDetailRepository.Query(r => r.AdvanceWriteOffId == advanceWriteOff.Id).Select().ToList();
                    if (advanceWriteOffDetail != null)
                    {
                        foreach (var item in advanceWriteOffDetail)
                        {
                            var advance = _advanceRepository.Find(item.AdvanceId);
                            var advanceDetail = _advanceDetailRepository.Find(item.AdvanceDetailId);

                            advanceDetail.WrittenOffAmount -= item.Amount;
                            advance.WrittenOffAmount -= item.Amount;
                            advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                            advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;

                            _advanceDetailRepository.Update(advanceDetail);
                            _advanceRepository.Update(advance);
                            _advanceWriteOffDetailRepository.Delete(item.Id);
                        }
                        _advanceWriteOffRepository.Delete(advanceWriteOff.Id);
                    }

                }
                _invoiceWriteOffRepository.Delete(invoiceWriteOffId);
                _voucherService.DeleteVoucher(voucher.Id);
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

        public void DeleteWriteOff(string invoiceWriteOffId, string voucherId, string deletedRemarks)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.InsertVoucherLogDeleted(voucherId, voucher.VoucherNo, "", "", "", invoiceWriteOffId, "", "", "", "", "", "", "", deletedRemarks);

                var voucherdetail = _voucherService.QueryVoucherDetail(voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherService.QueryVoucherDetailCurrency(voucherId).Select().ToList();
                var invoiceWriteOff = _invoiceWriteOffRepository.Find(invoiceWriteOffId);
                var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWriteOffId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var invoicetds = _additionalTaxRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var adjustmentNote = _adjustmentNoteRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                var advancewriteOff = _advanceWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }
                var bankCharges = _bankChargeRepository.Query(r => r.InvoiceWriteOffId == invoiceWriteOffId).Select().ToList();

                if (bankCharges != null)
                {
                    foreach (var item in bankCharges)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"UPDATE [TRN].VoucherDetail SET BankChargeId=NULL WHERE BankChargeId='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        _bankChargeRepository.Delete(item.Id);
                    }
                }
                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _voucherService.QueryGLTransactionDetail(item.Id).Select().FirstOrDefault();
                    var employeeSubsequentTran = _employeeSubsequentTransactionRepository.Query(r => r.VoucherDetailId == item.Id).Select().FirstOrDefault();

                    if (glTransactionDetail != null)
                    {
                        _voucherService.DeleteGLTransactionDetail(item.Id);
                    }
                    if (employeeSubsequentTran!=null)
                    {
                        _employeeSubsequentTransactionRepository.Delete(employeeSubsequentTran.Id);

                    }
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"UPDATE [TRN].InvoiceTax SET VoucherDetailId=NULL WHERE Id='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                        var invoicetaxDdetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == item.Id).Select().ToList();
                        foreach (var item1 in invoicetaxDdetail)
                        {
                            _invoiceTaxDetailRepository.Delete(item1.Id);
                        }
                        _invoiceTaxRepository.Delete(item.Id);
                    }
                }
                if (invoicetds != null)
                {

                    foreach (var tds in invoicetds)
                    {
                        if (tds.InvoiceWriteOffId == null && tds.InvoiceId != null)
                        {
                            var rdBuildertds = new System.Text.StringBuilder();
                            var builderSql = @"UPDATE [TRN].AdditionalTax SET VoucherId=NULL WHERE Id='" + tds.Id + "'";
                            rdBuildertds.Append(builderSql);
                            _sqlRepository.ExecuteSqlCommand(rdBuildertds.ToString());
                        }
                        if (tds.InvoiceWriteOffId != null && tds.InvoiceId == null)
                        {
                            var tdsdetail = _additionalTaxDetailRepository.Query(r => r.AdditionalTaxId == tds.Id).Select().ToList();
                            foreach (var item in tdsdetail)
                            {
                                _additionalTaxDetailRepository.Delete(item);
                            }
                            _additionalTaxRepository.Delete(tds);
                        }

                    }


                }
                foreach (var item in invoiceWriteOffDetail)
                {

                    var invoice = _invoiceService.Find(item.InvoiceId);
                    var invoiceDetail = _invoiceService.FindInvoiceDetail(item.InvoiceDetailId);
                    invoiceDetail.WrittenOffAmount -= item.Amount;
                    invoice.WrittenOffAmount -= item.Amount;
                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                    invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;

                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);
                    _invoiceService.Update(invoice);

                    _invoiceWriteOffDetailRepository.Delete(item.Id);
                }

                _invoiceWriteOffRepository.Delete(invoiceWriteOffId);

                if(advancewriteOff != null)
                {
                    var advanceWriteOffDetail = _advanceWriteOffDetailRepository.Query(r => r.AdvanceWriteOffId == advancewriteOff.Id).Select().ToList();
                    foreach (var item in advanceWriteOffDetail)
                    {
                        var advance = _advanceRepository.Find(item.AdvanceId);
                        var advanceDetail = _advanceDetailRepository.Find(item.AdvanceDetailId);
                        advanceDetail.WrittenOffAmount -= item.Amount;
                        advance.WrittenOffAmount -= item.Amount;
                        advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                        advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;

                        _advanceDetailRepository.Update(advanceDetail);
                        _advanceRepository.Update(advance);

                        _advanceWriteOffDetailRepository.Delete(item.Id);
                    }
                    _advanceWriteOffRepository.Delete(advancewriteOff.Id);
                }
                if (adjustmentNote != null)
                {
                    var adjustmentNoteDetail = _adjustmentNoteDetailRepository.Query(r => r.AdjustmentNoteId == adjustmentNote.Id).Select().ToList();
                    foreach (var item in adjustmentNoteDetail)
                    {
                        _adjustmentNoteDetailRepository.Delete(item.Id);
                    }
                    _adjustmentNoteRepository.Delete(adjustmentNote.Id);
                }
                _voucherService.DeleteVoucher(voucher.Id);
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

      
        public void DeleteInvoiceToAcceptance(string invoiceWriteOffId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var voucherdetail = _voucherService.QueryVoucherDetail(voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherService.QueryVoucherDetailCurrency(voucherId).Select().ToList();
                var invoiceWriteOff = _invoiceWriteOffRepository.Find(invoiceWriteOffId);
                var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWriteOffId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var invoicetds = _additionalTaxRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                //foreach (var item in voucherdetailcurrnecy)
                //{
                //    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                //}

                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId= '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId= '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                //foreach (var item in voucherdetail)
                //{
                //    var glTransactionDetail = _voucherService.QueryGLTransactionDetail(item.Id).Select().FirstOrDefault();
                //    if (glTransactionDetail != null)
                //    {
                //        _voucherService.DeleteGLTransactionDetail(item.Id);
                //    }
                //    _voucherService.DeleteVoucherDetail(item.Id);
                //}

                foreach (var item in invoiceWriteOffDetail)
                {
                    vendorAdWrsql = @"update TRN.InvoiceDetail set WrittenOffAmount=0,IsWrittenOff=0 where InvoiceId ='" + item.InvoiceId + "'";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"update TRN.Invoice set WrittenOffAmount=0,IsWrittenOff=0 where Id ='" + item.InvoiceId + "'";
                    vendorAdWr.Append(vendorAdWrsql);
                    vendorAdWrsql = @"delete from TRN.InvoiceWriteOffDetail where Id='" + item.Id + "'";
                    vendorAdWr.Append(vendorAdWrsql);
                }
                vendorAdWrsql = @"delete from TRN.InvoiceWriteOff where Id='" + invoiceWriteOffId + "'";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete from TRN.InvoiceDetail where InvoiceId in (select Id from TRN.Invoice  where VoucherId = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.Invoice  where VoucherId = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.voucher  where Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);

                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());

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

        public void DeleteCustomerBanksReceipt(string invoiceWriteOffGroupNo, SourceType sourceType)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var invoiceWriteOffList = _invoiceWriteOffRepository.Query(r => r.InvoiceWriteOffGroupNo == invoiceWriteOffGroupNo && r.SourceType == sourceType.ToString()).Select().ToList();
                if (invoiceWriteOffList != null)
                {
                    foreach (var invwriteOff in invoiceWriteOffList)
                    {
                        var voucher = _voucherService.FindVoucher(invwriteOff.VoucherId);
                        if (voucher.IsPark == false)
                            throw new CustomException("Delete is not allow after post ! ");

                        var bankCharges = _bankChargeRepository.Query(r => r.InvoiceWriteOffId == invwriteOff.Id).Select().ToList();
                        var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invwriteOff.Id).Select().ToList();
                        var invoiceTax = _invoiceTaxRepository.Query(r => r.VoucherId == invwriteOff.VoucherId).Select().ToList();
                        var invoicetds = _additionalTaxRepository.Query(r => r.VoucherId == invwriteOff.VoucherId).Select().ToList();
                        var adjustmentNote = _adjustmentNoteRepository.Query(r => r.VoucherId == invwriteOff.VoucherId).Select().FirstOrDefault();
                        var financingWriteOff = _financingWriteOffRepository.Query(r => r.VoucherId == invwriteOff.VoucherId).Select().FirstOrDefault();
                        var laonIntPayable = _loanInterestPayableRepository.Query(r => r.VoucherId == invwriteOff.VoucherId).Select().FirstOrDefault();
                        var vendorAdWr = new System.Text.StringBuilder();
                        var vendorAdWrsql = "";
                        if (financingWriteOff != null)
                        {
                            vendorAdWrsql = @"declare @writeOffAmount decimal(18,2)=(select Amount from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where  VoucherId = '" + invwriteOff.VoucherId + "'))";
                            vendorAdWr.Append(vendorAdWrsql);

                            vendorAdWrsql = @"update TRN.Financing set WrittenOffAmount=(WrittenOffAmount - @writeOffAmount),IsWrittenOff=case when (WrittenOffAmount-@writeOffAmount) =0 then 1 else 0 end
                                where Id in (select FinancingId from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where  VoucherId = '" + invwriteOff.VoucherId + "'))";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"update TRN.FinancingDetail set WrittenOffAmount=(WrittenOffAmount - @writeOffAmount)
                                where Id in (select FinancingDetailId from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where  VoucherId = '" + invwriteOff.VoucherId + "'))";
                            vendorAdWr.Append(vendorAdWrsql);
                        }
                        if (laonIntPayable != null)
                        {
                            vendorAdWrsql = @"delete from TRN.FinancingSubsequentTransaction where VoucherId  = '" + invwriteOff.VoucherId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                        }
                        vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId  = '" + invwriteOff.VoucherId + "')";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId  = '" + invwriteOff.VoucherId + "'";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"update trn.VoucherDetail SET BankChargeId=NULL where VoucherId  = '" + invwriteOff.VoucherId + "'";
                        vendorAdWr.Append(vendorAdWrsql);
                        vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId  = '" + invwriteOff.VoucherId + "'";
                        vendorAdWr.Append(vendorAdWrsql);
                        if (bankCharges.Count > 0)
                        {
                            vendorAdWrsql = @"delete TRN.BankCharge where InvoiceWriteOffId  = '" + invwriteOff.Id + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                        }
                        if (financingWriteOff != null)
                        {
                            vendorAdWrsql = @"delete from TRN.FinancingDetailWriteOff where FinancingWriteOffId in (select Id from TRN.FinancingWriteOff where VoucherId  = '" + invwriteOff.VoucherId + "')";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"delete from TRN.FinancingWriteOff where VoucherId  = '" + invwriteOff.VoucherId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                        }
                        if (invoiceTax.Count > 0)
                        {
                            vendorAdWrsql = @"update [TRN].InvoiceTax SET VoucherDetailId=NULL where VoucherId  = '" + invwriteOff.VoucherId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"delete from [TRN].InvoiceTaxDetail where InvoiceTaxId in (select Id from TRN.InvoiceTax where VoucherId  = '" + invwriteOff.VoucherId + "')";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"delete from [TRN].InvoiceTax  where VoucherId  = '" + invwriteOff.VoucherId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                        }
                        if (invoicetds.Count > 0)
                        {
                            vendorAdWrsql = @"update [TRN].AdditionalTax SET VoucherId=NULL where VoucherId  = '" + invwriteOff.VoucherId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"delete from [TRN].AdditionalTaxDetail where AdditionalTaxId in (select Id from [TRN].AdditionalTax where VoucherId  = '" + invwriteOff.VoucherId + "')";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"delete from [TRN].AdditionalTax  where VoucherId  = '" + invwriteOff.VoucherId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                        }
                        foreach (var item in invoiceWriteOffDetail)
                        {
                            vendorAdWrsql = @"update [TRN].InvoiceDetail SET WrittenOffAmount=(WrittenOffAmount - " + item.Amount + ") ,IsWrittenOff=0 where Id  = '" + item.InvoiceDetailId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"update [TRN].Invoice SET WrittenOffAmount=(WrittenOffAmount - " + item.Amount + ") ,IsWrittenOff=0 where Id  = '" + item.InvoiceId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"delete from [TRN].InvoiceWriteOffDetail  where Id  = '" + item.Id + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                        }
                        vendorAdWrsql = @"delete from [TRN].InvoiceWriteOff  where Id  = '" + invwriteOff.Id + "'";
                        vendorAdWr.Append(vendorAdWrsql);

                        if (adjustmentNote != null)
                        {
                            vendorAdWrsql = @"delete from [TRN].AdjustmentNoteDetail where AdjustmentNoteId in (select Id from [TRN].AdjustmentNote where VoucherId  = '" + invwriteOff.VoucherId + "')";
                            vendorAdWr.Append(vendorAdWrsql);
                            vendorAdWrsql = @"delete from [TRN].AdjustmentNote  where VoucherId  = '" + invwriteOff.VoucherId + "'";
                            vendorAdWr.Append(vendorAdWrsql);
                        }

                        vendorAdWrsql = @"delete trn.voucher  where Id = '" + invwriteOff.VoucherId + "'";
                        vendorAdWr.Append(vendorAdWrsql);
                        _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                        flag = false;
                    }
                }
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

        #region PurchaseLCCharges
        public void PurchaseLCChargesPost(VoucherViewModel voucherVM, IEnumerable<PurchaseLCCharges> voucherRows, IEnumerable<PurchaseLCChargesViewModel> purchaseLCChargesList)
        {
            var flag = false;
            try
            {
                if (voucherRows != null)
                {
                    var purchaseLC = _purchaseLCRepository.Find(voucherRows.Select(r => r.PurchaseLCId).FirstOrDefault());
                    //voucherVM.PostingDate = Convert.ToDateTime(purchaseLC.LCDate);
                    //voucherVM.DocDate = Convert.ToDateTime(purchaseLC.LCDate);
                    voucherVM.DocRefNo = purchaseLC.LCRef;
                    voucherVM.Narration = "Being LC Openning Charges for LC No. " + voucherVM.DocRefNo;

                    AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                    //_accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                    //_accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                    //_accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var currentVoucherDetailId = 0;
                    var currentTaxRecord = 0;
                    decimal totalTaxAmount = 0;
                    decimal totalDrAmount = 0;
                    decimal totalCrAmount = 0;
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                    foreach (var item in voucherRows)
                    {
                        voucherVM.PostingDate = Convert.ToDateTime(item.LCDate);
                        voucherVM.DocDate = Convert.ToDateTime(item.LCDate);
                        voucherVM.CurrencyId = item.CurrencyId;
                        _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                        _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                        _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                        var voucher = _voucherService.InsertVoucher(voucherVM);


                        var chargesList = purchaseLCChargesList.Where(r => r.OpeningBankMasterId == item.OpeningBankMasterId).ToList();
                        var taxDetailVMList = _purchaseLCTaxRepository.Query(r => r.PurchaseLCId == item.PurchaseLCId).Select().ToList();

                        decimal voucherDetailCurrencyCr = 0;

                        foreach (var cList in chargesList)
                        {
                            currentVoucherDetailId++;
                            var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                GLGeneralInfoId = cList.ExpensesGLId,
                                BudgetMasterId = cList.ExpensesBudgetMasterId,
                                ActivityId = cList.ExpensesActivityId,
                                DrAmount = cList.ChargesValue,
                            }, currentVoucherDetailId);
                            totalDrAmount += voucherDetailDr.DrAmount;
                            // INSERT INTO VoucherDetailCurrency
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = cList.Rate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, cList.Rate),
                                DrAmount = cList.Rate * voucherDetailDr.DrAmount
                            });
                            voucherDetailCurrencyCr += voucherDetailDr.DrAmount;

                            var purchaseLCCharges = _purchaseLCChargesRepository.Find(cList.Id);
                            purchaseLCCharges.VoucherId = voucher.Id;
                            purchaseLCCharges.Rate = cList.Rate;
                            _purchaseLCChargesRepository.Update(purchaseLCCharges);

                            if (taxDetailVMList != null)
                            {
                                var invoieTaxVM = taxDetailVMList.Where(r => r.PurchaseLCChargesId == cList.Id).ToList();
                                if (null != invoieTaxVM && invoieTaxVM.Count() > 0)
                                {
                                    //var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                                    foreach (var invoiceTaxVM in invoieTaxVM)
                                    {
                                        var taxCategoryGL = _accountsCommonService.GetTaxCategoryInputGL(invoiceTaxVM.TaxCategoryId);

                                        if (null == taxCategoryGL["ExpensesGLId"].ToString())
                                            throw new CustomException("Tax Category Expenses GL not found!");

                                        currentTaxRecord++;
                                        var invoiceTax = new InvoiceTax
                                        {
                                            Id = MakePK(purchaseLC.Id, currentTaxRecord, 2),
                                            // TaxCodeId = invoiceTaxVM.TaxCodeId,
                                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                            TaxAmount = invoiceTaxVM.TaxAmount,
                                            TaxAutoAmount = invoiceTaxVM.TaxAmount,
                                            PurchaseLCId = invoiceTaxVM.PurchaseLCId,
                                            TaxYearId = voucher.TaxYearId,
                                            VoucherId = voucher.Id,
                                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                                            AddedBy = voucher.AddedBy,
                                            AddedDate = voucher.AddedDate,
                                            AddedFromIP = voucher.AddedFromIP
                                        };
                                        _invoiceTaxRepository.Insert(invoiceTax);
                                        totalTaxAmount += invoiceTax.TaxAmount;

                                        if (!string.IsNullOrEmpty(taxCategoryGL["ExpensesGLId"].ToString()))
                                        {
                                            var invoiceTaxDetail = new InvoiceTaxDetail
                                            {
                                                GLGeneralInfoId = taxCategoryGL["ExpensesGLId"].ToString(),
                                                BudgetMasterId = taxCategoryGL["ExpensesBudgetMasterId"].ToString(),
                                                ActivityId = taxCategoryGL["ExpensesActivityId"].ToString(),
                                                Amount = invoiceTax.TaxAmount,
                                                AType = "Dr"
                                            };
                                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                            var voucherDetailTax = new VoucherDetail
                                            {
                                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                                ActivityId = invoiceTaxDetail.ActivityId,
                                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                                DrAmount = invoiceTaxDetail.Amount,
                                            };
                                            currentVoucherDetailId++;
                                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                                            totalDrAmount += voucherDetailTax.DrAmount;
                                            var voucherDetailCurrencyTax = new VoucherDetailCurrency
                                            {
                                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                                ToCurrencyId = companyCurrencyId,
                                                ParallelCurrencyId = companyCurrencyId,
                                                FromCurrencyId = companyCurrencyId,
                                                DrAmount = voucherDetailTax.DrAmount,//voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                                ToCurrencyConversion = 1 //ToDo:
                                            };
                                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                                        }
                                    }
                                }
                            }
                        }


                        var obBankData = chargesList.Where(r => r.OpeningBankMasterId == item.OpeningBankMasterId).FirstOrDefault();
                        currentVoucherDetailId++;
                        var voucherDetailCr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            GLGeneralInfoId = obBankData.GLGeneralInfoId,
                            BudgetMasterId = obBankData.BudgetMasterId,
                            ActivityId = obBankData.ActivityId,
                            CrAmount = chargesList.Where(r => r.OpeningBankMasterId == item.OpeningBankMasterId).Sum(r => r.ChargesValue) + totalTaxAmount,
                            BankMasterId = obBankData.OpeningBankMasterId

                        }, currentVoucherDetailId);
                        totalCrAmount += voucherDetailCr.CrAmount;
                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = item.Rate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, item.Rate),
                            CrAmount = voucherDetailCurrencyCr + totalTaxAmount
                        });

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = obBankData.OpeningBankMasterId,
                            CashMasterId = voucherVM.CashMasterId,
                            CrAmount = chargesList.Where(r => r.OpeningBankMasterId == item.OpeningBankMasterId).Sum(r => r.BankAmount) + totalTaxAmount
                        };
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        voucherDetailCurrencyCr = 0;
                    }
                    if (totalCrAmount != totalDrAmount)
                        throw new CustomException("Dr Cr Amount not match !.");
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        private DataTable GetGatePaymentAdviceData(string companyGroupId, string companyId, string plantId, string adviceNo)
        {
            var sql = @"SELECT co.UserName SendersName,bm.AccountNumber SendersACNumber,MPD.Amount AS TransactionAmount,P.Code AS BeneficiaryCode, P.UserName AS BeneficiaryName
									,ISNULL(PB.BankAccountNo,NULL) BeneficiaryACNo,ISNULL(PB.IFSCCode,NULL) ReceiverIFSC,I.Narration,AM.Email,0 ChequeNo,0 ChequeDate,MP.Id AdviceNo
									
                                    FROM TRN.MultiplePaymentDetail MPD 
									JOIN TRN.MultiplePayment MP ON MP.Id=MPD.MultiplePaymentId
									LEFT JOIN TRN.Invoice I ON I.Id=MPD.InvoiceId
									LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=I.Id
									LEFT JOIN ORG.Company CO ON CO.Id=I.CompanyId
									left join mst.BankMaster bm on bm.Id=MP.BankMasterId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
									LEFT join HKP.PartyBank PB on PB.CompanyPartyId=P.Id
									left join mst.AddressMaster AM on AM.Id=P.AddressMasterId
                                    WHERE MP.Id='" + adviceNo+"'";
            return _sqlRepository.GetDataTable(sql);
        }

        public IWorkbook PaymentAdviceReportxlx(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string adviceNo)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "PaymentAdviceReport";
            reportFileName = "PaymentAdviceReport";
            try
            {
                var dsLocal = GetGatePaymentAdviceData(companyGroupId, companyId, plantId, adviceNo);
                sheet = workbook.Worksheets[0];

                int ROW = 5; int COL = 1;
                #region columns

                sheet[ROW, COL].Text = "Sender's A/C Number";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColSendersACNumber = COL;
                COL++;

                sheet[ROW, COL].Text = "Sender's  Name";
                sheet[ROW, COL].ColumnWidth = 25;
                int ColSendersName = COL;
                COL++;

                sheet[ROW, COL].Text = "Transaction Amount";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColAmount = COL;
                COL++;


                sheet[ROW, COL].Text = "Beneficiary Name";
                sheet[ROW, COL].ColumnWidth = 40;
                int ColPartyName = COL;
                COL++;

                sheet[ROW, COL].Text = "Beneficiary A/C No";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColBeneficiaryACNo = COL;
                COL++;


                sheet[ROW, COL].Text = "Receiver IFSC";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColReceiverIFSC = COL;
                COL++;

                sheet[ROW, COL].Text = "Narration 1";
                sheet[ROW, COL].ColumnWidth = 40;
                int ColNarration = COL;
                COL++;

                sheet[ROW, COL].Text = "E-Mail";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColEMail = COL;
                COL++;

                sheet[ROW, COL].Text = "Cheque No";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColChequeNo = COL;
                COL++;


                sheet[ROW, COL].Text = "Cheque Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColChequeDate = COL;
                COL++;


                sheet[ROW, COL].Text = "Multiple Payment No";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColMultiplePaymentNo = COL;
                COL++;

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

                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    sheet[ROW, ColSendersACNumber].Text = dsLocal.Rows[i]["SendersACNumber"].ToString();
                    sheet[ROW, ColSendersName].Text = dsLocal.Rows[i]["SendersName"].ToString();
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(dsLocal.Rows[i]["TransactionAmount"].ToString());
                    sheet[ROW, ColAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColPartyName].Text = dsLocal.Rows[i]["BeneficiaryName"].ToString();
                    if (dsLocal.Rows[i]["BeneficiaryACNo"] != null)
                    {
                        sheet[ROW, ColBeneficiaryACNo].Text = dsLocal.Rows[i]["BeneficiaryACNo"].ToString();
                    }
                    if (dsLocal.Rows[i]["ReceiverIFSC"] != null)
                    {
                        sheet[ROW, ColReceiverIFSC].Text = dsLocal.Rows[i]["ReceiverIFSC"].ToString();
                    }
                    sheet[ROW, ColNarration].Text = dsLocal.Rows[i]["Narration"].ToString();
                    if (dsLocal.Rows[i]["Email"] !=null)
                    {
                    sheet[ROW, ColEMail].Text = dsLocal.Rows[i]["Email"].ToString();
                    }
                    sheet[ROW, ColChequeNo].Text = dsLocal.Rows[i]["ChequeNo"].ToString();
                    sheet[ROW, ColChequeDate].Text = dsLocal.Rows[i]["ChequeDate"].ToString();
                    sheet[ROW, ColMultiplePaymentNo].Text = dsLocal.Rows[i]["AdviceNo"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

               
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Payment Advice", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}