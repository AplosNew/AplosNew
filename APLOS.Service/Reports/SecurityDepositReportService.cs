using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Currencies;
using Library.Service.Helpers;
using Library.Service.SecurityDeposits;
using Library.Service.Vouchers;
using Syncfusion.XlsIO;
using System;

namespace Library.Service.Reports
{
    public class SecurityDepositReportService : ISecurityDepositReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ISecurityDepositService _securityDepositService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IVoucherService _voucherService;

        public SecurityDepositReportService(
            ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , ISecurityDepositService securityDepositService
            , IVoucherService voucherService)
        {
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _securityDepositService = securityDepositService;
            _voucherService = voucherService;
        }

        public IWorkbook GetSecurityDepositTakenReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = _securityDepositService.GetVoucherHeader(companyGroupId, companyId, plantId, voucherId, SourceType.SecurityDeposit);

            // Set report Name
            reportFileName = Convert.ToDateTime(headerData["PostingDate"]).ToString("yyMMdd") + " " + headerData["VoucherNo"];

            var row = 5;
            report.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            report.SetText(ref sheet, row, 2, headerData["VoucherNo"].ToString());
            row++;

            report.SetMasterHeaderText(ref sheet, row, 1, "Doc Date");
            report.SetText(ref sheet, row, 2, headerData["DocDate"].ToString());
            row++;

            report.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            report.SetText(ref sheet, row, 2, headerData["PostingDate"].ToString());
            row++;

            report.SetMasterHeaderText(ref sheet, row, 1, "Party");
            report.SetText(ref sheet, row, 2, headerData["PartyName"].ToString()); row++;

            report.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            report.SetText(ref sheet, row, 2, headerData["Narration"].ToString());
            sheet.Range[row, 2, row, 5].Merge();

            var _rowR = 5;
            report.SetMasterHeaderText(ref sheet, _rowR, 3, "Voucher Date");
            report.SetText(ref sheet, _rowR, 4, headerData["VoucherDate"].ToString());
            sheet.Range[_rowR, 4, _rowR, 5].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 3, "Doc No");
            report.SetText(ref sheet, _rowR, 4, headerData["DocRefNo"].ToString());
            sheet.Range[_rowR, 4, _rowR, 5].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 3, "Fiscal Year");
            report.SetText(ref sheet, _rowR, 4, headerData["FiscalYearName"] + " (" + headerData["PeriodNo"] + ")");
            sheet.Range[_rowR, 4, _rowR, 5].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 3, "Status");
            report.SetText(ref sheet, _rowR, 4, Convert.ToBoolean(headerData["IsPark"]) ? "Parked" : "Posted");
            sheet.Range[_rowR, 4, _rowR, 5].Merge();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var _rowL = 11;
            var headreColIndex = 1;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 32); headreColIndex++;

            var sumdrcrCol = headreColIndex;
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, headerData["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", 12);
                headreColIndex++;
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);
                headreColIndex++;
            }
            double _Total_Amount = 0;
            report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

            var shet2EndxlsCol = headreColIndex;
            double vAmount = 0;
            var col = 1;
            var Row_Total_Start = _rowL + 1;

            var data = _voucherService.GetVoucherData(companyId, voucherId);
            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                col = 1;
                report.SetText(ref sheet, _rowL, col, data[n]["GLGeneralInfoCode"] + " - " + data[n]["GLGeneralInfoName"]); col++;
                report.SetText(ref sheet, _rowL, col, data[n]["BudgetName"].ToString()); col++;
                report.SetText(ref sheet, _rowL, col, data[n]["ActivityName"].ToString()); col++;
                if (companyCurrencyId != headerData["CurrencyId"].ToString())
                {
                    report.SetText(ref sheet, _rowL, col, Convert.ToDouble(data[n]["DrAmount"])); col++;
                    report.SetText(ref sheet, _rowL, col, Convert.ToDouble(data[n]["CrAmount"])); col++;
                    vAmount += Convert.ToDouble(data[n]["DrAmount"].ToString());
                }
                report.SetText(ref sheet, _rowL, col, Convert.ToDouble(data[n]["CompanyCurrencyDrAmount"].ToString())); col++;
                report.SetText(ref sheet, _rowL, col, Convert.ToDouble(data[n]["CompanyCurrencyCrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(data[n]["CrAmount"].ToString());
            }

            _rowL++;
            report.SetText(ref sheet, _rowL, 1, "Total :", true);
            sheet.Range[_rowL, 1, _rowL, sumdrcrCol - 1].Merge();

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sumdrcrCol++;

                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sumdrcrCol++;
            }

            sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            sumdrcrCol++;

            sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            sumdrcrCol++;

            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            _rowL += 1;
            var _col = 2;
            report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                var _amountValue = report.InWord(vAmount, headerData["CurrencyId"].ToString());
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
                _rowL++;
                _col++;
            }

            var _amount = report.InWord(_Total_Amount, companyCurrencyId);
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amount;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

            _rowL = _rowL + 4;
            report.SetSignatureText(ref sheet, _rowL - 1, 1, headerData["AddedBy"].ToString());
            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

            report.SetSignatureText(ref sheet, _rowL - 1, 3, headerData["PostedBy"].ToString());
            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

            sheet.Range[_rowL, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 5, "Authorized By", true);

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headerData["VoucherTypeName"].ToString(), companyId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }
    }
}