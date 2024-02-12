using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Accounts;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Systems;
using Library.Model.Taxations;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Invoices;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Library.Service.Employees
{
    public class EmployeePayableService : IEmployeePayableService
    {
        #region Contractor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly IPKGeneratorService _pKGeneratorService;
        private readonly IRepositoryAsync<EmployeeTransactionTypeGL> _employeeTransactionTypeGLRepository;
        private readonly IRepositoryAsync<EmployeePayable> _employeePayableRepository;
        private readonly IRepositoryAsync<EmployeePayableDetail> _employeePayableDetailRepository;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<AdditionalTax> _additionalTaxRepository;
        private readonly IRepositoryAsync<AdditionalTaxDetail> _additionalTaxDetailRepository;
        private readonly IRepositoryAsync<EmployeeSubsequentTransaction> _employeeSubsequentTransactionRepository;
        public EmployeePayableService(
              IRepositoryAsync<EmployeePayable> employeePayableRepository
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IPKGeneratorService pkGeneratorService
            , IVoucherService voucherService
            , IRepositoryAsync<EmployeeTransactionTypeGL> employeeTransactionTypeGLRepository
            , IRepositoryAsync<EmployeePayableDetail> employeePayableDetailRepository
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            , IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository
            , IRepositoryAsync<AdditionalTax> additionalTaxRepository
            , IRepositoryAsync<AdditionalTaxDetail> additionalTaxDetailRepository
             , IRepositoryAsync<EmployeeSubsequentTransaction> employeeSubsequentTransactionRepository
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
            _employeePayableRepository = employeePayableRepository;
            _employeePayableDetailRepository = employeePayableDetailRepository;
            _employeeTransactionTypeGLRepository = employeeTransactionTypeGLRepository;
            _pKGeneratorService = pkGeneratorService;
            _invoiceTaxService = invoiceTaxService;
            _invoiceTaxRepository = invoiceTaxRepository;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _additionalTaxRepository = additionalTaxRepository;
            _additionalTaxDetailRepository = additionalTaxDetailRepository;
             _employeeSubsequentTransactionRepository = employeeSubsequentTransactionRepository;
        }

        #endregion Contractor

        public EmployeePayable InsertEmployeePayable(EmployeePayable employeePayable)
        {
            if (string.IsNullOrEmpty(employeePayable.Id))
                employeePayable.Id ="EP"+ _pKGeneratorService.GetAutoNumber(nameof(EmployeePayable), PKGeneratorEnum.Yearly, null, DateTime.Now);
            AuditService.AddedLog(employeePayable);
            _employeePayableRepository.Insert(employeePayable);
            return employeePayable;
        }

        public void UpdateEmployeePayable(EmployeePayable employeePayable)
        {
            _employeePayableRepository.Update(employeePayable);
        }

        public PKGenerator GetMaxNumber()
        {
            return _pKGeneratorService.GetMaxNumber(nameof(EmployeePayable), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public EmployeePayable InsertEmployeePayable(VoucherViewModel voucherVM)
        {
            var employeePayable = new EmployeePayable
            {
                CompanyGroupId = voucherVM.CompanyGroupId,
                CompanyId = voucherVM.CompanyId,
                PlantId = voucherVM.PlantId,
                EntityId = voucherVM.EntityId,
                CurrencyId = voucherVM.CurrencyId,
                EmployeeId = voucherVM.EmployeeId,
                EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId,
                FiscalYearId = voucherVM.FiscalYearId,
                FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                TaxYearId = voucherVM.TaxYearId,
                TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                VoucherTypeId = voucherVM.VoucherTypeId,
                DocDate = voucherVM.DocDate,
                DocRefNo = voucherVM.DocRefNo,
                PostingDate = voucherVM.PostingDate,
                Narration = voucherVM.Narration,
                SourceType = voucherVM.SourceType,
                PartyType = PartyType.Employee.ToString(),
                IsPark = voucherVM.IsPark,
                Amount = voucherVM.Amount,
                NetAmount = voucherVM.Amount,
                ExpenseBookingId = voucherVM.ExpenseBookingId,
                VoucherDate = voucherVM.VoucherDate,
                CompanyCurrencyRate = voucherVM.CompanyCurrencyRate,
                PartyId= voucherVM.PartyId,
                PartyPlantId=voucherVM.PartyPlantId,
                PaymentTermId=voucherVM.PaymentTermId
            };
            if(voucherVM.PaymentTermId != null)
            {
                employeePayable.BaseNoOfDays = voucherVM.BaseNoOfDays;
                employeePayable.BaseOnDueDate = voucherVM.BaseOnDueDate;
                employeePayable.ActualDueDate = voucherVM.BaseOnDueDate;
                employeePayable.RevisedDueDate = voucherVM.MatureDate;
            }
            return InsertEmployeePayable(employeePayable);
        }

        public EmployeePayableDetail InsertEmployeePayableDetail(EmployeePayable employeePayable, EmployeePayableDetail employeePayableDetail, int currentId)
        {
            employeePayableDetail.Id = _pKGeneratorService.MakePK(employeePayable.Id, currentId, 2);
            employeePayableDetail.EmployeePayableId = employeePayable.Id;
            employeePayableDetail.AddedBy = employeePayable.AddedBy;
            employeePayableDetail.AddedDate = employeePayable.AddedDate;
            employeePayableDetail.AddedFromIP = employeePayable.AddedFromIP;
            employeePayableDetail.Archive = employeePayable.Archive;
            _employeePayableDetailRepository.Insert(employeePayableDetail);
            return employeePayableDetail;
        }

        public void UpdateEmployeePayableDetail(EmployeePayableDetail employeePayableDetail)
        {
            _employeePayableDetailRepository.Update(employeePayableDetail);
        }
        private string GetEmployeeSubsequentTransactionPK()
        {
            return _pKGeneratorService.GetAutoNumber("EmployeeSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public string InsertEmployeePayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
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

                voucherVM.SourceType = SourceType.EmployeePayable.ToString();
                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    voucherVM.Amount = voucherDetailVMList.Sum(r => r.DrAmount)- taxDetailVMList.Sum(r=>r.TaxAmount);
                else
                    voucherVM.Amount = voucherDetailVMList.Sum(r => r.DrAmount);

                voucherVM.BaseNoOfDays = 0;
                var employeePayable = InsertEmployeePayable(voucherVM);

                var voucher = _voucherService.InsertVoucher(voucherVM);
                // Set VoucherId in EmployeePayable
                employeePayable.VoucherId = voucher.Id;

                // Employee Payable voucher Detail
                var currentVoucherDetailId = 0;
                var gl = GetEmployeePayableGL(employeePayable.CompanyId, employeePayable.EmployeeTransactionTypeId);

                var employeePayableDetailId = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    currentVoucherDetailId++;
                    var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                    {
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        DrAmount = voucherDetailVM.DrAmount
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
                }

                var withholdgl = false;


                if (null != taxDetailVMList && taxDetailVMList.Count() > 0)
                    {
                        var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                        foreach (var invoiceTaxVM in taxDetailVMList)
                        {
                        var taxCode = _accountsCommonService.GetTaxCode(invoiceTaxVM.TaxCodeId);

                        var taxCodeGL = _accountsCommonService.GetTaxCodeGL(taxCode["Id"].ToString());
                            if (null == taxCodeGL)
                                throw new CustomException("Tax code GL not found!");

                            var invoiceTax = new InvoiceTax
                            {
                                //VoucherDetailId = voucherDetailTempId,
                                TaxCodeId = invoiceTaxVM.TaxCodeId,
                                TaxCategoryId = invoiceTaxVM.TaxCategoryId,
                                TaxAmount = invoiceTaxVM.TaxAmount,
                                TaxAutoAmount = invoiceTaxVM.TaxAutoAmount
                            };
                           // totalAmountCr += invoiceTaxVM.TaxAmount;
                            _invoiceTaxService.InsertInvoiceTax(employeePayable, invoiceTax, invoiceTaxPk);

                            // Insert Into Customer Invoice Tax Detail (Withhold GL)
                            withholdgl = Convert.ToBoolean(taxCode["IsWithhold"]);
                            if (Convert.ToBoolean(taxCode["IsWithhold"]) && !string.IsNullOrEmpty(taxCodeGL["WithholdCreditableGLId"].ToString()))
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
                                    CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTax.CrAmount,
                                    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                                };
                                _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTax, voucherDetailCurrencyTax);
                            }
                        }
                    }
                

                employeePayableDetailId++;
                currentVoucherDetailId++;

                var employeePayableDetail = InsertEmployeePayableDetail(employeePayable, new EmployeePayableDetail
                {
                    GLGeneralInfoId = gl.PayableGLId,
                    BudgetMasterId = gl.PayableBudgetMasterId,
                    ActivityId = gl.PayableActivityId,
                    Amount = voucherVM.Amount,
                    NetAmount = voucherVM.Amount
                }, employeePayableDetailId);

                var voucherDetailCr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                {
                    EmployeePayableDetailId = employeePayableDetail.Id,
                    GLGeneralInfoId = gl.PayableGLId,
                    BudgetMasterId = gl.PayableBudgetMasterId,
                    ActivityId = gl.PayableActivityId,
                    EmployeeId = employeePayable.EmployeeId,
                    PartyType = employeePayable.PartyType,
                    CrAmount = voucherVM.Amount
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
                    EmployeePayableId = employeePayable.Id,
                    PartyType = employeePayable.PartyType,
                    CurrencyId = employeePayable.CurrencyId,
                    Amount = voucherDetailCr.CrAmount,
                    VoucherDate = voucherVM.VoucherDate,
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    JournalType = AdvanceType.General.ToString(),
                    TransactionType = EmployeeSubsequentTranEnum.Payable.ToString(),
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType.ToString(),
                    IsPark = voucherVM.IsPark,
                    Id = "ES" + GetEmployeeSubsequentTransactionPK(),
                    VoucherId = voucher.Id,
                    VoucherDetailId = voucherDetailCr.Id,
                    PaymentSource = voucherVM.PaymentSource,
                };
                AuditService.AddedLog(EmployeeSubsequentAdvance);
                _employeeSubsequentTransactionRepository.Insert(EmployeeSubsequentAdvance);


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

        public string UpdateEmployeePayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            try
            {
                var employeePayable = _employeePayableRepository.Find(voucherVM.Id);
                // Checking Posting status.
                CheckIsPosted(employeePayable);

                voucherVM.CompanyGroupId = employeePayable.CompanyGroupId;
                voucherVM.CompanyId = employeePayable.CompanyId;
                voucherVM.PlantId = employeePayable.PlantId;
                voucherVM.EntityId = employeePayable.EntityId;

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                employeePayable.Amount = voucherDetailVMList.Sum(r => r.DrAmount);
                UpdateEmployeePayable(employeePayable);

                var voucher = _voucherService.FindVoucher(employeePayable.VoucherId);
                voucher.PostedDate = employeePayable.PostingDate;
                voucher.CurrencyId = employeePayable.CurrencyId;
                voucher.Narration = employeePayable.Narration;
                voucher.DocDate = employeePayable.DocDate;
                voucher.DocRefNo = employeePayable.DocRefNo;
                voucher.UpdatedBy = employeePayable.UpdatedBy;
                voucher.UpdatedDate = employeePayable.UpdatedDate;
                voucher.UpdatedFromIP = employeePayable.UpdatedFromIP;
                _voucherService.UpdateVoucher(voucher);

                // Employee Payable voucher Detail
                var currentVoucherDetailId = _voucherService.GetVoucherDetailPK(voucher.Id);
                var gl = GetEmployeePayableGL(employeePayable.CompanyId, employeePayable.EmployeeTransactionTypeId);

                var voucherDetailList = _voucherService.GetVoucherDetailList(r => r.VoucherId == voucher.Id).Select().ToList();
                var voucherDetailCurrencyDbList = _voucherService.GetVoucherDetailCurrencyList(r => r.VoucherId == voucher.Id).Select().ToList();

                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    if (string.IsNullOrEmpty(voucherDetailVM.VoucherDetailId))
                    {
                        currentVoucherDetailId++;
                        var voucherDetailDr = _voucherService.InsertVoucherDetail(voucher, new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.DrAmount
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
                    }
                    else if (voucherDetailList.Any(r => r.Id == voucherDetailVM.VoucherDetailId))
                    {
                        var voucherDetailDr = voucherDetailList.FirstOrDefault(r => r.Id == voucherDetailVM.VoucherDetailId);
                        if (null == voucherDetailDr)
                            throw new CustomException("Voucher Detail (Dr) is null.");
                        voucherDetailDr.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                        voucherDetailDr.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                        voucherDetailDr.ActivityId = voucherDetailVM.ActivityId;
                        voucherDetailDr.DrAmount = voucherDetailVM.DrAmount;
                        _voucherService.UpdateVoucherDetail(voucher, voucherDetailDr);

                        var voucherDetailCurrency = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailDr.Id && r.ParallelCurrencyId == companyCurrencyId);
                        if (null == voucherDetailCurrency)
                            throw new CustomException("Voucher Detail Currency (Dr) is null.");

                        voucherDetailCurrency.FromCurrencyId = voucherDetailDr.CurrencyId;
                        voucherDetailCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                        voucherDetailCurrency.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailDr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                        voucherDetailCurrency.DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailDr.DrAmount;
                        _voucherService.UpdateVoucherDetailCompanyCurrency(voucherDetailDr, voucherDetailCurrency);
                    }
                }

                var employeePayableDetail = _employeePayableDetailRepository.Query(r => r.EmployeePayableId == employeePayable.Id).Select().FirstOrDefault();
                employeePayableDetail.GLGeneralInfoId = gl.PayableGLId;
                employeePayableDetail.BudgetMasterId = gl.PayableBudgetMasterId;
                employeePayableDetail.ActivityId = gl.PayableActivityId;
                employeePayableDetail.Amount = voucherDetailVMList.Sum(r => r.DrAmount);
                employeePayableDetail.NetAmount = voucherDetailVMList.Sum(r => r.DrAmount);

                var voucherDetailCr = voucherDetailList.FirstOrDefault(r => r.EmployeePayableDetailId == employeePayableDetail.Id);
                if (null == voucherDetailCr)
                    throw new CustomException("Voucher Detail (Cr) is null.");

                voucherDetailCr.EmployeePayableDetailId = employeePayableDetail.Id;
                voucherDetailCr.GLGeneralInfoId = gl.PayableGLId;
                voucherDetailCr.BudgetMasterId = gl.PayableBudgetMasterId;
                voucherDetailCr.ActivityId = gl.PayableActivityId;
                voucherDetailCr.EmployeeId = employeePayable.EmployeeId;
                voucherDetailCr.PartyType = employeePayable.PartyType;
                voucherDetailCr.CrAmount = voucherDetailVMList.Sum(r => r.DrAmount);
                _voucherService.UpdateVoucherDetail(voucher, voucherDetailCr);

                var voucherDetailCrCurrency = voucherDetailCurrencyDbList.FirstOrDefault(r => r.VoucherDetailId == voucherDetailCr.Id && r.ParallelCurrencyId == companyCurrencyId);
                if (null == voucherDetailCrCurrency)
                    throw new CustomException("Voucher Detail Currency (Cr) is null.");

                // INSERT INTO VoucherDetailCurrency
                voucherDetailCrCurrency.ParallelCurrencyId = companyCurrencyId;
                voucherDetailCrCurrency.FromCurrencyId = voucherDetailCr.CurrencyId;
                voucherDetailCrCurrency.ToCurrencyId = companyCurrencyId;
                voucherDetailCrCurrency.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                voucherDetailCrCurrency.ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailCr.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate);
                voucherDetailCrCurrency.CrAmount = voucherVM.CompanyCurrencyRate * voucherDetailCr.CrAmount;

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

        public GridModel GetEmployeePayableList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, EP.Id, EP.Id AS EmployeePayableId, EP.EmployeeId, EI.EmployeeCode, EI.EmployeeName, EP.VoucherId, EP.PostingDate, EP.DocDate, EP.DocRefNo, EP.CurrencyId, C.Code AS CurrencyCode
                                , EP.Amount, EP.IsWrittenOff, EP.WrittenOffAmount, EP.IsPark, EP.NetAmount,EP.CompanyCurrencyRate,V.VoucherTypeId,V.VoucherDate,EP.Narration
                                FROM [TRN].[EmployeePayable] AS EP
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EP.EmployeeId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=EP.CurrencyId
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=EP.VoucherId
                                WHERE EP.Archive=0 AND EP.OpeningBalanceId IS NULL AND EP.ExpenseBookingId IS NULL AND EP.CompanyGroupId='" + companyGroupId + "'AND EP.CompanyId='" + companyId + "' AND EP.PlantId='" + plantId + "' AND EP.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public EmployeeTransactionTypeGL GetEmployeePayableGL(string companyId, string employeeTransactionTypeId)
        {
            var sql = @"SELECT TOP(1) ETTGL.* FROM [HKP].[EmployeeTransactionTypeGL] AS ETTGL
                        INNER JOIN [ORG].[Company] AS C ON C.COAId=ETTGL.COAId
                        WHERE C.Id='" + companyId + "' AND ETTGL.EmployeeTransactionTypeId='" + employeeTransactionTypeId + "'";
            var glTemp = _employeeTransactionTypeGLRepository.SelectQuery(sql).FirstOrDefault();
            if (null == glTemp && string.IsNullOrEmpty(glTemp.PayableGLId))
                throw new CustomException("This transaction type GL not found!");
            return glTemp;
        }

        public void Post(string employeePayableId)
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

        private static void CheckIsPosted(EmployeePayable employeePayable)
        {
            if (!employeePayable.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
        }

        public IQueryFluent<EmployeePayable> GetEmployeePayableList(Expression<Func<EmployeePayable, bool>> query)
        {
            return _employeePayableRepository.Query(query);
        }

        public IQueryFluent<EmployeePayableDetail> GetEmployeePayableDetailList(Expression<Func<EmployeePayableDetail, bool>> query)
        {
            return _employeePayableDetailRepository.Query(query);
        }

        public Dictionary<string, object> GetEmployeePayableReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var sql = @"SELECT E.UserName AS EntityName, FY.FiscalYearName, FY.YearPrefix, FYP.PeriodName, FYP.PeriodNo, VT.UserName AS VoucherTypeName, V.CurrencyId, C.Code AS CurrencyCode, V.VoucherNo
                        , REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, V.IsPark
                        , V.AddedBy, V.PostedBy, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, UPPER(V.Narration) AS Narration, EI.EmployeeCode, EI.EmployeeName
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [ORG].[Entity] AS E ON E.Id=V.EntityId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                        LEFT JOIN [TRN].[EmployeePayable] AS EP ON EP.VoucherId=V.Id
                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EP.EmployeeId
                        WHERE V.Archive=0 AND V.Id='" + voucherId + "' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(sql);
        }

        public Dictionary<string, object> GetEmployeePayableExpenseBookingReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var sql = @"SELECT DISTINCT E.UserName AS EntityName, FY.FiscalYearName, FY.YearPrefix, FYP.PeriodName, FYP.PeriodNo, VT.UserName AS VoucherTypeName, V.CurrencyId, C.Code AS CurrencyCode, V.VoucherNo
                        , REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, V.IsPark
                        , AddedBy=CASE WHEN EB.AppliedBy='Self' THEN AEI.EmployeeName ELSE V.AddedBy END,U.FullName PostedBy, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, UPPER(V.Narration) AS Narration
						,EmployeeCode=CASE WHEN  EI.EmployeeCode<>'' THEN EI.EmployeeCode ELSE EPI.EmployeeCode END
						,EmployeeName=CASE WHEN EI.EmployeeName<>'' THEN EI.EmployeeName ELSE EPI.EmployeeName END
						, EB.BeneficiaryType
                        , EBAH.EmployeeId, EIA.EmployeeCode AS ApprovedByCode, EIA.EmployeeName AS ApprovedByName, P.Code AS PartyCode, P.UserName AS PartyName,REI.EmployeeName CheckedBy
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [ORG].[Entity] AS E ON E.Id=V.EntityId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                        LEFT JOIN [TRN].[ExpenseBooking] AS EB ON EB.VoucherId=V.Id
						LEFT JOIN TRN.EmployeePayable EP ON EP.VoucherId=V.Id
                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
                        LEFT JOIN [TRN].[ExpenseBookingApprovalHistory] AS EBAH ON EBAH.ExpenseBookingId=EB.Id
                        LEFT JOIN [dbo].[EmployeeInformation] AS EIA ON EIA.SystemId=EBAH.EmployeeId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
						LEFT JOIN [dbo].[EmployeeInformation] AS AEI ON AEI.SystemId=EB.EmployeeId
                        LEFT JOIN [dbo].[EmployeeInformation] AS REI ON REI.SystemId=EB.ResponsiblePersonId
                        LEFT JOIN [dbo].[EmployeeInformation] AS EPI ON EPI.SystemId=EP.EmployeeId
                        LEFT JOIN [SEC].[User] U on U.UserId=V.PostedBy
                        LEFT JOIN [dbo].[EmployeeInformation] AS EIAN ON EIAN.SystemId=V.PostedBy
                        WHERE V.Archive=0 AND V.Id='" + voucherId + "' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType in ('" + sourceType + "','VendorInvoice')";
            return _sqlRepository.GetData(sql);
        }

        public List<Dictionary<string, object>> GetAdvanceWriteOffReportData(string companyId, string voucherId)
        {
            var sql = @"SELECT GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, B.Code AS BudgetCode, B.UserName AS BudgetName, A.Code AS ActivityCode, A.UserName AS ActivityName
                          , ISNULL(VD.DrAmount,0) DrAmount, ISNULL(VD.CrAmount,0) CrAmount,ISNULL(CC.CompanyCurrencyDrAmount,0) CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount,0) CompanyCurrencyCrAmount
                        FROM [TRN].[VoucherDetail] AS VD
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE VD.VoucherId='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetExoenseBookingReportData(string companyId, string voucherId)
        {
            var sql = @"SELECT GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, B.Code AS BudgetCode, B.UserName AS BudgetName, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , ISNULL(VD.DrAmount,0) DrAmount, ISNULL(VD.CrAmount,0) CrAmount,ISNULL(CC.CompanyCurrencyDrAmount,0) CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount,0) CompanyCurrencyCrAmount
                        , MM.UserName AS AssetItem, FAR.InvoiceNo
                        FROM [TRN].[VoucherDetail] AS VD
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                        LEFT JOIN [TRN].[ExpenseBookingDetail] AS EBD ON EBD.Id=VD.ExpenseBookingDetailId
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=EBD.MaterialMasterId
                        LEFT JOIN [TRN].[FixedAssetRegister] AS FAR ON FAR.Id=EBD.FixedAssetRegisterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE VD.VoucherId='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public Dictionary<string, object> GetEmployeePayable(string id)
        {
            var sql = @"SELECT EP.Id, EP.VoucherId, EP.EmployeeId, EI.EmployeeName, EP.EmployeeTransactionTypeId, EP.VoucherDate, EP.VoucherTypeId, EP.PostingDate, EP.CurrencyId, EP.CompanyCurrencyRate, EP.Narration
                        , EP.DocDate, EP.DocRefNo
                        FROM [TRN].[EmployeePayable] AS EP
                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EP.EmployeeId
                        WHERE EP.Id='" + id + "'";
            return _sqlRepository.GetData(sql);
        }

        public List<Dictionary<string, object>> GetEmployeePayableDetailList(string voucherId)
        {
            var sql = @"SELECT VD.VoucherId, VD.Id AS VoucherDetailId, VD.GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, VD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                        , VD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, VD.DrAmount FROM
                        [TRN].[VoucherDetail] AS VD
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                        WHERE VD.VoucherId='" + voucherId + "' AND VD.EmployeePayableDetailId IS NULL";
            return _sqlRepository.GetDataCollection(sql);
        }

        public GridModel GetEmployeeReconGLBudgetActivity(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , B.BudgetMasterId, B.RefNo, B.BudgetCode, B.BudgetName, A.ActivityId, A.ActivityCode, A.ActivityName, GLTY.AccountType
                                    FROM [HKP].[GLGeneralInfo] AS GLGI
                                    LEFT JOIN [HKP].[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLAccountType] AS GLTY ON GLTY.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                    LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.GLGeneralInfoId, BM.RefNo
	                                    FROM [HKP].[Budget] AS B
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                    ) AS B ON B.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN (SELECT A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, BA.BudgetMasterId
	                                    FROM [HKP].[Activity] AS A
	                                    LEFT JOIN [MST].[BudgetMasterActivity] AS BA ON BA.ActivityId=A.Id
                                    ) AS A ON A.BudgetMasterId=B.BudgetMasterId
                                    WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLCG.CompanyGroupId='" + companyGroupId + "' AND GLCI.CompanyId='" + companyId + @"' 
                                    AND GLGI.Id NOT IN(SELECT BM.GLGeneralInfoId FROM [MST].[BankMaster] AS BM WHERE BM.GLGeneralInfoId <> '')
                                    AND GLGI.Id NOT IN(SELECT CM.GLGeneralInfoId FROM [MST].[CashMaster] AS CM WHERE CM.GLGeneralInfoId <> '') 
                                    AND GLGI.Id IN(SELECT AC.GLGeneralInfoId FROM [HKP].[GLAccountType] AS AC WHERE AC.AccountType ='" + ReconcileAccountEnum.Employee + "') ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetEmployeeReconAssetGLBudgetActivity(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , B.BudgetMasterId, B.RefNo, B.BudgetCode, B.BudgetName, A.ActivityId, A.ActivityCode, A.ActivityName, GLTY.AccountType
                                    FROM [HKP].[GLGeneralInfo] AS GLGI
                                    LEFT JOIN [HKP].[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLAccountType] AS GLTY ON GLTY.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                    LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.GLGeneralInfoId, BM.RefNo
	                                    FROM [HKP].[Budget] AS B
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                    ) AS B ON B.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN (SELECT A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, BA.BudgetMasterId
	                                    FROM [HKP].[Activity] AS A
	                                    LEFT JOIN [MST].[BudgetMasterActivity] AS BA ON BA.ActivityId=A.Id
                                    ) AS A ON A.BudgetMasterId=B.BudgetMasterId
                                    WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLCG.CompanyGroupId='" + companyGroupId + "' AND GLCI.CompanyId='" + companyId + @"' AND ACT.Id='Asset'
                                    AND GLGI.Id NOT IN(SELECT BM.GLGeneralInfoId FROM [MST].[BankMaster] AS BM WHERE BM.GLGeneralInfoId <> '')
                                    AND GLGI.Id NOT IN(SELECT CM.GLGeneralInfoId FROM [MST].[CashMaster] AS CM WHERE CM.GLGeneralInfoId <> '') 
                                    AND GLGI.Id IN(SELECT AC.GLGeneralInfoId FROM [HKP].[GLAccountType] AS AC WHERE AC.AccountType ='" + ReconcileAccountEnum.Employee + "') ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void DeleteInvoiceBeneficiaryEmployee(string invoiceId, string voucherId, string deletedRemarks)
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
                _accountsCommonService.InsertVoucherLogDeleted(voucherId, voucher.VoucherNo, "", "", invoiceId, "", "", "", "", "", "", "", "", deletedRemarks);


                var voucherdetail = _voucherService.QueryVoucherDetail(voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherService.QueryVoucherDetailCurrency(voucherId).Select().ToList();
                var employeePayable = _employeePayableRepository.Find(invoiceId);
                var employeePayableDetail = _employeePayableDetailRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();
                var invoiceTDS = _additionalTaxRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                foreach (var item in employeePayableDetail)
                {
                    _employeePayableDetailRepository.Delete(item.Id);
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
                if (invoiceTDS != null)
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
                _employeePayableRepository.Delete(employeePayable.Id);
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

        public void DeleteGRNBeneficiaryEmployee(string grnId, string invoiceId, string voucherId, string deletedRemarks)
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
                _accountsCommonService.InsertVoucherLogDeleted(voucherId, voucher.VoucherNo, "", "", invoiceId, "", "", "", "", "", "", "", "", deletedRemarks);

                var voucherdetail = _voucherService.QueryVoucherDetail(voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherService.QueryVoucherDetailCurrency(voucherId).Select().ToList();
                var employeePayable = _employeePayableRepository.Find(invoiceId);
                if (employeePayable.WrittenOffAmount > 0)
                    throw new CustomException("Please Delete Payment Voucher first ! ");
                var employeePayableDetail = _employeePayableDetailRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();
                var invoiceTDS = _additionalTaxRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();

                var grnBuilder = new System.Text.StringBuilder();
                var buildergrnSql = @"UPDATE [TRN].InventoryReceive set VoucherId =NULL,Status=NULL WHERE Id='" + grnId + "'";
                var builderemployeeSubSequentTransactionSql = @"Delete TRN.EmployeeSubsequentTransaction WHERE EmployeePayableId='" + invoiceId + "'";
                grnBuilder.Append(buildergrnSql);
                grnBuilder.Append(builderemployeeSubSequentTransactionSql);
                _sqlRepository.ExecuteSqlCommand(grnBuilder.ToString());

                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                foreach (var item in employeePayableDetail)
                {
                    _employeePayableDetailRepository.Delete(item.Id);
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
                if (invoiceTDS != null)
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
                _employeePayableRepository.Delete(employeePayable.Id);
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


        public void DeleteServiceBeneficiaryEmployee(string serviceAckId, string invoiceId, string voucherId)
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
                var employeePayable = _employeePayableRepository.Find(invoiceId);
                if (employeePayable.WrittenOffAmount > 0)
                    throw new CustomException("Please Delete Payment Voucher first ! ");
                var employeePayableDetail = _employeePayableDetailRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();
                var invoiceTDS = _additionalTaxRepository.Query(r => r.EmployeePayableId == invoiceId).Select().ToList();

                var grnBuilder = new System.Text.StringBuilder();
                var buildergrnSql = @"UPDATE [TRN].InventoryReceive set VoucherId =NULL,Status=NULL WHERE Id='" + serviceAckId + "'";
                grnBuilder.Append(buildergrnSql);
                _sqlRepository.ExecuteSqlCommand(grnBuilder.ToString());

                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }

                foreach (var item in voucherdetail)
                {
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                foreach (var item in employeePayableDetail)
                {
                    _employeePayableDetailRepository.Delete(item.Id);
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
                if (invoiceTDS != null)
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
                _employeePayableRepository.Delete(employeePayable.Id);
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