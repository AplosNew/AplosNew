using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.FixedAssets;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.FixedAssets;
using Library.Service.Properties;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Library.Model.Currencies;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Service.Core;
using Library.ViewModel.Accounts;
using Library.Model.Advances;
using Library.Service.Systems;
using System.IO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System.Drawing;
using System.Collections.Specialized;

namespace Library.Accounting.FixedAssets
{
    public class FixedAssetDisposeService
    {
        private readonly ISqlRepository _sqlRepository;
        public FixedAssetDisposeService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;

        }
        private void AddNewRow<T>(DataTable dt, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dt.Rows.Add(dr);
        }
        private void EditRow<T>(DataRow dr, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (item.ToUpper() == "ID")
                        continue;

                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();

        }
        private void EditRow(DataSet ds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].DefaultView[0].Row;

                dr.BeginEdit();
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
                dr.EndEdit();
            }
            clsStaticInfo obj = new clsStaticInfo();
            obj.SaveDataSets(ds);

        }
        public void InsertAdvance(Advance advance,  ref DataSet advanceds)
        {
            advance.Id = advance.Id;
            if (string.IsNullOrEmpty(advance.AddedBy))
                AuditService.AddedLog(advance);
            if (advanceds == null || advanceds.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[Advance] where 1=2", out advanceds);
            }

            AddNewRow<Advance>(advanceds.Tables[0], advance);
        }
        public void InsertAdvanceDetail(Advance advance, AdvanceDetail advanceDetail, ref DataSet advanceDetailds)
        {
            advanceDetail.Id = advanceDetail.Id;
            if (string.IsNullOrEmpty(advanceDetail.AddedBy))
                AuditService.AddedLog(advanceDetail);
            if (advanceDetailds == null || advanceDetailds.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[AdvanceDetail] where 1=2", out advanceDetailds);
            }

            AddNewRow<AdvanceDetail>(advanceDetailds.Tables[0], advanceDetail);
        }
        public void InsertEmployeeSubsequentTransaction(Advance advance, EmployeeSubsequentTransaction employeeSubsequentTransaction, ref DataSet employeeSubsequentTransactionds)
        {
            employeeSubsequentTransaction.Id = employeeSubsequentTransaction.Id;
            if (string.IsNullOrEmpty(employeeSubsequentTransaction.AddedBy))
                AuditService.AddedLog(employeeSubsequentTransaction);
            if (employeeSubsequentTransactionds == null || employeeSubsequentTransactionds.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from [TRN].[EmployeeSubsequentTransaction] where 1=2", out employeeSubsequentTransactionds);
            }

            AddNewRow<EmployeeSubsequentTransaction>(employeeSubsequentTransactionds.Tables[0], employeeSubsequentTransaction);
        }
        public void InsertAdvanceReqSchedule(VoucherViewModel voucherVM, AdvanceReqSchedule financingSchedule, string requisitionId, ref DataSet advRewSchedule)
        {
            financingSchedule.Id = financingSchedule.Id;
            financingSchedule.RequisitionId = requisitionId;
            financingSchedule.EmployeeSalaryAdvanceId = voucherVM.Id;
            if (string.IsNullOrEmpty(financingSchedule.AddedBy))
                AuditService.AddedLog(financingSchedule);
            //financingSchedule.AddedBy = voucherVM.AddedBy;
            //financingSchedule.AddedDate = voucherVM.AddedDate;
            //financingSchedule.AddedFromIP = voucherVM.AddedFromIP;
            if (advRewSchedule == null || advRewSchedule.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from dbo.AdvanceReqSchedule where 1=2", out advRewSchedule);
            }

            AddNewRow<AdvanceReqSchedule>(advRewSchedule.Tables[0], financingSchedule);
        }

        public void UpdateFixedAssetRegisterDispose(FixedAssetRegisterDisposed voucherVM, ref DataSet frdispose)
        {

            if (frdispose == null || frdispose.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.FixedAssetRegisterDisposed where Id='" + voucherVM.Id + "'", out frdispose);

            }

            EditRow<FixedAssetRegisterDisposed>(frdispose.Tables[0].Rows[0], voucherVM);
        }
        public void UpdateFixedAssetRegister(FixedAssetRegister voucherVM, ref DataSet fixedRegister)
        {

            if (fixedRegister == null || fixedRegister.Tables.Count == 0)
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from TRN.FixedAssetRegister where Id='" + voucherVM.Id + "'", out fixedRegister);

                DataView dv = new DataView(fixedRegister.Tables[0]);
                dv.RowFilter = "Id='" + voucherVM.Id + "'";

                if (dv.Count > 0)
                {
                    DataRow drmo = dv[0].Row;

                    drmo.BeginEdit();

                    drmo["DisposedVoucherId"] = voucherVM.DisposedVoucherId;
                    drmo["DisposedDate"] = voucherVM.DisposedDate;

                    drmo.EndEdit();

                }

            }
        }
        public void InsertFixedAssetDisposeSalesPosting(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<FixedAssetRegisterDisposedDetail> farDisposeDetailList
            , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                DataSet _invoiceData = null;
                DataSet _invoiceDetailData = null;
                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                DataSet _frDisposeData = null;
                DataSet _fixedAssetRegisterData = null;
                DataSet _advanceReqScheData = null;

                var invoice = new Invoice
                {
                    Amount = farDisposeDetailList.Sum(r=>r.NegotiationValue),
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    InvoiceNo = voucherVM.InvoiceNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    IsExcludingTax = voucherVM.IsExcludingTax,
                    IsSplit = voucherVM.IsSplit,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetDisposeJournal.ToString(),

                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate,
                    IsPark = false
                };

                invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                invoice.RevisedDueDate = voucherVM.MatureDate;
                invoice.ActualDueDate = voucherVM.MatureDate;
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = voucherVM.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = "Posting",//voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetDisposeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                AuditService.PostedLog(voucher);
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);
               
                    invoice.VoucherId = voucher.Id;
                    invoice.PartyType = PartyType.Customer.ToString();
                    _accountsCommonService.InsertInvoice(invoice, out DataSet _invoicedataSet);

                var currentVoucherDetaiRecord = 0;
                var currentInvoiceDetail = 0;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {

                        // INSERT INTO InvoiceDetail

                        var invoiceDetail = new InvoiceDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = Math.Round((voucherDetailVM.Amount / voucherVM.CompanyCurrencyRate),4),
                            NetAmount = Math.Round((voucherDetailVM.Amount / voucherVM.CompanyCurrencyRate), 4),
                            TaxAmount = 0,
                            AddedBy = invoice.AddedBy,
                            AddedDate = invoice.AddedDate,
                            AddedFromIP = invoice.AddedFromIP,
                            Archive = invoice.Archive,
                            InvoiceId = invoice.Id,
                        };
                        if (voucherVM.PartyId != null && voucherVM.Status == "Sales" && voucherDetailVM.OtherName== "A/R")
                        {
                            currentInvoiceDetail++;
                            _accountsCommonService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail, ref _invoiceDetailData);

                        }

                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = Math.Round((voucherDetailVM.Amount / voucherVM.CompanyCurrencyRate),4),
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            InvoiceDetailId = invoiceDetail.Id
                        };
                        //totalAmountDr += voucherDr.DrAmount;
                        totalAmountDr += voucherDetailVM.Amount;
                        if (voucherDetailVM.OtherName == "A/R")
                        {
                            voucherDr.PartyId = voucherVM.PartyId;
                            voucherDr.PartyPlantId = voucherVM.PartyPlantId;
                            voucherDr.PartyType = PartyType.Customer.ToString();
                        }
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucher.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucher.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailVM.Amount
                        }, ref _drvDetailCurrencyData);
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = Math.Round((voucherDetailVM.Amount / voucherVM.CompanyCurrencyRate),4),
                        };
                        //totalAmountCr += voucherCr.CrAmount;
                        totalAmountCr += voucherDetailVM.Amount;
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucher.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucher.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherDetailVM.Amount
                        }, ref _crvDetailCurrencyData);
                    }
                }
                if (farDisposeDetailList != null)
                {
                    foreach (var item in farDisposeDetailList)
                    {

                        var faRegisterDispose = new FixedAssetRegisterDisposed
                        {
                            DisposedVoucherId = voucher.Id,
                            Id = item.FixedAssetRegisterDisposedId,
                            Status = voucherVM.Status,
                            EmployeeId = voucherVM.EmployeeId,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            Remarks = voucherVM.Remarks,
                            DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId,
                            InvoicingByAddress = voucherVM.InvoicingByAddress,
                            DeliveryByAddress = voucherVM.DeliveryByAddress,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            CurrencyId = voucherVM.CurrencyId,
                            DocDate = voucherVM.DocDate,
                            AddedBy = item.AddedBy,
                            AddedDate = item.AddedDate,
                            AddedFromIP = item.AddedFromIP
                        };
                        UpdateFixedAssetRegisterDispose(faRegisterDispose, ref _frDisposeData);

                        var faRegister = new FixedAssetRegister
                        {
                            DisposedVoucherId = voucher.Id,
                            DisposedDate = voucher.PostingDate,
                            Id = item.FixedAssetRegisterId
                        };
                        UpdateFixedAssetRegister(faRegister, ref _fixedAssetRegisterData);

                    }
                }
                if (advanceSalarySchedulelist != null)
                {
                    foreach (var item in advanceSalarySchedulelist)
                    {
                        var advanceReqSchedule = new AdvanceReqSchedule
                        {
                            Id = _accountsCommonService.MakePK(voucherVM.Id, item.InstallmentNo, 3),
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
                        InsertAdvanceReqSchedule(voucherVM, advanceReqSchedule, voucherVM.RequisitionId, ref _advanceReqScheData);
                    }
                }
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                clsStaticInfo objApp = new clsStaticInfo();
                //objApp.SaveDataSets(_vdataset, _invoicedataSet, _invoiceDetailData, _crvDetailData, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData, _frDisposeData, _fixedAssetRegisterData, _advanceReqScheData
                // need to test 
                objApp.SaveDataSets(_vdataset, _invoicedataSet, _invoiceDetailData, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData, _frDisposeData, _fixedAssetRegisterData, _advanceReqScheData
                    );
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void InsertFixedAssetDisposeScrapPosting(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<FixedAssetRegisterDisposedDetail> farDisposeDetailList
           , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                DataSet _frDisposeData = null;
                DataSet _fixedAssetRegisterData = null;
                DataSet _advanceReqScheData = null;

              
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = "Posting",//voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetDisposeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                AuditService.PostedLog(voucher);
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;
                var currentInvoiceDetail = 0;

                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {

                        // INSERT INTO InvoiceDetail


                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            DrAmount = voucherDr.DrAmount 
                        }, ref _drvDetailCurrencyData);
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion =1,
                            CrAmount = voucherCr.CrAmount 
                        }, ref _crvDetailCurrencyData);
                    }
                }
                if (farDisposeDetailList != null)
                {
                    foreach (var item in farDisposeDetailList)
                    {

                        var faRegisterDispose = new FixedAssetRegisterDisposed
                        {
                            DisposedVoucherId = voucher.Id,
                            Id = item.FixedAssetRegisterDisposedId,
                            Status = voucherVM.Status,
                            Remarks = voucherVM.Remarks,
                            EmployeeId = voucherVM.EmployeeId,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            DocDate = voucherVM.DocDate,
                            AddedBy = item.AddedBy,
                            AddedDate = item.AddedDate,
                            AddedFromIP = item.AddedFromIP
                        };
                        UpdateFixedAssetRegisterDispose(faRegisterDispose, ref _frDisposeData);

                        var faRegister = new FixedAssetRegister
                        {
                            DisposedVoucherId = voucher.Id,
                            DisposedDate = voucher.PostingDate,
                            Id = item.FixedAssetRegisterId
                        };
                        UpdateFixedAssetRegister(faRegister, ref _fixedAssetRegisterData);

                    }
                }
                if (advanceSalarySchedulelist != null)
                {
                    foreach (var item in advanceSalarySchedulelist)
                    {
                        var advanceReqSchedule = new AdvanceReqSchedule
                        {
                            Id = _accountsCommonService.MakePK(voucherVM.Id, item.InstallmentNo, 3),
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
                        InsertAdvanceReqSchedule(voucherVM, advanceReqSchedule, voucherVM.RequisitionId, ref _advanceReqScheData);
                    }
                }
                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _crvDetailData, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData, _frDisposeData, _fixedAssetRegisterData, _advanceReqScheData
                    );
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public void InsertCapitalizeAssetDisposeScrapPosting(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<FixedAssetRegisterDisposedDetail> farDisposeDetailList
           , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                DataSet _advanceData = null;
                DataSet _advanceDetailData = null;
                DataSet _employeeSubsequentTransactionData = null;


                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = "Posting",//voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    EntityId = voucherVM.EntityId,
                    SourceType = SourceType.FixedAssetDisposeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                AuditService.PostedLog(voucher);
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;
                var currentInvoiceDetail = 0;
                string AdvanceDetailId = null;
                string EmployeeId = null;
                

                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {
                         AdvanceDetailId = null;
                         EmployeeId = null;
                        if (voucherDetailVM.OtherName == "Advance")
                        {
                            var advance = new Advance
                            {
                                Id = _accountsCommonService.GetAutoNumber(nameof(Advance), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                CompanyGroupId = voucherVM.CompanyGroupId,
                                CompanyId = voucherVM.CompanyId,
                                PlantId = voucherVM.PlantId,
                                EntityId = voucherVM.EntityId,
                                FiscalYearId = voucherVM.FiscalYearId,
                                FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                                TaxYearId = voucherVM.TaxYearId,
                                TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                                VoucherTypeId = voucherVM.VoucherTypeId,
                                VoucherId = voucher.Id,
                                CurrencyId = voucher.CurrencyId,
                                EmployeeId = voucherVM.EmployeeId,
                                PartyId = voucherVM.PartyId,
                                PartyPlantId = voucherVM.PartyPlantId,
                                ResponsiblePersonId = voucherVM.ResponsiblePersonId,
                                Amount = voucherDetailVM.Amount,
                                VoucherDate = voucherVM.VoucherDate,
                                PostingDate = voucherVM.PostingDate,
                                ReviewDate = voucherVM.ReviewDate,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = voucher.SourceType,
                                PartyType = "Employee",
                                SourceType = voucher.SourceType,
                                PaymentSource = voucherVM.PaymentSource,
                                BankMasterId = voucherVM.BankMasterId,
                                CashMasterId = voucherVM.CashMasterId,
                                JournalId = voucherVM.JournalId,
                                BankAmount = voucherVM.BankAmount,
                                EmployeeTransactionTypeId = voucherVM.EmployeeTransactionTypeId,
                                IsInterTransaction = voucherVM.IsInterTransaction,
                                FinancingTypeId = voucherVM.IsInterTransaction ? voucherVM.FinancingTypeId : null,
                                IsPark = voucherVM.IsPark,
                                JournalType = voucherVM.JournalType,
                                SettlementType = voucherVM.SettlementType,
                                RequisitionId = voucherVM.RequisitionId,
                                CompanyCurrencyRate = 1,
                                IsPosted = true,
                                POId = voucherVM.POId,
                                ContractId = voucherVM.ContractId,
                                MasterOrderId = voucherVM.MasterOrderId,
                                AdvanceGroupNo = voucherVM.AdvanceGroupNo
                            };
                            InsertAdvance(advance, ref _advanceData);
                            var advanceDetail = new AdvanceDetail
                            {
                                Id = _accountsCommonService.MakePK(advance.Id, 1),
                                AdvanceId = advance.Id,
                                PartyType = advance.PartyType,
                                CompanyId = advance.CompanyId,
                                EmployeeId = advance.EmployeeId,
                                PlantId = advance.PlantId,
                                PartyId = advance.PartyId,
                                PartyPlantId = advance.PartyPlantId,
                                PaymentType = voucherDetailVM.PaymentType,
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                AddedBy = advance.AddedBy,
                                AddedDate = advance.AddedDate,
                                AddedFromIP = advance.AddedFromIP,
                                Archive = advance.Archive,
                                Narration = voucher.SourceType,
                                Amount = voucherDetailVM.Amount,
                                NetAmount = voucherDetailVM.Amount,
                                BooksAmount = voucherDetailVM.Amount

                            };
                            InsertAdvanceDetail(advance, advanceDetail, ref _advanceDetailData);

                            var employeeSubsequentTransaction = new EmployeeSubsequentTransaction
                            {
                                Id = "ES" + _accountsCommonService.GetAutoNumber(nameof(EmployeeSubsequentTransaction), PKGeneratorEnum.Yearly, null, DateTime.Now),
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
                                Narration = voucher.SourceType,
                                SourceType = voucher.SourceType,
                                IsPark = voucherVM.IsPark,
                                VoucherId = voucher.Id,
                                VoucherDetailId = null,
                                PaymentSource = voucherVM.PaymentSource
                            };
                            InsertEmployeeSubsequentTransaction(advance, employeeSubsequentTransaction, ref _employeeSubsequentTransactionData);
                            AdvanceDetailId = advanceDetail.Id;
                            EmployeeId = advance.EmployeeId;
                        }
                        // INSERT INTO InvoiceDetail


                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            EmployeeId = EmployeeId,
                            AdvanceDetailId= AdvanceDetailId
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            DrAmount = voucherDr.DrAmount
                        }, ref _drvDetailCurrencyData);
                        
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            CrAmount = voucherCr.CrAmount
                        }, ref _crvDetailCurrencyData);
                    }
                }
                
                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _advanceData, _advanceDetailData, _employeeSubsequentTransactionData, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData);
                if (farDisposeDetailList != null)
                {
                    foreach (var item in farDisposeDetailList)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = "";
                        builderSql = @"UPDATE [TRN].[FixedAssetRegisterDisposed] SET DisposedVoucherId='" + voucher.Id + "' WHERE Id='" + item.FixedAssetRegisterDisposedId + "' ";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }
                }
                
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void InsertCapitalizeAssetDisposeSalesPosting(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
         , IEnumerable<FixedAssetRegisterDisposedDetail> farDisposeDetailList
         , IEnumerable<AdvanceReqSchedule> advanceSalarySchedulelist)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                DataSet _invoiceData = null;
                DataSet _invoiceDetailData = null;
                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                DataSet _frDisposeData = null;
                DataSet _fixedAssetRegisterData = null;
                DataSet _advanceReqScheData = null;

                var invoice = new Invoice
                {
                    Amount = voucherDetailVMList.Where(x => x.OtherName == "A/R").Sum(r => r.Amount),
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    InvoiceNo = voucherVM.InvoiceNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    IsExcludingTax = voucherVM.IsExcludingTax,
                    IsSplit = voucherVM.IsSplit,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetDisposeJournal.ToString(),

                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.CompanyCurrencyRate,
                    IsPark = false
                };

                invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                invoice.RevisedDueDate = voucherVM.MatureDate;
                invoice.ActualDueDate = voucherVM.MatureDate;
                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = voucherVM.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetDisposeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                AuditService.PostedLog(voucher);
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                invoice.VoucherId = voucher.Id;
                invoice.PartyType = PartyType.Customer.ToString();
                _accountsCommonService.InsertInvoice(invoice, out DataSet _invoicedataSet);

                var currentVoucherDetaiRecord = 0;
                var currentInvoiceDetail = 0;
                var totalAmountDr = 0.0M;
                var totalAmountCr = 0.0M;

                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {

                        // INSERT INTO InvoiceDetail

                        var invoiceDetail = new InvoiceDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = Math.Round((voucherDetailVM.Amount / voucherVM.CompanyCurrencyRate), 4),
                            NetAmount = Math.Round((voucherDetailVM.Amount / voucherVM.CompanyCurrencyRate), 4),
                            TaxAmount = 0,
                            AddedBy = invoice.AddedBy,
                            AddedDate = invoice.AddedDate,
                            AddedFromIP = invoice.AddedFromIP,
                            Archive = invoice.Archive,
                            InvoiceId = invoice.Id,
                        };
                        if (voucherVM.PartyId != null && voucherVM.Status == "Sales" && voucherDetailVM.OtherName == "A/R")
                        {
                            currentInvoiceDetail++;
                            _accountsCommonService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail, ref _invoiceDetailData);

                        }

                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = Math.Round((voucherDetailVM.Amount / voucherVM.CompanyCurrencyRate), 4),
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            InvoiceDetailId = invoiceDetail.Id
                        };
                        //totalAmountDr += voucherDr.DrAmount;
                        totalAmountDr += voucherDetailVM.Amount;
                        if (voucherDetailVM.OtherName == "A/R")
                        {
                            voucherDr.PartyId = voucherVM.PartyId;
                            voucherDr.PartyPlantId = voucherVM.PartyPlantId;
                            voucherDr.PartyType = PartyType.Customer.ToString();
                        }
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucher.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucher.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDetailVM.Amount
                        }, ref _drvDetailCurrencyData);
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = Math.Round((voucherDetailVM.Amount / voucherVM.CompanyCurrencyRate), 4),
                        };
                        //totalAmountCr += voucherCr.CrAmount;
                        totalAmountCr += voucherDetailVM.Amount;
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucher.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucher.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherDetailVM.Amount
                        }, ref _crvDetailCurrencyData);
                    }
                }
                
                if (advanceSalarySchedulelist != null)
                {
                    foreach (var item in advanceSalarySchedulelist)
                    {
                        var advanceReqSchedule = new AdvanceReqSchedule
                        {
                            Id = _accountsCommonService.MakePK(voucherVM.Id, item.InstallmentNo, 3),
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
                        InsertAdvanceReqSchedule(voucherVM, advanceReqSchedule, voucherVM.RequisitionId, ref _advanceReqScheData);
                    }
                }
                if (totalAmountDr != totalAmountCr)
                    throw new CustomException("Dr and Cr amount is not equal.");
                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _invoicedataSet, _invoiceDetailData, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData, _frDisposeData, _fixedAssetRegisterData, _advanceReqScheData);
                
                if (farDisposeDetailList != null)
                {
                    foreach (var item in farDisposeDetailList)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = "";
                        builderSql = @"UPDATE [TRN].[FixedAssetRegisterDisposed] SET DisposedVoucherId='" + voucher.Id + "' WHERE Id='" + item.FixedAssetRegisterDisposedId + "' ";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void InsertFixedAssetDepreciationPosting(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<FixedAssetDepreciationProcessVM> fixedAssetDepreciationList)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                var voucherDrId = "";
                

                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.DepreciationJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;
               

                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {

                        // INSERT INTO InvoiceDetail


                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            DrAmount = voucherDr.DrAmount
                        }, ref _drvDetailCurrencyData);
                        voucherDrId = voucherDr.Id;
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            CrAmount = voucherCr.CrAmount
                        }, ref _crvDetailCurrencyData);
                    }
                }
               
               
                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _crvDetailData, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData);
                if (fixedAssetDepreciationList != null)
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = @"UPDATE [TRN].[FixedAssetDepreciationProcess] SET VoucherDetailId='" + voucherDrId + "',DepreciationVoucherId='" + voucher.Id + "' WHERE FixedAssetMasterId='" + fixedAssetDepreciationList.FirstOrDefault().FixedAssetMasterId + "' AND DepreciationProcessDate='" + fixedAssetDepreciationList.FirstOrDefault().DepreciationProcessDate + "' ";
                    rdBuilder.Append(builderSql);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetCapitalizeAssetDisposeList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select frd.Id,frd.Id DisposeNo,cast(substring(frd.Id,3,8) as int)SlNo,frd.EmployeeId,ei.EmployeeName,D.UserName Department
									,frd.Status,frd.Remarks,frd.LorryNo,DG.UserName Designation,c.Code TrnCurrency,frd.IsPark,  c.Id trnCurrencyId,frd.ToCurrencyRate
									,format( frd.DocDate,'dd-MMM-yyyy')DocDate ,P.UserName CustomerName,frd.PartyId,frd.PartyPlantId 
									 ,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress,c.Code TrnPurchaseCurrency,v.VoucherNo
									,isnull( rdd.NegotiationValue,0)NegotiationValue
                                    ,isnull( rdd.BaseNagotiationValue,0)BaseNagotiationValue
                from TRN.FixedAssetRegisterDisposed frd 
				LEFT JOIN (SELECT sum(isnull( NegotiationValue,0))NegotiationValue,sum(isnull(BaseNagotiationValue,0))BaseNagotiationValue,FixedAssetRegisterDisposedId 
									FROM TRN.FixedAssetRegisterDisposedDetail GROUP BY FixedAssetRegisterDisposedId) rdd ON rdd.FixedAssetRegisterDisposedId=frd.Id
                left join dbo.EmployeeInformation ei on ei.SystemId=frd.EmployeeId
				left join ORG.Department D on D.Id=ei.DepartmentId
				left join HKP.Designation DG ON DG.Id=EI.GivenDesignationID
				LEFT JOIN HKP.Party P ON P.Id=FRD.PartyId
				LEFT JOIN HKP.PartyPlant PP ON PP.Id=FRD.PartyPlantId
	            LEFT JOIN SCS.Currency C ON C.Id =frd.CurrencyId
                LEFT JOIN TRN.Voucher V ON V.Id =frd.DisposedVoucherId
                ) AS TEMP WHERE " + strkey + " order by SlNo desc ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFixedAssetDisposeList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select frd.Id,frd.Id DisposeNo,cast(substring(frd.Id,3,8) as int)SlNo,fr.[Status]
									,frd.EmployeeId,ei.EmployeeName,D.UserName Department,DG.UserName Designation,fr.Remarks
									 ,c.Code TrnCurrency,frd.IsPark
									,  c.Id trnCurrencyId
									,format( frd.DocDate,'dd-MMM-yyyy')DocDate
                                    ,sum( ISNULL(FR.Price,0)) Price
									,sum( ISNULL(SAR.subAssetAmount,0) )SubAssetAmount
									,sum( ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)) PurchasePrice
									 ,sum( ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)-ISNULL(FR.ADBaseAmount,0)) NetBookValue 
								   , BC.Code BaseCurrency
                                    ,isnull( frd.ToCurrencyRate,0)CompanyCurrencyRate
                                    ,isnull( frd.ToCurrencyRate,0)ToCurrencyRate
									,sum( isnull(FR.FABaseAmount,0))FABaseAmount
									,sum(ISNULL(SAR.subAssetBaseAmount,0) )SubAssetBaseAmount
									,sum(isnull(FR.FABaseAmount,0) + ISNULL(SAR.subAssetBaseAmount,0)) PurchaseBaseAmount
									,sum(isnull( FR.ADBaseAmount,0))ADBaseAmount
                                    ,sum( isnull(FR.FABaseAmount,0)+ISNULL(SAR.subAssetBaseAmount,0)-ISNULL(FR.ADBaseAmount,0) )NetBaseBookValue 
										,sum(isnull( rdd.NegotiationValue,0))NegotiationValue
                                    ,sum(isnull( rdd.BaseNagotiationValue,0))BaseNagotiationValue

				,P.UserName CustomerName,frd.PartyId,frd.PartyPlantId ,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress,c.Code TrnPurchaseCurrency,v.VoucherNo
                --,IsOBBalance=case when FR.IsOpeningBalance=0 then 'No' Else 'Yes' End
                --,P.UserName Vendor
                from TRN.FixedAssetRegisterDisposed frd 
				join TRN.FixedAssetRegisterDisposedDetail rdd ON rdd.FixedAssetRegisterDisposedId=frd.Id
                left join TRN.FixedAssetRegister FR on FR.Id=rdd.FixedAssetRegisterId
                left join dbo.EmployeeInformation ei on ei.SystemId=frd.EmployeeId
				left join ORG.Department D on D.Id=ei.DepartmentId
				left join HKP.Designation DG ON DG.Id=EI.GivenDesignationID
				LEFT JOIN HKP.Party P ON P.Id=FRD.PartyId
				LEFT JOIN HKP.PartyPlant PP ON PP.Id=FRD.PartyPlantId
	            LEFT JOIN SCS.Currency C ON C.Id =frd.CurrencyId
	            LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId
                LEFT JOIN TRN.Voucher V ON V.Id =frd.DisposedVoucherId

                --LEFT JOIN HKP.Party P ON P.Id = FR.VendorId
                LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount ,ISNULL(Sum(BaseAmount),0) subAssetBaseAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
                    where fr.CompanyId='" + companyId+ @"' 
                    --AND frd.DisposedVoucherId IS NULL
                      group by fr.[Status],frd.Id ,frd.PartyId,frd.PartyPlantId, BC.Code,P.UserName,c.Id,c.Code,frd.IsPark,frd.ToCurrencyRate,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress
					 --,FR.IsOpeningBalance 
					 ,frd.DocDate,fr.Remarks,ei.EmployeeName,frd.EmployeeId	,P.UserName  ,frd.IsPark,D.UserName ,DG.UserName,v.VoucherNo	
                ) AS TEMP WHERE " + strkey+ " order by SlNo desc ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFixedAssetDisposeListForPosting(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select frd.Id,frd.Id DisposeNo,cast(substring(frd.Id,3,8) as int)SlNo,fr.[Status]
									,frd.EmployeeId,ei.EmployeeName,D.UserName Department,DG.UserName Designation,fr.Remarks
									 ,c.Code TrnCurrency,frd.IsPark
									,  c.Id trnCurrencyId
									,format( frd.DocDate,'dd-MMM-yyyy')DocDate
                                    ,sum( ISNULL(FR.Price,0)) Price
									,sum( ISNULL(SAR.subAssetAmount,0) )SubAssetAmount
									,sum( ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)) PurchasePrice
									 ,sum( ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)-ISNULL(FR.ADBaseAmount,0)) NetBookValue 
								   , BC.Code BaseCurrency
                                    ,isnull( frd.ToCurrencyRate,0)CompanyCurrencyRate
                                    ,isnull( frd.ToCurrencyRate,0)ToCurrencyRate
									,sum( isnull(FR.FABaseAmount,0))FABaseAmount
									,sum(ISNULL(SAR.subAssetBaseAmount,0) )SubAssetBaseAmount
									,sum(isnull(FR.FABaseAmount,0) + ISNULL(SAR.subAssetBaseAmount,0)) PurchaseBaseAmount
									,sum(isnull( FR.ADBaseAmount,0))ADBaseAmount
                                    ,sum( isnull(FR.FABaseAmount,0)+ISNULL(SAR.subAssetBaseAmount,0)-ISNULL(FR.ADBaseAmount,0) )NetBaseBookValue 
										,sum(isnull( rdd.NegotiationValue,0))NegotiationValue
                                    ,sum(isnull( rdd.BaseNagotiationValue,0))BaseNagotiationValue

				,P.UserName CustomerName,frd.PartyId,frd.PartyPlantId ,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress,c.Code TrnPurchaseCurrency
                --,IsOBBalance=case when FR.IsOpeningBalance=0 then 'No' Else 'Yes' End
                --,P.UserName Vendor
                from TRN.FixedAssetRegisterDisposed frd 
				join TRN.FixedAssetRegisterDisposedDetail rdd ON rdd.FixedAssetRegisterDisposedId=frd.Id
                left join TRN.FixedAssetRegister FR on FR.Id=rdd.FixedAssetRegisterId
                left join dbo.EmployeeInformation ei on ei.SystemId=frd.EmployeeId
				left join ORG.Department D on D.Id=ei.DepartmentId
				left join HKP.Designation DG ON DG.Id=EI.GivenDesignationID
				LEFT JOIN HKP.Party P ON P.Id=FRD.PartyId
				LEFT JOIN HKP.PartyPlant PP ON PP.Id=FRD.PartyPlantId
	            LEFT JOIN SCS.Currency C ON C.Id =frd.CurrencyId
	            LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId

                --LEFT JOIN HKP.Party P ON P.Id = FR.VendorId
                LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount ,ISNULL(Sum(BaseAmount),0) subAssetBaseAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
                    where fr.CompanyId='" + companyId + @"' 
                    AND frd.IsPark=1
                    --AND frd.DisposedVoucherId IS NULL
                       group by fr.[Status],frd.Id ,frd.PartyId,frd.PartyPlantId, BC.Code,P.UserName,c.Id,c.Code,frd.IsPark,frd.ToCurrencyRate,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress
					 --,FR.IsOpeningBalance 
					 ,frd.DocDate,fr.Remarks,ei.EmployeeName,frd.EmployeeId	,P.UserName  ,frd.IsPark,D.UserName ,DG.UserName	
                ) AS TEMP WHERE " + strkey + " order by SlNo desc ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizeAssetDisposeListForPosting(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select frd.Id,frd.Id DisposeNo,cast(substring(frd.Id,3,8) as int)SlNo,frd.EmployeeId,ei.EmployeeName,D.UserName Department
									,frd.Status,frd.Remarks,DG.UserName Designation,c.Code TrnCurrency,frd.IsPark,  c.Id trnCurrencyId,frd.ToCurrencyRate
									,format( frd.DocDate,'dd-MMM-yyyy')DocDate ,P.UserName CustomerName,frd.PartyId,frd.PartyPlantId 
									 ,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress,c.Code TrnPurchaseCurrency,v.VoucherNo
									,sum(isnull( rdd.NegotiationValue,0))NegotiationValue
                                    ,sum(isnull( rdd.BaseNagotiationValue,0))BaseNagotiationValue
                from TRN.FixedAssetRegisterDisposed frd 
				join TRN.FixedAssetRegisterDisposedDetail rdd ON rdd.FixedAssetRegisterDisposedId=frd.Id
                left join  TRN.AssetRegister AR on AR.Id=rdd.AssetRegisterId
                left join dbo.EmployeeInformation ei on ei.SystemId=frd.EmployeeId
				left join ORG.Department D on D.Id=ei.DepartmentId
				left join HKP.Designation DG ON DG.Id=EI.GivenDesignationID
				LEFT JOIN HKP.Party P ON P.Id=FRD.PartyId
				LEFT JOIN HKP.PartyPlant PP ON PP.Id=FRD.PartyPlantId
	            LEFT JOIN SCS.Currency C ON C.Id =frd.CurrencyId
                LEFT JOIN TRN.Voucher V ON V.Id =frd.DisposedVoucherId
                WHERE frd.DisposedVoucherId IS NULL
                group by frd.Id,frd.Status,frd.Remarks,frd.EmployeeId,ei.EmployeeName,D.UserName,DG.UserName ,c.Code,frd.IsPark,c.Id,frd.ToCurrencyRate,frd.DocDate
				,P.UserName ,frd.PartyId,frd.PartyPlantId ,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress,c.Code,v.VoucherNo		
                ) AS TEMP WHERE " + strkey + " order by SlNo desc ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFixedAssetDisposePostedList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select frd.Id DisposeNo,cast(substring(frd.Id,3,8) as int)SlNo,V.VoucherNo,V.PostingDate,V.Id,fr.Remarks,fr.[Status],ei.EmployeeName,D.UserName Department,DG.UserName Designation,frd.IsPark


			,P.UserName CustomerName
			,C.Code TrnCurrency
        ,sum( ISNULL(FR.Price,0)) Price
									,sum( ISNULL(SAR.subAssetAmount,0) )SubAssetAmount
									,sum( ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)) PurchasePrice
									 ,sum( ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)-ISNULL(FR.ADBaseAmount,0)) NetBookValue 
								--	, 0 NegotiationValue

								   , BC.Code BaseCurrency
									,sum( isnull(FR.FABaseAmount,0))FABaseAmount
									,sum(ISNULL(SAR.subAssetBaseAmount,0) )SubAssetBaseAmount
									,sum(isnull(FR.FABaseAmount,0) + ISNULL(SAR.subAssetBaseAmount,0)) PurchaseBaseAmount
									,sum(isnull( FR.ADBaseAmount,0))ADBaseAmount
                                    ,sum( isnull(FR.FABaseAmount,0)+ISNULL(SAR.subAssetBaseAmount,0)-ISNULL(FR.ADBaseAmount,0) )NetBaseBookValue 
										,sum(isnull( rdd.NegotiationValue,0))NegotiationValue
										,sum(isnull( rdd.BaseNagotiationValue,0))BaseNagotiationValue

                from TRN.FixedAssetRegisterDisposed frd 
				join TRN.FixedAssetRegisterDisposedDetail rdd ON rdd.FixedAssetRegisterDisposedId=frd.Id
                left join TRN.FixedAssetRegister FR on FR.Id=rdd.FixedAssetRegisterId
                left join dbo.EmployeeInformation ei on ei.SystemId=frd.EmployeeId
				left join ORG.Department D on D.Id=ei.DepartmentId
				left join HKP.Designation DG ON DG.Id=EI.GivenDesignationID
				LEFT JOIN HKP.Party P ON P.Id=FRD.PartyId
				LEFT JOIN HKP.PartyPlant PP ON PP.Id=FRD.PartyPlantId
				JOIN TRN.Voucher V ON V.Id=frd.DisposedVoucherId
                LEFT JOIN SCS.Currency C ON C.Id =frd.CurrencyId
                LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId
                LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount ,ISNULL(Sum(BaseAmount),0) subAssetBaseAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
                where V.Archive=0 AND fr.CompanyId='" + companyId+@"'
                group by fr.Remarks,fr.[Status],ei.EmployeeName,frd.IsPark,frd.Id,D.UserName 
				,DG.UserName,V.VoucherNo,V.PostingDate,V.Id,c.Code ,P.UserName , BC.Code ) AS TEMP WHERE "+strkey+ " order by SlNo desc ";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetCapitalizeAssetDisposePostedList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select frd.Id DisposeNo,cast(substring(frd.Id,3,8) as int)SlNo,frd.EmployeeId,ei.EmployeeName,D.UserName Department
									,frd.Status,frd.Remarks,DG.UserName Designation,c.Code TrnCurrency,frd.IsPark,  c.Id trnCurrencyId
									,format( frd.DocDate,'dd-MMM-yyyy')DocDate ,P.UserName CustomerName,frd.PartyId,frd.PartyPlantId 
									 ,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress,c.Code TrnPurchaseCurrency,V.VoucherNo,V.PostingDate,V.Id
									,sum(isnull( rdd.NegotiationValue,0))NegotiationValue
                                    ,sum(isnull( rdd.BaseNagotiationValue,0))BaseNagotiationValue
                from TRN.FixedAssetRegisterDisposed frd 
				join TRN.FixedAssetRegisterDisposedDetail rdd ON rdd.FixedAssetRegisterDisposedId=frd.Id
                left join  TRN.AssetRegister AR on AR.Id=rdd.AssetRegisterId
                left join dbo.EmployeeInformation ei on ei.SystemId=frd.EmployeeId
				left join ORG.Department D on D.Id=ei.DepartmentId
				left join HKP.Designation DG ON DG.Id=EI.GivenDesignationID
				LEFT JOIN HKP.Party P ON P.Id=FRD.PartyId
				LEFT JOIN HKP.PartyPlant PP ON PP.Id=FRD.PartyPlantId
	            LEFT JOIN SCS.Currency C ON C.Id =frd.CurrencyId
                LEFT JOIN TRN.Voucher V ON V.Id =frd.DisposedVoucherId
				WHERE V.SourceType='FixedAssetDisposeJournal'
                group by frd.Id,frd.Status,frd.Remarks,frd.EmployeeId,ei.EmployeeName,D.UserName,DG.UserName ,c.Code,frd.IsPark,c.Id,frd.DocDate
				,P.UserName ,frd.PartyId,frd.PartyPlantId ,frd.DeliveryPartyPlantId,frd.InvoicingByAddress,frd.DeliveryByAddress,c.Code,V.VoucherNo,V.PostingDate,V.Id) AS TEMP WHERE " + strkey + " order by SlNo desc ";
            return _sqlRepository.GetDataCollection(sql);
        }


        public void GetParallelCurrency(string companyId, out string companyCurrencyId, out string companyCurrencyCode)
        {
            var companyParallelCurrency = GetCompanyCurrencyId(companyId);
            if (null == companyParallelCurrency["CurrencyId"].ToString())
                throw new CustomException(ResourcesCore.CompanyParallelCurrencyNotConfigured);
            companyCurrencyId = companyParallelCurrency["CurrencyId"].ToString();
            companyCurrencyCode = companyParallelCurrency["CurrencyCode"].ToString();
        }
        private Dictionary<string, object> GetCompanyCurrencyId(string companyId)
        {
            var cmdText = @"select cpc.CurrencyId,C.Code CurrencyCode from SCS.CompanyParallelCurrency cpc
                            LEFT JOIN SCS.Currency C ON C.Id = CPC.CurrencyId where cpc.ParallelCurrencyType = '" + ParallelCurrencyType.CompanyCurrency.ToString() + "'";
            return _sqlRepository.GetData(cmdText);
        }
        //testing 
        //private bool GetPlantIsShowFCInWord(string plantId)
        //{
        //   var IsShowFCInWord = @"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='"+ plantId + "'";
        //    return bool.Parse(IsShowFCInWord);
        //}

        private bool GetPlantIsShowFCInWord(string plantId)
        {
            return bplib.clsWebLib.GetBoolData(_sqlRepository.GetDataCollection(@"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='" + plantId + "'")[0]["IsShowFCInWord"].ToString());
        }
        #region Fixed assets dispose post report

        //old vendor invoice charge set-off data
        private DataTable GetFixedAssetsDisposePostData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget

                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOff] AS IV ON IV.Id=IVD.InvoiceWriteOffId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        //vendor invoice header data old & NEW
        private Dictionary<string, object> GetFixedAssetsDisposePostHeader(string companyGroupId, string companyId, string plantId, string disposedVoucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
            , P.UserName AS Vendor, PP.UserName AS VendorPlant
			, V.CurrencyId, C.Code AS CurrencyCode
			,EI.EmployeeName,BJ.Status DisposedStatus,BJ.Id DisposedNo,I.Amount AS SalesAmount
            FROM [TRN].FixedAssetRegisterDisposed AS BJ
            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.DisposedVoucherId
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
			LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=BJ.EmployeeId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
			LEFT JOIN HKP.Party P ON P.Id = BJ.PartyId
			LEFT JOIN HKP.PartyPlant PP ON PP.Id = BJ.PartyPlantId
            LEFT JOIN [TRN].[Invoice] AS I ON V.Id=I.VoucherId
            WHERE v.Archive=0 AND v.CompanyGroupId='" + companyGroupId + "' AND v.CompanyId='" + companyId + "' AND v.PlantId='" + plantId + "' AND BJ.DisposedVoucherId='" + disposedVoucherId + "' AND v.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        public IWorkbook FixedAssetsDisposePostReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string disposedVoucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "FixedAssetsDisposePost";

            //    var advanceDataList = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, sourceType);
            //    var dtGeneralVoucher = advanceDataList;

            var header = GetFixedAssetsDisposePostHeader(companyGroupId, companyId, plantId, disposedVoucherId, SourceType.FixedAssetDisposeJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetFixedAssetsDisposePostData(companyGroupId, companyId, plantId, disposedVoucherId, SourceType.FixedAssetDisposeJournal);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, row, middleColumnCaption, "");
            //sheet[row, 3].ColumnWidth = 25;
            //reportUtility.SetText(ref sheet, row, middleColumnCaption, header[""].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Disposed No");
            reportUtility.SetText(ref sheet, row, 2, header["DisposedNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            //reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Disposal Type");
            //reportUtility.SetText(ref sheet, row, 5, header["DisposedStatus"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            row++;
            if (header["DisposedStatus"].ToString()== "CompensateByEmployee")
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee");
                reportUtility.SetText(ref sheet, row, 2, header["EmployeeName"].ToString());
            }
            if (header["DisposedStatus"].ToString() == "Sales")
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer");
                reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            }
            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Sales Amount");
                reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(header["SalesAmount"].ToString()) + " " + header["CurrencyCode"].ToString());
            }

            row++;

            if (header["DisposedStatus"].ToString() == "Sales")
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
                reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            }
            
            //reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            //reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 5;
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;

            row++;  //10

            //if (companyCurrencyId == transcationCurrency)
            //{
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
            //}
            //else
            //{
            //    reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
            //    sheet[row, 4, row, 5].Merge();

            //    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            //    sheet[row, 6, row, 7].Merge();
            //}
            sheet[row, 6].ColumnWidth = 15;
            //sheet[row, 6].RowHeight = 15;
            sheet[row, 7].ColumnWidth = 15;
            sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; xlsCol++;


            //if (companyCurrencyId != transcationCurrency)
            //{
            //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
            //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;


            //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
            //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
            //    colLast = xlsCol;

            //    //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
            //    //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

            //    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
            //    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
            //    //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //}
            //else
            //{

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //}


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();

                    //if (companyCurrencyId != transcationCurrency)
                    //{
                    //    reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                    //    reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                    //    reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                    //    reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    //    totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    //}
                    //else
                    //{
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    //}
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);

                //if (companyCurrencyId != transcationCurrency)
                //{
                //    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                //    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                //    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                //    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                //    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                //    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                //    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                //    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                //}
                //else
                //{
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                //}

                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                //if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                //{
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                //    row++;
                //}

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Fixed Asset Dispose" + "(" + header["DisposedStatus"].ToString() + ")", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

                //    //else
                //    //{
                //    //    sheet.UsedRange.WrapText = true;
                //    //    sheet.UsedRange.CellStyle.Font.Size = 8;
                //    //    reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                //    //    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, "Fixed Asset Dispose" + "(" + header["DisposedStatus"].ToString() + ")", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        #endregion Fixed assets dispose post report

        #region Fixed assets Depreciation post report

        //old vendor invoice charge set-off data
        private DataTable GetFixedAssetsDepreciationPostData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget

                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOff] AS IV ON IV.Id=IVD.InvoiceWriteOffId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        //vendor invoice header data old & NEW
        private Dictionary<string, object> GetFixedAssetsDepreciationPostHeader(string companyGroupId, string companyId, string plantId, string depreciationVoucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
			, V.CurrencyId, C.Code AS CurrencyCode
            ,( select TOP 1 FixedAssetMasterId from [TRN].[FixedAssetDepreciationProcess]  WHERE DepreciationVoucherId=V.Id)FixedAssetMasterId
            FROM [TRN].[Voucher] AS V 
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
            WHERE v.Archive=0 AND v.CompanyGroupId='" + companyGroupId + "' AND v.CompanyId='" + companyId + "' AND v.PlantId='" + plantId + "' AND V.Id='" + depreciationVoucherId + "' AND v.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private Dictionary<string, object> GetAssetsDepreciationPostHeader(string companyGroupId, string companyId, string plantId, string depreciationVoucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
			, V.CurrencyId, C.Code AS CurrencyCode
            ,AD.Id AssetDepreciationId,AD.ProcessName,FORMAT(AD.ProcessDate, 'dd-MMM-yyyy') ProcessDate
            FROM [TRN].[Voucher] AS V 
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
            LEFT JOIN [TRN].[AssetDepreciation] AD ON AD.VoucherId=V.Id
            WHERE v.Archive=0 AND v.CompanyGroupId='" + companyGroupId + "' AND v.CompanyId='" + companyId + "' AND v.PlantId='" + plantId + "' AND V.Id='" + depreciationVoucherId + "' AND v.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        public IWorkbook FixedAssetsDepreciationPostReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string depreciationVoucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "FixedAssetsDepreciationPost";
            var header = GetFixedAssetsDepreciationPostHeader(companyGroupId, companyId, plantId, depreciationVoucherId, SourceType.DepreciationJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetFixedAssetsDepreciationPostData(companyGroupId, companyId, plantId, depreciationVoucherId, SourceType.DepreciationJournal);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;


            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());

            

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Asset Master Id");
            reportUtility.SetText(ref sheet, row, 2, header["FixedAssetMasterId"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            //reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Disposal Type");
            //reportUtility.SetText(ref sheet, row, 5, header["DisposedStatus"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            row++;
           
            //if (companyCurrencyId != transcationCurrency)
            //{
            //    reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Sales Amount");
            //    reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(header["SalesAmount"].ToString()) + " " + header["CurrencyCode"].ToString());
            //}
            //row++;

            //if (header["DisposedStatus"].ToString() == "Sales")
            //{
            //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            //    reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            //}
            //row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 5;
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;
            row++;  //10

            
            reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            sheet[row, 4, row, 5].Merge();
            
            sheet[row, 6].ColumnWidth = 15;
            //sheet[row, 6].RowHeight = 15;
            sheet[row, 7].ColumnWidth = 15;
            sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; xlsCol++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
            colLast = xlsCol;
            sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
            


           
            int formulaStartRow = 0;
            int formulaEndRow = 0;
            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();

                    
                    reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                    reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                 
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                

                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Fixed Asset Depreciation", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

                
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, "Fixed Asset Depreciation", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }
        public IWorkbook AssetsDepreciationPostReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string depreciationVoucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "CapitalizeAssetDepreciationPost";
            var header = GetAssetsDepreciationPostHeader(companyGroupId, companyId, plantId, depreciationVoucherId, SourceType.DepreciationJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetFixedAssetsDepreciationPostData(companyGroupId, companyId, plantId, depreciationVoucherId, SourceType.DepreciationJournal);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Depreciation Id");
            reportUtility.SetText(ref sheet, row, 2, header["AssetDepreciationId"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Process Name");
            reportUtility.SetText(ref sheet, row, 2, header["ProcessName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Process Date");
            reportUtility.SetText(ref sheet, row, 5, header["ProcessDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 5;
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;
            row++;  //10


            reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            sheet[row, 4, row, 5].Merge();

            sheet[row, 6].ColumnWidth = 15;
            sheet[row, 7].ColumnWidth = 15;
            sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; xlsCol++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
            colLast = xlsCol;
            sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);




            int formulaStartRow = 0;
            int formulaEndRow = 0;
            if (dsLocal.Rows.Count > 0)
            {
                double totalBookCurrencyAmount = 0;
                row++; 

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                    reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));

                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Capitalize Asset Depreciation", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);


            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, "Capitalize Asset Depreciation", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        #endregion Fixed assets Depreciation post report

        public DataTable GetFixedAssetDisposeServiceData(string fixedAssetRegisterDisposeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT FARD.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId,FR.FixedAssetMasterId
                                    , FR.SerialNo, FR.Id AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType
                                    ,P.UserName Vendor
                                    ,c.Code TrnCurrency
	                                ,FAD.DocDate
                                    , ISNULL(FR.Price,0) Price
									,ISNULL(SAR.subAssetAmount,0) SubAssetAmount
									, ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0) PurchasePrice
									 ,ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)-ISNULL(FR.ADBaseAmount,0) NetBookValue 
								--	, 0 NegotiationValue

								   , BC.Code BaseCurrency
									,isnull(FR.FABaseAmount,0)FABaseAmount
									,ISNULL(SAR.subAssetBaseAmount,0) SubAssetBaseAmount
									,isnull(FR.FABaseAmount,0) + ISNULL(SAR.subAssetBaseAmount,0) PurchaseBaseAmount
									,isnull( FR.ADBaseAmount,0)+ ISNULL(FADP.FixedAssetDepreciationAmount,0) ADBaseAmount
                                    , isnull(FR.FABaseAmount,0)+ISNULL(SAR.subAssetBaseAmount,0)-ISNULL(FR.ADBaseAmount,0)- ISNULL(FADP.FixedAssetDepreciationAmount,0)  NetBaseBookValue 
									,FARD.BaseNagotiationValue, FARD.NegotiationValue

                                    , MMA.StandardName Article, FR.IsFinancial,IsOpeningBalance=case when FR.IsOpeningBalance=0 then 'No' Else 'Yes' End
                                    , GL.AccountCode GLGeneralInfoCode,GL.UserName GLGeneralInfoName,GL.Id GLGeneralInfoId
									, BM.Id BudgetMasterId,B.UserName BudgetName,BM.RefNo BudgetRefNo
									, A.UserName ActivityName, FR.FAActivityId ActivityId
                                   		,format( FR.CapitalizationDate,'dd-MMM-yyyy')CapitalizationDate
									,format(IR.GRNDate,'dd-MMM-yyyy') PurchaseDate
									,format( ii.IssueDate,'dd-MMM-yyyy')IssueDate
		                            ,FAD.Remarks,
									Customer.UserName CustomerName
									,CU.Code Currency

                                    FROM[TRN].[FixedAssetRegister] FR
                                   LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId= MM.Id
                                   LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                   LEFT JOIN MST.BudgetMaster BM ON FR.FABudgetMasterId = BM.Id
                                   LEFT JOIN [MST].[FixedAssetMaster] FAM ON FR.FixedAssetMasterId= FAM.Id
                                   LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId= FAC.Id
                                   LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId= FASC.Id

	                                LEFT JOIN TRN.FixedAssetRegisterDetail FRD ON FRD.CapitalizeRegisterNo=FR.CapitalizeRegisterNo
									LEFT JOIN TRN.InventoryIssueHistory IIH ON IIH.Id=FRD.InventoryIssueHistoryId
									LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IIH.CapitalizeVoucherDetailId
									LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									left join trn.InventoryIssue II on ii.Id = iid.InventoryIssueId
									LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
									left join trn.InventoryReceive IR on IR.Id =  IRD.InventoryReceiveId
									LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId 
                                    LEFT JOIN SCS.Currency C ON C.Id =FR.CurrencyId
                                    LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId

	                                LEFT JOIN HKP.Party P ON P.Id = FR.VendorId
								   LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId
								   LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
								   LEFT JOIN HKP.Activity A ON A.Id=FR.FAActivityId
								   LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount,ISNULL(Sum(BaseAmount),0) subAssetBaseAmount FROM
								   TRN.SubFixedAssetRegister 
								   group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
                                    LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
                                    LEFT JOIN TRN.FixedAssetRegisterDisposedDetail FARD ON FARD.FixedAssetRegisterId=FR.Id

                                    LEFT JOIN TRN.FixedAssetRegisterDisposed FAD ON FAD.Id=FARD.FixedAssetRegisterDisposedId
	                                LEFT JOIN HKP.Party Customer ON Customer.Id = FAD.PartyId
                                    LEFT JOIN SCS.Currency CU ON CU.Id =FAD.CurrencyId
                               
                                   WHERE FR.CompanyId= '" + identity.CompanyId + @"'  and FR.Archive= 0 and FR.IsAUC= 0 
                                    AND FARD.FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposeId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Capitalize Asset Register Posting
        public void InsertCapitalizeAssetRegisterPosting(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, Dictionary<string, object> capitalizationMasterdata)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                var voucherDrId = "";
                decimal totalDrAmount = 0;
                decimal totalCrAmount = 0;

                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetCapitalizeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                AuditService.PostedLog(voucher);
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;


                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {

                        // INSERT INTO InvoiceDetail


                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            BudgetMasterActivityId = voucherDetailVM.BudgetMasterActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            DrAmount = voucherDr.DrAmount
                        }, ref _drvDetailCurrencyData);

                        totalDrAmount += voucherDr.DrAmount;
                        voucherDrId = voucherDr.Id;
                        if (capitalizationMasterdata != null)
                        {
                            builderSql = "";
                            builderSql = @"UPDATE ARC SET ARC.VoucherDetailId='" + voucherDr.Id + "' FROM [TRN].[AssetRegisterChild] ARC INNER JOIN  MST.FixedAssetItem FAI ON FAI.Id = ARC.FixedAssetItemId WHERE ARC.CapitalizationMasterId = '" + capitalizationMasterdata["Id"].ToString() + "' AND FAI.FixedAssetMasterId = '" + voucherDetailVM.FixedAssetMasterId + "'  ";
                            rdBuilder.Append(builderSql);
                        }
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            BudgetMasterActivityId = voucherDetailVM.BudgetMasterActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };

                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            CrAmount = voucherCr.CrAmount
                        }, ref _crvDetailCurrencyData);

                        totalCrAmount += voucherCr.CrAmount;
                    }
                }
                if (totalCrAmount != totalDrAmount)
                    throw new CustomException("Dr Cr Amount not match !.");

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData);
                if (capitalizationMasterdata != null)
                {
                    builderSql = "";
                    builderSql = @"UPDATE [TRN].[CapitalizationMaster] SET VoucherId='" + voucher.Id + "' WHERE Id='" + capitalizationMasterdata["Id"].ToString() + "'  ";
                    rdBuilder.Append(builderSql);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public void InsertCapitalizeAssetRegisterPostingAddition(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, List<Dictionary<string, object>> assetRegisterList, Dictionary<string, object> capitalizationMasterdata)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                DataSet _assetRegisterData = null;
                DataSet _assetRegisterChildData = null;
                var voucherDrId = "";


                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.FixedAssetCapitalizeJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;


                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {

                        // INSERT INTO InvoiceDetail


                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            BudgetMasterActivityId = voucherDetailVM.BudgetMasterActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            DrAmount = voucherDr.DrAmount
                        }, ref _drvDetailCurrencyData);
                        voucherDrId = voucherDr.Id;
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            BudgetMasterActivityId = voucherDetailVM.BudgetMasterActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            CrAmount = voucherCr.CrAmount
                        }, ref _crvDetailCurrencyData);
                    }
                }
                
                ConnectionManager.DAL.ConManager objCon;
                string sqlChild = "SELECT * FROM [TRN].[AssetRegisterChild] WHERE 1=2 ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlChild, out _assetRegisterChildData, false, "1");
                var i = 0;
                foreach (var item in assetRegisterList)
                {
                    objCon.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[AssetRegisterChild] where  AssetRegisterId='" + item["AssetRegisterId"].ToString() + "'", out _assetRegisterData, false, "1");
                    var assetRegisterChildData = new
                    {
                        Id = _accountsCommonService.MakePK(item["AssetRegisterId"].ToString(), _assetRegisterData.Tables[0].Rows.Count + 1, 2),
                        FixedAssetItemId = item["FixedAssetItemId"].ToString(),
                        AssetRegisterId = item["AssetRegisterId"].ToString(),
                        CapitalizationMasterId = capitalizationMasterdata["Id"].ToString(),
                        CapitalizationChildId = capitalizationMasterdata["Id"].ToString() + "-" + (i + 1),
                        Amount = item["Amount"].ToString(),
                        NetAmount = item["Amount"].ToString(),
                        CompanyGroupId = identity.CompanyGroupId,
                        CompanyId = identity.CompanyId,
                        PlantId = identity.PlantId,
                        VoucherDetailId = voucherDrId,
                        AddedBy = identity.Name,
                        AddedDate = System.DateTime.Now.ToString(),
                        AddedFromIP = identity.IPAddress,
                    };
                    i++;
                    AddNewRow(_assetRegisterChildData.Tables[0], assetRegisterChildData);

                }



                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _crvDetailData, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData, _assetRegisterChildData);
                if (capitalizationMasterdata != null)
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = @"UPDATE [TRN].[CapitalizationMaster] SET VoucherId='" + voucher.Id + "' WHERE Id='" + capitalizationMasterdata["Id"].ToString() + "'  ";
                    rdBuilder.Append(builderSql);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void UpdateAssetRegister(List<Dictionary<string, object>> assetRegisterList)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                if (assetRegisterList != null)
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    foreach (var item in assetRegisterList)
                    {
                        builderSql = @"UPDATE [TRN].[AssetRegister] SET AssetSlNo='" + item["AssetSlNo"] + "' ,Status = '" + item["Status"] + "' ,AssetCondition = '" + item["AssetCondition"] + "' ,UserReference = '" + item["UserReference"] + "' ,OldReference = '" + item["OldReference"] + "' ,UserGroup = '" + item["UserGroup"] + "' ,Remarks = '" + item["Remarks"] + "' ,UpdatedBy = '" + identity.Name + "' ,UpdatedDate = '" + System.DateTime.Now.ToString() + "' ,UpdatedFromIP = '" + identity.IPAddress + "' WHERE Id='" + item["AssetRegisterId"].ToString() + "'  ";
                        rdBuilder.Append(builderSql);
                        builderSql = @"UPDATE [TRN].[AssetRegisterChild] SET Amount='" + item["Amount"] + "' ,NetAmount = '" + item["Amount"] + "',UpdatedBy = '" + identity.Name + "' ,UpdatedDate = '" + System.DateTime.Now.ToString() + "' ,UpdatedFromIP = '" + identity.IPAddress + "' WHERE Id='" + item["AssetRegisterChildId"].ToString() + "'  AND VoucherDetailId is null ";
                        rdBuilder.Append(builderSql);
                    }
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void UpdateAssetRegisterItem(string assetRegisterId,string assetRegisterChildId, string fixedAssetItemId)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (assetRegisterId != null && fixedAssetItemId != null)
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    builderSql = @"UPDATE [TRN].[AssetRegister] SET FixedAssetItemId='" + fixedAssetItemId + "'  ,UpdatedBy = '" + identity.Name + "' ,UpdatedDate = '" + System.DateTime.Now.ToString() + "' ,UpdatedFromIP = '" + identity.IPAddress + "' WHERE Id='" + assetRegisterId + "'  ";
                    rdBuilder.Append(builderSql);
                    builderSql = @"UPDATE [TRN].[AssetRegisterChild] SET FixedAssetItemId='" + fixedAssetItemId + "'  ,UpdatedBy = '" + identity.Name + "' ,UpdatedDate = '" + System.DateTime.Now.ToString() + "' ,UpdatedFromIP = '" + identity.IPAddress + "' WHERE Id='" + assetRegisterChildId + "'  ";
                    rdBuilder.Append(builderSql);

                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        private Dictionary<string, object> GetCapitalizeAssetRegisterPostHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
			, V.CurrencyId, C.Code AS CurrencyCode,CM.FixedAssetItemId
            ,FAM.UserName FixedAssetMaster,FAI.UserName FixedAssetItem
            FROM TRN.Voucher V 
		    INNER JOIN [TRN].[CapitalizationMaster] CM ON CM.VoucherId=V.Id
			LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=CM.FixedAssetItemId
			LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
            WHERE v.Archive=0 AND v.CompanyGroupId='" + companyGroupId + "' AND v.CompanyId='" + companyId + "' AND v.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND v.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public IWorkbook CapitalizeAssetRegisterPostReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "CapitalizeAssetRegisterPost";
            var header = GetCapitalizeAssetRegisterPostHeader(companyGroupId, companyId, plantId, voucherId, SourceType.FixedAssetCapitalizeJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetFixedAssetsDepreciationPostData(companyGroupId, companyId, plantId, voucherId, SourceType.FixedAssetCapitalizeJournal);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;


            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());



            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Asset Master");
            reportUtility.SetText(ref sheet, row, 2, header["FixedAssetMaster"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Asset Item");
            reportUtility.SetText(ref sheet, row, 5, header["FixedAssetItem"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Asset Item Id");
            reportUtility.SetText(ref sheet, row, 2, header["FixedAssetItemId"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 5;
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;
            row++;  //10


            reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            sheet[row, 4, row, 5].Merge();

            sheet[row, 6].ColumnWidth = 15;
            //sheet[row, 6].RowHeight = 15;
            sheet[row, 7].ColumnWidth = 15;
            sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; xlsCol++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
            colLast = xlsCol;
            sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);




            int formulaStartRow = 0;
            int formulaEndRow = 0;
            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                    reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));

                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                //reportUtility.CompanyPlantHeader(ref sheet, colLast, "Capitalize Asset Register", companyId, plantName, null);
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Capitalize Asset Register", companyId, plantId, plantName, null);

                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);


            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, "Capitalize Asset Register", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }
        #endregion

        #region Capitalize Asset Depreciation Posting
        public void InsertAssetDepreciationPosting(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
          , string assetDepreciationId)
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);

                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                var voucherDrId = "";


                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = companyCurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.DepreciationJournal.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                AuditService.PostedLog(voucher);
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;


                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {

                        // INSERT INTO InvoiceDetail


                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            DrAmount = voucherDr.DrAmount
                        }, ref _drvDetailCurrencyData);
                        voucherDrId = voucherDr.Id;
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = companyCurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = 1,
                            CrAmount = voucherCr.CrAmount
                        }, ref _crvDetailCurrencyData);
                    }
                }


                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _crvDetailData, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData);
                if (assetDepreciationId != null)
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    builderSql = @"UPDATE [TRN].[AssetDepreciation] SET VoucherId='" + voucher.Id + "' WHERE Id='" + assetDepreciationId + "' ";
                    rdBuilder.Append(builderSql);
                    builderSql = @"UPDATE [TRN].[AssetDepreciationDetail] SET VoucherDetailId='" + voucherDrId + "' WHERE AssetDepreciationId='" + assetDepreciationId + "' ";
                    rdBuilder.Append(builderSql);
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        #endregion
        public void FixedAssetTaxInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "DisposeTaxInvoice" + plantId + ".docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                WSection section = document.Sections[0];

                DataTable dsOrderMaster;

                dsOrderMaster = GetloadAssetDisposeTaxInvoiceDetail(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeLocalTaxInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                                                                                                                                        // var SalesTotal = makeOrderServiceTable(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {{ServiceItems}}
                var dsInventoryReceiveAdditionalTax = loadLocalTaxInvoiceAdditionalTax(salesId);


                var InventoryReceiveAdditionalTax = 0.00;
                if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                {
                    InventoryReceiveAdditionalTax = makeLocalTaxInvoiceTaxTable(document, dsInventoryReceiveAdditionalTax, salesId);//Service Details 
                    //document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", (MaterialTotal  + InventoryReceiveAdditionalTax).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["BaseCurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal   + InventoryReceiveAdditionalTax), dsOrderMaster.Rows[0]["BaseCurrencyId"].ToString()), true, true);


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        //ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Tables[0].Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                        document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                    if (text == "{PRINTEDBY}")
                    {
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                var sourceDoc = document.Clone();
                document.Replace("{FileCopyName}", "Original Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Duplicate Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Triplicate for recipient", false, false);


                //removing any unused place holder  
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "LocalTaxInvoice" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();


            }
            catch (Exception ex)
            {
                //throw ex;
            }

            document.Close();
        }
        public double makeLocalTaxInvoiceTaxTable(WordDocument document, DataTable dsOrderMaster, string salesId)
        {
            string replaceString = "{TaxCollectedAtSource}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadLocalTaxInvoiceAdditionalTax(salesId);


            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    //LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxname");
            range.ApplyCharacterFormat(FontBold);
            int colTaxname = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Percentage");
            range.ApplyCharacterFormat(FontBold);
            int colPercentage = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Tax Amount");
                range.ApplyCharacterFormat(FontBold);

            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;


            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //ROW++;
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;


                IParagraphItem p = TROW.Cells[colTaxname].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Taxname"].ToString());
                TROW.Cells[colPercentage].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("#,##0.0000"));

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyTaxAmount"].ToString()).ToString("#,##0.00"));
            }

            #region Sub Total


            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTaxAmount)", "").ToString());

            #endregion Total


            //ROW++;

            #region Total Payable
            #endregion Total Payable

            //ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle3 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle3.CharacterFormat.FontSize = 8f;
            myStyle3.CharacterFormat.TextColor = Color.Black;
            myStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            int k = document.Replace(replaceString, textBodyPart, false, false);
            return total;
        }
        public DataTable loadLocalTaxInvoiceAdditionalTax(string salesId)
        {
            string strSQL;

            try
            {
                strSQL = @"select TxC.UserName Taxname,SA.Id,SA.TaxCodeId as TaxCode,SA.BooksCurrencyTaxAmount,SA.Percentage
						from TRN.FixedAssetRegisterDisposedAdditionalTax SA
						left join TRN.FixedAssetRegisterDisposed as S on S.Id=SA.FixedAssetRegisterDisposedId
						left join MST.TaxCode as TxC on TxC.id = SA.TaxCodeId
                        where S.Id='" + salesId + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable GetloadAssetDisposeTaxInvoiceDetail(string disposeId)
        {
            string strSQL;
            try
            {


                strSQL = @"SELECT IR.Id CustomerNo, IRD.Id SalesMaterialId
                                 , ARC.CompanyGroupId
                                ,ARC.CompanyId,CRNC.Code
								,p.UserName Customer
                                , P.UserName Buyer
                                 , ir.CurrencyId
								,cmp.BaseCurrencyId
								,P.TINNO CustomerGSTNo
                                , p.VATResistrationNo as CustomerPANNo
								,Addres.Address1 VendorAddress
                                 , Plant.GSTIN
								,Plant.VATResistrationNo as PlantPANNo
                                ,DPARTYPL.GSTIN ShipGSTIN
                                , INVPARTYPL.GSTIN BillGSTIN
                                 , IR.Id DocRefNo
	                            ,IR.Id InvoiceNo
                                ,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                                , REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS InvoiceDate
                                   , IR.PartyPlantId
		                        ,INVPARTYPL.UserName InvoiceParty
                                , INVPARTYPL.UserName InvoiceParty2
                                 , IR.InvoicingByAddress as ConsigneeAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
                                , IR.DeliveryPartyPlantId
		                       
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
                              , FAI.UserName MaterialMaster
                              
                               , 1 POTransactionQty
	                          ,ROUND(IRD.BaseNagotiationValue, 4) TransactionRate
	                          ,ROUND((IRD.BaseNagotiationValue ), 2) AS TrnAmount
                              , IRD.BaseNagotiationValue 
	                          , BaseTaxAmount=  (
                                    SELECT SUM(Amount)

                                    FROM [TRN].[FixedAssetRegisterDisposedTax]
                                    WHERE FixedAssetRegisterDisposedDetailId = IRD.Id
		                            )
	                          ,TaxAmount = (
                                    SELECT SUM(Amount)

                                    FROM [TRN].[FixedAssetRegisterDisposedTax]
                                    WHERE FixedAssetRegisterDisposedDetailId = IRD.Id
		                            )
	                          ,0 ServiceTaxAmount  
							  ,'' PONumber
                        , IR.AddedBy CreatedBy, IR.Remarks ItemDescription, IR.LorryNo TransportVehicleNo
                        FROM TRN.FixedAssetRegisterDisposed IR
                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN trn.FixedAssetRegisterDisposedDetail AS IRD ON IRD.FixedAssetRegisterDisposedId = IR.Id
						 LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.PartyPlantId
                         LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                         LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
                         LEFT JOIN[MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
						 Left JOIN TRN.AssetRegister AR ON AR.Id=IRD.AssetRegisterId
						 Left JOIN (select distinct PlantId,AssetRegisterId,CompanyGroupId,CompanyId from TRN.AssetRegisterChild) ARC ON ARC.AssetRegisterId=AR.Id
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = ARC.PlantId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = Plant.CompanyId
						 LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = Plant.CompanyGroupId
						 LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = Cmp.BaseCurrencyId
						 LEFT JOIN MST.FixedAssetItem FAI ON FAI.ID=AR.FixedAssetItemId
                         WHERE IR.Id ='" + disposeId + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double makeLocalTaxInvoiceService(string companyGroupId, string companyId, string plantId, string disposeId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;
            //Sales== Master Query
            sales = GetloadAssetDisposeTaxInvoiceDetail(disposeId);
            materialTax = loadOrderMasterTax(disposeId);

            int LasColumnIndex = 9;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(materialTax.DefaultView.ToTable(true, "TaxCode"));


            for (int i = 0; i < dv.Count; i++)
            {
                LasColumnIndex++;
                dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                LasColumnIndex++;
            }


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 110;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 110;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("BuyerRef#");
            range.ApplyCharacterFormat(FontBold);
            int colBuyerRef = COL; COL++;
            wTable.Rows[ROW].Cells[colBuyerRef].Width = 80;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PONumber");
            range.ApplyCharacterFormat(FontBold);
            int colPONumber = COL; COL++;
            wTable.Rows[ROW].Cells[colPONumber].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 50;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 50;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount " + "(" + " " + sales.Rows[0]["BaseCurrencyName"].ToString() + " " + ")" + " ");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);

                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }



            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                //TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                //TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                //TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                //TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyBaseRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));


                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(materialTax.DefaultView.ToTable());

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' And FixedAssetRegisterDisposedDetailId = '" + dsOrderMaster.Rows[i]["SalesMaterialId"].ToString() + "' ";

                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("#,##0.00"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            range.ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colArticle || C == colBuyerRef || C == colPONumber || C == colHSN || C == colUoM || C == colRate || C == colChar1 || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total

            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                    //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                    + clsStdLib.dbl(materialTax.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }
        public DataTable loadOrderMasterTax(string SalesId)

        {
            string strSQL;
            try
            {
                strSQL = @"select  PO.FixedAssetRegisterDisposedid,PO.Id FixedAssetRegisterDisposedDetailId,
                                    IRT.Id AS SalesTax,tg.Code AS TaxCode,
                                    s.tocurrencyRate,
                                    IRT.Percentage,
                                    (IRT.Amount) as TaxAmount
                                   	,ISNULL(IRT.Amount,0) BooksCurrencyTransactionAmount
									,ISNULL(IRT.Amount,0) BooksCurrencyTaxAmount
									,1 BooksCurrencyBaseRate

							    from trn.FixedAssetRegisterDisposedDetail PO
                               Inner join [TRN].[FixedAssetRegisterDisposedTax] IRT ON IRT.FixedAssetRegisterDisposedid = PO.FixedAssetRegisterDisposedid 
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
							   left outer join trn.FixedAssetRegisterDisposed as s on s.id=po.FixedAssetRegisterDisposedid
                                 WHERE PO.FixedAssetRegisterDisposedid='" + SalesId + @"' 
								  ";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        class clsStdLib
        {
            public static string passWord = "prodDisplay";
            public clsStdLib()
            {

            }
            public enum mType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNames(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                //if (dayNo.ToString().Length > 1)
                //{
                //    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                //    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
                //        return dayNo + "th";
                //}

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";

                }

            }

            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOK(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                            InputFormat.ShortDatePattern = input_date_format;
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String makeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
            ///<summary>
            ///return day difference in integer. 
            ///    Example 1: firstDate[Less Than]lastDate returns positive value
            ///    Example 2: firstDate>lastDate returns negative value
            ///    Example 3: firstDate=lastDate returns 0 [zero]**/
            /// </summary>
            public static int dateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOK(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOK(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string getSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string getStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related

            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                Double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                double d;
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double dbl(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (isInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool isInt(string num)
            {

                bool isInt;
                int number;
                try
                {
                    isInt = System.Int32.TryParse(num, out number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric

            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection getTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string emptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string


            #region others
            public void copyDataset(DataSet source, ref DataSet destination)
            {
                StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
                DataRow drLocal = null;
                for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
                {
                    drLocal = destination.Tables[0].NewRow();
                    for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
                    {
                        if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
                        {
                            drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
                        }
                    }
                    destination.Tables[0].Rows.Add(drLocal);
                }


            }
            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others

            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;

                return Data;
            }
            public static double sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += dbl(d[columnName].ToString());
                }


                return total;
            }
        }
    }
}

