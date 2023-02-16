using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Invoices;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Vouchers;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Data;
using System.ComponentModel;
using Library.Service.Banks;
using Library.Service.Extension;
using Library.Service.Properties;
using Library.Model.Currencies;
using Library.Service.Extension.Accounts;

namespace Library.Service.Finances
{
    public class AutoLoanService : IAutoLoanService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;

        private readonly IVoucherService _voucherService;
        private readonly IFinancingService _financingService;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<InvoiceWriteOff> _invoiceWriteOffRepository;
        private readonly IRepositoryAsync<InvoiceWriteOffDetail> _invoiceWriteOffDetailRepository;
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<InvoiceDetail> _invoiceDetailRepository;
        public AutoLoanService(
             IUnitOfWork unitOfWork
            , IVoucherService voucherService
            , IFinancingService financingService
            , IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<InvoiceWriteOff> invoiceWriteOffRepository
            , IRepositoryAsync<InvoiceWriteOffDetail> invoiceWriteOffDetailRepository
            , IRepositoryAsync<Invoice> invoiceRepository
            , IRepositoryAsync<InvoiceDetail> invoiceDetailRepository
            )
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _voucherService = voucherService;
            _financingService = financingService;
            _invoiceWriteOffRepository = invoiceWriteOffRepository;
            _invoiceWriteOffDetailRepository = invoiceWriteOffDetailRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
        }

        #endregion Constructor


        public InvoiceWriteOff InsertInvoiceWriteOff(InvoiceWriteOff invoiceWriteOff)
        {
            invoiceWriteOff.Id = _pkGeneratorService.GetAutoNumber(nameof(InvoiceWriteOff), PKGeneratorEnum.Yearly, null, DateTime.Now);
            _invoiceWriteOffRepository.Insert(invoiceWriteOff);
            return invoiceWriteOff;
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
                VoucherId = voucherVM.VoucherId,
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
            //Check(invoiceWriteOff);
            return InsertInvoiceWriteOff(invoiceWriteOff);
        }

        public void InsertInvoiceWriteOffDetail(InvoiceWriteOff invoiceWriteOff, InvoiceWriteOffDetail invoiceWriteOffDetail, int currentId)
        {
            invoiceWriteOffDetail.AddedBy = invoiceWriteOff.AddedBy;
            invoiceWriteOffDetail.AddedDate = invoiceWriteOff.AddedDate;
            invoiceWriteOffDetail.AddedFromIP = invoiceWriteOff.AddedFromIP;
            invoiceWriteOffDetail.Archive = invoiceWriteOff.Archive;
            invoiceWriteOffDetail.InvoiceWriteOffId = invoiceWriteOff.Id;
            invoiceWriteOffDetail.Id = _pkGeneratorService.MakePK(invoiceWriteOff.Id, currentId, 2);
            _invoiceWriteOffDetailRepository.Insert(invoiceWriteOffDetail);
        }
        //private void Check(InvoiceWriteOff entity)
        //{
        //    _pkGeneratorService.CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        //}

        public string ParkAutoLoan(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService accountCommonExtensionService = new AccountCommonExtensionService();
                accountCommonExtensionService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                accountCommonExtensionService.CheckingFiscalYearPeriod(voucherVM);
                accountCommonExtensionService.CheckingTaxYearPeriod(voucherVM);



                _unitOfWork.BeginTransaction();
                flag = true;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster, dsMasterFSTran;

                // INSERT INTO Financing TABLE
                var financing = _financingService.InsertFinancing(new Financing
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    FinancingTypeId = voucherVM.FinancingTypeId,
                    //BankMasterId = voucherVM.BankMasterId,
                    OtherBankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    EmployeeId = voucherVM.EmployeeId,
                    //PartyId = voucherVM.PartyId,
                    PartyType = "Bank",
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType,
                    PaymentSource = voucherVM.PaymentSource,
                    Amount = voucherVM.Amount,
                    LifeOfYear = voucherVM.LifeOfYear,
                    NoOfInstallmentPerYear = voucherVM.NoOfInstallmentPerYear,
                    TotalNoOfInstallment = voucherVM.TotalNoOfInstallment,
                    ProfitRate = voucherVM.ProfitRate,
                    ProfitAmount = voucherVM.ProfitAmount,
                    RepaymentStartDate = voucherVM.RepaymentStartDate,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    IsPark = voucherVM.IsPark,
                    //PartyPlantId = voucherVM.PartyPlantId,
                    TransactionType = voucherVM.TransactionType,
                    IsSchedule = voucherVM.IsSchedule,
                    LoanAgainstAcceptanceId = voucherVM.LoanAgainstAcceptanceId
                });

                var voucher = _voucherService.InsertVoucher(voucherVM);
                voucherVM.AddedBy = voucher.AddedBy;
                voucherVM.AddedDate = voucher.AddedDate;
                voucherVM.AddedFromIP = voucher.AddedFromIP;
                voucherVM.VoucherId = voucher.Id;
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                objCon = new ConnectionManager.DAL.ConManager("1");
                
                    string sql = "SELECT * FROM LoanAgainstAcceptanceMaster WHERE Id='" + voucherVM.LoanAgainstAcceptanceId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (voucherVM.LoanAgainstAcceptanceId != null)
                    {

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();
                        //dr["PurchaseDocAcceptanceId"] = voucherVM.PurchaseDocAcceptanceId;
                        dr["VoucherId"] = voucher.Id;
                        //dr["BankMasterId"] = voucherVM.BankMasterId;
                        //dr["CompanyGroupId"] = voucherVM.CompanyGroupId;
                        //dr["CompanyId"] = voucherVM.CompanyId;
                        //dr["PlantId"] = voucherVM.PlantId;
                        //dr["CurrencyId"] = voucherVM.CurrencyId;
                        //dr["PartyType"] = "Vendor";
                        //dr["PartyId"] = voucherVM.PartyId;
                        //dr["PartyPlantId"] = voucherVM.PartyPlantId;
                        //dr["Amount"] = voucherVM.Amount;
                        //dr["PaymentSource"] = "Bank";
                        //dr["TransactionType"] = "LoanTaken";
                        //dr["IsPark"] = true;
                        dr["UpdatedBy"] = voucher.AddedBy;
                        dr["UpdatedDate"] = voucher.AddedDate;
                        dr["UpdatedFromIP"] = voucher.AddedFromIP;

                        dr.EndEdit();
                    }
                
               




                // Set to Financing
                financing.VoucherId = voucher.Id;

                // INSERT INTO FinancingDetail
                var investmentDetail = new FinancingDetail
                {
                    Amount = financing.Amount,
                };
                // Investment from side Voucher detail row.
                var voucherDetailFrom = new VoucherDetail
                {
                    PartyType = financing.PartyType,
                    PaymentSource = financing.PaymentSource
                };

                // Investment to side Voucher detail row.

                var voucherDetailLoanPayment = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                _financingService.InsertFinancingDetail(financing, investmentDetail);
                var currentVoucherDetailId = 1;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    #region From

                    var gl = accountCommonExtensionService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);

                    if (string.IsNullOrEmpty(gl["LiabilityGLId"].ToString()))
                        throw new CustomException("This Transaction Type GL not Found!");
                    if (string.IsNullOrEmpty(gl["LiabilityActivityId"].ToString()))
                        throw new CustomException("This Transaction Type Activity not Found!");
                    investmentDetail.GLGeneralInfoId = gl["LiabilityGLId"].ToString();
                    investmentDetail.BudgetMasterId = gl["LiabilityBudgetMasterId"].ToString();
                    investmentDetail.ActivityId = gl["LiabilityActivityId"].ToString();


                    voucherDetailFrom.FinancingDetailId = investmentDetail.Id;
                    voucherDetailFrom.CrAmount = voucherVM.Amount;
                    voucherDetailFrom.GLGeneralInfoId = investmentDetail.GLGeneralInfoId;
                    voucherDetailFrom.BudgetMasterId = investmentDetail.BudgetMasterId;
                    voucherDetailFrom.ActivityId = investmentDetail.ActivityId;
                    voucherDetailFrom.BankMasterId = voucherVM.BankMasterId;

                    AuditService.AddedLog(voucherDetailFrom);
                    voucherDetailFrom.ModelState = ModelState.Added;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailFrom.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = Math.Round(voucherDetailVMList.Sum(r => r.CompanyCurrencyRate), 4) / voucherDetailVMList.Count(),
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, Math.Round(voucherDetailVMList.Sum(r => r.CompanyCurrencyRate), 4) / voucherDetailVMList.Count()),
                        CrAmount = Math.Round(voucherDetailVMList.Sum(r => r.Amount * r.CompanyCurrencyRate), 4)
                    });
                    totalAmountCr += voucherDetailFrom.CrAmount;
                    #endregion From
                    var currentInvoiceWriteOffDetailId = 0;
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        #region To

                        var invoice = _invoiceRepository.Find(voucherDetailVM.InvoiceId);
                        var invoiceDetail = _invoiceDetailRepository.Find(voucherDetailVM.InvoiceDetailId);

                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");

                        invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                        if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                            throw new CustomException("Received amount can not cross balance amount.");

                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoiceDetail.UpdatedBy = voucher.AddedBy;
                        invoiceDetail.UpdatedDate = voucher.AddedDate;
                        invoiceDetail.UpdatedFromIP = voucher.AddedFromIP;
                        _invoiceDetailRepository.Update(invoiceDetail);

                        invoice.WrittenOffAmount += voucherDetailVM.Amount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = voucher.AddedBy;
                        invoice.UpdatedDate = voucher.AddedDate;
                        invoice.UpdatedFromIP = voucher.AddedFromIP;
                        _invoiceRepository.Update(invoice);

                        // INSERT INTO InvoiceDetail

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
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration
                        };
                        currentInvoiceWriteOffDetailId++;
                        InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                        var voucherDetailTo = new VoucherDetail
                        {
                            PartyType = voucherVM.PartyType
                        };
                        if (voucherVM.PartyType == PartyType.Vendor.ToString())
                        {
                            if (string.IsNullOrEmpty(financing.PartyId))
                                throw new CustomException("Vendor Id not found!");

                            voucherDetailTo.PartyId = financing.PartyId;
                            voucherDetailTo.PartyPlantId = financing.PartyPlantId;
                            voucherDetailTo.TrnNature = TransactionNature.Vendor.ToString();
                        }

                        voucherDetailTo.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                        voucherDetailTo.ActivityId = voucherDetailVM.ActivityId;
                        voucherDetailTo.TrnNature = TransactionNature.ToVendor.ToString();
                        voucherDetailTo.InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id;
                        voucherDetailTo.DrAmount = voucherDetailVM.Amount;

                        currentVoucherDetailId++;
                        AuditService.AddedLog(voucherDetailTo);
                        voucherDetailTo.ModelState = ModelState.Added;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailTo.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailTo.DrAmount
                        });
                        totalAmountDr += voucherDetailTo.DrAmount;
                        if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailVM.Amount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }


                        #endregion To

                        
                    }


                }

                // INSRT INTO GLTransactionDetail TABLE From
                if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                    {
                        BankMasterId = voucherDetailFrom.BankMasterId,
                        CashMasterId = voucherDetailFrom.CashMasterId,
                        CrAmount = Math.Round(voucherDetailVMList.Sum(r => r.Amount * r.CompanyCurrencyRate), 4),
                        SourceType = voucherDetailFrom.PaymentSource
                    });
                }
                // INSRT INTO GLTransactionDetail TABLE To

                if (voucherVM.IsSchedule)
                {
                    foreach (var item in financingScheduleVMList)
                    {
                        var financingSchedule = new FinancingSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance
                        };
                        _financingService.InsertFinancingSchedule(financing, financingSchedule);
                    }
                }
                string sql1 = "SELECT * FROM [trn].[FinancingSubsequentTransaction] WHERE Id='" + voucherVM.Id + "'";

                objCon.OpenDataSetThroughAdapter(sql1, out dsMasterFSTran, false, "1");

                if (dsMasterFSTran.Tables[0].Rows.Count == 0)
                {
                    DataRow dr1 = dsMasterFSTran.Tables[0].NewRow();
                    dr1["CompanyGroupId"] = voucherVM.CompanyGroupId;
                    dr1["CompanyId"] = voucherVM.CompanyId;
                    dr1["PlantId"] = voucherVM.PlantId;
                    dr1["EntityId"] = voucherVM.EntityId;
                    dr1["VoucherTypeId"] = voucherVM.VoucherTypeId;
                    dr1["FinancingId"] = financing.Id;
                    //dr1["PartyId"] = voucherVM.PartyId;
                    //dr1["PartyPlantId"] = voucherVM.PartyPlantId;
                    dr1["PartyType"] = "Bank";
                    dr1["CurrencyId"] = voucherVM.CurrencyId;
                    dr1["Amount"] = voucherVM.Amount;
                    dr1["VoucherDate"] = voucherVM.VoucherDate;
                    dr1["PostingDate"] = voucherVM.PostingDate;
                    dr1["DocDate"] = voucherVM.DocDate;
                    dr1["DocRefNo"] = voucherVM.DocRefNo;
                    dr1["TransactionType"] = LoanTransactionType.Loan.ToString();
                    dr1["Narration"] = voucherVM.Narration;
                    dr1["SourceType"] = voucherVM.SourceType.ToString();
                    dr1["IsPark"] = voucherVM.IsPark;
                    dr1["IsPosted"] = false;
                    dr1["Archive"] = false;
                    dr1["Id"] = "SL" + GetLoanInterestPayablePK();
                    dr1["VoucherId"] = voucher.Id;
                    dr1["VoucherDetailId"] = voucherDetailFrom.Id;
                    dr1["AddedBy"] = voucher.AddedBy;
                    dr1["AddedDate"] = voucher.AddedDate;
                    dr1["AddedFromIP"] = voucher.AddedFromIP;
                    dsMasterFSTran.Tables[0].Rows.Add(dr1);
                }
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();

                flag = false;
                _unitOfWork.Commit();
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterFSTran, dsMaster);

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
        public string ParkAutoLoanInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService accountCommonExtensionService = new AccountCommonExtensionService();
                accountCommonExtensionService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                accountCommonExtensionService.CheckingFiscalYearPeriod(voucherVM);
                accountCommonExtensionService.CheckingTaxYearPeriod(voucherVM);



                _unitOfWork.BeginTransaction();
                flag = true;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster, dsMasterFSTran;

                // INSERT INTO Financing TABLE
                var financing = _financingService.InsertFinancing(new Financing
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    FinancingTypeId = voucherVM.FinancingTypeId,
                    //BankMasterId = voucherVM.BankMasterId,
                    OtherBankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    EmployeeId = voucherVM.EmployeeId,
                    //PartyId = voucherVM.PartyId,
                    PartyType = "Bank",
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType,
                    PaymentSource = voucherVM.PaymentSource,
                    Amount = voucherVM.Amount,
                    LifeOfYear = voucherVM.LifeOfYear,
                    NoOfInstallmentPerYear = voucherVM.NoOfInstallmentPerYear,
                    TotalNoOfInstallment = voucherVM.TotalNoOfInstallment,
                    ProfitRate = voucherVM.ProfitRate,
                    ProfitAmount = voucherVM.ProfitAmount,
                    RepaymentStartDate = voucherVM.RepaymentStartDate,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    IsPark = voucherVM.IsPark,
                    //PartyPlantId = voucherVM.PartyPlantId,
                    TransactionType = voucherVM.TransactionType,
                    IsSchedule = voucherVM.IsSchedule,
                    InvoiceTaggingWithLCMasterId = voucherVM.LoanAgainstAcceptanceId
                });

                var voucher = _voucherService.InsertVoucher(voucherVM);
                voucherVM.AddedBy = voucher.AddedBy;
                voucherVM.AddedDate = voucher.AddedDate;
                voucherVM.AddedFromIP = voucher.AddedFromIP;
                voucherVM.VoucherId = voucher.Id;
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                objCon = new ConnectionManager.DAL.ConManager("1");
               
               
                    string sql = "SELECT * FROM InvoiceTaggingWithLCMaster WHERE Id='" + voucherVM.LoanAgainstAcceptanceId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (voucherVM.LoanAgainstAcceptanceId != null)
                    {

                        DataRow drInvoice = dsMaster.Tables[0].DefaultView[0].Row;

                        drInvoice.BeginEdit();

                        drInvoice["VoucherId"] = voucher.Id;
                        //drInvoice["BankMasterId"] = voucherVM.BankMasterId;
                        //drInvoice["CompanyGroupId"] = voucherVM.CompanyGroupId;
                        //drInvoice["CompanyId"] = voucherVM.CompanyId;
                        //drInvoice["PlantId"] = voucherVM.PlantId;
                        //drInvoice["CurrencyId"] = voucherVM.CurrencyId;
                        //drInvoice["PartyId"] = voucherVM.PartyId;
                        //drInvoice["PartyPlantId"] = voucherVM.PartyPlantId;
                        //drInvoice["Amount"] = voucherVM.Amount;
                        drInvoice["UpdatedBy"] = voucher.AddedBy;
                        drInvoice["UpdatedDate"] = voucher.AddedDate;
                        drInvoice["UpdatedFromIP"] = voucher.AddedFromIP;

                        drInvoice.EndEdit();
                    }
                




                // Set to Financing
                financing.VoucherId = voucher.Id;

                // INSERT INTO FinancingDetail
                var investmentDetail = new FinancingDetail
                {
                    Amount = financing.Amount,
                };
                // Investment from side Voucher detail row.
                var voucherDetailFrom = new VoucherDetail
                {
                    PartyType = financing.PartyType,
                    PaymentSource = financing.PaymentSource
                };

                // Investment to side Voucher detail row.

                var voucherDetailLoanPayment = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                _financingService.InsertFinancingDetail(financing, investmentDetail);
                var currentVoucherDetailId = 1;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    #region From

                    var gl = accountCommonExtensionService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);

                    if (string.IsNullOrEmpty(gl["LiabilityGLId"].ToString()))
                        throw new CustomException("This Transaction Type GL not Found!");
                    if (string.IsNullOrEmpty(gl["LiabilityActivityId"].ToString()))
                        throw new CustomException("This Transaction Type Activity not Found!");
                    investmentDetail.GLGeneralInfoId = gl["LiabilityGLId"].ToString();
                    investmentDetail.BudgetMasterId = gl["LiabilityBudgetMasterId"].ToString();
                    investmentDetail.ActivityId = gl["LiabilityActivityId"].ToString();


                    voucherDetailFrom.FinancingDetailId = investmentDetail.Id;
                    voucherDetailFrom.CrAmount = voucherVM.Amount;
                    voucherDetailFrom.GLGeneralInfoId = investmentDetail.GLGeneralInfoId;
                    voucherDetailFrom.BudgetMasterId = investmentDetail.BudgetMasterId;
                    voucherDetailFrom.ActivityId = investmentDetail.ActivityId;
                    voucherDetailFrom.BankMasterId = voucherVM.BankMasterId;

                    AuditService.AddedLog(voucherDetailFrom);
                    voucherDetailFrom.ModelState = ModelState.Added;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailFrom.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate = Math.Round(voucherDetailVMList.Sum(r => r.CompanyCurrencyRate), 4) / voucherDetailVMList.Count(),
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, Math.Round(voucherDetailVMList.Sum(r => r.CompanyCurrencyRate), 4) / voucherDetailVMList.Count()),
                        CrAmount = Math.Round(voucherDetailVMList.Sum(r => r.Amount * r.CompanyCurrencyRate), 4)
                    });
                    totalAmountCr += voucherDetailFrom.CrAmount;
                    #endregion From
                    var currentInvoiceWriteOffDetailId = 0;
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        #region To

                        var invoice = _invoiceRepository.Find(voucherDetailVM.InvoiceId);
                        var invoiceDetail = _invoiceDetailRepository.Find(voucherDetailVM.InvoiceDetailId);

                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");

                        invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                        if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                            throw new CustomException("Received amount can not cross balance amount.");

                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoiceDetail.UpdatedBy = voucher.AddedBy;
                        invoiceDetail.UpdatedDate = voucher.AddedDate;
                        invoiceDetail.UpdatedFromIP = voucher.AddedFromIP;
                        _invoiceDetailRepository.Update(invoiceDetail);

                        invoice.WrittenOffAmount += voucherDetailVM.Amount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = voucher.AddedBy;
                        invoice.UpdatedDate = voucher.AddedDate;
                        invoice.UpdatedFromIP = voucher.AddedFromIP;
                        _invoiceRepository.Update(invoice);

                        // INSERT INTO InvoiceDetail

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
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration
                        };
                        currentInvoiceWriteOffDetailId++;
                        InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                        var voucherDetailTo = new VoucherDetail
                        {
                            PartyType = voucherVM.PartyType
                        };
                        //if (voucherVM.PartyType == PartyType.Vendor.ToString())
                        //{
                        //    if (string.IsNullOrEmpty(financing.PartyId))
                        //        throw new CustomException("Vendor Id not found!");

                        //    voucherDetailTo.PartyId = financing.PartyId;
                        //    voucherDetailTo.PartyPlantId = financing.PartyPlantId;
                        //    voucherDetailTo.TrnNature = TransactionNature.Vendor.ToString();
                        //}

                        voucherDetailTo.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                        voucherDetailTo.ActivityId = voucherDetailVM.ActivityId;
                        voucherDetailTo.TrnNature = TransactionNature.ToVendor.ToString();
                        voucherDetailTo.InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id;
                        voucherDetailTo.DrAmount = voucherDetailVM.Amount;

                        currentVoucherDetailId++;
                        AuditService.AddedLog(voucherDetailTo);
                        voucherDetailTo.ModelState = ModelState.Added;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailTo.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailTo.DrAmount
                        });
                        totalAmountDr += voucherDetailTo.DrAmount;
                        if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = voucherDetailVM.CompanyCurrencyRate * voucherDetailVM.Amount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }


                        #endregion To


                    }


                }

                // INSRT INTO GLTransactionDetail TABLE From
                if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                    {
                        BankMasterId = voucherDetailFrom.BankMasterId,
                        CashMasterId = voucherDetailFrom.CashMasterId,
                        CrAmount = Math.Round(voucherDetailVMList.Sum(r => r.Amount * r.CompanyCurrencyRate), 4),
                        SourceType = voucherDetailFrom.PaymentSource
                    });
                }
                // INSRT INTO GLTransactionDetail TABLE To

                if (voucherVM.IsSchedule)
                {
                    foreach (var item in financingScheduleVMList)
                    {
                        var financingSchedule = new FinancingSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance
                        };
                        _financingService.InsertFinancingSchedule(financing, financingSchedule);
                    }
                }
                string sql1 = "SELECT * FROM [trn].[FinancingSubsequentTransaction] WHERE Id='" + voucherVM.Id + "'";

                objCon.OpenDataSetThroughAdapter(sql1, out dsMasterFSTran, false, "1");

                if (dsMasterFSTran.Tables[0].Rows.Count == 0)
                {
                    DataRow dr1 = dsMasterFSTran.Tables[0].NewRow();
                    dr1["CompanyGroupId"] = voucherVM.CompanyGroupId;
                    dr1["CompanyId"] = voucherVM.CompanyId;
                    dr1["PlantId"] = voucherVM.PlantId;
                    dr1["EntityId"] = voucherVM.EntityId;
                    dr1["VoucherTypeId"] = voucherVM.VoucherTypeId;
                    dr1["FinancingId"] = financing.Id;
                    //dr1["PartyId"] = voucherVM.PartyId;
                    //dr1["PartyPlantId"] = voucherVM.PartyPlantId;
                    dr1["PartyType"] = "Bank";
                    dr1["CurrencyId"] = voucherVM.CurrencyId;
                    dr1["Amount"] = voucherVM.Amount;
                    dr1["VoucherDate"] = voucherVM.VoucherDate;
                    dr1["PostingDate"] = voucherVM.PostingDate;
                    dr1["DocDate"] = voucherVM.DocDate;
                    dr1["DocRefNo"] = voucherVM.DocRefNo;
                    dr1["TransactionType"] = LoanTransactionType.Loan.ToString();
                    dr1["Narration"] = voucherVM.Narration;
                    dr1["SourceType"] = voucherVM.SourceType.ToString();
                    dr1["IsPark"] = voucherVM.IsPark;
                    dr1["IsPosted"] = false;
                    dr1["Archive"] = false;
                    dr1["Id"] = "SL" + GetLoanInterestPayablePK();
                    dr1["VoucherId"] = voucher.Id;
                    dr1["VoucherDetailId"] = voucherDetailFrom.Id;
                    dr1["AddedBy"] = voucher.AddedBy;
                    dr1["AddedDate"] = voucher.AddedDate;
                    dr1["AddedFromIP"] = voucher.AddedFromIP;
                    dsMasterFSTran.Tables[0].Rows.Add(dr1);
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();

                flag = false;
                _unitOfWork.Commit();
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterFSTran, dsMaster);

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
        public string ParkAutoLoanInvoiceDifferentCurrency(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> financingScheduleVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService accountCommonExtensionService = new AccountCommonExtensionService();
                accountCommonExtensionService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                accountCommonExtensionService.CheckingFiscalYearPeriod(voucherVM);
                accountCommonExtensionService.CheckingTaxYearPeriod(voucherVM);



                _unitOfWork.BeginTransaction();
                flag = true;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster, dsMasterFSTran;
                voucherVM.Amount = voucherVM.BankBookAmount;
                // INSERT INTO Financing TABLE
                var financing = _financingService.InsertFinancing(new Financing
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    CurrencyId = voucherVM.CurrencyId,
                    FinancingTypeId = voucherVM.FinancingTypeId,
                    //BankMasterId = voucherVM.BankMasterId,
                    OtherBankMasterId = voucherVM.BankMasterId,
                    CashMasterId = voucherVM.CashMasterId,
                    EmployeeId = voucherVM.EmployeeId,
                    //PartyId = voucherVM.PartyId,
                    PartyType = "Bank",
                    PostingDate = voucherVM.PostingDate,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    SourceType = voucherVM.SourceType,
                    PaymentSource = voucherVM.PaymentSource,
                    Amount = voucherVM.Amount,
                    LifeOfYear = voucherVM.LifeOfYear,
                    NoOfInstallmentPerYear = voucherVM.NoOfInstallmentPerYear,
                    TotalNoOfInstallment = voucherVM.TotalNoOfInstallment,
                    ProfitRate = voucherVM.ProfitRate,
                    ProfitAmount = voucherVM.ProfitAmount,
                    RepaymentStartDate = voucherVM.RepaymentStartDate,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    IsPark = voucherVM.IsPark,
                    //PartyPlantId = voucherVM.PartyPlantId,
                    TransactionType = voucherVM.TransactionType,
                    IsSchedule = voucherVM.IsSchedule,
                    InvoiceTaggingWithLCMasterId = voucherVM.LoanAgainstAcceptanceId
                });

                var voucher = _voucherService.InsertVoucher(voucherVM);
                voucherVM.AddedBy = voucher.AddedBy;
                voucherVM.AddedDate = voucher.AddedDate;
                voucherVM.AddedFromIP = voucher.AddedFromIP;
                voucherVM.VoucherId = voucher.Id;
                var invoiceWriteOff = InsertInvoiceWriteOff(voucherVM);

                objCon = new ConnectionManager.DAL.ConManager("1");
               
                string sql = "SELECT * FROM InvoiceTaggingWithLCMaster WHERE Id='" + voucherVM.LoanAgainstAcceptanceId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (voucherVM.LoanAgainstAcceptanceId != null)
                {

                    DataRow drInvoice = dsMaster.Tables[0].DefaultView[0].Row;

                    drInvoice.BeginEdit();

                    drInvoice["VoucherId"] = voucher.Id;
                    //drInvoice["BankMasterId"] = voucherVM.BankMasterId;
                    //drInvoice["CompanyGroupId"] = voucherVM.CompanyGroupId;
                    //drInvoice["CompanyId"] = voucherVM.CompanyId;
                    //drInvoice["PlantId"] = voucherVM.PlantId;
                    //drInvoice["CurrencyId"] = voucherVM.CurrencyId;
                    //drInvoice["PartyId"] = voucherVM.PartyId;
                    //drInvoice["PartyPlantId"] = voucherVM.PartyPlantId;
                    //drInvoice["Amount"] = voucherVM.Amount;
                    drInvoice["UpdatedBy"] = voucher.AddedBy;
                    drInvoice["UpdatedDate"] = voucher.AddedDate;
                    drInvoice["UpdatedFromIP"] = voucher.AddedFromIP;

                    drInvoice.EndEdit();
                }





                // Set to Financing
                financing.VoucherId = voucher.Id;

                // INSERT INTO FinancingDetail
                var investmentDetail = new FinancingDetail
                {
                    Amount = financing.Amount,
                };
                // Investment from side Voucher detail row.
                var voucherDetailFrom = new VoucherDetail
                {
                    PartyType = financing.PartyType,
                    PaymentSource = financing.PaymentSource
                };

                // Investment to side Voucher detail row.

                var voucherDetailLoanPayment = new VoucherDetail
                {
                    PartyType = voucherVM.PartyType
                };
                _financingService.InsertFinancingDetail(financing, investmentDetail);
                var currentVoucherDetailId = 1;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;
                if (financing.TransactionType == TransactionType.LoanTaken.ToString())
                {
                    #region From

                    var gl = accountCommonExtensionService.GetInvestmentGL(financing.CompanyId, financing.FinancingTypeId);

                    if (string.IsNullOrEmpty(gl["LiabilityGLId"].ToString()))
                        throw new CustomException("This Transaction Type GL not Found!");
                    if (string.IsNullOrEmpty(gl["LiabilityActivityId"].ToString()))
                        throw new CustomException("This Transaction Type Activity not Found!");
                    investmentDetail.GLGeneralInfoId = gl["LiabilityGLId"].ToString();
                    investmentDetail.BudgetMasterId = gl["LiabilityBudgetMasterId"].ToString();
                    investmentDetail.ActivityId = gl["LiabilityActivityId"].ToString();


                    voucherDetailFrom.FinancingDetailId = investmentDetail.Id;
                    voucherDetailFrom.CrAmount = voucherVM.Amount;
                    voucherDetailFrom.GLGeneralInfoId = investmentDetail.GLGeneralInfoId;
                    voucherDetailFrom.BudgetMasterId = investmentDetail.BudgetMasterId;
                    voucherDetailFrom.ActivityId = investmentDetail.ActivityId;
                    voucherDetailFrom.BankMasterId = voucherVM.BankMasterId;

                    AuditService.AddedLog(voucherDetailFrom);
                    voucherDetailFrom.ModelState = ModelState.Added;
                    _voucherService.InsertVoucherDetail(voucher, voucherDetailFrom, currentVoucherDetailId);
                    _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailFrom, new VoucherDetailCurrency
                    {
                        ParallelCurrencyId = companyCurrencyId,
                        FromCurrencyId = voucherDetailFrom.CurrencyId,
                        ToCurrencyId = companyCurrencyId,
                        ToCurrencyRate =1,
                        ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailFrom.CurrencyId, companyCurrencyId, Math.Round(voucherDetailVMList.Sum(r => r.CompanyCurrencyRate), 4) / voucherDetailVMList.Count()),
                        CrAmount = Math.Round(voucherVM.Amount, 4)
                    });
                    totalAmountCr += voucherDetailFrom.CrAmount;
                    #endregion From
                    var currentInvoiceWriteOffDetailId = 0;
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        #region To

                        var invoice = _invoiceRepository.Find(voucherDetailVM.InvoiceId);
                        var invoiceDetail = _invoiceDetailRepository.Find(voucherDetailVM.InvoiceDetailId);

                        if (null == invoiceDetail)
                            throw new CustomException("Invoice not found!");

                        invoiceDetail.WrittenOffAmount += voucherDetailVM.Amount;

                        if (invoiceDetail.NetAmount < invoiceDetail.WrittenOffAmount)
                            throw new CustomException("Received amount can not cross balance amount.");

                        invoiceDetail.IsWrittenOff = invoiceDetail.NetAmount == invoiceDetail.WrittenOffAmount;
                        invoiceDetail.UpdatedBy = voucher.AddedBy;
                        invoiceDetail.UpdatedDate = voucher.AddedDate;
                        invoiceDetail.UpdatedFromIP = voucher.AddedFromIP;
                        _invoiceDetailRepository.Update(invoiceDetail);

                        invoice.WrittenOffAmount += voucherDetailVM.Amount;
                        invoice.IsWrittenOff = invoice.Amount == invoice.WrittenOffAmount;
                        invoice.UpdatedBy = voucher.AddedBy;
                        invoice.UpdatedDate = voucher.AddedDate;
                        invoice.UpdatedFromIP = voucher.AddedFromIP;
                        _invoiceRepository.Update(invoice);

                        // INSERT INTO InvoiceDetail

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
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherVM.Narration
                        };
                        currentInvoiceWriteOffDetailId++;
                        InsertInvoiceWriteOffDetail(invoiceWriteOff, invoiceWriteOffDetail, currentInvoiceWriteOffDetailId);

                        var voucherDetailTo = new VoucherDetail
                        {
                            PartyType = voucherVM.PartyType
                        };
                        if (voucherVM.PartyType == PartyType.Vendor.ToString())
                        {
                            if (string.IsNullOrEmpty(voucherVM.PartyId))
                                throw new CustomException("Vendor Id not found!");

                            voucherDetailTo.PartyId = voucherVM.PartyId;
                            voucherDetailTo.PartyPlantId = voucherVM.PartyPlantId;
                            voucherDetailTo.TrnNature = TransactionNature.Vendor.ToString();
                        }

                        voucherDetailTo.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                        voucherDetailTo.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                        voucherDetailTo.ActivityId = voucherDetailVM.ActivityId;
                        voucherDetailTo.TrnNature = TransactionNature.ToVendor.ToString();
                        voucherDetailTo.InvoiceWriteOffDetailId = invoiceWriteOffDetail.Id;
                        voucherDetailTo.DrAmount = voucherVM.Amount;

                        currentVoucherDetailId++;
                        AuditService.AddedLog(voucherDetailTo);
                        voucherDetailTo.ModelState = ModelState.Added;
                        _voucherService.InsertVoucherDetail(voucher, voucherDetailTo, currentVoucherDetailId);

                        _voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTo, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucherDetailTo.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherDetailVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherDetailTo.CurrencyId, companyCurrencyId, voucherDetailVM.CompanyCurrencyRate),
                            DrAmount = voucherVM.Amount
                    });
                        totalAmountDr += voucherDetailTo.DrAmount;
                        if (!string.IsNullOrEmpty(voucherDetailTo.BankMasterId) || !string.IsNullOrEmpty(voucherDetailTo.CashMasterId))
                        {
                            _voucherService.InsertGLTransactionDetail(voucherDetailTo, new GLTransactionDetail
                            {
                                BankMasterId = voucherDetailTo.BankMasterId,
                                CashMasterId = voucherDetailTo.CashMasterId,
                                DrAmount = voucherDetailVM.Amount,
                                SourceType = voucherDetailTo.PaymentSource
                            });
                        }


                        #endregion To


                    }


                }

                // INSRT INTO GLTransactionDetail TABLE From
                if (!string.IsNullOrEmpty(voucherDetailFrom.BankMasterId) || !string.IsNullOrEmpty(voucherDetailFrom.CashMasterId))
                {
                    _voucherService.InsertGLTransactionDetail(voucherDetailFrom, new GLTransactionDetail
                    {
                        BankMasterId = voucherDetailFrom.BankMasterId,
                        CashMasterId = voucherDetailFrom.CashMasterId,
                        CrAmount = Math.Round(voucherVM.Amount, 4),
                        SourceType = voucherDetailFrom.PaymentSource
                    });
                }

                string sql1 = "SELECT * FROM [trn].[FinancingSubsequentTransaction] WHERE Id='" + voucherVM.Id + "'";

                objCon.OpenDataSetThroughAdapter(sql1, out dsMasterFSTran, false, "1");

                if (dsMasterFSTran.Tables[0].Rows.Count == 0)
                {
                    DataRow dr1 = dsMasterFSTran.Tables[0].NewRow();
                    dr1["CompanyGroupId"] = voucherVM.CompanyGroupId;
                    dr1["CompanyId"] = voucherVM.CompanyId;
                    dr1["PlantId"] = voucherVM.PlantId;
                    dr1["EntityId"] = voucherVM.EntityId;
                    dr1["VoucherTypeId"] = voucherVM.VoucherTypeId;
                    dr1["FinancingId"] = financing.Id;
                    //dr1["PartyId"] = voucherVM.PartyId;
                    //dr1["PartyPlantId"] = voucherVM.PartyPlantId;
                    dr1["PartyType"] = "Bank";
                    dr1["CurrencyId"] = voucherVM.CurrencyId;
                    dr1["Amount"] = voucherVM.Amount;
                    dr1["VoucherDate"] = voucherVM.VoucherDate;
                    dr1["PostingDate"] = voucherVM.PostingDate;
                    dr1["DocDate"] = voucherVM.DocDate;
                    dr1["DocRefNo"] = voucherVM.DocRefNo;
                    dr1["TransactionType"] = LoanTransactionType.Loan.ToString();
                    dr1["Narration"] = voucherVM.Narration;
                    dr1["SourceType"] = voucherVM.SourceType.ToString();
                    dr1["IsPark"] = voucherVM.IsPark;
                    dr1["IsPosted"] = false;
                    dr1["Archive"] = false;
                    dr1["Id"] = "SL" + GetLoanInterestPayablePK();
                    dr1["VoucherId"] = voucher.Id;
                    dr1["VoucherDetailId"] = voucherDetailFrom.Id;
                    dr1["AddedBy"] = voucher.AddedBy;
                    dr1["AddedDate"] = voucher.AddedDate;
                    dr1["AddedFromIP"] = voucher.AddedFromIP;
                    dsMasterFSTran.Tables[0].Rows.Add(dr1);
                }

                // INSRT INTO GLTransactionDetail TABLE To

                if (voucherVM.IsSchedule)
                {
                    foreach (var item in financingScheduleVMList)
                    {
                        var financingSchedule = new FinancingSchedule
                        {
                            InstallmentAmount = item.InstallmentAmount,
                            InstallmentDate = item.InstallmentDate,
                            InstallmentNo = item.InstallmentNo,
                            PrincipalAmount = item.PrincipalAmount,
                            ProfitAmount = item.ProfitAmount,
                            ScheduleNo = item.ScheduleNo,
                            Balance = item.Balance
                        };
                        _financingService.InsertFinancingSchedule(financing, financingSchedule);
                    }
                }

                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");

                _unitOfWork.SaveChanges();

                flag = false;
                _unitOfWork.Commit();
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterFSTran, dsMaster);

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

        private string GetLoanInterestPayablePK()
        {
            return _pkGeneratorService.GetAutoNumber("FinancingSubsequentTransaction", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
      
    }
}