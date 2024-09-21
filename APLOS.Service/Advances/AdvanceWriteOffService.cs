using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Advances;
using Library.Model.Banks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Vouchers;
using Library.Service.Banks;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Finances;
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
using System.Reflection;

namespace Library.Service.Advances
{
    public class AdvanceWriteOffService : Service<AdvanceWriteOff>, IAdvanceWriteOffService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IAdvanceService _advanceService;
        private readonly IVoucherService _voucherService;
        private readonly IEmployeePayableWriteOffService _employeePayableWriteOffService;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceWriteOffService _invoiceWriteOffService;
        private readonly IPKGeneratorService _pKGeneratorService;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IRepositoryAsync<Advance> _advanceRepository;
        private readonly IRepositoryAsync<AdvanceDetail> _advanceDetailRepository;
        private readonly IRepositoryAsync<AdvanceWriteOff> _advanceWriteOffRepository;
        private readonly IRepositoryAsync<AdvanceWriteOffDetail> _advanceWriteOffDetailRepository;
        private readonly IRepositoryAsync<InvoiceWriteOffDetail> _invoiceWriteOffDetailRepository;
        private readonly IRepositoryAsync<EmployeePayable> _employeePayableRepository;
        private readonly IRepositoryAsync<EmployeePayableDetail> _employeePayableDetailRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;
        private readonly IBankChargeService _bankChargeService;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IRepositoryAsync<EmployeeSubsequentTransaction> _employeeSubsequentTransactionRepository;
        private readonly IFinancingTypeGLService _financingTypeGLService;

        public AdvanceWriteOffService(
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
            , IEmployeePayableWriteOffService employeePayableWriteOffService
            , IEmployeePayableService employeePayableService
            , IBankChargeService bankChargeService
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository
            , IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository
            , IRepositoryAsync<EmployeePayable> employeePayableRepository
            , IRepositoryAsync<EmployeePayableDetail> employeePayableDetailRepository
            , IRepositoryAsync<Advance> advanceRepository
            , IRepositoryAsync<AdvanceDetail> advanceDetailRepository
            , IRepositoryAsync<EmployeeSubsequentTransaction> employeeSubsequentTransactionRepository
            , IFinancingTypeGLService financingTypeGLService
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
            _employeePayableWriteOffService = employeePayableWriteOffService;
            _employeePayableService = employeePayableService;
            _bankChargeService = bankChargeService;
            _invoiceTaxService = invoiceTaxService;
            _invoiceTaxRepository = invoiceTaxRepository;
            _voucherRepository = voucherRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _employeePayableRepository = employeePayableRepository;
            _employeePayableDetailRepository = employeePayableDetailRepository;
            _advanceRepository = advanceRepository;
            _advanceDetailRepository = advanceDetailRepository;
            _employeeSubsequentTransactionRepository = employeeSubsequentTransactionRepository;
            _financingTypeGLService = financingTypeGLService;
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

        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, AW.Id, AW.Id AdvanceId,P.Code AS PartyCode, P.UserName AS PartyName, EI.EmployeeCode, EI.EmployeeName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode,AW.Amount
                                    , AW.VoucherId, AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark,AW.SettlementType
                                    ,Status = case when AW.IsPark = 0 then 'Posted' else 'Parked' end
                                    FROM [TRN].[AdvanceWriteOff] AS AW
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
									LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AW.EmployeeId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=AW.VoucherId
                                    WHERE AW.Archive=0 AND V.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.[SourceType]='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetInvoiceCharge(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, EI.EmployeeCode, EI.EmployeeName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode, X.DrAmount AS Amount
                                    , AW.VoucherId, AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark
                                    FROM [TRN].[InvoiceWriteOff] AS AW
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
									LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AW.EmployeeId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=AW.VoucherId
									LEFT JOIN (SELECT VDC.VoucherId,  SUM(VDC.DrAmount) AS DrAmount FROM [TRN].[VoucherDetail] AS VDC
										GROUP BY VoucherId
									) AS X ON X.VoucherId=AW.VoucherId AND  X.DrAmount > 0
                                    WHERE AW.Archive=0 AND V.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.[SourceType]='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel QueryEmployee(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, V.VoucherDate,V.CurrencyId, V.VoucherTypeId, AW.Id, EI.SystemId AS EmployeeId,  EI.EmployeeCode, EI.EmployeeName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode, AW.Narration,AW.SettlementType,  X.DrAmount AS Amount
                                    , AW.VoucherId,AW.IsPark, Status= case when AW.IsPark=1 then 'Parked' else 'Posted' end
                                    FROM [TRN].[AdvanceWriteOff] AS AW
									LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AW.EmployeeId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=AW.VoucherId
									LEFT JOIN (SELECT VDC.VoucherId, VDC.ParallelCurrencyId, SUM(VDC.DrAmount) AS DrAmount FROM [TRN].[VoucherDetailCurrency] AS VDC
										GROUP BY VoucherId, ParallelCurrencyId
									) AS X ON X.VoucherId=AW.VoucherId AND X.ParallelCurrencyId=AW.CurrencyId AND X.DrAmount > 0
                                    WHERE AW.Archive=0 AND V.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.[SourceType]='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public string InsertCustomerAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
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

                // INSERT INTO AdvanceWriteOff
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);
                var totalAmountDr = advanceDetailVMList != null ? advanceDetailVMList.Sum(r => r.DrAmount) : voucherVM.Amount;

                // Set total Debit amount in write of master.
                advanceWriteOff.Amount = totalAmountDr;

                var totalAmountCr = 0.00M;
                var totalCurrencyAmountDr = 0.00M;
                var totalCurrencyAmountCr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOff.VoucherId = voucher.Id;

                // Advance
                var advance = _advanceService.Find(voucherVM.AdvanceId);
                if (null == advance)
                    throw new CustomException("Advance Id not found!");
                advance.WrittenOffAmount += totalAmountDr;
                advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                advance.UpdatedBy = advanceWriteOff.AddedBy;
                advance.UpdatedDate = advanceWriteOff.AddedDate;
                advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                _advanceService.Update(advance);

                var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                if (null == advanceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                advanceDetail.WrittenOffAmount += totalAmountDr;
                if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");
                advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                advanceDetail.UpdatedBy = advance.UpdatedBy;
                advanceDetail.UpdatedDate = advance.UpdatedDate;
                advanceDetail.UpdatedFromIP = advance.UpdatedFromIP;
                _advanceService.UpdateAdvanceDetail(advanceDetail);

                var advanceWriteOffDetail = new AdvanceWriteOffDetail
                {
                    AdvanceId = advance.Id,
                    AdvanceDetailId = advanceDetail.Id,
                    AdvanceWriteOffId = advanceWriteOff.Id,
                    GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceDetail.BudgetMasterId,
                    ActivityId = advanceDetail.ActivityId,
                    CurrencyId = advance.CurrencyId,
                    PartyType = advanceDetail.PartyType,
                    CompanyId = advanceDetail.CompanyId,
                    PlantId = advanceDetail.PlantId,
                    PartyId = advanceDetail.PartyId,
                    PartyPlantId = advanceDetail.PartyPlantId,
                    Amount = advanceWriteOff.Amount,
                    BooksAmount = Math.Round(advanceWriteOff.Amount * advance.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero)
                };
                InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, 1);

