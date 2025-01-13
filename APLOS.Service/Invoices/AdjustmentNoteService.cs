using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Taxations;
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
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.Invoices
{
    public class AdjustmentNoteService : Service<AdjustmentNote>, IAdjustmentNoteService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;
        private readonly IRepositoryAsync<AdjustmentNoteDetail> _adjustmentNoteDetailRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<AdditionalTax> _additionalTaxRepository;
        private readonly IRepositoryAsync<AdditionalTaxDetail> _additionalTaxDetailRepository;

        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly IVoucherService _voucherService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly IRepositoryAsync<FinancingTypeGL> _financingTypeGLRepository;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly ITaxCategoryGLService _taxCategoryGLService;
        private readonly IRepositoryAsync<TaxCode> _taxCodeRepository;
        private readonly IRepositoryAsync<TaxCodeGL> _taxCodeGLRepository;
        private readonly IRepositoryAsync<InvoiceDetailCharges> _invoiceDetailChargesRepository;

        public AdjustmentNoteService(
              IRepositoryAsync<AdjustmentNote> repository
            , IRepositoryAsync<AdjustmentNoteDetail> adjustmentNoteDetailRepository
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            , IRepositoryAsync<AdditionalTax> additionalTaxRepository
            , IRepositoryAsync<AdditionalTaxDetail> additionalTaxDetailRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository
            , ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , ICompanyTaxYearService companyTaxYearService
            , IVoucherService voucherService
            , ICompanyFiscalYearService companyFiscalYearService
            , IRepositoryAsync<FinancingTypeGL> financingTypeGLRepository
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository
            , ITaxCategoryGLService taxCategoryGLService
            , IRepositoryAsync<TaxCode> taxCodeRepository
            , IRepositoryAsync<TaxCodeGL> taxCodeGLRepository
            , IRepositoryAsync<InvoiceDetailCharges> invoiceDetailChargesRepository
            ) : base(repository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _adjustmentNoteDetailRepository = adjustmentNoteDetailRepository;
            _invoiceTaxRepository = invoiceTaxRepository;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _additionalTaxRepository = additionalTaxRepository;
            _additionalTaxDetailRepository = additionalTaxDetailRepository;
            _voucherRepository = voucherRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _companyTaxYearService = companyTaxYearService;
            _voucherService = voucherService;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _companyFiscalYearService = companyFiscalYearService;
            _financingTypeGLRepository = financingTypeGLRepository;
            _invoiceTaxService = invoiceTaxService;
            _taxCategoryGLService = taxCategoryGLService;
            _taxCodeRepository = taxCodeRepository;
            _taxCodeGLRepository = taxCodeGLRepository;
            _invoiceDetailChargesRepository = invoiceDetailChargesRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, A.Id, A.Id AS AdjustmentNoteId, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId, PP.UserName AS PartyPlantName, A.VoucherId, A.PostingDate, A.DocDate
                                , A.DocRefNo, A.CurrencyId, C.Code AS CurrencyCode, A.Amount, A.IsPark, A.PartyType,[Status]=case when A.IsPark=0 Then 'Posted' Else 'Parked' END
                                ,IsExpenseDistribution=CASE WHEN ISNULL((select COUNT(ID.Id) from TRN.InvoiceDetailCharges ID
										INNER JOIN TRN.VoucherDetail VD ON VD.Id=ID.VoucherDetailId
										WHERE VD.VoucherId=A.VoucherId),0)>0 THEN 1 ELSE 0 END
                                ,E.UserName EntityName,V.EntityId,V.Narration
                                FROM [TRN].[AdjustmentNote] AS A
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=V.EntityId
                                WHERE A.Archive=0 AND V.Archive=0 AND A.CompanyGroupId='" + companyGroupId + "'AND A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' AND A.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public string InsertCreditNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList
            , IEnumerable<InvoiceTaxViewModel> additionalTaxList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                decimal totalVoucherDetailTaxAmount = 0;
                decimal totalcreditableDrAmount = 0, totalcreditableCrAmount=0, totalExpensesDrAmount = 0, totalExpensesCrAmount = 0, totalwithholdCrAmount = 0, totalwithholdDrAmount = 0, taxDrAmount = 0;
                decimal totalcreditableDrAmountAddTax = 0, totalExpensesDrAmountAddTax = 0, totalwithholdCrAmountAddTax = 0, taxDrAmountAddTax = 0;
                decimal totalBaseCurrencyCrAmount = 0;
                decimal totalBaseCurrencyDrAmount = 0;
                decimal totalAPBaseCurrencyCrAmount = 0;
                decimal totalAPBaseCurrencyDrAmount = 0;
                var creditablegl = false;
                var withholdgl = false;
                var merge = false;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();

                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                if (null != invoiceTaxVMList && invoiceTaxVMList.Count() > 0)
                    voucherVM.Amount += invoiceTaxVMList.Sum(r => r.TaxAmount);
                // INSERT INTO AdjustmentNote
                var adjustmentNote = InsertAdjustmentNote(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set VoucherId
                adjustmentNote.VoucherId = voucher.Id;
                var currentVoucherDetailId = 0;
                
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (adjustmentNote.SettlementType == SettlementType.Invoice.ToString())
                    {
                        var voucherDetailDb = _voucherService.FindVoucherDetail(voucherDetailVM.Id);
                        voucherDetailVM.GLGeneralInfoId = voucherDetailDb.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = voucherDetailDb.BudgetMasterId;
                        voucherDetailVM.ActivityId = voucherDetailDb.ActivityId;
                    }
                    var voucherDetaiSales = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        TrnNature = TransactionNature.Sales.ToString(),
                        DrAmount = voucherDetailVM.Amount
                    };
                    totalAmountDr += voucherDetaiSales.DrAmount;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetaiSales, currentVoucherDetailId);

                    if (null != invoiceDetailChargesList && invoiceDetailChargesList.Count() > 0 && voucherDetailVM.IsOrderSpecific == true)
                    {

                        foreach (var item in invoiceDetailChargesList.Where(r => r.GLGeneralInfoId == voucherDetailVM.GLGeneralInfoId && r.BudgetMasterId == voucherDetailVM.BudgetMasterId && r.ActivityId == voucherDetailVM.ActivityId))
                        {

                            if (CheckInvoiceDetailActivity(item.InvoiceDetailId, item.ActivityId,voucher.SourceType) == true)
                                throw new CustomException("InvoiceDetailId " + item.InvoiceDetailId + " and Activity " + voucherDetailVM.ActivityName + " already distributed!");

                            var invoiceDetailChargesId = base.GetAutoNumber(nameof(InvoiceDetailCharges), PKGeneratorEnum.Yearly, null, DateTime.Now);
                            var invoiceChargesId = 0;
                            if (item.Id == null)
                            {
                                invoiceChargesId++;
                                var invoiceCharges = new InvoiceDetailCharges
                                {
                                    Id = MakePK(invoiceDetailChargesId, invoiceChargesId, 2),
                                    InvoiceDetailId = item.InvoiceDetailId,
                                    InvoiceId = item.InvoiceId,
                                    DistributedAmount = item.DistributedAmount,
                                    InvoiceServiceMasterChargesId = null,
                                    VoucherDetailId = voucherDetaiSales.Id,
                                    Amount = item.Amount,
                                    InvoiceType = item.InvoiceType,
                                    MasterOrderId = item.MasterOrderId,
                                    ContractId = item.ContractId
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

                    if (voucherVM.PartyType==PartyType.Vendor.ToString() && null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
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
                                VoucherDetailId = voucherDetaiSales.Id,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = Math.Round(invoiceTaxVM.TaxAmount, 4),
                                TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.VendorInvoiceTax.ToString(),
                                AdjustmentNoteId= adjustmentNote.Id
                            };
                            taxDrAmount += Math.Round(invoiceTaxVM.TaxAmount, 4);
                            _invoiceTaxService.InsertInvoiceTax(adjustmentNote, invoiceTax, invoiceTaxPk);

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
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
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
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
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
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    CurrencyId = voucherDetaiSales.CurrencyId,
                                    DrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
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
                                voucherDetaiSales.DrAmount += invoiceTax.TaxAmount;
                                totalAmountDr += invoiceTax.TaxAmount;
                            }
                        }
                    }
                    if (voucherVM.PartyType == PartyType.Customer.ToString() && null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
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
                                VoucherDetailId = voucherDetaiSales.Id,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = Math.Round(invoiceTaxVM.TaxAmount, 4),
                                TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.CustomerInvoiceTax.ToString(),
                                AdjustmentNoteId = adjustmentNote.Id
                            };
                            taxDrAmount += Math.Round(invoiceTaxVM.TaxAmount, 4);
                            _invoiceTaxService.InsertInvoiceTax(adjustmentNote, invoiceTax, invoiceTaxPk);

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
                                    AType = "Dr"
                                };
                                totalwithholdDrAmount += invoiceTaxDetail.Amount;
                                totalVoucherDetailTaxAmount += totalwithholdDrAmount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    DrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
                                };
                                totalAmountDr += voucherDetailTax.DrAmount;
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
                                totalBaseCurrencyDrAmount += voucherDetailCurrencydb.DrAmount;
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
                                totalcreditableCrAmount += invoiceTaxDetail.Amount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 2);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    DrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
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
                                totalBaseCurrencyCrAmount += voucherDetailCurrencybase.DrAmount;
                                totalAPBaseCurrencyCrAmount += voucherDetailCurrencybase.DrAmount;
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
                                    AType = "Cr"

                                };
                                totalExpensesCrAmount += invoiceTaxDetail.Amount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 3);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    CurrencyId = voucherDetaiSales.CurrencyId,
                                    CrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
                                };
                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);
                                totalAmountCr += voucherDetailTax.CrAmount;
                                var voucherDetailCurrencybase = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherVM.CurrencyId,
                                    CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                };

                                totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                            }
                            //if (merge && !taxCode.IsCreditable)
                            //{
                            //    voucherDetaiSales.CrAmount += invoiceTax.TaxAmount;
                            //    totalAmountCr += invoiceTax.TaxAmount;
                            //}
                        }
                    }

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetaiSales, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetaiSales.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetaiSales.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherVM.CompanyCurrencyRate * voucherDetaiSales.DrAmount
                    });
                }
                var gl = GetCreditNoteGL(adjustmentNote.CompanyId, voucherVM.FinancingTypeId);
                var adjustmentNoteDetail = new AdjustmentNoteDetail
                {
                    GLGeneralInfoId = gl.LiabilityGLId,
                    BudgetMasterId = gl.LiabilityBudgetMasterId,
                    ActivityId = gl.LiabilityActivityId,
                };
                if (voucherVM.PartyType == "Customer")
                {
                    if (voucherVM.PartyType == PartyType.Customer.ToString() && null != additionalTaxList && additionalTaxList.Count() > 0)
                    {
                        adjustmentNoteDetail.Amount = adjustmentNote.Amount + totalwithholdDrAmount + totalcreditableCrAmount - additionalTaxList.Sum(r => r.TaxAmount);
                    }
                    else
                    adjustmentNoteDetail.Amount = adjustmentNote.Amount  +  totalwithholdDrAmount+ totalcreditableCrAmount;

                }
                else
                {
                    if (voucherVM.PartyType == PartyType.Vendor.ToString() && null != additionalTaxList && additionalTaxList.Count() > 0)
                    {
                        adjustmentNoteDetail.Amount = adjustmentNote.Amount - totalwithholdDrAmount + taxDrAmount - additionalTaxList.Sum(r => r.TaxAmount);
                    }
                    else
                        adjustmentNoteDetail.Amount = adjustmentNote.Amount - totalwithholdCrAmount + totalBaseCurrencyDrAmount;

                }
                InsertAdjustmentNoteDetail(adjustmentNote, adjustmentNoteDetail, 1);



                // INSERT INTO VoucherDetail
                var voucherDetail = new VoucherDetail
                {
                    GLGeneralInfoId = adjustmentNoteDetail.GLGeneralInfoId,
                    BudgetMasterId = adjustmentNoteDetail.BudgetMasterId,
                    ActivityId = adjustmentNoteDetail.ActivityId,
                    EntityId = voucher.EntityId,
                    PartyType = adjustmentNote.PartyType,
                    PartyId = adjustmentNote.PartyId,
                    PartyPlantId = adjustmentNote.PartyPlantId,
                    TrnNature = TransactionNature.CreditNote.ToString(),
                    AdjustmentNoteDetailId = adjustmentNoteDetail.Id,
                    CrAmount = adjustmentNoteDetail.Amount
                };
                totalAmountCr += voucherDetail.CrAmount;
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    CrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.CrAmount
                });

                if (null != invoiceTaxVMList && invoiceTaxVMList.Count() > 0)
                {
                    var invoiceTaxPk1 = _invoiceTaxService.GetMaxNumber();
                    var currentInvoiceTaxDetailId = 0;
                    foreach (var invoiceTaxVM in invoiceTaxVMList)
                    {
                        var invoiceTax = new InvoiceTax
                        {
                            TaxAmount = invoiceTaxVM.TaxAmount,
                            TaxAutoAmount = invoiceTaxVM.TaxAmount,
                        };
                        _invoiceTaxService.InsertInvoiceTax(adjustmentNote, invoiceTax, invoiceTaxPk1);

                        if (adjustmentNote.PartyType == PartyType.Customer.ToString())
                        {
                            var taxCategoryGL = _taxCategoryGLService.GetTaxCategoryGLOutput(invoiceTaxVM.TaxCategoryId);
                            if (null == taxCategoryGL)
                                throw new CustomException("Output type Tax GL not found!"); invoiceTaxVM.GLGeneralInfoId = taxCategoryGL.GLGeneralInfoId;
                            invoiceTaxVM.BudgetMasterId = taxCategoryGL.BudgetMasterId;
                            invoiceTaxVM.ActivityId = taxCategoryGL.ActivityId;
                        }
                        else if (adjustmentNote.PartyType == PartyType.Vendor.ToString())
                        {
                            var taxCategoryGL = _taxCategoryGLService.GetTaxCategoryGLInput(invoiceTaxVM.TaxCategoryId);
                            if (null == taxCategoryGL)
                                throw new CustomException("Input type Tax GL not found!");
                            invoiceTaxVM.GLGeneralInfoId = taxCategoryGL.GLGeneralInfoId;
                            invoiceTaxVM.BudgetMasterId = taxCategoryGL.BudgetMasterId;
                            invoiceTaxVM.ActivityId = taxCategoryGL.ActivityId;
                        }
                        var invoiceTaxDetail = new InvoiceTaxDetail
                        {
                            GLGeneralInfoId = invoiceTaxVM.GLGeneralInfoId,
                            BudgetMasterId = invoiceTaxVM.BudgetMasterId,
                            ActivityId = invoiceTaxVM.ActivityId,
                            Amount = invoiceTax.TaxAmount,
                            AType = "Dr"
                        };
                        currentInvoiceTaxDetailId++;
                        _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, currentInvoiceTaxDetailId);

                        var voucherDetailTax = new VoucherDetail
                        {
                            InvoiceTaxDetailId = invoiceTaxDetail.Id,
                            GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                            ActivityId = invoiceTaxDetail.ActivityId,
                            DrAmount = invoiceTaxDetail.Amount,
                            TrnNature = TransactionNature.SalesTax.ToString()
                        };
                        totalAmountDr += voucherDetailTax.DrAmount;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailTax.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTax.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailTax.DrAmount
                        });
                    }
                }
                if (voucherVM.PartyType == PartyType.Vendor.ToString() && null != additionalTaxList && additionalTaxList.Count() > 0)
                {
                    AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                    var tdsTax = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = additionalTaxList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = additionalTaxList.Sum(r => r.TaxAutoAmount),
                        PartyId = voucherVM.PartyId,
                        VoucherId = voucher.Id,
                        SourceType = voucher.SourceType,
                        PartyPlantId = voucherVM.PartyPlantId,
                        AdjustmentNoteId = adjustmentNote.Id,
                        Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _additionalTaxRepository.Insert(tdsTax);

                    int addtionalTaxDetailId = 0;
                    foreach (var tdsTaxVM in additionalTaxList)
                    {

                        if (null == tdsTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _accountsCommonService.GetTaxCodeGL(tdsTaxVM.TaxCodeId);


                        addtionalTaxDetailId++;
                        var tdsTaxDetail = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                            BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                            ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                            Amount = tdsTaxVM.TaxAmount,
                            AdditionalTaxId = tdsTax.Id,
                            TaxCodeId = tdsTaxVM.TaxCodeId,
                            TaxCategoryId = tdsTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id = MakePK(tdsTax.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxDetailRepository.Insert(tdsTaxDetail);

                        var voucherDetailCr = new VoucherDetail
                        {
                            GLGeneralInfoId = tdsTaxDetail.GLGeneralInfoId,
                            BudgetMasterId = tdsTaxDetail.BudgetMasterId,
                            ActivityId = tdsTaxDetail.ActivityId,
                            EntityId = voucherVM.EntityId,
                            CrAmount = tdsTaxDetail.Amount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            //PartyId = voucherVM.PartyId,
                            //PartyPlantId = voucherVM.PartyPlantId,
                            //PartyType = voucherVM.PartyType,
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
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount
                        });
                        totalAmountCr += voucherDetailCr.CrAmount;
                    }
                }
                if (voucherVM.PartyType == PartyType.Customer.ToString() && null != additionalTaxList && additionalTaxList.Count() > 0)
                {
                    AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                    var tdsTax = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = additionalTaxList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = additionalTaxList.Sum(r => r.TaxAutoAmount),
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        VoucherId = voucher.Id,
                        SourceType = voucher.SourceType,
                        AdjustmentNoteId = adjustmentNote.Id,
                        Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _additionalTaxRepository.Insert(tdsTax);

                    int addtionalTaxDetailId = 0;
                    foreach (var tdsTaxVM in additionalTaxList)
                    {

                        if (null == tdsTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _accountsCommonService.GetTaxCodeGL(tdsTaxVM.TaxCodeId);


                        addtionalTaxDetailId++;
                        var tdsTaxDetail = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL["CreditableGLId"].ToString(),
                            BudgetMasterId = taxCodeGL["CreditableGLBudgetMasterId"].ToString(),
                            ActivityId = taxCodeGL["CreditableGLActivityId"].ToString(),
                            Amount = tdsTaxVM.TaxAmount,
                            AdditionalTaxId = tdsTax.Id,
                            TaxCodeId = tdsTaxVM.TaxCodeId,
                            TaxCategoryId = tdsTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id = MakePK(tdsTax.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxDetailRepository.Insert(tdsTaxDetail);

                        var voucherDetailCr = new VoucherDetail
                        {
                            GLGeneralInfoId = tdsTaxDetail.GLGeneralInfoId,
                            BudgetMasterId = tdsTaxDetail.BudgetMasterId,
                            ActivityId = tdsTaxDetail.ActivityId,
                            EntityId = voucherVM.EntityId,
                            CrAmount = tdsTaxDetail.Amount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            //PartyId = voucherVM.PartyId,
                            //PartyPlantId = voucherVM.PartyPlantId,
                            //PartyType = voucherVM.PartyType,
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
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount
                        });
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

        public bool CheckInvoiceDetailActivity(string InvoiceDetailId, string ActivityId, string SourceType)
        {
            try
            {
                var sql = "IF EXISTS(SELECT * FROM(" +
                        "SELECT I.InvoiceDetailId InvoiceDetailId, VD.ActivityId ActivityId, V.SourceType  " +
                         "FROM trn.InvoiceDetailCHarges I  " +
                         "LEFT JOIN TRN.VoucherDetail VD ON VD.Id = I.VoucherDetailId  " +
                         "LEFT JOIN TRN.Voucher V ON V.Id = VD.VoucherId  " +
                         ") A WHERE InvoiceDetailId = '" + InvoiceDetailId + "' AND ActivityId = '" + ActivityId + @"' AND SourceType = '" + SourceType + @"') SELECT 1 ELSE SELECT 0 RETURN ";
                return Convert.ToBoolean(_adjustmentNoteDetailRepository.SqlQuery<int>(sql).Single());
            }
            catch (Exception)
            {
                return false;
            }
        }
        public string InsertDebitNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList, IEnumerable<InvoiceTaxViewModel> additionalTaxList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                decimal totalVoucherDetailTaxAmount = 0;
                decimal totalcreditableDrAmount = 0, totalExpensesDrAmount = 0, totalExpensesCrAmount = 0, totalwithholdCrAmount = 0, totalwithholdDrAmount = 0, taxDrAmount = 0;
                decimal totalcreditableDrAmountAddTax = 0, totalExpensesDrAmountAddTax = 0, totalwithholdCrAmountAddTax = 0, taxDrAmountAddTax = 0;
                decimal totalBaseCurrencyCrAmount = 0;
                decimal totalBaseCurrencyDrAmount = 0;
                decimal totalAPBaseCurrencyCrAmount = 0;
                
                decimal totalAPBaseCurrencyDrAmount = 0;
                var creditablegl = false;
                var withholdgl = false;
                var merge = false;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var currentVoucherDetailId = 0;
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();


                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                // INSERT INTO AdjustmentNote
                var adjustmentNote = InsertAdjustmentNote(voucherVM);

                //invoicewriteoff
               // var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set VoucherId
                adjustmentNote.VoucherId = voucher.Id;

               

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (adjustmentNote.SettlementType == SettlementType.Invoice.ToString())
                    {
                        var voucherDetailDb = _voucherService.FindVoucherDetail(voucherDetailVM.Id);
                        voucherDetailVM.GLGeneralInfoId = voucherDetailDb.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = voucherDetailDb.BudgetMasterId;
                        voucherDetailVM.ActivityId = voucherDetailDb.ActivityId;
                    }
                    var voucherDetaiSales = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        TrnNature = TransactionNature.Sales.ToString(),
                        CrAmount = voucherDetailVM.Amount
                    };
                    totalAmountCr += voucherDetaiSales.CrAmount;
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetaiSales, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetaiSales, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetaiSales.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetaiSales.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetaiSales.CrAmount
                    });

                    if (null != invoiceDetailChargesList && invoiceDetailChargesList.Count() > 0 && voucherDetailVM.IsOrderSpecific == true)
                    {

                        foreach (var item in invoiceDetailChargesList.Where(r => r.GLGeneralInfoId == voucherDetailVM.GLGeneralInfoId && r.BudgetMasterId == voucherDetailVM.BudgetMasterId && r.ActivityId == voucherDetailVM.ActivityId))
                        {

                            if (CheckInvoiceDetailActivity(item.InvoiceDetailId, item.ActivityId,voucher.SourceType) == true)
                                throw new CustomException("InvoiceDetailId " + item.InvoiceDetailId + " and Activity " + voucherDetailVM.ActivityName + " already distributed!");

                            var invoiceDetailChargesId = base.GetAutoNumber(nameof(InvoiceDetailCharges), PKGeneratorEnum.Yearly, null, DateTime.Now);
                            var invoiceChargesId = 0;
                            if (item.Id == null)
                            {
                                invoiceChargesId++;
                                var invoiceCharges = new InvoiceDetailCharges
                                {
                                    Id = MakePK(invoiceDetailChargesId, invoiceChargesId, 2),
                                    InvoiceDetailId = item.InvoiceDetailId,
                                    InvoiceId = item.InvoiceId,
                                    DistributedAmount = item.DistributedAmount,
                                    InvoiceServiceMasterChargesId = null,
                                    VoucherDetailId = voucherDetaiSales.Id,
                                    Amount = item.Amount,
                                    InvoiceType = item.InvoiceType,
                                    MasterOrderId = item.MasterOrderId,
                                    ContractId = item.ContractId
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

                    if (voucherVM.PartyType == PartyType.Vendor.ToString() && null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
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
                                VoucherDetailId = voucherDetaiSales.Id,
                                AdjustmentNoteId = adjustmentNote.Id,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = Math.Round(invoiceTaxVM.TaxAmount, 4),
                                TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.DebitNote.ToString()
                            };
                            taxDrAmount += Math.Round(invoiceTaxVM.TaxAmount, 4);
                            _invoiceTaxService.InsertInvoiceTax(adjustmentNote, invoiceTax, invoiceTaxPk);

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
                                    AType = "Dr"
                                };
                                totalwithholdDrAmount += invoiceTaxDetail.Amount;
                                totalVoucherDetailTaxAmount += totalwithholdDrAmount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 1);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    DrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
                                };
                                totalAmountDr += voucherDetailTax.DrAmount;
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
                                totalBaseCurrencyDrAmount += voucherDetailCurrencydb.DrAmount;
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
                                    AType = "Cr"
                                };
                                totalcreditableDrAmount += invoiceTaxDetail.Amount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 2);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    CrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
                                };
                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);
                                totalAmountCr += voucherDetailTax.CrAmount;
                                var voucherDetailCurrencybase = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherVM.CurrencyId,
                                    CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                };
                                totalBaseCurrencyCrAmount += voucherDetailCurrencybase.CrAmount;
                                totalAPBaseCurrencyCrAmount += voucherDetailCurrencybase.CrAmount;
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
                                    AType = "Cr"

                                };
                                totalExpensesCrAmount += invoiceTaxDetail.Amount;
                                _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 3);

                                var voucherDetailTax = new VoucherDetail
                                {
                                    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                                    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                                    ActivityId = invoiceTaxDetail.ActivityId,
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    CurrencyId = voucher.CurrencyId,
                                    CrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
                                };
                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);
                                totalAmountCr += voucherDetailTax.CrAmount;
                                var voucherDetailCurrencybase = new VoucherDetailCurrency
                                {
                                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                    ToCurrencyId = companyCurrencyId,
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherVM.CurrencyId,
                                    CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                };

                                totalBaseCurrencyCrAmount += voucherDetailCurrencybase.CrAmount;
                                totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.CrAmount;
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencybase);
                            }
                            if (merge && !taxCode.IsCreditable)
                            {
                                voucherDetaiSales.DrAmount += invoiceTax.TaxAmount;
                                totalAmountCr += invoiceTax.TaxAmount;
                            }
                        }
                    }

                    if (voucherVM.PartyType == PartyType.Customer.ToString() && null != voucherDetailVM.InvoiceTaxViewModel && voucherDetailVM.InvoiceTaxViewModel.Count > 0)
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
                                VoucherDetailId = voucherDetaiSales.Id,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = Math.Round(invoiceTaxVM.TaxAmount, 4),
                                TaxAutoAmount = invoiceTaxVM.TaxAutoAmount,
                                PartyId = voucherVM.PartyId,
                                SourceType = SourceType.VendorInvoiceTax.ToString(),
                                AdjustmentNoteId = adjustmentNote.Id
                            };
                            taxDrAmount += Math.Round(invoiceTaxVM.TaxAmount, 4);
                            _invoiceTaxService.InsertInvoiceTax(adjustmentNote, invoiceTax, invoiceTaxPk);

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
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
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
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
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
                                    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                                    CurrencyId = voucherDetaiSales.CurrencyId,
                                    DrAmount = invoiceTaxDetail.Amount,
                                    PostingWithoutTaxAllow = voucherDetaiSales.PostingWithoutTaxAllow
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
                            //if (merge && !taxCode.IsCreditable)
                            //{
                            //    voucherDetaiSales.DrAmount += invoiceTax.TaxAmount;
                            //    totalAmountDr += invoiceTax.TaxAmount;
                            //}
                        }
                    }


                }

                var gl = GetDebitNoteGL(adjustmentNote.CompanyId, voucherVM.FinancingTypeId);
                var adjustmentNoteDetail = new AdjustmentNoteDetail
                {
                    GLGeneralInfoId = gl.AssetGLId,
                    BudgetMasterId = gl.AssetBudgetMasterId,
                    ActivityId = gl.AssetActivityId,
                    WrittenOffAmount = 0,
                    IsWrittenOff = false
                };


                if (voucherVM.PartyType == "Customer")
                {
                    adjustmentNoteDetail.Amount = adjustmentNote.Amount + totalwithholdDrAmount + totalBaseCurrencyCrAmount;

                }
                else
                {
                    if (voucherVM.PartyType == PartyType.Vendor.ToString() && null != additionalTaxList && additionalTaxList.Count() > 0)
                    {
                        adjustmentNoteDetail.Amount = adjustmentNote.Amount - totalwithholdDrAmount + totalBaseCurrencyCrAmount- additionalTaxList.Sum(r=>r.TaxAmount);
                    }
                    else
                        adjustmentNoteDetail.Amount = adjustmentNote.Amount - totalwithholdDrAmount + totalBaseCurrencyCrAmount;

                }
                InsertAdjustmentNoteDetail(adjustmentNote, adjustmentNoteDetail, 1);


                // INSERT INTO VoucherDetail
                var voucherDetail = new VoucherDetail
                {
                    GLGeneralInfoId = adjustmentNoteDetail.GLGeneralInfoId,
                    BudgetMasterId = adjustmentNoteDetail.BudgetMasterId,
                    ActivityId = adjustmentNoteDetail.ActivityId,
                    EntityId = voucher.EntityId,
                    PartyType = adjustmentNote.PartyType,
                    PartyId = adjustmentNote.PartyId,
                    PartyPlantId = adjustmentNote.PartyPlantId,
                    TrnNature = voucherVM.SourceType,
                    AdjustmentNoteDetailId = adjustmentNoteDetail.Id,
                    DrAmount = adjustmentNoteDetail.Amount - totalwithholdDrAmount //+ totalBaseCurrencyCrAmount
                };
                totalAmountDr += voucherDetail.DrAmount;
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount
                });

                if (null != invoiceTaxVMList && invoiceTaxVMList.Count() > 0)
                {
                    var invoiceTaxPk1 = _invoiceTaxService.GetMaxNumber();
                    var currentInvoiceTaxDetailId = 0;
                    foreach (var invoiceTaxVM in invoiceTaxVMList)
                    {
                        var invoiceTax = new InvoiceTax
                        {
                            TaxAmount = invoiceTaxVM.TaxAmount,
                            TaxAutoAmount = invoiceTaxVM.TaxAmount,
                        };

                        _invoiceTaxService.InsertInvoiceTax(adjustmentNote, invoiceTax, invoiceTaxPk);

                        if (adjustmentNote.PartyType == PartyType.Customer.ToString())
                        {
                            var taxCategoryGL = _taxCategoryGLService.GetTaxCategoryGLOutput(invoiceTaxVM.TaxCategoryId);
                            if (null == taxCategoryGL)
                                throw new CustomException("Output type Tax GL not found!"); invoiceTaxVM.GLGeneralInfoId = taxCategoryGL.GLGeneralInfoId;
                            invoiceTaxVM.BudgetMasterId = taxCategoryGL.BudgetMasterId;
                            invoiceTaxVM.ActivityId = taxCategoryGL.ActivityId;
                        }
                        else if (adjustmentNote.PartyType == PartyType.Vendor.ToString())
                        {
                            var taxCategoryGL = _taxCategoryGLService.GetTaxCategoryGLInput(invoiceTaxVM.TaxCategoryId);
                            if (null == taxCategoryGL)
                                throw new CustomException("Input type Tax GL not found!");
                            invoiceTaxVM.GLGeneralInfoId = taxCategoryGL.GLGeneralInfoId;
                            invoiceTaxVM.BudgetMasterId = taxCategoryGL.BudgetMasterId;
                            invoiceTaxVM.ActivityId = taxCategoryGL.ActivityId;
                        }
                        var invoiceTaxDetail = new InvoiceTaxDetail
                        {
                            GLGeneralInfoId = invoiceTaxVM.GLGeneralInfoId,
                            BudgetMasterId = invoiceTaxVM.BudgetMasterId,
                            ActivityId = invoiceTaxVM.ActivityId,
                            Amount = invoiceTax.TaxAmount,
                            AType = "Cr"
                        };
                        currentInvoiceTaxDetailId++;
                        _invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, currentInvoiceTaxDetailId);

                        var voucherDetailTax = new VoucherDetail
                        {
                            InvoiceTaxDetailId = invoiceTaxDetail.Id,
                            GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                            ActivityId = invoiceTaxDetail.ActivityId,
                            CrAmount = invoiceTaxDetail.Amount,
                            TrnNature = TransactionNature.SalesTax.ToString()
                        };
                        totalAmountCr += voucherDetailTax.CrAmount;
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailTax, currentVoucherDetailId);

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailTax.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTax.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherDetailTax.CrAmount
                        });
                    }
                }
                if (voucherVM.PartyType == PartyType.Vendor.ToString() && null != additionalTaxList && additionalTaxList.Count() > 0)
                {
                    AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                    var tdsTax = new AdditionalTax
                    {

                        TaxYearId = voucher.TaxYearId,
                        TaxYearPeriodId = voucher.TaxYearPeriodId,
                        TaxAmount = additionalTaxList.Sum(r => r.TaxAmount),
                        TaxAutoAmount = additionalTaxList.Sum(r => r.TaxAutoAmount),
                        PartyId = voucherVM.PartyId,
                        PartyPlantId = voucherVM.PartyPlantId,
                        AdjustmentNoteId = adjustmentNote.Id,
                        Id = base.GetAutoNumber(nameof(AdditionalTax), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP
                    };
                    _additionalTaxRepository.Insert(tdsTax);

                    int addtionalTaxDetailId = 0;
                    foreach (var tdsTaxVM in additionalTaxList)
                    {

                        if (null == tdsTaxVM.TaxCodeId)
                            throw new CustomException("Tax code not found!");

                        var taxCodeGL = _accountsCommonService.GetTaxCodeGL(tdsTaxVM.TaxCodeId);


                        addtionalTaxDetailId++;
                        var tdsTaxDetail = new AdditionalTaxDetail
                        {
                            GLGeneralInfoId = taxCodeGL["WithholdCreditableGLId"].ToString(),
                            BudgetMasterId = taxCodeGL["WithholdCreditableBudgetMasterId"].ToString(),
                            ActivityId = taxCodeGL["WithholdCreditableActivityId"].ToString(),
                            Amount = tdsTaxVM.TaxAmount,
                            AdditionalTaxId = tdsTax.Id,
                            TaxCodeId = tdsTaxVM.TaxCodeId,
                            TaxCategoryId = tdsTaxVM.TaxCategoryId,
                            AType = "Cr",
                            Id = MakePK(tdsTax.Id, addtionalTaxDetailId, 3),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _additionalTaxDetailRepository.Insert(tdsTaxDetail);

                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = tdsTaxDetail.GLGeneralInfoId,
                            BudgetMasterId = tdsTaxDetail.BudgetMasterId,
                            ActivityId = tdsTaxDetail.ActivityId,
                            EntityId = voucherVM.EntityId,
                            DrAmount = tdsTaxDetail.Amount,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            PartyType = voucherVM.PartyType,
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
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount
                        });
                        totalAmountDr += voucherDetailDr.DrAmount;
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

        private AdjustmentNote InsertAdjustmentNote(AdjustmentNote adjustmentNote)
        {
            adjustmentNote.Id = base.GetAutoNumber(nameof(AdjustmentNote), PKGeneratorEnum.Yearly, null, DateTime.Now);
            base.InsertGraph(adjustmentNote);
            return adjustmentNote;
        }
        public IEnumerable<AdjustmentNoteDetail> QueryInvoiceDetailEnumerable(IEnumerable<string> query)
        {
            return _adjustmentNoteDetailRepository.Query(r => query.Contains(r.Id)).Select().ToList();
        }
        public IQueryFluent<AdjustmentNoteDetail> QueryAdjustmentNoteDetail(string adjustmentNoteId)
        {
            return _adjustmentNoteDetailRepository.Query(r => r.AdjustmentNoteId == adjustmentNoteId);
        }
        public void UpdateAdjustmentNoteDetail(AdjustmentNoteDetail adjustmentNoteDetail)
        {
            _adjustmentNoteDetailRepository.Update(adjustmentNoteDetail);
        }
        private void Check(AdjustmentNote entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        }
        public  AdjustmentNote InsertAdjustmentNote(VoucherViewModel voucherVM)
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
                PartyType = voucherVM.PartyType,
                PartyId = voucherVM.PartyId,
                PartyPlantId = voucherVM.PartyPlantId,
                SourceType = voucherVM.SourceType,
                IsPark = voucherVM.IsPark,
                NoteType = voucherVM.NoteType,
                InvoiceId = voucherVM.InvoiceId,
                Archive = false,
                SettlementType = voucherVM.SettlementType
            };
            if (adjustmentNote.SourceType == SourceType.CreditNote.ToString())
            {
                if (adjustmentNote.NoteType == NoteType.CustomerCreditNote.ToString())
                    adjustmentNote.PartyType = PartyType.Customer.ToString();
                else if (adjustmentNote.NoteType == NoteType.VendorCreditNote.ToString())
                    adjustmentNote.PartyType = PartyType.Vendor.ToString();
                else throw new CustomException("Party type is null.");
            }
            else if (adjustmentNote.SourceType == SourceType.DebitNote.ToString())
            {
                if (adjustmentNote.NoteType == NoteType.CustomerDebitNote.ToString())
                    adjustmentNote.PartyType = PartyType.Customer.ToString();
                else if (adjustmentNote.NoteType == NoteType.VendorDebitNote.ToString())
                    adjustmentNote.PartyType = PartyType.Vendor.ToString();
                else throw new CustomException("Party type is null.");
            }
            Check(adjustmentNote);
            return InsertAdjustmentNote(adjustmentNote);
        }

        public AdjustmentNoteDetail InsertAdjustmentNoteDetail(AdjustmentNote adjustmentNote, AdjustmentNoteDetail adjustmentNoteDetail, int currentId)
        {
            adjustmentNoteDetail.Id = MakePK(adjustmentNote.Id, currentId, 1);
            adjustmentNoteDetail.AdjustmentNoteId = adjustmentNote.Id;
            adjustmentNoteDetail.InvoiceId = adjustmentNote.InvoiceId;
            adjustmentNoteDetail.AddedBy = adjustmentNote.AddedBy;
            adjustmentNoteDetail.AddedDate = adjustmentNote.AddedDate;
            adjustmentNoteDetail.AddedFromIP = adjustmentNote.AddedFromIP;
            adjustmentNoteDetail.Archive = adjustmentNote.Archive;
            _adjustmentNoteDetailRepository.Insert(adjustmentNoteDetail);
            return adjustmentNoteDetail;
        }

        public FinancingTypeGL GetCreditNoteGL(string companyId, string financingTypeId)
        {
            var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                        WHERE C.Id='" + companyId + "' AND LTGGL.FinancingTypeId='" + financingTypeId + "'";
            var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
            if (null == glTemp || string.IsNullOrEmpty(glTemp.LiabilityGLId))
                throw new CustomException("This transaction type GL not found!");
            return glTemp;
        }

        public FinancingTypeGL GetDebitNoteGL(string companyId, string financingTypeId)
        {
            var sql = @"SELECT TOP(1) LTGGL.* FROM [HKP].[FinancingTypeGL] AS LTGGL
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=LTGGL.COAId
                        WHERE C.Id='" + companyId + "' AND LTGGL.FinancingTypeId='" + financingTypeId + "'";
            var glTemp = _financingTypeGLRepository.SelectQuery(sql).FirstOrDefault();
            if (null == glTemp || string.IsNullOrEmpty(glTemp.AssetGLId))
                throw new CustomException("This transaction type GL not found!");
            return glTemp;
        }

        public void Post(string adjustmentNoteId, string entityId, string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var financing = Find(adjustmentNoteId);
                CheckIsPosted(financing);

                financing.IsPark = false;
                AuditService.UpdatedLog(financing);
                base.UpdateGraph(financing);

                _voucherService.PostVoucher(financing.VoucherId);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                var inDirect = new System.Text.StringBuilder();
                var inDirectsql = "";

                inDirectsql = @"update [TRN].[Voucher] set EntityId='" + entityId + @"' where Id='" + voucherId + @"'
                            update [TRN].[VoucherDetail]  set EntityId='" + entityId + @"' where VoucherId='" + voucherId + @"' 
                            update [TRN].[AdjustmentNote]  set EntityId='" + entityId + @"' where VoucherId='" + voucherId + @"' ";
                inDirect.Append(inDirectsql);
                _sqlRepository.ExecuteSqlCommand(inDirect.ToString());
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

        public void DeleteAdjustmentNote(string adjustmentNoteId,string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var voucherDetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherDetailCurrency = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var adjustmentNoteDetail = _adjustmentNoteDetailRepository.Query(r => r.AdjustmentNoteId == adjustmentNoteId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.AdjustmentNoteId == adjustmentNoteId).Select().ToList();
                var additionalTax = _additionalTaxRepository.Query(r => r.AdjustmentNoteId == adjustmentNoteId).Select().ToList();

                foreach (var item in voucherDetailCurrency)
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
                if(additionalTax != null)
                {
                    foreach (var addtax in additionalTax)
                    {
                        var additionalDetailTax = _additionalTaxDetailRepository.Query(r => r.AdditionalTaxId == addtax.Id).Select().ToList();
                        if (additionalDetailTax != null)
                        {
                            foreach (var adDetailTax in additionalDetailTax)
                            {
                                _additionalTaxDetailRepository.Delete(adDetailTax);
                            }
                        }
                        _additionalTaxRepository.Delete(addtax);
                    }
                }
                
                foreach (var item in voucherDetail)
                {
                  var invoiceDetailCharges = _invoiceDetailChargesRepository.Query(r=>r.VoucherDetailId==item.Id).Select().FirstOrDefault();
                    if (invoiceDetailCharges!=null)
                    {
                        _invoiceDetailChargesRepository.Delete(invoiceDetailCharges.Id);
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
                foreach (var item in adjustmentNoteDetail)
                {
                    _adjustmentNoteDetailRepository.Delete(item.Id);
                }
                base.Delete(adjustmentNoteId);
                _voucherRepository.Delete(voucherId);
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
        public void DeleteAdjustmentNoteSetOff(string adjustmentNoteSetOffId, string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var voucherDetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherDetailCurrency = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                //var adjustmentNoteDetail = _adjustmentNoteDetailRepository.Query(r => r.AdjustmentNoteId == adjustmentNoteId).Select().ToList();
                foreach (var item in voucherDetailCurrency)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }
                foreach (var item in voucherDetail)
                {
                    _voucherDetailRepository.Delete(item.Id);
                }
                //foreach (var item in adjustmentNoteDetail)
                //{
                //    _adjustmentNoteDetailRepository.Delete(item.Id);
                //}
               // base.Delete(adjustmentNoteId);
                _voucherRepository.Delete(voucherId);
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
        public GridModel GetDebitNoteList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string partyId, string partyType)
        {
            try
            {
                parameters.CmdText = @"SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS AdjustmentNoteId, ID.Id AS AdjustmentNoteDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName,   I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName
                                    , (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
                                    ,V.TransactionRefNo
                                    ,IsExpenseDistribution=CASE WHEN ISNULL((select COUNT(ID.Id) from TRN.InvoiceDetailCharges ID
										INNER JOIN TRN.VoucherDetail VD ON VD.Id=ID.VoucherDetailId
										WHERE VD.VoucherId=I.VoucherId),0)>0 THEN 1 ELSE 0 END
                                    FROM [TRN].[AdjustmentNoteDetail] AS ID
                                    LEFT JOIN [TRN].[AdjustmentNote] AS I ON I.Id=ID.AdjustmentNoteId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND I.IsPark=0 AND I.SourceType in ('"+SourceType.InventoryReturnPayable+ @"')
                                    AND I.PartyType='" + partyType + "' AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='"+ plantId + "' AND I.PartyId='"+ partyId + @"'
                                    AND convert(Date,V.AddedDate) <= '04-Aug-2024'

                                    UNION ALL
                                    SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS AdjustmentNoteId, ID.Id AS AdjustmentNoteDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName,   I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName
                                    , (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
                                    ,V.TransactionRefNo
                                    ,IsExpenseDistribution=CASE WHEN ISNULL((select COUNT(ID.Id) from TRN.InvoiceDetailCharges ID
										INNER JOIN TRN.VoucherDetail VD ON VD.Id=ID.VoucherDetailId
										WHERE VD.VoucherId=I.VoucherId),0)>0 THEN 1 ELSE 0 END
                                    FROM [TRN].[AdjustmentNoteDetail] AS ID
                                    LEFT JOIN [TRN].[AdjustmentNote] AS I ON I.Id=ID.AdjustmentNoteId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND I.IsPark=0 AND I.SourceType in ('" + SourceType.DebitNote + @"')
                                    AND I.PartyType='" + partyType + "' AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "' AND I.PartyId='" + partyId + @"' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetCreditNoteList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string partyId, string partyType)
        {
            try
            {
                parameters.CmdText = @"SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS AdjustmentNoteId, ID.Id AS AdjustmentNoteDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName,   I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName
                                    , (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
                                    ,V.TransactionRefNo
                                    FROM [TRN].[AdjustmentNoteDetail] AS ID
                                    LEFT JOIN [TRN].[AdjustmentNote] AS I ON I.Id=ID.AdjustmentNoteId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND V.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND I.IsPark=0  AND I.SourceType in ('" + SourceType.CreditNote + "','"+ SourceType.VendorPayment + @"') AND V.IsPark=0
                                    AND I.PartyType='" + partyType + "' AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "' AND I.PartyId='" + partyId + @"'
                                    AND VD.VoucherId NOT IN(select ISNULL(VoucherId,'') from [TRN].[SalesReturn])

                                    UNION ALL
                                    SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS AdjustmentNoteId, ID.Id AS AdjustmentNoteDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName,   I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName
                                    , (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
                                    ,V.TransactionRefNo
                                    FROM [TRN].[AdjustmentNoteDetail] AS ID
                                    LEFT JOIN [TRN].[AdjustmentNote] AS I ON I.Id=ID.AdjustmentNoteId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND V.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND I.IsPark=0  AND I.SourceType in ('" + SourceType.CreditNote + @"') AND V.IsPark=0
                                    AND I.PartyType='" + partyType + "' AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "' AND I.PartyId='" + partyId + @"' 
                                    AND VD.VoucherId IN(select ISNULL(VoucherId,'') from [TRN].[SalesReturn]) 
                                    AND convert(Date,V.AddedDate) <= '18-Aug-2024'

                                    UNION ALL
                                    SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS AdjustmentNoteId, ID.Id AS AdjustmentNoteDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName,   I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.Amount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName
                                    , (ISNULL(ID.Amount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
                                    ,V.TransactionRefNo
                                    FROM [TRN].[AdjustmentNoteDetail] AS ID
                                    LEFT JOIN [TRN].[AdjustmentNote] AS I ON I.Id=ID.AdjustmentNoteId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [TRN].[SalesReturn] AS SR ON V.Id=SR.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND V.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND I.IsPark=0  AND I.SourceType in ('" + SourceType.CreditNote + @"') AND V.IsPark=0
                                    AND I.PartyType='" + partyType + "' AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "' AND I.PartyId='" + partyId + @"' 
                                    AND VD.VoucherId IN(select ISNULL(VoucherId,'') from [TRN].[SalesReturn]) 
                                    AND SR.IsCreditNote=1 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        private static void CheckIsPosted(AdjustmentNote adjustmentNote)
        {
            if (!adjustmentNote.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }

        private string InsertInventoryShortagePayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                // INSERT INTO AdjustmentNote
                var adjustmentNote = InsertAdjustmentNote(voucherVM);

                //invoicewriteoff
                // var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set VoucherId
                adjustmentNote.VoucherId = voucher.Id;

                var gl = GetDebitNoteGL(adjustmentNote.CompanyId, voucherVM.FinancingTypeId);
                var adjustmentNoteDetail = new AdjustmentNoteDetail
                {
                    GLGeneralInfoId = gl.AssetGLId,
                    BudgetMasterId = gl.AssetBudgetMasterId,
                    ActivityId = gl.AssetActivityId,
                    Amount = adjustmentNote.Amount,
                    WrittenOffAmount = 0,
                    IsWrittenOff = false
                };
                InsertAdjustmentNoteDetail(adjustmentNote, adjustmentNoteDetail, 1);

                var currentVoucherDetailId = 0;

                // INSERT INTO VoucherDetail
                var voucherDetail = new VoucherDetail
                {
                    GLGeneralInfoId = adjustmentNoteDetail.GLGeneralInfoId,
                    BudgetMasterId = adjustmentNoteDetail.BudgetMasterId,
                    ActivityId = adjustmentNoteDetail.ActivityId,
                    EntityId = voucher.EntityId,
                    PartyType = adjustmentNote.PartyType,
                    PartyId = adjustmentNote.PartyId,
                    PartyPlantId = adjustmentNote.PartyPlantId,
                    TrnNature = TransactionNature.CreditNote.ToString(),
                    AdjustmentNoteDetailId = adjustmentNoteDetail.Id,
                    DrAmount = voucherVM.Amount
                };

                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherVM.CompanyCurrencyRate * voucherDetail.DrAmount
                });

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (adjustmentNote.SettlementType == SettlementType.Invoice.ToString())
                    {
                        var voucherDetailDb = _voucherService.FindVoucherDetail(voucherDetailVM.Id);
                        voucherDetailVM.GLGeneralInfoId = voucherDetailDb.GLGeneralInfoId;
                        voucherDetailVM.BudgetMasterId = voucherDetailDb.BudgetMasterId;
                        voucherDetailVM.ActivityId = voucherDetailDb.ActivityId;
                    }
                    var voucherDetaiSales = new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        TrnNature = TransactionNature.Sales.ToString(),
                        CrAmount = voucherDetailVM.Amount
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetaiSales, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetaiSales, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetaiSales.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetaiSales.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherVM.CompanyCurrencyRate * voucherDetaiSales.CrAmount
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

    }
}