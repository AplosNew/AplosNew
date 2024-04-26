using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Advances;
using Library.Model.Banks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Systems;
using Library.Model.Taxations;
using Library.Model.Vouchers;
using Library.Service.Advances;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;
using static Library.Service.Helpers.ReportUtility;

namespace Library.Service.SalaryDisbursement
{
    public class SalaryDisbursementService : ISalaryDisbursementService
    {
        #region Contractor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly IPKGeneratorService _pKGeneratorService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IRepositoryAsync<EmployeeTransactionTypeGL> _employeeTransactionTypeGLRepository;
        private readonly IRepositoryAsync<EmployeePayable> _employeePayableRepository;
        private readonly IRepositoryAsync<EmployeePayableDetail> _employeePayableDetailRepository;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IRepositoryAsync<TaxCode> _taxCodeRepository;
        private readonly IRepositoryAsync<TaxCodeGL> _taxCodeGLRepository;
        private readonly IRepositoryAsync<BankMaster> _bankMasterRepository;
        private readonly IRepositoryAsync<CashMaster> _cashMasterRepository;
        private readonly IRepositoryAsync<EmployeeSubsequentTransaction> _employeeSubsequentTransactionRepository;
        private readonly IRepositoryAsync<AdvanceWriteOff> _advanceWriteOffRepository;
        private readonly IRepositoryAsync<AdvanceWriteOffDetail> _advanceWriteOffDetailRepository;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;
        private readonly IRepositoryAsync<GLTransactionDetail> _gLTransactionDetailRepository;
        private readonly IAdvanceService _advanceService;

        public SalaryDisbursementService(
              IRepositoryAsync<EmployeePayable> employeePayableRepository
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IPKGeneratorService pkGeneratorService
            , IVoucherService voucherService
            , ICompanyTaxYearService companyTaxYearService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IRepositoryAsync<EmployeeTransactionTypeGL> employeeTransactionTypeGLRepository
            , IRepositoryAsync<EmployeePayableDetail> employeePayableDetailRepository
            , ICompanyFiscalYearService companyFiscalYearService
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<TaxCode> taxCodeRepository
            , IRepositoryAsync<TaxCodeGL> taxCodeGLRepository
            , IRepositoryAsync<BankMaster> bankMasterRepository
            , IRepositoryAsync<CashMaster> cashMasterRepository
            , IRepositoryAsync<EmployeeSubsequentTransaction> employeeSubsequentTransactionRepository
            , IRepositoryAsync<AdvanceWriteOff> advanceWriteOffRepository
            , IRepositoryAsync<AdvanceWriteOffDetail> advanceWriteOffDetailRepository
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository
            , IRepositoryAsync<GLTransactionDetail> gLTransactionDetailRepository
            , IAdvanceService advanceService
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
            _companyTaxYearService = companyTaxYearService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _employeePayableRepository = employeePayableRepository;
            _employeePayableDetailRepository = employeePayableDetailRepository;
            _employeeTransactionTypeGLRepository = employeeTransactionTypeGLRepository;
            _companyFiscalYearService = companyFiscalYearService;
            _pKGeneratorService = pkGeneratorService;
            _invoiceTaxService = invoiceTaxService;
            _taxCodeGLRepository = taxCodeGLRepository;
            _taxCodeRepository = taxCodeRepository;
            _bankMasterRepository = bankMasterRepository;
            _cashMasterRepository = cashMasterRepository;
            _employeeSubsequentTransactionRepository = employeeSubsequentTransactionRepository;
            _advanceWriteOffRepository = advanceWriteOffRepository;
            _advanceWriteOffDetailRepository = advanceWriteOffDetailRepository;
            _advanceService = advanceService;
            _voucherRepository = voucherRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _gLTransactionDetailRepository = gLTransactionDetailRepository;
        }

        #endregion Contractor