                // INSERT INTO VoucherDetail Debit
                var currentVoucherDetailId = 0;
                var voucherDetailDr = new VoucherDetail
                {
                    AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                    GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                    ActivityId = advanceWriteOffDetail.ActivityId,
                    DrAmount = advanceWriteOffDetail.Amount,
                    PartyType = advanceWriteOffDetail.PartyType,
                    PartyId = advanceWriteOffDetail.PartyId,
                    PartyPlantId = advanceWriteOffDetail.PartyPlantId
                };

               
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = Math.Round(voucherDetailDr.DrAmount * advance.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero),
                    CrAmount = Math.Round(voucherDetailDr.CrAmount * advance.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero)
                });
                totalCurrencyAmountDr += Math.Round(voucherDetailDr.DrAmount * advance.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);

                if (voucherVM.SettlementType == SettlementType.SetOff.ToString() && voucherVM.PaymentSource==PaymentSource.Invoice.ToString())
                {
                    // INSERT INTO InvoiceWriteOff
                    var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                    invoiceWriteOff.VoucherId = voucher.Id;
                    invoiceWriteOff.Amount = totalAmountDr;

                    // Invoice
                    var invoiceIds = advanceDetailVMList.Select(r => r.InvoiceId);
                    var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = advanceDetailVMList.Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                    var currentInvoiceDetail = 0;
                    foreach (var voucherDetailVM in advanceDetailVMList)
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
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = voucherDetailVM.DrAmount,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PartyType = advanceWriteOff.PartyType,
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
                            CrAmount = Math.Round(voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero),
                        });
                        totalCurrencyAmountCr += Math.Round(voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
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
                                CrAmount = Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero)
                            });
                            totalCurrencyAmountCr += Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero);
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
                                DrAmount = Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero)
                            });
                            totalCurrencyAmountDr += Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                }
                else if (voucherVM.SettlementType == SettlementType.Return.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                        throw new CustomException("Bank Id is null!");
                    if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id is null!");

                    var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);

                    // INSERT INTO VoucherDetail
                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString(),
                        BudgetMasterId = bankMaster["BudgetMasterId"].ToString(),
                        ActivityId = bankMaster["ActivityId"].ToString(),
                        EntityId = voucherVM.EntityId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        CrAmount = voucherVM.Amount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyType = PartyType.Bank.ToString(),
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;

                    // INSRT INTO GLTransactionDetail
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (!string.IsNullOrEmpty(voucherDetailCr.BankMasterId) || !string.IsNullOrEmpty(voucherDetailCr.CashMasterId))
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                            {
                                VoucherDetailId = voucherDetailCr.Id,
                                SourceType = voucherDetailCr.PaymentSource,
                                BankMasterId = voucherDetailCr.BankMasterId,
                                CashMasterId = voucherDetailCr.CashMasterId,
                                CrAmount = (bankMaster["CurrencyId"].ToString() == voucher.CurrencyId)? voucherDetailCr.CrAmount : voucherVM.BankAmount,
                                AddedBy = voucherDetailCr.AddedBy,
                                AddedDate = voucherDetailCr.AddedDate,
                                AddedFromIP = voucherDetailCr.AddedFromIP
                            });
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                    }
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round(voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountCr += Math.Round(voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
                    if (voucherVM.CompanyCurrencyRate < advance.CompanyCurrencyRate)
                    {
                        var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                        var voucherDetailGain = new VoucherDetail
                        {
                            GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId
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
                            CrAmount = Math.Round(voucherDetailCr.CrAmount * (advance.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountCr += Math.Round(voucherDetailCr.CrAmount * (advance.CompanyCurrencyRate-voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);
                    }
                    else if (voucherVM.CompanyCurrencyRate > advance.CompanyCurrencyRate)
                    {
                        var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                        var voucherDetailLoss = new VoucherDetail
                        {
                            GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailLoss, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoss, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailLoss.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.OtherCompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = Math.Round(voucherDetailCr.CrAmount * (voucherVM.CompanyCurrencyRate - advance.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountDr += Math.Round(voucherDetailCr.CrAmount * (voucherVM.CompanyCurrencyRate-advance.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);
                    }

                   
                }
                else if (voucherVM.SettlementType == SettlementType.SetOff.ToString() && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                        throw new CustomException("Bank Id is null!");
                    var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                    // INSERT INTO VoucherDetail
                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString(),
                        BudgetMasterId = bankMaster["BudgetMasterId"].ToString(),
                        ActivityId = bankMaster["ActivityId"].ToString(),
                        EntityId = voucherVM.EntityId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        CrAmount = voucherVM.Amount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyType = PartyType.Bank.ToString(),
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;
                     // INSRT INTO GLTransactionDetail
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (!string.IsNullOrEmpty(voucherDetailCr.BankMasterId) || !string.IsNullOrEmpty(voucherDetailCr.CashMasterId))
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                            {
                                VoucherDetailId = voucherDetailCr.Id,
                                SourceType = voucherDetailCr.PaymentSource,
                                BankMasterId = voucherDetailCr.BankMasterId,
                                CashMasterId = voucherDetailCr.CashMasterId,
                                CrAmount = voucherVM.CurrencyId==voucherVM.BankCurrencyId? voucherVM.Amount: voucherVM.BankAmount,
                                AddedBy = voucherDetailCr.AddedBy,
                                AddedDate = voucherDetailCr.AddedDate,
                                AddedFromIP = voucherDetailCr.AddedFromIP
                            });
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                    }
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round(voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountCr += Math.Round(voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
                    if (voucherVM.CompanyCurrencyRate > advance.CompanyCurrencyRate)
                    {
                        var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                        var voucherDetailGain = new VoucherDetail
                        {
                            GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId
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
                            CrAmount = Math.Round(voucherDetailCr.CrAmount * (voucherVM.CompanyCurrencyRate - advance.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountCr += Math.Round(voucherDetailCr.CrAmount * (voucherVM.CompanyCurrencyRate - advance.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);
                    }
                    else if (voucherVM.CompanyCurrencyRate < advance.CompanyCurrencyRate)
                    {
                        var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                        var voucherDetailLoss = new VoucherDetail
                        {
                            GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailLoss, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoss, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailLoss.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.OtherCompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = Math.Round(voucherDetailCr.CrAmount * (advance.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountDr += Math.Round(voucherDetailCr.CrAmount * (advance.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);
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
        
        public string InsertMultiCustomerAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> advanceDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailListNew, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
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

                // INSERT INTO AdvanceWriteOff
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);
                var totalAmountDr = advanceDetailVMList != null ? advanceDetailVMList.Sum(r => r.DrAmount) : voucherVM.Amount;

                // Set total Debit amount in write of master.
                advanceWriteOff.Amount = totalAmountDr;

                var totalAmountCr = 0.00M;
                var totalCurrencyAmountDr = 0.00M;
                var totalCurrencyAmountCr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOff.VoucherId = voucher.Id;

                // Advance


                // Invoice
                var advanceIds = advanceDetailVMList.Select(r => r.AdvanceId);
                var advanceDbList = _advanceService.Query(r => advanceIds.Contains(r.Id)).Select().ToList();
                var advanceDetailIds = advanceDetailVMList.Select(r => r.AdvanceDetailId);
                var advanceDetailDbList = _advanceService.GetAdvanceDetailList(r => advanceDetailIds.Contains(r.Id)).Select().ToList();
                var currentAdvanceDetail = 0;
                var currentVoucherDetailId = 0;
                foreach (var voucherDetailVM in advanceDetailVMList)
                {
                    var advanceDetail = advanceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.AdvanceDetailId);
                    if (null == advanceDetail)
                        throw new CustomException("Advance Detail Id not found!");

                    advanceDetail.WrittenOffAmount += voucherDetailVM.DrAmount;
                    if (advanceDetail.NetAmount < advanceDetail.WrittenOffAmount)
                        throw new CustomException("Received amount can not cross balance amount.");

                    advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                    advanceDetail.UpdatedBy = advanceWriteOff.AddedBy;
                    advanceDetail.UpdatedDate = advanceWriteOff.AddedDate;
                    advanceDetail.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                    _advanceService.UpdateAdvanceDetail(advanceDetail);

                    var advance = advanceDbList.First(r => r.Id == advanceDetail.AdvanceId);
                    advance.WrittenOffAmount = advanceDetail.WrittenOffAmount;
                    advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                    advance.UpdatedBy = advanceWriteOff.AddedBy;
                    advance.UpdatedDate = advanceWriteOff.AddedDate;
                    advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                    _advanceService.Update(advance);

                    // INSERT INTO InvoiceWriteOffDetail
                    currentAdvanceDetail++;
                    var advanceWriteOffDetail = new AdvanceWriteOffDetail
                    {
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId,
                        CurrencyId = advance.CurrencyId,
                        AdvanceWriteOffId = advanceWriteOff.Id,
                        AdvanceId = voucherDetailVM.AdvanceId,
                        AdvanceDetailId = voucherDetailVM.AdvanceDetailId,
                        CompanyId = voucherDetailVM.CompanyId,
                        PlantId = voucherDetailVM.PlantId,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        PartyType = voucherDetailVM.PartyType,
                        Amount = voucherDetailVM.DrAmount,
                        AddedBy = advanceWriteOff.AddedBy,
                        AddedDate = advanceWriteOff.AddedDate,
                        AddedFromIP = advanceWriteOff.AddedFromIP,
                        Archive = advanceWriteOff.Archive 
                    };
                    InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentAdvanceDetail);

                    // INSERT INTO VoucherDetail
                    var voucherDetailDr = new VoucherDetail
                    {
                        VoucherId = voucher.Id,
                        AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                        GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceDetail.BudgetMasterId,
                        ActivityId = advanceDetail.ActivityId,
                        CurrencyId = voucher.CurrencyId,
                        DrAmount = voucherDetailVM.DrAmount,
                        DocDate = voucherDetailVM.DocDate,
                        DocRefNo = voucherDetailVM.DocRefNo,
                        Narration = voucherDetailVM.Narration,
                        PartyType = advanceWriteOff.PartyType,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                        DrAmount = Math.Round(voucherDetailDr.DrAmount * voucherDetailVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero),
                    });
                    totalCurrencyAmountDr += Math.Round(voucherDetailDr.DrAmount * voucherDetailVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
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
                            CrAmount = Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountCr += Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero);
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
                            DrAmount = Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountDr += Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero);
                    }
                }


                if (voucherVM.SettlementType == SettlementType.SetOff.ToString() && voucherVM.PaymentSource == PaymentSource.Invoice.ToString())
                {
                    // INSERT INTO InvoiceWriteOff
                    var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                    invoiceWriteOff.VoucherId = voucher.Id;
                    invoiceWriteOff.Amount = totalAmountDr;

                    // Invoice
                    var invoiceIds = voucherDetailListNew.Select(r => r.InvoiceId);
                    var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = voucherDetailListNew.Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                    var currentInvoiceDetail = 0;
                    foreach (var voucherDetailVM in   voucherDetailListNew)
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
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            CrAmount = voucherDetailVM.DrAmount,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PartyType = advanceWriteOff.PartyType,
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
                            CrAmount = Math.Round(voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero),
                        });
                        totalCurrencyAmountCr += Math.Round(voucherDetailCr.CrAmount * voucherDetailVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
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
                                CrAmount = Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero)
                            });
                            totalCurrencyAmountCr += Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero);
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
                                DrAmount = Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero)
                            });
                            totalCurrencyAmountDr += Math.Round(voucherDetailVM.ExchangeAmount, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                }
                else if (voucherVM.SettlementType == SettlementType.Return.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                        throw new CustomException("Bank Id is null!");
                    if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id is null!");

                    var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);

                    // INSERT INTO VoucherDetail
                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString(),
                        BudgetMasterId = bankMaster["BudgetMasterId"].ToString(),
                        ActivityId = bankMaster["ActivityId"].ToString(),
                        EntityId = voucherVM.EntityId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        CrAmount = voucherVM.Amount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyType = PartyType.Bank.ToString(),
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;

                    // INSRT INTO GLTransactionDetail
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (!string.IsNullOrEmpty(voucherDetailCr.BankMasterId) || !string.IsNullOrEmpty(voucherDetailCr.CashMasterId))
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                            {
                                VoucherDetailId = voucherDetailCr.Id,
                                SourceType = voucherDetailCr.PaymentSource,
                                BankMasterId = voucherDetailCr.BankMasterId,
                                CashMasterId = voucherDetailCr.CashMasterId,
                                CrAmount = voucherDetailCr.CrAmount,
                                AddedBy = voucherDetailCr.AddedBy,
                                AddedDate = voucherDetailCr.AddedDate,
                                AddedFromIP = voucherDetailCr.AddedFromIP
                            });
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                    }
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round(voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountCr += Math.Round(voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
                }
                else if (voucherVM.SettlementType == SettlementType.SetOff.ToString() && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                        throw new CustomException("Bank Id is null!");
                    var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);
                    // INSERT INTO VoucherDetail
                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString(),
                        BudgetMasterId = bankMaster["BudgetMasterId"].ToString(),
                        ActivityId = bankMaster["ActivityId"].ToString(),
                        PlantId = voucher.PlantId,
                        EntityId = voucherVM.EntityId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        CrAmount = (advanceWriteOff.Amount- voucherVM.DiscountAmount),
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyType = PartyType.Bank.ToString(),
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };
                    if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                        voucherDetailCr.CrAmount += voucherVM.ExchangeAmount;
                    else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                        voucherDetailCr.CrAmount -= voucherVM.ExchangeAmount;

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    totalAmountCr += voucherDetailCr.CrAmount;
                    // INSRT INTO GLTransactionDetail
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (!string.IsNullOrEmpty(voucherDetailCr.BankMasterId) || !string.IsNullOrEmpty(voucherDetailCr.CashMasterId))
                        {
                            if (voucherVM.CurrencyId != voucherVM.BankCurrencyId)
                            {
                                if (voucherVM.ExchangeType == "ExchangeLoss" && voucherVM.ExchangeAmount > 0)
                                    voucherVM.BankAmount += Math.Round(voucherVM.ExchangeAmount* voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
                                else if (voucherVM.ExchangeType == "ExchangeGain" && voucherVM.ExchangeAmount > 0)
                                    voucherVM.BankAmount -= Math.Round(voucherVM.ExchangeAmount* voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
                            }
                            _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                            {
                                VoucherDetailId = voucherDetailCr.Id,
                                SourceType = voucherDetailCr.PaymentSource,
                                BankMasterId = voucherDetailCr.BankMasterId,
                                CashMasterId = voucherDetailCr.CashMasterId,
                                CrAmount = voucherVM.CurrencyId == voucherVM.BankCurrencyId ? (voucherDetailCr.CrAmount - voucherVM.DiscountAmount) : voucherVM.BankAmount,
                                AddedBy = voucherDetailCr.AddedBy,
                                AddedDate = voucherDetailCr.AddedDate,
                                AddedFromIP = voucherDetailCr.AddedFromIP
                            });
                            
                                
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                    }
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = Math.Round(voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountCr += Math.Round(voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
                   
                }
                if (voucherVM.DiscountAmount>0)
                {
                    // INSERT INTO VoucherDetail (Bank or cash side Dr)
                    var voucherDetailCr = new VoucherDetail
                    {
                        Narration = voucher.Narration,
                        CrAmount = voucherVM.DiscountAmount,
                        PaymentSource = PaymentSource.Discount.ToString()
                    };
                    totalAmountCr += voucherDetailCr.CrAmount;

                    var financingTypeGL = _accountsCommonService.GetDiscountGL(voucher.CompanyId,FinancingTypeEnum.PurchaseDiscount.ToString());
                    if (financingTypeGL != null)
                    {
                        voucherDetailCr.GLGeneralInfoId = financingTypeGL["ExpensesGLId"].ToString();
                        voucherDetailCr.BudgetMasterId = financingTypeGL["ExpensesBudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = financingTypeGL["ExpensesActivityId"].ToString();

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
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount= voucherDetailCr.CrAmount* voucherVM.CompanyCurrencyRate
                    };
                    totalCurrencyAmountCr += voucherDetailCurrencyCr.CrAmount;
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
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
                        DrAmount = Math.Round(voucherVM.ExchangeAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountDr += Math.Round(voucherVM.ExchangeAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
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
                        CrAmount = Math.Round(voucherVM.ExchangeAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountCr += Math.Round(voucherVM.ExchangeAmount * voucherVM.CompanyCurrencyRate, 2, MidpointRounding.AwayFromZero);
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

        public string InsertCustomerPaymentWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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

                // INSERT INTO AdvanceWriteOff table.
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);
                var totalAmountDr = voucherDetailVMList != null ? voucherDetailVMList.Sum(r => r.DrAmount) : voucherVM.Amount;

                // Set total Debit amount in write of master.
                advanceWriteOff.Amount = totalAmountDr;

                var totalAmountCr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOff.VoucherId = voucher.Id;

                // Advance
                var advance = _advanceService.Find(voucherVM.AdvanceId);
                if (null == advance)
                    throw new CustomException("Advance Id not found!");
                advance.WrittenOffAmount += totalAmountDr;
                advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                advance.UpdatedBy = advanceWriteOff.AddedBy;
                advance.UpdatedDate = advanceWriteOff.AddedDate;
                advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                _advanceService.Update(advance);

                var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                if (null == advanceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                advanceDetail.WrittenOffAmount += totalAmountDr;
                if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");
                advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                advanceDetail.UpdatedBy = advance.UpdatedBy;
                advanceDetail.UpdatedDate = advance.UpdatedDate;
                advanceDetail.UpdatedFromIP = advance.UpdatedFromIP;
                _advanceService.UpdateAdvanceDetail(advanceDetail);

                var advanceWriteOffDetail = new AdvanceWriteOffDetail
                {
                    AdvanceId = advance.Id,
                    AdvanceDetailId = advanceDetail.Id,
                    AdvanceWriteOffId = advanceWriteOff.Id,
                    GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceDetail.BudgetMasterId,
                    ActivityId = advanceDetail.ActivityId,
                    CurrencyId = advance.CurrencyId,
                    PartyType = advanceDetail.PartyType,
                    CompanyId = advanceDetail.CompanyId,
                    PlantId = advanceDetail.PlantId,
                    PartyId = advanceDetail.PartyId,
                    PartyPlantId = advanceDetail.PartyPlantId,
                    Amount = advanceWriteOff.Amount
                };
                InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, 1);

                // INSERT INTO VoucherDetail Debit
                var currentVoucherDetailId = 0;
                var voucherDetailDr = new VoucherDetail
                {
                    AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                    GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                    ActivityId = advanceWriteOffDetail.ActivityId,
                    DrAmount = advanceWriteOffDetail.Amount,
                    PartyType = advanceWriteOffDetail.PartyType,
                    PartyId = advanceWriteOffDetail.PartyId,
                    PartyPlantId = advanceWriteOffDetail.PartyPlantId
                };

                if (voucherVM.SettlementType == SettlementType.Charge.ToString())
                {
                    voucherDetailDr.CrAmount = voucherDetailDr.DrAmount;
                    voucherDetailDr.DrAmount = 0;

                    totalAmountDr = 0;
                    totalAmountCr = voucherDetailDr.CrAmount;
                }
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                    CrAmount = voucherDetailDr.CrAmount * voucherVM.CompanyCurrencyRate
                });

                if (voucherVM.SettlementType == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO InvoiceWriteOff
                    var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                    invoiceWriteOff.VoucherId = voucher.Id;

                    // Invoice
                    var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                    var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                    var currentInvoiceDetail = 0;
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
                            CrAmount = voucherDetailVM.DrAmount,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            PartyType = advanceWriteOff.PartyType,
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
                else if (voucherVM.SettlementType == SettlementType.Return.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                        throw new CustomException("Bank Id is null!");
                    if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id is null!");

                    var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);

                    // INSERT INTO VoucherDetail
                    var voucherDetailCr = new VoucherDetail
                    {
                        GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString(),
                        BudgetMasterId = bankMaster["BudgetMasterId"].ToString(),
                        ActivityId = bankMaster["ActivityId"].ToString(),
                        EntityId = voucherVM.EntityId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        CrAmount = voucherVM.Amount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyType = PartyType.Bank.ToString(),
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                    totalAmountDr += voucherDetailCr.DrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;

                    // INSRT INTO GLTransactionDetail
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        if (!string.IsNullOrEmpty(voucherDetailCr.BankMasterId) || !string.IsNullOrEmpty(voucherDetailCr.CashMasterId))
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                            {
                                VoucherDetailId = voucherDetailCr.Id,
                                SourceType = voucherDetailCr.PaymentSource,
                                BankMasterId = voucherDetailCr.BankMasterId,
                                CashMasterId = voucherDetailCr.CashMasterId,
                                CrAmount = voucherDetailCr.CrAmount,
                                AddedBy = voucherDetailCr.AddedBy,
                                AddedDate = voucherDetailCr.AddedDate,
                                AddedFromIP = voucherDetailCr.AddedFromIP
                            });
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                    }
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailCr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        CrAmount = voucherDetailCr.CrAmount * voucherVM.OtherCompanyCurrencyRate
                    });

                    if (voucherVM.CompanyCurrencyRate > voucherVM.OtherCompanyCurrencyRate)
                    {
                        var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Receivable);
                        var voucherDetailGain = new VoucherDetail
                        {
                            GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailGain, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailGain, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailGain.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.OtherCompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailGain.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherDetailCr.CrAmount * (voucherVM.CompanyCurrencyRate - voucherVM.OtherCompanyCurrencyRate)
                        });
                    }
                    else if (voucherVM.CompanyCurrencyRate < voucherVM.OtherCompanyCurrencyRate)
                    {
                        var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Receivable);
                        var voucherDetailLoss = new VoucherDetail
                        {
                            GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailLoss, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoss, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailLoss.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.OtherCompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailCr.CrAmount * (voucherVM.OtherCompanyCurrencyRate - voucherVM.CompanyCurrencyRate)
                        });
                    }
                }
                else if (voucherVM.SettlementType == SettlementType.InterTransaction.ToString())
                {
                    if (string.IsNullOrEmpty(voucherVM.InterCompanyId))
                        throw new CustomException("Inter Company Id is null!");
                    if (string.IsNullOrEmpty(voucherVM.InterPlantId))
                        throw new CustomException("Inter Company Plant Id is null!");

                    var party = _accountsCommonService.GetPartyByCompany(advanceWriteOff.CompanyGroupId, voucherVM.InterCompanyId);
                    var partyPlant = _accountsCommonService.GetPartyPlantByPlant(party["Id"].ToString(), voucherVM.InterPlantId);

                    var interAdvance = _advanceService.InsertAdvance(new Advance
                    {
                        CompanyGroupId = advanceWriteOff.CompanyGroupId,
                        CompanyId = advanceWriteOff.CompanyId,
                        PlantId = advanceWriteOff.PlantId,
                        EntityId = advanceWriteOff.EntityId,
                        DocDate = advanceWriteOff.DocDate,
                        DocRefNo = advanceWriteOff.DocRefNo,
                        CurrencyId = advanceWriteOff.CurrencyId,
                        FiscalYearId = advanceWriteOff.FiscalYearId,
                        FiscalYearPeriodId = advanceWriteOff.FiscalYearPeriodId,
                        TaxYearId = advanceWriteOff.TaxYearId,
                        TaxYearPeriodId = advanceWriteOff.TaxYearPeriodId,
                        IsInterTransaction = true,
                        IsPark = advanceWriteOff.IsPark,
                        JournalId = voucherVM.AdvanceId,
                        Amount = voucherVM.Amount,
                        JournalType = "Payable",
                        Narration = advanceWriteOff.Narration,
                        PartyType = PartyType.Customer.ToString(),
                        PaymentSource = PaymentSource.Journal.ToString(),
                        PostingDate = voucherVM.PostingDate,
                        SourceType = voucherVM.SourceType,
                        VoucherDate = DateTime.Now,
                        VoucherTypeId = advanceWriteOff.VoucherTypeId,
                        VoucherId = advanceWriteOff.VoucherId,
                        PartyId = party["Id"].ToString(),
                        PartyPlantId = partyPlant["Id"].ToString()
                    });

                    var financingTypeGL = _accountsCommonService.GetFinancingTypeGL(voucherVM.CompanyId, voucherVM.FinancingTypeId);
                   

                    var interAdvanceDetail = _advanceService.InsertAdvanceDetail(interAdvance, new AdvanceDetail
                    {
                        GLGeneralInfoId = financingTypeGL["LiabilityGLId"].ToString(),
                        BudgetMasterId = financingTypeGL["LiabilityBudgetMasterId"].ToString(),
                        ActivityId = financingTypeGL["LiabilityActivityId"].ToString(),
                        CompanyId = voucherVM.InterCompanyId,
                        PlantId = voucherVM.InterPlantId,
                        PartyType = PartyType.Company.ToString(),
                        PartyId = interAdvance.PartyId,
                        PartyPlantId = interAdvance.PartyPlantId,
                        Amount = interAdvance.Amount,
                        NetAmount = interAdvance.Amount,
                        Narration = interAdvance.Narration,
                    }, 1);

                    // INSERT INTO VoucherDetail (liability side Cr)
                    var voucherDetailCr = new VoucherDetail
                    {
                        Narration = interAdvanceDetail.Narration,
                        CrAmount = interAdvanceDetail.NetAmount,
                        PartyId = interAdvanceDetail.PartyId,
                        PartyType = interAdvanceDetail.PartyType,
                        PartyPlantId = interAdvanceDetail.PartyPlantId,
                        AdvanceDetailId = interAdvanceDetail.Id,
                        GLGeneralInfoId = interAdvanceDetail.GLGeneralInfoId,
                        BudgetMasterId = interAdvanceDetail.BudgetMasterId,
                        ActivityId = interAdvanceDetail.ActivityId
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
                        CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate
                    });
                }
                else if (voucherVM.SettlementType == SettlementType.Transfer.ToString())
                {
                    var interAdvance = _advanceService.InsertAdvance(new Advance
                    {
                        CompanyGroupId = advanceWriteOff.CompanyGroupId,
                        CompanyId = advanceWriteOff.CompanyId,
                        PlantId = advanceWriteOff.PlantId,
                        EntityId = advanceWriteOff.EntityId,
                        DocDate = advanceWriteOff.DocDate,
                        DocRefNo = advanceWriteOff.DocRefNo,
                        CurrencyId = advanceWriteOff.CurrencyId,
                        FiscalYearId = advanceWriteOff.FiscalYearId,
                        FiscalYearPeriodId = advanceWriteOff.FiscalYearPeriodId,
                        TaxYearId = advanceWriteOff.TaxYearId,
                        TaxYearPeriodId = advanceWriteOff.TaxYearPeriodId,
                        IsPark = advanceWriteOff.IsPark,
                        JournalId = voucherVM.AdvanceId,
                        Amount = voucherVM.Amount,
                        Narration = advanceWriteOff.Narration,
                        PartyType = PartyType.Customer.ToString(),
                        PaymentSource = PaymentSource.Journal.ToString(),
                        PostingDate = advanceWriteOff.PostingDate,
                        SourceType = advanceWriteOff.SourceType,
                        VoucherDate = DateTime.Now,
                        VoucherTypeId = advanceWriteOff.VoucherTypeId,
                        VoucherId = advanceWriteOff.VoucherId,
                        PartyId = advanceWriteOff.PartyId,
                        PartyPlantId = advanceWriteOff.PartyPlantId
                    });

                    var companyParty = _accountsCommonService.GetCompanyParty(interAdvance.CompanyId,interAdvance.PlantId,interAdvance.PartyId,interAdvance.PartyType);
                    var companyPartyGLList = _accountsCommonService.GetCompanyPartyGL(companyParty["PartyId"].ToString(),companyParty["Id"].ToString(), PartyGLType.DownPaymentGL.ToString());
                   
                  
                    var interAdvanceDetail = _advanceService.InsertAdvanceDetail(interAdvance, new AdvanceDetail
                    {
                        GLGeneralInfoId = companyPartyGLList["GLGeneralInfoId"].ToString(),
                        BudgetMasterId = companyPartyGLList["BudgetMasterId"].ToString(),
                        ActivityId = companyPartyGLList["ActivityId"].ToString(),
                        CompanyId = interAdvance.CompanyId,
                        PlantId = interAdvance.PlantId,
                        PartyType = PartyType.Customer.ToString(),
                        PartyId = interAdvance.PartyId,
                        PartyPlantId = interAdvance.PartyPlantId,
                        Amount = interAdvance.Amount,
                        NetAmount = interAdvance.Amount,
                        Narration = interAdvance.Narration,
                    }, 1);

                    // INSERT INTO VoucherDetail (liability side Cr)
                    var voucherDetailCr = new VoucherDetail
                    {
                        Narration = interAdvanceDetail.Narration,
                        CrAmount = interAdvanceDetail.NetAmount,
                        PartyId = interAdvanceDetail.PartyId,
                        PartyType = interAdvanceDetail.PartyType,
                        PartyPlantId = interAdvanceDetail.PartyPlantId,
                        AdvanceDetailId = interAdvanceDetail.Id,
                        GLGeneralInfoId = interAdvanceDetail.GLGeneralInfoId,
                        BudgetMasterId = interAdvanceDetail.BudgetMasterId,
                        ActivityId = interAdvanceDetail.ActivityId
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
                        CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate
                    });
                }
                else if (voucherVM.SettlementType == SettlementType.Charge.ToString())
                {
                    var currentBankChargeDetailId = 1;
                    var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                    {
                        FinancingTypeId = voucherVM.FinancingTypeId,
                        AdvanceId = advance.Id,
                        AdvanceWriteOffId = advanceWriteOff.Id,
                        BankMasterId = advance.BankMasterId,
                        CashMasterId = advance.CashMasterId,
                        Archive = advanceWriteOff.Archive,
                        SourceType = advanceWriteOff.SourceType,
                        Amount = advanceWriteOff.Amount,
                        Narration = advanceWriteOff.Narration,
                        AddedBy = advanceWriteOff.AddedBy,
                        AddedDate = advanceWriteOff.AddedDate,
                        AddedFromIP = advanceWriteOff.AddedFromIP
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
                        DrAmount = voucherDetailChargeDr.DrAmount * voucherVM.CompanyCurrencyRate
                    });
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

        public string InsertCustomerInvoiceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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

                var voucherFlag = voucherDetailVMList.Any(r => r.ExchangeType == "ExchangeGain" || r.ExchangeType == "ExchangeLoss");

                var voucher = _voucherService.InsertVoucher(voucherVM);
                // INSERT INTO AdvanceWriteOff table.
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);
                advanceWriteOff.VoucherId = voucher.Id;

                var totalAmountDr = voucherDetailVMList != null ? voucherDetailVMList.Sum(r => r.DrAmount) : voucherVM.Amount;
                //var totalAmountCr = 0.00M;
                // Set total Debit amount in write of master.
                advanceWriteOff.Amount = totalAmountDr;
              
                // Advance
                var advance = _advanceService.Find(voucherVM.AdvanceId);
                if (null == advance)
                    throw new CustomException("Advance Id not found!");
                advance.WrittenOffAmount += totalAmountDr;
                advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                advance.UpdatedBy = advanceWriteOff.AddedBy;
                advance.UpdatedDate = advanceWriteOff.AddedDate;
                advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                _advanceService.Update(advance);

                var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                if (null == advanceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                advanceDetail.WrittenOffAmount += totalAmountDr;
                if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");
                advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                advanceDetail.UpdatedBy = advance.UpdatedBy;
                advanceDetail.UpdatedDate = advance.UpdatedDate;
                advanceDetail.UpdatedFromIP = advance.UpdatedFromIP;
                _advanceService.UpdateAdvanceDetail(advanceDetail);

                var advanceWriteOffDetail = new AdvanceWriteOffDetail
                {
                    AdvanceId = advance.Id,
                    AdvanceDetailId = advanceDetail.Id,
                    AdvanceWriteOffId = advanceWriteOff.Id,
                    GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceDetail.BudgetMasterId,
                    ActivityId = advanceDetail.ActivityId,
                    CurrencyId = advance.CurrencyId,
                    PartyType = advanceDetail.PartyType,
                    CompanyId = advanceDetail.CompanyId,
                    PlantId = advanceDetail.PlantId,
                    PartyId = advanceDetail.PartyId,
                    PartyPlantId = advanceDetail.PartyPlantId,
                    Amount = advanceWriteOff.Amount
                };
                InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, 1);

                // INSERT INTO Voucher
                var currentVoucherDetailId = 0;

                
                if (voucherFlag)
                {
                    // Set to InvoiceWriteOff
                    // INSERT INTO VoucherDetail Debit
                    var voucherDetailDr = new VoucherDetail
                    {
                        AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                        GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                        BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                        ActivityId = advanceWriteOffDetail.ActivityId,
                        PartyType = advanceWriteOffDetail.PartyType,
                        PartyId = advanceWriteOffDetail.PartyId,
                        PartyPlantId = advanceWriteOffDetail.PartyPlantId
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    // INSERT INTO VoucherDetailCurrency
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                        CrAmount = voucherDetailDr.CrAmount * voucherVM.CompanyCurrencyRate
                    });
                }

                // INSERT INTO InvoiceWriteOff
                var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                invoiceWriteOff.VoucherId = voucher.Id;
                invoiceWriteOff.Amount = totalAmountDr;


                // Invoice
                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                var currentInvoiceDetail = 0;
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

                //if (totalAmountDr != totalAmountCr)
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

        public string InsertVendorAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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
                // INSERT INTO AdvanceWriteOff
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);
                var totalCrAmount = 0.0M;
                if(voucherDetailVMList!=null)
                {
                    totalCrAmount = voucherDetailVMList.Sum(r => r.DrAmount);
                }
                else
                {
                    totalCrAmount = voucherVM.Amount;
                }
                 

                // Set total Credit amount in write of master.
                advanceWriteOff.Amount = totalCrAmount;

                

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOff.VoucherId = voucher.Id;
                
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalCurrencyAmountDr = 0.0M;
                var totalCurrencyAmountCr = 0.0M;
                // Advance
                var advance = _advanceService.Find(voucherVM.AdvanceId);
                if (null == advance)
                    throw new CustomException("Advance Id not found!");
                advance.WrittenOffAmount += totalCrAmount;
                advance.IsWrittenOff = advance.Amount + advance.AdditionalAmount == advance.WrittenOffAmount;
                advance.UpdatedBy = advanceWriteOff.AddedBy;
                advance.UpdatedDate = advanceWriteOff.AddedDate;
                advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                _advanceService.Update(advance);

                var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                if (null == advanceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                advanceDetail.WrittenOffAmount += totalCrAmount;
                if (advanceDetail.Amount + advanceDetail.AdditionalAmount < advanceDetail.WrittenOffAmount)
                    throw new CustomException("Invoice amount can not cross Advance amount.");
                advanceDetail.IsWrittenOff = advanceDetail.Amount + advanceDetail.AdditionalAmount == advanceDetail.WrittenOffAmount;
                advanceDetail.UpdatedBy = advanceWriteOff.AddedBy;
                advanceDetail.UpdatedDate = advanceWriteOff.AddedDate;
                advanceDetail.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                _advanceService.UpdateAdvanceDetail(advanceDetail);

                var advanceWriteOffDetail = new AdvanceWriteOffDetail
                {
                    AdvanceId = advance.Id,
                    AdvanceDetailId = advanceDetail.Id,
                    AdvanceWriteOffId = advanceWriteOff.Id,
                    GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceDetail.BudgetMasterId,
                    ActivityId = advanceDetail.ActivityId,
                    CurrencyId = advance.CurrencyId,
                    PartyType = advanceDetail.PartyType,
                    CompanyId = advanceDetail.CompanyId,
                    PlantId = advanceDetail.PlantId,
                    PartyId = advanceDetail.PartyId,
                    PartyPlantId = advanceDetail.PartyPlantId,
                    Amount = advanceWriteOff.Amount,
                    BooksAmount = Math.Round((advanceWriteOff.Amount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                };
                InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, 1);

                // INSERT INTO VoucherDetail Credit
                var currentVoucherDetailId = 0;
                var voucherDetailCr = new VoucherDetail
                {
                    AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                    GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                    ActivityId = advanceWriteOffDetail.ActivityId,
                    CurrencyId = advanceWriteOffDetail.CurrencyId,
                    EntityId = voucher.EntityId,
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    CrAmount = advanceWriteOffDetail.Amount,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = voucher.Narration,
                    PartyType = advanceWriteOff.PartyType,
                    PartyId = advanceWriteOff.PartyId,
                    PartyPlantId = advanceWriteOff.PartyPlantId
                };
                totalAmountCr += voucherDetailCr.CrAmount;
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailCr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                    CrAmount = Math.Round((voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                });
                totalCurrencyAmountCr += Math.Round((voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);

                // Invoice
                if (voucherVM.SettlementType == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO InvoiceWriteOff
                    var invoiceWriteOff = _invoiceWriteOffService.InsertInvoiceWriteOff(voucherVM);
                    invoiceWriteOff.VoucherId = voucher.Id;
                    var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                    var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                    var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                    var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();

                    var currentInvoiceDetail = 0;
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                        if (null == invoiceDetail)
                            throw new CustomException("Invoice Id not found!");
                        invoiceDetail.WrittenOffAmount += voucherDetailVM.DrAmount;
                        if (invoiceDetail.NetAmount + invoiceDetail.AdditionalAmount < invoiceDetail.WrittenOffAmount)
                            throw new CustomException("Invoice amount can not cross balance amount.");
                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount + invoiceDetail.AdditionalAmount == invoiceDetail.WrittenOffAmount;
                        invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                        var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                        invoice.WrittenOffAmount = invoiceDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount + invoice.AdditionalAmount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                        invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                        invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                        _invoiceService.UpdateGraph(invoice);

                        // INSERT INTO InvoiceWriteOffDetail
                        var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                        {
                            GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceDetail.BudgetMasterId,
                            ActivityId = invoiceDetail.ActivityId,
                            CurrencyId = invoice.CurrencyId,
                            InvoiceWriteOffId = invoiceWriteOff.Id,
                            InvoiceId = invoiceDetail.InvoiceId,
                            InvoiceDetailId = invoiceDetail.Id,
                            Amount = voucherDetailVM.DrAmount,
                            DocDate = invoice.DocDate,
                            DocRefNo = invoice.DocRefNo,
                            Narration = invoice.Narration
                        };
                        currentInvoiceDetail++;
                        _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceDetail);

                        // Set amount in master.
                        invoiceWriteOff.Amount += invoiceWriteOffDetail.Amount;

                        var voucherDetailDr = new VoucherDetail
                        {
                            InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                            GLGeneralInfoId = invoiceWriteOffDetail.GLGeneralInfoId,
                            BudgetMasterId = invoiceWriteOffDetail.BudgetMasterId,
                            ActivityId = invoiceWriteOffDetail.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            EntityId = voucher.EntityId,
                            DrAmount = invoiceWriteOffDetail.Amount,
                            PartyType = advanceWriteOff.PartyType,
                            PartyId = advanceWriteOff.PartyId,
                            PartyPlantId = advanceWriteOff.PartyPlantId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                        totalAmountDr += voucherDetailDr.DrAmount;
                        totalAmountCr += voucherDetailDr.CrAmount;

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
                            totalCurrencyAmountCr += voucherDetailVM.ExchangeAmount;
                        }

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1 / voucherDetailVM.CompanyCurrencyRate,
                            DrAmount = Math.Round((voucherDetailDr.DrAmount * voucherDetailVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountDr += Math.Round((voucherDetailDr.DrAmount * voucherDetailVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);
                    }
                }
                else if (voucherVM.SettlementType == SettlementType.Return.ToString() && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                    throw new CustomException("Bank Id is null!");

                var bankMaster = _accountsCommonService.GetBankMaster(voucherVM.BankMasterId);

                // INSERT INTO VoucherDetail
                var voucherDetailDr = new VoucherDetail
                {
                    GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString(),
                    BudgetMasterId = bankMaster["BudgetMasterId"].ToString(),
                    ActivityId = bankMaster["ActivityId"].ToString(),
                    EntityId = voucherVM.EntityId,
                    AddedBy = voucher.AddedBy,
                    AddedDate = voucher.AddedDate,
                    AddedFromIP = voucher.AddedFromIP,
                    DrAmount = voucherVM.Amount,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PartyType = PartyType.Bank.ToString(),
                    BankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    PaymentSource = voucherVM.PaymentSource
                };
                    if (voucherVM.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailDr.DrAmount -= voucherVM.RoundingAmount;
                    if (voucherVM.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailDr.DrAmount += voucherVM.RoundingAmount;

                    currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                totalAmountDr += voucherDetailDr.DrAmount;
                totalAmountCr += voucherDetailDr.CrAmount;

                    // INSRT INTO GLTransactionDetail
                    var glTransactionDetail = new GLTransactionDetail
                    {
                        VoucherDetailId = voucherDetailDr.Id,
                        SourceType = voucherDetailDr.PaymentSource,
                        BankMasterId = voucherDetailDr.BankMasterId,
                        CashMasterId = voucherDetailDr.CashMasterId,
                        AddedBy = voucherDetailDr.AddedBy,
                        AddedDate = voucherDetailDr.AddedDate,
                        AddedFromIP = voucherDetailDr.AddedFromIP
                    };

                    if (bankMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                        glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                    else
                        glTransactionDetail.DrAmount = Math.Round((voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);

                    _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);


                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = Math.Round((voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                });
                    totalCurrencyAmountDr += Math.Round((voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);

                if (voucherVM.CompanyCurrencyRate > advance.CompanyCurrencyRate)
                {
                    var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                    var voucherDetailGain = new VoucherDetail
                    {
                        GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                        BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                        ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                        CurrencyId = voucher.CurrencyId
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
                        CrAmount = Math.Round((voucherDetailDr.DrAmount * (voucherVM.CompanyCurrencyRate - advance.CompanyCurrencyRate)), 2, MidpointRounding.AwayFromZero)
                    });
                        totalCurrencyAmountCr += Math.Round((voucherDetailDr.DrAmount * (voucherVM.CompanyCurrencyRate - advance.CompanyCurrencyRate)), 2, MidpointRounding.AwayFromZero);
                }
                else if (voucherVM.CompanyCurrencyRate < advance.CompanyCurrencyRate)
                {
                    var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                    var voucherDetailLoss = new VoucherDetail
                    {
                        GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                        BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                        ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                        CurrencyId = voucher.CurrencyId
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailLoss, currentVoucherDetailId);

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoss, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailLoss.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.OtherCompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = Math.Round((voucherDetailDr.DrAmount * (advance.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate)), 2, MidpointRounding.AwayFromZero)
                    });
                  totalCurrencyAmountDr += Math.Round((voucherDetailDr.DrAmount * (advance.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate)), 2, MidpointRounding.AwayFromZero);
                }
            }
                else if (voucherVM.SettlementType == SettlementType.Return.ToString() && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id is null!");


                    var cashMaster = _accountsCommonService.GetCashMaster(voucherVM.CashMasterId);

                    // INSERT INTO VoucherDetail
                    var voucherDetailDr = new VoucherDetail
                    {
                        GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString(),
                        BudgetMasterId = cashMaster["BudgetMasterId"].ToString(),
                        ActivityId = cashMaster["ActivityId"].ToString(),
                        EntityId = voucherVM.EntityId,
                        AddedBy = voucher.AddedBy,
                        AddedDate = voucher.AddedDate,
                        AddedFromIP = voucher.AddedFromIP,
                        DrAmount = voucherVM.Amount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        PartyType = PartyType.Bank.ToString(),
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };
                    if (voucherVM.RoundingType == RoundingType.RoundDown.ToString())
                        voucherDetailDr.DrAmount -= voucherVM.RoundingAmount;
                    if (voucherVM.RoundingType == RoundingType.RoundUp.ToString())
                        voucherDetailDr.DrAmount += voucherVM.RoundingAmount;

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);
                    totalAmountDr += voucherDetailDr.DrAmount;
                    totalAmountCr += voucherDetailDr.CrAmount;

                    // INSRT INTO GLTransactionDetail
                    var glTransactionDetail = new GLTransactionDetail
                    {
                        VoucherDetailId = voucherDetailDr.Id,
                        SourceType = voucherDetailDr.PaymentSource,
                        BankMasterId = voucherDetailDr.BankMasterId,
                        CashMasterId = voucherDetailDr.CashMasterId,
                        AddedBy = voucherDetailDr.AddedBy,
                        AddedDate = voucherDetailDr.AddedDate,
                        AddedFromIP = voucherDetailDr.AddedFromIP
                    };

                    if (cashMaster["CurrencyId"].ToString() == voucherVM.CurrencyId)
                        glTransactionDetail.DrAmount = voucherDetailDr.DrAmount;
                    else
                        glTransactionDetail.DrAmount = Math.Round((voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);

                    _voucherService.InsertGLTransactionDetail(voucherDetailDr, glTransactionDetail);

                        
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = Math.Round((voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero)
                    });
                    totalCurrencyAmountDr += Math.Round((voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate), 2, MidpointRounding.AwayFromZero);
                    if (voucherVM.CompanyCurrencyRate > advance.CompanyCurrencyRate)
                    {
                        var gainGL = _accountsCommonService.GetExchangeGainGL(FinancingTypeEnum.Payable);
                        var voucherDetailGain = new VoucherDetail
                        {
                            GLGeneralInfoId = gainGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = gainGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = gainGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId
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
                            CrAmount = Math.Round((voucherDetailDr.DrAmount * (voucherVM.CompanyCurrencyRate - advance.CompanyCurrencyRate)), 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountCr += Math.Round((voucherDetailDr.DrAmount * (voucherVM.CompanyCurrencyRate - advance.CompanyCurrencyRate)), 2, MidpointRounding.AwayFromZero);
                    }
                    else if (voucherVM.CompanyCurrencyRate < advance.CompanyCurrencyRate)
                    {
                        var lossGL = _accountsCommonService.GetExchangeLossGL(FinancingTypeEnum.Payable);
                        var voucherDetailLoss = new VoucherDetail
                        {
                            GLGeneralInfoId = lossGL["CompanyCurrencyGLId"].ToString(),
                            BudgetMasterId = lossGL["CompanyCurrencyBudgetMasterId"].ToString(),
                            ActivityId = lossGL["CompanyCurrencyActivityId"].ToString(),
                            CurrencyId = voucher.CurrencyId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailLoss, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailLoss, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailLoss.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.OtherCompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailLoss.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = Math.Round((voucherDetailDr.DrAmount * (advance.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate)), 2, MidpointRounding.AwayFromZero)
                        });
                        totalCurrencyAmountDr += Math.Round((voucherDetailDr.DrAmount * (advance.CompanyCurrencyRate - voucherVM.CompanyCurrencyRate)), 2, MidpointRounding.AwayFromZero);
                    }
                }

                if (!string.IsNullOrEmpty(voucherVM.RoundingType))
                {
                    if (voucherVM.RoundingType == RoundingType.RoundDown.ToString() || voucherVM.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(voucherVM.CompanyId);
                        if (voucherVM.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingCr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = voucherVM.RoundingAmount,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherVM.Narration,
                                PartyType = voucherVM.PartyType
                            };
                            currentVoucherDetailId++;
                            totalAmountCr += voucherDetailRoundingCr.CrAmount;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingCr, currentVoucherDetailId);

                            var voucherDetailCurrencyRoundingDr = _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailRoundingCr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailRoundingCr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailRoundingCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailRoundingCr.CrAmount), 3, MidpointRounding.AwayFromZero)
                            });
                            totalCurrencyAmountCr += voucherDetailCurrencyRoundingDr.CrAmount;
                        }
                        if (voucherVM.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = voucherVM.RoundingAmount,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucherVM.Narration,
                                PartyType = voucherVM.PartyType
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

        public string InsertVendorAdvanceWriteOffDifferentCurrency(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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
                // INSERT INTO AdvanceWriteOff
                var AdvanceCurrencyId = "";
                AdvanceCurrencyId = voucherVM.CurrencyId;
                var advanceWriteOffVM = new AdvanceWriteOff
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
                    CurrencyId = AdvanceCurrencyId,
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
                };
                var advanceWriteOff = InsertAdvanceWriteOffDifferentCurrency(advanceWriteOffVM);

                var totalCrAmount = 0.0M;

                // Set total Credit amount in write of master.
                if (AdvanceCurrencyId == companyCurrencyId)
                {
                    totalCrAmount = Math.Round(voucherDetailVMList.Sum(r => r.DrAmount * r.CompanyCurrencyRate), 4);
                }
                else
                {
                    totalCrAmount = Math.Round(voucherDetailVMList.Sum(r => r.DrAmount) / voucherVM.CompanyCurrencyRate, 4);
                }

                advanceWriteOff.Amount += totalCrAmount;
                // INSERT INTO InvoiceWriteOff
                var invoiceWriteOffVM = new InvoiceWriteOff
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    CurrencyId = voucherDetailVMList.FirstOrDefault().CurrencyId,
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
               
                var invoiceWriteOff = _invoiceWriteOffService.InsertInvoiceWriteOffDifferentCurrency(invoiceWriteOffVM);

                // INSERT INTO Voucher
                
                voucherVM.CurrencyId = companyCurrencyId;
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOff.VoucherId = voucher.Id;
                invoiceWriteOff.VoucherId = voucher.Id;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                var totalAmountDrCurrency = 0.0M;
                var totalAmountCrCurrency = 0.0M;
                var DrAmountCurrency = 0.0M;
                // Advance
                var advance = _advanceService.Find(voucherVM.AdvanceId);
                if (null == advance)
                    throw new CustomException("Advance Id not found!");
                
                advance.WrittenOffAmount += totalCrAmount;
                advance.IsWrittenOff = advance.Amount + advance.AdditionalAmount == totalCrAmount;
                advance.UpdatedBy = invoiceWriteOff.AddedBy;
                advance.UpdatedDate = invoiceWriteOff.AddedDate;
                advance.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                _advanceService.Update(advance);

                var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                if (null == advanceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                
                advanceDetail.WrittenOffAmount += totalCrAmount;
                if (advanceDetail.Amount + advanceDetail.AdditionalAmount < advanceDetail.WrittenOffAmount)
                    throw new CustomException("Invoice amount can not cross Advance amount.");
                
                advanceDetail.IsWrittenOff = advanceDetail.Amount + advanceDetail.AdditionalAmount == advanceDetail.WrittenOffAmount ;
                advanceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                advanceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                advanceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                _advanceService.UpdateAdvanceDetail(advanceDetail);

                var booksAmount = 0.0M;
                if (AdvanceCurrencyId == companyCurrencyId)
                {
                    booksAmount = advanceWriteOff.Amount;
                   
                }
                else
                {
                    booksAmount = voucherDetailVMList.Sum(r => r.DrAmount);
                   
                }

                var advanceWriteOffDetail = new AdvanceWriteOffDetail
                {
                    AdvanceId = advance.Id,
                    AdvanceDetailId = advanceDetail.Id,
                    AdvanceWriteOffId = advanceWriteOff.Id,
                    GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceDetail.BudgetMasterId,
                    ActivityId = advanceDetail.ActivityId,
                    CurrencyId = advance.CurrencyId,
                    PartyType = advanceDetail.PartyType,
                    CompanyId = advanceDetail.CompanyId,
                    PlantId = advanceDetail.PlantId,
                    PartyId = advanceDetail.PartyId,
                    PartyPlantId = advanceDetail.PartyPlantId,
                    Amount = advanceWriteOff.Amount,
                    BooksAmount = booksAmount
                };
                InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, 1);

                // INSERT INTO VoucherDetail Credit
                var currentVoucherDetailId = 0;
                var voucherDetailCr = new VoucherDetail
                {
                    AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                    GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                    ActivityId = advanceWriteOffDetail.ActivityId,
                    CurrencyId = advanceWriteOffDetail.CurrencyId,
                    EntityId = voucher.EntityId,
                    FiscalYearId = voucher.FiscalYearId,
                    FiscalYearPeriodId = voucher.FiscalYearPeriodId,
                    CrAmount = advanceWriteOffDetail.Amount,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = voucher.Narration,
                    PartyType = advanceWriteOff.PartyType,
                    PartyId = advanceWriteOff.PartyId,
                    PartyPlantId = advanceWriteOff.PartyPlantId
                };
               
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetailDifferentCurrency(voucher, voucherDetailCr, currentVoucherDetailId);
               
                var CrAmountCurrency = 0.0M;
                if (AdvanceCurrencyId == companyCurrencyId)
                {
                    CrAmountCurrency = voucherDetailCr.CrAmount;
                    totalAmountCr += voucherDetailCr.CrAmount;
                }
                else
                {
                    CrAmountCurrency = voucherDetailVMList.Sum(r => r.DrAmount);
                    totalAmountCr += voucherDetailVMList.Sum(r => r.DrAmount);
                }
                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailCr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                    CrAmount = CrAmountCurrency
                });;
                totalAmountCrCurrency += CrAmountCurrency;
                // Invoice
                var invoiceIds = voucherDetailVMList.Select(r => r.InvoiceId);
                var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                var invoiceDetailIds = voucherDetailVMList.Select(r => r.InvoiceDetailId);
                var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();

                var currentInvoiceDetail = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    var invoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                    if (null == invoiceDetail)
                        throw new CustomException("Invoice Id not found!");
                    invoiceDetail.WrittenOffAmount += voucherDetailVM.DrAmount;
                    if (invoiceDetail.NetAmount + invoiceDetail.AdditionalAmount < invoiceDetail.WrittenOffAmount)
                        throw new CustomException("Invoice amount can not cross balance amount.");
                    invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount + invoiceDetail.AdditionalAmount == invoiceDetail.WrittenOffAmount;
                    invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                    var invoice = inviceDbList.First(r => r.Id == invoiceDetail.InvoiceId);
                    invoice.WrittenOffAmount = invoiceDetail.WrittenOffAmount;
                    invoice.IsWrittenOff = invoice.Amount + invoice.AdditionalAmount == invoice.WrittenOffAmount;
                    invoice.UpdatedBy = invoiceWriteOff.AddedBy;
                    invoice.UpdatedDate = invoiceWriteOff.AddedDate;
                    invoice.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                    _invoiceService.UpdateGraph(invoice);

                    // INSERT INTO InvoiceWriteOffDetail
                    var invoiceWriteOffDetail = new InvoiceWriteOffDetail
                    {
                        GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                        BudgetMasterId = invoiceDetail.BudgetMasterId,
                        ActivityId = invoiceDetail.ActivityId,
                        CurrencyId = invoice.CurrencyId,
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        InvoiceId = invoiceDetail.InvoiceId,
                        InvoiceDetailId = invoiceDetail.Id,
                        Amount = voucherDetailVM.DrAmount,
                        DocDate = invoice.DocDate,
                        DocRefNo = invoice.DocRefNo,
                        Narration = invoice.Narration
                    };
                    currentInvoiceDetail++;
                    _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceDetail);

                    // Set amount in master.
                    invoiceWriteOff.Amount += invoiceWriteOffDetail.Amount;

                    if (AdvanceCurrencyId == companyCurrencyId)
                    {
                        DrAmountCurrency = Math.Round(invoiceWriteOffDetail.Amount * voucherDetailVM.CompanyCurrencyRate, 4);
                    }
                    else
                    {
                        DrAmountCurrency = Math.Round(invoiceWriteOffDetail.Amount, 4);
                    }

                    var voucherDetailDr = new VoucherDetail
                    {
                        InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                        GLGeneralInfoId = invoiceWriteOffDetail.GLGeneralInfoId,
                        BudgetMasterId = invoiceWriteOffDetail.BudgetMasterId,
                        ActivityId = invoiceWriteOffDetail.ActivityId,
                        CurrencyId = voucherDetailVM.CurrencyId,
                        EntityId = voucher.EntityId,
                        DrAmount =  DrAmountCurrency,
                        PartyType = advanceWriteOff.PartyType,
                        PartyId = advanceWriteOff.PartyId,
                        PartyPlantId = advanceWriteOff.PartyPlantId
                    };
                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetailDifferentCurrency(voucher, voucherDetailDr, currentVoucherDetailId);
                    
                    
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailDr.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                        ToCurrencyConversion = 1 / voucherDetailVM.CompanyCurrencyRate,
                        DrAmount = DrAmountCurrency
                    });
                    totalAmountDr += DrAmountCurrency;
                    totalAmountDrCurrency += DrAmountCurrency;
                }
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                if (totalAmountDrCurrency != totalAmountCrCurrency)
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
        private string GetEmployeeSubsequentTransactionPK()
        {
            return _pKGeneratorService.GetAutoNumber("EmployeeSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertEmployeeAdvanceWriteOff(VoucherViewModel voucherVM, VoucherDetailViewModel VoucherDetailVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailGLList)
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
                // INSERT INTO AdvanceWriteOff
                if (voucherDetailVMList != null)
                {
                    voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                }
                if (voucherDetailGLList != null)
                {
                    voucherVM.Amount = voucherDetailGLList.Sum(r => r.DrAmount);
                }
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOff.VoucherId = voucher.Id;

                var currentAdvanceWriteOffDetailId = 0;
                var currentInvoiceDetail = 0;
                var currentVoucherDetailId = 0;

                // Advance
                if(voucherVM.AdvanceId != null)
                { 
                    var advance = _advanceService.Find(voucherVM.AdvanceId);
                    advance.WrittenOffAmount += advanceWriteOff.Amount;
                    advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                    advance.UpdatedBy = advanceWriteOff.AddedBy;
                    advance.UpdatedDate = advanceWriteOff.AddedDate;
                    advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                    _advanceService.Update(advance);

                    var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                    if (null == advanceDetail)
                        throw new CustomException("Advance detail not found!");
                    advanceWriteOff.Amount += voucherVM.Amount;
                    advanceDetail.WrittenOffAmount += voucherVM.Amount;

                    if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                        throw new CustomException($"{advanceWriteOff.SettlementType} amount cannot exceed the balance advance amount.");

                    advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                    advanceDetail.UpdatedBy = advance.AddedBy;
                    advanceDetail.UpdatedDate = advance.AddedDate;
                    advanceDetail.UpdatedFromIP = advance.AddedFromIP;
                    _advanceService.UpdateAdvanceDetail(advanceDetail);
                }

                currentAdvanceWriteOffDetailId++;
                var advanceWriteOffDetail = new AdvanceWriteOffDetail
                {
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    AdvanceId = voucherVM.AdvanceId,
                    AdvanceDetailId = voucherVM.AdvanceDetailId,
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                    CurrencyId = advanceWriteOff.CurrencyId,
                    PartyType = voucherVM.PartyType,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    Amount = voucherVM.Amount,
                    EmployeeId= voucherVM.EmployeeId,
                    EmployeeAdvanceDetailId = VoucherDetailVM.EmployeeAdvanceDetailId
                };
                InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                

                // INSERT INTO VoucherDetail Debit or Credit
                var voucherDetail = new VoucherDetail
                {
                    GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                    ActivityId = advanceWriteOffDetail.ActivityId,
                    EmployeeId = advanceWriteOff.EmployeeId,
                    CurrencyId = voucher.CurrencyId,
                    EntityId = voucher.EntityId,
                    AdvanceWriteOffDetailId = advanceWriteOffDetail.Id
                };

                if (advanceWriteOff.SettlementType == SettlementType.SetOff.ToString())
                {
                    voucherDetail.CrAmount = advanceWriteOffDetail.Amount;
                    voucherDetail.DrAmount = 0;
                }
                else if (advanceWriteOff.SettlementType == SettlementType.Return.ToString()|| advanceWriteOff.SettlementType == SettlementType.Others.ToString())
                {
                    voucherDetail.DrAmount = 0;
                    voucherDetail.CrAmount = advanceWriteOffDetail.Amount;
                }
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetail.DrAmount,
                    CrAmount = voucherDetail.CrAmount
                });

                if (null != voucherDetailVMList && voucherDetailVMList.Count() > 0 && advanceWriteOff.SettlementType == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO EmployeePayableWriteOff
                    var employeePayableWriteOff = new EmployeePayableWriteOff
                    {
                        CompanyGroupId = advanceWriteOff.CompanyGroupId,
                        CompanyId = advanceWriteOff.CompanyId,
                        PlantId = advanceWriteOff.PlantId,
                        EmployeeId = voucherVM.EmployeeId,
                        FiscalYearId = advanceWriteOff.FiscalYearId,
                        FiscalYearPeriodId = advanceWriteOff.FiscalYearPeriodId,
                        TaxYearId = advanceWriteOff.TaxYearId,
                        TaxYearPeriodId = advanceWriteOff.TaxYearPeriodId,
                        VoucherTypeId = advanceWriteOff.VoucherTypeId,
                        CurrencyId = advanceWriteOff.CurrencyId,
                        SourceType = advanceWriteOff.SourceType,
                        PartyType = advanceWriteOff.PartyType,
                        VoucherDate = advanceWriteOff.VoucherDate,
                        PostingDate = advanceWriteOff.PostingDate,
                        DocDate = advanceWriteOff.DocDate,
                        DocRefNo = advanceWriteOff.DocRefNo,
                        Narration = advanceWriteOff.Narration,
                        Amount = voucherVM.Amount,
                        AddedBy = advanceWriteOff.AddedBy,
                        AddedDate = advanceWriteOff.AddedDate,
                        AddedFromIP = advanceWriteOff.AddedFromIP,
                        RowState = RowState.Parked.ToString()
                    };
                    _employeePayableWriteOffService.InsertEmployeePayableWriteOff(employeePayableWriteOff);
                    employeePayableWriteOff.VoucherId = voucher.Id;

                    // EmployeePayable
                    var employeePayableIds = voucherDetailVMList.Select(r => r.EmployeePayableId);
                    var employeePayableDbList = _employeePayableService.GetEmployeePayableList(r => employeePayableIds.Contains(r.Id)).Select().ToList();
                    var employeePayableDetailIds = voucherDetailVMList.Select(r => r.EmployeePayableDetailId);
                    var employeePayableDetailDbList = _employeePayableService.GetEmployeePayableDetailList(r => employeePayableDetailIds.Contains(r.Id)).Select().ToList();

                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        var employeePayableDetail = employeePayableDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.EmployeePayableDetailId);
                        if (null == employeePayableDetail)
                            throw new CustomException("Invoice not found!");
                        employeePayableDetail.WrittenOffAmount += voucherDetailVM.Amount;

                        if (employeePayableDetail.NetAmount < employeePayableDetail.WrittenOffAmount)
                            throw new CustomException("Set-Off amount cannot exceed the balance Payable amount.");

                        employeePayableDetail.IsWrittenOff = employeePayableDetail.NetAmount == employeePayableDetail.WrittenOffAmount;
                        employeePayableDetail.UpdatedBy = employeePayableWriteOff.AddedBy;
                        employeePayableDetail.UpdatedDate = employeePayableWriteOff.AddedDate;
                        employeePayableDetail.UpdatedFromIP = employeePayableWriteOff.AddedFromIP;
                        _employeePayableService.UpdateEmployeePayableDetail(employeePayableDetail);

                        // TODO: have a gap here if invoice split
                        var invoice = employeePayableDbList.First(r => r.Id == employeePayableDetail.EmployeePayableId);
                        invoice.WrittenOffAmount = employeePayableDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = employeePayableWriteOff.AddedBy;
                        invoice.UpdatedDate = employeePayableWriteOff.AddedDate;
                        invoice.UpdatedFromIP = employeePayableWriteOff.AddedFromIP;
                        _employeePayableService.UpdateEmployeePayable(invoice);

                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceWriteOffDetail
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
                            AddedBy = employeePayableWriteOff.AddedBy,
                            AddedDate = employeePayableWriteOff.AddedDate,
                            AddedFromIP = employeePayableWriteOff.AddedFromIP,
                            Archive = employeePayableWriteOff.Archive,
                            ModelState = employeePayableWriteOff.ModelState,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration
                        };
                        _employeePayableWriteOffService.InsertEmployeePayableWriteOffDetail(employeePayableWriteOff, employeePayableWriteOffDetail, currentInvoiceDetail);

                        // in liability side Cr.
                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            EntityId = voucherDetailVM.EntityId,
                            DrAmount = voucherDetailVM.Amount,
                            Narration = employeePayableWriteOff.Narration,
                            EmployeePayableWriteOffDetailId = employeePayableWriteOffDetail.Id,
                            EmployeeId = employeePayableWriteOff.EmployeeId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailDr.DrAmount
                        });

                        var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            AdvanceId = null,
                            EmployeeId = employeePayableWriteOff.EmployeeId,
                            AdvanceWriteOffId = null,
                            EmployeePayableWriteOffId = employeePayableWriteOff.Id,
                            EmployeePayableId = null,
                            PartyType = employeePayableWriteOff.PartyType,
                            CurrencyId = employeePayableWriteOff.CurrencyId,
                            Amount = voucherDetailDr.DrAmount,
                            VoucherDate = voucherVM.VoucherDate,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            JournalType = voucherVM.JournalType,
                            TransactionType = EmployeeSubsequentTranEnum.Payment.ToString(),
                            Narration = voucherVM.Narration,
                            SourceType = employeePayableWriteOff.SourceType,
                            IsPark = voucherVM.IsPark,
                            Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                            VoucherId = voucher.Id,
                            VoucherDetailId = voucherDetailDr.Id,
                            PaymentSource = voucherVM.PaymentSource,
                            EmployeeAdvanceDetailId = VoucherDetailVM.EmployeeAdvanceDetailId
                        };
                        AuditService.AddedLog(EmployeeSubsequentAdvance);
                        _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);

                    }
                }
                else if (advanceWriteOff.SettlementType == SettlementType.Return.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                        throw new CustomException("Bank Id not found!");
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id not found!");

                    // INSERT INTO VoucherDetail
                    var voucherDetailCr = new VoucherDetail
                    {
                        CurrencyId = voucher.CurrencyId,
                        DrAmount = voucherVM.Amount,
                        PartyType = voucherVM.PaymentSource,
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };

                    // INSRT INTO GLTransactionDetail
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherDetailCr.BankMasterId);
                        if (string.IsNullOrEmpty(bankMaster["GLGeneralInfoId"].ToString()))
                            throw new CustomException("GL Id not found!");
                        else if (string.IsNullOrEmpty(bankMaster["BudgetMasterId"].ToString()))
                            throw new CustomException("Budget Master Id not found!");
                        else if (string.IsNullOrEmpty(bankMaster["ActivityId"].ToString()))
                            throw new CustomException("Activity Id not found!");
                        voucherDetailCr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = bankMaster["ActivityId"].ToString();
                        voucherDetailCr.PartyType = PartyType.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        var cashMaster = _accountsCommonService.GetCashMaster(voucherDetailCr.CashMasterId);
                        if (string.IsNullOrEmpty(cashMaster["GLGeneralInfoId"].ToString()))
                            throw new CustomException("GL Id not found!");
                        else if (string.IsNullOrEmpty(cashMaster["BudgetMasterId"].ToString()))
                            throw new CustomException("Budget Master Id not found!");
                        else if (string.IsNullOrEmpty(cashMaster["ActivityId"].ToString()))
                            throw new CustomException("Activity Id not found!");
                        voucherDetailCr.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = cashMaster["ActivityId"].ToString();
                        voucherDetailCr.PartyType = PartyType.Cash.ToString();
                    }

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                    _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                    {
                        SourceType = voucherDetailCr.PaymentSource,
                        BankMasterId = voucherDetailCr.BankMasterId,
                        CashMasterId = voucherDetailCr.CashMasterId,
                        DrAmount = voucherDetailCr.DrAmount
                    });

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = companyCurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailCr.DrAmount,
                    });

                    var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        AdvanceId = null,
                        EmployeeId = voucherVM.EmployeeId,
                        AdvanceWriteOffId = advanceWriteOff.Id,
                        EmployeePayableWriteOffId = null,
                        EmployeePayableId = null,
                        PartyType = voucherVM.PartyType,
                        CurrencyId = voucherVM.CurrencyId,
                        Amount = voucherVM.Amount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        JournalType = voucherVM.JournalType,
                        TransactionType = EmployeeSubsequentTranEnum.Payment.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = voucherVM.SourceType,
                        IsPark = voucherVM.IsPark,
                        Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                        VoucherId = voucher.Id,
                        VoucherDetailId = voucherDetailCr.Id,
                        PaymentSource = voucherVM.PaymentSource,
                        EmployeeAdvanceDetailId = VoucherDetailVM.EmployeeAdvanceDetailId
                    };
                    AuditService.AddedLog(EmployeeSubsequentAdvance);
                    _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);
                }
                else if (advanceWriteOff.SettlementType == SettlementType.Others.ToString())
                {
                    if (voucherDetailGLList!=null) {
                        foreach (var glitem in voucherDetailGLList)
                        {
                            var voucherDetailCr = new VoucherDetail
                            {
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = glitem.DrAmount,
                                PartyType = voucherVM.PaymentSource,
                                PaymentSource = "GL",
                                GLGeneralInfoId = glitem.GLGeneralInfoId,
                                BudgetMasterId = glitem.BudgetMasterId,
                                ActivityId = glitem.ActivityId,
                                DocRefNo = glitem.DocRefNo,
                                Narration = glitem.Narration,
                            };



                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);


                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = voucherDetailCr.DrAmount,
                            });

                            var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                            {
                                CompanyGroupId = voucherVM.CompanyGroupId,
                                CompanyId = voucherVM.CompanyId,
                                PlantId = voucherVM.PlantId,
                                EntityId = voucherVM.EntityId,
                                VoucherTypeId = voucherVM.VoucherTypeId,
                                AdvanceId = null,
                                EmployeeId = voucherVM.EmployeeId,
                                AdvanceWriteOffId = advanceWriteOff.Id,
                                EmployeePayableWriteOffId = null,
                                EmployeePayableId = null,
                                PartyType = voucherVM.PartyType,
                                CurrencyId = voucherVM.CurrencyId,
                                Amount = glitem.DrAmount,
                                VoucherDate = voucherVM.VoucherDate,
                                PostingDate = voucherVM.PostingDate,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                JournalType = voucherVM.JournalType,
                                TransactionType = EmployeeSubsequentTranEnum.Payment.ToString(),
                                Narration = voucherVM.Narration,
                                SourceType = voucherVM.SourceType,
                                IsPark = voucherVM.IsPark,
                                Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                                VoucherId = voucher.Id,
                                VoucherDetailId = voucherDetailCr.Id,
                                PaymentSource = "GL",
                                EmployeeAdvanceDetailId = VoucherDetailVM.EmployeeAdvanceDetailId
                            };
                            AuditService.AddedLog(EmployeeSubsequentAdvance);
                            _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);
                        }
                    }
                   
                    
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                _unitOfWork.BeginTransaction();
                flag = true;
                if (VoucherDetailVM.EmployeeAdvanceDetailId != null)
                {
                    var direct = new System.Text.StringBuilder();
                    var directsql = "";
                    directsql = @"DECLARE @advanceAmount decimal(18,2),@writeOffAmount decimal(18,2),@IsWrittenOff bit=0,@newWrittenOffAmount decimal(18,2)=" + voucherVM.Amount + @",@employeeAdvanceDetailId varchar(50)='" + VoucherDetailVM.EmployeeAdvanceDetailId + @"'
                                select @advanceAmount =ISNULL(AdvanceAmount,0),@writeOffAmount =ISNULL(WrittenOffAmount,0) from [TRN].[EmployeeAdvanceDetail]  where Id=@employeeAdvanceDetailId
                                --print @writeOffAmount
                                --print @newWrittenOffAmount
                                --print @advanceAmount
                                IF(@advanceAmount = @writeOffAmount+@newWrittenOffAmount)
                                BEGIN 
	                                SET @IsWrittenOff =1
                                END
                                --print @IsWrittenOff
                                update [TRN].[EmployeeAdvanceDetail] set WrittenOffAmount= ISNULL(WrittenOffAmount,0) +  @newWrittenOffAmount, IsWrittenOff=@IsWrittenOff where Id=@employeeAdvanceDetailId ";
                    direct.Append(directsql);
                _sqlRepository.ExecuteSqlCommand(direct.ToString());
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
        public string InsertVendorPaymentEmployeeAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
               , IEnumerable<BankChargeViewModel> bankChargeDetailVMList,IEnumerable<VoucherViewModel> advanceVMList)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(voucherVM.BankMasterId) && voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    throw new CustomException("Bank Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.CashMasterId) && voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    throw new CustomException("Cash Id not found!");
                else if (string.IsNullOrEmpty(voucherVM.AdvanceId) && voucherVM.PaymentSource == PaymentSource.Employee.ToString())
                    throw new CustomException("Advance Id not found!");
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
               
                // INSERT INTO InvoiceWriteOff
                var invoiceWriteOff = _invoiceWriteOffService.InsertInvoiceWriteOff(voucherVM);

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
                    _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

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

                if (voucherVM.PaymentSource == PaymentSource.Employee.ToString())
                {
                    if (null != advanceVMList && advanceVMList.Count() > 0)
                    {
                        foreach (var advanceVM in advanceVMList)
                        {
                            advanceVM.CompanyGroupId = voucherVM.CompanyGroupId;
                            advanceVM.CompanyId = voucherVM.CompanyId;
                            advanceVM.PlantId = voucherVM.PlantId;
                            advanceVM.EntityId = voucherVM.EntityId;
                            advanceVM.FiscalYearId = voucherVM.FiscalYearId;
                            advanceVM.FiscalYearPeriodId = voucherVM.FiscalYearPeriodId;
                            advanceVM.TaxYearId = voucherVM.TaxYearId;
                            advanceVM.TaxYearPeriodId = voucherVM.TaxYearPeriodId;
                            advanceVM.VoucherTypeId = voucherVM.VoucherTypeId;
                            advanceVM.CurrencyId = voucherVM.CurrencyId;
                            advanceVM.PartyType = voucherVM.PartyType;
                            advanceVM.PartyId = voucherVM.PartyId;
                            advanceVM.PartyPlantId = voucherVM.PartyPlantId;
                            advanceVM.VoucherDate = voucherVM.VoucherDate;
                            advanceVM.PostingDate = voucherVM.PostingDate;
                            advanceVM.DocDate = voucherVM.DocDate;
                            advanceVM.DocRefNo = voucherVM.DocRefNo;
                            advanceVM.Narration = voucherVM.Narration;
                            advanceVM.SourceType = voucherVM.SourceType;
                            advanceVM.IsPark = voucherVM.IsPark;
                            advanceVM.SettlementType = voucherVM.SettlementType;
                            advanceVM.PaymentSource = voucherVM.PaymentSource;
                            var advanceWriteOff = InsertAdvanceWriteOff(advanceVM);
                            // Set to InvoiceWriteOff
                            advanceWriteOff.VoucherId = voucher.Id;

                            var currentAdvanceWriteOffDetailId = 0;

                            //Advance
                           var advance = _advanceService.Find(voucherVM.AdvanceId);
                            advance.WrittenOffAmount += advanceWriteOff.Amount;
                            advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                            advance.UpdatedBy = advanceWriteOff.AddedBy;
                            advance.UpdatedDate = advanceWriteOff.AddedDate;
                            advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                            _advanceService.Update(advance);

                            var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                            if (null == advanceDetail)
                                throw new CustomException("Advance detail not found!");

                            currentAdvanceWriteOffDetailId++;
                            var advanceWriteOffDetail = new AdvanceWriteOffDetail
                            {
                                CompanyId = advanceDetail.CompanyId,
                                PlantId = advanceDetail.PlantId,
                                AdvanceId = voucherVM.AdvanceId,
                                AdvanceDetailId = voucherVM.AdvanceDetailId,
                                GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                                BudgetMasterId = advanceDetail.BudgetMasterId,
                                ActivityId = advanceDetail.ActivityId,
                                CurrencyId = voucherVM.CurrencyId,
                                PartyType = advanceDetail.PartyType,
                                PartyId = advanceDetail.PartyId,
                                PartyPlantId = advanceDetail.PartyPlantId,
                                Amount = advanceVM.Amount,
                                EmployeeId = advanceVM.EmployeeId
                            };
                            InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                            advanceDetail.WrittenOffAmount += advanceVM.Amount;

                            if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                                throw new CustomException($"{advanceWriteOff.SettlementType} amount cannot exceed the balance advance amount.");

                            advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                            advanceDetail.UpdatedBy = advance.AddedBy;
                            advanceDetail.UpdatedDate = advance.AddedDate;
                            advanceDetail.UpdatedFromIP = advance.AddedFromIP;
                            _advanceService.UpdateAdvanceDetail(advanceDetail);

                            // INSERT INTO VoucherDetail Debit or Credit
                            var voucherDetailAdvance = new VoucherDetail
                            {
                                GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                                BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                                ActivityId = advanceWriteOffDetail.ActivityId,
                                EmployeeId = advanceVM.EmployeeId,
                                CurrencyId = voucher.CurrencyId,
                                EntityId = voucher.EntityId,
                                AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                                CrAmount = advanceWriteOffDetail.Amount,
                                DrAmount = 0
                            };

                            currentVoucherDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailAdvance, currentVoucherDetailId);
                            totalAmountCr += voucherDetailAdvance.CrAmount;
                            var voucherDetailCurrencyAdvance = new VoucherDetailCurrency
                            {
                                ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                                ToCurrencyId = companyCurrencyId,
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = companyCurrencyId,
                                CrAmount = Math.Round((voucherVM.CompanyCurrencyRate * voucherDetailAdvance.CrAmount), 3, MidpointRounding.AwayFromZero),
                                ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                            };
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailAdvance, voucherDetailCurrencyAdvance);
                            totalCurrencyAmountCr += voucherDetailCurrencyAdvance.CrAmount;

                            var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                            {
                                CompanyGroupId = voucherVM.CompanyGroupId,
                                CompanyId = voucherVM.CompanyId,
                                PlantId = voucherVM.PlantId,
                                EntityId = voucherVM.EntityId,
                                VoucherTypeId = voucherVM.VoucherTypeId,
                                AdvanceId = voucherVM.AdvanceId,
                                EmployeeId = advanceVM.EmployeeId,
                                AdvanceWriteOffId = advanceWriteOff.Id,
                                EmployeePayableWriteOffId = null,
                                EmployeePayableId = null,
                                PartyType = voucherVM.PartyType,
                                CurrencyId = voucherVM.CurrencyId,
                                Amount = advanceVM.Amount,
                                VoucherDate = voucherVM.VoucherDate,
                                PostingDate = voucherVM.PostingDate,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                JournalType = voucherVM.JournalType,
                                TransactionType = EmployeeSubsequentTranEnum.Payment.ToString(),
                                Narration = voucherVM.Narration,
                                SourceType = voucherVM.SourceType,
                                IsPark = voucherVM.IsPark,
                                Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                                VoucherId = voucher.Id,
                                VoucherDetailId = voucherDetailAdvance.Id,
                                PaymentSource = voucherVM.PaymentSource,
                            };
                            AuditService.AddedLog(EmployeeSubsequentAdvance);
                            _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);
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
        public string InsertEmployeeTotalAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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
                // INSERT INTO AdvanceWriteOff
                if (voucherDetailVMList != null)
                {
                    voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                }
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOff.VoucherId = voucher.Id;

                var currentAdvanceWriteOffDetailId = 0;
                var currentInvoiceDetail = 0;
                var currentVoucherDetailId = 0;

                // Advance
                //var advance = _advanceService.Find(voucherVM.AdvanceId);
                //advance.WrittenOffAmount += advanceWriteOff.Amount;
                //advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                //advance.UpdatedBy = advanceWriteOff.AddedBy;
                //advance.UpdatedDate = advanceWriteOff.AddedDate;
                //advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                //_advanceService.Update(advance);

                //var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                //if (null == advanceDetail)
                //    throw new CustomException("Advance detail not found!");

                currentAdvanceWriteOffDetailId++;
                var advanceWriteOffDetail = new AdvanceWriteOffDetail
                {
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    AdvanceId = voucherVM.AdvanceId,
                    AdvanceDetailId = voucherVM.AdvanceDetailId,
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                    CurrencyId = advanceWriteOff.CurrencyId,
                    PartyType = voucherVM.PartyType,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    Amount = voucherVM.Amount,
                    EmployeeId = voucherVM.EmployeeId
                };
                InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                advanceWriteOff.Amount += voucherVM.Amount;
                //advanceDetail.WrittenOffAmount += voucherVM.Amount;

                //if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                //    throw new CustomException($"{advanceWriteOff.SettlementType} amount cannot exceed the balance advance amount.");

                //advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                //advanceDetail.UpdatedBy = advance.AddedBy;
                //advanceDetail.UpdatedDate = advance.AddedDate;
                //advanceDetail.UpdatedFromIP = advance.AddedFromIP;
                //_advanceService.UpdateAdvanceDetail(advanceDetail);

                // INSERT INTO VoucherDetail Debit or Credit
                var voucherDetail = new VoucherDetail
                {
                    GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                    ActivityId = advanceWriteOffDetail.ActivityId,
                    EmployeeId = advanceWriteOff.EmployeeId,
                    CurrencyId = voucher.CurrencyId,
                    EntityId = voucher.EntityId,
                    AdvanceWriteOffDetailId = advanceWriteOffDetail.Id
                };

                if (advanceWriteOff.SettlementType == SettlementType.SetOff.ToString())
                {
                    voucherDetail.CrAmount = advanceWriteOffDetail.Amount;
                    voucherDetail.DrAmount = 0;
                }
                else if (advanceWriteOff.SettlementType == SettlementType.Return.ToString())
                {
                    voucherDetail.DrAmount = 0;
                    voucherDetail.CrAmount = advanceWriteOffDetail.Amount;
                }
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetail.DrAmount,
                    CrAmount = voucherDetail.CrAmount
                });

                if (null != voucherDetailVMList && voucherDetailVMList.Count() > 0 && advanceWriteOff.SettlementType == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO EmployeePayableWriteOff
                    var employeePayableWriteOff = new EmployeePayableWriteOff
                    {
                        CompanyGroupId = advanceWriteOff.CompanyGroupId,
                        CompanyId = advanceWriteOff.CompanyId,
                        PlantId = advanceWriteOff.PlantId,
                        EmployeeId = voucherVM.EmployeeId,
                        FiscalYearId = advanceWriteOff.FiscalYearId,
                        FiscalYearPeriodId = advanceWriteOff.FiscalYearPeriodId,
                        TaxYearId = advanceWriteOff.TaxYearId,
                        TaxYearPeriodId = advanceWriteOff.TaxYearPeriodId,
                        VoucherTypeId = advanceWriteOff.VoucherTypeId,
                        CurrencyId = advanceWriteOff.CurrencyId,
                        SourceType = advanceWriteOff.SourceType,
                        PartyType = advanceWriteOff.PartyType,
                        VoucherDate = advanceWriteOff.VoucherDate,
                        PostingDate = advanceWriteOff.PostingDate,
                        DocDate = advanceWriteOff.DocDate,
                        DocRefNo = advanceWriteOff.DocRefNo,
                        Narration = advanceWriteOff.Narration,
                        Amount = voucherVM.Amount,
                        AddedBy = advanceWriteOff.AddedBy,
                        AddedDate = advanceWriteOff.AddedDate,
                        AddedFromIP = advanceWriteOff.AddedFromIP,
                        RowState = RowState.Parked.ToString()
                    };
                    _employeePayableWriteOffService.InsertEmployeePayableWriteOff(employeePayableWriteOff);
                    employeePayableWriteOff.VoucherId = voucher.Id;

                    // EmployeePayable
                    var employeePayableIds = voucherDetailVMList.Select(r => r.EmployeePayableId);
                    var employeePayableDbList = _employeePayableService.GetEmployeePayableList(r => employeePayableIds.Contains(r.Id)).Select().ToList();
                    var employeePayableDetailIds = voucherDetailVMList.Select(r => r.EmployeePayableDetailId);
                    var employeePayableDetailDbList = _employeePayableService.GetEmployeePayableDetailList(r => employeePayableDetailIds.Contains(r.Id)).Select().ToList();

                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        var employeePayableDetail = employeePayableDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.EmployeePayableDetailId);
                        if (null == employeePayableDetail)
                            throw new CustomException("Invoice not found!");
                        employeePayableDetail.WrittenOffAmount += voucherDetailVM.Amount;

                        if (employeePayableDetail.NetAmount < employeePayableDetail.WrittenOffAmount)
                            throw new CustomException("Set-Off amount cannot exceed the balance Payable amount.");

                        employeePayableDetail.IsWrittenOff = employeePayableDetail.NetAmount == employeePayableDetail.WrittenOffAmount;
                        employeePayableDetail.UpdatedBy = employeePayableWriteOff.AddedBy;
                        employeePayableDetail.UpdatedDate = employeePayableWriteOff.AddedDate;
                        employeePayableDetail.UpdatedFromIP = employeePayableWriteOff.AddedFromIP;
                        _employeePayableService.UpdateEmployeePayableDetail(employeePayableDetail);

                        // TODO: have a gap here if invoice split
                        var invoice = employeePayableDbList.First(r => r.Id == employeePayableDetail.EmployeePayableId);
                        invoice.WrittenOffAmount = employeePayableDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = employeePayableWriteOff.AddedBy;
                        invoice.UpdatedDate = employeePayableWriteOff.AddedDate;
                        invoice.UpdatedFromIP = employeePayableWriteOff.AddedFromIP;
                        _employeePayableService.UpdateEmployeePayable(invoice);

                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceWriteOffDetail
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
                            AddedBy = employeePayableWriteOff.AddedBy,
                            AddedDate = employeePayableWriteOff.AddedDate,
                            AddedFromIP = employeePayableWriteOff.AddedFromIP,
                            Archive = employeePayableWriteOff.Archive,
                            ModelState = employeePayableWriteOff.ModelState,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration
                        };
                        _employeePayableWriteOffService.InsertEmployeePayableWriteOffDetail(employeePayableWriteOff, employeePayableWriteOffDetail, currentInvoiceDetail);

                        // in liability side Cr.
                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            EntityId = voucherDetailVM.EntityId,
                            DrAmount = voucherDetailVM.Amount,
                            Narration = employeePayableWriteOff.Narration,
                            EmployeePayableWriteOffDetailId = employeePayableWriteOffDetail.Id,
                            EmployeeId = employeePayableWriteOff.EmployeeId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailDr.DrAmount
                        });

                        var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            AdvanceId = null,
                            EmployeeId = employeePayableWriteOff.EmployeeId,
                            AdvanceWriteOffId = advanceWriteOff.Id,
                            EmployeePayableWriteOffId = employeePayableWriteOff.Id,
                            EmployeePayableId = null,
                            PartyType = employeePayableWriteOff.PartyType,
                            CurrencyId = employeePayableWriteOff.CurrencyId,
                            Amount = voucherDetailDr.DrAmount,
                            VoucherDate = voucherVM.VoucherDate,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            JournalType = "General",
                            TransactionType = EmployeeSubsequentTranEnum.Payment.ToString(),
                            Narration = voucherVM.Narration,
                            SourceType = employeePayableWriteOff.SourceType,
                            IsPark = voucherVM.IsPark,
                            Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                            VoucherId = voucher.Id,
                            VoucherDetailId = voucherDetailDr.Id,
                            PaymentSource = voucherVM.PaymentSource,
                        };
                        AuditService.AddedLog(EmployeeSubsequentAdvance);
                        _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);

                    }
                }
                else if (advanceWriteOff.SettlementType == SettlementType.Return.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                        throw new CustomException("Bank Id not found!");
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id not found!");

                    // INSERT INTO VoucherDetail
                    var voucherDetailDr = new VoucherDetail
                    {
                        CurrencyId = voucher.CurrencyId,
                        DrAmount = voucherVM.Amount,
                        PartyType = voucherVM.PaymentSource,
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };

                    // INSRT INTO GLTransactionDetail
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherDetailDr.BankMasterId);
                        voucherDetailDr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailDr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailDr.ActivityId = bankMaster["ActivityId"].ToString();
                        voucherDetailDr.PartyType = PartyType.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        var cashMaster = _accountsCommonService.GetCashMaster(voucherDetailDr.CashMasterId);
                        voucherDetailDr.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailDr.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailDr.ActivityId = cashMaster["ActivityId"].ToString();
                        voucherDetailDr.PartyType = PartyType.Cash.ToString();
                    }

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                    _voucherService.InsertGLTransactionDetail(voucherDetailDr, new GLTransactionDetail
                    {
                        SourceType = voucherDetailDr.PaymentSource,
                        BankMasterId = voucherDetailDr.BankMasterId,
                        CashMasterId = voucherDetailDr.CashMasterId,
                        DrAmount = voucherDetailDr.DrAmount
                    });

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = companyCurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailDr.DrAmount,
                    });

                    var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        AdvanceId = null,
                        EmployeeId = voucherVM.EmployeeId,
                        AdvanceWriteOffId = advanceWriteOff.Id,
                        EmployeePayableWriteOffId = null,
                        EmployeePayableId = null,
                        PartyType = voucherVM.PartyType,
                        CurrencyId = voucherVM.CurrencyId,
                        Amount = voucherVM.Amount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        JournalType = "General",
                        TransactionType = EmployeeSubsequentTranEnum.Payment.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = voucherVM.SourceType,
                        IsPark = voucherVM.IsPark,
                        Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                        VoucherId = voucher.Id,
                        VoucherDetailId = voucherDetailDr.Id,
                        PaymentSource = voucherVM.PaymentSource,
                    };
                    AuditService.AddedLog(EmployeeSubsequentAdvance);
                    _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);
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

        public string UpdateEmployeeAdvanceWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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
                // INSERT INTO AdvanceWriteOff
                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                var advanceWriteOff = InsertAdvanceWriteOff(voucherVM);

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOff.VoucherId = voucher.Id;

                var currentAdvanceWriteOffDetailId = 0;
                var currentInvoiceDetail = 0;
                var currentVoucherDetailId = 0;

                // Advance
                var advance = _advanceService.Find(voucherVM.AdvanceId);
                advance.WrittenOffAmount += advanceWriteOff.Amount;
                advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                advance.UpdatedBy = advanceWriteOff.AddedBy;
                advance.UpdatedDate = advanceWriteOff.AddedDate;
                advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                _advanceService.Update(advance);

                var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.AdvanceDetailId);
                if (null == advanceDetail)
                    throw new CustomException("Advance detail not found!");

                currentAdvanceWriteOffDetailId++;
                var advanceWriteOffDetail = new AdvanceWriteOffDetail
                {
                    CompanyId = advanceDetail.CompanyId,
                    PlantId = advanceDetail.PlantId,
                    AdvanceId = voucherVM.AdvanceId,
                    AdvanceDetailId = voucherVM.AdvanceDetailId,
                    GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceDetail.BudgetMasterId,
                    ActivityId = advanceDetail.ActivityId,
                    CurrencyId = advanceWriteOff.CurrencyId,
                    PartyType = advanceDetail.PartyType,
                    PartyId = advanceDetail.PartyId,
                    PartyPlantId = advanceDetail.PartyPlantId,
                    Amount = voucherVM.Amount
                };
                InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                advanceWriteOff.Amount += voucherVM.Amount;
                advanceDetail.WrittenOffAmount += voucherVM.Amount;

                if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                    throw new CustomException("Received Amount can not cross Balance Amount");

                advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                advanceDetail.UpdatedBy = advance.AddedBy;
                advanceDetail.UpdatedDate = advance.AddedDate;
                advanceDetail.UpdatedFromIP = advance.AddedFromIP;
                _advanceService.UpdateAdvanceDetail(advanceDetail);

                // INSERT INTO VoucherDetail Debit or Credit
                var voucherDetail = new VoucherDetail
                {
                    GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                    ActivityId = advanceWriteOffDetail.ActivityId,
                    EmployeeId = advanceWriteOff.EmployeeId,
                    CurrencyId = voucher.CurrencyId,
                    EntityId = voucher.EntityId,
                    AdvanceWriteOffDetailId = advanceWriteOffDetail.Id
                };

                if (advanceWriteOff.SettlementType == SettlementType.SetOff.ToString())
                {
                    voucherDetail.CrAmount = advanceWriteOffDetail.Amount;
                    voucherDetail.DrAmount = 0;
                }
                else if (advanceWriteOff.SettlementType == SettlementType.Return.ToString())
                {
                    voucherDetail.CrAmount = advanceWriteOffDetail.Amount;
                    voucherDetail.DrAmount = 0;
                }
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);

                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetail.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetail.DrAmount,
                    CrAmount = voucherDetail.CrAmount
                });

                if (null != voucherDetailVMList && voucherDetailVMList.Count() > 0 && advanceWriteOff.SettlementType == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO EmployeePayableWriteOff
                    var employeePayableWriteOff = new EmployeePayableWriteOff
                    {
                        CompanyGroupId = advanceWriteOff.CompanyGroupId,
                        CompanyId = advanceWriteOff.CompanyId,
                        PlantId = advanceWriteOff.PlantId,
                        EmployeeId = voucherVM.EmployeeId,
                        FiscalYearId = advanceWriteOff.FiscalYearId,
                        FiscalYearPeriodId = advanceWriteOff.FiscalYearPeriodId,
                        TaxYearId = advanceWriteOff.TaxYearId,
                        TaxYearPeriodId = advanceWriteOff.TaxYearPeriodId,
                        VoucherTypeId = advanceWriteOff.VoucherTypeId,
                        CurrencyId = advanceWriteOff.CurrencyId,
                        SourceType = advanceWriteOff.SourceType,
                        PartyType = advanceWriteOff.PartyType,
                        VoucherDate = advanceWriteOff.VoucherDate,
                        PostingDate = advanceWriteOff.PostingDate,
                        DocDate = advanceWriteOff.DocDate,
                        DocRefNo = advanceWriteOff.DocRefNo,
                        Narration = advanceWriteOff.Narration,
                        Amount = voucherVM.Amount
                    };
                    _employeePayableWriteOffService.InsertEmployeePayableWriteOff(employeePayableWriteOff);
                    employeePayableWriteOff.VoucherId = voucher.Id;

                    // EmployeePayable
                    var employeePayableIds = voucherDetailVMList.Select(r => r.EmployeePayableId);
                    var employeePayableDbList = _employeePayableService.GetEmployeePayableList(r => employeePayableIds.Contains(r.Id)).Select().ToList();
                    var employeePayableDetailIds = voucherDetailVMList.Select(r => r.EmployeePayableDetailId);
                    var employeePayableDetailDbList = _employeePayableService.GetEmployeePayableDetailList(r => employeePayableDetailIds.Contains(r.Id)).Select().ToList();

                    foreach (var voucherDetailVM in voucherDetailVMList)
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
                        var invoice = employeePayableDbList.First(r => r.Id == employeePayableDetail.EmployeePayableId);
                        invoice.WrittenOffAmount = employeePayableDetail.WrittenOffAmount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = employeePayableWriteOff.AddedBy;
                        invoice.UpdatedDate = employeePayableWriteOff.AddedDate;
                        invoice.UpdatedFromIP = employeePayableWriteOff.AddedFromIP;
                        _employeePayableService.UpdateEmployeePayable(invoice);

                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceWriteOffDetail
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
                            AddedBy = employeePayableWriteOff.AddedBy,
                            AddedDate = employeePayableWriteOff.AddedDate,
                            AddedFromIP = employeePayableWriteOff.AddedFromIP,
                            Archive = employeePayableWriteOff.Archive,
                            ModelState = employeePayableWriteOff.ModelState,
                            DocDate = voucherDetailVM.DocDate,
                            DocRefNo = voucherDetailVM.DocRefNo,
                            Narration = voucherDetailVM.Narration
                        };
                        _employeePayableWriteOffService.InsertEmployeePayableWriteOffDetail(employeePayableWriteOff, employeePayableWriteOffDetail, currentInvoiceDetail);

                        // in liability side Cr.
                        var voucherDetailDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            EntityId = voucherDetailVM.EntityId,
                            DrAmount = voucherDetailVM.Amount,
                            Narration = employeePayableWriteOff.Narration,
                            EmployeePayableWriteOffDetailId = employeePayableWriteOffDetail.Id,
                            EmployeeId = employeePayableWriteOff.EmployeeId
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailDr.DrAmount
                        });
                    }
                }
                else if (advanceWriteOff.SettlementType == SettlementType.Return.ToString())
                {
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && string.IsNullOrEmpty(voucherVM.BankMasterId))
                        throw new CustomException("Bank Id not found!");
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && !string.IsNullOrEmpty(voucherVM.CashMasterId))
                        throw new CustomException("Cash Id not found!");

                    // INSERT INTO VoucherDetail
                    var voucherDetailCr = new VoucherDetail
                    {
                        CurrencyId = voucher.CurrencyId,
                        DrAmount = voucherVM.Amount,
                        PartyType = voucherVM.PaymentSource,
                        BankMasterId = voucherVM.BankMasterId,
                        CashMasterId = voucherVM.CashMasterId,
                        PaymentSource = voucherVM.PaymentSource
                    };

                    // INSRT INTO GLTransactionDetail
                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString())
                    {
                        var bankMaster = _accountsCommonService.GetBankMaster(voucherDetailCr.BankMasterId);
                        voucherDetailCr.GLGeneralInfoId = bankMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = bankMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = bankMaster["ActivityId"].ToString();
                        voucherDetailCr.PartyType = PartyType.Bank.ToString();
                    }
                    else if (voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        var cashMaster = _accountsCommonService.GetCashMaster(voucherDetailCr.CashMasterId);
                        voucherDetailCr.GLGeneralInfoId = cashMaster["GLGeneralInfoId"].ToString();
                        voucherDetailCr.BudgetMasterId = cashMaster["BudgetMasterId"].ToString();
                        voucherDetailCr.ActivityId = cashMaster["ActivityId"].ToString();
                        voucherDetailCr.PartyType = PartyType.Cash.ToString();
                    }

                    currentVoucherDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);

                    _voucherService.InsertGLTransactionDetail(voucherDetailCr, new GLTransactionDetail
                    {
                        SourceType = voucherDetailCr.PaymentSource,
                        BankMasterId = voucherDetailCr.BankMasterId,
                        CashMasterId = voucherDetailCr.CashMasterId,
                        DrAmount = voucherDetailCr.DrAmount
                    });

                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = companyCurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        DrAmount = voucherDetailCr.DrAmount,
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

        public List<Dictionary<string, object>> GetEmployeeAdvanceDetail(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var sql = @"  SELECT EWD.GLGeneralInfoId AS GLGeneralInfoId,AW.EmployeeId,  EWD.BudgetMasterId,  EWD.ActivityId,
                                V.VoucherNo, AW.DocDate, AW.DocRefNo, AW.Narration,  AW.VoucherId,EWD.EmployeePayableDetailId, EWD.EmployeePayableWriteOffId,EWD.Id AS EmployeePayableWriteOffDetailId,
                                VD.Id AS VoucherDetailId, AW.CurrencyId, C.Code AS CurrencyCode, VD.EntityId,VD.PlantId,
                                EPD.Amount AS Receivable,EPD.WrittenOffAmount - EWD.Amount AS Received, EWD.Amount + EPD.WrittenOffAmount AS Balance,EWD.Amount,
								CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion
                                FROM [TRN].[VoucherDetail] AS VD
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
								LEFT JOIN [TRN].[EmployeePayableWriteOffDetail] AS EWD ON EWD.Id=VD.AdvanceWriteOffDetailId
								LEFT JOIN [TRN].[EmployeePayableWriteOff] AS AW ON AW.Id=EWD.EmployeePayableWriteOffId
                                LEFT JOIN [TRN].[EmployeePayableDetail] AS EPD ON EPD.Id=EWD.EmployeePayableDetailId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
								LEFT JOIN (
								SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								FROM [TRN].[VoucherDetailCurrency] AS VDC
								JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.VoucherId='" + voucherId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public void Post(string advanceWriteOffId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var advanceWriteOff = Find(advanceWriteOffId);
                CheckIsPosted(advanceWriteOff);
                advanceWriteOff.IsPark = false;
                base.UpdateGraph(advanceWriteOff);
                _voucherService.PostVoucher(advanceWriteOff.VoucherId);
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

        private static void CheckIsPosted(AdvanceWriteOff advanceWriteOff)
        {
            if (!advanceWriteOff.IsPark)
                throw new CustomException(ServiceResources.UpdateOrDeleteNotAllow);
        }

        public Dictionary<string, object> GetAdvanceWriteOffReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var sql = @"SELECT E.UserName AS EntityName, FY.FiscalYearName, FY.YearPrefix, FYP.PeriodName, FYP.PeriodNo, VT.UserName AS VoucherTypeName, V.CurrencyId, C.Code AS CurrencyCode, V.VoucherNo
                        , REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo
                        , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, UPPER(V.Narration) AS Narration, V.IsPark, V.AddedBy, V.PostedBy, AWO.PartyType, P.Code AS PartyCode
                        , P.UserName AS PartyName, PP.UserName AS PartyPlantName, EI.EmployeeCode, EI.EmployeeName
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [ORG].[Entity] AS E ON E.Id=V.EntityId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                        LEFT JOIN [TRN].[AdvanceWriteOff] AS AWO ON AWO.VoucherId=V.Id
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=AWO.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AWO.PartyPlantId
                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AWO.EmployeeId
                        WHERE V.Archive=0 AND V.Id='" + voucherId + "' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(sql);
        }

        public List<Dictionary<string, object>> GetAdvanceWriteOffReportData(string companyId, string voucherId)
        {
            var sql = @"SELECT GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, B.Code AS BudgetCode, B.UserName AS BudgetName, A.Code AS ActivityCode,AD.DocRefNo Particulars
                    , ActivityName=case when vd.BankMasterId<>'' then BNKM.AccountTitle else A.UserName End , VD.BankMasterId, BNKM.AccountNumber, BNKM.AccountTitle, VD.DrAmount, VD.CrAmount, CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount
                        FROM [TRN].[VoucherDetail] AS VD
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
						LEFT JOIN [MST].[BankMaster] AS BNKM ON BNKM.Id=VD.BankMasterId
                        LEFT JOIN TRN.AdvanceWriteOffDetail AWD ON AWD.Id=VD.AdvanceWriteOffDetailId
						LEFT JOIN TRN.Advance AD ON AD.Id=AWD.AdvanceId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE VD.VoucherId='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public string InsertInvoiceChargeWriteOff(VoucherViewModel voucherVM)
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

                // INSERT INTO InvoiceWriteOff
                var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                var totalAmountCr = 0.00M;
                var totalAmountDr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                invoiceWriteOff.VoucherId = voucher.Id;

                // INSERT INTO VoucherDetail Debit
                var currentVoucherDetailId = 0;

                // Invoice
                var currentInvoiceDetail = 0;
                var invoiceDetail = _invoiceService.FindInvoiceDetail(voucherVM.InvoiceDetailId);
                if (null == invoiceDetail)
                    throw new CustomException("Invoice not found!");

                invoiceDetail.WrittenOffAmount += invoiceWriteOff.Amount;
                if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");

                invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                var invoice = _invoiceService.Find(voucherVM.InvoiceId);
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
                    InvoiceId = voucherVM.InvoiceId,
                    InvoiceDetailId = voucherVM.InvoiceDetailId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType,
                    Amount = voucherVM.Amount,
                    AddedBy = invoiceWriteOff.AddedBy,
                    AddedDate = invoiceWriteOff.AddedDate,
                    AddedFromIP = invoiceWriteOff.AddedFromIP,
                    Archive = invoiceWriteOff.Archive,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration
                };
                _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceDetail);
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
                    CrAmount = invoiceWriteOffDetail.Amount,
                    DocDate = invoiceWriteOffDetail.DocDate,
                    DocRefNo = invoiceWriteOffDetail.DocRefNo,
                    Narration = invoiceWriteOffDetail.Narration,
                    PartyType = invoiceWriteOff.PartyType,
                    PartyId = invoiceWriteOffDetail.PartyId,
                    PartyPlantId = invoiceWriteOffDetail.PartyPlantId
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
                    CrAmount = voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate,
                });

                if (voucherVM.SettlementType == SettlementType.Charge.ToString())
                {
                    var currentBankChargeDetailId = 1;
                    var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                    {
                        FinancingTypeId = voucherVM.FinancingTypeId,
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        BankMasterId = invoiceWriteOff.BankMasterId,
                        CashMasterId = invoiceWriteOff.CashMasterId,
                        Archive = invoiceWriteOff.Archive,
                        SourceType = invoiceWriteOff.SourceType,
                        Amount = invoiceWriteOff.Amount,
                        Narration = invoiceWriteOff.Narration,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP
                    }, currentBankChargeDetailId);

                    // Get Expense GL
                    var expenseGL = _bankChargeService.GetExpensesGL(invoiceWriteOff.CompanyId, bankChargeDetail.FinancingTypeId);

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
                        DrAmount = voucherDetailChargeDr.DrAmount * voucherVM.CompanyCurrencyRate
                    });
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


        public string InsertVendorChargeWriteOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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

                // INSERT INTO InvoiceWriteOff
                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
                var invoiceWriteOff = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                var totalAmountCr = 0.00M;
                var totalAmountDr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                invoiceWriteOff.VoucherId = voucher.Id;

                // INSERT INTO VoucherDetail Debit
                var currentVoucherDetailId = 0;

                // Invoice
                var currentInvoiceDetail = 0;
                var invoiceDetail = _invoiceService.FindInvoiceDetail(voucherVM.InvoiceDetailId);
                if (null == invoiceDetail)
                    throw new CustomException("Invoice not found!");

                invoiceDetail.WrittenOffAmount += invoiceWriteOff.Amount;
                if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");

                invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                invoiceDetail.UpdatedBy = invoiceWriteOff.AddedBy;
                invoiceDetail.UpdatedDate = invoiceWriteOff.AddedDate;
                invoiceDetail.UpdatedFromIP = invoiceWriteOff.AddedFromIP;
                _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                var invoice = _invoiceService.Find(voucherVM.InvoiceId);
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
                    InvoiceId = voucherVM.InvoiceId,
                    InvoiceDetailId = voucherVM.InvoiceDetailId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = voucherVM.PartyType,
                    Amount = voucherVM.Amount,
                    AddedBy = invoiceWriteOff.AddedBy,
                    AddedDate = invoiceWriteOff.AddedDate,
                    AddedFromIP = invoiceWriteOff.AddedFromIP,
                    Archive = invoiceWriteOff.Archive,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration
                };
                _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceDetail);
                invoiceWriteOff.Amount = invoiceWriteOffDetail.Amount;

                // INSERT INTO VoucherDetail
                var voucherDetailDr = new VoucherDetail
                {
                    VoucherId = voucher.Id,
                    InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id,
                    GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                    BudgetMasterId = invoiceDetail.BudgetMasterId,
                    ActivityId = invoiceDetail.ActivityId,
                    CurrencyId = voucher.CurrencyId,
                    DrAmount = invoiceWriteOffDetail.Amount,
                    DocDate = invoiceWriteOffDetail.DocDate,
                    DocRefNo = invoiceWriteOffDetail.DocRefNo,
                    Narration = invoiceWriteOffDetail.Narration,
                    PartyType = invoiceWriteOff.PartyType,
                    PartyId = invoiceWriteOffDetail.PartyId,
                    PartyPlantId = invoiceWriteOffDetail.PartyPlantId
                };
                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                totalAmountDr += voucherDetailDr.DrAmount;
                totalAmountCr += voucherDetailDr.CrAmount;

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                });

                if (voucherVM.SettlementType == SettlementType.Charge.ToString())
                {
                    var currentBankChargeDetailId = 1;
                    var bankChargeDetail = _bankChargeService.InsertBankCharge(new BankCharge
                    {
                        FinancingTypeId = voucherVM.FinancingTypeId,
                        InvoiceWriteOffId = invoiceWriteOff.Id,
                        BankMasterId = invoiceWriteOff.BankMasterId,
                        CashMasterId = invoiceWriteOff.CashMasterId,
                        Archive = invoiceWriteOff.Archive,
                        SourceType = invoiceWriteOff.SourceType,
                        Amount = invoiceWriteOff.Amount,
                        Narration = invoiceWriteOff.Narration,
                        AddedBy = invoiceWriteOff.AddedBy,
                        AddedDate = invoiceWriteOff.AddedDate,
                        AddedFromIP = invoiceWriteOff.AddedFromIP
                    }, currentBankChargeDetailId);

                    foreach (var item in voucherDetailVMList)
                    {
                        // Insert Bank charges Debit
                        currentVoucherDetailId++;
                        var voucherDetailChargeCr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            BankChargeId = bankChargeDetail.Id,
                            CrAmount = item.Amount,
                            Narration = bankChargeDetail.Narration,
                            GLGeneralInfoId = item.GLGeneralInfoId,
                            BudgetMasterId = item.BudgetMasterId,
                            ActivityId = item.ActivityId
                        }, currentVoucherDetailId);

                        totalAmountDr += voucherDetailChargeCr.DrAmount;
                        totalAmountCr += voucherDetailChargeCr.CrAmount;

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailChargeCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailChargeCr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailChargeCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherDetailChargeCr.CrAmount * voucherVM.CompanyCurrencyRate
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

        private static void CheckIsPosted(InvoiceWriteOff invoicewriteOff)
        {
            if (!invoicewriteOff.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }
        public void PostVendorInvoiceCharge(string invoicewriteOffId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var invoicewriteOff = _invoiceWriteOffService.Find(invoicewriteOffId);
                CheckIsPosted(invoicewriteOff);
                invoicewriteOff.IsPark = false;
              _invoiceWriteOffService.UpdateGraph(invoicewriteOff);
                _voucherService.PostVoucher(invoicewriteOff.VoucherId);
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

        public void DeleteVendorInvoiceCharge(string invoiceWriteOffId, string voucherId)
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
                var invoicewriteOff = _invoiceWriteOffService.Find(invoiceWriteOffId);
                var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWriteOffId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var bangcHarges = _bankChargeService.QueryByInvoiceWriteOff(invoiceWriteOffId).Select().FirstOrDefault();

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
                    
                        var rdBuildervd = new System.Text.StringBuilder();
                        var buildervdSql = @"UPDATE [TRN].VoucherDetail SET BankChargeId=NULL WHERE Id='" + item.Id + "'";
                        rdBuildervd.Append(buildervdSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuildervd.ToString());
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
                _bankChargeService.BankChargeDelete(bangcHarges.Id);
                _invoiceWriteOffService.Delete(invoiceWriteOffId);
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


        public string InsertEmployeeSalaryPayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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

                voucherVM.Amount = voucherDetailVMList.Where(r => r.PartyType == "Employee" && r.TrnType == "Dr").Sum(r => r.DrAmount) == 0
                        ? voucherDetailVMList.Where(r => r.PartyType == "Employee" && r.TrnType == "Cr").Sum(r => r.CrAmount) : voucherDetailVMList.Where(r => r.PartyType == "Employee" && r.TrnType == "Dr").Sum(r => r.DrAmount);
                ;

                voucherVM.BaseNoOfDays = 0;
                var employeePayable = _employeePayableService.InsertEmployeePayable(voucherVM);


                string advanceId = null;
                var advanceWriteOff = new AdvanceWriteOff
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
                };

                var empAdvance = voucherDetailVMList.Where(r => r.PartyType == "Advance").FirstOrDefault();

                if (empAdvance != null)
                {
                    advanceWriteOff.Id = _pKGeneratorService.GetAutoNumber(nameof(AdvanceWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
                    advanceWriteOff.Amount = voucherDetailVMList.Where(r => r.PartyType == "Advance" && r.TrnType=="Dr").Sum(r => r.DrAmount)==0
                        ? voucherDetailVMList.Where(r => r.PartyType == "Advance" && r.TrnType == "Cr").Sum(r => r.CrAmount) : voucherDetailVMList.Where(r => r.PartyType == "Advance" && r.TrnType == "Dr").Sum(r => r.DrAmount);

                    AuditService.AddedLog(advanceWriteOff);
                    _advanceWriteOffRepository.Insert(advanceWriteOff);
                    advanceId = advanceWriteOff.Id;
                }

                var voucher = _voucherService.InsertVoucher(voucherVM);
                employeePayable.VoucherId = voucher.Id;
                advanceWriteOff.VoucherId = voucher.Id;
                var currentVoucherDetailId = 0;

                var employeePayableDetailId = 0;
                var currentTaxRecord = 0;
                var currentAdvanceWriteOffDetailId = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (voucherDetailVM.PartyType == PartyType.GL.ToString())
                    {
                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.DrAmount,
                            CrAmount = voucherDetailVM.CrAmount,
                            PartyType = voucherDetailVM.PartyType
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
                            CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount
                        });
                    }
                    if (voucherDetailVM.PartyType == PartyType.Employee.ToString())
                    {
                        employeePayableDetailId++;
                        var employeePayableDetail = _employeePayableService.InsertEmployeePayableDetail(employeePayable, new EmployeePayableDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = voucherDetailVM.CrAmount,
                            NetAmount = voucherDetailVM.CrAmount
                        }, employeePayableDetailId);

                        var voucherDetailCr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            EmployeePayableDetailId = employeePayableDetail.Id,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            EmployeeId = employeePayable.EmployeeId,
                            PartyType = voucherDetailVM.PartyType,
                            CrAmount = voucherDetailVM.CrAmount
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
                    }

                    if (voucherDetailVM.PartyType == "Advance")
                    {
                        var advance = _advanceService.Find(voucherDetailVM.AdvanceId);
                        advance.WrittenOffAmount += voucherDetailVM.CrAmount==0? voucherDetailVM.DrAmount: voucherDetailVM.CrAmount;
                        advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                        advance.UpdatedBy = voucher.AddedBy;
                        advance.UpdatedDate = voucher.AddedDate;
                        advance.UpdatedFromIP = voucher.AddedFromIP;
                        _advanceService.Update(advance);
                        var advanceDetail = _advanceService.FindAdvanceDetail(voucherDetailVM.AdvanceDetailId);
                        if (null == advanceDetail)
                            throw new CustomException("Advance detail not found!");


                        currentAdvanceWriteOffDetailId++;
                        var advanceWriteOffDetail = new AdvanceWriteOffDetail
                        {
                            CompanyId = voucher.CompanyId,
                            PlantId = voucher.PlantId,
                            AdvanceId = voucherDetailVM.AdvanceId,
                            AdvanceDetailId = voucherDetailVM.AdvanceDetailId,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            PartyType = voucherDetailVM.PartyType,
                            PartyId = voucherDetailVM.PartyId,
                            PartyPlantId = voucherDetailVM.PartyPlantId,
                            Amount = voucherDetailVM.CrAmount == 0 ? voucherDetailVM.DrAmount : voucherDetailVM.CrAmount
                        };
                        InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                        advanceWriteOff.Amount += voucherDetailVM.CrAmount == 0 ? voucherDetailVM.DrAmount : voucherDetailVM.CrAmount;
                        advanceDetail.WrittenOffAmount += voucherDetailVM.CrAmount == 0 ? voucherDetailVM.DrAmount : voucherDetailVM.CrAmount;

                        if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                            throw new CustomException($"{advanceWriteOff.SettlementType} amount cannot exceed the balance advance amount.");

                        advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                        advanceDetail.UpdatedBy = advance.AddedBy;
                        advanceDetail.UpdatedDate = advance.AddedDate;
                        advanceDetail.UpdatedFromIP = advance.AddedFromIP;
                        _advanceService.UpdateAdvanceDetail(advanceDetail);

                        // INSERT INTO VoucherDetail Debit or Credit
                        var voucherDetail = new VoucherDetail
                        {
                            GLGeneralInfoId = advanceWriteOffDetail.GLGeneralInfoId,
                            BudgetMasterId = advanceWriteOffDetail.BudgetMasterId,
                            ActivityId = advanceWriteOffDetail.ActivityId,
                            EmployeeId = advanceWriteOff.EmployeeId,
                            CurrencyId = voucher.CurrencyId,
                            EntityId = voucher.EntityId,
                            DrAmount = voucherDetailVM.DrAmount,
                            CrAmount = voucherDetailVM.CrAmount,
                            PartyType = voucherDetailVM.PartyType,
                            AdvanceWriteOffDetailId = advanceWriteOffDetail.Id
                        };
                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetail, currentVoucherDetailId);
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetail.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetail.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetail.DrAmount,
                            CrAmount = voucherDetail.CrAmount
                        });
                    }
                    if (voucherDetailVM.PartyType == "Tax")
                    {
                        currentTaxRecord++;
                        var invoiceTax = new InvoiceTax
                        {
                            Archive = false,
                            Id = MakePK(employeePayable.Id, currentTaxRecord, 2),
                            VoucherId = voucher.Id,
                            EmployeePayableId = employeePayable.Id,
                            TaxYearId = voucher.TaxYearId,
                            TaxYearPeriodId = voucher.TaxYearPeriodId,
                            TaxCategoryId = voucherDetailVM.TaxCategoryId,
                            TaxCodeId = voucherDetailVM.TaxCodeId,
                            TaxAmount = voucherDetailVM.CrAmount,
                            TaxAutoAmount = 0,
                            PartyId = voucherVM.PartyId,
                            SourceType = SourceType.EmployeePayable.ToString(),
                            AddedBy = voucher.AddedBy,
                            AddedDate = voucher.AddedDate,
                            AddedFromIP = voucher.AddedFromIP
                        };
                        _invoiceTaxRepository.Insert(invoiceTax);

                        var invoiceTaxDetail = new InvoiceTaxDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = voucherDetailVM.CrAmount,
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
                            PartyType= voucherDetailVM.PartyType
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

        public void DeleteEmployeeSalaryPayable(string payableId, string voucherId)
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
                var payable = _employeePayableRepository.Find(payableId);
                var invoiceDetail = _employeePayableDetailRepository.Query(r => r.EmployeePayableId == payableId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.EmployeePayableId == payableId).Select().ToList();
                var advancewriteOff = _advanceWriteOffRepository.Query(r=>r.VoucherId==voucherId).Select().ToList(); 
                
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
                    _employeePayableDetailRepository.Delete(item.Id);
                }
                _employeePayableRepository.Delete(payableId);
                if (advancewriteOff != null)
                {
                    foreach (var item in advancewriteOff)
                    {
                        var advancewriteOffDetail = _advanceWriteOffDetailRepository.Query(r => r.AdvanceWriteOffId == item.Id).Select().ToList();
                        foreach (var detail in advancewriteOffDetail)
                        {
                            var advance = _advanceRepository.Find(detail.AdvanceId);
                            var advanceDetail = _advanceDetailRepository.Find(detail.AdvanceDetailId);

                            advanceDetail.WrittenOffAmount -= detail.Amount;
                            advance.WrittenOffAmount -= detail.Amount;
                            advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                            advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;

                            _advanceDetailRepository.Update(advanceDetail);
                            _advanceRepository.Update(advance);
                            _advanceWriteOffDetailRepository.Delete(detail.Id);
                        }
                        _advanceWriteOffRepository.Delete(item);
                    }
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
        private static void CheckIsPosted(EmployeePayable employeePayable)
        {
            if (!employeePayable.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }
        public void PostEmployeeSalaryPayable(string employeePayableId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var employeePayable = _employeePayableRepository.Find(employeePayableId);
                CheckIsPosted(employeePayable);
                employeePayable.IsPark = false;
                _employeePayableRepository.Update(employeePayable);
                _voucherService.PostVoucher(employeePayable.VoucherId);
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

        private string GetInvoiceWriteOffGroupNoPK()
        {
            return base.GetAutoNumber("InvoiceWriteOffGroupNo", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertPartyLiabilityReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> invoiceDetailVMList)
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
                string _invoiceWriteOffGroupNo = GetInvoiceWriteOffGroupNoPK();


                // INSERT INTO AdvanceWriteOff
                voucherVM.Amount = invoiceDetailVMList.Sum(r => r.Amount);
                voucherVM.InvoiceWriteOffGroupNo = _invoiceWriteOffGroupNo;

                var invoiceWriteOffParent =_invoiceWriteOffService.InsertInvoiceWriteOff(voucherVM);
                var totalAmountDr = invoiceDetailVMList.Sum(r => r.Amount);

                // Set total Debit amount in write of master.
                invoiceWriteOffParent.Amount = totalAmountDr;

                var totalAmountCr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                invoiceWriteOffParent.VoucherId = voucher.Id;

                // Advance
                var invoice = _invoiceService.Find(voucherVM.InvoiceId);
                if (null == invoice)
                    throw new CustomException("Advance Id not found!");
                invoice.WrittenOffAmount += totalAmountDr;
                invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                invoice.UpdatedBy = invoiceWriteOffParent.AddedBy;
                invoice.UpdatedDate = invoiceWriteOffParent.AddedDate;
                invoice.UpdatedFromIP = invoiceWriteOffParent.AddedFromIP;
                _invoiceService.Update(invoice);

                var invoiceDetail = _invoiceService.FindInvoiceDetail(voucherVM.InvoiceDetailId);
                if (null == invoiceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                invoiceDetail.WrittenOffAmount += totalAmountDr;
                if (invoiceDetail.Amount < invoiceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");
                invoiceDetail.IsWrittenOff = invoiceDetail.Amount == invoiceDetail.WrittenOffAmount;
                invoiceDetail.UpdatedBy = invoice.UpdatedBy;
                invoiceDetail.UpdatedDate = invoice.UpdatedDate;
                invoiceDetail.UpdatedFromIP = invoice.UpdatedFromIP;
                _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                var invoiceWriteOffDetailParent = new InvoiceWriteOffDetail
                {
                    InvoiceId = invoice.Id,
                    InvoiceDetailId = invoiceDetail.Id,
                    InvoiceWriteOffId = invoiceWriteOffParent.Id,
                    GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                    BudgetMasterId = invoiceDetail.BudgetMasterId,
                    ActivityId = invoiceDetail.ActivityId,
                    CurrencyId = invoice.CurrencyId,
                    PartyType = invoice.PartyType,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    PartyId = invoice.PartyId,
                    PartyPlantId = invoice.PartyPlantId,
                    Amount = invoiceWriteOffParent.Amount,
                    AddedBy = invoiceWriteOffParent.AddedBy,
                    AddedDate = invoiceWriteOffParent.AddedDate,
                    AddedFromIP = invoiceWriteOffParent.AddedFromIP,
                    Archive = invoiceWriteOffParent.Archive,
                    DocDate = invoiceWriteOffParent.DocDate,
                    DocRefNo = invoiceWriteOffParent.DocRefNo,
                    Narration = invoiceWriteOffParent.Narration
                };
                _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOffParent, invoiceWriteOffDetailParent, 1);

                // INSERT INTO VoucherDetail Debit
                var currentVoucherDetailId = 0;
                var voucherDetailDr = new VoucherDetail
                {
                    //AdvanceWriteOffDetailId = invoiceWriteOffDetail.Id,
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                    DrAmount = voucherVM.Amount,
                    PartyType = voucherVM.PartyType,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    InvoiceWriteOffDetailId= invoiceWriteOffDetailParent.Id
                };


                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                    CrAmount = voucherDetailDr.CrAmount * voucherVM.CompanyCurrencyRate
                });


                if (voucherVM.SettlementType == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO InvoiceWriteOff
                    var isInvoice = invoiceDetailVMList.Where(r => r.OtherName == "Invoice").ToList();
                    var isAdvance = invoiceDetailVMList.Where(r => r.OtherName == "Advance").ToList();
                    if(isInvoice.Count > 0)
                    {
                        var invoiceWriteOffChild = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                        invoiceWriteOffChild.PartyType = "Customer";
                        invoiceWriteOffChild.VoucherId = voucher.Id;
                        invoiceWriteOffChild.Amount = invoiceDetailVMList.Where(r => r.OtherName == "Invoice").Sum(r => r.Amount); 

                        // Invoice
                        var invoiceIds = invoiceDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = invoiceDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;
                        foreach (var voucherDetailVM in invoiceDetailVMList.Where(r => r.OtherName == "Invoice"))
                        {
                            var setOffinvoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == setOffinvoiceDetail)
                                throw new CustomException("Invoice not found!");

                            setOffinvoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                            if (setOffinvoiceDetail.NetAmount < setOffinvoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            setOffinvoiceDetail.IsWrittenOff = setOffinvoiceDetail.NetAmount == setOffinvoiceDetail.WrittenOffAmount;
                            setOffinvoiceDetail.UpdatedBy = invoiceWriteOffChild.AddedBy;
                            setOffinvoiceDetail.UpdatedDate = invoiceWriteOffChild.AddedDate;
                            setOffinvoiceDetail.UpdatedFromIP = invoiceWriteOffChild.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(setOffinvoiceDetail);

                            var setoffinvoice = inviceDbList.First(r => r.Id == setOffinvoiceDetail.InvoiceId);
                            setoffinvoice.WrittenOffAmount = setOffinvoiceDetail.WrittenOffAmount;
                            setoffinvoice.IsWrittenOff = setoffinvoice.Amount == setoffinvoice.WrittenOffAmount;
                            setoffinvoice.UpdatedBy = invoiceWriteOffChild.AddedBy;
                            setoffinvoice.UpdatedDate = invoiceWriteOffChild.AddedDate;
                            setoffinvoice.UpdatedFromIP = invoiceWriteOffChild.AddedFromIP;
                            _invoiceService.Update(setoffinvoice);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetailChild = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = setOffinvoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffinvoiceDetail.BudgetMasterId,
                                ActivityId = setOffinvoiceDetail.ActivityId,
                                CurrencyId = setoffinvoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOffChild.Id,
                                InvoiceId = voucherDetailVM.InvoiceId,
                                InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = setoffinvoice.CompanyId,
                                PlantId = setoffinvoice.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.Amount,
                                AddedBy = invoiceWriteOffChild.AddedBy,
                                AddedDate = invoiceWriteOffChild.AddedDate,
                                AddedFromIP = invoiceWriteOffChild.AddedFromIP,
                                Archive = invoiceWriteOffChild.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOffChild, invoiceWriteOffDetailChild, currentInvoiceDetail);
                            //invoiceWriteOffParent.Amount = invoiceWriteOffDetailChild.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetailChild.Id,
                                GLGeneralInfoId = setOffinvoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffinvoiceDetail.BudgetMasterId,
                                ActivityId = setOffinvoiceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = invoiceWriteOffChild.PartyType,
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
                    if (isAdvance.Count > 0)
                    {
                        var advanceWriteOffChild = InsertAdvanceWriteOff(voucherVM);
                        advanceWriteOffChild.PartyType = "Vendor";
                        advanceWriteOffChild.VoucherId = voucher.Id;
                        advanceWriteOffChild.Amount = invoiceDetailVMList.Where(r=>r.OtherName== "Advance").Sum(r=>r.Amount);

                        // Invoice
                        var advanceIds = invoiceDetailVMList.Where(r=>r.OtherName== "Advance").Select(r => r.InvoiceId);//Here InvoiceId is AdvanceId value
                        var advanceDbList = _advanceService.Query(r => advanceIds.Contains(r.Id)).Select().ToList();
                        var advanceDetailIds = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Select(r => r.InvoiceDetailId);
                        var advanceDetailDbList = _advanceService.GetAdvanceDetailList(r => advanceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;
                        foreach (var voucherDetailVM in invoiceDetailVMList.Where(r => r.OtherName == "Advance"))
                        {
                            var setOffadvanceDetail = advanceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == setOffadvanceDetail)
                                throw new CustomException("Invoice not found!");

                            setOffadvanceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                            if (setOffadvanceDetail.NetAmount < setOffadvanceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            setOffadvanceDetail.IsWrittenOff = setOffadvanceDetail.NetAmount == setOffadvanceDetail.WrittenOffAmount;
                            setOffadvanceDetail.UpdatedBy = advanceWriteOffChild.AddedBy;
                            setOffadvanceDetail.UpdatedDate = advanceWriteOffChild.AddedDate;
                            setOffadvanceDetail.UpdatedFromIP = advanceWriteOffChild.AddedFromIP;
                            _advanceService.UpdateAdvanceDetail(setOffadvanceDetail);

                            var setoffadvance = advanceDbList.First(r => r.Id == setOffadvanceDetail.AdvanceId);
                            setoffadvance.WrittenOffAmount = setOffadvanceDetail.WrittenOffAmount;
                            setoffadvance.IsWrittenOff = setoffadvance.Amount == setoffadvance.WrittenOffAmount;
                            setoffadvance.UpdatedBy = advanceWriteOffChild.AddedBy;
                            setoffadvance.UpdatedDate = advanceWriteOffChild.AddedDate;
                            setoffadvance.UpdatedFromIP = advanceWriteOffChild.AddedFromIP;
                            _advanceService.Update(setoffadvance);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var advanceWriteOffDetailChild = new AdvanceWriteOffDetail
                            {
                                GLGeneralInfoId = setOffadvanceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffadvanceDetail.BudgetMasterId,
                                ActivityId = setOffadvanceDetail.ActivityId,
                                CurrencyId = setoffadvance.CurrencyId,
                                AdvanceWriteOffId = advanceWriteOffChild.Id,
                                AdvanceId = voucherDetailVM.InvoiceId,
                                AdvanceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = setoffadvance.CompanyId,
                                PlantId = setoffadvance.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.Amount,
                                AddedBy = advanceWriteOffChild.AddedBy,
                                AddedDate = advanceWriteOffChild.AddedDate,
                                AddedFromIP = advanceWriteOffChild.AddedFromIP,
                                Archive = advanceWriteOffChild.Archive,
                            };
                            InsertAdvanceWriteOffDetail(advanceWriteOffChild, advanceWriteOffDetailChild, currentInvoiceDetail);
                            invoiceWriteOffParent.Amount = advanceWriteOffDetailChild.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                AdvanceWriteOffDetailId = advanceWriteOffDetailChild.Id,
                                GLGeneralInfoId = setOffadvanceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffadvanceDetail.BudgetMasterId,
                                ActivityId = setOffadvanceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advanceWriteOffChild.PartyType,
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

        public string InsertPartyAssetReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> invoiceDetailVMList)
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
                string _invoiceWriteOffGroupNo = GetInvoiceWriteOffGroupNoPK();


                // INSERT INTO AdvanceWriteOff
                voucherVM.Amount = invoiceDetailVMList.Sum(r => r.Amount);
                voucherVM.InvoiceWriteOffGroupNo = _invoiceWriteOffGroupNo;

                var invoiceWriteOffParent = _invoiceWriteOffService.InsertInvoiceWriteOff(voucherVM);
                var totalAmountDr = invoiceDetailVMList.Sum(r => r.Amount);

                // Set total Debit amount in write of master.
                invoiceWriteOffParent.Amount = totalAmountDr;

                var totalAmountCr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                invoiceWriteOffParent.VoucherId = voucher.Id;

                // Advance
                var invoice = _invoiceService.Find(voucherVM.InvoiceId);
                if (null == invoice)
                    throw new CustomException("Advance Id not found!");
                invoice.WrittenOffAmount += totalAmountDr;
                invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                invoice.UpdatedBy = invoiceWriteOffParent.AddedBy;
                invoice.UpdatedDate = invoiceWriteOffParent.AddedDate;
                invoice.UpdatedFromIP = invoiceWriteOffParent.AddedFromIP;
                _invoiceService.Update(invoice);

                var invoiceDetail = _invoiceService.FindInvoiceDetail(voucherVM.InvoiceDetailId);
                if (null == invoiceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                invoiceDetail.WrittenOffAmount += totalAmountDr;
                if (invoiceDetail.Amount < invoiceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");
                invoiceDetail.IsWrittenOff = invoiceDetail.Amount == invoiceDetail.WrittenOffAmount;
                invoiceDetail.UpdatedBy = invoice.UpdatedBy;
                invoiceDetail.UpdatedDate = invoice.UpdatedDate;
                invoiceDetail.UpdatedFromIP = invoice.UpdatedFromIP;
                _invoiceService.UpdateInvoiceDetail(invoiceDetail);

                var invoiceWriteOffDetailParent = new InvoiceWriteOffDetail
                {
                    InvoiceId = invoice.Id,
                    InvoiceDetailId = invoiceDetail.Id,
                    InvoiceWriteOffId = invoiceWriteOffParent.Id,
                    GLGeneralInfoId = invoiceDetail.GLGeneralInfoId,
                    BudgetMasterId = invoiceDetail.BudgetMasterId,
                    ActivityId = invoiceDetail.ActivityId,
                    CurrencyId = invoice.CurrencyId,
                    PartyType = invoice.PartyType,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    PartyId = invoice.PartyId,
                    PartyPlantId = invoice.PartyPlantId,
                    Amount = invoiceWriteOffParent.Amount,
                    AddedBy = invoiceWriteOffParent.AddedBy,
                    AddedDate = invoiceWriteOffParent.AddedDate,
                    AddedFromIP = invoiceWriteOffParent.AddedFromIP,
                    Archive = invoiceWriteOffParent.Archive,
                    DocDate = invoiceWriteOffParent.DocDate,
                    DocRefNo = invoiceWriteOffParent.DocRefNo,
                    Narration = invoiceWriteOffParent.Narration
                };
                _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOffParent, invoiceWriteOffDetailParent, 1);

                // INSERT INTO VoucherDetail Debit
                var currentVoucherDetailId = 0;
                var voucherDetailDr = new VoucherDetail
                {
                    //AdvanceWriteOffDetailId = invoiceWriteOffDetail.Id,
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                    CrAmount = voucherVM.Amount,
                    PartyType = voucherVM.PartyType,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    InvoiceWriteOffDetailId = invoiceWriteOffDetailParent.Id

                };


                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                    CrAmount = voucherDetailDr.CrAmount * voucherVM.CompanyCurrencyRate
                });


                if (voucherVM.SettlementType == SettlementType.SetOff.ToString())
                {
                    var isInvoice = invoiceDetailVMList.Select(r => r.OtherName == "Invoice").ToList();
                    var isAdvance = invoiceDetailVMList.Select(r => r.OtherName == "Advance").ToList();
                    if (isInvoice.Count > 0)
                    {
                        var invoiceWriteOffChild = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                        invoiceWriteOffChild.VoucherId = voucher.Id;
                        invoiceWriteOffChild.Amount = invoiceDetailVMList.Where(r => r.OtherName == "Invoice").Sum(r => r.Amount); ;
                        invoiceWriteOffChild.PartyType = "Vendor";

                        // Invoice
                        var invoiceIds = invoiceDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = invoiceDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;
                        foreach (var voucherDetailVM in invoiceDetailVMList.Where(r=>r.OtherName=="Invoice"))
                        {
                            var setOffinvoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == setOffinvoiceDetail)
                                throw new CustomException("Invoice not found!");

                            setOffinvoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                            if (setOffinvoiceDetail.NetAmount < setOffinvoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            setOffinvoiceDetail.IsWrittenOff = setOffinvoiceDetail.NetAmount == setOffinvoiceDetail.WrittenOffAmount;
                            setOffinvoiceDetail.UpdatedBy = invoiceWriteOffChild.AddedBy;
                            setOffinvoiceDetail.UpdatedDate = invoiceWriteOffChild.AddedDate;
                            setOffinvoiceDetail.UpdatedFromIP = invoiceWriteOffChild.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(setOffinvoiceDetail);

                            var setoffinvoice = inviceDbList.First(r => r.Id == setOffinvoiceDetail.InvoiceId);
                            setoffinvoice.WrittenOffAmount = setOffinvoiceDetail.WrittenOffAmount;
                            setoffinvoice.IsWrittenOff = setoffinvoice.Amount == setoffinvoice.WrittenOffAmount;
                            setoffinvoice.UpdatedBy = invoiceWriteOffChild.AddedBy;
                            setoffinvoice.UpdatedDate = invoiceWriteOffChild.AddedDate;
                            setoffinvoice.UpdatedFromIP = invoiceWriteOffChild.AddedFromIP;
                            _invoiceService.Update(setoffinvoice);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetailChild = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = setOffinvoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffinvoiceDetail.BudgetMasterId,
                                ActivityId = setOffinvoiceDetail.ActivityId,
                                CurrencyId = setoffinvoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOffChild.Id,
                                InvoiceId = voucherDetailVM.InvoiceId,
                                InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = setoffinvoice.CompanyId,
                                PlantId = setoffinvoice.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.Amount,
                                AddedBy = invoiceWriteOffChild.AddedBy,
                                AddedDate = invoiceWriteOffChild.AddedDate,
                                AddedFromIP = invoiceWriteOffChild.AddedFromIP,
                                Archive = invoiceWriteOffChild.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOffChild, invoiceWriteOffDetailChild, currentInvoiceDetail);
                            //invoiceWriteOffParent.Amount = invoiceWriteOffDetailChild.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetailChild.Id,
                                GLGeneralInfoId = setOffinvoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffinvoiceDetail.BudgetMasterId,
                                ActivityId = setOffinvoiceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = invoiceWriteOffChild.PartyType,
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
                    if (isAdvance.Count > 0)
                    {
                        var advanceWriteOffChild = InsertAdvanceWriteOff(voucherVM);
                        advanceWriteOffChild.PartyType = "Customer";
                        advanceWriteOffChild.VoucherId = voucher.Id;
                        advanceWriteOffChild.Amount = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Sum(r => r.Amount);

                        // Invoice
                        var advanceIds = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Select(r => r.InvoiceId);//Here InvoiceId is AdvanceId value
                        var advanceDbList = _advanceService.Query(r => advanceIds.Contains(r.Id)).Select().ToList();
                        var advanceDetailIds = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Select(r => r.InvoiceDetailId);
                        var advanceDetailDbList = _advanceService.GetAdvanceDetailList(r => advanceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;
                        foreach (var voucherDetailVM in invoiceDetailVMList.Where(r => r.OtherName == "Advance"))
                        {
                            var setOffadvanceDetail = advanceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == setOffadvanceDetail)
                                throw new CustomException("Invoice not found!");

                            setOffadvanceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                            if (setOffadvanceDetail.NetAmount < setOffadvanceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            setOffadvanceDetail.IsWrittenOff = setOffadvanceDetail.NetAmount == setOffadvanceDetail.WrittenOffAmount;
                            setOffadvanceDetail.UpdatedBy = advanceWriteOffChild.AddedBy;
                            setOffadvanceDetail.UpdatedDate = advanceWriteOffChild.AddedDate;
                            setOffadvanceDetail.UpdatedFromIP = advanceWriteOffChild.AddedFromIP;
                            _advanceService.UpdateAdvanceDetail(setOffadvanceDetail);

                            var setoffadvance = advanceDbList.First(r => r.Id == setOffadvanceDetail.AdvanceId);
                            setoffadvance.WrittenOffAmount = setOffadvanceDetail.WrittenOffAmount;
                            setoffadvance.IsWrittenOff = setoffadvance.Amount == setoffadvance.WrittenOffAmount;
                            setoffadvance.UpdatedBy = advanceWriteOffChild.AddedBy;
                            setoffadvance.UpdatedDate = advanceWriteOffChild.AddedDate;
                            setoffadvance.UpdatedFromIP = advanceWriteOffChild.AddedFromIP;
                            _advanceService.Update(setoffadvance);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var advanceWriteOffDetailChild = new AdvanceWriteOffDetail
                            {
                                GLGeneralInfoId = setOffadvanceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffadvanceDetail.BudgetMasterId,
                                ActivityId = setOffadvanceDetail.ActivityId,
                                CurrencyId = setoffadvance.CurrencyId,
                                AdvanceWriteOffId = advanceWriteOffChild.Id,
                                AdvanceId = voucherDetailVM.InvoiceId,
                                AdvanceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = setoffadvance.CompanyId,
                                PlantId = setoffadvance.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.Amount,
                                AddedBy = advanceWriteOffChild.AddedBy,
                                AddedDate = advanceWriteOffChild.AddedDate,
                                AddedFromIP = advanceWriteOffChild.AddedFromIP,
                                Archive = advanceWriteOffChild.Archive,
                            };
                            InsertAdvanceWriteOffDetail(advanceWriteOffChild, advanceWriteOffDetailChild, currentInvoiceDetail);
                            //invoiceWriteOffParent.Amount = advanceWriteOffDetailChild.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                AdvanceWriteOffDetailId = advanceWriteOffDetailChild.Id,
                                GLGeneralInfoId = setOffadvanceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffadvanceDetail.BudgetMasterId,
                                ActivityId = setOffadvanceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advanceWriteOffChild.PartyType,
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
        public string InsertPartyLiabilityAdvanceReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> invoiceDetailVMList)
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
                string _invoiceWriteOffGroupNo = GetInvoiceWriteOffGroupNoPK();


                // INSERT INTO AdvanceWriteOff
                voucherVM.Amount = invoiceDetailVMList.Sum(r => r.Amount);
                voucherVM.InvoiceWriteOffGroupNo = _invoiceWriteOffGroupNo;

                var advanceWriteOffParent = InsertAdvanceWriteOff(voucherVM);
                var totalAmountDr = invoiceDetailVMList.Sum(r => r.Amount);

                // Set total Debit amount in write of master.
                advanceWriteOffParent.Amount = totalAmountDr;

                var totalAmountCr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOffParent.VoucherId = voucher.Id;

                // Advance
                var advance = _advanceService.Find(voucherVM.InvoiceId);
                if (null == advance)
                    throw new CustomException("Advance Id not found!");
                advance.WrittenOffAmount += totalAmountDr;
                advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                advance.UpdatedBy = advanceWriteOffParent.AddedBy;
                advance.UpdatedDate = advanceWriteOffParent.AddedDate;
                advance.UpdatedFromIP = advanceWriteOffParent.AddedFromIP;
                _advanceService.Update(advance);

                var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.InvoiceDetailId);
                if (null == advanceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                advanceDetail.WrittenOffAmount += totalAmountDr;
                if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");
                advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                advanceDetail.UpdatedBy = advance.UpdatedBy;
                advanceDetail.UpdatedDate = advance.UpdatedDate;
                advanceDetail.UpdatedFromIP = advance.UpdatedFromIP;
                _advanceService.UpdateAdvanceDetail(advanceDetail);

                var advanceWriteOffDetailParent = new AdvanceWriteOffDetail
                {
                    AdvanceId = advance.Id,
                    AdvanceDetailId = advanceDetail.Id,
                    AdvanceWriteOffId = advanceWriteOffParent.Id,
                    GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceDetail.BudgetMasterId,
                    ActivityId = advanceDetail.ActivityId,
                    CurrencyId = advance.CurrencyId,
                    PartyType = advance.PartyType,
                    CompanyId = advance.CompanyId,
                    PlantId = advance.PlantId,
                    PartyId = advance.PartyId,
                    PartyPlantId = advance.PartyPlantId,
                    Amount = advanceWriteOffParent.Amount,
                    AddedBy = advanceWriteOffParent.AddedBy,
                    AddedDate = advanceWriteOffParent.AddedDate,
                    AddedFromIP = advanceWriteOffParent.AddedFromIP,
                    Archive = advanceWriteOffParent.Archive
                };
                InsertAdvanceWriteOffDetail(advanceWriteOffParent, advanceWriteOffDetailParent, 1);

                // INSERT INTO VoucherDetail Debit
                var currentVoucherDetailId = 0;
                var voucherDetailDr = new VoucherDetail
                {
                    //AdvanceWriteOffDetailId = invoiceWriteOffDetail.Id,
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                    DrAmount = voucherVM.Amount,
                    PartyType = voucherVM.PartyType,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    AdvanceWriteOffDetailId = advanceWriteOffDetailParent.Id
                };


                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                    CrAmount = voucherDetailDr.CrAmount * voucherVM.CompanyCurrencyRate
                });


                if (voucherVM.SettlementType == SettlementType.SetOff.ToString())
                {
                    // INSERT INTO InvoiceWriteOff
                    var isInvoice = invoiceDetailVMList.Where(r => r.OtherName == "Invoice").ToList();
                    var isAdvance = invoiceDetailVMList.Where(r => r.OtherName == "Advance").ToList();
                    if (isInvoice.Count > 0)
                    {
                        var invoiceWriteOffChild = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                        invoiceWriteOffChild.PartyType = "Customer";
                        invoiceWriteOffChild.VoucherId = voucher.Id;
                        invoiceWriteOffChild.Amount = invoiceDetailVMList.Where(r => r.OtherName == "Invoice").Sum(r => r.Amount);

                        // Invoice
                        var invoiceIds = invoiceDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = invoiceDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;
                        foreach (var voucherDetailVM in invoiceDetailVMList.Where(r => r.OtherName == "Invoice"))
                        {
                            var setOffinvoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == setOffinvoiceDetail)
                                throw new CustomException("Invoice not found!");

                            setOffinvoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                            if (setOffinvoiceDetail.NetAmount < setOffinvoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            setOffinvoiceDetail.IsWrittenOff = setOffinvoiceDetail.NetAmount == setOffinvoiceDetail.WrittenOffAmount;
                            setOffinvoiceDetail.UpdatedBy = invoiceWriteOffChild.AddedBy;
                            setOffinvoiceDetail.UpdatedDate = invoiceWriteOffChild.AddedDate;
                            setOffinvoiceDetail.UpdatedFromIP = invoiceWriteOffChild.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(setOffinvoiceDetail);

                            var setoffinvoice = inviceDbList.First(r => r.Id == setOffinvoiceDetail.InvoiceId);
                            setoffinvoice.WrittenOffAmount = setOffinvoiceDetail.WrittenOffAmount;
                            setoffinvoice.IsWrittenOff = setoffinvoice.Amount == setoffinvoice.WrittenOffAmount;
                            setoffinvoice.UpdatedBy = invoiceWriteOffChild.AddedBy;
                            setoffinvoice.UpdatedDate = invoiceWriteOffChild.AddedDate;
                            setoffinvoice.UpdatedFromIP = invoiceWriteOffChild.AddedFromIP;
                            _invoiceService.Update(setoffinvoice);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetailChild = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = setOffinvoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffinvoiceDetail.BudgetMasterId,
                                ActivityId = setOffinvoiceDetail.ActivityId,
                                CurrencyId = setoffinvoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOffChild.Id,
                                InvoiceId = voucherDetailVM.InvoiceId,
                                InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = setoffinvoice.CompanyId,
                                PlantId = setoffinvoice.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.Amount,
                                AddedBy = invoiceWriteOffChild.AddedBy,
                                AddedDate = invoiceWriteOffChild.AddedDate,
                                AddedFromIP = invoiceWriteOffChild.AddedFromIP,
                                Archive = invoiceWriteOffChild.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOffChild, invoiceWriteOffDetailChild, currentInvoiceDetail);
                            advanceWriteOffParent.Amount = invoiceWriteOffDetailChild.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetailChild.Id,
                                GLGeneralInfoId = setOffinvoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffinvoiceDetail.BudgetMasterId,
                                ActivityId = setOffinvoiceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = invoiceWriteOffChild.PartyType,
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
                    if (isAdvance.Count > 0)
                    {
                        var advanceWriteOffChild = InsertAdvanceWriteOff(voucherVM);
                        advanceWriteOffChild.PartyType = "Vendor";
                        advanceWriteOffChild.VoucherId = voucher.Id;
                        advanceWriteOffChild.Amount = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Sum(r => r.Amount);

                        // Invoice
                        var advanceIds = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Select(r => r.InvoiceId);//Here InvoiceId is AdvanceId value
                        var advanceDbList = _advanceService.Query(r => advanceIds.Contains(r.Id)).Select().ToList();
                        var advanceDetailIds = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Select(r => r.InvoiceDetailId);
                        var advanceDetailDbList = _advanceService.GetAdvanceDetailList(r => advanceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;
                        foreach (var voucherDetailVM in invoiceDetailVMList.Where(r => r.OtherName == "Advance"))
                        {
                            var setOffadvanceDetail = advanceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == setOffadvanceDetail)
                                throw new CustomException("Invoice not found!");

                            setOffadvanceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                            if (setOffadvanceDetail.NetAmount < setOffadvanceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            setOffadvanceDetail.IsWrittenOff = setOffadvanceDetail.NetAmount == setOffadvanceDetail.WrittenOffAmount;
                            setOffadvanceDetail.UpdatedBy = advanceWriteOffChild.AddedBy;
                            setOffadvanceDetail.UpdatedDate = advanceWriteOffChild.AddedDate;
                            setOffadvanceDetail.UpdatedFromIP = advanceWriteOffChild.AddedFromIP;
                            _advanceService.UpdateAdvanceDetail(setOffadvanceDetail);

                            var setoffadvance = advanceDbList.First(r => r.Id == setOffadvanceDetail.AdvanceId);
                            setoffadvance.WrittenOffAmount = setOffadvanceDetail.WrittenOffAmount;
                            setoffadvance.IsWrittenOff = setoffadvance.Amount == setoffadvance.WrittenOffAmount;
                            setoffadvance.UpdatedBy = advanceWriteOffChild.AddedBy;
                            setoffadvance.UpdatedDate = advanceWriteOffChild.AddedDate;
                            setoffadvance.UpdatedFromIP = advanceWriteOffChild.AddedFromIP;
                            _advanceService.Update(setoffadvance);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var advanceWriteOffDetailChild = new AdvanceWriteOffDetail
                            {
                                GLGeneralInfoId = setOffadvanceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffadvanceDetail.BudgetMasterId,
                                ActivityId = setOffadvanceDetail.ActivityId,
                                CurrencyId = setoffadvance.CurrencyId,
                                AdvanceWriteOffId = advanceWriteOffChild.Id,
                                AdvanceId = voucherDetailVM.InvoiceId,
                                AdvanceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = setoffadvance.CompanyId,
                                PlantId = setoffadvance.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.Amount,
                                AddedBy = advanceWriteOffChild.AddedBy,
                                AddedDate = advanceWriteOffChild.AddedDate,
                                AddedFromIP = advanceWriteOffChild.AddedFromIP,
                                Archive = advanceWriteOffChild.Archive,
                            };
                            InsertAdvanceWriteOffDetail(advanceWriteOffChild, advanceWriteOffDetailChild, currentInvoiceDetail);
                            advanceWriteOffParent.Amount = advanceWriteOffDetailChild.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                AdvanceWriteOffDetailId = advanceWriteOffDetailChild.Id,
                                GLGeneralInfoId = setOffadvanceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffadvanceDetail.BudgetMasterId,
                                ActivityId = setOffadvanceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                CrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advanceWriteOffChild.PartyType,
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

        public string InsertPartyAssetAdvanceReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> invoiceDetailVMList)
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
                string _invoiceWriteOffGroupNo = GetInvoiceWriteOffGroupNoPK();


                // INSERT INTO AdvanceWriteOff
                voucherVM.Amount = invoiceDetailVMList.Sum(r => r.Amount);
                voucherVM.InvoiceWriteOffGroupNo = _invoiceWriteOffGroupNo;

                var advanceWriteOffParent = InsertAdvanceWriteOff(voucherVM);
                var totalAmountDr = invoiceDetailVMList.Sum(r => r.Amount);

                // Set total Debit amount in write of master.
                advanceWriteOffParent.Amount = totalAmountDr;

                var totalAmountCr = 0.00M;

                // INSERT INTO Voucher
                var voucher = _voucherService.InsertVoucher(voucherVM);

                // Set to InvoiceWriteOff
                advanceWriteOffParent.VoucherId = voucher.Id;

                // Advance
                var advance = _advanceService.Find(voucherVM.InvoiceId);
                if (null == advance)
                    throw new CustomException("Advance Id not found!");
                advance.WrittenOffAmount += totalAmountDr;
                advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                advance.UpdatedBy = advanceWriteOffParent.AddedBy;
                advance.UpdatedDate = advanceWriteOffParent.AddedDate;
                advance.UpdatedFromIP = advanceWriteOffParent.AddedFromIP;
                _advanceService.Update(advance);

                var advanceDetail = _advanceService.FindAdvanceDetail(voucherVM.InvoiceDetailId);
                if (null == advanceDetail)
                    throw new CustomException("Advance Detail Id not found!");
                advanceDetail.WrittenOffAmount += totalAmountDr;
                if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                    throw new CustomException("Received amount can not cross balance amount.");
                advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                advanceDetail.UpdatedBy = advance.UpdatedBy;
                advanceDetail.UpdatedDate = advance.UpdatedDate;
                advanceDetail.UpdatedFromIP = advance.UpdatedFromIP;
                _advanceService.UpdateAdvanceDetail(advanceDetail);

                var advanceWriteOffDetailParent = new AdvanceWriteOffDetail
                {
                    AdvanceId = advance.Id,
                    AdvanceDetailId = advanceDetail.Id,
                    AdvanceWriteOffId = advanceWriteOffParent.Id,
                    GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                    BudgetMasterId = advanceDetail.BudgetMasterId,
                    ActivityId = advanceDetail.ActivityId,
                    CurrencyId = advance.CurrencyId,
                    PartyType = advance.PartyType,
                    CompanyId = advance.CompanyId,
                    PlantId = advance.PlantId,
                    PartyId = advance.PartyId,
                    PartyPlantId = advance.PartyPlantId,
                    Amount = advanceWriteOffParent.Amount,
                    AddedBy = advanceWriteOffParent.AddedBy,
                    AddedDate = advanceWriteOffParent.AddedDate,
                    AddedFromIP = advanceWriteOffParent.AddedFromIP,
                    Archive = advanceWriteOffParent.Archive
                };
                InsertAdvanceWriteOffDetail(advanceWriteOffParent, advanceWriteOffDetailParent, 1);

                // INSERT INTO VoucherDetail Debit
                var currentVoucherDetailId = 0;
                var voucherDetailDr = new VoucherDetail
                {
                    //AdvanceWriteOffDetailId = invoiceWriteOffDetail.Id,
                    GLGeneralInfoId = voucherVM.GLGeneralInfoId,
                    BudgetMasterId = voucherVM.BudgetMasterId,
                    ActivityId = voucherVM.ActivityId,
                    CrAmount = voucherVM.Amount,
                    PartyType = voucherVM.PartyType,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    AdvanceWriteOffDetailId = advanceWriteOffDetailParent.Id,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration
                };


                currentVoucherDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherDetailDr, currentVoucherDetailId);

                // INSERT INTO VoucherDetailCurrency
                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                {
                    ParallelCurrencyId = companyCurrencyId,
                    FromCurrencyId = voucherDetailDr.CurrencyId,
                    ToCurrencyId = companyCurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                    DrAmount = voucherDetailDr.DrAmount * voucherVM.CompanyCurrencyRate,
                    CrAmount = voucherDetailDr.CrAmount * voucherVM.CompanyCurrencyRate
                });


                if (voucherVM.SettlementType == SettlementType.SetOff.ToString())
                {
                    var isInvoice = invoiceDetailVMList.Where(r => r.OtherName == "Invoice").ToList();
                    var isAdvance = invoiceDetailVMList.Where(r => r.OtherName == "Advance").ToList();
                    if (isInvoice.Count > 0)
                    {
                        var invoiceWriteOffChild = _invoiceWriteOffService.InsertCustomerInvoiceSetOff(voucherVM);
                        invoiceWriteOffChild.VoucherId = voucher.Id;
                        invoiceWriteOffChild.Amount = invoiceDetailVMList.Where(r => r.OtherName == "Invoice").Sum(r => r.Amount); 
                        invoiceWriteOffChild.PartyType = "Vendor";

                        // Invoice
                        var invoiceIds = invoiceDetailVMList.Select(r => r.InvoiceId);
                        var inviceDbList = _invoiceService.Query(r => invoiceIds.Contains(r.Id)).Select().ToList();
                        var invoiceDetailIds = invoiceDetailVMList.Select(r => r.InvoiceDetailId);
                        var inviceDetailDbList = _invoiceService.GetInvoiceDetailList(r => invoiceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;
                        foreach (var voucherDetailVM in invoiceDetailVMList.Where(r => r.OtherName == "Invoice"))
                        {
                            var setOffinvoiceDetail = inviceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == setOffinvoiceDetail)
                                throw new CustomException("Invoice not found!");

                            setOffinvoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                            if (setOffinvoiceDetail.NetAmount < setOffinvoiceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            setOffinvoiceDetail.IsWrittenOff = setOffinvoiceDetail.NetAmount == setOffinvoiceDetail.WrittenOffAmount;
                            setOffinvoiceDetail.UpdatedBy = invoiceWriteOffChild.AddedBy;
                            setOffinvoiceDetail.UpdatedDate = invoiceWriteOffChild.AddedDate;
                            setOffinvoiceDetail.UpdatedFromIP = invoiceWriteOffChild.AddedFromIP;
                            _invoiceService.UpdateInvoiceDetail(setOffinvoiceDetail);

                            var setoffinvoice = inviceDbList.First(r => r.Id == setOffinvoiceDetail.InvoiceId);
                            setoffinvoice.WrittenOffAmount = setOffinvoiceDetail.WrittenOffAmount;
                            setoffinvoice.IsWrittenOff = setoffinvoice.Amount == setoffinvoice.WrittenOffAmount;
                            setoffinvoice.UpdatedBy = invoiceWriteOffChild.AddedBy;
                            setoffinvoice.UpdatedDate = invoiceWriteOffChild.AddedDate;
                            setoffinvoice.UpdatedFromIP = invoiceWriteOffChild.AddedFromIP;
                            _invoiceService.Update(setoffinvoice);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetailChild = new InvoiceWriteOffDetail
                            {
                                GLGeneralInfoId = setOffinvoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffinvoiceDetail.BudgetMasterId,
                                ActivityId = setOffinvoiceDetail.ActivityId,
                                CurrencyId = setoffinvoice.CurrencyId,
                                InvoiceWriteOffId = invoiceWriteOffChild.Id,
                                InvoiceId = voucherDetailVM.InvoiceId,
                                InvoiceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = setoffinvoice.CompanyId,
                                PlantId = setoffinvoice.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.Amount,
                                AddedBy = invoiceWriteOffChild.AddedBy,
                                AddedDate = invoiceWriteOffChild.AddedDate,
                                AddedFromIP = invoiceWriteOffChild.AddedFromIP,
                                Archive = invoiceWriteOffChild.Archive,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration
                            };
                            _invoiceWriteOffService.InsertInvoiceWriteOffDetail(invoiceWriteOffChild, invoiceWriteOffDetailChild, currentInvoiceDetail);
                            //advanceWriteOffParent.Amount = invoiceWriteOffDetailChild.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetailChild.Id,
                                GLGeneralInfoId = setOffinvoiceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffinvoiceDetail.BudgetMasterId,
                                ActivityId = setOffinvoiceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = invoiceWriteOffChild.PartyType,
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
                    if (isAdvance.Count > 0)
                    {
                        var advanceWriteOffChild = InsertAdvanceWriteOff(voucherVM);
                        advanceWriteOffChild.PartyType = "Customer";
                        advanceWriteOffChild.VoucherId = voucher.Id;
                        advanceWriteOffChild.Amount = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Sum(r => r.Amount);

                        // Invoice
                        var advanceIds = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Select(r => r.InvoiceId);//Here InvoiceId is AdvanceId value
                        var advanceDbList = _advanceService.Query(r => advanceIds.Contains(r.Id)).Select().ToList();
                        var advanceDetailIds = invoiceDetailVMList.Where(r => r.OtherName == "Advance").Select(r => r.InvoiceDetailId);
                        var advanceDetailDbList = _advanceService.GetAdvanceDetailList(r => advanceDetailIds.Contains(r.Id)).Select().ToList();
                        var currentInvoiceDetail = 0;
                        foreach (var voucherDetailVM in invoiceDetailVMList.Where(r => r.OtherName == "Advance"))
                        {
                            var setOffadvanceDetail = advanceDetailDbList.FirstOrDefault(r => r.Id == voucherDetailVM.InvoiceDetailId);
                            if (null == setOffadvanceDetail)
                                throw new CustomException("Invoice not found!");

                            setOffadvanceDetail.WrittenOffAmount += voucherDetailVM.Amount;
                            if (setOffadvanceDetail.NetAmount < setOffadvanceDetail.WrittenOffAmount)
                                throw new CustomException("Received amount can not cross balance amount.");

                            setOffadvanceDetail.IsWrittenOff = setOffadvanceDetail.NetAmount == setOffadvanceDetail.WrittenOffAmount;
                            setOffadvanceDetail.UpdatedBy = advanceWriteOffChild.AddedBy;
                            setOffadvanceDetail.UpdatedDate = advanceWriteOffChild.AddedDate;
                            setOffadvanceDetail.UpdatedFromIP = advanceWriteOffChild.AddedFromIP;
                            _advanceService.UpdateAdvanceDetail(setOffadvanceDetail);

                            var setoffadvance = advanceDbList.First(r => r.Id == setOffadvanceDetail.AdvanceId);
                            setoffadvance.WrittenOffAmount = setOffadvanceDetail.WrittenOffAmount;
                            setoffadvance.IsWrittenOff = setoffadvance.Amount == setoffadvance.WrittenOffAmount;
                            setoffadvance.UpdatedBy = advanceWriteOffChild.AddedBy;
                            setoffadvance.UpdatedDate = advanceWriteOffChild.AddedDate;
                            setoffadvance.UpdatedFromIP = advanceWriteOffChild.AddedFromIP;
                            _advanceService.Update(setoffadvance);

                            // INSERT INTO InvoiceWriteOffDetail
                            currentInvoiceDetail++;
                            var invoiceWriteOffDetailChild = new AdvanceWriteOffDetail
                            {
                                GLGeneralInfoId = setOffadvanceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffadvanceDetail.BudgetMasterId,
                                ActivityId = setOffadvanceDetail.ActivityId,
                                CurrencyId = setoffadvance.CurrencyId,
                                AdvanceWriteOffId = advanceWriteOffChild.Id,
                                AdvanceId = voucherDetailVM.InvoiceId,
                                AdvanceDetailId = voucherDetailVM.InvoiceDetailId,
                                CompanyId = setoffadvance.CompanyId,
                                PlantId = setoffadvance.PlantId,
                                PartyId = voucherDetailVM.PartyId,
                                PartyPlantId = voucherDetailVM.PartyPlantId,
                                PartyType = voucherDetailVM.PartyType,
                                Amount = voucherDetailVM.Amount,
                                AddedBy = advanceWriteOffChild.AddedBy,
                                AddedDate = advanceWriteOffChild.AddedDate,
                                AddedFromIP = advanceWriteOffChild.AddedFromIP,
                                Archive = advanceWriteOffChild.Archive,
                            };
                            InsertAdvanceWriteOffDetail(advanceWriteOffChild, invoiceWriteOffDetailChild, currentInvoiceDetail);
                            //invoiceWriteOffParent.Amount = invoiceWriteOffDetailChild.Amount;

                            // INSERT INTO VoucherDetail
                            var voucherDetailCr = new VoucherDetail
                            {
                                VoucherId = voucher.Id,
                                InvoiceWriteOffDetailId = invoiceWriteOffDetailChild.Id,
                                GLGeneralInfoId = setOffadvanceDetail.GLGeneralInfoId,
                                BudgetMasterId = setOffadvanceDetail.BudgetMasterId,
                                ActivityId = setOffadvanceDetail.ActivityId,
                                CurrencyId = voucher.CurrencyId,
                                DrAmount = voucherDetailVM.Amount,
                                DocDate = voucherDetailVM.DocDate,
                                DocRefNo = voucherDetailVM.DocRefNo,
                                Narration = voucherDetailVM.Narration,
                                PartyType = advanceWriteOffChild.PartyType,
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
        private static void InvoiceWriteOffCheckIsPosted(InvoiceWriteOff invoiceWriteOff)
        {
            if (!invoiceWriteOff.IsPark)
                throw new CustomException(ServiceResources.UpdateOrDeleteNotAllow);
        }
        public void PostPartyReconciliation(string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var invoiceWriteOff = _invoiceWriteOffService.Query(r => r.VoucherId == voucherId).Select().ToList();
                var advanceWriteOff = base.Query(r => r.VoucherId == voucherId).Select().ToList();
                if (invoiceWriteOff.Count > 0)
                {
                    foreach (var item in invoiceWriteOff)
                    {
                        InvoiceWriteOffCheckIsPosted(item);
                        item.IsPark = false;
                        _invoiceWriteOffService.UpdateGraph(item);
                    }
                }

                if (advanceWriteOff.Count > 0)
                {
                    foreach (var item in advanceWriteOff)
                    {
                        CheckIsPosted(item);
                        item.IsPark = false;
                        base.UpdateGraph(item);
                    }
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

        public void DeleteCustomerAdvanceWriteOff(string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var rdBuilder = new System.Text.StringBuilder();
                var voucher = _voucherRepository.Find(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var advanceWriteOff = _advanceWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                var advanceWriteOffDetail = _advanceWriteOffDetailRepository.Query(r => r.AdvanceWriteOffId == advanceWriteOff.Id).Select().ToList();
                var invoiceWriteOff = _invoiceWriteOffService.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();


                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    //var gltransactionDetail = _gLTransactionDetailRepository.Find(item.Id);
                    //if (gltransactionDetail != null)
                    //    _gLTransactionDetailRepository.Delete(gltransactionDetail.Id);
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT * from trn.GLTransactionDetail where Id = '" + item.Id + "'";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        var voucherSql = @"Delete [TRN].GLTransactionDetail  WHERE Id='" + item.Id + "'";
                        rdBuilder.Append(voucherSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }

                    _voucherDetailRepository.Delete(item.Id);
                }


                if (invoiceWriteOff != null)
                {
                    var invoiceWriteOffDetail = _invoiceWriteOffDetailRepository.Query(r => r.InvoiceWriteOffId == invoiceWriteOff.Id).Select().ToList();
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
                    _invoiceWriteOffService.Delete(invoiceWriteOff.Id);
                }

                if (advanceWriteOffDetail != null)
                {
                    foreach (var item in advanceWriteOffDetail)
                    {
                        var advance = _advanceService.Find(item.AdvanceId);
                        var advanceDetail = _advanceDetailRepository.Find(item.AdvanceDetailId);

                        advanceDetail.WrittenOffAmount -= item.Amount;
                        advance.WrittenOffAmount -= item.Amount;
                        advanceDetail.IsWrittenOff = advanceDetail.NetAmount == advanceDetail.WrittenOffAmount;
                        advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;

                        _advanceDetailRepository.Update(advanceDetail);
                        _advanceService.Update(advance);
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
    }
}