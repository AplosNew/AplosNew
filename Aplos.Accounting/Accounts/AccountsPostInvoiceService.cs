using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Vouchers;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsPostInvoiceService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsPostInvoiceService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public string InsertPostInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailViewModel> fGInventoryGLBudgetActivityVMList
          )
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
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
                DataSet _postGRNInvoiceData = null;
                DataSet _inventoryReceiveDetailData = null;

               
                voucherVM.DocDate = Convert.ToDateTime(voucherVM.DocDate);
                voucherVM.PostingDate = Convert.ToDateTime(voucherVM.PostingDate);

                var invoice = new Invoice
                {
                    Amount = voucherVM.Amount,
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
                    PartyType = PartyType.Vendor.ToString(),
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.PostInvoice.ToString(),

                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate
                };

                invoice.BaseNoOfDays = voucherVM.BaseNoOfDays;
                invoice.BaseOnDueDate = voucherVM.BaseOnDueDate;
                invoice.RevisedDueDate = voucherVM.MatureDate;
                invoice.ActualDueDate = voucherVM.MatureDate;

                _accountsCommonService.InsertInvoice(invoice, out DataSet _invoicedataSet);

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
                    SourceType = SourceType.PostInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;
                var currentInvoiceDetail = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {
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
                            FromCurrencyId = voucher.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucher.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate
                        }, ref _drvDetailCurrencyData);



                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {

                        currentInvoiceDetail++;
                        // INSERT INTO InvoiceDetail
                        var invoiceDetail = new InvoiceDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            MaterialGroupMasterId = voucherDetailVM.MaterialGroupMasterId,
                            Amount = voucherDetailVM.Amount,
                            NetAmount = voucherDetailVM.Amount,
                            TaxAmount = 0,
                            AddedBy = invoice.AddedBy,
                            AddedDate = invoice.AddedDate,
                            AddedFromIP = invoice.AddedFromIP,
                            Archive = invoice.Archive,
                            InvoiceId = invoice.Id,
                        };
                        invoice.Amount = invoiceDetail.Amount;
                        if (voucherDetailVM.OtherName == "Vendor")
                        {
                            _accountsCommonService.InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail, ref _invoiceDetailData);

                        }
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
                            InvoiceDetailId= invoiceDetail.Id
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucher.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucher.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate
                        }, ref _crvDetailCurrencyData);
                    }
                }


                con.OpenDataSetThroughAdapter(@"SELECT * FROM [dbo].[PostGRNInvoice] WHERE Id='" + voucherVM.Id + "'", out _postGRNInvoiceData, false, "1");
               // con.OpenDataSetThroughAdapter(@"SELECT * FROM trn.InventoryReceiveDetail WHERE InventoryReceiveId='" + voucherVM.InventoryReceiveId + "'", out _inventoryReceiveDetailData, false, "1");

                if (_postGRNInvoiceData.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < _postGRNInvoiceData.Tables[0].Rows.Count; j++)
                    {
                        _postGRNInvoiceData.Tables[0].DefaultView.RowFilter = "Id='" + voucherVM.InventoryReceiveId + @"'";

                        if (_postGRNInvoiceData.Tables[0].DefaultView.Count > 0)
                        {
                            //edit
                            DataRow dr = _postGRNInvoiceData.Tables[0].DefaultView[0].Row;
                            if (string.IsNullOrEmpty(dr["VoucherId"].ToString()))
                            {
                                dr.BeginEdit();

                                dr["Status"] = "Posting";
                                dr["VoucherId"] = voucher.Id;
                                dr["UpdatedBy"] = voucher.AddedBy;
                                dr["UpdatedDate"] = voucher.AddedDate;
                                dr.EndEdit();
                            }
                            else
                            {
                                throw new Exception("This FG Inventory already posted.");
                            }
                        }
                    }
                }

                //foreach (var item in fGInventoryGLBudgetActivityVMList.Where(r => r.TrnType == "Dr"))
                //{
                //    _inventoryReceiveDetailData.Tables[0].DefaultView.RowFilter = "Id='" + item.InventoryReceiveDetailId + @"'";

                //    DataRow drDetail = _inventoryReceiveDetailData.Tables[0].DefaultView[0].Row;
                //    if (string.IsNullOrEmpty(drDetail["PostDrGLGeneralInfoId"].ToString()))
                //    {
                //        drDetail.BeginEdit();

                //        drDetail["PostDrGLGeneralInfoId"] = item.GLGeneralInfoId;
                //        drDetail["PostDrBudgetMasterId"] = item.BudgetMasterId;
                //        drDetail["PostDrActivityId"] = item.ActivityId;
                //        drDetail["UpdatedDate"] = voucher.AddedDate;
                //        drDetail.EndEdit();
                //    }
                //    else
                //    {
                //        throw new Exception("This FG Inventory already posted.");
                //    }

                //}

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData, _postGRNInvoiceData, _inventoryReceiveDetailData
                    );
                return "";
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
    }
}
