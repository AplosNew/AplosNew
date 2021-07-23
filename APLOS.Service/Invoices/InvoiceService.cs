using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.Addresses;
using Library.Model.Banks;
using Library.Model.Commercial;
using Library.Model.Currencies;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Organizations;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Taxations;
using Library.Model.Vouchers;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Organizations;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Currencies;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Library.Service.Invoices
{
    public class InvoiceService : Service<Invoice>, IInvoiceService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MultiplePayment> _multiplePaymentRepository;
        private readonly IRepositoryAsync<MultiplePaymentDetail> _multiplePaymentDetailRepository;
        private readonly IRepositoryAsync<InvoiceDetail> _invoiceDetailRepository;
        private readonly IRepositoryAsync<InvoiceMaterial> _invoiceMaterialRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<InvoiceWriteOff> _invoiceWriteOffRepository;
        private readonly IRepositoryAsync<InvoiceWriteOffDetail> _invoiceWriteOffDetailRepository;
        private readonly IRepositoryAsync<TaxCode> _taxCodeRepository;
        private readonly IRepositoryAsync<TaxCodeGL> _taxCodeGLRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly ICompanyService _companyService;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IHSNTaxPercentageService _hsnTaxPercentageService;
        private readonly TaxCategoryGLService _taxCategoryGlService;
        private readonly IRepositoryAsync<Entity> _entityRepository;
        private readonly IRepositoryAsync<TaxCategory> _taxCategoryRepository;
        private readonly IRepositoryAsync<AddressMaster> _addressMasterRepository;
        private readonly IRepositoryAsync<Party> _partyRepository;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly IVoucherService _voucherService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly IRepositoryAsync<CompanyPartyGL> _companyPartyGLRepository;
        private readonly IRepositoryAsync<CashMaster> _cashMasterRepository;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;
        private readonly IRepositoryAsync<GLTransactionDetail> _gLTransactionDetailRepository;
        private readonly IRepositoryAsync<InvoiceDetailCharges> _invoiceDetailChargesRepository;
        private readonly IRepositoryAsync<InvoiceServiceMasterCharges> _invoiceServiceMasterChargesRepository;
        private readonly IRepositoryAsync<InvoiceServiceMasterChargesDetail> _invoiceServiceMasterChargesDetailRepository;
        private readonly IRepositoryAsync<InvoiceServiceMasterChargesTax> _invoiceServiceMasterChargesTaxRepository;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IRepositoryAsync<AdditionalTax> _additionalTaxRepository;
        private readonly IRepositoryAsync<AdditionalTaxDetail> _additionalTaxDetailRepository;
        private readonly IRepositoryAsync<OtherInvoice> _otherInvoiceRepository;
        public InvoiceService(
              IRepositoryAsync<Invoice> repository
            , IRepositoryAsync<InvoiceDetail> invoiceDetailRepository
            , IRepositoryAsync<InvoiceMaterial> invoiceMaterialRepository
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            , IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository
            , IRepositoryAsync<InvoiceWriteOff> invoiceWriteOffRepository
            , IRepositoryAsync<InvoiceWriteOffDetail> invoiceWriteOffDetailRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<MultiplePayment> multiplePaymentRepository
            , IRepositoryAsync<MultiplePaymentDetail> multiplePaymentDetailRepository
            , ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IRepositoryAsync<TaxCode> taxCodeRepository
            , IRepositoryAsync<TaxCodeGL> taxCodeGLRepository
            , ICompanyService companyService
            , IMaterialMasterService materialMasterService
            , IHSNTaxPercentageService hsnTaxPercentageService
            , TaxCategoryGLService taxCategoryGlService
            , IRepositoryAsync<Entity> entityRepository
            , IRepositoryAsync<TaxCategory> taxCategoryRepository
            , IRepositoryAsync<AddressMaster> addressMasterRepository
            , ICompanyTaxYearService companyTaxYearService
            , IRepositoryAsync<Party> partyRepository
            , IVoucherService voucherService
            , ICompanyFiscalYearService companyFiscalYearService
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , IRepositoryAsync<CompanyPartyGL> companyPartyGLRepository
            , IRepositoryAsync<CashMaster> cashMasterRepository
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository
            , IRepositoryAsync<GLTransactionDetail> gLTransactionDetailRepository
            , IRepositoryAsync<InvoiceDetailCharges> invoiceDetailChargesRepository
            , IRepositoryAsync<InvoiceServiceMasterCharges> invoiceServiceMasterChargesRepository
            , IRepositoryAsync<InvoiceServiceMasterChargesDetail> invoiceServiceMasterChargesDetailRepository
            , IRepositoryAsync<InvoiceServiceMasterChargesTax> invoiceServiceMasterChargesTaxRepository
            , IEmployeePayableService employeePayableService
            , IRepositoryAsync<AdditionalTax> additionalTaxRepository
            , IRepositoryAsync<AdditionalTaxDetail> additionalTaxDetailRepository
            , IRepositoryAsync<OtherInvoice> otherInvoiceRepository
            ) : base(repository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _invoiceMaterialRepository = invoiceMaterialRepository;
            _invoiceTaxRepository = invoiceTaxRepository;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _invoiceWriteOffRepository = invoiceWriteOffRepository;
            _invoiceWriteOffDetailRepository = invoiceWriteOffDetailRepository;
            _multiplePaymentRepository = multiplePaymentRepository;
            _multiplePaymentDetailRepository = multiplePaymentDetailRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _taxCodeRepository = taxCodeRepository;
            _taxCodeGLRepository = taxCodeGLRepository;
            _companyService = companyService;
            _materialMasterService = materialMasterService;
            _hsnTaxPercentageService = hsnTaxPercentageService;
            _taxCategoryGlService = taxCategoryGlService;
            _entityRepository = entityRepository;
            _taxCategoryRepository = taxCategoryRepository;
            _addressMasterRepository = addressMasterRepository;
            _partyRepository = partyRepository;
            _companyTaxYearService = companyTaxYearService;
            _voucherService = voucherService;
            _companyFiscalYearService = companyFiscalYearService;
            _invoiceTaxService = invoiceTaxService;
            _companyPartyRepository = companyPartyRepository;
            _companyPartyGLRepository = companyPartyGLRepository;
            _cashMasterRepository = cashMasterRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherRepository = voucherRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _gLTransactionDetailRepository = gLTransactionDetailRepository;
            _invoiceDetailChargesRepository = invoiceDetailChargesRepository;
            _invoiceServiceMasterChargesRepository = invoiceServiceMasterChargesRepository;
            _invoiceServiceMasterChargesDetailRepository = invoiceServiceMasterChargesDetailRepository;
            _invoiceServiceMasterChargesTaxRepository = invoiceServiceMasterChargesTaxRepository;
            _employeePayableService = employeePayableService;
            _additionalTaxRepository = additionalTaxRepository;
            _additionalTaxDetailRepository = additionalTaxDetailRepository;
            _otherInvoiceRepository = otherInvoiceRepository;
        }

        #endregion Constructor

        public Invoice InsertInvoice(Invoice invoice)
        {
            Check(invoice);
            invoice.Id = base.GetAutoNumber(nameof(Invoice), PKGeneratorEnum.Yearly, null, DateTime.Now);
            base.InsertGraph(invoice);
            return invoice;
        }
        public Invoice UpdateInvoice(Invoice invoice)
        {
            base.UpdateGraph(invoice);
            return invoice;
        }

        public Invoice InsertInvoice(Invoice invoice, long currentId)
        {
            invoice.Id = DateTime.Now.Year + currentId.ToString();
            base.InsertGraph(invoice);
            return invoice;
        }

        public Invoice InsertInvoice(VoucherViewModel voucherVM)
        {
            var invoice = new Invoice
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
                CompanyCurrencyRate = voucherVM.CompanyCurrencyRate,
                PaymentTermId = voucherVM.PaymentTermId,
                BaseOnDueDate = voucherVM.BaseOnDueDate,
                BaseNoOfDays = voucherVM.BaseNoOfDays,
                Amount = voucherVM.Amount,
                VoucherDate = voucherVM.VoucherDate,
                PostingDate = voucherVM.PostingDate,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                Narration = voucherVM.Narration,
                IsExcludingTax = voucherVM.IsExcludingTax,
                IsSplit = voucherVM.IsSplit,
                PartyType = voucherVM.PartyType,
                EmployeeId = voucherVM.EmployeeId,
                PartyId = voucherVM.PartyId,
                PartyPlantId = voucherVM.PartyPlantId,
                DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId,
                SourceType = voucherVM.SourceType,
                RevisedDueDate = voucherVM.MatureDate,
                ActualDueDate = voucherVM.MatureDate,
                SalesTypeId = voucherVM.SalesTypeId,
                IsPark = voucherVM.IsPark,
                PaymentSource = voucherVM.PaymentSource,
                ExpenseBookingId = voucherVM.ExpenseBookingId,
                InvoiceServiceMasterChargesId = voucherVM.InvoiceServiceMasterChargesId
            };

            return InsertInvoice(invoice);
        }

        public void InsertInvoiceDetail(Invoice invoice, InvoiceDetail invoiceDetail, int currentId)
        {
            invoiceDetail.Id = "IND" + MakePK(invoice.Id, currentId, 1);
            invoiceDetail.InvoiceId = invoice.Id;
            invoiceDetail.Archive = invoice.Archive;
            invoiceDetail.AddedBy = invoice.AddedBy;
            invoiceDetail.AddedDate = invoice.AddedDate;
            invoiceDetail.AddedFromIP = invoice.AddedFromIP;
            _invoiceDetailRepository.Insert(invoiceDetail);
        }

        public void UpdateInvoiceDetail(InvoiceDetail invoiceDetail)
        {
            _invoiceDetailRepository.Update(invoiceDetail);
        }

        private void Check(Invoice entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        }

        public string InsertCustomerInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, OtherInvoice otherInvoiceVM)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                if (voucherVM.IsExcludingTax == false)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    // INSERT INTO Invoice
                    var invoice = InsertInvoice(voucherVM);

                    // INSERT INTO Voucher TABLE
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    // Set to Invoice
                    invoice.VoucherId = voucher.Id;

                    // InvoiceTaxDetail invoiceTaxDetail = null;
                    decimal totalVoucherDetailTaxAmount = 0;
                    decimal taxDrAmount;

                    decimal totalcreditableDrAmount = 0, totalExpensesDrAmount = 0, totalwithholdCrAmount = 0;
                    decimal totalcreditableDrAmountAddTax = 0, totalExpensesDrAmountAddTax = 0, totalwithholdCrAmountAddTax = 0, taxDrAmountAddTax = 0;
                    decimal totalARBaseCurrencyDrAmount = 0;
                    decimal totalBaseCurrencyCrAmount = 0;
                    decimal totalBaseCurrencyDrAmount = 0;
                    decimal totalAPBaseCurrencyDrAmount = 0;

                    var totalAmountDr = 0.0M;
                    var totalAmountCr = 0.0M;

                    var creditablegl = false;
                    var withholdgl = false;
                    var merge = false;

                    var currentInvoiceDetail = 0;
                    var currentVoucherDetaiRecord = 0;
                    var currentTaxRecord = 0;
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.TrnType == "Cr")
                        {
                            var voucherCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                FiscalYearId = voucher.FiscalYearId,
                                FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                EntityId = voucherVM.EntityId,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                IsPark = voucher.IsPark,
                                Archive = voucher.Archive,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                TrnNature = TransactionNature.Sales.ToString()
                            };
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                            totalAmountCr += voucherCr.CrAmount;

                            if (null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
                            {
                                taxDrAmount = 0;
                                foreach (var cInvoiceTax in voucherDetailVM.InvoiceTaxViewModel)
                                {
                                    var taxCode = _taxCodeRepository.Find(cInvoiceTax.TaxCodeId);
                                    if (null == taxCode)
                                        throw new CustomException("Tax code not found!");
                                    if (taxCode.IsWithhold == false)
                                        throw new CustomException("Tax code must be withhold !");

                                    merge = taxCode.IsMerge;
                                    var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                                    if (null == taxCodeGL)
                                        throw new CustomException("Tax code GL not found!");

                                    currentTaxRecord++;
                                    var cInvoiceTaxdb = new InvoiceTax
                                    {
                                        Id = MakePK(invoice.Id, currentTaxRecord, 2),
                                        VoucherDetailId = voucherCr.Id,
                                        InvoiceId = invoice.Id,
                                        InvoiceDetailId = null,
                                        TaxCodeId = cInvoiceTax.TaxCodeId,
                                        TaxCategoryId = cInvoiceTax.TaxCategoryId,
                                        TaxAmount = cInvoiceTax.TaxAmount,
                                        TaxAutoAmount = cInvoiceTax.TaxAutoAmount,
                                        SourceType = SourceType.CustomerInvoiceTax.ToString(),
                                        AddedBy = voucher.AddedBy,
                                        AddedDate = voucher.AddedDate,
                                        AddedFromIP = voucher.AddedFromIP,
                                        TaxYearId = voucherVM.TaxYearId,
                                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                                        PartyId = voucherVM.PartyId
                                    };
                                    taxDrAmount += cInvoiceTax.TaxAmount;
                                    _invoiceTaxService.InsertInvoiceTax(invoice, cInvoiceTaxdb, invoiceTaxPk);
                                    // Insert Into Customer Invoice Tax Detail (Withhold GL)
                                    withholdgl = taxCode.IsWithhold;
                                    if (taxCode.IsWithhold && string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                        throw new CustomException("Withhold GL is not found of TaxCode " + taxCode.StandardName);
                                    if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                    {
                                        var invoiceTaxDetail = new InvoiceTaxDetail
                                        {
                                            GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                            BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                            ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                            Amount = cInvoiceTaxdb.TaxAmount,
                                            AType = "Cr"
                                        };

                                        totalwithholdCrAmount += invoiceTaxDetail.Amount;
                                        totalVoucherDetailTaxAmount += totalwithholdCrAmount;
                                        _invoiceTaxService.InsertInvoiceTaxDetail(cInvoiceTaxdb, invoiceTaxDetail, 1);
                                        _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);

                                        var voucherDetailTax = new VoucherDetail
                                        {
                                            CurrencyId = voucherCr.CurrencyId,
                                            DocDate = voucherCr.DocDate,
                                            DocRefNo = voucherCr.DocRefNo,
                                            CrAmount = invoiceTaxDetail.Amount,
                                            GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                            BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                            ActivityId = invoiceTaxDetail.ActivityId,
                                            Narration = voucherCr.Narration,
                                            FiscalYearId = voucher.FiscalYearId,
                                            FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                            EntityId = voucherCr.EntityId,
                                            PlantId = voucherCr.PlantId,
                                            PostingWithoutTaxAllow = voucherCr.PostingWithoutTaxAllow,
                                            RefCode = voucherCr.RefCode,
                                            VoucherId = voucher.Id,
                                            AddedBy = voucherCr.AddedBy,
                                            AddedDate = voucherCr.AddedDate,
                                            AddedFromIP = voucherCr.AddedFromIP,
                                            TrnNature = TransactionNature.SalesTax.ToString()
                                        };
                                        currentVoucherDetaiRecord++;
                                        _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetaiRecord);
                                        totalAmountCr += voucherDetailTax.CrAmount;

                                        var voucherDetailCurrencydb = new VoucherDetailCurrency
                                        {
                                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                            ToCurrencyId = companyCurrencyId,
                                            ParallelCurrencyId = companyCurrencyId,
                                            FromCurrencyId = voucherCr.CurrencyId,
                                            DrAmount = 0,
                                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                            ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                        };
                                        totalBaseCurrencyCrAmount += voucherDetailCurrencydb.CrAmount;
                                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencydb);
                                        voucherDetailCurrencydb = null;

                                        voucherDetailTax = null;
                                        invoiceTaxDetail = null;
                                    }
                                   
                                    //if (!merge && !taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                        //throw new CustomException("Expenses GL is not found of TaxCode " + taxCode.StandardName);
                                    //when expenses then tax amount will deduct from  voucher detail Dr Amount
                                    //if (!merge && !taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                    //{
                                    //    var invoiceTaxDetail = new InvoiceTaxDetail
                                    //    {
                                    //        GLGeneralInfoId = taxCodeGL.ExpensesGLId,
                                    //        BudgetMasterId = taxCodeGL.ExpensesGLBudgetMasterId,
                                    //        ActivityId = taxCodeGL.ExpensesGLActivityId,
                                    //        Amount = cInvoiceTaxdb.TaxAmount,
                                    //        AType = "Dr"
                                    //    };
                                    //    totalExpensesDrAmount += invoiceTaxDetail.Amount;
                                    //    _invoiceTaxService.InsertInvoiceTaxDetail(cInvoiceTaxdb, invoiceTaxDetail, 3);

                                    //    var voucherDetailTax = new VoucherDetail
                                    //    {
                                    //        CurrencyId = voucherCr.CurrencyId,
                                    //        DocDate = voucherCr.DocDate,
                                    //        DocRefNo = voucherCr.DocRefNo,
                                    //        DrAmount = invoiceTaxDetail.Amount,
                                    //        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    //        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    //        ActivityId = invoiceTaxDetail.ActivityId,
                                    //        Narration = voucherCr.Narration,
                                    //        FiscalYearId = voucher.FiscalYearId,
                                    //        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                    //        EntityId = voucherCr.EntityId,
                                    //        PlantId = voucherCr.PlantId,
                                    //        PostingWithoutTaxAllow = voucherCr.PostingWithoutTaxAllow,
                                    //        RefCode = voucherCr.RefCode,
                                    //        VoucherId = voucher.Id,
                                    //        AddedBy = voucherCr.AddedBy,
                                    //        AddedDate = voucherCr.AddedDate,
                                    //        AddedFromIP = voucherCr.AddedFromIP,
                                    //        TrnNature = TransactionNature.SalesTax.ToString()
                                    //    };
                                    //    currentVoucherDetaiRecord++;
                                    //    _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetaiRecord);
                                    //    totalAmountDr += voucherDetailTax.DrAmount;

                                    //    var voucherDetailCurrencybase = new VoucherDetailCurrency
                                    //    {
                                    //        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    //        ToCurrencyId = companyCurrencyId,
                                    //        ParallelCurrencyId = companyCurrencyId,
                                    //        FromCurrencyId = voucherVM.CurrencyId,
                                    //        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.DrAmount,
                                    //        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                    //    };
                                    //    totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    //    totalARBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    //    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                                    //    voucherDetailCurrencybase = null;
                                    //}
                                }
                            }
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherCr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate,
                            });
                        }
                    }
                    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        var invoiceTaxPk1 = _invoiceTaxService.GetMaxNumber();
                        foreach (var invoiceTaxVM in taxDetailVMList)
                        {
                            var taxCodeadd = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                            if (null == taxCodeadd)
                                throw new CustomException("Tax code not found!");

                            var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCodeadd.Id).Select().FirstOrDefault();
                            if (null == taxCodeGL)
                                throw new CustomException("Tax code GL not found!");

                            var invoiceTaxAdditional = new InvoiceTax
                            {
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = invoiceTaxVM.TaxAmount,
                                TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                            };

                            _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTaxAdditional, invoiceTaxPk1);

                            // Insert Into Customer Invoice Tax Detail (Withhold GL)
                            withholdgl = taxCodeadd.IsWithhold;
                            if (taxCodeadd.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                            {
                                totalAmountCr += invoiceTaxVM.TaxAmount;
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                    BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                    ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                    Amount = invoiceTaxAdditional.TaxAmount,
                                    AType = "Cr"
                                };
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTaxAdditional, invoiceTaxDetail, 1);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    CrAmount = invoiceTaxDetail.Amount,
                                };
                                currentVoucherDetaiRecord++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetaiRecord);
                                totalwithholdCrAmountAddTax += voucherDetailTax.CrAmount;

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
                            }
                            // Insert Into Customer Invoice Tax Detail (Creditable GL)
                            creditablegl = taxCodeadd.IsCreditable;
                            if (taxCodeadd.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                throw new CustomException("Creditable GL is not found of TaxCode " + taxCodeadd.StandardName);
                            if (taxCodeadd.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                            {
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                    BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                    ActivityId = taxCodeGL.CreditableGLActivityId,
                                    Amount = invoiceTaxAdditional.TaxAmount,
                                    AType = "Dr"
                                };
                                totalcreditableDrAmountAddTax += invoiceTaxDetail.Amount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTaxAdditional, invoiceTaxDetail, 2);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    DrAmount = invoiceTaxDetail.Amount

                                };
                                currentVoucherDetaiRecord++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetaiRecord);
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
                                totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                            }
                            
                        }
                    }

                    currentInvoiceDetail++;
                    // INSERT INTO InvoiceDetail
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

                    // INSERT INTO VoucherDetail
                    var voucherDr = new VoucherDetail
                    {
                        VoucherId = voucher.Id,
                        InvoiceDetailId = invoiceDetail.Id,
                        GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                        BudgetMasterId = invoiceDetail.BudgetMasterId,
                        ActivityId = invoiceDetail.ActivityId,
                        CurrencyId = voucher.CurrencyId,
                        FiscalYearId = voucher.FiscalYearId,
                        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        Archive = invoiceDetail.Archive,
                        DrAmount = voucherVM.Amount,
                        DocDate = voucher.DocDate,
                        DocRefNo = voucher.DocRefNo,
                        Narration = invoice.Narration,
                        EmployeeId = invoice.EmployeeId,
                        EntityId = voucherVM.EntityId,
                        PartyType = invoice.PartyType,
                        PartyId = invoice.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        PostingWithoutTaxAllow = invoice.IsExcludingTax,
                        IsPark = voucher.IsPark,
                        TrnNature = TransactionNature.ToCustomer.ToString()
                    };

                    voucherDr.EntityId = invoice.EntityId;
                    invoiceDetail.Amount = voucherVM.Amount;
                    invoiceDetail.NetAmount = voucherVM.Amount;
                    invoiceDetail.TaxAmount = totalVoucherDetailTaxAmount;
                    if(null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        invoiceDetail.Amount = voucherVM.Amount + taxDetailVMList.Sum(r=>r.TaxAmount);
                        invoiceDetail.NetAmount = voucherVM.Amount + taxDetailVMList.Sum(r => r.TaxAmount);
                        invoiceDetail.TaxAmount = totalVoucherDetailTaxAmount+taxDetailVMList.Sum(r => r.TaxAmount);
                    }
                    voucherDr.DrAmount = invoiceDetail.Amount;
                    totalAmountDr += voucherDr.DrAmount;

                    InsertInvoiceDetail(invoice, invoiceDetail, 1);

                    currentVoucherDetaiRecord++;
                    voucherDr.InvoiceDetailId = invoiceDetail.Id;
                    _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate
                    });

                    if (totalAmountDr != totalAmountCr)
                        throw new CustomException("Dr and Cr amount is not equal.");

                    if (otherInvoiceVM.PartyId!=null && otherInvoiceVM.GLGeneralInfoId!=null )
                    {
                        var otherInvoice = new OtherInvoice
                        {

                            Amount = otherInvoiceVM.Amount,
                            PartyId = otherInvoiceVM.PartyId,
                            PartyPlantId = otherInvoiceVM.PartyPlantId,
                            InvoiceId = invoice.Id,
                            GLGeneralInfoId = otherInvoiceVM.GLGeneralInfoId,
                            BudgetMasterId = otherInvoiceVM.BudgetMasterId,
                            ActivityId = otherInvoiceVM.ActivityId,
                            SourceType=invoice.SourceType,
                            IsPark=true,
                            Id = base.GetAutoNumber(nameof(OtherInvoice), PKGeneratorEnum.Yearly, null, DateTime.Now),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _otherInvoiceRepository.Insert(otherInvoice);
                    }
                        _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                {
                    InsertCustomerInvoiceExcludeTax(voucherVM, voucherDetailVMList, taxDetailVMList, companyCurrencyId);
                }
                return voucherVM.VoucherNo;
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

        private string InsertCustomerInvoiceExcludeTax(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList,string companyCurrencyId)
        {
            var flag = false;
            try
            {
              
                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice
                var invoice = InsertInvoice(voucherVM);

                // INSERT INTO Voucher TABLE
                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set to Invoice
                invoice.VoucherId = voucher.Id;

                // InvoiceTaxDetail invoiceTaxDetail = null;
                decimal totalVoucherDetailTaxAmount = 0;
                decimal taxDrAmount;

                decimal totalcreditableDrAmount = 0, totalExpensesDrAmount = 0, totalwithholdCrAmount = 0;
                decimal totalcreditableDrAmountAddTax = 0, totalExpensesDrAmountAddTax = 0, totalwithholdCrAmountAddTax = 0, taxDrAmountAddTax = 0;

                decimal totalARBaseCurrencyDrAmount = 0;
                decimal totalBaseCurrencyCrAmount = 0;
                decimal totalBaseCurrencyDrAmount = 0;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                var creditablegl = false;
                var withholdgl = false;
                var merge = false;

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.TrnType == "Cr")
                    {
                        var voucherCr = new VoucherDetail
                        {
                            VoucherId = voucher.Id,
                            FiscalYearId = voucher.FiscalYearId,
                            FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            EntityId = voucherVM.EntityId,
                            CrAmount = voucherDetailVM.Amount,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            IsPark = voucher.IsPark,
                            Archive = voucher.Archive,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax,
                            TrnNature = TransactionNature.Sales.ToString()
                        };
                        currentVoucherDetaiRecord++;
                        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                        totalAmountCr += voucherCr.CrAmount;

                        if (null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
                        {
                            taxDrAmount = 0;
                            foreach (var cInvoiceTax in voucherDetailVM.InvoiceTaxViewModel)
                            {
                                var taxCode = _taxCodeRepository.Find(cInvoiceTax.TaxCodeId);
                                if (null == taxCode)
                                    throw new CustomException("Tax code not found!");

                                if (voucherVM.IsExcludingTax)
                                {
                                    if (!taxCode.IsWithhold)
                                        throw new CustomException("Withhold  is not configured for TaxCode " + taxCode.StandardName);
                                }

                                merge = taxCode.IsMerge;
                                var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                                if (null == taxCodeGL)
                                    throw new CustomException("Tax code GL not found!");

                                // totalVoucherDetailTaxAmount = cInvoiceTax.TaxAmount;

                                currentTaxRecord++;
                                var cInvoiceTaxdb = new InvoiceTax
                                {
                                    Id = MakePK(invoice.Id, currentTaxRecord, 2),
                                    VoucherDetailId = voucherCr.Id,
                                    InvoiceId = invoice.Id,
                                    InvoiceDetailId = null,
                                    TaxCodeId = cInvoiceTax.TaxCodeId,
                                    TaxCategoryId = cInvoiceTax.TaxCategoryId,
                                    TaxAmount = cInvoiceTax.TaxAmount,
                                    TaxAutoAmount = cInvoiceTax.TaxAutoAmount,
                                    SourceType = SourceType.CustomerInvoiceTax.ToString(),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP,
                                    TaxYearId = voucherVM.TaxYearId,
                                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                                    PartyId = voucherVM.PartyId
                                };
                                taxDrAmount += cInvoiceTax.TaxAmount;
                                _invoiceTaxService.InsertInvoiceTax(invoice, cInvoiceTaxdb, invoiceTaxPk);
                                // Insert Into Customer Invoice Tax Detail (Withhold GL)
                                withholdgl = taxCode.IsWithhold;
                                if (taxCode.IsWithhold && string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                    throw new CustomException("Withhold GL is not found of TaxCode " + taxCode.StandardName);
                                if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                        BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                        ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                        Amount = cInvoiceTaxdb.TaxAmount,
                                        AType = "Cr"
                                    };

                                    totalwithholdCrAmount += invoiceTaxDetail.Amount;
                                    totalVoucherDetailTaxAmount += totalwithholdCrAmount;
                                    _invoiceTaxService.InsertInvoiceTaxDetail(cInvoiceTaxdb, invoiceTaxDetail, 1);
                                    _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);

                                    var voucherDetailTax = new VoucherDetail
                                    {
                                        CurrencyId = voucherCr.CurrencyId,
                                        DocDate = voucherCr.DocDate,
                                        DocRefNo = voucherCr.DocRefNo,
                                        CrAmount = invoiceTaxDetail.Amount,
                                        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                        ActivityId = invoiceTaxDetail.ActivityId,
                                        Narration = voucherCr.Narration,
                                        FiscalYearId = voucher.FiscalYearId,
                                        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                        EntityId = voucherCr.EntityId,
                                        PlantId = voucherCr.PlantId,
                                        PostingWithoutTaxAllow = voucherCr.PostingWithoutTaxAllow,
                                        RefCode = voucherCr.RefCode,
                                        VoucherId = voucher.Id,
                                        AddedBy = voucherCr.AddedBy,
                                        AddedDate = voucherCr.AddedDate,
                                        AddedFromIP = voucherCr.AddedFromIP,
                                        TrnNature = TransactionNature.SalesTax.ToString()
                                    };
                                    currentVoucherDetaiRecord++;
                                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetaiRecord);
                                    totalAmountCr += voucherDetailTax.CrAmount;

                                    var voucherDetailCurrencydb = new VoucherDetailCurrency
                                    {
                                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                        ToCurrencyId = companyCurrencyId,
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = voucherCr.CurrencyId,
                                        DrAmount = 0,
                                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                    };
                                    totalBaseCurrencyCrAmount += voucherDetailCurrencydb.CrAmount;
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencydb);
                                    voucherDetailCurrencydb = null;

                                    voucherDetailTax = null;
                                    invoiceTaxDetail = null;
                                }

                                // Insert Into Customer Invoice Tax Detail (Creditable GL)
                                creditablegl = taxCode.IsCreditable;
                                if (taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                    throw new CustomException("Creditable GL is not found of TaxCode " + taxCode.StandardName);
                                if (taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                        BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                        ActivityId = taxCodeGL.CreditableGLActivityId,
                                        Amount = cInvoiceTaxdb.TaxAmount,
                                        AType = "Dr"
                                    };
                                    totalcreditableDrAmount += invoiceTaxDetail.Amount;
                                    _invoiceTaxService.InsertInvoiceTaxDetail(cInvoiceTaxdb, invoiceTaxDetail, 2);

                                    var voucherDetailTax = new VoucherDetail
                                    {
                                        CurrencyId = voucherCr.CurrencyId,
                                        DocDate = voucherCr.DocDate,
                                        DocRefNo = voucherCr.DocRefNo,
                                        DrAmount = invoiceTaxDetail.Amount,
                                        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                        ActivityId = invoiceTaxDetail.ActivityId,
                                        Narration = voucherCr.Narration,
                                        FiscalYearId = voucher.FiscalYearId,
                                        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                        EntityId = voucherCr.EntityId,
                                        PlantId = voucherCr.PlantId,
                                        PostingWithoutTaxAllow = voucherCr.PostingWithoutTaxAllow,
                                        RefCode = voucherCr.RefCode,
                                        VoucherId = voucher.Id,
                                        AddedBy = voucherCr.AddedBy,
                                        AddedDate = voucherCr.AddedDate,
                                        AddedFromIP = voucherCr.AddedFromIP,
                                        TrnNature = TransactionNature.SalesTax.ToString()
                                    };
                                    currentVoucherDetaiRecord++;
                                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetaiRecord);
                                    totalAmountDr += voucherDetailTax.DrAmount;

                                    var voucherDetailCurrencybase = new VoucherDetailCurrency
                                    {
                                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                        ToCurrencyId = companyCurrencyId,
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = voucherVM.CurrencyId,
                                        CrAmount = 0,
                                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.DrAmount,
                                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                    };
                                    totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    totalARBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                                    voucherDetailCurrencybase = null;
                                }

                                if (!merge && !taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                    throw new CustomException("Expenses GL is not found of TaxCode " + taxCode.StandardName);
                                //when expenses then tax amount will deduct from  voucher detail Dr Amount
                                if (!merge && !taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL.ExpensesGLId,
                                        BudgetMasterId = taxCodeGL.ExpensesGLBudgetMasterId,
                                        ActivityId = taxCodeGL.ExpensesGLActivityId,
                                        Amount = cInvoiceTaxdb.TaxAmount,
                                        AType = "Dr"
                                    };
                                    totalExpensesDrAmount += invoiceTaxDetail.Amount;
                                    _invoiceTaxService.InsertInvoiceTaxDetail(cInvoiceTaxdb, invoiceTaxDetail, 3);

                                    var voucherDetailTax = new VoucherDetail
                                    {
                                        CurrencyId = voucherCr.CurrencyId,
                                        DocDate = voucherCr.DocDate,
                                        DocRefNo = voucherCr.DocRefNo,
                                        DrAmount = invoiceTaxDetail.Amount,
                                        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                        ActivityId = invoiceTaxDetail.ActivityId,
                                        Narration = voucherCr.Narration,
                                        FiscalYearId = voucher.FiscalYearId,
                                        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                                        EntityId = voucherCr.EntityId,
                                        PlantId = voucherCr.PlantId,
                                        PostingWithoutTaxAllow = voucherCr.PostingWithoutTaxAllow,
                                        RefCode = voucherCr.RefCode,
                                        VoucherId = voucher.Id,
                                        AddedBy = voucherCr.AddedBy,
                                        AddedDate = voucherCr.AddedDate,
                                        AddedFromIP = voucherCr.AddedFromIP,
                                        TrnNature = TransactionNature.SalesTax.ToString()
                                    };
                                    currentVoucherDetaiRecord++;
                                    _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetaiRecord);
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
                                    totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    totalARBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                                    voucherDetailCurrencybase = null;
                                }
                            }
                        }
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate,
                        });
                    }
                }

                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                {
                    var invoiceTaxPk1 = _invoiceTaxService.GetMaxNumber();
                    foreach (var invoiceTaxVM in taxDetailVMList)
                    {
                        var taxCodeadd = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                        if (null == taxCodeadd)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCodeadd.Id).Select().FirstOrDefault();
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");

                        var invoiceTax = new InvoiceTax
                        {
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            TaxAmount = invoiceTaxVM.TaxAmount,
                            TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                        };
                        _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk1);

                       
                        creditablegl = taxCodeadd.IsCreditable;
                        if (taxCodeadd.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                            throw new CustomException("Creditable GL is not found of TaxCode " + taxCodeadd.StandardName);
                        if (taxCodeadd.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                ActivityId = taxCodeGL.CreditableGLActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Dr"
                            };
                            totalcreditableDrAmountAddTax += invoiceTaxDetail.Amount;
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 2);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                DrAmount = invoiceTaxDetail.Amount

                            };
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetaiRecord);
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
                            totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            totalARBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                        }
                       
                    }
                }

                currentInvoiceDetail++;
                // INSERT INTO InvoiceDetail
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

                // INSERT INTO VoucherDetail
                var voucherDr = new VoucherDetail
                {
                    VoucherId = voucher.Id,
                    InvoiceDetailId = invoiceDetail.Id,
                    GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                    BudgetMasterId = invoiceDetail.BudgetMasterId,
                    ActivityId = invoiceDetail.ActivityId,
                    CurrencyId = voucher.CurrencyId,
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    AddedBy = voucher.AddedBy,
                    AddedDate = voucher.AddedDate,
                    AddedFromIP = voucher.AddedFromIP,
                    Archive = invoiceDetail.Archive,
                    DrAmount = voucherVM.Amount,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = invoice.Narration,
                    EmployeeId = invoice.EmployeeId,
                    EntityId = voucherVM.EntityId,
                    PartyType = invoice.PartyType,
                    PartyId = invoice.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PostingWithoutTaxAllow = invoice.IsExcludingTax,
                    IsPark = voucher.IsPark,
                    TrnNature = TransactionNature.ToCustomer.ToString()
                };

                invoiceDetail.Amount = voucherVM.Amount;
                voucherDr.EntityId = invoice.EntityId;
                invoiceDetail.NetAmount =   voucherVM.Amount;
                invoiceDetail.TaxAmount = totalVoucherDetailTaxAmount;
                voucherDr.DrAmount =  voucherVM.Amount;
                totalAmountDr += voucherDr.DrAmount;

                InsertInvoiceDetail(invoice, invoiceDetail, 1);

                currentVoucherDetaiRecord++;
                voucherDr.InvoiceDetailId = invoiceDetail.Id;
                _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);

                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate
                });

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

        public void InsertSales(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<ExchangeRateViewModel> exchangeRateVMList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallel Currency Id

                var parallerCurrency = _companyParallelCurrencyService.Query(r => r.CompanyId == voucherVM.CompanyId).Select();
                if (null == parallerCurrency)
                    throw new CustomException("Company Parallel Currency not found!");
                var companyCurrency = parallerCurrency.FirstOrDefault(r => r.ParallelCurrencyType == ParallelCurrencyType.CompanyCurrency.ToString());
                var groupCurrency = parallerCurrency.FirstOrDefault(r => r.ParallelCurrencyType == ParallelCurrencyType.CompanyGroupCurrency.ToString());
                var hardCurrency = parallerCurrency.FirstOrDefault(r => r.ParallelCurrencyType == ParallelCurrencyType.HardCurrency.ToString());
                var companyCurrencyId = companyCurrency != null ? companyCurrency.CurrencyId : throw new CustomException("Company Parallel Currency Id not found!");
                var companyGroupCurrencyId = groupCurrency?.CurrencyId;
                var hardCurrencyId = hardCurrency?.CurrencyId;

                #endregion Get Company Parallel Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Invoice TABLE
                var invoice = new Invoice
                {
                    Amount = voucherDetailVMList.Sum(r => r.Amount),
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    //BaseOnDueDate = voucherVM.BaseOnDueDate,
                    BaseOnDueDate = DateTime.Now,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    IsExcludingTax = voucherVM.IsExcludingTax,
                    ModelState = ModelState.Added,
                    PartyId = voucherVM.PartyId,
                    PartyType = PartyType.Customer.ToString(),
                    //PaymentTermId = paymentTerm?.Id,
                    PostingDate = voucherVM.DocDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    RevisedDueDate = null,
                    ActualDueDate = null,
                    OpeningBalanceId = null,
                    EmployeeId = null,
                    UpdatedBy = null,
                    UpdatedDate = null,
                    UpdatedFromIP = null,
                    Archive = false,
                    IsPark = false,
                    IsSplit = false,
                    IsWrittenOff = false,
                    VoucherId = null,
                    SalesTypeId = voucherVM.SalesTypeId,
                    WrittenOffAmount = 0
                };
                AuditService.AddedLog(invoice);

                // INSERT INTO Voucher TABLE

                var voucher = new Voucher
                {
                    CompanyGroupId = invoice.CompanyGroupId,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    CurrencyId = invoice.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    AddedBy = invoice.AddedBy,
                    AddedDate = invoice.AddedDate,
                    AddedFromIP = invoice.AddedFromIP,
                    VoucherDate = voucherVM.VoucherDate,
                    DocDate = invoice.DocDate,
                    DocRefNo = invoice.DocRefNo,
                    Id = null,
                    Archive = invoice.Archive,
                    IsPark = invoice.IsPark,
                    Narration = invoice.Narration,
                    PostingDate = voucherVM.PostingDate,
                    TransactionRefNo = null,
                    SourceType = invoice.SourceType,
                    UpdatedBy = null,
                    UpdatedDate = null,
                    UpdatedFromIP = null,
                    VoucherNo = null,
                    ModelState = ModelState.Added
                };
                Check(voucher);
                voucher.Id = base.GetAutoNumber("Voucher", PKGeneratorEnum.Yearly, null, invoice.PostingDate);
                voucher.VoucherNo = base.GetAutoNumber("Voucher" + voucher.CompanyId, PKGeneratorEnum.Daily, null, DateTime.Now);
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, null);
                // Set to Invoice
                invoice.VoucherId = voucher.Id;
                InsertGraph(invoice);

                var invoiceDetailPk = GetMaxNumber("InvoiceDetail", PKGeneratorEnum.Auto, null, DateTime.Now);
                var invoiceMaterialPk = GetMaxNumber("CustomerInvoiceMaterial", PKGeneratorEnum.Auto, null, DateTime.Now);
                var invoiceTaxPk = GetMaxNumber("InvoiceTax", PKGeneratorEnum.Auto, null, DateTime.Now);
                var invoiceTaxDetailPk = GetMaxNumber("InvoiceTaxDetail", PKGeneratorEnum.Auto, null, DateTime.Now);

                invoiceDetailPk.MaxNumber++;
                var invoiceDetail = new InvoiceDetail
                {
                    Id = "IND" + invoiceDetailPk.MaxNumber.ToString(),
                    AddedBy = invoice.AddedBy,
                    AddedDate = invoice.AddedDate,
                    AddedFromIP = invoice.AddedFromIP,
                    Archive = false,
                    ModelState = ModelState.Added,
                    Amount = invoice.Amount,
                    TaxAmount = 0,
                    NetAmount = invoice.Amount,
                    BlockDate = null,
                    BlockReason = null,
                    GLGeneralInfoId = voucherVM.DrGLId,
                    BudgetMasterId = voucherVM.DrBudgetMasterId,
                    ActivityId = voucherVM.DrActivityId,
                    InvoiceId = invoice.Id,
                    IsBlock = false,
                    IsWrittenOff = false,
                    WrittenOffAmount = 0,
                    UpdatedBy = null
                };

                // INSERT INTO VoucherDetail
                var voucherDr = new VoucherDetail
                {
                    ActivityId = invoiceDetail.ActivityId,
                    AddedBy = voucher.AddedBy,
                    AddedDate = voucher.AddedDate,
                    AddedFromIP = voucher.AddedFromIP,
                    Archive = invoiceDetail.Archive,
                    BudgetMasterId = invoiceDetail.BudgetMasterId,
                    DrAmount = invoiceDetail.Amount,
                    CrAmount = 0,
                    CurrencyId = voucher.CurrencyId,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = invoice.Narration,
                    EmployeeId = invoice.EmployeeId,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                    InvoiceDetailId = invoiceDetail.Id,
                    ModelState = invoiceDetail.ModelState,
                    PartyId = invoice.PartyId,
                    PartyType = invoice.PartyType,
                    PostingWithoutTaxAllow = invoice.IsExcludingTax,
                    VoucherId = voucher.Id,
                    IsPark = voucher.IsPark
                };

                var partyTaxCategory = _taxCategoryRepository.Query(r => r.Active).Select().ToList();

                var company = _companyService.Find(voucher.CompanyId);
                var companyAddress = _addressMasterRepository.Find(company.AddressMasterId);
                var party = _partyRepository.Find(invoice.PartyId);
                var partyAddress = _addressMasterRepository.Find(party.AddressMasterId);

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    invoiceMaterialPk.MaxNumber++;
                    var invoiceMaterial = new InvoiceMaterial
                    {
                        AddedBy = invoice.AddedBy,
                        AddedDate = invoice.AddedDate,
                        AddedFromIP = invoice.AddedFromIP,
                        Amount = voucherDetailVM.Amount,
                        InvoiceId = invoice.Id,
                        InvoiceDetailId = invoiceDetail.Id,
                        Id = invoiceMaterialPk.MaxNumber.ToString(),
                        ModelState = ModelState.Added,
                        MaterialMasterId = voucherDetailVM.MaterialMasterId,
                        MaterialMasterArticleId = voucherDetailVM.MaterialMasterArticleId,
                        UOMId = voucherDetailVM.UomId,
                        Qty = voucherDetailVM.Qty
                    };
                    var materialMaster = _materialMasterService.Find(invoiceMaterial.MaterialMasterId);
                    var hsnPercentageList = _hsnTaxPercentageService.Query(r => r.HSNCodeId == materialMaster.HSNCodeId).Select().ToList();
                    foreach (var item in hsnPercentageList)
                    {
                        string circle = null;
                        circle = companyAddress.StateId == partyAddress.StateId ? TaxCircle.InsideState.ToString() : TaxCircle.OutsideState.ToString();

                        var tax = partyTaxCategory.FirstOrDefault(r => r.TaxCircle == circle);
                        if (null != tax)
                        {
                            invoiceTaxPk.MaxNumber++;
                            var invoiceTax = new InvoiceTax
                            {
                                InvoiceDetailId = invoiceDetail.Id,
                                InvoiceMaterialId = invoiceMaterial.Id,
                                VoucherDetail = voucherDr,
                                VoucherDetailId = voucherDr.Id,
                                TaxCategoryId = item.TaxCategoryId,
                                AddedBy = invoice.AddedBy,
                                AddedDate = invoice.AddedDate,
                                AddedFromIP = invoice.AddedFromIP,
                                Archive = false,
                                ModelState = ModelState.Added,
                                SourceType = SourceType.SalesInvoice.ToString(),
                                TaxAmount = invoice.Amount / 100 * item.Percentage,
                                TaxAutoAmount = 0
                            };
                            _invoiceTaxRepository.Insert(invoiceTax);

                            var taxGl = _taxCategoryGlService.Query(r => r.TaxCategoryId == item.TaxCategoryId).Select().FirstOrDefault();
                            invoiceTaxDetailPk.MaxNumber++;
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                AType = "Dr",
                                Id = invoiceTaxDetailPk.MaxNumber.ToString(),
                                ActivityId = taxGl.ActivityId,
                                BudgetMasterId = taxGl.BudgetMasterId,
                                GLGeneralInfoId = taxGl.GLGeneralInfoId,
                                AddedBy = invoiceTax.AddedBy,
                                AddedFromIP = invoiceTax.AddedFromIP,
                                AddedDate = invoiceTax.AddedDate,
                                Amount = invoice.Amount / 100 * item.Percentage,
                                InvoiceTaxId = invoiceTax.Id
                            };
                            _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                        }
                    }
                    _invoiceMaterialRepository.Insert(invoiceMaterial);
                    InsertInvoiceDetail(invoice, invoiceDetail, 1);
                }

                var voucherCr = new VoucherDetail
                {
                    CurrencyId = voucher.CurrencyId,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    AddedBy = voucher.AddedBy,
                    AddedDate = voucher.AddedDate,
                    AddedFromIP = voucher.AddedFromIP,
                    DrAmount = 0,
                    CrAmount = invoice.Amount,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = voucher.Narration,
                    IsPark = voucher.IsPark,
                    Archive = voucher.Archive,
                    ModelState = voucher.ModelState,
                    PostingWithoutTaxAllow = invoice.IsExcludingTax,
                    VoucherId = voucher.Id
                };
                _voucherService.InsertVoucherDetail(voucher, voucherCr, 1);

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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }


        private string InsertVendorInvoiceExcludeTax(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Invoice
                var invoice = InsertInvoice(voucherVM);

                // INSERT INTO Voucher TABLE
                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set to Invoice
                invoice.VoucherId = voucher.Id;

                decimal totalVoucherDetailTaxAmount = 0;
                decimal totalcreditableDrAmount = 0, totalExpensesDrAmount = 0, totalwithholdCrAmount = 0, taxDrAmount = 0;
                decimal totalcreditableDrAmountAddTax = 0, totalExpensesDrAmountAddTax = 0, totalwithholdCrAmountAddTax = 0, taxDrAmountAddTax = 0;
                decimal totalBaseCurrencyCrAmount = 0;
                decimal totalBaseCurrencyDrAmount = 0;
                decimal totalAPBaseCurrencyDrAmount = 0;
                var creditablegl = false;
                var withholdgl = false;
                var merge = false;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var currentVoucherDetailId = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                if (invoice.PaymentSource == PaymentSource.GL.ToString())
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        // in libility side Dr.
                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax,
                            TrnNature = TransactionNature.Purchases.ToString(),
                            PaymentSource = PaymentSource.GL.ToString()
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        totalAmountDr += voucherDetailDr.DrAmount;
                        if (null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
                        {
                            taxDrAmount = 0;
                            foreach (var invoiceTaxVM in voucherDetailVM.InvoiceTaxViewModel)
                            {
                                var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                                if (null == taxCode)
                                    throw new CustomException("Tax code not found!");

                                if (voucherVM.IsExcludingTax)
                                {
                                    if (taxCode.IsWithhold == false)
                                        throw new CustomException("Withhold  is not configured for TaxCode " + taxCode.StandardName);
                                }

                                merge = taxCode.IsMerge;
                                var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                                if (null == taxCodeGL)
                                    throw new CustomException("Tax code GL not found!");

                                var invoiceTax = new InvoiceTax
                                {
                                    VoucherDetailId = voucherDetailDr.Id,
                                    InvoiceId = invoice.Id,
                                    TaxCodeId = invoiceTaxVM.TaxCodeId,
                                    TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                    TaxAmount = invoiceTaxVM.TaxAmount,
                                    TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.VendorInvoiceTax.ToString()
                                };
                                taxDrAmount += invoiceTaxVM.TaxAmount;
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);

                                // Insert Into Customer Invoice Tax Detail (Withhold GL)
                                withholdgl = taxCode.IsWithhold;
                                if (taxCode.IsWithhold && string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                    throw new CustomException("Withhold GL is not found of TaxCode " + taxCode.StandardName);
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
                                    totalwithholdCrAmount += invoiceTaxDetail.Amount;
                                    totalVoucherDetailTaxAmount += totalwithholdCrAmount;
                                    _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                    var voucherDetailTax = new VoucherDetail
                                    {
                                        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                        ActivityId = invoiceTaxDetail.ActivityId,
                                        InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                        CrAmount = invoiceTaxDetail.Amount,
                                        PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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
                                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                    };
                                    totalBaseCurrencyCrAmount += voucherDetailCurrencydb.CrAmount;
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencydb);
                                }

                                // Insert Into Customer Invoice Tax Detail (Creditable GL)
                                creditablegl = taxCode.IsCreditable;
                                if (taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                    throw new CustomException("Creditable GL is not found of TaxCode " + taxCode.StandardName);
                                if (taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                        BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                        ActivityId = taxCodeGL.CreditableGLActivityId,
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
                                        DrAmount = invoiceTaxDetail.Amount,
                                        PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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
                                    totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                                }
                            }
                        }
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                        });

                        totalBaseCurrencyDrAmount = 0;
                    }
                }
                else if (invoice.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id not found!");
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);

                    invoice.CashMasterId = voucherVM.CashMasterId;

                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = cashMaster.GLGeneralInfoId,
                        BudgetMasterId = cashMaster.BudgetMasterId,
                        ActivityId = cashMaster.ActivityId,
                        DrAmount = invoice.Amount,
                        PostingWithoutTaxAllow = invoice.IsExcludingTax,
                        PaymentSource = PaymentSource.Cash.ToString(),
                        CashMasterId = invoice.CashMasterId
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                    totalAmountDr += voucherDetailDr.DrAmount;

                    var glTransactionDetail = new GLTransactionDetail
                    {
                        SourceType = voucherDetailDr.PaymentSource,
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId
                    };

                    if (cashMaster.CurrencyId == voucherVM.CurrencyId)
                        glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                    else
                        glTransactionDetail.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                    _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                    });
                }


                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                {
                    var invoiceTaxPk1 = _invoiceTaxService.GetMaxNumber();
                    foreach (var invoiceTaxVM in taxDetailVMList)
                    {
                        var taxCodeadd = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                        if (null == taxCodeadd)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCodeadd.Id).Select().FirstOrDefault();
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");

                        var invoiceTax = new InvoiceTax
                        {
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            TaxAmount = invoiceTaxVM.TaxAmount,
                            TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                        };

                        _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk1);

                        // Insert Into Customer Invoice Tax Detail (Withhold GL)
                        withholdgl = taxCodeadd.IsWithhold;
                        if (taxCodeadd.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                        {
                            totalAmountCr += invoiceTaxVM.TaxAmount;
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
                            totalwithholdCrAmountAddTax += voucherDetailTax.CrAmount;

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
                        }
                        // Insert Into Customer Invoice Tax Detail (Creditable GL)
                        creditablegl = taxCodeadd.IsCreditable;
                        if (taxCodeadd.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                            throw new CustomException("Creditable GL is not found of TaxCode " + taxCodeadd.StandardName);
                        if (taxCodeadd.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                ActivityId = taxCodeGL.CreditableGLActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Dr"
                            };
                            totalcreditableDrAmountAddTax += invoiceTaxDetail.Amount;
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 2);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                DrAmount = invoiceTaxDetail.Amount

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
                            totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                        }
                        if (!taxCodeadd.IsMerge && !taxCodeadd.IsCreditable && string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                            throw new CustomException("Expenses GL is not found of TaxCode " + taxCodeadd.StandardName);
                        if (!taxCodeadd.IsMerge && !taxCodeadd.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.ExpensesGLId,
                                BudgetMasterId = taxCodeGL.ExpensesGLBudgetMasterId,
                                ActivityId = taxCodeGL.ExpensesGLActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Dr"

                            };
                            totalExpensesDrAmountAddTax += invoiceTaxDetail.Amount;
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 3);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                DrAmount = invoiceTaxDetail.Amount
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

                            totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                        }
                        if (taxCodeadd.IsMerge && !taxCodeadd.IsCreditable)
                        {
                            //voucherDetailTax.DrAmount += invoiceTax.TaxAmount;
                            totalAmountDr += invoiceTax.TaxAmount;
                        }
                    }
                }



                var partyType = PartyType.Vendor.ToString();
                var companyParty = _companyPartyRepository.Query(r => r.CompanyId == invoice.CompanyId && r.PlantId == invoice.PlantId && r.PartyId == invoice.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                if (null == companyParty)
                    throw new CustomException("Plant party mapping not found!");
                var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                if (null == companyPartyGLList)
                    throw new CustomException("Party GL not found!");

                var reconGL = PartyGLType.ReconciliationGL.ToString();
                var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                if (null == regularGL)
                    throw new CustomException("Party Reconciliation GL not found!");

                // INSERT INTO InvoiceDetail
                var invoiceDetail = new InvoiceDetail
                {
                    GLGeneralInfoId = regularGL.GLGeneralInfoId,
                    BudgetMasterId = regularGL.BudgetMasterId,
                    ActivityId = regularGL.ActivityId,
                    // Amount = voucherVM.Amount,
                    // Amount = voucherVM.IsExcludingTax ? voucherVM.Amount  : voucherVM.Amount + totalcreditableDrAmount,
                    Amount = voucherVM.Amount,
                    NetAmount = voucherVM.Amount,
                    TaxAmount = totalwithholdCrAmount
                };
                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                {
                    invoiceDetail.Amount = voucherVM.Amount + totalcreditableDrAmountAddTax - totalwithholdCrAmountAddTax + totalExpensesDrAmountAddTax;
                    invoiceDetail.NetAmount -= taxDetailVMList.Sum(r => r.TaxAmount);

                }
                InsertInvoiceDetail(invoice, invoiceDetail, 1);
                invoice.Amount = invoiceDetail.Amount;
                // INSERT INTO VoucherDetail
                var voucherDetailCr = new VoucherDetail
                {
                    GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                    BudgetMasterId = invoiceDetail.BudgetMasterId,
                    ActivityId = invoiceDetail.ActivityId,
                    CurrencyId = voucher.CurrencyId,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = invoice.Narration,
                    EmployeeId = invoice.EmployeeId,
                    InvoiceDetailId = invoiceDetail.Id,
                    PartyType = invoice.PartyType,
                    PartyId = invoice.PartyId,
                    PartyPlantId = invoice.PartyPlantId,
                    PostingWithoutTaxAllow = invoice.IsExcludingTax,
                    CrAmount = invoiceDetail.Amount
                };
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                totalAmountCr += voucherDetailCr.CrAmount;

                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailCr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate
                });

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (null != tdsVMList && tdsVMList.Count() > 0)
                {
                    var invoiceTax = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = tdsVMList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = tdsVMList.Sum(r => r.TaxAutoAmount),
                        InventoryReceiveId = null,
                        InvoiceId = invoice.Id,
                        EmployeePayableId = null,
                        PartyId = invoice.PartyId,
                        PartyPlantId = invoice.PartyPlantId,
                        Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _additionalTaxRepository.Insert(invoiceTax);

                    int addtionalTaxDetailId = 0;
                    foreach (var invoiceTaxVM in tdsVMList)
                    {

                        if (null == invoiceTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == invoiceTaxVM.TaxCodeId).Select().FirstOrDefault();
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");


                        addtionalTaxDetailId++;
                        var invoiceTaxDetail = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                            BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                            ActivityId = taxCodeGL.WithholdCreditableActivityId,
                            Amount = invoiceTaxVM.TaxAmount,
                            AdditionalTaxId = invoiceTax.Id,
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id = MakePK(invoiceTax.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxDetailRepository.Insert(invoiceTaxDetail);


                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return voucherVM.VoucherNo = voucher.VoucherNo;
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

        public string InsertVendorInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);


                if (voucherVM.IsExcludingTax == false)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    // INSERT INTO Invoice
                    var invoice = InsertInvoice(voucherVM);

                    // INSERT INTO Voucher TABLE
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    voucherVM.VoucherNo = voucher.VoucherNo;
                    // Set to Invoice
                    invoice.VoucherId = voucher.Id;

                    decimal totalVoucherDetailTaxAmount = 0;
                    decimal totalcreditableDrAmount = 0, totalExpensesDrAmount = 0, totalwithholdCrAmount = 0, taxDrAmount = 0;
                    decimal totalcreditableDrAmountAddTax = 0, totalExpensesDrAmountAddTax = 0, totalwithholdCrAmountAddTax = 0, taxDrAmountAddTax = 0;
                    decimal totalBaseCurrencyCrAmount = 0;
                    decimal totalBaseCurrencyDrAmount = 0;
                    decimal totalAPBaseCurrencyDrAmount = 0;
                    var creditablegl = false;
                    var withholdgl = false;
                    var merge = false;
                    var totalAmountDr = 0.0M;
                    var totalAmountCr = 0.0M;
                    var currentVoucherDetailId = 0;
                    var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                    if (invoice.PaymentSource == PaymentSource.GL.ToString())
                    {
                        foreach (var voucherDetailVM in voucherDetailVMList)
                        {
                            // in libility side Dr.
                            var voucherDetailDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.Amount,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                TrnNature = TransactionNature.Purchases.ToString(),
                                PaymentSource = PaymentSource.GL.ToString()
                            };
                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                            totalAmountDr += voucherDetailDr.DrAmount;
                            if (null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
                            {
                                taxDrAmount = 0;
                                foreach (var invoiceTaxVM in voucherDetailVM.InvoiceTaxViewModel)
                                {
                                    var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                                    if (null == taxCode)
                                        throw new CustomException("Tax code not found!");

                                    if (voucherVM.IsExcludingTax)
                                    {
                                        if (taxCode.IsWithhold == false)
                                            throw new CustomException("Withhold  is not configured for TaxCode " + taxCode.StandardName);
                                    }

                                    merge = taxCode.IsMerge;
                                    var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                                    if (null == taxCodeGL)
                                        throw new CustomException("Tax code GL not found!");

                                    var invoiceTax = new InvoiceTax
                                    {
                                        VoucherDetailId = voucherDetailDr.Id,
                                        InvoiceId = invoice.Id,
                                        TaxCodeId = invoiceTaxVM.TaxCodeId,
                                        TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                        TaxAmount = Math.Round(invoiceTaxVM.TaxAmount, 4),
                                        TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                                        PartyId = voucherVM.PartyId,
                                        SourceType = SourceType.VendorInvoiceTax.ToString()
                                    };
                                    taxDrAmount += Math.Round(invoiceTaxVM.TaxAmount, 4);
                                    _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);

                                    // Insert Into Customer Invoice Tax Detail (Withhold GL)
                                    withholdgl = taxCode.IsWithhold;
                                    if (taxCode.IsWithhold && string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                        throw new CustomException("Withhold GL is not found of TaxCode " + taxCode.StandardName);
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
                                        totalwithholdCrAmount += invoiceTaxDetail.Amount;
                                        totalVoucherDetailTaxAmount += totalwithholdCrAmount;
                                        _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                        var voucherDetailTax = new VoucherDetail
                                        {
                                            GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                            BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                            ActivityId = invoiceTaxDetail.ActivityId,
                                            InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                            CrAmount = invoiceTaxDetail.Amount,
                                            PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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
                                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                            ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                        };
                                        totalBaseCurrencyCrAmount += voucherDetailCurrencydb.CrAmount;
                                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencydb);
                                    }

                                    // Insert Into Customer Invoice Tax Detail (Creditable GL)
                                    creditablegl = taxCode.IsCreditable;
                                    if (taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                        throw new CustomException("Creditable GL is not found of TaxCode " + taxCode.StandardName);
                                    if (taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                    {
                                        var invoiceTaxDetail = new InvoiceTaxDetail
                                        {
                                            GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                            BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                            ActivityId = taxCodeGL.CreditableGLActivityId,
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
                                            DrAmount = invoiceTaxDetail.Amount,
                                            PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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
                                        totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                        totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                                    }
                                    if (!merge && !taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                        throw new CustomException("Expenses GL is not found of TaxCode " + taxCode.StandardName);
                                    if (!merge && !taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                    {
                                        var invoiceTaxDetail = new InvoiceTaxDetail
                                        {
                                            GLGeneralInfoId = taxCodeGL.ExpensesGLId,
                                            BudgetMasterId = taxCodeGL.ExpensesGLBudgetMasterId,
                                            ActivityId = taxCodeGL.ExpensesGLActivityId,
                                            Amount = invoiceTax.TaxAmount,
                                            AType = "Dr"

                                        };
                                        totalExpensesDrAmount += invoiceTaxDetail.Amount;
                                        _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 3);

                                        var voucherDetailTax = new VoucherDetail
                                        {
                                            GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                            BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                            ActivityId = invoiceTaxDetail.ActivityId,
                                            CurrencyId = voucherDetailDr.CurrencyId,
                                            DrAmount = invoiceTaxDetail.Amount,
                                            PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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

                                        totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                        totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                                    }
                                    if (merge && !taxCode.IsCreditable)
                                    {
                                        voucherDetailDr.DrAmount += invoiceTax.TaxAmount;
                                        totalAmountDr += invoiceTax.TaxAmount;
                                    }
                                }
                            }


                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                            });

                            totalBaseCurrencyDrAmount = 0;
                        }
                    }
                    else if (invoice.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                            throw new CustomException("Cash Id not found!");
                        var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);

                        invoice.CashMasterId = voucherVM.CashMasterId;

                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = cashMaster.GLGeneralInfoId,
                            BudgetMasterId = cashMaster.BudgetMasterId,
                            ActivityId = cashMaster.ActivityId,
                            DrAmount = invoice.Amount,
                            PostingWithoutTaxAllow = invoice.IsExcludingTax,
                            PaymentSource = PaymentSource.Cash.ToString(),
                            CashMasterId = invoice.CashMasterId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        totalAmountDr += voucherDetailDr.DrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailDr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        if (cashMaster.CurrencyId == voucherVM.CurrencyId)
                            glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                        else
                            glTransactionDetail.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                        });
                    }


                    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        var invoiceTaxPk1 = _invoiceTaxService.GetMaxNumber();
                        foreach (var invoiceTaxVM in taxDetailVMList)
                        {
                            var taxCodeadd = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                            if (null == taxCodeadd)
                                throw new CustomException("Tax code not found!");

                            var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCodeadd.Id).Select().FirstOrDefault();
                            if (null == taxCodeGL)
                                throw new CustomException("Tax code GL not found!");

                            var invoiceTaxAdditional = new InvoiceTax
                            {
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = invoiceTaxVM.TaxAmount,
                                TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                            };

                            _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTaxAdditional, invoiceTaxPk1);

                            // Insert Into Customer Invoice Tax Detail (Withhold GL)
                            withholdgl = taxCodeadd.IsWithhold;
                            if (taxCodeadd.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                            {
                                totalAmountCr += invoiceTaxVM.TaxAmount;
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                    BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                    ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                    Amount = invoiceTaxAdditional.TaxAmount,
                                    AType = "Cr"
                                };
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTaxAdditional, invoiceTaxDetail, 1);

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
                                totalwithholdCrAmountAddTax += voucherDetailTax.CrAmount;

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
                            }
                            // Insert Into Customer Invoice Tax Detail (Creditable GL)
                            creditablegl = taxCodeadd.IsCreditable;
                            if (taxCodeadd.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                throw new CustomException("Creditable GL is not found of TaxCode " + taxCodeadd.StandardName);
                            if (taxCodeadd.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                            {
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                    BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                    ActivityId = taxCodeGL.CreditableGLActivityId,
                                    Amount = invoiceTaxAdditional.TaxAmount,
                                    AType = "Dr"
                                };
                                totalcreditableDrAmountAddTax += invoiceTaxDetail.Amount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTaxAdditional, invoiceTaxDetail, 2);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    DrAmount = invoiceTaxDetail.Amount

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
                                totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                            }
                            if (!taxCodeadd.IsMerge && !taxCodeadd.IsCreditable && string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                throw new CustomException("Expenses GL is not found of TaxCode " + taxCodeadd.StandardName);
                            if (!taxCodeadd.IsMerge && !taxCodeadd.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                            {
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    GLGeneralInfoId = taxCodeGL.ExpensesGLId,
                                    BudgetMasterId = taxCodeGL.ExpensesGLBudgetMasterId,
                                    ActivityId = taxCodeGL.ExpensesGLActivityId,
                                    Amount = invoiceTaxAdditional.TaxAmount,
                                    AType = "Dr"

                                };
                                totalExpensesDrAmountAddTax += invoiceTaxDetail.Amount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTaxAdditional, invoiceTaxDetail, 3);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    DrAmount = invoiceTaxDetail.Amount
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

                                totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                            }
                            if (taxCodeadd.IsMerge && !taxCodeadd.IsCreditable)
                            {
                                //voucherDetailTax.DrAmount += invoiceTax.TaxAmount;
                                totalAmountDr += invoiceTaxAdditional.TaxAmount;
                            }
                        }
                    }



                    var partyType = PartyType.Vendor.ToString();
                    var companyParty = _companyPartyRepository.Query(r => r.CompanyId == invoice.CompanyId && r.PlantId == invoice.PlantId && r.PartyId == invoice.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                    if (null == companyParty)
                        throw new CustomException("Plant party mapping not found!");
                    var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                    if (null == companyPartyGLList)
                        throw new CustomException("Party GL not found!");

                    var reconGL = PartyGLType.ReconciliationGL.ToString();
                    var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                    if (null == regularGL)
                        throw new CustomException("Party Reconciliation GL not found!");

                    // INSERT INTO InvoiceDetail
                    var invoiceDetail = new InvoiceDetail
                    {
                        GLGeneralInfoId = regularGL.GLGeneralInfoId,
                        BudgetMasterId = regularGL.BudgetMasterId,
                        ActivityId = regularGL.ActivityId,
                        // Amount = voucherVM.Amount,
                        // Amount = voucherVM.IsExcludingTax ? voucherVM.Amount  : voucherVM.Amount + totalcreditableDrAmount,
                        Amount = voucherVM.Amount,
                        NetAmount = voucherVM.Amount - totalwithholdCrAmount,
                        TaxAmount = totalwithholdCrAmount
                    };
                    if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        invoiceDetail.Amount = voucherVM.Amount + totalcreditableDrAmountAddTax - totalwithholdCrAmountAddTax + totalExpensesDrAmountAddTax;
                        invoiceDetail.NetAmount -= taxDetailVMList.Sum(r => r.TaxAmount);

                    }
                    InsertInvoiceDetail(invoice, invoiceDetail, 1);
                    invoice.Amount = invoiceDetail.Amount;
                    // INSERT INTO VoucherDetail
                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                        BudgetMasterId = invoiceDetail.BudgetMasterId,
                        ActivityId = invoiceDetail.ActivityId,
                        CurrencyId = voucher.CurrencyId,
                        DocDate = voucher.DocDate,
                        DocRefNo = voucher.DocRefNo,
                        Narration = invoice.Narration,
                        EmployeeId = invoice.EmployeeId,
                        InvoiceDetailId = invoiceDetail.Id,
                        PartyType = invoice.PartyType,
                        PartyId = invoice.PartyId,
                        PartyPlantId = invoice.PartyPlantId,
                        PostingWithoutTaxAllow = invoice.IsExcludingTax,
                        CrAmount = invoiceDetail.Amount
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    totalAmountCr += voucherDetailCr.CrAmount;

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate
                    });

                    if (totalAmountDr != totalAmountCr)
                        throw new CustomException("Dr and Cr amount is not equal.");
                    if (null != tdsVMList && tdsVMList.Count() > 0)
                    {
                        var invoiceTax = new AdditionalTax
                        {

                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            TaxAmount = tdsVMList.Sum(r => r.TaxAmount),
                            TaxAutoAmount = tdsVMList.Sum(r => r.TaxAutoAmount),
                            InventoryReceiveId = null,
                            InvoiceId = invoice.Id,
                            EmployeePayableId = null,
                            PartyId = invoice.PartyId,
                            PartyPlantId = invoice.PartyPlantId,
                            Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxRepository.Insert(invoiceTax);

                        int addtionalTaxDetailId = 0;
                        foreach (var invoiceTaxVM in tdsVMList)
                        {

                            if (null == invoiceTaxVM.TaxCodeId)
                                throw new CustomException("Tax code not found!");

                            var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == invoiceTaxVM.TaxCodeId).Select().FirstOrDefault();
                            if (null == taxCodeGL)
                                throw new CustomException("Tax code GL not found!");


                            addtionalTaxDetailId++;
                            var invoiceTaxDetail = new AdditionalTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                                BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                                ActivityId = taxCodeGL.WithholdCreditableActivityId,
                                Amount = invoiceTaxVM.TaxAmount,
                                AdditionalTaxId = invoiceTax.Id,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                AType = "Cr",
                                Id = MakePK(invoiceTax.Id, addtionalTaxDetailId, 3),
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            _additionalTaxDetailRepository.Insert(invoiceTaxDetail);


                        }
                    }
                    _unitOfWork.SaveChanges();
                    //clsStaticInfo objApp = new clsStaticInfo();
                    //objApp.SaveDataSets(_adgrnAccMapset);
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                    InsertVendorInvoiceExcludeTax(voucherVM, voucherDetailVMList, taxDetailVMList, tdsVMList);

                return voucherVM.VoucherNo;
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

        public string InsertVendorInvoiceBeneficiaryEmployee(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Invoice
                if (voucherVM.EmployeeTransactionTypeId == null)
                    throw new CustomException("Please select Transaction Type!");
                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                {
                    voucherVM.Amount = voucherVM.Amount + taxDetailVMList.Sum(r => r.TaxAmount);
                }
                var employeePayable = _employeePayableService.InsertEmployeePayable(voucherVM);

                // INSERT INTO Voucher TABLE
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to Invoice
                employeePayable.VoucherId = voucher.Id;

                decimal totalVoucherDetailTaxAmount = 0;
                decimal totalcreditableDrAmount = 0, totalExpensesDrAmount = 0, totalwithholdCrAmount = 0, taxDrAmount = 0;
                decimal totalBaseCurrencyCrAmount = 0;
                decimal totalBaseCurrencyDrAmount = 0;
                decimal totalAPBaseCurrencyDrAmount = 0;
                var creditablegl = false;
                var withholdgl = false;
                var merge = false;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var currentVoucherDetailId = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                if (voucherVM.PaymentSource == PaymentSource.GL.ToString())
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        // in libility side Dr.
                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            PostingWithoutTaxAllow = voucherVM.IsExcludingTax,
                            TrnNature = TransactionNature.Purchases.ToString(),
                            PaymentSource = PaymentSource.GL.ToString()
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                        totalAmountDr += voucherDetailDr.DrAmount;
                        if (null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
                        {
                            taxDrAmount = 0;
                            foreach (var invoiceTaxVM in voucherDetailVM.InvoiceTaxViewModel)
                            {
                                var taxCode = _taxCodeRepository.Find(invoiceTaxVM.TaxCodeId);
                                if (null == taxCode)
                                    throw new CustomException("Tax code not found!");

                                if (voucherVM.IsExcludingTax)
                                {
                                    if (taxCode.IsWithhold == false)
                                        throw new CustomException("Withhold  is not configured for TaxCode " + taxCode.StandardName);
                                }

                                merge = taxCode.IsMerge;
                                var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == taxCode.Id).Select().FirstOrDefault();
                                if (null == taxCodeGL)
                                    throw new CustomException("Tax code GL not found!");

                                var invoiceTax = new InvoiceTax
                                {
                                    VoucherDetailId = voucherDetailDr.Id,
                                    EmployeePayableId = employeePayable.Id,
                                    TaxCodeId = invoiceTaxVM.TaxCodeId,
                                    TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                    TaxAmount = invoiceTaxVM.TaxAmount,
                                    TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.VendorInvoiceTax.ToString()
                                };
                                taxDrAmount += invoiceTaxVM.TaxAmount;
                                _invoiceTaxService.InsertInvoiceTax(employeePayable, invoiceTax, invoiceTaxPk);

                                // Insert Into Customer Invoice Tax Detail (Withhold GL)
                                withholdgl = taxCode.IsWithhold;
                                if (taxCode.IsWithhold && string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                                    throw new CustomException("Withhold GL is not found of TaxCode " + taxCode.StandardName);
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
                                    totalwithholdCrAmount += invoiceTaxDetail.Amount;
                                    totalVoucherDetailTaxAmount += totalwithholdCrAmount;
                                    _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                    var voucherDetailTax = new VoucherDetail
                                    {
                                        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                        ActivityId = invoiceTaxDetail.ActivityId,
                                        InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                        CrAmount = invoiceTaxDetail.Amount,
                                        PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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
                                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                        ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                    };
                                    totalBaseCurrencyCrAmount += voucherDetailCurrencydb.CrAmount;
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencydb);
                                }

                                // Insert Into Customer Invoice Tax Detail (Creditable GL)
                                creditablegl = taxCode.IsCreditable;
                                if (taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                    throw new CustomException("Creditable GL is not found of TaxCode " + taxCode.StandardName);
                                if (taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                        BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                        ActivityId = taxCodeGL.CreditableGLActivityId,
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
                                        DrAmount = invoiceTaxDetail.Amount,
                                        PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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
                                    totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                                }
                                if (!merge && !taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                    throw new CustomException("Expenses GL is not found of TaxCode " + taxCode.StandardName);
                                if (!merge && !taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                                {
                                    var invoiceTaxDetail = new InvoiceTaxDetail
                                    {
                                        GLGeneralInfoId = taxCodeGL.ExpensesGLId,
                                        BudgetMasterId = taxCodeGL.ExpensesGLBudgetMasterId,
                                        ActivityId = taxCodeGL.ExpensesGLActivityId,
                                        Amount = invoiceTax.TaxAmount,
                                        AType = "Dr"

                                    };
                                    totalExpensesDrAmount += invoiceTaxDetail.Amount;
                                    _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 3);

                                    var voucherDetailTax = new VoucherDetail
                                    {
                                        GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                        BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                        ActivityId = invoiceTaxDetail.ActivityId,
                                        CurrencyId = voucherDetailDr.CurrencyId,
                                        DrAmount = invoiceTaxDetail.Amount,
                                        PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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

                                    totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                                }
                                if (merge && !taxCode.IsCreditable)
                                {
                                    voucherDetailDr.DrAmount += invoiceTax.TaxAmount;
                                    totalAmountDr += invoiceTax.TaxAmount;
                                }
                            }
                        }


                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                        });

                        totalBaseCurrencyDrAmount = 0;
                    }
                }


                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                {
                    var invoiceTaxPk1 = _invoiceTaxService.GetMaxNumber();
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
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            TaxAmount = invoiceTaxVM.TaxAmount,
                            TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                        };

                        _invoiceTaxService.InsertInvoiceTax(employeePayable, invoiceTax, invoiceTaxPk1);

                        // Insert Into Customer Invoice Tax Detail (Withhold GL)
                        withholdgl = taxCode.IsWithhold;
                        if (taxCode.IsWithhold && !string.IsNullOrEmpty(taxCodeGL.WithholdCreditableGLId))
                        {
                            totalAmountCr += invoiceTaxVM.TaxAmount;
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
                        }
                        // Insert Into Customer Invoice Tax Detail (Creditable GL)
                        creditablegl = taxCode.IsCreditable;
                        if (taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                            throw new CustomException("Creditable GL is not found of TaxCode " + taxCode.StandardName);
                        if (taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.CreditableGLId))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.CreditableGLId,
                                BudgetMasterId = taxCodeGL.CreditableGLBudgetMasterId,
                                ActivityId = taxCodeGL.CreditableGLActivityId,
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
                            totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                        }
                        if (!merge && !taxCode.IsCreditable && string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                            throw new CustomException("Expenses GL is not found of TaxCode " + taxCode.StandardName);
                        if (!merge && !taxCode.IsCreditable && !string.IsNullOrEmpty(taxCodeGL.ExpensesGLId))
                        {
                            var invoiceTaxDetail = new InvoiceTaxDetail
                            {
                                GLGeneralInfoId = taxCodeGL.ExpensesGLId,
                                BudgetMasterId = taxCodeGL.ExpensesGLBudgetMasterId,
                                ActivityId = taxCodeGL.ExpensesGLActivityId,
                                Amount = invoiceTax.TaxAmount,
                                AType = "Dr"

                            };
                            totalExpensesDrAmount += invoiceTaxDetail.Amount;
                            _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 3);

                            var voucherDetailTax = new VoucherDetail
                            {
                                GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                ActivityId = invoiceTaxDetail.ActivityId,
                                DrAmount = invoiceTaxDetail.Amount
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

                            totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                        }
                        if (merge && !taxCode.IsCreditable)
                        {
                            //voucherDetailTax.DrAmount += invoiceTax.TaxAmount;
                            totalAmountDr += invoiceTax.TaxAmount;
                        }
                    }
                }


                var employeepayableDetail = new EmployeePayableDetail
                {
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                    Amount = voucherVM.IsExcludingTax ? voucherVM.Amount : voucherVM.Amount + totalcreditableDrAmount,
                    NetAmount = voucherVM.IsExcludingTax ? voucherVM.Amount : voucherVM.Amount - totalwithholdCrAmount,
                };

                employeepayableDetail.Amount = voucherVM.Amount;
                employeepayableDetail.NetAmount = voucherVM.Amount;
                _employeePayableService.InsertEmployeePayableDetail(employeePayable, employeepayableDetail, 1);

                employeePayable.Amount = employeepayableDetail.Amount;
                // INSERT INTO VoucherDetail
                var voucherDetailCr = new VoucherDetail
                {
                    GLGeneralInfoId = employeepayableDetail.GLGeneralInfoId,
                    BudgetMasterId = employeepayableDetail.BudgetMasterId,
                    ActivityId = employeepayableDetail.ActivityId,
                    CurrencyId = voucher.CurrencyId,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = employeePayable.Narration,
                    EmployeeId = employeePayable.EmployeeId,
                    EmployeePayableDetailId = employeepayableDetail.Id,
                    PartyType = employeePayable.PartyType,
                    PostingWithoutTaxAllow = voucherVM.IsExcludingTax,
                    CrAmount = employeepayableDetail.Amount
                };
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                totalAmountCr += voucherDetailCr.CrAmount;

                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailCr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate
                });

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                if (null != tdsVMList && tdsVMList.Count() > 0)
                {
                    var invoiceTaxTDS = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = tdsVMList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = tdsVMList.Sum(r => r.TaxAutoAmount),
                        InventoryReceiveId = null,
                        InvoiceId = null,
                        EmployeePayableId = employeePayable.Id,
                        Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        PartyId = employeePayable.PartyId,
                        PartyPlantId = employeePayable.PartyPlantId
                    };
                    _additionalTaxRepository.Insert(invoiceTaxTDS);

                    int addtionalTaxDetailId = 0;
                    foreach (var invoiceTaxVM in tdsVMList)
                    {

                        if (null == invoiceTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _taxCodeGLRepository.Query(r => r.TaxCodeId == invoiceTaxVM.TaxCodeId).Select().FirstOrDefault();
                        if (null == taxCodeGL)
                            throw new CustomException("Tax code GL not found!");


                        addtionalTaxDetailId++;
                        var invoiceTaxDetailTDS = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL.WithholdCreditableGLId,
                            BudgetMasterId = taxCodeGL.WithholdCreditableBudgetMasterId,
                            ActivityId = taxCodeGL.WithholdCreditableActivityId,
                            Amount = invoiceTaxVM.TaxAmount,
                            AdditionalTaxId = invoiceTaxTDS.Id,
                            TaxCodeId = invoiceTaxVM.TaxCodeId,
                            TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id = MakePK(invoiceTaxTDS.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxDetailRepository.Insert(invoiceTaxDetailTDS);


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


        private static void CheckIsPosted(Voucher voucher)
        {
            if (!voucher.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }
        public string UpdateVendorInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                // Update INTO Invoice

                var invoice = new Invoice();
                invoice = base.Find(voucherVM.Id);
                invoice.Id = voucherVM.Id;
                invoice.Amount = voucherVM.Amount;
                invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                invoice.CompanyCurrencyRate = voucherVM.CompanyCurrencyRate;
                invoice.CurrencyId = voucherVM.CurrencyId;
                invoice.DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId;
                invoice.PostingDate = voucherVM.PostingDate;
                invoice.DocDate = voucherVM.DocDate;
                invoice.DocRefNo = voucherVM.DocRefNo;
                invoice.FiscalYearId = voucherVM.FiscalYearId;
                invoice.FiscalYearPeriodId = voucherVM.FiscalYearPeriodId;
                invoice.IsExcludingTax = voucherVM.IsExcludingTax;
                invoice.Narration = voucherVM.Narration;
                invoice.PartyId = voucherVM.PartyId;
                invoice.PartyPlantId = voucherVM.PartyPlantId;
                invoice.PartyType = voucherVM.PartyType;
                invoice.PaymentTermId = voucherVM.PaymentTermId;
                invoice.SalesTypeId = voucherVM.SalesTypeId;
                invoice.TaxYearId = voucherVM.TaxYearId;
                invoice.TaxYearPeriodId = voucherVM.TaxYearPeriodId;
                invoice.VoucherDate = voucherVM.VoucherDate;
                invoice.VoucherTypeId = voucherVM.VoucherTypeId;
                invoice.EntityId = voucherVM.EntityId;


                // Update INTO Voucher TABLE
                var voucher = new Voucher();
                voucher = _voucherService.FindVoucher(voucherVM.VoucherId);
                CheckIsPosted(voucher);
                voucher.CurrencyId = voucherVM.CurrencyId;
                voucher.PostingDate = voucherVM.PostingDate;
                voucher.DocDate = voucherVM.DocDate;
                voucher.DocRefNo = voucherVM.DocRefNo;
                voucher.FiscalYearId = voucherVM.FiscalYearId;
                voucher.FiscalYearPeriodId = voucherVM.FiscalYearPeriodId;
                voucher.Narration = voucherVM.Narration;
                voucher.VoucherTypeId = voucherVM.VoucherTypeId;
                voucher.EntityId = voucherVM.EntityId;
                _voucherService.UpdateVoucher(voucher);

                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var taxDrAmount = 0.0M;
                var taxCrAmount = 0.0M;
                if (invoice.PaymentSource == PaymentSource.GL.ToString())
                {
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        // in libility side Dr.
                        var voucherDetailDr = new VoucherDetail();
                        voucherDetailDr = _voucherService.FindVoucherDetail(voucherDetailVM.Id);
                        taxDrAmount = 0.0M;

                        if (null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
                        {
                            foreach (var invoiceTaxVM in voucherDetailVM.InvoiceTaxViewModel)
                            {
                                var invoiceTax = new InvoiceTax();
                                invoiceTax = _invoiceTaxService.Query(r => r.Id == invoiceTaxVM.InvoiceTaxId).Select().FirstOrDefault();
                                invoiceTax.TaxAmount = invoiceTaxVM.TaxAmount;
                                _invoiceTaxService.Update(invoiceTax);

                                var invoiceTaxDetail = new InvoiceTaxDetail();
                                invoiceTaxDetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == invoiceTax.Id).Select().FirstOrDefault();
                                invoiceTaxDetail.Amount = invoiceTaxVM.TaxAmount;
                                _invoiceTaxDetailRepository.Update(invoiceTaxDetail);


                                var voucherDetail = new VoucherDetail();
                                voucherDetail = _voucherDetailRepository.Query(r => r.InvoiceTaxDetailId == invoiceTaxDetail.Id).Select().FirstOrDefault();
                                if (invoiceTaxVM.AType == "Dr")
                                {
                                    voucherDetail.DrAmount = invoiceTaxVM.TaxAmount;
                                    totalAmountDr += voucherDetail.DrAmount;
                                    taxDrAmount += invoiceTaxVM.TaxAmount;
                                }
                                else
                                {
                                    voucherDetail.CrAmount = invoiceTaxVM.TaxAmount;
                                    totalAmountCr += voucherDetail.CrAmount;
                                    taxCrAmount += invoiceTaxVM.TaxAmount;
                                }
                                _voucherService.UpdateVoucherDetail(voucher, voucherDetail);

                                var voucherDetailCurrency = new VoucherDetailCurrency();
                                voucherDetailCurrency = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id && r.VoucherDetailId == voucherDetail.Id).Select().FirstOrDefault();
                                if (invoiceTaxVM.AType == "Dr")
                                    voucherDetailCurrency.DrAmount = invoiceTaxVM.TaxAmount * voucherDetailCurrency.ToCurrencyRate;
                                else
                                    voucherDetailCurrency.CrAmount = invoiceTaxVM.TaxAmount * voucherDetailCurrency.ToCurrencyRate;
                                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetail, voucherDetailCurrency);
                            }

                        }
                        voucherDetailDr.DrAmount = voucherDetailVM.TotalAmount - taxDrAmount;
                        voucherDetailDr.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                        voucherDetailDr.ActivityId = voucherDetailVM.ActivityId;
                        _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);
                        totalAmountDr += voucherDetailDr.DrAmount;
                        var voucherDetailCurrencyDr = new VoucherDetailCurrency();
                        voucherDetailCurrencyDr = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id && r.VoucherDetailId == voucherDetailDr.Id).Select().FirstOrDefault();
                        voucherDetailCurrencyDr.DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate;
                        _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrencyDr);
                    }
                }
                else if (invoice.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id not found!");
                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);

                    invoice.CashMasterId = voucherVM.CashMasterId;

                    var voucherDetailDr = _voucherDetailRepository.Query(r => r.VoucherId == voucherVM.VoucherId && r.CashMasterId == voucherVM.CashMasterId).Select().FirstOrDefault();
                    voucherDetailDr.DrAmount = voucherVM.Amount;
                    _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);
                    totalAmountDr += voucherDetailDr.DrAmount;

                    var glTransactionDetail = _gLTransactionDetailRepository.Find(voucherDetailDr.Id);
                    if (cashMaster.CurrencyId == voucherVM.CurrencyId)
                        glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                    else
                        glTransactionDetail.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                    _gLTransactionDetailRepository.Update(glTransactionDetail);

                    var voucherDetailCurrency = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id && r.VoucherDetailId == voucherDetailDr.Id).Select().FirstOrDefault();
                    voucherDetailCurrency.DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate;
                    _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrency);

                }
                var voucherDetailCr = _voucherDetailRepository.Query(r => r.VoucherId == voucher.Id && r.PartyId == voucherVM.PartyId).Select().FirstOrDefault();
                var partyType = PartyType.Vendor.ToString();
                // UPdate INTO InvoiceDetail
                var invoiceDetail = new InvoiceDetail();
                invoiceDetail = _invoiceDetailRepository.Find(voucherDetailCr.InvoiceDetailId);
                invoiceDetail.Amount = voucherVM.Amount - taxCrAmount;
                _invoiceDetailRepository.Update(invoiceDetail);

                invoice.Amount = voucherVM.Amount - taxCrAmount;
                base.UpdateGraph(invoice);
                // Update INTO VoucherDetail
                voucherDetailCr.CrAmount = voucherVM.Amount - taxCrAmount;
                _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);
                totalAmountCr += voucherDetailCr.CrAmount;

                var voucherDetailCurrencyCr = new VoucherDetailCurrency();
                voucherDetailCurrencyCr = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id && r.VoucherDetailId == voucherDetailCr.Id).Select().FirstOrDefault();
                voucherDetailCurrencyCr.CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate;
                _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);

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
        private string GetMultiplePaymentPK()
        {
            return "MP" + base.GetAutoNumber("MultiplePayment", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public string InsertMultiplePaymnet(MultiplePayment entity, IEnumerable<MultiplePaymentDetail> multiplePaymentDetailList)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO InvoiceDetail
                var mulPayment = new MultiplePayment
                {
                    Id = GetMultiplePaymentPK(),
                    CompanyGroupId = entity.CompanyGroupId,
                    CompanyId = entity.CompanyId,
                    PlantId = entity.PlantId,
                    BankMasterId = entity.BankMasterId,
                    ApprovalStatus = entity.ApprovalStatus,
                    SourceType = entity.SourceType,
                    ApprovedBy = null,
                    TentativeDate = entity.TentativeDate,
                    DueUpToDate = entity.DueUpToDate,
                    IsPark = true
                };
                AuditService.AddedLog(mulPayment);
                _multiplePaymentRepository.Insert(mulPayment);

                int mulpayDetailId = 0;
                foreach (var voucherDetailVM in multiplePaymentDetailList)
                {
                    mulpayDetailId++;
                    var mulpayDetail = new MultiplePaymentDetail
                    {
                        Id = MakePK(mulPayment.Id, mulpayDetailId, 2),
                        MultiplePaymentId = mulPayment.Id,
                        InvoiceId = voucherDetailVM.InvoiceId,
                        InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        Amount = voucherDetailVM.Amount,
                        AddedBy = mulPayment.AddedBy,
                        AddedDate = mulPayment.AddedDate,
                        AddedFromIP = mulPayment.AddedFromIP,
                        IsPark = true
                    };
                    _multiplePaymentDetailRepository.Insert(mulpayDetail);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return mulPayment.Id;
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

        public void InsertMultipleVendorAvailableApproved(IEnumerable<MultipleVendorIdViewModel> partyIdList, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                var parallerCurrency = _companyParallelCurrencyService.Query(r => r.CompanyId == voucherVM.CompanyId).Select();
                if (null == parallerCurrency)
                    throw new CustomException("Company Parallel Currency not found!");
                var companyCurrency = parallerCurrency.FirstOrDefault(r => r.ParallelCurrencyType == ParallelCurrencyType.CompanyCurrency.ToString());
                var groupCurrency = parallerCurrency.FirstOrDefault(r => r.ParallelCurrencyType == ParallelCurrencyType.CompanyGroupCurrency.ToString());
                var hardCurrency = parallerCurrency.FirstOrDefault(r => r.ParallelCurrencyType == ParallelCurrencyType.HardCurrency.ToString());
                var companyCurrencyId = companyCurrency != null ? companyCurrency.CurrencyId : throw new CustomException("Company Parallel Currency Id not found!");
                var companyGroupCurrencyId = groupCurrency?.CurrencyId;
                var hardCurrencyId = hardCurrency?.CurrencyId;

                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;

                foreach (var item in partyIdList)
                {
                    decimal totalBankCrAmount = 0, totalBankCrAmountExLoss = 0, totalBankCrAmountExgain = 0,
                        totalBankCrAmountCompanyCurr = 0, totalBankCrAmountGroupCurr = 0, totalBankCrAmountHardCurr = 0,
                        totalBankCrAmountExLossCompanyCurr = 0, totalBankCrAmountExLossGroupCurr = 0, totalBankCrAmountExLossHardCurr = 0,
                        totalBankCrAmountExGainCompanyCurr = 0, totalBankCrAmountExGainGroupCurr = 0, totalBankCrAmountExGainHardCurr = 0;
                    var ranN = new Random();
                    // INSERT INTO InvoiceWriteOff TABLE
                    var invoiceWriteOff = new InvoiceWriteOff
                    {
                        Id = GetInvoiceWriteOffAutoNumber(),
                        CompanyGroupId = item.CompanyGroupId,
                        CompanyId = item.CompanyId,
                        CurrencyId = companyCurrencyId,
                        PartyId = item.PartyId,
                        PartyType = PartyType.Vendor.ToString(),
                        Amount = voucherVM.Amount,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = ranN.Next(1000, 99999).ToString(), //(DateTime.Now.Month.ToString().Substring(2)+ DateTime.Now.Day.ToString().Substring(2) + ranN.Next(1000, 99999)).ToString() ,
                        Narration = "Multiple Vendor Payment",
                        ModelState = ModelState.Added,
                        SourceType = SourceType.VendorPayment.ToString(),
                        VoucherId = null,
                        EmployeeId = null,
                        UpdatedBy = null,
                        UpdatedDate = null,
                        UpdatedFromIP = null,
                        Archive = false
                    };
                    AuditService.AddedLog(invoiceWriteOff);

                    // INSERT INTO Voucher TABLE
                    var voucher = new Voucher
                    {
                        CompanyGroupId = invoiceWriteOff.CompanyGroupId,
                        CompanyId = invoiceWriteOff.CompanyId,
                        // EntityId = invoiceWriteOff.EntityId,
                        CurrencyId = invoiceWriteOff.CurrencyId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = invoiceWriteOff.DocDate,
                        DocRefNo = invoiceWriteOff.DocRefNo,
                        Narration = invoiceWriteOff.Narration,
                        Id = null,
                        Archive = invoiceWriteOff.Archive,
                        IsPark = false,
                        TransactionRefNo = null,
                        SourceType = invoiceWriteOff.SourceType,
                        UpdatedBy = null,
                        UpdatedDate = null,
                        UpdatedFromIP = null,
                        VoucherNo = null,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        ModelState = ModelState.Added
                    };
                    voucher.Id = base.GetAutoNumber("Voucher", PKGeneratorEnum.Auto, null, DateTime.Now);
                    voucher.VoucherNo = base.GetAutoNumber("Voucher" + voucher.CompanyId, PKGeneratorEnum.Daily, null, DateTime.Now);
                    voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                    _voucherService.InsertVoucher(voucher, null);

                    // Set to InvoiceWriteOff
                    invoiceWriteOff.VoucherId = voucher.Id;
                    var invoiceWriteOffDetailPk = GetMaxNumber("InvoiceWriteOffDetail", PKGeneratorEnum.Auto, null, DateTime.Now);
                    // INSERT INTO VoucherDetail

                    var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                    var inviceDbList = Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceDetailRepository.Query(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                    var multiplePaymentIds = voucherDetailVMList.Select(r => r.MultiplePaymentId);
                    var multiplePaymentDbList = _multiplePaymentRepository.Query(r => multiplePaymentIds.Contains(r.Id)).Select().ToList();
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.PartyId == item.PartyId)
                        {
                            #region InvoiceDetail

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
                            _invoiceDetailRepository.Update(invoiceDetail);

                            #endregion InvoiceDetail

                            #region Invoice

                            // TODO: have a gap here if invoice split
                            var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                            invoice.WrittenOffAmount += invoiceDetail.WrittenOffAmount;
                            invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                            invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                            invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                            invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            UpdateGraph(invoice);

                            #endregion Invoice

                            #region MultiplePayment

                            var multiplepayment = multiplePaymentDbList.First(r => r.Id == voucherDetailVM.MultiplePaymentId);
                            multiplepayment.UpdatedBy = invoiceWriteOff.AddedBy;
                            multiplepayment.UpdatedDate = invoiceWriteOff.AddedDate;
                            multiplepayment.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                            multiplepayment.ApprovalStatus = ApprovalStatus.Approved.ToString();
                            multiplepayment.ApprovedBy = invoiceWriteOff.AddedBy;
                            multiplepayment.ApprovedDate = invoiceWriteOff.AddedDate;
                            _multiplePaymentRepository.Update(multiplepayment);

                            #endregion MultiplePayment

                            #region InvoiceWriteOffDetail

                            invoiceWriteOffDetailPk.MaxNumber++;
                            // INSERT INTO InvoiceDetail
                            var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
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
                                Id = invoiceWriteOffDetailPk.MaxNumber.ToString(),
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null,
                                DocDate = invoiceWriteOff.DocDate,
                                DocRefNo = invoiceWriteOff.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffDetailRepository.Insert(invoiceWriteOffDetail);

                            #endregion InvoiceWriteOffDetail

                            // in libility side Cr.
                            var voucherDr = new VoucherDetail
                            {
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
                                DrAmount = voucherDetailVM.ConvertedAmount,
                                CrAmount = 0,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                IsPark = voucher.IsPark,
                                Archive = voucher.Archive,
                                ModelState = voucher.ModelState,
                                PostingWithoutTaxAllow = false,
                                PartyId = invoiceWriteOff.PartyId,
                                PartyType = invoiceWriteOff.PartyType,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                                VoucherId = voucher.Id,
                                AdvanceWriteOffDetailId = null,
                            };
                            totalBankCrAmount += voucherDr.DrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDr, 1);

                            var voucherDetailCurrency = voucherDetailCurrencyVMList.FirstOrDefault(r => r.TrnType == "Dr" && r.GLGeneralInfoId == voucherDr.GLGeneralInfoId && r.DocRefNo == voucherDr.DocRefNo);
                            if (null != voucherDetailCurrency)
                            {
                                // INSERT INTO VoucherDetailCurrency
                                _voucherService.CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, companyGroupCurrencyId, hardCurrencyId, voucherDetailCurrency.CompanyCurrencyDr, voucherDetailCurrency.CompanyGroupCurrencyDr, voucherDetailCurrency);
                                if (!string.IsNullOrEmpty(companyCurrencyId))
                                {
                                    if (voucherDetailCurrency.CompanyCurrencyDr <= 0)
                                        throw new CustomException($"{voucherDetailCurrency.GLGeneralInfoName} GL {voucherDetailCurrency.CompanyCurrencyName} Dr amount must have to greater than zero!");

                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                                    {
                                        DrAmount = voucherDetailCurrency.CompanyCurrencyDr,
                                        FromCurrencyId = voucherDetailCurrency.CompanyFromCurrencyId,
                                        ParallelCurrencyId = voucherDetailCurrency.CompanyCurrencyId,
                                        ToCurrencyConversion = 1 / voucherDetailCurrency.CompanyCurrencyRate,
                                        ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                        ToCurrencyRate = voucherDetailCurrency.CompanyCurrencyRate
                                    });
                                    totalBankCrAmountCompanyCurr += voucherDetailCurrency.CompanyCurrencyDr;
                                }
                                if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                                {
                                    if (voucherDetailCurrency.CompanyGroupCurrencyDr <= 0)
                                        throw new CustomException($"{voucherDetailCurrency.GLGeneralInfoName} GL {voucherDetailCurrency.CompanyGroupCurrencyName} Dr amount must have to greater than zero!");

                                    _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherDr, new VoucherDetailCurrency
                                    {
                                        AddedBy = voucherDr.AddedBy,
                                        AddedDate = voucherDr.AddedDate,
                                        AddedFromIP = voucherDr.AddedFromIP,
                                        CrAmount = 0,
                                        DrAmount = voucherDetailCurrency.CompanyGroupCurrencyDr,
                                        FromCurrencyId = voucherDetailCurrency.CompanyGroupFromCurrencyId,
                                        ParallelCurrencyId = voucherDetailCurrency.CompanyGroupCurrencyId,
                                        ToCurrencyConversion = 1 / voucherDetailCurrency.CompanyGroupCurrencyRate,
                                        ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                        ToCurrencyRate = voucherDetailCurrency.CompanyGroupCurrencyRate,
                                        VoucherDetailId = voucherDr.Id,
                                        VoucherId = voucherDr.VoucherId
                                    });
                                    totalBankCrAmountGroupCurr += voucherDetailCurrency.CompanyGroupCurrencyDr;
                                }
                                if (!string.IsNullOrEmpty(hardCurrencyId))
                                {
                                    if (voucherDetailCurrency.HardCurrencyDr <= 0)
                                        throw new CustomException($"{voucherDetailCurrency.GLGeneralInfoName} GL {voucherDetailCurrency.HardCurrencyName} Dr amount must have to greater than zero!");

                                    _voucherService.InsertVoucherDetailHardCurrency(voucherDr, new VoucherDetailCurrency
                                    {
                                        AddedBy = voucherDr.AddedBy,
                                        AddedDate = voucherDr.AddedDate,
                                        AddedFromIP = voucherDr.AddedFromIP,
                                        CrAmount = 0,
                                        DrAmount = voucherDetailCurrency.HardCurrencyDr,
                                        FromCurrencyId = voucherDetailCurrency.HardFromCurrencyId,
                                        ParallelCurrencyId = voucherDetailCurrency.HardCurrencyId,
                                        ToCurrencyConversion = 1 / voucherDetailCurrency.HardCurrencyRate,
                                        ToCurrencyId = voucherDetailCurrency.ToCurrencyId,
                                        ToCurrencyRate = voucherDetailCurrency.HardCurrencyRate,
                                        VoucherDetailId = voucherDr.Id,
                                        VoucherId = voucherDr.VoucherId
                                    });
                                    totalBankCrAmountHardCurr += voucherDetailCurrency.HardCurrencyDr;
                                }
                            }
                        }
                    }

                    #region Exchange Loss

                    var voucherDetailCurrencyExchangeLoss = voucherDetailCurrencyVMList.FirstOrDefault(r => r.TrnType == "Dr" && r.ExchangeStatus == "ExchangeLoss");
                    if (null != voucherDetailCurrencyExchangeLoss)
                    {
                        var voucherDtEx = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailCurrencyExchangeLoss.GLGeneralInfoId,
                            ActivityId = voucherVM.ActivityId,
                            BudgetMasterId = voucherVM.BudgetMasterId,
                            CurrencyId = voucher.CurrencyId,
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
                            PostingWithoutTaxAllow = false,
                            VoucherId = voucher.Id,
                            IsPark = voucher.IsPark
                        };
                        totalBankCrAmountExLoss += voucherDtEx.DrAmount;
                        _voucherService.InsertVoucherDetail(voucher, voucherDtEx, 1);
                        //CompanyCurrency
                        if (voucherDetailCurrencyExchangeLoss.Exchange == "Base")
                        {
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtEx, new VoucherDetailCurrency
                            {
                                CrAmount = 0,
                                DrAmount = voucherDetailCurrencyExchangeLoss.CompanyCurrencyDr,
                                FromCurrencyId = voucherDetailCurrencyExchangeLoss.CompanyFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrencyExchangeLoss.CompanyCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeLoss.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrency.CurrencyId,
                                ToCurrencyRate = voucherDetailCurrencyExchangeLoss.CompanyCurrencyRate
                            });
                            totalBankCrAmountExLossCompanyCurr += voucherDetailCurrencyExchangeLoss.CompanyCurrencyDr;
                        }
                        if (voucherDetailCurrencyExchangeLoss.Exchange == "Group")
                        {
                            _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherDtEx, new VoucherDetailCurrency
                            {
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                CrAmount = 0,
                                DrAmount = voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyDr,
                                FromCurrencyId = voucherDetailCurrencyExchangeLoss.CompanyGroupFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyRate,
                                ToCurrencyId = companyCurrency.CurrencyId,
                                ToCurrencyRate = voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyRate,
                                VoucherDetailId = voucherDtEx.Id,
                                VoucherId = voucher.Id
                            });
                            totalBankCrAmountExLossGroupCurr += voucherDetailCurrencyExchangeLoss.CompanyGroupCurrencyDr;
                        }
                        if (voucherDetailCurrencyExchangeLoss.Exchange == "Hard")
                        {
                            _voucherService.InsertVoucherDetailHardCurrency(voucherDtEx, new VoucherDetailCurrency
                            {
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                CrAmount = 0,
                                DrAmount = voucherDetailCurrencyExchangeLoss.HardCurrencyDr,
                                FromCurrencyId = voucherDetailCurrencyExchangeLoss.HardFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrencyExchangeLoss.HardCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeLoss.HardCurrencyRate,
                                ToCurrencyId = companyCurrency.CurrencyId,
                                ToCurrencyRate = voucherDetailCurrencyExchangeLoss.HardCurrencyRate,
                                VoucherDetailId = voucherDtEx.Id,
                                VoucherId = voucher.Id
                            });
                            totalBankCrAmountExLossHardCurr += voucherDetailCurrencyExchangeLoss.HardCurrencyDr;
                        }
                    }

                    #endregion Exchange Loss

                    #region Exchange Gain

                    var voucherDetailCurrencyExchangeGain = voucherDetailCurrencyVMList.FirstOrDefault(r => r.TrnType == "Cr" && r.ExchangeStatus == "ExchangeGain");
                    if (null != voucherDetailCurrencyExchangeGain)
                    {
                        var voucherDtExGain = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailCurrencyExchangeGain.GLGeneralInfoId,
                            ActivityId = voucherVM.ActivityId,
                            BudgetMasterId = voucherVM.BudgetMasterId,
                            CurrencyId = voucher.CurrencyId,
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
                            ModelState = ModelState.Added,
                            PostingWithoutTaxAllow = false,
                            VoucherId = voucher.Id,
                            IsPark = voucher.IsPark
                        };
                        totalBankCrAmountExgain += voucherDtExGain.CrAmount;
                        _voucherService.InsertVoucherDetail(voucher, voucherDtExGain, 1);
                        //CompanyCurrency
                        if (voucherDetailCurrencyExchangeGain.Exchange == "Base")
                        {
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDtExGain, new VoucherDetailCurrency
                            {
                                CrAmount = voucherDetailCurrencyExchangeGain.CompanyCurrencyCr,
                                FromCurrencyId = voucherDetailCurrencyExchangeGain.CompanyFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrencyExchangeGain.CompanyCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeGain.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrency.CurrencyId,
                                ToCurrencyRate = voucherDetailCurrencyExchangeGain.CompanyCurrencyRate
                            });
                            totalBankCrAmountExGainCompanyCurr += voucherDetailCurrencyExchangeGain.CompanyCurrencyCr;
                        }
                        if (voucherDetailCurrencyExchangeGain.Exchange == "Group")
                        {
                            _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherDtExGain, new VoucherDetailCurrency
                            {
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                CrAmount = voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyCr,
                                DrAmount = 0,
                                FromCurrencyId = voucherDetailCurrencyExchangeGain.CompanyGroupFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyRate,
                                ToCurrencyId = companyCurrency.CurrencyId,
                                ToCurrencyRate = voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyRate,
                                VoucherDetailId = voucherDtExGain.Id,
                                VoucherId = voucher.Id
                            });
                            totalBankCrAmountExGainGroupCurr += voucherDetailCurrencyExchangeGain.CompanyGroupCurrencyCr;
                        }
                        if (voucherDetailCurrencyExchangeGain.Exchange == "Hard")
                        {
                            _voucherService.InsertVoucherDetailHardCurrency(voucherDtExGain, new VoucherDetailCurrency
                            {
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP,
                                DrAmount = 0,
                                CrAmount = voucherDetailCurrencyExchangeGain.HardCurrencyCr,
                                FromCurrencyId = voucherDetailCurrencyExchangeGain.HardFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrencyExchangeGain.HardCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrencyExchangeGain.HardCurrencyRate,
                                ToCurrencyId = companyCurrency.CurrencyId,
                                ToCurrencyRate = voucherDetailCurrencyExchangeGain.HardCurrencyRate,
                                VoucherDetailId = voucherDtExGain.Id,
                                VoucherId = voucher.Id
                            });
                            totalBankCrAmountExGainHardCurr += voucherDetailCurrencyExchangeGain.HardCurrencyCr;
                        }
                    }

                    #endregion Exchange Gain

                    #region Bank Payment

                    var voucherCr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                        ActivityId = voucherVM.ActivityId,
                        BudgetMasterId = voucherVM.BudgetMasterId,
                        CurrencyId = voucher.CurrencyId,
                        EntityId = voucherVM.EntityId,
                        FiscalYearId = voucher.FiscalYearId,
                        FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        Archive = voucher.Archive,
                        DrAmount = 0,
                        CrAmount = (totalBankCrAmount + totalBankCrAmountExLoss) - totalBankCrAmountExgain,
                        DocDate = voucher.DocDate,
                        DocRefNo = voucher.DocRefNo,
                        Narration = invoiceWriteOff.Narration,
                        ModelState = ModelState.Added,
                        PostingWithoutTaxAllow = false,
                        BankMasterId = voucherVM.BankMasterId,
                        VoucherId = voucher.Id,
                        IsPark = voucher.IsPark
                    };
                    _voucherService.InsertVoucherDetail(voucher, voucherCr, 1);

                    var voucherDetailCurrency2 = voucherDetailCurrencyVMList.FirstOrDefault(r => r.TrnType == "Cr" && r.GLGeneralInfoId == voucherCr.GLGeneralInfoId);
                    if (null != voucherDetailCurrency2)
                    {
                        // INSERT INTO voucherDetailCurrency2
                        _voucherService.CurrencyExchange(voucherVM.CurrencyId, companyCurrencyId, companyGroupCurrencyId, hardCurrencyId,
                                           voucherDetailCurrency2.CompanyCurrencyCr, voucherDetailCurrency2.CompanyGroupCurrencyCr, voucherDetailCurrency2);

                        if (!string.IsNullOrEmpty(companyCurrencyId))
                        {
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                            {
                                CrAmount = (totalBankCrAmountCompanyCurr + totalBankCrAmountExLossCompanyCurr - totalBankCrAmountExGainCompanyCurr),
                                FromCurrencyId = voucherDetailCurrency2.CompanyFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrency2.CompanyCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrency2.CompanyCurrencyRate,
                                ToCurrencyId = voucherDetailCurrency2.ToCurrencyId,
                                ToCurrencyRate = voucherDetailCurrency2.CompanyCurrencyRate
                            });

                            if (companyCurrencyId == voucherVM.BankCurrencyId)
                                voucherVM.BankAmount = voucherDetailCurrency2.CompanyCurrencyCr;
                        }
                        if (!string.IsNullOrEmpty(companyGroupCurrencyId))
                        {
                            _voucherService.InsertVoucherDetailCompanyGroupCurrency(voucherCr, new VoucherDetailCurrency
                            {
                                AddedBy = voucherCr.AddedBy,
                                AddedDate = voucherCr.AddedDate,
                                AddedFromIP = voucherCr.AddedFromIP,
                                CrAmount = (totalBankCrAmountGroupCurr + totalBankCrAmountExLossGroupCurr - totalBankCrAmountExGainGroupCurr),
                                DrAmount = 0,
                                FromCurrencyId = voucherDetailCurrency2.CompanyGroupFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrency2.CompanyGroupCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrency2.CompanyGroupCurrencyRate,
                                ToCurrencyId = voucherDetailCurrency2.ToCurrencyId,
                                ToCurrencyRate = voucherDetailCurrency2.CompanyGroupCurrencyRate,
                                VoucherDetailId = voucherCr.Id,
                                VoucherId = voucherCr.VoucherId
                            });

                            if (companyGroupCurrencyId == voucherVM.BankCurrencyId)
                                voucherVM.BankAmount = voucherDetailCurrency2.CompanyGroupCurrencyCr;
                        }
                        if (!string.IsNullOrEmpty(hardCurrencyId))
                        {
                            if (voucherDetailCurrency2.HardCurrencyCr <= 0)
                                throw new CustomException($"{voucherDetailCurrency2.GLGeneralInfoName} GL {voucherDetailCurrency2.HardCurrencyName} Cr amount must have to greater than zero!");

                            _voucherService.InsertVoucherDetailHardCurrency(voucherCr, new VoucherDetailCurrency
                            {
                                AddedBy = voucherCr.AddedBy,
                                AddedDate = voucherCr.AddedDate,
                                AddedFromIP = voucherCr.AddedFromIP,
                                CrAmount = totalBankCrAmountHardCurr + totalBankCrAmountExLossHardCurr - totalBankCrAmountExGainHardCurr,
                                DrAmount = 0,
                                FromCurrencyId = voucherDetailCurrency2.HardFromCurrencyId,
                                ParallelCurrencyId = voucherDetailCurrency2.HardCurrencyId,
                                ToCurrencyConversion = 1 / voucherDetailCurrency2.HardCurrencyRate,
                                ToCurrencyId = voucherDetailCurrency2.ToCurrencyId,
                                ToCurrencyRate = voucherDetailCurrency2.HardCurrencyRate,
                                VoucherDetailId = voucherCr.Id,
                                VoucherId = voucherCr.VoucherId
                            });

                            if (hardCurrencyId == voucherVM.BankCurrencyId)
                                voucherVM.BankAmount = voucherDetailCurrency2.HardCurrencyCr;
                        }
                    }

                    if (!string.IsNullOrEmpty(voucherCr.BankMasterId))
                    {
                        // INSRT INTO GLTransactionDetail TABLE
                        _voucherService.InsertGLTransactionDetail(voucherCr, new GLTransactionDetail
                        {
                            AddedBy = voucherCr.AddedBy,
                            AddedDate = voucherCr.AddedDate,
                            AddedFromIP = voucherCr.AddedFromIP,
                            BankMasterId = voucherCr.BankMasterId,
                            DrAmount = voucherVM.BankAmount,
                            VoucherDetailId = voucherCr.Id
                        });
                    }

                    #endregion Bank Payment

                    _invoiceWriteOffRepository.Insert(invoiceWriteOff);
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

        private string GetInvoiceWriteOffAutoNumber()
        {
            return base.GetAutoNumber("InvoiceWriteOff", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(Voucher entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.DocRefNo == entity.DocRefNo && r.Id != entity.Id && r.CompanyId == entity.CompanyId);
        }

        public IQueryFluent<InvoiceDetail> GetInvoiceDetailList(Expression<Func<InvoiceDetail, bool>> query)
        {
            return _invoiceDetailRepository.Query(query);
        }

        public void Post(string invoiceId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var invoice = Find(invoiceId);
                CheckIsPosted(invoice);

                invoice.IsPark = false;
                base.UpdateGraph(invoice);
                _voucherService.PostVoucher(invoice.VoucherId);
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

        private static void CheckIsPosted(Invoice invoice)
        {
            if (!invoice.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }

        public InvoiceDetail FindInvoiceDetail(string invoiceDetailId)
        {
            return _invoiceDetailRepository.Find(invoiceDetailId);
        }

        public void DeleteInvoice(string invoiceId, string voucherId)
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
                var invoice = base.Find(invoiceId);
                var invoiceDetail = _invoiceDetailRepository.Query(r => r.InvoiceId == invoiceId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.InvoiceId == invoiceId).Select().ToList();
                var invoiceTDS = _additionalTaxRepository.Query(r => r.InvoiceId == invoiceId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"UPDATE [TRN].InvoiceTax SET VoucherDetailId=NULL WHERE Id='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }
                }
                if (invoiceTDS.Count>0)
                {
                    foreach (var item in invoiceTDS)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"DELETE [TRN].AdditionalTaxDetail  WHERE AdditionalTaxId='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        _additionalTaxRepository.Delete(item.Id);
                    }
                }
                foreach (var item in voucherdetail)
                {
                    var gltransaction = _gLTransactionDetailRepository.Query(r => r.VoucherDetailId == item.Id).Select().ToList();
                    if (gltransaction.Count>0)
                    {
                        foreach (var item1 in gltransaction)
                        {
                            _gLTransactionDetailRepository.Delete(item1.Id);

                        }

                    }
                    _voucherDetailRepository.Delete(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var invoicetaxDdetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == item.Id).Select().ToList();
                        foreach (var item1 in invoicetaxDdetail)
                        {
                            _invoiceTaxDetailRepository.Delete(item1.Id);
                        }
                        _invoiceTaxRepository.Delete(item.Id);
                    }
                }
                foreach (var item in invoiceDetail)
                {
                    _invoiceDetailRepository.Delete(item.Id);
                }
                base.Delete(invoiceId);
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

        #region InvoiceOverHead


        public string InsertInvoiceOverhead(VoucherViewModel voucherVM, IEnumerable<ServiceChargesViewModel> voucherDetailVMList
           , IEnumerable<ServiceChargesTaxViewModel> taxDetailVMList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                // INSERT INTO Invoice
                var invoiceServiceMasterChargesPK = "SMC" + base.GetAutoNumber(nameof(InvoiceServiceMasterCharges), PKGeneratorEnum.Yearly, null, DateTime.Now);
                var invoiceServiceMasterChargesDetailId = 0;
                var invoiceServiceMasterChargesTaxId = 0;

                var invoiceServiceMasterCharges = new InvoiceServiceMasterCharges
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = voucherVM.CurrencyId,
                    CompanyCurrencyRate = voucherVM.CompanyCurrencyRate,
                    IsNonCreditable = voucherVM.IsExcludingTax,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType,
                    PaymentTermId = voucherVM.PaymentTermId,
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    IsPark = voucherVM.IsPark,
                    ActualDueDate = voucherVM.BaseOnDueDate,
                    RevisedDueDate = voucherVM.BaseOnDueDate,
                    Narration = voucherVM.Narration,
                };
                if (voucherVM.Id == null)
                {
                    invoiceServiceMasterCharges.Id = invoiceServiceMasterChargesPK;
                    AuditService.AddedLog(invoiceServiceMasterCharges);
                    _invoiceServiceMasterChargesRepository.Insert(invoiceServiceMasterCharges);
                }
                else
                {
                    invoiceServiceMasterCharges.Id = voucherVM.Id;
                    AuditService.UpdatedLog(invoiceServiceMasterCharges);
                    _invoiceServiceMasterChargesRepository.Update(invoiceServiceMasterCharges);
                }

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invServiceMasterChargesDetail = new InvoiceServiceMasterChargesDetail();
                    if (voucherDetailVM.Id == null)
                    {
                        invoiceServiceMasterChargesDetailId++;

                        invServiceMasterChargesDetail.Id = MakePK(invoiceServiceMasterChargesPK, invoiceServiceMasterChargesDetailId, 2);
                        invServiceMasterChargesDetail.OverHeadTypeId = voucherDetailVM.OverHeadTypeId;
                        invServiceMasterChargesDetail.InvoiceServiceMasterChargesId = invoiceServiceMasterCharges.Id;
                        invServiceMasterChargesDetail.TransactionAmount = voucherDetailVM.TransactionAmount;
                        invServiceMasterChargesDetail.TotalTaxAmount = voucherDetailVM.TotalTaxAmount;
                        invServiceMasterChargesDetail.AddedBy = invoiceServiceMasterCharges.AddedBy;
                        invServiceMasterChargesDetail.AddedFromIP = invoiceServiceMasterCharges.AddedFromIP;
                        invServiceMasterChargesDetail.AddedDate = invoiceServiceMasterCharges.AddedDate;
                        _invoiceServiceMasterChargesDetailRepository.Insert(invServiceMasterChargesDetail);

                    }
                    else
                    {
                        var updateinvServiceMasCharDetail = _invoiceServiceMasterChargesDetailRepository.Find(voucherDetailVM.Id);
                        _invoiceServiceMasterChargesDetailRepository.Update(updateinvServiceMasCharDetail);
                    }
                    if (null != taxDetailVMList)
                    {
                        foreach (var item in taxDetailVMList.Where(r => r.OverHeadTypeId == voucherDetailVM.OverHeadTypeId))
                        {
                            if (item.Id == null)
                            {
                                invoiceServiceMasterChargesTaxId++;
                                var invoiceServiceMasterChargesTax = new InvoiceServiceMasterChargesTax
                                {
                                    Id = MakePK(invoiceServiceMasterChargesPK, invoiceServiceMasterChargesTaxId, 3),
                                    TaxCategoryId = item.TaxCategoryId,
                                    TaxAmount = item.TaxAmount,
                                    InvoiceServiceMasterChargesId = invoiceServiceMasterCharges.Id,
                                    InvoiceServiceMasterChargesDetailId = invServiceMasterChargesDetail.Id,
                                    HSNCodeId = item.HSNCodeId,
                                    Percentage = item.Percentage
                                };
                                AuditService.AddedLog(invoiceServiceMasterChargesTax);
                                _invoiceServiceMasterChargesTaxRepository.Insert(invoiceServiceMasterChargesTax);
                            }
                            else
                            {
                                var invoiceSvMasterTax = _invoiceServiceMasterChargesTaxRepository.Find(item.Id);
                                AuditService.UpdatedLog(invoiceSvMasterTax);
                                invoiceSvMasterTax.TaxAmount = item.TaxAmount;
                                _invoiceServiceMasterChargesTaxRepository.Update(invoiceSvMasterTax);
                            }

                        }
                    }
                }

                if (null != invoiceDetailChargesList)
                {

                    var invoiceDetailChargesId = base.GetAutoNumber(nameof(InvoiceDetailCharges), PKGeneratorEnum.Yearly, null, DateTime.Now);
                    var invoiceChargesId = 0;
                    foreach (var item in invoiceDetailChargesList)
                    {
                        if (item.Id == null)
                        {
                            invoiceChargesId++;
                            var invoiceCharges = new InvoiceDetailCharges
                            {
                                Id = MakePK(invoiceDetailChargesId, invoiceChargesId, 2),
                                InvoiceDetailId = item.InvoiceDetailId,
                                InvoiceId = item.InvoiceId,
                                DistributedAmount = item.DistributedAmount,
                                InvoiceServiceMasterChargesId = invoiceServiceMasterCharges.Id,
                                Amount = item.Amount,
                                InvoiceType = item.InvoiceType
                            };
                            AuditService.AddedLog(invoiceCharges);
                            _invoiceDetailChargesRepository.Insert(invoiceCharges);
                        }
                        else
                        {
                            var invoiceCharges = _invoiceDetailChargesRepository.Find(item.Id);
                            invoiceCharges.DistributedAmount = item.DistributedAmount;
                            AuditService.UpdatedLog(invoiceCharges);
                            _invoiceDetailChargesRepository.Update(invoiceCharges);
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return invoiceServiceMasterCharges.Id;
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

        public string InsertInvoiceOverheadPost(VoucherViewModel voucherVM, IEnumerable<ServiceChargesViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                var invoiceserviceMasterCharges = _invoiceServiceMasterChargesRepository.Find(voucherVM.Id);
                invoiceserviceMasterCharges.IsPark = false;
                _invoiceServiceMasterChargesRepository.Update(invoiceserviceMasterCharges);

                var taxDetailVMList = _invoiceServiceMasterChargesTaxRepository.Query(r => r.InvoiceServiceMasterChargesId == voucherVM.Id).Select().ToList();
                // INSERT INTO Invoice
                voucherVM.Amount = voucherDetailVMList.Sum(r => r.TransactionAmount) + taxDetailVMList.Sum(r => r.TaxAmount);
                voucherVM.VoucherDate = DateTime.Now;
                voucherVM.InvoiceServiceMasterChargesId = voucherVM.Id;
                var invoice = InsertInvoice(voucherVM);

                // INSERT INTO Voucher TABLE
                var voucher = _voucherService.InsertVoucher(voucherVM);


                // Set to Invoice
                invoice.VoucherId = voucher.Id;

                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var currentVoucherDetailId = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    // in libility side Dr.
                    //var invMaterial = _sqlRepository.GetModelCollection<ServiceChargesViewModel>(@"SELECT * FROM [HKP].[ServiceMasterCharges] WHERE Id='" + voucherDetailVM.ServiceMasterChargesId + "'").FirstOrDefault();
                    if (voucherDetailVM.ExpensesGLId == null)
                        throw new CustomException("GL not found!");
                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.ExpensesGLId,
                        BudgetMasterId = voucherDetailVM.ExpensesBudgetMasterId,
                        ActivityId = voucherDetailVM.ExpensesActivityId,
                        DrAmount = voucherDetailVM.TransactionAmount,
                        PostingWithoutTaxAllow = invoice.IsExcludingTax,
                        TrnNature = TransactionNature.Purchases.ToString(),
                        PaymentSource = PaymentSource.GL.ToString()
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    totalAmountDr += voucherDetailDr.DrAmount;


                    if (null != taxDetailVMList)
                    {
                        foreach (var item in taxDetailVMList.Where(r => r.InvoiceServiceMasterChargesDetailId == voucherDetailVM.Id))
                        {


                            var taxCategoryGl = _taxCategoryGlService.Query(r => r.TaxCategoryId == item.TaxCategoryId && r.InputTaxOutPutTax == "Input").Select().FirstOrDefault();
                            if (null == taxCategoryGl)
                                throw new CustomException("Tax Category not found!");

                            var invoiceTax = new InvoiceTax
                            {
                                VoucherDetailId = voucherDetailDr.Id,
                                InvoiceId = invoice.Id,
                                TaxCategoryId = item.TaxCategoryId,
                                TaxAmount = item.TaxAmount,
                                TaxAutoAmount = item.TaxAmount,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.InvoiceCharge.ToString()
                            };
                            _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);


                            if (!string.IsNullOrEmpty(taxCategoryGl.GLGeneralInfoId))
                            {
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    GLGeneralInfoId = taxCategoryGl.GLGeneralInfoId,
                                    BudgetMasterId = taxCategoryGl.BudgetMasterId,
                                    ActivityId = taxCategoryGl.ActivityId,
                                    Amount = invoiceTax.TaxAmount,
                                    AType = "Dr"
                                };
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 2);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    DrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetailDr.PostingWithoutTaxAllow
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
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                            }
                        }
                    }

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                    });

                }


                var partyType = PartyType.Vendor.ToString();
                var companyParty = _companyPartyRepository.Query(r => r.CompanyId == invoice.CompanyId && r.PlantId == invoice.PlantId && r.PartyId == invoice.PartyId && r.PartyType == partyType).Select().FirstOrDefault();
                if (null == companyParty)
                    throw new CustomException("Plant party mapping not found!");
                var companyPartyGLList = _companyPartyGLRepository.Query(r => r.PartyId == companyParty.PartyId && r.CompanyPartyId == companyParty.Id).Select().ToList();
                if (null == companyPartyGLList)
                    throw new CustomException("Party GL not found!");

                var reconGL = PartyGLType.ReconciliationGL.ToString();
                var regularGL = companyPartyGLList.FirstOrDefault(r => r.PartyGLType == reconGL);
                if (null == regularGL)
                    throw new CustomException("Party Reconciliation GL not found!");

                // INSERT INTO InvoiceDetail
                var invoiceDetail = new InvoiceDetail
                {
                    GLGeneralInfoId = regularGL.GLGeneralInfoId,
                    BudgetMasterId = regularGL.BudgetMasterId,
                    ActivityId = regularGL.ActivityId,
                    Amount = voucherDetailVMList.Sum(r => r.TransactionAmount),
                    NetAmount = voucherDetailVMList.Sum(r => r.TransactionAmount) + taxDetailVMList.Sum(r => r.TaxAmount),
                    TaxAmount = taxDetailVMList.Sum(r => r.TaxAmount)
                };

                InsertInvoiceDetail(invoice, invoiceDetail, 1);

                // INSERT INTO VoucherDetail
                var voucherDetailCr = new VoucherDetail
                {
                    GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                    BudgetMasterId = invoiceDetail.BudgetMasterId,
                    ActivityId = invoiceDetail.ActivityId,
                    CurrencyId = voucher.CurrencyId,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = invoice.Narration,
                    EmployeeId = invoice.EmployeeId,
                    InvoiceDetailId = invoiceDetail.Id,
                    PartyType = invoice.PartyType,
                    PartyId = invoice.PartyId,
                    PartyPlantId = invoice.PartyPlantId,
                    PostingWithoutTaxAllow = invoice.IsExcludingTax,
                    CrAmount = voucherDetailVMList.Sum(r => r.TransactionAmount) + taxDetailVMList.Sum(r => r.TaxAmount)
                };
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                totalAmountCr += voucherDetailCr.CrAmount;

                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailCr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate
                });


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

        public void DeleteInvoiceOverhead(string invoiceId, string voucherId)
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
                var invoice = base.Find(invoiceId);
                var invoiceDetail = _invoiceDetailRepository.Query(r => r.InvoiceId == invoiceId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.InvoiceId == invoiceId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"UPDATE [TRN].InvoiceTax SET VoucherDetailId=NULL WHERE Id='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }
                }
                foreach (var item in voucherdetail)
                {

                    _voucherDetailRepository.Delete(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var invoicetaxDdetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == item.Id).Select().ToList();
                        foreach (var item1 in invoicetaxDdetail)
                        {
                            _invoiceTaxDetailRepository.Delete(item1.Id);
                        }
                        _invoiceTaxRepository.Delete(item.Id);
                    }
                }
                foreach (var item in invoiceDetail)
                {
                    _invoiceDetailRepository.Delete(item.Id);
                }
                base.Delete(invoiceId);
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

        #endregion



    }
}