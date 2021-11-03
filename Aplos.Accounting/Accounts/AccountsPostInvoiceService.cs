using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
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
        public string InsertPostInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
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
                    Narration =voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.PostInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    IsPark=false

                };
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);
                invoice.VoucherId = voucher.Id;
                invoice.PostGRNInvoiceId = voucherVM.Id;

                _accountsCommonService.InsertInvoice(invoice, out DataSet _invoicedataSet);
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
                            DrAmount = voucherDetailVM.BaseDrAmount
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
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            PartyType = PartyType.Vendor.ToString(),
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
                            CrAmount = voucherDetailVM.BaseCrAmount
                        }, ref _crvDetailCurrencyData);
                    }
                }


                con.OpenDataSetThroughAdapter(@"SELECT * FROM [dbo].[PostGRNInvoice] WHERE Id='" + voucherVM.Id + "'", out _postGRNInvoiceData, false, "1");
               // con.OpenDataSetThroughAdapter(@"SELECT * FROM trn.InventoryReceiveDetail WHERE InventoryReceiveId='" + voucherVM.InventoryReceiveId + "'", out _inventoryReceiveDetailData, false, "1");

                if (_postGRNInvoiceData.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < _postGRNInvoiceData.Tables[0].Rows.Count; j++)
                    {
                        _postGRNInvoiceData.Tables[0].DefaultView.RowFilter = "Id='" + voucherVM.Id + @"'";

                        if (_postGRNInvoiceData.Tables[0].DefaultView.Count > 0)
                        {
                            //edit
                            DataRow dr = _postGRNInvoiceData.Tables[0].DefaultView[0].Row;
                            if (string.IsNullOrEmpty(dr["VoucherId"].ToString()))
                            {
                                dr.BeginEdit();

                                dr["VoucherId"] = voucher.Id;
                                dr["UpdatedBy"] = voucher.AddedBy;
                                dr["UpdatedDate"] = voucher.AddedDate;
                                dr.EndEdit();
                            }
                            else
                            {
                                throw new Exception("This PostInvoice already posted.");
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
                objApp.SaveDataSets(_vdataset, _invoicedataSet, _drvDetailData, _drvDetailCurrencyData, _invoiceDetailData, _crvDetailData, _crvDetailCurrencyData, _postGRNInvoiceData, _inventoryReceiveDetailData
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
        private DataTable GetPostInvoiceVoucherData(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS TDrAmount, VD.CrAmount AS TCrAmount, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Vendor, PP.UserName AS VendorPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
                            ,PGI.Id as InvoiceNo
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							left join dbo.PostGRNInvoice PGI ON PGI.VoucherId=V.Id
                            LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
                            LEFT JOIN [TRN].[Invoice] AS IV ON IV.VoucherId=V.Id
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
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetPostInvoiceVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = GetPostInvoiceVoucherData(voucherId);
            var _CurrencyId = dsLocal.Rows[0]["CurrencyId"].ToString();
            var plCurrencyId = dsLocal.Rows[0]["ParallelCurrencyId"].ToString();
            var trnCurrency = dsLocal.Rows[0]["TrnCurrency"].ToString();
            var plCurrencyCode = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var dvNarration = new DataView(dsLocal)
            {
                RowFilter = "Narration IS NOT NULL"
            };
            var dtNarration = dvNarration.ToTable(true, "Narration");
            if (dsLocal.Rows.Count == 0)
                throw new Exception("No Data Found!");
            int row = 5;


            #region Header

            reportUtility.SetMasterHeaderText(ref sheet, 5, 1, "Voucher No");
            reportUtility.SetText(ref sheet, 5, 2, dsLocal.Rows[0]["VoucherNo"].ToString());
            sheet.Range[5, 2, 5, 3].Merge();
            reportUtility.SetMasterHeaderText(ref sheet, 5, 5, "Voucher Date");
            reportUtility.SetText(ref sheet, 5, 6, dsLocal.Rows[0]["VoucherDate"].ToString());
            sheet.Range[5, 6, 5, 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 1, "Doc Date");
            reportUtility.SetText(ref sheet, 6, 2, dsLocal.Rows[0]["DocDate"].ToString());
            sheet.Range[6, 2, 6, 3].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 5, "Doc No");
            reportUtility.SetText(ref sheet, 6, 6, dsLocal.Rows[0]["DocRefNo"].ToString());
            sheet.Range[6, 6, 6, 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 1, "Posting Date");
            reportUtility.SetText(ref sheet, 7, 2, dsLocal.Rows[0]["PostingDate"].ToString());
            sheet.Range[7, 2, 7, 3].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 5, "Fiscal Year");
            reportUtility.SetText(ref sheet, 7, 6, dsLocal.Rows[0]["PeriodName"].ToString());
            sheet.Range[7, 6, 7, 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Vendor");
            reportUtility.SetText(ref sheet, 8, 2, dsLocal.Rows[0]["Vendor"].ToString());
            sheet.Range[8, 2, 8, 3].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 5, "Vendor Plant");
            reportUtility.SetText(ref sheet, 8, 6, dsLocal.Rows[0]["VendorPlant"].ToString());
            sheet.Range[8, 6, 8, 7].Merge();


            reportUtility.SetMasterHeaderText(ref sheet, 9, 1, "InvoiceNo.");
            reportUtility.SetText(ref sheet, 9, 2, dsLocal.Rows[0]["InvoiceNo"].ToString());
            sheet.Range[9, 2, 9, 3].Merge();


            reportUtility.SetMasterHeaderText(ref sheet, 10, 1, "Narration");
            reportUtility.SetText(ref sheet, 10, 2, dtNarration.Rows[0]["Narration"].ToString());
            sheet.Range[10, 2, 10, 3].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 10, 5, "Status");
            reportUtility.SetText(ref sheet, 10, 6, dsLocal.Rows[0]["Park/Post"].ToString());
            sheet.Range[10, 6, 10, 7].Merge();

            #endregion Header



            // Set report Name
            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["VoucherNo"];



            int colGL = 1;

            int col = 0;
            row = 12;
            reportUtility.SetHeaderText(ref sheet, row, colGL, "GL", 15);

            if (_CurrencyId != plCurrencyId)
            {
                sheet.Range[row, colGL, row, 3].Merge();
                col = 4;
            }
            else
            {
                sheet.Range[row, colGL, row, 5].Merge();
                col = 6;
            }

            var summerCol = col - 1;
            if (_CurrencyId != plCurrencyId)
            {
                reportUtility.SetHeaderText(ref sheet, 11, col, dsLocal.Rows[0]["TrnCurrency"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[11, col, 11, col + 1].Merge();
                sheet.Range[11, col, 11, col + 1].BorderAround(ExcelLineStyle.Thin);
                reportUtility.SetHeaderText(ref sheet, 12, col, "Debit", ExcelHAlign.HAlignRight); int colUsdDebit = col; col++;
                reportUtility.SetHeaderText(ref sheet, 12, col, "Credit", ExcelHAlign.HAlignRight); int colUsdCredit = col; col++;
            }
            reportUtility.SetHeaderText(ref sheet, 11, col, dsLocal.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
            sheet[11, col, 11, col + 1].Merge();
            sheet.Range[11, col, 11, col + 1].BorderAround(ExcelLineStyle.Thin);
            reportUtility.SetHeaderText(ref sheet, 12, col, "Debit", ExcelHAlign.HAlignRight); int colDebit = col; col++;
            reportUtility.SetHeaderText(ref sheet, 12, col, "Credit", ExcelHAlign.HAlignRight); int colCredit = col;
            var colLast = col;
            row = 13;
            var startRow = row;
            double _Total_Amount = 0;
            double vAmount = 0;
            for (int n = 0; n < dsLocal.Rows.Count; n++)
            {

                col = 1;
                var AccountCodeId = dsLocal.Rows[n]["GLGeneralInfoCode"].ToString();

                reportUtility.SetText(ref sheet, row, 1, AccountCodeId + " - " + dsLocal.Rows[n]["Budget"] + " - " + dsLocal.Rows[n]["Activity"]);

                if (_CurrencyId != plCurrencyId)
                {
                    sheet.Range[row, colGL, row, 3].Merge();
                    col = 4;
                }
                else
                {
                    sheet.Range[row, colGL, row, 5].Merge();
                    col = 6;
                }

                if (_CurrencyId != plCurrencyId)
                {
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["TDrAmount"].ToString())); col++;
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["TCrAmount"].ToString())); col++;
                    vAmount += Convert.ToDouble(dsLocal.Rows[n]["TCrAmount"].ToString());
                }
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(dsLocal.Rows[n]["CrAmount"].ToString());
                row++;
            }
            var lastRow = row;

            #region sumCalc

            reportUtility.SetText(ref sheet, lastRow, 1, "Total:", true);
            sheet.Range[reportUtility.GetColumnNameForXls(1) + lastRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + lastRow].Merge();
            if (_CurrencyId != plCurrencyId)
            {
                for (int i = 0; i < 4; i++)
                {
                    summerCol++;
                    sheet.Range[lastRow, summerCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(summerCol) + startRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + (lastRow - 1) + ")";
                    sheet.Range[lastRow, summerCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[lastRow, summerCol].CellStyle.Font.Bold = true;
                    sheet.Range[lastRow, summerCol].BorderAround(ExcelLineStyle.Hair);
                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    summerCol++;
                    sheet.Range[lastRow, summerCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(summerCol) + startRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + (lastRow - 1) + ")";
                    sheet.Range[lastRow, summerCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[lastRow, summerCol].CellStyle.Font.Bold = true;
                    sheet.Range[lastRow, summerCol].BorderAround(ExcelLineStyle.Hair);
                }
            }

            #endregion sumCalc

            sheet.Range[13, 1, lastRow, colLast].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[13, 1, lastRow, colLast].BorderAround(ExcelLineStyle.Hair);

            #region InWord

            var _amountValue = reportUtility.InWord(vAmount, _CurrencyId);
            var _amount = reportUtility.InWord(_Total_Amount, plCurrencyId);
            row++;

            reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

            if (_CurrencyId != plCurrencyId)
            {
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = _amountValue;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                row++;
            }
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = _amount;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

            #endregion InWord

            row = row + 4;

            #region Signature

            reportUtility.SetSignatureText(ref sheet, row - 1, 1, dsLocal.Rows[0]["AddedBy"].ToString());
            sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);



            reportUtility.SetSignatureText(ref sheet, row - 1, 4, dsLocal.Rows[0]["PostedBy"].ToString());
            sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 4, "Checked By", true);



            sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

            #endregion Signature

            sheet.UsedRange.AutofitColumns();
            sheet[row, 2].ColumnWidth = 22;
            sheet[row, 4].ColumnWidth = 15;




            sheet.UsedRange.CellStyle.Font.Size = 8;
            //  reportUtility.CompanyPlantHeader(ref sheet, 5, "MasterOrder Sales Post", companyId, plantName, null);
            reportUtility.CompanyPlantHeader2(ref sheet, colCredit, "Post Invoice", companyId, plantId, plantName, null);

            reportUtility.PageSetup(ref sheet, colCredit, ExcelPageOrientation.Portrait);

            return workbook;
        }

    }
}