        public GridModel GetSalaryPayableVoucherList(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT V.Id PayableVoucherId, V.VoucherDate, V.PostingDate, V.DocRefNo, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId, C.Code AS CurrencyCode
                                    , VD.DrAmount, V.VoucherNo, V.IsPark, V.Narration,[Month]=case when sl.MonthNo=1 then 'January'
                                    when sl.MonthNo=2 then 'February'
                                    when sl.MonthNo=3 then 'March'
                                    when sl.MonthNo=4 then 'April'
                                    when sl.MonthNo=5 then 'May'
                                    when sl.MonthNo=6 then 'June'
                                    when sl.MonthNo=7 then 'July'
                                    when sl.MonthNo=8 then 'August'
                                    when sl.MonthNo=9 then 'September'
                                    when sl.MonthNo=10 then 'October'
                                    when sl.MonthNo=11 then 'November'
                                    when sl.MonthNo=12 then 'December' end,sl.MonthNo ,sl.YearNo
                                    FROM TRN.[Voucher] AS V
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 GROUP BY VD.VoucherId
                                    ) AS VD ON VD.VoucherId=V.Id
									left join (select distinct PayableVoucherId,MonthNo,YearNo from dbo.SalaryLock) sl on sl.PayableVoucherId=v.Id
                                    WHERE  V.Archive=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + "'AND V.CompanyId='" + identity.CompanyId + "' AND V.PlantId='" + identity.PlantId + "' AND V.SourceType='" + SourceType.SalaryPayable + @"'
                                    AND sl.MonthNo<>'' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex);
            }
        }


        private string GetEmployeeSubsequentTransactionPK()
        {
            return _pKGeneratorService.GetAutoNumber("EmployeeSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private AdvanceWriteOff InsertAdvanceWriteOff(AdvanceWriteOff advanceWriteOff)
        {
            advanceWriteOff.Id = _pKGeneratorService.GetAutoNumber(nameof(AdvanceWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
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
                Archive = false,
                VoucherId = voucherVM.VoucherId
            });
        }

        private void InsertAdvanceWriteOffDetail(AdvanceWriteOff advanceWriteOff, AdvanceWriteOffDetail advanceWriteOffDetail, int currentId)
        {
            advanceWriteOffDetail.Id = _pKGeneratorService.MakePK(advanceWriteOff.Id, currentId, 2);
            advanceWriteOffDetail.AddedBy = advanceWriteOff.AddedBy;
            advanceWriteOffDetail.AddedDate = advanceWriteOff.AddedDate;
            advanceWriteOffDetail.AddedFromIP = advanceWriteOff.AddedFromIP;
            advanceWriteOffDetail.AdvanceWriteOffId = advanceWriteOff.Id;
            advanceWriteOffDetail.Archive = advanceWriteOff.Archive;
            _advanceWriteOffDetailRepository.Insert(advanceWriteOffDetail);
        }
        private bool CheckAdvanceWriteOff(IEnumerable<VoucherDetailViewModel> list)
        {
            AccountCommonExtensionService accountCommonExtensionService = new AccountCommonExtensionService();

            bool isAdvance = false;
            foreach (var item in list)
            {
                var advance = _advanceService.Find(item.AdvanceId);
                var advancesalaryAdvance = accountCommonExtensionService.GetEmployeeSalaryAdvane(item.EmployeeSalaryAdvanceId);
                if (advance != null)
                {
                    isAdvance = true;
                    break;
                }
                else if (advancesalaryAdvance.Count > 0)
                {
                    isAdvance = true;
                    break;
                }
                else
                {
                    isAdvance = false;
                }
            }
            return isAdvance;
        }
        public string ParkSalaryPayable(VoucherViewModel voucherVM, string yearNo, string monthNo, string monthName
            , IEnumerable<VoucherDetailViewModel> directJVList, IEnumerable<VoucherDetailViewModel> inDirectJVList
            , IEnumerable<VoucherDetailViewModel> directSalaryLockList, IEnumerable<VoucherDetailViewModel> indirectSalaryLockList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService accountCommonExtensionService = new AccountCommonExtensionService();
                accountCommonExtensionService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                accountCommonExtensionService.CheckingFiscalYearPeriod(voucherVM);
                accountCommonExtensionService.CheckingTaxYearPeriod(voucherVM);
                var directVoucherId = "";
                var InDirectVoucherId = "";
                var currentVoucherDetailId = 0;
                voucherVM.CurrencyId = companyCurrencyId;
                voucherVM.CompanyCurrencyRate = 1;

                var directVoucherData = voucherVM;
                var InDirectVoucherData = voucherVM;

                //**************Insert Direct Salary JV****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directJVList != null)
                {
                    directVoucherData.DocRefNo = "D" + voucherVM.DocRefNo;
                    directVoucherData.Narration = "Direct Salary for the month of " + monthName + " " + yearNo;
                    var voucherdirect = _voucherService.InsertVoucher(directVoucherData);
                    directVoucherId = voucherdirect.Id;


                    foreach (var directVoucherDetailVM in directJVList)
                    {
                        currentVoucherDetailId++;
                        if (directVoucherDetailVM.SalaryHeadCategory != "Advance" && directVoucherDetailVM.SalaryHeadCategory != "Interest Deduction")
                        {
                            var directVoucherDetailDr = _voucherService.InsertVoucherDetail(voucherdirect, new VoucherDetail
                            {
                                GLGeneralInfoId = directVoucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = directVoucherDetailVM.BudgetMasterId,
                                ActivityId = directVoucherDetailVM.ActivityId,
                                DrAmount = directVoucherDetailVM.DrAmount,
                                CrAmount = directVoucherDetailVM.CrAmount,
                                TrnNature = directVoucherDetailVM.SalaryHead,
                                SalaryHeadId = directVoucherDetailVM.SalaryHeadId,
                                SalaryType = directVoucherDetailVM.SalaryType,
                                AccountsGroupId = directVoucherDetailVM.AccountsGroupId
                            }, currentVoucherDetailId);

                            // INSERT INTO VoucherDetailCurrency
                            _voucherService.InsertVoucherDetailCompanyCurrency(directVoucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = directVoucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(directVoucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.DrAmount,
                                CrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.CrAmount
                            });
                        }
                        else if (directVoucherDetailVM.SalaryHeadCategory == "Advance")
                        {
                            var currentAdvanceWriteOffDetailId = 0;
                            if (directSalaryLockList != null)
                            {
                                var directdata = voucherVM;
                                directdata.VoucherId = directVoucherId;
                                directdata.PartyType = "Employee";
                                directdata.Amount = directSalaryLockList.Where(r => r.SalaryHeadCategory == "Advance" && r.ActivityId == directVoucherDetailVM.ActivityId).Sum(r => r.Amount);
                                bool isAdvance = CheckAdvanceWriteOff(directSalaryLockList.Where(r => r.SalaryHeadCategory == "Advance"  && r.ActivityId == directVoucherDetailVM.ActivityId));
                                if (isAdvance)
                                {
                                    var advanceWriteOff = InsertAdvanceWriteOff(directdata);
                                    decimal directAmount = 0;
                                    foreach (var item in directSalaryLockList.Where(r => r.SalaryHeadCategory == "Advance" && r.ActivityId == directVoucherDetailVM.ActivityId))
                                    {

                                        var advance = _advanceService.Find(item.AdvanceId);
                                        var advancesalaryAdvance = accountCommonExtensionService.GetEmployeeSalaryAdvane(item.EmployeeSalaryAdvanceId);

                                        if (advancesalaryAdvance == null && advance!=null && advance.EmployeeId==item.EmployeeId && item.IsOrderSpecific == false)
                                        {


                                            advance.WrittenOffAmount += item.Amount;
                                            advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                                            advance.UpdatedBy = advanceWriteOff.AddedBy;
                                            advance.UpdatedDate = advanceWriteOff.AddedDate;
                                            advance.UpdatedFromIP = advanceWriteOff.AddedFromIP;
                                            _advanceService.Update(advance);

                                            var advanceDetail = _advanceService.FindAdvanceDetail(item.AdvanceDetailId);
                                            if (null == advanceDetail)
                                                throw new CustomException("Advance detail not found!");


                                            currentAdvanceWriteOffDetailId++;
                                            var advanceWriteOffDetail = new AdvanceWriteOffDetail
                                            {
                                                CompanyId = advanceDetail.CompanyId,
                                                PlantId = advanceDetail.PlantId,
                                                AdvanceId = item.AdvanceId,
                                                AdvanceDetailId = advanceDetail.Id,
                                                GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                                                BudgetMasterId = advanceDetail.BudgetMasterId,
                                                ActivityId = advanceDetail.ActivityId,
                                                CurrencyId = advanceWriteOff.CurrencyId,
                                                PartyType = advanceDetail.PartyType,
                                                Amount = item.Amount,
                                                EmployeeId = item.EmployeeId
                                            };
                                            InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                                            advanceWriteOff.Amount += item.Amount;
                                            advanceDetail.WrittenOffAmount += item.Amount;

                                            if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                                                throw new CustomException($"{advanceWriteOff.SettlementType} amount cannot exceed the balance advance amount.");

                                            advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                                            advanceDetail.UpdatedBy = advance.AddedBy;
                                            advanceDetail.UpdatedDate = advance.AddedDate;
                                            advanceDetail.UpdatedFromIP = advance.AddedFromIP;
                                            _advanceService.UpdateAdvanceDetail(advanceDetail);

                                            currentVoucherDetailId++;
                                            // INSERT INTO VoucherDetail Debit or Credit
                                            var directVoucherDetailDr = _voucherService.InsertVoucherDetail(voucherdirect, new VoucherDetail
                                            {
                                                GLGeneralInfoId = directVoucherDetailVM.GLGeneralInfoId,
                                                BudgetMasterId = directVoucherDetailVM.BudgetMasterId,
                                                ActivityId = directVoucherDetailVM.ActivityId,
                                                DrAmount = directVoucherDetailVM.DrAmount,
                                                CrAmount = item.Amount,
                                                EmployeeId = item.EmployeeId,
                                                TrnNature = directVoucherDetailVM.SalaryHead,
                                                AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                                                PartyType = "Employee",
                                                SalaryHeadId = directVoucherDetailVM.SalaryHeadId,
                                                SalaryType = directVoucherDetailVM.SalaryType,
                                                AccountsGroupId = directVoucherDetailVM.AccountsGroupId
                                            }, currentVoucherDetailId);
                                            directAmount += directVoucherDetailDr.CrAmount;
                                            // INSERT INTO VoucherDetailCurrency
                                            _voucherService.InsertVoucherDetailCompanyCurrency(directVoucherDetailDr, new VoucherDetailCurrency
                                            {
                                                ParallelCurrencyId = companyCurrencyId,
                                                FromCurrencyId = directVoucherDetailDr.CurrencyId,
                                                ToCurrencyId = companyCurrencyId,
                                                ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(directVoucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                                DrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.DrAmount,
                                                CrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.CrAmount
                                            });

                                            var EmployeeSubsequentAdvancedirect = new EmployeeSubsequentTransaction
                                            {
                                                CompanyGroupId = voucherVM.CompanyGroupId,
                                                CompanyId = voucherVM.CompanyId,
                                                PlantId = voucherVM.PlantId,
                                                EntityId = voucherVM.EntityId,
                                                VoucherTypeId = voucherVM.VoucherTypeId,
                                                AdvanceId = null,
                                                EmployeeId = item.EmployeeId,
                                                EmployeeTransactionTypeId = item.EmployeeTransactionTypeId,
                                                AdvanceWriteOffId = advanceWriteOff.Id,
                                                EmployeePayableWriteOffId = null,
                                                EmployeePayableId = null,
                                                PartyType = "Employee",
                                                CurrencyId = companyCurrencyId,
                                                Amount = item.Amount,
                                                VoucherDate = voucherVM.VoucherDate,
                                                PostingDate = voucherVM.PostingDate,
                                                DocDate = voucherVM.DocDate,
                                                DocRefNo = voucherVM.DocRefNo,
                                                JournalType = AdvanceType.Salary.ToString(),
                                                TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                                                Narration = voucherVM.Narration,
                                                SourceType = SourceType.SalaryPayable.ToString(),
                                                IsPark = voucherVM.IsPark,
                                                Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                                                VoucherId = directVoucherId,
                                                VoucherDetailId = directVoucherDetailDr.Id,
                                                PaymentSource = voucherVM.PaymentSource,
                                            };
                                            AuditService.AddedLog(EmployeeSubsequentAdvancedirect);
                                            _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvancedirect);
                                            item.IsOrderSpecific = true;

                                        }
                                        else if(advancesalaryAdvance.Count>0 && advancesalaryAdvance["EmployeeId"].ToString()== item.EmployeeId && item.IsOrderSpecific == false)
                                        {
                                            currentAdvanceWriteOffDetailId++;
                                            var advanceWriteOffDetail = new AdvanceWriteOffDetail
                                            {
                                                CompanyId = advancesalaryAdvance["CompanyId"].ToString(),
                                                PlantId = advancesalaryAdvance["PlantId"].ToString(),
                                                AdvanceId = item.AdvanceId,
                                                AdvanceDetailId = null,
                                                GLGeneralInfoId = advancesalaryAdvance["GLGeneralInfoId"].ToString(),
                                                BudgetMasterId = advancesalaryAdvance["BudgetMasterId"].ToString(),
                                                ActivityId = advancesalaryAdvance["ActivityId"].ToString(),
                                                CurrencyId = advanceWriteOff.CurrencyId,
                                                PartyType = "Employee",
                                                Amount = item.Amount,
                                                EmployeeId = item.EmployeeId,
                                                EmployeeSalaryAdvanceId = item.EmployeeSalaryAdvanceId
                                            };
                                            InsertAdvanceWriteOffDetail(advanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                                            currentVoucherDetailId++;
                                            // INSERT INTO VoucherDetail Debit or Credit
                                            var directVoucherDetailDr = _voucherService.InsertVoucherDetail(voucherdirect, new VoucherDetail
                                            {
                                                GLGeneralInfoId = directVoucherDetailVM.GLGeneralInfoId,
                                                BudgetMasterId = directVoucherDetailVM.BudgetMasterId,
                                                ActivityId = directVoucherDetailVM.ActivityId,
                                                DrAmount = directVoucherDetailVM.DrAmount,
                                                CrAmount = item.Amount,
                                                EmployeeId = item.EmployeeId,
                                                TrnNature = directVoucherDetailVM.SalaryHead,
                                                AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                                                PartyType = "Employee",
                                                SalaryHeadId = directVoucherDetailVM.SalaryHeadId,
                                                SalaryType = directVoucherDetailVM.SalaryType,
                                                AccountsGroupId = directVoucherDetailVM.AccountsGroupId
                                            }, currentVoucherDetailId);
                                            directAmount += directVoucherDetailDr.CrAmount;
                                            // INSERT INTO VoucherDetailCurrency
                                            _voucherService.InsertVoucherDetailCompanyCurrency(directVoucherDetailDr, new VoucherDetailCurrency
                                            {
                                                ParallelCurrencyId = companyCurrencyId,
                                                FromCurrencyId = directVoucherDetailDr.CurrencyId,
                                                ToCurrencyId = companyCurrencyId,
                                                ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(directVoucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                                DrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.DrAmount,
                                                CrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.CrAmount
                                            });

                                            var EmployeeSubsequentAdvancedirect = new EmployeeSubsequentTransaction
                                            {
                                                CompanyGroupId = voucherVM.CompanyGroupId,
                                                CompanyId = voucherVM.CompanyId,
                                                PlantId = voucherVM.PlantId,
                                                EntityId = voucherVM.EntityId,
                                                VoucherTypeId = voucherVM.VoucherTypeId,
                                                AdvanceId = null,
                                                EmployeeId = item.EmployeeId,
                                                EmployeeTransactionTypeId = item.EmployeeTransactionTypeId,
                                                AdvanceWriteOffId = advanceWriteOff.Id,
                                                EmployeePayableWriteOffId = null,
                                                EmployeePayableId = null,
                                                PartyType = "Employee",
                                                CurrencyId = companyCurrencyId,
                                                Amount = item.Amount,
                                                VoucherDate = voucherVM.VoucherDate,
                                                PostingDate = voucherVM.PostingDate,
                                                DocDate = voucherVM.DocDate,
                                                DocRefNo = voucherVM.DocRefNo,
                                                JournalType = AdvanceType.Salary.ToString(),
                                                TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                                                Narration = voucherVM.Narration,
                                                SourceType = SourceType.SalaryPayable.ToString(),
                                                IsPark = voucherVM.IsPark,
                                                Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                                                VoucherId = directVoucherId,
                                                VoucherDetailId = directVoucherDetailDr.Id,
                                                PaymentSource = voucherVM.PaymentSource,
                                            };
                                            AuditService.AddedLog(EmployeeSubsequentAdvancedirect);
                                            _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvancedirect);
                                            item.IsOrderSpecific = true;

                                        }
                                    }
                                    if (directVoucherDetailVM.CrAmount - directAmount > 0)
                                    {
                                        currentVoucherDetailId++;
                                        var directVoucherDetailDrAdd = _voucherService.InsertVoucherDetail(voucherdirect, new VoucherDetail
                                        {
                                            GLGeneralInfoId = directVoucherDetailVM.GLGeneralInfoId,
                                            BudgetMasterId = directVoucherDetailVM.BudgetMasterId,
                                            ActivityId = directVoucherDetailVM.ActivityId,
                                            DrAmount = directVoucherDetailVM.DrAmount,
                                            CrAmount = directVoucherDetailVM.CrAmount - directAmount,
                                            TrnNature = directVoucherDetailVM.SalaryHead,
                                            SalaryHeadId = directVoucherDetailVM.SalaryHeadId,
                                            SalaryType = directVoucherDetailVM.SalaryType,
                                            AccountsGroupId=directVoucherDetailVM.AccountsGroupId
                                        }, currentVoucherDetailId);

                                        // INSERT INTO VoucherDetailCurrency
                                        _voucherService.InsertVoucherDetailCompanyCurrency(directVoucherDetailDrAdd, new VoucherDetailCurrency
                                        {
                                            ParallelCurrencyId = companyCurrencyId,
                                            FromCurrencyId = directVoucherDetailDrAdd.CurrencyId,
                                            ToCurrencyId = companyCurrencyId,
                                            ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(directVoucherDetailDrAdd.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                            DrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDrAdd.DrAmount,
                                            CrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDrAdd.CrAmount
                                        });
                                    }


                                }
                                else
                                {
                                    currentVoucherDetailId++;
                                    var directVoucherDetailDr = _voucherService.InsertVoucherDetail(voucherdirect, new VoucherDetail
                                    {
                                        GLGeneralInfoId = directVoucherDetailVM.GLGeneralInfoId,
                                        BudgetMasterId = directVoucherDetailVM.BudgetMasterId,
                                        ActivityId = directVoucherDetailVM.ActivityId,
                                        DrAmount = directVoucherDetailVM.DrAmount,
                                        CrAmount = directVoucherDetailVM.CrAmount,
                                        TrnNature = directVoucherDetailVM.SalaryHead,
                                        SalaryHeadId = directVoucherDetailVM.SalaryHeadId,
                                        SalaryType = directVoucherDetailVM.SalaryType,
                                        AccountsGroupId = directVoucherDetailVM.AccountsGroupId
                                    }, currentVoucherDetailId);

                                    // INSERT INTO VoucherDetailCurrency
                                    _voucherService.InsertVoucherDetailCompanyCurrency(directVoucherDetailDr, new VoucherDetailCurrency
                                    {
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = directVoucherDetailDr.CurrencyId,
                                        ToCurrencyId = companyCurrencyId,
                                        ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(directVoucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                        DrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.DrAmount,
                                        CrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.CrAmount
                                    });
                                }

                            }
                        }
                        else if (directVoucherDetailVM.SalaryHeadCategory == "Interest Deduction")
                        {
                            if (directSalaryLockList != null)
                            {
                                foreach (var item in directSalaryLockList.Where(r => r.SalaryHeadCategory == "Interest Deduction"))
                                {

                                    currentVoucherDetailId++;
                                    // INSERT INTO VoucherDetail Debit or Credit
                                    var directVoucherDetailDr = _voucherService.InsertVoucherDetail(voucherdirect, new VoucherDetail
                                    {
                                        GLGeneralInfoId = directVoucherDetailVM.GLGeneralInfoId,
                                        BudgetMasterId = directVoucherDetailVM.BudgetMasterId,
                                        ActivityId = directVoucherDetailVM.ActivityId,
                                        DrAmount = 0,
                                        CrAmount = item.ProfitAmount,
                                        EmployeeId = item.EmployeeId,
                                        TrnNature = directVoucherDetailVM.SalaryHead,
                                        PartyType = "Employee",
                                        SalaryHeadId = directVoucherDetailVM.SalaryHeadId,
                                        SalaryType = directVoucherDetailVM.SalaryType,
                                        AccountsGroupId = directVoucherDetailVM.AccountsGroupId
                                    }, currentVoucherDetailId);

                                    // INSERT INTO VoucherDetailCurrency
                                    _voucherService.InsertVoucherDetailCompanyCurrency(directVoucherDetailDr, new VoucherDetailCurrency
                                    {
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = directVoucherDetailDr.CurrencyId,
                                        ToCurrencyId = companyCurrencyId,
                                        ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(directVoucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                        DrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.DrAmount,
                                        CrAmount = directVoucherData.CompanyCurrencyRate * directVoucherDetailDr.CrAmount
                                    });

                                    //var EmployeeSubsequentAdvancedirect = new EmployeeSubsequentTransaction
                                    //{
                                    //    CompanyGroupId = voucherVM.CompanyGroupId,
                                    //    CompanyId = voucherVM.CompanyId,
                                    //    PlantId = voucherVM.PlantId,
                                    //    EntityId = voucherVM.EntityId,
                                    //    VoucherTypeId = voucherVM.VoucherTypeId,
                                    //    AdvanceId = null,
                                    //    EmployeeId = item.EmployeeId,
                                    //    EmployeeTransactionTypeId = item.EmployeeTransactionTypeId,
                                    //    AdvanceWriteOffId = null,
                                    //    EmployeePayableWriteOffId = null,
                                    //    EmployeePayableId = null,
                                    //    PartyType = "Employee",
                                    //    CurrencyId = companyCurrencyId,
                                    //    Amount = item.Amount,
                                    //    VoucherDate = voucherVM.VoucherDate,
                                    //    PostingDate = voucherVM.PostingDate,
                                    //    DocDate = voucherVM.DocDate,
                                    //    DocRefNo = voucherVM.DocRefNo,
                                    //    JournalType = AdvanceType.Salary.ToString(),
                                    //    TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                                    //    Narration = voucherVM.Narration,
                                    //    SourceType = SourceType.SalaryPayable.ToString(),
                                    //    IsPark = voucherVM.IsPark,
                                    //    Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                                    //    VoucherId = directVoucherId,
                                    //    VoucherDetailId = directVoucherDetailDr.Id,
                                    //    PaymentSource = voucherVM.PaymentSource,
                                    //};
                                    //AuditService.AddedLog(EmployeeSubsequentAdvancedirect);
                                    //_employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvancedirect);

                                }
                            }
                        }

                    }


                }

                if (inDirectJVList != null)
                {
                    InDirectVoucherData.DocRefNo = "I" + voucherVM.DocRefNo;
                    InDirectVoucherData.Narration = "InDirect Salary for the month of " + monthName + " " + yearNo;
                    var voucherI = _voucherService.InsertVoucher(InDirectVoucherData);
                    InDirectVoucherId = voucherI.Id;

                    foreach (var voucherDetailVM in inDirectJVList)
                    {
                        currentVoucherDetailId++;
                        if (voucherDetailVM.SalaryHeadCategory != "Advance" && voucherDetailVM.SalaryHeadCategory != "Interest Deduction")
                        {

                            var voucherDetailDr = _voucherService.InsertVoucherDetail(voucherI, new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.DrAmount,
                                CrAmount = voucherDetailVM.CrAmount,
                                TrnNature = voucherDetailVM.SalaryHead,
                                SalaryHeadId = voucherDetailVM.SalaryHeadId,
                                SalaryType = voucherDetailVM.SalaryType,
                                AccountsGroupId = voucherDetailVM.AccountsGroupId
                            }, currentVoucherDetailId);

                            // INSERT INTO VoucherDetailCurrency
                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDetailDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = InDirectVoucherData.CompanyCurrencyRate,
                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                DrAmount = InDirectVoucherData.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                                CrAmount = InDirectVoucherData.CompanyCurrencyRate * voucherDetailDr.CrAmount
                            });
                        }
                        else if (voucherDetailVM.SalaryHeadCategory == "Advance")
                        {
                            var currentAdvanceWriteOffDetailId = 0;
                            if (indirectSalaryLockList != null)
                            {
                                var indirectdata = voucherVM;
                                indirectdata.VoucherId = InDirectVoucherId;
                                indirectdata.PartyType = "Employee";
                                indirectdata.Amount = indirectSalaryLockList.Where(r => r.SalaryHeadCategory == "Advance" && r.ActivityId == voucherDetailVM.ActivityId).Sum(r => r.Amount);
                                bool isAdvance = CheckAdvanceWriteOff(indirectSalaryLockList.Where(r => r.SalaryHeadCategory == "Advance" && r.ActivityId == voucherDetailVM.ActivityId));
                                if (isAdvance)
                                {
                                    var indirectadvanceWriteOff = InsertAdvanceWriteOff(indirectdata);
                                    decimal indirectAdvanceAmountTemp = 0;
                                    foreach (var item in indirectSalaryLockList.Where(r => r.SalaryHeadCategory == "Advance" && r.ActivityId == voucherDetailVM.ActivityId))
                                    {

                                        var advance = _advanceService.Find(item.AdvanceId);
                                        var advancesalaryAdvance = accountCommonExtensionService.GetEmployeeSalaryAdvane(item.EmployeeSalaryAdvanceId);

                                        if (advancesalaryAdvance == null && advance != null && advance.EmployeeId == item.EmployeeId && item.IsOrderSpecific == false)
                                        {
                                            advance.WrittenOffAmount += item.Amount;
                                            advance.IsWrittenOff = advance.Amount == advance.WrittenOffAmount;
                                            advance.UpdatedBy = indirectadvanceWriteOff.AddedBy;
                                            advance.UpdatedDate = indirectadvanceWriteOff.AddedDate;
                                            advance.UpdatedFromIP = indirectadvanceWriteOff.AddedFromIP;
                                            _advanceService.Update(advance);

                                            var advanceDetail = _advanceService.FindAdvanceDetail(item.AdvanceDetailId);
                                            if (null == advanceDetail)
                                                throw new CustomException("Advance detail not found!");


                                            currentAdvanceWriteOffDetailId++;
                                            var advanceWriteOffDetail = new AdvanceWriteOffDetail
                                            {
                                                CompanyId = advanceDetail.CompanyId,
                                                PlantId = advanceDetail.PlantId,
                                                AdvanceId = advanceDetail.AdvanceId,
                                                AdvanceDetailId = advanceDetail.Id,
                                                GLGeneralInfoId = advanceDetail.GLGeneralInfoId,
                                                BudgetMasterId = advanceDetail.BudgetMasterId,
                                                ActivityId = advanceDetail.ActivityId,
                                                CurrencyId = indirectadvanceWriteOff.CurrencyId,
                                                PartyType = advanceDetail.PartyType,
                                                Amount = item.Amount
                                            };
                                            InsertAdvanceWriteOffDetail(indirectadvanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                                            indirectadvanceWriteOff.Amount += item.Amount;
                                            advanceDetail.WrittenOffAmount += item.Amount;

                                            if (advanceDetail.Amount < advanceDetail.WrittenOffAmount)
                                                throw new CustomException($"{indirectadvanceWriteOff.SettlementType} amount cannot exceed the balance advance amount.");

                                            advanceDetail.IsWrittenOff = advanceDetail.Amount == advanceDetail.WrittenOffAmount;
                                            advanceDetail.UpdatedBy = advance.AddedBy;
                                            advanceDetail.UpdatedDate = advance.AddedDate;
                                            advanceDetail.UpdatedFromIP = advance.AddedFromIP;
                                            _advanceService.UpdateAdvanceDetail(advanceDetail);
                                            currentVoucherDetailId++;
                                            // INSERT INTO VoucherDetail Debit or Credit
                                            var VoucherDetailDr = _voucherService.InsertVoucherDetail(voucherI, new VoucherDetail
                                            {
                                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                                ActivityId = voucherDetailVM.ActivityId,
                                                DrAmount = voucherDetailVM.DrAmount,
                                                CrAmount = item.Amount,
                                                EmployeeId = item.EmployeeId,
                                                TrnNature = voucherDetailVM.SalaryHead,
                                                AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                                                PartyType = "Employee",
                                                SalaryHeadId = voucherDetailVM.SalaryHeadId,
                                                SalaryType = voucherDetailVM.SalaryType,
                                                AccountsGroupId = voucherDetailVM.AccountsGroupId

                                            }, currentVoucherDetailId);
                                            indirectAdvanceAmountTemp += VoucherDetailDr.CrAmount;

                                            // INSERT INTO VoucherDetailCurrency
                                            _voucherService.InsertVoucherDetailCompanyCurrency(VoucherDetailDr, new VoucherDetailCurrency
                                            {
                                                ParallelCurrencyId = companyCurrencyId,
                                                FromCurrencyId = VoucherDetailDr.CurrencyId,
                                                ToCurrencyId = companyCurrencyId,
                                                ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(VoucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                                DrAmount = directVoucherData.CompanyCurrencyRate * VoucherDetailDr.DrAmount,
                                                CrAmount = directVoucherData.CompanyCurrencyRate * VoucherDetailDr.CrAmount
                                            });

                                            var EmployeeSubsequentAdvancedirect = new EmployeeSubsequentTransaction
                                            {
                                                CompanyGroupId = voucherVM.CompanyGroupId,
                                                CompanyId = voucherVM.CompanyId,
                                                PlantId = voucherVM.PlantId,
                                                EntityId = voucherVM.EntityId,
                                                VoucherTypeId = voucherVM.VoucherTypeId,
                                                AdvanceId = null,
                                                EmployeeId = item.EmployeeId,
                                                EmployeeTransactionTypeId = item.EmployeeTransactionTypeId,
                                                AdvanceWriteOffId = indirectadvanceWriteOff.Id,
                                                EmployeePayableWriteOffId = null,
                                                EmployeePayableId = null,
                                                PartyType = "Employee",
                                                CurrencyId = companyCurrencyId,
                                                Amount = item.Amount,
                                                VoucherDate = voucherVM.VoucherDate,
                                                PostingDate = voucherVM.PostingDate,
                                                DocDate = voucherVM.DocDate,
                                                DocRefNo = voucherVM.DocRefNo,
                                                JournalType = AdvanceType.Salary.ToString(),
                                                TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                                                Narration = voucherVM.Narration,
                                                SourceType = SourceType.SalaryPayable.ToString(),
                                                IsPark = voucherVM.IsPark,
                                                Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                                                VoucherId = InDirectVoucherId,
                                                VoucherDetailId = VoucherDetailDr.Id,
                                                PaymentSource = voucherVM.PaymentSource,
                                            };
                                            AuditService.AddedLog(EmployeeSubsequentAdvancedirect);
                                            _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvancedirect);
                                            item.IsOrderSpecific = true;
                                        }
                                        else if (advancesalaryAdvance.Count > 0 && advancesalaryAdvance["EmployeeId"].ToString()==item.EmployeeId && item.IsOrderSpecific == false)
                                        {
                                           

                                            currentAdvanceWriteOffDetailId++;
                                            var advanceWriteOffDetail = new AdvanceWriteOffDetail
                                            {
                                                CompanyId = advancesalaryAdvance["CompanyId"].ToString(),
                                                PlantId = advancesalaryAdvance["PlantId"].ToString(),
                                                AdvanceId = item.AdvanceId,
                                                AdvanceDetailId = null,
                                                GLGeneralInfoId = advancesalaryAdvance["GLGeneralInfoId"].ToString(),
                                                BudgetMasterId = advancesalaryAdvance["BudgetMasterId"].ToString(),
                                                ActivityId = advancesalaryAdvance["ActivityId"].ToString(),
                                                CurrencyId = indirectadvanceWriteOff.CurrencyId,
                                                PartyType = "Employee",
                                                Amount = item.Amount,
                                                EmployeeId = item.EmployeeId,
                                                EmployeeSalaryAdvanceId = item.EmployeeSalaryAdvanceId
                                            };
                                            InsertAdvanceWriteOffDetail(indirectadvanceWriteOff, advanceWriteOffDetail, currentAdvanceWriteOffDetailId);

                                           
                                            currentVoucherDetailId++;
                                            // INSERT INTO VoucherDetail Debit or Credit
                                            var VoucherDetailDr = _voucherService.InsertVoucherDetail(voucherI, new VoucherDetail
                                            {
                                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                                ActivityId = voucherDetailVM.ActivityId,
                                                DrAmount = voucherDetailVM.DrAmount,
                                                CrAmount = item.Amount,
                                                EmployeeId = item.EmployeeId,
                                                TrnNature = voucherDetailVM.SalaryHead,
                                                AdvanceWriteOffDetailId = advanceWriteOffDetail.Id,
                                                PartyType = "Employee",
                                                SalaryHeadId = voucherDetailVM.SalaryHeadId,
                                                SalaryType = voucherDetailVM.SalaryType,
                                                AccountsGroupId = voucherDetailVM.AccountsGroupId
                                            }, currentVoucherDetailId);
                                            indirectAdvanceAmountTemp += VoucherDetailDr.CrAmount;

                                            // INSERT INTO VoucherDetailCurrency
                                            _voucherService.InsertVoucherDetailCompanyCurrency(VoucherDetailDr, new VoucherDetailCurrency
                                            {
                                                ParallelCurrencyId = companyCurrencyId,
                                                FromCurrencyId = VoucherDetailDr.CurrencyId,
                                                ToCurrencyId = companyCurrencyId,
                                                ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                                ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(VoucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                                DrAmount = directVoucherData.CompanyCurrencyRate * VoucherDetailDr.DrAmount,
                                                CrAmount = directVoucherData.CompanyCurrencyRate * VoucherDetailDr.CrAmount
                                            });

                                            var EmployeeSubsequentAdvancedirect = new EmployeeSubsequentTransaction
                                            {
                                                CompanyGroupId = voucherVM.CompanyGroupId,
                                                CompanyId = voucherVM.CompanyId,
                                                PlantId = voucherVM.PlantId,
                                                EntityId = voucherVM.EntityId,
                                                VoucherTypeId = voucherVM.VoucherTypeId,
                                                AdvanceId = null,
                                                EmployeeId = item.EmployeeId,
                                                EmployeeTransactionTypeId = item.EmployeeTransactionTypeId,
                                                AdvanceWriteOffId = indirectadvanceWriteOff.Id,
                                                EmployeePayableWriteOffId = null,
                                                EmployeePayableId = null,
                                                PartyType = "Employee",
                                                CurrencyId = companyCurrencyId,
                                                Amount = item.Amount,
                                                VoucherDate = voucherVM.VoucherDate,
                                                PostingDate = voucherVM.PostingDate,
                                                DocDate = voucherVM.DocDate,
                                                DocRefNo = voucherVM.DocRefNo,
                                                JournalType = AdvanceType.Salary.ToString(),
                                                TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                                                Narration = voucherVM.Narration,
                                                SourceType = SourceType.SalaryPayable.ToString(),
                                                IsPark = voucherVM.IsPark,
                                                Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                                                VoucherId = InDirectVoucherId,
                                                VoucherDetailId = VoucherDetailDr.Id,
                                                PaymentSource = voucherVM.PaymentSource,
                                            };
                                            AuditService.AddedLog(EmployeeSubsequentAdvancedirect);
                                            _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvancedirect);
                                            item.IsOrderSpecific = true;
                                        }
                                        
                                    }

                                    if (voucherDetailVM.CrAmount - indirectAdvanceAmountTemp > 0)
                                    {
                                        currentVoucherDetailId++;
                                        // If no advance found against employee then only voucher will save. Advance SetOff will not occur.
                                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucherI, new VoucherDetail
                                        {
                                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                            ActivityId = voucherDetailVM.ActivityId,
                                            DrAmount = voucherDetailVM.DrAmount,
                                            CrAmount = voucherDetailVM.CrAmount - indirectAdvanceAmountTemp,
                                            TrnNature = voucherDetailVM.SalaryHead,
                                            SalaryHeadId = voucherDetailVM.SalaryHeadId,
                                            SalaryType = voucherDetailVM.SalaryType,
                                            AccountsGroupId = voucherDetailVM.AccountsGroupId
                                        }, currentVoucherDetailId);

                                        // INSERT INTO VoucherDetailCurrency
                                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                                        {
                                            ParallelCurrencyId = companyCurrencyId,
                                            FromCurrencyId = voucherDetailDr.CurrencyId,
                                            ToCurrencyId = companyCurrencyId,
                                            ToCurrencyRate = InDirectVoucherData.CompanyCurrencyRate,
                                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                            DrAmount = InDirectVoucherData.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                                            CrAmount = InDirectVoucherData.CompanyCurrencyRate * voucherDetailDr.CrAmount
                                        });
                                    }
                                }
                                else
                                {

                                    var voucherDetailDr = _voucherService.InsertVoucherDetail(voucherI, new VoucherDetail
                                    {
                                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                        ActivityId = voucherDetailVM.ActivityId,
                                        DrAmount = voucherDetailVM.DrAmount,
                                        CrAmount = voucherDetailVM.CrAmount,
                                        TrnNature = voucherDetailVM.SalaryHead,
                                        SalaryHeadId = voucherDetailVM.SalaryHeadId,
                                        SalaryType = voucherDetailVM.SalaryType,
                                        AccountsGroupId = voucherDetailVM.AccountsGroupId
                                    }, currentVoucherDetailId);

                                    // INSERT INTO VoucherDetailCurrency
                                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                                    {
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = voucherDetailDr.CurrencyId,
                                        ToCurrencyId = companyCurrencyId,
                                        ToCurrencyRate = InDirectVoucherData.CompanyCurrencyRate,
                                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                        DrAmount = InDirectVoucherData.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                                        CrAmount = InDirectVoucherData.CompanyCurrencyRate * voucherDetailDr.CrAmount
                                    });
                                }
                            }
                        }
                        else if (voucherDetailVM.SalaryHeadCategory != "Interest Deduction")
                        {
                            if (indirectSalaryLockList != null)
                            {
                                foreach (var item in indirectSalaryLockList.Where(r => r.SalaryHeadCategory != "Interest Deduction"))
                                {

                                    // INSERT INTO VoucherDetail Debit or Credit
                                    var VoucherDetailDr = _voucherService.InsertVoucherDetail(voucherI, new VoucherDetail
                                    {
                                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                        ActivityId = voucherDetailVM.ActivityId,
                                        DrAmount = voucherDetailVM.DrAmount,
                                        CrAmount = item.ProfitAmount,
                                        TrnNature = voucherDetailVM.SalaryHead,
                                        SalaryHeadId = voucherDetailVM.SalaryHeadId,
                                        SalaryType = voucherDetailVM.SalaryType,
                                        AccountsGroupId = voucherDetailVM.AccountsGroupId
                                    }, currentVoucherDetailId);

                                    currentVoucherDetailId++;
                                    // INSERT INTO VoucherDetailCurrency
                                    _voucherService.InsertVoucherDetailCompanyCurrency(VoucherDetailDr, new VoucherDetailCurrency
                                    {
                                        ParallelCurrencyId = companyCurrencyId,
                                        FromCurrencyId = VoucherDetailDr.CurrencyId,
                                        ToCurrencyId = companyCurrencyId,
                                        ToCurrencyRate = directVoucherData.CompanyCurrencyRate,
                                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(VoucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                                        DrAmount = directVoucherData.CompanyCurrencyRate * VoucherDetailDr.DrAmount,
                                        CrAmount = directVoucherData.CompanyCurrencyRate * VoucherDetailDr.CrAmount
                                    });

                                    //var EmployeeSubsequentAdvancedirect = new EmployeeSubsequentTransaction
                                    //{
                                    //    CompanyGroupId = voucherVM.CompanyGroupId,
                                    //    CompanyId = voucherVM.CompanyId,
                                    //    PlantId = voucherVM.PlantId,
                                    //    EntityId = voucherVM.EntityId,
                                    //    VoucherTypeId = voucherVM.VoucherTypeId,
                                    //    AdvanceId = null,
                                    //    EmployeeId = item.EmployeeId,
                                    //    EmployeeTransactionTypeId = item.EmployeeTransactionTypeId,
                                    //    AdvanceWriteOffId = indirectadvanceWriteOff.Id,
                                    //    EmployeePayableWriteOffId = null,
                                    //    EmployeePayableId = null,
                                    //    PartyType = "Employee",
                                    //    CurrencyId = companyCurrencyId,
                                    //    Amount = item.Amount,
                                    //    VoucherDate = voucherVM.VoucherDate,
                                    //    PostingDate = voucherVM.PostingDate,
                                    //    DocDate = voucherVM.DocDate,
                                    //    DocRefNo = voucherVM.DocRefNo,
                                    //    JournalType = AdvanceType.Salary.ToString(),
                                    //    TransactionType = EmployeeSubsequentTranEnum.Advance.ToString(),
                                    //    Narration = voucherVM.Narration,
                                    //    SourceType = SourceType.SalaryPayable.ToString(),
                                    //    IsPark = voucherVM.IsPark,
                                    //    Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                                    //    VoucherId = InDirectVoucherId,
                                    //    VoucherDetailId = VoucherDetailDr.Id,
                                    //    PaymentSource = voucherVM.PaymentSource,
                                    //};
                                    //AuditService.AddedLog(EmployeeSubsequentAdvancedirect);
                                    //_employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvancedirect);

                                }
                            }
                        }
                    }
                }


                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                _unitOfWork.BeginTransaction();
                flag = true;
                if (directVoucherId != null && directVoucherId != "")
                {
                    var direct = new System.Text.StringBuilder();
                    var directsql = "";

                    directsql = @"update [dbo].[SalaryLock] set PayableVoucherId='" + directVoucherId + @"' where Id in (
                                    select sl.Id     from [dbo].[SalaryLock] sl 
                         left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
						 left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                                    left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						            left join ORG.Position PO on PO.Id=MPB.PositionId
                                    where sl.YearNo='" + yearNo + "' and sl.MonthNo='" + monthNo + @"'   
                                    and  PO.DirectManpowerCost=1 AND  spd.PlantId='" + voucherVM.PlantId + "' and sl.PayableVoucherId IS NULL)";
                    direct.Append(directsql);
                    _sqlRepository.ExecuteSqlCommand(direct.ToString());



                }

                if (InDirectVoucherId != null)
                {
                    var inDirect = new System.Text.StringBuilder();
                    var inDirectsql = "";

                    inDirectsql = @"update [dbo].[SalaryLock] set PayableVoucherId='" + InDirectVoucherId + @"' where Id in (
                                    select sl.Id     from [dbo].[SalaryLock] sl 
                         left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
						 left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                                    left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						            left join ORG.Position PO on PO.Id=MPB.PositionId
                                    where sl.YearNo='" + yearNo + "' and sl.MonthNo='" + monthNo + @"'   
                                    and  PO.DirectManpowerCost=0 AND  spd.PlantId='" + voucherVM.PlantId + "' and sl.PayableVoucherId IS NULL)";
                    inDirect.Append(inDirectsql);
                    _sqlRepository.ExecuteSqlCommand(inDirect.ToString());

                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return "Save Successful";
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

        private DataTable GetDirectSalaryLockData(string yearNo, string monthNo)
        {
            var cmdText = @"select * from  [dbo].[SalaryLock] sl 
                                    left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						            left join ORG.Position PO on PO.Id=MPB.PositionId
                                    where sl.YearNo='" + yearNo + "' and sl.MonthNo='" + monthNo + @"'   
                                    and  PO.DirectManpowerCost=0 and sl.PayableVoucherId IS NULL";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public string ParkFinalSettlementDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);
                var directVoucherId = "";
                voucherVM.DocDate = voucherVM.PostingDate;
                voucherVM.CurrencyId = companyCurrencyId;
                voucherVM.CompanyCurrencyRate = 1;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                //**************Insert Direct Salary JV****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directJVList != null)
                {
                    voucherVM.DocRefNo = disbursementAdviceId;
                    voucherVM.Narration = "";
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    directVoucherId = voucher.Id;
                    var currentVoucherDetailId = 0;

                    foreach (var voucherDetailVM in directJVList)
                    { 
                        if(voucherDetailVM.OtherName == "Bank/Cash")
                        {
                            if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                            {
                                // INSERT INTO VoucherDetail (Bank or cash side Cr)
                                var voucherDetailCr = new VoucherDetail
                                {
                                    Narration = voucher.Narration,
                                    CrAmount = voucherDetailVM.CrAmount,
                                    PaymentSource = voucherVM.PaymentSource
                                };
                                totalAmountCr += voucherDetailCr.CrAmount;

                                var glTransactionDetail = new GLTransactionDetail
                                {
                                    SourceType = voucherDetailCr.PaymentSource,
                                    BankMasterId = voucherVM.BankMasterId,
                                    CashMasterId = voucherVM.CashMasterId
                                };

                                if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                                {
                                    var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                                    voucherDetailCr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                                    voucherDetailCr.BudgetMasterId = bankMaster.BudgetMasterId;
                                    voucherDetailCr.ActivityId = bankMaster.ActivityId;
                                    voucherDetailCr.BankMasterId = bankMaster.Id;
                                    voucherDetailCr.PartyType = PartyType.Bank.ToString();
                                    if (bankMaster.CurrencyId == voucherVM.CurrencyId)
                                        glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                                    else
                                        glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                                }
                                else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                                {
                                    var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                                    voucherDetailCr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                                    voucherDetailCr.BudgetMasterId = cashMaster.BudgetMasterId;
                                    voucherDetailCr.ActivityId = cashMaster.ActivityId;
                                    voucherDetailCr.CashMasterId = cashMaster.Id;
                                    voucherDetailCr.PartyType = PartyType.Cash.ToString();
                                    if (cashMaster.CurrencyId == voucherVM.CurrencyId)
                                        glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                                    else
                                        glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                                }
                                else
                                    throw new CustomException("Bank or Cash Id not found!");
                                // INSRT INTO GLTransactionDetail

                                currentVoucherDetailId++;
                                _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                                _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                                //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                                // INSERT INTO VoucherDetailCurrency
                                var voucherDetailCurrencyCr = new VoucherDetailCurrency();
                                voucherDetailCurrencyCr.ParallelCurrencyId = companyCurrencyId;
                                voucherDetailCurrencyCr.FromCurrencyId = voucherDetailCr.CurrencyId;
                                voucherDetailCurrencyCr.ToCurrencyId = companyCurrencyId;
                                voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                                voucherDetailCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                                voucherDetailCurrencyCr.CrAmount = (voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate);

                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                            }
                        }
                        else
                        {
                            currentVoucherDetailId++;
                            var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.DrAmount,
                                CrAmount = voucherDetailVM.CrAmount,
                            }, currentVoucherDetailId);

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
                                DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                                CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.CrAmount,
                            });
                        }
                        
                    }

                   

                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                //**************update salary lock VoucherPayableId Direct and InDirect Salary ****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directVoucherId != null)
                {
                    var direct = new System.Text.StringBuilder();
                    var directsql = "";
                    directsql = @"update [dbo].[SalaryLock] set DisbursementVoucherId='" + directVoucherId + @"' where EmployeeFinalSettlementId='" + disbursementAdviceId + @"' ";
                    direct.Append(directsql);
                    directsql = @"
                        UPDATE  [dbo].[EmployeeFinalSettlement] SET DisbursementVoucherId='" + directVoucherId + @"' WHERE Id='" + disbursementAdviceId + @"'";
                    direct.Append(directsql);
                    _sqlRepository.ExecuteSqlCommand(direct.ToString());

                }
                flag = false;
                _unitOfWork.Commit();
                return "Save Successful";
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

        public string ParkSalaryPayableDisbursement(VoucherViewModel voucherVM, string yearNo, string monthNo, string monthName, string pMode, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, string empSystemIds)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);
                var directVoucherId = "";
                voucherVM.DocDate = voucherVM.PostingDate;
                voucherVM.CurrencyId = companyCurrencyId;
                voucherVM.CompanyCurrencyRate = 1;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                //**************Insert Direct Salary JV****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directJVList != null)
                {
                    voucherVM.DocRefNo = disbursementAdviceId;
                    voucherVM.Narration = "Salary disbursement for the month of " + monthName + " " + yearNo;
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    directVoucherId = voucher.Id;
                    var currentVoucherDetailId = 0;

                    foreach (var voucherDetailVM in directJVList)
                    {
                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.CrAmount,
                        }, currentVoucherDetailId);

                        totalAmountDr += voucherDetailDr.DrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                        });
                    }

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        // INSERT INTO VoucherDetail (Bank or cash side Dr)
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = directJVList.Sum(r => r.CrAmount),
                            PaymentSource = voucherVM.PaymentSource
                        };

                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                        {
                            var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                            voucherDetailCr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                            voucherDetailCr.BudgetMasterId = bankMaster.BudgetMasterId;
                            voucherDetailCr.ActivityId = bankMaster.ActivityId;
                            voucherDetailCr.BankMasterId = bankMaster.Id;
                            voucherDetailCr.PartyType = PartyType.Bank.ToString();
                            if (bankMaster.CurrencyId == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }
                        else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                        {
                            var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                            voucherDetailCr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                            voucherDetailCr.BudgetMasterId = cashMaster.BudgetMasterId;
                            voucherDetailCr.ActivityId = cashMaster.ActivityId;
                            voucherDetailCr.CashMasterId = cashMaster.Id;
                            voucherDetailCr.PartyType = PartyType.Cash.ToString();
                            if (cashMaster.CurrencyId == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = new VoucherDetailCurrency();
                        voucherDetailCurrencyCr.ParallelCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.FromCurrencyId = voucherDetailCr.CurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                        voucherDetailCurrencyCr.CrAmount = (voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                    }

                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                //**************update salary lock VoucherPayableId Direct and InDirect Salary ****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directVoucherId != null)
                {
                    var direct = new System.Text.StringBuilder();
                    var directsql = "";
                        directsql = @"update [dbo].[SalaryLock] set DisbursementVoucherId='" + directVoucherId + @"' where Id in (
                        select sl.Id
                        from [dbo].[SalaryLock] sl 
                        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                        left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                        left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        where sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL and sl.IsDisbursed=1 
                        and sl.EmpSystemId IN (" + empSystemIds + @") and sl.DisbursementAdviceId='" + disbursementAdviceId + @"'
                        and spc.DisbusmentAmount!=0  
						and ISNULL(sh.SalaryHead, '')  in ('Net Pay'))";
                    direct.Append(directsql);
                    directsql = @"
                        UPDATE  [dbo].[DisbursementAdvice] SET Status=CASE WHEN (select COUNT(sl.Id)Id
                        from [dbo].[SalaryLock] sl 
                        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                        left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                        left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        where sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL and sl.IsDisbursed=1 
                        and sl.DisbursementAdviceId='" + disbursementAdviceId + @"'
                        and spc.DisbusmentAmount!=0 and ISNULL(sh.SalaryHead, '')  in ('Net Pay'))>0
                        THEN 'InProgress' ELSE 'Close' END WHERE Id='" + disbursementAdviceId + @"'";
                    direct.Append(directsql);
                    _sqlRepository.ExecuteSqlCommand(direct.ToString());

                }
                flag = false;
                _unitOfWork.Commit();
                return "Save Successful";
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
        public string ParkGoodWorkPaymentAdviseDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, string goodWorkPaymentAdviseDetailIds)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);
                var directVoucherId = "";
                voucherVM.DocDate = voucherVM.PostingDate;
                voucherVM.CurrencyId = companyCurrencyId;
                voucherVM.CompanyCurrencyRate = 1;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                //**************Insert Direct Salary JV****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directJVList != null)
                {
                    voucherVM.DocRefNo = disbursementAdviceId;
                    voucherVM.Narration = "";
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    directVoucherId = voucher.Id;
                    var currentVoucherDetailId = 0;

                    foreach (var voucherDetailVM in directJVList)
                    {
                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.DrAmount,
                        }, currentVoucherDetailId);

                        totalAmountDr += voucherDetailDr.DrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                        });
                    }

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        // INSERT INTO VoucherDetail (Bank or cash side Dr)
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = directJVList.Sum(r => r.DrAmount),
                            PaymentSource = voucherVM.PaymentSource
                        };

                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                        {
                            var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                            voucherDetailCr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                            voucherDetailCr.BudgetMasterId = bankMaster.BudgetMasterId;
                            voucherDetailCr.ActivityId = bankMaster.ActivityId;
                            voucherDetailCr.BankMasterId = bankMaster.Id;
                            voucherDetailCr.PartyType = PartyType.Bank.ToString();
                            if (bankMaster.CurrencyId == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }
                        else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                        {
                            var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                            voucherDetailCr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                            voucherDetailCr.BudgetMasterId = cashMaster.BudgetMasterId;
                            voucherDetailCr.ActivityId = cashMaster.ActivityId;
                            voucherDetailCr.CashMasterId = cashMaster.Id;
                            voucherDetailCr.PartyType = PartyType.Cash.ToString();
                            if (cashMaster.CurrencyId == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = new VoucherDetailCurrency();
                        voucherDetailCurrencyCr.ParallelCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.FromCurrencyId = voucherDetailCr.CurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                        voucherDetailCurrencyCr.CrAmount = (voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                    }

                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                //**************update salary lock VoucherPayableId Direct and InDirect Salary ****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directVoucherId != null)
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    var direct = new System.Text.StringBuilder();
                    var directsql = "";
                    directsql = @"UPDATE [dbo].[GoodWorkPaymentAdvisedetail] SET IsDisburse=1,PaymentsDate=GETDATE(), DisbursementVoucherId='" + directVoucherId + @"', PaymentsById='" + identity.EmployeeId + "'  where Id in (" + goodWorkPaymentAdviseDetailIds + @") ";
                    direct.Append(directsql);
                    directsql = @"
                        UPDATE [dbo].[GoodWorkPaymentAdvise] SET PaymentsStatus=CASE WHEN (SELECT COUNT(Id)Id FROM [dbo].[GoodWorkPaymentAdvisedetail]   where PaymentAdviseId= '" + disbursementAdviceId + "' AND ISNULL(IsCheck,0)=1 AND ISNULL(IsDisburse,0)=0 AND DisbursementVoucherId IS NULL)>0 THEN 'Partial Payments' ELSE 'Full Payments' END  where Id='" + disbursementAdviceId + @"' ";
                    direct.Append(directsql);
                    _sqlRepository.ExecuteSqlCommand(direct.ToString());

                }
                flag = false;
                _unitOfWork.Commit();
                return "Save Successful";
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
        public string ParkEmployeeMultipleAdvanceDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, string goodWorkPaymentAdviseDetailIds)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);
                var directVoucherId = "";
                voucherVM.DocDate = voucherVM.PostingDate;
                voucherVM.CurrencyId = companyCurrencyId;
                voucherVM.CompanyCurrencyRate = 1;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                //**************Insert Direct Salary JV****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directJVList != null)
                {
                    voucherVM.DocRefNo = disbursementAdviceId;
                    voucherVM.Narration = "";
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    directVoucherId = voucher.Id;
                    var currentVoucherDetailId = 0;

                    foreach (var voucherDetailVM in directJVList)
                    {
                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.DrAmount,
                        }, currentVoucherDetailId);

                        totalAmountDr += voucherDetailDr.DrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                        });
                    }

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        // INSERT INTO VoucherDetail (Bank or cash side Dr)
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = directJVList.Sum(r => r.DrAmount),
                            PaymentSource = voucherVM.PaymentSource
                        };

                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                        {
                            var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                            voucherDetailCr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                            voucherDetailCr.BudgetMasterId = bankMaster.BudgetMasterId;
                            voucherDetailCr.ActivityId = bankMaster.ActivityId;
                            voucherDetailCr.BankMasterId = bankMaster.Id;
                            voucherDetailCr.PartyType = PartyType.Bank.ToString();
                            if (bankMaster.CurrencyId == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }
                        else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                        {
                            var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                            voucherDetailCr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                            voucherDetailCr.BudgetMasterId = cashMaster.BudgetMasterId;
                            voucherDetailCr.ActivityId = cashMaster.ActivityId;
                            voucherDetailCr.CashMasterId = cashMaster.Id;
                            voucherDetailCr.PartyType = PartyType.Cash.ToString();
                            if (cashMaster.CurrencyId == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = new VoucherDetailCurrency();
                        voucherDetailCurrencyCr.ParallelCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.FromCurrencyId = voucherDetailCr.CurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                        voucherDetailCurrencyCr.CrAmount = (voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                    }

                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                //**************update salary lock VoucherPayableId Direct and InDirect Salary ****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directVoucherId != null)
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    var direct = new System.Text.StringBuilder();
                    var directsql = "";
                    directsql = @"UPDATE [dbo].[WorkerAdvanceDetail] SET IsDisburse=1,PaymentsDate=GETDATE(), DisbursementVoucherId='" + directVoucherId + @"', PaymentsById='" + identity.EmployeeId + "'  where Id in (" + goodWorkPaymentAdviseDetailIds + @") ";
                    direct.Append(directsql);
                    directsql = @"
                        UPDATE [dbo].[WorkerAdvance] SET PaymentsStatus=CASE WHEN (SELECT COUNT(Id)Id FROM [dbo].[WorkerAdvanceDetail]   where WorkerAdvanceId= '" + disbursementAdviceId + "' AND ISNULL(IsCheck,0)=1 AND ISNULL(IsDisburse,0)=0 AND DisbursementVoucherId IS NULL)>0 THEN 'Partial Payments' ELSE 'Full Payments' END  where Id='" + disbursementAdviceId + @"' ";
                    direct.Append(directsql);
                    _sqlRepository.ExecuteSqlCommand(direct.ToString());

                }
                flag = false;
                _unitOfWork.Commit();
                return "Save Successful";
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
        public string SaveBonusDisbursementPosting(VoucherViewModel voucherVM, string fromDate, string toDate, string pMode, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, string empSystemIds)
        {
            var flag = false;
            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);
                var directVoucherId = "";
                voucherVM.DocDate = voucherVM.PostingDate;
                voucherVM.CurrencyId = companyCurrencyId;
                voucherVM.CompanyCurrencyRate = 1;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                //**************Insert Direct Bonus JV****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directJVList != null)
                {
                    voucherVM.DocRefNo = disbursementAdviceId;
                    voucherVM.Narration = "Bonus disbursement from " + fromDate + " to " + toDate;
                    var voucher = _voucherService.InsertVoucher(voucherVM);
                    directVoucherId = voucher.Id;
                    var currentVoucherDetailId = 0;

                    foreach (var voucherDetailVM in directJVList)
                    {
                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.CrAmount,
                        }, currentVoucherDetailId);

                        totalAmountDr += voucherDetailDr.DrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailDr.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount,
                        });
                    }

                    if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() || voucherVM.PaymentSource == PaymentSource.Cash.ToString())
                    {
                        // INSERT INTO VoucherDetail (Bank or cash side Dr)
                        var voucherDetailCr = new VoucherDetail
                        {
                            Narration = voucher.Narration,
                            CrAmount = directJVList.Sum(r => r.CrAmount),
                            PaymentSource = voucherVM.PaymentSource
                        };

                        totalAmountCr += voucherDetailCr.CrAmount;

                        var glTransactionDetail = new GLTransactionDetail
                        {
                            SourceType = voucherDetailCr.PaymentSource,
                            BankMasterId = voucherVM.BankMasterId,
                            CashMasterId = voucherVM.CashMasterId
                        };

                        if (!string.IsNullOrEmpty(voucherVM.BankMasterId))
                        {
                            var bankMaster = _bankMasterRepository.Find(voucherVM.BankMasterId);
                            voucherDetailCr.GLGeneralInfoId = bankMaster.GLGeneralInfoId;
                            voucherDetailCr.BudgetMasterId = bankMaster.BudgetMasterId;
                            voucherDetailCr.ActivityId = bankMaster.ActivityId;
                            voucherDetailCr.BankMasterId = bankMaster.Id;
                            voucherDetailCr.PartyType = PartyType.Bank.ToString();
                            if (bankMaster.CurrencyId == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }
                        else if (!string.IsNullOrEmpty(voucherVM.CashMasterId))
                        {
                            var cashMaster = _cashMasterRepository.Find(voucherVM.CashMasterId);
                            voucherDetailCr.GLGeneralInfoId = cashMaster.GLGeneralInfoId;
                            voucherDetailCr.BudgetMasterId = cashMaster.BudgetMasterId;
                            voucherDetailCr.ActivityId = cashMaster.ActivityId;
                            voucherDetailCr.CashMasterId = cashMaster.Id;
                            voucherDetailCr.PartyType = PartyType.Cash.ToString();
                            if (cashMaster.CurrencyId == voucherVM.CurrencyId)
                                glTransactionDetail.CrAmount = voucherDetailCr.CrAmount;
                            else
                                glTransactionDetail.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;
                        }
                        else
                            throw new CustomException("Bank or Cash Id not found!");
                        // INSRT INTO GLTransactionDetail

                        currentVoucherDetailId++;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailCr, currentVoucherDetailId);
                        _voucherService.InsertGLTransactionDetail(voucherDetailCr, glTransactionDetail);

                        //glTransactionDetail.CrAmount = totalCurrencyAmountDr voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

                        // INSERT INTO VoucherDetailCurrency
                        var voucherDetailCurrencyCr = new VoucherDetailCurrency();
                        voucherDetailCurrencyCr.ParallelCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.FromCurrencyId = voucherDetailCr.CurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyId = companyCurrencyId;
                        voucherDetailCurrencyCr.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrencyCr.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                        voucherDetailCurrencyCr.CrAmount = (voucherDetailCr.CrAmount * voucherVM.CompanyCurrencyRate);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailCr, voucherDetailCurrencyCr);
                    }

                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                //**************update salary lock VoucherPayableId Direct and InDirect Salary ****************
                _unitOfWork.BeginTransaction();
                flag = true;
                if (directVoucherId != null)
                {
                    var direct = new System.Text.StringBuilder();
                    var directsql = "";
                    directsql = @"update [dbo].[SalaryLock] set BonusDisbursementVoucherId='" + directVoucherId + @"' where Id in (
                        select sl.Id
                        from [dbo].[SalaryLock] sl 
                        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                        left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                        left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        where sl.PayableVoucherId<>'' AND sl.BonusDisbursementVoucherId IS NULL and sl.IsBonusDisbursed=1  
                        and sl.Id IN (" + empSystemIds + @") and sl.BonusDisbursementAdviceId='" + disbursementAdviceId + @"'
                        and spc.DisbusmentAmount!=0  
						and ISNULL(SH.HeadCategory, '')  in ('Monthly Bonus Retain') )";
                    direct.Append(directsql);
                    directsql = @"
                        UPDATE  [dbo].[BonusDisbursementAdvice] SET Status=CASE WHEN (select COUNT(sl.Id)Id
                        from [dbo].[SalaryLock] sl 
                        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
                        left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID and sl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                        left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
						left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join hkp.Designation d on d.Id=spd.DesignationId
						left join hkp.Bank b on spd.BankSystemID=b.Id
						left join trn.Voucher v on v.Id=sl.PayableVoucherId
                        where sl.PayableVoucherId<>'' AND sl.BonusDisbursementVoucherId IS NULL and sl.IsBonusDisbursed=1 
                        and sl.BonusDisbursementAdviceId='" + disbursementAdviceId + @"'
                        and spc.DisbusmentAmount!=0 and ISNULL(SH.HeadCategory, '')  in ('Monthly Bonus Retain') )>0
                        THEN 'InProgress' ELSE 'Close' END WHERE Id='" + disbursementAdviceId + @"'";
                    direct.Append(directsql);
                    _sqlRepository.ExecuteSqlCommand(direct.ToString());

                }
                flag = false;
                _unitOfWork.Commit();
                return "Save Successful";
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

        public void DeleteBonusDisbursementVoucher(string plantId, string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var direct = new System.Text.StringBuilder();
                var directsql = "";

                directsql = @"UPDATE DA SET DA.Status='InProgress' FROM [dbo].[BonusDisbursementAdvice] DA
						      INNER JOIN [dbo].[SalaryLock] sl ON sl.BonusDisbursementAdviceId=DA.Id
                              where sl.BonusDisbursementVoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                directsql = @"
                                update [dbo].[SalaryLock] set BonusDisbursementVoucherId=NULL where Id in (
                                select sl.Id     from [dbo].[SalaryLock] sl 
						        left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
						        left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                                left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						        left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						        left join ORG.Position PO on PO.Id=MPB.PositionId
                                where spd.PlantId='" + plantId + @"'  and sl.BonusDisbursementVoucherId='" + voucherId + @"' )";
                direct.Append(directsql);
                _sqlRepository.ExecuteSqlCommand(direct.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");


                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _gLTransactionDetailRepository.Query(r => r.VoucherDetailId == item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _gLTransactionDetailRepository.Delete(item.Id);
                    }
                    _voucherDetailRepository.Delete(item.Id);
                }

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
        public void DeleteSalaryPayable(string plantId, string voucherId, string monthNo, string yearNo)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var direct = new System.Text.StringBuilder();

                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var emoloyeeSubsequentTransaction = _employeeSubsequentTransactionRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var advancewriteOff = _advanceWriteOffRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }
                if (emoloyeeSubsequentTransaction.Count > 0)
                {
                    foreach (var est in emoloyeeSubsequentTransaction)
                    {
                        _employeeSubsequentTransactionRepository.Delete(est.Id);
                    }
                }
                foreach (var item in voucherdetail)
                {
                    _voucherDetailRepository.Delete(item.Id);
                }

                if (advancewriteOff.Count > 0)
                {
                    foreach (var awo in advancewriteOff)
                    {
                        var advancewriteOffdetail = _advanceWriteOffDetailRepository.Query(r => r.AdvanceWriteOffId == awo.Id).Select().ToList();
                        if (advancewriteOffdetail.Count > 0)
                        {

                            foreach (var awd in advancewriteOffdetail)
                            {
                                if (awd.AdvanceDetailId != null)
                                {
                                    var advanceDetail = _advanceService.FindAdvanceDetail(awd.AdvanceDetailId);
                                    if (null == advanceDetail)
                                        throw new CustomException("Advance Detail Id not found!");
                                    advanceDetail.WrittenOffAmount -= awd.Amount;
                                    advanceDetail.IsWrittenOff = false;
                                    advanceDetail.UpdatedBy = voucher.UpdatedBy;
                                    advanceDetail.UpdatedDate = voucher.UpdatedDate;
                                    advanceDetail.UpdatedFromIP = voucher.UpdatedFromIP;
                                    _advanceService.UpdateAdvanceDetail(advanceDetail);

                                    var advance = _advanceService.Find(awd.AdvanceId);
                                    advance.WrittenOffAmount -= awd.Amount;
                                    advance.IsWrittenOff = false;
                                    advance.UpdatedBy = voucher.AddedBy;
                                    advance.UpdatedDate = voucher.AddedDate;
                                    advance.UpdatedFromIP = voucher.AddedFromIP;
                                    _advanceService.Update(advance);
                                }
                                

                                _advanceWriteOffDetailRepository.Delete(awd.Id);
                            }
                            
                        }
                        _advanceWriteOffRepository.Delete(awo.Id);
                    }
                }
                
               
                var directsql = "";

                directsql = @"update [dbo].[SalaryLock] set PayableVoucherId=NULL where Id in (
                        select sl.Id     from [dbo].[SalaryLock] sl 
						 left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
						 left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                                    left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						            left join ORG.Position PO on PO.Id=MPB.PositionId
                                    where sl.YearNo='" + yearNo + "' and sl.MonthNo='" + monthNo + "'   and spd.PlantId='" + plantId + @"'
                                     and sl.PayableVoucherId='" + voucherId + @"' )";
                direct.Append(directsql);
                _sqlRepository.ExecuteSqlCommand(direct.ToString());

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
        public void PostSalarydisbursement(string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
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
        public void DeleteSalaryDisbursementVoucher(string plantId, string voucherId, string monthNo, string yearNo)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var direct = new System.Text.StringBuilder();
                var directsql = "";

                directsql = @"UPDATE DA SET DA.Status='InProgress' FROM [dbo].[DisbursementAdvice] DA
						      INNER JOIN [dbo].[SalaryLock] sl ON sl.DisbursementAdviceId=DA.Id
                              where sl.YearNo='" + yearNo + "' and sl.MonthNo='" + monthNo + @"' and sl.Islocked=1 and sl.PayableVoucherId<>''
                              and sl.DisbursementVoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                directsql = @"
                              update [dbo].[SalaryLock] set DisbursementVoucherId=NULL where Id in (
                        select sl.Id     from [dbo].[SalaryLock] sl 
						 left join dbo.SalaryProcMaster spm on   spm.MonthNo=sl.MonthNo and spm.YearNo=sl.YearNo
						 left join dbo.SalaryProcessLogDetail spd on   spd.EmpSystemId=sl.EmpSystemId and spm.SystemID=spd.SalaryProcessId
                                    left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						            left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						            left join ORG.Position PO on PO.Id=MPB.PositionId
                                    where sl.YearNo='" + yearNo + "' and sl.MonthNo='" + monthNo + "'   and spd.PlantId='" + plantId + @"' and sl.Islocked=1 and sl.PayableVoucherId<>''
                                     and sl.DisbursementVoucherId='" + voucherId + @"' )";
                direct.Append(directsql);
                _sqlRepository.ExecuteSqlCommand(direct.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");


                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _gLTransactionDetailRepository.Query(r => r.VoucherDetailId == item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _gLTransactionDetailRepository.Delete(item.Id);
                    }
                    _voucherDetailRepository.Delete(item.Id);
                }

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

        public void DeleteGoodWorkPaymentAdviseDisbursement(string plantId, string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var direct = new System.Text.StringBuilder();
                var directsql = "";

                directsql = @"UPDATE  gwpa SET gwpa.PaymentsStatus=NULL
					from GoodWorkPaymentAdvise gwpa
					left join  GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
					WHERE  gwpad.DisbursementVoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                directsql = @"
                              UPDATE [dbo].[GoodWorkPaymentAdvisedetail] SET IsDisburse=NULL,PaymentsDate=NULL, DisbursementVoucherId=NULL, PaymentsById=NULL  where DisbursementVoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                _sqlRepository.ExecuteSqlCommand(direct.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");


                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _gLTransactionDetailRepository.Query(r => r.VoucherDetailId == item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _gLTransactionDetailRepository.Delete(item.Id);
                    }
                    _voucherDetailRepository.Delete(item.Id);
                }

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

        public void DeleteEmployeeMultipleAdvanceDisbursement(string plantId, string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var direct = new System.Text.StringBuilder();
                var directsql = "";

                directsql = @"UPDATE  gwpa SET gwpa.PaymentsStatus=NULL
					from WorkerAdvance gwpa
					left join  WorkerAdvanceDetail gwpad on gwpad.WorkerAdvanceId=gwpa.Id
					WHERE  gwpad.DisbursementVoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                directsql = @"
                              UPDATE [dbo].[WorkerAdvanceDetail] SET IsDisburse=NULL,PaymentsDate=NULL, DisbursementVoucherId=NULL, PaymentsById=NULL  where DisbursementVoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                _sqlRepository.ExecuteSqlCommand(direct.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");


                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _gLTransactionDetailRepository.Query(r => r.VoucherDetailId == item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _gLTransactionDetailRepository.Delete(item.Id);
                    }
                    _voucherDetailRepository.Delete(item.Id);
                }

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

        public void DeleteFinalSettlementDisbursementVoucher(string plantId, string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var direct = new System.Text.StringBuilder();
                var directsql = "";

                directsql = @"UPDATE [dbo].[EmployeeFinalSettlement] SET DisbursementVoucherId=NULL where DisbursementVoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                directsql = @"
                              update [dbo].[SalaryLock] set DisbursementVoucherId=NULL  where DisbursementVoucherId='" + voucherId + @"' ";
                direct.Append(directsql);
                _sqlRepository.ExecuteSqlCommand(direct.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");


                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == voucherId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    var glTransactionDetail = _gLTransactionDetailRepository.Query(r => r.VoucherDetailId == item.Id).Select().FirstOrDefault();
                    if (glTransactionDetail != null)
                    {
                        _gLTransactionDetailRepository.Delete(item.Id);
                    }
                    _voucherDetailRepository.Delete(item.Id);
                }

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


        #region Salary Payable in Voucher

        public void GetEmployeeInfoDetailSalaryLogWise(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef, string voucherId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";


                string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                salaryProcessId = "''";
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }
            }
            string wcEmpStatus = " AND (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage
                            FROM
                                    (
									SELECT DISTINCT E.SystemID EmpSystemId,ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID,  F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,EC.WorkingDaysInAMonth--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode
											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(spld.BankAccNo,'') BankAccNo
                                    ,ISNULL(spld.IFSCCode,'') IFSCCode
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost

                            			, sl.PayableVoucherId
                                     FROM  dbo.SalaryLock sl
									join EmployeeInformation E on sl.EmpSystemId=E.SystemId

                                          Left JOIN (
                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID

								 --left join dbo.SalaryLock sl on sl.MonthNo=m.MonthNo and sl.YearNo=m.YearNo and sl.PayableVoucherId='" + voucherId + @"'

                                    WHERE SlrProcMstSystemID IN(" + salaryProcessId + @") 
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessId + @") AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '202022' 
                         
									 			LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and SPLD.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id

												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
            --                                    (
            --                                    SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												--LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												--)EC ON EC.DesignationId=E.GivenDesignationId
												[HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId
											

                                      --Where SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      --WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        --WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        --AND MonthNo =   MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')   )   
									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"') AND MMDSA.PlantID = '" + plantId + @"' 
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"'  AND EmpBasic.PlantId ='" + plantId + @"' 
		                                                                                    and EmpBasic.PayableVoucherId='" + voucherId + @"'";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"and EmpBasic.EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSQL += @"Order by EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, out DataTable distinctSalaryHead)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            distinctSalaryHead = new DataTable("Tmp");
            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + @"') AND YearNo = Year('" + fromDate + @"')";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }


            try
            {
                strSQL = @"SELECT EmpSlr.*,PSH.Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID EmpSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
                                                    , sh.SalaryHead, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay

                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID



                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID= spc.SalaryHeadID


                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id

                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR

                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster


                                                                                                            WHERE SystemID IN(" + salaryProcessID + @")
																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode

                                                                                            AND SPC.PlantID = Exr.PlantID

                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        where isnull(SPC.SlrProcMstSystemID,'')  IN(" + salaryProcessID + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EmpSlr.PlantId = '" + plantId + @"'";

                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"AND EmpSlr.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";

                        }
                    }
                }
                catch (Exception)
                {
                }
                strSQL += "ORDER BY EmpSystemId ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);

                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "PartOfNetPay", "IsCTCComponent", "IsGrossComponent");
                distinctSalaryHead.DefaultView.Sort = "Sequence";
                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpSystemID"].ToString();
                }

                return dicBonus;


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function

        private void SetCellTextAttdn(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }

        private void SetCellTextNumber(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }

        private Dictionary<string, object> GetSalaryPayableSheetheader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        public IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWiseInVoucher(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, string voucherId)
        {
            #region Variable
            clsReport objRpt = null;

            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;

            var reportUtility = new ReportUtility();
            // var excelEngine = new ExcelEngine();
            //var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            // workbook.Version = ExcelVersion.Excel2013;
            // var sheet = workbook.Worksheets[0];
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;

                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                //sheet.Name = "Voucher";



                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo, voucherId);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                var header = GetSalaryPayableSheetheader(companyGroupId, companyId, plantId, voucherId, SourceType.SalaryPayable);

                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 1, "Voucher No");
                reportUtility.SetText(ref sheet1, xlsRow, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 4, "Entry Date");
                reportUtility.SetText(ref sheet1, xlsRow, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

                sheet1[reportUtility.GetColumnNameForXls(2) + xlsRow + ":" + reportUtility.GetColumnNameForXls(3) + xlsRow].Merge();

                xlsRow++;

                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 1, "Posting Date");
                reportUtility.SetText(ref sheet1, xlsRow, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 4, "DocDate");
                reportUtility.SetText(ref sheet1, xlsRow, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

                sheet1[reportUtility.GetColumnNameForXls(2) + xlsRow + ":" + reportUtility.GetColumnNameForXls(3) + xlsRow].Merge();

                xlsRow++;

                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 1, "Status");
                reportUtility.SetText(ref sheet1, xlsRow, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 4, "Doc Ref");
                reportUtility.SetText(ref sheet1, xlsRow, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

                sheet1[reportUtility.GetColumnNameForXls(2) + xlsRow + ":" + reportUtility.GetColumnNameForXls(3) + xlsRow].Merge();

                xlsRow++;

                //colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 1, "Narration");
                reportUtility.SetText(ref sheet1, xlsRow, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
                sheet1[reportUtility.GetColumnNameForXls(2) + xlsRow + ":" + reportUtility.GetColumnNameForXls(3) + xlsRow].Merge();

                #region Column Variables
                xlsRow++;
                xlsRow++;

                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, colBank = 0, cDMP = 0, colBankAccountNo = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                //SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                SetCellValue("IFSCCode", sheet1, xlsRow, ref xlsCol, out int colBankIFSCCode, 25);

                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                //SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out cDMP, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                endGenericColumn = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                //xlsCol += 1;
                ColGrs = ColTotalOTHR;
                // 9

                var _count_earning_head = 0;
                var _count_earning_ctchead = 0;
                var _count_deducting_head = 0;
                var _total_head_count = 0;

                Dictionary<string, SalaryHeadSequence> shtList = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                salList.AddRange(shtList.Values);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                npstruct = 0;
                if (shtList.Count > 0)
                {
                    xlsCol++;
                    npstruct = ColGrs + shtList.Count + 1;
                    sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                    //sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                    //sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                }

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, npstruct].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].VerticalAlignment = ExcelVAlign.VAlignCenter;
                endXlsCol = npstruct;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankName"].ToString()) == false)
                            sheet1.Range[xlsRow, colBank].Text = dtEmployees.Rows[i]["BankName"].ToString();
                        sheet1.Range[xlsRow, colBank].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBank].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankAccNo"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankAccountNo].Text = dtEmployees.Rows[i]["BankAccNo"].ToString();
                        sheet1.Range[xlsRow, colBankAccountNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankAccountNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IFSCCode"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankIFSCCode].Text = dtEmployees.Rows[i]["IFSCCode"].ToString();
                        sheet1.Range[xlsRow, colBankIFSCCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankIFSCCode].VerticalAlignment = ExcelVAlign.VAlignCenter;



                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5 "Section", "SubSection", 

                        #endregion
                        #region Attendance Data
                        //if (dtEmpAttdnInfo.Rows.Count > 0)
                        //{
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = 0.00;
                        // clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"]);
                        if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                        {
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

                            }
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());
                            }
                        }
                        else
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        }
                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));

                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        //}
                        #endregion


                        //var _total_head_count_body = 0;

                        #region ------------------------------------Salary Sheet----------------------------------
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            if (drSalaryHeadCollection.Count > 0)
                            {
                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                {
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                    {
                                        sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                        continue;
                                    }
                                    try
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                            {
                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                            }

                                            else
                                            {

                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            }

                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }

                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                //var strFileName = "EmpSalaryStrSheet-" + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + Convert.ToDateTime(fdateOfMonth).ToString("yyyy") + "-" + para.SalaryProcessId + ".xls";

                if (isTopSheet == true)
                {
                    #region Salary Summary
                    string filePath = HostingEnvironment.MapPath("~/") + "TempSalaeySummary.xlsx";
                    workbook.SaveAs(filePath);
                    workbook = application.Workbooks.Open(filePath);

                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Move(1);

                    #region PivotSheet1
                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    pivotSheet.Name = "Summary";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(1) + pivotSheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(npstruct) + (sheetEndXlsRow)];
                    IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Department Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    pivotTable2.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[ColSr - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    int pivotColumnCount = 0;
                    IPivotField fieldGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldGross = null;
                        pivotColumnCount++;
                        fieldGross = pivotTable2_1.Fields[npstruct - 1];
                        pivotTable2_1.DataFields.Add(fieldGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }
                    catch (Exception)
                    {

                    }

                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    int totalColumns = pivotTable2_1.RowFields.Count + pivotColumnCount;

                    int lastCloumn = totalColumns + 2;

                    #endregion
                    #region PivotTable2

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "Employee Category Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    //Add data field
                    IPivotField field = pivotTable.Fields[ColSr - 1];
                    pivotTable.DataFields.Add(field, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot2ColumnCount = 0;
                    IPivotField fieldGross2 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross2 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross2 = null;
                    pivot2ColumnCount++;
                    fieldGross2 = pivotTable.Fields[npstruct - 1];
                    pivotTable.DataFields.Add(fieldGross2, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion

                    #region PivotTable3

                    totalColumns += pivotTable.RowFields.Count + pivot2ColumnCount;


                    lastCloumn += totalColumns - 10;

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "EmployeeStatus ,Employee Category and Department Wise  Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    //Create "PivotTable1" with the cache at the specified range
                    IPivotTable pivotTable3 = pivotSheet.PivotTables.Add("PivotTable13", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable3.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable13_1 = pivotSheet.PivotTables["PivotTable13"];
                    pivotTable13_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable13_1.Options.ShowDrillIndicators = false;

                    pivotTable13_1.DisplayFieldCaptions = true;
                    pivotTable13_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;


                    //Add data field
                    IPivotField fields3 = pivotTable13_1.Fields[ColSr - 1];
                    pivotTable13_1.DataFields.Add(fields3, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot3ColumnCount = 0;
                    IPivotField fieldGross3 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross3 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot3ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }

                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross3 = null;
                    pivot2ColumnCount++;
                    fieldGross3 = pivotTable13_1.Fields[npstruct - 1];
                    pivotTable13_1.DataFields.Add(fieldGross3, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion
                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;
                    pivotSheet.IsDisplayZeros = false;

                    pivotSheet.UsedRange.WrapText = false;

                    #endregion
                    #endregion

                    workbook.ActiveSheetIndex = 0;
                }

                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }

        private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out Dictionary<string, SalaryHeadSequence> list)
        {
            try
            {
                list = new Dictionary<string, SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGrossPostion = 0;
                string deductionFormula = "";

                xlsCol += 1;
                countGrossPostion++;

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {

                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper())
                        {
                            _total_head_count++;


                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;

                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                            salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["PartOfNetPay"]);
                            salaryHeadSequence.IsGrossComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsGrossComponent"]);
                            salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"]);

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);
                            countCTCPosition++;
                        }


                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;


                _count_earning_ctchead = countCTCPosition - 1;

                int countDeductionPosition = countCTCPosition - 1;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        //{
                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                        {
                            _total_head_count++;
                            countDeductionPosition++;

                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                            //sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                            if (deductionFormula.Length == 0)
                            {
                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                            }
                            else
                            {
                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                            }

                            //countDeductionPosition++;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;

                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();


                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

                            _count_deducting_head++;
                        }
                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;

            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 7;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        //Salary Payable Disbursment Screen
        public void GetEmployeeInfoDetailSalaryLogWiseSalaryPayable(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef, string voucherId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";


                string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                salaryProcessId = "''";
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }
            }
            string wcEmpStatus = " AND (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage
                            FROM
                                    (
									SELECT DISTINCT E.SystemID EmpSystemId,ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric
                                            ,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID,  F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,EC.WorkingDaysInAMonth--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode
											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(spld.BankAccNo,'') BankAccNo
                                    ,ISNULL(spld.IFSCCode,'') IFSCCode
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost

                            			, sl.PayableVoucherId
                                     FROM  dbo.SalaryLock sl
									join EmployeeInformation E on sl.EmpSystemId=E.SystemId

                                          Left JOIN (
                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID

								 --left join dbo.SalaryLock sl on sl.MonthNo=m.MonthNo and sl.YearNo=m.YearNo and sl.PayableVoucherId=<>''

                                    WHERE SlrProcMstSystemID IN(" + salaryProcessId + @") 
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessId + @") AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '202022' 
                         
									 			LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and SPLD.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id

												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
            --                                    (
            --                                    SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												--LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												--)EC ON EC.DesignationId=E.GivenDesignationId
												[HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId
											

                                      --Where SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      --WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        --WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        --AND MonthNo =   MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')   )   
									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"') AND MMDSA.PlantID = '" + plantId + @"' 
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"'  AND EmpBasic.PlantId ='" + plantId + @"' 
		                                                                                    and EmpBasic.PayableVoucherId <>''  ";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"and EmpBasic.EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSQL += @"Order by EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWiseSalaryPayableInVoucher(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, string voucherId)
        {
            #region Variable
            clsReport objRpt = null;

            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;

            var reportUtility = new ReportUtility();
            // var excelEngine = new ExcelEngine();
            //var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            // workbook.Version = ExcelVersion.Excel2013;
            // var sheet = workbook.Worksheets[0];
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;

                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                //sheet.Name = "Voucher";



                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetailSalaryLogWiseSalaryPayable(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo, voucherId);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                var header = GetSalaryPayableSheetheader(companyGroupId, companyId, plantId, voucherId, SourceType.SalaryPayable);

                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 1, "Voucher No");
                reportUtility.SetText(ref sheet1, xlsRow, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 4, "Entry Date");
                reportUtility.SetText(ref sheet1, xlsRow, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

                sheet1[reportUtility.GetColumnNameForXls(2) + xlsRow + ":" + reportUtility.GetColumnNameForXls(3) + xlsRow].Merge();

                xlsRow++;

                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 1, "Posting Date");
                reportUtility.SetText(ref sheet1, xlsRow, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 4, "DocDate");
                reportUtility.SetText(ref sheet1, xlsRow, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

                sheet1[reportUtility.GetColumnNameForXls(2) + xlsRow + ":" + reportUtility.GetColumnNameForXls(3) + xlsRow].Merge();

                xlsRow++;

                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 1, "Status");
                reportUtility.SetText(ref sheet1, xlsRow, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 4, "Doc Ref");
                reportUtility.SetText(ref sheet1, xlsRow, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

                sheet1[reportUtility.GetColumnNameForXls(2) + xlsRow + ":" + reportUtility.GetColumnNameForXls(3) + xlsRow].Merge();

                xlsRow++;

                //colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
                reportUtility.SetMasterHeaderText(ref sheet1, xlsRow, 1, "Narration");
                reportUtility.SetText(ref sheet1, xlsRow, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
                sheet1[reportUtility.GetColumnNameForXls(2) + xlsRow + ":" + reportUtility.GetColumnNameForXls(3) + xlsRow].Merge();

                #region Column Variables
                xlsRow++;
                xlsRow++;

                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, colBank = 0, cDMP = 0, colBankAccountNo = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                //SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                SetCellValue("IFSCCode", sheet1, xlsRow, ref xlsCol, out int colBankIFSCCode, 25);

                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                //SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out cDMP, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                endGenericColumn = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                //xlsCol += 1;
                ColGrs = ColTotalOTHR;
                // 9

                var _count_earning_head = 0;
                var _count_earning_ctchead = 0;
                var _count_deducting_head = 0;
                var _total_head_count = 0;

                Dictionary<string, SalaryHeadSequence> shtList = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                salList.AddRange(shtList.Values);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                npstruct = 0;
                if (shtList.Count > 0)
                {
                    xlsCol++;
                    npstruct = ColGrs + shtList.Count + 1;
                    sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                    //sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                    //sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                }

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, npstruct].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].VerticalAlignment = ExcelVAlign.VAlignCenter;
                endXlsCol = npstruct;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankName"].ToString()) == false)
                            sheet1.Range[xlsRow, colBank].Text = dtEmployees.Rows[i]["BankName"].ToString();
                        sheet1.Range[xlsRow, colBank].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBank].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankAccNo"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankAccountNo].Text = dtEmployees.Rows[i]["BankAccNo"].ToString();
                        sheet1.Range[xlsRow, colBankAccountNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankAccountNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IFSCCode"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankIFSCCode].Text = dtEmployees.Rows[i]["IFSCCode"].ToString();
                        sheet1.Range[xlsRow, colBankIFSCCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankIFSCCode].VerticalAlignment = ExcelVAlign.VAlignCenter;



                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5 "Section", "SubSection", 

                        #endregion
                        #region Attendance Data
                        //if (dtEmpAttdnInfo.Rows.Count > 0)
                        //{
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = 0.00;
                        // clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"]);
                        if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                        {
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

                            }
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());
                            }
                        }
                        else
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        }
                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));

                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        //}
                        #endregion


                        //var _total_head_count_body = 0;

                        #region ------------------------------------Salary Sheet----------------------------------
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            if (drSalaryHeadCollection.Count > 0)
                            {
                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                {
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                    {
                                        sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                        continue;
                                    }
                                    try
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                            {
                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                            }

                                            else
                                            {

                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            }

                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }

                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                //var strFileName = "EmpSalaryStrSheet-" + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + Convert.ToDateTime(fdateOfMonth).ToString("yyyy") + "-" + para.SalaryProcessId + ".xls";

                if (isTopSheet == true)
                {
                    #region Salary Summary
                    string filePath = HostingEnvironment.MapPath("~/") + "TempSalaeySummary.xlsx";
                    workbook.SaveAs(filePath);
                    workbook = application.Workbooks.Open(filePath);

                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Move(1);

                    #region PivotSheet1
                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    pivotSheet.Name = "Summary";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(1) + pivotSheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(npstruct) + (sheetEndXlsRow)];
                    IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Department Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    pivotTable2.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[ColSr - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    int pivotColumnCount = 0;
                    IPivotField fieldGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldGross = null;
                        pivotColumnCount++;
                        fieldGross = pivotTable2_1.Fields[npstruct - 1];
                        pivotTable2_1.DataFields.Add(fieldGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }
                    catch (Exception)
                    {

                    }

                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    int totalColumns = pivotTable2_1.RowFields.Count + pivotColumnCount;

                    int lastCloumn = totalColumns + 2;

                    #endregion
                    #region PivotTable2

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "Employee Category Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    //Add data field
                    IPivotField field = pivotTable.Fields[ColSr - 1];
                    pivotTable.DataFields.Add(field, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot2ColumnCount = 0;
                    IPivotField fieldGross2 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross2 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross2 = null;
                    pivot2ColumnCount++;
                    fieldGross2 = pivotTable.Fields[npstruct - 1];
                    pivotTable.DataFields.Add(fieldGross2, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion

                    #region PivotTable3

                    totalColumns += pivotTable.RowFields.Count + pivot2ColumnCount;


                    lastCloumn += totalColumns - 10;

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "EmployeeStatus ,Employee Category and Department Wise  Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    //Create "PivotTable1" with the cache at the specified range
                    IPivotTable pivotTable3 = pivotSheet.PivotTables.Add("PivotTable13", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable3.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable13_1 = pivotSheet.PivotTables["PivotTable13"];
                    pivotTable13_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable13_1.Options.ShowDrillIndicators = false;

                    pivotTable13_1.DisplayFieldCaptions = true;
                    pivotTable13_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;


                    //Add data field
                    IPivotField fields3 = pivotTable13_1.Fields[ColSr - 1];
                    pivotTable13_1.DataFields.Add(fields3, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot3ColumnCount = 0;
                    IPivotField fieldGross3 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross3 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot3ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }

                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross3 = null;
                    pivot2ColumnCount++;
                    fieldGross3 = pivotTable13_1.Fields[npstruct - 1];
                    pivotTable13_1.DataFields.Add(fieldGross3, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion
                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;
                    pivotSheet.IsDisplayZeros = false;

                    pivotSheet.UsedRange.WrapText = false;

                    #endregion
                    #endregion

                    workbook.ActiveSheetIndex = 0;
                }

                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }


        #endregion

    }
}