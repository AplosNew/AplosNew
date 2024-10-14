using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Expenses;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Finances;
using Library.Service.Invoices;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Employees
{
    public class EmployeePayableWriteOffService : IEmployeePayableWriteOffService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IRepositoryAsync<EmployeePayableWriteOff> _employeePayableWriteOffRepository;
        private readonly IRepositoryAsync<EmployeePayableWriteOffDetail> _employeePayableWriteOffDetailRepository;
        private readonly IRepositoryAsync<EmployeePayableDetail> _employeePayableDetailRepository;
        private readonly IRepositoryAsync<EmployeePayable> _employeePayableRepository;
        private readonly IRepositoryAsync<ExpenseBooking> _expenseBookingRepository;
        private readonly IRepositoryAsync<ExpenseBookingDetail> _expenseBookingDetailRepository;
        private readonly IRepositoryAsync<ExpenseBookingApprovalHistory> _expenseBookingApprovalHistoryRepository;
        //private readonly IRepositoryAsync<Voucher> _voucherRepository;
        //private readonly IRepositoryAsync<GLTransactionDetail> _gLTransactionDetailRepository;
        private readonly IPKGeneratorService _pKGeneratorService;
        private readonly IFinancingTypeGLService _financingTypeGLService;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IRepositoryAsync<EmployeeSubsequentTransaction> _employeeSubsequentTransactionRepository;
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<InvoiceDetail> _invoiceDetailRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<AdditionalTax> _additionalTaxRepository;

        public EmployeePayableWriteOffService(
              IRepositoryAsync<EmployeePayableWriteOff> employeePayableWriteOffRepository
            , IRepositoryAsync<EmployeePayable> employeePayableRepository
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IVoucherService voucherService
            , IPKGeneratorService pkGeneratorService
            , IEmployeePayableService employeePayableService
            , IRepositoryAsync<EmployeePayableWriteOffDetail> employeePayableWriteOffDetailRepository
            , IRepositoryAsync<EmployeePayableDetail> employeePayableDetailRepository
            //, IRepositoryAsync<Voucher> voucherRepository
            //, IRepositoryAsync<GLTransactionDetail> gLTransactionDetailRepository
            , IFinancingTypeGLService financingTypeGLService
            , IRepositoryAsync<ExpenseBooking> expenseBookingRepository
            , IRepositoryAsync<ExpenseBookingDetail> expenseBookingDetailRepository
           , IRepositoryAsync<ExpenseBookingApprovalHistory> expenseBookingApprovalHistoryRepository
            , IRepositoryAsync<EmployeeSubsequentTransaction> employeeSubsequentTransactionRepository
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<Invoice> invoiceRepository
            , IRepositoryAsync<InvoiceDetail> invoiceDetailRepository
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            , IRepositoryAsync<AdditionalTax> additionalTaxRepository)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
            _employeePayableService = employeePayableService;
            _employeePayableWriteOffRepository = employeePayableWriteOffRepository;
            _employeePayableWriteOffDetailRepository = employeePayableWriteOffDetailRepository;
            _employeePayableDetailRepository = employeePayableDetailRepository;
            _employeePayableRepository = employeePayableRepository;
            _pKGeneratorService = pkGeneratorService;
            _financingTypeGLService = financingTypeGLService;
            _invoiceTaxService = invoiceTaxService;
           // _voucherRepository = voucherRepository;
            //_gLTransactionDetailRepository = gLTransactionDetailRepository;
            _expenseBookingRepository = expenseBookingRepository;
            _expenseBookingDetailRepository = expenseBookingDetailRepository;
            _expenseBookingApprovalHistoryRepository = expenseBookingApprovalHistoryRepository;
            _employeeSubsequentTransactionRepository = employeeSubsequentTransactionRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _invoiceTaxRepository = invoiceTaxRepository;
            _additionalTaxRepository = additionalTaxRepository;
        }

        #endregion Constructor

        public EmployeePayableWriteOff InsertEmployeePayableWriteOff(EmployeePayableWriteOff employeePayableWriteOff)
        {
            employeePayableWriteOff.Id = _pKGeneratorService.GetAutoNumber(nameof(EmployeePayableWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
            AuditService.AddedLog(employeePayableWriteOff);
            _employeePayableWriteOffRepository.Insert(employeePayableWriteOff);
            return employeePayableWriteOff;
        }

        public EmployeePayableWriteOffDetail InsertEmployeePayableWriteOffDetail(EmployeePayableWriteOff employeePayableWriteOff, EmployeePayableWriteOffDetail employeePayableWriteOffDetail, int currentId)
        {
            employeePayableWriteOffDetail.Id = _pKGeneratorService.MakePK(employeePayableWriteOff.Id, currentId, 2);
            employeePayableWriteOffDetail.EmployeePayableWriteOffId = employeePayableWriteOff.Id;
            employeePayableWriteOffDetail.AddedBy = employeePayableWriteOff.AddedBy;
            employeePayableWriteOffDetail.AddedDate = employeePayableWriteOff.AddedDate;
            employeePayableWriteOffDetail.AddedFromIP = employeePayableWriteOff.AddedFromIP;
            _employeePayableWriteOffDetailRepository.Insert(employeePayableWriteOffDetail);
            return employeePayableWriteOffDetail;
        }

        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT EPW.Id, EPWD.EmployeePayableWriteOffId, EPW.VoucherId, V.VoucherNo, V.VoucherDate, V.TransactionRefNo, V.Narration, V.VoucherTypeId, V.PostingDate, V.DocRefNo, V.DocDate, EI.EmployeeCode, EI.EmployeeName AS EmployeeName
                                    , EI.SystemId AS EmployeeId, V.CurrencyId, VT.UserName AS VoucherType, C.Code AS CurrencyCode, EPW.RowState
                                    ,(SELECT SUM(DrAmount) FROM TRN.VoucherDetail WHERE VoucherId=EPW.VoucherId group by VoucherId) AS Amount
                                    FROM [TRN].[EmployeePayableWriteOff] AS EPW
                                    LEFT JOIN TRN.EmployeePayableWriteOffDetail AS EPWD ON EPWD.EmployeePayableWriteOffId=EPW.Id
                                    LEFT JOIN TRN.VoucherDetail AS VD ON VD.EmployeePayableWriteOffDetailId = EPWD.Id
                                    LEFT JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EPW.EmployeeId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                                    LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                                    WHERE V.Archive=0 AND V.SourceType='" + SourceType.EmployeePayment + "' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        private string GetEmployeeSubsequentTransactionPK()
        {
            return _pKGeneratorService.GetAutoNumber("EmployeeSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertEmployeePayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
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
                voucherVM.Amount = voucherDetailVMList.Sum(r => r.Amount);
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
                InsertEmployeePayableWriteOff(employeePayableWriteOff);

                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = employeePayableWriteOff.CompanyGroupId,
                    CompanyId = employeePayableWriteOff.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = employeePayableWriteOff.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    AddedBy = employeePayableWriteOff.AddedBy,
                    AddedDate = employeePayableWriteOff.AddedDate,
                    AddedFromIP = employeePayableWriteOff.AddedFromIP,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = employeePayableWriteOff.DocDate,
                    DocRefNo = employeePayableWriteOff.DocRefNo,
                    Narration = employeePayableWriteOff.Narration,
                    Archive = employeePayableWriteOff.Archive,
                    SourceType = employeePayableWriteOff.SourceType,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    IsPark = true
                };
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                // Set to InvoiceWriteOff
                employeePayableWriteOff.VoucherId = voucher.Id;

                var employeePayableWriteOffDetailPk = _pKGeneratorService.GetMaxNumber("EmployeePayableWriteOffDetail", PKGeneratorEnum.Auto, null, DateTime.Now);

                var currentVoucheDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;
                if (employeePayableWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                    voucherVM.Amount -= voucherVM.RoundingAmount;
                if (employeePayableWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    voucherVM.Amount += voucherVM.RoundingAmount;
                // INSERT INTO VoucherDetail
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
                    CrAmount = voucherVM.Amount,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = employeePayableWriteOff.Narration,
                    PostingWithoutTaxAllow = false,
                    BankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    VoucherId = voucher.Id,
                    IsPark = voucher.IsPark
                };
                currentVoucheDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucheDetailId);

                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                    {
                        CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate,
                        FromCurrencyId = companyCurrencyId,
                        ParallelCurrencyId = companyCurrencyId,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        ToCurrencyId = voucherVM.CurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate
                    });

                    if (companyCurrencyId == voucherVM.BankCurrencyId)
                        voucherVM.BankAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate;
                }

                if (!string.IsNullOrEmpty(voucherCr.BankMasterId))
                {
                    // INSRT INTO GLTransactionDetail TABLE
                    _voucherService.InsertGLTransactionDetail(voucherCr, new GLTransactionDetail
                    {
                        Id = voucherCr.Id,
                        AddedBy = voucherCr.AddedBy,
                        AddedDate = voucherCr.AddedDate,
                        AddedFromIP = voucherCr.AddedFromIP,
                        BankMasterId = voucherCr.BankMasterId,
                        CrAmount = voucherVM.BankAmount,
                        VoucherDetailId = voucherCr.Id,
                        SourceType = PaymentSource.Bank.ToString()
                    });
                }
                if (!string.IsNullOrEmpty(voucherCr.CashMasterId))
                {
                    // INSRT INTO GLTransactionDetail TABLE
                    _voucherService.InsertGLTransactionDetail(voucherCr, new GLTransactionDetail
                    {
                        Id = voucherCr.Id,
                        AddedBy = voucherCr.AddedBy,
                        AddedDate = voucherCr.AddedDate,
                        AddedFromIP = voucherCr.AddedFromIP,
                        CashMasterId = voucherCr.CashMasterId,
                        CrAmount = voucherVM.Amount * voucherVM.CompanyCurrencyRate,
                        ModelState = ModelState.Added,
                        VoucherDetailId = voucherCr.Id,
                        SourceType = PartyType.Cash.ToString()
                    });
                }

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
                    InsertEmployeePayableWriteOffDetail(employeePayableWriteOff, employeePayableWriteOffDetail, currentInvoiceWriteOffDetailId);

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
                    currentVoucheDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucheDetailId);

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
                    var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        AdvanceId = null,
                        EmployeeId = employeePayable.EmployeeId,
                        EmployeeTransactionTypeId = employeePayable.EmployeeTransactionTypeId,
                        AdvanceWriteOffId = null,
                        EmployeePayableWriteOffId = employeePayableWriteOff.Id,
                        EmployeePayableId = null,
                        PartyType = employeePayable.PartyType,
                        CurrencyId = employeePayable.CurrencyId,
                        Amount = voucherDr.DrAmount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        JournalType = voucherDetailVM.JournalType,
                        TransactionType = EmployeeSubsequentTranEnum.Payment.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = employeePayableWriteOff.SourceType,
                        IsPark = voucherVM.IsPark,
                        Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                        VoucherId = voucher.Id,
                        VoucherDetailId = voucherDr.Id,
                        PaymentSource = voucherVM.PaymentSource,
                    };
                    AuditService.AddedLog(EmployeeSubsequentAdvance);
                    _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);

                }

                if (!string.IsNullOrEmpty(employeePayableWriteOff.RoundingType))
                {
                    if (employeePayableWriteOff.RoundingType == RoundingType.RoundDown.ToString() || employeePayableWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(employeePayableWriteOff.CompanyId);
                        if (employeePayableWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = employeePayableWriteOff.RoundingAmount,
                                DocDate = employeePayableWriteOff.DocDate,
                                DocRefNo = employeePayableWriteOff.DocRefNo,
                                Narration = employeePayableWriteOff.Narration,
                                PartyType = employeePayableWriteOff.PartyType
                            };
                            currentVoucheDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucheDetailId);

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
                        if (employeePayableWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = employeePayableWriteOff.RoundingAmount,
                                DocDate = employeePayableWriteOff.DocDate,
                                DocRefNo = employeePayableWriteOff.DocRefNo,
                                Narration = employeePayableWriteOff.Narration,
                                PartyType = employeePayableWriteOff.PartyType
                            };
                            currentVoucheDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucheDetailId);

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

        public string InsertMultipleEmployeePayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> employeeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                
                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO InvoiceWriteOff TABLE
                foreach (var mulpay in employeeDetailVMList)
                {
                    _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                    _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                    voucherVM.Amount = voucherDetailVMList.Where(r => r.EmployeeId == mulpay.EmployeeId).Sum(r => r.Amount);
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
                InsertEmployeePayableWriteOff(employeePayableWriteOff);

                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = employeePayableWriteOff.CompanyGroupId,
                    CompanyId = employeePayableWriteOff.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = employeePayableWriteOff.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    AddedBy = employeePayableWriteOff.AddedBy,
                    AddedDate = employeePayableWriteOff.AddedDate,
                    AddedFromIP = employeePayableWriteOff.AddedFromIP,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = employeePayableWriteOff.DocDate,
                    DocRefNo = employeePayableWriteOff.DocRefNo,
                    Narration = employeePayableWriteOff.Narration,
                    Archive = employeePayableWriteOff.Archive,
                    SourceType = employeePayableWriteOff.SourceType,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    IsPark = true
                };
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                // Set to InvoiceWriteOff
                employeePayableWriteOff.VoucherId = voucher.Id;

                var employeePayableWriteOffDetailPk = _pKGeneratorService.GetMaxNumber("EmployeePayableWriteOffDetail", PKGeneratorEnum.Auto, null, DateTime.Now);

                var currentVoucheDetailId = 0;
                var currentInvoiceWriteOffDetailId = 0;
                if (employeePayableWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                    voucherVM.Amount -= voucherVM.RoundingAmount;
                if (employeePayableWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    voucherVM.Amount += voucherVM.RoundingAmount;
                // INSERT INTO VoucherDetail
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
                    CrAmount = voucherVM.Amount,
                    DocDate = voucher.DocDate,
                    DocRefNo = voucher.DocRefNo,
                    Narration = employeePayableWriteOff.Narration,
                    PostingWithoutTaxAllow = false,
                    BankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    VoucherId = voucher.Id,
                    IsPark = voucher.IsPark
                };
                currentVoucheDetailId++;
                _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucheDetailId);

                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                    {
                        CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate,
                        FromCurrencyId = companyCurrencyId,
                        ParallelCurrencyId = companyCurrencyId,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                        ToCurrencyId = voucherVM.CurrencyId,
                        ToCurrencyRate = voucherVM.CompanyCurrencyRate
                    });

                    if (companyCurrencyId == voucherVM.BankCurrencyId)
                        voucherVM.BankAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate;
                }

                if (!string.IsNullOrEmpty(voucherCr.BankMasterId))
                {
                    // INSRT INTO GLTransactionDetail TABLE
                    _voucherService.InsertGLTransactionDetail(voucherCr, new GLTransactionDetail
                    {
                        Id = voucherCr.Id,
                        AddedBy = voucherCr.AddedBy,
                        AddedDate = voucherCr.AddedDate,
                        AddedFromIP = voucherCr.AddedFromIP,
                        BankMasterId = voucherCr.BankMasterId,
                        CrAmount = voucherVM.BankAmount,
                        VoucherDetailId = voucherCr.Id,
                        SourceType = PaymentSource.Bank.ToString()
                    });
                }
                if (!string.IsNullOrEmpty(voucherCr.CashMasterId))
                {
                    // INSRT INTO GLTransactionDetail TABLE
                    _voucherService.InsertGLTransactionDetail(voucherCr, new GLTransactionDetail
                    {
                        Id = voucherCr.Id,
                        AddedBy = voucherCr.AddedBy,
                        AddedDate = voucherCr.AddedDate,
                        AddedFromIP = voucherCr.AddedFromIP,
                        CashMasterId = voucherCr.CashMasterId,
                        CrAmount = voucherVM.Amount * voucherVM.CompanyCurrencyRate,
                        ModelState = ModelState.Added,
                        VoucherDetailId = voucherCr.Id,
                        SourceType = PartyType.Cash.ToString()
                    });
                }

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
                    InsertEmployeePayableWriteOffDetail(employeePayableWriteOff, employeePayableWriteOffDetail, currentInvoiceWriteOffDetailId);

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
                    currentVoucheDetailId++;
                    _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucheDetailId);

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
                    var EmployeeSubsequentAdvance = new EmployeeSubsequentTransaction
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        VoucherTypeId = voucherVM.VoucherTypeId,
                        AdvanceId = null,
                        EmployeeId = employeePayable.EmployeeId,
                        EmployeeTransactionTypeId = employeePayable.EmployeeTransactionTypeId,
                        AdvanceWriteOffId = null,
                        EmployeePayableWriteOffId = employeePayableWriteOff.Id,
                        EmployeePayableId = null,
                        PartyType = employeePayable.PartyType,
                        CurrencyId = employeePayable.CurrencyId,
                        Amount = voucherDr.DrAmount,
                        VoucherDate = voucherVM.VoucherDate,
                        PostingDate = voucherVM.PostingDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        JournalType = voucherDetailVM.JournalType,
                        TransactionType = EmployeeSubsequentTranEnum.Payment.ToString(),
                        Narration = voucherVM.Narration,
                        SourceType = employeePayableWriteOff.SourceType,
                        IsPark = voucherVM.IsPark,
                        Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                        VoucherId = voucher.Id,
                        VoucherDetailId = voucherDr.Id,
                        PaymentSource = voucherVM.PaymentSource,
                    };
                    AuditService.AddedLog(EmployeeSubsequentAdvance);
                    _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);

                }

                if (!string.IsNullOrEmpty(employeePayableWriteOff.RoundingType))
                {
                    if (employeePayableWriteOff.RoundingType == RoundingType.RoundDown.ToString() || employeePayableWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                    {
                        var gl = _financingTypeGLService.GetRoundingGL(employeePayableWriteOff.CompanyId);
                        if (employeePayableWriteOff.RoundingType == RoundingType.RoundUp.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                DrAmount = employeePayableWriteOff.RoundingAmount,
                                DocDate = employeePayableWriteOff.DocDate,
                                DocRefNo = employeePayableWriteOff.DocRefNo,
                                Narration = employeePayableWriteOff.Narration,
                                PartyType = employeePayableWriteOff.PartyType
                            };
                            currentVoucheDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucheDetailId);

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
                        if (employeePayableWriteOff.RoundingType == RoundingType.RoundDown.ToString())
                        {
                            var voucherDetailRoundingDr = new VoucherDetail
                            {
                                GLGeneralInfoId = gl.ExpensesGLId,
                                BudgetMasterId = gl.ExpensesBudgetMasterId,
                                ActivityId = gl.ExpensesActivityId,
                                EntityId = voucher.EntityId,
                                CrAmount = employeePayableWriteOff.RoundingAmount,
                                DocDate = employeePayableWriteOff.DocDate,
                                DocRefNo = employeePayableWriteOff.DocRefNo,
                                Narration = employeePayableWriteOff.Narration,
                                PartyType = employeePayableWriteOff.PartyType
                            };
                            currentVoucheDetailId++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDetailRoundingDr, currentVoucheDetailId);

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
            }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return "Saved successfully";
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

        public void Post(string employeePayableWriteOffId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var employeePayableWrite = _employeePayableWriteOffRepository.Find(employeePayableWriteOffId);
                CheckIsPosted(employeePayableWrite);
                employeePayableWrite.RowState = RowState.Posted.ToString();
                _employeePayableWriteOffRepository.Update(employeePayableWrite);
                _voucherService.PostVoucher(employeePayableWrite.VoucherId);
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

        private static void CheckIsPosted(EmployeePayableWriteOff employeePayableWrite)
        {
            if (employeePayableWrite.RowState != RowState.Parked.ToString())
                throw new CustomException(ServiceResources.UpdateOrDeleteNotAllow);
        }
        public void DeletePayableWriteOff(string employeePayableWriteOffId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var voucherdetail = _voucherService.QueryVoucherDetail(voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherService.QueryVoucherDetailCurrency(voucherId).Select().ToList();
                var empSubsequent = _employeeSubsequentTransactionRepository.Query(r=>r.VoucherId== voucherId).Select().ToList();
                var invoiceWriteOff = _employeePayableWriteOffRepository.Find(employeePayableWriteOffId);
                var invoiceWriteOffDetail = _employeePayableWriteOffDetailRepository.Query(r => r.EmployeePayableWriteOffId == employeePayableWriteOffId).Select().ToList();
                //var invoiceTax = _invoiceTaxRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }
                foreach (var item in empSubsequent)
                {
                    _employeeSubsequentTransactionRepository.Delete(item.Id);
                }
                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _voucherService.QueryGLTransactionDetail(item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _voucherService.DeleteGLTransactionDetail(item.Id);
                    }
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = @"UPDATE [TRN].VoucherDetail SET EmployeePayableWriteOffDetailId=NULL,UpdatedBy='" + identity.UserId + "' WHERE Id='" + item.Id + "'";
                    rdBuilder.Append(builderSql);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                //if (invoiceTax != null)
                //{
                //    foreach (var item in invoiceTax)
                //    {
                //        var rdBuilder = new System.Text.StringBuilder();
                //        var builderSql = @"UPDATE [TRN].InvoiceTax SET VoucherDetailId=NULL WHERE Id='" + item.Id + "'";
                //        rdBuilder.Append(builderSql);
                //        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                //        var invoicetaxDdetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == item.Id).Select().ToList();
                //        foreach (var item1 in invoicetaxDdetail)
                //        {
                //            _invoiceTaxDetailRepository.Delete(item1.Id);
                //        }
                //        _invoiceTaxRepository.Delete(item.Id);
                //    }
                //}
                foreach (var item in invoiceWriteOffDetail)
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
                _employeePayableWriteOffRepository.Delete(employeePayableWriteOffId);
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

        public void DeleteEmployeePayable(string employeeBookingId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");
                var employeePayable = new EmployeePayable();
                var employeePayableDetail = new List<EmployeePayableDetail>();

                var invoice = new Invoice();
                var invoiceDetail = new List<InvoiceDetail>();
                var invoiceTax = new List<InvoiceTax>();
                var invoiceTDS = new List<AdditionalTax>();

                var voucherdetail = _voucherService.QueryVoucherDetail(voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherService.QueryVoucherDetailCurrency(voucherId).Select().ToList();

                var expensesBooking = _expenseBookingRepository.Query(r => r.Id == employeeBookingId).Select().FirstOrDefault();
                var expensesBookingDetail = _expenseBookingDetailRepository.Query(r => r.ExpenseBookingId == employeeBookingId).Select().ToList();
                var expenseBookingApprovalHistoryRepository = _expenseBookingApprovalHistoryRepository.Query(r => r.ExpenseBookingId == employeeBookingId).Select().ToList();

                if (expensesBooking.BeneficiaryType == "Vendor")
                {
                     invoice = _invoiceRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                     invoiceDetail = _invoiceDetailRepository.Query(r => r.InvoiceId == invoice.Id).Select().ToList();
                     invoiceTax = _invoiceTaxRepository.Query(r => r.InvoiceId == invoice.Id).Select().ToList();
                     invoiceTDS = _additionalTaxRepository.Query(r => r.InvoiceId == invoice.Id).Select().ToList();
                }
                if (expensesBooking.BeneficiaryType == "Self")
                {
                     employeePayable = _employeePayableRepository.Query(r => r.VoucherId == voucherId).Select().FirstOrDefault();
                     employeePayableDetail = _employeePayableDetailRepository.Query(r => r.EmployeePayableId == employeePayable.Id).Select().ToList();
                }
                    
               
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = @"DELETE FROM [TRN].EmployeeSubsequentTransaction  WHERE VoucherId='" + voucherId + "'";
                rdBuilder.Append(builderSql);
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _voucherService.QueryGLTransactionDetail(item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _voucherService.DeleteGLTransactionDetail(item.Id);
                    }
                    _voucherService.DeleteVoucherDetail(item.Id);
                }

                foreach (var item in expenseBookingApprovalHistoryRepository)
                {
                    _expenseBookingApprovalHistoryRepository.Delete(item.Id);
                }
                foreach (var item in expensesBookingDetail)
                {
                    _expenseBookingDetailRepository.Delete(item.Id);
                }
                if (expensesBooking.BeneficiaryType == "Vendor")
                {
                    if (invoiceTax != null)
                    {
                        foreach (var item in invoiceTax)
                        {
                            
                            var builderSqlInvoiceTax = @"UPDATE [TRN].InvoiceTax SET VoucherDetailId=NULL WHERE Id='" + item.Id + "'";
                            rdBuilder.Append(builderSqlInvoiceTax);
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        }
                    }
                    if (invoiceTDS.Count > 0)
                    {
                        foreach (var item in invoiceTDS)
                        {
                           
                            var builderSqlAdditionalTaxDetail = @"DELETE [TRN].AdditionalTaxDetail  WHERE AdditionalTaxId='" + item.Id + "'";
                            rdBuilder.Append(builderSqlAdditionalTaxDetail);
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                            _additionalTaxRepository.Delete(item.Id);
                        }
                    }
                    if (invoiceTax != null)
                    {
                        foreach (var item in invoiceTax)
                        {
                            var builderSqlAdditionalTaxDetail = @"DELETE [TRN].InvoiceTaxDetail  WHERE InvoiceTaxId='" + item.Id + "'";
                            rdBuilder.Append(builderSqlAdditionalTaxDetail);
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                            _invoiceTaxRepository.Delete(item.Id);
                        }
                    }
                    foreach (var item in invoiceDetail)
                    {
                        _invoiceDetailRepository.Delete(item.Id);
                    }

                    var builderSqlInvoice = @"UPDATE [TRN].Invoice SET ExpenseBookingId=NULL WHERE Id='" + invoice.Id + "'";
                    rdBuilder.Append(builderSqlInvoice);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                    _invoiceRepository.Delete(invoice.Id);
                }

                if (expensesBooking.BeneficiaryType == "Self")
                {
                    foreach (var item in employeePayableDetail)
                    {
                        _employeePayableDetailRepository.Delete(item.Id);
                    }
                    _employeePayableRepository.Delete(employeePayable.Id);
                }
                
                _expenseBookingRepository.Delete(expensesBooking.Id);
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

    }
}