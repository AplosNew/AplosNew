using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Model.Enums;
using Library.Model.Vouchers;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Properties;
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
    public class AccountsSalesReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly CompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        public AccountsSalesReportService(ISqlRepository sqlRepository
            , CompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService
            )
        {
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }
        private DataTable GetCustomerInvoiceVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS TDrAmount, VD.CrAmount AS TCrAmount, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
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
        public IWorkbook GetSalesInvoiceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = GetCustomerInvoiceVoucher(voucherId);
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

            // Set report Name
            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["VoucherNo"];

            reportUtility.SetMasterHeaderText(ref sheet, 5, 1, "Voucher No");
            reportUtility.SetText(ref sheet, 5, 2, dsLocal.Rows[0]["VoucherNo"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 5, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, 5, 4, dsLocal.Rows[0]["VoucherDate"].ToString());
            sheet.Range[5, 4, 5, 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 1, "Doc Date");
            reportUtility.SetText(ref sheet, 6, 2, dsLocal.Rows[0]["DocDate"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 6, 3, "Doc No");
            reportUtility.SetText(ref sheet, 6, 4, dsLocal.Rows[0]["DocRefNo"].ToString());
            sheet.Range[6, 4, 6, 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 1, "Posting Date");
            reportUtility.SetText(ref sheet, 7, 2, dsLocal.Rows[0]["PostingDate"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 7, 3, "Fiscal Year");
            reportUtility.SetText(ref sheet, 7, 4, dsLocal.Rows[0]["PeriodName"].ToString());
            sheet.Range[7, 4, 7, 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Customer");
            reportUtility.SetText(ref sheet, 8, 2, dsLocal.Rows[0]["Customer"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 8, 3, "Customer Plant");
            reportUtility.SetText(ref sheet, 8, 4, dsLocal.Rows[0]["CustomerPlant"].ToString());
            sheet.Range[8, 4, 8, 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 9, 1, "Narration");
            reportUtility.SetText(ref sheet, 9, 2, dtNarration.Rows[0]["Narration"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 9, 3, "Status");
            reportUtility.SetText(ref sheet, 9, 4, dsLocal.Rows[0]["Park/Post"].ToString());
            sheet.Range[9, 4, 9, 5].Merge();

            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 11, col, "GL", 22); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Budget", 28); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Activity", 34); col++;
            var summerCol = col - 1;
            if (_CurrencyId != plCurrencyId)
            {
                reportUtility.SetHeaderText(ref sheet, 10, col, dsLocal.Rows[0]["TrnCurrency"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[10, col, 10, col + 1].Merge();
                reportUtility.SetHeaderText(ref sheet, 11, col, "Debit", ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, 11, col, "Credit", ExcelHAlign.HAlignRight); col++;
            }
            reportUtility.SetHeaderText(ref sheet, 10, col, dsLocal.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
            sheet[10, col, 10, col + 1].Merge();
            reportUtility.SetHeaderText(ref sheet, 11, col, "Debit", ExcelHAlign.HAlignRight); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Credit", ExcelHAlign.HAlignRight);
            var colLast = col;
            var row = 12;
            var startRow = row;
            double _Total_Amount = 0;
            double vAmount = 0;
            for (int n = 0; n < dsLocal.Rows.Count; n++)
            {
                col = 1;
                var AccountCodeId = dsLocal.Rows[n]["GLGeneralInfoCode"].ToString();
                reportUtility.SetText(ref sheet, row, col, AccountCodeId + " - " + dsLocal.Rows[n]["GL"]); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["Budget"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["Activity"].ToString()); col++;
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

            sheet.Range[12, 1, lastRow, colLast].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, lastRow, colLast].BorderAround(ExcelLineStyle.Hair);

            #region InWord

            var _amountValue = reportUtility.InWord(vAmount, _CurrencyId);
            var _amount = reportUtility.InWord(_Total_Amount, plCurrencyId);
            row++;

            reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

            if (_CurrencyId != plCurrencyId)
            {
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = _amountValue;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast - 2) + row].Merge();
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

            reportUtility.SetSignatureText(ref sheet, row - 1, 3, dsLocal.Rows[0]["PostedBy"].ToString());
            sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

            sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

            #endregion Signature

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyPlantHeader(ref sheet, 7, "Sales Invoice Voucher", companyId, plantName, null);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);

            return workbook;
        }



        private DataTable GetMasterOrderSalesPostSql(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS TDrAmount, VD.CrAmount AS TCrAmount, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
                            ,V.DocRefNo as InvoiceNo,''OrderNo
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
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

        public IWorkbook GetMasterOrderSalesPostReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = GetMasterOrderSalesPostSql(voucherId);
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

            reportUtility.SetMasterHeaderText(ref sheet, 7,  5, "Fiscal Year");
            reportUtility.SetText(ref sheet, 7, 6, dsLocal.Rows[0]["PeriodName"].ToString());
            sheet.Range[7, 6, 7, 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Customer");
            reportUtility.SetText(ref sheet, 8, 2, dsLocal.Rows[0]["Customer"].ToString());
            sheet.Range[8, 2, 8, 3].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 5, "Customer Plant");
            reportUtility.SetText(ref sheet, 8, 6, dsLocal.Rows[0]["CustomerPlant"].ToString());
            sheet.Range[8, 6, 8, 7].Merge();


            reportUtility.SetMasterHeaderText(ref sheet, 9, 1, "InvoiceNo.");
            reportUtility.SetText(ref sheet, 9, 2, dsLocal.Rows[0]["InvoiceNo"].ToString());
            sheet.Range[9, 2, 9, 3].Merge();


            reportUtility.SetMasterHeaderText(ref sheet, 9,  5, "OrderNo.");
            reportUtility.SetText(ref sheet, 9, 6, dsLocal.Rows[0]["OrderNo"].ToString());
            sheet.Range[9, 6, 9, 7].Merge();

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
                sheet.Range[row, colGL, row,3].Merge();
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
                reportUtility.SetHeaderText(ref sheet, 12, col, "Debit", ExcelHAlign.HAlignRight);int colUsdDebit = col; col++;
                reportUtility.SetHeaderText(ref sheet, 12, col, "Credit", ExcelHAlign.HAlignRight);int colUsdCredit = col; col++;
            }
            reportUtility.SetHeaderText(ref sheet, 11, col, dsLocal.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
            sheet[11, col, 11, col + 1].Merge();
            sheet.Range[11, col, 11, col + 1].BorderAround(ExcelLineStyle.Thin);
            reportUtility.SetHeaderText(ref sheet, 12, col, "Debit", ExcelHAlign.HAlignRight);int colDebit = col; col++;
            reportUtility.SetHeaderText(ref sheet, 12, col, "Credit", ExcelHAlign.HAlignRight);int colCredit = col;
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
                reportUtility.SetText(ref sheet, row,col, Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString())); col++;
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
            reportUtility.CompanyPlantHeader2(ref sheet, colCredit, "Master Order Sales Post", companyId, plantId, plantName, null);
          
            reportUtility.PageSetup(ref sheet, colCredit, ExcelPageOrientation.Portrait);

            return workbook;
        }




      
        public IWorkbook GetSalesPostingReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetSalesPostingHeader(companyGroupId, companyId, plantId, voucherId, SourceType.SalesInvoice); //
            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];
            var dsLocal = GetSalesPostingReportData(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Invoice No");
            reportUtility.SetText(ref sheet, row, 2, header["InvoiceNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Sales No");
            reportUtility.SetText(ref sheet, row, 5, header["SalesNo"].ToString());

            row++;
            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();


            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet.Range[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet.Range[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();

                sheet.Range[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);

            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++;
            xlsCol++;
            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;
                }

                reportUtility.SetText(ref sheet, row, 2, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + 12 + ":" + reportUtility.GetColumnNameForXls(7) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);


                    //sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    //sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    //sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    //sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    //sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    //sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    //sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    //sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    //sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    //sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    //sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    //sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    //sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    //sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    //sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    //sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 30;
                sheet[1, 3].ColumnWidth = 15;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast,"Sales Invoice", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Sales Invoice", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        private Dictionary<string, object> GetSalesPostingHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor, PP.UserName AS VendorPlant, V.CurrencyId, C.Code AS CurrencyCode
                            ,BJ.DocRefNo as InvoiceNo
							,IVN.Id as SalesNo
                            FROM [TRN].[Voucher] AS V
                            LEFT JOIN [TRN].[Invoice] AS BJ ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            LEFT JOIN TRN.InventorySales  as IVN on IVN.VoucherId=v.Id 
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private DataTable GetSalesPostingReportData(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
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




        public IWorkbook GetInventorySalesPostingReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetInventorySalesPostingHeader(companyGroupId, companyId, plantId, voucherId, SourceType.SalesInvoice); //
            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];
            var dsLocal = GetInventorySalesPostingReportData(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Sales No");
            reportUtility.SetText(ref sheet, row, 2, header["SalesNo"].ToString());

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();


            row++;


            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet.Range[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet.Range[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet.Range[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);




            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++;
            xlsCol++;
            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;
                }

                reportUtility.SetText(ref sheet, row, 2, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 30;
                sheet[1, 3].ColumnWidth = 15;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Inventory Sales Invoice", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Inventory Sales Invoice", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        private Dictionary<string, object> GetInventorySalesPostingHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor, PP.UserName AS VendorPlant, V.CurrencyId, C.Code AS CurrencyCode
                            ,BJ.DocRefNo as InvoiceNo
							,IVN.Id as SalesNo
                            FROM [TRN].[Voucher] AS V
                            LEFT JOIN [TRN].[Invoice] AS BJ ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            LEFT JOIN TRN.InventorySales  as IVN on IVN.VoucherId=v.Id 
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private DataTable GetInventorySalesPostingReportData(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
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




    }
}
